using System;
using System.Collections.Generic;
using System.Data.Common;
using FastData.Core.Property;
using FastData.Core.Type;
using FastData.Core.Model;
using FastUntility.Core.Base;

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
                        try
                        {
                            var id = dr.GetOrdinal(config.DbType == DataDbType.Oracle ? info.Name.ToUpper() : info.Name);

                            if (!dr.IsDBNull(id))
                            {
                                if (info.PropertyType.Name == "Nullable`1" && info.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                    dynSet.SetValue(item, info.Name, Convert.ChangeType(dr.GetValue(id), Nullable.GetUnderlyingType(info.PropertyType)), config.IsPropertyCache);
                                else
                                    dynSet.SetValue(item, info.Name, Convert.ChangeType(dr.GetValue(id), info.PropertyType), config.IsPropertyCache);
                            }
                        }
                        catch { }
                    }
                }
                else
                {
                    for (var i = 0; i < field.Count; i++)
                    {
                        var id = dr.GetOrdinal(config.DbType == DataDbType.Oracle ? field[i].ToUpper() : field[i]);

                        try
                        {
                            if (!dr.IsDBNull(id))
                            {
                                var info = propertyList.Find(a => a.Name.ToLower() == field[i].ToLower());
                                if (info.PropertyType.Name == "Nullable`1" && info.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                    dynSet.SetValue(item, info.Name, Convert.ChangeType(dr.GetValue(id), Nullable.GetUnderlyingType(info.PropertyType)), config.IsPropertyCache);
                                else
                                    dynSet.SetValue(item, field[i], Convert.ChangeType(dr.GetValue(id), info.PropertyType), config.IsPropertyCache);
                            }
                        }
                        catch { }
                    }
                }

                list.Add(item);
            }

            dr.Close();
            dr.Dispose();
            
            return list;
        }
        #endregion 
    }   
}
