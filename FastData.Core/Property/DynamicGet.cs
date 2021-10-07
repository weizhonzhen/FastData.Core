using FastData.Core.Base;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FastData.Core.Property
{
    /// <summary>
    /// 动态属性getvalue
    /// </summary>
    internal class DynamicGet<T>
    {
        private Func<object, string, object> GetValueDelegate;

        // 构建函数        
        public DynamicGet()
        {
            var key = string.Format("DynamicGet<T>.{0}.{1}", typeof(T)?.Namespace, typeof(T).Name);
            if (!DbCache.Exists(CacheType.Web, key))
            {
                GetValueDelegate = GenerateGetValue();
                DbCache.Set<object>(CacheType.Web, key, GetValueDelegate);
            }
            else
                GetValueDelegate = DbCache.Get<object>(CacheType.Web, key) as Func<object, string, object>;
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
        private Func<object, string, object> GenerateGetValue()
        {
            var instance = Expression.Parameter(typeof(object), "instance");
            var memberName = Expression.Parameter(typeof(string), "memberName");
            var nameHash = Expression.Variable(typeof(int), "nameHash");
            var calHash = Expression.Assign(nameHash, Expression.Call(memberName, typeof(object).GetMethod("GetHashCode")));
            var cases = new List<SwitchCase>();

            foreach (var propertyInfo in PropertyCache.GetPropertyInfo<T>())
            {
                if (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() != typeof(Nullable<>))
                    continue;

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

    internal class DynamicGet
    {
        private object Instance;
        private Func<object, string, object> GetValueDelegate;

        // 构建函数        
        public DynamicGet(object model)
        {
            Instance = model;

            var key = string.Format("DynamicGet.{0}.{1}", model.GetType()?.Namespace, model.GetType().Name);
            if (!DbCache.Exists(CacheType.Web, key))
            {
                GetValueDelegate = GenerateGetValue();
                DbCache.Set<object>(CacheType.Web, key, GetValueDelegate);
            }
            else
                GetValueDelegate = DbCache.Get<object>(CacheType.Web, key) as Func<object, string, object>;
        }

        #region 动态getvalue
        /// <summary>
        /// 动态getvalue
        /// </summary>
        /// <param name="instance">类型</param>
        /// <param name="memberName">成员</param>
        /// <returns></returns>
        public object GetValue(object instance, string memberName)
        {
            return GetValueDelegate(instance, memberName);
        }
        #endregion

        #region 动态生成getvalue
        /// <summary>
        /// 动态生成getvalue
        /// </summary>
        /// <returns></returns>
        private Func<object, string, object> GenerateGetValue()
        {
            var instance = Expression.Parameter(typeof(object), "instance");
            var memberName = Expression.Parameter(typeof(string), "memberName");
            var nameHash = Expression.Variable(typeof(int), "nameHash");
            var calHash = Expression.Assign(nameHash, Expression.Call(memberName, typeof(object).GetMethod("GetHashCode")));
            var cases = new List<SwitchCase>();

            foreach (var propertyInfo in PropertyCache.GetPropertyInfo(Instance))
            {
                if (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() != typeof(Nullable<>))
                    continue;

                var property = Expression.Property(Expression.Convert(instance, Instance.GetType()), propertyInfo.Name);
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
