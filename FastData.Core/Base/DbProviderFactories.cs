using System;
using System.Data.Common;
using System.Reflection;
using FastData.Core.Model;
using System.Linq;

namespace FastData.Core.Base
{
    internal class DbProviderFactories : DbProviderFactory
    {
        /// <summary>
        /// 动态加载数据库工厂
        /// </summary>
        /// <param name="providerInvariantName"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static DbProviderFactory GetFactory(ConfigModel config)
        {
            try
            {
                var assembly = AppDomain.CurrentDomain.GetAssemblies().ToList().Find(a => a.FullName.Split(',')[0] == config.ProviderName);
                if (assembly == null)
                    assembly = Assembly.Load(config.ProviderName);

                var type = assembly.GetType(config.FactoryClient, false);
                object instance = null;

                if (type != null)
                    instance = type.InvokeMember("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.GetField | BindingFlags.GetProperty, null, type, null);
                
                return instance as DbProviderFactory;
            }
            catch (Exception ex)
            {
                DbLog.LogException(config.IsOutError, config.DbType, ex, "GetFactory", "");
                return null;
            }
        }
    }
}
