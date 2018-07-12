using Data.Core.Base;
using Data.Core.CacheModel;
using Data.Core.Check;
using Data.Core.Context;
using Data.Core.Model;
using Data.Core.Type;
using Redis.Core;
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
using Untility.Core.Base;
using Untility.Core.Page;

namespace Data.Core
{
    /// <summary>
    /// map
    /// </summary>
    public static class LambdaMap
    {
        #region 初始化model成员 1
        /// <summary>
        /// 初始化model成员 1
        /// </summary>
        /// <param name="list"></param>
        /// <param name="nameSpace">命名空间</param>
        /// <param name="dll">dll名称</param>
        public static void InstanceProperties(Assembly[] list, string nameSpace, string dll)
        {
            foreach (var item in list)
            {
                if (item.ManifestModule.Name == dll)
                {
                    foreach (var temp in item.ExportedTypes)
                    {
                        Task.Factory.StartNew(() =>
                        {
                            var typeInfo = (temp as TypeInfo);
                            if (typeInfo.Namespace.Contains(nameSpace))
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

                                RedisInfo.SetItem<List<PropertyModel>>(key, cacheList, 8640, RedisDb.Properties);
                            }
                        });
                    }
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
        public static void InstanceTable(Assembly[] list, string nameSpace, string dll, string dbKey = null)
        {
            var query = new DataQuery();
            query.Config = DataConfig.Get(dbKey);
            query.Key = dbKey;

            foreach (var item in list)
            {
                if (item.ManifestModule.Name == dll)
                {
                    foreach (var temp in item.ExportedTypes)
                    {
                        var typeInfo = (temp as TypeInfo);
                        if (typeInfo.Namespace.Contains(nameSpace))
                            BaseTable.Check(query, temp.Name, temp.Namespace, typeInfo.DeclaredProperties.ToList(), typeInfo.GetCustomAttributes().ToList());
                    }
                }
            }

            if (query.Config.DbType == DataDbType.Oracle)
            {
                var listInfo = typeof(Data.Core.DataModel.Oracle.Data_MapFile).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList();
                var listAttribute = typeof(Data.Core.DataModel.Oracle.Data_MapFile).GetTypeInfo().GetCustomAttributes().ToList();
                BaseTable.Check(query, "Data_MapFile", "Data.Core.DataModel.Oracle", listInfo, listAttribute);
            }

            if (query.Config.DbType == DataDbType.MySql)
            {
                var listInfo = typeof(Data.Core.DataModel.MySql.Data_MapFile).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList();
                var listAttribute = typeof(Data.Core.DataModel.MySql.Data_MapFile).GetTypeInfo().GetCustomAttributes().ToList();
                BaseTable.Check(query, "Data_MapFile", "Data.Core.DataModel.MySql", listInfo, listAttribute);
            }

            if (query.Config.DbType == DataDbType.SqlServer)
            {
                var listInfo = typeof(Data.Core.DataModel.SqlServer.Data_MapFile).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList();
                var listAttribute = typeof(Data.Core.DataModel.SqlServer.Data_MapFile).GetTypeInfo().GetCustomAttributes().ToList();
                BaseTable.Check(query, "Data_MapFile", "Data.Core.DataModel.SqlServer", listInfo, listAttribute);
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
            var list = BaseConfig.GetValue<MapConfigModel>(AppSettingKey.Map);
            var config = DataConfig.Get(dbKey);
            var db = new DataContext(dbKey, config);

            foreach (var item in list.Path)
            {
                var info = new FileInfo(item);
                var key = BaseSymmetric.Generate(info.FullName);

                if (!RedisInfo.Exists(key, RedisDb.Xml))
                {
                    var temp = new MapXmlModel();
                    temp.LastWrite = info.LastWriteTime;
                    temp.FileKey = ReadXml(info.FullName, config);
                    temp.FileName = info.FullName;
                    if (SaveXml(dbKey, key, info, config, db))
                        RedisInfo.SetItem<MapXmlModel>(key, temp, 8640, RedisDb.Xml);
                }
                else if ((RedisInfo.GetItem<MapXmlModel>(key, RedisDb.Xml).LastWrite - info.LastWriteTime).Minutes != 0)
                {
                    foreach (var temp in RedisInfo.GetItem<MapXmlModel>(key, RedisDb.Xml).FileKey)
                        RedisInfo.RemoveItem(temp, RedisDb.Xml);

                    var model = new MapXmlModel();
                    model.LastWrite = info.LastWriteTime;
                    model.FileKey = ReadXml(info.FullName, config);
                    model.FileName = info.FullName;
                    if (SaveXml(dbKey, key, info, config, db))
                        RedisInfo.SetItem<MapXmlModel>(key, model, 8640, RedisDb.Xml);
                }
            }

            db.Dispose();
        }
        #endregion

        #region maq 执行返回结果
        /// <summary>
        /// maq 执行返回结果
        /// </summary>
        public static List<T> ExecuteMap<T>(string name, DbParameter[] param, DataContext db = null, string key = null) where T : class, new()
        {
            InstanceMap(key);

            if (RedisInfo.Exists(name.ToLower(), RedisDb.Xml))
            {
                var sql = GetMapSql(name, ref param);
                return LambdaRead.ExecuteSql<T>(sql, param, db, key);
            }
            else
                return new List<T>();
        }
        #endregion

        #region maq 执行返回结果 asy
        /// <summary>
        /// 执行sql asy
        /// </summary>
        public static async Task<List<T>> ExecuteMapAsy<T>(string name, DbParameter[] param, DataContext db = null, string key = null) where T : class, new()
        {
            return await Task.Factory.StartNew(() =>
            {
                return ExecuteMap<T>(name, param, db, key);
            });
        }
        #endregion

        #region maq 执行返回结果 lazy
        /// <summary>
        /// maq 执行返回结果 lazy
        /// </summary>
        public static Lazy<List<T>> ExecuteLazyMap<T>(string name, DbParameter[] param, DataContext db = null, string key = null) where T : class, new()
        {
            return new Lazy<List<T>>(() => ExecuteMap<T>(name, param, db, key));
        }
        #endregion

        #region maq 执行返回结果 lazy asy
        /// <summary>
        /// maq 执行返回结果 lazy asy
        /// </summary>
        public static async Task<Lazy<List<T>>> ExecuteLazyMapAsy<T>(string name, DbParameter[] param, DataContext db = null, string key = null) where T : class, new()
        {
            return await Task.Factory.StartNew(() =>
            {
                return new Lazy<List<T>>(() => ExecuteMap<T>(name, param, db, key));
            });
        }
        #endregion


        #region maq 执行返回 List<Dictionary<string, object>>
        /// <summary>
        /// maq 执行返回 List<Dictionary<string, object>>
        /// </summary>
        public static List<Dictionary<string, object>> ExecuteMap(string name, DbParameter[] param, DataContext db = null, string key = null)
        {
            InstanceMap(key);

            if (RedisInfo.Exists(name.ToLower(), RedisDb.Xml))
            {
                var sql = GetMapSql(name, ref param);

                return LambdaRead.ExecuteSql(sql, param, db, key);
            }
            else
                return new List<Dictionary<string, object>>();
        }
        #endregion

        #region maq 执行返回 List<Dictionary<string, object>> asy
        /// <summary>
        /// 执行sql List<Dictionary<string, object>> asy
        /// </summary>
        public static async Task<List<Dictionary<string, object>>> ExecuteMapAsy(string name, DbParameter[] param, DataContext db = null, string key = null)
        {
            return await Task.Factory.StartNew(() =>
            {
                return ExecuteMap(name, param, db, key);
            });
        }
        #endregion

        #region maq 执行返回 List<Dictionary<string, object>> lazy
        /// <summary>
        /// maq 执行返回 List<Dictionary<string, object>> lazy
        /// </summary>
        public static Lazy<List<Dictionary<string, object>>> ExecuteLazyMap(string name, DbParameter[] param, DataContext db = null, string key = null)
        {
            return new Lazy<List<Dictionary<string, object>>>(() => ExecuteMap(name, param, db, key));
        }
        #endregion

        #region maq 执行返回 List<Dictionary<string, object>> lazy asy
        /// <summary>
        /// maq 执行返回 List<Dictionary<string, object>> lazy asy
        /// </summary>
        public static async Task<Lazy<List<Dictionary<string, object>>>> ExecuteLazyMapAsy(string name, DbParameter[] param, DataContext db = null, string key = null)
        {
            return await Task.Factory.StartNew(() =>
            {
                return new Lazy<List<Dictionary<string, object>>>(() => ExecuteMap(name, param, db, key));
            });
        }
        #endregion


        #region maq 执行写操作
        /// <summary>
        /// 执行写操作
        /// </summary>
        public static WriteReturn ExecuteWriteMap(string name, DbParameter[] param, DataContext db = null, string key = null)
        {
            InstanceMap(key);

            if (RedisInfo.Exists(name.ToLower(), RedisDb.Xml))
            {
                var sql = GetMapSql(name, ref param);

                return LambdaWrite.ExecuteSql(sql, param, db, key);
            }
            else
                return new WriteReturn();
        }
        #endregion

        #region maq 执行写操作 asy
        /// <summary>
        ///  maq 执行写操作 asy
        /// </summary>
        public static async Task<WriteReturn> ExecuteWriteMapAsy(string name, DbParameter[] param, DataContext db = null, string key = null)
        {
            return await Task.Factory.StartNew(() =>
            {
                return ExecuteWriteMap(name, param, db, key);
            });
        }
        #endregion

        #region maq 执行写操作 asy lazy
        /// <summary>
        /// maq 执行写操作 asy lazy
        /// </summary>
        public static Lazy<WriteReturn> ExecuteLazyWriteMap(string name, DbParameter[] param, DataContext db = null, string key = null)
        {
            return new Lazy<WriteReturn>(() => ExecuteWriteMap(name, param, db, key));
        }
        #endregion

        #region maq 执行写操作 asy lazy asy
        /// <summary>
        /// maq 执行写操作 asy lazy asy
        /// </summary>
        public static async Task<Lazy<WriteReturn>> ExecuteLazyWriteMapAsy(string name, DbParameter[] param, DataContext db = null, string key = null)
        {
            return await Task.Factory.StartNew(() =>
            {
                return new Lazy<WriteReturn>(() => ExecuteWriteMap(name, param, db, key));
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

            Task.Factory.StartNew(() => { DbLog.LogSql(config.IsOutSql, result.Sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds); });

            return result.PageResult;
        }
        #endregion

        #region maq 执行分页
        /// <summary>
        /// maq 执行分页
        /// </summary>
        public static PageResult ExecuteMapPage(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null)
        {
            InstanceMap(key);

            if (RedisInfo.Exists(name.ToLower(), RedisDb.Xml))
            {
                var sql = GetMapSql(name, ref param);

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
        public static async Task<PageResult> ExecuteMapPageAsy(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null)
        {
            return await Task.Factory.StartNew(() =>
            {
                return ExecuteMapPage(pModel, name, param, db, key);
            });
        }
        #endregion

        #region maq 执行分页 lazy
        /// <summary>
        /// maq 执行分页 lazy
        /// </summary>
        public static Lazy<PageResult> ExecuteLazyMapPage(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null)
        {
            return new Lazy<PageResult>(() => ExecuteMapPage(pModel, name, param, db, key));
        }
        #endregion

        #region maq 执行分页 lazy asy
        /// <summary>
        /// maq 执行分页lazy asy
        /// </summary>
        public static async Task<Lazy<PageResult>> ExecuteLazyMapPageAsy(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null)
        {
            return await Task.Factory.StartNew(() =>
            {
                return new Lazy<PageResult>(() => ExecuteMapPage(pModel, name, param, db, key));
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

            Task.Factory.StartNew(() => { DbLog.LogSql(config.IsOutSql, result.sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds); });

            return result.pageResult;
        }
        #endregion

        #region maq 执行分页
        /// <summary>
        /// maq 执行分页
        /// </summary>
        public static PageResult<T> ExecuteMapPage<T>(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null) where T : class, new()
        {
            InstanceMap(key);

            if (RedisInfo.Exists(name.ToLower(), RedisDb.Xml))
            {
                var sql = GetMapSql(name, ref param);

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
        public static async Task<PageResult<T>> ExecuteMapPageAsy<T>(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null) where T : class, new()
        {
            return await Task.Factory.StartNew(() =>
            {
                return ExecuteMapPage<T>(pModel, name, param, db, key);
            });
        }
        #endregion

        #region maq 执行分页 lazy
        /// <summary>
        /// maq 执行分页 lazy
        /// </summary>
        public static Lazy<PageResult<T>> ExecuteLazyMapPage<T>(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null) where T : class, new()
        {
            return new Lazy<PageResult<T>>(() => ExecuteMapPage<T>(pModel, name, param, db, key));
        }
        #endregion

        #region maq 执行分页 lazy asy
        /// <summary>
        /// maq 执行分页lazy asy
        /// </summary>
        public static async Task<Lazy<PageResult<T>>> ExecuteLazyMapPageAsy<T>(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null) where T : class, new()
        {
            return await Task.Factory.StartNew(() =>
            {
                return new Lazy<PageResult<T>>(() => ExecuteMapPage<T>(pModel, name, param, db, key));
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
            GetXmlList(path, "sqlMap", ref key, ref sql, config);

            for (var i = 0; i < key.Count; i++)
                RedisInfo.SetItem(key[i].ToLower(), sql[i], 8640, RedisDb.Xml);

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
        private static void GetXmlList(string path, string xmlNode, ref List<string> key, ref List<string> sql, ConfigModel config)
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
                            #region XmlElement
                            tempKey = temp.Attributes["id"].Value.ToLower();

                            //节点数
                            if (Array.Exists(key.ToArray(), element => element == tempKey))
                                Task.Factory.StartNew(() => { BaseLog.SaveLog(string.Format("xml文件:{0},存在相同键:{1}", path, tempKey), "MapKeyExists"); });

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
                                    key.Add(string.Format("{0}.format.{1}", tempKey, i));
                                    sql.Add(node.Attributes["prepend"].Value.ToLower());

                                    foreach (XmlNode dyn in node.ChildNodes)
                                    {
                                        if (dyn.Name != "isPropertyAvailable")
                                        {
                                            //条件类型
                                            key.Add(string.Format("{0}.{1}.condition.{2}", tempKey, dyn.Attributes["property"].Value.ToLower(), i));
                                            sql.Add(dyn.Name);

                                            //比较条件值
                                            if (dyn.Attributes["compareValue"] != null)
                                            {
                                                key.Add(string.Format("{0}.{1}.condition.value.{2}", tempKey, dyn.Attributes["property"].Value.ToLower(), i));
                                                sql.Add(dyn.Attributes["compareValue"].Value.ToLower());
                                            }

                                            //判断条件内容
                                            if (dyn.Attributes["condition"] != null)
                                            {
                                                key.Add(string.Format("{0}.{1}.condition.value.{2}", tempKey, dyn.Attributes["property"].Value.ToLower(), i));
                                                sql.Add(dyn.Attributes["condition"].Value.ToLower());
                                            }
                                        }

                                        //属性和值
                                        key.Add(string.Format("{0}.{1}.{2}", tempKey, dyn.Attributes["property"].Value.ToLower(), i));
                                        sql.Add(string.Format("{0}{1}", dyn.Attributes["prepend"].Value.ToLower(), dyn.InnerText));
                                    }
                                }
                                #endregion

                                i++;
                            }
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
                Task.Factory.StartNew(() =>
                {
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
        private static string GetMapSql(string name, ref DbParameter[] param)
        {
            var tempParam = param.ToList();
            var sql = new StringBuilder();

            for (var i = 0; i <= RedisInfo.GetItem(name.ToLower(), RedisDb.Xml).ToInt(0); i++)
            {
                #region 文本
                var txtKey = string.Format("{0}.{1}", name.ToLower(), i);
                if (RedisInfo.Exists(txtKey, RedisDb.Xml))
                    sql.Append(RedisInfo.GetItem(txtKey, RedisDb.Xml));
                #endregion

                #region 动态
                var dynKey = string.Format("{0}.format.{1}", name.ToLower(), i);
                if (RedisInfo.Exists(dynKey, RedisDb.Xml))
                {
                    if (param != null)
                    {
                        var tempSql = new StringBuilder();
                        foreach (var temp in param)
                        {
                            var paramKey = string.Format("{0}.{1}.{2}", name.ToLower(), temp.ParameterName.ToLower(), i);
                            var conditionKey = string.Format("{0}.{1}.condition.{2}", name.ToLower(), temp.ParameterName.ToLower(), i);
                            var conditionValueKey = string.Format("{0}.{1}.condition.value.{2}", name.ToLower(), temp.ParameterName.ToLower(), i);
                            if (RedisInfo.Exists(paramKey, RedisDb.Xml))
                            {
                                var tempKey = string.Format("#{0}#", temp.ParameterName.ToLower());
                                var paramSql = RedisInfo.GetItem(paramKey, RedisDb.Xml).ToLower();
                                var condition = RedisInfo.GetItem(conditionKey).ToStr().ToLower();
                                var conditionValue = RedisInfo.GetItem(conditionValueKey).ToStr().ToLower();
                                switch (condition)
                                {
                                    case "isEqual":
                                        {
                                            if (conditionValue == temp.Value.ToStr())
                                            {
                                                if (paramSql.IndexOf(tempKey) >= 0)
                                                {
                                                    tempParam.Remove(temp);
                                                    tempSql.Append(paramSql.ToString().Replace(tempKey, temp.Value.ToString()));
                                                }
                                                else
                                                    tempSql.Append(RedisInfo.GetItem(paramKey, RedisDb.Xml));
                                            }
                                            else
                                                tempParam.Remove(temp);
                                            break;
                                        }
                                    case "isNotEqual":
                                        {
                                            if (conditionValue != temp.Value.ToStr())
                                            {
                                                if (paramSql.IndexOf(tempKey) >= 0)
                                                {
                                                    tempParam.Remove(temp);
                                                    tempSql.Append(paramSql.ToString().Replace(temp.ParameterName.ToLower(), temp.Value.ToString()));
                                                }
                                                else
                                                    tempSql.Append(RedisInfo.GetItem(paramKey, RedisDb.Xml));
                                            }
                                            else
                                                tempParam.Remove(temp);
                                            break;
                                        }
                                    case "isGreaterThan":
                                        {
                                            if (temp.Value.ToStr().ToDecimal(0) > conditionValue.ToDecimal(0))
                                            {
                                                if (paramSql.IndexOf(tempKey) >= 0)
                                                {
                                                    tempParam.Remove(temp);
                                                    tempSql.Append(paramSql.ToString().Replace(tempKey, temp.Value.ToString()));
                                                }
                                                else
                                                    tempSql.Append(RedisInfo.GetItem(paramKey, RedisDb.Xml));
                                            }
                                            else
                                                tempParam.Remove(temp);
                                            break;
                                        }
                                    case "isLessThan":
                                        {
                                            if (temp.Value.ToStr().ToDecimal(0) < conditionValue.ToDecimal(0))
                                            {
                                                if (paramSql.IndexOf(tempKey) >= 0)
                                                {
                                                    tempParam.Remove(temp);
                                                    tempSql.Append(paramSql.ToString().Replace(tempKey, temp.Value.ToString()));
                                                }
                                                else
                                                    tempSql.Append(RedisInfo.GetItem(paramKey, RedisDb.Xml));
                                            }
                                            else
                                                tempParam.Remove(temp);
                                            break;
                                        }
                                    case "isNullOrEmpty":
                                        {
                                            if (string.IsNullOrEmpty(temp.Value.ToStr()))
                                            {
                                                if (paramSql.IndexOf(tempKey) >= 0)
                                                {
                                                    tempParam.Remove(temp);
                                                    tempSql.Append(paramSql.ToString().Replace(tempKey, temp.Value.ToString()));
                                                }
                                                else
                                                    tempSql.Append(RedisInfo.GetItem(paramKey, RedisDb.Xml));
                                            }
                                            else
                                                tempParam.Remove(temp);
                                            break;
                                        }
                                    case "isNotNullOrEmpty":
                                        {
                                            if (!string.IsNullOrEmpty(temp.Value.ToStr()))
                                            {
                                                if (paramSql.IndexOf(tempKey) >= 0)
                                                {
                                                    tempParam.Remove(temp);
                                                    tempSql.Append(paramSql.ToString().Replace(tempKey, temp.Value.ToString()));
                                                }
                                                else
                                                    tempSql.Append(RedisInfo.GetItem(paramKey, RedisDb.Xml));
                                            }
                                            else
                                                tempParam.Remove(temp);
                                            break;
                                        }
                                    case "if":
                                        {
                                            conditionValue = conditionValue.Replace(temp.ParameterName.ToLower(), temp.Value.ToStr());
                                            if (BaseCodeDom.GetResult(conditionValue))
                                            {
                                                if (paramSql.IndexOf(tempKey) >= 0)
                                                {
                                                    tempParam.Remove(temp);
                                                    tempSql.Append(paramSql.ToString().Replace(tempKey, temp.Value.ToString()));
                                                }
                                                else
                                                    tempSql.Append(RedisInfo.GetItem(paramKey, RedisDb.Xml));
                                            }
                                            else
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
                                            else
                                                tempSql.Append(RedisInfo.GetItem(paramKey, RedisDb.Xml));

                                            break;
                                        }
                                }
                            }
                        }

                        if (tempSql.ToString() != "")
                        {
                            sql.Append(RedisInfo.GetItem(dynKey, RedisDb.Xml));
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
                    var query = LambdaRead.Query<DataModel.MySql.Data_MapFile>(a => a.MapId == key, null, dbKey);

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
                    var query = LambdaRead.Query<DataModel.Oracle.Data_MapFile>(a => a.MapId == key, null, dbKey);

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
                    var query = LambdaRead.Query<DataModel.SqlServer.Data_MapFile>(a => a.MapId == key, null, dbKey);

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
    }
}
