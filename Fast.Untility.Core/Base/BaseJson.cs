using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fast.Untility.Core.Base
{
    public static class BaseJson
    {
        #region json键是否存在或空值
        /// <summary>
        /// 标签：2015.7.13，魏中针
        /// 说明：json键是否存在或空值
        /// </summary>
        /// <param name="Key">json键</param>
        /// <param name="Jo">json对象</param>
        /// <returns></returns>
        public static bool JsonIsNull(string key, JObject jo)
        {
            if (jo.Property(key) == null || jo[key].ToString() == "")
                return true;
            else
                return false;
        }
        #endregion

        #region 获取json键值
        /// <summary>
        /// 标签：2015.7.13，魏中针
        /// 说明：获取json键值
        /// </summary>
        /// <param name="Key">json键</param>
        /// <param name="ReturnValue">json键为空时,默认值</param>
        /// <param name="Item">json 对象</param>
        /// <returns></returns>
        public static string JsonValue(string key, string returnValue, JObject item)
        {
            if (item.Property(key) == null || item[key].ToString() == "")
                return returnValue;
            else
                return item[key].ToString();
        }
        #endregion

        #region list 转json
        /// <summary>
        /// list 转json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string ListToJson<T>(List<T> list)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("[");
            try
            {
                foreach (var item in list)
                {
                    sb.Append(ModelToJson(item) + ",");
                }

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
        /// 标签：2015.7.13，魏中针
        /// 说明：model转json
        /// </summary>
        /// <param name="Model">实体</param>
        /// <returns></returns>
        public static string ModelToJson(object model)
        {
            try
            {
                return JsonConvert.SerializeObject(model).ToString();
            }
            catch
            {
                return "";
            }
        }
        #endregion

        #region Json转model
        /// <summary>
        /// 标签：2015.7.13，魏中针
        /// 说明：Json转model
        /// </summary>
        /// <typeparam name="T">实体</typeparam>
        /// <param name="Json">json</param>
        /// <returns></returns>
        public static T JsonToModel<T>(string jsonValue) where T : class, new()
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(jsonValue);
            }
            catch
            {
                return new T();
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

                var ja = JArray.Parse(jsonValue);

                foreach (var jo in ja)
                {
                    list.Add(JsonToModel<T>(jo.ToString()));
                }
                return list;
            }
            catch
            {
                return new List<T>();
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

                var jo = JObject.Parse(jsonValue);

                foreach (var temp in jo)
                {
                    item.Add(temp.Key, temp.Value);
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

                var ja = JArray.Parse(jsonValue);

                foreach (var jo in ja)
                {
                    item.Add(JsonToDic(jo.ToString()));
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
        public static string DataReaderToJson(DbDataReader reader)
        {
            var result = new List<Dictionary<string, object>>();
            var cols = new List<string>();

            //列名
            for (var i = 0; i < reader.FieldCount; i++)
                cols.Add(reader.GetName(i));

            while (reader.Read())
            {
                var dic = new Dictionary<string, object>();
                foreach (var col in cols)
                {
                    if (reader[col] is DBNull)
                        dic.Add(col, "");
                    else
                        dic.Add(col, reader[col]);
                }

                result.Add(dic);
            }

            return JsonConvert.SerializeObject(result, Formatting.None);
        }
        #endregion

        #region datareader to List<Dictionary<string, object>>
        /// <summary>
        /// datareader to List<Dictionary<string, object>>
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        public static List<Dictionary<string, object>> DataReaderToDic(DbDataReader reader, string delCol = "")
        {
            var result = new List<Dictionary<string, object>>();
            var cols = new List<string>();

            //列名
            for (var i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).ToLower() != delCol.ToLower())
                    cols.Add(reader.GetName(i));
            }

            while (reader.Read())
            {
                var dic = new Dictionary<string, object>();
                foreach (var col in cols)
                {
                    if (reader[col] is DBNull)
                        dic.Add(col.ToLower(), "");
                    else
                        dic.Add(col.ToLower(), reader[col]);
                }

                result.Add(dic);
            }

            return result;
        }
        #endregion
    }
}
