using FastRedis.Core.Repository;
using FastUntility.Core;
using FastUntility.Core.Cache;

namespace FastData.Core.Base
{
    /// <summary>
    /// 缓存
    /// </summary>
    internal static class DbCache
    {
        /// <summary>
        /// 设置缓存
        /// </summary>
        public static void Set(string cacheType, string key, string value, int Hours = 8640)
        {
            var IRedis = ServiceContext.Engine.Resolve<IRedisRepository>();
            if (cacheType.ToLower() == CacheType.Web)
                BaseCache.Set(key, value, Hours);
            else if (cacheType.ToLower() == CacheType.Redis)
                IRedis.SetAsy(key, value, Hours);
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        public static void Set<T>(string cacheType, string key, T value, int Hours = 8640) where T : class, new()
        {
            var IRedis = ServiceContext.Engine.Resolve<IRedisRepository>();
            if (cacheType.ToLower() == CacheType.Web)
                BaseCache.Set<T>(key, value, Hours);
            else if (cacheType.ToLower() == CacheType.Redis)
                IRedis.SetAsy<T>(key, value, Hours);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        public static string Get(string cacheType, string key)
        {
            var IRedis = ServiceContext.Engine.Resolve<IRedisRepository>();
            if (cacheType.ToLower() == CacheType.Web)
                return BaseCache.Get(key);
            else if (cacheType.ToLower() == CacheType.Redis)
               return IRedis.GetAsy(key).Result;

            return "";
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        public static T Get<T>(string cacheType, string key) where T : class, new()
        {
            var IRedis = ServiceContext.Engine.Resolve<IRedisRepository>();
            if (cacheType.ToLower() == CacheType.Web)
                return BaseCache.Get<T>(key);
            else if (cacheType.ToLower() == CacheType.Redis)
                return IRedis.GetAsy<T>(key).Result;
            return new T();
        }

        /// <summary>
        /// 删除缓存
        /// </summary>
        public static void Remove(string cacheType, string key)
        {
            var IRedis = ServiceContext.Engine.Resolve<IRedisRepository>();
            if (cacheType.ToLower() == CacheType.Web)
                BaseCache.Remove(key);
            else if (cacheType.ToLower() == CacheType.Redis)
                IRedis.RemoveAsy(key);
        }

        /// <summary>
        /// 是否存在
        /// </summary>
        public static bool Exists(string cacheType, string key)
        {
            var IRedis = ServiceContext.Engine.Resolve<IRedisRepository>();
            if (cacheType.ToLower() == CacheType.Web)
               return  BaseCache.Exists(key);
            else if (cacheType.ToLower() == CacheType.Redis)
                return IRedis.ExistsAsy(key).Result;
            return false;
        }
    }
}
