using System;
using System.Collections.Generic;
using System.Data.Common;
using FastData.Core.Property;
using FastData.Core.Type;
using FastData.Core.Model;
using FastData.Core.CacheModel;
using System.Linq;
using System.Collections;
using FastUntility.Core.Base;
using System.Dynamic;
using System.Threading;

namespace FastData.Core.Base
{
    /// <summary>
    /// datareader操作类
    /// </summary>
    internal static class BaseDataReader
    {
        #region to list
        /// <summary>
        ///  to list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static List<T> ToList<T>(DbDataReader dr, ConfigModel config, List<string> field = null) where T : class,new()
        {
            var list = new List<T>();
            var colList = new List<string>();

            if (dr == null)
                return list;

            if (dr.HasRows)
                colList = GetCol(dr);

            var propertyList = PropertyCache.GetPropertyInfo<T>(config.IsPropertyCache);

            while (dr.Read())
            {
                var item = new T();

                if (field == null || field.Count == 0)
                {
                    foreach (var info in propertyList)
                    {
                        if (!colList.Exists(a => string.Compare(a,info.Name,true)==0))
                            continue;

                        if (info.PropertyType.IsGenericType && info.PropertyType.GetGenericTypeDefinition() != typeof(Nullable<>))
                            continue;

                        item = SetValue<T>(item, dr, info, config);
                    }
                }
                else
                {
                    for (var i = 0; i < field.Count; i++)
                    {
                        if (!colList.Exists(a => string.Compare(a, field[i], true) == 0))
                            continue;

                        if (propertyList.Exists(a => string.Compare(a.Name, field[i], true) ==0))
                        {
                            var info = propertyList.Find(a => string.Compare(a.Name, field[i], true) == 0);
                            item = SetValue<T>(item, dr, info, config);
                        }
                    }
                }

                list.Add(item);
            }
                                    
            return list;
        }
        #endregion

        #region to dyns
        /// <summary>
        ///  to dyns
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static List<dynamic> ToDyns(DbDataReader dr, ConfigModel config)
        {
            List<dynamic> list = new List<dynamic>();
            var colList = new List<string>();

            if (dr == null)
                return list;

            if (dr.HasRows)
                colList = GetCol(dr);

            while (dr.Read())
            {
                dynamic item = new ExpandoObject();
                var dic = (IDictionary<string, object>)item;

                foreach (var key in colList)
                {
                    dic[key] = dr[key];
                }

                list.Add(item);
            }

            return list;
        }
        #endregion


        #region to list
        /// <summary>
        ///  to list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static IList ToList(System.Type type,Object model, DbDataReader dr, ConfigModel config, List<string> field = null)
        {
            var list = Activator.CreateInstance(type);
            var colList = new List<string>();

            if (dr == null)
                return null;

            if (dr.HasRows)
                colList = GetCol(dr);

            var propertyList = PropertyCache.GetPropertyInfo(model,config.IsPropertyCache);

            while (dr.Read())
            {
                var item = Activator.CreateInstance(model.GetType());

                if (field == null || field.Count == 0)
                {
                    foreach (var info in propertyList)
                    {
                        if (!colList.Exists(a => string.Compare(a,info.Name, true) ==0))
                            continue;

                        if (info.PropertyType.IsGenericType && info.PropertyType.GetGenericTypeDefinition() != typeof(Nullable<>))
                            continue;

                        item = SetValue(item, dr, info, config);
                    }
                }
                else
                {
                    for (var i = 0; i < field.Count; i++)
                    {
                        if (!colList.Exists(a => string.Compare(a,field[i], true) ==0))
                            continue;

                        if (propertyList.Exists(a => string.Compare(a.Name, field[i], true) == 0))
                        {
                            var info = propertyList.Find(a => string.Compare(a.Name, field[i], true) == 0);
                            item = SetValue(item, dr, info, config);
                        }
                    }
                }

                list.GetType().GetMethods().ToList().ForEach(m =>
                {
                    if (m.Name == "Add")
                        BaseEmit.Invoke(list, m, new object[] { item });
                });
            }

            return (IList)list;
        }
        #endregion

