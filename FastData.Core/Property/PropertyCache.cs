using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FastData.Core.CacheModel;
using FastData.Core.Base;
using FastUntility.Core.Base;

namespace FastData.Core.Property
{
    /// <summary>
    /// 缓存类
    /// </summary>
    internal static class PropertyCache
    {
        #region 泛型缓存属性成员
        /// <summary>
        /// 泛型缓存属性成员
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<PropertyModel> GetPropertyInfo<T>(bool IsCache = true)
        {
            var config = DataConfig.Get();
            var list = new List<PropertyModel>();
            var key = string.Format("{0}.{1}", typeof(T).Namespace, typeof(T).Name);

            if (IsCache)
            {
                if (DbCache.Exists(config.CacheType,key))
                    return DbCache.Get<List<PropertyModel>>(config.CacheType, key);
                else
                {
                    typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList().ForEach(a =>
                      {
                          if (!a.GetMethod.IsVirtual)
                          {
                              var temp = new PropertyModel();
                              temp.Name = a.Name;
                              temp.PropertyType = a.PropertyType;
                              list.Add(temp);
                          }
                      });

                    DbCache.Set<List<PropertyModel>>(config.CacheType, key, list);
                }
            }
            else
            {
                typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList().ForEach(a =>
                {
                    if (!a.GetMethod.IsVirtual)
                    {
                        var temp = new PropertyModel();
                        temp.Name = a.Name;
                        temp.PropertyType = a.PropertyType;
                        list.Add(temp);
                    }
                });
            }

            return list;
        }
        #endregion

        #region 缓存发属性成员
        public static List<PropertyModel> GetPropertyInfo(object model,bool IsCache=true)
        {
            var config = DataConfig.Get();
            var list = new List<PropertyModel>();
            var key = string.Format("{0}.{1}", model.GetType().Namespace, model.GetType().Name);

            if (IsCache)
            {
                if (DbCache.Exists(config.CacheType, key))
                    return DbCache.Get<List<PropertyModel>>(config.CacheType, key);
                else
                {
                    model.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList().ForEach(a =>
                    {
                        if (!a.GetMethod.IsVirtual)
                        {
                            var temp = new PropertyModel();
                            temp.Name = a.Name;
                            temp.PropertyType = a.PropertyType;
                            list.Add(temp);
                        }
                    });

                    DbCache.Set<List<PropertyModel>>(config.CacheType, key, list);
                }
            }
            else
            {
                model.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList().ForEach(a =>
                {
                    if (!a.GetMethod.IsVirtual)
                    {
                        var temp = new PropertyModel();
                        temp.Name = a.Name;
                        temp.PropertyType = a.PropertyType;
                        list.Add(temp);
                    }
                });
            }

            return list;
        }
        #endregion

        #region 泛型特性成员
        /// <summary>
        /// 泛型特性成员
        /// </summary>
        public static List<ColumnModel> GetAttributesColumnInfo(string tableName, List<PropertyInfo> ListInfo)
        {
            var list = new List<ColumnModel>();

            ListInfo.ForEach(a => {
                var temp = new ColumnModel();
                temp.Name = a.Name;
                var paramList = GetPropertyInfo<ColumnModel>(true);

                a.CustomAttributes.ToList().ForEach(b => {
                    if (b.AttributeType.Name == typeof(ColumnAttribute).Name)
                    {
                        b.NamedArguments.ToList().ForEach(c => {
                            if (paramList.Exists(p => string.Compare( p.Name, c.MemberName,false)==0))
                                BaseEmit.Set(temp, c.MemberName, c.TypedValue.Value);
                        });
                    }
                });

                if (temp.IsKey && temp.IsNull)
                    temp.IsNull = false;

                list.Add(temp);
            });

            return list;
        }
        #endregion

        #region 泛型缓存特性成员
        /// <summary>
        /// 泛型缓存特性成员
        /// </summary>
        public static string GetAttributesTableInfo(List<Attribute> listAttribute)
        {
            var result = "";

            listAttribute.ForEach(a => {
                if (a is TableAttribute)
                    result = (a as TableAttribute).Comments;
            });

            return result;
        }
        #endregion
    }
}
