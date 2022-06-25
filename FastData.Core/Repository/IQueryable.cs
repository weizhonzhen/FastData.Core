using FastData.Core.Context;
using FastUntility.Core.Page;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FastData.Core.Repository
{
    public abstract class IQueryable<T> where T : class, new()
    {
        public abstract IQueryable<T> AndIf(bool condtion, Expression<Func<T, bool>> predicate);

        public abstract IQueryable<T> And( Expression<Func<T, bool>> predicate);

        public abstract IQueryable<T> OrIf(bool condtion, Expression<Func<T, bool>> predicate);

        public abstract IQueryable<T> Or(Expression<Func<T, bool>> predicate);

        public abstract IQueryable<T,T1> LeftJoin<T1>(Expression<Func<T, T1, bool>> predicate, Expression<Func<T1, object>> field = null, bool isDblink = false);

        public abstract IQueryable<T, T1> RightJoin<T1>(Expression<Func<T, T1, bool>> predicate, Expression<Func<T1, object>> field = null, bool isDblink = false) where T1 : class, new();

        public abstract IQueryable<T, T1> InnerJoin<T1>(Expression<Func<T, T1, bool>> predicate, Expression<Func<T1, object>> field = null, bool isDblink = false) where T1 : class, new();

        public abstract IQueryable<T> OrderBy(Expression<Func<T, object>> field, bool isDesc = true);

        public abstract IQueryable<T> GroupBy(Expression<Func<T, object>> field);

        public abstract IQueryable<T> Take(int i);

        public abstract IQueryable<T> Filter(bool isFilter = true);

        public abstract string ToJson(DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<string> ToJsonAsy(DataContext db = null, bool isOutSql = false);

        public abstract Lazy<string> ToLazyJson(DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<Lazy<string>> ToLazyJsonAsy(DataContext db = null, bool isOutSql = false);

        public abstract T ToItem(DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<T> ToItemAsy(DataContext db = null, bool isOutSql = false);

        public abstract Lazy<T> ToLazyItem(DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<Lazy<T>> ToLazyItemAsy(DataContext db = null, bool isOutSql = false);

        public abstract R ToItem<R>(DataContext db = null, bool isOutSql = false) where R : class, new();

        public abstract ValueTask<R> ToItemAsy<R>(DataContext db = null, bool isOutSql = false) where R : class, new();

        public abstract Lazy<R> ToLazyItem<R>(DataContext db = null, bool isOutSql = false) where R : class, new();

        public abstract ValueTask<Lazy<R>> ToLazyItemAsy<R>(DataContext db = null, bool isOutSql = false) where R : class, new();

        public abstract int ToCount(DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<int> ToCountAsy<T1>(DataContext db = null, bool isOutSql = false);

        public abstract PageResult<T> ToPage(PageModel pModel, DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<PageResult<T>> ToPageAsy(PageModel pModel, DataContext db = null, bool isOutSql = false);

        public abstract Lazy<PageResult<T>> ToLazyPage(PageModel pModel, DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<Lazy<PageResult<T>>> ToLazyPageAsy(PageModel pModel, DataContext db = null, bool isOutSql = false);

        public abstract PageResult<R> ToPage<R>(PageModel pModel, DataContext db = null, bool isOutSql = false) where R : class, new();

        public abstract ValueTask<PageResult<R>> ToPageAsy<R>(PageModel pModel, DataContext db = null, bool isOutSql = false) where R : class, new();

        public abstract Lazy<PageResult<R>> ToLazyPage<R>(PageModel pModel, DataContext db = null, bool isOutSql = false) where R : class, new();

        public abstract ValueTask<Lazy<PageResult<R>>> ToLazyPageAsy<R>(PageModel pModel, DataContext db = null, bool isOutSql = false) where R : class, new();

        public abstract PageResult ToPageDic(PageModel pModel, DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<PageResult> ToPageDicAsy(PageModel pModel, DataContext db = null, bool isOutSql = false);

        public abstract Lazy<PageResult> ToLazyPageDic(PageModel pModel, DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<Lazy<PageResult>> ToLazyPageDicAsy(PageModel pModel, DataContext db = null, bool isOutSql = false);

        public abstract DataTable ToDataTable(DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<DataTable> ToDataTableAsy(DataContext db = null, bool isOutSql = false);

        public abstract Lazy<DataTable> ToLazyDataTable(DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<Lazy<DataTable>> ToLazyDataTableAsy(DataContext db = null, bool isOutSql = false);

        public abstract List<Dictionary<string, object>> ToDics(DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<List<Dictionary<string, object>>> ToDicsAsy(DataContext db = null, bool isOutSql = false);

        public abstract Lazy<List<Dictionary<string, object>>> ToLazyDics(DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<Lazy<List<Dictionary<string, object>>>> ToLazyDicsAsy(DataContext db = null, bool isOutSql = false);

        public abstract Dictionary<string, object> ToDic(DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<Dictionary<string, object>> ToDicAsy(DataContext db = null, bool isOutSql = false);

        public abstract Lazy<Dictionary<string, object>> ToLazyDic(DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<Lazy<Dictionary<string, object>>> ToLazyDicAsy(DataContext db = null, bool isOutSql = false);

        public abstract List<T> ToList(DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<List<T>> ToListAsy(DataContext db = null, bool isOutSql = false);

        public abstract Lazy<List<T>> ToLazyList(DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<Lazy<List<T>>> ToLazyListAsy(DataContext db = null, bool isOutSql = false);

        public abstract List<R> ToList<R>(DataContext db = null, bool isOutSql = false) where R : class, new();

        public abstract ValueTask<List<R>> ToListAsy<R>(DataContext db = null, bool isOutSql = false) where R : class, new();

        public abstract Lazy<List<R>> ToLazyList<R>(DataContext db = null, bool isOutSql = false) where R : class, new();

        public abstract ValueTask<Lazy<List<R>>> ToLazyListAsy<R>(DataContext db = null, bool isOutSql = false) where R : class, new();
    }

    public abstract class IQueryable<T,T1> where T : class, new()
    {
        public abstract IQueryable<T,T1> AndIf(bool condtion, Expression<Func<T, T1, bool>> predicate);

        public abstract IQueryable<T, T1> And(Expression<Func<T, T1, bool>> predicate);

        public abstract IQueryable<T, T1> OrIf(bool condtion, Expression<Func<T, T1, bool>> predicate);

        public abstract IQueryable<T, T1> Or(Expression<Func<T, T1, bool>> predicate);

        public abstract IQueryable<T, T1> OrderBy(Expression<Func<T, T1, object>> field, bool isDesc = true);

        public abstract IQueryable<T, T1> GroupBy(Expression<Func<T, T1, object>> field);

        public abstract IQueryable<T, T1> Take(int i);

        public abstract IQueryable<T, T1> Filter(bool isFilter = true);

        public abstract string ToJson(DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<string> ToJsonAsy(DataContext db = null, bool isOutSql = false);

        public abstract Lazy<string> ToLazyJson(DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<Lazy<string>> ToLazyJsonAsy(DataContext db = null, bool isOutSql = false);

        public abstract R ToItem<R>(DataContext db = null, bool isOutSql = false) where R : class, new();

        public abstract ValueTask<R> ToItemAsy<R>(DataContext db = null, bool isOutSql = false) where R : class, new();

        public abstract Lazy<R> ToLazyItem<R>(DataContext db = null, bool isOutSql = false) where R : class, new();

        public abstract ValueTask<Lazy<R>> ToLazyItemAsy<R>(DataContext db = null, bool isOutSql = false) where R : class, new();

        public abstract int ToCount(DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<int> ToCountAsy(DataContext db = null, bool isOutSql = false);

        public abstract PageResult<R> ToPage<R>(PageModel pModel, DataContext db = null, bool isOutSql = false) where R : class, new();

        public abstract ValueTask<PageResult<R>> ToPageAsy<R>(PageModel pModel, DataContext db = null, bool isOutSql = false) where R : class, new();

        public abstract Lazy<PageResult<R>> ToLazyPage<R>(PageModel pModel, DataContext db = null, bool isOutSql = false) where R : class, new();

        public abstract ValueTask<Lazy<PageResult<R>>> ToLazyPageAsy<R>(PageModel pModel, DataContext db = null, bool isOutSql = false) where R : class, new();

        public abstract PageResult ToPageDic(PageModel pModel, DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<PageResult> ToPageDicAsy(PageModel pModel, DataContext db = null, bool isOutSql = false);

        public abstract Lazy<PageResult> ToLazyPageDic(PageModel pModel, DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<Lazy<PageResult>> ToLazyPageDicAsy(PageModel pModel, DataContext db = null, bool isOutSql = false);

        public abstract DataTable ToDataTable(DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<DataTable> ToDataTableAsy(DataContext db = null, bool isOutSql = false);

        public abstract Lazy<DataTable> ToLazyDataTable(DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<Lazy<DataTable>> ToLazyDataTableAsy(DataContext db = null, bool isOutSql = false);

        public abstract List<Dictionary<string, object>> ToDics(DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<List<Dictionary<string, object>>> ToDicsAsy(DataContext db = null, bool isOutSql = false);

        public abstract Lazy<List<Dictionary<string, object>>> ToLazyDics(DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<Lazy<List<Dictionary<string, object>>>> ToLazyDicsAsy(DataContext db = null, bool isOutSql = false);

        public abstract Dictionary<string, object> ToDic(DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<Dictionary<string, object>> ToDicAsy(DataContext db = null, bool isOutSql = false);

        public abstract Lazy<Dictionary<string, object>> ToLazyDic(DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<Lazy<Dictionary<string, object>>> ToLazyDicAsy(DataContext db = null, bool isOutSql = false);

        public abstract List<R> ToList<R>(DataContext db = null, bool isOutSql = false) where R : class, new();

        public abstract ValueTask<List<R>> ToListAsy<R>(DataContext db = null, bool isOutSql = false) where R : class, new();

        public abstract Lazy<List<R>> ToLazyList<R>(DataContext db = null, bool isOutSql = false) where R : class, new();

        public abstract ValueTask<Lazy<List<R>>> ToLazyListAsy<R>(DataContext db = null, bool isOutSql = false) where R : class, new();
    }
}
