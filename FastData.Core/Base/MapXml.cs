using FastData.Core.Aop;
using FastData.Core.Check;
using FastData.Core.Context;
using FastData.Core.DataModel.Oracle;
using FastData.Core.Model;
using FastData.Core.Type;
using FastUntility.Core;
using FastUntility.Core.Base;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FastData.Core.Base
{
    public static class MapXml
    {
        #region froeach数量
        /// <summary>
        /// froeach数量
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        internal static int MapForEachCount(string name, ConfigModel config)
        {
            return DbCache.Get(config.CacheType, string.Format("{0}.foreach", name.ToLower())).ToInt(1);
        }
        #endregion

        #region 获取map sql语句
        /// <summary>
        /// 获取map sql语句
        /// </summary>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static string GetMapSql(string name, ref DbParameter[] param, DataContext db, string key)
        {
            var tempParam = new List<DbParameter>();
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

            for (var i = 0; i <= DbCache.Get(cacheType, name.ToLower()).ToInt(0); i++)
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
                        foreach (var item in DbCache.Get<List<string>>(cacheType, string.Format("{0}.param", name.ToLower())))
                        {
                            if (!param.ToList().Exists(a => string.Compare(a.ParameterName, item, true) == 0))
                                continue;
                            var temp = param.ToList().Find(a => string.Compare(a.ParameterName, item, true) == 0);
                            if (!tempParam.ToList().Exists(a => a.ParameterName == temp.ParameterName))
                                tempParam.Add(temp);

                            var sqlModel = new SqlModel() { CacheType = cacheType, I = i, MapName = name, Param = temp, Flag = flag };

                            if (DbCache.Exists(cacheType, sqlModel.ParamKey))
                            {
                                var condition = DbCache.Get(cacheType, sqlModel.ConditionKey).ToStr().ToLower();
                                switch (condition)
                                {
                                    case "isequal":
                                        {
                                            XmlOption.IsEqualSql(sqlModel, tempParam);
                                            break;
                                        }
                                    case "isnotequal":
                                        {
                                            XmlOption.IsNotEqualSql(sqlModel, tempParam);
                                            break;
                                        }
                                    case "isgreaterthan":
                                        {
                                            XmlOption.IsGreaterThanSql(sqlModel, tempParam);
                                            break;
                                        }
                                    case "islessthan":
                                        {
                                            XmlOption.IsLessThanSql(sqlModel, tempParam);
                                            break;
                                        }
                                    case "isnullorempty":
                                        {
                                            XmlOption.IsNullOrEmptySql(sqlModel, tempParam);
                                            break;
                                        }
                                    case "isnotnullorempty":
                                        {
                                            XmlOption.IsNotNullOrEmptySql(sqlModel, tempParam);
                                            break;
                                        }
                                    case "if":
                                        {
                                            XmlOption.IfSql(sqlModel, tempParam);
                                            break;
                                        }
                                    case "choose":
                                        {
                                            XmlOption.ChooseSql(sqlModel, tempParam);
                                            break;
                                        }
                                    default:
                                        {
                                            if (DbCache.Get(cacheType, sqlModel.IncludeKey).ToStr().ToLower() == "include")
                                                GetMapSql(sqlModel.IncludeRefIdKey, ref param, db, key);
                                            else
                                                XmlOption.IsPropertyAvailableSql(sqlModel, tempParam);
                                            break;
                                        }
                                }
                            }
                            tempSql.Append(sqlModel.Sql);
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

            if (DbCache.Get<List<string>>(cacheType, string.Format("{0}.param", name.ToLower())).Count > 0)
                param = tempParam.ToArray();
            else
            {
                foreach (var item in param)
                {
                    var tempKey = string.Format("#{0}#", item.ParameterName).ToLower();
                    var flagParam = string.Format("{0}{1}", flag, item.ParameterName).ToLower();

                    if (sql.ToString().ToLower().IndexOf(tempKey) >= 0)
                    {
                        var tempSql = sql.ToString().ToLower().Replace(tempKey, item.Value.ToStr());
                        sql.Clear();
                        sql.Append(tempSql);
                    }
                    else if (sql.ToString().ToLower().IndexOf(flagParam) >= 0 && flag != "")
                        tempParam.Add(item);
                }
                param = tempParam.ToArray();
            }

            if (!sql.ToString().Contains(flag))
            {
                tempParam.Clear();
                param = tempParam.ToArray();
            }

            return sql.ToString();
        }
        #endregion

        #region fastmap sql
        internal static string GetFastMapSql(MethodInfo methodInfo, ConfigModel config, ref List<DbParameter> param)
        {
            var temp = param.ToArray();
            var key = string.Format("{0}.{1}", methodInfo.DeclaringType.FullName, methodInfo.Name);
            var sql = GetMapSql(key, ref temp, null, config.Key);
            param = temp.ToList();
            return sql;
        }
        #endregion

        #region 是否foreach
        /// <summary>
        /// 是否foreach
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static bool MapIsForEach(string name, ConfigModel config, int i = 1)
        {
            var keyName = string.Format("{0}.foreach.name.{1}", name.ToLower(), i);
            var keyField = string.Format("{0}.foreach.field.{1}", name.ToLower(), i);
            var keySql = string.Format("{0}.foreach.sql.{1}", name.ToLower(), i);

            return !string.IsNullOrEmpty(DbCache.Get(config.CacheType, keyName)) &&
                !string.IsNullOrEmpty(DbCache.Get(config.CacheType, keyField)) &&
                !string.IsNullOrEmpty(DbCache.Get(config.CacheType, keySql));
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
        internal static List<Dictionary<string, object>> MapForEach(List<Dictionary<string, object>> data, string name, DataContext db, string key, ConfigModel config, int i = 1)
        {
            var result = new List<Dictionary<string, object>>();
            var param = new List<DbParameter>();
            var dicName = DbCache.Get(config.CacheType, string.Format("{0}.foreach.name.{1}", name.ToLower(), i));
            var field = DbCache.Get(config.CacheType, string.Format("{0}.foreach.field.{1}", name.ToLower(), i));
            var sql = DbCache.Get(config.CacheType, string.Format("{0}.foreach.sql.{1}", name.ToLower(), i));

            data.ForEach(a =>
            {
                param.Clear();
                if (field.IndexOf(',') > 0)
                {
                    foreach (var split in field.Split(','))
                    {
                        var tempParam = DbProviderFactories.GetFactory(config).CreateParameter();
                        tempParam.ParameterName = split;
                        tempParam.Value = a.GetValue(split);
                        param.Add(tempParam);
                    }
                }
                else
                {
                    var tempParam = DbProviderFactories.GetFactory(config).CreateParameter();
                    tempParam.ParameterName = field;
                    tempParam.Value = a.GetValue(field);
                    param.Add(tempParam);
                }

                a.Add(dicName, FastRead.ExecuteSql(sql, param.ToArray(), db, key));
                result.Add(a);
            });

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
        /// <param name="config"></param>
        /// <returns></returns>
        internal static List<T> MapForEach<T>(List<T> data, string name, DataContext db, ConfigModel config, int i = 1) where T : class, new()
        {
            var result = new List<T>();
            var param = new List<DbParameter>();
            var dicName = DbCache.Get(config.CacheType, string.Format("{0}.foreach.name.{1}", name.ToLower(), i));
            var type = DbCache.Get(config.CacheType, string.Format("{0}.foreach.type.{1}", name.ToLower(), i));
            var field = DbCache.Get(config.CacheType, string.Format("{0}.foreach.field.{1}", name.ToLower(), i));
            var sql = DbCache.Get(config.CacheType, string.Format("{0}.foreach.sql.{1}", name.ToLower(), i));
            Assembly assembly;

            if (type.IndexOf(',') > 0)
            {
                var key = string.Format("ForEach.{0}", type.Split(',')[1]);
                if (DbCache.Exists(CacheType.Web, key))
                    assembly = DbCache.Get<object>(CacheType.Web, key) as Assembly;
                else
                    assembly = AppDomain.CurrentDomain.GetAssemblies().ToList().Find(a => a.FullName.Split(',')[0] == type.Split(',')[1]);
                if (assembly == null)
                    assembly = Assembly.Load(type.Split(',')[1]);
                if (assembly == null)
                    return data;
                else
                {
                    DbCache.Set<object>(CacheType.Web, key, assembly);
                    if (assembly.GetType(type.Split(',')[0]) == null)
                        return data;

                    foreach (var item in data)
                    {
                        var model = Activator.CreateInstance(assembly.GetType(type.Split(',')[0]));
                        var list = Activator.CreateInstance(typeof(List<>).MakeGenericType(assembly.GetType(type.Split(',')[0])));
                        var infoResult = BaseDic.PropertyInfo<T>().Find(a => a.PropertyType == list.GetType() && a.Name == dicName);

                        //param
                        param.Clear();
                        if (field.IndexOf(',') > 0)
                        {
                            foreach (var split in field.Split(','))
                            {
                                var infoField = BaseDic.PropertyInfo<T>().Find(a => string.Compare(a.Name, split, true) == 0);
                                var tempParam = DbProviderFactories.GetFactory(config).CreateParameter();
                                tempParam.ParameterName = split;
                                tempParam.Value = infoField.GetValue(item, null);
                                param.Add(tempParam);
                            }
                        }
                        else
                        {
                            var infoField = BaseDic.PropertyInfo<T>().Find(a => string.Compare(a.Name, field, true) == 0);
                            var tempParam = DbProviderFactories.GetFactory(config).CreateParameter();
                            tempParam.ParameterName = field;
                            tempParam.Value = BaseEmit.Get(item, infoField.Name);
                            param.Add(tempParam);
                        }

                        list = db.ExecuteSqlList(model.GetType(), sql, param.ToArray(), false, false).List;

                        BaseEmit.Set(item, infoResult.Name, list);
                        result.Add(item);
                    }
                    return result;
                }
            }
            else
                return data;
        }
        #endregion

        #region map xml 存数据库
        /// <summary>
        /// map xml 存数据库
        /// </summary>
        /// <param name="dbKey"></param>
        /// <param name="key"></param>
        /// <param name="info"></param>
        internal static bool SaveXml(string dbKey, string key, FileInfo info, ConfigModel config, DataContext db)
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

                var model = new Data_MapFile();
                model.MapId = key;
                var query = FastRead.Query<Data_MapFile>(a => a.MapId == key, null, dbKey);

                if (query.ToCount() == 0)
                {
                    model.FileName = info.Name;
                    model.FilePath = info.FullName;
                    model.LastTime = info.LastWriteTime;
                    model.EnFileContent = enContent;
                    model.DeFileContent = deContent;
                    return db.Add(model).WriteReturn.IsSuccess;
                }
                else
                    return db.Update<Data_MapFile>(model, a => a.MapId == model.MapId, a => new { a.LastTime, a.EnFileContent, a.DeFileContent }).WriteReturn.IsSuccess;
            }

            return true;
        }
        #endregion

        #region 读取xml map并缓存
        /// <summary>
        /// 读取xml map并缓存
        /// </summary>
        internal static List<string> ReadXml(string path, ConfigModel config, string fileName, string xml = null)
        {
            var map = DbCache.Get<Dictionary<string, object>>(DataConfig.Get().CacheType, "FastMap.Api") ?? new Dictionary<string, object>();
            var result = GetXmlList(path, "sqlMap", config, xml);

            for (var i = 0; i < result.Key.Count; i++)
            {
                DbCache.Set(config.CacheType, result.Key[i].ToLower(), result.Sql[i]);
            }

            var apilist = new List<string>();
            result.Db.ToList().ForEach(a =>
            {
                DbCache.Set(config.CacheType, string.Format("{0}.db", a.Key.ToLower()), a.Value.ToStr());
                apilist.Add(a.Key.ToLower());
            });

            map.Remove(fileName);
            map.Add(fileName, apilist);
            DbCache.Set<Dictionary<string, object>>(config.CacheType, "FastMap.Api", map);

            result.Type.ToList().ForEach(a =>
            {
                DbCache.Set(config.CacheType, string.Format("{0}.type", a.Key.ToLower()), a.Value.ToStr());
                result.Key.Add(string.Format("{0}.type", a.Key.ToLower()));
            });

            result.View.ToList().ForEach(a =>
            {
                DbCache.Set(config.CacheType, string.Format("{0}.view", a.Key.ToLower()), a.Value.ToStr());
                result.Key.Add(string.Format("{0}.view", a.Key.ToLower()));
            });

            result.Param.ToList().ForEach(a =>
            {
                DbCache.Set<List<string>>(config.CacheType, string.Format("{0}.param", a.Key.ToLower()), a.Value as List<string>);
                result.Key.Add(string.Format("{0}.param", a.Key.ToLower()));
            });

            result.Check.ToList().ForEach(a =>
            {
                DbCache.Set(config.CacheType, a.Key, a.Value.ToStr());
                result.Key.Add(a.Key);
            });

            result.Name.ToList().ForEach(a =>
            {
                DbCache.Set(config.CacheType, a.Key, a.Value.ToStr());
                result.Key.Add(a.Key);
            });

            result.ParameName.ToList().ForEach(a =>
            {
                DbCache.Set(config.CacheType, a.Key, a.Value.ToStr());
                result.Key.Add(a.Key);
            });
            return result.Key;
        }
        #endregion

        #region 读取fastmap xml 并缓存
        /// <summary>
        /// 读取fastmap xml 并缓存
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="methodInfo"></param>
        internal static void ReadFastMap(string xml, MethodInfo methodInfo, ConfigModel config)
        {
            var key = string.Format("{0}.{1}", methodInfo.DeclaringType.FullName, methodInfo.Name).ToLower();
            var result = GetXmlList(null, "sqlMap", config, string.Format("<sqlMap>{0}</sqlMap>", xml), key);

            for (var i = 0; i < result.Key.Count; i++)
            {
                DbCache.Set(config.CacheType, result.Key[i].ToLower(), result.Sql[i]);
            }

            result.Param.ToList().ForEach(a =>
            {
                DbCache.Set<List<string>>(config.CacheType, string.Format("{0}.param", a.Key.ToLower()), a.Value as List<string>);
                result.Key.Add(string.Format("{0}.param", a.Key.ToLower()));
            });
        }
        #endregion

        #region 返回字符串列表
        /// <summary>
        /// 返回字符串列表
        /// </summary>
        /// <param name="path">文件名</param>
        /// <param name="xmlNode">结点</param>
        /// <returns></returns>
        internal static XmlModel GetXmlList(string path, string xmlNode, ConfigModel config, string xml = null, string id = null)
        {
            var result = new XmlModel();
            result.IsSuccess = true;
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
                else if (!string.IsNullOrEmpty(xml))
                    xmlDoc.LoadXml(xml);
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
                            if (id == null)
                                tempKey = temp.Attributes["id"].Value.ToLower();
                            else
                                tempKey = id;

                            //节点数
                            if (Array.Exists(result.Key.ToArray(), element => element == tempKey))
                            {
                                result.IsSuccess = false;
                                Task.Run(() => { BaseLog.SaveLog(string.Format("xml文件:{0},存在相同键:{1}", path, tempKey), "MapKeyExists"); }).ConfigureAwait(false);
                            }
                            result.Key.Add(tempKey);
                            result.Sql.Add(temp.ChildNodes.Count.ToString());

                            //name
                            if (temp.Attributes["name"] != null)
                                result.Name.Add(string.Format("{0}.remark", tempKey), temp.Attributes["name"].Value);

                            //log
                            if (temp.Attributes["log"] != null)
                                result.Name.Add(string.Format("{0}.log", tempKey), temp.Attributes["log"].Value);

                            foreach (XmlNode node in temp.ChildNodes)
                            {
                                #region XmlText
                                if (node is XmlText)
                                {
                                    result.Key.Add(string.Format("{0}.{1}", tempKey, i));
                                    result.Sql.Add(node.InnerText.Replace("&lt;", "<").Replace("&gt", ">"));
                                }
                                #endregion

                                #region XmlElement 动态条件
                                if (node is XmlElement)
                                {
                                    if (node.Attributes["prepend"] != null)
                                    {
                                        result.Key.Add(string.Format("{0}.format.{1}", tempKey, i));
                                        result.Sql.Add(node.Attributes["prepend"].Value.ToLower());
                                    }

                                    //foreach
                                    if (string.Compare(node.Name, "foreach", true) == 0)
                                    {
                                        XmlOption.ForeachXml(result, tempKey, node, foreachCount);
                                        foreachCount++;
                                        continue;
                                    }

                                    //include
                                    if (string.Compare(node.Name, "include", true) == 0)
                                    {
                                        XmlOption.IncludeXml(result, tempKey, node, i);
                                    }

                                    foreach (XmlNode dyn in node.ChildNodes)
                                    {
                                        //foreach
                                        if (string.Compare(dyn.Name, "foreach", true) == 0)
                                        {
                                            XmlOption.ForeachXml(result, tempKey, dyn, foreachCount);
                                            foreachCount++;
                                            continue;
                                        }

                                        //check
                                        XmlOption.CheckXml(result, tempKey, dyn);

                                        //参数
                                        if (dyn.Attributes["property"] != null)
                                            tempParam.Add(dyn.Attributes["property"].Value);

                                        //param name
                                        if (dyn.Attributes["name"] != null)
                                            result.ParameName.Add(string.Format("{0}.{1}.remark", tempKey, dyn.Attributes["property"].Value.ToLower()), dyn.Attributes["name"].Value);

                                        if (string.Compare(dyn.Name, "ispropertyavailable", true) == 0)
                                        {
                                            //属性和值
                                            result.Key.Add(string.Format("{0}.{1}.{2}", tempKey, dyn.Attributes["property"].Value.ToLower(), i));
                                            result.Sql.Add(string.Format("{0}{1}", dyn.Attributes["prepend"].Value.ToLower(), dyn.InnerText));
                                        }
                                        else if (string.Compare(dyn.Name, "include", true) == 0)
                                        {
                                            XmlOption.IncludeXml(result, tempKey, dyn, i);
                                        }
                                        else if (string.Compare(dyn.Name, "choose", true) != 0)
                                        {
                                            XmlOption.ConditionXml(result, tempKey, dyn, i);
                                        }
                                        else
                                        {
                                            XmlOption.ChooseXml(result, tempKey, dyn, i);
                                        }
                                    }
                                }
                                #endregion

                                i++;
                            }

                            //db
                            if (temp.Attributes["db"] != null)
                                result.Db.Add(tempKey, temp.Attributes["db"].Value.ToStr());

                            //type
                            if (temp.Attributes["type"] != null)
                                result.Type.Add(tempKey, temp.Attributes["type"].Value.ToStr());

                            //view
                            if (temp.Attributes["view"] != null)
                                result.View.Add(tempKey, temp.Attributes["view"].Value.ToStr());

                            //foreach count
                            result.Key.Add(string.Format("{0}.foreach", tempKey));
                            result.Sql.Add((foreachCount - 1).ToStr());

                            result.Param.Add(tempKey, tempParam);
                            #endregion
                        }
                        else if (temp is XmlText)
                        {
                            #region XmlText
                            if (id == null)
                                result.Key.Add(string.Format("{0}.{1}", item.Attributes["id"].Value.ToLower(), i));
                            else
                                result.Key.Add(string.Format("{0}.{1}", id.ToLower(), i));

                            result.Sql.Add(temp.InnerText.Replace("&lt;", "<").Replace("&gt", ">"));

                            if (id == null)
                                result.Key.Add(item.Attributes["id"].Value.ToLower());
                            else
                                result.Key.Add(id);
                            result.Sql.Add("0");
                            #endregion
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                var aop = ServiceContext.Engine.Resolve<IFastAop>();

                if (config.SqlErrorType == SqlErrorType.Db)
                    DbLogTable.LogException(config, ex, "InstanceMap", "GetXmlList");
                else
                    DbLog.LogException(true, "InstanceMap", ex, "GetXmlList", "");
                BaseAop.AopException(ex, "Parsing xml", AopType.ParsingXml, config);
                result.IsSuccess = false;
                return result;
            }
        }
        #endregion

        #region 初始化建日记表
        /// <summary>
        /// 初始化建日记表
        /// </summary>
        /// <param name="query"></param>
        internal static void CreateLogTable(DataQuery query)
        {
            if (string.Compare(query.Config.SqlErrorType, SqlErrorType.Db, true) == 0)
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
    }
}