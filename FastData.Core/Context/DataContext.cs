﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Data.Common;
using FastUntility.Core.Page;
using FastUntility.Core.Base;
using FastData.Core.Base;
using FastData.Core.Model;
using FastData.Core.Type;
using System.Linq.Expressions;
using FastData.Core.Property;
using System.Data;

namespace FastData.Core.Context
{
    public class DataContext : IDisposable
    {
        //变量
        public ConfigModel config;
        private DbConnection conn;
        private DbCommand cmd;
        private DbTransaction trans;

        #region 回收资源
        /// <summary>
        /// 回收资源
        /// </summary>
        public void Dispose()
        {
            conn.Close();
            cmd.Dispose();
            conn.Dispose();
            GC.SuppressFinalize(true);
        }
        #endregion

        #region 初始化
        /// <summary>
        /// 初始化
        /// </summary>
        public DataContext(string key = null)
        {
            try
            {
                this.config = DataConfig.Get(key);
                conn = DbProviderFactories.GetFactory(this.config).CreateConnection();
                conn.ConnectionString = this.config.ConnStr;
                conn.Open();
                cmd = conn.CreateCommand();
            }
            catch (Exception ex)
            {
                Task.Run(() =>
                {
                    if (config.SqlErrorType == SqlErrorType.Db)
                        DbLogTable.LogException(config, ex, "DataContext", "");
                    else
                        DbLog.LogException(true,  this.config.DbType, ex, "DataContext", "");
                });
            }
        }
        #endregion

        #region 获取列表
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        public DataReturn<T> GetList<T>(DataQuery item) where T : class,new()
        {
            var param = new List<DbParameter>();
            var result = new DataReturn<T>();
            var sql = new StringBuilder();

            try
            {
                //是否前几条或单条
                if (item.Config.DbType == DataDbType.SqlServer && item.Take != 0)
                    sql.AppendFormat("select top {2} {0} from {1}", string.Join(",", item.Field), item.Table[0], item.Take);
                else
                    sql.AppendFormat("select {0} from {1}", string.Join(",", item.Field), item.Table[0]);

                for (var i = 1; i < item.Predicate.Count; i++)
                {
                    sql.AppendFormat(" {0} on {1}", item.Table[i], item.Predicate[i].Where);

                    if (item.Predicate[i].Param.Count != 0)
                        param.AddRange(item.Predicate[i].Param);
                }

                sql.AppendFormat(" where {0}", item.Predicate[0].Where);

                //是否前几条或单条
                if (item.Config.DbType == DataDbType.Oracle && item.Take != 0)
                    sql.AppendFormat(" and rownum <={0}", item.Take);
                else if (item.Config.DbType == DataDbType.DB2 && item.Take != 0)
                    sql.AppendFormat(" and fetch first {0} rows only", item.Take);
                else if (item.Config.DbType == DataDbType.MySql && item.Take != 0)
                    sql.AppendFormat(" and limit {0}", item.Take);
                else if (item.Config.DbType == DataDbType.PostgreSql && item.Take != 0)
                    sql.AppendFormat(" and limit {0}", item.Take);
                else if (item.Config.DbType == DataDbType.SQLite && item.Take != 0)
                    sql.AppendFormat(" and limit 0 offset {0}", item.Take);

                if (item.Predicate[0].Param.Count != 0)
                    param.AddRange(item.Predicate[0].Param);

                if (item.GroupBy.Count > 0)
                    sql.AppendFormat(" group by {0}", string.Join(",", item.GroupBy));

                if (item.OrderBy.Count > 0)
                    sql.AppendFormat(" order by {0}", string.Join(",", item.OrderBy));

                result.sql = ParameterToSql.ObjectParamToSql(param, sql.ToString(), item.Config);

                cmd.Parameters.Clear();

                if (param.Count != 0)
                    cmd.Parameters.AddRange(param.ToArray());

                var dr = BaseExecute.ToDataReader(cmd, sql.ToString());

                if (item.Take == 1)
                    result.item = BaseDataReader.ToList<T>(dr, item.Config, item.AsName).FirstOrDefault<T>()??new T();
                else
                    result.list = BaseDataReader.ToList<T>(dr, item.Config, item.AsName);

                dr.Close();
                dr.Dispose();
                dr = null;

                return result;
            }
            catch (Exception ex)
            {
                Task.Run(() =>
                {
                    if (config.SqlErrorType.ToLower() == SqlErrorType.Db)
                        DbLogTable.LogException<T>(config, ex, "GetList<T>", "");
                    else
                        DbLog.LogException<T>(item.Config.IsOutError, item.Config.DbType, ex, "GetList<T>", result.sql);
                });
                return result;
            }
        }
        #endregion

