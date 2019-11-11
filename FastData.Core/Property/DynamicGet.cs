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
        public object GetValue(T instance, string memberName, bool IsCache)
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

            foreach (var propertyInfo in PropertyCache.GetPropertyInfo<T>(IsGetCache))
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
        private static object Instance;
        private static bool IsGetCache;
        private static Func<object, string, object> GetValueDelegate;

        // 构建函数        
        public DynamicGet(object model)
        {
            Instance = model;
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

            foreach (var propertyInfo in PropertyCache.GetPropertyInfo(Instance, IsGetCache))
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
