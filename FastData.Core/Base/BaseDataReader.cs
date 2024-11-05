using FastData.Core.CacheModel;
using FastData.Core.Model;
using FastData.Core.Property;
using FastData.Core.Type;
using FastUntility.Core.Base;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;
using System.Linq;

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
        public static List<T> ToList<T>(DbDataReader dr, ConfigModel config, List<string> field = null) where T : class, new()
        {
            var list = new List<T>();
            var colList = new List<string>();

            if (dr == null)
                return list;

            if (dr.HasRows)
                colList = GetCol(dr);

            var propertyList = PropertyCache.GetPropertyInfo<T>(config.IsPropertyCache);
            var dics = new List<Dictionary<string, object>>();

            while (dr.Read())
            {
                var item = new T();
                var dic = new Dictionary<string, object>();

                if (field == null || field.Count == 0)
                {
                    colList.ForEach(a =>
                    {
                        if (dr[a] is DBNull)
                            return;
                        else
                        {
                            var info = propertyList.Find(b => string.Compare(b.Name, a, true) == 0);

                            if (info == null)
                                return;

                            if (info.PropertyType.IsGenericType && info.PropertyType.GetGenericTypeDefinition() != typeof(Nullable<>))
                                return;

                            dic.Add(info.Name, dr[a]);
                        }
                    });
                }
                else
                {
                    colList.ForEach(a =>
                    {
                        if (dr[a] is DBNull)
                            return;
                        else
                        {
                            if (!field.Exists(b => string.Compare(a, b, true) == 0))
                                return;

                            var info = propertyList.Find(b => string.Compare(b.Name, a, true) == 0);

                            if (info == null)
                                return;

                            if (info.PropertyType.IsGenericType && info.PropertyType.GetGenericTypeDefinition() != typeof(Nullable<>))
                                return;

                            dic.Add(info.Name, dr[a]);
                        }
                    });
                }

                dics.Add(dic);
                //BaseEmit.Set(item, dic);
                //list.Add(item);
            }

            BaseEmit.Set<T>(list, dics);
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
        public static IList ToList(System.Type type, Object model, DbDataReader dr, ConfigModel config, List<string> field = null)
        {
            var list = Activator.CreateInstance(type);
            var colList = new List<string>();

            if (dr == null)
                return null;

            if (dr.HasRows)
                colList = GetCol(dr);

            var propertyList = PropertyCache.GetPropertyInfo(model, config.IsPropertyCache);
            var dics = new List<Dictionary<string, object>>();

            while (dr.Read())
            {
                var item = Activator.CreateInstance(model.GetType());
                var dic = new Dictionary<string, object>();

                if (field == null || field.Count == 0)
                {
                    colList.ForEach(a =>
                    {
                        if (dr[a] is DBNull)
                            return;
                        else
                        {
                            var info = propertyList.Find(b => string.Compare(b.Name, a, true) == 0);

                            if (info == null)
                                return;

                            if (info.PropertyType.IsGenericType && info.PropertyType.GetGenericTypeDefinition() != typeof(Nullable<>))
                                return;

                            dic.Add(info.Name, dr[a]);
                        }
                    });
                }
                else
                {
                    colList.ForEach(a =>
                    {
                        if (dr[a] is DBNull)
                            return;
                        else
                        {
                            if (!field.Exists(b => string.Compare(a, b, true) == 0))
                                return;

                            var info = propertyList.Find(b => string.Compare(b.Name, a, true) == 0);

                            if (info == null)
                                return;

                            if (info.PropertyType.IsGenericType && info.PropertyType.GetGenericTypeDefinition() != typeof(Nullable<>))
                                return;

                            dic.Add(info.Name, dr[a]);
                        }
                    });
                }

                //BaseEmit.Set(item, dic);
                //list.GetType().GetMethods().ToList().ForEach(m =>
                //{
                //    if (m.Name == "Add")
                //        BaseEmit.Invoke(list, m, new object[] { item });
                //});
                dics.Add(dic);
            }

            BaseEmit.Set(model.GetType(), list, dics);
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
                var dic = new Dictionary<string, object>();
                if (field == null || field.Count == 0)
                {
                    colList.ForEach(a =>
                    {
                        if (dr[a] is DBNull)
                            return;
                        else
                        {
                            var info = propertyList.Find(b => string.Compare(b.Name, a, true) == 0);

                            if (info == null)
                                return;

                            if (info.PropertyType.IsGenericType && info.PropertyType.GetGenericTypeDefinition() != typeof(Nullable<>))
                                return;

                            dic.Add(info.Name, dr[a]);
                        }
                    });
                }
                else
                {
                    colList.ForEach(a =>
                    {
                        if (dr[a] is DBNull)
                            return;
                        else
                        {
                            if (!field.Exists(b => string.Compare(a, b, true) == 0))
                                return;

                            var info = propertyList.Find(b => string.Compare(b.Name, a, true) == 0);

                            if (info == null)
                                return;

                            if (info.PropertyType.IsGenericType && info.PropertyType.GetGenericTypeDefinition() != typeof(Nullable<>))
                                return;

                            dic.Add(info.Name, dr[a]);
                        }
                    });
                }
                BaseEmit.Set(result, dic);
            }

            return result;
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
        //private static Object SetValue(Object item, DbDataReader dr, PropertyModel info, ConfigModel config)
        //{
        //    try
        //    {
        //        var colName = config.DbType == DataDbType.Oracle ? info.Name.ToUpper() : info.Name;
        //        var id = dr.GetOrdinal(colName);
        //        if (DataDbType.Oracle == config.DbType)
        //            ReadOracle(item, dr, id, info);
        //        else if (!dr.IsDBNull(id))
        //            BaseEmit.Set(item, info.Name, dr.GetValue(id));

        //        return item;
        //    }
        //    catch
        //    {
        //        return item;
        //    }
        //}
        #endregion

        //private static void ReadOracle(Object item, DbDataReader dr, int id, PropertyModel info)
        //{
        //    object value = null;
        //    var typeName = dr.GetDataTypeName(id);
        //    if (string.Compare(typeName, "clob", true) == 0 || string.Compare(typeName, "nclob", true) == 0)
        //    {
        //        var temp = BaseEmit.Invoke(dr, dr.GetType().GetMethod("GetOracleClob"), new object[] { id });
        //        if (temp != null)
        //        {
        //            value = BaseEmit.Get(temp, "Value");
        //            BaseEmit.Invoke(temp, temp.GetType().GetMethod("Close"), null);
        //            BaseEmit.Invoke(temp, temp.GetType().GetMethod("Dispose"), null);
        //        }
        //    }
        //    else if (string.Compare(typeName, "blob", true) == 0)
        //    {
        //        var temp = BaseEmit.Invoke(dr, dr.GetType().GetMethod("GetOracleBlob"), new object[] { id });
        //        if (temp != null)
        //        {
        //            value = BaseEmit.Get(temp, "Value");
        //            BaseEmit.Invoke(temp, temp.GetType().GetMethod("Close"), null);
        //            BaseEmit.Invoke(temp, temp.GetType().GetMethod("Dispose"), null);
        //        }
        //    }
        //    else
        //        value = dr.GetValue(id);

        //    if (!dr.IsDBNull(id))
        //        BaseEmit.Set(item, info.Name, value);
        //}

        #region get datareader col
        private static List<string> GetCol(DbDataReader dr)
        {
            var list = new List<string>();
            for (var i = 0; i < dr.FieldCount; i++)
            {
                var colName = dr.GetName(i);
                if (!list.Exists(a => string.Compare(a, colName, true) == 0))
                    list.Add(colName);
            }
            return list;
        }
        #endregion
    }
}
