using FastData.Core.Base;
using FastData.Core.CacheModel;
using FastData.Core.Check;
using FastData.Core.Context;
using FastData.Core.Model;
using FastData.Core.Type;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using FastUntility.Core.Base;
using FastUntility.Core.Page;
using FastData.Core.Aop;

namespace FastData.Core
{
    /// <summary>
    /// map
    /// </summary>
    public static class FastMap
    {
        #region 初始化model成员 1
        /// <summary>
        /// 初始化model成员 1
        /// </summary>
        /// <param name="list"></param>
        /// <param name="nameSpace">命名空间</param>
        /// <param name="dll">dll名称</param>
        public static void InstanceProperties(string nameSpace, string dbFile = "db.json", string projectName = null)
        {
            if (projectName == null)
                projectName = Assembly.GetCallingAssembly().GetName().Name;

            var config = DataConfig.Get("", projectName, dbFile);

            var assembly = AppDomain.CurrentDomain.GetAssemblies().ToList().Find(a => a.FullName.Split(',')[0] == projectName);
            if (assembly == null)
                assembly = Assembly.Load(projectName);

            if (assembly != null)
            {
                assembly.ExportedTypes.ToList().ForEach(p =>
                {
                    var typeInfo = (p as TypeInfo);
                    if (typeInfo.Namespace != null && typeInfo.Namespace == nameSpace)
                    {
                        var key = string.Format("{0}.{1}", typeInfo.Namespace, typeInfo.Name);
                        var cacheList = new List<PropertyModel>();
                        typeInfo.DeclaredProperties.ToList().ForEach(a =>
                        {
                            var model = new PropertyModel();
                            model.Name = a.Name;
                            model.PropertyType = a.PropertyType;
                            cacheList.Add(model);
                        });

                        DbCache.Set<List<PropertyModel>>(config.CacheType, key, cacheList);
                    }
                });
            }
        }
        #endregion

        #region 初始化code first 2
        /// <summary>
        /// 初始化code first 2
        /// </summary>
        /// <param name="list"></param>
        /// <param name="nameSpace">命名空间</param>
        /// <param name="dll">dll名称</param>
        public static void InstanceTable(string nameSpace, string dbKey = null, string dbFile = "db.json", string projectName = null)
        {
            if (projectName == null)
                projectName = Assembly.GetCallingAssembly().GetName().Name;

            var query = new DataQuery();
            query.Config = DataConfig.Get(dbKey, projectName, dbFile);
            query.Key = dbKey;

            MapXml.CreateLogTable(query);
            var assembly = AppDomain.CurrentDomain.GetAssemblies().ToList().Find(a => a.FullName.Split(',')[0] == projectName);

            if (assembly == null)
                assembly = Assembly.Load(projectName);

            if (assembly != null)
            {
                assembly.ExportedTypes.ToList().ForEach(a =>
                {
                    var typeInfo = (a as TypeInfo);
                    if (typeInfo.Namespace != null && typeInfo.Namespace == nameSpace)
                        BaseTable.Check(query, a.Name, typeInfo.DeclaredProperties.ToList(), typeInfo.GetCustomAttributes().ToList());
                });
            }
        }
        #endregion

