using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Data.Common;
using Untility.Core.Page;
using Untility.Core.Base;
using Data.Core.Base;
using Data.Core.Model;
using Data.Core.Type;

namespace Data.Core.Context
{
    public class ReadContext : IDisposable
    {
        //变量
        public ConfigModel config;
        private DbConnection conn;
        private DbCommand cmd;

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
        public ReadContext(string key = null, ConfigModel config = null)
        {
            try
            {
                this.config = config == null ? DataConfig.Read(key) : config;
                conn = DbProviderFactories.GetFactory(this.config).CreateConnection();
                conn.ConnectionString = this.config.ConnStr;
                conn.Open();
                cmd = conn.CreateCommand();
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() =>
                {
                    DbLog.LogException(true,  this.config.DbType, ex, "LambdaReadBase", "");
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
                    result.item = BaseDataReader.ToList<T>(dr, item.Config, item.AsName).First<T>();
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

                    if (pModel.StarId > pModel.TotalPage)
                        pModel.StarId = pModel.TotalPage;

                    var dr = BaseExecute.ToPageDataReader(item, cmd, pModel, ref sql);
                    result.pageResult.list = BaseDataReader.ToList<T>(dr, item.Config, item.AsName);
                    result.sql = sql;

                    dr.Close();
                    dr.Dispose();
                }

                result.pageResult.pModel = pModel;
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() =>
                {
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

                    var dr = BaseExecute.ToPageDataReader(item, cmd, pModel, ref sql);
                    result.PageResult.list = BaseJson.DataReaderToDic(dr);
                    result.Sql = sql;

                    dr.Close();
                    dr.Dispose();
                }

                result.PageResult.pModel = pModel;
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() =>
                {
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

                    var dr = BaseExecute.ToPageDataReaderSql(param, cmd, pModel, sql, config, ref pageSql);

                    result.PageResult.list = BaseJson.DataReaderToDic(dr);
                    result.Sql = string.Format("count:{0},page:{1}", countSql, pageSql);

                    dr.Close();
                    dr.Dispose();
                }

                result.PageResult.pModel = pModel;
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() =>
                {
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
                    
                    if (pModel.StarId > pModel.TotalPage)
                        pModel.StarId = pModel.TotalPage;
                    
                    var dr = BaseExecute.ToPageDataReaderSql(param, cmd, pModel, sql, config, ref pageSql);

                    result.pageResult.list = BaseDataReader.ToList<T>(dr, config, null);
                    result.sql = string.Format("count:{0},page:{1}", countSql, pageSql);

                    dr.Close();
                    dr.Dispose();
                }

                result.pageResult.pModel = pModel;
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() =>
                {
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

                cmd.Parameters.Clear();

                if (isLog)
                    Task.Factory.StartNew(() =>
                    {
                        DbLog.LogSql(true, result.Sql, config.DbType, 0, "codefirst");
                    });

                if (param != null)
                    cmd.Parameters.AddRange(Parameter.ReNewParam(param.ToList(), config).ToArray());

                var dr = BaseExecute.ToDataReader(cmd, sql);

                result.DicList = BaseJson.DataReaderToDic(dr);

                dr.Close();
                dr.Dispose();
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() =>
                {
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

                result.Sql = ParameterToSql.ObjectParamToSql(param, sql.ToString(), item.Config);

                cmd.Parameters.Clear();

                if (param.Count != 0)
                    cmd.Parameters.AddRange(param.ToArray());

                var dr = BaseExecute.ToDataReader(cmd, sql.ToString());

                if (item.Take == 1)
                    result.Dic = BaseJson.DataReaderToDic(dr).FirstOrDefault();
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
                    DbLog.LogException(item.Config.IsOutError, item.Config.DbType, ex, "GetJson", result.Sql);
                });
                return result;
            }
        }
        #endregion
    }
}
