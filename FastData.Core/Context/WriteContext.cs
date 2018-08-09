using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Data.Common;
using Data.Core.Base;
using Data.Core.Model;
using Data.Core.Type;
using System.Threading.Tasks;

namespace Data.Core.Context
{
    /// <summary>
    /// 数据库操作基类
    /// </summary>
    public class WriteContext : IDisposable
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
        /// <param name="ConnStr">连接字符串</param>
        public WriteContext(string key=null,ConfigModel config=null)
        {
            try
            {
                this.config = config == null ? DataConfig.Read(key) : config;
                conn = DbProviderFactories.GetFactory(this.config).CreateConnection();
                conn.ConnectionString = this.config.ConnStr;
                conn.Open();
                cmd = conn.CreateCommand();
            }
            catch(Exception ex)
            {
                Task.Factory.StartNew(() =>
                {
                    DbLog.LogException(true, this.config.DbType, ex, "DataBase", "");
                });
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
        public DataReturn<T> Delete<T>(Expression<Func<T, bool>> predicate, bool isTrans = false) where T :class, new()
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
                    cmd.Parameters.AddRange(Parameter.ReNewParam(visitModel.Param,config).ToArray());
                
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
                    DbLog.LogException<T>(config.IsOutError, config.DbType, ex, "Delete<T>", result.sql);
                });

                result.writeReturn.IsError = true;
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

                update = BaseModel.UpdateToSql<T>(model,predicate, config, field);
                
                if (update.Result)
                {
                    visitModel = VisitExpression.LambdaWhere<T>(predicate, config);
                    
                    sql = string.Format("{0} {1}", update.Sql, string.IsNullOrEmpty(visitModel.Where) ? "" : string.Format("where {0}", visitModel.Where.Replace(string.Format("{0}.", predicate.Parameters[0].Name), "")));
                    
                    cmd.Parameters.Clear();

                    if (update.Param.Count!=0)
                        cmd.Parameters.AddRange(Parameter.ReNewParam(update.Param, config).ToArray());

                    if (visitModel.Param.Count != 0)
                        cmd.Parameters.AddRange(Parameter.ReNewParam(visitModel.Param,config).ToArray());

                    result.sql = ParameterToSql.ObjectParamToSql(Parameter.ParamMerge(update.Param, visitModel.Param), sql, config);
                    
                    result.writeReturn.IsSuccess = BaseExecute.ToBool(cmd,sql);                    
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
                    DbLog.LogException<T>(config.IsOutError, config.DbType, ex, "Update<T>", result.sql);
                });
                result.writeReturn.IsSuccess = false;
                result.writeReturn.IsError = true;
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
        public DataReturn<T> Add<T>(T model, Expression<Func<T, object>> notAddField = null, bool isTrans = false) where T : class,new()
        {
            var result = new DataReturn<T>();
            var insert = new OptionModel();

            try
            {
                if (isTrans)
                    BeginTrans();

                insert = BaseModel.InsertToSql<T>(model, config, notAddField);
                
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
                    DbLog.LogException<T>(config.IsOutError, config.DbType, ex, "Add<T>", result.sql);
                });
                result.writeReturn.IsError = true;
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
        public DataReturn<T> AddList<T>(List<T> list, bool isTrans = false) where T : class,new()
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
                    //    RedisInfo.SetDic(dic);
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
                    //    param.Value = RedisInfo.GetDic<object>(cacheKey.ToArray()).ToArray();
                    //    BaseCache.Clear(cacheKey);
                    //    cmd.Parameters.Add(param);                       
                    //}

                    //cmd.CommandText = sql.ToString();
                    //result.writeReturn.isSuccess = cmd.ExecuteNonQuery() > 0;
                    #endregion
                }

                if (config.DbType == DataDbType.SqlServer)
                {
                    #region sqlserver
                    cmd.Parameters.GetType().GetProperty("AddWithValue").SetValue(cmd.Parameters,"");

                    
                    #endregion
                }

                if (config.DbType == DataDbType.MySql)
                {
                    #region mysql



                    #endregion
                }

                if(config.DbType==DataDbType.SQLite)
                {


                }
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() =>
                {
                    DbLog.LogException<T>(config.IsOutError, config.DbType, ex, "Add<T>", result.sql);
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
        public DataReturn ExecuteSql(string sql, DbParameter[] param, bool isTrans = false,bool isLog=false)
        {
            var result = new DataReturn();
            try
            {
                if (isTrans)
                    BeginTrans();

                if (param != null)
                    result.Sql = ParameterToSql.ObjectParamToSql(param.ToList(), sql, config);

                if (isLog)
                    Task.Factory.StartNew(() =>
                    {
                        DbLog.LogSql(true, result.Sql, config.DbType, 0, "codefirst");
                    });

                cmd.Parameters.Clear();

                if (param != null)
                    cmd.Parameters.AddRange(Parameter.ReNewParam(param.ToList(), config).ToArray());

                result.writeReturn.IsSuccess = BaseExecute.ToBool(cmd, sql);

                if (isTrans)
                    SubmitTrans();
            }
            catch (Exception ex)
            {
                if (isTrans)
                    RollbackTrans();

                Task.Factory.StartNew(() => { DbLog.LogException(config.IsOutError, config.DbType, ex, "ExecuteSql", result.Sql); });
                result.writeReturn.IsSuccess = false;
                result.writeReturn.IsError = true;
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
