using FastData.Core.Context;
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
        public abstract IQuery LeftJoin<T, T1>(Expression<Func<T, T1, bool>> predicate, Expression<Func<T1, object>> field = null, bool isDblink = false);

        public abstract IQuery RightJoin<T, T1>(Expression<Func<T, T1, bool>> predicate, Expression<Func<T1, object>> field = null, bool isDblink = false) where T1 : class, new();

        public abstract IQuery InnerJoin<T, T1>(Expression<Func<T, T1, bool>> predicate, Expression<Func<T1, object>> field = null, bool isDblink = false) where T1 : class, new();

        public abstract IQuery OrderBy<T>(Expression<Func<T, object>> field, bool isDesc = true);

        public abstract IQuery GroupBy<T>(Expression<Func<T, object>> field);

        public abstract IQuery Take(int i);

        public abstract string ToJson(DataContext db = null);

        public abstract Task<string> ToJsonAsy(DataContext db = null);

        public abstract Lazy<string> ToLazyJson(DataContext db = null);

        public abstract Task<Lazy<string>> ToLazyJsonAsy(DataContext db = null);

        public abstract T ToItem<T>(DataContext db = null) where T : class, new();

        public abstract Task<T> ToItemAsy<T>(DataContext db = null) where T : class, new();

        public abstract Lazy<T> ToLazyItem<T>(DataContext db = null) where T : class, new();

        public abstract Task<Lazy<T>> ToLazyItemAsy<T>(DataContext db = null) where T : class, new();

        public abstract int ToCount(DataContext db = null);

        public abstract Task<int> ToCountAsy<T, T1>(DataContext db = null);

        public abstract PageResult<T> ToPage<T>(PageModel pModel, DataContext db = null) where T : class, new();

        public abstract Task<PageResult<T>> ToPageAsy<T>(PageModel pModel, DataContext db = null) where T : class, new();

        public abstract Lazy<PageResult<T>> ToLazyPage<T>(PageModel pModel, DataContext db = null) where T : class, new();

        public abstract Task<Lazy<PageResult<T>>> ToLazyPageAsy<T>(PageModel pModel, DataContext db = null) where T : class, new();

        public abstract PageResult ToPage(PageModel pModel, DataContext db = null);

        public abstract Task<PageResult> ToPageAsy(PageModel pModel, DataContext db = null);

        public abstract Lazy<PageResult> ToLazyPage(PageModel pModel, DataContext db = null);

        public abstract Task<Lazy<PageResult>> ToLazyPageAsy(PageModel pModel, DataContext db = null);

        public abstract DataTable ToDataTable(DataContext db = null);

        public abstract Task<DataTable> ToDataTableAsy(DataContext db = null);

        public abstract Lazy<DataTable> ToLazyDataTable(DataContext db = null);

        public abstract Task<Lazy<DataTable>> ToLazyDataTableAsy(DataContext db = null);

        public abstract List<Dictionary<string, object>> ToDics(DataContext db = null);

        public abstract Task<List<Dictionary<string, object>>> ToDicsAsy(DataContext db = null);

        public abstract Lazy<List<Dictionary<string, object>>> ToLazyDics(DataContext db = null);

        public abstract Task<Lazy<List<Dictionary<string, object>>>> ToLazyDicsAsy(DataContext db = null);

        public abstract Dictionary<string, object> ToDic(DataContext db = null);

        public abstract Task<Dictionary<string, object>> ToDicAsy(DataContext db = null);

        public abstract Lazy<Dictionary<string, object>> ToLazyDic(DataContext db = null);

        public abstract Task<Lazy<Dictionary<string, object>>> ToLazyDicAsy(DataContext db = null);

        public abstract List<T> ToList<T>(DataContext db = null) where T : class, new();

        public abstract Task<List<T>> ToListAsy<T>(DataContext db = null) where T : class, new();

        public abstract Lazy<List<T>> ToLazyList<T>(DataContext db = null) where T : class, new();

        public abstract Task<Lazy<List<T>>> ToLazyListAsy<T>(DataContext db = null) where T : class, new();
    }
}
