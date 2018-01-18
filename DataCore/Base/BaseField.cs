using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Data.Core.Property;
using Data.Core.Type;
using Data.Core.Model;

namespace Data.Core.Base
{
    /// <summary>
    /// lambda field
    /// </summary>
    internal static class BaseField
    {
        #region query field 1个表
        /// <summary>
        /// query field 1个表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <returns></returns>
        public static FieldModel QueryField<T>(Expression<Func<T, bool>> predicate,Expression<Func<T, object>> field, ConfigModel config)
        {
            try
            {
                var result = new FieldModel();
                var queryFields = new List<string>();
                var i = 0;

                if (field == null)
                {
                    #region 无返回列
                    var list = PropertyCache.GetPropertyInfo<T>(config.IsPropertyCache);

                    foreach (var item in PropertyCache.GetPropertyInfo<T>(config.IsPropertyCache))
                    {
                        if (list.Exists(a => a.Name == item.Name))
                            queryFields.Add(string.Format("{0}.{1}", predicate.Parameters[0].Name, item.Name));
                        else
                            queryFields.Add(item.Name);

                        result.AsName.Add(item.Name);
                    }

                    result.Field = string.Join(",", queryFields);

                    #endregion
                }
                else
                {
                    #region 有返回列
                    foreach (var item in (field.Body as NewExpression).Arguments)
                    {
                        if (item is MethodCallExpression)
                        {
                            var methodName = "";
                            var ower = "";
                            var propertyName = GetPropertyMethod(item, out methodName, false, out ower);

                            if (methodName.ToLower() == "distinct")
                            {
                                queryFields.Add(string.Format("{2}{0} {3}.{1} ", methodName, propertyName, ower, predicate.Parameters[0].Name));
                                result.AsName.Add((item as MemberExpression).Member.Name);
                            }
                            else if (methodName.ToLower() == "sum")
                            {
                                if (config.DbType == DataDbType.SqlServer)
                                    propertyName = string.Format("isnull({1}.{0},0)", propertyName, predicate.Parameters[0].Name);

                                if (config.DbType == DataDbType.MySql || config.DbType == DataDbType.SQLite)
                                    propertyName = string.Format("ifnull({1}.{0},0)", propertyName, predicate.Parameters[0].Name);

                                if (config.DbType == DataDbType.Oracle)
                                    propertyName = string.Format("nvl({1}.{0},0)", propertyName, predicate.Parameters[0].Name);

                                if (config.DbType == DataDbType.DB2)
                                    propertyName = string.Format("coalesce({1}.{0},0)", propertyName, predicate.Parameters[0].Name);

                                queryFields.Add(string.Format("{0}({2}.{1})", methodName, propertyName, predicate.Parameters[0].Name));
                                result.AsName.Add((field.Body as NewExpression).Members[i].Name);
                            }
                            else
                            {
                                queryFields.Add(string.Format("{2}{0}({3}.{1})", methodName, propertyName, ower, predicate.Parameters[0].Name));
                                result.AsName.Add((field.Body as NewExpression).Members[i].Name);
                            }
                        }
                        else
                        {
                            queryFields.Add(string.Format("{0}.{1}", predicate.Parameters[0].Name, (item as MemberExpression).Member.Name));
                            result.AsName.Add((item as MemberExpression).Member.Name);
                        }
                        i++;
                    }
                    #endregion
                }

                result.Field = string.Join(",", queryFields);
                
                return result;
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() =>
                {
                    DbLog.LogException<T>(config.IsOutError,  config.DbType, ex, "QueryField<T>", "");
                });

                return new FieldModel
                {
                    Field = string.Format("{0}.*", predicate.Parameters[0].Name),
                    AsName = new List<string> { predicate.Parameters[0].Name }
                };
            }
        }
        #endregion
        
