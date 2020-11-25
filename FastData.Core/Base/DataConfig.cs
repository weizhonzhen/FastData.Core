using FastData.Core.Model;
using FastData.Core.Type;
using FastUntility.Core.Base;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FastData.Core.Base
{
    public static class DataConfig
    {
        /// <summary>
        /// 获取配置实体
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static ConfigModel Get(string key = null, string projectName = null,string dbFile= "db.json")
        {
            var list = new List<ConfigModel>();
            var item = new ConfigModel();
            var cacheKey = "FastData.Core.Config";

            if (DbCache.Exists(CacheType.Web, cacheKey))
                list = DbCache.Get<List<ConfigModel>>(CacheType.Web, cacheKey);
            else if (projectName != null)
            {
                var assembly = Assembly.Load(projectName);
                using (var resource = assembly.GetManifestResourceStream(string.Format("{0}.{1}", projectName,dbFile)))
                {
                    if (resource != null)
                    {
                        using (var reader = new StreamReader(resource))
                        {
                            var content = reader.ReadToEnd();
                            list = BaseJson.JsonToList<ConfigModel>(BaseJson.ModelToJson(BaseJson.JsonToDic(content).GetValue("DataConfig")));
                            list.ForEach(a => { a.IsUpdateCache = false; });
                            DbCache.Set<List<ConfigModel>>(CacheType.Web, cacheKey, list);
                        }
                    }
                    else
                    {
                        list = BaseConfig.GetListValue<ConfigModel>(AppSettingKey.Config, dbFile);
                        DbCache.Set<List<ConfigModel>>(CacheType.Web, cacheKey, list);
                    }
                }
            }
            else
            {
                list = BaseConfig.GetListValue<ConfigModel>(AppSettingKey.Config, dbFile);
                DbCache.Set<List<ConfigModel>>(CacheType.Web, cacheKey, list);
            }

            if (string.IsNullOrEmpty(key))
                item = list[0];
            else
                item = list.Find(a => a.Key.ToLower() == key.ToLower());

            if (item.DesignModel == "")
                item.DesignModel = Config.DbFirst;

            if (item.SqlErrorType == "")
                item.SqlErrorType = SqlErrorType.File;

            if (item.CacheType == "")
                item.CacheType = CacheType.Web;

            item.IsPropertyCache = true;
            item.DbType = item.DbType.ToLower();

            if (projectName != null)
                item.IsUpdateCache = false;
            return item;
        }

        public static bool DataType(string key = null, string projectName = null, string dbFile = "db.json")
        {
            var cacheKey = "FastData.Core.Config";

            if (!DbCache.Exists(CacheType.Web, cacheKey))
                DataConfig.Get(key, projectName, dbFile);

            var list = BaseConfig.GetListValue<ConfigModel>(AppSettingKey.Config, dbFile);

            var result = new List<bool>();
            result.Add(list.Count(a => a.DbType.ToLower() == DataDbType.Oracle) > 0);
            result.Add(list.Count(a => a.DbType.ToLower() == DataDbType.DB2) > 0);
            result.Add(list.Count(a => a.DbType.ToLower() == DataDbType.MySql) > 0);
            result.Add(list.Count(a => a.DbType.ToLower() == DataDbType.PostgreSql) > 0);
            result.Add(list.Count(a => a.DbType.ToLower() == DataDbType.SQLite) > 0);
            result.Add(list.Count(a => a.DbType.ToLower() == DataDbType.SqlServer) > 0);

            return result.Count(a => a == true) > 1;
        }
    }
}
