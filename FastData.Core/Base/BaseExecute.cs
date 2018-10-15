using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;
using FastUntility.Core.Page;
using FastData.Core.Type;
using FastData.Core.Model;
using FastUntility.Core.Base;

namespace FastData.Core.Base
{
    /// <summary>
    /// 执行数据库操作
    /// </summary>
    internal static class BaseExecute
    {
        #region 返回DataTable
        /// <summary>
        ///  返回DataTable
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="dbType"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static DataTable ToDataTable(DbCommand cmd, string sql, bool IsProcedure = false)
        {
            var dt = new DataTable();
            var dr = ToDataReader(cmd, sql, IsProcedure);

            dt.Load(dr);

            dr.Close();
            dr.Dispose();

            return dt;
        }
        #endregion

        #region 返回DataReader
        /// <summary>
        ///  返回DataReader
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="dbType"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static DbDataReader ToDataReader(DbCommand cmd, string sql, bool IsProcedure = false)
        {
            if (IsProcedure)
                cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = sql;
            return cmd.ExecuteReader();
        }
        #endregion

        #region 返回bool
        /// <summary>
        ///  返回bool
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="dbType"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static bool ToBool(DbCommand cmd, string sql, DataQuery item = null)
        {
            if (item != null)
            {
                Task.Factory.StartNew(() =>
                {
                    DbLog.LogSql(item.Config.IsOutSql, sql, item.Config.DbType);
                });
            }

            cmd.CommandText = sql;
            return cmd.ExecuteNonQuery() > 0;
        }
        #endregion
                
