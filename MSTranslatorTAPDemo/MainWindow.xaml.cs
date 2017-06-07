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
       
        public MainWindow()
        {
            InitializeComponent();
            tokenProvider = new AzureAuthToken(TEXT_TRANSLATION_API_SUBSCRIPTION_KEY);
            var languageCodes = TranslateApi.GetLanguageCodesForTranslate(tokenProvider.GetAccessToken());
            var languageCodesAndTitles = TranslateApi.GetLanguageNamesMethod(tokenProvider.GetAccessToken(), languageCodes);
            //List of languages that have a synthetic voice for text to speech
            speakLanguages = TranslateApi.GetLanguagesForSpeakMethod(tokenProvider.GetAccessToken()); 
            PopulateLanguagesComboBox(languageCodesAndTitles); //Create the drop down list of langauges
        }

        //*****POPULATE COMBOBOX*****
        private void PopulateLanguagesComboBox(List<TranslateApi.LangDesc> languageCodesAndTitles)
        {
            //run a loop to load the combobox from the dictionary
            foreach (var langDesc in languageCodesAndTitles)
            {
                LanguageComboBox.Items.Add(langDesc);
            } 
        }

        //*****BUTTON TO START TRANSLATION PROCESS
        private void translateButton_Click(object sender, EventArgs e)
        {
            var languageCode = (string)LanguageComboBox.SelectedValue ?? "en";

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

       
    }
}
