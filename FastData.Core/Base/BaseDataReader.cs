using System;
using System.Collections.Generic;
using System.Data.Common;
using FastData.Core.Property;
using FastData.Core.Type;
using FastData.Core.Model;
using FastData.Core.CacheModel;
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
        public static List<T> ToList<T>(DbDataReader dr, ConfigModel config, List<string> field = null) where T : class,new()
        {
            var list = new List<T>();
            var dynSet = new DynamicSet<T>();

            if (dr == null)
                return list;
            
            var propertyList = PropertyCache.GetPropertyInfo<T>(config.IsPropertyCache);

            while (dr.Read())
            {
                var item = new T();

                if (field == null || field.Count == 0)
                {
                    foreach (var info in propertyList)
                    {
                        if (info.PropertyType.IsGenericType && info.PropertyType.GetGenericTypeDefinition() != typeof(Nullable<>))
                            continue;

                        item = SetValue<T>(item, dynSet, dr, info, config);
                    }
                }
                else
                {
                    for (var i = 0; i < field.Count; i++)
                    {
                        if (propertyList.Exists(a => a.Name.ToLower() == field[i].ToLower()))
                        {
                            var info = propertyList.Find(a => a.Name.ToLower() == field[i].ToLower());
                            item = SetValue<T>(item, dynSet, dr, info, config);
                        }
                    }
                }

                list.Add(item);
            }
                                    
            return list;
        }
        #endregion 

        /// <summary>
        /// set value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="dynSet"></param>
        /// <param name="dr"></param>
        /// <param name="info"></param>
        /// <param name="config"></param>
        private static T SetValue<T>(T item ,DynamicSet<T> dynSet, DbDataReader dr, PropertyModel info, ConfigModel config)
        {
            try
            {
                var id = dr.GetOrdinal(config.DbType == DataDbType.Oracle ? info.Name.ToUpper() : info.Name);
                if (DataDbType.Oracle == config.DbType)
                {
                    object value = null;
                    var typeName = dr.GetDataTypeName(id).ToLower();
                    if (typeName == "clob" || typeName == "nclob")
                    {
                        dr.GetType().GetMethods().ToList().ForEach(m =>
                        {
                            if (m.Name == "GetOracleClob")
                            {
                                var param = new object[1];
                                param[0] = id;
                                var temp = m.Invoke(dr, param);
                                temp.GetType().GetMethods().ToList().ForEach(v =>
                                {
                                    if (v.Name == "get_Value" && !dr.IsDBNull(id))
                                        value = v.Invoke(temp, null);
                                });
                                temp.GetType().GetMethods().ToList().ForEach(v =>
                                {
                                    if (v.Name == "Close")
                                        v.Invoke(temp, null);
                                });
                                temp.GetType().GetMethods().ToList().ForEach(v =>
                                {
                                    if (v.Name == "Dispose")
                                        v.Invoke(temp, null);
                                });
                            }
                        });
                    }
                    else if (typeName == "blob")
                    {
                        dr.GetType().GetMethods().ToList().ForEach(m =>
                        {
                            if (m.Name == "GetOracleBlob")
                            {
                                var param = new object[1];
                                param[0] = id;
                                var temp = m.Invoke(dr, param);
                                temp.GetType().GetMethods().ToList().ForEach(v =>
                                {
                                    if (v.Name == "get_Value" && !dr.IsDBNull(id))
                                        value = v.Invoke(temp, null);
                                });
                                temp.GetType().GetMethods().ToList().ForEach(v =>
                                {
                                    if (v.Name == "Close")
                                        v.Invoke(temp, null);
                                });
                                temp.GetType().GetMethods().ToList().ForEach(v =>
                                {
                                    if (v.Name == "Dispose")
                                       v.Invoke(temp, null);
                                });
                            }
                        });
                    }
                    else
                        value = dr.GetValue(id);

                    if (!dr.IsDBNull(id))
                    {
                        if (info.PropertyType.Name == "Nullable`1" && info.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                            dynSet.SetValue(item, info.Name, Convert.ChangeType(value, Nullable.GetUnderlyingType(info.PropertyType)), config.IsPropertyCache);
                        else
                            dynSet.SetValue(item, info.Name, Convert.ChangeType(value, info.PropertyType), config.IsPropertyCache);
                    }
                }
                else
                {
                    if (!dr.IsDBNull(id))
                    {
                        if (info.PropertyType.Name == "Nullable`1" && info.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                            dynSet.SetValue(item, info.Name, Convert.ChangeType(dr.GetValue(id), Nullable.GetUnderlyingType(info.PropertyType)), config.IsPropertyCache);
                        else
                            dynSet.SetValue(item, info.Name, Convert.ChangeType(dr.GetValue(id), info.PropertyType), config.IsPropertyCache);
                    }
                }

                return item;
            }
            catch { return item; }
        }
    }   
}