        #region to model
        /// <summary>
        /// to model
        /// </summary>
        /// <param name="model"></param>
        /// <param name="dr"></param>
        /// <param name="config"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static Object ToModel(Object model, DbDataReader dr, ConfigModel config, List<string> field = null)
        {
            var result = Activator.CreateInstance(model.GetType());
            var colList = new List<string>();

            if (dr == null)
                return null;

            if (dr.HasRows)
                colList = GetCol(dr);

            var propertyList = PropertyCache.GetPropertyInfo(model, config.IsPropertyCache);

            while (dr.Read())
            {
                if (field == null || field.Count == 0)
                {
                    foreach (var info in propertyList)
                    {
                        if (!colList.Exists(a => string.Compare(a,info.Name, true) ==0))
                            continue;

                        if (info.PropertyType.IsGenericType && info.PropertyType.GetGenericTypeDefinition() != typeof(Nullable<>))
                            continue;

                        result = SetValue(result, dr, info, config);
                    }
                }
                else
                {
                    for (var i = 0; i < field.Count; i++)
                    {
                        if (!colList.Exists(a => string.Compare(a, field[i], true) == 0))
                            continue;

                        if (propertyList.Exists(a => string.Compare(a.Name, field[i], true) == 0))
                        {
                            var info = propertyList.Find(a => string.Compare(a.Name, field[i], true) == 0);
                            result = SetValue(result, dr, info, config);
                        }
                    }
                }
            }

            return result;
        }
        #endregion

        #region set value T
        /// <summary>
        /// set value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="dynSet"></param>
        /// <param name="dr"></param>
        /// <param name="info"></param>
        /// <param name="config"></param>
        private static T SetValue<T>(T item, DbDataReader dr, PropertyModel info, ConfigModel config)
        {
            try
            {
                var colName = config.DbType == DataDbType.Oracle ? info.Name.ToUpper() : info.Name;
                var id = dr.GetOrdinal(colName);
                if (DataDbType.Oracle == config.DbType)
                    ReadOracle(item, dr, id, info);
                else if (!dr.IsDBNull(id))
                    BaseEmit.Set(item, info.Name, dr.GetValue(id));

                return item;
            }
            catch { return item; }
        }
        #endregion

        #region set value model
        /// <summary>
        /// set value
        /// </summary>
        /// <param name="model"></param>
        /// <param name="dynSet"></param>
        /// <param name="dr"></param>
        /// <param name="info"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        private static Object SetValue(Object item, DbDataReader dr, PropertyModel info, ConfigModel config)
        {
            try
            {
                var colName = config.DbType == DataDbType.Oracle ? info.Name.ToUpper() : info.Name;
                var id = dr.GetOrdinal(colName);
                if (DataDbType.Oracle == config.DbType)
                    ReadOracle(item, dr, id, info);
                else if (!dr.IsDBNull(id))
                    BaseEmit.Set(item, info.Name, dr.GetValue(id));

                return item;
            }
            catch
            {
                return item;
            }
        }
        #endregion

        private static void ReadOracle(Object item,DbDataReader dr, int id, PropertyModel info)
        {
            object value = null;
            var typeName = dr.GetDataTypeName(id);
            if (string.Compare(typeName,"clob", true) ==0 || string.Compare(typeName, "nclob", true) == 0)
            {
                var temp = BaseEmit.Invoke(dr, dr.GetType().GetMethod("GetOracleClob"), new object[] { id });
                if (temp != null)
                {
                    value = BaseEmit.Get(temp, "Value");
                    BaseEmit.Invoke(temp, temp.GetType().GetMethod("Close"), null);
                    BaseEmit.Invoke(temp, temp.GetType().GetMethod("Dispose"), null);
                }
            }
            else if (string.Compare(typeName, "blob", true) == 0)
            {
                var temp = BaseEmit.Invoke(dr, dr.GetType().GetMethod("GetOracleBlob"), new object[] { id });
                if (temp != null)
                {
                    value = BaseEmit.Get(temp, "Value");
                    BaseEmit.Invoke(temp, temp.GetType().GetMethod("Close"), null);
                    BaseEmit.Invoke(temp, temp.GetType().GetMethod("Dispose"), null);
                }
            }
            else
                value = dr.GetValue(id);

            if (!dr.IsDBNull(id))
                BaseEmit.Set(item, info.Name, value);
        }

        #region get datareader col
        private static List<string> GetCol(DbDataReader dr)
        {
            var list = new List<string>();
            for (var i = 0; i < dr.FieldCount; i++)
            {
                list.Add(dr.GetName(i));
            }
            list.Distinct();
            return list;
        }
        #endregion
    }
}
