using FastData.Core.Context;
using FastData.Core.Model;
using FastUntility.Core.Page;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FastData.Core.Repository
{
    public abstract class IQuery
    {
        public abstract IQuery AndIf<T>(bool condtion, Expression<Func<T, bool>> predicate);

        public abstract IQuery And<T>(Expression<Func<T, bool>> predicate);

        public abstract IQuery OrIf<T>(bool condtion, Expression<Func<T, bool>> predicate);

        public abstract IQuery Or<T>(Expression<Func<T, bool>> predicate);

        public abstract IQuery AndIf<T, T1>(bool condtion, Expression<Func<T, T1, bool>> predicate);

        public abstract IQuery And<T, T1>(Expression<Func<T, T1, bool>> predicate);

        public abstract IQuery OrIf<T, T1>(bool condtion, Expression<Func<T, T1, bool>> predicate);

        public abstract IQuery Or<T, T1>(Expression<Func<T, T1, bool>> predicate);

        public abstract IQuery LeftJoin<T, T1>(Expression<Func<T, T1, bool>> predicate, Expression<Func<T1, object>> field = null, bool isDblink = false);

        public abstract IQuery RightJoin<T, T1>(Expression<Func<T, T1, bool>> predicate, Expression<Func<T1, object>> field = null, bool isDblink = false) where T1 : class, new();

        public abstract IQuery InnerJoin<T, T1>(Expression<Func<T, T1, bool>> predicate, Expression<Func<T1, object>> field = null, bool isDblink = false) where T1 : class, new();

        public abstract IQuery OrderBy<T>(Expression<Func<T, object>> field, bool isDesc = true);

        public abstract IQuery GroupBy<T>(Expression<Func<T, object>> field);

        public abstract IQuery Take(int i);

        public abstract IQuery Filter(bool isFilter = true);

        public abstract IQuery Navigate(bool isNavigate = true);

        public abstract string ToJson(DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<string> ToJsonAsy(DataContext db = null, bool isOutSql = false);

        public abstract Lazy<string> ToLazyJson(DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<Lazy<string>> ToLazyJsonAsy(DataContext db = null, bool isOutSql = false);

        public abstract T ToItem<T>(DataContext db = null, bool isOutSql = false) where T : class, new();

        public abstract ValueTask<T> ToItemAsy<T>(DataContext db = null, bool isOutSql = false) where T : class, new();

        public abstract Lazy<T> ToLazyItem<T>(DataContext db = null, bool isOutSql = false) where T : class, new();

        public abstract ValueTask<Lazy<T>> ToLazyItemAsy<T>(DataContext db = null, bool isOutSql = false) where T : class, new();

        public abstract int ToCount(DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<int> ToCountAsy(DataContext db = null, bool isOutSql = false);

        public abstract PageResult<T> ToPage<T>(PageModel pModel, DataContext db = null, bool isOutSql = false) where T : class, new();

        public abstract ValueTask<PageResult<T>> ToPageAsy<T>(PageModel pModel, DataContext db = null, bool isOutSql = false) where T : class, new();

        public abstract Lazy<PageResult<T>> ToLazyPage<T>(PageModel pModel, DataContext db = null, bool isOutSql = false) where T : class, new();

        public abstract ValueTask<Lazy<PageResult<T>>> ToLazyPageAsy<T>(PageModel pModel, DataContext db = null, bool isOutSql = false) where T : class, new();

        public abstract PageResult ToPage(PageModel pModel, DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<PageResult> ToPageAsy(PageModel pModel, DataContext db = null, bool isOutSql = false);

        public abstract Lazy<PageResult> ToLazyPage(PageModel pModel, DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<Lazy<PageResult>> ToLazyPageAsy(PageModel pModel, DataContext db = null, bool isOutSql = false);

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

        public abstract List<T> ToList<T>(DataContext db = null, bool isOutSql = false) where T : class, new();

        public abstract ValueTask<List<T>> ToListAsy<T>(DataContext db = null, bool isOutSql = false) where T : class, new();

        public abstract Lazy<List<T>> ToLazyList<T>(DataContext db = null, bool isOutSql = false) where T : class, new();

        public abstract ValueTask<Lazy<List<T>>> ToLazyListAsy<T>(DataContext db = null, bool isOutSql = false) where T : class, new();

        public abstract dynamic ToDyn(DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<dynamic> ToDynAsy(DataContext db = null, bool isOutSql = false);

        public abstract Lazy<dynamic> ToLazyDyn(DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<Lazy<dynamic>> ToLazyDynAsy(DataContext db = null, bool isOutSql = false);

        public abstract List<dynamic> ToDyns(DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<List<dynamic>> ToDynsAsy(DataContext db = null, bool isOutSql = false);

        public abstract Lazy<List<dynamic>> ToLazyDyns(DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<Lazy<List<dynamic>>> ToLazyDynsAsy(DataContext db = null, bool isOutSql = false);

        public abstract PageResultDyn ToDynPage(PageModel pModel, DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<PageResultDyn> ToDynPageAsy(PageModel pModel, DataContext db = null, bool isOutSql = false);

        public abstract Lazy<PageResultDyn> ToLazyDynPage(PageModel pModel, DataContext db = null, bool isOutSql = false);

        public abstract ValueTask<Lazy<PageResultDyn>> ToLazyDynPageAsy(PageModel pModel, DataContext db = null, bool isOutSql = false);
    }
}
