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
using FastData.Core.Aop;
using FastData.Core.Filter;
using FastUntility.Core;
using Microsoft.Extensions.DependencyInjection;

namespace FastData.Core.Repository
{
    public class FastRepository : IFastRepository
    {
        internal Query query = new Query();
        internal DataQuery data = new DataQuery();

        #region maq 执行返回结果
        /// <summary>
        /// maq 执行返回结果
        /// </summary>
        public List<T> Query<T>(string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            key = key == null ? MapDb(name) : key;
            var config = db == null ? DataConfig.Get(key) : db.config;
            if (config.IsUpdateCache)
                InstanceMap(key);
            if (DbCache.Exists(config.CacheType, name.ToLower()))
            {
                var sql = MapXml.GetMapSql(name, ref param, db, key);
                isOutSql = isOutSql ? isOutSql : IsMapLog(name);

                BaseAop.AopMapBefore(name, sql, param, config,AopType.Map_List_Model);

                var result = FastRead.ExecuteSql<T>(sql, param, db, key, isOutSql,false);
                if (MapXml.MapIsForEach(name, config))
                {
                    if (db == null)
                    {
                        using (var tempDb = new DataContext(key))
                        {
                            for (var i = 1; i <= MapXml.MapForEachCount(name, config); i++)
                            {
                                result = MapXml.MapForEach<T>(result, name, tempDb, config, i);
                            }
                        }
                    }
                    else
                        result = MapXml.MapForEach<T>(result, name, db, config);
                }

                BaseAop.AopMapAfter(name, sql, param, config, AopType.Map_List_Model, result);
                return result;
            }
            else
            {
                BaseAop.AopMapBefore(name, "", param, config, AopType.Map_List_Model);
                var data = new List<T>();
                BaseAop.AopMapAfter(name, "", param, config, AopType.Map_List_Model, data);
                return data;
            }
        }
        #endregion

        #region maq 执行返回结果 asy
        /// <summary>
        /// 执行sql asy
        /// </summary>
        public ValueTask<List<T>> QueryAsy<T>(string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            return new ValueTask<List<T>>(Query<T>(name, param, db, key, isOutSql));
        }
        #endregion

        #region maq 执行返回结果 lazy
        /// <summary>
        /// maq 执行返回结果 lazy
        /// </summary>
        public Lazy<List<T>> QueryLazy<T>(string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            return new Lazy<List<T>>(() => Query<T>(name, param, db, key, isOutSql));
        }
        #endregion

        #region maq 执行返回结果 lazy asy
        /// <summary>
        /// maq 执行返回结果 lazy asy
        /// </summary>
        public ValueTask<Lazy<List<T>>> QueryLazyAsy<T>(string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            return new ValueTask<Lazy<List<T>>>(new Lazy<List<T>>(() => Query<T>(name, param, db, key, isOutSql)));
        }
        #endregion


        #region maq 执行返回 List<Dictionary<string, object>>
        /// <summary>
        /// maq 执行返回 List<Dictionary<string, object>>
        /// </summary>
        public List<Dictionary<string, object>> Query(string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            key = key == null ? MapDb(name) : key;
            var config = db == null ? DataConfig.Get(key) : db.config;
            if (config.IsUpdateCache)
                InstanceMap(key);

            if (DbCache.Exists(config.CacheType, name.ToLower()))
            {
                var sql = MapXml.GetMapSql(name, ref param, db, key);
                isOutSql = isOutSql ? isOutSql : IsMapLog(name);

                BaseAop.AopMapBefore(name, sql, param, config,AopType.Map_List_Dic);

                var result = FastRead.ExecuteSql(sql, param, db, key, isOutSql,false);

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

                BaseAop.AopMapAfter(name, sql, param, config, AopType.Map_List_Dic, result);
                return result;
            }
            else
            {
                BaseAop.AopMapBefore(name, "", param, config, AopType.Map_List_Dic);
                var data = new List<Dictionary<string, object>>();
                BaseAop.AopMapAfter(name, "", param, config, AopType.Map_List_Dic, data);
                return data;
            }
        }
        #endregion

        #region maq 执行返回 List<Dictionary<string, object>> asy
        /// <summary>
        /// 执行sql List<Dictionary<string, object>> asy
        /// </summary>
        public ValueTask<List<Dictionary<string, object>>> QueryAsy(string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            return new ValueTask<List<Dictionary<string, object>>>(Query(name, param, db, key, isOutSql));
        }
        #endregion

