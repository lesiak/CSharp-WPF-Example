using System;
using System.Windows;
using System.Net;
using System.Web;
using System.IO;
using System.Media;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.Translator.Samples;

namespace MSTranslatorTAPDemo
{
    /// <summary>
    /// The goal of this WPF app is to demonstrate code for getting a security token, and translating a word or phrase into another langauge.
    /// The target langauge is selected from a combobox. The text of the translation is displayed and the translation is heard as speech.
    /// </summary>
    public partial class MainWindow : Window
    {
        // Before running the application, input the secret key for your subscription to Translator Text Translation API.
        private const string TEXT_TRANSLATION_API_SUBSCRIPTION_KEY = "ENTER_YOUR_CLIENT_SECRET";

        // Object to get an authentication token
        private AzureAuthToken tokenProvider;

        // Cache list of languages for speech synthesis
        private List<string> speakLanguages;

        // Dictionary to map language code from friendly name
        private Dictionary<string, string> languageCodesAndTitles = new Dictionary<string, string>();

        public MainWindow()
        {
            InitializeComponent();
            tokenProvider = new AzureAuthToken(TEXT_TRANSLATION_API_SUBSCRIPTION_KEY);
            var languageCodes = GetLanguageCodesForTranslate().ToArray();
            GetLanguageNamesMethod(tokenProvider.GetAccessToken(), languageCodes);
            speakLanguages = TranslateApi.GetLanguagesForSpeakMethod(tokenProvider
                .GetAccessToken()); //List of languages that have a synthetic voice for text to speech
            enumLanguages(); //Create the drop down list of langauges
        }

        //*****POPULATE COMBOBOX*****
        private void enumLanguages()
        {
            //run a loop to load the combobox from the dictionary
            var count = languageCodesAndTitles.Count;

            for (int i = 0; i < count; i++)
            {
                LanguageComboBox.Items.Add(languageCodesAndTitles.ElementAt(i).Key);
            }
        }

        //*****BUTTON TO START TRANSLATION PROCESS
        private void translateButton_Click(object sender, EventArgs e)
        {
            string languageCode;
            languageCodesAndTitles.TryGetValue(LanguageComboBox.Text,
                out languageCode); //get the language code from the dictionary based on the selection in the combobox

            if (languageCode == null) //in case no language is selected.
            {
                languageCode = "en";
            }

            //*****BEGIN CODE TO MAKE THE CALL TO THE TRANSLATOR SERVICE TO PERFORM A TRANSLATION FROM THE USER TEXT ENTERED INCLUDES A CALL TO A SPEECH METHOD*****

            string txtToTranslate = textToTranslate.Text;

            string uri =
                string.Format(
                    "http://api.microsofttranslator.com/v2/Http.svc/Translate?text=" +
                    System.Web.HttpUtility.UrlEncode(txtToTranslate) + "&to={0}", languageCode);

            WebRequest translationWebRequest = WebRequest.Create(uri);

            translationWebRequest.Headers.Add("Authorization",
                tokenProvider.GetAccessToken()); //header value is the "Bearer plus the token from ADM

            WebResponse response = null;

            response = translationWebRequest.GetResponse();

            Stream stream = response.GetResponseStream();

            Encoding encode = Encoding.GetEncoding("utf-8");

            StreamReader translatedStream = new StreamReader(stream, encode);

            System.Xml.XmlDocument xTranslation = new System.Xml.XmlDocument();

            xTranslation.LoadXml(translatedStream.ReadToEnd());

            translatedTextLabel.Content = DateTime.Now + "Translation -->   " + xTranslation.InnerText;

            if (speakLanguages.Contains(languageCode) && txtToTranslate != "")
            {
                //call the method to speak the translated text
                SpeakMethod(tokenProvider.GetAccessToken(), xTranslation.InnerText, languageCode);
            }
        }

        //*****SPEECH CODE*****
        private void SpeakMethod(string authToken, string textToVoice, String languageCode)
        {
            string translatedString = textToVoice;

            string uri =
                string.Format(
                    "http://api.microsofttranslator.com/v2/Http.svc/Speak?text={0}&language={1}&format=" +
                    HttpUtility.UrlEncode("audio/wav") + "&options=MaxQuality", translatedString, languageCode);

            WebRequest webRequest = WebRequest.Create(uri);
            webRequest.Headers.Add("Authorization", authToken);
            WebResponse response = null;
            try
            {
                response = webRequest.GetResponse();

                using (Stream stream = response.GetResponseStream())
                {
                    using (SoundPlayer player = new SoundPlayer(stream))
                    {
                        player.PlaySync();
                    }
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                    response = null;
                }
            }
        }


        //*****CODE TO GET TRANSLATABLE LANGAUGE CODES*****
        private List<string> GetLanguageCodesForTranslate()
        {
            string uri = "http://api.microsofttranslator.com/v2/Http.svc/GetLanguagesForTranslate";
            WebRequest WebRequest = WebRequest.Create(uri);
            WebRequest.Headers.Add("Authorization", tokenProvider.GetAccessToken());

            WebResponse response = null;

            try
            {
                response = WebRequest.GetResponse();
                using (Stream stream = response.GetResponseStream())
                {
                    DataContractSerializer dcs = new DataContractSerializer(typeof(List<string>));
                    List<string> languagesForTranslate = (List<string>) dcs.ReadObject(stream);
                    return languagesForTranslate;
                }
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }
        }


        //*****CODE TO GET TRANSLATABLE LANGAUGE FRIENDLY NAMES FROM THE TWO CHARACTER CODES*****
        private void GetLanguageNamesMethod(string authToken, string[] languageCodes)
        {
            string uri = "http://api.microsofttranslator.com/v2/Http.svc/GetLanguageNames?locale=en";
            // create the request
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(uri);
            request.Headers.Add("Authorization", tokenProvider.GetAccessToken());
            request.ContentType = "text/xml";
            request.Method = "POST";
            DataContractSerializer dcs = new DataContractSerializer(Type.GetType("System.String[]"));
            using (Stream stream = request.GetRequestStream())
            {
                dcs.WriteObject(stream, languageCodes);
            }
            WebResponse response = null;
            try
            {
                response = request.GetResponse();

                using (Stream stream = response.GetResponseStream())
                {
                    string[] languageNames = (string[]) dcs.ReadObject(stream);

                    for (int i = 0; i < languageNames.Length; i++)
                    {
                        languageCodesAndTitles.Add(languageNames[i],
                            languageCodes[i]); //load the dictionary for the combo box
                    }
                }
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                    response = null;
                }
            }
        }

       
    }
}