        #region query field 2个表
        /// <summary>
        /// query field 2个表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <returns></returns>
        public static FieldModel QueryField<T, T1>(Expression<Func<T, T1, bool>> predicate, Expression<Func<T1, object>> field, ConfigModel config)
        {
            try
            {
                var result = new FieldModel();
                var queryFields = new List<string>();

                if (field == null)
                {
                    var list = PropertyCache.GetPropertyInfo<T1>(config.IsPropertyCache);

                    foreach (var item in PropertyCache.GetPropertyInfo<T1>(config.IsPropertyCache))
                    {
                        if (list.Exists(a => a.Name == item.Name))
                            queryFields.Add(string.Format("{0}.{1}", predicate.Parameters[1].Name, item.Name));
                        else
                            queryFields.Add(item.Name);

                        result.AsName.Add(item.Name);
                    }

                    result.Field = string.Join(",", queryFields);
                    return result;
                }

                var i = 0;
                foreach (var item in (field.Body as NewExpression).Arguments)
                {
                    if (item is MethodCallExpression)
                    {
                        var methodName = "";
                        var ower = "";
                        var propertyName = GetPropertyMethod(item, out methodName, true, out ower);

                        if (methodName.ToLower() == "distinct")
                        {
                            queryFields.Add(string.Format("{2}{0} {2}.{1}", methodName, propertyName, ower, predicate.Parameters[0].Name));
                            result.AsName.Add((item as MemberExpression).Member.Name);
                        }
                        else if (methodName.ToLower() == "sum")
                        {
                            if (config.DbType == DataDbType.SqlServer)
                                propertyName = string.Format("isnull({1}.{0},0)", propertyName, predicate.Parameters[0].Name);

                            if (config.DbType == DataDbType.MySql || config.DbType == DataDbType.SQLite)
                                propertyName = string.Format("ifnull({1}.{0},0)", propertyName, predicate.Parameters[0].Name);

                            if (config.DbType == DataDbType.Oracle)
                                propertyName = string.Format("nvl({1}.{0},0)", propertyName, predicate.Parameters[0].Name);

                            if (config.DbType == DataDbType.DB2)
                                propertyName = string.Format("coalesce({1}.{0},0)", propertyName, predicate.Parameters[0].Name);

                            queryFields.Add(string.Format("{0}({2}.{1})", methodName, propertyName, predicate.Parameters[0].Name));
                            result.AsName.Add((field.Body as NewExpression).Members[i].Name);
                        }
                        else
                        {
                            queryFields.Add(string.Format("{2}{0}({3}.{1})", methodName, propertyName, ower, predicate.Parameters[0].Name));
                            result.AsName.Add((field.Body as NewExpression).Members[i].Name);
                        }
                    }
                    else
                    {
                        if (item is MemberExpression)
                        {
                            queryFields.Add(string.Format("{0}.{1}", ((item as MemberExpression).Expression as ParameterExpression).Name, (item as MemberExpression).Member.Name));
                            result.AsName.Add((item as MemberExpression).Member.Name);
                        }
                    }
                    i++;
                }

                result.Field = string.Join(",", queryFields);
                return result;
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() =>
                {
                    DbLog.LogException(config.IsOutError, config.DbType, ex, "QueryField<T1,T2,T>", "");
                });
                return new FieldModel { Field = "*" };
            }
        }
        #endregion

        #region group by 1个表
        /// <summary>
        /// group by 1个表
        /// </summary>
        /// <param name="field"></param>
        /// <param name="isDesc"></param>
        /// <returns></returns>
        public static List<string> GroupBy<T>(Expression<Func<T, object>> field,ConfigModel config)
        {
            try
            {
                var result = new List<string>();

                foreach (var item in (field.Body as NewExpression).Arguments)
                {
                    var asName = ((item as MemberExpression).Expression as ParameterExpression).Name;

                    result.Add(string.Format("{0}.{1}", asName, (item as MemberExpression).Member.Name));
                }

                return result;
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() =>
                {
                    DbLog.LogException(config.IsOutError, config.DbType, ex, "GroupBy<T>", "");
                });

                return new List<string>();
            }
        }
        #endregion
        
        #region order by 1个表
        /// <summary>
        /// order by 1个表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="config"></param>
        /// <param name="isDesc"></param>
        /// <returns></returns>
        public static List<string> OrderBy<T>(Expression<Func<T, object>> field, ConfigModel config, bool isDesc = true)
        {
            try
            {
                var result = new List<string>();

                foreach (var item in (field.Body as NewExpression).Arguments)
                {
                    var asName = ((item as MemberExpression).Expression as ParameterExpression).Name;

                    result.Add(string.Format("{0}.{1} {2}", asName, (item as MemberExpression).Member.Name, isDesc ? "desc" : "asc"));
                }

                return result;
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() =>
                {
                    DbLog.LogException(config.IsOutError, config.DbType, ex, "OrderBy<T>", "");
                });
                return new List<string>();
            }
        }
        #endregion

        #region 获取属性方法
        /// <summary>
        /// 获取属性方法
        /// </summary>
        /// <param name="item"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        private static string GetPropertyMethod(Expression item, out string methodName, bool IsMoreTable, out string ower)
        {
            var result = new List<string>();
            methodName = (item as MethodCallExpression).Method.Name;
            ower = "";

            var meExp = (MethodCallExpression)(item.ReduceExtensions().Reduce());
            var count = 0;
            foreach (var temp in meExp.Arguments)
            {
                count++;
                if (temp is UnaryExpression)
                {
                    if (IsMoreTable)
                    {
                        var asName = "";
                        var name = "";
                        if (temp is MemberExpression)
                        {
                            asName = ((temp as MemberExpression).Expression as ParameterExpression).Name;
                            name = (temp as MemberExpression).Member.Name;
                        }
                        else if (temp is UnaryExpression)
                        {
                            asName = (((temp as UnaryExpression).Operand as MemberExpression).Expression as ParameterExpression).Name;
                            name = ((temp as UnaryExpression).Operand as MemberExpression).Member.Name;
                        }

                        result.Add(string.Format("{0}.{1}", asName, name));
                    }
                    else
                        result.Add(temp is MemberExpression ? (temp as MemberExpression).Member.Name : ((temp as UnaryExpression).Operand as MemberExpression).Member.Name);
                }

                if (temp is MemberExpression || temp is NewArrayExpression)
                {
                    if ((temp as MemberExpression).Expression is ConstantExpression)
                        result.Add(Expression.Lambda(temp).Compile().DynamicInvoke().ToString());

                    if ((temp as MemberExpression).Expression is MemberExpression)
                        result.Add(Expression.Lambda(temp).Compile().DynamicInvoke().ToString());
                }

                if (temp is ConstantExpression && count != meExp.Arguments.Count)
                    result.Add((temp as ConstantExpression).Value.ToString());

                if (temp is ConstantExpression && count == meExp.Arguments.Count)
                    ower = (temp as ConstantExpression).Value.ToString();
            }

            return string.Join(",", result);

        }
        #endregion
    }
}