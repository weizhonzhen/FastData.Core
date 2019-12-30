using System;
using System.Threading.Tasks;
using System.IO;
using StackExchange.Redis;
using FastUntility.Core.Base;

namespace FastRedis.Core
{
    /// <summary>
    /// RedisContext.GetContext()操作类
    /// </summary>
    public static class RedisInfo
    {
        private static readonly int _db = 0;
        private static readonly ConnectionMultiplexer Context;
        static RedisInfo()
        {
            var config = BaseConfig.GetValue<ConfigModel>(AppSettingKey.Redis, "db.json");
            _db = config.Db;
            Context = ConnectionMultiplexer.Connect(config.Server);
        }

        #region 是否存在
        /// <summary>
        /// 是否存在 
        /// </summary>
        /// <returns></returns>
        public static bool Exists(string key, int db = 0)
        {
            try
            {
                db = db == 0 ? _db : db;
                if (string.IsNullOrEmpty(key))
                    return false;
                else
                    return Context.GetDatabase(db).KeyExists(key);
            }
            catch (RedisException ex)
            {
                Task.Factory.StartNew(() =>
                {
                    SaveLog(ex, "Exists", key);
                });
                return false;
            }
        }
        #endregion

        #region 是否存在 asy
        /// <summary>
        /// 是否存在 asy
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> ExistsAsy(string key, int db = 0)
        {
            return await Task.Factory.StartNew(() =>
            {
                return Exists(key, db);
            });
        }
        #endregion

        #region 设置值 item
        /// <summary>
        /// 设置值 item
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="key">键</param>
        /// <param name="model">值</param>
        /// <param name="hours">存期限</param>
        /// <returns></returns>
        public static bool Set<T>(string key, T model, int hours = 24 * 30 * 12, int db = 0)
        {
            try
            {
                db = db == 0 ? _db : db;
                if (!string.IsNullOrEmpty(key))
                    return Context.GetDatabase(db).StringSet(key, BaseJson.ModelToJson(model), TimeSpan.FromHours(hours));
                else
                    return false;
            }
            catch (RedisException ex)
            {
                Task.Factory.StartNew(() =>
                {
                    SaveLog<T>(ex, "Set<T>");
                });
                return false;
            }
        }
        #endregion

        #region 设置值 item asy
        /// <summary>
        /// 设置值 item asy
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="key">键</param>
        /// <param name="model">值</param>
        /// <param name="hours">存期限</param>
        /// <returns></returns>
        public static async Task<bool> SetAsy<T>(string key, T model, int hours = 24 * 30 * 12, int db = 0)
        {
            return await Task.Factory.StartNew(() =>
            {
                return Set<T>(key, model, hours, db);
            });
        }
        #endregion

        #region 设置值 item
        /// <summary>
        /// 设置值 item
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="key">键</param>
        /// <param name="model">值</param>
        /// <param name="hours">存期限</param>
        /// <returns></returns>
        public static bool Set(string key, string model, int hours = 24 * 30 * 12, int db = 0)
        {
            try
            {
                db = db == 0 ? _db : db;
                if (!string.IsNullOrEmpty(key))
                    return Context.GetDatabase(db).StringSet(key, model, TimeSpan.FromHours(hours));
                else
                    return false;
            }
            catch (RedisException ex)
            {
                Task.Factory.StartNew(() =>
                {
                    SaveLog(ex, "Set", key);
                });
                return false;
            }
        }
        #endregion

        #region 设置值 item asy
        /// <summary>
        /// 设置值 item asy
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="key">键</param>
        /// <param name="model">值</param>
        /// <param name="hours">存期限</param>
        /// <returns></returns>
        public static async Task<bool> SetAsy(string key, string model, int hours = 24 * 30 * 12, int db = 0)
        {
            return await Task.Factory.StartNew(() =>
             {
                return Set(key, model, hours, db);
             });
        }
        #endregion

        #region 设置值 item
        /// <summary>
        /// 设置值 item
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="key">键</param>
        /// <param name="model">值</param>
        /// <param name="Minutes">存期限</param>
        /// <returns></returns>
        public static bool Set(string key, string model, double Minutes, int db = 0)
        {
            try
            {
                db = db == 0 ? _db : db;
                if (!string.IsNullOrEmpty(key))
                    return Context.GetDatabase(db).StringSet(key, model, TimeSpan.FromMilliseconds(Minutes));
                else
                    return false;
            }
            catch (RedisException ex)
            {
                Task.Factory.StartNew(() =>
                {
                    SaveLog(ex, "Set", key);
                });
                return false;
            }
        }
        #endregion

        #region 设置值 item asy
        /// <summary>
        /// 设置值 item asy
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="key">键</param>
        /// <param name="model">值</param>
        /// <param name="Minutes">存期限</param>
        /// <returns></returns>
        public static async Task<bool> SetAsy(string key, string model, double Minutes, int db = 0)
        {
            return await Task.Factory.StartNew(() =>
            {
                return Set(key, model, Minutes, db);
            });
        }
        #endregion