        #region 获取分页
        /// <summary>
        /// 获取分页
        /// </summary>
        /// <returns></returns>
        public DataReturn<T> GetPage<T>(DataQuery item, PageModel pModel) where T : class,new()
        {
            var param = new List<DbParameter>();
            var result = new DataReturn<T>();
            var sql = "";

            try
            {
                pModel.StarId = (pModel.PageId - 1) * pModel.PageSize + 1;
                pModel.EndId = pModel.PageId * pModel.PageSize;
                pModel.TotalRecord = BaseExecute.ToPageCount(item, cmd, ref sql);

                if (pModel.TotalRecord > 0)
                {
                    if ((pModel.TotalRecord % pModel.PageSize) == 0)
                        pModel.TotalPage = pModel.TotalRecord / pModel.PageSize;
                    else
                        pModel.TotalPage = (pModel.TotalRecord / pModel.PageSize) + 1;

                    if (pModel.PageId > pModel.TotalPage)
                        pModel.PageId = pModel.TotalPage;

                    var dr = BaseExecute.ToPageDataReader(item, cmd, pModel, ref sql);
                    result.pageResult.list = BaseDataReader.ToList<T>(dr, item.Config, item.AsName);
                    result.sql = sql;

                    dr.Close();
                    dr.Dispose();
                }
                else
                    result.pageResult.list = new List<T>();

              result.pageResult.pModel = pModel;
            }
            catch (Exception ex)
            {
                Task.Run(() =>
                {
                    if (config.SqlErrorType.ToLower() == SqlErrorType.Db)
                        DbLogTable.LogException<T>(config, ex, "GetPage<T>", "");
                    else
                        DbLog.LogException<T>(item.Config.IsOutError, item.Config.DbType, ex, "GetPage<T>", result.sql);
                });
            }

            return result;
        }
        #endregion

        #region 获取分页
        /// <summary>
        /// 获取分页
        /// </summary>
        /// <returns></returns>
        public DataReturn GetPage(DataQuery item, PageModel pModel)
        {
            var param = new List<DbParameter>();
            var result = new DataReturn();
            var sql = "";

            try
            {
                pModel.StarId = (pModel.PageId - 1) * pModel.PageSize + 1;
                pModel.EndId = pModel.PageId * pModel.PageSize;
                pModel.TotalRecord = BaseExecute.ToPageCount(item, cmd, ref sql);

                if (pModel.TotalRecord > 0)
                {
                    if ((pModel.TotalRecord % pModel.PageSize) == 0)
                        pModel.TotalPage = pModel.TotalRecord / pModel.PageSize;
                    else
                        pModel.TotalPage = (pModel.TotalRecord / pModel.PageSize) + 1;
 
                    if (pModel.PageId > pModel.TotalPage)
                        pModel.PageId = pModel.TotalPage;
                    
                    var dr = BaseExecute.ToPageDataReader(item, cmd, pModel, ref sql);
                    result.PageResult.list = BaseJson.DataReaderToDic(dr);
                    result.Sql = sql;

                    dr.Close();
                    dr.Dispose();
                }
                else
                    result.PageResult.list = new List<Dictionary<string, object>>();

                result.PageResult.pModel = pModel;
            }
            catch (Exception ex)
            {
                Task.Run(() =>
                {
                    if (config.SqlErrorType == SqlErrorType.Db)
                        DbLogTable.LogException(config, ex, "GetPage", result.Sql);
                    else
                        DbLog.LogException(item.Config.IsOutError, item.Config.DbType, ex, "GetPage", result.Sql);
                });
            }

            return result;
        }
        #endregion

        #region 获取分页sql
        /// <summary>
        /// 获取分页
        /// </summary>
        /// <returns></returns>
        public DataReturn GetPageSql(PageModel pModel, string sql, DbParameter[] param)
        {
            var result = new DataReturn();
            var countSql = "";
            var pageSql = "";

            try
            {
                pModel.StarId = (pModel.PageId - 1) * pModel.PageSize + 1;
                pModel.EndId = pModel.PageId * pModel.PageSize;
                pModel.TotalRecord = BaseExecute.ToPageCountSql(param, cmd, sql, config, ref countSql);

                if (pModel.TotalRecord > 0)
                {
                    if ((pModel.TotalRecord % pModel.PageSize) == 0)
                        pModel.TotalPage = pModel.TotalRecord / pModel.PageSize;
                    else
                        pModel.TotalPage = (pModel.TotalRecord / pModel.PageSize) + 1;

                    if (pModel.PageId > pModel.TotalPage)
                        pModel.PageId = pModel.TotalPage;

                    var dr = BaseExecute.ToPageDataReaderSql(param, cmd, pModel, sql, config, ref pageSql);

                    result.PageResult.list = BaseJson.DataReaderToDic(dr);
                    result.Sql = string.Format("count:{0},page:{1}", countSql, pageSql);

                    dr.Close();
                    dr.Dispose();
                }
                else
                    result.PageResult.list = new List<Dictionary<string, object>>();

                result.PageResult.pModel = pModel;
            }
            catch (Exception ex)
            {
                Task.Run(() =>
                {
                    if (config.SqlErrorType == SqlErrorType.Db)
                        DbLogTable.LogException(config, ex, "GetPageSql", result.Sql);
                    else
                        DbLog.LogException(config.IsOutError,config.DbType, ex, "GetPageSql", result.Sql);
                });
            }

            return result;
        }
        #endregion
        
