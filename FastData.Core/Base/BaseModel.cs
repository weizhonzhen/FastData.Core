using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Reflection;
using FastData.Core.Property;
using FastData.Core.Model;
using System.Data.Common;
using FastData.Core.Type;
using System.Data;
using NPOI.SS.Util;
using System.Linq;

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
        public static OptionModel UpdateToSql<T>(T model, ConfigModel config, Expression<Func<T, object>> field = null)
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
                    pInfo.ForEach(a => {
                        result.Sql = string.Format("{2} {0}={1}{0},", a.Name, config.Flag, result.Sql);
                        var itemValue = dynGet.GetValue(model, a.Name, config.IsPropertyCache);
                        var temp = DbProviderFactories.GetFactory(config).CreateParameter();
                        temp.ParameterName = a.Name;
                        temp.Value = itemValue == null ? DBNull.Value : itemValue;
                        result.Param.Add(temp);
                    });
                    #endregion
                }
                else
                {
                    #region lambda
                    (field.Body as NewExpression).Members.ToList().ForEach(a => {
                        result.Sql = string.Format("{2} {0}={1}{0},", a.Name, config.Flag, result.Sql);
                        var itemValue = dynGet.GetValue(model, a.Name, config.IsPropertyCache);
                        var temp = DbProviderFactories.GetFactory(config).CreateParameter();
                        temp.ParameterName = a.Name;
                        temp.Value = itemValue == null ? DBNull.Value : itemValue;
                        result.Param.Add(temp);
                    });
                    #endregion
                }

                result.Sql = result.Sql.Substring(0, result.Sql.Length - 1);
                result.IsSuccess = true;

                return result;
            }
            catch(Exception ex)
            {
                Task.Run(() =>
                {
                    if (config.SqlErrorType == SqlErrorType.Db)
                        DbLogTable.LogException<T>(config, ex, "UpdateToSql<T>", result.Sql);
                    else
                        DbLog.LogException<T>(config.IsOutError, config.DbType, ex, "UpdateToSql<T>", result.Sql);
                });
                result.IsSuccess = false;
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
                PropertyCache.GetPropertyInfo<T>(config.IsPropertyCache).ForEach(p => {
                    if (!list.Exists(a => a.Name == p.Name))
                    {
                        sbName.AppendFormat("{0},", p.Name);
                        sbValue.AppendFormat("{1}{0},", p.Name, config.Flag);
                        var itemValue = dynGet.GetValue(model, p.Name, config.IsPropertyCache);
                        var temp = DbProviderFactories.GetFactory(config).CreateParameter();
                        temp.ParameterName = p.Name;
                        temp.Value = itemValue == null ? DBNull.Value : itemValue;
                        result.Param.Add(temp);
                    }
                });

                result.Sql = string.Format("{0}) {1})", sbName.ToString().Substring(0, sbName.ToString().Length - 1)
                                                , sbValue.ToString().Substring(0, sbValue.ToString().Length - 1));
                result.IsSuccess = true;
                return result;
            }
            catch(Exception ex)
            {
                Task.Run(() =>
                {
                    if (config.SqlErrorType == SqlErrorType.Db)
                        DbLogTable.LogException<T>(config, ex, "InsertToSql<T>", result.Sql);
                    else
                        DbLog.LogException(config.IsOutError, config.DbType, ex, "InsertToSql<T>", result.Sql);
                });
                result.IsSuccess = false;
                return result;
            }
        }
        #endregion

        #region model 转 update sql
        /// <summary>
        /// model 转 update sql
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="model">实体</param>
        /// <param name="sql">sql</param>
        /// <param name="oracleParam">参数</param>
        /// <returns></returns>
        public static OptionModel UpdateToSql<T>(DbCommand cmd,T model, ConfigModel config, Expression<Func<T, object>> field = null)
        {
            var result = new OptionModel();
            var dynGet = new DynamicGet<T>();
            result.IsCache = config.IsPropertyCache;
            var where = PrimaryKey(config, cmd, typeof(T).Name);

            if (where.Count == 0)
            {
                result.Message = string.Format("{0}没有主键", typeof(T).Name);
                result.IsSuccess = false;
                return result;
            }

            try
            {
                result.Sql = string.Format("update {0} set", typeof(T).Name);
                var pInfo = PropertyCache.GetPropertyInfo<T>(config.IsPropertyCache);

                if (field == null)
                {
                    #region 属性
                    foreach (var item in pInfo)
                    {
                        if (where.Exists(a => a == item.Name))
                            continue;

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
                        if (where.Exists(a => a == item.Name))
                            continue;

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

                var count = 1;
                foreach(var  item in where)
                {
                    var itemValue = dynGet.GetValue(model, item, config.IsPropertyCache);

                    if (itemValue == null)
                    {
                        result.IsSuccess = false;
                        result.Message = string.Format("主键{0}值为空", item);
                        return result;
                    }

                    if (count == 1)
                        result.Sql = string.Format("{2} where {0}={1}{0} ", item, config.Flag, result.Sql);
                    else
                        result.Sql = string.Format("{2} and {0}={1}{0} ", item, config.Flag, result.Sql);

                    var temp = DbProviderFactories.GetFactory(config).CreateParameter();
                    temp.ParameterName = item;
                    temp.Value = itemValue == null ? DBNull.Value : itemValue;

                    result.Param.Add(temp);

                    count ++;
                }

                result.IsSuccess = true;

                return result;
            }
            catch (Exception ex)
            {
                Task.Run(() =>
                {
                    if (config.SqlErrorType == SqlErrorType.Db)
                        DbLogTable.LogException<T>(config, ex, "UpdateToSql<T>", result.Sql);
                    else
                        DbLog.LogException<T>(config.IsOutError, config.DbType, ex, "UpdateToSql<T>", result.Sql);
                });
                result.IsSuccess = false;
                return result;
            }
        }
        #endregion

        #region model 转 update list sql
        /// <summary>
        /// model 转 update list sql
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="model">实体</param>
        /// <param name="sql">sql</param>
        /// <param name="oracleParam">参数</param>
        /// <returns></returns>
        public static OptionModel UpdateListToSql<T>(DbCommand cmd,List<T> list, ConfigModel config, Expression<Func<T, object>> field = null)
        {
            var dynGet = new DynamicGet<T>();
            var result = new OptionModel();
            result.IsCache = config.IsPropertyCache;
            var where = PrimaryKey(config, cmd, typeof(T).Name);

            if (where.Count == 0)
            {
                result.Message = string.Format("{0}没有主键", typeof(T).Name);
                result.IsSuccess = false;
                return result;
            }

            try
            {
                result.table = BaseExecute.ToDataTable<T>(cmd,config, where, field);

                result.Sql = string.Format("update {0} set", typeof(T).Name);
                var pInfo = PropertyCache.GetPropertyInfo<T>(config.IsPropertyCache);

                if (field == null)
                {
                    #region 属性
                    foreach (var item in pInfo)
                    {
                        if (where.Exists(a => a == item.Name))
                            continue;
                        result.Sql = string.Format("{2} {0}={1}{0},", item.Name, config.Flag, result.Sql);
                        var temp = DbProviderFactories.GetFactory(config).CreateParameter();
                        temp.ParameterName = item.Name;
                        temp.SourceColumn = item.Name;                           
                        result.Param.Add(temp);
                    }
                    #endregion
                }
                else
                {
                    #region lambda
                    foreach (var item in (field.Body as NewExpression).Members)
                    {
                        if (where.Exists(a => a == item.Name))
                            continue;
                        result.Sql = string.Format("{2} {0}={1}{0},", item.Name, config.Flag, result.Sql);
                        var temp = DbProviderFactories.GetFactory(config).CreateParameter();
                        temp.ParameterName = item.Name;
                        temp.SourceColumn = item.Name;
                        result.Param.Add(temp);
                    }
                    #endregion
                }

                result.Sql = result.Sql.Substring(0, result.Sql.Length - 1);

                var count = 1;
                where.ForEach(a => {
                    if (count == 1)
                        result.Sql = string.Format("{2} where {0}={1}{0} ", a, config.Flag, result.Sql);
                    else
                        result.Sql = string.Format("{2} and {0}={1}{0} ", a, config.Flag, result.Sql);

                    var temp = DbProviderFactories.GetFactory(config).CreateParameter();
                    temp.ParameterName = a;
                    temp.SourceColumn = a;
                    result.Param.Add(temp);
                    count++;
                });

                result.IsSuccess = true;

                list.ForEach(p => {
                    var row = result.table.NewRow();
                    where.ForEach(a => {
                        row[a] = dynGet.GetValue(p, a, true);
                    });

                    if (field == null)
                    {
                        PropertyCache.GetPropertyInfo<T>().ForEach(a => {
                            row[a.Name] = dynGet.GetValue(p, a.Name, true);
                        });
                    }
                    else
                    {
                        (field.Body as NewExpression).Members.ToList().ForEach(a => {
                            row[a.Name] = dynGet.GetValue(p, a.Name, true);
                        });
                    }
                    result.table.Rows.Add(row);

                });

                return result;
            }
            catch (Exception ex)
            {
                Task.Run(() =>
                {
                    if (config.SqlErrorType == SqlErrorType.Db)
                        DbLogTable.LogException<T>(config, ex, "UpdateListToSql<T>", result.Sql);
                    else
                        DbLog.LogException<T>(config.IsOutError, config.DbType, ex, "UpdateListToSql<T>", result.Sql);
                });
                result.IsSuccess = false;
                return result;
            }
        }
        #endregion

        #region model 转 delete sql
        /// <summary>
        /// model 转 delete sql
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="model">实体</param>
        /// <param name="sql">sql</param>
        /// <param name="oracleParam">参数</param>
        /// <returns></returns>
        public static OptionModel DeleteToSql<T>(DbCommand cmd, T model, ConfigModel config)
        {
            var result = new OptionModel();
            var dynGet = new DynamicGet<T>();
            result.IsCache = config.IsPropertyCache;
            var where = PrimaryKey(config, cmd, typeof(T).Name);

            if (where.Count == 0)
            {
                result.Message = string.Format("{0}没有主键", typeof(T).Name);
                result.IsSuccess = false;
                return result;
            }

            try
            {
                result.Sql = string.Format("delete {0} ", typeof(T).Name);

                var count = 1;
                foreach (var item in where)
                {
                    var itemValue = dynGet.GetValue(model, item, config.IsPropertyCache);

                    if (itemValue == null)
                    {
                        result.IsSuccess = false;
                        result.Message = string.Format("主键{0}值为空", item);
                        return result;
                    }

                    if (count == 1)
                        result.Sql = string.Format("{2} where {0}={1}{0} ", item, config.Flag, result.Sql);
                    else
                        result.Sql = string.Format("{2} and {0}={1}{0} ", item, config.Flag, result.Sql);

                    var temp = DbProviderFactories.GetFactory(config).CreateParameter();
                    temp.ParameterName =  item;
                    temp.Value = itemValue == null ? DBNull.Value : itemValue;

                    result.Param.Add(temp);

                    count++;
                }

                result.IsSuccess = true;

                return result;
            }
            catch (Exception ex)
            {
                Task.Run(() =>
                {
                    if (config.SqlErrorType == SqlErrorType.Db)
                        DbLogTable.LogException<T>(config, ex, "UpdateToSql<T>", result.Sql);
                    else
                        DbLog.LogException<T>(config.IsOutError, config.DbType, ex, "DeleteToSql<T>", result.Sql);
                });
                result.IsSuccess = false;
                return result;
            }
        }
        #endregion

        #region 主键
        /// <summary>
        /// 主键
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="config"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static List<string> PrimaryKey(ConfigModel config, DbCommand cmd,string tableName)
        {
            var key = string.Format("{0}.Primary.Key", tableName);
            var list = new List<string>();

            if (config.DbType == DataDbType.Oracle)
                cmd.CommandText = string.Format("select a.COLUMN_NAME from all_cons_columns a,all_constraints b where a.constraint_name = b.constraint_name and b.constraint_type = 'P' and b.table_name = '{0}'", tableName.ToUpper());

            if (config.DbType == DataDbType.SqlServer)
                cmd.CommandText = string.Format("select column_name from INFORMATION_SCHEMA.KEY_COLUMN_USAGE where TABLE_NAME='{0}'", tableName);

            if (config.DbType == DataDbType.MySql)
                cmd.CommandText = string.Format("select column_name from INFORMATION_SCHEMA.KEY_COLUMN_USAGE a where TABLE_NAME='{0}' and constraint_name='PRIMARY'", tableName.ToUpper());

            if (config.DbType == DataDbType.DB2)
                cmd.CommandText = string.Format("select a.colname from sysibm.syskeycoluse a，syscat.tabconst b where a.tabname=b.tabnameand b.tabname='{0}' and b.type=p", tableName.ToUpper());

            if (string.IsNullOrEmpty(cmd.CommandText))
                return list;
            else
            {
                var dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    list.Add(dr[0].ToString());
                }

                dr.Close();
                DbCache.Set<List<string>>(config.CacheType, key, list);
                return list;
            }
        }
        #endregion
    }
}