        #region 初始化map 3  by Resource
        public static void InstanceMapResource(string dbKey = null, string dbFile = "db.json", string mapFile = "map.json", string projectName = null)
        {
            if (projectName == null)
                projectName = Assembly.GetCallingAssembly().GetName().Name;

            var config = DataConfig.Get(dbKey, projectName, dbFile);
            using (var db = new DataContext(dbKey))
            {
                var assembly = Assembly.Load(projectName);
                var map = new MapConfigModel();
                using (var resource = assembly.GetManifestResourceStream(string.Format("{0}.{1}", projectName, mapFile)))
                {
                    if (resource != null)
                    {
                        using (var reader = new StreamReader(resource))
                        {
                            var content = reader.ReadToEnd();
                            map.Path = BaseJson.JsonToModel<List<string>>(BaseJson.JsonToDic(BaseJson.ModelToJson(BaseJson.JsonToDic(content).GetValue(AppSettingKey.Map))).GetValue("Path").ToStr());
                        }
                    }
                    else
                        map = BaseConfig.GetValue<MapConfigModel>(AppSettingKey.Map, mapFile);
                }

                if (map.Path == null)
                    return;

                map.Path.ForEach(a =>
                {
                    using (var resource = assembly.GetManifestResourceStream(string.Format("{0}.{1}", projectName, a.Replace("/", "."))))
                    {
                        var xml = "";
                        if (resource != null)
                        {
                            using (var reader = new StreamReader(resource))
                            {
                                xml = reader.ReadToEnd();
                            }
                        }
                        var info = new FileInfo(a);
                        var key = BaseSymmetric.Generate(info.FullName);
                        if (!DbCache.Exists(config.CacheType, key))
                        {
                            var temp = new MapXmlModel();
                            temp.LastWrite = info.LastWriteTime;
                            temp.FileKey = MapXml.ReadXml(info.FullName, config, info.Name.ToLower().Replace(".xml", ""), xml);
                            temp.FileName = info.FullName;
                            if (MapXml.SaveXml(dbKey, key, info, config, db))
                                DbCache.Set<MapXmlModel>(config.CacheType, key, temp);
                        }
                        else if ((DbCache.Get<MapXmlModel>(config.CacheType, key).LastWrite - info.LastWriteTime).Milliseconds != 0)
                        {
                            DbCache.Get<MapXmlModel>(config.CacheType, key).FileKey.ForEach(f => { DbCache.Remove(config.CacheType, f); });

                            var model = new MapXmlModel();
                            model.LastWrite = info.LastWriteTime;
                            model.FileKey = MapXml.ReadXml(info.FullName, config, info.Name.ToLower().Replace(".xml", ""), xml);
                            model.FileName = info.FullName;
                            if (MapXml.SaveXml(dbKey, key, info, config, db))
                                DbCache.Set<MapXmlModel>(config.CacheType, key, model);
                        }
                    }
                });
            }
        }
        #endregion

        #region 初始化map 3
        /// <summary>
        /// 初始化map 3
        /// </summary>
        /// <returns></returns>
        public static void InstanceMap(string dbKey = null, string dbFile = "db.json", string mapFile = "map.json")
        {
            var list = BaseConfig.GetValue<MapConfigModel>(AppSettingKey.Map, mapFile);
            var config = DataConfig.Get(dbKey, null, dbFile);
            using (var db = new DataContext(dbKey))
            {
                var query = new DataQuery { Config = config, Key = dbKey };

                if (list.Path == null)
                    return;

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
            }
        }
        #endregion

        #region maq 执行返回结果
        /// <summary>
        /// maq 执行返回结果
        /// </summary>
        public static List<T> Query<T>(string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            key = key == null ? MapDb(name) : key;
            var config = db == null ? DataConfig.Get(key) : db.config;
            if (config.IsUpdateCache)
                InstanceMap(key);
            if (DbCache.Exists(config.CacheType, name.ToLower()))
            {
                var sql = MapXml.GetMapSql(name, ref param, db, key);
                isOutSql = isOutSql ? isOutSql : IsMapLog(name);
                AopMapBefore(name, sql, param, config, AopType.Map_List_Model);
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
                AopMapAfter(name, sql, param, config, AopType.Map_List_Model, result);
                return result;
            }
            else
            {
                AopMapBefore(name, "", param, config, AopType.Map_List_Model);
                var data = new List<T>();
                AopMapAfter(name, "", param, config, AopType.Map_List_Model, data);
                return data;
            }
        }
        #endregion

        #region maq 执行返回结果 asy
        /// <summary>
        /// 执行sql asy
        /// </summary>
        public static Task<List<T>> QueryAsy<T>(string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            return Task.Run(() =>
            {
                return Query<T>(name, param, db, key, isOutSql);
            });
        }
        #endregion

        #region maq 执行返回结果 lazy
        /// <summary>
        /// maq 执行返回结果 lazy
        /// </summary>
        public static Lazy<List<T>> QueryLazy<T>(string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            return new Lazy<List<T>>(() => Query<T>(name, param, db, key, isOutSql));
        }
        #endregion

