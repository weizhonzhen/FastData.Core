using FastData.Core.Model;
using FastUntility.Core.Base;

namespace FastData.Core.Base
{
    internal static class DataConfig
    {
        /// <summary>
        /// 获取配置实体
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static ConfigModel Get(string key=null)
        {
            var item = new ConfigModel();
            if (key != null)
                item = BaseConfig.GetListValue<ConfigModel>(AppSettingKey.Config,"db.json").Find(a => a.Key == key);
            else
                item = BaseConfig.GetListValue<ConfigModel>(AppSettingKey.Config,"db.json")[0] ?? new ConfigModel();

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
