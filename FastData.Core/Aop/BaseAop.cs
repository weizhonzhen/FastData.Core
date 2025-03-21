using FastData.Core.Model;
using FastUntility.Core;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace FastData.Core.Aop
{
    internal static class BaseAop
    {
       private static IFastAop aop = ServiceContext.Engine.Resolve<IFastAop>();

        #region Aop Before
        /// <summary>
        /// Aop Before
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <param name="config"></param>
        public static void AopBefore(List<string> tableName, string sql, List<DbParameter> param, ConfigModel config, bool isRead, AopType type, object model = null)
        {
            if (aop != null)
            {
                var context = new BeforeContext();

                if (tableName != null)
                    context.tableName = tableName;

                context.sql = sql;

                if (param != null)
                    context.param = param;

                context.dbType = config.DbType;
                context.isRead = isRead;
                context.isWrite = !isRead;
                context.type = type;
                context.model = model;

                aop.Before(context);
            }
        }
        #endregion

        #region Aop After
        /// <summary>
        /// Aop After
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <param name="config"></param>
        public static void AopAfter(List<string> tableName, string sql, List<DbParameter> param, ConfigModel config, bool isRead, AopType type, object result, object model = null)
        {
            if (aop != null)
            {
                var context = new AfterContext();

                if (tableName != null)
                    context.tableName = tableName;

                context.sql = sql;

                if (param != null)
                    context.param = param;

                context.dbType = config.DbType;
                context.isRead = isRead;
                context.isWrite = !isRead;
                context.result = result;
                context.type = type;
                context.model = model;

                aop.After(context);
            }
        }
        #endregion

        #region Aop Exception
        /// <summary>
        /// Aop Exception
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="name"></param>
        public static void AopException(Exception ex, string name, AopType type, ConfigModel config, object model = null)
        {
            if (aop != null)
            {
                var context = new ExceptionContext();
                context.name = name;
                context.type = type;
                context.ex = ex;
                context.dbType = config?.DbType;
                context.model = model;

                aop.Exception(context);
            }
        }
        #endregion

        #region Aop Map Before
        /// <summary>
        /// Aop Map Before
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <param name="config"></param>
        public static void AopMapBefore(string mapName, string sql, DbParameter[] param, ConfigModel config, AopType type)
        {
            if (aop != null)
            {
                var context = new MapBeforeContext();
                context.mapName = mapName;
                context.sql = sql;
                context.type = type;

                if (param != null)
                    context.param = param.ToList();

                context.dbType = config.DbType;

                aop.MapBefore(context);
            }
        }
        #endregion

        #region Aop Map After
        /// <summary>
        /// Aop Map After
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <param name="config"></param>
        public static void AopMapAfter(string mapName, string sql, DbParameter[] param, ConfigModel config, AopType type, object data)
        {
            if (aop != null)
            {
                var context = new MapAfterContext();
                context.mapName = mapName;
                context.sql = sql;
                context.type = type;

                if (param != null)
                    context.param = param.ToList();

                context.dbType = config.DbType;
                context.result = data;

                aop.MapAfter(context);
            }
        }
        #endregion
    }
}
