using FastUntility.Core.Base;
using StackExchange.Redis;

namespace FastRedis.Core
{
    public static class RedisContext
    {
        #region 获取上下文
        /// <summary>
        /// 获取上下文
        /// </summary>
        /// <returns></returns>
        public static ConnectionMultiplexer GetContext()
        {
            var config = BaseConfig.GetValue<ConfigModel>(AppSettingKey.Redis, "db.json");
            return ConnectionMultiplexer.Connect(config.Server);
        }
        #endregion
    }
}
