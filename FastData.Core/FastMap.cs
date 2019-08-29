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
                return FastRead.ExecuteSql<T>(sql, param, db, key);
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

                return FastRead.ExecuteSql(sql, param, db, key);
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

            if (DbCache.Exists(config.CacheType,name.ToLower()))
            {
                var sql = GetMapSql(name, ref param,db,key);

                return ExecuteSqlPage(pModel, sql, param, db, key);
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

                return ExecuteSqlPage<T>(pModel, sql, param, db, key);
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
            var key = new List<string>();
            var sql = new List<string>();
            var db = new List<string>(); 
            var param = new Dictionary<string,object>();
            GetXmlList(path, "sqlMap", ref key, ref sql,ref db,ref param, config);
            
            for (var i = 0; i < key.Count; i++)
            {
                DbCache.Set(config.CacheType, key[i].ToLower(), sql[i]);
                DbCache.Set(config.CacheType, string.Format("{0}.db", key[i]), db[i]);
                DbCache.Set<List<string>>(config.CacheType, string.Format("{0}.param", key[i].ToLower()), param.GetValue(key[i].ToLower()) as List<string>);
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
        private static void GetXmlList(string path, string xmlNode, ref List<string> key, ref List<string> sql,ref List<string> db,ref Dictionary<string, object> param, ConfigModel config)
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
                            db.Add(temp.Attributes["db"].Value.ToStr().ToLower());

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
                                    key.Add(string.Format("{0}.format.{1}", tempKey, i));
                                    sql.Add(node.Attributes["prepend"].Value.ToLower());

                                    foreach (XmlNode dyn in node.ChildNodes)
                                    {
                                        //参数
                                        tempParam.Add(dyn.Attributes["property"].Value.ToLower());

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
                        foreach (var temp in param)
                        {
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
    }
}
