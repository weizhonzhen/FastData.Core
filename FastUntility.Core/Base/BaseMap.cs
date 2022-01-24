using System;
using System.Collections;
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
        public static T CopyModel<T, T1>(T1 model, Expression<Func<T1, object>> field=null) where T : class, new()
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

            BaseDic.PropertyInfo<T1>().ForEach(m => {
                if (list.Exists(a => a.Name.ToLower() == m.Name.ToLower()))
                {
                    var property = list.Find(a => a.Name.ToLower() == m.Name.ToLower());
                    var isList = m.PropertyType.GetGenericArguments().Length > 0;
                    var isLeafSystemType = isList && m.PropertyType.GetGenericArguments()[0].FullName.StartsWith("System.");
                    var isSystemType = m.PropertyType.FullName.StartsWith("System.");

                    if (isList && !isLeafSystemType)
                    {
                        var leafList = Activator.CreateInstance(typeof(List<>).MakeGenericType(property.PropertyType.GetGenericArguments()[0]));
                        var tempList = Convert.ChangeType(BaseEmit.Get<T1>(model, m.Name), m.PropertyType) as IEnumerable;

                        if (tempList != null)
                        {
                            foreach (var temp in tempList)
                            {
                                var leafModel = Activator.CreateInstance(property.PropertyType.GetGenericArguments()[0]);
                                var propertyList = leafModel.GetType().GetProperties().ToList();

                                temp.GetType().GetProperties().ToList().ForEach(p =>
                                {
                                    if (propertyList.Exists(a => a.Name == p.Name))
                                    {
                                        var tempProperty = propertyList.Find(a => a.Name.ToLower() == p.Name.ToLower());
                                        tempProperty.SetValue(leafModel, p.GetValue(temp));
                                    }
                                });

                                var method = leafList.GetType().GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
                                method.Invoke(leafList, new object[] { leafModel });
                            }
                            BaseEmit.Set<T>(result, property.Name, leafList);
                        }
                    }
                    else if (isSystemType)
                    {
                        BaseEmit.Set<T>(result, property.Name, BaseEmit.Get<T1>(model, m.Name));
                    }
                    else
                    {
                        var tempModel = Convert.ChangeType(BaseEmit.Get<T1>(model, m.Name), m.PropertyType); 
                        var leafModel = Activator.CreateInstance(property.PropertyType);
                        var propertyList = (property.PropertyType as TypeInfo).GetProperties().ToList();

                        (m.PropertyType as TypeInfo).GetProperties().ToList().ForEach(p =>
                        {
                            if (propertyList.Exists(a => a.Name == p.Name))
                            {
                                var temp = propertyList.Find(a => a.Name.ToLower() == p.Name.ToLower());
                                temp.SetValue(leafModel, p.GetValue(tempModel));
                            }
                        });
                        BaseEmit.Set<T>(result, property.Name, leafModel);
                    }
                }
                else
                {
                    if (dic.ToList().Exists(n => (n.Value as MemberExpression).Member.Name.ToLower() == m.Name.ToLower()))
                    {
                        var temp = dic.ToList().Find(n => (n.Value as MemberExpression).Member.Name.ToLower() == m.Name.ToLower());
                        BaseEmit.Set<T>(result, temp.Key.Name, BaseEmit.Get<T1>(model, (temp.Value as MemberExpression).Member.Name));
                    }
                }
            });
            return result;
        }

        public static List<Result> Parameters<T,Result>(T item, Expression<Func<T, object>> field) where Result : class, new()
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
                    BaseEmit.Set<Result>(param, "Value", BaseEmit.Get<T>(item,a.Value.ToStr()));

                result.Add(param);
            });

            return result;
        }
    }
}
