using FastData.Core.Context;
using FastData.Core.Model;
using FastUntility.Core.Base;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Linq;
using System.Xml;
using System.IO;
using System.Reflection;
using FastData.Core.Property;
using System.Threading.Tasks;
using FastData.Core.Type;
using FastData.Core.Check;

namespace FastData.Core.Base
{
    internal static class MapXml
    {
        #region froeach数量
        /// <summary>
        /// froeach数量
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static int MapForEachCount(string name, ConfigModel config)
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
                        foreach (var item in DbCache.Get<List<string>>(DataConfig.Get().CacheType, string.Format("{0}.param", name.ToLower())))
                        {
                            if (!param.ToList().Exists(a => a.ParameterName.ToLower() == item.ToLower()))
                                continue;
                            var temp = param.ToList().Find(a => a.ParameterName.ToLower() == item.ToLower());
                            if (!tempParam.ToList().Exists(a => a.ParameterName == temp.ParameterName))
                                tempParam.Add(temp);

                            var paramKey = string.Format("{0}.{1}.{2}", name.ToLower(), temp.ParameterName.ToLower(), i);
                            var conditionKey = string.Format("{0}.{1}.condition.{2}", name.ToLower(), temp.ParameterName.ToLower(), i);
                            var conditionValueKey = string.Format("{0}.{1}.condition.value.{2}", name.ToLower(), temp.ParameterName.ToLower(), i);
                            if (DbCache.Exists(cacheType, paramKey))
                            {
                                var flagParam = string.Format("{0}{1}", flag, temp.ParameterName);
                                var tempKey = string.Format("#{0}#", temp.ParameterName);
                                var paramSql = DbCache.Get(cacheType, paramKey.ToLower());
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
                                                    tempSql.Append(paramSql.ToString().Replace(tempKey, temp.Value.ToStr()));
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
                                                    tempSql.Append(paramSql.ToString().Replace(temp.ParameterName.ToLower(), temp.Value.ToStr()));
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
                                                    tempSql.Append(paramSql.ToString().Replace(tempKey, temp.Value.ToStr()));
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
                                                    tempSql.Append(paramSql.ToString().Replace(tempKey, temp.Value.ToStr()));
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
                                                    tempSql.Append(paramSql.ToString().Replace(tempKey, temp.Value.ToStr()));
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
                                                    tempSql.Append(paramSql.ToString().Replace(tempKey, temp.Value.ToStr()));
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
                                            conditionValue = conditionValue.Replace(temp.ParameterName, temp.Value == null ? null : temp.Value.ToStr());
                                            conditionValue = conditionValue.Replace("#", "\"");
                                            if (CSharpScript.EvaluateAsync<bool>(conditionValue).Result)
                                            {
                                                if (paramSql.IndexOf(tempKey) >= 0)
                                                {
                                                    tempParam.Remove(temp);
                                                    tempSql.Append(paramSql.ToString().Replace(tempKey, temp.Value.ToStr()));
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
                                                condition = DbCache.Get(cacheType, conditionKey).ToStr();

                                                conditionValueKey = string.Format("{0}.choose.condition.{1}", paramKey, j);
                                                conditionValue = DbCache.Get(cacheType, conditionValueKey).ToStr();
                                                conditionValue = conditionValue.Replace(temp.ParameterName, temp.Value == null ? null : temp.Value.ToStr());
                                                conditionValue = conditionValue.Replace("#", "\"");

                                                if (CSharpScript.EvaluateAsync<bool>(conditionValue).Result)
                                                {
                                                    isSuccess = true;
                                                    if (condition.IndexOf(tempKey) >= 0)
                                                    {
                                                        tempParam.Remove(temp);
                                                        tempSql.Append(condition.Replace(tempKey, temp.Value.ToStr()));
                                                    }
                                                    else if (condition.IndexOf(flagParam) < 0 && flag != "")
                                                    {
                                                        tempParam.Remove(temp);
                                                        tempSql.Append(condition.Replace(tempKey, temp.Value.ToStr()));
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
                                                tempSql.Append(paramSql.ToString().Replace(tempKey, temp.Value.ToStr()));
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

        #region 是否foreach
        /// <summary>
        /// 是否foreach
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool MapIsForEach(string name, ConfigModel config, int i = 1)
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
        public static List<Dictionary<string, object>> MapForEach(List<Dictionary<string, object>> data, string name, DataContext db,string key, ConfigModel config, int i = 1)
        {
            var result = new List<Dictionary<string, object>>();
            var param = new List<DbParameter>();
            var dicName = DbCache.Get(config.CacheType, string.Format("{0}.foreach.name.{1}", name.ToLower(), i));
            var field = DbCache.Get(config.CacheType, string.Format("{0}.foreach.field.{1}", name.ToLower(), i));
            var sql = DbCache.Get(config.CacheType, string.Format("{0}.foreach.sql.{1}", name.ToLower(), i));

            data.ForEach(a => {
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
        public static List<T> MapForEach<T>(List<T> data, string name, DataContext db, ConfigModel config, int i = 1) where T : class, new()
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
                        var infoResult = BaseDic.PropertyInfo<T>().Find(a => a.PropertyType.FullName == list.GetType().FullName);
                        var dynSet = new DynamicSet(model);

                        //param
                        param.Clear();
                        if (field.IndexOf(',') > 0)
                        {
                            foreach (var split in field.Split(','))
                            {
                                var infoField = BaseDic.PropertyInfo<T>().Find(a => a.Name.ToLower() == split);
                                var tempParam = DbProviderFactories.GetFactory(config).CreateParameter();
                                tempParam.ParameterName = split;
                                tempParam.Value = infoField.GetValue(item, null);
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

                        var tempData = db.ExecuteSql(sql, param.ToArray(), false);

                        foreach (var temp in tempData.DicList)
                        {
                            foreach (var info in model.GetType().GetProperties())
                            {
                                if (temp.GetValue(info.Name).ToStr() == "" && info.PropertyType.Name == "Nullable`1")
                                    continue;

                                if (info.PropertyType.Name == "Nullable`1" && info.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                    dynSet.SetValue(model, info.Name, Convert.ChangeType(temp.GetValue(info.Name), Nullable.GetUnderlyingType(info.PropertyType)), config.IsPropertyCache);
                                else
                                    dynSet.SetValue(model, info.Name, Convert.ChangeType(temp.GetValue(info.Name), info.PropertyType), config.IsPropertyCache);
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

        #region map xml 存数据库
        /// <summary>
        /// map xml 存数据库
        /// </summary>
        /// <param name="dbKey"></param>
        /// <param name="key"></param>
        /// <param name="info"></param>
        public static bool SaveXml(string dbKey, string key, FileInfo info, ConfigModel config, DataContext db)
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

        #region 读取xml map并缓存
        /// <summary>
        /// 读取xml map并缓存
        /// </summary>
        public static List<string> ReadXml(string path, ConfigModel config, string fileName,string xml = null)
        {
            var map = DbCache.Get<Dictionary<string, object>>(DataConfig.Get().CacheType, "FastMap.Api") ?? new Dictionary<string, object>();
            var key = new List<string>();
            var sql = new List<string>();
            var db = new Dictionary<string, object>();
            var type = new Dictionary<string, object>();
            var param = new Dictionary<string, object>();
            var check = new Dictionary<string, object>();
            var name = new Dictionary<string, object>();
            var parameName = new Dictionary<string, object>();

            GetXmlList(path, "sqlMap", ref key, ref sql, ref db, ref type, ref check, ref param, ref name, ref parameName, config, xml);

            for (var i = 0; i < key.Count; i++)
            {
                DbCache.Set(config.CacheType, key[i].ToLower(), sql[i]);
            }

            var apilist = new List<string>();
            db.ToList().ForEach(a => {
                DbCache.Set(config.CacheType, string.Format("{0}.db", a.Key.ToLower()), a.Value.ToStr());
                apilist.Add(a.Key.ToLower());
            });

            map.Remove(fileName);
            map.Add(fileName, apilist);
            DbCache.Set<Dictionary<string, object>>(config.CacheType, "FastMap.Api", map);

            type.ToList().ForEach(a => {
                DbCache.Set(config.CacheType, string.Format("{0}.type", a.Key.ToLower()), a.Value.ToStr());
                key.Add(string.Format("{0}.type", a.Key.ToLower()));
            });

            param.ToList().ForEach(a => {
                DbCache.Set<List<string>>(config.CacheType, string.Format("{0}.param", a.Key.ToLower()), a.Value as List<string>);
                key.Add(string.Format("{0}.param", a.Key.ToLower()));
            });

            check.ToList().ForEach(a => {
                DbCache.Set(config.CacheType, a.Key, a.Value.ToStr());
                key.Add(a.Key);
            });

            name.ToList().ForEach(a => {
                DbCache.Set(config.CacheType, a.Key, a.Value.ToStr());
                key.Add(a.Key);
            });

            parameName.ToList().ForEach(a => {
                DbCache.Set(config.CacheType, a.Key, a.Value.ToStr());
                key.Add(a.Key);
            });
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
        public static bool GetXmlList(string path, string xmlNode,
            ref List<string> key, ref List<string> sql, ref Dictionary<string, object> db,
            ref Dictionary<string, object> type, ref Dictionary<string, object> check,
            ref Dictionary<string, object> param, ref Dictionary<string, object> name,
            ref Dictionary<string, object> parameName, ConfigModel config,string xml=null)
        {
            try
            {
                var result = true;
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
                            tempKey = temp.Attributes["id"].Value.ToLower();

                            //节点数
                            if (Array.Exists(key.ToArray(), element => element == tempKey))
                            {
                                result = false;
                                Task.Run(() => { BaseLog.SaveLog(string.Format("xml文件:{0},存在相同键:{1}", path, tempKey), "MapKeyExists"); });
                            }
                            key.Add(tempKey);
                            sql.Add(temp.ChildNodes.Count.ToString());

                            //name
                            if (temp.Attributes["name"] != null)
                                name.Add(string.Format("{0}.remark", tempKey), temp.Attributes["name"].Value);

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
                                        key.Add(string.Format("{0}.foreach.name.{1}", tempKey, foreachCount));
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

                                        //param name
                                        if (dyn.Attributes["name"] != null)
                                            parameName.Add(string.Format("{0}.{1}.remark", tempKey, dyn.Attributes["property"].Value.ToLower()), dyn.Attributes["name"].Value);

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
                            sql.Add((foreachCount - 1).ToStr());

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
                return result;
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
                return false;
            }
        }
        #endregion


        #region 初始化建日记表
        /// <summary>
        /// 初始化建日记表
        /// </summary>
        /// <param name="query"></param>
        public static void CreateLogTable(DataQuery query)
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
    }
}
