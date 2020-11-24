using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;
using FastUntility.Core.Cache;

namespace FastUntility.Core.Base
{
    /// <summary>
    /// 获取配置文件
    /// </summary>
    public static class BaseConfig
    {
        #region 获取配置文件
        /// <summary>
        /// 获取配置文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键名</param>
        /// <returns></returns>
        public static T GetValue<T>(string key,string fileName= "appsettings.json", bool isCache = true) where T : class, new()
        {
            var cacheKey = string.Format("json.{0}.{1}", key, fileName);
            if (isCache && BaseCache.Exists(cacheKey))
                return BaseCache.Get<T>(cacheKey);

            var build = new ConfigurationBuilder();
            build.SetBasePath(Directory.GetCurrentDirectory());
            build.AddJsonFile(fileName, optional: true, reloadOnChange: true);
            var config = build.Build();
            var item = new ServiceCollection().AddOptions().Configure<T>(config.GetSection(key)).BuildServiceProvider().GetService<IOptions<T>>().Value;
            if (isCache)
                BaseCache.Set<T>(cacheKey, item);

            return item;
        }
        #endregion
        
        #region 获取配置文件
        /// <summary>
        /// 获取配置文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键名</param>
        /// <returns></returns>
        public static List<T> GetListValue<T>(string key, string fileName = "appsettings.json",bool isCache=true) where T : class, new()
        {
            var cacheKey = string.Format("json.{0}.{1}", key, fileName);
            if (isCache && BaseCache.Exists(cacheKey))
                return BaseCache.Get<List<T>>(cacheKey);

            var build = new ConfigurationBuilder();
            build.SetBasePath(Directory.GetCurrentDirectory());
            build.AddJsonFile(fileName, optional: true, reloadOnChange: true);
            var config = build.Build();
            var list = new ServiceCollection().AddOptions().Configure<List<T>>(config.GetSection(key)).BuildServiceProvider().GetService<IOptions<List<T>>>().Value;

            if (isCache)
                BaseCache.Set<List<T>>(cacheKey, list);

            return list;
        }
        #endregion
    }
}
