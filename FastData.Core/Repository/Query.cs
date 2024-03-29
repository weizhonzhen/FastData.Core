﻿using FastData.Core.Base;
using FastData.Core.Context;
using FastData.Core.Model;
using FastUntility.Core;
using FastUntility.Core.Page;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FastData.Core.Repository
{
    internal class Query : IQuery
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
            this.Data.TableName.Add(typeof(T1).Name);
            this.Data.TableAsName.SetValue(typeof(T1).Name, predicate.Parameters[1].Name);
            var queryField = BaseField.QueryField<T, T1>(predicate, field, this.Data);
            this.Data.Field.Add(queryField.Field);
            this.Data.AsName.AddRange(queryField.AsName);

            var condtion = VisitExpression.LambdaWhere<T, T1>(predicate, this.Data);
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
        public override IQuery LeftJoin<T, T1>(Expression<Func<T, T1, bool>> predicate, Expression<Func<T1, object>> field = null, bool isDblink = false)
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
        public override IQuery RightJoin<T, T1>(Expression<Func<T, T1, bool>> predicate, Expression<Func<T1, object>> field = null, bool isDblink = false)
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
        public override IQuery InnerJoin<T, T1>(Expression<Func<T, T1, bool>> predicate, Expression<Func<T1, object>> field = null, bool isDblink = false)
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
        public override IQuery OrderBy<T>(Expression<Func<T, object>> field, bool isDesc = true)
        {
            var orderBy = BaseField.OrderBy<T>(field, this.Data, isDesc);
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
        public override IQuery GroupBy<T>(Expression<Func<T, object>> field)
        {
            var groupBy = BaseField.GroupBy<T>(field, this.Data);
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
        public override IQuery Take(int i)
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
        public override IQuery Filter(bool isFilter = true)
        {
            this.Data.IsFilter = isFilter;
            return this;
        }
        #endregion

        #region 是否导航
        /// <summary>
        /// 是否导航
        /// </summary>
        /// <param name="isNavigate"></param>
        /// <returns></returns>
        public override IQuery Navigate(bool isNavigate = true)
        {
            this.Data.IsNavigate = isNavigate;
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
        public override List<T> ToList<T>(DataContext db = null, bool isOutSql = false)
        {
            var stopwatch = new Stopwatch();
            var result = new DataReturn<T>();

            if (this.Data.Predicate.Exists(a => a.IsSuccess == false))
                return result.List;

            stopwatch.Start();

            db = db == null ? ServiceContext.Engine.Resolve<IUnitOfWorK>().Contexts(this.Data.Key) : db;
            result = db.GetList<T>(this.Data);

            stopwatch.Stop();

            this.Data.Config.IsOutSql = this.Data.Config.IsOutSql ? this.Data.Config.IsOutSql : isOutSql;
            DbLog.LogSql(this.Data.Config.IsOutSql, result.Sql, this.Data.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
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
        public override ValueTask<List<T>> ToListAsy<T>(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<List<T>>(ToList<T>(db, isOutSql));
        }
        #endregion

        #region 返回lazy<list>
        /// <summary>
        /// 返回lazy<list>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public override Lazy<List<T>> ToLazyList<T>(DataContext db = null, bool isOutSql = false)
        {
            return new Lazy<List<T>>(() => ToList<T>(db, isOutSql));
        }
        #endregion

        #region 返回lazy<list> asy
        /// <summary>
        /// 返回lazy<list> asy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public override ValueTask<Lazy<List<T>>> ToLazyListAsy<T>(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<Lazy<List<T>>>(new Lazy<List<T>>(() => ToList<T>(db, isOutSql)));
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

            db = db == null ? ServiceContext.Engine.Resolve<IUnitOfWorK>().Contexts(this.Data.Key) : db;
            result = db.GetJson(this.Data);

            stopwatch.Stop();

            this.Data.Config.IsOutSql = this.Data.Config.IsOutSql ? this.Data.Config.IsOutSql : isOutSql;
            DbLog.LogSql(this.Data.Config.IsOutSql, result.Sql, this.Data.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
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
        public override T ToItem<T>(DataContext db = null, bool isOutSql = false)
        {
            var result = new DataReturn<T>();
            var stopwatch = new Stopwatch();

            if (this.Data.Predicate.Exists(a => a.IsSuccess == false))
                return result.Item;

            stopwatch.Start();

            this.Data.Take = 1;

            db = db == null ? ServiceContext.Engine.Resolve<IUnitOfWorK>().Contexts(this.Data.Key) : db;
            result = db.GetList<T>(this.Data);

            stopwatch.Stop();

            this.Data.Config.IsOutSql = this.Data.Config.IsOutSql ? this.Data.Config.IsOutSql : isOutSql;
            DbLog.LogSql(this.Data.Config.IsOutSql, result.Sql, this.Data.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
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
        public override ValueTask<T> ToItemAsy<T>(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<T>(ToItem<T>(db, isOutSql));
        }
        #endregion

        #region 返回Lazy<item>
        /// <summary>
        /// 返回Lazy<item>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public override Lazy<T> ToLazyItem<T>(DataContext db = null, bool isOutSql = false)
        {
            return new Lazy<T>(() => ToItem<T>(db, isOutSql));
        }
        #endregion

        #region 返回Lazy<item> asy
        /// <summary>
        /// 返回Lazy<item> asy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public override ValueTask<Lazy<T>> ToLazyItemAsy<T>(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<Lazy<T>>(new Lazy<T>(() => ToItem<T>(db, isOutSql)));
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

            db = db == null ? ServiceContext.Engine.Resolve<IUnitOfWorK>().Contexts(this.Data.Key) : db;
            result = db.GetCount(this.Data);

            stopwatch.Stop();

            this.Data.Config.IsOutSql = this.Data.Config.IsOutSql ? this.Data.Config.IsOutSql : isOutSql;
            DbLog.LogSql(this.Data.Config.IsOutSql, result.Sql, this.Data.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
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
        public override ValueTask<int> ToCountAsy(DataContext db = null, bool isOutSql = false)
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
        public override PageResult<T> ToPage<T>(PageModel pModel, DataContext db = null, bool isOutSql = false)
        {
            var result = new DataReturn<T>();
            var stopwatch = new Stopwatch();

            if (this.Data.Predicate.Exists(a => a.IsSuccess == false))
                return result.PageResult;

            stopwatch.Start();

            db = db == null ? ServiceContext.Engine.Resolve<IUnitOfWorK>().Contexts(this.Data.Key) : db;
            result = db.GetPage<T>(this.Data, pModel);

            stopwatch.Stop();

            this.Data.Config.IsOutSql = this.Data.Config.IsOutSql ? this.Data.Config.IsOutSql : isOutSql;
            DbLog.LogSql(this.Data.Config.IsOutSql, result.Sql, this.Data.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
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
        public override ValueTask<PageResult<T>> ToPageAsy<T>(PageModel pModel, DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<PageResult<T>>(ToPage<T>(pModel, db, isOutSql));
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
        public override Lazy<PageResult<T>> ToLazyPage<T>(PageModel pModel, DataContext db = null, bool isOutSql = false)
        {
            return new Lazy<PageResult<T>>(() => ToPage<T>(pModel, db, isOutSql));
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
        public override ValueTask<Lazy<PageResult<T>>> ToLazyPageAsy<T>(PageModel pModel, DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<Lazy<PageResult<T>>>(new Lazy<PageResult<T>>(() => ToPage<T>(pModel, db, isOutSql)));
        }
        #endregion


        #region 返回分页Dictionary<string, object>
        /// <summary>
        /// 返回分页Dictionary<string, object>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pModel"></param>
        /// <returns></returns>
        public override PageResult ToPage(PageModel pModel, DataContext db = null, bool isOutSql = false)
        {
            var result = new DataReturn();
            var stopwatch = new Stopwatch();

            if (this.Data.Predicate.Exists(a => a.IsSuccess == false))
                return result.PageResult;

            stopwatch.Start();

            db = db == null ? ServiceContext.Engine.Resolve<IUnitOfWorK>().Contexts(this.Data.Key) : db;
            result = db.GetPage(this.Data, pModel);

            stopwatch.Stop();

            this.Data.Config.IsOutSql = this.Data.Config.IsOutSql ? this.Data.Config.IsOutSql : isOutSql;
            DbLog.LogSql(this.Data.Config.IsOutSql, result.Sql, this.Data.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
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
        public override ValueTask<PageResult> ToPageAsy(PageModel pModel, DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<PageResult>(ToPage(pModel, db, isOutSql));
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
        public override Lazy<PageResult> ToLazyPage(PageModel pModel, DataContext db = null, bool isOutSql = false)
        {
            return new Lazy<PageResult>(() => ToPage(pModel, db, isOutSql));
        }
        #endregion

        #region 返回分页Dictionary<string, object> lazy asy
        /// <summary>
        /// 返回分页Dictionary<string, object> lazy asy
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pModel"></param>
        /// <returns></returns>
        public override ValueTask<Lazy<PageResult>> ToLazyPageAsy(PageModel pModel, DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<Lazy<PageResult>>(new Lazy<PageResult>(() => ToPage(pModel, db, isOutSql)));
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

            db = db == null ? ServiceContext.Engine.Resolve<IUnitOfWorK>().Contexts(this.Data.Key) : db;
            result = db.GetDataTable(this.Data);

            stopwatch.Stop();

            this.Data.Config.IsOutSql = this.Data.Config.IsOutSql ? this.Data.Config.IsOutSql : isOutSql;
            DbLog.LogSql(this.Data.Config.IsOutSql, result.Sql, this.Data.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
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

            db = db == null ? ServiceContext.Engine.Resolve<IUnitOfWorK>().Contexts(this.Data.Key) : db;
            result = db.GetDic(this.Data);

            stopwatch.Stop();

            this.Data.Config.IsOutSql = this.Data.Config.IsOutSql ? this.Data.Config.IsOutSql : isOutSql;
            DbLog.LogSql(this.Data.Config.IsOutSql, result.Sql, this.Data.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
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

            db = db == null ? ServiceContext.Engine.Resolve<IUnitOfWorK>().Contexts(this.Data.Key) : db;
            result = db.GetDic(this.Data);

            stopwatch.Stop();

            this.Data.Config.IsOutSql = this.Data.Config.IsOutSql ? this.Data.Config.IsOutSql : isOutSql;
            DbLog.LogSql(this.Data.Config.IsOutSql, result.Sql, this.Data.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
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


        #region and 条件
        /// <summary>
        /// and 条件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="condtion"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public override IQuery AndIf<T>(bool condtion, Expression<Func<T, bool>> predicate)
        {
            if (condtion)
            {
                var visitModel = VisitExpression.LambdaWhere<T>(predicate, this.Data);
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
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public override IQuery And<T>(Expression<Func<T, bool>> predicate)
        {
            var visitModel = VisitExpression.LambdaWhere<T>(predicate, this.Data);
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
        /// <typeparam name="T"></typeparam>
        /// <param name="condtion"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public override IQuery OrIf<T>(bool condtion, Expression<Func<T, bool>> predicate)
        {
            if (condtion)
            {
                var visitModel = VisitExpression.LambdaWhere<T>(predicate, this.Data);
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
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public override IQuery Or<T>(Expression<Func<T, bool>> predicate)
        {
            var visitModel = VisitExpression.LambdaWhere<T>(predicate, this.Data);
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

        #region and 条件
        /// <summary>
        /// and 条件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="condtion"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public override IQuery AndIf<T, T1>(bool condtion, Expression<Func<T, T1, bool>> predicate)
        {
            if (condtion)
            {
                var visitModel = VisitExpression.LambdaWhere<T, T1>(predicate, this.Data);
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
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public override IQuery And<T, T1>(Expression<Func<T, T1, bool>> predicate)
        {
            var visitModel = VisitExpression.LambdaWhere<T, T1>(predicate, this.Data);
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
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="condtion"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public override IQuery OrIf<T, T1>(bool condtion, Expression<Func<T, T1, bool>> predicate)
        {
            if (condtion)
            {
                var visitModel = VisitExpression.LambdaWhere<T, T1>(predicate, this.Data);
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
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public override IQuery Or<T, T1>(Expression<Func<T, T1, bool>> predicate)
        {
            var visitModel = VisitExpression.LambdaWhere<T, T1>(predicate, this.Data);
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


        #region dynamic
        /// <summary>
        /// dynamic
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override dynamic ToDyn(DataContext db = null, bool isOutSql = false)
        {
            var result = new DataReturnDyn();
            var stopwatch = new Stopwatch();

            if (Data.Predicate.Exists(a => a.IsSuccess == false))
                return result.Item;

            stopwatch.Start();
            Data.Take = 1;

            db = db == null ? ServiceContext.Engine.Resolve<IUnitOfWorK>().Contexts(this.Data.Key) : db;
            result = db.GetDyns(Data);

            stopwatch.Stop();

            Data.Config.IsOutSql = Data.Config.IsOutSql ? Data.Config.IsOutSql : isOutSql;
            DbLog.LogSql(Data.Config.IsOutSql, result.Sql, Data.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;
            return result.Item;
        }
        #endregion

        #region dynamic asy
        /// <summary>
        /// dynamic asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override ValueTask<dynamic> ToDynAsy(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<dynamic>(ToDyn(db, isOutSql));
        }
        #endregion

        #region dynamic lazy
        /// <summary>
        /// dynamic
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override Lazy<dynamic> ToLazyDyn(DataContext db = null, bool isOutSql = false)
        {
            return new Lazy<dynamic>(() => ToDyn(db, isOutSql));
        }
        #endregion

        #region dynamic asy lazy
        /// <summary>
        /// dynamic asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override ValueTask<Lazy<dynamic>> ToLazyDynAsy(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<Lazy<dynamic>>(new Lazy<dynamic>(() => ToDyn(db, isOutSql)));
        }
        #endregion throw new NotImplementedException();


        #region dynamics
        /// <summary>
        /// dynamics
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override List<dynamic> ToDyns(DataContext db = null, bool isOutSql = false)
        {
            var result = new DataReturnDyn();
            var stopwatch = new Stopwatch();

            if (Data.Predicate.Exists(a => a.IsSuccess == false))
                return result.List;

            stopwatch.Start();

            db = db == null ? ServiceContext.Engine.Resolve<IUnitOfWorK>().Contexts(this.Data.Key) : db;
            result = db.GetDyns(Data);

            stopwatch.Stop();

            Data.Config.IsOutSql = Data.Config.IsOutSql ? Data.Config.IsOutSql : isOutSql;
            DbLog.LogSql(Data.Config.IsOutSql, result.Sql, Data.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;
            return result.List;
        }
        #endregion

        #region dynamics asy
        /// <summary>
        /// dynamics asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override ValueTask<List<dynamic>> ToDynsAsy(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<List<dynamic>>(ToDyns(db, isOutSql));
        }
        #endregion

        #region dynamics lazy
        /// <summary>
        /// dynamics
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override Lazy<List<dynamic>> ToLazyDyns(DataContext db = null, bool isOutSql = false)
        {
            return new Lazy<List<dynamic>>(() => ToDyns(db, isOutSql));
        }
        #endregion

        #region dynamics asy lazy
        /// <summary>
        /// dynamics asy
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override ValueTask<Lazy<List<dynamic>>> ToLazyDynsAsy(DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<Lazy<List<dynamic>>>(new Lazy<List<dynamic>>(() => ToDyns(db, isOutSql)));
        }
        #endregion


        #region 返回分页dyn
        /// <summary>
        /// 返回分页dyn
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pModel"></param>
        /// <returns></returns>
        public override PageResultDyn ToDynPage(PageModel pModel, DataContext db = null, bool isOutSql = false)
        {
            var result = new DataReturnDyn();
            var stopwatch = new Stopwatch();

            if (Data.Predicate.Exists(a => a.IsSuccess == false))
                return result.PageResult;

            stopwatch.Start();

            db = db == null ? ServiceContext.Engine.Resolve<IUnitOfWorK>().Contexts(this.Data.Key) : db;
            result = db.GetDynPage(Data, pModel);

            stopwatch.Stop();
            Data.Config.IsOutSql = Data.Config.IsOutSql ? Data.Config.IsOutSql : isOutSql;
            DbLog.LogSql(Data.Config.IsOutSql, result.Sql, Data.Config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;
            return result.PageResult;
        }
        #endregion

        #region 返回分页dyn asy
        /// <summary>
        /// 返回分页dyn asy
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pModel"></param>
        /// <returns></returns>
        public override ValueTask<PageResultDyn> ToDynPageAsy(PageModel pModel, DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<PageResultDyn>(ToDynPage(pModel, db, isOutSql));
        }
        #endregion

        #region 返回分页dyn lazy
        /// <summary>
        /// 返回分页dyn lazy
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pModel"></param>
        /// <returns></returns>
        public override Lazy<PageResultDyn> ToLazyDynPage(PageModel pModel, DataContext db = null, bool isOutSql = false)
        {
            return new Lazy<PageResultDyn>(() => ToDynPage(pModel, db, isOutSql));
        }
        #endregion

        #region 返回分页dyn lazy asy
        /// <summary>
        /// 返回分页dyn lazy asy
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pModel"></param>
        /// <returns></returns>
        public override ValueTask<Lazy<PageResultDyn>> ToLazyDynPageAsy(PageModel pModel, DataContext db = null, bool isOutSql = false)
        {
            return new ValueTask<Lazy<PageResultDyn>>(new Lazy<PageResultDyn>(() => ToDynPage(pModel, db, isOutSql)));
        }
        #endregion
    }
}
