using System;
using System.Collections;
using System.Collections.Generic;
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
        public static T CopyModel<T, T1>(T1 model) where T : class, new()
        {
            var result = new T();
            var dynGet = new DynamicGet<T1>();
            var dynSet = new DynamicSet<T>();
            var list = BaseDic.PropertyInfo<T>();

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
                        var tempList = Convert.ChangeType(dynGet.GetValue(model, m.Name, true), m.PropertyType) as IEnumerable;

                        if (tempList != null)
                        {
                            foreach (var temp in tempList)
                            {
                                var leafModel = Activator.CreateInstance(property.PropertyType.GetGenericArguments()[0]);
                                var propertyList = leafModel.GetType().GetProperties().ToList();

                                temp.GetType().GetProperties().ToList().ForEach(p => {
                                    if (propertyList.Exists(a => a.Name == p.Name))
                                    {
                                        var tempProperty = propertyList.Find(a => a.Name.ToLower() == p.Name.ToLower());
                                        tempProperty.SetValue(leafModel, p.GetValue(temp));
                                    }
                                });

                                var method = leafList.GetType().GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
                                method.Invoke(leafList, new object[] { leafModel });
                            }
                            dynSet.SetValue(result, property.Name, leafList, true);
                        }
                    }
                    else if (isSystemType)
                    {
                        if (m.PropertyType.Name == "Nullable`1" && m.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                            dynSet.SetValue(result, property.Name, dynGet.GetValue(model, m.Name, true), true);
                        else
                            dynSet.SetValue(result, property.Name, Convert.ChangeType(dynGet.GetValue(model, m.Name, true), m.PropertyType), true);
                    }
                    else
                    {
                        var tempModel = Convert.ChangeType(dynGet.GetValue(model, m.Name, true), m.PropertyType);
                        var leafModel = Activator.CreateInstance(property.PropertyType);
                        var propertyList = (property.PropertyType as TypeInfo).GetProperties().ToList();

                        (m.PropertyType as TypeInfo).GetProperties().ToList().ForEach(p => {
                            if (propertyList.Exists(a => a.Name == p.Name))
                            {
                                var temp = propertyList.Find(a => a.Name.ToLower() == p.Name.ToLower());
                                temp.SetValue(leafModel, p.GetValue(tempModel));
                            }
                        });
                        dynSet.SetValue(result, property.Name, leafModel, true);
                    }
                }
            });
            return result;
        }

        public static List<Result> Parameters<T,Result>(T item, Expression<Func<T, object>> field) where Result : class, new()
        {
            var result = new List<Result>();
            var dyn = new DynamicGet<T>();
            var dynResult = new DynamicSet<Result>();
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
                dynResult.SetValue(param, "ParameterName", a.Key, true);
                if (a.Value is ConstantExpression)
                    dynResult.SetValue(param, "Value", (a.Value as ConstantExpression).Value, true);
                else if (a.Value is MethodCallExpression)
                    dynResult.SetValue(param, "Value", Expression.Lambda((a.Value as MethodCallExpression).ReduceExtensions().Reduce()).Compile().DynamicInvoke().ToString(), true);
                else if (a.Value is MemberExpression)
                {
                    if ((a.Value as MemberExpression).Expression is ParameterExpression)
                        dynResult.SetValue(param, "Value", dyn.GetValue(item, (a.Value as MemberExpression).Member.Name, true), true);
                    else
                        dynResult.SetValue(param, "Value", Expression.Lambda(a.Value as MemberExpression).Compile().DynamicInvoke(), true);
                }
                else
                    dynResult.SetValue(param, "Value", dyn.GetValue(item, a.Value.ToStr(), true), true);

                result.Add(param);
            });

            return result;
        }
    }
}
