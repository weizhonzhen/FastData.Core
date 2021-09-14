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
using Microsoft.CodeAnalysis.Scripting;
using FastData.Core.Aop;
using FastUntility.Core;

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
                        foreach (var item in DbCache.Get<List<string>>(cacheType, string.Format("{0}.param", name.ToLower())))
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
                                var flagParam = string.Format("{0}{1}", flag, temp.ParameterName).ToLower();
                                var tempKey = string.Format("#{0}#", temp.ParameterName).ToLower();
                                var paramSql = DbCache.Get(cacheType, paramKey.ToLower()).ToLower();
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
                                            conditionValue = DbCache.Get(cacheType, conditionValueKey).ToStr();
                                            conditionValue = conditionValue.Replace(temp.ParameterName, temp.Value == null ? null : temp.Value.ToStr());
                                            conditionValue = conditionValue.Replace("#", "\"");

                                            //references
                                            var ifSuccess = false;
                                            var referencesKey = string.Format("{0}.{1}.references.{2}", name.ToLower(), temp.ParameterName.ToLower(), i);
                                            if (DbCache.Get(cacheType, referencesKey).ToStr() != "")
                                            {
                                                var assembly = AppDomain.CurrentDomain.GetAssemblies().ToList().Find(a => a.FullName.Split(',')[0] == DbCache.Get(cacheType, referencesKey));
                                                if (assembly == null)
                                                    assembly = Assembly.Load(DbCache.Get(cacheType, referencesKey));
                                                if (assembly != null)
                                                {
                                                    var options = ScriptOptions.Default.AddReferences(assembly);
                                                    ifSuccess = CSharpScript.EvaluateAsync<bool>(conditionValue, options).Result;
                                                }
                                                else
                                                    ifSuccess = CSharpScript.EvaluateAsync<bool>(conditionValue).Result;
                                            }
                                            else
                                                ifSuccess = CSharpScript.EvaluateAsync<bool>(conditionValue).Result;

                                            if (ifSuccess)
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
                                            var conditionOther = "";
                                            var isSuccess = false;
                                            for (int j = 0; j < DbCache.Get(cacheType, paramKey).ToStr().ToInt(0); j++)
                                            {
                                                var conditionOtherKey = string.Format("{0}.choose.other.{1}", paramKey, j);
                                                if (DbCache.Get(cacheType, conditionOtherKey).ToStr() != "")
                                                    conditionOther = DbCache.Get(cacheType, conditionOtherKey).ToLower();

                                                conditionKey = string.Format("{0}.choose.{1}", paramKey, j);
                                                condition = DbCache.Get(cacheType, conditionKey).ToStr().ToLower();
                                                conditionValueKey = string.Format("{0}.choose.condition.{1}", paramKey, j);
                                                conditionValue = DbCache.Get(cacheType, conditionValueKey).ToStr();
                                                conditionValue = conditionValue.Replace(temp.ParameterName, temp.Value == null ? null : temp.Value.ToStr());
                                                conditionValue = conditionValue.Replace("#", "\"");

                                                //references
                                                var referencesKey = string.Format("{0}.choose.references.{1}", paramKey, j);
                                                if (DbCache.Get(cacheType, referencesKey).ToStr() != "")
                                                {
                                                    var assembly = AppDomain.CurrentDomain.GetAssemblies().ToList().Find(a => a.FullName.Split(',')[0] == DbCache.Get(cacheType, referencesKey));
                                                    if (assembly == null)
                                                        assembly = Assembly.Load(DbCache.Get(cacheType, referencesKey));
                                                    if (assembly != null)
                                                    {
                                                        var options = ScriptOptions.Default.AddReferences(assembly);
                                                        isSuccess = CSharpScript.EvaluateAsync<bool>(conditionValue, options).Result;
                                                    }
                                                    else
                                                        isSuccess = CSharpScript.EvaluateAsync<bool>(conditionValue).Result;
                                                }
                                                else
                                                    isSuccess = CSharpScript.EvaluateAsync<bool>(conditionValue).Result;

                                                if (isSuccess)
                                                {
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
                                            {
                                                if (conditionOther == "")
                                                    tempParam.Remove(temp);
                                                else if (conditionOther.IndexOf(tempKey) >= 0)
                                                {
                                                    tempParam.Remove(temp);
                                                    tempSql.Append(conditionOther.Replace(tempKey, temp.Value.ToStr()));
                                                }
                                                else if (conditionOther.IndexOf(flagParam) < 0 && flag != "")
                                                {
                                                    tempParam.Remove(temp);
                                                    tempSql.Append(conditionOther.Replace(tempKey, temp.Value.ToStr()));
                                                }
                                                else
                                                    tempSql.Append(conditionOther);
                                            }
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

                        var tempData = db.ExecuteSqlList(sql, param.ToArray(), false,false);

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
            var result = GetXmlList(path, "sqlMap", config, xml);

            for (var i = 0; i < result.key.Count; i++)
            {
                DbCache.Set(config.CacheType, result.key[i].ToLower(), result.sql[i]);
            }

            var apilist = new List<string>();
            result.db.ToList().ForEach(a => {
                DbCache.Set(config.CacheType, string.Format("{0}.db", a.Key.ToLower()), a.Value.ToStr());
                apilist.Add(a.Key.ToLower());
            });

            map.Remove(fileName);
            map.Add(fileName, apilist);
            DbCache.Set<Dictionary<string, object>>(config.CacheType, "FastMap.Api", map);

            result.type.ToList().ForEach(a => {
                DbCache.Set(config.CacheType, string.Format("{0}.type", a.Key.ToLower()), a.Value.ToStr());
                result.key.Add(string.Format("{0}.type", a.Key.ToLower()));
            });

            result.view.ToList().ForEach(a => {
                DbCache.Set(config.CacheType, string.Format("{0}.view", a.Key.ToLower()), a.Value.ToStr());
                result.key.Add(string.Format("{0}.view", a.Key.ToLower()));
            });

            result.param.ToList().ForEach(a => {
                DbCache.Set<List<string>>(config.CacheType, string.Format("{0}.param", a.Key.ToLower()), a.Value as List<string>);
                result.key.Add(string.Format("{0}.param", a.Key.ToLower()));
            });

            result.check.ToList().ForEach(a => {
                DbCache.Set(config.CacheType, a.Key, a.Value.ToStr());
                result.key.Add(a.Key);
            });

            result.name.ToList().ForEach(a => {
                DbCache.Set(config.CacheType, a.Key, a.Value.ToStr());
                result.key.Add(a.Key);
            });

            result.parameName.ToList().ForEach(a => {
                DbCache.Set(config.CacheType, a.Key, a.Value.ToStr());
                result.key.Add(a.Key);
            });
            return result.key;
        }
        #endregion

        #region 返回字符串列表
        /// <summary>
        /// 返回字符串列表
        /// </summary>
        /// <param name="path">文件名</param>
        /// <param name="xmlNode">结点</param>
        /// <returns></returns>
        public static XmlModel GetXmlList(string path, string xmlNode, ConfigModel config,string xml=null)
        {
            var result = new XmlModel();
            result.isSuccess = true;
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
                            tempKey = temp.Attributes["id"].Value.ToLower();

                            //节点数
                            if (Array.Exists(result.key.ToArray(), element => element == tempKey))
                            {
                                result.isSuccess = false;
                                Task.Run(() => { BaseLog.SaveLog(string.Format("xml文件:{0},存在相同键:{1}", path, tempKey), "MapKeyExists"); }).ConfigureAwait(false);
                            }
                            result.key.Add(tempKey);
                            result.sql.Add(temp.ChildNodes.Count.ToString());

                            //name
                            if (temp.Attributes["name"] != null)
                                result.name.Add(string.Format("{0}.remark", tempKey), temp.Attributes["name"].Value);

                            //log
                            if (temp.Attributes["log"] != null)
                                result.name.Add(string.Format("{0}.log", tempKey), temp.Attributes["log"].Value);

                            foreach (XmlNode node in temp.ChildNodes)
                            {
                                #region XmlText
                                if (node is XmlText)
                                {
                                    result.key.Add(string.Format("{0}.{1}", tempKey, i));
                                    result.sql.Add(node.InnerText.Replace("&lt;", "<").Replace("&gt", ">"));
                                }
                                #endregion

                                #region XmlElement 动态条件
                                if (node is XmlElement)
                                {
                                    if (node.Attributes["prepend"] != null)
                                    {
                                        result.key.Add(string.Format("{0}.format.{1}", tempKey, i));
                                        result.sql.Add(node.Attributes["prepend"].Value.ToLower());
                                    }

                                    //foreach
                                    if (node.Name.ToLower() == "foreach")
                                    {
                                        //type
                                        if (node.Attributes["type"] != null)
                                        {
                                            result.key.Add(string.Format("{0}.foreach.type.{1}", tempKey, foreachCount));
                                            result.sql.Add(node.Attributes["type"].Value);
                                        }

                                        //result name
                                        result.key.Add(string.Format("{0}.foreach.name.{1}", tempKey, foreachCount));
                                        if (node.Attributes["name"] != null)
                                            result.sql.Add(node.Attributes["name"].Value.ToLower());
                                        else
                                            result.sql.Add("data");

                                        //field
                                        if (node.Attributes["field"] != null)
                                        {
                                            result.key.Add(string.Format("{0}.foreach.field.{1}", tempKey, foreachCount));
                                            result.sql.Add(node.Attributes["field"].Value.ToLower());
                                        }

                                        //sql
                                        if (node.ChildNodes[0] is XmlText)
                                        {
                                            result.key.Add(string.Format("{0}.foreach.sql.{1}", tempKey, foreachCount));
                                            result.sql.Add(node.ChildNodes[0].InnerText.Replace("&lt;", "<").Replace("&gt", ">"));
                                        }
                                        foreachCount++;
                                    }

                                    foreach (XmlNode dyn in node.ChildNodes)
                                    {
                                        if (dyn is XmlText)
                                            continue;

                                        //check required
                                        if (dyn.Attributes["required"] != null)
                                            result.check.Add(string.Format("{0}.{1}.required", tempKey, dyn.Attributes["property"].Value.ToLower()), dyn.Attributes["required"].Value.ToStr());

                                        //check maxlength
                                        if (dyn.Attributes["maxlength"] != null)
                                            result.check.Add(string.Format("{0}.{1}.maxlength", tempKey, dyn.Attributes["property"].Value.ToLower()), dyn.Attributes["maxlength"].Value.ToStr());

                                        //check existsmap
                                        if (dyn.Attributes["existsmap"] != null)
                                            result.check.Add(string.Format("{0}.{1}.existsmap", tempKey, dyn.Attributes["property"].Value.ToLower()), dyn.Attributes["existsmap"].Value.ToStr());

                                        //check checkmap
                                        if (dyn.Attributes["checkmap"] != null)
                                            result.check.Add(string.Format("{0}.{1}.checkmap", tempKey, dyn.Attributes["property"].Value.ToLower()), dyn.Attributes["checkmap"].Value.ToStr());

                                        //check date
                                        if (dyn.Attributes["date"] != null)
                                            result.check.Add(string.Format("{0}.{1}.date", tempKey, dyn.Attributes["property"].Value.ToLower()), dyn.Attributes["date"].Value.ToStr());

                                        //参数
                                        tempParam.Add(dyn.Attributes["property"].Value);

                                        //param name
                                        if (dyn.Attributes["name"] != null)
                                            result.parameName.Add(string.Format("{0}.{1}.remark", tempKey, dyn.Attributes["property"].Value.ToLower()), dyn.Attributes["name"].Value);

                                        if (dyn.Name.ToLower() == "ispropertyavailable")
                                        {
                                            //属性和值
                                            result.key.Add(string.Format("{0}.{1}.{2}", tempKey, dyn.Attributes["property"].Value.ToLower(), i));
                                            result.sql.Add(string.Format("{0}{1}", dyn.Attributes["prepend"].Value.ToLower(), dyn.InnerText));
                                        }
                                        else if (dyn.Name.ToLower() != "choose")
                                        {
                                            //属性和值
                                            result.key.Add(string.Format("{0}.{1}.{2}", tempKey, dyn.Attributes["property"].Value.ToLower(), i));
                                            result.sql.Add(string.Format("{0}{1}", dyn.Attributes["prepend"].Value.ToLower(), dyn.InnerText));

                                            //条件类型
                                            result.key.Add(string.Format("{0}.{1}.condition.{2}", tempKey, dyn.Attributes["property"].Value.ToLower(), i));
                                            result.sql.Add(dyn.Name);

                                            //判断条件内容
                                            if (dyn.Attributes["condition"] != null)
                                            {
                                                result.key.Add(string.Format("{0}.{1}.condition.value.{2}", tempKey, dyn.Attributes["property"].Value.ToLower(), i));
                                                result.sql.Add(dyn.Attributes["condition"].Value);
                                            }

                                            //比较条件值
                                            if (dyn.Attributes["compareValue"] != null)
                                            {
                                                result.key.Add(string.Format("{0}.{1}.condition.value.{2}", tempKey, dyn.Attributes["property"].Value.ToLower(), i));
                                                result.sql.Add(dyn.Attributes["compareValue"].Value.ToLower());
                                            }

                                            //引用dll
                                            if (dyn.Attributes["references"] != null)
                                            {
                                                result.key.Add(string.Format("{0}.{1}.references.{2}", tempKey, dyn.Attributes["property"].Value.ToLower(), i));
                                                result.sql.Add(dyn.Attributes["references"].Value);
                                            }
                                        }
                                        else
                                        {
                                            //条件类型
                                            result.key.Add(string.Format("{0}.{1}.condition.{2}", tempKey, dyn.Attributes["property"].Value.ToLower(), i));
                                            result.sql.Add(dyn.Name);

                                            if (dyn is XmlElement)
                                            {
                                                var count = 0;
                                                result.key.Add(string.Format("{0}.{1}.{2}", tempKey, dyn.Attributes["property"].Value.ToLower(), i));
                                                result.sql.Add(dyn.ChildNodes.Count.ToStr());
                                                foreach (XmlNode child in dyn.ChildNodes)
                                                {
                                                    //other
                                                    if (child.Name == "other")
                                                    {
                                                        result.key.Add(string.Format("{0}.{1}.{2}.choose.other.{3}", tempKey, dyn.Attributes["property"].Value.ToLower(), i, count));
                                                        result.sql.Add(string.Format("{0}{1}", child.Attributes["prepend"].Value.ToLower(), child.InnerText));
                                                    }
                                                    else
                                                    {
                                                        //条件
                                                        if (child.Attributes["property"] != null)
                                                        {
                                                            result.key.Add(string.Format("{0}.{1}.{2}.choose.condition.{3}", tempKey, dyn.Attributes["property"].Value.ToLower(), i, count));
                                                            result.sql.Add(child.Attributes["property"].Value);
                                                        }

                                                        //内容
                                                        result.key.Add(string.Format("{0}.{1}.{2}.choose.{3}", tempKey, dyn.Attributes["property"].Value.ToLower(), i, count));
                                                        result.sql.Add(string.Format("{0}{1}", child.Attributes["prepend"].Value.ToLower(), child.InnerText));

                                                        //引用dll
                                                        if (child.Attributes["references"] != null)
                                                        {
                                                            result.key.Add(string.Format("{0}.{1}.{2}.choose.references.{3}", tempKey, dyn.Attributes["property"].Value.ToLower(), i, count));
                                                            result.sql.Add(child.Attributes["references"].Value);
                                                        }
                                                    }
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
                                result.db.Add(tempKey, temp.Attributes["db"].Value.ToStr());

                            //type
                            if (temp.Attributes["type"] != null)
                                result.type.Add(tempKey, temp.Attributes["type"].Value.ToStr());

                            //view
                            if (temp.Attributes["view"] != null)
                                result.view.Add(tempKey, temp.Attributes["view"].Value.ToStr());

                            //foreach count
                            result.key.Add(string.Format("{0}.foreach", tempKey));
                            result.sql.Add((foreachCount - 1).ToStr());

                            result.param.Add(tempKey, tempParam);
                            #endregion
                        }
                        else if (temp is XmlText)
                        {
                            #region XmlText
                            result.key.Add(string.Format("{0}.{1}", item.Attributes["id"].Value.ToLower(), i));
                            result.sql.Add(temp.InnerText.Replace("&lt;", "<").Replace("&gt", ">"));

                            result.key.Add(item.Attributes["id"].Value.ToLower());
                            result.sql.Add("0");
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
                result.isSuccess = false;
                return result;
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
