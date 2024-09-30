using FastUntility.Core.Base;
using FastUntility.Core.Cache;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace FastUntility.Core.Base
{
    /// <summary>
    /// dic to t/list
    /// </summary>
    public static class BaseDic
    {
        #region dic to T
        /// <summary>
        ///  dic to T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static T DicToModel<T>(Dictionary<string, object> dic) where T : class, new()
        {
            var result = new T();
            PropertyInfo<T>().ForEach(a =>
            {
                if (dic.Keys.ToList().Exists(d => string.Compare(d, a.Name, true) == 0 && dic[d].ToStr() != ""))
                {
                    var key = dic.Keys.ToList().Find(d => string.Compare(d, a.Name, true) == 0 && dic[d].ToStr() != "");
                    BaseEmit.Set(result, a.Name, dic[key]);
                }
            });

            return result;
        }
        #endregion

        #region T to dic
        /// <summary>
        ///  T to dic
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, object> ModelToDic<T>(T model) where T : class, new()
        {
            var dic = new Dictionary<string, object>();
            PropertyInfo<T>().ForEach(a =>
            {
                dic.Add(a.Name, BaseEmit.Get<T>(model, a.Name));
            });

            return dic;
        }
        #endregion

        #region List<dic> to List<T>
        /// <summary>
        ///  dic to T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static List<T> DicToModel<T>(List<Dictionary<string, object>> dic) where T : class, new()
        {
            var result = new List<T>();

            dic.ForEach(a =>
            {
                result.Add(DicToModel<T>(a));
            });

            return result;
        }
        #endregion

        #region 泛型缓存属性成员
        /// <summary>
        /// 泛型缓存属性成员
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static List<PropertyInfo> PropertyInfo<T>(bool IsCache = true)
        {
            var key = string.Format("{0}.to.{1}", typeof(T).Namespace, typeof(T).Name);

            if (IsCache)
            {
                if (BaseCache.Exists(key))
                    return BaseCache.Get<List<PropertyInfo>>(key);
                else
                {
                    var info = typeof(T).GetProperties().ToList();

                    BaseCache.Set<List<PropertyInfo>>(key, info);
                    return info;
                }
            }
            else
            {
                return typeof(T).GetProperties().ToList();
            }
        }
        #endregion

        #region 泛型缓存属性成员
        /// <summary>
        /// 泛型缓存属性成员
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static List<PropertyInfo> PropertyInfo(object model,bool IsCache = true)
        {
            var key = string.Format("{0}.to.{1}", model.GetType().Namespace, model.GetType().Name);

            if (IsCache)
            {
                if (BaseCache.Exists(key))
                    return BaseCache.Get<List<PropertyInfo>>(key);
                else
                {
                    var info = model.GetType().GetProperties().ToList();

                    BaseCache.Set<List<PropertyInfo>>(key, info);
                    return info;
                }
            }
            else
            {
                return typeof(T).GetProperties().ToList();
            }
        }
        #endregion
    }
}



namespace System.Collections.Generic
{
    public static class Dic
    {
        public static Object GetValue(this Dictionary<string, object> item, string key)
        {
            if (string.IsNullOrEmpty(key))
                return "";

            if (item == null)
                return "";

            key = item.Keys.ToList().Find(a => string.Compare(a, key, true) == 0);

            if (string.IsNullOrEmpty(key))
                return "";
            else
                return item[key];
        }

        public static Dictionary<string, object> SetValue(this Dictionary<string, object> item, string key, object value)
        {
            if (string.IsNullOrEmpty(key))
                return item;

            if (item == null)
                return item;

            if (item.Keys.ToList().Exists(a => string.Compare(a, key, true) == 0))
                item[key] = value;
            else
                item.Add(key, value);

            return item;
        }
    }
}
