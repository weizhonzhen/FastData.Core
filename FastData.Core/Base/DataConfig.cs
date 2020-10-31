using FastData.Core.Model;
using FastData.Core.Type;
using FastUntility.Core.Base;
using FastUntility.Core.BuilderMethod;
using NPOI.SS.Formula.Functions;
using System.Collections.Generic;
using System.Linq;

namespace FastData.Core.Base
{
    public static class DataConfig
    {
        /// <summary>
        /// 获取配置实体
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static ConfigModel Get(string key = null)
        {
            var list = new List<ConfigModel>();
            var item = new ConfigModel();
            var cacheKey = key == null ? "config" : string.Format("config.{0}", key);

            if (DbCache.Exists(CacheType.Web, cacheKey))
                list = DbCache.Get<List<ConfigModel>>(CacheType.Web, cacheKey);
            else
            {
                list = BaseConfig.GetListValue<ConfigModel>(AppSettingKey.Config, "db.json");
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

            return item;
        }

        public static bool DataType(string key = null)
        {
            var list = new List<ConfigModel>();
            var cacheKey = key == null ? "config" : string.Format("config.{0}", key);

            if (DbCache.Exists(CacheType.Web, cacheKey))
                list = DbCache.Get<List<ConfigModel>>(CacheType.Web, cacheKey);
            else
            {
                list = BaseConfig.GetListValue<ConfigModel>(AppSettingKey.Config, "db.json");
                DbCache.Set<List<ConfigModel>>(CacheType.Web, cacheKey, list);
            }

            var result = new List<bool>();
            result.Add(list.Count(a => a.DbType.ToLower() == DataDbType.Oracle.ToLower()) > 0);
            result.Add(list.Count(a => a.DbType.ToLower() == DataDbType.DB2.ToLower()) > 0);
            result.Add(list.Count(a => a.DbType.ToLower() == DataDbType.MySql.ToLower()) > 0);
            result.Add(list.Count(a => a.DbType.ToLower() == DataDbType.PostgreSql.ToLower()) > 0);
            result.Add(list.Count(a => a.DbType.ToLower() == DataDbType.SQLite.ToLower()) > 0);
            result.Add(list.Count(a => a.DbType.ToLower() == DataDbType.SqlServer.ToLower()) > 0);

            return result.Count(a => a == true) > 1;
        }
    }
}
