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
            var param = new Dictionary<string, object>();
            PropertyInfo<T>().ForEach(a =>
            {
                if (dic.Keys.ToList().Exists(d => string.Compare(d, a.Name, true) == 0 && dic[d].ToStr() != ""))
                {
                    var key = dic.Keys.ToList().Find(d => string.Compare(d, a.Name, true) == 0 && dic[d].ToStr() != "");
                    param.Add(a.Name, dic[key]);
                }
            });

            BaseEmit.Set(result, param);
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
        public static List<PropertyInfo> PropertyInfo(object model, bool IsCache = true)
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


    public static class BaseDyn
    {
        #region dyn to T
        /// <summary>
        ///  dyn to T
        /// </summary>
        /// <returns></returns>
        public static T DynToModel<T>(dynamic dyn) where T : class, new()
        {
            var result = new T();

            if (dyn != null && dyn is IDictionary<string, object>)
            {
                var param = new Dictionary<string, object>();
                var dic = dyn as IDictionary<string, object>;
                BaseDic.PropertyInfo<T>().ForEach(a =>
                {
                    if (dic.Keys.ToList().Exists(d => string.Compare(d, a.Name, true) == 0 && dic[d].ToStr() != ""))
                    {
                        var key = dic.Keys.ToList().Find(d => string.Compare(d, a.Name, true) == 0 && dic[d].ToStr() != "");
                        param.Add(key, dic[key]);
                    }
                });

                BaseEmit.Set(result, param);
            }
            return result;
        }
        #endregion

        #region List<dyn> to List<T>
        /// <summary>
        ///  List<dyn> to List<T>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static List<T> DynToModel<T>(List<dynamic> dic) where T : class, new()
        {
            var result = new List<T>();

            dic.ForEach(a =>
            {
                result.Add(DynToModel<T>(a));
            });

            return result;
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
        public DynamicSet(string name)
        {
            var key = string.Format("DynamicSet<T>.{0}.{1}.{2}", typeof(T)?.Namespace, typeof(T).Name, name);
            if (!BaseCache.Exists(key))
            {
                SetValueDelegate = GenerateSetValue(name);
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
        private static Action<object, string, object> GenerateSetValue(string name)
        {
            var instance = Expression.Parameter(typeof(object), "instance");
            var memberName = Expression.Parameter(typeof(string), "memberName");
            var newValue = Expression.Parameter(typeof(object), "newValue");
            var nameHash = Expression.Variable(typeof(int), "nameHash");
            var calHash = Expression.Assign(nameHash, Expression.Call(memberName, typeof(object).GetMethod("GetHashCode")));
            var cases = new List<SwitchCase>();
            foreach (var propertyInfo in BaseDic.PropertyInfo<T>())
            {
                if (propertyInfo.Name.ToLower() == name.ToLower())
                {
                    var property = Expression.Property(Expression.Convert(instance, typeof(T)), propertyInfo.Name);
                    var setValue = Expression.Assign(property, Expression.Convert(newValue, propertyInfo.PropertyType));
                    var propertyHash = Expression.Constant(propertyInfo.Name.GetHashCode(), typeof(int));
                    cases.Add(Expression.SwitchCase(Expression.Convert(setValue, typeof(object)), propertyHash));
                }
            }
            var switchEx = Expression.Switch(nameHash, Expression.Constant(null), cases.ToArray());
            var methodBody = Expression.Block(typeof(object), new[] { nameHash }, calHash, switchEx);

            return Expression.Lambda<Action<object, string, object>>(methodBody, instance, memberName, newValue).Compile();
        }
        #endregion
    }


    /// <summary>
    /// 动态属性setvalue
    /// </summary>
    public class DynamicSet
    {
        private static Action<object, string, object> SetValueDelegate;

        public DynamicSet(object model, string name)
        {
            var key = string.Format("DynamicSet.{0}.{1}.{2}", model.GetType()?.Namespace, model.GetType().Name, name);
            if (!BaseCache.Exists(key))
            {
                SetValueDelegate = GenerateSetValue(model, name);
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
        public void SetValue(object instance, string memberName, object newValue)
        {
            SetValueDelegate(instance, memberName, newValue);
        }
        #endregion

        #region 动态生成setvalue
        /// <summary>
        /// 动态生成setvalue
        /// </summary>
        /// <returns></returns>
        private static Action<object, string, object> GenerateSetValue(object model, string name)
        {
            var instance = Expression.Parameter(typeof(object), "instance");
            var memberName = Expression.Parameter(typeof(string), "memberName");
            var newValue = Expression.Parameter(typeof(object), "newValue");
            var nameHash = Expression.Variable(typeof(int), "nameHash");
            var calHash = Expression.Assign(nameHash, Expression.Call(memberName, typeof(object).GetMethod("GetHashCode")));
            var cases = new List<SwitchCase>();
            foreach (var propertyInfo in BaseDic.PropertyInfo(model))
            {
                if (propertyInfo.Name == name)
                {
                    var property = Expression.Property(Expression.Convert(instance, model.GetType()), propertyInfo.Name);
                    var setValue = Expression.Assign(property, Expression.Convert(newValue, propertyInfo.PropertyType));
                    var propertyHash = Expression.Constant(propertyInfo.Name.GetHashCode(), typeof(int));
                    cases.Add(Expression.SwitchCase(Expression.Convert(setValue, typeof(object)), propertyHash));
                }
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
                return string.Empty;

            key = item.Keys.ToList().Find(a => string.Compare(a, key, true) == 0);

            if (string.IsNullOrEmpty(key))
                return string.Empty;
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