        #region 获取分页sql
        /// <summary>
        /// 获取分页
        /// </summary>
        /// <returns></returns>
        public DataReturn<T> GetPageSql<T>(PageModel pModel, string sql, DbParameter[] param) where T : class, new()
        {
            var result = new DataReturn<T>();
            var countSql = "";
            var pageSql = "";

            try
            {
                pModel.StarId = (pModel.PageId - 1) * pModel.PageSize + 1;
                pModel.EndId = pModel.PageId * pModel.PageSize;
                pModel.TotalRecord = BaseExecute.ToPageCountSql(param, cmd, sql, config, ref countSql);

                if (pModel.TotalRecord > 0)
                {
                    if ((pModel.TotalRecord % pModel.PageSize) == 0)
                        pModel.TotalPage = pModel.TotalRecord / pModel.PageSize;
                    else
                        pModel.TotalPage = (pModel.TotalRecord / pModel.PageSize) + 1;

                    if (pModel.PageId > pModel.TotalPage)
                        pModel.PageId = pModel.TotalPage;

                    var dr = BaseExecute.ToPageDataReaderSql(param, cmd, pModel, sql, config, ref pageSql);

                    result.pageResult.list = BaseDataReader.ToList<T>(dr, config, null);
                    result.sql = string.Format("count:{0},page:{1}", countSql, pageSql);

                    dr.Close();
                    dr.Dispose();
                }
                else
                    result.pageResult.list = new List<T>();

               result.pageResult.pModel = pModel;
            }
            catch (Exception ex)
            {
                Task.Run(() =>
                {
                    if (config.SqlErrorType == SqlErrorType.Db)
                        DbLogTable.LogException<T>(config, ex, "GetPageSql", result.sql);
                    else
                        DbLog.LogException(config.IsOutError, config.DbType, ex, "GetPageSql", result.sql);
                });
            }

            return result;
        }
        #endregion

        #region 获取json
        /// <summary>
        /// 获取json多表
        /// </summary>
        /// <returns></returns>
        public DataReturn GetJson(DataQuery item)
        {
            var param = new List<DbParameter>();
            var result = new DataReturn();
            var sql = new StringBuilder();

            try
            {
                //是否前几条或单条
                if (item.Config.DbType == DataDbType.SqlServer && item.Take != 0)
                    sql.AppendFormat("select top {2} {0} from {1}", string.Join(",", item.Field), item.Table[0], item.Take);
                else
                    sql.AppendFormat("select {0} from {1}", string.Join(",", item.Field), item.Table[0]);

                for (var i = 1; i < item.Predicate.Count; i++)
                {
                    sql.AppendFormat(" {0} on {1}", item.Table[i], item.Predicate[i].Where);

                    if (item.Predicate[i].Param.Count != 0)
                        param.AddRange(item.Predicate[i].Param);
                }

                sql.AppendFormat(" where {0}", item.Predicate[0].Where);

                //是否前几条或单条
                if (item.Config.DbType == DataDbType.Oracle && item.Take != 0)
                    sql.AppendFormat(" and rownum <={0}", item.Take);
                else if (item.Config.DbType == DataDbType.DB2 && item.Take != 0)
                    sql.AppendFormat(" and fetch first {0} rows only", item.Take);
                else if (item.Config.DbType == DataDbType.MySql && item.Take != 0)
                    sql.AppendFormat(" and limit {0}", item.Take);
                else if (item.Config.DbType == DataDbType.PostgreSql && item.Take != 0)
                    sql.AppendFormat(" and limit {0}", item.Take);
                else if (item.Config.DbType == DataDbType.SQLite && item.Take != 0)
                    sql.AppendFormat(" and limit 0 offset {0}", item.Take);

                if (item.Predicate[0].Param.Count != 0)
                    param.AddRange(item.Predicate[0].Param);
                
                if (item.GroupBy.Count > 0)
                    sql.AppendFormat(" group by {0}", string.Join(",", item.GroupBy));

                if (item.OrderBy.Count > 0)
                    sql.AppendFormat(" order by {0}", string.Join(",", item.OrderBy));

                result.Sql = ParameterToSql.ObjectParamToSql(param, sql.ToString(), item.Config);

                cmd.Parameters.Clear();

                if (param.Count != 0)
                    cmd.Parameters.AddRange(param.ToArray());

                var dr = BaseExecute.ToDataReader(cmd, sql.ToString());

                result.Json = BaseJson.DataReaderToJson(dr);

                dr.Close();
                dr.Dispose();

                return result;
            }
            catch (Exception ex)
            {
                Task.Run(() =>
                {
                    if (config.SqlErrorType == SqlErrorType.Db)
                        DbLogTable.LogException(config, ex, "GetJson", result.Sql);
                    else
                        DbLog.LogException(item.Config.IsOutError, item.Config.DbType, ex, "GetJson", result.Sql);
                });
                return result;
            }
        }
        #endregion

        #region 获取条数
        /// <summary>
        /// 获取条数
        /// </summary>
        /// <returns></returns>
        public DataReturn GetCount(DataQuery item)
        {
            var sql = new StringBuilder();
            var result = new DataReturn();
            var param = new List<DbParameter>();

            try
            {
                sql.AppendFormat("select count(0) from {0}", item.Table[0]);

                for (var i = 1; i < item.Predicate.Count; i++)
                {
                    sql.AppendFormat(" {0} on {1}", item.Table[i], item.Predicate[i].Where);

                    if (item.Predicate[i].Param.Count != 0)
                        param.AddRange(item.Predicate[i].Param);
                }

                if (!string.IsNullOrEmpty(item.Predicate[0].Where))
                {
                    sql.AppendFormat(" where {0}", item.Predicate[0].Where);

                    if (item.Predicate[0].Param.Count != 0)
                        param.AddRange(item.Predicate[0].Param);
                }
                
                if (item.GroupBy.Count > 0)
                    sql.AppendFormat(" group by {0}", string.Join(",", item.GroupBy));

                if (item.OrderBy.Count > 0)
                    sql.AppendFormat(" order by {0}", string.Join(",", item.OrderBy));

                result.Sql = ParameterToSql.ObjectParamToSql(param, sql.ToString(), item.Config);

                cmd.Parameters.Clear();

                if (param.Count != 0)
                    cmd.Parameters.AddRange(param.ToArray());

               var dt = BaseExecute.ToDataTable(cmd, sql.ToString());

                if (dt.Rows.Count > 0)
                    result.Count = dt.Rows[0][0].ToString().ToInt(0);
                else
                    result.Count = 0;

                return result;
            }
            catch (Exception ex)
            {
                Task.Run(() =>
                {
                    if (config.SqlErrorType == SqlErrorType.Db)
                        DbLogTable.LogException(config, ex, "GetCount", result.Sql);
                    else
                        DbLog.LogException(item.Config.IsOutError, item.Config.DbType, ex, "GetCount", result.Sql);
                });
                return result;
            }
        }
        #endregion

