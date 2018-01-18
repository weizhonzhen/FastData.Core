using System;
using System.Collections.Generic;
using System.Data.Common;
using Data.Core.Property;
using Data.Core.Type;
using Data.Core.Model;

namespace Data.Core.Base
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
                                if (dr.GetValue(id) != DBNull.Value)
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
                                if (dr.GetValue(id) != DBNull.Value)
                                    dynSet.SetValue(item, field[i], Convert.ChangeType(dr.GetValue(id)
                                        , propertyList.Find(a => a.Name.ToLower() == field[i].ToLower()).PropertyType), config.IsPropertyCache);
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
