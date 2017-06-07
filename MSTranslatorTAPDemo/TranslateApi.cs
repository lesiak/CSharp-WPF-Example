using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MSTranslatorTAPDemo
{
    /// <summary>
    /// Client for MS Translate api.
    /// Documentation, https://docs.microsofttranslator.com/text-translate.html
    /// </summary>
    public class TranslateApi
    {
        public static HttpClient httpClient = new HttpClient();

        /// <summary>
        /// List of languages that have a synthetic voice for text to speech
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        public static async Task<List<string>> GetLanguagesForSpeakMethod(string authToken)
        {
            const string uri = "http://api.microsofttranslator.com/v2/Http.svc/GetLanguagesForSpeak";
            return await GetAndDeserialize(uri, authToken, DeserializeFromStream<List<string>>);
        }

        /// <summary>
        /// CODE TO GET TRANSLATABLE LANGAUGE CODES
        /// </summary>
        /// <returns></returns>
        public static async Task<List<string>> GetLanguageCodesForTranslate(string authToken)
        {
            const string uri = "http://api.microsofttranslator.com/v2/Http.svc/GetLanguagesForTranslate";
            return await GetAndDeserialize(uri, authToken, DeserializeFromStream<List<string>>);
        }

        /// <summary>
        /// CODE TO GET TRANSLATABLE LANGAUGE FRIENDLY NAMES FROM THE TWO CHARACTER CODES
        /// </summary>
        /// <param name="authToken"></param>
        /// <param name="languageCodes"></param>
        /// <returns></returns>
        public static List<LangDesc> GetLanguageNamesMethod(string authToken, List<string> languageCodes)
        {
            const string uri = "http://api.microsofttranslator.com/v2/Http.svc/GetLanguageNames?locale=en";
            var languageNames = PostAndDeserializeResponse(uri, authToken, languageCodes,
                DeserializeFromStream<List<string>>);
            return languageNames.Zip(languageCodes, (langName, langCode) => new LangDesc(langName, langCode)).ToList();
        }

        public static async Task<string> Translate(string authToken, string txtToTranslate, string toLanguageCode)
        {
            string uri = string.Format(
                "http://api.microsofttranslator.com/v2/Http.svc/Translate?text=" +
                HttpUtility.UrlEncode(txtToTranslate) + "&to={0}", toLanguageCode);

            return await GetAndDeserialize(uri, authToken, GetXmlInnerText);
        }


        public static async Task SpeakMethod(string authToken, string textToSpeak, string languageCode,
            Action<Stream> playAction)
        {
            string uri = string.Format(
                "http://api.microsofttranslator.com/v2/Http.svc/Speak?text={0}&language={1}&format=" +
                HttpUtility.UrlEncode("audio/wav") + "&options=MaxQuality", textToSpeak, languageCode);

            await GetAndRunAction(uri, authToken, playAction);
        }


        private static async Task GetAndRunAction(string uri, string authToken, Action<Stream> action)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Authorization", authToken);
            using (var response = await httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    action(stream);
                }
            }
        }


        private static async Task<T> GetAndDeserialize<T>(string uri, string authToken, Func<Stream, T> deserializeFunc)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Authorization", authToken);
            using (var response = await httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                //response = httpWebRequest.GetResponse();
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    return deserializeFunc(stream);
                }
            }
        }


        private static TRespdata PostAndDeserializeResponse<TPostdata, TRespdata>(string uri, string authToken,
            TPostdata postData, Func<Stream, TRespdata> deserializeFunc)
        {
            // create the request
            var request = WebRequest.Create(uri);
            request.Headers.Add("Authorization", authToken);
            request.ContentType = "text/xml";
            request.Method = "POST";
            var serializer = new DataContractSerializer(typeof(TPostdata));
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
                    return deserializeFunc(stream);
                }
            }
            finally
            {
                response?.Close();
            }
        }


        private static string GetXmlInnerText(Stream stream)
        {
            Encoding encode = Encoding.GetEncoding("utf-8");
            StreamReader translatedStream = new StreamReader(stream, encode);
            System.Xml.XmlDocument xTranslation = new System.Xml.XmlDocument();
            xTranslation.LoadXml(translatedStream.ReadToEnd());
            return xTranslation.InnerText;
        }


        private static T DeserializeFromStream<T>(Stream stream)
        {
            var serializer = new DataContractSerializer(typeof(T));
            var retList = (T) serializer.ReadObject(stream);
            return retList;
        }


        public class LangDesc
        {
            public LangDesc(string name, string code)
            {
                Name = name;
                Code = code;
            }

            public string Name { get; }
            public string Code { get; }
        }
    }
}