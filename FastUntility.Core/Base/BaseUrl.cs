using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace FastUntility.Core.Base
{
    /// <summary>
    /// post、get、put到url
    /// </summary>
    public static class BaseUrl
    {
        private static readonly IHttpClientFactory httpClientFactory;

        #region get url(select)
        /// <summary>
        /// get url(select)
        /// </summary>
        public static string GetUrl(string url, string name = "")
        {
            try
            {
                var http = httpClientFactory.CreateClient(name);
                http.DefaultRequestHeaders.Connection.Add("keep-alive");
                var response = http.GetAsync(new Uri(url)).Result;
                response.EnsureSuccessStatusCode();
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
        public static string PostUrl(string url, Dictionary<string, object> dic, string name = "", string mediaType = "application/json")
        {
            try
            {
                var count = 0;
                foreach (var item in dic)
                {
                    if (count == 0)
                        url = string.Format("{0}?{1}={2}", url, item.Key, item.Value);
                    else
                        url = string.Format("{0}&{1}={2}", url, item.Key, item.Value);
                    count++;
                }

                var content = new StringContent("", Encoding.UTF8, mediaType);
                var http = httpClientFactory.CreateClient(name);
                http.DefaultRequestHeaders.Connection.Add("keep-alive");
                var response = http.PostAsync(new Uri(url), content).Result;
                response.EnsureSuccessStatusCode();
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
        public static string PostContent(string url, Dictionary<string, object> dic, string name = "", string mediaType = "application/json")
        {
            try
            {
                var content = new StringContent(BaseJson.ModelToJson(dic), Encoding.UTF8, mediaType);
                var http = httpClientFactory.CreateClient(name);
                http.DefaultRequestHeaders.Connection.Add("keep-alive");
                var response = http.PostAsync(new Uri(url), content).Result;
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() => { BaseLog.SaveLog(url + ":" + ex.ToString(), "PostUrl_exp"); });
                return null;
            }
        }
        #endregion

        #region put url (update)
        /// <summary>
        /// put url(update)
        /// </summary>
        public static string PutUrl(string url, Dictionary<string, object> dic, string name = "", string mediaType = "application/json")
        {
            try
            {
                var count = 0;
                foreach (var item in dic)
                {
                    if (count == 0)
                        url = string.Format("{0}?{1}={2}", url, item.Key, item.Value);
                    else
                        url = string.Format("{0}&{1}={2}", url, item.Key, item.Value);
                    count++;
                }

                var content = new StringContent("", Encoding.UTF8, mediaType);
                var http = httpClientFactory.CreateClient(name);
                http.DefaultRequestHeaders.Connection.Add("keep-alive");
                var response = http.PostAsync(new Uri(url), content).Result;
                response.EnsureSuccessStatusCode();
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
        public static string PutContent(string url, Dictionary<string, object> dic, string name = "", string mediaType = "application/json")
        {
            try
            {
                var content = new StringContent(BaseJson.ModelToJson(dic), Encoding.UTF8, mediaType);
                var http = httpClientFactory.CreateClient(name);
                http.DefaultRequestHeaders.Connection.Add("keep-alive");
                var response = http.PostAsync(new Uri(url), content).Result;
                response.EnsureSuccessStatusCode();
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
        public static string DeleteUrl(string url, string name = "")
        {
            try
            {
                var http = httpClientFactory.CreateClient(name);
                http.DefaultRequestHeaders.Connection.Add("keep-alive");
                var response = http.DeleteAsync(new Uri(url)).Result;
                response.EnsureSuccessStatusCode();
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

        #region send url
        /// <summary>
        /// send url
        /// </summary>
        public static string SendUrl(string url, HttpMethod mothod, string name = "")
        {
            try
            {
                var mes = new HttpRequestMessage(mothod, new Uri(url));
                var http = httpClientFactory.CreateClient(name);
                http.DefaultRequestHeaders.Connection.Add("keep-alive");
                var response = http.SendAsync(mes).Result;
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() => { BaseLog.SaveLog(url + ":" + ex.ToString(), "SendUrl_exp"); });
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
