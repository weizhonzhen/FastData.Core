using FastData.Core.Model;
using FastUntility.Core.Base;
using System.Collections.Generic;

namespace FastData.Core.Base
{
    public static class DataConfig
    {
        /// <summary>
        /// 获取配置实体
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static ConfigModel Get(string key=null)
        {
            var list = new List<ConfigModel>();
            var item = new ConfigModel();
            var cacheKey = key == null ? "config" : string.Format("config.{0}", key);

            if (DbCache.Exists(CacheType.Web, cacheKey))
                list = DbCache.Get<List<ConfigModel>>(CacheType.Web,cacheKey);
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
    }
}