        #region maq 执行返回结果 lazy asy
        /// <summary>
        /// maq 执行返回结果 lazy asy
        /// </summary>
        public static Task<Lazy<List<T>>> QueryLazyAsy<T>(string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            return Task.Run(() =>
            {
                return new Lazy<List<T>>(() => Query<T>(name, param, db, key, isOutSql));
            });
        }
        #endregion


        #region maq 执行返回 List<Dictionary<string, object>>
        /// <summary>
        /// maq 执行返回 List<Dictionary<string, object>>
        /// </summary>
        public static List<Dictionary<string, object>> Query(string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            key = key == null ? MapDb(name) : key;
            var config = db == null ? DataConfig.Get(key) : db.config;
            if (config.IsUpdateCache)
                InstanceMap(key);

            if (DbCache.Exists(config.CacheType, name.ToLower()))
            {
                var sql = MapXml.GetMapSql(name, ref param, db, key);
                isOutSql = isOutSql ? isOutSql : IsMapLog(name);

                AopMapBefore(name, sql, param, config, AopType.Map_List_Dic);

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

                AopMapAfter(name, sql, param, config, AopType.Map_List_Dic, result);
                return result;
            }
            else
            {
                AopMapBefore(name, "", param, config, AopType.Map_List_Dic);
                var data = new List<Dictionary<string, object>>();
                AopMapAfter(name, "", param, config, AopType.Map_List_Dic, data);
                return data;
            }
        }
        #endregion

        #region maq 执行返回 List<Dictionary<string, object>> asy
        /// <summary>
        /// 执行sql List<Dictionary<string, object>> asy
        /// </summary>
        public static Task<List<Dictionary<string, object>>> QueryAsy(string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            return Task.Run(() =>
           {
               return Query(name, param, db, key, isOutSql);
           });
        }
        #endregion

        #region maq 执行返回 List<Dictionary<string, object>> lazy
        /// <summary>
        /// maq 执行返回 List<Dictionary<string, object>> lazy
        /// </summary>
        public static Lazy<List<Dictionary<string, object>>> QueryLazy(string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            return new Lazy<List<Dictionary<string, object>>>(() => Query(name, param, db, key, isOutSql));
        }
        #endregion

        #region maq 执行返回 List<Dictionary<string, object>> lazy asy
        /// <summary>
        /// maq 执行返回 List<Dictionary<string, object>> lazy asy
        /// </summary>
        public static Task<Lazy<List<Dictionary<string, object>>>> QueryLazyAsy(string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            return Task.Run(() =>
           {
               return new Lazy<List<Dictionary<string, object>>>(() => Query(name, param, db, key, isOutSql));
           });
        }
        #endregion


        #region maq 执行写操作
        /// <summary>
        /// 执行写操作
        /// </summary>
        public static WriteReturn Write(string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            key = key == null ? MapDb(name) : key;
            var config = db == null ? DataConfig.Get(key) : db.config;
            if (config.IsUpdateCache)
                InstanceMap(key);

            if (DbCache.Exists(config.CacheType, name.ToLower()))
            {
                var sql = MapXml.GetMapSql(name, ref param, db, key);
                AopMapBefore(name, sql, param, config, AopType.Map_Write);
                isOutSql = isOutSql ? isOutSql : IsMapLog(name);
                var result = FastWrite.ExecuteSql(sql, param, db, key, isOutSql,false);
                AopMapAfter(name, sql, param, config, AopType.Map_Write, result.IsSuccess);
                return result;
            }
            else
            {
                AopMapBefore(name, "", param, config, AopType.Map_Write);
                var data = new WriteReturn();
                AopMapAfter(name, "", param, config, AopType.Map_Write, data.IsSuccess);
                return data;
            }
        }
        #endregion

        #region maq 执行写操作 asy
        /// <summary>
        ///  maq 执行写操作 asy
        /// </summary>
        public static Task<WriteReturn> WriteAsy(string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            return Task.Run(() =>
           {
               return Write(name, param, db, key, isOutSql);
           });
        }
        #endregion

        #region maq 执行写操作 asy lazy
        /// <summary>
        /// maq 执行写操作 asy lazy
        /// </summary>
        public static Lazy<WriteReturn> WriteLazy(string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            return new Lazy<WriteReturn>(() => Write(name, param, db, key, isOutSql));
        }
        #endregion

