using System;
using System.Collections.Generic;
using System.IO;
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