        #region 获取值 item
        /// <summary>
        /// 获取值 item
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="key">键</param>
        /// <returns></returns>
        public static string Get(string key, int db = 0)
        {
            try
            {
                db = db == 0 ? _db : db;
                if (string.IsNullOrEmpty(key))
                    return "";
                else
                    return Context.GetDatabase(db).StringGet(key);
            }
            catch (RedisException ex)
            {
                Task.Factory.StartNew(() =>
                {
                    SaveLog(ex, "Get", key);
                });
                return "";
            }
        }
        #endregion

        #region 获取值 item asy
        /// <summary>
        /// 获取值 item asy
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="key">键</param>
        /// <returns></returns>
        public static async Task<string> GetAsy(string key, int db = 0)
        {
            return await Task.Factory.StartNew(() =>
            {
                return Get(key, db);
            });
        }
        #endregion

        #region 获取值 item
        /// <summary>
        /// 获取值 item
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="key">键</param>
        /// <returns></returns>
        public static T Get<T>(string key, int db = 0) where T : class, new()
        {
            try
            {
                db = db == 0 ? _db : db;
                if (string.IsNullOrEmpty(key))
                    return new T();
                else
                    return BaseJson.JsonToModel<T>(Context.GetDatabase(db).StringGet(key)) ?? new T();
            }
            catch (RedisException ex)
            {
                Task.Factory.StartNew(() =>
                {
                    SaveLog<T>(ex, "Get<T>");
                });
                return new T();
            }
        }
        #endregion

        #region 获取值 item asy
        /// <summary>
        /// 获取值 item asy
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="key">键</param>
        /// <returns></returns>
        public static async Task<T> GetAsy<T>(string key, int db = 0) where T : class, new()
        {
            return await Task.Factory.StartNew(() =>
            {
                return Get<T>(key, db);
            });
        }
        #endregion

        #region 删除值 item
        /// <summary>
        /// 删除值 item
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        public static bool Remove(string key, int db = 0)
        {
            try
            {
                db = db == 0 ? _db : db;
                if (!string.IsNullOrEmpty(key))
                    return Context.GetDatabase(db).KeyDelete(key);
                else
                    return false;
            }
            catch (RedisException ex)
            {
                Task.Factory.StartNew(() =>
                {
                    SaveLog(ex, "Remove", key);
                });
                return false;
            }
        }
        #endregion

        #region 删除值 item asy
        /// <summary>
        /// 删除值 item asy
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        public static async Task<bool> RemoveAsy(string key, int db = 0)
        {
            return await Task.Factory.StartNew(() =>
            {
                return Remove(key, db);
            });
        }
        #endregion
                     
        #region 出错日志
        /// <summary>
        /// 出错日志
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ex"></param>
        /// <param name="CurrentMethod"></param>
        private static void SaveLog<T>(Exception ex, string CurrentMethod)
        {
            SaveLog(string.Format("方法：{0},对象：{1},出错详情：{2}", CurrentMethod, typeof(T).Name, ex.ToString()), "RedisContext.GetContext()_exp");
        }
        #endregion

        #region 出错日志
        /// <summary>
        /// 出错日志
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ex"></param>
        /// <param name="CurrentMethod"></param>
        private static void SaveLog(Exception ex, string CurrentMethod, string key)
        {
            SaveLog(string.Format("方法：{0},键：{1},出错详情：{2}", CurrentMethod, key, ex.ToString()), "RedisContext.GetContext()_exp");
        }
        #endregion

        #region 写日志
        /// <summary>
        /// 说明：写日记
        /// </summary>
        /// <param name="StrContent">日志内容</param>
        private static void SaveLog(string logContent, string fileName, string headName = "", bool IsWrap = false, int logCount = 10)
        {
            var path = string.Format("{0}/App_Data/log/{1}", AppDomain.CurrentDomain.BaseDirectory, DateTime.Now.ToString("yyyy-MM"));

            try
            {
                logCount--;

                //新建文件
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                if (fileName == "")
                    fileName = string.Format("{0}.txt", DateTime.Now.ToString("yyyy-MM-dd-HH"));
                else
                    fileName = string.Format("{0}_{1}.txt", fileName, DateTime.Now.ToString("yyyy-MM-dd-HH"));

                //写日志
                using (var fs = new FileStream(string.Format("{0}/{1}", path, fileName), FileMode.OpenOrCreate, FileAccess.Write))
                {
                    var m_streamWriter = new StreamWriter(fs);
                    m_streamWriter.BaseStream.Seek(0, SeekOrigin.End);
                    m_streamWriter.WriteLine(string.Format("{0}[{1}]{2}", headName, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), logContent));
                    m_streamWriter.WriteLine("");

                    if (IsWrap)
                        m_streamWriter.WriteLine("");

                    m_streamWriter.Flush();
                    m_streamWriter.Close();
                    fs.Close();
                }
            }
            catch
            {
                if (logCount != 0)
                    SaveLog(fileName, path, headName, IsWrap, logCount--);
            }
        }
        #endregion
    }
}
