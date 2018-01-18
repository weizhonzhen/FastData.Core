using Data.Core.Model;
using Untility.Core.Base;

namespace Data.Core.Base
{
    internal static class DataConfig
    {
        /// <summary>
        /// 获取配置实体
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static ConfigModel Write(string key=null)
        {
            var item = new ConfigModel();
            if (key != null)
                item = BaseConfig.GetListValue<ConfigModel>(AppSettingKey.Write).Find(a => a.Key == key);
            else
                item = BaseConfig.GetListValue<ConfigModel>(AppSettingKey.Write)[0] ?? new ConfigModel();

            if (item.DesignModel == "")
                item.DesignModel = Config.DbFirst;

            item.IsPropertyCache = true;
            item.DbType = item.DbType.ToLower();

            return item;
        }

        /// <summary>
        /// 获取配置实体
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static ConfigModel Read(string key=null)
        {
            var item = new ConfigModel();
            if (key != null)
                item = BaseConfig.GetListValue<ConfigModel>(AppSettingKey.Read).Find(a => a.Key == key);
            else
                item = BaseConfig.GetListValue<ConfigModel>(AppSettingKey.Read)[0] ?? new ConfigModel();

            if (item.DesignModel == "")
                item.DesignModel = Config.DbFirst;
            
            item.DbType = item.DbType.ToLower();

            return item;
        }
    }
}