        #region 返回分页DataReader
        /// <summary>
        /// 返回分页DataReader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbType"></param>
        /// <param name="cmd"></param>
        /// <param name="pModel"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public static DbDataReader ToPageDataReader(DataQuery item, DbCommand cmd, PageModel pModel, ref string sql)
        {
            try
            {
                var table = new StringBuilder();
                var sb = new StringBuilder();
                var param = new List<DbParameter>();
                
                cmd.Parameters.Clear();

                table.Append(item.Table[0]);
                for (var i = 1; i < item.Predicate.Count; i++)
                {
                    table.AppendFormat(" {0} on {1}", item.Table[i], item.Predicate[i].Where);

                    if (item.Predicate[i].Param.Count != 0)
                        param.AddRange(Parameter.ReNewParam(item.Predicate[i].Param,item.Config));
                }
                                
                if (item.Predicate[0].Param.Count != 0)
                    param.AddRange(Parameter.ReNewParam(item.Predicate[0].Param,item.Config));

                if (item.Config.DbType == DataDbType.SqlServer)
                {
                    #region sqlserver
                    var orderByLenght = item.Predicate[0].Where.IndexOf("order by");
                    sb.AppendFormat(@"select top {0} * from (select row_number()over({5})temprownumber,* 
                                        from (select tempcolumn=0,{3} from {1} where {4})t)tt where temprownumber>={2}"
                                            , pModel.PageSize
                                            , table
                                            , pModel.StarId - 1
                                            , string.Join(",", item.Field)
                                            , orderByLenght == -1 ? item.Predicate[0].Where : item.Predicate[0].Where.Substring(0, orderByLenght)
                                            , orderByLenght == -1 ? "order by tempcolumn" : item.Predicate[0].Where.Substring(orderByLenght, item.Predicate[0].Where.Length - orderByLenght));
                    #endregion
                }
                else if (item.Config.DbType == DataDbType.Oracle)
                {
                    #region oracle
                    sb = new StringBuilder();
                    if (item.Predicate.Count > 0)
                    {
                        sb.AppendFormat("select * from(select field.*,ROWNUM RN from(select {0} from {1} where {2}) field where rownum<={3}) where rn>={4}"
                                        , string.Join(",", item.Field)
                                        , table
                                        , item.Predicate[0].Where
                                        , pModel.EndId
                                        , pModel.StarId);
                    }
                    else
                    { 
                        sb.AppendFormat(@"select * from {3} 
                                    where rowid in(select rid from 
                                    (select rownum rn,rid from 
                                    (select rowid rid from {3}) 
                                    where rownum<={0}) where rn>{1}) and {4}"
                                        , pModel.EndId.ToString()
                                        , (pModel.StarId - 1).ToString()
                                        , string.Join(",", item.Field)
                                        , table
                                        , item.Predicate[0].Where);
                    }
                    #endregion
                }
                else if (item.Config.DbType == DataDbType.MySql)
                {
                    #region MySql
                    sb.AppendFormat("select {2} from {3} where {4} limit {0}, {1}"
                                       , pModel.StarId
                                       , pModel.PageSize
                                       , string.Join(",", item.Field)
                                       , table
                                       , item.Predicate[0].Where);
                    #endregion
                }
                else if (item.Config.DbType == DataDbType.DB2)
                {
                    #region DB2
                    var orderByLenght = item.Predicate[0].Where.IndexOf("order by");
                    sb.AppendFormat("select * from (select row_number() over ({5}) as row_number,{2} from {3} where {4}) a where a.row_number>{0} and row_number<{1} "
                                       , pModel.StarId
                                       , pModel.EndId
                                       , string.Join(",", item.Field)
                                       , table
                                       , item.Predicate[0].Where == "" ? "" : item.Predicate[0].Where
                                       , orderByLenght == -1 ? "" : item.Predicate[0].Where.Substring(orderByLenght, item.Predicate[0].Where.Length - orderByLenght));
                    #endregion
                }
                else if (item.Config.DbType == DataDbType.SQLite)
                {
                    #region sqlite
                    sb.AppendFormat("select {2} from {3} where {4} limit {1} offset {0}"
                                       , pModel.StarId
                                       , pModel.PageSize
                                       , string.Join(",", item.Field)
                                       , table
                                       , item.Predicate[0].Where);
                    #endregion
                }
                else if (item.Config.DbType == DataDbType.PostgreSql)
                {
                    #region PostgreSql
                    sb.AppendFormat("select {2} from {3} where {4} limit {1} offset {0}"
                                       , pModel.StarId
                                       , pModel.PageSize
                                       , string.Join(",", item.Field)
                                       , table
                                       , item.Predicate[0].Where);
                    #endregion
                }

                if (param.Count != 0)
                    cmd.Parameters.AddRange(param.ToArray());
                cmd.CommandText = sb.ToString();
                sql = string.Format("count:{0},page:{1}", sql, ParameterToSql.ObjectParamToSql(param, sb.ToString(), item.Config));
                return cmd.ExecuteReader();
            }
            catch(Exception ex)
            {
                Task.Factory.StartNew(() =>
                {
                    if (item.Config.SqlErrorType == SqlErrorType.Db)
                        DbLogTable.LogException(item.Config, ex, "ToPageDataReader", "");
                    else
                        DbLog.LogException(true, item.Config.DbType, ex, "ToPageDataReader", "");
                });
                return null;
            }
        }
        #endregion
        