        #region 执行sql
        /// <summary>
        /// 执行sql
        /// </summary>
        /// <returns></returns>
        public DataReturn<T> ExecuteSql<T>(string sql, DbParameter[] param=null) where T : class,new()
        {
            var result = new DataReturn<T>();
            try
            {
                if (param != null)
                    result.sql = ParameterToSql.ObjectParamToSql(param.ToList(), sql, config);
                else
                    result.sql = sql;

                cmd.Parameters.Clear();

                if (param != null)
                    cmd.Parameters.AddRange(param.ToArray());

                var dr = BaseExecute.ToDataReader(cmd, sql);

                result.list = BaseDataReader.ToList<T>(dr, config);

                dr.Close();
                dr.Dispose();
            }
            catch (Exception ex)
            {
                Task.Run(() =>
                {
                    if (config.SqlErrorType.ToLower() == SqlErrorType.Db)
                        DbLogTable.LogException<T>(config, ex, "ExecuteSql<T>", "");
                    else
                        DbLog.LogException<T>(config.IsOutError, config.DbType, ex, "ExecuteSql<T>", result.sql);
                });
            }

            return result;
        }
        #endregion
        
        #region 执行sql
        /// <summary>
        /// 执行sql
        /// </summary>
        /// <returns></returns>
        public DataReturn ExecuteSql(string sql, DbParameter[] param=null, bool isLog = false)
        {
            var result = new DataReturn();
            try
            {
                if (param != null)
                    result.Sql = ParameterToSql.ObjectParamToSql(param.ToList(), sql, config);
                else
                    result.Sql = sql;

                cmd.Parameters.Clear();

                DbLog.LogSql(isLog, result.Sql, config.DbType, 0);

                if (param != null)
                    cmd.Parameters.AddRange(param.ToArray());

                var dr = BaseExecute.ToDataReader(cmd, sql);

                result.writeReturn.IsSuccess = true;
                result.DicList = BaseJson.DataReaderToDic(dr);

                dr.Close();
                dr.Dispose();
            }
            catch (Exception ex)
            {
                result.writeReturn.IsSuccess = false;
                result.writeReturn.Message = ex.Message;
                Task.Run(() =>
                {
                    if (config.SqlErrorType == SqlErrorType.Db)
                        DbLogTable.LogException(config, ex, "ExecuteSql", result.Sql);
                    else
                        DbLog.LogException(config.IsOutError, config.DbType, ex, "ExecuteSql", result.Sql);
                });
            }

            return result;
        }
        #endregion

        #region 获取dic
        /// <summary>
        /// 获取dic
        /// </summary>
        /// <returns></returns>
        public DataReturn GetDic(DataQuery item)
        {
            var param = new List<DbParameter>();
            var result = new DataReturn();
            var sql = new StringBuilder();

            try
            {
                //是否前几条或单条
                if (item.Config.DbType == DataDbType.SqlServer && item.Take != 0)
                    sql.AppendFormat("select top {2} {0} from {1}", string.Join(",", item.Field), item.Table[0], item.Take);
                else
                    sql.AppendFormat("select {0} from {1}", string.Join(",", item.Field), item.Table[0]);

                for (var i = 1; i < item.Predicate.Count; i++)
                {
                    sql.AppendFormat(" {0} on {1}", item.Table[i], item.Predicate[i].Where);

                    if (item.Predicate[i].Param.Count != 0)
                        param.AddRange(item.Predicate[i].Param);
                }

                sql.AppendFormat(" where {0}", item.Predicate[0].Where);

                //是否前几条或单条
                if (item.Config.DbType == DataDbType.Oracle && item.Take != 0)
                    sql.AppendFormat(" and rownum <={0}", item.Take);
                else if (item.Config.DbType == DataDbType.DB2 && item.Take != 0)
                    sql.AppendFormat(" and fetch first {0} rows only", item.Take);
                else if (item.Config.DbType == DataDbType.PostgreSql && item.Take != 0)
                    sql.AppendFormat(" and limit {0}", item.Take);
                else if (item.Config.DbType == DataDbType.MySql && item.Take != 0)
                    sql.AppendFormat(" and limit {0}", item.Take);
                else if (item.Config.DbType == DataDbType.SQLite && item.Take != 0)
                    sql.AppendFormat(" and limit 0 offset {0}", item.Take);

                if (item.Predicate[0].Param.Count != 0)
                    param.AddRange(item.Predicate[0].Param);

                if (item.GroupBy.Count > 0)
                    sql.AppendFormat(" group by {0}", string.Join(",", item.GroupBy));

                if (item.OrderBy.Count > 0)
                    sql.AppendFormat(" order by {0}", string.Join(",", item.OrderBy));

                result.Sql = ParameterToSql.ObjectParamToSql(param, sql.ToString(), item.Config);

                cmd.Parameters.Clear();

                if (param.Count != 0)
                    cmd.Parameters.AddRange(param.ToArray());

                var dr = BaseExecute.ToDataReader(cmd, sql.ToString());

                if (item.Take == 1)
                    result.Dic = BaseJson.DataReaderToDic(dr).FirstOrDefault()??new Dictionary<string, object>();
                else
                    result.DicList = BaseJson.DataReaderToDic(dr);

                dr.Close();
                dr.Dispose();

                return result;
            }
            catch (Exception ex)
            {
                Task.Run(() =>
                {
                    if (config.SqlErrorType == SqlErrorType.Db)
                        DbLogTable.LogException(config, ex, "GetDic", result.Sql);
                    else
                        DbLog.LogException(item.Config.IsOutError, item.Config.DbType, ex, "GetDic", result.Sql);
                });
                return result;
            }
        }
        #endregion