        #region maq 执行返回 List<Dictionary<string, object>> lazy
        /// <summary>
        /// maq 执行返回 List<Dictionary<string, object>> lazy
        /// </summary>
        public Lazy<List<Dictionary<string, object>>> QueryLazy(string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            return new Lazy<List<Dictionary<string, object>>>(() => Query(name, param, db, key), isOutSql);
        }
        #endregion

        #region maq 执行返回 List<Dictionary<string, object>> lazy asy
        /// <summary>
        /// maq 执行返回 List<Dictionary<string, object>> lazy asy
        /// </summary>
        public ValueTask<Lazy<List<Dictionary<string, object>>>> QueryLazyAsy(string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            return new ValueTask<Lazy<List<Dictionary<string, object>>>>(new Lazy<List<Dictionary<string, object>>>(() => Query(name, param, db, key, isOutSql)));
        }
        #endregion


        #region maq 执行写操作
        /// <summary>
        /// 执行写操作
        /// </summary>
        public WriteReturn Write(string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            key = key == null ? MapDb(name) : key;
            var config = db == null ? DataConfig.Get(key) : db.config;
            if (config.IsUpdateCache)
                InstanceMap(key);

            if (DbCache.Exists(config.CacheType, name.ToLower()))
            {
                var sql = MapXml.GetMapSql(name, ref param, db, key);
                isOutSql = isOutSql ? isOutSql : IsMapLog(name);

                BaseAop.AopMapBefore(name, sql, param, config, AopType.Map_Write);
                var data = FastWrite.ExecuteSql(sql, param, db, key, isOutSql,false);
                BaseAop.AopMapAfter(name, sql, param, config, AopType.Map_Write,data);
                return data;
            }
            else
            {
                BaseAop.AopMapBefore(name, "", param, config, AopType.Map_Write);
                var data = new WriteReturn();
                BaseAop.AopMapAfter(name, "", param, config, AopType.Map_Write, data);
                return data;
            }
        }
        #endregion

        #region maq 执行写操作 asy
        /// <summary>
        ///  maq 执行写操作 asy
        /// </summary>
        public ValueTask<WriteReturn> WriteAsy(string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            return new ValueTask<WriteReturn>(Write(name, param, db, key, isOutSql));
        }
        #endregion

        #region maq 执行写操作 asy lazy
        /// <summary>
        /// maq 执行写操作 asy lazy
        /// </summary>
        public Lazy<WriteReturn> WriteLazy(string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            return new Lazy<WriteReturn>(() => Write(name, param, db, key, isOutSql));
        }
        #endregion

        #region maq 执行写操作 asy lazy asy
        /// <summary>
        /// maq 执行写操作 asy lazy asy
        /// </summary>
        public ValueTask<Lazy<WriteReturn>> WriteLazyAsy(string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            return new ValueTask<Lazy<WriteReturn>>(new Lazy<WriteReturn>(() => Write(name, param, db, key, isOutSql)));
        }
        #endregion


