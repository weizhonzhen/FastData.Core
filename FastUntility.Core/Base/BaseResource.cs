using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FastUntility.Core.Base
{
    public static class BaseResource
    {
        /// <summary>
        /// 获取资源文件
        /// </summary>
        public static string GetValue(string projectName,string file)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().ToList().Find(a => a.FullName.Split(',')[0] == projectName);
            if (assembly == null)
                assembly = Assembly.Load(projectName);
            using (var resource = assembly.GetManifestResourceStream(string.Format("{0}.{1}", projectName, file)))
            {
                if (resource != null)
                {
                    using (var reader = new StreamReader(resource))
                    {
                        return reader.ReadToEnd();
                    }
                }
                else
                    return "";
            }
        }

        #region 获取资源文件 json
        /// <summary>
        /// 获取资源文件
        /// </summary>
        public static T GetValue<T>(string key, string projectName, string jsonFile) where T : class, new()
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().ToList().Find(a => a.FullName.Split(',')[0] == projectName);
            if (assembly == null)
                assembly = Assembly.Load(projectName);
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

        #region 获取资源文件 json
        /// <summary>
        /// 获取资源文件 json
        /// </summary>
        public static List<T> GetListValue<T>(string key, string projectName, string jsonFile) where T : class, new()
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().ToList().Find(a => a.FullName.Split(',')[0] == projectName);
            if (assembly == null)
                assembly = Assembly.Load(projectName);
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
