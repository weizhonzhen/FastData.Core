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
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using FastUntility.Core.Base;
using FastUntility.Core.Page;
using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace FastData.Core
{
    /// <summary>
    /// map
    /// </summary>
    public static class FastMap
    {
        #region 初始化建日记表
        /// <summary>
        /// 初始化建日记表
        /// </summary>
        /// <param name="query"></param>
        private static void CreateLogTable(DataQuery query)
        {
            if (query.Config.SqlErrorType.ToLower() == SqlErrorType.Db)
            {
                query.Config.DesignModel = FastData.Core.Base.Config.CodeFirst;
                if (query.Config.DbType == DataDbType.Oracle)
                {
                    var listInfo = typeof(FastData.Core.DataModel.Oracle.Data_LogError).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList();
                    var listAttribute = typeof(FastData.Core.DataModel.Oracle.Data_LogError).GetTypeInfo().GetCustomAttributes().ToList();
                    BaseTable.Check(query, "Data_LogError", listInfo, listAttribute);
                }

                if (query.Config.DbType == DataDbType.MySql)
                {
                    var listInfo = typeof(FastData.Core.DataModel.MySql.Data_LogError).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList();
                    var listAttribute = typeof(FastData.Core.DataModel.MySql.Data_LogError).GetTypeInfo().GetCustomAttributes().ToList();
                    BaseTable.Check(query, "Data_LogError", listInfo, listAttribute);
                }

                if (query.Config.DbType == DataDbType.SqlServer)
                {
                    var listInfo = typeof(FastData.Core.DataModel.SqlServer.Data_LogError).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList();
                    var listAttribute = typeof(FastData.Core.DataModel.SqlServer.Data_LogError).GetTypeInfo().GetCustomAttributes().ToList();
                    BaseTable.Check(query, "Data_LogError", listInfo, listAttribute);
                }
            }
        }
        #endregion

        #region 初始化model成员 1
        /// <summary>
        /// 初始化model成员 1
        /// </summary>
        /// <param name="list"></param>
        /// <param name="nameSpace">命名空间</param>
        /// <param name="dll">dll名称</param>
        public static void InstanceProperties(string nameSpace, string dll)
        {
            var config = DataConfig.Get();

            var assembly = AppDomain.CurrentDomain.GetAssemblies().ToList().Find(a => a.FullName.Split(',')[0] == dll.Replace(".dll", ""));
            if (assembly == null)
                assembly = Assembly.Load(dll.Replace(".dll", ""));

            if (assembly != null)
            {
                foreach (var temp in assembly.ExportedTypes)
                {
                    Task.Factory.StartNew(() =>
                    {
                        var typeInfo = (temp as TypeInfo);
                        if (typeInfo.Namespace != null && typeInfo.Namespace.Contains(nameSpace))
                        {
                            var key = string.Format("{0}.{1}", typeInfo.Namespace, typeInfo.Name);

                            var cacheList = new List<PropertyModel>();
                            foreach (var info in typeInfo.DeclaredProperties)
                            {
                                var model = new PropertyModel();
                                model.Name = info.Name;
                                model.PropertyType = info.PropertyType;
                                cacheList.Add(model);
                            }

                            DbCache.Set<List<PropertyModel>>(config.CacheType, key, cacheList);
                        }
                    });
                }
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
        public static void InstanceTable(string nameSpace, string dll, string dbKey = null)
        {
            var query = new DataQuery();
            query.Config = DataConfig.Get(dbKey);
            query.Key = dbKey;

            CreateLogTable(query);
            var assembly = AppDomain.CurrentDomain.GetAssemblies().ToList().Find(a => a.FullName.Split(',')[0] == dll.Replace(".dll", ""));

            if (assembly == null)
                assembly = Assembly.Load(dll.Replace(".dll", ""));

            if (assembly != null)
            {
                foreach (var temp in assembly.ExportedTypes)
                {
                    var typeInfo = (temp as TypeInfo);
                    if (typeInfo.Namespace != null && typeInfo.Namespace.Contains(nameSpace))
                        BaseTable.Check(query, temp.Name, typeInfo.DeclaredProperties.ToList(), typeInfo.GetCustomAttributes().ToList());
                }
            }
        }
        #endregion

        #region 初始化map 3
        /// <summary>
        /// 初始化map 3
        /// </summary>
        /// <returns></returns>
        public static void InstanceMap(string dbKey = null)
        {
            var list = BaseConfig.GetValue<MapConfigModel>(AppSettingKey.Map,"map.json");
            var config = DataConfig.Get(dbKey);
            var db = new DataContext(dbKey);
            var query = new DataQuery { Config = config, Key = dbKey };

            foreach (var item in list.Path)
            {
                var info = new FileInfo(item);
                var key = BaseSymmetric.Generate(info.FullName);

                if (!DbCache.Exists(config.CacheType,key))
                {
                    var temp = new MapXmlModel();
                    temp.LastWrite = info.LastWriteTime;
                    temp.FileKey = ReadXml(info.FullName, config);
                    temp.FileName = info.FullName;
                    if (SaveXml(dbKey, key, info, config, db))
                        DbCache.Set<MapXmlModel>(config.CacheType, key, temp);
                }
                else if ((DbCache.Get<MapXmlModel>(config.CacheType, key).LastWrite - info.LastWriteTime).Minutes != 0)
                {
                    foreach (var temp in DbCache.Get<MapXmlModel>(config.CacheType, key).FileKey)
                        DbCache.Remove(config.CacheType, temp);

                    var model = new MapXmlModel();
                    model.LastWrite = info.LastWriteTime;
                    model.FileKey = ReadXml(info.FullName, config);
                    model.FileName = info.FullName;
                    if (SaveXml(dbKey, key, info, config, db))
                        DbCache.Set<MapXmlModel>(config.CacheType, key, model);
                }
            }

            CreateLogTable(query);

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

        #region maq 执行返回结果
        /// <summary>
        /// maq 执行返回结果
        /// </summary>
        public static List<T> Query<T>(string name, DbParameter[] param, DataContext db = null, string key = null) where T : class, new()
        {
            key = key == null ? MapDb(name) : key;
            var config = db == null ? DataConfig.Get(key) : db.config;
            if (config.IsUpdateCache)
                InstanceMap(key);
            if (DbCache.Exists(config.CacheType,name.ToLower()))
            {
                var sql = GetMapSql(name, ref param,db,key);
                var result = FastRead.ExecuteSql<T>(sql, param, db, key);
                if (MapIsForEach(name, config))
                {
                    if (db == null)
                    {
                        var tempDb = BaseContext.GetContext(key);

                        for (var i = 1; i <= MapForEachCount(name, config); i++)
                        {
                            result = MapForEach<T>(result, name, tempDb, key, config,i);
                        }
                        tempDb.Dispose();
                    }
                    else
                        result = MapForEach<T>(result, name, db, key, config);
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
        public static async Task<List<T>> QueryAsy<T>(string name, DbParameter[] param, DataContext db = null, string key = null) where T : class, new()
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
        public static Lazy<List<T>> QueryLazy<T>(string name, DbParameter[] param, DataContext db = null, string key = null) where T : class, new()
        {
            return new Lazy<List<T>>(() => Query<T>(name, param, db, key));
        }
        #endregion

        #region maq 执行返回结果 lazy asy
        /// <summary>
        /// maq 执行返回结果 lazy asy
        /// </summary>
        public static async Task<Lazy<List<T>>> QueryLazyAsy<T>(string name, DbParameter[] param, DataContext db = null, string key = null) where T : class, new()
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
        public static List<Dictionary<string, object>> Query(string name, DbParameter[] param, DataContext db = null, string key = null)
        {
            key = key == null ? MapDb(name) : key;
            var config = db == null ? DataConfig.Get(key) : db.config;
            if (config.IsUpdateCache)
                InstanceMap(key);

            if (DbCache.Exists(config.CacheType,name.ToLower()))
            {
                var sql = GetMapSql(name, ref param,db,key);

                var result = FastRead.ExecuteSql(sql, param, db, key);

                if (MapIsForEach(name,config))
                {
                    if (db == null)
                    {
                        var tempDb = BaseContext.GetContext(key);
                        for (var i = 1; i <= MapForEachCount(name, config); i++)
                        {
                            result = MapForEach(result, name, tempDb, key, config,i);
                        }
                        tempDb.Dispose();
                    }
                    else
                        result = MapForEach(result, name, db, key, config);
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
        public static async Task<List<Dictionary<string, object>>> QueryAsy(string name, DbParameter[] param, DataContext db = null, string key = null)
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
        public static Lazy<List<Dictionary<string, object>>> QueryLazy(string name, DbParameter[] param, DataContext db = null, string key = null)
        {
            return new Lazy<List<Dictionary<string, object>>>(() => Query(name, param, db, key));
        }
        #endregion

        #region maq 执行返回 List<Dictionary<string, object>> lazy asy
        /// <summary>
        /// maq 执行返回 List<Dictionary<string, object>> lazy asy
        /// </summary>
        public static async Task<Lazy<List<Dictionary<string, object>>>> QueryLazyAsy(string name, DbParameter[] param, DataContext db = null, string key = null)
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
        public static WriteReturn Write(string name, DbParameter[] param, DataContext db = null, string key = null)
        {
            key = key == null ? MapDb(name) : key;
            var config = db == null ? DataConfig.Get(key) : db.config;
            if (config.IsUpdateCache)
                InstanceMap(key);

            if (DbCache.Exists(config.CacheType,name.ToLower()))
            {
                var sql = GetMapSql(name, ref param,db,key);

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
        public static async Task<WriteReturn> WriteAsy(string name, DbParameter[] param, DataContext db = null, string key = null)
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
        public static Lazy<WriteReturn> WriteLazy(string name, DbParameter[] param, DataContext db = null, string key = null)
        {
            return new Lazy<WriteReturn>(() => Write(name, param, db, key));
        }
        #endregion

        #region maq 执行写操作 asy lazy asy
        /// <summary>
        /// maq 执行写操作 asy lazy asy
        /// </summary>
        public static async Task<Lazy<WriteReturn>> WriteLazyAsy(string name, DbParameter[] param, DataContext db = null, string key = null)
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
        private static PageResult ExecuteSqlPage(PageModel pModel, string sql, DbParameter[] param, DataContext db = null, string key = null)
        {
            var result = new DataReturn();
            var config = DataConfig.Get(key);
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            if (db == null)
            {
                var tempDb = BaseContext.GetContext(key);
                result = tempDb.GetPageSql(pModel, sql, param);
                tempDb.Dispose();
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
        public static PageResult QueryPage(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null)
        {
            key = key == null ? MapDb(name) : key;
            var config = db == null ? DataConfig.Get(key) : db.config;
            if (config.IsUpdateCache)
                InstanceMap(key);

            if (DbCache.Exists(config.CacheType, name.ToLower()))
            {
                var sql = GetMapSql(name, ref param, db, key);

                var result = ExecuteSqlPage(pModel, sql, param, db, key);

                if (MapIsForEach(name,config))
                {
                    if (db == null)
                    {
                        var tempDb = BaseContext.GetContext(key);

                        for (var i = 1; i <= MapForEachCount(name, config); i++)
                        {
                            result.list = MapForEach(result.list, name, tempDb, key, config,i);
                        }
                        tempDb.Dispose();
                    }
                    else
                        result.list = MapForEach(result.list, name, db, key, config);
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
        public static async Task<PageResult> QueryPageAsy(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null)
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
        public static Lazy<PageResult> QueryPageLazy(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null)
        {
            return new Lazy<PageResult>(() => QueryPage(pModel, name, param, db, key));
        }
        #endregion

        #region maq 执行分页 lazy asy
        /// <summary>
        /// maq 执行分页lazy asy
        /// </summary>
        public static async Task<Lazy<PageResult>> QueryPageLazyAsy(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null)
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
        private static PageResult<T> ExecuteSqlPage<T>(PageModel pModel, string sql, DbParameter[] param, DataContext db = null, string key = null) where T : class, new()
        {
            var result = new DataReturn<T>();
            var config = DataConfig.Get(key);
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            if (db == null)
            {
                var tempDb = BaseContext.GetContext(key);
                result = tempDb.GetPageSql<T>(pModel, sql, param);
                tempDb.Dispose();
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
        public static PageResult<T> QueryPage<T>(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null) where T : class, new()
        {
            key = key == null ? MapDb(name) : key;
            var config = db == null ? DataConfig.Get(key) : db.config;
            if (config.IsUpdateCache)
                InstanceMap(key);

            if (DbCache.Exists(config.CacheType,name.ToLower()))
            {
                var sql = GetMapSql(name, ref param,db,key);

                var result = ExecuteSqlPage<T>(pModel, sql, param, db, key);

                if (MapIsForEach(name, config))
                {
                    if (db == null)
                    {
                        var tempDb = BaseContext.GetContext(key);

                        for (var i = 1; i <= MapForEachCount(name, config); i++)
                        {
                            result.list = MapForEach<T>(result.list, name, tempDb, key, config,i);
                        }
                        tempDb.Dispose();
                    }
                    else
                        result.list = MapForEach<T>(result.list, name, db, key, config);
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
        public static async Task<PageResult<T>> QueryPageAsy<T>(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null) where T : class, new()
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
        public static Lazy<PageResult<T>> QueryPageLazy<T>(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null) where T : class, new()
        {
            return new Lazy<PageResult<T>>(() => QueryPage<T>(pModel, name, param, db, key));
        }
        #endregion

        #region maq 执行分页 lazy asy
        /// <summary>
        /// maq 执行分页lazy asy
        /// </summary>
        public static async Task<Lazy<PageResult<T>>> QueryPageLazyAsy<T>(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null) where T : class, new()
        {
            return await Task.Run(() =>
            {
                return new Lazy<PageResult<T>>(() => QueryPage<T>(pModel, name, param, db, key));
            });
        }
        #endregion


        #region 读取xml map并缓存
        /// <summary>
        /// 读取xml map并缓存
        /// </summary>
        private static List<string> ReadXml(string path, ConfigModel config)
        {
            var map = Api ?? new List<string>();
            var key = new List<string>();
            var sql = new List<string>();
            var db = new Dictionary<string, object>();
            var type = new Dictionary<string, object>();
            var param = new Dictionary<string,object>();
            var check = new Dictionary<string, object>();
            
            GetXmlList(path, "sqlMap", ref key, ref sql,ref db, ref type, ref check, ref param, config);
            
            for (var i = 0; i < key.Count; i++)
            {
                DbCache.Set(config.CacheType, key[i].ToLower(), sql[i]);
            }

            foreach (KeyValuePair<string,object> item in db)
            {
                DbCache.Set(config.CacheType, string.Format("{0}.db", item.Key.ToLower()),item.Value);

                if (!map.Exists(a => a.ToLower() == item.Key.ToLower()))
                    map.Add(item.Key.ToLower());
            }
            DbCache.Set<List<string>>(config.CacheType, "FastMap.Api", map);
            key.Add("FastMap.Api");

            foreach (KeyValuePair<string, object> item in type)
            {
                DbCache.Set(config.CacheType, string.Format("{0}.type", item.Key.ToLower()), item.Value);
                key.Add(string.Format("{0}.type", item.Key.ToLower()));
            }

            foreach (KeyValuePair<string, object> item in param)
            {
                DbCache.Set<List<string>>(config.CacheType, string.Format("{0}.param", item.Key.ToLower()), item.Value as List<string>);
                key.Add(string.Format("{0}.param", item.Key.ToLower()));
            }

            foreach (KeyValuePair<string, object> item in check)
            {
                DbCache.Set(config.CacheType, item.Key, item.Value);
                key.Add(item.Key);
            }

            return key;
        }
        #endregion

        #region 返回字符串列表
        /// <summary>
        /// 返回字符串列表
        /// </summary>
        /// <param name="path">文件名</param>
        /// <param name="xmlNode">结点</param>
        /// <returns></returns>
        private static void GetXmlList(string path, string xmlNode, 
            ref List<string> key, ref List<string> sql,ref Dictionary<string, object> db, 
            ref Dictionary<string, object> type, ref Dictionary<string, object> check,
            ref Dictionary<string, object> param, ConfigModel config)
        {
            try
            {
                var tempKey = "";

                //变量
                var xmlDoc = new XmlDocument();

                //载入xml
                if (config.IsEncrypt)
                {
                    var temp = BaseSymmetric.DecodeGB2312(File.ReadAllText(path));
                    if (temp != "")
                        xmlDoc.LoadXml(temp);
                    else
                        xmlDoc.Load(path);
                }
                else
                    xmlDoc.Load(path);

                //结点
                var nodelList = xmlDoc.SelectNodes(xmlNode);

                var list = new List<string>();

                foreach (XmlNode item in nodelList)
                {
                    foreach (XmlNode temp in item.ChildNodes)
                    {
                        var foreachCount = 1;
                        var i = 0;
                        if (temp is XmlElement)
                        {
                            var tempParam = new List<string>();
                            #region XmlElement
                            tempKey = temp.Attributes["id"].Value.ToLower();

                            //节点数
                            if (Array.Exists(key.ToArray(), element => element == tempKey))
                                Task.Run(() => { BaseLog.SaveLog(string.Format("xml文件:{0},存在相同键:{1}", path, tempKey), "MapKeyExists"); });
                            key.Add(tempKey);
                            sql.Add(temp.ChildNodes.Count.ToString());                  

                            foreach (XmlNode node in temp.ChildNodes)
                            {
                                #region XmlText
                                if (node is XmlText)
                                {
                                    key.Add(string.Format("{0}.{1}", tempKey, i));
                                    sql.Add(node.InnerText.Replace("&lt;", "<").Replace("&gt", ">"));
                                }
                                #endregion

                                #region XmlElement 动态条件
                                if (node is XmlElement)
                                {
                                    if (node.Attributes["prepend"] != null)
                                    {
                                        key.Add(string.Format("{0}.format.{1}", tempKey, i));
                                        sql.Add(node.Attributes["prepend"].Value.ToLower());
                                    }

                                    //foreach
                                    if (node.Name.ToLower() == "foreach")
                                    {
                                        //type
                                        if (node.Attributes["type"] != null)
                                        {
                                            key.Add(string.Format("{0}.foreach.type.{1}", tempKey, foreachCount));
                                            sql.Add(node.Attributes["type"].Value);
                                        }

                                        //result name
                                        key.Add(string.Format("{0}.foreach.name.{1}", tempKey,foreachCount));
                                        if (node.Attributes["name"] != null)
                                            sql.Add(node.Attributes["name"].Value.ToLower());
                                        else
                                            sql.Add("data");

                                        //field
                                        if (node.Attributes["field"] != null)
                                        {
                                            key.Add(string.Format("{0}.foreach.field.{1}", tempKey, foreachCount));
                                            sql.Add(node.Attributes["field"].Value.ToLower());
                                        }

                                        //sql
                                        if (node.ChildNodes[0] is XmlText)
                                        {
                                            key.Add(string.Format("{0}.foreach.sql.{1}", tempKey, foreachCount));
                                            sql.Add(node.ChildNodes[0].InnerText.Replace("&lt;", "<").Replace("&gt", ">"));
                                        }
                                        foreachCount++;
                                    }

                                    foreach (XmlNode dyn in node.ChildNodes)
                                    {
                                        if (dyn is XmlText)
                                            continue;

                                        //check required
                                        if (dyn.Attributes["required"] != null)
                                            check.Add(string.Format("{0}.{1}.required", tempKey, dyn.Attributes["property"].Value.ToLower()), dyn.Attributes["required"].Value.ToStr());

                                        //check maxlength
                                        if (dyn.Attributes["maxlength"] != null)
                                            check.Add(string.Format("{0}.{1}.maxlength", tempKey, dyn.Attributes["property"].Value.ToLower()), dyn.Attributes["maxlength"].Value.ToStr());

                                        //check existsmap
                                        if (dyn.Attributes["existsmap"] != null)
                                            check.Add(string.Format("{0}.{1}.existsmap", tempKey, dyn.Attributes["property"].Value.ToLower()), dyn.Attributes["existsmap"].Value.ToStr());

                                        //check checkmap
                                        if (dyn.Attributes["checkmap"] != null)
                                            check.Add(string.Format("{0}.{1}.checkmap", tempKey, dyn.Attributes["property"].Value.ToLower()), dyn.Attributes["checkmap"].Value.ToStr());

                                        //check date
                                        if (dyn.Attributes["date"] != null)
                                            check.Add(string.Format("{0}.{1}.date", tempKey, dyn.Attributes["property"].Value.ToLower()), dyn.Attributes["date"].Value.ToStr());
                                        
                                        //参数
                                        tempParam.Add(dyn.Attributes["property"].Value);

                                        if (dyn.Name.ToLower() == "ispropertyavailable")
                                        {
                                            //属性和值
                                            key.Add(string.Format("{0}.{1}.{2}", tempKey, dyn.Attributes["property"].Value.ToLower(), i));
                                            sql.Add(string.Format("{0}{1}", dyn.Attributes["prepend"].Value.ToLower(), dyn.InnerText));
                                        }
                                        else if (dyn.Name.ToLower() != "choose")
                                        {
                                            //属性和值
                                            key.Add(string.Format("{0}.{1}.{2}", tempKey, dyn.Attributes["property"].Value.ToLower(), i));
                                            sql.Add(string.Format("{0}{1}", dyn.Attributes["prepend"].Value.ToLower(), dyn.InnerText));

                                            //条件类型
                                            key.Add(string.Format("{0}.{1}.condition.{2}", tempKey, dyn.Attributes["property"].Value.ToLower(), i));
                                            sql.Add(dyn.Name);

                                            //判断条件内容
                                            if (dyn.Attributes["condition"] != null)
                                            {
                                                key.Add(string.Format("{0}.{1}.condition.value.{2}", tempKey, dyn.Attributes["property"].Value.ToLower(), i));
                                                sql.Add(dyn.Attributes["condition"].Value);
                                            }

                                            //比较条件值
                                            if (dyn.Attributes["compareValue"] != null)
                                            {
                                                key.Add(string.Format("{0}.{1}.condition.value.{2}", tempKey, dyn.Attributes["property"].Value.ToLower(), i));
                                                sql.Add(dyn.Attributes["compareValue"].Value.ToLower());
                                            }
                                        }
                                        else
                                        {
                                            //条件类型
                                            key.Add(string.Format("{0}.{1}.condition.{2}", tempKey, dyn.Attributes["property"].Value.ToLower(), i));
                                            sql.Add(dyn.Name);

                                            if (dyn is XmlElement)
                                            {
                                                var count = 0;
                                                key.Add(string.Format("{0}.{1}.{2}", tempKey, dyn.Attributes["property"].Value.ToLower(), i));
                                                sql.Add(dyn.ChildNodes.Count.ToStr());
                                                foreach (XmlNode child in dyn.ChildNodes)
                                                {
                                                    //条件
                                                    key.Add(string.Format("{0}.{1}.{2}.choose.condition.{3}", tempKey, dyn.Attributes["property"].Value.ToLower(), i, count));
                                                    sql.Add(child.Attributes["property"].Value);

                                                    //内容
                                                    key.Add(string.Format("{0}.{1}.{2}.choose.{3}", tempKey, dyn.Attributes["property"].Value.ToLower(), i, count));
                                                    sql.Add(string.Format("{0}{1}", child.Attributes["prepend"].Value.ToLower(), child.InnerText));

                                                    count++;
                                                }
                                            }
                                        }
                                    }
                                }
                                #endregion

                                i++;
                            }

                            //db
                            if (temp.Attributes["db"] != null)
                                db.Add(tempKey, temp.Attributes["db"].Value.ToStr());

                            //type
                            if (temp.Attributes["type"] != null)
                                type.Add(tempKey, temp.Attributes["type"].Value.ToStr());

                            //foreach count
                            key.Add(string.Format("{0}.foreach", tempKey));
                            sql.Add((foreachCount-1).ToStr());

                            param.Add(tempKey, tempParam);
                            #endregion
                        }
                        else if (temp is XmlText)
                        {
                            #region XmlText
                            key.Add(string.Format("{0}.{1}", item.Attributes["id"].Value.ToLower(), i));
                            sql.Add(temp.InnerText.Replace("&lt;", "<").Replace("&gt", ">"));

                            key.Add(item.Attributes["id"].Value.ToLower());
                            sql.Add("0");
                            #endregion
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Task.Run(() =>
                {
                    if (config.SqlErrorType == SqlErrorType.Db)
                        DbLogTable.LogException(config, ex, "InstanceMap", "GetXmlList");
                    else
                        DbLog.LogException(true, "InstanceMap", ex, "GetXmlList", ""); 
                });
            }
        }
        #endregion

        #region 获取map sql语句
        /// <summary>
        /// 获取map sql语句
        /// </summary>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private static string GetMapSql(string name, ref DbParameter[] param, DataContext db, string key)
        {
            var tempParam = param.ToList();
            var sql = new StringBuilder();
            var flag = "";
            var cacheType = "";
            
            if (db != null)
            {
                flag = db.config.Flag;
                cacheType = db.config.CacheType;
            }
            else if (key != null)
            {
                flag = DataConfig.Get(key).Flag;
                cacheType = DataConfig.Get(key).CacheType;
            }

            for (var i = 0; i <= DbCache.Get(cacheType,name.ToLower()).ToInt(0); i++)
            {
                #region 文本
                var txtKey = string.Format("{0}.{1}", name.ToLower(), i);
                if (DbCache.Exists(cacheType, txtKey))
                    sql.Append(DbCache.Get(cacheType, txtKey));
                #endregion

                #region 动态
                var dynKey = string.Format("{0}.format.{1}", name.ToLower(), i);
                if (DbCache.Exists(cacheType, dynKey))
                {
                    if (param != null)
                    {
                        var tempSql = new StringBuilder();
                        foreach (var item in MapParam(name))
                        {
                            if (!param.ToList().Exists(a => a.ParameterName.ToLower() == item.ToLower()))
                                continue;
                            var temp = param.ToList().Find(a => a.ParameterName.ToLower() == item.ToLower());

                            var paramKey = string.Format("{0}.{1}.{2}", name.ToLower(), temp.ParameterName.ToLower(), i);
                            var conditionKey = string.Format("{0}.{1}.condition.{2}", name.ToLower(), temp.ParameterName.ToLower(), i);
                            var conditionValueKey = string.Format("{0}.{1}.condition.value.{2}", name.ToLower(), temp.ParameterName.ToLower(), i);
                            if (DbCache.Exists(cacheType, paramKey))
                            {
                                var flagParam = string.Format("{0}{1}", flag, temp.ParameterName.ToLower());
                                var tempKey = string.Format("#{0}#", temp.ParameterName.ToLower());
                                var paramSql = DbCache.Get(cacheType, paramKey).ToLower();
                                var condition = DbCache.Get(cacheType, conditionKey).ToStr().ToLower();
                                var conditionValue = DbCache.Get(cacheType, conditionValueKey).ToStr().ToLower();
                                switch (condition)
                                {
                                    case "isequal":
                                        {
                                            if (conditionValue == temp.Value.ToStr())
                                            {
                                                if (paramSql.IndexOf(tempKey) >= 0)
                                                {
                                                    tempParam.Remove(temp);
                                                    tempSql.Append(paramSql.ToString().Replace(tempKey, temp.Value.ToString()));
                                                }
                                                else if (paramSql.IndexOf(flagParam) < 0 && flag != "")
                                                {
                                                    tempParam.Remove(temp);
                                                    tempSql.Append(DbCache.Get(cacheType, paramKey));
                                                }
                                                else
                                                    tempSql.Append(DbCache.Get(cacheType, paramKey));
                                            }
                                            else
                                                tempParam.Remove(temp);
                                            break;
                                        }
                                    case "isnotequal":
                                        {
                                            if (conditionValue != temp.Value.ToStr())
                                            {
                                                if (paramSql.IndexOf(tempKey) >= 0)
                                                {
                                                    tempParam.Remove(temp);
                                                    tempSql.Append(paramSql.ToString().Replace(temp.ParameterName.ToLower(), temp.Value.ToString()));
                                                }
                                                else if (paramSql.IndexOf(flagParam) < 0 && flag != "")
                                                {
                                                    tempParam.Remove(temp);
                                                    tempSql.Append(DbCache.Get(cacheType, paramKey));
                                                }
                                                else
                                                    tempSql.Append(DbCache.Get(cacheType, paramKey));
                                            }
                                            else
                                                tempParam.Remove(temp);
                                            break;
                                        }
                                    case "isgreaterthan":
                                        {
                                            if (temp.Value.ToStr().ToDecimal(0) > conditionValue.ToDecimal(0))
                                            {
                                                if (paramSql.IndexOf(tempKey) >= 0)
                                                {
                                                    tempParam.Remove(temp);
                                                    tempSql.Append(paramSql.ToString().Replace(tempKey, temp.Value.ToString()));
                                                }
                                                else if (paramSql.IndexOf(flagParam) < 0 && flag != "")
                                                {
                                                    tempParam.Remove(temp);
                                                    tempSql.Append(DbCache.Get(cacheType, paramKey));
                                                }
                                                else
                                                    tempSql.Append(DbCache.Get(cacheType, paramKey));
                                            }
                                            else
                                                tempParam.Remove(temp);
                                            break;
                                        }
                                    case "islessthan":
                                        {
                                            if (temp.Value.ToStr().ToDecimal(0) < conditionValue.ToDecimal(0))
                                            {
                                                if (paramSql.IndexOf(tempKey) >= 0)
                                                {
                                                    tempParam.Remove(temp);
                                                    tempSql.Append(paramSql.ToString().Replace(tempKey, temp.Value.ToString()));
                                                }
                                                else if (paramSql.IndexOf(flagParam) < 0 && flag != "")
                                                {
                                                    tempParam.Remove(temp);
                                                    tempSql.Append(DbCache.Get(cacheType, paramKey));
                                                }
                                                else
                                                    tempSql.Append(DbCache.Get(cacheType, paramKey));
                                            }
                                            else
                                                tempParam.Remove(temp);
                                            break;
                                        }
                                    case "isnullorempty":
                                        {
                                            if (string.IsNullOrEmpty(temp.Value.ToStr()))
                                            {
                                                if (paramSql.IndexOf(tempKey) >= 0)
                                                {
                                                    tempParam.Remove(temp);
                                                    tempSql.Append(paramSql.ToString().Replace(tempKey, temp.Value.ToString()));
                                                }
                                                else if (paramSql.IndexOf(flagParam) < 0 && flag != "")
                                                {
                                                    tempParam.Remove(temp);
                                                    tempSql.Append(DbCache.Get(cacheType, paramKey));
                                                }
                                                else
                                                    tempSql.Append(DbCache.Get(cacheType, paramKey));
                                            }
                                            else
                                                tempParam.Remove(temp);
                                            break;
                                        }
                                    case "isnotnullorempty":
                                        {
                                            if (!string.IsNullOrEmpty(temp.Value.ToStr()))
                                            {
                                                if (paramSql.IndexOf(tempKey) >= 0)
                                                {
                                                    tempParam.Remove(temp);
                                                    tempSql.Append(paramSql.ToString().Replace(tempKey, temp.Value.ToString()));
                                                }
                                                else if (paramSql.IndexOf(flagParam) < 0 && flag != "")
                                                {
                                                    tempParam.Remove(temp);
                                                    tempSql.Append(DbCache.Get(cacheType, paramKey));
                                                }
                                                else
                                                    tempSql.Append(DbCache.Get(cacheType, paramKey));
                                            }
                                            else
                                                tempParam.Remove(temp);
                                            break;
                                        }
                                    case "if":
                                        {
                                            //conditionValue = conditionValue.Replace(temp.ParameterName.ToLower(), temp.Value.ToStr());
                                            conditionValue = conditionValue.Replace(temp.ParameterName, temp.Value == null ? null : temp.Value.ToStr());
                                            conditionValue = conditionValue.Replace("#", "\"");
                                            if (CSharpScript.EvaluateAsync<bool>(conditionValue).Result)
                                            {
                                                if (paramSql.IndexOf(tempKey) >= 0)
                                                {
                                                    tempParam.Remove(temp);
                                                    tempSql.Append(paramSql.ToString().Replace(tempKey, temp.Value.ToString()));
                                                }
                                                else if (paramSql.IndexOf(flagParam) < 0 && flag != "")
                                                {
                                                    tempParam.Remove(temp);
                                                    tempSql.Append(DbCache.Get(cacheType, paramKey));
                                                }
                                                else
                                                    tempSql.Append(DbCache.Get(cacheType, paramKey));
                                            }
                                            else
                                                tempParam.Remove(temp);
                                            break;
                                        }
                                    case "choose":
                                        {
                                            var isSuccess = false;
                                            for (int j = 0; j < DbCache.Get(cacheType, paramKey).ToStr().ToInt(0); j++)
                                            {
                                                conditionKey = string.Format("{0}.choose.{1}", paramKey, j);
                                                condition = DbCache.Get(cacheType, conditionKey).ToStr().ToLower();

                                                conditionValueKey = string.Format("{0}.choose.condition.{1}", paramKey, j);
                                                conditionValue = DbCache.Get(cacheType, conditionValueKey).ToStr().ToLower();
                                                //conditionValue = conditionValue.Replace(temp.ParameterName.ToLower(), temp.Value.ToStr());
                                                conditionValue = conditionValue.Replace(temp.ParameterName, temp.Value == null ? null : temp.Value.ToStr());
                                                conditionValue = conditionValue.Replace("#", "\"");
                                                if (CSharpScript.EvaluateAsync<bool>(conditionValue).Result)
                                                {
                                                    isSuccess = true;
                                                    if (condition.IndexOf(tempKey) >= 0)
                                                    {
                                                        tempParam.Remove(temp);
                                                        tempSql.Append(condition.Replace(tempKey, temp.Value.ToString()));
                                                    }
                                                    else if (condition.IndexOf(flagParam) < 0 && flag != "")
                                                    {
                                                        tempParam.Remove(temp);
                                                        tempSql.Append(condition.Replace(tempKey, temp.Value.ToString()));
                                                    }
                                                    else
                                                        tempSql.Append(condition);
                                                    break;
                                                }
                                            }

                                            if (!isSuccess)
                                                tempParam.Remove(temp);

                                            break;
                                        }
                                    default:
                                        {
                                            //isPropertyAvailable
                                            if (paramSql.IndexOf(tempKey) >= 0)
                                            {
                                                tempParam.Remove(temp);
                                                tempSql.Append(paramSql.ToString().Replace(tempKey, temp.Value.ToString()));
                                            }
                                            else if (paramSql.IndexOf(flagParam) < 0 && flag != "")
                                            {
                                                tempParam.Remove(temp);
                                                tempSql.Append(DbCache.Get(cacheType, paramKey));
                                            }
                                            else
                                                tempSql.Append(DbCache.Get(cacheType, paramKey));

                                            break;
                                        }
                                }
                            }
                        }

                        if (tempSql.ToString() != "")
                        {
                            sql.Append(DbCache.Get(cacheType, dynKey));
                            sql.Append(tempSql.ToString());
                        }
                    }
                }
                #endregion
            }

            param = tempParam.ToArray();
            return sql.ToString();
        }
        #endregion

        #region map xml 存数据库
        /// <summary>
        /// map xml 存数据库
        /// </summary>
        /// <param name="dbKey"></param>
        /// <param name="key"></param>
        /// <param name="info"></param>
        private static bool SaveXml(string dbKey, string key, FileInfo info, ConfigModel config, DataContext db)
        {
            if (config.IsMapSave)
            {
                //加密
                var enContent = File.ReadAllText(info.FullName);

                //明文
                var deContent = "";

                if (config.IsEncrypt)
                {
                    deContent = BaseSymmetric.DecodeGB2312(deContent);
                    if (deContent == "")
                        deContent = enContent;
                }
                else
                    deContent = enContent;

                if (config.DbType == DataDbType.MySql)
                {
                    var model = new DataModel.MySql.Data_MapFile();
                    model.MapId = key;
                    var query = FastRead.Query<DataModel.MySql.Data_MapFile>(a => a.MapId == key, null, dbKey);

                    if (query.ToCount() == 0)
                    {
                        model.FileName = info.Name;
                        model.FilePath = info.FullName;
                        model.LastTime = info.LastWriteTime;
                        model.EnFileContent = enContent;
                        model.DeFileContent = deContent;
                       return db.Add(model).writeReturn.IsSuccess;
                    }
                    else
                       return db.Update<DataModel.MySql.Data_MapFile>(model, a => a.MapId == model.MapId, a => new { a.LastTime, a.EnFileContent, a.DeFileContent }).writeReturn.IsSuccess;
                }

                if (config.DbType == DataDbType.Oracle)
                {
                    var model = new DataModel.Oracle.Data_MapFile();
                    model.MapId = key;
                    var query = FastRead.Query<DataModel.Oracle.Data_MapFile>(a => a.MapId == key, null, dbKey);

                    if (query.ToCount() == 0)
                    {
                        model.FileName = info.Name;
                        model.FilePath = info.FullName;
                        model.LastTime = info.LastWriteTime;
                        model.EnFileContent = enContent;
                        model.DeFileContent = deContent;
                        return db.Add(model).writeReturn.IsSuccess;
                    }
                    else
                       return db.Update<DataModel.Oracle.Data_MapFile>(model, a => a.MapId == model.MapId, a => new { a.LastTime, a.EnFileContent, a.DeFileContent }).writeReturn.IsSuccess;
                }

                if (config.DbType == DataDbType.SqlServer)
                {
                    var model = new DataModel.SqlServer.Data_MapFile();
                    model.MapId = key;
                    var query = FastRead.Query<DataModel.SqlServer.Data_MapFile>(a => a.MapId == key, null, dbKey);

                    if (query.ToCount() == 0)
                    {
                        model.FileName = info.Name;
                        model.FilePath = info.FullName;
                        model.LastTime = info.LastWriteTime;
                        model.EnFileContent = enContent;
                        model.DeFileContent = deContent;
                        return db.Add(model).writeReturn.IsSuccess;
                    }
                    else
                        return db.Update<DataModel.SqlServer.Data_MapFile>(model, a => a.MapId == model.MapId, a => new { a.LastTime, a.EnFileContent, a.DeFileContent }).writeReturn.IsSuccess;
                }
            }

            return true;
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
            return DbCache.Get(DataConfig.Get().CacheType, string.Format("{0}.db", name.ToLower()));
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
            return DbCache.Get(DataConfig.Get().CacheType, string.Format("{0}.type", name.ToLower()));
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
        public static List<string> Api
        {
            get
            {
                return DbCache.Get<List<string>>(DataConfig.Get().CacheType, "FastMap.Api");
            }
        }
        #endregion

        #region 获取map验证必填
        /// <summary>
        /// 获取map验证必填
        /// </summary>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static string MapRequired(string name,string param)
        {
            return DbCache.Get(DataConfig.Get().CacheType, string.Format("{0}.{1}.required", name.ToLower(), param.ToLower()));
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
            return DbCache.Get(DataConfig.Get().CacheType, string.Format("{0}.{1}.maxlength", name.ToLower(), param.ToLower()));
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
            return DbCache.Get(DataConfig.Get().CacheType, string.Format("{0}.{1}.date", name.ToLower(), param.ToLower()));
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
            return DbCache.Get(DataConfig.Get().CacheType, string.Format("{0}.{1}.checkmap", name.ToLower(), param.ToLower()));
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
            return DbCache.Get(DataConfig.Get().CacheType, string.Format("{0}.{1}.existsmap", name.ToLower(), param.ToLower()));
        }
        #endregion

        #region 是否foreach
        /// <summary>
        /// 是否foreach
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static bool MapIsForEach(string name, ConfigModel config, int i = 1)
        {
            var keyName = string.Format("{0}.foreach.name.{1}", name.ToLower(), i);
            var keyField = string.Format("{0}.foreach.field.{1}", name.ToLower(), i);
            var keySql = string.Format("{0}.foreach.sql.{1}", name.ToLower(), i);

            return DbCache.Get(config.CacheType, keyName) != "" &&
                DbCache.Get(config.CacheType, keyField) != "" &&
                DbCache.Get(config.CacheType, keySql) != "";
        }
        #endregion

        #region froeach数量
        /// <summary>
        /// froeach数量
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        private static int MapForEachCount(string name,ConfigModel config)
        {
            return DbCache.Get(config.CacheType, string.Format("{0}.foreach", name.ToLower())).ToInt(1);
        }
        #endregion

        #region foreach数据
        /// <summary>
        /// foreach数据
        /// </summary>
        /// <param name="name"></param>
        /// <param name="db"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static List<Dictionary<string, object>> MapForEach(List<Dictionary<string, object>> data, string name, DataContext db, string key, ConfigModel config,int i=1)
        {
            var result = new List<Dictionary<string, object>>();
            var param = new List<DbParameter>();
            var dicName = DbCache.Get(config.CacheType, string.Format("{0}.foreach.name.{1}", name.ToLower(),i));
            var field = DbCache.Get(config.CacheType, string.Format("{0}.foreach.field.{1}", name.ToLower(),i));
            var sql = DbCache.Get(config.CacheType, string.Format("{0}.foreach.sql.{1}", name.ToLower(),i));
            
            foreach (var item in data)
            {
                param.Clear();
                if (field.IndexOf(',') > 0)
                {
                    foreach (var split in field.Split(','))
                    {
                        var tempParam = DbProviderFactories.GetFactory(config).CreateParameter();
                        tempParam.ParameterName = split;
                        tempParam.Value = item.GetValue(split);
                        param.Add(tempParam);
                    }
                }
                else
                {
                    var tempParam = DbProviderFactories.GetFactory(config).CreateParameter();
                    tempParam.ParameterName = field;
                    tempParam.Value = item.GetValue(field);
                    param.Add(tempParam);
                }

                item.Add(dicName, FastRead.ExecuteSql(sql, param.ToArray(), db, key));
                result.Add(item);
            }

            return result;
        }
        #endregion

        #region  foreach数据
        /// <summary>
        /// foreach数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="name"></param>
        /// <param name="db"></param>
        /// <param name="key"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        private static List<T> MapForEach<T>(List<T> data, string name, DataContext db, string key, ConfigModel config,int i=1) where T : class, new()
        {
            var result = new List<T>();
            var param = new List<DbParameter>();
            var dicName = DbCache.Get(config.CacheType, string.Format("{0}.foreach.name.{1}", name.ToLower(),i));
            var type = DbCache.Get(config.CacheType, string.Format("{0}.foreach.type.{1}", name.ToLower(),i));
            var field = DbCache.Get(config.CacheType, string.Format("{0}.foreach.field.{1}", name.ToLower(),i));
            var sql = DbCache.Get(config.CacheType, string.Format("{0}.foreach.sql.{1}", name.ToLower(),i));
            Assembly assembly;

            if (type.IndexOf(',') > 0)
            {
                assembly = Assembly.Load(type.Split(',')[1]);
                if (assembly == null)
                    return data;
                else
                {
                    if (assembly.GetType(type.Split(',')[0]) == null)
                        return data;
                    
                    foreach (var item in data)
                    {
                        var model = Activator.CreateInstance(assembly.GetType(type.Split(',')[0]));
                        var list = Activator.CreateInstance(typeof(List<>).MakeGenericType(assembly.GetType(type.Split(',')[0])));
                        var infoResult = BaseDic.PropertyInfo<T>().Find(a => a.PropertyType.FullName == list.GetType().FullName);
                        
                        //param
                        param.Clear();
                        if (field.IndexOf(',') > 0)
                        {
                            foreach (var split in field.Split(','))
                            {
                                var infoField = BaseDic.PropertyInfo<T>().Find(a => a.Name.ToLower() == split);
                                var tempParam = DbProviderFactories.GetFactory(config).CreateParameter();
                                tempParam.ParameterName = split;
                                tempParam.Value = infoField.GetValue(item,null);
                                param.Add(tempParam);
                            }
                        }
                        else
                        {
                            var infoField = BaseDic.PropertyInfo<T>().Find(a => a.Name.ToLower() == field);
                            var tempParam = DbProviderFactories.GetFactory(config).CreateParameter();
                            tempParam.ParameterName = field;
                            tempParam.Value = infoField.GetValue(item, null);
                            param.Add(tempParam);
                        }

                        var tempData = db.ExecuteSql(sql, param.ToArray(), true);

                        foreach (var temp in tempData.DicList)
                        {
                            foreach (var info in model.GetType().GetProperties())
                            {
                                if (temp.GetValue(info.Name).ToStr() == "" && info.PropertyType.Name == "Nullable`1")
                                    continue;
                                    
                                if (info.PropertyType.Name == "Nullable`1" && info.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                    info.SetValue(model, Convert.ChangeType(temp.GetValue(info.Name), Nullable.GetUnderlyingType(info.PropertyType)), null);
                                else
                                    info.SetValue(model, Convert.ChangeType(temp.GetValue(info.Name), info.PropertyType), null);                                
                            }

                            var method = list.GetType().GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
                            method.Invoke(list, new object[] { model });
                        }

                        infoResult.SetValue(item, list);
                        result.Add(item);
                    }
                    return result;
                }
            }
            else
                return data;           
        }
        #endregion
    }
}
