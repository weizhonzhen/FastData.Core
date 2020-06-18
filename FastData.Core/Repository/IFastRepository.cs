using FastData.Core.Context;
using FastData.Core.Model;
using FastUntility.Core.Page;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FastData.Core.Repository
{
    public interface IFastRepository
    {
        List<T> Query<T>(string name, DbParameter[] param, DataContext db = null, string key = null) where T : class, new();

        Task<List<T>> QueryAsy<T>(string name, DbParameter[] param, DataContext db = null, string key = null) where T : class, new();

        Lazy<List<T>> QueryLazy<T>(string name, DbParameter[] param, DataContext db = null, string key = null) where T : class, new();

        Task<Lazy<List<T>>> QueryLazyAsy<T>(string name, DbParameter[] param, DataContext db = null, string key = null) where T : class, new();

        List<Dictionary<string, object>> Query(string name, DbParameter[] param, DataContext db = null, string key = null);

        Task<List<Dictionary<string, object>>> QueryAsy(string name, DbParameter[] param, DataContext db = null, string key = null);

        Lazy<List<Dictionary<string, object>>> QueryLazy(string name, DbParameter[] param, DataContext db = null, string key = null);

        Task<Lazy<List<Dictionary<string, object>>>> QueryLazyAsy(string name, DbParameter[] param, DataContext db = null, string key = null);

        WriteReturn Write(string name, DbParameter[] param, DataContext db = null, string key = null);

        Task<WriteReturn> WriteAsy(string name, DbParameter[] param, DataContext db = null, string key = null);

        Lazy<WriteReturn> WriteLazy(string name, DbParameter[] param, DataContext db = null, string key = null);

        Task<Lazy<WriteReturn>> WriteLazyAsy(string name, DbParameter[] param, DataContext db = null, string key = null);

        PageResult QueryPage(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null);

        Task<PageResult> QueryPageAsy(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null);

        Lazy<PageResult> QueryPageLazy(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null);

        Task<Lazy<PageResult>> QueryPageLazyAsy(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null);

        PageResult<T> QueryPage<T>(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null) where T : class, new();

        Task<PageResult<T>> QueryPageAsy<T>(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null) where T : class, new();

        Lazy<PageResult<T>> QueryPageLazy<T>(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null) where T : class, new();

        Task<Lazy<PageResult<T>>> QueryPageLazyAsy<T>(PageModel pModel, string name, DbParameter[] param, DataContext db = null, string key = null) where T : class, new();

        string MapDb(string name);

        List<string> MapParam(string name);

        Dictionary<string, object> Api();

        WriteReturn AddList<T>(List<T> list, DataContext db = null, string key = null) where T : class, new();

        Task<WriteReturn> AddListAsy<T>(List<T> list, DataContext db = null, string key = null) where T : class, new();

        WriteReturn Add<T>(T model, DataContext db = null, string key = null) where T : class, new();

        Task<WriteReturn> AddAsy<T>(T model, DataContext db = null, string key = null) where T : class, new();

        WriteReturn Delete<T>(Expression<Func<T, bool>> predicate, DataContext db = null, string key = null) where T : class, new();

        Task<WriteReturn> DeleteAsy<T>(Expression<Func<T, bool>> predicate, DataContext db = null, string key = null) where T : class, new();

        WriteReturn Delete<T>(T model, DataContext db = null, string key = null, bool isTrans = false) where T : class, new();

        Task<WriteReturn> UpdateAsy<T>(T model, DataContext db = null, string key = null, bool isTrans = false) where T : class, new();

        WriteReturn Update<T>(T model, Expression<Func<T, bool>> predicate, Expression<Func<T, object>> field = null, DataContext db = null, string key = null) where T : class, new();

        Task<WriteReturn> UpdateAsy<T>(T model, Expression<Func<T, bool>> predicate, Expression<Func<T, object>> field = null, DataContext db = null, string key = null) where T : class, new();

        WriteReturn Update<T>(T model, Expression<Func<T, object>> field = null, DataContext db = null, string key = null, bool isTrans = false) where T : class, new();

        Task<WriteReturn> UpdateAsy<T>(T model, Expression<Func<T, object>> field = null, DataContext db = null, string key = null, bool isTrans = false) where T : class, new();

        WriteReturn UpdateList<T>(List<T> list, Expression<Func<T, object>> field = null, DataContext db = null, string key = null) where T : class, new();

        Task<WriteReturn> UpdateListAsy<T>(List<T> list, Expression<Func<T, object>> field = null, DataContext db = null, string key = null) where T : class, new();

        WriteReturn ExecuteSql(string sql, DbParameter[] param, DataContext db = null, string key = null);

        Task<WriteReturn> ExecuteSqlAsy(string sql, DbParameter[] param, DataContext db = null, string key = null);

        DataQuery Query<T>(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> field = null, string key = null);

        DataQuery LeftJoin<T, T1>(DataQuery item, Expression<Func<T, T1, bool>> predicate, Expression<Func<T1, object>> field = null, bool isDblink = false);

        DataQuery RightJoin<T, T1>(DataQuery item, Expression<Func<T, T1, bool>> predicate, Expression<Func<T1, object>> field = null, bool isDblink = false) where T1 : class, new();

        DataQuery InnerJoin<T, T1>(DataQuery item, Expression<Func<T, T1, bool>> predicate, Expression<Func<T1, object>> field = null, bool isDblink = false) where T1 : class, new();

        DataQuery OrderBy<T>(DataQuery item, Expression<Func<T, object>> field, bool isDesc = true);

        DataQuery GroupBy<T>(DataQuery item, Expression<Func<T, object>> field);

        DataQuery Take(DataQuery item, int i);

        List<T> ToList<T>(DataQuery item, DataContext db = null) where T : class, new();

        Task<List<T>> ToListAsy<T>(DataQuery item, DataContext db = null) where T : class, new();

        Lazy<List<T>> ToLazyList<T>(DataQuery item, DataContext db = null) where T : class, new();

        Task<Lazy<List<T>>> ToLazyListAsy<T>(DataQuery item, DataContext db = null) where T : class, new();

        string ToJson(DataQuery item, DataContext db = null);

        Task<string> ToJsonAsy(DataQuery item, DataContext db = null);

        Lazy<string> ToLazyJson(DataQuery item, DataContext db = null);

        Task<Lazy<string>> ToLazyJsonAsy(DataQuery item, DataContext db = null);

        T ToItem<T>(DataQuery item, DataContext db = null) where T : class, new();

        Task<T> ToItemAsy<T>(DataQuery item, DataContext db = null) where T : class, new();

        Lazy<T> ToLazyItem<T>(DataQuery item, DataContext db = null) where T : class, new();

        Task<Lazy<T>> ToLazyItemAsy<T>(DataQuery item, DataContext db = null) where T : class, new();

        int ToCount(DataQuery item, DataContext db = null);

        Task<int> ToCountAsy<T, T1>(DataQuery item, DataContext db = null);

        PageResult<T> ToPage<T>(DataQuery item, PageModel pModel, DataContext db = null) where T : class, new();

        Task<PageResult<T>> ToPageAsy<T>(DataQuery item, PageModel pModel, DataContext db = null) where T : class, new();

        Lazy<PageResult<T>> ToLazyPage<T>(DataQuery item, PageModel pModel, DataContext db = null) where T : class, new();

        Task<Lazy<PageResult<T>>> ToLazyPageAsy<T>(DataQuery item, PageModel pModel, DataContext db = null) where T : class, new();

        PageResult ToPage(DataQuery item, PageModel pModel, DataContext db = null);

        Task<PageResult> ToPageAsy(DataQuery item, PageModel pModel, DataContext db = null);

        Lazy<PageResult> ToLazyPage(DataQuery item, PageModel pModel, DataContext db = null);

        Task<Lazy<PageResult>> ToLazyPageAsy(DataQuery item, PageModel pModel, DataContext db = null);

        DataTable ToDataTable(DataQuery item, DataContext db = null);

        Task<DataTable> ToDataTableAsy(DataQuery item, DataContext db = null);

        Lazy<DataTable> ToLazyDataTable(DataQuery item, DataContext db = null);

        Task<Lazy<DataTable>> ToLazyDataTableAsy(DataQuery item, DataContext db = null);

        List<Dictionary<string, object>> ToDics(DataQuery item, DataContext db = null);

        Task<List<Dictionary<string, object>>> ToDicsAsy(DataQuery item, DataContext db = null);

        Lazy<List<Dictionary<string, object>>> ToLazyDics(DataQuery item, DataContext db = null);

        Task<Lazy<List<Dictionary<string, object>>>> ToLazyDicsAsy(DataQuery item, DataContext db = null);

        Dictionary<string, object> ToDic(DataQuery item, DataContext db = null);

        Task<Dictionary<string, object>> ToDicAsy(DataQuery item, DataContext db = null);

        Lazy<Dictionary<string, object>> ToLazyDic(DataQuery item, DataContext db = null);

        Task<Lazy<Dictionary<string, object>>> ToLazyDicAsy(DataQuery item, DataContext db = null);
    }
}
