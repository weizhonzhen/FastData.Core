using FastData.Core.Base;
using FastData.Core.Context;
using FastData.Core.Model;
using FastUntility.Core.Base;
using FastUntility.Core.Page;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using FastData.Core.CacheModel;
using FastData.Core.Check;
using FastData.Core.Type;
using System.Reflection;
using System.IO;
using System.Linq.Expressions;
using System.Data;

namespace FastData.Core.Repository
{
    public class FastRepository : IFastRepository
    {
        #region maq 执行返回结果
        /// <summary>
        /// maq 执行返回结果
        /// </summary>
        public List<T> Query<T>(string name, DbParameter[] param, DataContext db = null, string key = null) where T : class, new()
        {
            key = key == null ? MapDb(name) : key;
            var config = db == null ? DataConfig.Get(key) : db.config;
            if (config.IsUpdateCache)
                InstanceMap(key);
            if (DbCache.Exists(config.CacheType, name.ToLower()))
            {
                var sql = MapXml.GetMapSql(name, ref param, db, key);
                var result = FastRead.ExecuteSql<T>(sql, param, db, key);
                if (MapXml.MapIsForEach(name, config))
                {
                    if (db == null)
                    {
                        using (var tempDb = new DataContext(key))
                        {

                            for (var i = 1; i <= MapXml.MapForEachCount(name, config); i++)
                            {
                                result = MapXml.MapForEach<T>(result, name, tempDb, key, config, i);
                            }
                        }
                    }
                    else
                        result = MapXml.MapForEach<T>(result, name, db, key, config);
                }
                return result;
            }
            else
                return new List<T>();
        }
        #endregion

        #region maq 执行返回结果 asy
        /// <summary>
        /// 执行sql asy
        /// </summary>
        public async Task<List<T>> QueryAsy<T>(string name, DbParameter[] param, DataContext db = null, string key = null) where T : class, new()
        {
            return await Task.Run(() =>
            {
                return Query<T>(name, param, db, key);
            });
        }
        #endregion

        #region maq 执行返回结果 lazy
        /// <summary>
        /// maq 执行返回结果 lazy
        /// </summary>
        public Lazy<List<T>> QueryLazy<T>(string name, DbParameter[] param, DataContext db = null, string key = null) where T : class, new()
        {
            return new Lazy<List<T>>(() => Query<T>(name, param, db, key));
        }
        #endregion

        #region maq 执行返回结果 lazy asy
        /// <summary>
        /// maq 执行返回结果 lazy asy
        /// </summary>
        public async Task<Lazy<List<T>>> QueryLazyAsy<T>(string name, DbParameter[] param, DataContext db = null, string key = null) where T : class, new()
        {
            return await Task.Run(() =>
            {
                return new Lazy<List<T>>(() => Query<T>(name, param, db, key));
            });
        }
        #endregion

        #region maq 执行返回 List<Dictionary<string, object>>
        /// <summary>
        /// maq 执行返回 List<Dictionary<string, object>>
        /// </summary>
        public List<Dictionary<string, object>> Query(string name, DbParameter[] param, DataContext db = null, string key = null)
        {
            key = key == null ? MapDb(name) : key;
            var config = db == null ? DataConfig.Get(key) : db.config;
            if (config.IsUpdateCache)
                InstanceMap(key);

            if (DbCache.Exists(config.CacheType, name.ToLower()))
            {
                var sql = MapXml.GetMapSql(name, ref param, db, key);

                var result = FastRead.ExecuteSql(sql, param, db, key);

                if (MapXml.MapIsForEach(name, config))
                {
                    if (db == null)
                    {
                        using (var tempDb = new DataContext(key))
                        {
                            for (var i = 1; i <= MapXml.MapForEachCount(name, config); i++)
                            {
                                result = MapXml.MapForEach(result, name, tempDb, key, config, i);
                            }
                        }
                    }
                    else
                        result = MapXml.MapForEach(result, name, db, key, config);
                }

                return result;
            }
            else
                return new List<Dictionary<string, object>>();
        }
        #endregion

        #region maq 执行返回 List<Dictionary<string, object>> asy
        /// <summary>
        /// 执行sql List<Dictionary<string, object>> asy
        /// </summary>
        public async Task<List<Dictionary<string, object>>> QueryAsy(string name, DbParameter[] param, DataContext db = null, string key = null)
        {
            return await Task.Run(() =>
            {
                return Query(name, param, db, key);
            });
        }
        #endregion

        #region maq 执行返回 List<Dictionary<string, object>> lazy
        /// <summary>
        /// maq 执行返回 List<Dictionary<string, object>> lazy
        /// </summary>
        public Lazy<List<Dictionary<string, object>>> QueryLazy(string name, DbParameter[] param, DataContext db = null, string key = null)
        {
            return new Lazy<List<Dictionary<string, object>>>(() => Query(name, param, db, key));
        }
        #endregion

        #region maq 执行返回 List<Dictionary<string, object>> lazy asy
        /// <summary>
        /// maq 执行返回 List<Dictionary<string, object>> lazy asy
        /// </summary>
        public async Task<Lazy<List<Dictionary<string, object>>>> QueryLazyAsy(string name, DbParameter[] param, DataContext db = null, string key = null)
        {
            return await Task.Run(() =>
            {
                return new Lazy<List<Dictionary<string, object>>>(() => Query(name, param, db, key));
            });
        }
        #endregion

