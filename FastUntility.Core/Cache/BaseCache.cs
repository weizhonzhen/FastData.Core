using FastUntility.Core.Base;
using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Text;

namespace FastUntility.Core.Cache
{
    /// <summary>
    /// 缓存
    /// </summary>
    public static class BaseCache
    {
        public static ObjectCache cache =MemoryCache.Default;

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
                cache.Remove(key);
                var policy = new CacheItemPolicy();
                policy.AbsoluteExpiration = DateTime.Now.AddHours(Hours);
                cache.Set(key, value, policy);
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
                cache.Remove(key);
                var policy = new CacheItemPolicy();
                policy.AbsoluteExpiration = DateTime.Now.AddHours(Hours);
                cache.Set(key, value, policy);
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
            if (!string.IsNullOrEmpty(key))
                return cache.Contains(key);
            else
                return false;
        }
    }
}