        #region 获取DataTable
        /// <summary>
        /// 获取DataTable
        /// </summary>
        /// <returns></returns>
        public DataReturn GetDataTable(DataQuery item)
        {
            var param = new List<DbParameter>();
            var result = new DataReturn();
            var sql = new StringBuilder();

            try
            {
                //是否前几条或单条
                if (item.Config.DbType == DataDbType.SqlServer && item.Take != 0)
                    sql.AppendFormat("select top {2} {0} from {1}", string.Join(",", item.Field), item.Table[0], item.Take);
                else
                    sql.AppendFormat("select {0} from {1}", string.Join(",", item.Field), item.Table[0]);

                for (var i = 1; i < item.Predicate.Count; i++)
                {
                    sql.AppendFormat(" {0} on {1}", item.Table[i], item.Predicate[i].Where);

                    if (item.Predicate[i].Param.Count != 0)
                        param.AddRange(item.Predicate[i].Param);
                }

                sql.AppendFormat(" where {0}", item.Predicate[0].Where);

                //是否前几条或单条
                if (item.Config.DbType == DataDbType.Oracle && item.Take != 0)
                    sql.AppendFormat(" and rownum <={0}", item.Take);
                else if (item.Config.DbType == DataDbType.DB2 && item.Take != 0)
                    sql.AppendFormat(" and fetch first {0} rows only", item.Take);
                else if (item.Config.DbType == DataDbType.PostgreSql && item.Take != 0)
                    sql.AppendFormat(" and limit {0}", item.Take);
                else if (item.Config.DbType == DataDbType.MySql && item.Take != 0)
                    sql.AppendFormat(" and limit {0}", item.Take);
                else if (item.Config.DbType == DataDbType.SQLite && item.Take != 0)
                    sql.AppendFormat(" and limit 0 offset {0}", item.Take);

                if (item.Predicate[0].Param.Count != 0)
                    param.AddRange(item.Predicate[0].Param);
                
                if (item.GroupBy.Count > 0)
                    sql.AppendFormat(" group by {0}", string.Join(",", item.GroupBy));

                if (item.OrderBy.Count > 0)
                    sql.AppendFormat(" order by {0}", string.Join(",", item.OrderBy));

                result.Sql = ParameterToSql.ObjectParamToSql(param, sql.ToString(), item.Config);

                cmd.Parameters.Clear();

                if (param.Count != 0)
                    cmd.Parameters.AddRange(param.ToArray());

                var dr = BaseExecute.ToDataReader(cmd, sql.ToString());

                result.Table.Load(dr);

                dr.Close();
                dr.Dispose();

                return result;
            }
            catch (Exception ex)
            {
                Task.Run(() =>
                {
                    if (config.SqlErrorType.ToLower() == SqlErrorType.Db)
                        DbLogTable.LogException(config, ex, "GetDataTable", result.Sql);
                    else
                        DbLog.LogException(item.Config.IsOutError, item.Config.DbType, ex, "GetDataTable", result.Sql);
                });
                return result;
            }
        }
        #endregion

        #region 删除(Lambda表达式)
        /// <summary>
        /// 删除(Lambda表达式)
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="predicate">表达式</param>
        /// <returns></returns>
        public DataReturn<T> Delete<T>(Expression<Func<T, bool>> predicate, bool isTrans = false) where T : class, new()
        {
            var result = new DataReturn<T>();
            var sql = new StringBuilder();
            var visitModel = new VisitModel();

            try
            {
                if (isTrans)
                    BeginTrans();

                visitModel = VisitExpression.LambdaWhere<T>(predicate, config);

                sql.AppendFormat("delete from {0} {1}", typeof(T).Name
                    , string.IsNullOrEmpty(visitModel.Where) ? "" : string.Format("where {0}", visitModel.Where.Replace(string.Format("{0}.", predicate.Parameters[0].Name), "")));

                result.sql = ParameterToSql.ObjectParamToSql(visitModel.Param, sql.ToString(), config);

                cmd.Parameters.Clear();

                if (visitModel.Param.Count != 0)
                    cmd.Parameters.AddRange(visitModel.Param.ToArray());

                if (visitModel.IsSuccess)
                    result.writeReturn.IsSuccess = BaseExecute.ToBool(cmd, sql.ToString());
                else
                    result.writeReturn.IsSuccess = false;

                if (isTrans && result.writeReturn.IsSuccess)
                    SubmitTrans();
                else if (isTrans && result.writeReturn.IsSuccess == false)
                    RollbackTrans();
            }
            catch (Exception ex)
            {
                if (isTrans)
                    RollbackTrans();

                Task.Run(() =>
                {
                    if (config.SqlErrorType.ToLower() == SqlErrorType.Db)
                        DbLogTable.LogException<T>(config, ex, "Delete<T>", "");
                    else
                        DbLog.LogException<T>(config.IsOutError, config.DbType, ex, "Delete<T>", result.sql);
                });

                result.writeReturn.IsSuccess = false;
                result.writeReturn.Message = ex.Message;
            }

            return result;
        }
        #endregion

