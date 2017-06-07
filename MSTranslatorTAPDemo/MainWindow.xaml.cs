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
using System.Threading.Tasks;
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
            InitializeUiData();
        }

        private async void InitializeUiData()
        {
            var languageCodes = await TranslateApi.GetLanguageCodesForTranslate(tokenProvider.GetAccessToken());
            var languageCodesAndTitles = TranslateApi.GetLanguageNamesMethod(tokenProvider.GetAccessToken(), languageCodes);
            //List of languages that have a synthetic voice for text to speech
            speakLanguages = await TranslateApi.GetLanguagesForSpeakMethod(tokenProvider.GetAccessToken());
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
        private async void translateButton_Click(object sender, EventArgs e)
        {
            var languageCode = (string)LanguageComboBox.SelectedValue ?? "en";

            //*****BEGIN CODE TO MAKE THE CALL TO THE TRANSLATOR SERVICE TO PERFORM A TRANSLATION FROM THE USER TEXT ENTERED INCLUDES A CALL TO A SPEECH METHOD*****

            string txtToTranslate = textToTranslate.Text;

            string translatedText = await TranslateApi.Translate(tokenProvider.GetAccessToken(), txtToTranslate, languageCode);

            translatedTextLabel.Content = "Translation -->   " + translatedText;

            if (speakLanguages.Contains(languageCode) && txtToTranslate != "")
            {
                //call the method to speak the translated text
                await SpeakMethod(tokenProvider.GetAccessToken(), translatedText, languageCode);
            }
        }

       

        //*****SPEECH CODE*****
        private async Task SpeakMethod(string authToken, string textToSpeak, String languageCode)
        {
           await TranslateApi.SpeakMethod(authToken, textToSpeak, languageCode, PlayStream);
        }


       

        private static void PlayStream(Stream stream)
        {
            using (SoundPlayer player = new SoundPlayer(stream))
            {
                player.Play();
            }
        }
    }
}
