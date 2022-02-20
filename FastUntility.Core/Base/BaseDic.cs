using FastUntility.Core.Base;
using FastUntility.Core.Cache;
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
            PropertyInfo<T>().ForEach(a => {
                if (dic.ContainsKey(a.Name.ToLower()) && !string.IsNullOrEmpty(dic[a.Name.ToLower()].ToStr()))
                    BaseEmit.Set(result, a.Name, dic[a.Name.ToLower()]);
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
            PropertyInfo<T>().ForEach(a => {
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

            dic.ForEach(a => {
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
    }


    /// <summary>
    /// 动态属性setvalue
    /// </summary>
    public class DynamicSet<T>
    {
        private static Action<object, string, object> SetValueDelegate;

        // 构建函数        
        static DynamicSet()
        {
            var key = string.Format("DynamicSet<T>.{0}.{1}", typeof(T)?.Namespace, typeof(T).Name);
            if (!BaseCache.Exists(key))
            {
                SetValueDelegate = GenerateSetValue();
                BaseCache.Set<object>(key, SetValueDelegate);
            }
            else
                SetValueDelegate = BaseCache.Get<object>(key) as Action<object, string, object>;
        }

        #region 动态setvalue
        /// <summary>
        /// 动态setvalue
        /// </summary>
        /// <param name="instance">类型</param>
        /// <param name="memberName">成员</param>
        /// <param name="newValue">值</param>
        public void SetValue(T instance, string memberName, object newValue)
        {
            SetValueDelegate(instance, memberName, newValue);
        }
        #endregion

        #region 动态生成setvalue
        /// <summary>
        /// 动态生成setvalue
        /// </summary>
        /// <returns></returns>
        private static Action<object, string, object> GenerateSetValue()
        {
            var instance = Expression.Parameter(typeof(object), "instance");
            var memberName = Expression.Parameter(typeof(string), "memberName");
            var newValue = Expression.Parameter(typeof(object), "newValue");
            var nameHash = Expression.Variable(typeof(int), "nameHash");
            var calHash = Expression.Assign(nameHash, Expression.Call(memberName, typeof(object).GetMethod("GetHashCode")));
            var cases = new List<SwitchCase>();
            foreach (var propertyInfo in BaseDic.PropertyInfo<T>())
            {
                var property = Expression.Property(Expression.Convert(instance, typeof(T)), propertyInfo.Name);
                var setValue = Expression.Assign(property, Expression.Convert(newValue, propertyInfo.PropertyType));
                var propertyHash = Expression.Constant(propertyInfo.Name.GetHashCode(), typeof(int));
                cases.Add(Expression.SwitchCase(Expression.Convert(setValue, typeof(object)), propertyHash));
            }
            var switchEx = Expression.Switch(nameHash, Expression.Constant(null), cases.ToArray());
            var methodBody = Expression.Block(typeof(object), new[] { nameHash }, calHash, switchEx);

            return Expression.Lambda<Action<object, string, object>>(methodBody, instance, memberName, newValue).Compile();
        }
        #endregion
    }

    /// <summary>
    /// 动态属性getvalue
    /// </summary>
    public class DynamicGet<T>
    {
        private static Func<object, string, object> GetValueDelegate;

        // 构建函数        
        static DynamicGet()
        {
            var key = string.Format("DynamicGet<T>.{0}.{1}", typeof(T)?.Namespace, typeof(T).Name);
            if (!BaseCache.Exists(key))
            {
                GetValueDelegate = GenerateGetValue();
                BaseCache.Set<object>(key, GetValueDelegate);
            }
            else
                GetValueDelegate = BaseCache.Get<object>(key) as Func<object, string, object>;
        }

        #region 动态getvalue
        /// <summary>
        /// 动态getvalue
        /// </summary>
        /// <param name="instance">类型</param>
        /// <param name="memberName">成员</param>
        /// <returns></returns>
        public object GetValue(T instance, string memberName)
        {
            return GetValueDelegate(instance, memberName);
        }
        #endregion

        #region 动态生成getvalue
        /// <summary>
        /// 动态生成getvalue
        /// </summary>
        /// <returns></returns>
        private static Func<object, string, object> GenerateGetValue()
        {
            var instance = Expression.Parameter(typeof(object), "instance");
            var memberName = Expression.Parameter(typeof(string), "memberName");
            var nameHash = Expression.Variable(typeof(int), "nameHash");
            var calHash = Expression.Assign(nameHash, Expression.Call(memberName, typeof(object).GetMethod("GetHashCode")));
            var cases = new List<SwitchCase>();

            foreach (var propertyInfo in BaseDic.PropertyInfo<T>())
            {
                var property = Expression.Property(Expression.Convert(instance, typeof(T)), propertyInfo.Name);
                var propertyHash = Expression.Constant(propertyInfo.Name.GetHashCode(), typeof(int));
                cases.Add(Expression.SwitchCase(Expression.Convert(property, typeof(object)), propertyHash));
            }

            var switchEx = Expression.Switch(nameHash, Expression.Constant(null), cases.ToArray());
            var methodBody = Expression.Block(typeof(object), new[] { nameHash }, calHash, switchEx);

            return Expression.Lambda<Func<object, string, object>>(methodBody, instance, memberName).Compile();
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

            key = item.Keys.ToList().Find(a =>string.Compare( a, key,false)==0);

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
            
            if (item.Keys.ToList().Exists(a => string.Compare( a, key,false)==0))
                item[key] = value;
            else
                item.Add(key, value);

            return item;
        }
    }
}
