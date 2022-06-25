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
    public class Queryable<T> : IQueryable<T> where T : class, new()
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
        private IQueryable<T, T1> JoinType<T1>(string joinType, Expression<Func<T, T1, bool>> predicate, Expression<Func<T1, object>> field = null, bool isDblink = false)
        {
            var queryField = BaseField.QueryField<T, T1>(predicate, field, this.Data.Config);
            this.Data.Field.Add(queryField.Field);
            this.Data.AsName.AddRange(queryField.AsName);

            var condtion = VisitExpression.LambdaWhere<T, T1>(predicate, this.Data.Config);
            this.Data.Predicate.Add(condtion);
            this.Data.Table.Add(string.Format("{2} {0}{3} {1}", typeof(T1).Name, predicate.Parameters[1].Name
            , joinType, isDblink && !string.IsNullOrEmpty(this.Data.Config.DbLinkName) ? string.Format("@", this.Data.Config.DbLinkName) : ""));
            this.Data.TableName.Add(typeof(T1).Name);

            return new Queryable<T, T1>() { Data=this.Data };
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
        public override IQueryable<T, T1> LeftJoin<T1>(Expression<Func<T, T1, bool>> predicate, Expression<Func<T1, object>> field = null, bool isDblink = false)
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
        public override IQueryable<T, T1> RightJoin<T1>(Expression<Func<T, T1, bool>> predicate, Expression<Func<T1, object>> field = null, bool isDblink = false)
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
        public override IQueryable<T, T1> InnerJoin<T1>(Expression<Func<T, T1, bool>> predicate, Expression<Func<T1, object>> field = null, bool isDblink = false)
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
        public override IQueryable<T> OrderBy(Expression<Func<T, object>> field, bool isDesc = true)
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
        public override IQueryable<T> GroupBy(Expression<Func<T, object>> field)
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
        public override IQueryable<T> Take(int i)
        {
            this.Data.Take = i;
            return this;
        }
        #endregion

        #region 是否过滤
        /// <summary>
        /// 是否过滤
        /// </summary>
        /// <param name="isFilter"></param>
        /// <returns></returns>
        public override IQueryable<T> Filter(bool isFilter = true)
        {
            this.Data.IsFilter = isFilter;
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
        public override List<T> ToList(DataContext db = null, bool isOutSql = false)
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

            this.Data.Config.IsOutSql = this.Data.Config.IsOutSql ? this.Data.Config.IsOutSql : isOutSql;
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
        public override ValueTask<List<T>> ToListAsy(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<List<T>>(ToList(db, isOutSql));
        }
        #endregion

        #region 返回lazy<list>
        /// <summary>
        /// 返回lazy<list>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public override Lazy<List<T>> ToLazyList(DataContext db = null, bool isOutSql = false)
        {
            return new Lazy<List<T>>(() => ToList(db, isOutSql));
        }
        #endregion

        #region 返回lazy<list> asy
        /// <summary>
        /// 返回lazy<list> asy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public override ValueTask<Lazy<List<T>>> ToLazyListAsy(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<Lazy<List<T>>>(new Lazy<List<T>>(() => ToList(db, isOutSql)));
        }
        #endregion


        #region 返回json
        /// <summary>
        /// 返回json
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override string ToJson(DataContext db = null, bool isOutSql = false)
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

            this.Data.Config.IsOutSql = this.Data.Config.IsOutSql ? this.Data.Config.IsOutSql : isOutSql;
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
        public override ValueTask<string> ToJsonAsy(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<string>(ToJson(db, isOutSql));
        }
        #endregion

        #region 返回lazy<json>
        /// <summary>
        /// 返回lazy<json>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override Lazy<string> ToLazyJson(DataContext db = null, bool isOutSql = false)
        {
            return new Lazy<string>(() => ToJson(db, isOutSql));
        }
        #endregion

        #region 返回lazy<json> asy
        /// <summary>
        /// 返回lazy<json> asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override ValueTask<Lazy<string>> ToLazyJsonAsy(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<Lazy<string>>(new Lazy<string>(() => ToJson(db, isOutSql)));
        }
        #endregion


        #region 返回item
        /// <summary>
        /// 返回item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public override T ToItem(DataContext db = null, bool isOutSql = false)
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

            this.Data.Config.IsOutSql = this.Data.Config.IsOutSql ? this.Data.Config.IsOutSql : isOutSql;
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
        public override ValueTask<T> ToItemAsy(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<T>(ToItem(db, isOutSql));
        }
        #endregion

        #region 返回Lazy<item>
        /// <summary>
        /// 返回Lazy<item>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public override Lazy<T> ToLazyItem(DataContext db = null, bool isOutSql = false)
        {
            return new Lazy<T>(() => ToItem(db, isOutSql));
        }
        #endregion

        #region 返回Lazy<item> asy
        /// <summary>
        /// 返回Lazy<item> asy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public override ValueTask<Lazy<T>> ToLazyItemAsy(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<Lazy<T>>(new Lazy<T>(() => ToItem(db, isOutSql)));
        }
        #endregion


        #region 返回条数
        /// <summary>
        /// 返回条数
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override int ToCount(DataContext db = null, bool isOutSql = false)
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

            this.Data.Config.IsOutSql = this.Data.Config.IsOutSql ? this.Data.Config.IsOutSql : isOutSql;
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
        public override ValueTask<int> ToCountAsy<T1>(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<int>(ToCount(db, isOutSql));
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
        public override PageResult<T> ToPage(PageModel pModel, DataContext db = null, bool isOutSql = false)
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

            this.Data.Config.IsOutSql = this.Data.Config.IsOutSql ? this.Data.Config.IsOutSql : isOutSql;
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
        public override ValueTask<PageResult<T>> ToPageAsy(PageModel pModel, DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<PageResult<T>>(ToPage(pModel, db, isOutSql));
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
        public override Lazy<PageResult<T>> ToLazyPage(PageModel pModel, DataContext db = null, bool isOutSql = false)
        {
            return new Lazy<PageResult<T>>(() => ToPage(pModel, db, isOutSql));
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
        public override ValueTask<Lazy<PageResult<T>>> ToLazyPageAsy(PageModel pModel, DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<Lazy<PageResult<T>>>(new Lazy<PageResult<T>>(() => ToPage(pModel, db, isOutSql)));
        }
        #endregion


        #region 返回分页Dictionary<string, object>
        /// <summary>
        /// 返回分页Dictionary<string, object>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pModel"></param>
        /// <returns></returns>
        public override PageResult ToPageDic(PageModel pModel, DataContext db = null, bool isOutSql = false)
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

            this.Data.Config.IsOutSql = this.Data.Config.IsOutSql ? this.Data.Config.IsOutSql : isOutSql;
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
        public override ValueTask<PageResult> ToPageDicAsy(PageModel pModel, DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<PageResult>(ToPageDic(pModel, db, isOutSql));
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
        public override Lazy<PageResult> ToLazyPageDic(PageModel pModel, DataContext db = null, bool isOutSql = false)
        {
            return new Lazy<PageResult>(() => ToPageDic(pModel, db, isOutSql));
        }
        #endregion

        #region 返回分页Dictionary<string, object> lazy asy
        /// <summary>
        /// 返回分页Dictionary<string, object> lazy asy
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pModel"></param>
        /// <returns></returns>
        public override ValueTask<Lazy<PageResult>> ToLazyPageDicAsy(PageModel pModel, DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<Lazy<PageResult>>(new Lazy<PageResult>(() => ToPageDic(pModel, db, isOutSql)));
        }
        #endregion


        #region DataTable
        /// <summary>
        /// DataTable
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override DataTable ToDataTable(DataContext db = null, bool isOutSql = false)
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

            this.Data.Config.IsOutSql = this.Data.Config.IsOutSql ? this.Data.Config.IsOutSql : isOutSql;
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
        public override ValueTask<DataTable> ToDataTableAsy(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<DataTable>(ToDataTable(db, isOutSql));
        }
        #endregion

        #region DataTable lazy
        /// <summary>
        /// DataTable lazy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override Lazy<DataTable> ToLazyDataTable(DataContext db = null, bool isOutSql = false)
        {
            return new Lazy<DataTable>(() => ToDataTable(db, isOutSql));
        }
        #endregion

        #region DataTable lazy asy
        /// <summary>
        /// DataTable lazy asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override ValueTask<Lazy<DataTable>> ToLazyDataTableAsy(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<Lazy<DataTable>>(new Lazy<DataTable>(() => ToDataTable(db, isOutSql)));
        }
        #endregion


        #region 返回List<Dictionary<string, object>>
        /// <summary>
        /// 返回List<Dictionary<string, object>>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override List<Dictionary<string, object>> ToDics(DataContext db = null, bool isOutSql = false)
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

            this.Data.Config.IsOutSql = this.Data.Config.IsOutSql ? this.Data.Config.IsOutSql : isOutSql;
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
        public override ValueTask<List<Dictionary<string, object>>> ToDicsAsy(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<List<Dictionary<string, object>>>(ToDics(db, isOutSql));
        }
        #endregion

        #region 返回lazy<List<Dictionary<string, object>>>
        /// <summary>
        /// 返回lazy<List<Dictionary<string, object>>>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override Lazy<List<Dictionary<string, object>>> ToLazyDics(DataContext db = null, bool isOutSql = false)
        {
            return new Lazy<List<Dictionary<string, object>>>(() => ToDics(db, isOutSql));
        }
        #endregion

        #region 返回lazy<List<Dictionary<string, object>>> asy
        /// <summary>
        /// 返回lazy<List<Dictionary<string, object>>> asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override ValueTask<Lazy<List<Dictionary<string, object>>>> ToLazyDicsAsy(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<Lazy<List<Dictionary<string, object>>>>(new Lazy<List<Dictionary<string, object>>>(() => ToDics(db, isOutSql)));
        }
        #endregion


        #region Dictionary<string, object>
        /// <summary>
        /// Dictionary<string, object>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override Dictionary<string, object> ToDic(DataContext db = null, bool isOutSql = false)
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

            this.Data.Config.IsOutSql = this.Data.Config.IsOutSql ? this.Data.Config.IsOutSql : isOutSql;
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
        public override ValueTask<Dictionary<string, object>> ToDicAsy(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<Dictionary<string, object>>(ToDic(db, isOutSql));
        }
        #endregion

        #region Dictionary<string, object>
        /// <summary>
        /// Dictionary<string, object>>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override Lazy<Dictionary<string, object>> ToLazyDic(DataContext db = null, bool isOutSql = false)
        {
            return new Lazy<Dictionary<string, object>>(() => ToDic(db, isOutSql));
        }
        #endregion

        #region Dictionary<string, object> asy
        /// <summary>
        /// Dictionary<string, object> asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override ValueTask<Lazy<Dictionary<string, object>>> ToLazyDicAsy(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<Lazy<Dictionary<string, object>>>(new Lazy<Dictionary<string, object>>(() => ToDic(db, isOutSql)));
        }
        #endregion


        #region 返回item
        /// <summary>
        ///  返回item
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="db"></param>
        /// <param name="isOutSql"></param>
        /// <returns></returns>
        public override R ToItem<R>(DataContext db = null, bool isOutSql = false)
        {
            var result = new DataReturn<R>();
            var stopwatch = new Stopwatch();

            if (this.Data.Predicate.Exists(a => a.IsSuccess == false))
                return result.item;

            stopwatch.Start();

            this.Data.Take = 1;

            if (db == null)
            {
                using (var tempDb = new DataContext(this.Data.Key))
                {
                    result = tempDb.GetList<R>(this.Data);
                }
            }
            else
                result = db.GetList<R>(this.Data);

            stopwatch.Stop();

            this.Data.Config.IsOutSql = this.Data.Config.IsOutSql ? this.Data.Config.IsOutSql : isOutSql;
            DbLog.LogSql(this.Data.Config.IsOutSql, result.sql, this.Data.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            return result.item;
        }
        #endregion

        #region 返回item asy
        /// <summary>
        ///  返回item asy
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="db"></param>
        /// <param name="isOutSql"></param>
        /// <returns></returns>
        public override ValueTask<R> ToItemAsy<R>(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<R>(ToItem<R>(db, isOutSql));
        }
        #endregion

        #region 返回Lazy<item>
        /// <summary>
        /// 返回Lazy<item>
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="db"></param>
        /// <param name="isOutSql"></param>
        /// <returns></returns>
        public override Lazy<R> ToLazyItem<R>(DataContext db = null, bool isOutSql = false)
        {
            return new Lazy<R>(() => ToItem<R>(db, isOutSql));
        }
        #endregion

        #region 返回Lazy<item> asy
        /// <summary>
        /// 返回Lazy<item> asy
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="db"></param>
        /// <param name="isOutSql"></param>
        /// <returns></returns>
        public override ValueTask<Lazy<R>> ToLazyItemAsy<R>(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<Lazy<R>>(new Lazy<R>(() => ToItem<R>(db, isOutSql)));
        }
        #endregion


        #region 返回分页
        /// <summary>
        /// 返回分页
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="pModel"></param>
        /// <param name="db"></param>
        /// <param name="isOutSql"></param>
        /// <returns></returns>
        public override PageResult<R> ToPage<R>(PageModel pModel, DataContext db = null, bool isOutSql = false)
        {
            var result = new DataReturn<R>();
            var stopwatch = new Stopwatch();

            if (this.Data.Predicate.Exists(a => a.IsSuccess == false))
                return result.pageResult;

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(this.Data.Key))
                {
                    result = tempDb.GetPage<R>(this.Data, pModel);
                }
            }
            else
                result = db.GetPage<R>(this.Data, pModel);

            stopwatch.Stop();

            this.Data.Config.IsOutSql = this.Data.Config.IsOutSql ? this.Data.Config.IsOutSql : isOutSql;
            DbLog.LogSql(this.Data.Config.IsOutSql, result.sql, this.Data.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            return result.pageResult;
        }
        #endregion

        #region 返回分页 asy
        /// <summary>
        /// 返回分页 asy
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="pModel"></param>
        /// <param name="db"></param>
        /// <param name="isOutSql"></param>
        /// <returns></returns>
        public override ValueTask<PageResult<R>> ToPageAsy<R>(PageModel pModel, DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<PageResult<R>>(ToPage<R>(pModel, db, isOutSql));
        }
        #endregion

        #region 返回分页 lazy
        /// <summary>
        /// 返回分页 lazy
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="pModel"></param>
        /// <param name="db"></param>
        /// <param name="isOutSql"></param>
        /// <returns></returns>
        public override Lazy<PageResult<R>> ToLazyPage<R>(PageModel pModel, DataContext db = null, bool isOutSql = false)
        {
            return new Lazy<PageResult<R>>(() => ToPage<R>(pModel, db, isOutSql));
        }
        #endregion

        #region 返回分页 lazy asy
        /// <summary>
        /// 返回分页 lazy asy
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="pModel"></param>
        /// <param name="db"></param>
        /// <param name="isOutSql"></param>
        /// <returns></returns>
        public override ValueTask<Lazy<PageResult<R>>> ToLazyPageAsy<R>(PageModel pModel, DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<Lazy<PageResult<R>>>(new Lazy<PageResult<R>>(() => ToPage<R>(pModel, db, isOutSql)));
        }
        #endregion


        #region 返回list
        /// <summary>
        /// 返回list
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="db"></param>
        /// <param name="isOutSql"></param>
        /// <returns></returns>
        public override List<R> ToList<R>(DataContext db = null, bool isOutSql = false)
        {
            var stopwatch = new Stopwatch();
            var result = new DataReturn<R>();

            if (this.Data.Predicate.Exists(a => a.IsSuccess == false))
                return result.list;

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(this.Data.Key))
                {
                    result = tempDb.GetList<R>(this.Data);
                }
            }
            else
                result = db.GetList<R>(this.Data);

            stopwatch.Stop();

            this.Data.Config.IsOutSql = this.Data.Config.IsOutSql ? this.Data.Config.IsOutSql : isOutSql;
            DbLog.LogSql(this.Data.Config.IsOutSql, result.sql, this.Data.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            return result.list;
        }
        #endregion

        #region  返回list asy
        /// <summary>
        /// 返回list asy
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="db"></param>
        /// <param name="isOutSql"></param>
        /// <returns></returns>
        public override ValueTask<List<R>> ToListAsy<R>(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<List<R>>(ToList<R>(db, isOutSql));
        }
        #endregion

        #region 返回 Lazy<list
        /// <summary>
        /// 返回 Lazy<list
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="db"></param>
        /// <param name="isOutSql"></param>
        /// <returns></returns>
        public override Lazy<List<R>> ToLazyList<R>(DataContext db = null, bool isOutSql = false)
        {
            return new Lazy<List<R>>(() => ToList<R>(db, isOutSql));
        }
        #endregion

        #region 返回 Lazy<list> asy
        /// <summary>
        /// 返回 Lazy<list> asy
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="db"></param>
        /// <param name="isOutSql"></param>
        /// <returns></returns>
        public override ValueTask<Lazy<List<R>>> ToLazyListAsy<R>(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<Lazy<List<R>>>(new Lazy<List<R>>(() => ToList<R>(db, isOutSql)));
        }
        #endregion


        #region and 条件
        /// <summary>
        /// and 条件
        /// </summary>
        /// <param name="condtion"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public override IQueryable<T> AndIf(bool condtion, Expression<Func<T, bool>> predicate)
        {
            if (condtion)
            {
                var visitModel = VisitExpression.LambdaWhere<T>(predicate, this.Data.Config);
                if (this.Data.Predicate.Count >= 1)
                    this.Data.Predicate[0].Where += $" and {visitModel.Where}";
                if (this.Data.Predicate.Count == 0)
                {
                    this.Data.Table.Add(string.Format("{0} {1}", typeof(T).Name, predicate.Parameters[0].Name));
                    this.Data.TableName.Add(typeof(T).Name);
                    this.Data.Predicate.Add(visitModel);
                }
            }
            return this;
        }
        #endregion

        #region and
        /// <summary>
        /// and
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public override IQueryable<T> And(Expression<Func<T, bool>> predicate)
        {
            var visitModel = VisitExpression.LambdaWhere<T>(predicate, this.Data.Config);
            if (this.Data.Predicate.Count >= 1)
                this.Data.Predicate[0].Where += $" and {visitModel.Where}";
            if (this.Data.Predicate.Count == 0)
            {
                this.Data.Table.Add(string.Format("{0} {1}", typeof(T).Name, predicate.Parameters[0].Name));
                this.Data.TableName.Add(typeof(T).Name);
                this.Data.Predicate.Add(visitModel);
            }
            return this;
        }
        #endregion

        #region or条件
        /// <summary>
        /// or条件
        /// </summary>
        /// <param name="condtion"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public override IQueryable<T> OrIf(bool condtion, Expression<Func<T, bool>> predicate)
        {
            if (condtion)
            {
                var visitModel = VisitExpression.LambdaWhere<T>(predicate, this.Data.Config);
                if (this.Data.Predicate.Count >= 1)
                    this.Data.Predicate[0].Where += $" or {visitModel.Where}";
                if (this.Data.Predicate.Count == 0)
                {
                    this.Data.Table.Add(string.Format("{0} {1}", typeof(T).Name, predicate.Parameters[0].Name));
                    this.Data.TableName.Add(typeof(T).Name);
                    this.Data.Predicate.Add(visitModel);
                }
            }
            return this;
        }
        #endregion

        #region or
        /// <summary>
        /// or
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public override IQueryable<T> Or(Expression<Func<T, bool>> predicate)
        {
            var visitModel = VisitExpression.LambdaWhere<T>(predicate, this.Data.Config);
            if (this.Data.Predicate.Count >= 1)
                this.Data.Predicate[0].Where += $" or {visitModel.Where}";
            if (this.Data.Predicate.Count == 0)
            {
                this.Data.Table.Add(string.Format("{0} {1}", typeof(T).Name, predicate.Parameters[0].Name));
                this.Data.TableName.Add(typeof(T).Name);
                this.Data.Predicate.Add(visitModel);
            }
            return this;
        }
        #endregion
    }

    public class Queryable<T,T1> : IQueryable<T,T1> where T : class, new()
    {
        internal DataQuery Data { get; set; } = new DataQuery();

        #region and 条件
        /// <summary>
        /// and 条件
        /// </summary>
        /// <param name="condtion"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public override IQueryable<T,T1> AndIf(bool condtion, Expression<Func<T, T1, bool>> predicate)
        {
            if (condtion)
            {
                var visitModel = VisitExpression.LambdaWhere<T, T1>(predicate, this.Data.Config);
                if (this.Data.Predicate.Count >= 1)
                    this.Data.Predicate[0].Where += $" and {visitModel.Where}";
                if (this.Data.Predicate.Count == 0)
                {
                    this.Data.Table.Add(string.Format("{0} {1}", typeof(T).Name, predicate.Parameters[0].Name));
                    this.Data.TableName.Add(typeof(T).Name);
                    this.Data.Predicate.Add(visitModel);
                }
            }
            return this;
        }
        #endregion

        #region and
        /// <summary>
        /// and
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public override IQueryable<T, T1> And(Expression<Func<T, T1, bool>> predicate)
        {
            var visitModel = VisitExpression.LambdaWhere<T, T1>(predicate, this.Data.Config);
            if (this.Data.Predicate.Count >= 1)
                this.Data.Predicate[0].Where += $" and {visitModel.Where}";
            if (this.Data.Predicate.Count == 0)
            {
                this.Data.Table.Add(string.Format("{0} {1}", typeof(T).Name, predicate.Parameters[0].Name));
                this.Data.TableName.Add(typeof(T).Name);
                this.Data.Predicate.Add(visitModel);
            }
            return this;
        }
        #endregion

        #region or 条件
        /// <summary>
        /// or 条件
        /// </summary>
        /// <param name="condtion"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public override IQueryable<T, T1> OrIf(bool condtion, Expression<Func<T, T1, bool>> predicate)
        {
            if (condtion)
            {
                var visitModel = VisitExpression.LambdaWhere<T, T1>(predicate, this.Data.Config);
                if (this.Data.Predicate.Count >= 1)
                    this.Data.Predicate[0].Where += $" or {visitModel.Where}";
                if (this.Data.Predicate.Count == 0)
                {
                    this.Data.Table.Add(string.Format("{0} {1}", typeof(T).Name, predicate.Parameters[0].Name));
                    this.Data.TableName.Add(typeof(T).Name);
                    this.Data.Predicate.Add(visitModel);
                }
            }
            return this;
        }
        #endregion

        #region or
        /// <summary>
        /// or
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public override IQueryable<T, T1> Or(Expression<Func<T, T1, bool>> predicate)
        {
            var visitModel = VisitExpression.LambdaWhere<T, T1>(predicate, this.Data.Config);
            if (this.Data.Predicate.Count >= 1)
                this.Data.Predicate[0].Where += $" or {visitModel.Where}";
            if (this.Data.Predicate.Count == 0)
            {
                this.Data.Table.Add(string.Format("{0} {1}", typeof(T).Name, predicate.Parameters[0].Name));
                this.Data.TableName.Add(typeof(T).Name);
                this.Data.Predicate.Add(visitModel);
            }
            return this;
        }
        #endregion

        #region OrderBy
        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="field"></param>
        /// <param name="isDesc"></param>
        /// <returns></returns>
        public override IQueryable<T, T1> OrderBy(Expression<Func<T, T1, object>> field, bool isDesc = true)
        {
            var orderBy = BaseField.OrderBy(field, this.Data.Config, isDesc);
            this.Data.OrderBy.AddRange(orderBy);
            return this;
        }
        #endregion

        #region GroupBy
        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public override IQueryable<T, T1> GroupBy(Expression<Func<T, T1, object>> field)
        {
            var groupBy = BaseField.GroupBy(field, this.Data.Config);
            this.Data.GroupBy.AddRange(groupBy);
            return this;
        }
        #endregion

        #region 查询take
        /// <summary>
        /// 查询take
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override IQueryable<T, T1> Take(int i)
        {
            this.Data.Take = i;
            return this;
        }
        #endregion

        #region 是否过滤
        /// <summary>
        /// 是否过滤
        /// </summary>
        /// <param name="isFilter"></param>
        /// <returns></returns>
        public override IQueryable<T, T1> Filter(bool isFilter = true)
        {
            this.Data.IsFilter = isFilter;
            return this;
        }
        #endregion

        #region 返回json
        /// <summary>
        /// 返回json
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override string ToJson(DataContext db = null, bool isOutSql = false)
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

            this.Data.Config.IsOutSql = this.Data.Config.IsOutSql ? this.Data.Config.IsOutSql : isOutSql;
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
        public override ValueTask<string> ToJsonAsy(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<string>(ToJson(db, isOutSql));
        }
        #endregion

        #region 返回lazy<json>
        /// <summary>
        /// 返回lazy<json>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override Lazy<string> ToLazyJson(DataContext db = null, bool isOutSql = false)
        {
            return new Lazy<string>(() => ToJson(db, isOutSql));
        }
        #endregion

        #region 返回lazy<json> asy
        /// <summary>
        /// 返回lazy<json> asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override ValueTask<Lazy<string>> ToLazyJsonAsy(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<Lazy<string>>(new Lazy<string>(() => ToJson(db, isOutSql)));
        }
        #endregion


        #region 返回item
        /// <summary>
        ///  返回item
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="db"></param>
        /// <param name="isOutSql"></param>
        /// <returns></returns>
        public override R ToItem<R>(DataContext db = null, bool isOutSql = false)
        {
            var result = new DataReturn<R>();
            var stopwatch = new Stopwatch();

            if (this.Data.Predicate.Exists(a => a.IsSuccess == false))
                return result.item;

            stopwatch.Start();

            this.Data.Take = 1;

            if (db == null)
            {
                using (var tempDb = new DataContext(this.Data.Key))
                {
                    result = tempDb.GetList<R>(this.Data);
                }
            }
            else
                result = db.GetList<R>(this.Data);

            stopwatch.Stop();

            this.Data.Config.IsOutSql = this.Data.Config.IsOutSql ? this.Data.Config.IsOutSql : isOutSql;
            DbLog.LogSql(this.Data.Config.IsOutSql, result.sql, this.Data.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            return result.item;
        }
        #endregion

        #region 返回item asy
        /// <summary>
        ///  返回item asy
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="db"></param>
        /// <param name="isOutSql"></param>
        /// <returns></returns>
        public override ValueTask<R> ToItemAsy<R>(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<R>(ToItem<R>(db, isOutSql));
        }
        #endregion

        #region 返回Lazy<item>
        /// <summary>
        /// 返回Lazy<item>
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="db"></param>
        /// <param name="isOutSql"></param>
        /// <returns></returns>
        public override Lazy<R> ToLazyItem<R>(DataContext db = null, bool isOutSql = false)
        {
            return new Lazy<R>(() => ToItem<R>(db, isOutSql));
        }
        #endregion

        #region 返回Lazy<item> asy
        /// <summary>
        /// 返回Lazy<item> asy
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="db"></param>
        /// <param name="isOutSql"></param>
        /// <returns></returns>
        public override ValueTask<Lazy<R>> ToLazyItemAsy<R>(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<Lazy<R>>(new Lazy<R>(() => ToItem<R>(db, isOutSql)));
        }
        #endregion


        #region 返回条数
        /// <summary>
        /// 返回条数
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override int ToCount(DataContext db = null, bool isOutSql = false)
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

            this.Data.Config.IsOutSql = this.Data.Config.IsOutSql ? this.Data.Config.IsOutSql : isOutSql;
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
        public override ValueTask<int> ToCountAsy(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<int>(ToCount(db, isOutSql));
        }
        #endregion


        #region 返回分页
        /// <summary>
        /// 返回分页
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="pModel"></param>
        /// <param name="db"></param>
        /// <param name="isOutSql"></param>
        /// <returns></returns>
        public override PageResult<R> ToPage<R>(PageModel pModel, DataContext db = null, bool isOutSql = false)
        {
            var result = new DataReturn<R>();
            var stopwatch = new Stopwatch();

            if (this.Data.Predicate.Exists(a => a.IsSuccess == false))
                return result.pageResult;

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(this.Data.Key))
                {
                    result = tempDb.GetPage<R>(this.Data, pModel);
                }
            }
            else
                result = db.GetPage<R>(this.Data, pModel);

            stopwatch.Stop();

            this.Data.Config.IsOutSql = this.Data.Config.IsOutSql ? this.Data.Config.IsOutSql : isOutSql;
            DbLog.LogSql(this.Data.Config.IsOutSql, result.sql, this.Data.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            return result.pageResult;
        }
        #endregion

        #region 返回分页 asy
        /// <summary>
        /// 返回分页 asy
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="pModel"></param>
        /// <param name="db"></param>
        /// <param name="isOutSql"></param>
        /// <returns></returns>
        public override ValueTask<PageResult<R>> ToPageAsy<R>(PageModel pModel, DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<PageResult<R>>(ToPage<R>(pModel, db, isOutSql));
        }
        #endregion

        #region 返回分页 lazy
        /// <summary>
        /// 返回分页 lazy
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="pModel"></param>
        /// <param name="db"></param>
        /// <param name="isOutSql"></param>
        /// <returns></returns>
        public override Lazy<PageResult<R>> ToLazyPage<R>(PageModel pModel, DataContext db = null, bool isOutSql = false)
        {
            return new Lazy<PageResult<R>>(() => ToPage<R>(pModel, db, isOutSql));
        }
        #endregion

        #region 返回分页 lazy asy
        /// <summary>
        /// 返回分页 lazy asy
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="pModel"></param>
        /// <param name="db"></param>
        /// <param name="isOutSql"></param>
        /// <returns></returns>
        public override ValueTask<Lazy<PageResult<R>>> ToLazyPageAsy<R>(PageModel pModel, DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<Lazy<PageResult<R>>>(new Lazy<PageResult<R>>(() => ToPage<R>(pModel, db, isOutSql)));
        }
        #endregion


        #region 返回分页Dictionary<string, object>
        /// <summary>
        /// 返回分页Dictionary<string, object>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pModel"></param>
        /// <returns></returns>
        public override PageResult ToPageDic(PageModel pModel, DataContext db = null, bool isOutSql = false)
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

            this.Data.Config.IsOutSql = this.Data.Config.IsOutSql ? this.Data.Config.IsOutSql : isOutSql;
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
        public override ValueTask<PageResult> ToPageDicAsy(PageModel pModel, DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<PageResult>(ToPageDic(pModel, db, isOutSql));
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
        public override Lazy<PageResult> ToLazyPageDic(PageModel pModel, DataContext db = null, bool isOutSql = false)
        {
            return new Lazy<PageResult>(() => ToPageDic(pModel, db, isOutSql));
        }
        #endregion

        #region 返回分页Dictionary<string, object> lazy asy
        /// <summary>
        /// 返回分页Dictionary<string, object> lazy asy
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pModel"></param>
        /// <returns></returns>
        public override ValueTask<Lazy<PageResult>> ToLazyPageDicAsy(PageModel pModel, DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<Lazy<PageResult>>(new Lazy<PageResult>(() => ToPageDic(pModel, db, isOutSql)));
        }
        #endregion  throw new NotImplementedException();


        #region DataTable
        /// <summary>
        /// DataTable
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override DataTable ToDataTable(DataContext db = null, bool isOutSql = false)
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

            this.Data.Config.IsOutSql = this.Data.Config.IsOutSql ? this.Data.Config.IsOutSql : isOutSql;
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
        public override ValueTask<DataTable> ToDataTableAsy(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<DataTable>(ToDataTable(db, isOutSql));
        }
        #endregion

        #region DataTable lazy
        /// <summary>
        /// DataTable lazy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override Lazy<DataTable> ToLazyDataTable(DataContext db = null, bool isOutSql = false)
        {
            return new Lazy<DataTable>(() => ToDataTable(db, isOutSql));
        }
        #endregion

        #region DataTable lazy asy
        /// <summary>
        /// DataTable lazy asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override ValueTask<Lazy<DataTable>> ToLazyDataTableAsy(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<Lazy<DataTable>>(new Lazy<DataTable>(() => ToDataTable(db, isOutSql)));
        }
        #endregion


        #region 返回List<Dictionary<string, object>>
        /// <summary>
        /// 返回List<Dictionary<string, object>>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override List<Dictionary<string, object>> ToDics(DataContext db = null, bool isOutSql = false)
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

            this.Data.Config.IsOutSql = this.Data.Config.IsOutSql ? this.Data.Config.IsOutSql : isOutSql;
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
        public override ValueTask<List<Dictionary<string, object>>> ToDicsAsy(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<List<Dictionary<string, object>>>(ToDics(db, isOutSql));
        }
        #endregion

        #region 返回lazy<List<Dictionary<string, object>>>
        /// <summary>
        /// 返回lazy<List<Dictionary<string, object>>>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override Lazy<List<Dictionary<string, object>>> ToLazyDics(DataContext db = null, bool isOutSql = false)
        {
            return new Lazy<List<Dictionary<string, object>>>(() => ToDics(db, isOutSql));
        }
        #endregion

        #region 返回lazy<List<Dictionary<string, object>>> asy
        /// <summary>
        /// 返回lazy<List<Dictionary<string, object>>> asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override ValueTask<Lazy<List<Dictionary<string, object>>>> ToLazyDicsAsy(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<Lazy<List<Dictionary<string, object>>>>(new Lazy<List<Dictionary<string, object>>>(() => ToDics(db, isOutSql)));
        }
        #endregion


        #region Dictionary<string, object>
        /// <summary>
        /// Dictionary<string, object>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override Dictionary<string, object> ToDic(DataContext db = null, bool isOutSql = false)
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

            this.Data.Config.IsOutSql = this.Data.Config.IsOutSql ? this.Data.Config.IsOutSql : isOutSql;
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
        public override ValueTask<Dictionary<string, object>> ToDicAsy(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<Dictionary<string, object>>(ToDic(db, isOutSql));
        }
        #endregion

        #region Dictionary<string, object>
        /// <summary>
        /// Dictionary<string, object>>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override Lazy<Dictionary<string, object>> ToLazyDic(DataContext db = null, bool isOutSql = false)
        {
            return new Lazy<Dictionary<string, object>>(() => ToDic(db, isOutSql));
        }
        #endregion

        #region Dictionary<string, object> asy
        /// <summary>
        /// Dictionary<string, object> asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override ValueTask<Lazy<Dictionary<string, object>>> ToLazyDicAsy(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<Lazy<Dictionary<string, object>>>(new Lazy<Dictionary<string, object>>(() => ToDic(db, isOutSql)));
        }
        #endregion


        #region 返回list
        /// <summary>
        /// 返回list
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="db"></param>
        /// <param name="isOutSql"></param>
        /// <returns></returns>
        public override List<R> ToList<R>(DataContext db = null, bool isOutSql = false)
        {
            var stopwatch = new Stopwatch();
            var result = new DataReturn<R>();

            if (this.Data.Predicate.Exists(a => a.IsSuccess == false))
                return result.list;

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(this.Data.Key))
                {
                    result = tempDb.GetList<R>(this.Data);
                }
            }
            else
                result = db.GetList<R>(this.Data);

            stopwatch.Stop();

            this.Data.Config.IsOutSql = this.Data.Config.IsOutSql ? this.Data.Config.IsOutSql : isOutSql;
            DbLog.LogSql(this.Data.Config.IsOutSql, result.sql, this.Data.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            return result.list;
        }
        #endregion

        #region  返回list asy
        /// <summary>
        /// 返回list asy
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="db"></param>
        /// <param name="isOutSql"></param>
        /// <returns></returns>
        public override ValueTask<List<R>> ToListAsy<R>(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<List<R>>(ToList<R>(db, isOutSql));
        }
        #endregion

        #region 返回 Lazy<list
        /// <summary>
        /// 返回 Lazy<list
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="db"></param>
        /// <param name="isOutSql"></param>
        /// <returns></returns>
        public override Lazy<List<R>> ToLazyList<R>(DataContext db = null, bool isOutSql = false)
        {
            return new Lazy<List<R>>(() => ToList<R>(db, isOutSql));
        }
        #endregion

        #region 返回 Lazy<list> asy
        /// <summary>
        /// 返回 Lazy<list> asy
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="db"></param>
        /// <param name="isOutSql"></param>
        /// <returns></returns>
        public override ValueTask<Lazy<List<R>>> ToLazyListAsy<R>(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<Lazy<List<R>>>(new Lazy<List<R>>(() => ToList<R>(db, isOutSql)));
        }
        #endregion
    }
}
