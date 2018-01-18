using System;
using System.Data.Common;
using System.Reflection;
using Data.Core.Model;
using Data.Core.Type;

namespace Data.Core.Base
{
    internal class DbProviderFactories: DbProviderFactory
    {
        /// <summary>
        /// 动态加载数据库工厂
        /// </summary>
        /// <param name="providerInvariantName"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static DbProviderFactory GetFactory(ConfigModel config)
        {
            if (config.DbType.ToLower() == DataDbType.SqlServer.ToLower())
                return System.Data.SqlClient.SqlClientFactory.Instance;
            else if (config.DbType.ToLower() == DataDbType.MySql.ToLower())
                return MySql.Data.MySqlClient.MySqlClientFactory.Instance;
            else
            {
                var assembly = Assembly.LoadFile(string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, config.ProviderName));
                return assembly.CreateInstance(config.FactoryClient) as DbProviderFactory;
            }
        }
    }
}
