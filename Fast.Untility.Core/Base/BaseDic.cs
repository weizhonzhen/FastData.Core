using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Fast.Untility.Core.Base
{
    /// <summary>
    /// 动态属性setvalue
    /// </summary>
    internal class DynamicSet<T>
    {
        private static bool IsSetCache;
        private static Action<object, string, object> SetValueDelegate;

        // 构建函数        
        static DynamicSet()
        {
            SetValueDelegate = GenerateSetValue();
        }

        #region 动态setvalue
        /// <summary>
        /// 动态setvalue
        /// </summary>
        /// <param name="instance">类型</param>
        /// <param name="memberName">成员</param>
        /// <param name="newValue">值</param>
        public void SetValue(T instance, string memberName, object newValue, bool IsCache)
        {
            IsSetCache = IsCache;
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
            foreach (var propertyInfo in BaseDic.PropertyInfo<T>(IsSetCache))
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
    internal class DynamicGet<T>
    {
        private static bool IsGetCache;
        private static Func<object, string, object> GetValueDelegate;

        // 构建函数        
        static DynamicGet()
        {
            GetValueDelegate = GenerateGetValue();
        }

        #region 动态getvalue
        /// <summary>
        /// 动态getvalue
        /// </summary>
        /// <param name="instance">类型</param>
        /// <param name="memberName">成员</param>
        /// <returns></returns>
        public object GetValue(object instance, string memberName, bool IsCache)
        {
            IsGetCache = IsCache;
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

            foreach (var propertyInfo in BaseDic.PropertyInfo<T>(IsGetCache))
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
        public static T DicToModel<T>(Dictionary<string, object> dic, bool isCache=true) where T : class, new()
        {
            var result = new T();

            foreach (var item in PropertyInfo<T>(isCache))
            {
                    var info = new DynamicSet<T>();
                if (dic.ContainsKey(item.Name.ToLower()) && !string.IsNullOrEmpty(dic[item.Name.ToLower()].ToStr()))
                    info.SetValue(result, item.Name, Convert.ChangeType(dic[item.Name.ToLower()], item.PropertyType), isCache);
            }
            
            return result;
        }
        #endregion

        #region T to dic
        /// <summary>
        ///  T to dic
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, object> ModelToDic<T>(T model, bool isCache = true) where T : class, new()
        {
            var dic = new Dictionary<string, object>();

            foreach (var item in PropertyInfo<T>(isCache))
            {
                    var info = new DynamicGet<T>();
                    dic.Add(item.Name, info.GetValue(model, item.Name, isCache));
            }
            
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
        public static List<T> DicToModel<T>(List<Dictionary<string, object>> dic, bool isCache=true) where T : class, new()
        {
            var result = new List<T>();

            foreach (var item in dic)
            {
                    result.Add(DicToModel<T>(item, isCache));
            }
            
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
            var key = typeof(T).Namespace + "." + typeof(T).Name;

            if (IsCache)
            {
                //if (BaseCache.Exists(key))
                //    return BaseCache.Get<List<PropertyInfo>>(key);
                //else
                //{
                    var info = typeof(T).GetProperties().ToList();

                    //BaseCache.Set<List<PropertyInfo>>(key, info);
                    return info;
                //}
            }
            else
            {
                //BaseCache.Clear(key);
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

            foreach (var temp in item.Keys)
            {
                if (temp.ToLower() == key.ToLower())
                    return item[temp];
            }

            return "";
        }

        public static void SetValue(this Dictionary<string, object> item, string key, object value)
        {

            if (string.IsNullOrEmpty(key))
                return;

            if (item == null)
                return;

            foreach (var temp in item.Keys)
            {
                if (temp.ToLower() == key.ToLower())
                {
                    item[temp] = value;
                    return;
                }
            }
        }
    }
}