        #region 删除
        /// <summary>
        /// 删除
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <returns></returns>
        public DataReturn<T> Delete<T>(T model,bool isTrans = false) where T : class, new()
        {
            var result = new DataReturn<T>();
            var optionModel = new OptionModel();

            try
            {
                if (isTrans)
                    BeginTrans();

                optionModel = BaseModel.DeleteToSql<T>(cmd, model, config);

                result.sql = ParameterToSql.ObjectParamToSql(optionModel.Param, optionModel.Sql, config);

                cmd.Parameters.Clear();

                if (optionModel.Param.Count != 0)
                    cmd.Parameters.AddRange(optionModel.Param.ToArray());

                if (optionModel.IsSuccess)
                    result.writeReturn.IsSuccess = BaseExecute.ToBool(cmd, optionModel.Sql);
                else
                    result.writeReturn.IsSuccess = false;

                if (isTrans && result.writeReturn.IsSuccess)
                    SubmitTrans();
                else if (isTrans && result.writeReturn.IsSuccess == false)
                    RollbackTrans();
            }
            catch (Exception ex)
            {
                if (isTrans)
                    RollbackTrans();

                Task.Run(() =>
                {
                    if (config.SqlErrorType.ToLower() == SqlErrorType.Db)
                        DbLogTable.LogException<T>(config, ex, "Delete<T>", "");
                    else
                        DbLog.LogException<T>(config.IsOutError, config.DbType, ex, "Delete<T>", result.sql);
                });

                result.writeReturn.IsSuccess = false;
                result.writeReturn.Message = ex.Message;
            }

            return result;
        }
        #endregion

        #region 修改(Lambda表达式)
        /// <summary>
        /// 修改(Lambda表达式)
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="model">实体</param>
        /// <param name="predicate">表达式</param>
        /// <returns></returns>
        public DataReturn<T> Update<T>(T model, Expression<Func<T, bool>> predicate, Expression<Func<T, object>> field = null, bool isTrans = false) where T : class, new()
        {
            string sql = "";
            var result = new DataReturn<T>();
            var visitModel = new VisitModel();
            var update = new OptionModel();

            try
            {
                if (isTrans)
                    BeginTrans();

                update = BaseModel.UpdateToSql<T>(model, config, field);

                if (update.IsSuccess)
                {
                    visitModel = VisitExpression.LambdaWhere<T>(predicate, config);

                    sql = string.Format("{0} {1}", update.Sql, string.IsNullOrEmpty(visitModel.Where) ? "" : string.Format("where {0}", visitModel.Where.Replace(string.Format("{0}.", predicate.Parameters[0].Name), "")));

                    cmd.Parameters.Clear();

                    if (update.Param.Count != 0)
                        cmd.Parameters.AddRange(update.Param.ToArray());

                    if (visitModel.Param.Count != 0)
                        cmd.Parameters.AddRange(visitModel.Param.ToArray());

                    result.sql = ParameterToSql.ObjectParamToSql(Parameter.ParamMerge(update.Param, visitModel.Param), sql, config);

                    if (visitModel.IsSuccess)
                        result.writeReturn.IsSuccess = BaseExecute.ToBool(cmd, sql);
                    else
                        result.writeReturn.IsSuccess = false;
                }
                else
                    result.writeReturn.IsSuccess = false;

                if (isTrans && result.writeReturn.IsSuccess)
                    SubmitTrans();
                else if (isTrans && result.writeReturn.IsSuccess == false)
                    RollbackTrans();
            }
            catch (Exception ex)
            {
                if (isTrans)
                    RollbackTrans();

                Task.Run(() =>
                {
                    if (config.SqlErrorType.ToLower() == SqlErrorType.Db)
                        DbLogTable.LogException<T>(config, ex, "Update<T>", "");
                    else
                        DbLog.LogException<T>(config.IsOutError, config.DbType, ex, "Update<T>", result.sql);
                });
                result.writeReturn.IsSuccess = false;
                result.writeReturn.Message = ex.Message;
            }

            return result;
        }
        #endregion

        #region 修改
        /// <summary>
        /// 修改
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="field"></param>
        /// <param name="isTrans"></param>
        /// <returns></returns>
        public DataReturn<T> Update<T>(T model, Expression<Func<T, object>> field = null, bool isTrans = false) where T : class, new()
        {
            var result = new DataReturn<T>();
            var update = new OptionModel();
            try
            {
                update = BaseModel.UpdateToSql<T>(cmd, model, config, field);
                if (isTrans)
                    BeginTrans();
                if (update.IsSuccess)
                {
                    cmd.Parameters.Clear();

                    if (update.Param.Count != 0)
                        cmd.Parameters.AddRange(update.Param.ToArray());

                    result.sql = ParameterToSql.ObjectParamToSql(update.Param, update.Sql, config);
                    result.writeReturn.IsSuccess = BaseExecute.ToBool(cmd, update.Sql);
                }
                else
                {
                    result.writeReturn.Message = update.Message;
                    result.writeReturn.IsSuccess = false;
                }

                if (isTrans && result.writeReturn.IsSuccess)
                    SubmitTrans();
                else if (isTrans && result.writeReturn.IsSuccess == false)
                    RollbackTrans();
            }
            catch (Exception ex)
            {
                if (isTrans)
                    RollbackTrans();

                Task.Run(() =>
                {
                    if (config.SqlErrorType.ToLower() == SqlErrorType.Db)
                        DbLogTable.LogException<T>(config, ex, "Update<T>", "");
                    else
                        DbLog.LogException<T>(config.IsOutError, config.DbType, ex, "Update<T>", result.sql);
                });
                result.writeReturn.IsSuccess = false;
                result.writeReturn.Message = ex.Message;
            }

            return result;
        }
        #endregion