        #region 执行分页
        /// <summary>
        /// 执行分页 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private PageResult ExecuteSqlPage(PageModel pModel, string sql, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            var result = new DataReturn();
            var config = DataConfig.Get(key);
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(key))
                {
                    result = tempDb.GetPageSql(pModel, sql, param,false);
                }
            }
            else
                result = db.GetPageSql(pModel, sql, param,false);

            stopwatch.Stop();

            config.IsOutSql = config.IsOutSql ? config.IsOutSql : isOutSql;

            DbLog.LogSql(config.IsOutSql, result.Sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;

            return result.PageResult;
        }
        #endregion

        #region maq 执行分页
        /// <summary>
        /// maq 执行分页
        /// </summary>
        public PageResult QueryPage(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            key = key == null ? MapDb(name) : key;
            var config = db == null ? DataConfig.Get(key) : db.config;
            if (config.IsUpdateCache)
                InstanceMap(key);

            if (DbCache.Exists(config.CacheType, name.ToLower()))
            {
                var sql = MapXml.GetMapSql(name, ref param, db, key);

                isOutSql = isOutSql ? isOutSql : IsMapLog(name);
                BaseAop.AopMapBefore(name, sql, param, config, AopType.Map_Page_Dic);
                var result = ExecuteSqlPage(pModel, sql, param, db, key, isOutSql);

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

                BaseAop.AopMapAfter(name, sql, param, config, AopType.Map_Page_Dic,result.list);
                return result;
            }
            else
            {
                BaseAop.AopMapBefore(name, "", param, config, AopType.Map_Page_Dic);
                var data = new PageResult();
                BaseAop.AopMapAfter(name, "", param, config, AopType.Map_Page_Dic, data.list);
                return data;
            }
        }
        #endregion

        #region maq 执行分页 asy
        /// <summary>
        /// 执行分页 asy
        /// </summary>
        public ValueTask<PageResult> QueryPageAsy(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            return new ValueTask<PageResult>(QueryPage(pModel, name, param, db, key, isOutSql));
        }
        #endregion

        #region maq 执行分页 lazy
        /// <summary>
        /// maq 执行分页 lazy
        /// </summary>
        public Lazy<PageResult> QueryPageLazy(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            return new Lazy<PageResult>(() => QueryPage(pModel, name, param, db, key, isOutSql));
        }
        #endregion

        #region maq 执行分页 lazy asy
        /// <summary>
        /// maq 执行分页lazy asy
        /// </summary>
        public ValueTask<Lazy<PageResult>> QueryPageLazyAsy(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            return new ValueTask<Lazy<PageResult>>(new Lazy<PageResult>(() => QueryPage(pModel, name, param, db, key, isOutSql)));
        }
        #endregion


        #region 执行分页
        /// <summary>
        /// 执行分页 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private PageResult<T> ExecuteSqlPage<T>(PageModel pModel, string sql, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
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

            config.IsOutSql = config.IsOutSql ? config.IsOutSql : isOutSql;
            stopwatch = null;
            DbLog.LogSql(config.IsOutSql, result.Sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);

            return result.PageResult;
        }
        #endregion

        #region maq 执行分页
        /// <summary>
        /// maq 执行分页
        /// </summary>
        public PageResult<T> QueryPage<T>(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            key = key == null ? MapDb(name) : key;
            var config = db == null ? DataConfig.Get(key) : db.config;
            if (config.IsUpdateCache)
                InstanceMap(key);

            if (DbCache.Exists(config.CacheType, name.ToLower()))
            {
                var sql = MapXml.GetMapSql(name, ref param, db, key);

                isOutSql = isOutSql ? isOutSql : IsMapLog(name);

                BaseAop.AopMapBefore(name, sql, param, config, AopType.Map_Page_Model);

                var result = ExecuteSqlPage<T>(pModel, sql, param, db, key, isOutSql);

                if (MapXml.MapIsForEach(name, config))
                {
                    if (db == null)
                    {
                        using (var tempDb = new DataContext(key))
                        {

                            for (var i = 1; i <= MapXml.MapForEachCount(name, config); i++)
                            {
                                result.list = MapXml.MapForEach<T>(result.list, name, tempDb, config, i);
                            }
                        }
                    }
                    else
                        result.list = MapXml.MapForEach<T>(result.list, name, db, config);
                }

                BaseAop.AopMapAfter(name, sql, param, config, AopType.Map_Page_Model, result.list);
                return result;
            }
            else
            {
                BaseAop.AopMapBefore(name, "", param, config, AopType.Map_Page_Model);
                var data = new PageResult<T>();
                BaseAop.AopMapAfter(name, "", param, config, AopType.Map_Page_Model, data.list);
                return data;
            }
        }
        #endregion

        #region maq 执行分页 asy
        /// <summary>
        /// 执行分页 asy
        /// </summary>
        public ValueTask<PageResult<T>> QueryPageAsy<T>(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            return new ValueTask<PageResult<T>>(QueryPage<T>(pModel, name, param, db, key, isOutSql));
        }
        #endregion

        #region maq 执行分页 lazy
        /// <summary>
        /// maq 执行分页 lazy
        /// </summary>
        public Lazy<PageResult<T>> QueryPageLazy<T>(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            return new Lazy<PageResult<T>>(() => QueryPage<T>(pModel, name, param, db, key, isOutSql));
        }
        #endregion

        #region maq 执行分页 lazy asy
        /// <summary>
        /// maq 执行分页lazy asy
        /// </summary>
        public ValueTask<Lazy<PageResult<T>>> QueryPageLazyAsy<T>(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            return new ValueTask<Lazy<PageResult<T>>>(new Lazy<PageResult<T>>(() => QueryPage<T>(pModel, name, param, db, key, isOutSql)));
        }
        #endregion


        #region map db
        /// <summary>
        /// map db
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string MapDb(string name, bool isMapDb = false)
        {
            if (string.IsNullOrEmpty(this.query.Data.Key) && isMapDb == false)
                return DbCache.Get(DataConfig.Get().CacheType, string.Format("{0}.db", name.ToLower()));
            else
                return this.query.Data.Key;
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

        #region 初始化map
        /// <summary>
        /// 初始化map 3
        /// </summary>
        /// <returns></returns>
        private void InstanceMap(string dbKey = null, string dbFile = "db.json", string mapFile = "map.json")
        {
            var list = BaseConfig.GetValue<MapConfigModel>(AppSettingKey.Map, mapFile);
            var config = DataConfig.Get(dbKey, null, dbFile);
            var db = new DataContext(dbKey);
            var query = new DataQuery { Config = config, Key = dbKey };

            list.Path.ForEach(p =>
            {
                var info = new FileInfo(p);
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
                    DbCache.Get<MapXmlModel>(config.CacheType, key).FileKey.ForEach(a => { DbCache.Remove(config.CacheType, a); });

                    var model = new MapXmlModel();
                    model.LastWrite = info.LastWriteTime;
                    model.FileKey = MapXml.ReadXml(info.FullName, config, info.Name.ToLower().Replace(".xml", ""));
                    model.FileName = info.FullName;
                    if (MapXml.SaveXml(dbKey, key, info, config, db))
                        DbCache.Set<MapXmlModel>(config.CacheType, key, model);
                }
            });

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

        #region 验证xml
        /// <summary>
        /// 验证xml
        /// </summary>
        /// <returns></returns>
        public bool CheckMap(string xml, string dbKey = null)
        {
            var config = DataConfig.Get(dbKey);
            var info = new FileInfo(xml);
            return MapXml.GetXmlList(info.FullName, "sqlMap", config).IsSuccess;
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
            return DbCache.Get(DataConfig.Get().CacheType, string.Format("{0}.db", name.ToLower())).ToStr();
        }
        #endregion

        #region map type
        /// <summary>
        /// map db
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string MapType(string name)
        {
            return DbCache.Get(DataConfig.Get().CacheType, string.Format("{0}.type", name.ToLower())).ToStr();
        }
        #endregion

        #region map view
        /// <summary>
        /// map view
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string MapView(string name)
        {
            return DbCache.Get(DataConfig.Get().CacheType, string.Format("{0}.view", name.ToLower())).ToStr();
        }
        #endregion

        #region 是否存在map id
        /// <summary>
        /// 是否存在map id
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsExists(string name)
        {
            return DbCache.Exists(DataConfig.Get().CacheType, name.ToLower());
        }
        #endregion

        #region 获取map备注
        public string MapRemark(string name)
        {
            return DbCache.Get(DataConfig.Get().CacheType, string.Format("{0}.remark", name.ToLower())).ToStr();
        }
        #endregion

        #region 获取map log
        public bool IsMapLog(string name)
        {
            return string.Compare( DbCache.Get(DataConfig.Get().CacheType, string.Format("{0}.log", name.ToLower())).ToStr(), "true", true) ==0;
        }
        #endregion

        #region 获取map参数备注
        public string MapParamRemark(string name, string param)
        {
            return DbCache.Get(DataConfig.Get().CacheType, string.Format("{0}.{1}.remark", name.ToLower(), param.ToLower())).ToStr();
        }
        #endregion

        #region 获取map验证必填
        /// <summary>
        /// 获取map验证必填
        /// </summary>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public string MapRequired(string name, string param)
        {
            return DbCache.Get(DataConfig.Get().CacheType, string.Format("{0}.{1}.required", name.ToLower(), param.ToLower())).ToStr();
        }
        #endregion

        #region 获取map验证长度
        /// <summary>
        /// 获取map验证长度
        /// </summary>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public string MapMaxlength(string name, string param)
        {
            return DbCache.Get(DataConfig.Get().CacheType, string.Format("{0}.{1}.maxlength", name.ToLower(), param.ToLower())).ToStr();
        }
        #endregion

        #region 获取map验证日期
        /// <summary>
        /// 获取map验证长度
        /// </summary>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public string MapDate(string name, string param)
        {
            return DbCache.Get(DataConfig.Get().CacheType, string.Format("{0}.{1}.date", name.ToLower(), param.ToLower())).ToStr();
        }
        #endregion

        #region 获取map验证map
        /// <summary>
        /// 获取map验证map
        /// </summary>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public string MapCheck(string name, string param)
        {
            return DbCache.Get(DataConfig.Get().CacheType, string.Format("{0}.{1}.checkmap", name.ToLower(), param.ToLower())).ToStr();
        }
        #endregion

        #region 获取map验证map
        /// <summary>
        /// 获取map验证map
        /// </summary>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public string MapExists(string name, string param)
        {
            return DbCache.Get(DataConfig.Get().CacheType, string.Format("{0}.{1}.existsmap", name.ToLower(), param.ToLower())).ToStr();
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
        public WriteReturn AddList<T>(List<T> list, string key = null, bool IsTrans = false, bool isLog = true) where T : class, new()
        {
            ConfigModel config = null;
            var result = new DataReturn<T>();
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            using (var tempDb = new DataContext(key))
            {
                result = tempDb.AddList<T>(list, IsTrans, isLog);
                config = tempDb.config;
            }

            stopwatch.Stop();

            DbLog.LogSql(config.IsOutSql, result.Sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;

            return result.WriteReturn;
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
        public ValueTask<WriteReturn> AddListAsy<T>(List<T> list, string key = null, bool IsTrans = false, bool isLog = true) where T : class, new()
        {
            return new ValueTask<WriteReturn>(AddList<T>(list, key, IsTrans, isLog));
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
        public WriteReturn Add<T>(T model, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
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

            config.IsOutSql = config.IsOutSql ? config.IsOutSql : isOutSql;
            DbLog.LogSql(config.IsOutSql, result.Sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;
            return result.WriteReturn;
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
        public ValueTask<WriteReturn> AddAsy<T>(T model, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            return new ValueTask<WriteReturn>(Add<T>(model, db, key, isOutSql));
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
        public WriteReturn Delete<T>(Expression<Func<T, bool>> predicate, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
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

            config.IsOutSql = config.IsOutSql ? config.IsOutSql : isOutSql;
            DbLog.LogSql(config.IsOutSql, result.Sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;
            return result.WriteReturn;
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
        public ValueTask<WriteReturn> DeleteAsy<T>(Expression<Func<T, bool>> predicate, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            return new ValueTask<WriteReturn>(Delete<T>(predicate, db, key, isOutSql));
        }
        #endregion

        #region 删除
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public WriteReturn Delete<T>(T model, DataContext db = null, string key = null, bool isTrans = false, bool isOutSql = false) where T : class, new()
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

            config.IsOutSql = config.IsOutSql ? config.IsOutSql : isOutSql;
            DbLog.LogSql(config.IsOutSql, result.Sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;

            return result.WriteReturn;
        }
        #endregion

        #region 删除asy
        /// <summary>
        /// 删除asy
        /// </summary>
        /// <returns></returns>
        public ValueTask<WriteReturn> UpdateAsy<T>(T model, DataContext db = null, string key = null, bool isTrans = false, bool isOutSql = false) where T : class, new()
        {
            return new ValueTask<WriteReturn>(Delete<T>(model, db, key, isOutSql));
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
        public WriteReturn Update<T>(T model, Expression<Func<T, bool>> predicate, Expression<Func<T, object>> field = null, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
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

            config.IsOutSql = config.IsOutSql ? config.IsOutSql : isOutSql;
            DbLog.LogSql(config.IsOutSql, result.Sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;

            return result.WriteReturn;
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
        public ValueTask<WriteReturn> UpdateAsy<T>(T model, Expression<Func<T, bool>> predicate, Expression<Func<T, object>> field = null, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            return new ValueTask<WriteReturn>(Update<T>(model, predicate, field, db, key, isOutSql));
        }
        #endregion

        #region 修改
        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public WriteReturn Update<T>(T model, Expression<Func<T, object>> field = null, DataContext db = null, string key = null, bool isTrans = false, bool isOutSql = false) where T : class, new()
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

            config.IsOutSql = config.IsOutSql ? config.IsOutSql : isOutSql;
            DbLog.LogSql(config.IsOutSql, result.Sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;

            return result.WriteReturn;
        }
        #endregion

        #region 修改asy
        /// <summary>
        /// 修改asy
        /// </summary>
        /// <returns></returns>
        public ValueTask<WriteReturn> UpdateAsy<T>(T model, Expression<Func<T, object>> field = null, DataContext db = null, string key = null, bool isTrans = false, bool isOutSql = false) where T : class, new()
        {
            return new ValueTask<WriteReturn>(Update<T>(model, field, db, key, isTrans, isOutSql));
        }
        #endregion

        #region 修改list
        /// <summary>
        /// 修改list
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public WriteReturn UpdateList<T>(List<T> list, Expression<Func<T, object>> field = null, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
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

            config.IsOutSql = config.IsOutSql ? config.IsOutSql : isOutSql;
            DbLog.LogSql(config.IsOutSql, result.Sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;

            return result.WriteReturn;
        }
        #endregion

        #region 修改list asy
        /// <summary>
        /// 修改list asy
        /// </summary>
        /// <returns></returns>
        public ValueTask<WriteReturn> UpdateListAsy<T>(List<T> list, Expression<Func<T, object>> field = null, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            return new ValueTask<WriteReturn>(UpdateList<T>(list, field, db, key, isOutSql));
        }
        #endregion

        #region 执行sql
        /// <summary>
        /// 执行sql
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public WriteReturn ExecuteSql(string sql, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            ConfigModel config = null;
            var result = new DataReturn();
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(key))
                {
                    config = tempDb.config;
                    config.IsOutSql = config.IsOutSql ? config.IsOutSql : isOutSql;
                    result = tempDb.ExecuteSql(sql, param, false, config.IsOutSql,false);
                }
            }
            else
            {
                config = db.config;
                config.IsOutSql = config.IsOutSql ? config.IsOutSql : isOutSql;
                result = db.ExecuteSql(sql, param,false,config.IsOutSql,false);
            }

            stopwatch.Stop();

            DbLog.LogSql(config.IsOutSql, result.Sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;

            return result.WriteReturn;
        }
        #endregion

        #region 执行sql asy
        /// <summary>
        /// 执行sql asy
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public ValueTask<WriteReturn> ExecuteSqlAsy(string sql, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            return new ValueTask<WriteReturn>(ExecuteSql(sql, param, db, key, isOutSql));
        }
        #endregion

        #region 执行sql DDL
        /// <summary>
        /// 执行sql DDL
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public WriteReturn ExecuteDDL(string sql, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            ConfigModel config = null;
            var result = new DataReturn();
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(key))
                {
                    config = tempDb.config;
                    config.IsOutSql = config.IsOutSql ? config.IsOutSql : isOutSql;
                    result = tempDb.ExecuteDDL(sql, param, config.IsOutSql);
                }
            }
            else
            {
                config = db.config;
                config.IsOutSql = config.IsOutSql ? config.IsOutSql : isOutSql;
                result = db.ExecuteSql(sql, param, false, config.IsOutSql);
            }

            stopwatch.Stop();

            DbLog.LogSql(config.IsOutSql, result.Sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;

            return result.WriteReturn;
        }
        #endregion

        #region 执行sql DDL asy
        /// <summary>
        /// 执行sql DDL asy
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public ValueTask<WriteReturn> ExecuteDDLAsy(string sql, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            return new ValueTask<WriteReturn>(ExecuteDDL(sql, param, db, key, isOutSql));
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
        public Queryable<T> Queryable<T>(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> field = null, string key = null, string dbFile = "db.json") where T : class, new()
        {
            var projectName = FastDataExtension.config.Current.GetName().Name;
            var cacheKey = $"FastData.Key.{typeof(Microsoft.Extensions.DependencyInjection.ConfigKey).Name}";

            if(DbCache.Exists(CacheType.Web,cacheKey) && key == null)            
                key = DbCache.Get<Microsoft.Extensions.DependencyInjection.ConfigKey>(CacheType.Web, cacheKey).dbKey;               
            else if (DataConfig.DataType(key, projectName, dbFile) && key == null)
                throw new Exception("数据库查询key不能为空,数据库类型有多个");

            if (this.data.Config != null && this.data.Config.IsChangeDb)
            {
                this.data = new DataQuery();
                key = this.data.Key;
                this.data.Config = DataConfig.Get(key);
                this.data.Key = key;
            }
            else
            {
                this.data = new DataQuery();
                this.data.Config = DataConfig.Get(key);
                this.data.Key = key;
            }

            this.data.TableName.Add(typeof(T).Name);
            this.data.TableAsName.Add(typeof(T).Name, predicate.Parameters[0].Name);

            var queryField = BaseField.QueryField<T>(predicate, field, this.data);
            this.data.Field.Add(queryField.Field);
            this.data.AsName.AddRange(queryField.AsName);

            var condtion = VisitExpression.LambdaWhere<T>(predicate, this.data);
            this.data.Predicate.Add(condtion);
            this.data.Table.Add(string.Format("{0} {1}", typeof(T).Name, predicate.Parameters[0].Name));
            return new Queryable<T>() { Data = this.data };
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
        public IQuery Query<T>(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> field = null, string key = null, string dbFile = "db.json")
        {
            var projectName = FastDataExtension.config.Current.GetName().Name;
            var cacheKey = $"FastData.Key.{typeof(Microsoft.Extensions.DependencyInjection.ConfigKey).Name}";

            if (DbCache.Exists(CacheType.Web, cacheKey) && key == null)
                key = DbCache.Get<Microsoft.Extensions.DependencyInjection.ConfigKey>(CacheType.Web, cacheKey).dbKey;
            else if (DataConfig.DataType(key, projectName, dbFile) && key == null)
                throw new Exception("数据库查询key不能为空,数据库类型有多个");

            if (this.query.Data.Config != null && this.query.Data.Config.IsChangeDb)
            {
                key = this.query.Data.Key;
                this.query.Data = new DataQuery();
                this.query.Data.Config = DataConfig.Get(key);
                this.query.Data.Key = key;
            }
            else
            {
                this.query.Data = new DataQuery();
                this.query.Data.Config = DataConfig.Get(key);
                this.query.Data.Key = key;
            }

            this.query.Data.TableAsName.Add(typeof(T).Name, predicate.Parameters[0].Name);
            this.query.Data.TableName.Add(typeof(T).Name);

            var queryField = BaseField.QueryField<T>(predicate, field, this.query.Data);
            query.Data.Field.Add(queryField.Field);
            query.Data.AsName.AddRange(queryField.AsName);

            var condtion = VisitExpression.LambdaWhere<T>(predicate, this.query.Data);
            this.query.Data.Predicate.Add(condtion);
            this.query.Data.Table.Add(string.Format("{0} {1}", typeof(T).Name, predicate.Parameters[0].Name));
            return this.query;
        }
        #endregion

        #region 多种数据库类型切换
        /// <summary>
        /// 多种数据库类型切换
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IFastRepository SetKey(string key)
        {
            this.query.Data.Config = DataConfig.Get(key);
            this.query.Data.Key = key;
            this.query.Data.Config.IsChangeDb = true;

            this.data.Config = DataConfig.Get(key);
            this.data.Key = key;
            this.data.Config.IsChangeDb = true;
            return this;
        }
        #endregion
    }

    public class FastRepository<T> : IFastRepository<T> where T : class, new ()
    {
        internal DataQuery data = new DataQuery();

        #region 批量增加
        /// <summary>
        /// 批量增加
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="model">实体</param>
        /// <param name="IsTrans">是否事务</param>
        /// <returns></returns>
        public WriteReturn AddList(List<T> list, string key = null, bool IsTrans = false, bool isLog = true)
        {
            ConfigModel config = null;
            var result = new DataReturn<T>();
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            using (var tempDb = new DataContext(key))
            {
                result = tempDb.AddList(list, IsTrans, isLog);
                config = tempDb.config;
            }

            stopwatch.Stop();

            DbLog.LogSql(config.IsOutSql, result.Sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;

            return result.WriteReturn;
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
        public ValueTask<WriteReturn> AddListAsy(List<T> list, string key = null, bool IsTrans = false, bool isLog = true)
        {
            return new ValueTask<WriteReturn>(AddList(list, key, IsTrans, isLog));
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
        public WriteReturn Add(T model, DataContext db = null, string key = null, bool isOutSql = false)
        {
            ConfigModel config = null;
            var result = new DataReturn<T>();
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(key))
                {
                    result = tempDb.Add(model, false);
                    config = tempDb.config;
                }
            }
            else
            {
                result = db.Add(model, false);
                config = db.config;
            }

            stopwatch.Stop();

            config.IsOutSql = config.IsOutSql ? config.IsOutSql : isOutSql;
            DbLog.LogSql(config.IsOutSql, result.Sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;
            return result.WriteReturn;
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
        public ValueTask<WriteReturn> AddAsy(T model, DataContext db = null, string key = null, bool isOutSql = false)
        {
            return new ValueTask<WriteReturn>(Add(model, db, key, isOutSql));
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
        public WriteReturn Delete(Expression<Func<T, bool>> predicate, DataContext db = null, string key = null, bool isOutSql = false)
        {
            ConfigModel config = null;
            var result = new DataReturn<T>();
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(key))
                {
                    result = tempDb.Delete(predicate);
                    config = tempDb.config;
                }
            }
            else
            {
                result = db.Delete(predicate);
                config = db.config;
            }

            stopwatch.Stop();

            config.IsOutSql = config.IsOutSql ? config.IsOutSql : isOutSql;
            DbLog.LogSql(config.IsOutSql, result.Sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;
            return result.WriteReturn;
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
        public ValueTask<WriteReturn> DeleteAsy(Expression<Func<T, bool>> predicate, DataContext db = null, string key = null, bool isOutSql = false)
        {
            return new ValueTask<WriteReturn>(Delete(predicate, db, key, isOutSql));
        }
        #endregion

        #region 删除
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public WriteReturn Delete(T model, DataContext db = null, string key = null, bool isTrans = false, bool isOutSql = false)
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

            config.IsOutSql = config.IsOutSql ? config.IsOutSql : isOutSql;
            DbLog.LogSql(config.IsOutSql, result.Sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;

            return result.WriteReturn;
        }
        #endregion

        #region 删除asy
        /// <summary>
        /// 删除asy
        /// </summary>
        /// <returns></returns>
        public ValueTask<WriteReturn> UpdateAsy(T model, DataContext db = null, string key = null, bool isTrans = false, bool isOutSql = false)
        {
            return new ValueTask<WriteReturn>(Delete(model, db, key, isOutSql));
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
        public WriteReturn Update(T model, Expression<Func<T, bool>> predicate, Expression<Func<T, object>> field = null, DataContext db = null, string key = null, bool isOutSql = false)
        {
            ConfigModel config = null;
            var result = new DataReturn<T>();
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(key))
                {
                    result = tempDb.Update(model, predicate, field);
                    config = tempDb.config;
                }
            }
            else
            {
                result = db.Update(model, predicate, field);
                config = db.config;
            }

            stopwatch.Stop();

            config.IsOutSql = config.IsOutSql ? config.IsOutSql : isOutSql;
            DbLog.LogSql(config.IsOutSql, result.Sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;

            return result.WriteReturn;
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
        public ValueTask<WriteReturn> UpdateAsy(T model, Expression<Func<T, bool>> predicate, Expression<Func<T, object>> field = null, DataContext db = null, string key = null, bool isOutSql = false)
        {
            return new ValueTask<WriteReturn>(Update(model, predicate, field, db, key, isOutSql));
        }
        #endregion

        #region 修改
        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public WriteReturn Update(T model, Expression<Func<T, object>> field = null, DataContext db = null, string key = null, bool isTrans = false, bool isOutSql = false)
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

            config.IsOutSql = config.IsOutSql ? config.IsOutSql : isOutSql;
            DbLog.LogSql(config.IsOutSql, result.Sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;

            return result.WriteReturn;
        }
        #endregion

        #region 修改asy
        /// <summary>
        /// 修改asy
        /// </summary>
        /// <returns></returns>
        public ValueTask<WriteReturn> UpdateAsy(T model, Expression<Func<T, object>> field = null, DataContext db = null, string key = null, bool isTrans = false, bool isOutSql = false)
        {
            return new ValueTask<WriteReturn>(Update(model, field, db, key, isTrans, isOutSql));
        }
        #endregion

        #region 修改list
        /// <summary>
        /// 修改list
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public WriteReturn UpdateList(List<T> list, Expression<Func<T, object>> field = null, DataContext db = null, string key = null, bool isOutSql = false)
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

            config.IsOutSql = config.IsOutSql ? config.IsOutSql : isOutSql;
            DbLog.LogSql(config.IsOutSql, result.Sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);

            return result.WriteReturn;
        }
        #endregion

        #region 修改list asy
        /// <summary>
        /// 修改list asy
        /// </summary>
        /// <returns></returns>
        public ValueTask<WriteReturn> UpdateListAsy(List<T> list, Expression<Func<T, object>> field = null, DataContext db = null, string key = null, bool isOutSql = false)
        {
            return new ValueTask<WriteReturn>(UpdateList(list, field, db, key, isOutSql));
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
        public Queryable<T> Queryable(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> field = null, string key = null, string dbFile = "db.json")
        {
            var projectName = FastDataExtension.config.Current.GetName().Name;
            var cacheKey = $"FastData.Key.{typeof(Microsoft.Extensions.DependencyInjection.ConfigKey).Name}";

            if (DbCache.Exists(CacheType.Web, cacheKey) && key == null)
                key = DbCache.Get<Microsoft.Extensions.DependencyInjection.ConfigKey>(CacheType.Web, cacheKey).dbKey;
            else if (DataConfig.DataType(key, projectName, dbFile) && key == null)
                throw new Exception("数据库查询key不能为空,数据库类型有多个");

            if (this.data.Config != null && this.data.Config.IsChangeDb)
            {
                this.data = new DataQuery();
                key = this.data.Key;
                this.data.Config = DataConfig.Get(key);
                this.data.Key = key;
            }
            else
            {
                this.data = new DataQuery();
                this.data.Config = DataConfig.Get(key);
                this.data.Key = key;
            }

            this.data.TableAsName.Add(typeof(T).Name, predicate.Parameters[0].Name);
            this.data.TableName.Add(typeof(T).Name);

            var queryField = BaseField.QueryField<T>(predicate, field, this.data);
            this.data.Field.Add(queryField.Field);
            this.data.AsName.AddRange(queryField.AsName);

            var condtion = VisitExpression.LambdaWhere<T>(predicate, this.data);
            this.data.Predicate.Add(condtion);
            this.data.Table.Add(string.Format("{0} {1}", typeof(T).Name, predicate.Parameters[0].Name));
            return new Queryable<T>() { Data = this.data };
        }
        #endregion
    }
}