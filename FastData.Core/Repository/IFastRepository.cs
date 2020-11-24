using FastData.Core.Context;
using FastData.Core.Model;
using FastUntility.Core.Page;
using System;
using System.Collections.Generic;
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

        string MapDb(string name, bool isMapDb = false);

        List<string> MapParam(string name);

        Dictionary<string, object> Api();

        bool CheckMap(string xml, string dbKey = null);

        string MapDb(string name);

        string MapType(string name);

        bool IsExists(string name);

        string MapRemark(string name);

        string MapParamRemark(string name, string param);

        string MapRequired(string name, string param);

        string MapMaxlength(string name, string param);

        string MapDate(string name, string param);

        string MapCheckMap(string name, string param);

        string MapExistsMap(string name, string param);

        WriteReturn AddList<T>(List<T> list, string key = null, bool IsTrans = false, bool isLog = true) where T : class, new();

        Task<WriteReturn> AddListAsy<T>(List<T> list, string key = null, bool IsTrans = false, bool isLog = true) where T : class, new();

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

        IQuery Query<T>(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> field = null, string key = null, string projectName = null, string dbFile = "db.json");

        IFastRepository SetKey(string key);
    }
}
