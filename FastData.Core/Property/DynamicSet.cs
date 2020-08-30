using FastUntility.Core.Cache;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FastData.Core.Property
{
    /// <summary>
    /// 动态属性setvalue
    /// </summary>
    public class DynamicSet<T>
    {
        private bool IsSetCache;
        private Action<object, string, object> SetValueDelegate;

        // 构建函数        
        public DynamicSet()
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
        private Action<object, string, object> GenerateSetValue()
        {
            var instance = Expression.Parameter(typeof(object), "instance");
            var memberName = Expression.Parameter(typeof(string), "memberName");
            var newValue = Expression.Parameter(typeof(object), "newValue");
            var nameHash = Expression.Variable(typeof(int), "nameHash");
            var calHash = Expression.Assign(nameHash, Expression.Call(memberName, typeof(object).GetMethod("GetHashCode")));
            var cases = new List<SwitchCase>();
            foreach (var propertyInfo in PropertyCache.GetPropertyInfo<T>(IsSetCache))
            {
                if (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() != typeof(Nullable<>))
                    continue;

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
    /// 动态属性setvalue
    /// </summary>
    internal class DynamicSet
    {
        private object Instance;
        private bool IsSetCache;
        private Action<object, string, object> SetValueDelegate;

        // 构建函数        
        public DynamicSet(object model)
        {
            Instance = model;
            var key = string.Format("DynamicSet.{0}.{1}", model.GetType()?.Namespace, model.GetType().Name);
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
        public void SetValue(object instance, string memberName, object newValue, bool IsCache)
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
        private Action<object, string, object> GenerateSetValue()
        {
            var instance = Expression.Parameter(typeof(object), "instance");
            var memberName = Expression.Parameter(typeof(string), "memberName");
            var newValue = Expression.Parameter(typeof(object), "newValue");
            var nameHash = Expression.Variable(typeof(int), "nameHash");
            var calHash = Expression.Assign(nameHash, Expression.Call(memberName, typeof(object).GetMethod("GetHashCode")));
            var cases = new List<SwitchCase>();

            foreach (var propertyInfo in PropertyCache.GetPropertyInfo(Instance, IsSetCache))
            {
                if (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() != typeof(Nullable<>))
                    continue;

                var property = Expression.Property(Expression.Convert(instance, Instance.GetType()), propertyInfo.Name);
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
}