        #region maq 执行写操作
        /// <summary>
        /// 执行写操作
        /// </summary>
        public WriteReturn Write(string name, DbParameter[] param, DataContext db = null, string key = null)
        {
            key = key == null ? MapDb(name) : key;
            var config = db == null ? DataConfig.Get(key) : db.config;
            if (config.IsUpdateCache)
                InstanceMap(key);

            if (DbCache.Exists(config.CacheType, name.ToLower()))
            {
                var sql = MapXml.GetMapSql(name, ref param, db, key);

                return FastWrite.ExecuteSql(sql, param, db, key);
            }
            else
                return new WriteReturn();
        }
        #endregion

        #region maq 执行写操作 asy
        /// <summary>
        ///  maq 执行写操作 asy
        /// </summary>
        public async Task<WriteReturn> WriteAsy(string name, DbParameter[] param, DataContext db = null, string key = null)
        {
            return await Task.Run(() =>
            {
                return Write(name, param, db, key);
            });
        }
        #endregion

        #region maq 执行写操作 asy lazy
        /// <summary>
        /// maq 执行写操作 asy lazy
        /// </summary>
        public Lazy<WriteReturn> WriteLazy(string name, DbParameter[] param, DataContext db = null, string key = null)
        {
            return new Lazy<WriteReturn>(() => Write(name, param, db, key));
        }
        #endregion

        #region maq 执行写操作 asy lazy asy
        /// <summary>
        /// maq 执行写操作 asy lazy asy
        /// </summary>
        public async Task<Lazy<WriteReturn>> WriteLazyAsy(string name, DbParameter[] param, DataContext db = null, string key = null)
        {
            return await Task.Run(() =>
            {
                return new Lazy<WriteReturn>(() => Write(name, param, db, key));
            });
        }
        #endregion

        #region 执行分页
        /// <summary>
        /// 执行分页 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private PageResult ExecuteSqlPage(PageModel pModel, string sql, DbParameter[] param, DataContext db = null, string key = null)
        {
            var result = new DataReturn();
            var config = DataConfig.Get(key);
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(key))
                {
                    result = tempDb.GetPageSql(pModel, sql, param);
                }
            }
            else
                result = db.GetPageSql(pModel, sql, param);

            stopwatch.Stop();

            DbLog.LogSql(config.IsOutSql, result.Sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);

