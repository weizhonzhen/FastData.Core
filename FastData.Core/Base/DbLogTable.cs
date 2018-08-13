using FastData.Core.Context;
using FastData.Core.Model;
using FastData.Core.Type;
using System;

namespace FastData.Core.Base
{
    internal static class DbLogTable
    {
        /// <summary>
        /// 数据库出错日志
        /// </summary>
        public static void LogException<T>(ConfigModel config, Exception ex, string CurrentMethod, string sql)
        {
            SaveToDb(config, ex, CurrentMethod, sql, typeof(T).Name);
        }


        /// <summary>
        /// 数据库出错日志
        /// </summary>
        public static void LogException(ConfigModel config, Exception ex, string CurrentMethod, string sql)
        {
            SaveToDb(config, ex, CurrentMethod, sql, "");
        }

        /// <summary>
        /// 存数据库
        /// </summary>
        private static void SaveToDb(ConfigModel config, Exception ex, string CurrentMethod, string sql, string type)
        {
            if (config.IsOutError)
            {
                using (var db = new DataContext(config.Key))
                {
                    if (config.DbType == DataDbType.MySql)
                    {
                        var model = new FastData.Core.DataModel.MySql.Data_LogError();
                        model.AddTime = DateTime.Now;
                        model.Content = ex.StackTrace;
                        model.ErrorId = Guid.NewGuid().ToString();
                        model.Method = CurrentMethod;
                        model.Type = type;
                        model.Sql = sql;
                        db.Add(model);
                    }

                    if (config.DbType == DataDbType.Oracle)
                    {
                        var model = new FastData.Core.DataModel.Oracle.Data_LogError();
                        model.AddTime = DateTime.Now;
                        model.Content = ex.StackTrace;
                        model.ErrorId = Guid.NewGuid().ToString();
                        model.Method = CurrentMethod;
                        model.Type = type;
                        model.Sql = sql;
                        db.Add(model);
                    }

                    if (config.DbType == DataDbType.SqlServer)
                    {
                        var model = new FastData.Core.DataModel.SqlServer.Data_LogError();
                        model.AddTime = DateTime.Now;
                        model.Content = ex.StackTrace;
                        model.ErrorId = Guid.NewGuid().ToString();
                        model.Method = CurrentMethod;
                        model.Type = type;
                        model.Sql = sql;
                        db.Add(model);
                    }
                }
            }
        }
    }
}