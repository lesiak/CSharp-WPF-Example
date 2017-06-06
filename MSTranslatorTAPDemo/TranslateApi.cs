using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;

namespace MSTranslatorTAPDemo
{
    public class TranslateApi
    {
        /// <summary>
        /// List of languages that have a synthetic voice for text to speech
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        public static List<string> GetLanguagesForSpeakMethod(string authToken)
        {
            const string uri = "http://api.microsofttranslator.com/v2/Http.svc/GetLanguagesForSpeak";
            return GetAndDeserialize(uri, authToken, DeserializeFromStream<List<string>>);
        }

        /// <summary>
        /// CODE TO GET TRANSLATABLE LANGAUGE CODES
        /// </summary>
        /// <returns></returns>
        public static List<string> GetLanguageCodesForTranslate(string authToken)
        {
            const string uri = "http://api.microsofttranslator.com/v2/Http.svc/GetLanguagesForTranslate";
            return GetAndDeserialize(uri, authToken, DeserializeFromStream<List<string>>);
        }

        //*****CODE TO GET TRANSLATABLE LANGAUGE FRIENDLY NAMES FROM THE TWO CHARACTER CODES*****
        public static Dictionary<string, string> GetLanguageNamesMethod(string authToken, List<string> languageCodes)
        {
            string uri = "http://api.microsofttranslator.com/v2/Http.svc/GetLanguageNames?locale=en";
            

            var languageNames = PostAndDeserializeResponse(uri, authToken, languageCodes, DeserializeFromStream<List<string>>);

            var languageCodesAndTitles = new Dictionary<string, string>();
            for (int i = 0; i < languageNames.Count; i++)
            {
                languageCodesAndTitles.Add(languageNames[i], languageCodes[i]);
            }
            return languageCodesAndTitles;
            
        }

        private static TRESPDATA PostAndDeserializeResponse<TPOSTDATA, TRESPDATA>(string uri, string authToken, TPOSTDATA postData, Func<Stream, TRESPDATA> deserializeFunc)
        {          
            // create the request
            var request = WebRequest.Create(uri);
            request.Headers.Add("Authorization", authToken);
            request.ContentType = "text/xml";
            request.Method = "POST";
            var serializer = new DataContractSerializer(typeof(TPOSTDATA));
            using (var stream = request.GetRequestStream())
            {
                serializer.WriteObject(stream, postData);
            }
            WebResponse response = null;
            try
            {
                response = request.GetResponse();
                using (var stream = response.GetResponseStream())
                {
                    var respData = (TRESPDATA)serializer.ReadObject(stream);
                    return respData;
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

        private static T GetAndDeserialize<T>(string uri, string authToken, Func<Stream, T> deserializeFunc)
        {
            var httpWebRequest = WebRequest.Create(uri);
            httpWebRequest.Headers.Add("Authorization", authToken);
            WebResponse response = null;
            try
            {
                response = httpWebRequest.GetResponse();
                using (var stream = response.GetResponseStream())
                {
                    return deserializeFunc(stream);
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

        private static T DeserializeFromStream<T>(Stream stream)
        {
            var serializer = new DataContractSerializer(typeof(T));
            var retList = (T)serializer.ReadObject(stream);
            return retList;
        }
    }
}