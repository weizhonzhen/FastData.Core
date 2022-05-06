using FastUntility.Core.Base;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace FastRedis.Core.Repository
{
    public class RedisRepository : IRedisRepository
    {
        private int _db = 0;
        private ConnectionMultiplexer Context;
        public RedisRepository(IOptions<ConfigModel> options)
        {
            try
            {
                if (options == null || options.Value.Server == null)
                {
                    var config = BaseConfig.GetValue<ConfigModel>(AppSettingKey.Redis, "db.json");
                    _db = config.Db;
                    Context = ConnectionMultiplexer.Connect(config.Server);
                }
                else
                {
                    var config = options.Value;
                    _db = config.Db;
                    Context = ConnectionMultiplexer.Connect(config.Server);
                }
            }
            catch
            {
                throw new Exception(@"services.AddFastRedis(a => { a.Server = 'redis address' }); 
                                    or ( services.AddFastRedis(); and db.json add 'Redis':{'Server':'redis address'} )");
            }
        }

        #region 是否存在
        /// <summary>
        /// 是否存在 
        /// </summary>
        /// <returns></returns>
        public bool Exists(string key, int db = 0)
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
                SaveLog(ex, "Exists", key);
                return false;
            }
        }
        #endregion

        #region 是否存在 asy
        /// <summary>
        /// 是否存在 asy
        /// </summary>
        /// <returns></returns>
        public ValueTask<bool> ExistsAsy(string key, int db = 0)
        {
            try
            {
                db = db == 0 ? _db : db;
                if (string.IsNullOrEmpty(key))
                    return new ValueTask<bool>(false);
                else
                    return new ValueTask<bool>(Context.GetDatabase(db).KeyExistsAsync(key));
            }
            catch (RedisException ex)
            {
                SaveLog(ex, "Exists", key);
                return new ValueTask<bool>(false);
            }
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
        public bool Set<T>(string key, T model, int hours = 24 * 30 * 12, int db = 0)
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
                SaveLog<T>(ex, "Set<T>");
                return false;
            }
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
        public bool Set<T>(string key, T model, TimeSpan timeSpan, int db = 0)
        {
            try
            {
                db = db == 0 ? _db : db;
                if (!string.IsNullOrEmpty(key))
                    return Context.GetDatabase(db).StringSet(key, BaseJson.ModelToJson(model), timeSpan);
                else
                    return false;
            }
            catch (RedisException ex)
            {
                SaveLog<T>(ex, "Set<T>");
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
        public ValueTask<bool> SetAsy<T>(string key, T model, int hours = 24 * 30 * 12, int db = 0)
        {
            try
            {
                db = db == 0 ? _db : db;
                if (!string.IsNullOrEmpty(key))
                    return new ValueTask<bool>(Context.GetDatabase(db).StringSetAsync(key, BaseJson.ModelToJson(model), TimeSpan.FromHours(hours)));
                else
                    return new ValueTask<bool>(false);
            }
            catch (RedisException ex)
            {
                SaveLog<T>(ex, "Set<T>");
                return new ValueTask<bool>(false);
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
        public ValueTask<bool> SetAsy<T>(string key, T model, TimeSpan timeSpan, int db = 0)
        {
            try
            {
                db = db == 0 ? _db : db;
                if (!string.IsNullOrEmpty(key))
                    return new ValueTask<bool>(Context.GetDatabase(db).StringSetAsync(key, BaseJson.ModelToJson(model), timeSpan));
                else
                    return new ValueTask<bool>(false);
            }
            catch (RedisException ex)
            {
                SaveLog<T>(ex, "Set<T>");
                return new ValueTask<bool>(false);
            }
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
        public bool Set(string key, string model, int hours = 24 * 30 * 12, int db = 0)
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
                SaveLog(ex, "Set", key);
                return false;
            }
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
        public bool Set(string key, string model, TimeSpan timeSpan, int db = 0)
        {
            try
            {
                db = db == 0 ? _db : db;
                if (!string.IsNullOrEmpty(key))
                    return Context.GetDatabase(db).StringSet(key, model, timeSpan);
                else
                    return false;
            }
            catch (RedisException ex)
            {
                SaveLog(ex, "Set", key);
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
        public ValueTask<bool> SetAsy(string key, string model, int hours = 24 * 30 * 12, int db = 0)
        {
            try
            {
                db = db == 0 ? _db : db;
                if (!string.IsNullOrEmpty(key))
                    return new ValueTask<bool>(Context.GetDatabase(db).StringSetAsync(key, model, TimeSpan.FromHours(hours)));
                else
                    return new ValueTask<bool>(false);
            }
            catch (RedisException ex)
            {
                SaveLog(ex, "Set", key);
                return new ValueTask<bool>(false);
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
        public ValueTask<bool> SetAsy(string key, string model, TimeSpan timeSpan, int db = 0)
        {
            try
            {
                db = db == 0 ? _db : db;
                if (!string.IsNullOrEmpty(key))
                    return new ValueTask<bool>(Context.GetDatabase(db).StringSetAsync(key, model, timeSpan));
                else
                    return new ValueTask<bool>(false);
            }
            catch (RedisException ex)
            {
                SaveLog(ex, "Set", key);
                return new ValueTask<bool>(false);
            }
        }
        #endregion

        #region 获取值 item
        /// <summary>
        /// 获取值 item
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="key">键</param>
        /// <returns></returns>
        public string Get(string key, int db = 0)
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
                SaveLog(ex, "Get", key);
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
        public async ValueTask<string> GetAsy(string key, int db = 0)
        {
            try
            {
                db = db == 0 ? _db : db;
                if (string.IsNullOrEmpty(key))
                    return "";
                else
                    return await Context.GetDatabase(db).StringGetAsync(key).ConfigureAwait(false);
            }
            catch (RedisException ex)
            {
                SaveLog(ex, "Get", key);
                return "";
            }
        }
        #endregion

        #region 获取值 item
        /// <summary>
        /// 获取值 item
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="key">键</param>
        /// <returns></returns>
        public T Get<T>(string key, int db = 0) where T : class, new()
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
                SaveLog<T>(ex, "Get<T>");
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
        public async ValueTask<T> GetAsy<T>(string key, int db = 0) where T : class, new()
        {
            try
            {
                db = db == 0 ? _db : db;
                if (string.IsNullOrEmpty(key))
                    return new T();
                else
                    return BaseJson.JsonToModel<T>(await Context.GetDatabase(db).StringGetAsync(key).ConfigureAwait(false)) ?? new T();
            }
            catch (RedisException ex)
            {
                SaveLog<T>(ex, "Get<T>");
                return new T();
            }
        }
        #endregion

        #region 删除值 item
        /// <summary>
        /// 删除值 item
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        public bool Remove(string key, int db = 0)
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
                SaveLog(ex, "Remove", key);
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
        public ValueTask<bool> RemoveAsy(string key, int db = 0)
        {
            try
            {
                db = db == 0 ? _db : db;
                if (!string.IsNullOrEmpty(key))
                    return new ValueTask<bool>(Context.GetDatabase(db).KeyDeleteAsync(key));
                else
                    return new ValueTask<bool>(false);
            }
            catch (RedisException ex)
            {
                SaveLog(ex, "Remove", key);
                return new ValueTask<bool>(false);
            }
        }
        #endregion

        #region 命令
        /// <summary>
        /// 命令
        /// </summary>
        /// <param name="command"></param>
        /// <param name="args"></param>
        /// <exception cref="NotImplementedException"></exception>
        public RedisResult Execute(string command, int db = 0, params object[] args)
        {
            try
            {
                db = db == 0 ? _db : db;
                if (!string.IsNullOrEmpty(command))
                    return Context.GetDatabase(db).Execute(command, args);
                else
                    return RedisResult.Create(RedisValue.Null);
            }
            catch (RedisException ex)
            {
                SaveLog(ex, "Execute", command);
                return RedisResult.Create(RedisValue.Null);
            }
        }
        #endregion

        #region 命令
        /// <summary>
        /// 命令
        /// </summary>
        /// <param name="command"></param>
        /// <param name="args"></param>
        public ValueTask<RedisResult> ExecuteAsy(string command, int db = 0, params object[] args)
        {
            try
            {
                db = db == 0 ? _db : db;
                if (!string.IsNullOrEmpty(command))
                    return new ValueTask<RedisResult>(Context.GetDatabase(db).ExecuteAsync(command, args));
                else
                    return new ValueTask<RedisResult>(RedisResult.Create(RedisValue.Null));
            }
            catch (RedisException ex)
            {
                SaveLog(ex, "Execute", command);
                return new ValueTask<RedisResult>(RedisResult.Create(RedisValue.Null));
            }
        }
        #endregion


        #region 出错日志
        /// <summary>
        /// 出错日志
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ex"></param>
        /// <param name="CurrentMethod"></param>
        private void SaveLog<T>(Exception ex, string CurrentMethod)
        {
            BaseLog.SaveLog(string.Format("方法：{0},对象：{1},出错详情：{2}", CurrentMethod, typeof(T).Name, ex.ToString()), "RedisContext.GetContext()_exp");
        }
        #endregion

        #region 出错日志
        /// <summary>
        /// 出错日志
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ex"></param>
        /// <param name="CurrentMethod"></param>
        private void SaveLog(Exception ex, string CurrentMethod, string key)
        {
            BaseLog.SaveLog(string.Format("方法：{0},键：{1},出错详情：{2}", CurrentMethod, key, ex.ToString()), "RedisContext.GetContext()_exp");
        }
        #endregion
    }
}
