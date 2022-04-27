using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using System.Text.Json;
using System.Linq;
using NPOI.OpenXmlFormats.Dml.Diagram;
using System.Text.Unicode;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;

namespace FastUntility.Core.Base
{
    public static class BaseJsonSys
    {
        #region list 转json
        /// <summary>
        /// list 转json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        /// 
        public static string ListToJson<T>(List<T> list)
        {
            try
            {
                var jsonOption = new JsonSerializerOptions() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
                return JsonSerializer.Serialize<List<T>>(list, jsonOption);
            }
            catch
            {
                return "[]";
            }
        }
        #endregion

        #region model转json
        /// <summary>
        /// 说明：model转json
        /// </summary>
        /// <param name="Model">实体</param>
        /// <returns></returns>
        public static string ModelToJson(object model)
        {
            try
            {
                var jsonOption = new JsonSerializerOptions() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
                return JsonSerializer.Serialize(model, jsonOption);
            }
            catch
            {
                return "";
            }
        }
        #endregion

        #region Json转model
        /// <summary>
        /// 说明：Json转model
        /// </summary>
        /// <typeparam name="T">实体</typeparam>
        /// <param name="Json">json</param>
        /// <returns></returns>
        public static T JsonToModel<T>(string jsonValue) where T : class, new()
        {
            try
            {
                var jsonOption = new JsonSerializerOptions() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
                return JsonSerializer.Deserialize<T>(jsonValue, jsonOption);                
            }
            catch
            {
                return new T();
            }
        }
        #endregion

        #region Json转model
        /// <summary>
        /// Json转model
        /// </summary>
        /// <param name="jsonValue"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object JsonToModel(string jsonValue,Type type)
        {
            try
            {
                var jsonOption = new JsonSerializerOptions() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
                return JsonSerializer.Deserialize(jsonValue, type, jsonOption);
            }
            catch
            {
                return new object();
            }
        }
        #endregion

        #region json转list
        /// <summary>
        /// json转list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonValue"></param>
        /// <returns></returns>
        public static List<T> JsonToList<T>(string jsonValue) where T : class, new()
        {
            try
            {
                var jsonOption = new JsonSerializerOptions() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
                return JsonSerializer.Deserialize<List<T>>(jsonValue, jsonOption);
            }
            catch
            {
                return new List<T>();
            }
        }
        #endregion

        #region json转list
        /// <summary>
        /// json转list
        /// </summary>
        /// <param name="jsonValue"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<object> JsonToList(string jsonValue,Type type)
        {
            try
            {
                var list = new List<object>(); ;

                if (string.IsNullOrEmpty(jsonValue))
                    return list;

                using (var document = JsonDocument.Parse(jsonValue))
                {
                    foreach (var element in document.RootElement.EnumerateArray())
                    {
                        list.Add(JsonToModel(element.ToString(),type));
                    }
                }

                return list;
            }
            catch
            {
                return new List<object>();
            }
        }
        #endregion

        #region json转dic
        /// <summary>
        /// json转dic
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonValue"></param>
        /// <returns></returns>
        public static Dictionary<string, object> JsonToDic(string jsonValue)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonValue))
                    return new Dictionary<string, object>();

                var jsonOption = new JsonSerializerOptions() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
                return JsonSerializer.Deserialize<Dictionary<string, object>>(jsonValue, jsonOption);
            }
            catch
            {
                return new Dictionary<string, object>();
            }
        }
        #endregion

        #region json转dics
        /// <summary>
        /// json转dics
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonValue"></param>
        /// <returns></returns>
        public static List<Dictionary<string, object>> JsonToDics(string jsonValue)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonValue))
                    return new List<Dictionary<string, object>>();

                var jsonOption = new JsonSerializerOptions() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
                return JsonSerializer.Deserialize<List<Dictionary<string, object>>>(jsonValue, jsonOption);
            }
            catch
            {
                return new List<Dictionary<string, object>>();
            }
        }
        #endregion
    }
}
