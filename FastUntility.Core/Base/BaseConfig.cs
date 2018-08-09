using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;

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
        public static T GetValue<T>(string key,string fileName= "appsettings.json") where T : class, new()
        {            
            var build = new ConfigurationBuilder();

            //内存
            build.AddInMemoryCollection();

            //目录
            build.SetBasePath(Directory.GetCurrentDirectory());

            //加载配置文件
            build.AddJsonFile(fileName, optional: true, reloadOnChange: true);

            //编译成对象
            var config = build.Build();

            return new ServiceCollection().AddOptions().Configure<T>(config.GetSection(key)).BuildServiceProvider().GetService<IOptions<T>>().Value;
        }
        #endregion
        
        #region 获取配置文件
        /// <summary>
        /// 获取配置文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键名</param>
        /// <returns></returns>
        public static List<T> GetListValue<T>(string key, string fileName = "appsettings.json") where T : class, new()
        {
            var build = new ConfigurationBuilder();

            //内存
            build.AddInMemoryCollection();

            //目录
            build.SetBasePath(Directory.GetCurrentDirectory());

            //加载配置文件
            build.AddJsonFile(fileName, optional: true, reloadOnChange: true);

            //编译成对象
            var config = build.Build();

            return new ServiceCollection().AddOptions().Configure<List<T>>(config.GetSection(key)).BuildServiceProvider().GetService<IOptions<List<T>>>().Value;
        }
        #endregion
    }
}
