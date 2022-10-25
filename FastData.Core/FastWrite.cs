using FastData.Core.Base;
using FastData.Core.Context;
using FastData.Core.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FastData.Core
{
    /// <summary>
    /// 标签：2015.9.6，魏中针
    /// 说明：数据库操作类
    /// </summary>
    public static class FastWrite
    {
        #region 批量增加
        /// <summary>
        /// 批量增加
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="model">实体</param>
        /// <param name="IsTrans">是否事务</param>
        /// <returns></returns>
        public static WriteReturn AddList<T>(List<T> list, string key = null, bool IsTrans = false, bool isLog = true) where T : class, new()
        {
            ConfigModel config = null;
            var result = new DataReturn<T>();
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            using (var tempDb = new DataContext(key))
            {
                result = tempDb.AddList<T>(list, IsTrans, isLog);
                config = tempDb.config;
            }

            stopwatch.Stop();

            DbLog.LogSql(config.IsOutSql, result.Sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;

            return result.WriteReturn;
        }
        #endregion

        #region 批量增加 asy
        /// <summary>
        /// 批量增加 asy
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="model">实体</param>
        /// <param name="IsTrans">是否事务</param>
        /// <returns></returns>
        public static ValueTask<WriteReturn> AddListAsy<T>(List<T> list, string key = null, bool IsTrans = false, bool isLog = true) where T : class, new()
        {
            return new ValueTask<WriteReturn>(AddList<T>(list, key, IsTrans, isLog));
        }
        #endregion


        #region 增加
        /// <summary>
        /// 增加
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="model">实体</param>
        /// <param name="IsTrans">是否事务</param>
        /// <param name="notAddField">不需要增加的字段</param>
        /// <returns></returns>
        public static WriteReturn Add<T>(T model, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            ConfigModel config = null;
            var result = new DataReturn<T>();
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(key))
                {
                    result = tempDb.Add<T>(model, false);
                    config = tempDb.config;
                }
            }
            else
            {
                result = db.Add<T>(model, false);
                config = db.config;
            }

            stopwatch.Stop();

            config.IsOutSql = config.IsOutSql ? config.IsOutSql : isOutSql;
            DbLog.LogSql(config.IsOutSql, result.Sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;
            return result.WriteReturn;
        }
        #endregion

        #region 增加 asy
        /// <summary>
        /// 增加 asy
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="model">实体</param>
        /// <param name="IsTrans">是否事务</param>
        /// <param name="notAddField">不需要增加的字段</param>
        /// <returns></returns>
        public static ValueTask<WriteReturn> AddAsy<T>(T model, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            return new ValueTask<WriteReturn>(Add<T>(model, db, key));
        }
        #endregion


        #region 删除(Lambda表达式)
        /// <summary>
        /// 删除(Lambda表达式)
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="predicate">表达式</param>
        /// <param name="IsTrans">是否事务</param>
        /// <returns></returns>
        public static WriteReturn Delete<T>(Expression<Func<T, bool>> predicate, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            ConfigModel config = null;
            var result = new DataReturn<T>();
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(key))
                {
                    result = tempDb.Delete<T>(predicate);
                    config = tempDb.config;
                }
            }
            else
            {
                result = db.Delete<T>(predicate);
                config = db.config;
            }

            stopwatch.Stop();

            config.IsOutSql = config.IsOutSql ? config.IsOutSql : isOutSql;
            DbLog.LogSql(config.IsOutSql, result.Sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;
            return result.WriteReturn;
        }
        #endregion

        #region 删除(Lambda表达式)asy
        /// <summary>
        /// 删除(Lambda表达式)asy
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="predicate">表达式</param>
        /// <param name="IsTrans">是否事务</param>
        /// <returns></returns>
        public static ValueTask<WriteReturn> DeleteAsy<T>(Expression<Func<T, bool>> predicate, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            return new ValueTask<WriteReturn>(Delete<T>(predicate, db, key, isOutSql));
        }
        #endregion


        #region 删除
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static WriteReturn Delete<T>(T model, DataContext db = null, string key = null, bool isTrans = false, bool isOutSql = false) where T : class, new()
        {
            ConfigModel config = null;
            var result = new DataReturn<T>();
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(key))
                {
                    result = tempDb.Delete(model, isTrans);
                    config = tempDb.config;
                }
            }
            else
            {
                result = db.Delete(model, isTrans);
                config = db.config;
            }

            stopwatch.Stop();

            config.IsOutSql = config.IsOutSql ? config.IsOutSql : isOutSql;
            DbLog.LogSql(config.IsOutSql, result.Sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;

            return result.WriteReturn;
        }
        #endregion

        #region 删除asy
        /// <summary>
        /// 删除asy
        /// </summary>
        /// <returns></returns>
        public static ValueTask<WriteReturn> UpdateAsy<T>(T model, DataContext db = null, string key = null, bool isTrans = false, bool isOutSql = false) where T : class, new()
        {
            return new ValueTask<WriteReturn>(Delete<T>(model, db, key, isTrans));
        }
        #endregion


        #region 修改(Lambda表达式)
        /// <summary>
        /// 修改(Lambda表达式)
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="model">实体</param>
        /// <param name="predicate">表达式</param>
        /// <param name="IsTrans">是否事务</param>
        /// <param name="field">需要修改的字段</param>
        /// <returns></returns>
        public static WriteReturn Update<T>(T model, Expression<Func<T, bool>> predicate, Expression<Func<T, object>> field = null, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            ConfigModel config = null;
            var result = new DataReturn<T>();
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(key))
                {
                    result = tempDb.Update<T>(model, predicate, field);
                    config = tempDb.config;
                }
            }
            else
            {
                result = db.Update<T>(model, predicate, field);
                config = db.config;
            }

            stopwatch.Stop();

            config.IsOutSql = config.IsOutSql ? config.IsOutSql : isOutSql;
            DbLog.LogSql(config.IsOutSql, result.Sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;

            return result.WriteReturn;
        }
        #endregion

        #region 修改(Lambda表达式)asy
        /// <summary>
        /// 修改(Lambda表达式)asy
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="model">实体</param>
        /// <param name="predicate">表达式</param>
        /// <param name="IsTrans">是否事务</param>
        /// <param name="field">需要修改的字段</param>
        /// <returns></returns>
        public static ValueTask<WriteReturn> UpdateAsy<T>(T model, Expression<Func<T, bool>> predicate, Expression<Func<T, object>> field = null, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            return new ValueTask<WriteReturn>(Update<T>(model, predicate, field, db, key, isOutSql));
        }
        #endregion


        #region 修改
        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static WriteReturn Update<T>(T model, Expression<Func<T, object>> field = null, DataContext db = null, string key = null, bool isTrans = false, bool isOutSql = false) where T : class, new()
        {
            ConfigModel config = null;
            var result = new DataReturn<T>();
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(key))
                {
                    result = tempDb.Update(model, field, isTrans);
                    config = tempDb.config;
                }
            }
            else
            {
                result = db.Update(model, field, isTrans);
                config = db.config;
            }

            stopwatch.Stop();

            config.IsOutSql = config.IsOutSql ? config.IsOutSql : isOutSql;
            DbLog.LogSql(config.IsOutSql, result.Sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;

            return result.WriteReturn;
        }
        #endregion

        #region 修改asy
        /// <summary>
        /// 修改asy
        /// </summary>
        /// <returns></returns>
        public static ValueTask<WriteReturn> UpdateAsy<T>(T model, Expression<Func<T, object>> field = null, DataContext db = null, string key = null, bool isTrans = false, bool isOutSql = false) where T : class, new()
        {
            return new ValueTask<WriteReturn>(Update<T>(model, field, db, key, isTrans, isOutSql));
        }
        #endregion


        #region 修改list
        /// <summary>
        /// 修改list
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static WriteReturn UpdateList<T>(List<T> list, Expression<Func<T, object>> field = null, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            ConfigModel config = null;
            var result = new DataReturn<T>();
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(key))
                {
                    result = tempDb.UpdateList(list, field);
                    config = tempDb.config;
                }
            }
            else
            {
                result = db.UpdateList(list, field);
                config = db.config;
            }

            stopwatch.Stop();

            config.IsOutSql = config.IsOutSql ? config.IsOutSql : isOutSql;
            DbLog.LogSql(config.IsOutSql, result.Sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;

            return result.WriteReturn;
        }
        #endregion

        #region 修改list asy
        /// <summary>
        /// 修改list asy
        /// </summary>
        /// <returns></returns>
        public static ValueTask<WriteReturn> UpdateListAsy<T>(List<T> list, Expression<Func<T, object>> field = null, DataContext db = null, string key = null, bool isOutSql = false) where T : class, new()
        {
            return new ValueTask<WriteReturn>(UpdateList<T>(list, field, db, key, isOutSql));
        }
        #endregion


        #region 执行sql
        /// <summary>
        /// 执行sql
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static WriteReturn ExecuteSql(string sql, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false, bool isAop = true)
        {
            ConfigModel config = null;
            var result = new DataReturn();
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(key))
                {
                    config = tempDb.config;
                    config.IsOutSql = config.IsOutSql ? config.IsOutSql : isOutSql;
                    result = tempDb.ExecuteSql(sql, param, false, config.IsOutSql, false);
                }
            }
            else
            {
                config = db.config;
                config.IsOutSql = config.IsOutSql ? config.IsOutSql : isOutSql;
                result = db.ExecuteSql(sql, param, false, config.IsOutSql, false);
            }

            stopwatch.Stop();

            DbLog.LogSql(config.IsOutSql, result.Sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;

            return result.WriteReturn;
        }
        #endregion

        #region 执行sql asy
        /// <summary>
        /// 执行sql asy
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static ValueTask<WriteReturn> ExecuteSqlAsy(string sql, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            return new ValueTask<WriteReturn>(ExecuteSql(sql, param, db, key, isOutSql));
        }
        #endregion


        #region 执行sql DDL
        /// <summary>
        /// 执行sql DDL
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static WriteReturn ExecuteDDL(string sql, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            ConfigModel config = null;
            var result = new DataReturn();
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            if (db == null)
            {
                using (var tempDb = new DataContext(key))
                {
                    config = tempDb.config;
                    config.IsOutSql = config.IsOutSql ? config.IsOutSql : isOutSql;
                    result = tempDb.ExecuteDDL(sql, param, config.IsOutSql);
                }
            }
            else
            {
                config = db.config;
                config.IsOutSql = config.IsOutSql ? config.IsOutSql : isOutSql;
                result = db.ExecuteSql(sql, param, false, config.IsOutSql);
            }

            stopwatch.Stop();

            DbLog.LogSql(config.IsOutSql, result.Sql, config.DbType, stopwatch.Elapsed.TotalMilliseconds);
            stopwatch = null;

            return result.WriteReturn;
        }
        #endregion

        #region 执行sql DDL asy
        /// <summary>
        /// 执行sql DDL asy
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static ValueTask<WriteReturn> ExecuteDDLAsy(string sql, DbParameter[] param, DataContext db = null, string key = null, bool isOutSql = false)
        {
            return new ValueTask<WriteReturn>(ExecuteDDL(sql, param, db, key, isOutSql));
        }
        #endregion
    }
}