        #region 修改list
        /// <summary>
        /// 修改list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="field"></param>
        /// <param name="isTrans"></param>
        /// <returns></returns>
        public DataReturn<T> UpdateList<T>(List<T> list, Expression<Func<T, object>> field = null) where T : class, new()
        {
            var result = new DataReturn<T>();
            var update = new OptionModel();
            try
            {
                if (list.Count == 0)
                {
                    result.writeReturn.IsSuccess = false;
                    result.writeReturn.Message = "更新数据不能为空";
                    return result;
                }

                update = BaseModel.UpdateListToSql<T>(cmd,list, config, field);
    
                if (update.IsSuccess)
                {
                    using (var adapter = DbProviderFactories.GetFactory(config).CreateDataAdapter())
                    {
                        BeginTrans();                        
                        cmd.Parameters.Clear();
                        adapter.InsertCommand = cmd;
                        adapter.InsertCommand.CommandText = update.Sql;
                        adapter.InsertCommand.UpdatedRowSource = UpdateRowSource.None;
                        adapter.UpdateBatchSize = 0;

                        if (update.Param.Count != 0)
                            adapter.InsertCommand.Parameters.AddRange(update.Param.ToArray());

                        result.sql = ParameterToSql.ObjectParamToSql(update.Param, update.Sql, config);

                        result.writeReturn.IsSuccess = adapter.Update(update.table) > 0;
                        if (result.writeReturn.IsSuccess)
                            SubmitTrans();
                        else
                            RollbackTrans();
                    }
                }
                else
                {
                    result.writeReturn.Message = update.Message;
                    result.writeReturn.IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                Task.Run(() =>
                {
                    if (config.SqlErrorType.ToLower() == SqlErrorType.Db)
                        DbLogTable.LogException<T>(config, ex, "UpdateList<T>", "");
                    else
                        DbLog.LogException<T>(config.IsOutError, config.DbType, ex, "UpdateList<T>", result.sql);
                });
                result.writeReturn.IsSuccess = false;
                result.writeReturn.Message = ex.Message;
            }

            return result;
        }
        #endregion

        #region 增加
        /// <summary>
        /// 增加
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="model">实体</param>
        /// <returns></returns>
        public DataReturn<T> Add<T>(T model, bool isTrans = false) where T : class, new()
        {
            var result = new DataReturn<T>();
            var insert = new OptionModel();

            try
            {
                if (isTrans)
                    BeginTrans();

                insert = BaseModel.InsertToSql<T>(model, config);

                if (insert.IsSuccess)
                {
                    result.sql = ParameterToSql.ObjectParamToSql(insert.Param, insert.Sql, config);

                    cmd.Parameters.Clear();

                    if (insert.Param.Count != 0)
                        cmd.Parameters.AddRange(insert.Param.ToArray());

                    result.writeReturn.IsSuccess = BaseExecute.ToBool(cmd, insert.Sql);

                    if (isTrans && result.writeReturn.IsSuccess)
                        SubmitTrans();
                    else if (isTrans && result.writeReturn.IsSuccess == false)
                        RollbackTrans();

                    return result;
                }
                else
                    return result;
            }
            catch (Exception ex)
            {
                if (isTrans)
                    RollbackTrans();

                Task.Run(() =>
                {
                    if (config.SqlErrorType.ToLower() == SqlErrorType.Db)
                        DbLogTable.LogException<T>(config, ex, "Add<T>", "");
                    else
                        DbLog.LogException<T>(config.IsOutError, config.DbType, ex, "Add<T>", result.sql);
                });
                result.writeReturn.Message = ex.Message;
                result.writeReturn.IsSuccess = false;
                return result;
            }
        }
        #endregion

        #region 批量增加 
        /// <summary>
        /// 批量增加
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="IsTrans"></param>
        /// <param name="IsAsync"></param>
        /// <returns></returns>
        public DataReturn<T> AddList<T>(List<T> list,bool isLog=false) where T : class, new()
        {
            var result = new DataReturn<T>();
            var sql = new StringBuilder();
            var dyn = new Property.DynamicGet<T>();

            try
            {
                BeginTrans();
                
                if (config.DbType == DataDbType.Oracle)
                {
                    #region oracle
                    if (!isLog)
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = string.Format("alter table {0} nologging", typeof(T).Name);
                        cmd.ExecuteNonQuery();
                    }

                    cmd.GetType().GetMethods().ToList().ForEach(a => {
                        if (a.Name == "set_ArrayBindCount")
                        {
                            var param = new object[1];
                            param[0] = list.Count;
                            a.Invoke(cmd, param);
                        }

                        if (a.Name == "set_BindByName")
                        {
                            var param = new object[1];
                            param[0] = true;
                            a.Invoke(cmd, param);
                        }
                    });

                    sql.AppendFormat("insert into {0} values(", typeof(T).Name);
                    PropertyCache.GetPropertyInfo<T>().ForEach(a => {
                        object[] pValue = new object[list.Count];
                        var param = DbProviderFactories.GetFactory(config).CreateParameter();
                        if (a.PropertyType.Name.ToLower() == "nullable`1")
                            param.DbType = CommandParam.GetOracleDbType(a.PropertyType.GetGenericArguments()[0].Name);
                        else
                            param.DbType = CommandParam.GetOracleDbType(a.PropertyType.Name);

                        param.Direction = ParameterDirection.Input;
                        param.ParameterName = a.Name;
                        sql.AppendFormat("{0}{1},", config.Flag, a.Name);

                        for (var i = 0; i < list.Count; i++)
                        {
                            var value = dyn.GetValue(list[i], a.Name, true);
                            if (value == null)
                                value = DBNull.Value;
                            pValue[i] = value;
                        }

                        param.Value = pValue;
                        cmd.Parameters.Add(param);
                    });

                    sql.Append(")");
                    cmd.CommandText = sql.ToString().Replace(",)", ")");
                    result.writeReturn.IsSuccess = cmd.ExecuteNonQuery() > 0;

                    if (!isLog)
                    {
                        cmd.CommandText = string.Format("alter table {0} logging", typeof(T).Name);
                        cmd.ExecuteNonQuery();
                    }
                    #endregion
                }

                if (config.DbType == DataDbType.SqlServer)
                {
                    #region sqlserver
                    CommandParam.InitTvps<T>(cmd);
                    cmd.Parameters.Clear();
                    foreach (var method in cmd.Parameters.GetType().GetMethods())
                    {
                        if (method.Name == "AddWithValue")
                        {
                            var param = new object[2];
                            param[0] = string.Format("@{0}", typeof(T).Name);
                            param[1] = CommandParam.GetTable<T>(cmd, list);
                            var sqlParam = method.Invoke(cmd.Parameters, param);

                            sqlParam.GetType().GetMethods().ToList().ForEach(a => {
                                if (a.Name == "set_SqlDbType")
                                {
                                    param = new object[1];
                                    param[0] = SqlDbType.Structured;
                                    a.Invoke(sqlParam, param);
                                }
                                if (a.Name == "set_TypeName")
                                {
                                    param = new object[1];
                                    param[0] = typeof(T).Name;
                                    a.Invoke(sqlParam, param);
                                }
                            });
                            break;
                        }
                    }

                    cmd.CommandText = CommandParam.GetTvps<T>();
                    result.writeReturn.IsSuccess = cmd.ExecuteNonQuery() > 0;
                    #endregion
                }

                if (config.DbType == DataDbType.MySql)
                {
                    #region mysql
                    cmd.CommandText = CommandParam.GetMySql<T>(list);
                    result.writeReturn.IsSuccess = cmd.ExecuteNonQuery() > 0;
                    #endregion
                }

                if (config.DbType == DataDbType.SQLite)
                {
                    #region sqlite



                    #endregion
                }

                if (result.writeReturn.IsSuccess)
                    SubmitTrans();
                else if (result.writeReturn.IsSuccess == false)
                    RollbackTrans();
            }
            catch (Exception ex)
            {
                RollbackTrans();

                Task.Run(() =>
                {
                    if (config.SqlErrorType.ToLower() == SqlErrorType.Db)
                        DbLogTable.LogException<T>(config, ex, "AddList<T>", "");
                    else
                        DbLog.LogException<T>(config.IsOutError, config.DbType, ex, "AddList<T>", result.sql);
                });

                result.writeReturn.Message = ex.Message;
                result.writeReturn.IsSuccess = false;
            }

            return result;
        }
        #endregion

