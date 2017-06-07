﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;

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

        /// <summary>
        /// CODE TO GET TRANSLATABLE LANGAUGE FRIENDLY NAMES FROM THE TWO CHARACTER CODES
        /// </summary>
        /// <param name="authToken"></param>
        /// <param name="languageCodes"></param>
        /// <returns></returns>
        public static List<LangDesc> GetLanguageNamesMethod(string authToken, List<string> languageCodes)
        {
            const string uri = "http://api.microsofttranslator.com/v2/Http.svc/GetLanguageNames?locale=en";
            var languageNames = PostAndDeserializeResponse(uri, authToken, languageCodes, DeserializeFromStream<List<string>>);
            return languageNames.Zip(languageCodes, (langName, langCode) => new LangDesc(langName, langCode)).ToList();
            
        }

        public static string Translate(string authToken, string txtToTranslate, string toLanguageCode)
        {
            string uri =
                string.Format(
                    "http://api.microsofttranslator.com/v2/Http.svc/Translate?text=" +
                    System.Web.HttpUtility.UrlEncode(txtToTranslate) + "&to={0}", toLanguageCode);

            WebRequest translationWebRequest = WebRequest.Create(uri);

            translationWebRequest.Headers.Add("Authorization",
                authToken); //header value is the "Bearer plus the token from ADM

            WebResponse response = null;

            response = translationWebRequest.GetResponse();

            Stream stream = response.GetResponseStream();

            Encoding encode = Encoding.GetEncoding("utf-8");

            StreamReader translatedStream = new StreamReader(stream, encode);

            System.Xml.XmlDocument xTranslation = new System.Xml.XmlDocument();

            xTranslation.LoadXml(translatedStream.ReadToEnd());

            return xTranslation.InnerText;
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

        private static TRespdata PostAndDeserializeResponse<TPostdata, TRespdata>(string uri, string authToken, TPostdata postData, Func<Stream, TRespdata> deserializeFunc)
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
                    var respData = (TRespdata)serializer.ReadObject(stream);
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