        #region 返回分页条数
        /// <summary>
        /// 返回分页条数
        /// </summary>
        /// <returns></returns>
        public static int ToPageCount(DataQuery item, DbCommand cmd,  ref string sql)
        {
            try
            {
                cmd.Parameters.Clear();
                var param = new List<DbParameter>();
                sql = string.Format("select count(0) from {0}", item.Table[0]);

                for (var i = 1; i < item.Predicate.Count; i++)
                {
                    sql = string.Format("{2} {0} on {1}", item.Table[i], item.Predicate[i].Where, sql);

                    if (item.Predicate[i].Param.Count != 0)
                        param.AddRange(Parameter.ReNewParam(item.Predicate[i].Param,item.Config));
                }

                if (!string.IsNullOrEmpty(item.Predicate[0].Where))
                    sql = string.Format("{1} where {0}", item.Predicate[0].Where, sql);

                if (item.Predicate[0].Param.Count != 0)
                    param.AddRange(Parameter.ReNewParam(item.Predicate[0].Param,item.Config));

                if(param.Count!=0)
                    cmd.Parameters.AddRange(param.ToArray());

                var dt = BaseExecute.ToDataTable(cmd, sql.ToString());

                return int.Parse(dt.Rows[0][0].ToString());
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() =>
                {
                    if (item.Config.SqlErrorType == SqlErrorType.Db)
                        DbLogTable.LogException(item.Config, ex, "ToPageCount", "");
                    else
                        DbLog.LogException(true, item.Config.DbType, ex, "ToPageCount", "");
                });

                return 0;
            }
        }
        #endregion

        #region 返回分页条数sql
        /// <summary>
        /// 返回分页条数sql
        /// </summary>
        /// <returns></returns>
        public static int ToPageCountSql(DbParameter[] param, DbCommand cmd, string sql, ConfigModel config, ref string tempSql)
        {
            try
            {
                cmd.Parameters.Clear();

                sql = string.Format("select count(0) from ({0})t", sql);

                tempSql = ParameterToSql.ObjectParamToSql(param.ToList(), sql, config);

                if (param != null)
                    cmd.Parameters.AddRange(Parameter.ReNewParam(param.ToList(), config).ToArray());
                
                var dt = BaseExecute.ToDataTable(cmd, sql.ToString());

                return int.Parse(dt.Rows[0][0].ToString());
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() =>
                {
                    if (config.SqlErrorType == SqlErrorType.Db)
                        DbLogTable.LogException(config, ex, "ToPageCountSql", "");
                    else
                        DbLog.LogException(config.IsOutError,config.DbType, ex, "ToPageCountSql", "");
                });
                return 0;
            }
        }
        #endregion

        #region 返回分页DataReader sql
        /// <summary>
        /// 返回分页DataReader sql
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbType"></param>
        /// <param name="cmd"></param>
        /// <param name="pModel"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public static DbDataReader ToPageDataReaderSql(DbParameter[] param, DbCommand cmd, PageModel pModel, string sql, ConfigModel config, ref string tempSql)
        {
            try
            {
                cmd.Parameters.Clear();

                if (config.DbType == DataDbType.Oracle)
                    sql = string.Format("select * from(select field.*,ROWNUM RN from({0}) field where rownum<={1}) where rn>={2}"
                                    , sql, pModel.EndId, pModel.StarId);

                if (config.DbType == DataDbType.SqlServer)
                    sql = string.Format(@"select top {1} * from (select row_number()over(order by tempcolumn)temprownumber,* 
                                         from(select tempcolumn = 0, * from ({0})t)tt)ttt where temprownumber >= {2}"
                                    , sql, pModel.PageSize, pModel.StarId - 1);

                if (config.DbType == DataDbType.MySql)
                    sql = string.Format("{0} limit {1},{2}", sql, pModel.StarId, pModel.PageSize);

                if (config.DbType == DataDbType.DB2)
                    sql = string.Format("select * from(select field.*,ROWNUM RN from({0}) field where rownum<={1}) where rn>={2}"
                                    , sql, pModel.EndId, pModel.StarId);

                if (config.DbType == DataDbType.PostgreSql)
                    sql = string.Format("{0} limit {1} offset {2}"  , sql, pModel.PageSize, pModel.StarId);

                if (config.DbType == DataDbType.SQLite)
                    sql = string.Format("{0} limit {1} offset {2}", sql, pModel.PageSize, pModel.StarId);
                
                tempSql = ParameterToSql.ObjectParamToSql(param.ToList(), sql, config);

                if (param != null)
                    cmd.Parameters.AddRange(Parameter.ReNewParam(param.ToList(), config).ToArray());

                cmd.CommandText = sql;

                return cmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() =>
                {
                    if (config.SqlErrorType == SqlErrorType.Db)
                        DbLogTable.LogException(config, ex, "ToPageDataReaderSql", "");
                    else
                        DbLog.LogException(config.IsOutError, config.DbType, ex, "ToPageDataReaderSql", "");
                });
                return null;
            }
        }
        #endregion
    }
}
