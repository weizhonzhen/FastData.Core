using FastData.Core.Aop;
using FastData.Core.Base;
using FastData.Core.CacheModel;
using FastData.Core.Check;
using FastData.Core.Context;
using FastData.Core.Model;
using FastData.Core.Property;
using FastData.Core.Proxy;
using FastData.Core.Type;
using FastUntility.Core;
using FastUntility.Core.Base;
using FastUntility.Core.Page;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace FastData.Core
{
    /// <summary>
    /// map
    /// </summary>
    public static class FastMap
    {
        #region 获取导航属性
        /// <summary>
        /// 获取导航属性
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static NavigateModel GetNavigate(System.Type type)
        {
            var navigate = new NavigateModel();
            type.GetProperties().ToList().ForEach(a =>
            {
                var attribute = a.GetCustomAttribute<NavigateAttribute>();
                if (attribute != null)
                {
                    navigate.Appand.Add(attribute.Appand);
                    navigate.Name.Add(a.Name);
                    navigate.Key.Add(attribute.Name);
                }
            });
            return navigate;
        }
        #endregion

        #region 初始化model成员 1
        /// <summary>
        /// 初始化model成员 1
        /// </summary>
        /// <param name="list"></param>
        /// <param name="nameSpace">命名空间</param>
        /// <param name="dll">dll名称</param>
        internal static void InstanceProperties(string nameSpace, string dbFile = "db.json", string projectName = null)
        {
            var config = DataConfig.Get("", projectName, dbFile);

            AppDomain.CurrentDomain.GetAssemblies().ToList().ForEach(assembly =>
            {
                try
                {
                    assembly.ExportedTypes.ToList().ForEach(p =>
                    {
                        var typeInfo = (p as TypeInfo);
                        if (typeInfo.Namespace != null && typeInfo.Namespace == nameSpace)
                        {
                            var key = string.Format("{0}.{1}", typeInfo.Namespace, typeInfo.Name);
                            var navigateKey = string.Format("{0}.navigate", key);
                            var cacheList = new List<PropertyModel>();
                            var cacheNavigate = new List<NavigateModel>();

                            typeInfo.DeclaredProperties.ToList().ForEach(a =>
                            {
                                var navigateType = a.GetCustomAttribute<NavigateTypeAttribute>();

                                if (navigateType != null && a.PropertyType == typeof(Dictionary<string, object>) && a.GetMethod.IsVirtual)
                                {
                                    var navigate = GetNavigate(navigateType.Type);
                                    navigate.IsList = false;
                                    navigate.PropertyType = navigateType.Type;
                                    navigate.MemberName = a.Name;
                                    navigate.MemberType = a.PropertyType;
                                    navigate.IsUpdate = navigateType.IsUpdate;
                                    navigate.IsDel = navigateType.IsDel;
                                    navigate.IsAdd = navigateType.IsAdd;
                                    if (navigate.Name.Count != 0)
                                        cacheNavigate.Add(navigate);
                                }
                                else if (navigateType != null && a.PropertyType == typeof(List<Dictionary<string, object>>) && a.GetMethod.IsVirtual)
                                {
                                    var navigate = GetNavigate(navigateType.Type);
                                    navigate.IsList = true;
                                    navigate.PropertyType = navigateType.Type;
                                    navigate.MemberName = a.Name;
                                    navigate.MemberType = a.PropertyType;
                                    navigate.IsUpdate = navigateType.IsUpdate;
                                    navigate.IsDel = navigateType.IsDel;
                                    navigate.IsAdd = navigateType.IsAdd;
                                    if (navigate.Name.Count != 0)
                                        cacheNavigate.Add(navigate);
                                }
                                else if (a.PropertyType.GetGenericArguments().Length > 0 && a.GetMethod.IsVirtual)
                                {
                                    var navigate = GetNavigate(a.PropertyType.GenericTypeArguments[0]);
                                    navigate.IsList = true;
                                    navigate.PropertyType = a.PropertyType.GenericTypeArguments[0];
                                    navigate.MemberName = a.Name;
                                    navigate.MemberType = a.PropertyType;
                                    if (navigateType != null)
                                    {
                                        navigate.IsUpdate = navigateType.IsUpdate;
                                        navigate.IsDel = navigateType.IsDel;
                                        navigate.IsAdd = navigateType.IsAdd;
                                    }
                                    if (navigate.Name.Count != 0)
                                        cacheNavigate.Add(navigate);
                                }
                                else if (a.GetMethod.IsVirtual)
                                {
                                    var navigate = GetNavigate(a.PropertyType);
                                    navigate.IsList = false;
                                    navigate.PropertyType = a.PropertyType;
                                    navigate.MemberName = a.Name;
                                    navigate.MemberType = a.PropertyType;
                                    if (navigateType != null)
                                    {
                                        navigate.IsUpdate = navigateType.IsUpdate;
                                        navigate.IsDel = navigateType.IsDel;
                                        navigate.IsAdd = navigateType.IsAdd;
                                    }
                                    if (navigate.Name.Count != 0)
                                        cacheNavigate.Add(navigate);
                                }
                                else
                                {
                                    var model = new PropertyModel();
                                    model.Name = a.Name;
                                    model.PropertyType = a.PropertyType;
                                    cacheList.Add(model);
                                }
                            });

                            if (cacheNavigate.Count > 0)
                                DbCache.Set<List<NavigateModel>>(config.CacheType, navigateKey, cacheNavigate);

                            DbCache.Set<List<PropertyModel>>(config.CacheType, key, cacheList);
                        }
                    });
                }
                catch (Exception ex) { }
            });
        }
        #endregion

        #region 初始化code first 2
        /// <summary>
        /// 初始化code first 2
        /// </summary>
        /// <param name="list"></param>
        /// <param name="nameSpace">命名空间</param>
        /// <param name="dll">dll名称</param>
        internal static void InstanceTable(string nameSpace, string dbKey = null, string dbFile = "db.json", string projectName = null)
        {
            var query = new DataQuery();
            query.Config = DataConfig.Get(dbKey, projectName, dbFile);
            query.Key = dbKey;

            MapXml.CreateLogTable(query);
            AppDomain.CurrentDomain.GetAssemblies().ToList().ForEach(assembly =>
            {
                try
                {
                    assembly.ExportedTypes.ToList().ForEach(a =>
                    {
                        var typeInfo = (a as TypeInfo);
                        if (typeInfo.Namespace != null && typeInfo.Namespace == nameSpace && typeInfo.GetCustomAttribute<TableAttribute>() != null)
                        {
                            var tableName = typeInfo.GetCustomAttribute<TableAttribute>().Name ?? a.Name;
                            BaseTable.Check(query, tableName, typeInfo.DeclaredProperties.ToList(), typeInfo.GetCustomAttributes().ToList());
                        }
                    });
                }
                catch (Exception ex) { }
            });
        }
        #endregion

        #region 初始化map 3  by Resource
        internal static void InstanceMapResource(string dbKey = null, string dbFile = "db.json", string mapFile = "map.json", string projectName = null)
        {
            if (projectName == null)
                projectName = FastDataExtension.config == null ? null : FastDataExtension.config.Current.GetName().Name;

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
                            map.Path = BaseJsonSys.JsonToModel<List<string>>(BaseJsonSys.JsonToDic(BaseJsonSys.ModelToJson(BaseJsonSys.JsonToDic(content).GetValue(AppSettingKey.Map))).GetValue("Path").ToStr());
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

        #region 初始化 interface service
        internal static void InstanceService(IServiceCollection serviceCollection, string nameSpace)
        {
            var handler = new ProxyHandler();

            AppDomain.CurrentDomain.GetAssemblies().ToList().ForEach(assembly =>
            {
                try
                {
                    foreach (var a in assembly.ExportedTypes.ToList())
                    {
                        if (a.Namespace == nameSpace)
                        {
                            var isRegister = false;
                            a.GetMethods().ToList().ForEach(m =>
                            {
                                ConfigModel config = new ConfigModel();
                                var model = new ServiceModel();
                                var read = m.GetCustomAttribute<FastReadAttribute>();
                                var write = m.GetCustomAttribute<FastWriteAttribute>();
                                var map = m.GetCustomAttribute<FastMapAttribute>();

                                if (read != null)
                                {
                                    isRegister = true;
                                    model.isWrite = false;
                                    model.sql = read.sql.ToLower();
                                    model.dbKey = read.dbKey;
                                    config = DataConfig.Get(model.dbKey);
                                    model.isPage = read.isPage;
                                    model.type = m.ReturnType;
                                    ServiceParam(m, model, config);
                                }

                                if (write != null)
                                {
                                    isRegister = true;
                                    model.isWrite = true;
                                    model.sql = write.sql.ToLower();
                                    model.dbKey = write.dbKey;
                                    model.type = m.ReturnType;
                                    config = DataConfig.Get(model.dbKey);
                                    ServiceParam(m, model, config);
                                    model.isList = false;
                                }

                                if (map != null)
                                {
                                    isRegister = true;
                                    model.isWrite = false;
                                    model.isXml = true;
                                    model.dbKey = map.dbKey;
                                    model.isPage = map.isPage;
                                    model.type = m.ReturnType;
                                    config = DataConfig.Get(model.dbKey);
                                    MapXml.ReadFastMap(map.xml, m, config);
                                    ServiceParam(m, model, config);
                                }

                                if (isRegister)
                                {
                                    var key = string.Format("{0}.{1}", a.FullName, m.Name);
                                    DbCache.Set<ServiceModel>(config.CacheType, key, model);
                                }
                            });

                            if (isRegister)
                            {
                                var service = FastProxy.Invoke(a, handler);
                                serviceCollection.AddSingleton(a, service);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex is ProxyException)
                        throw ex;
                }
            });
        }
        #endregion

        #region 服务参数
        /// <summary>
        /// 服务参数
        /// </summary>
        /// <param name="info"></param>
        /// <param name="model"></param>
        private static void ServiceParam(MethodInfo info, ServiceModel model, ConfigModel config)
        {
            if (info.ReturnType != typeof(WriteReturn) && model.isWrite)
                throw new ProxyException($"[return type only WriteReturn, service:{info.DeclaringType.Name}, method:{info.Name}, return type:{info.ReturnType} is not support]");

            if (string.IsNullOrEmpty(model.dbKey))
                throw new ProxyException($"[service:{info.DeclaringType.Name}, method:{info.Name}, dbkey is not null]");

            if (info.ReturnType.isSysType())
                throw new ProxyException($"[service:{info.DeclaringType.Name}, method:{info.Name}, return type:{info.ReturnType} is not support]");

            if (string.IsNullOrEmpty(model.sql) && !model.isXml)
                throw new ProxyException($"[service:{info.DeclaringType.Name}, method:{info.Name}, sql is not null]");

            if (model.isPage && !info.GetParameters().ToList().Exists(a => a.ParameterType == typeof(PageModel)))
                throw new ProxyException($"[service:{info.DeclaringType.Name}, method:{info.Name}, read data by page , parameter type:{typeof(PageModel).FullName} not exists]");

            if (info.GetParameters().Length == 1 && info.GetParameters()[0].ParameterType.IsGenericType)
                throw new ProxyException($"[service:{info.DeclaringType.Name}, method:{info.Name}, parameter type:{info.GetParameters()[0].ParameterType} is not support]");

            if (model.isPage && info.ReturnType.GetGenericArguments().Length > 0 && info.ReturnType == typeof(PageResult<>).MakeGenericType(new System.Type[] { info.ReturnType.GetGenericArguments()[0] }))
                model.type = info.ReturnType.GetGenericArguments()[0];
            else if (model.isPage && info.ReturnType == typeof(PageResult))
                model.type = null;
            else if (model.isPage)
                throw new ProxyException($"[service:{info.DeclaringType.Name}, method:{info.Name}, read data by page , return type:{info.ReturnType} is not support]");

            if (info.ReturnType == typeof(Dictionary<string, object>) && (!model.isWrite || model.isXml))
                model.isList = false;
            else if (info.ReturnType == typeof(List<Dictionary<string, object>>) && (!model.isWrite || model.isXml))
                model.isList = true;
            else if (!model.isWrite || model.isXml)
            {
                model.isList = info.ReturnType.GetGenericArguments().Length > 0;
                System.Type argType;

                if (model.isList)
                    argType = info.ReturnType.GetGenericArguments()[0];
                else
                    argType = info.ReturnType;

                if (argType.isSysType())
                    throw new ProxyException($"[service:{info.DeclaringType.Name}, method:{info.Name}, return type:{info.ReturnType} is not support]");
            }

            var dic = new Dictionary<int, string>();

            if (info.GetParameters().ToList().Exists(a => a.ParameterType == typeof(Dictionary<string, object>)))
                model.isDic = true;
            else if (!info.GetParameters().ToList().Exists(a => a.ParameterType.isSysType()))
            {
                var type = info.GetParameters().ToList().Find(a => a.ParameterType != typeof(PageModel)).ParameterType;
                var pro = PropertyCache.GetPropertyInfo(Activator.CreateInstance(type));
                pro.ForEach(a =>
                {
                    var key = string.Format("{0}{1}", config.Flag, a.Name).ToLower();
                    if (!model.isXml && model.sql.IndexOf(key) > 0)
                    {
                        dic.Add(model.sql.IndexOf(key), a.Name);
                    }
                });
            }
            else
            {
                for (int i = 0; i < info.GetParameters().Length; i++)
                {
                    var key = string.Format("{0}{1}", config.Flag, info.GetParameters()[i].Name).ToLower();
                    if (!model.isXml && model.sql.IndexOf(key) > 0)
                    {
                        dic.Add(model.sql.IndexOf(key), info.GetParameters()[i].Name.ToLower());
                    }
                }
                model.isSysType = true;
            }

            var list = dic.OrderBy(d => d.Key).ToList();

            for (int i = 0; i < list.Count; i++)
            {
                model.param.Add(i.ToString(), dic[list[i].Key]);
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
            param = Parameter.ToDbParameter(param, config);
            if (config.IsUpdateCache)
                InstanceMap(key);
            if (DbCache.Exists(config.CacheType, name.ToLower()))
            {
                var sql = MapXml.GetMapSql(name, ref param, db, key);
                isOutSql = isOutSql ? isOutSql : IsMapLog(name);
                BaseAop.AopMapBefore(name, sql, param, config, AopType.Map_List_Model);
                var result = FastRead.ExecuteSql<T>(sql, param, db, key, isOutSql, false);
                if (MapXml.MapIsForEach(name, config))
                {
                    db = db == null ? ServiceContext.Engine.Resolve<IUnitOfWorK>().Contexts(key) : db;
                    for (var i = 1; i <= MapXml.MapForEachCount(name, config); i++)
                    {
                        result = MapXml.MapForEach<T>(result, name, db, config, i);
                    }
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
        public static ValueTask<List<T>> QueryAsy<T>(string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            return new ValueTask<List<T>>(Query<T>(name, param, db, key, isOutSql));
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
        public static ValueTask<Lazy<List<T>>> QueryLazyAsy<T>(string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            return new ValueTask<Lazy<List<T>>>(new Lazy<List<T>>(() => Query<T>(name, param, db, key, isOutSql)));
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
            param = Parameter.ToDbParameter(param, config);
            if (config.IsUpdateCache)
                InstanceMap(key);

            if (DbCache.Exists(config.CacheType, name.ToLower()))
            {
                var sql = MapXml.GetMapSql(name, ref param, db, key);
                isOutSql = isOutSql ? isOutSql : IsMapLog(name);

                BaseAop.AopMapBefore(name, sql, param, config, AopType.Map_List_Dic);

                var result = FastRead.ExecuteSql(sql, param, db, key, isOutSql, false);

                if (MapXml.MapIsForEach(name, config))
                {
                    db = db == null ? ServiceContext.Engine.Resolve<IUnitOfWorK>().Contexts(key) : db;

                    for (var i = 1; i <= MapXml.MapForEachCount(name, config); i++)
                    {
                        result = MapXml.MapForEach(result, name, db, key, config, i);
                    }

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
        public static ValueTask<List<Dictionary<string, object>>> QueryAsy(string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            return new ValueTask<List<Dictionary<string, object>>>(Query(name, param, db, key, isOutSql));
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
        public static ValueTask<Lazy<List<Dictionary<string, object>>>> QueryLazyAsy(string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            return new ValueTask<Lazy<List<Dictionary<string, object>>>>(new Lazy<List<Dictionary<string, object>>>(() => Query(name, param, db, key, isOutSql)));
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
            param = Parameter.ToDbParameter(param, config);
            if (config.IsUpdateCache)
                InstanceMap(key);

            if (DbCache.Exists(config.CacheType, name.ToLower()))
            {
                var sql = MapXml.GetMapSql(name, ref param, db, key);
                BaseAop.AopMapBefore(name, sql, param, config, AopType.Map_Write);
                isOutSql = isOutSql ? isOutSql : IsMapLog(name);
                var result = FastWrite.ExecuteSql(sql, param, db, key, isOutSql, false);
                BaseAop.AopMapAfter(name, sql, param, config, AopType.Map_Write, result);
                return result;
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
        public static ValueTask<WriteReturn> WriteAsy(string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            return new ValueTask<WriteReturn>(Write(name, param, db, key, isOutSql));
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
        public static ValueTask<Lazy<WriteReturn>> WriteLazyAsy(string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
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
        private static PageResult ExecuteSqlPage(PageModel pModel, string sql, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            var result = new DataReturn();
            var config = DataConfig.Get(key);
            param = Parameter.ToDbParameter(param, config);
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            db = db == null ? ServiceContext.Engine.Resolve<IUnitOfWorK>().Contexts(key) : db;
            result = db.GetPageSql(pModel, sql, param, false);

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
            param = Parameter.ToDbParameter(param, config);
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
                    db = db == null ? ServiceContext.Engine.Resolve<IUnitOfWorK>().Contexts(key) : db;

                    for (var i = 1; i <= MapXml.MapForEachCount(name, config); i++)
                    {
                        result.list = MapXml.MapForEach(result.list, name, db, key, config, i);
                    }
                }

                BaseAop.AopMapAfter(name, sql, param, config, AopType.Map_Page_Dic, result.list);
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
        public static ValueTask<PageResult> QueryPageAsy(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            return new ValueTask<PageResult>(QueryPage(pModel, name, param, db, key, isOutSql));
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
        public static ValueTask<Lazy<PageResult>> QueryPageLazyAsy(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
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
        private static PageResult<T> ExecuteSqlPage<T>(PageModel pModel, string sql, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            var result = new DataReturn<T>();
            var config = DataConfig.Get(key);
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            param = Parameter.ToDbParameter(param, config);
            db = db == null ? ServiceContext.Engine.Resolve<IUnitOfWorK>().Contexts(key) : db;
            result = db.GetPageSql<T>(pModel, sql, param, false);

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
        public static PageResult<T> QueryPage<T>(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            key = key == null ? MapDb(name) : key;
            var config = db == null ? DataConfig.Get(key) : db.config;
            param = Parameter.ToDbParameter(param, config);
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
                    db = db == null ? ServiceContext.Engine.Resolve<IUnitOfWorK>().Contexts(key) : db;
                    for (var i = 1; i <= MapXml.MapForEachCount(name, config); i++)
                    {
                        result.list = MapXml.MapForEach<T>(result.list, name, db, config, i);
                    }
                }

                return result;
            }
            else
            {
                BaseAop.AopMapBefore(name, "", param, config, AopType.Map_Page_Model);
                return new PageResult<T>();
            }
        }
        #endregion

        #region maq 执行分页 asy
        /// <summary>
        /// 执行分页 asy
        /// </summary>
        public static ValueTask<PageResult<T>> QueryPageAsy<T>(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            return new ValueTask<PageResult<T>>(QueryPage<T>(pModel, name, param, db, key, isOutSql));
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
        public static ValueTask<Lazy<PageResult<T>>> QueryPageLazyAsy<T>(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            return new ValueTask<Lazy<PageResult<T>>>(new Lazy<PageResult<T>>(() => QueryPage<T>(pModel, name, param, db, key, isOutSql)));
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
            return MapXml.GetXmlList(info.FullName, "sqlMap", config).IsSuccess;
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
        public static string MapCheck(string name, string param)
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
        public static string MapExists(string name, string param)
        {
            return DbCache.Get(DataConfig.Get().CacheType, string.Format("{0}.{1}.existsmap", name.ToLower(), param.ToLower())).ToStr();
        }
        #endregion
    }
}