            return result.PageResult;
        }
        #endregion

        #region maq 执行分页
        /// <summary>
        /// maq 执行分页
        /// </summary>
        public PageResult QueryPage(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null)
        {
            key = key == null ? MapDb(name) : key;
            var config = db == null ? DataConfig.Get(key) : db.config;
            if (config.IsUpdateCache)
                InstanceMap(key);

            if (DbCache.Exists(config.CacheType, name.ToLower()))
            {
                var sql = MapXml.GetMapSql(name, ref param, db, key);

                var result = ExecuteSqlPage(pModel, sql, param, db, key);

                if (MapXml.MapIsForEach(name, config))
                {
                    if (db == null)
                    {
                        using (var tempDb = new DataContext(key))
                        {

                            for (var i = 1; i <= MapXml.MapForEachCount(name, config); i++)
                            {
                                result.list = MapXml.MapForEach(result.list, name, tempDb, key, config, i);
                            }
                        }
                    }
                    else
                        result.list = MapXml.MapForEach(result.list, name, db, key, config);
                }

                return result;
            }
            else
                return new PageResult();
        }
        #endregion

        #region maq 执行分页 asy
        /// <summary>
        /// 执行分页 asy
        /// </summary>
        public async Task<PageResult> QueryPageAsy(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null)
        {
            return await Task.Run(() =>
            {
                return QueryPage(pModel, name, param, db, key);
            });
        }
        #endregion

        #region maq 执行分页 lazy
        /// <summary>
        /// maq 执行分页 lazy
        /// </summary>
        public Lazy<PageResult> QueryPageLazy(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null)
        {
            return new Lazy<PageResult>(() => QueryPage(pModel, name, param, db, key));
        }
        #endregion

        #region maq 执行分页 lazy asy
        /// <summary>
        /// maq 执行分页lazy asy
        /// </summary>
        public async Task<Lazy<PageResult>> QueryPageLazyAsy(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null)
        {
            return await Task.Run(() =>
            {
                return new Lazy<PageResult>(() => QueryPage(pModel, name, param, db, key));
            });
        }
        #endregion


        #region 执行分页
        /// <summary>
        /// 执行分页 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private PageResult<T> ExecuteSqlPage<T>(PageModel pModel, string sql, DbParameter[] param, DataContext db = null, string key = null) where T : class, new()
        {
            var result = new DataReturn<T>();
            var config = DataConfig.Get(key);
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(key))
                {
                    result = tempDb.GetPageSql<T>(pModel, sql, param);
                }
            }
            else
                result = db.GetPageSql<T>(pModel, sql, param);

            stopwatch.Stop();

            DbLog.LogSql(config.IsOutSql, result.sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);

            return result.pageResult;
        }
        #endregion

        #region maq 执行分页
        /// <summary>
        /// maq 执行分页
        /// </summary>
        public PageResult<T> QueryPage<T>(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null) where T : class, new()
        {
            key = key == null ? MapDb(name) : key;
            var config = db == null ? DataConfig.Get(key) : db.config;
            if (config.IsUpdateCache)
                InstanceMap(key);

            if (DbCache.Exists(config.CacheType, name.ToLower()))
            {
                var sql = MapXml.GetMapSql(name, ref param, db, key);

                var result = ExecuteSqlPage<T>(pModel, sql, param, db, key);

                if (MapXml.MapIsForEach(name, config))
                {
                    if (db == null)
                    {
                        using (var tempDb = new DataContext(key))
                        {

                            for (var i = 1; i <= MapXml.MapForEachCount(name, config); i++)
                            {
                                result.list = MapXml.MapForEach<T>(result.list, name, tempDb, key, config, i);
                            }
                        }
                    }
                    else
                        result.list = MapXml.MapForEach<T>(result.list, name, db, key, config);
                }

                return result;
            }
            else
                return new PageResult<T>();
        }
        #endregion

        #region maq 执行分页 asy
        /// <summary>
        /// 执行分页 asy
        /// </summary>
        public async Task<PageResult<T>> QueryPageAsy<T>(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null) where T : class, new()
        {
            return await Task.Run(() =>
            {
                return QueryPage<T>(pModel, name, param, db, key);
            });
        }
        #endregion

        #region maq 执行分页 lazy
        /// <summary>
        /// maq 执行分页 lazy
        /// </summary>
        public Lazy<PageResult<T>> QueryPageLazy<T>(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null) where T : class, new()
        {
            return new Lazy<PageResult<T>>(() => QueryPage<T>(pModel, name, param, db, key));
        }
        #endregion

        #region maq 执行分页 lazy asy
        /// <summary>
        /// maq 执行分页lazy asy
        /// </summary>
        public async Task<Lazy<PageResult<T>>> QueryPageLazyAsy<T>(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null) where T : class, new()
        {
            return await Task.Run(() =>
            {
                return new Lazy<PageResult<T>>(() => QueryPage<T>(pModel, name, param, db, key));
            });
        }
        #endregion

        #region map db
        /// <summary>
        /// map db
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string MapDb(string name)
        {
            return DbCache.Get(DataConfig.Get().CacheType, string.Format("{0}.db", name.ToLower()));
        }
        #endregion

        #region map 参数列表
        /// <summary>
        /// map 参数列表
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<string> MapParam(string name)
        {
            return DbCache.Get<List<string>>(DataConfig.Get().CacheType, string.Format("{0}.param", name.ToLower()));
        }
        #endregion

        #region 初始化map 3
        /// <summary>
        /// 初始化map 3
        /// </summary>
        /// <returns></returns>
        private void InstanceMap(string dbKey = null)
        {
            var list = BaseConfig.GetValue<MapConfigModel>(AppSettingKey.Map, "map.json");
            var config = DataConfig.Get(dbKey);
            var db = new DataContext(dbKey);
            var query = new DataQuery { Config = config, Key = dbKey };

            foreach (var item in list.Path)
            {
                var info = new FileInfo(item);
                var key = BaseSymmetric.Generate(info.FullName);

                if (!DbCache.Exists(config.CacheType, key))
                {
                    var temp = new MapXmlModel();
                    temp.LastWrite = info.LastWriteTime;
                    temp.FileKey = MapXml.ReadXml(info.FullName, config, info.Name.ToLower().Replace(".xml", ""));
                    temp.FileName = info.FullName;
                    if (MapXml.SaveXml(dbKey, key, info, config, db))
                        DbCache.Set<MapXmlModel>(config.CacheType, key, temp);
                }
                else if ((DbCache.Get<MapXmlModel>(config.CacheType, key).LastWrite - info.LastWriteTime).Milliseconds != 0)
                {
                    foreach (var temp in DbCache.Get<MapXmlModel>(config.CacheType, key).FileKey)
                        DbCache.Remove(config.CacheType, temp);

                    var model = new MapXmlModel();
                    model.LastWrite = info.LastWriteTime;
                    model.FileKey = MapXml.ReadXml(info.FullName, config, info.Name.ToLower().Replace(".xml", ""));
                    model.FileName = info.FullName;
                    if (MapXml.SaveXml(dbKey, key, info, config, db))
                        DbCache.Set<MapXmlModel>(config.CacheType, key, model);
                }
            }

            if (config.IsMapSave)
            {
                query.Config.DesignModel = FastData.Core.Base.Config.CodeFirst;
                if (query.Config.DbType == DataDbType.Oracle)
                {
                    var listInfo = typeof(FastData.Core.DataModel.Oracle.Data_MapFile).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList();
                    var listAttribute = typeof(FastData.Core.DataModel.Oracle.Data_MapFile).GetTypeInfo().GetCustomAttributes().ToList();
                    BaseTable.Check(query, "Data_MapFile", listInfo, listAttribute);
                }

                if (query.Config.DbType == DataDbType.MySql)
                {
                    var listInfo = typeof(FastData.Core.DataModel.MySql.Data_MapFile).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList();
                    var listAttribute = typeof(FastData.Core.DataModel.MySql.Data_MapFile).GetTypeInfo().GetCustomAttributes().ToList();
                    BaseTable.Check(query, "Data_MapFile", listInfo, listAttribute);
                }

                if (query.Config.DbType == DataDbType.SqlServer)
                {
                    var listInfo = typeof(FastData.Core.DataModel.SqlServer.Data_MapFile).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList();
                    var listAttribute = typeof(FastData.Core.DataModel.SqlServer.Data_MapFile).GetTypeInfo().GetCustomAttributes().ToList();
                    BaseTable.Check(query, "Data_MapFile", listInfo, listAttribute);
                }
            }

            db.Dispose();
        }
        #endregion

        #region  获取api接口key
        /// <summary>
        /// 获取api接口key
        /// </summary>
        public Dictionary<string, object> Api()
        {
            return DbCache.Get<Dictionary<string, object>>(DataConfig.Get().CacheType, "FastMap.Api") ?? new Dictionary<string, object>();
        }
        #endregion

        #region 批量增加
        /// <summary>
        /// 批量增加
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="model">实体</param>
        /// <param name="IsTrans">是否事务</param>
        /// <returns></returns>
        public WriteReturn AddList<T>(List<T> list, DataContext db = null, string key = null, bool isLog = false) where T : class, new()
        {
            ConfigModel config = null;
            var result = new DataReturn<T>();
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(key))
                {
                    result = tempDb.AddList<T>(list,isLog);
                    config = tempDb.config;
                }
            }
            else
            {
                result = db.AddList<T>(list,isLog);
                config = db.config;
            }

            stopwatch.Stop();

            DbLog.LogSql(config.IsOutSql, result.sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);

            return result.writeReturn;
        }
        #endregion

        #region 批量增加 asy
        /// <summary>
        /// 批量增加 asy
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="model">实体</param>
        /// <param name="IsTrans">是否事务</param>
        /// <returns></returns>
        public async Task<WriteReturn> AddListAsy<T>(List<T> list, DataContext db = null, string key = null, bool isLog = false) where T : class, new()
        {
            return await Task.Run(() =>
            {
                return AddList<T>(list, db, key,isLog);
            });
        }
        #endregion

        #region 增加
        /// <summary>
        /// 增加
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="model">实体</param>
        /// <param name="IsTrans">是否事务</param>
        /// <param name="notAddField">不需要增加的字段</param>
        /// <returns></returns>
        public WriteReturn Add<T>(T model, DataContext db = null, string key = null) where T : class, new()
        {
            ConfigModel config = null;
            var result = new DataReturn<T>();
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(key))
                {
                    result = tempDb.Add<T>(model, false);
                    config = tempDb.config;
                }
            }
            else
            {
                result = db.Add<T>(model, false);
                config = db.config;
            }

            stopwatch.Stop();

            DbLog.LogSql(config.IsOutSql, result.sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            return result.writeReturn;
        }
        #endregion

        #region 增加 asy
        /// <summary>
        /// 增加 asy
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="model">实体</param>
        /// <param name="IsTrans">是否事务</param>
        /// <param name="notAddField">不需要增加的字段</param>
        /// <returns></returns>
        public async Task<WriteReturn> AddAsy<T>(T model, DataContext db = null, string key = null) where T : class, new()
        {
            return await Task.Run(() =>
            {
                return Add<T>(model, db, key);
            });
        }
        #endregion

        #region 删除(Lambda表达式)
        /// <summary>
        /// 删除(Lambda表达式)
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="predicate">表达式</param>
        /// <param name="IsTrans">是否事务</param>
        /// <returns></returns>
        public WriteReturn Delete<T>(Expression<Func<T, bool>> predicate, DataContext db = null, string key = null) where T : class, new()
        {
            ConfigModel config = null;
            var result = new DataReturn<T>();
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(key))
                {
                    result = tempDb.Delete<T>(predicate);
                    config = tempDb.config;
                }
            }
            else
            {
                result = db.Delete<T>(predicate);
                config = db.config;
            }

            stopwatch.Stop();

            DbLog.LogSql(config.IsOutSql, result.sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            return result.writeReturn;
        }
        #endregion

        #region 删除(Lambda表达式)asy
        /// <summary>
        /// 删除(Lambda表达式)asy
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="predicate">表达式</param>
        /// <param name="IsTrans">是否事务</param>
        /// <returns></returns>
        public async Task<WriteReturn> DeleteAsy<T>(Expression<Func<T, bool>> predicate, DataContext db = null, string key = null) where T : class, new()
        {
            return await Task.Run(() =>
            {
                return Delete<T>(predicate, db, key);
            });
        }
        #endregion

        #region 删除
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public WriteReturn Delete<T>(T model, DataContext db = null, string key = null, bool isTrans = false) where T : class, new()
        {
            ConfigModel config = null;
            var result = new DataReturn<T>();
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(key))
                {
                    result = tempDb.Delete(model, isTrans);
                    config = tempDb.config;
                }
            }
            else
            {
                result = db.Delete(model, isTrans);
                config = db.config;
            }

            stopwatch.Stop();

            DbLog.LogSql(config.IsOutSql, result.sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);

            return result.writeReturn;
        }
        #endregion

        #region 删除asy
        /// <summary>
        /// 删除asy
        /// </summary>
        /// <returns></returns>
        public async Task<WriteReturn> UpdateAsy<T>(T model, DataContext db = null, string key = null, bool isTrans = false) where T : class, new()
        {
            return await Task.Run(() =>
            {
                return Delete<T>(model, db, key);
            });
        }
        #endregion

        #region 修改(Lambda表达式)
        /// <summary>
        /// 修改(Lambda表达式)
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="model">实体</param>
        /// <param name="predicate">表达式</param>
        /// <param name="IsTrans">是否事务</param>
        /// <param name="field">需要修改的字段</param>
        /// <returns></returns>
        public WriteReturn Update<T>(T model, Expression<Func<T, bool>> predicate, Expression<Func<T, object>> field = null, DataContext db = null, string key = null) where T : class, new()
        {
            ConfigModel config = null;
            var result = new DataReturn<T>();
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(key))
                {
                    result = tempDb.Update<T>(model, predicate, field);
                    config = tempDb.config;
                }
            }
            else
            {
                result = db.Update<T>(model, predicate, field);
                config = db.config;
            }

            stopwatch.Stop();

            DbLog.LogSql(config.IsOutSql, result.sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);

            return result.writeReturn;
        }
        #endregion

        #region 修改(Lambda表达式)asy
        /// <summary>
        /// 修改(Lambda表达式)asy
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="model">实体</param>
        /// <param name="predicate">表达式</param>
        /// <param name="IsTrans">是否事务</param>
        /// <param name="field">需要修改的字段</param>
        /// <returns></returns>
        public async Task<WriteReturn> UpdateAsy<T>(T model, Expression<Func<T, bool>> predicate, Expression<Func<T, object>> field = null, DataContext db = null, string key = null) where T : class, new()
        {
            return await Task.Run(() =>
            {
                return Update<T>(model, predicate, field, db, key);
            });
        }
        #endregion

        #region 修改
        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public WriteReturn Update<T>(T model, Expression<Func<T, object>> field = null, DataContext db = null, string key = null, bool isTrans = false) where T : class, new()
        {
            ConfigModel config = null;
            var result = new DataReturn<T>();
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(key))
                {
                    result = tempDb.Update(model, field, isTrans);
                    config = tempDb.config;
                }
            }
            else
            {
                result = db.Update(model, field, isTrans);
                config = db.config;
            }

            stopwatch.Stop();

            DbLog.LogSql(config.IsOutSql, result.sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);

            return result.writeReturn;
        }
        #endregion

        #region 修改asy
        /// <summary>
        /// 修改asy
        /// </summary>
        /// <returns></returns>
        public async Task<WriteReturn> UpdateAsy<T>(T model, Expression<Func<T, object>> field = null, DataContext db = null, string key = null, bool isTrans = false) where T : class, new()
        {
            return await Task.Run(() =>
            {
                return Update<T>(model, field, db, key, isTrans);
            });
        }
        #endregion

        #region 修改list
        /// <summary>
        /// 修改list
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public WriteReturn UpdateList<T>(List<T> list, Expression<Func<T, object>> field = null, DataContext db = null, string key = null) where T : class, new()
        {
            ConfigModel config = null;
            var result = new DataReturn<T>();
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(key))
                {
                    result = tempDb.UpdateList(list, field);
                    config = tempDb.config;
                }
            }
            else
            {
                result = db.UpdateList(list, field);
                config = db.config;
            }

            stopwatch.Stop();

            DbLog.LogSql(config.IsOutSql, result.sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);

            return result.writeReturn;
        }
        #endregion

        #region 修改list asy
        /// <summary>
        /// 修改list asy
        /// </summary>
        /// <returns></returns>
        public async Task<WriteReturn> UpdateListAsy<T>(List<T> list, Expression<Func<T, object>> field = null, DataContext db = null, string key = null) where T : class, new()
        {
            return await Task.Run(() =>
            {
                return UpdateList<T>(list, field, db, key);
            });
        }
        #endregion

        #region 执行sql
        /// <summary>
        /// 执行sql
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public WriteReturn ExecuteSql(string sql, DbParameter[] param, DataContext db = null, string key = null)
        {
            ConfigModel config = null;
            var result = new DataReturn();
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(key))
                {
                    result = tempDb.ExecuteSql(sql, param, false);
                    config = tempDb.config;
                }
            }
            else
            {
                result = db.ExecuteSql(sql, param, false);
                config = db.config;
            }

            stopwatch.Stop();

            DbLog.LogSql(config.IsOutSql, result.Sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);

            return result.writeReturn;
        }
        #endregion

        #region 执行sql asy
        /// <summary>
        /// 执行sql asy
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public async Task<WriteReturn> ExecuteSqlAsy(string sql, DbParameter[] param, DataContext db = null, string key = null)
        {
            return await Task.Run(() =>
            {
                return ExecuteSql(sql, param, db, key);
            });
        }
        #endregion

        #region 查询join
        /// <summary>
        /// 查询join
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <typeparam name="T1">泛型</typeparam>
        /// <param name="joinType">left join,right join,inner join</param>
        /// <param name="item"></param>
        /// <param name="predicate">条件</param>
        /// <param name="field">字段</param>
        /// <returns></returns>
        private DataQuery JoinType<T, T1>(string joinType, DataQuery item, Expression<Func<T, T1, bool>> predicate, Expression<Func<T1, object>> field = null, bool isDblink = false)
        {
            var queryField = BaseField.QueryField<T, T1>(predicate, field, item.Config);
            item.Field.Add(queryField.Field);
            item.AsName.AddRange(queryField.AsName);

            var condtion = VisitExpression.LambdaWhere<T, T1>(predicate, item.Config);
            item.Predicate.Add(condtion);
            item.Table.Add(string.Format("{2} {0}{3} {1}", typeof(T1).Name, predicate.Parameters[1].Name
            , joinType, isDblink && !string.IsNullOrEmpty(item.Config.DbLinkName) ? string.Format("@", item.Config.DbLinkName) : ""));

            return item;
        }
        #endregion

        #region 表查询
        /// <summary>
        /// 表查询
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="predicate">条件</param>
        /// <param name="field">字段</param>
        /// <param name="Key"></param>
        /// <returns></returns>
        public DataQuery Query<T>(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> field = null, string key = null)
        {
            var result = new DataQuery();
            result.Config = DataConfig.Get(key);
            result.Key = key;

            var queryField = BaseField.QueryField<T>(predicate, field, result.Config);
            result.Field.Add(queryField.Field);
            result.AsName.AddRange(queryField.AsName);

            var condtion = VisitExpression.LambdaWhere<T>(predicate, result.Config);
            result.Predicate.Add(condtion);
            result.Table.Add(string.Format("{0} {1}", typeof(T).Name, predicate.Parameters[0].Name));

            return result;
        }
        #endregion

        #region 查询left join
        /// <summary>
        /// 查询left join
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="item"></param>
        /// <param name="predicate"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public DataQuery LeftJoin<T, T1>(DataQuery item, Expression<Func<T, T1, bool>> predicate, Expression<Func<T1, object>> field = null, bool isDblink = false)
        {
            return JoinType("left join", item, predicate, field);
        }
        #endregion

        #region 查询right join
        /// <summary>
        /// 查询right join
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="item"></param>
        /// <param name="predicate"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public DataQuery RightJoin<T, T1>(DataQuery item, Expression<Func<T, T1, bool>> predicate, Expression<Func<T1, object>> field = null, bool isDblink = false) where T1 : class, new()
        {
            return JoinType("right join", item, predicate, field);
        }
        #endregion

        #region 查询inner join
        /// <summary>
        /// 查询inner join
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="item"></param>
        /// <param name="predicate"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public DataQuery InnerJoin<T, T1>(DataQuery item, Expression<Func<T, T1, bool>> predicate, Expression<Func<T1, object>> field = null, bool isDblink = false) where T1 : class, new()
        {
            return JoinType("inner join", item, predicate, field);
        }
        #endregion

        #region 查询order by
        /// <summary>
        /// 查询order by
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public DataQuery OrderBy<T>(DataQuery item, Expression<Func<T, object>> field, bool isDesc = true)
        {
            var orderBy = BaseField.OrderBy<T>(field, item.Config, isDesc);
            item.OrderBy.AddRange(orderBy);
            return item;
        }
        #endregion

        #region 查询group by
        /// <summary>
        /// 查询group by
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public DataQuery GroupBy<T>(DataQuery item, Expression<Func<T, object>> field)
        {
            var groupBy = BaseField.GroupBy<T>(field, item.Config);
            item.GroupBy.AddRange(groupBy);
            return item;
        }
        #endregion

        #region 查询take
        /// <summary>
        /// 查询take
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public DataQuery Take(DataQuery item, int i)
        {
            item.Take = i;
            return item;
        }
        #endregion

        #region 返回list
        /// <summary>
        /// 返回list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public List<T> ToList<T>(DataQuery item, DataContext db = null) where T : class, new()
        {
            var stopwatch = new Stopwatch();
            var result = new DataReturn<T>();

            if (item.Predicate.Exists(a => a.IsSuccess == false))
                return result.list;

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(item.Key))
                {
                    result = tempDb.GetList<T>(item);
                }
            }
            else
                result = db.GetList<T>(item);

            stopwatch.Stop();

            DbLog.LogSql(item.Config.IsOutSql, result.sql, item.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            return result.list;
        }
        #endregion

        #region 返回list asy
        /// <summary>
        /// 返回list asy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<List<T>> ToListAsy<T>(DataQuery item, DataContext db = null) where T : class, new()
        {
            return await Task.Factory.StartNew(() =>
            {
                return ToList<T>(item, db);
            });
        }
        #endregion

        #region 返回lazy<list>
        /// <summary>
        /// 返回lazy<list>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public Lazy<List<T>> ToLazyList<T>(DataQuery item, DataContext db = null) where T : class, new()
        {
            return new Lazy<List<T>>(() => ToList<T>(item, db));
        }
        #endregion

        #region 返回lazy<list> asy
        /// <summary>
        /// 返回lazy<list> asy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<Lazy<List<T>>> ToLazyListAsy<T>(DataQuery item, DataContext db = null) where T : class, new()
        {
            return await Task.Factory.StartNew(() =>
            {
                return new Lazy<List<T>>(() => ToList<T>(item, db));
            });
        }
        #endregion


        #region 返回json
        /// <summary>
        /// 返回json
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public string ToJson(DataQuery item, DataContext db = null)
        {
            var result = new DataReturn();
            var stopwatch = new Stopwatch();

            if (item.Predicate.Exists(a => a.IsSuccess == false))
                return result.Json;

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(item.Key))
                {
                    result = tempDb.GetJson(item);
                }
            }
            else
                result = db.GetJson(item);

            stopwatch.Stop();

            DbLog.LogSql(item.Config.IsOutSql, result.Sql, item.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            return result.Json;
        }
        #endregion

        #region 返回json asy
        /// <summary>
        /// 返回json asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<string> ToJsonAsy(DataQuery item, DataContext db = null)
        {
            return await Task.Factory.StartNew(() =>
            {
                return ToJson(item, db);
            });
        }
        #endregion

        #region 返回lazy<json>
        /// <summary>
        /// 返回lazy<json>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Lazy<string> ToLazyJson(DataQuery item, DataContext db = null)
        {
            return new Lazy<string>(() => ToJson(item, db));
        }
        #endregion

        #region 返回lazy<json> asy
        /// <summary>
        /// 返回lazy<json> asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<Lazy<string>> ToLazyJsonAsy(DataQuery item, DataContext db = null)
        {
            return await Task.Factory.StartNew(() =>
            {
                return new Lazy<string>(() => ToJson(item, db));
            });
        }
        #endregion


        #region 返回item
        /// <summary>
        /// 返回item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public T ToItem<T>(DataQuery item, DataContext db = null) where T : class, new()
        {
            var result = new DataReturn<T>();
            var stopwatch = new Stopwatch();

            if (item.Predicate.Exists(a => a.IsSuccess == false))
                return result.item;

            stopwatch.Start();

            item.Take = 1;

            if (db == null)
            {
                using (var tempDb = new DataContext(item.Key))
                {
                    result = tempDb.GetList<T>(item);
                }
            }
            else
                result = db.GetList<T>(item);

            stopwatch.Stop();

            DbLog.LogSql(item.Config.IsOutSql, result.sql, item.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            return result.item;
        }
        #endregion

        #region 返回item asy
        /// <summary>
        /// 返回item asy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<T> ToItemAsy<T>(DataQuery item, DataContext db = null) where T : class, new()
        {
            return await Task.Factory.StartNew(() =>
            {
                return ToItem<T>(item, db);
            });
        }
        #endregion

        #region 返回Lazy<item>
        /// <summary>
        /// 返回Lazy<item>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public Lazy<T> ToLazyItem<T>(DataQuery item, DataContext db = null) where T : class, new()
        {
            return new Lazy<T>(() => ToItem<T>(item, db));
        }
        #endregion

        #region 返回Lazy<item> asy
        /// <summary>
        /// 返回Lazy<item> asy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<Lazy<T>> ToLazyItemAsy<T>(DataQuery item, DataContext db = null) where T : class, new()
        {
            return await Task.Factory.StartNew(() =>
            {
                return new Lazy<T>(() => ToItem<T>(item, db));
            });
        }
        #endregion


        #region 返回条数
        /// <summary>
        /// 返回条数
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int ToCount(DataQuery item, DataContext db = null)
        {
            var result = new DataReturn();
            var stopwatch = new Stopwatch();

            if (item.Predicate.Exists(a => a.IsSuccess == false))
                return result.Count;

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(item.Key))
                {
                    result = tempDb.GetCount(item);
                }
            }
            else
                result = db.GetCount(item);

            stopwatch.Stop();

            DbLog.LogSql(item.Config.IsOutSql, result.Sql, item.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);

            return result.Count;
        }
        #endregion

        #region 返回条数 asy
        /// <summary>
        /// 返回条数 asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<int> ToCountAsy<T, T1>(DataQuery item, DataContext db = null)
        {
            return await Task.Factory.StartNew(() =>
            {
                return ToCount(item, db);
            });
        }
        #endregion


        #region 返回分页
        /// <summary>
        /// 返回分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="pModel"></param>
        /// <returns></returns>
        public PageResult<T> ToPage<T>(DataQuery item, PageModel pModel, DataContext db = null) where T : class, new()
        {
            var result = new DataReturn<T>();
            var stopwatch = new Stopwatch();

            if (item.Predicate.Exists(a => a.IsSuccess == false))
                return result.pageResult;

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(item.Key))
                {
                    result = tempDb.GetPage<T>(item, pModel);
                }
            }
            else
                result = db.GetPage<T>(item, pModel);

            stopwatch.Stop();

            DbLog.LogSql(item.Config.IsOutSql, result.sql, item.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            return result.pageResult;
        }
        #endregion

        #region 返回分页 asy
        /// <summary>
        /// 返回分页 asy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="pModel"></param>
        /// <returns></returns>
        public async Task<PageResult<T>> ToPageAsy<T>(DataQuery item, PageModel pModel, DataContext db = null) where T : class, new()
        {
            return await Task.Factory.StartNew(() =>
            {
                return ToPage<T>(item, pModel, db);
            });
        }
        #endregion

        #region 返回分页lazy
        /// <summary>
        /// 返回分页lazy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="pModel"></param>
        /// <returns></returns>
        public Lazy<PageResult<T>> ToLazyPage<T>(DataQuery item, PageModel pModel, DataContext db = null) where T : class, new()
        {
            return new Lazy<PageResult<T>>(() => ToPage<T>(item, pModel, db));
        }
        #endregion

        #region 返回分页lazy asy
        /// <summary>
        /// 返回分页lazy asy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="pModel"></param>
        /// <returns></returns>
        public async Task<Lazy<PageResult<T>>> ToLazyPageAsy<T>(DataQuery item, PageModel pModel, DataContext db = null) where T : class, new()
        {
            return await Task.Factory.StartNew(() =>
            {
                return new Lazy<PageResult<T>>(() => ToPage<T>(item, pModel, db));
            });
        }
        #endregion


        #region 返回分页Dictionary<string, object>
        /// <summary>
        /// 返回分页Dictionary<string, object>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pModel"></param>
        /// <returns></returns>
        public PageResult ToPage(DataQuery item, PageModel pModel, DataContext db = null)
        {
            var result = new DataReturn();
            var stopwatch = new Stopwatch();

            if (item.Predicate.Exists(a => a.IsSuccess == false))
                return result.PageResult;

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(item.Key))
                {
                    result = tempDb.GetPage(item, pModel);
                }
            }
            else
                result = db.GetPage(item, pModel);

            stopwatch.Stop();

            DbLog.LogSql(item.Config.IsOutSql, result.Sql, item.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            return result.PageResult;
        }
        #endregion

        #region 返回分页Dictionary<string, object> asy
        /// <summary>
        /// 返回分页Dictionary<string, object> asy
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pModel"></param>
        /// <returns></returns>
        public async Task<PageResult> ToPageAsy(DataQuery item, PageModel pModel, DataContext db = null)
        {
            return await Task.Factory.StartNew(() =>
            {
                return ToPage(item, pModel, db);
            });
        }
        #endregion

        #region 返回分页Dictionary<string, object> lazy
        /// <summary>
        /// 返回分页Dictionary<string, object> lazy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="pModel"></param>
        /// <returns></returns>
        public Lazy<PageResult> ToLazyPage(DataQuery item, PageModel pModel, DataContext db = null)
        {
            return new Lazy<PageResult>(() => ToPage(item, pModel, db));
        }
        #endregion

        #region 返回分页Dictionary<string, object> lazy asy
        /// <summary>
        /// 返回分页Dictionary<string, object> lazy asy
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pModel"></param>
        /// <returns></returns>
        public async Task<Lazy<PageResult>> ToLazyPageAsy(DataQuery item, PageModel pModel, DataContext db = null)
        {
            return await Task.Factory.StartNew(() =>
            {
                return new Lazy<PageResult>(() => ToPage(item, pModel, db));
            });
        }
        #endregion


        #region DataTable
        /// <summary>
        /// DataTable
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public DataTable ToDataTable(DataQuery item, DataContext db = null)
        {
            var result = new DataReturn();
            var stopwatch = new Stopwatch();

            if (item.Predicate.Exists(a => a.IsSuccess == false))
                return result.Table;

            stopwatch.Start();
            item.Take = 1;

            if (db == null)
            {
                using (var tempDb = new DataContext(item.Key))
                {
                    result = tempDb.GetDataTable(item);
                }
            }
            else
                result = db.GetDataTable(item);

            stopwatch.Stop();

            DbLog.LogSql(item.Config.IsOutSql, result.Sql, item.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            return result.Table;
        }
        #endregion

        #region DataTable asy
        /// <summary>
        /// DataTable asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<DataTable> ToDataTableAsy(DataQuery item, DataContext db = null)
        {
            return await Task.Factory.StartNew(() =>
            {
                return ToDataTable(item, db);
            });
        }
        #endregion

        #region DataTable lazy
        /// <summary>
        /// DataTable lazy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Lazy<DataTable> ToLazyDataTable(DataQuery item, DataContext db = null)
        {
            return new Lazy<DataTable>(() => ToDataTable(item, db));
        }
        #endregion

        #region DataTable lazy asy
        /// <summary>
        /// DataTable lazy asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<Lazy<DataTable>> ToLazyDataTableAsy(DataQuery item, DataContext db = null)
        {
            return await Task.Factory.StartNew(() =>
            {
                return new Lazy<DataTable>(() => ToDataTable(item, db));
            });
        }
        #endregion

        #region 返回List<Dictionary<string, object>>
        /// <summary>
        /// 返回List<Dictionary<string, object>>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> ToDics(DataQuery item, DataContext db = null)
        {
            var result = new DataReturn();
            var stopwatch = new Stopwatch();

            if (item.Predicate.Exists(a => a.IsSuccess == false))
                return result.DicList;

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(item.Key))
                {
                    result = tempDb.GetDic(item);
                }
            }
            else
                result = db.GetDic(item);

            stopwatch.Stop();

            DbLog.LogSql(item.Config.IsOutSql, result.Sql, item.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            return result.DicList;
        }
        #endregion

        #region 返回List<Dictionary<string, object>> asy
        /// <summary>
        /// 返回List<Dictionary<string, object>> asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<List<Dictionary<string, object>>> ToDicsAsy(DataQuery item, DataContext db = null)
        {
            return await Task.Factory.StartNew(() =>
            {
                return ToDics(item, db);
            });
        }
        #endregion

        #region 返回lazy<List<Dictionary<string, object>>>
        /// <summary>
        /// 返回lazy<List<Dictionary<string, object>>>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Lazy<List<Dictionary<string, object>>> ToLazyDics(DataQuery item, DataContext db = null)
        {
            return new Lazy<List<Dictionary<string, object>>>(() => ToDics(item, db));
        }
        #endregion

        #region 返回lazy<List<Dictionary<string, object>>> asy
        /// <summary>
        /// 返回lazy<List<Dictionary<string, object>>> asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<Lazy<List<Dictionary<string, object>>>> ToLazyDicsAsy(DataQuery item, DataContext db = null)
        {
            return await Task.Factory.StartNew(() =>
            {
                return new Lazy<List<Dictionary<string, object>>>(() => ToDics(item, db));
            });
        }
        #endregion


        #region Dictionary<string, object>
        /// <summary>
        /// Dictionary<string, object>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Dictionary<string, object> ToDic(DataQuery item, DataContext db = null)
        {
            var result = new DataReturn();
            var stopwatch = new Stopwatch();

            if (item.Predicate.Exists(a => a.IsSuccess == false))
                return result.Dic;

            stopwatch.Start();
            item.Take = 1;

            if (db == null)
            {
                using (var tempDb = new DataContext(item.Key))
                {
                    result = tempDb.GetDic(item);
                }
            }
            else
                result = db.GetDic(item);

            stopwatch.Stop();

            DbLog.LogSql(item.Config.IsOutSql, result.Sql, item.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            return result.Dic;
        }
        #endregion

        #region Dictionary<string, object> asy
        /// <summary>
        /// Dictionary<string, object> asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, object>> ToDicAsy(DataQuery item, DataContext db = null)
        {
            return await Task.Factory.StartNew(() =>
            {
                return ToDic(item, db);
            });
        }
        #endregion

        #region Dictionary<string, object>
        /// <summary>
        /// Dictionary<string, object>>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Lazy<Dictionary<string, object>> ToLazyDic(DataQuery item, DataContext db = null)
        {
            return new Lazy<Dictionary<string, object>>(() => ToDic(item, db));
        }
        #endregion

        #region Dictionary<string, object> asy
        /// <summary>
        /// Dictionary<string, object> asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<Lazy<Dictionary<string, object>>> ToLazyDicAsy(DataQuery item, DataContext db = null)
        {
            return await Task.Factory.StartNew(() =>
            {
                return new Lazy<Dictionary<string, object>>(() => ToDic(item, db));
            });
        }
        #endregion
    }
}
