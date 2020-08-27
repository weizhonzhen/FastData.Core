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
    public interface IQuery
    {
        DataQuery SetKey(string key);

        DataQuery LeftJoin<T, T1>(Expression<Func<T, T1, bool>> predicate, Expression<Func<T1, object>> field = null, bool isDblink = false);

        DataQuery RightJoin<T, T1>(Expression<Func<T, T1, bool>> predicate, Expression<Func<T1, object>> field = null, bool isDblink = false) where T1 : class, new();

        DataQuery InnerJoin<T, T1>(Expression<Func<T, T1, bool>> predicate, Expression<Func<T1, object>> field = null, bool isDblink = false) where T1 : class, new();

        DataQuery OrderBy<T>(Expression<Func<T, object>> field, bool isDesc = true);

        DataQuery GroupBy<T>(Expression<Func<T, object>> field);

        DataQuery Take(int i);

        string ToJson(DataContext db = null);

        Task<string> ToJsonAsy(DataContext db = null);

        Lazy<string> ToLazyJson(DataContext db = null);

        Task<Lazy<string>> ToLazyJsonAsy(DataContext db = null);

        T ToItem<T>(DataContext db = null) where T : class, new();

        Task<T> ToItemAsy<T>(DataContext db = null) where T : class, new();

        Lazy<T> ToLazyItem<T>(DataContext db = null) where T : class, new();

        Task<Lazy<T>> ToLazyItemAsy<T>(DataContext db = null) where T : class, new();

        int ToCount(DataContext db = null);

        Task<int> ToCountAsy<T, T1>(DataContext db = null);

        PageResult<T> ToPage<T>(PageModel pModel, DataContext db = null) where T : class, new();

        Task<PageResult<T>> ToPageAsy<T>(PageModel pModel, DataContext db = null) where T : class, new();

        Lazy<PageResult<T>> ToLazyPage<T>(PageModel pModel, DataContext db = null) where T : class, new();

        Task<Lazy<PageResult<T>>> ToLazyPageAsy<T>(PageModel pModel, DataContext db = null) where T : class, new();

        PageResult ToPage(PageModel pModel, DataContext db = null);

        Task<PageResult> ToPageAsy(PageModel pModel, DataContext db = null);

        Lazy<PageResult> ToLazyPage(PageModel pModel, DataContext db = null);

        Task<Lazy<PageResult>> ToLazyPageAsy(PageModel pModel, DataContext db = null);

        DataTable ToDataTable(DataContext db = null);

        Task<DataTable> ToDataTableAsy(DataContext db = null);

        Lazy<DataTable> ToLazyDataTable(DataContext db = null);

        Task<Lazy<DataTable>> ToLazyDataTableAsy(DataContext db = null);

        List<Dictionary<string, object>> ToDics(DataContext db = null);

        Task<List<Dictionary<string, object>>> ToDicsAsy(DataContext db = null);

        Lazy<List<Dictionary<string, object>>> ToLazyDics(DataContext db = null);

        Task<Lazy<List<Dictionary<string, object>>>> ToLazyDicsAsy(DataContext db = null);

        Dictionary<string, object> ToDic(DataContext db = null);

        Task<Dictionary<string, object>> ToDicAsy(DataContext db = null);

        Lazy<Dictionary<string, object>> ToLazyDic(DataContext db = null);

        Task<Lazy<Dictionary<string, object>>> ToLazyDicAsy(DataContext db = null);

        List<T> ToList<T>(DataContext db = null) where T : class, new();

        Task<List<T>> ToListAsy<T>(DataContext db = null) where T : class, new();

        Lazy<List<T>> ToLazyList<T>(DataContext db = null) where T : class, new();

        Task<Lazy<List<T>>> ToLazyListAsy<T>(DataContext db = null) where T : class, new();
    }
}
