using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace FastUntility.Core.Base
{
    public static class BaseMap
    {
        /// <summary>
        /// 对象映射
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public static T CopyModel<T, T1>(T1 model, Expression<Func<T1, object>> field = null) where T : class, new()
        {
            var result = new T();
            var list = BaseDic.PropertyInfo<T>();
            var dic = new Dictionary<MemberInfo, Expression>();
            if (field != null)
            {
                var name = (field.Body as NewExpression).Members.ToList();
                var value = (field.Body as NewExpression).Arguments.ToList();
                for (var i = 0; i < name.Count; i++)
                {
                    dic.Add(name[i], value[i]);
                }
            }

            BaseDic.PropertyInfo<T1>().ForEach(m =>
            {
                if (list.Exists(a => string.Compare(a.Name, m.Name, true) == 0))
                    BaseEmit.Set<T>(result, m.Name, BaseEmit.Get<T1>(model, m.Name));
                else
                {
                    if (dic.ToList().Exists(n => string.Compare((n.Value as MemberExpression).Member.Name, m.Name, true) == 0))
                    {
                        var temp = dic.ToList().Find(n => string.Compare((n.Value as MemberExpression).Member.Name, m.Name, true) == 0);
                        BaseEmit.Set<T>(result, temp.Key.Name, BaseEmit.Get<T1>(model, (temp.Value as MemberExpression).Member.Name));
                    }
                }
            });
            return result;
        }

        public static List<Result> Parameters<T, Result>(T item, Expression<Func<T, object>> field) where Result : class, new()
        {
            if (typeof(Result).BaseType != typeof(DbParameter))
                throw new Exception("Result type error is not DbParameter");

            var result = new List<Result>();
            var dic = new Dictionary<string, object>();

            var name = (field.Body as NewExpression).Members.ToList();
            var value = (field.Body as NewExpression).Arguments.ToList();

            for (var i = 0; i < name.Count; i++)
            {
                dic.Add(name[i].Name, value[i]);
            }

            dic.ToList().ForEach(a =>
            {
                var param = new Result();
                BaseEmit.Set<Result>(param, "ParameterName", a.Key);
                if (a.Value is ConstantExpression)
                    BaseEmit.Set<Result>(param, "Value", (a.Value as ConstantExpression).Value);
                else if (a.Value is MethodCallExpression)
                    BaseEmit.Set<Result>(param, "Value", Expression.Lambda((a.Value as MethodCallExpression).ReduceExtensions().Reduce()).Compile().DynamicInvoke().ToString());
                else if (a.Value is MemberExpression)
                {
                    if ((a.Value as MemberExpression).Expression is ParameterExpression)
                        BaseEmit.Set<Result>(param, "Value", BaseEmit.Get<T>(item, (a.Value as MemberExpression).Member.Name));
                    else
                        BaseEmit.Set<Result>(param, "Value", Expression.Lambda(a.Value as MemberExpression).Compile().DynamicInvoke());
                }
                else
                    BaseEmit.Set<Result>(param, "Value", BaseEmit.Get<T>(item, a.Value.ToStr()));

                result.Add(param);
            });

            return result;
        }
    }
}