        #region maq 执行写操作 asy lazy asy
        /// <summary>
        /// maq 执行写操作 asy lazy asy
        /// </summary>
        public static Task<Lazy<WriteReturn>> WriteLazyAsy(string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            return Task.Run(() =>
           {
               return new Lazy<WriteReturn>(() => Write(name, param, db, key, isOutSql));
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
        private static PageResult ExecuteSqlPage(PageModel pModel, string sql, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
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

            return result.PageResult;
        }
        #endregion

        #region maq 执行分页
        /// <summary>
        /// maq 执行分页
        /// </summary>
        public static PageResult QueryPage(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            key = key == null ? MapDb(name) : key;
            var config = db == null ? DataConfig.Get(key) : db.config;
            if (config.IsUpdateCache)
                InstanceMap(key);

            if (DbCache.Exists(config.CacheType, name.ToLower()))
            {
                var sql = MapXml.GetMapSql(name, ref param, db, key);

                isOutSql = isOutSql ? isOutSql : IsMapLog(name);

                AopMapBefore(name, sql, param, config, AopType.Map_Page_Dic);

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

                AopMapAfter(name, sql, param, config, AopType.Map_Page_Dic, result.list);
                return result;
            }
            else
            {
                AopMapBefore(name, "", param, config, AopType.Map_Page_Dic);
                var data = new PageResult();
                AopMapAfter(name, "", param, config, AopType.Map_Page_Dic, data.list);
                return data;
            }
        }
        #endregion

        #region maq 执行分页 asy
        /// <summary>
        /// 执行分页 asy
        /// </summary>
        public static Task<PageResult> QueryPageAsy(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            return Task.Run(() =>
           {
               return QueryPage(pModel, name, param, db, key, isOutSql);
           });
        }
        #endregion

        #region maq 执行分页 lazy
        /// <summary>
        /// maq 执行分页 lazy
        /// </summary>
        public static Lazy<PageResult> QueryPageLazy(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            return new Lazy<PageResult>(() => QueryPage(pModel, name, param, db, key, isOutSql));
        }
        #endregion

        #region maq 执行分页 lazy asy
        /// <summary>
        /// maq 执行分页lazy asy
        /// </summary>
        public static Task<Lazy<PageResult>> QueryPageLazyAsy(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            return Task.Run(() =>
           {
               return new Lazy<PageResult>(() => QueryPage(pModel, name, param, db, key, isOutSql));
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
        private static PageResult<T> ExecuteSqlPage<T>(PageModel pModel, string sql, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            var result = new DataReturn<T>();
            var config = DataConfig.Get(key);
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(key))
                {
                    result = tempDb.GetPageSql<T>(pModel, sql, param,false);
                }
            }
            else
                result = db.GetPageSql<T>(pModel, sql, param,false);

            stopwatch.Stop();

            config.IsOutSql = config.IsOutSql ? config.IsOutSql : isOutSql;
            DbLog.LogSql(config.IsOutSql, result.sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);

            return result.pageResult;
        }
        #endregion

        #region maq 执行分页
        /// <summary>
        /// maq 执行分页
        /// </summary>
        public static PageResult<T> QueryPage<T>(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            key = key == null ? MapDb(name) : key;
            var config = db == null ? DataConfig.Get(key) : db.config;
            if (config.IsUpdateCache)
                InstanceMap(key);

            if (DbCache.Exists(config.CacheType, name.ToLower()))
            {
                var sql = MapXml.GetMapSql(name, ref param, db, key);

                isOutSql = isOutSql ? isOutSql : IsMapLog(name);

                AopMapBefore(name, sql, param, config, AopType.Map_Page_Model);

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

                return result;
            }
            else
            {
                AopMapBefore(name, "", param, config, AopType.Map_Page_Model);
                return new PageResult<T>();
            }
        }
        #endregion

        #region maq 执行分页 asy
        /// <summary>
        /// 执行分页 asy
        /// </summary>
        public static Task<PageResult<T>> QueryPageAsy<T>(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            return Task.Run(() =>
           {
               return QueryPage<T>(pModel, name, param, db, key, isOutSql);
           });
        }
        #endregion

        #region maq 执行分页 lazy
        /// <summary>
        /// maq 执行分页 lazy
        /// </summary>
        public static Lazy<PageResult<T>> QueryPageLazy<T>(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            return new Lazy<PageResult<T>>(() => QueryPage<T>(pModel, name, param, db, key, isOutSql));
        }
        #endregion

        #region maq 执行分页 lazy asy
        /// <summary>
        /// maq 执行分页lazy asy
        /// </summary>
        public static Task<Lazy<PageResult<T>>> QueryPageLazyAsy<T>(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            return Task.Run(() =>
           {
               return new Lazy<PageResult<T>>(() => QueryPage<T>(pModel, name, param, db, key, isOutSql));
           });
        }
        #endregion


        #region 验证xml
        /// <summary>
        /// 验证xml
        /// </summary>
        /// <returns></returns>
        public static bool CheckMap(string xml, string dbKey = null)
        {
            var config = DataConfig.Get(dbKey);
            var info = new FileInfo(xml);
            return MapXml.GetXmlList(info.FullName, "sqlMap", config).isSuccess;
        }
        #endregion

        #region map 参数列表
        /// <summary>
        /// map 参数列表
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static List<string> MapParam(string name)
        {
            return DbCache.Get<List<string>>(DataConfig.Get().CacheType, string.Format("{0}.param", name.ToLower()));
        }
        #endregion

        #region map db
        /// <summary>
        /// map db
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string MapDb(string name)
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
        public static string MapType(string name)
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
        public static string MapView(string name)
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
        public static bool IsExists(string name)
        {
            return DbCache.Exists(DataConfig.Get().CacheType, name.ToLower());
        }
        #endregion

        #region  获取api接口key
        /// <summary>
        /// 获取api接口key
        /// </summary>
        public static Dictionary<string, object> Api
        {
            get
            {
                return DbCache.Get<Dictionary<string, object>>(DataConfig.Get().CacheType, "FastMap.Api") ?? new Dictionary<string, object>();
            }
        }
        #endregion

        #region 获取map备注
        public static string MapRemark(string name)
        {
            return DbCache.Get(DataConfig.Get().CacheType, string.Format("{0}.remark", name.ToLower())).ToStr();
        }
        #endregion

        #region 获取map log
        public static bool IsMapLog(string name)
        {
            return DbCache.Get(DataConfig.Get().CacheType, string.Format("{0}.log", name.ToLower())).ToStr().ToLower() == "true";
        }
        #endregion

        #region 获取map参数备注
        public static string MapParamRemark(string name, string param)
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
        public static string MapRequired(string name, string param)
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
        public static string MapMaxlength(string name, string param)
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
        public static string MapDate(string name, string param)
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
        public static string MapCheckMap(string name, string param)
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
        public static string MapExistsMap(string name, string param)
        {
            return DbCache.Get(DataConfig.Get().CacheType, string.Format("{0}.{1}.existsmap", name.ToLower(), param.ToLower())).ToStr();
        }
        #endregion

        /// <summary>
        /// Aop Map Before
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <param name="config"></param>
        private static void AopMapBefore(string mapName, string sql, DbParameter[] param, ConfigModel config, AopType type)
        {
            var aop = FastUntility.Core.ServiceContext.Engine.Resolve<IFastAop>();
            if (aop != null)
            {
                var context = new MapBeforeContext();
                context.mapName = mapName;
                context.sql = sql;
                context.type = type;

                if (param != null)
                    context.param = param.ToList();

                context.dbType = config.DbType;

                aop.MapBefore(context);
            }
        }

        /// <summary>
        /// Aop Map After
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <param name="config"></param>
        private static void AopMapAfter(string mapName, string sql, DbParameter[] param, ConfigModel config, AopType type, object data)
        {
            var aop = FastUntility.Core.ServiceContext.Engine.Resolve<IFastAop>();
            if (aop != null)
            {
                var context = new MapAfterContext();
                context.mapName = mapName;
                context.sql = sql;
                context.type = type;

                if (param != null)
                    context.param = param.ToList();

                context.dbType = config.DbType;
                context.result = data;

                aop.MapAfter(context);
            }
        }
    }
}