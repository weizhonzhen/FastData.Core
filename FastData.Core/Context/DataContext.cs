using System;
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
            GC.SuppressFinalize(this);
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
                Task.Factory.StartNew(() =>
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
                        param.AddRange(Parameter.ReNewParam(item.Predicate[i].Param,item.Config));
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
                    param.AddRange(Parameter.ReNewParam(item.Predicate[0].Param,item.Config));

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

                return result;
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() =>
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
                Task.Factory.StartNew(() =>
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
                Task.Factory.StartNew(() =>
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
                Task.Factory.StartNew(() =>
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
                Task.Factory.StartNew(() =>
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
                        param.AddRange(Parameter.ReNewParam(item.Predicate[i].Param,item.Config));
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
                    param.AddRange(Parameter.ReNewParam(item.Predicate[0].Param,item.Config));

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
                Task.Factory.StartNew(() =>
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
                        param.AddRange(Parameter.ReNewParam(item.Predicate[i].Param,item.Config));
                }

                if (!string.IsNullOrEmpty(item.Predicate[0].Where))
                {
                    sql.AppendFormat(" where {0}", item.Predicate[0].Where);

                    if (item.Predicate[0].Param.Count != 0)
                        param.AddRange(Parameter.ReNewParam(item.Predicate[0].Param,item.Config));
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
                Task.Factory.StartNew(() =>
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
                    cmd.Parameters.AddRange(Parameter.ReNewParam(param.ToList(), config).ToArray());

                var dr = BaseExecute.ToDataReader(cmd, sql);

                result.list = BaseDataReader.ToList<T>(dr, config);

                dr.Close();
                dr.Dispose();
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() =>
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

                if (isLog)
                    Task.Factory.StartNew(() =>
                    {
                        DbLog.LogSql(true, result.Sql, config.DbType, 0);
                    });

                if (param != null)
                    cmd.Parameters.AddRange(Parameter.ReNewParam(param.ToList(), config).ToArray());

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
                Task.Factory.StartNew(() =>
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
                        param.AddRange(Parameter.ReNewParam(item.Predicate[i].Param, item.Config));
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
                    param.AddRange(Parameter.ReNewParam(item.Predicate[0].Param, item.Config));

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
                Task.Factory.StartNew(() =>
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
                        param.AddRange(Parameter.ReNewParam(item.Predicate[i].Param, item.Config));
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
                    param.AddRange(Parameter.ReNewParam(item.Predicate[0].Param, item.Config));
                
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
                Task.Factory.StartNew(() =>
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
                    cmd.Parameters.AddRange(Parameter.ReNewParam(visitModel.Param, config).ToArray());

                result.writeReturn.IsSuccess = BaseExecute.ToBool(cmd, sql.ToString());

                if (isTrans)
                    SubmitTrans();
            }
            catch (Exception ex)
            {
                if (isTrans)
                    RollbackTrans();

                Task.Factory.StartNew(() =>
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

                update = BaseModel.UpdateToSql<T>(model, predicate, config, field);

                if (update.Result)
                {
                    visitModel = VisitExpression.LambdaWhere<T>(predicate, config);

                    sql = string.Format("{0} {1}", update.Sql, string.IsNullOrEmpty(visitModel.Where) ? "" : string.Format("where {0}", visitModel.Where.Replace(string.Format("{0}.", predicate.Parameters[0].Name), "")));

                    cmd.Parameters.Clear();

                    if (update.Param.Count != 0)
                        cmd.Parameters.AddRange(Parameter.ReNewParam(update.Param, config).ToArray());

                    if (visitModel.Param.Count != 0)
                        cmd.Parameters.AddRange(Parameter.ReNewParam(visitModel.Param, config).ToArray());

                    result.sql = ParameterToSql.ObjectParamToSql(Parameter.ParamMerge(update.Param, visitModel.Param), sql, config);

                    result.writeReturn.IsSuccess = BaseExecute.ToBool(cmd, sql);
                }
                else
                    result.writeReturn.IsSuccess = false;

                if (isTrans)
                    SubmitTrans();
            }
            catch (Exception ex)
            {
                if (isTrans)
                    RollbackTrans();

                Task.Factory.StartNew(() =>
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

                if (insert.Result)
                {
                    result.sql = ParameterToSql.ObjectParamToSql(insert.Param, insert.Sql, config);

                    cmd.Parameters.Clear();

                    if (insert.Param.Count != 0)
                        cmd.Parameters.AddRange(Parameter.ReNewParam(insert.Param, config).ToArray());

                    result.writeReturn.IsSuccess = BaseExecute.ToBool(cmd, insert.Sql);

                    if (isTrans)
                        SubmitTrans();

                    return result;
                }
                else
                    return result;
            }
            catch (Exception ex)
            {
                if (isTrans)
                    RollbackTrans();

                Task.Factory.StartNew(() =>
                {
                    if (config.SqlErrorType.ToLower() == SqlErrorType.Db)
                        DbLogTable.LogException<T>(config, ex, "Add<T>", "");
                    else
                        DbLog.LogException<T>(config.IsOutError, config.DbType, ex, "Add<T>", result.sql);
                });
                result.writeReturn.IsSuccess = false;
                result.writeReturn.Message = ex.Message;
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
        public DataReturn<T> AddList<T>(List<T> list, bool isTrans = false) where T : class, new()
        {
            var result = new DataReturn<T>();
            var sql = new StringBuilder();

            try
            {
                if (config.DbType == DataDbType.Oracle)
                {
                    #region oracle
                    //var key = Guid.NewGuid().ToString();
                    //var dynGet=new DynamicGet<T>();
                    //var propertyList = PropertyCache.GetPropertyInfo<T>(config.IsPropertyCache);

                    ////按列存入缓存
                    //foreach (var item in list)
                    //{
                    //    var dic = new Dictionary<string, object>();
                    //    dic.Add(string.Format("{0}{1}", item.GetType().Name, key), dynGet.GetValue(item, item.GetType().Name, config.IsPropertyCache) ?? DBNull.Value);
                    //    DbCache.SetDic(dic);
                    //}

                    //cmd.GetType().GetProperty("ArrayBindCount").SetValue(cmd, list.Count, null);
                    //cmd.GetType().GetProperty("BindByName").SetValue(cmd, true, null);

                    //sql.AppendFormat("insert into {0} values(", typeof(T).Name);

                    //foreach (var item in propertyList)
                    //{
                    //    var cacheKey = string.Format("{0}{1}", item.Name, key);
                    //    var param = DbProviderFactories.GetFactory(config.ProviderName).CreateParameter();                        
                    //    //param.GetType().GetProperty("DbType").SetValue(param, 11, null);//??                        
                    //    param.ParameterName = item.Name;
                    //    param.Direction = ParameterDirection.Input;
                    //    param.Value = DbCache.GetDic<object>(cacheKey.ToArray()).ToArray();
                    //    DbCache.Clear(cacheKey);
                    //    cmd.Parameters.Add(param);                       
                    //}

                    //cmd.CommandText = sql.ToString();
                    //result.writeReturn.isSuccess = cmd.ExecuteNonQuery() > 0;
                    #endregion
                }

                if (config.DbType == DataDbType.SqlServer)
                {
                    #region sqlserver
                    cmd.Parameters.GetType().GetProperty("AddWithValue").SetValue(cmd.Parameters, "");


                    #endregion
                }

                if (config.DbType == DataDbType.MySql)
                {
                    #region mysql



                    #endregion
                }

                if (config.DbType == DataDbType.SQLite)
                {


                }
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() =>
                {
                    if (config.SqlErrorType.ToLower() == SqlErrorType.Db)
                        DbLogTable.LogException<T>(config, ex, "AddList<T>", "");
                    else
                        DbLog.LogException<T>(config.IsOutError, config.DbType, ex, "AddList<T>", result.sql);
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

                if (isLog)
                    Task.Factory.StartNew(() =>
                    {
                        DbLog.LogSql(true, result.Sql, config.DbType, 0);
                    });

                cmd.Parameters.Clear();

                if (param != null)
                    cmd.Parameters.AddRange(Parameter.ReNewParam(param.ToList(), config).ToArray());

                result.writeReturn.IsSuccess = BaseExecute.ToBool(cmd, sql,IsProcedure);

                if (isTrans)
                    SubmitTrans();
            }
            catch (Exception ex)
            {
                if (isTrans)
                    RollbackTrans();

                Task.Factory.StartNew(() => 
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
