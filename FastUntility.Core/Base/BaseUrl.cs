using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace FastUntility.Core.Base
{ /// <summary>
  /// post、get、put到url
  /// </summary>
    public static class BaseUrl
    {
        private static HttpClient http;

        static BaseUrl()
        {
            http = new HttpClient(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip });
            http.DefaultRequestHeaders.Connection.Add("keep-alive");
        }

        #region get url(select)
        /// <summary>
        /// get url(select)
        /// </summary>
        public static string GetUrl(string url, IHttpClientFactory client = null, int version = 1, int minor = 1, string mediaType = "application/json")
        {
            try
            {
                if (client != null)
                    http = client.CreateClient();

                var handle = new HttpRequestMessage();
                handle.Version = new Version(version, minor);
                handle.Content = new StringContent("", Encoding.UTF8, mediaType);
                handle.Method = HttpMethod.Get;
                handle.RequestUri = new Uri(url);
                var response = http.SendAsync(handle).Result;
                return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {

                BaseLog.SaveLog(url + ":" + ex.ToString(), "GetUrl_exp");
                return null;
            }
        }
        #endregion

        #region post url(insert)
        /// <summary>
        /// post url(insert)
        /// </summary>
        public static string PostUrl( string url, Dictionary<string, object> dic, IHttpClientFactory client = null, int version = 1, int minor = 1, string mediaType = "application/json")
        {
            try
            {
                if (client != null)
                     http = client.CreateClient();

                var count = 0;
                foreach (var item in dic)
                {
                    if (url.Contains("?"))
                        url = string.Format("{0}&{1}={2}", url, item.Key, item.Value);
                    else
                    {
                        if (count == 0)
                            url = string.Format("{0}?{1}={2}", url, item.Key, item.Value);
                        else
                            url = string.Format("{0}&{1}={2}", url, item.Key, item.Value);
                    }
                    count++;
                }

                var handle = new HttpRequestMessage();
                handle.Version = new Version(version, minor);
                handle.Content = new StringContent("", Encoding.UTF8, mediaType);
                handle.Method = HttpMethod.Post;
                handle.RequestUri = new Uri(url);
                var response = http.SendAsync(handle).Result;
                return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() => { BaseLog.SaveLog(url + ":" + ex.ToString(), "PostUrl_exp"); });
                return null;
            }
        }
        #endregion

        #region post content(insert)
        /// <summary>
        /// post content(insert)
        /// </summary>
        public static string PostContent(string url, string param, IHttpClientFactory client = null, int version = 1, int minor = 1, string mediaType = "application/json")
        {
            try
            {
                if (client != null)
                    http = client.CreateClient();

                var handle = new HttpRequestMessage();
                handle.Version = new Version(version, minor);
                handle.Content = new StringContent(param, Encoding.UTF8, mediaType);
                handle.Method = HttpMethod.Post;
                handle.RequestUri = new Uri(url);
                var response = http.SendAsync(handle).Result;
                return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() => { BaseLog.SaveLog(url + ":" + ex.ToString(), "PostContent_exp"); });
                return null;
            }
        }
        #endregion

        #region put url (update)
        /// <summary>
        /// put url(update)
        /// </summary>
        public static string PutUrl(string url, Dictionary<string, object> dic, IHttpClientFactory client = null, int version = 1, int minor = 1, string mediaType = "application/json")
        {
            try
            {
                if (client != null)
                    http = client.CreateClient();

                var count = 0;
                foreach (var item in dic)
                {
                    if (url.Contains("?"))
                        url = string.Format("{0}&{1}={2}", url, item.Key, item.Value);
                    else
                    {
                        if (count == 0)
                            url = string.Format("{0}?{1}={2}", url, item.Key, item.Value);
                        else
                            url = string.Format("{0}&{1}={2}", url, item.Key, item.Value);
                    }
                    count++;
                }
                
                var handle = new HttpRequestMessage();
                handle.Version = new Version(version, minor);
                handle.Content = new StringContent("", Encoding.UTF8, mediaType);
                handle.Method = HttpMethod.Put;
                handle.RequestUri = new Uri(url);
                var response = http.SendAsync(handle).Result;
                return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() => { BaseLog.SaveLog(url + ":" + ex.ToString(), "PutUrl_exp"); });
                return null;
            }
        }
        #endregion

        #region put content (update)
        /// <summary>
        /// put content(update)
        /// </summary>
        public static string PutContent(string url, string param, IHttpClientFactory client = null, int version = 1, int minor = 1, string mediaType = "application/json")
        {
            try
            {
                if (client != null)
                    http = client.CreateClient();

                var handle = new HttpRequestMessage();
                handle.Version = new Version(version, minor);
                handle.Content = new StringContent(param, Encoding.UTF8, mediaType);
                handle.Method = HttpMethod.Put;
                handle.RequestUri = new Uri(url);
                var response = http.SendAsync(handle).Result;
                return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() => { BaseLog.SaveLog(url + ":" + ex.ToString(), "PutUrl_exp"); });
                return null;
            }
        }
        #endregion

        #region delete url (delete)
        /// <summary>
        /// delete url (delete)
        /// </summary>
        public static string DeleteUrl(string url, IHttpClientFactory client = null, int version = 1, int minor = 1, string mediaType = "application/json")
        {
            try
            {
                if (client != null)
                    http = client.CreateClient();

                var handle = new HttpRequestMessage();
                handle.Version = new Version(version, minor);
                handle.Content = new StringContent("", Encoding.UTF8, mediaType);
                handle.Method = HttpMethod.Delete;
                handle.RequestUri = new Uri(url);
                var response = http.SendAsync(handle).Result;
                return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() => { BaseLog.SaveLog(url + ":" + ex.ToString(), "PutUrl_exp"); });
                BaseLog.SaveLog(url + ":" + ex.ToString(), "DeleteUrl_exp");
                return null;
            }
        }
        #endregion
        
        #region post soap
        /// <summary>
        /// post content(insert)
        /// </summary>
        public static string PostSoap(string url, string method, Dictionary<string, object> param, string Namespace = "http://tempuri.org/", IHttpClientFactory client = null, int version = 1, int minor = 1)
        {
            try
            {
                if (client != null)
                    http = client.CreateClient();

                var xml = new StringBuilder();
                xml.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                xml.Append("<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
                xml.Append("<soap:Header />");
                xml.Append("<soap:Body>");
                xml.AppendFormat("<{0} xmlns=\"{1}\">", method, Namespace);

                foreach (KeyValuePair<string, object> item in param)
                {
                    xml.AppendFormat("<{0}>{1}</{0}>", item.Key, item.Value);
                }

                xml.AppendFormat("</{0}>", method);
                xml.Append("</soap:Body>");
                xml.Append("</soap:Envelope>");;

                var handle = new HttpRequestMessage();
                handle.Version = new Version(version, minor);
                handle.Content = new StringContent(xml.ToString(), Encoding.UTF8, "text/xml");
                handle.Method = HttpMethod.Post;
                handle.RequestUri = new Uri(url);
                var response = http.SendAsync(handle).Result;
                var result = response.Content.ReadAsStringAsync().Result;

                result = result.Replace("soap:Envelope", "Envelope");
                result = result.Replace("soap:Body", "Body");
                result = result.Replace(" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"", "");
                result = result.Replace(" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "");
                result = result.Replace(" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "");
                result = result.Replace(" xmlns=\"http://openmas.chinamobile.com/pulgin\"", "");
                result = result.Replace(string.Format(" xmlns=\"{0}\"", Namespace), "");
                return BaseXml.GetXmlString(result, string.Format("Envelope/Body/{0}Response/{0}Result", method)).Replace("&lt;", "<").Replace("&gt;", ">");
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() => { BaseLog.SaveLog(url + ":" + ex.ToString(), "PostSoap_exp"); });
                return null;
            }
        }
        #endregion
    }
}

/*
    GET  请求获取由Request-URI所标识的资源。
         
    POST  在Request-URI所标识的资源后附加新的数据。
         
    HEAD  请求获取由Request-URI所标识的资源的响应消息报头。
         
    OPTIONS  请求查询服务器的性能，或查询与资源相关的选项和需求。
         
    PUT   请求服务器存储一个资源，并用Request-URI作为其标识。
         
    DELETE  请求服务器删除由Request-URI所标识的资源。
         
    TRACE  请求服务器回送收到的请求信息，主要用语测试或诊断。         
 */