        #region 执行sql
        /// <summary>
        /// 执行sql
        /// </summary>
        /// <returns></returns>
        public DataReturn ExecuteSql(string sql, DbParameter[] param, bool isTrans = false, bool isLog = false, bool IsProcedure = false)
        {
            var result = new DataReturn();
            try
            {
                if (isTrans)
                    BeginTrans();

                if (param != null)
                    result.Sql = ParameterToSql.ObjectParamToSql(param.ToList(), sql, config);
                else
                    result.Sql = sql;

                DbLog.LogSql(isLog, result.Sql, config.DbType, 0);

                cmd.Parameters.Clear();

                if (param != null)
                    cmd.Parameters.AddRange(param.ToArray());

                result.writeReturn.IsSuccess = BaseExecute.ToBool(cmd, sql,IsProcedure);

                if (isTrans && result.writeReturn.IsSuccess)
                    SubmitTrans();
                else if (isTrans && result.writeReturn.IsSuccess == false)
                    RollbackTrans();
            }
            catch (Exception ex)
            {
                if (isTrans)
                    RollbackTrans();

                Task.Run(() => 
                {
                    if (config.SqlErrorType == SqlErrorType.Db)
                        DbLogTable.LogException(config, ex, "ExecuteSql", result.Sql);
                    else
                        DbLog.LogException(config.IsOutError, config.DbType, ex, "ExecuteSql", result.Sql);
                });
                result.writeReturn.IsSuccess = false;
                result.writeReturn.Message = ex.Message;
            }

            return result;
        }
        #endregion

        #region 开始事务
        public void BeginTrans()
        {
            this.trans = this.conn.BeginTransaction();
            this.cmd.Transaction = trans;
        }
        #endregion

        #region 提交事务
        public void SubmitTrans()
        {
            this.trans.Commit();
        }
        #endregion

        #region 回滚事务
        public void RollbackTrans()
        {
            this.trans.Rollback();
        }
        #endregion
    }
}
