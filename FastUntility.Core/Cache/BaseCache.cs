using FastUntility.Core.Base;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace FastUntility.Core.Cache
{
    /// <summary>
    /// 缓存
    /// </summary>
    public static class BaseCache
    {
        public static MemoryCache cache = new MemoryCache(new MemoryCacheOptions());

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="Hours">过期小时</param>
        public static void Set(string key, string value,int Hours = 24 * 30 * 12)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(Hours));
                cache.Remove(key);
                cache.Set(key, value, options);
            }
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="Hours">过期小时</param>
        public static void Set<T>(string key, T value, int Hours = 24 * 30 * 12) where T : class, new()
        {
            if (!string.IsNullOrEmpty(key))
            {
                var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(Hours));
                cache.Remove(key);
                cache.Set(key, value, options);
            }
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="key">键</param>
        public static string Get(string key)
        {
            try
            {
                if (!string.IsNullOrEmpty(key))
                    return cache.Get(key).ToStr();
                else
                    return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="key">键</param>
        public static T Get<T>(string key) where T : class, new()
        {
            try
            {
                if (!string.IsNullOrEmpty(key))
                {
                    var result = new T();
                    var obj = cache.Get(key);
                    if (obj != null)
                        result = (T)obj;
                    return result;
                }
                else
                    return new T();
            }
            catch
            {
                return new T();
            }
        }

        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="key">键</param>
        public static void Remove(string key)
        {
            if (!string.IsNullOrEmpty(key))
                cache.Remove(key);
        }

        /// <summary>
        /// 是否存在
        /// </summary>
        /// <param name="key">键</param>
        public static bool Exists(string key)
        {
            object result;
            if (!string.IsNullOrEmpty(key))
                return cache.TryGetValue(key, out result);
            else
                return false;
        }
    }
}
