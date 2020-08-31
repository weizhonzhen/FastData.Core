using FastData.Core.Base;
using FastData.Core.Context;
using FastData.Core.Model;
using FastUntility.Core.Page;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FastData.Core.Repository
{
    public class Query : IQuery
    {
        internal DataQuery Data { get; set; } = new DataQuery();

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
        private IQuery JoinType<T, T1>(string joinType, Expression<Func<T, T1, bool>> predicate, Expression<Func<T1, object>> field = null, bool isDblink = false)
        {
            var queryField = BaseField.QueryField<T, T1>(predicate, field, this.Data.Config);
            this.Data.Field.Add(queryField.Field);
            this.Data.AsName.AddRange(queryField.AsName);

            var condtion = VisitExpression.LambdaWhere<T, T1>(predicate, this.Data.Config);
            this.Data.Predicate.Add(condtion);
            this.Data.Table.Add(string.Format("{2} {0}{3} {1}", typeof(T1).Name, predicate.Parameters[1].Name
            , joinType, isDblink && !string.IsNullOrEmpty(this.Data.Config.DbLinkName) ? string.Format("@", this.Data.Config.DbLinkName) : ""));

            return this;
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
        public IQuery LeftJoin<T, T1>(Expression<Func<T, T1, bool>> predicate, Expression<Func<T1, object>> field = null, bool isDblink = false)
        {
            return JoinType("left join", predicate, field);
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
        public IQuery RightJoin<T, T1>(Expression<Func<T, T1, bool>> predicate, Expression<Func<T1, object>> field = null, bool isDblink = false) where T1 : class, new()
        {
            return JoinType("right join", predicate, field);
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
        public IQuery InnerJoin<T, T1>(Expression<Func<T, T1, bool>> predicate, Expression<Func<T1, object>> field = null, bool isDblink = false) where T1 : class, new()
        {
            return JoinType("inner join", predicate, field);
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
        public IQuery OrderBy<T>(Expression<Func<T, object>> field, bool isDesc = true)
        {
            var orderBy = BaseField.OrderBy<T>(field, this.Data.Config, isDesc);
            this.Data.OrderBy.AddRange(orderBy);
            return this;
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
        public IQuery GroupBy<T>(Expression<Func<T, object>> field)
        {
            var groupBy = BaseField.GroupBy<T>(field, this.Data.Config);
            this.Data.GroupBy.AddRange(groupBy);
            return this;
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
        public IQuery Take(int i)
        {
            this.Data.Take = i;
            return this;
        }
        #endregion


        #region 返回list
        /// <summary>
        /// 返回list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public List<T> ToList<T>(DataContext db = null) where T : class, new()
        {
            var stopwatch = new Stopwatch();
            var result = new DataReturn<T>();

            if (this.Data.Predicate.Exists(a => a.IsSuccess == false))
                return result.list;

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(this.Data.Key))
                {
                    result = tempDb.GetList<T>(this.Data);
                }
            }
            else
                result = db.GetList<T>(this.Data);

            stopwatch.Stop();

            DbLog.LogSql(this.Data.Config.IsOutSql, result.sql, this.Data.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            return result.list;
        }
        #endregion

        #region 返回list asy
        /// <summary>
        /// 返回list asy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<List<T>> ToListAsy<T>(DataContext db = null) where T : class, new()
        {
            return await Task.Factory.StartNew(() =>
            {
                return ToList<T>(db);
            });
        }
        #endregion

        #region 返回lazy<list>
        /// <summary>
        /// 返回lazy<list>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public Lazy<List<T>> ToLazyList<T>(DataContext db = null) where T : class, new()
        {
            return new Lazy<List<T>>(() => ToList<T>(db));
        }
        #endregion

        #region 返回lazy<list> asy
        /// <summary>
        /// 返回lazy<list> asy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<Lazy<List<T>>> ToLazyListAsy<T>(DataContext db = null) where T : class, new()
        {
            return await Task.Factory.StartNew(() =>
            {
                return new Lazy<List<T>>(() => ToList<T>(db));
            });
        }
        #endregion


        #region 返回json
        /// <summary>
        /// 返回json
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public string ToJson(DataContext db = null)
        {
            var result = new DataReturn();
            var stopwatch = new Stopwatch();

            if (this.Data.Predicate.Exists(a => a.IsSuccess == false))
                return result.Json;

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(this.Data.Key))
                {
                    result = tempDb.GetJson(this.Data);
                }
            }
            else
                result = db.GetJson(this.Data);

            stopwatch.Stop();

            DbLog.LogSql(this.Data.Config.IsOutSql, result.Sql, this.Data.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            return result.Json;
        }
        #endregion

        #region 返回json asy
        /// <summary>
        /// 返回json asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<string> ToJsonAsy(DataContext db = null)
        {
            return await Task.Factory.StartNew(() =>
            {
                return ToJson(db);
            });
        }
        #endregion

        #region 返回lazy<json>
        /// <summary>
        /// 返回lazy<json>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Lazy<string> ToLazyJson(DataContext db = null)
        {
            return new Lazy<string>(() => ToJson(db));
        }
        #endregion

        #region 返回lazy<json> asy
        /// <summary>
        /// 返回lazy<json> asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<Lazy<string>> ToLazyJsonAsy(DataContext db = null)
        {
            return await Task.Factory.StartNew(() =>
            {
                return new Lazy<string>(() => ToJson(db));
            });
        }
        #endregion


        #region 返回item
        /// <summary>
        /// 返回item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public T ToItem<T>(DataContext db = null) where T : class, new()
        {
            var result = new DataReturn<T>();
            var stopwatch = new Stopwatch();

            if (this.Data.Predicate.Exists(a => a.IsSuccess == false))
                return result.item;

            stopwatch.Start();

            this.Data.Take = 1;

            if (db == null)
            {
                using (var tempDb = new DataContext(this.Data.Key))
                {
                    result = tempDb.GetList<T>(this.Data);
                }
            }
            else
                result = db.GetList<T>(this.Data);

            stopwatch.Stop();

            DbLog.LogSql(this.Data.Config.IsOutSql, result.sql, this.Data.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            return result.item;
        }
        #endregion

        #region 返回item asy
        /// <summary>
        /// 返回item asy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<T> ToItemAsy<T>(DataContext db = null) where T : class, new()
        {
            return await Task.Factory.StartNew(() =>
            {
                return ToItem<T>(db);
            });
        }
        #endregion

        #region 返回Lazy<item>
        /// <summary>
        /// 返回Lazy<item>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public Lazy<T> ToLazyItem<T>(DataContext db = null) where T : class, new()
        {
            return new Lazy<T>(() => ToItem<T>(db));
        }
        #endregion

        #region 返回Lazy<item> asy
        /// <summary>
        /// 返回Lazy<item> asy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<Lazy<T>> ToLazyItemAsy<T>(DataContext db = null) where T : class, new()
        {
            return await Task.Factory.StartNew(() =>
            {
                return new Lazy<T>(() => ToItem<T>(db));
            });
        }
        #endregion


        #region 返回条数
        /// <summary>
        /// 返回条数
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int ToCount(DataContext db = null)
        {
            var result = new DataReturn();
            var stopwatch = new Stopwatch();

            if (this.Data.Predicate.Exists(a => a.IsSuccess == false))
                return result.Count;

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(this.Data.Key))
                {
                    result = tempDb.GetCount(this.Data);
                }
            }
            else
                result = db.GetCount(this.Data);

            stopwatch.Stop();

            DbLog.LogSql(this.Data.Config.IsOutSql, result.Sql, this.Data.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);

            return result.Count;
        }
        #endregion

        #region 返回条数 asy
        /// <summary>
        /// 返回条数 asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<int> ToCountAsy<T, T1>(DataContext db = null)
        {
            return await Task.Factory.StartNew(() =>
            {
                return ToCount(db);
            });
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
        public PageResult<T> ToPage<T>(PageModel pModel, DataContext db = null) where T : class, new()
        {
            var result = new DataReturn<T>();
            var stopwatch = new Stopwatch();

            if (this.Data.Predicate.Exists(a => a.IsSuccess == false))
                return result.pageResult;

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(this.Data.Key))
                {
                    result = tempDb.GetPage<T>(this.Data, pModel);
                }
            }
            else
                result = db.GetPage<T>(this.Data, pModel);

            stopwatch.Stop();

            DbLog.LogSql(this.Data.Config.IsOutSql, result.sql, this.Data.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            return result.pageResult;
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
        public async Task<PageResult<T>> ToPageAsy<T>(PageModel pModel, DataContext db = null) where T : class, new()
        {
            return await Task.Factory.StartNew(() =>
            {
                return ToPage<T>(pModel, db);
            });
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
        public Lazy<PageResult<T>> ToLazyPage<T>(PageModel pModel, DataContext db = null) where T : class, new()
        {
            return new Lazy<PageResult<T>>(() => ToPage<T>(pModel, db));
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
        public async Task<Lazy<PageResult<T>>> ToLazyPageAsy<T>(PageModel pModel, DataContext db = null) where T : class, new()
        {
            return await Task.Factory.StartNew(() =>
            {
                return new Lazy<PageResult<T>>(() => ToPage<T>(pModel, db));
            });
        }
        #endregion


        #region 返回分页Dictionary<string, object>
        /// <summary>
        /// 返回分页Dictionary<string, object>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pModel"></param>
        /// <returns></returns>
        public PageResult ToPage(PageModel pModel, DataContext db = null)
        {
            var result = new DataReturn();
            var stopwatch = new Stopwatch();

            if (this.Data.Predicate.Exists(a => a.IsSuccess == false))
                return result.PageResult;

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(this.Data.Key))
                {
                    result = tempDb.GetPage(this.Data, pModel);
                }
            }
            else
                result = db.GetPage(this.Data, pModel);

            stopwatch.Stop();

            DbLog.LogSql(this.Data.Config.IsOutSql, result.Sql, this.Data.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
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
        public async Task<PageResult> ToPageAsy(PageModel pModel, DataContext db = null)
        {
            return await Task.Factory.StartNew(() =>
            {
                return ToPage(pModel, db);
            });
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
        public Lazy<PageResult> ToLazyPage(PageModel pModel, DataContext db = null)
        {
            return new Lazy<PageResult>(() => ToPage(pModel, db));
        }
        #endregion

        #region 返回分页Dictionary<string, object> lazy asy
        /// <summary>
        /// 返回分页Dictionary<string, object> lazy asy
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pModel"></param>
        /// <returns></returns>
        public async Task<Lazy<PageResult>> ToLazyPageAsy(PageModel pModel, DataContext db = null)
        {
            return await Task.Factory.StartNew(() =>
            {
                return new Lazy<PageResult>(() => ToPage(pModel, db));
            });
        }
        #endregion


        #region DataTable
        /// <summary>
        /// DataTable
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public DataTable ToDataTable(DataContext db = null)
        {
            var result = new DataReturn();
            var stopwatch = new Stopwatch();

            if (this.Data.Predicate.Exists(a => a.IsSuccess == false))
                return result.Table;

            stopwatch.Start();
            this.Data.Take = 1;

            if (db == null)
            {
                using (var tempDb = new DataContext(this.Data.Key))
                {
                    result = tempDb.GetDataTable(this.Data);
                }
            }
            else
                result = db.GetDataTable(this.Data);

            stopwatch.Stop();

            DbLog.LogSql(this.Data.Config.IsOutSql, result.Sql, this.Data.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            return result.Table;
        }
        #endregion

        #region DataTable asy
        /// <summary>
        /// DataTable asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<DataTable> ToDataTableAsy(DataContext db = null)
        {
            return await Task.Factory.StartNew(() =>
            {
                return ToDataTable(db);
            });
        }
        #endregion

        #region DataTable lazy
        /// <summary>
        /// DataTable lazy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Lazy<DataTable> ToLazyDataTable(DataContext db = null)
        {
            return new Lazy<DataTable>(() => ToDataTable(db));
        }
        #endregion

        #region DataTable lazy asy
        /// <summary>
        /// DataTable lazy asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<Lazy<DataTable>> ToLazyDataTableAsy(DataContext db = null)
        {
            return await Task.Factory.StartNew(() =>
            {
                return new Lazy<DataTable>(() => ToDataTable(db));
            });
        }
        #endregion


        #region 返回List<Dictionary<string, object>>
        /// <summary>
        /// 返回List<Dictionary<string, object>>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> ToDics(DataContext db = null)
        {
            var result = new DataReturn();
            var stopwatch = new Stopwatch();

            if (this.Data.Predicate.Exists(a => a.IsSuccess == false))
                return result.DicList;

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(this.Data.Key))
                {
                    result = tempDb.GetDic(this.Data);
                }
            }
            else
                result = db.GetDic(this.Data);

            stopwatch.Stop();

            DbLog.LogSql(this.Data.Config.IsOutSql, result.Sql, this.Data.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            return result.DicList;
        }
        #endregion

        #region 返回List<Dictionary<string, object>> asy
        /// <summary>
        /// 返回List<Dictionary<string, object>> asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<List<Dictionary<string, object>>> ToDicsAsy(DataContext db = null)
        {
            return await Task.Factory.StartNew(() =>
            {
                return ToDics(db);
            });
        }
        #endregion

        #region 返回lazy<List<Dictionary<string, object>>>
        /// <summary>
        /// 返回lazy<List<Dictionary<string, object>>>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Lazy<List<Dictionary<string, object>>> ToLazyDics(DataContext db = null)
        {
            return new Lazy<List<Dictionary<string, object>>>(() => ToDics(db));
        }
        #endregion

        #region 返回lazy<List<Dictionary<string, object>>> asy
        /// <summary>
        /// 返回lazy<List<Dictionary<string, object>>> asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<Lazy<List<Dictionary<string, object>>>> ToLazyDicsAsy(DataContext db = null)
        {
            return await Task.Factory.StartNew(() =>
            {
                return new Lazy<List<Dictionary<string, object>>>(() => ToDics(db));
            });
        }
        #endregion


        #region Dictionary<string, object>
        /// <summary>
        /// Dictionary<string, object>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Dictionary<string, object> ToDic(DataContext db = null)
        {
            var result = new DataReturn();
            var stopwatch = new Stopwatch();

            if (this.Data.Predicate.Exists(a => a.IsSuccess == false))
                return result.Dic;

            stopwatch.Start();
            this.Data.Take = 1;

            if (db == null)
            {
                using (var tempDb = new DataContext(this.Data.Key))
                {
                    result = tempDb.GetDic(this.Data);
                }
            }
            else
                result = db.GetDic(this.Data);

            stopwatch.Stop();

            DbLog.LogSql(this.Data.Config.IsOutSql, result.Sql, this.Data.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            return result.Dic;
        }
        #endregion

        #region Dictionary<string, object> asy
        /// <summary>
        /// Dictionary<string, object> asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, object>> ToDicAsy(DataContext db = null)
        {
            return await Task.Factory.StartNew(() =>
            {
                return ToDic(db);
            });
        }
        #endregion

        #region Dictionary<string, object>
        /// <summary>
        /// Dictionary<string, object>>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Lazy<Dictionary<string, object>> ToLazyDic(DataContext db = null)
        {
            return new Lazy<Dictionary<string, object>>(() => ToDic(db));
        }
        #endregion

        #region Dictionary<string, object> asy
        /// <summary>
        /// Dictionary<string, object> asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<Lazy<Dictionary<string, object>>> ToLazyDicAsy(DataContext db = null)
        {
            return await Task.Factory.StartNew(() =>
            {
                return new Lazy<Dictionary<string, object>>(() => ToDic(db));
            });
        }
        #endregion
    }
}
