﻿using FastUntility.Core.Base;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace FastUntility.Core.Base
{
    public static class BaseResource
    {
        #region 获取资源文件
        /// <summary>
        /// 获取资源文件
        /// </summary>
        public static T GetValue<T>(string key, string projectName, string jsonFile) where T : class, new()
        {
            var assembly = Assembly.Load(projectName);
            using (var resource = assembly.GetManifestResourceStream(string.Format("{0}.{1}", projectName, jsonFile)))
            {
                if (resource != null)
                {
                    using (var reader = new StreamReader(resource))
                    {
                        var content = reader.ReadToEnd();
                        return BaseJson.JsonToModel<T>(BaseJson.ModelToJson(BaseJson.JsonToDic(content).GetValue(key)));
                    }
                }
                else
                    return new T();
            }
        }
        #endregion

        #region 获取资源文件
        /// <summary>
        /// 获取资源文件
        /// </summary>
        public static List<T> GetListValue<T>(string key, string projectName, string jsonFile) where T : class, new()
        {
            var assembly = Assembly.Load(projectName);
            using (var resource = assembly.GetManifestResourceStream(string.Format("{0}.{1}", projectName, jsonFile)))
            {
                if (resource != null)
                {
                    using (var reader = new StreamReader(resource))
                    {
                        var content = reader.ReadToEnd();
                        return BaseJson.JsonToModel<List<T>>(BaseJson.ModelToJson(BaseJson.JsonToDic(content).GetValue(key)));
                    }
                }
                else
                    return new List<T>();
            }
        }
        #endregion
    }
}
