using FastData.Core.Base;
using FastData.Core.Context;
using FastData.Core.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FastUntility.Core.Page;
using System.Data;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace FastData.Core
{
    /// <summary>
    /// orm查询
    /// </summary>
    public static class FastRead
    {
        #region 查询join
        /// <summary>
        /// 查询join
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <typeparam name="T1">泛型</typeparam>
        /// <param name="joinType">left join,right join,inner join</param>
        /// <param name="item"></param>
        /// <param name="predicate">条件</param>
        /// <param name="field">字段</param>
        /// <returns></returns>
        private static DataQuery JoinType<T, T1>(string joinType, DataQuery item, Expression<Func<T, T1, bool>> predicate, Expression<Func<T1, object>> field = null, bool isDblink = false)
        {
            item.TableName.Add(typeof(T1).Name);
            item.TableAsName.Add(typeof(T1).Name, predicate.Parameters[1].Name);
            item.TableAsName.SetValue(typeof(T1).Name, predicate.Parameters[1].Name);
            var queryField = BaseField.QueryField<T, T1>(predicate, field, item);
            item.Field.Add(queryField.Field);
            item.AsName.AddRange(queryField.AsName);

            var condtion = VisitExpression.LambdaWhere<T, T1>(predicate, item);
            item.Predicate.Add(condtion);
            item.Table.Add(string.Format("{2} {0}{3} {1}", typeof(T1).Name, predicate.Parameters[1].Name
            , joinType, isDblink && !string.IsNullOrEmpty(item.Config.DbLinkName) ? string.Format("@", item.Config.DbLinkName) : ""));

            return item;
        }
        #endregion

        #region 是否过滤
        /// <summary>
        /// 是否过滤
        /// </summary>
        /// <param name="isFilter"></param>
        /// <returns></returns>
        public static DataQuery Filter(this DataQuery item, bool isFilter = true)
        {
            item.IsFilter = isFilter;
            return item;
        }
        #endregion

        #region 表查询
        /// <summary>
        /// 表查询
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="predicate">条件</param>
        /// <param name="field">字段</param>
        /// <param name="Key"></param>
        /// <returns></returns>
        public static DataQuery Query<T>(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> field = null, string key = null, string dbFile = "db.json")
        {
            var result = new DataQuery();

            var projectName = FastDataExtension.config.Current.GetName().Name;

            var cacheKey = $"FastData.Key.{typeof(Microsoft.Extensions.DependencyInjection.ConfigKey).Name}";

            if (DbCache.Exists(CacheType.Web, cacheKey) && key == null)
                key = DbCache.Get<Microsoft.Extensions.DependencyInjection.ConfigKey>(CacheType.Web, cacheKey).dbKey;
            else if (DataConfig.DataType(key, projectName, dbFile) && key == null)
                throw new Exception("数据库查询key不能为空,数据库类型有多个");

            result.Config = DataConfig.Get(key, projectName, dbFile);
            result.Key = key;

            result.TableName.Add(typeof(T).Name);
            result.TableAsName.Add(typeof(T).Name, predicate.Parameters[0].Name);

            var queryField = BaseField.QueryField<T>(predicate, field, result);
            result.Field.Add(queryField.Field);
            result.AsName.AddRange(queryField.AsName);

            var condtion = VisitExpression.LambdaWhere<T>(predicate, result);
            result.Predicate.Add(condtion);
            result.Table.Add(string.Format("{0} {1}", typeof(T).Name, predicate.Parameters[0].Name));
            return result;
        }
        #endregion

        #region 表查询
        /// <summary>
        /// 表查询
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="predicate">条件</param>
        /// <param name="field">字段</param>
        /// <param name="Key"></param>
        /// <returns></returns>
        public static FastQueryable<T> Queryable<T>(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> field = null, string key = null, string dbFile = "db.json") where T : class, new()
        {
            var result = new FastQueryable<T>();
            var projectName = FastDataExtension.config.Current.GetName().Name;

            var cacheKey = $"FastData.Key.{typeof(Microsoft.Extensions.DependencyInjection.ConfigKey).Name}";

            if (DbCache.Exists(CacheType.Web, cacheKey) && key == null)
                key = DbCache.Get<Microsoft.Extensions.DependencyInjection.ConfigKey>(CacheType.Web, cacheKey).dbKey;
            else if (DataConfig.DataType(key, projectName, dbFile) && key == null)
                throw new Exception("数据库查询key不能为空,数据库类型有多个");

            result.Query.Config = DataConfig.Get(key, projectName, dbFile);
            result.Query.Key = key;

            result.Query.TableName.Add(typeof(T).Name);
            result.Query.TableAsName.Add(typeof(T).Name, predicate.Parameters[0].Name);

            var queryField = BaseField.QueryField<T>(predicate, field, result.Query);
            result.Query.Field.Add(queryField.Field);
            result.Query.AsName.AddRange(queryField.AsName);

            var condtion = VisitExpression.LambdaWhere<T>(predicate, result.Query);
            result.Query.Predicate.Add(condtion);
            result.Query.Table.Add(string.Format("{0} {1}", typeof(T).Name, predicate.Parameters[0].Name));
            result.Query.TableName.Add(typeof(T).Name);
            return result;
        }
        #endregion

        #region 查询left join
        /// <summary>
        /// 查询left join
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="item"></param>
        /// <param name="predicate"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static DataQuery LeftJoin<T, T1>(this DataQuery item, Expression<Func<T, T1, bool>> predicate, Expression<Func<T1, object>> field = null, bool isDblink = false)
        {
            return JoinType("left join", item, predicate, field);
        }
        #endregion

        #region 查询right join
        /// <summary>
        /// 查询right join
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="item"></param>
        /// <param name="predicate"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static DataQuery RightJoin<T, T1>(this DataQuery item, Expression<Func<T, T1, bool>> predicate, Expression<Func<T1, object>> field = null, bool isDblink = false) where T1 : class, new()
        {
            return JoinType("right join", item, predicate, field);
        }
        #endregion

        #region 查询inner join
        /// <summary>
        /// 查询inner join
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="item"></param>
        /// <param name="predicate"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static DataQuery InnerJoin<T, T1>(this DataQuery item, Expression<Func<T, T1, bool>> predicate, Expression<Func<T1, object>> field = null, bool isDblink = false) where T1 : class, new()
        {
            return JoinType("inner join", item, predicate, field);
        }
        #endregion

        #region 查询order by
        /// <summary>
        /// 查询order by
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static DataQuery OrderBy<T>(this DataQuery item, Expression<Func<T, object>> field, bool isDesc = true)
        {
            var orderBy = BaseField.OrderBy<T>(field, item, isDesc);
            item.OrderBy.AddRange(orderBy);
            return item;
        }
        #endregion

        #region 查询group by
        /// <summary>
        /// 查询group by
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static DataQuery GroupBy<T>(this DataQuery item, Expression<Func<T, object>> field)
        {
            var groupBy = BaseField.GroupBy<T>(field, item);
            item.GroupBy.AddRange(groupBy);
            return item;
        }
        #endregion

        #region 查询take
        /// <summary>
        /// 查询take
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static DataQuery Take(this DataQuery item, int i)
        {
            item.Take = i;
            return item;
        }
        #endregion


        #region 返回list
        /// <summary>
        /// 返回list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static List<T> ToList<T>(this DataQuery item, DataContext db = null, bool isOutSql = false) where T : class, new()
        {
            var stopwatch = new Stopwatch();
            var result = new DataReturn<T>();

            if (item.Predicate.Exists(a => a.IsSuccess == false))
                return result.List;

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(item.Key))
                {
                    result = tempDb.GetList<T>(item);
                }
            }
            else
                result = db.GetList<T>(item);

            stopwatch.Stop();
            item.Config.IsOutSql = item.Config.IsOutSql ? item.Config.IsOutSql : isOutSql;
            DbLog.LogSql(item.Config.IsOutSql, result.Sql, item.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;
            return result.List;
        }
        #endregion

        #region 返回list asy
        /// <summary>
        /// 返回list asy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static ValueTask<List<T>> ToListAsy<T>(this DataQuery item, DataContext db = null, bool isOutSql = false) where T : class, new()
        {
            return new ValueTask<List<T>>(ToList<T>(item, db, isOutSql));
        }
        #endregion

        #region 返回lazy<list>
        /// <summary>
        /// 返回lazy<list>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static Lazy<List<T>> ToLazyList<T>(this DataQuery item, DataContext db = null, bool isOutSql = false) where T : class, new()
        {
            return new Lazy<List<T>>(() => ToList<T>(item, db, isOutSql));
        }
        #endregion

        #region 返回lazy<list> asy
        /// <summary>
        /// 返回lazy<list> asy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static ValueTask<Lazy<List<T>>> ToLazyListAsy<T>(this DataQuery item, DataContext db = null, bool isOutSql = false) where T : class, new()
        {
            return new ValueTask<Lazy<List<T>>>(new Lazy<List<T>>(() => ToList<T>(item, db, isOutSql)));
        }
        #endregion


        #region 返回json
        /// <summary>
        /// 返回json
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string ToJson(this DataQuery item, DataContext db = null, bool isOutSql = false)
        {
            var result = new DataReturn();
            var stopwatch = new Stopwatch();

            if (item.Predicate.Exists(a => a.IsSuccess == false))
                return result.Json;

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(item.Key))
                {
                    result = tempDb.GetJson(item);
                }
            }
            else
                result = db.GetJson(item);

            stopwatch.Stop();
            item.Config.IsOutSql = item.Config.IsOutSql ? item.Config.IsOutSql : isOutSql;
            DbLog.LogSql(item.Config.IsOutSql, result.Sql, item.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;
            return result.Json;
        }
        #endregion

        #region 返回json asy
        /// <summary>
        /// 返回json asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static ValueTask<string> ToJsonAsy(this DataQuery item, DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<string>(ToJson(item, db, isOutSql));
        }
        #endregion

        #region 返回lazy<json>
        /// <summary>
        /// 返回lazy<json>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static Lazy<string> ToLazyJson(this DataQuery item, DataContext db = null, bool isOutSql = false)
        {
            return new Lazy<string>(() => ToJson(item, db, isOutSql));
        }
        #endregion

        #region 返回lazy<json> asy
        /// <summary>
        /// 返回lazy<json> asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static ValueTask<Lazy<string>> ToLazyJsonAsy(this DataQuery item, DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<Lazy<string>>(new Lazy<string>(() => ToJson(item, db, isOutSql)));
        }
        #endregion


        #region 返回item
        /// <summary>
        /// 返回item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static T ToItem<T>(this DataQuery item, DataContext db = null, bool isOutSql = false) where T : class, new()
        {
            var result = new DataReturn<T>();
            var stopwatch = new Stopwatch();

            if (item.Predicate.Exists(a => a.IsSuccess == false))
                return result.Item;

            stopwatch.Start();

            item.Take = 1;

            if (db == null)
            {
                using (var tempDb = new DataContext(item.Key))
                {
                    result = tempDb.GetList<T>(item);
                }
            }
            else
                result = db.GetList<T>(item);

            stopwatch.Stop();
            item.Config.IsOutSql = item.Config.IsOutSql ? item.Config.IsOutSql : isOutSql;
            DbLog.LogSql(item.Config.IsOutSql, result.Sql, item.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;
            return result.Item;
        }
        #endregion

        #region 返回item asy
        /// <summary>
        /// 返回item asy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static ValueTask<T> ToItemAsy<T>(this DataQuery item, DataContext db = null, bool isOutSql = false) where T : class, new()
        {
            return new ValueTask<T>(ToItem<T>(item, db, isOutSql));
        }
        #endregion

        #region 返回Lazy<item>
        /// <summary>
        /// 返回Lazy<item>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static Lazy<T> ToLazyItem<T>(this DataQuery item, DataContext db = null, bool isOutSql = false) where T : class, new()
        {
            return new Lazy<T>(() => ToItem<T>(item, db, isOutSql));
        }
        #endregion

        #region 返回Lazy<item> asy
        /// <summary>
        /// 返回Lazy<item> asy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static ValueTask<Lazy<T>> ToLazyItemAsy<T>(this DataQuery item, DataContext db = null, bool isOutSql = false) where T : class, new()
        {
            return new ValueTask<Lazy<T>>(new Lazy<T>(() => ToItem<T>(item, db, isOutSql)));
        }
        #endregion


        #region 返回条数
        /// <summary>
        /// 返回条数
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static int ToCount(this DataQuery item, DataContext db = null, bool isOutSql = false)
        {
            var result = new DataReturn();
            var stopwatch = new Stopwatch();

            if (item.Predicate.Exists(a => a.IsSuccess == false))
                return result.Count;

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(item.Key))
                {
                    result = tempDb.GetCount(item);
                }
            }
            else
                result = db.GetCount(item);

            stopwatch.Stop();
            item.Config.IsOutSql = item.Config.IsOutSql ? item.Config.IsOutSql : isOutSql;
            DbLog.LogSql(item.Config.IsOutSql, result.Sql, item.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;

            return result.Count;
        }
        #endregion

        #region 返回条数 asy
        /// <summary>
        /// 返回条数 asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static ValueTask<int> ToCountAsy(this DataQuery item, DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<int>(ToCount(item, db, isOutSql));
        }
        #endregion


        #region 返回分页
        /// <summary>
        /// 返回分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="pModel"></param>
        /// <returns></returns>
        public static PageResult<T> ToPage<T>(this DataQuery item, PageModel pModel, DataContext db = null, bool isOutSql = false) where T : class, new()
        {
            var result = new DataReturn<T>();
            var stopwatch = new Stopwatch();

            if (item.Predicate.Exists(a => a.IsSuccess == false))
                return result.PageResult;

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(item.Key))
                {
                    result = tempDb.GetPage<T>(item, pModel);
                }
            }
            else
                result = db.GetPage<T>(item, pModel);

            stopwatch.Stop();
            item.Config.IsOutSql = item.Config.IsOutSql ? item.Config.IsOutSql : isOutSql;
            DbLog.LogSql(item.Config.IsOutSql, result.Sql, item.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;
            return result.PageResult;
        }
        #endregion

        #region 返回分页 asy
        /// <summary>
        /// 返回分页 asy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="pModel"></param>
        /// <returns></returns>
        public static ValueTask<PageResult<T>> ToPageAsy<T>(this DataQuery item, PageModel pModel, DataContext db = null, bool isOutSql = false) where T : class, new()
        {
            return new ValueTask<PageResult<T>>(ToPage<T>(item, pModel, db, isOutSql));
        }
        #endregion

        #region 返回分页lazy
        /// <summary>
        /// 返回分页lazy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="pModel"></param>
        /// <returns></returns>
        public static Lazy<PageResult<T>> ToLazyPage<T>(this DataQuery item, PageModel pModel, DataContext db = null, bool isOutSql = false) where T : class, new()
        {
            return new Lazy<PageResult<T>>(() => ToPage<T>(item, pModel, db, isOutSql));
        }
        #endregion

        #region 返回分页lazy asy
        /// <summary>
        /// 返回分页lazy asy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="pModel"></param>
        /// <returns></returns>
        public static ValueTask<Lazy<PageResult<T>>> ToLazyPageAsy<T>(this DataQuery item, PageModel pModel, DataContext db = null, bool isOutSql = false) where T : class, new()
        {
            return new ValueTask<Lazy<PageResult<T>>>(new Lazy<PageResult<T>>(() => ToPage<T>(item, pModel, db, isOutSql)));
        }
        #endregion


        #region 返回分页Dictionary<string, object>
        /// <summary>
        /// 返回分页Dictionary<string, object>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pModel"></param>
        /// <returns></returns>
        public static PageResult ToPage(this DataQuery item, PageModel pModel, DataContext db = null, bool isOutSql = false)
        {
            var result = new DataReturn();
            var stopwatch = new Stopwatch();

            if (item.Predicate.Exists(a => a.IsSuccess == false))
                return result.PageResult;

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(item.Key))
                {
                    result = tempDb.GetPage(item, pModel);
                }
            }
            else
                result = db.GetPage(item, pModel);

            stopwatch.Stop();
            item.Config.IsOutSql = item.Config.IsOutSql ? item.Config.IsOutSql : isOutSql;
            DbLog.LogSql(item.Config.IsOutSql, result.Sql, item.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;
            return result.PageResult;
        }
        #endregion

        #region 返回分页Dictionary<string, object> asy
        /// <summary>
        /// 返回分页Dictionary<string, object> asy
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pModel"></param>
        /// <returns></returns>
        public static ValueTask<PageResult> ToPageAsy(this DataQuery item, PageModel pModel, DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<PageResult>(ToPage(item, pModel, db, isOutSql));
        }
        #endregion

        #region 返回分页Dictionary<string, object> lazy
        /// <summary>
        /// 返回分页Dictionary<string, object> lazy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="pModel"></param>
        /// <returns></returns>
        public static Lazy<PageResult> ToLazyPage(this DataQuery item, PageModel pModel, DataContext db = null, bool isOutSql = false)
        {
            return new Lazy<PageResult>(() => ToPage(item, pModel, db, isOutSql));
        }
        #endregion

        #region 返回分页Dictionary<string, object> lazy asy
        /// <summary>
        /// 返回分页Dictionary<string, object> lazy asy
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pModel"></param>
        /// <returns></returns>
        public static ValueTask<Lazy<PageResult>> ToLazyPageAsy(this DataQuery item, PageModel pModel, DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<Lazy<PageResult>>(new Lazy<PageResult>(() => ToPage(item, pModel, db, isOutSql)));
        }
        #endregion


        #region DataTable
        /// <summary>
        /// DataTable
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static DataTable ToDataTable(this DataQuery item, DataContext db = null, bool isOutSql = false)
        {
            var result = new DataReturn();
            var stopwatch = new Stopwatch();

            if (item.Predicate.Exists(a => a.IsSuccess == false))
                return result.Table;

            stopwatch.Start();
            item.Take = 1;

            if (db == null)
            {
                using (var tempDb = new DataContext(item.Key))
                {
                    result = tempDb.GetDataTable(item);
                }
            }
            else
                result = db.GetDataTable(item);

            stopwatch.Stop();

            item.Config.IsOutSql = item.Config.IsOutSql ? item.Config.IsOutSql : isOutSql;
            DbLog.LogSql(item.Config.IsOutSql, result.Sql, item.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;
            return result.Table;
        }
        #endregion

        #region DataTable asy
        /// <summary>
        /// DataTable asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static ValueTask<DataTable> ToDataTableAsy(this DataQuery item, DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<DataTable>(ToDataTable(item, db, isOutSql));
        }
        #endregion

        #region DataTable lazy
        /// <summary>
        /// DataTable lazy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static Lazy<DataTable> ToLazyDataTable(this DataQuery item, DataContext db = null, bool isOutSql = false)
        {
            return new Lazy<DataTable>(() => ToDataTable(item, db, isOutSql));
        }
        #endregion

        #region DataTable lazy asy
        /// <summary>
        /// DataTable lazy asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static ValueTask<Lazy<DataTable>> ToLazyDataTableAsy(this DataQuery item, DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<Lazy<DataTable>>(new Lazy<DataTable>(() => ToDataTable(item, db, isOutSql)));
        }
        #endregion


        #region 执行sql
        /// <summary>
        /// 执行sql
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static List<T> ExecuteSql<T>(string sql, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false, bool isAop = true) where T : class, new()
        {
            ConfigModel config = null;
            var result = new DataReturn<T>();
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(key))
                {
                    result = tempDb.ExecuteSql<T>(sql, param, isAop);
                    config = tempDb.config;
                }
            }
            else
            {
                result = db.ExecuteSql<T>(sql, param, isAop);
                config = db.config;
            }

            stopwatch.Stop();
            config.IsOutSql = config.IsOutSql ? config.IsOutSql : isOutSql;
            DbLog.LogSql(config.IsOutSql, result.Sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;

            return result.List;
        }
        #endregion

        #region 执行sql asy
        /// <summary>
        /// 执行sql asy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static ValueTask<List<T>> ExecuteSqlAsy<T>(string sql, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            return new ValueTask<List<T>>(ExecuteSql<T>(sql, param, db, key, isOutSql));
        }
        #endregion

        #region 执行sql lazy
        /// <summary>
        /// 执行sql lazy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static Lazy<List<T>> ExecuteLazySql<T>(string sql, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            return new Lazy<List<T>>(() => ExecuteSql<T>(sql, param, db, key, isOutSql));
        }
        #endregion

        #region 执行sql lazy asy
        /// <summary>
        /// 执行sql lazy asy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static ValueTask<Lazy<List<T>>> ExecuteLazySqlAsy<T>(string sql, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            return new ValueTask<Lazy<List<T>>>(new Lazy<List<T>>(() => ExecuteSql<T>(sql, param, db, key, isOutSql)));
        }
        #endregion


        #region 返回List<Dictionary<string, object>>
        /// <summary>
        /// 返回List<Dictionary<string, object>>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static List<Dictionary<string, object>> ToDics(this DataQuery item, DataContext db = null, bool isOutSql = false)
        {
            var result = new DataReturn();
            var stopwatch = new Stopwatch();

            if (item.Predicate.Exists(a => a.IsSuccess == false))
                return result.DicList;

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(item.Key))
                {
                    result = tempDb.GetDic(item);
                }
            }
            else
                result = db.GetDic(item);

            stopwatch.Stop();
            item.Config.IsOutSql = item.Config.IsOutSql ? item.Config.IsOutSql : isOutSql;
            DbLog.LogSql(item.Config.IsOutSql, result.Sql, item.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;
            return result.DicList;
        }
        #endregion

        #region 返回List<Dictionary<string, object>> asy
        /// <summary>
        /// 返回List<Dictionary<string, object>> asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static ValueTask<List<Dictionary<string, object>>> ToDicsAsy(this DataQuery item, DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<List<Dictionary<string, object>>>(ToDics(item, db, isOutSql));
        }
        #endregion

        #region 返回lazy<List<Dictionary<string, object>>>
        /// <summary>
        /// 返回lazy<List<Dictionary<string, object>>>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static Lazy<List<Dictionary<string, object>>> ToLazyDics(this DataQuery item, DataContext db = null, bool isOutSql = false)
        {
            return new Lazy<List<Dictionary<string, object>>>(() => ToDics(item, db, isOutSql));
        }
        #endregion

        #region 返回lazy<List<Dictionary<string, object>>> asy
        /// <summary>
        /// 返回lazy<List<Dictionary<string, object>>> asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static ValueTask<Lazy<List<Dictionary<string, object>>>> ToLazyDicsAsy(this DataQuery item, DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<Lazy<List<Dictionary<string, object>>>>(new Lazy<List<Dictionary<string, object>>>(() => ToDics(item, db, isOutSql)));
        }
        #endregion


        #region Dictionary<string, object>
        /// <summary>
        /// Dictionary<string, object>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ToDic(this DataQuery item, DataContext db = null, bool isOutSql = false)
        {
            var result = new DataReturn();
            var stopwatch = new Stopwatch();

            if (item.Predicate.Exists(a => a.IsSuccess == false))
                return result.Dic;

            stopwatch.Start();
            item.Take = 1;

            if (db == null)
            {
                using (var tempDb = new DataContext(item.Key))
                {
                    result = tempDb.GetDic(item);
                }
            }
            else
                result = db.GetDic(item);

            stopwatch.Stop();

            item.Config.IsOutSql = item.Config.IsOutSql ? item.Config.IsOutSql : isOutSql;
            DbLog.LogSql(item.Config.IsOutSql, result.Sql, item.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;
            return result.Dic;
        }
        #endregion

        #region Dictionary<string, object> asy
        /// <summary>
        /// Dictionary<string, object> asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static ValueTask<Dictionary<string, object>> ToDicAsy(this DataQuery item, DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<Dictionary<string, object>>(ToDic(item, db, isOutSql));
        }
        #endregion

        #region Dictionary<string, object>
        /// <summary>
        /// Dictionary<string, object>>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static Lazy<Dictionary<string, object>> ToLazyDic(this DataQuery item, DataContext db = null, bool isOutSql = false)
        {
            return new Lazy<Dictionary<string, object>>(() => ToDic(item, db, isOutSql));
        }
        #endregion

        #region Dictionary<string, object> asy
        /// <summary>
        /// Dictionary<string, object> asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static ValueTask<Lazy<Dictionary<string, object>>> ToLazyDicAsy(this DataQuery item, DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<Lazy<Dictionary<string, object>>>(new Lazy<Dictionary<string, object>>(() => ToDic(item, db, isOutSql)));
        }
        #endregion


        #region 执行sql
        /// <summary>
        /// 执行sql
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static List<Dictionary<string, object>> ExecuteSql(string sql, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false, bool isAop = true)
        {
            ConfigModel config = null;
            var result = new DataReturn();
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(key))
                {
                    result = tempDb.ExecuteSqlList(sql, param, false, isAop);
                    config = tempDb.config;
                }
            }
            else
            {
                result = db.ExecuteSqlList(sql, param, false, isAop);
                config = db.config;
            }

            stopwatch.Stop();

            config.IsOutSql = config.IsOutSql ? config.IsOutSql : isOutSql;
            DbLog.LogSql(config.IsOutSql, result.Sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;

            return result.DicList;
        }
        #endregion

        #region 执行sql asy
        /// <summary>
        /// 执行sql asy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static ValueTask<List<Dictionary<string, object>>> ExecuteSqlAsy(string sql, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            return new ValueTask<List<Dictionary<string, object>>>(ExecuteSql(sql, param, db, key, isOutSql));
        }
        #endregion

        #region 执行sql lazy
        /// <summary>
        /// 执行sql lazy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static Lazy<List<Dictionary<string, object>>> ExecuteLazySql(string sql, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            return new Lazy<List<Dictionary<string, object>>>(() => ExecuteSql(sql, param, db, key, isOutSql));
        }
        #endregion

        #region 执行sql lazy asy
        /// <summary>
        /// 执行sql lazy asy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static ValueTask<Lazy<List<Dictionary<string, object>>>> ExecuteLazySqlAsy(string sql, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            return new ValueTask<Lazy<List<Dictionary<string, object>>>>(new Lazy<List<Dictionary<string, object>>>(() => ExecuteSql(sql, param, db, key, isOutSql)));
        }
        #endregion
    }
}