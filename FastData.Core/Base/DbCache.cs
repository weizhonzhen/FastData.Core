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
            if (string.Compare(cacheType, CacheType.Web, false) == 0)
                BaseCache.Set(key, value, Hours);
            else if (string.Compare( cacheType,CacheType.Redis,false)==0)
                IRedis.SetAsy(key, value, Hours);
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        public static void Set<T>(string cacheType, string key, T value, int Hours = 8640) where T : class, new()
        {
            if (string.Compare(cacheType, CacheType.Web, false) == 0)
                BaseCache.Set<T>(key, value, Hours);
            else if (string.Compare(cacheType, CacheType.Redis, false) == 0)
                IRedis.SetAsy<T>(key, value, Hours);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        public static string Get(string cacheType, string key)
        {           
            if (string.Compare(cacheType, CacheType.Web, false) == 0)
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
            if (string.Compare(cacheType, CacheType.Web, false) == 0)
                return BaseCache.Get<T>(key);
            else if (string.Compare(cacheType, CacheType.Redis, false) == 0)
                return IRedis.GetAsy<T>(key).Result;
            return new T();
        }

        /// <summary>
        /// 删除缓存
        /// </summary>
        public static void Remove(string cacheType, string key)
        {
            if (string.Compare(cacheType, CacheType.Web, false) == 0)
                BaseCache.Remove(key);
            else if (string.Compare(cacheType, CacheType.Redis, false) == 0)
                IRedis.RemoveAsy(key);
        }

        /// <summary>
        /// 是否存在
        /// </summary>
        public static bool Exists(string cacheType, string key)
        {
            if (string.Compare(cacheType, CacheType.Web, false) == 0)
               return  BaseCache.Exists(key);
            else if (string.Compare(cacheType, CacheType.Redis, false) == 0)
                return IRedis.ExistsAsy(key).Result;
            return false;
        }

        private static IRedisRepository IRedis 
        {
            get
            {
                var IRedis = ServiceContext.Engine.Resolve<IRedisRepository>();
                if (IRedis == null)
                    throw new System.Exception("ConfigureServices first add services.AddFastRedis;");
                return IRedis;
            }
        }
    }
}
