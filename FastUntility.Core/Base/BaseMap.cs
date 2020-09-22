using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

            foreach (var item in BaseDic.PropertyInfo<T1>())
            {
                if (list.Exists(a => a.Name.ToLower() == item.Name.ToLower()))
                {
                    var property = list.Find(a => a.Name.ToLower() == item.Name.ToLower());
                    var isList = item.PropertyType.GetGenericArguments().Length > 0;
                    var isLeafSystemType = isList && item.PropertyType.GetGenericArguments()[0].FullName.StartsWith("System.");
                    var isSystemType = item.PropertyType.FullName.StartsWith("System.");

                    if (isList && !isLeafSystemType)
                    {
                        var leafList = Activator.CreateInstance(typeof(List<>).MakeGenericType(property.PropertyType.GetGenericArguments()[0]));
                        var tempList = Convert.ChangeType(dynGet.GetValue(model, item.Name, true), item.PropertyType) as IEnumerable;

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
                        if (item.PropertyType.Name == "Nullable`1" && item.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                            dynSet.SetValue(result, property.Name, dynGet.GetValue(model, item.Name, true), true);
                        else
                            dynSet.SetValue(result, property.Name, Convert.ChangeType(dynGet.GetValue(model, item.Name, true), item.PropertyType), true);
                    }
                    else
                    {
                        var tempModel = Convert.ChangeType(dynGet.GetValue(model, item.Name, true), item.PropertyType);
                        var leafModel = Activator.CreateInstance(property.PropertyType);
                        var propertyList = (property.PropertyType as TypeInfo).GetProperties().ToList();
                        
                        (item.PropertyType as TypeInfo).GetProperties().ToList().ForEach(p => {
                            if (propertyList.Exists(a => a.Name == p.Name))
                            {
                                var temp = propertyList.Find(a => a.Name.ToLower() == p.Name.ToLower());
                                temp.SetValue(leafModel, p.GetValue(tempModel));
                            }
                        });
                        dynSet.SetValue(result, property.Name, leafModel, true);
                    }
                }
            }

            return result;
        }
    }
}
