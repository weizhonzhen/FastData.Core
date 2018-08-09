using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Reflection;
using FastData.Core.Property;
using FastData.Core.Model;

namespace FastData.Core.Base
{
    /// <summary>
    /// 标签：2015.7.13，魏中针
    /// 说明：实体转化SQL类
    /// </summary>
    internal static class BaseModel
    {
        #region model 转 update sql
        /// <summary>
        /// model 转 update sql
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="model">实体</param>
        /// <param name="sql">sql</param>
        /// <param name="oracleParam">参数</param>
        /// <returns></returns>
        public static OptionModel UpdateToSql<T>(T model, Expression<Func<T, bool>> predicate, ConfigModel config, Expression<Func<T, object>> field = null)
        {
            var result = new OptionModel();
            var dynGet = new DynamicGet<T>();
            result.IsCache = config.IsPropertyCache;

            try
            {
                result.Sql = string.Format("update {0} set", typeof(T).Name);
                var pInfo = PropertyCache.GetPropertyInfo<T>(config.IsPropertyCache);

                if (field == null)
                {
                    #region 属性
                    foreach (var item in pInfo)
                    {
                        result.Sql = string.Format("{2} {0}={1}{0},", item.Name, config.Flag, result.Sql);

                        var itemValue = dynGet.GetValue(model, item.Name, config.IsPropertyCache);
                        var temp = DbProviderFactories.GetFactory(config).CreateParameter();
                        temp.ParameterName = item.Name;
                        temp.Value = itemValue == null ? DBNull.Value : itemValue;
                        result.Param.Add(temp);                        
                    }
                    #endregion
                }
                else
                {
                    #region lambda
                    var list = (field.Body as NewExpression).Members;
                    foreach (var item in list)
                    {
                        result.Sql = string.Format("{2} {0}={1}{0},", item.Name, config.Flag, result.Sql);

                        var itemValue = dynGet.GetValue(model, item.Name, config.IsPropertyCache);
                        var temp = DbProviderFactories.GetFactory(config).CreateParameter();
                        temp.ParameterName = item.Name;
                        temp.Value = itemValue == null ? DBNull.Value : itemValue;
                        result.Param.Add(temp);
                    }
                    #endregion
                }

                result.Sql = result.Sql.Substring(0, result.Sql.Length - 1);
                result.Result = true;

                return result;
            }
            catch(Exception ex)
            {
                Task.Factory.StartNew(() =>
                {
                    DbLog.LogException(config.IsOutError, config.DbType, ex, "UpdateToSql<T>", result.Sql);
                });
                result.Result = false;
                return result;
            }
        }
        #endregion

        #region model 转 insert sql
        /// <summary>
        /// model 转 insert sql
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="model">实体</param>
        /// <param name="sql">sql</param>
        /// <param name="oracleParam">参数</param>
        /// <returns></returns>
        public static OptionModel InsertToSql<T>(T model, ConfigModel config)
        {
            var sbName = new StringBuilder();
            var sbValue = new StringBuilder();
            var dynGet = new DynamicGet<T>();
            var list = new List<MemberInfo>();
            var result = new OptionModel();

            try
            {
                sbName.AppendFormat("insert into {0} (", typeof(T).Name);
                sbValue.Append(" values (");
                
                var pInfo = PropertyCache.GetPropertyInfo<T>(config.IsPropertyCache);

                foreach (var item in pInfo)
                {
                    if (!list.Exists(a => a.Name == item.Name))
                    {
                        sbName.AppendFormat("{0},", item.Name);

                        sbValue.AppendFormat("{1}{0},", item.Name, config.Flag);

                        var itemValue = dynGet.GetValue(model, item.Name, config.IsPropertyCache);
                        var temp = DbProviderFactories.GetFactory(config).CreateParameter();
                        temp.ParameterName = item.Name;
                        temp.Value = itemValue == null ? DBNull.Value : itemValue;
                        result.Param.Add(temp);
                    }
                }

                result.Sql = string.Format("{0}) {1})", sbName.ToString().Substring(0, sbName.ToString().Length - 1)
                                                , sbValue.ToString().Substring(0, sbValue.ToString().Length - 1));
                result.Result = true;
                return result;
            }
            catch(Exception ex)
            {
                Task.Factory.StartNew(() =>
                {
                    DbLog.LogException(config.IsOutError, config.DbType, ex, "InsertToSql<T>", result.Sql);
                });
                result.Result = false;
                return result;
            }
        }
        #endregion        
    }
}
