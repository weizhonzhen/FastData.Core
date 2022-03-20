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
    public static class BaseJson
    {
        #region list 转json
        /// <summary>
        /// list 转json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string ListToJson<T>(List<T> list)
        {
            var sb = new StringBuilder();

            sb.Append("[");
            try
            {
                list.ForEach(a => { sb.Append(ModelToJson(a) + ","); });

                sb.Append("]").Replace(",]", "]");

                return sb.ToString();
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
                var jsonOption = new JsonSerializerOptions() { Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) };
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
                var jsonOption = new JsonSerializerOptions() { Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) };
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
                var jsonOption = new JsonSerializerOptions() { Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) };
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
                var list = new List<T>(); ;

                if (string.IsNullOrEmpty(jsonValue))
                    return list;

                using (var document = JsonDocument.Parse(jsonValue))
                {
                    foreach (var element in document.RootElement.EnumerateArray())
                    {
                        list.Add(JsonToModel<T>(element.ToString()));
                    }
                }

                return list;
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
                var item = new Dictionary<string, object>();

                if (string.IsNullOrEmpty(jsonValue))
                    return item;

                using (var document = JsonDocument.Parse(jsonValue))
                {
                    foreach (var element in document.RootElement.EnumerateObject())
                    {
                        item.Add(element.Name, element.Value.GetRawText());
                    }
                }
                return item;
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
                var item = new List<Dictionary<string, object>>();

                if (string.IsNullOrEmpty(jsonValue))
                    return item;

                using (var document = JsonDocument.Parse(jsonValue))
                {
                    foreach (var element in document.RootElement.EnumerateArray())
                    {
                        foreach(var temp in ((JsonElement)element).EnumerateObject())
                        {
                            var dic = new Dictionary<string, object>();
                            dic.Add(temp.Name, temp.Value.GetRawText());
                            item.Add(dic);
                        }
                    }
                }

                return item;
            }
            catch
            {
                return new List<Dictionary<string, object>>();
            }
        }
        #endregion

        #region datareader to json
        /// <summary>
        /// datareader to json
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        public static string DataReaderToJson(DbDataReader reader, bool isOracle = false)
        {
            var jsonOption = new JsonSerializerOptions() { Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) };
            var result = new List<Dictionary<string, object>>();
            var cols = GetCol(reader);

            while (reader.Read())
            {
                var dic = new Dictionary<string, object>();

                cols.ForEach(a =>
                {
                    if (reader[a] is DBNull)
                        dic.Add(a.ToLower(), "");
                    else if(isOracle)
                        ReadOracle(reader, a, dic);
                    else
                        dic.Add(a.ToLower(), reader[a]);
                });

                result.Add(dic);
            }

            return JsonSerializer.Serialize(result, jsonOption);
        }
        #endregion

        #region datareader to List<Dictionary<string, object>>
        /// <summary>
        /// datareader to List<Dictionary<string, object>>
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        public static List<Dictionary<string, object>> DataReaderToDic(DbDataReader reader, bool isOracle = false)
        {
            var result = new List<Dictionary<string, object>>();
            var cols = GetCol(reader);

            while (reader.Read())
            {
                var dic = new Dictionary<string, object>();
                cols.ForEach(a =>
                {
                    if (reader[a] is DBNull)
                        dic.Add(a.ToLower(), "");
                    else if (isOracle)
                        ReadOracle(reader, a, dic);
                });

                result.Add(dic);
            }

            return result;
        }
        #endregion

        private static void ReadOracle(DbDataReader reader, string a, Dictionary<string, object> dic)
        {
            var id = reader.GetOrdinal(a.ToUpper());
            var typeName = reader.GetDataTypeName(id).ToLower();
            if (typeName == "clob" || typeName == "nclob")
            {
                var temp = BaseEmit.Invoke(reader, reader.GetType().GetMethod("GetOracleClob"), new object[] { id });
                dic.Add(a.ToLower(), BaseEmit.Get(temp, "Value"));
                BaseEmit.Invoke(temp, temp.GetType().GetMethod("Close"), null);
                BaseEmit.Invoke(temp, temp.GetType().GetMethod("Dispose"), null);
            }
            else if (typeName == "blob")
            {
                var temp = BaseEmit.Invoke(reader, reader.GetType().GetMethod("GetOracleBlob"), new object[] { id });
                dic.Add(a.ToLower(), BaseEmit.Get(temp, "Value"));
                BaseEmit.Invoke(temp, temp.GetType().GetMethod("Close"), null);
                BaseEmit.Invoke(temp, temp.GetType().GetMethod("Dispose"), null);
            }
            else
                dic.Add(a.ToLower(), reader[a]);
        }

        #region get datareader col
        private static List<string> GetCol(DbDataReader dr)
        {
            var list = new List<string>();
            for (var i = 0; i < dr.FieldCount; i++)
            {
                list.Add(dr.GetName(i));
            }
            return list;
        }
        #endregion
    }
}
