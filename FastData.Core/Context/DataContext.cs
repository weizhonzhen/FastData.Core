using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using FastUntility.Core.Page;
using FastData.Core.Base;
using FastData.Core.Model;
using FastData.Core.Type;
using System.Linq.Expressions;
using FastData.Core.Property;
using System.Data;
using FastUntility.Core.Base;
using DbProviderFactories = FastData.Core.Base.DbProviderFactories;
using FastData.Core.Aop;
using FastData.Core.CacheModel;
using System.Collections;
using FastUntility.Core;
using FastData.Core.Filter;

namespace FastData.Core.Context
{
    public class DataContext : IDisposable
    {
        public ConfigModel config;
        private DbConnection conn;
        private DbCommand cmd;
        private DbTransaction trans;

        #region Navigate
        /// <summary>
        /// Navigate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        private void Navigate<T>(DataReturn<T> model, ConfigModel config, bool isPage) where T : class, new()
        {
            try
            {
                var key = string.Format("{0}.{1}.navigate", typeof(T).Namespace, typeof(T).Name);
                if (DbCache.Exists(config.CacheType, key))
                {
                    var dynGet = new Property.DynamicGet<T>();
                    var list = DbCache.Get<List<NavigateModel>>(config.CacheType, key);
                    list.ForEach(a =>
                    {
                        var instance = Activator.CreateInstance(a.PropertyType);
                        List<T> data;
                        if (isPage)
                            data = model.pageResult.list;
                        else
                            data = model.list;

                        data.ForEach(d =>
                        {
                            var table = new List<string>();
                            var paramList = new List<DbParameter>();
                            var sql = new StringBuilder();

                            table.Add(a.PropertyType.Name);
                            sql.AppendFormat("select * from {0} where 1=1 ", a.PropertyType.Name);

                            cmd.Parameters.Clear();
                            for (var i = 0; i < a.Name.Count; i++)
                            {
                                sql.AppendFormat(" and {0}={1}{0} ", a.Name[i], config.Flag);

                                if (!string.IsNullOrEmpty(a.Appand[i]) && a.Appand[i].ToLower().TrimStart().StartsWith("and"))
                                    sql.Append(a.Appand[i]);

                                if (!string.IsNullOrEmpty(a.Appand[i]) && !a.Appand[i].ToLower().TrimStart().StartsWith("and"))
                                    sql.AppendFormat(" and {0} ", a.Appand[i]);

                                var param = DbProviderFactories.GetFactory(config).CreateParameter();
                                param.ParameterName = a.Name[i];
                                param.Value = dynGet.GetValue(d, a.Key[i]);
                                cmd.Parameters.Add(param);
                                paramList.Add(param);
                            }

                            if (config.DbType == DataDbType.Oracle && !a.IsList)
                                sql.Append(" and rownum <=1");
                            else if (config.DbType == DataDbType.DB2 && !a.IsList)
                                sql.Append(" and fetch first 1 rows only");
                            else if (config.DbType == DataDbType.MySql && !a.IsList)
                                sql.Append(" and limit 1");
                            else if (config.DbType == DataDbType.PostgreSql && !a.IsList)
                                sql.Append(" and limit 1");
                            else if (config.DbType == DataDbType.SQLite && !a.IsList)
                                sql.Append(" and limit 0 offset 1");

                            cmd.CommandText = sql.ToString();

                            BaseAop.AopBefore(table, cmd.CommandText, paramList, config, true, AopType.Navigate);
                            var dr = BaseExecute.ToDataReader(cmd, sql.ToString());
                            object result;

                            if (a.MemberType == typeof(List<Dictionary<string, object>>))
                                result = BaseJson.DataReaderToDic(dr, config.DbType == DataDbType.Oracle);
                            else if (a.MemberType == typeof(Dictionary<string, object>))
                                result = BaseJson.DataReaderToDic(dr, config.DbType == DataDbType.Oracle)?.FirstOrDefault() ?? new Dictionary<string, object>();
                            else if (a.IsList)
                                result = BaseDataReader.ToList(a.MemberType, instance, dr, config);
                            else
                                result = BaseDataReader.ToModel(instance, dr, config);

                            dr.Close();
                            BaseAop.AopAfter(table, cmd.CommandText, paramList, config, true, AopType.Navigate, result);
                            if (result != null)
                                d.GetType().GetProperties().ToList().ForEach(p =>
                                {
                                    if (p.Name == a.MemberName)
                                        p.SetValue(d, Convert.ChangeType(result, a.MemberType));
                                });
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                BaseAop.AopException(ex, "to List tableName:" + typeof(T).Name, AopType.Navigate, config);
                if (config.SqlErrorType.ToLower() == SqlErrorType.Db)
                    DbLogTable.LogException<T>(config, ex, "Navigate<T>", "");
                else
                    DbLog.LogException<T>(config.IsOutError, config.DbType, ex, "Navigate", "");
            }
        }
        #endregion

        #region fastread
        /// <summary>
        /// fastread
        /// </summary>
        /// <param name="model"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        internal object FastReadAttribute(ServiceModel model, List<DbParameter> param, PageModel pModel)
        {
            var sql = "";
            try
            {
                object result;
                sql = ParameterToSql.ObjectParamToSql(param, sql.ToString(), config);
                if (model.isPage && model.type == null)
                    result = GetPageSql(pModel, model.sql, param.ToArray()).PageResult;
                else if (model.isPage && model.type != null)
                    result = GetPageSql(model, param, pModel);
                else
                {
                    BaseAop.AopBefore(null, model.sql, param, config, true, AopType.FastRead);
                    cmd.Parameters.Clear();
                    var instance = Activator.CreateInstance(model.type);
                    cmd.Parameters.AddRange(param.ToArray());
                    var dr = BaseExecute.ToDataReader(cmd, model.sql);
                    if (model.type == typeof(List<Dictionary<string, object>>) && model.isList)
                        result = BaseJson.DataReaderToDic(dr, config.DbType == DataDbType.Oracle);
                    else if (model.type == typeof(Dictionary<string, object>))
                        result = BaseJson.DataReaderToDic(dr, config.DbType == DataDbType.Oracle)?.FirstOrDefault() ?? new Dictionary<string, object>();
                    else if (model.isList)
                    {
                        instance = Activator.CreateInstance(model.type.GetGenericArguments()[0]);
                        result = BaseDataReader.ToList(model.type, instance, dr, config);
                    }
                    else
                        result = BaseDataReader.ToModel(instance, dr, config);
                    dr.Close();
                    BaseAop.AopAfter(null, cmd.CommandText, param, config, true, AopType.FastRead, result);
                }

                return result;
            }
            catch(Exception ex)
            {
                BaseAop.AopException(ex, "FastReadAttribute", AopType.FastRead, config);
                if (config.SqlErrorType == SqlErrorType.Db)
                    DbLogTable.LogException(config, ex, "FastReadAttribute", sql);
                else
                    DbLog.LogException(config.IsOutError, config.DbType, ex, "FastReadAttribute", sql);
                return null;
            }
        }
        #endregion

        #region dispose parameter
        /// <summary>
        /// dispose parameter
        /// </summary>
        /// <param name="cmd"></param>
        private void Dispose(DbCommand cmd)
        {
            if (cmd == null) return;
            if (cmd.Parameters != null && config.DbType == DataDbType.Oracle)
            {
                foreach (var param in cmd.Parameters)
                {
                    param.GetType().GetMethods().ToList().ForEach(m =>
                    {
                        if (m.Name == "Dispose")
                            m.Invoke(param, null);
                    });
                }
            }
            cmd.Parameters.Clear();
        }
        #endregion

        #region 回收资源
        /// <summary>
        /// 回收资源
        /// </summary>
        public void Dispose()
        {
            Dispose(cmd);
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
                BaseAop.AopException(ex, "DataContext :" + key, AopType.DataContext, DataConfig.Get(key));
                if (config.SqlErrorType == SqlErrorType.Db)
                    DbLogTable.LogException(config, ex, "DataContext", "");
                else
                    DbLog.LogException(true, this.config.DbType, ex, "DataContext", "");
            }
        }
        #endregion

        #region 获取列表
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        public DataReturn<T> GetList<T>(DataQuery item) where T : class, new()
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

                if (item.IsFilter)
                    BaseFilter.Filter(param, FilterType.Query_List_Lambda, item.TableName, item.Config, sql);

                result.sql = ParameterToSql.ObjectParamToSql(param, sql.ToString(), item.Config);

                Dispose(cmd);

                if (param.Count != 0)
                    cmd.Parameters.AddRange(param.ToArray());

                BaseAop.AopBefore(item.TableName, sql.ToString(), param, config, true, AopType.Query_List_Lambda);

                var dr = BaseExecute.ToDataReader(cmd, sql.ToString());

                if (item.Take == 1)
                    result.item = BaseDataReader.ToList<T>(dr, item.Config, item.AsName).FirstOrDefault<T>() ?? new T();
                else
                    result.list = BaseDataReader.ToList<T>(dr, item.Config, item.AsName);

                dr.Close();
                dr.Dispose();
                dr = null;

                BaseAop.AopAfter(item.TableName, sql.ToString(), param, config, true, AopType.Query_List_Lambda, result.list);

                Navigate<T>(result, item.Config, false);

                return result;
            }
            catch (Exception ex)
            {
                BaseAop.AopException(ex, "to List tableName:" + typeof(T).Name, AopType.Query_List_Lambda, config);
                if (config.SqlErrorType.ToLower() == SqlErrorType.Db)
                    DbLogTable.LogException<T>(config, ex, "GetList<T>", "");
                else
                    DbLog.LogException<T>(item.Config.IsOutError, item.Config.DbType, ex, "GetList<T>", result.sql);
                return result;
            }
        }
        #endregion

        #region fastread page
        private object GetPageSql(ServiceModel model, List<DbParameter> param, PageModel pModel)
        {
            var result = Activator.CreateInstance(typeof(PageResult<>).MakeGenericType(model.type));
            var countSql = "";
            var pageSql = "";
            try
            {
                pModel.StarId = (pModel.PageId - 1) * pModel.PageSize + 1;
                pModel.EndId = pModel.PageId * pModel.PageSize;
                Dispose(cmd);
                pModel.TotalRecord = BaseExecute.ToPageCountSql(param.ToArray(), cmd, model.sql, config, ref countSql, FilterType.FastRead_Page, model.type.Name);
                if (pModel.TotalRecord > 0)
                {
                    if ((pModel.TotalRecord % pModel.PageSize) == 0)
                        pModel.TotalPage = pModel.TotalRecord / pModel.PageSize;
                    else
                        pModel.TotalPage = (pModel.TotalRecord / pModel.PageSize) + 1;

                    if (pModel.PageId > pModel.TotalPage)
                        pModel.PageId = pModel.TotalPage;

                    BaseAop.AopBefore(null, model.sql, param?.ToList(), config, true, AopType.FastRead_Page);

                    Dispose(cmd);
                    var dr = BaseExecute.ToPageDataReaderSql(param.ToArray(), cmd, pModel, model.sql, config, ref pageSql, FilterType.FastRead_Page, model.type.Name);

                    var instance = Activator.CreateInstance(model.type);
                    var type = typeof(List<>).MakeGenericType(model.type);
                    var list = BaseDataReader.ToList(type, instance, dr, config);

                    result.GetType().GetFields().ToList().ForEach(a => {
                        if (a.Name == "pModel")
                            a.SetValue(result, pModel);
                        if (a.Name == "list")
                            a.SetValue(result, list);
                    });

                    dr.Close();
                    dr.Dispose();
                    dr = null;

                    BaseAop.AopAfter(null, cmd.CommandText, param?.ToList(), config, true, AopType.FastRead_Page, list);
                }
            }
            catch (Exception ex)
            {
                var sql = string.Format("count:{0},page:{1}", countSql, pageSql);
                BaseAop.AopException(ex, "to Page tableName:" + model.type.Name, AopType.FastRead_Page, config);
                if (config.SqlErrorType == SqlErrorType.Db)
                    DbLogTable.LogException(config, ex, "GetPageSql", sql);
                else
                    DbLog.LogException(config.IsOutError, config.DbType, ex, "GetPageSql", sql);
            }

            return result;
        }
        #endregion

        #region 获取分页
        /// <summary>
        /// 获取分页
        /// </summary>
        /// <returns></returns>
        public DataReturn<T> GetPage<T>(DataQuery item, PageModel pModel) where T : class, new()
        {
            var param = new List<DbParameter>();
            var result = new DataReturn<T>();
            var sql = "";

            try
            {
                pModel.StarId = (pModel.PageId - 1) * pModel.PageSize + 1;
                pModel.EndId = pModel.PageId * pModel.PageSize;
                Dispose(cmd);
                pModel.TotalRecord = BaseExecute.ToPageCount(item, cmd, ref sql, FilterType.Query_Page_Lambda_Model);

                if (pModel.TotalRecord > 0)
                {
                    if ((pModel.TotalRecord % pModel.PageSize) == 0)
                        pModel.TotalPage = pModel.TotalRecord / pModel.PageSize;
                    else
                        pModel.TotalPage = (pModel.TotalRecord / pModel.PageSize) + 1;

                    if (pModel.PageId > pModel.TotalPage)
                        pModel.PageId = pModel.TotalPage;

                    BaseAop.AopBefore(item.TableName, sql, param, config, true, AopType.Query_Page_Lambda_Model);

                    Dispose(cmd);
                    var dr = BaseExecute.ToPageDataReader(item, cmd, pModel, ref sql, FilterType.Query_Page_Lambda_Model);

                    result.pageResult.list = BaseDataReader.ToList<T>(dr, item.Config, item.AsName);
                    result.sql = sql;

                    dr.Close();
                    dr.Dispose();
                    dr = null;

                    BaseAop.AopAfter(item.TableName, cmd.CommandText, param, config, true, AopType.Query_Page_Lambda_Model, result.pageResult.list);

                    Navigate<T>(result, item.Config, true);
                }
                else
                    result.pageResult.list = new List<T>();

                result.pageResult.pModel = pModel;
            }
            catch (Exception ex)
            {
                BaseAop.AopException(ex, "to Page tableName:" + typeof(T).Name, AopType.Query_Page_Lambda_Model, config);
                if (config.SqlErrorType.ToLower() == SqlErrorType.Db)
                    DbLogTable.LogException<T>(config, ex, "GetPage<T>", "");
                else
                    DbLog.LogException<T>(item.Config.IsOutError, item.Config.DbType, ex, "GetPage<T>", result.sql);
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
                Dispose(cmd);
                pModel.TotalRecord = BaseExecute.ToPageCount(item, cmd, ref sql, FilterType.Query_Page_Lambda_Dic);

                if (pModel.TotalRecord > 0)
                {
                    if ((pModel.TotalRecord % pModel.PageSize) == 0)
                        pModel.TotalPage = pModel.TotalRecord / pModel.PageSize;
                    else
                        pModel.TotalPage = (pModel.TotalRecord / pModel.PageSize) + 1;

                    if (pModel.PageId > pModel.TotalPage)
                        pModel.PageId = pModel.TotalPage;

                    BaseAop.AopBefore(item.TableName, sql, param, config, true, AopType.Query_Page_Lambda_Dic);

                    Dispose(cmd);
                    var dr = BaseExecute.ToPageDataReader(item, cmd, pModel, ref sql, FilterType.Query_Page_Lambda_Dic);

                    result.PageResult.list = BaseJson.DataReaderToDic(dr, config.DbType == DataDbType.Oracle);
                    result.Sql = sql;

                    dr.Close();
                    dr.Dispose();
                    dr = null;

                    BaseAop.AopAfter(item.TableName, cmd.CommandText, param, config, true, AopType.Query_Page_Lambda_Dic, result.PageResult.list);
                }
                else
                    result.PageResult.list = new List<Dictionary<string, object>>();

                result.PageResult.pModel = pModel;
            }
            catch (Exception ex)
            {
                BaseAop.AopException(ex, "to Page ", AopType.Query_Page_Lambda_Dic, config);
                if (config.SqlErrorType == SqlErrorType.Db)
                    DbLogTable.LogException(config, ex, "GetPage", result.Sql);
                else
                    DbLog.LogException(item.Config.IsOutError, item.Config.DbType, ex, "GetPage", result.Sql);
            }

            return result;
        }
        #endregion

        #region 获取分页sql
        /// <summary>
        /// 获取分页
        /// </summary>
        /// <returns></returns>
        public DataReturn GetPageSql(PageModel pModel, string sql, DbParameter[] param, bool isAop = true)
        {
            var result = new DataReturn();
            var countSql = "";
            var pageSql = "";

            try
            {
                pModel.StarId = (pModel.PageId - 1) * pModel.PageSize + 1;
                pModel.EndId = pModel.PageId * pModel.PageSize;
                Dispose(cmd);
                pModel.TotalRecord = BaseExecute.ToPageCountSql(param, cmd, sql, config, ref countSql, FilterType.Query_Page_Sql_Dic, null);

                if (pModel.TotalRecord > 0)
                {
                    if ((pModel.TotalRecord % pModel.PageSize) == 0)
                        pModel.TotalPage = pModel.TotalRecord / pModel.PageSize;
                    else
                        pModel.TotalPage = (pModel.TotalRecord / pModel.PageSize) + 1;

                    if (pModel.PageId > pModel.TotalPage)
                        pModel.PageId = pModel.TotalPage;

                    if (isAop)
                        BaseAop.AopBefore(null, sql, param?.ToList(), config, true, AopType.Query_Page_Sql_Dic);

                    Dispose(cmd);
                    var dr = BaseExecute.ToPageDataReaderSql(param, cmd, pModel, sql, config, ref pageSql, FilterType.Query_Page_Sql_Dic, null);

                    result.PageResult.list = BaseJson.DataReaderToDic(dr, config.DbType == DataDbType.Oracle);
                    result.Sql = string.Format("count:{0},page:{1}", countSql, pageSql);

                    dr.Close();
                    dr.Dispose();
                    dr = null;

                    if (isAop)
                        BaseAop.AopAfter(null, cmd.CommandText, param?.ToList(), config, true, AopType.Query_Page_Sql_Dic, result.PageResult.list);
                }
                else
                    result.PageResult.list = new List<Dictionary<string, object>>();

                result.PageResult.pModel = pModel;
            }
            catch (Exception ex)
            {
                BaseAop.AopException(ex, "to Page sql", AopType.Query_Page_Sql_Dic, config);
                if (config.SqlErrorType == SqlErrorType.Db)
                    DbLogTable.LogException(config, ex, "GetPageSql", result.Sql);
                else
                    DbLog.LogException(config.IsOutError, config.DbType, ex, "GetPageSql", result.Sql);
            }

            return result;
        }
        #endregion

        #region 获取分页sql
        /// <summary>
        /// 获取分页
        /// </summary>
        /// <returns></returns>
        public DataReturn<T> GetPageSql<T>(PageModel pModel, string sql, DbParameter[] param, bool isAop = true) where T : class, new()
        {
            var result = new DataReturn<T>();
            var countSql = "";
            var pageSql = "";

            try
            {
                pModel.StarId = (pModel.PageId - 1) * pModel.PageSize + 1;
                pModel.EndId = pModel.PageId * pModel.PageSize;
                Dispose(cmd);
                pModel.TotalRecord = BaseExecute.ToPageCountSql(param, cmd, sql, config, ref countSql, FilterType.Query_Page_Lambda_Model, typeof(T).Name);

                if (pModel.TotalRecord > 0)
                {
                    if ((pModel.TotalRecord % pModel.PageSize) == 0)
                        pModel.TotalPage = pModel.TotalRecord / pModel.PageSize;
                    else
                        pModel.TotalPage = (pModel.TotalRecord / pModel.PageSize) + 1;

                    if (pModel.PageId > pModel.TotalPage)
                        pModel.PageId = pModel.TotalPage;

                    if (isAop)
                        BaseAop.AopBefore(null, sql, param?.ToList(), config, true, AopType.Query_Page_Sql_Model);

                    Dispose(cmd);
                    var dr = BaseExecute.ToPageDataReaderSql(param, cmd, pModel, sql, config, ref pageSql, FilterType.Query_Page_Lambda_Model, typeof(T).Name);

                    result.pageResult.list = BaseDataReader.ToList<T>(dr, config, null);
                    result.sql = string.Format("count:{0},page:{1}", countSql, pageSql);

                    dr.Close();
                    dr.Dispose();
                    dr = null;

                    if (isAop)
                        BaseAop.AopAfter(null, cmd.CommandText, param?.ToList(), config, true, AopType.Query_Page_Sql_Model, result.pageResult.list);
                }
                else
                    result.pageResult.list = new List<T>();

                result.pageResult.pModel = pModel;
            }
            catch (Exception ex)
            {
                BaseAop.AopException(ex, "to Page tableName:" + typeof(T).Name, AopType.Query_Page_Sql_Model, config);
                if (config.SqlErrorType == SqlErrorType.Db)
                    DbLogTable.LogException<T>(config, ex, "GetPageSql", result.sql);
                else
                    DbLog.LogException(config.IsOutError, config.DbType, ex, "GetPageSql", result.sql);
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

                if (item.IsFilter)
                    BaseFilter.Filter(param, FilterType.Query_Json_Lambda, item.TableName, item.Config, sql);

                if (item.GroupBy.Count > 0)
                    sql.AppendFormat(" group by {0}", string.Join(",", item.GroupBy));

                if (item.OrderBy.Count > 0)
                    sql.AppendFormat(" order by {0}", string.Join(",", item.OrderBy));

                result.Sql = ParameterToSql.ObjectParamToSql(param, sql.ToString(), item.Config);

                Dispose(cmd);

                if (param.Count != 0)
                    cmd.Parameters.AddRange(param.ToArray());

                BaseAop.AopBefore(item.TableName, sql.ToString(), param.ToList(), config, true, AopType.Query_Json_Lambda);

                var dr = BaseExecute.ToDataReader(cmd, sql.ToString());

                result.Json = BaseJson.DataReaderToJson(dr, config.DbType == DataDbType.Oracle);

                dr.Close();
                dr.Dispose();
                dr = null;

                BaseAop.AopAfter(item.TableName, sql.ToString(), param.ToList(), config, true, AopType.Query_Json_Lambda, result.Json);

                return result;
            }
            catch (Exception ex)
            {
                BaseAop.AopException(ex, "to Json", AopType.Query_Json_Lambda, config);
                if (config.SqlErrorType == SqlErrorType.Db)
                    DbLogTable.LogException(config, ex, "GetJson", result.Sql);
                else
                    DbLog.LogException(item.Config.IsOutError, item.Config.DbType, ex, "GetJson", result.Sql);
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

                if (item.IsFilter)
                    BaseFilter.Filter(param, FilterType.Query_Count_Lambda, item.TableName, item.Config, sql);

                if (item.GroupBy.Count > 0)
                    sql.AppendFormat(" group by {0}", string.Join(",", item.GroupBy));

                if (item.OrderBy.Count > 0)
                    sql.AppendFormat(" order by {0}", string.Join(",", item.OrderBy));

                result.Sql = ParameterToSql.ObjectParamToSql(param, sql.ToString(), item.Config);

                Dispose(cmd);

                if (param.Count != 0)
                    cmd.Parameters.AddRange(param.ToArray());

                BaseAop.AopBefore(item.TableName, sql.ToString(), param.ToList(), config, true, AopType.Query_Count_Lambda);

                var dt = BaseExecute.ToDataTable(cmd, sql.ToString());

                if (dt.Rows.Count > 0)
                    result.Count = dt.Rows[0][0].ToString().ToInt(0);
                else
                    result.Count = 0;

                BaseAop.AopAfter(item.TableName, sql.ToString(), param.ToList(), config, true, AopType.Query_Count_Lambda, result.Count);

                return result;
            }
            catch (Exception ex)
            {
                BaseAop.AopException(ex, "to Count", AopType.Query_Count_Lambda, config);
                if (config.SqlErrorType == SqlErrorType.Db)
                    DbLogTable.LogException(config, ex, "GetCount", result.Sql);
                else
                    DbLog.LogException(item.Config.IsOutError, item.Config.DbType, ex, "GetCount", result.Sql);
                return result;
            }
        }
        #endregion

        #region 执行sql
        /// <summary>
        /// 执行sql
        /// </summary>
        /// <returns></returns>
        public DataReturn<T> ExecuteSql<T>(string sql, DbParameter[] param = null, bool isAop = true) where T : class, new()
        {
            var result = new DataReturn<T>();
            var tableName = new List<string>();
            try
            {
                tableName.Add(typeof(T).Name);

                if (param != null)
                    result.sql = ParameterToSql.ObjectParamToSql(param.ToList(), sql, config);
                else
                    result.sql = sql;

                Dispose(cmd);

                BaseFilter.Filter(param, FilterType.Execute_Sql_Model, tableName, config, ref sql);

                if (param != null)
                    cmd.Parameters.AddRange(param.ToArray());

                if (isAop)
                    BaseAop.AopBefore(tableName, sql, param?.ToList(), config, true, AopType.Execute_Sql_Model);

                var dr = BaseExecute.ToDataReader(cmd, sql);

                result.list = BaseDataReader.ToList<T>(dr, config);

                dr.Close();
                dr.Dispose();
                dr = null;

                if (isAop)
                    BaseAop.AopAfter(tableName, sql, param.ToList(), config, true, AopType.Execute_Sql_Model, result.list);
            }
            catch (Exception ex)
            {
                BaseAop.AopException(ex, "ExecuteSql tableName:" + typeof(T).Name, AopType.Execute_Sql_Model, config);
                if (config.SqlErrorType.ToLower() == SqlErrorType.Db)
                    DbLogTable.LogException<T>(config, ex, "ExecuteSql<T>", "");
                else
                    DbLog.LogException<T>(config.IsOutError, config.DbType, ex, "ExecuteSql<T>", result.sql);
            }

            return result;
        }
        #endregion

        #region 执行sql
        /// <summary>
        /// 执行sql
        /// </summary>
        /// <returns></returns>
        public DataReturn ExecuteSqlList(string sql, DbParameter[] param = null, bool isLog = false, bool isAop = true)
        {
            var result = new DataReturn();
            try
            {
                if (param != null)
                    result.Sql = ParameterToSql.ObjectParamToSql(param.ToList(), sql, config);
                else
                    result.Sql = sql;

                Dispose(cmd);

                DbLog.LogSql(isLog, result.Sql, config.DbType, 0);

                if (param != null)
                    cmd.Parameters.AddRange(param.ToArray());

                if (isAop)
                    BaseAop.AopBefore(null, sql, param?.ToList(), config, true, AopType.Execute_Sql_Dic);

                var dr = BaseExecute.ToDataReader(cmd, sql);

                result.writeReturn.IsSuccess = true;
                result.DicList = BaseJson.DataReaderToDic(dr, config.DbType == DataDbType.Oracle);

                dr.Close();
                dr.Dispose();
                dr = null;

                if (isAop)
                    BaseAop.AopAfter(null, sql, param.ToList(), config, true, AopType.Execute_Sql_Dic, result.DicList);
            }
            catch (Exception ex)
            {
                BaseAop.AopException(ex, "Excute Sql", AopType.Execute_Sql_Dic, config);
                result.writeReturn.IsSuccess = false;
                result.DicList = new List<Dictionary<string, object>>();

                if (config.SqlErrorType == SqlErrorType.Db)
                    DbLogTable.LogException(config, ex, "ExecuteSql", result.Sql);
                else
                    DbLog.LogException(config.IsOutError, config.DbType, ex, "ExecuteSql", result.Sql);
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
            object data;

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

                if (item.IsFilter)
                    BaseFilter.Filter(param, FilterType.Query_Dic_Lambda, item.TableName, item.Config, sql);

                if (item.GroupBy.Count > 0)
                    sql.AppendFormat(" group by {0}", string.Join(",", item.GroupBy));

                if (item.OrderBy.Count > 0)
                    sql.AppendFormat(" order by {0}", string.Join(",", item.OrderBy));

                result.Sql = ParameterToSql.ObjectParamToSql(param, sql.ToString(), item.Config);

                Dispose(cmd);

                if (param.Count != 0)
                    cmd.Parameters.AddRange(param.ToArray());

                BaseAop.AopBefore(null, sql.ToString(), param.ToList(), config, true, AopType.Query_Dic_Lambda);

                var dr = BaseExecute.ToDataReader(cmd, sql.ToString());

                if (item.Take == 1)
                {
                    result.Dic = BaseJson.DataReaderToDic(dr, config.DbType == DataDbType.Oracle).FirstOrDefault() ?? new Dictionary<string, object>();
                    data = result.Dic;
                }
                else
                {
                    result.DicList = BaseJson.DataReaderToDic(dr, config.DbType == DataDbType.Oracle);
                    data = result.DicList;
                }

                dr.Close();
                dr.Dispose();
                dr = null;

                BaseAop.AopAfter(null, sql.ToString(), param.ToList(), config, true, AopType.Query_Dic_Lambda, data);

                return result;
            }
            catch (Exception ex)
            {
                BaseAop.AopException(ex, "to Dic", AopType.Query_Dic_Lambda, config);
                if (config.SqlErrorType == SqlErrorType.Db)
                    DbLogTable.LogException(config, ex, "GetDic", result.Sql);
                else
                    DbLog.LogException(item.Config.IsOutError, item.Config.DbType, ex, "GetDic", result.Sql);
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

                if (item.IsFilter)
                    BaseFilter.Filter(param, FilterType.Query_DataTable_Lambda, item.TableName, item.Config, sql);

                if (item.GroupBy.Count > 0)
                    sql.AppendFormat(" group by {0}", string.Join(",", item.GroupBy));

                if (item.OrderBy.Count > 0)
                    sql.AppendFormat(" order by {0}", string.Join(",", item.OrderBy));

                result.Sql = ParameterToSql.ObjectParamToSql(param, sql.ToString(), item.Config);

                Dispose(cmd);

                if (param.Count != 0)
                    cmd.Parameters.AddRange(param.ToArray());

                BaseAop.AopBefore(item.Table, sql.ToString(), param.ToList(), config, true, AopType.Query_DataTable_Lambda);

                var dr = BaseExecute.ToDataReader(cmd, sql.ToString());

                result.Table.Load(dr);

                dr.Close();
                dr.Dispose();
                dr = null;

                BaseAop.AopAfter(null, sql.ToString(), param.ToList(), config, true, AopType.Query_DataTable_Lambda, result.Table);

                return result;
            }
            catch (Exception ex)
            {
                BaseAop.AopException(ex, "to DataTable", AopType.Query_DataTable_Lambda, config);
                if (config.SqlErrorType.ToLower() == SqlErrorType.Db)
                    DbLogTable.LogException(config, ex, "GetDataTable", result.Sql);
                else
                    DbLog.LogException(item.Config.IsOutError, item.Config.DbType, ex, "GetDataTable", result.Sql);
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
            var tableName = new List<string>();

            try
            {
                if (isTrans)
                    BeginTrans();

                visitModel = VisitExpression.LambdaWhere<T>(predicate, config);

                sql.AppendFormat("delete from {0} {1}", typeof(T).Name
                    , string.IsNullOrEmpty(visitModel.Where) ? "" : string.Format("where {0}", visitModel.Where.Replace(string.Format("{0}.", predicate.Parameters[0].Name), "")));

                result.sql = ParameterToSql.ObjectParamToSql(visitModel.Param, sql.ToString(), config);

                Dispose(cmd);

                if (visitModel.Param.Count != 0)
                    cmd.Parameters.AddRange(visitModel.Param.ToArray());

                tableName.Add(typeof(T).Name);
                BaseAop.AopBefore(tableName, sql.ToString(), visitModel.Param, config, false, AopType.Delete_Lambda);

                if (visitModel.IsSuccess)
                    result.writeReturn.IsSuccess = BaseExecute.ToBool(cmd, sql.ToString());
                else
                    result.writeReturn.IsSuccess = false;

                if (isTrans && result.writeReturn.IsSuccess)
                    SubmitTrans();
                else if (isTrans && result.writeReturn.IsSuccess == false)
                    RollbackTrans();

                BaseAop.AopAfter(tableName, sql.ToString(), visitModel.Param, config, false, AopType.Delete_Lambda, result.writeReturn.IsSuccess);
            }
            catch (Exception ex)
            {
                BaseAop.AopException(ex, "Delete by Lambda tableName" + typeof(T).Name, AopType.Delete_Lambda, config);

                if (isTrans)
                    RollbackTrans();

                if (config.SqlErrorType.ToLower() == SqlErrorType.Db)
                    DbLogTable.LogException<T>(config, ex, "Delete<T>", "");
                else
                    DbLog.LogException<T>(config.IsOutError, config.DbType, ex, "Delete<T>", result.sql);

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
        public DataReturn<T> Delete<T>(T model, bool isTrans = false) where T : class, new()
        {
            var result = new DataReturn<T>();
            var optionModel = new OptionModel();
            var tableName = new List<string>();

            try
            {
                if (isTrans)
                    BeginTrans();

                optionModel = BaseModel.DeleteToSql<T>(cmd, model, config);

                result.sql = ParameterToSql.ObjectParamToSql(optionModel.Param, optionModel.Sql, config);

                Dispose(cmd);

                if (optionModel.Param.Count != 0)
                    cmd.Parameters.AddRange(optionModel.Param.ToArray());

                tableName.Add(typeof(T).Name);
                BaseAop.AopBefore(tableName, optionModel.Sql, optionModel.Param, config, false, AopType.Delete_PrimaryKey);

                if (optionModel.IsSuccess)
                    result.writeReturn.IsSuccess = BaseExecute.ToBool(cmd, optionModel.Sql);
                else
                {
                    result.writeReturn.IsSuccess = false;
                    result.writeReturn.Message = optionModel.Message;
                }

                if (isTrans && result.writeReturn.IsSuccess)
                    SubmitTrans();
                else if (isTrans && result.writeReturn.IsSuccess == false)
                    RollbackTrans();

                BaseAop.AopAfter(tableName, optionModel.Sql, optionModel.Param, config, false, AopType.Delete_PrimaryKey, result.writeReturn.IsSuccess);
            }
            catch (Exception ex)
            {
                BaseAop.AopException(ex, "Delete by Primary Key tableName" + typeof(T).Name, AopType.Delete_PrimaryKey, config);

                if (isTrans)
                    RollbackTrans();

                if (config.SqlErrorType.ToLower() == SqlErrorType.Db)
                    DbLogTable.LogException<T>(config, ex, "Delete<T>", "");
                else
                    DbLog.LogException<T>(config.IsOutError, config.DbType, ex, "Delete<T>", result.sql);

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
            var tableName = new List<string>();

            try
            {
                if (isTrans)
                    BeginTrans();

                update = BaseModel.UpdateToSql<T>(model, config, field, cmd);

                if (update.IsSuccess)
                {
                    visitModel = VisitExpression.LambdaWhere<T>(predicate, config);

                    sql = string.Format("{0} {1}", update.Sql, string.IsNullOrEmpty(visitModel.Where) ? "" : string.Format("where {0}", visitModel.Where.Replace(string.Format("{0}.", predicate.Parameters[0].Name), "")));

                    Dispose(cmd);

                    if (update.Param.Count != 0)
                        cmd.Parameters.AddRange(update.Param.ToArray());

                    if (visitModel.Param.Count != 0)
                        cmd.Parameters.AddRange(visitModel.Param.ToArray());

                    result.sql = ParameterToSql.ObjectParamToSql(Parameter.ParamMerge(update.Param, visitModel.Param), sql, config);

                    tableName.Add(typeof(T).Name);
                    BaseAop.AopBefore(tableName, sql, Parameter.ParamMerge(update.Param, visitModel.Param), config, false, AopType.Update_Lambda);

                    if (visitModel.IsSuccess)
                        result.writeReturn.IsSuccess = BaseExecute.ToBool(cmd, sql);
                    else
                        result.writeReturn.IsSuccess = false;
                }
                else
                {
                    result.writeReturn.IsSuccess = false;
                    result.writeReturn.Message = update.Message;
                }

                if (isTrans && result.writeReturn.IsSuccess)
                    SubmitTrans();
                else if (isTrans && result.writeReturn.IsSuccess == false)
                    RollbackTrans();

                BaseAop.AopAfter(tableName, sql, Parameter.ParamMerge(update.Param, visitModel.Param), config, false, AopType.Update_Lambda, result.writeReturn.IsSuccess);
            }
            catch (Exception ex)
            {
                BaseAop.AopException(ex, "Update by Lambda tableName:" + typeof(T).Name, AopType.Update_Lambda, config);

                if (isTrans)
                    RollbackTrans();

                if (config.SqlErrorType.ToLower() == SqlErrorType.Db)
                    DbLogTable.LogException<T>(config, ex, "Update<T>", "");
                else
                    DbLog.LogException<T>(config.IsOutError, config.DbType, ex, "Update<T>", result.sql);

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
            var tableName = new List<string>();
            try
            {
                update = BaseModel.UpdateToSql<T>(cmd, model, config, field);
                if (isTrans)
                    BeginTrans();
                if (update.IsSuccess)
                {
                    Dispose(cmd);

                    if (update.Param.Count != 0)
                        cmd.Parameters.AddRange(update.Param.ToArray());

                    result.sql = ParameterToSql.ObjectParamToSql(update.Param, update.Sql, config);

                    tableName.Add(typeof(T).Name);
                    BaseAop.AopBefore(tableName, update.Sql, update.Param, config, false, AopType.Update_PrimaryKey);

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

                BaseAop.AopAfter(tableName, update.Sql, update.Param, config, false, AopType.Update_PrimaryKey, result.writeReturn.IsSuccess);
            }
            catch (Exception ex)
            {
                BaseAop.AopException(ex, "Update by Primary Key tableName:" + typeof(T).Name, AopType.Update_PrimaryKey, config);

                if (isTrans)
                    RollbackTrans();

                if (config.SqlErrorType.ToLower() == SqlErrorType.Db)
                    DbLogTable.LogException<T>(config, ex, "Update<T>", "");
                else
                    DbLog.LogException<T>(config.IsOutError, config.DbType, ex, "Update<T>", result.sql);

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
            var tableName = new List<string>();
            try
            {
                if (list.Count == 0)
                {
                    result.writeReturn.IsSuccess = false;
                    result.writeReturn.Message = "更新数据不能为空";
                    return result;
                }

                update = BaseModel.UpdateListToSql<T>(cmd, list, config, field);

                if (update.IsSuccess)
                {
                    using (var adapter = DbProviderFactories.GetFactory(config).CreateDataAdapter())
                    {
                        BeginTrans();
                        Dispose(cmd);
                        adapter.InsertCommand = cmd;
                        adapter.InsertCommand.CommandText = update.Sql;
                        adapter.InsertCommand.UpdatedRowSource = UpdateRowSource.None;
                        adapter.UpdateBatchSize = 0;

                        if (update.Param.Count != 0)
                            adapter.InsertCommand.Parameters.AddRange(update.Param.ToArray());

                        result.sql = ParameterToSql.ObjectParamToSql(update.Param, update.Sql, config);

                        tableName.Add(typeof(T).Name);
                        BaseAop.AopBefore(tableName, update.Sql, update.Param, config, false, AopType.UpdateList);

                        result.writeReturn.IsSuccess = adapter.Update(update.table) > 0;
                        if (result.writeReturn.IsSuccess)
                            SubmitTrans();
                        else
                            RollbackTrans();

                        BaseAop.AopAfter(tableName, update.Sql, update.Param, config, false, AopType.UpdateList, result.writeReturn.IsSuccess);
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
                BaseAop.AopException(ex, "Update List tableName:" + typeof(T).Name, AopType.UpdateList, config);

                if (config.SqlErrorType.ToLower() == SqlErrorType.Db)
                    DbLogTable.LogException<T>(config, ex, "UpdateList<T>", "");
                else
                    DbLog.LogException<T>(config.IsOutError, config.DbType, ex, "UpdateList<T>", result.sql);

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
            var tableName = new List<string>();

            try
            {
                if (isTrans)
                    BeginTrans();

                insert = BaseModel.InsertToSql<T>(model, config);

                if (insert.IsSuccess)
                {
                    result.sql = ParameterToSql.ObjectParamToSql(insert.Param, insert.Sql, config);

                    Dispose(cmd);

                    if (insert.Param.Count != 0)
                        cmd.Parameters.AddRange(insert.Param.ToArray());

                    tableName.Add(typeof(T).Name);
                    BaseAop.AopBefore(tableName, insert.Sql, insert.Param, config, false, AopType.Add);

                    result.writeReturn.IsSuccess = BaseExecute.ToBool(cmd, insert.Sql);

                    if (isTrans && result.writeReturn.IsSuccess)
                        SubmitTrans();
                    else if (isTrans && result.writeReturn.IsSuccess == false)
                        RollbackTrans();

                    BaseAop.AopAfter(tableName, insert.Sql, insert.Param, config, false, AopType.Add, result.writeReturn.IsSuccess);

                    return result;
                }
                else
                    return result;
            }
            catch (Exception ex)
            {
                BaseAop.AopException(ex, "Add tableName: " + typeof(T).Name, AopType.Add, config);

                if (isTrans)
                    RollbackTrans();

                if (config.SqlErrorType.ToLower() == SqlErrorType.Db)
                    DbLogTable.LogException<T>(config, ex, "Add<T>", "");
                else
                    DbLog.LogException<T>(config.IsOutError, config.DbType, ex, "Add<T>", result.sql);

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
        public DataReturn<T> AddList<T>(List<T> list, bool IsTrans = false, bool isLog = true) where T : class, new()
        {
            var result = new DataReturn<T>();
            var sql = new StringBuilder();
            var dyn = new Property.DynamicGet<T>();
            var tableName = new List<string>();

            try
            {
                if (IsTrans)
                    BeginTrans();

                if (config.DbType == DataDbType.Oracle)
                {
                    #region oracle
                    Dispose(cmd);
                    if (!isLog)
                    {
                        cmd.CommandText = string.Format("alter table {0} nologging", typeof(T).Name);
                        cmd.ExecuteNonQuery();
                    }

                    cmd.GetType().GetMethods().ToList().ForEach(a =>
                    {
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
                    PropertyCache.GetPropertyInfo<T>().ForEach(a =>
                    {
                        var pValue = new List<object>();
                        var param = DbProviderFactories.GetFactory(config).CreateParameter();
                        if (a.PropertyType.Name.ToLower() == "nullable`1")
                            param.DbType = CommandParam.GetOracleDbType(a.PropertyType.GetGenericArguments()[0].Name);
                        else
                            param.DbType = CommandParam.GetOracleDbType(a.PropertyType.Name);

                        param.Direction = ParameterDirection.Input;
                        param.ParameterName = a.Name;

                        sql.AppendFormat("{0}{1},", config.Flag, a.Name);

                        list.ForEach(l =>
                        {
                            var value = dyn.GetValue(l, a.Name);
                            if (value == null)
                                value = DBNull.Value;
                            pValue.Add(value);
                        });

                        param.Value = pValue.ToArray();
                        cmd.Parameters.Add(param);
                    });

                    sql.Append(")");
                    cmd.CommandText = sql.ToString().Replace(",)", ")");

                    tableName.Add(typeof(T).Name);
                    BaseAop.AopBefore(tableName, cmd.CommandText, null, config, false, AopType.AddList);

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
                    Dispose(cmd);
                    CommandParam.InitTvps<T>(cmd);
                    foreach (var method in cmd.Parameters.GetType().GetMethods())
                    {
                        if (method.Name == "AddWithValue")
                        {
                            var param = new object[2];
                            param[0] = string.Format("@{0}", typeof(T).Name);
                            param[1] = CommandParam.GetTable<T>(cmd, list);
                            var sqlParam = method.Invoke(cmd.Parameters, param);

                            sqlParam.GetType().GetMethods().ToList().ForEach(a =>
                            {
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

                    tableName.Add(typeof(T).Name);
                    BaseAop.AopBefore(tableName, cmd.CommandText, null, config, false, AopType.AddList);

                    result.writeReturn.IsSuccess = cmd.ExecuteNonQuery() > 0;
                    #endregion
                }

                if (config.DbType == DataDbType.MySql)
                {
                    #region mysql
                    Dispose(cmd);
                    cmd.CommandText = CommandParam.GetMySql<T>(list);

                    tableName.Add(typeof(T).Name);
                    BaseAop.AopBefore(tableName, cmd.CommandText, null, config, false, AopType.AddList);

                    result.writeReturn.IsSuccess = cmd.ExecuteNonQuery() > 0;
                    #endregion
                }

                if (config.DbType == DataDbType.SQLite)
                {
                    #region sqlite



                    #endregion
                }

                if (result.writeReturn.IsSuccess && IsTrans)
                    SubmitTrans();
                else if (result.writeReturn.IsSuccess == false && IsTrans)
                    RollbackTrans();

                BaseAop.AopAfter(tableName, cmd.CommandText, null, config, false, AopType.AddList, result.writeReturn.IsSuccess);
            }
            catch (Exception ex)
            {
                BaseAop.AopException(ex, "Add List tableName:" + typeof(T).Name, AopType.AddList, config);

                if (IsTrans)
                    RollbackTrans();

                if (config.SqlErrorType.ToLower() == SqlErrorType.Db)
                    DbLogTable.LogException<T>(config, ex, "AddList<T>", "");
                else
                    DbLog.LogException<T>(config.IsOutError, config.DbType, ex, "AddList<T>", result.sql);

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
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="type"></param>
        /// <param name="isTrans"></param>
        /// <param name="isLog"></param>
        /// <param name="IsProcedure"></param>
        /// <param name="isAop"></param>
        /// <returns></returns>
        internal DataReturn ExecuteSql(string sql, DbParameter[] param,AopType type, bool isTrans = false, bool isLog = false, bool IsProcedure = false, bool isAop = true)
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

                Dispose(cmd);

                if (param != null)
                    cmd.Parameters.AddRange(param.ToArray());

                if (isAop)
                    BaseAop.AopBefore(null, sql, param?.ToList(), config, false, type);

                result.writeReturn.IsSuccess = BaseExecute.ToBool(cmd, sql, IsProcedure);

                if (isTrans && result.writeReturn.IsSuccess)
                    SubmitTrans();
                else if (isTrans && result.writeReturn.IsSuccess == false)
                    RollbackTrans();

                if (isAop)
                    BaseAop.AopAfter(null, sql, param?.ToList(), config, false, type, result.writeReturn.IsSuccess);
            }
            catch (Exception ex)
            {
                BaseAop.AopException(ex, "Excute Sql", type, config);

                if (isTrans)
                    RollbackTrans();

                if (config.SqlErrorType == SqlErrorType.Db)
                    DbLogTable.LogException(config, ex, "ExecuteSql", result.Sql);
                else
                    DbLog.LogException(config.IsOutError, config.DbType, ex, "ExecuteSql", result.Sql);
                result.writeReturn.IsSuccess = false;
                result.writeReturn.Message = ex.Message;
            }

            return result;
        }

        #endregion

        #region 执行sql
        /// <summary>
        /// 执行sql
        /// </summary>
        /// <returns></returns>
        public DataReturn ExecuteSql(string sql, DbParameter[] param, bool isTrans = false, bool isLog = false, bool IsProcedure = false, bool isAop = true)
        {
            return ExecuteSql(sql, param, AopType.Execute_Sql_Bool, isTrans, isLog, IsProcedure, isAop);
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