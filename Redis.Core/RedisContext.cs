using ServiceStack.Redis;
using System.Collections.Generic;
using Untility.Core.Base;
using Redis.Core.Base;
using System.Threading;

namespace Redis.Core
{
    internal static class RedisContext
    {
        #region 获取上下文
        /// <summary>
        /// 获取上下文
        /// </summary>
        /// <returns></returns>
        public static PooledRedisClientManager GetContext(int db=0)
        {
            return ClientInfo(db);
        }
        #endregion


        #region 连接配置
        /// <summary>
        /// 连接配置
        /// </summary>
        /// <returns></returns>
        private static PooledRedisClientManager ClientInfo(int db=0)
        {
            //获取配置
            var config = BaseConfig.GetValue<ConfigModel>(AppSettingKey.Redis);

            //redis连接
            return new PooledRedisClientManager(config.WriteServerList.Split(',')
             , config.ReadServerList.Split(','), new RedisClientManagerConfig
             {
                 DefaultDb = db,
                 MaxReadPoolSize = config.MaxReadPoolSize == 0 ? 60 : config.MaxReadPoolSize,
                 MaxWritePoolSize = config.MaxWritePoolSize == 0 ? 60 : config.MaxWritePoolSize,
                 AutoStart = config.AutoStart
             });
        }
        #endregion
    }
}
