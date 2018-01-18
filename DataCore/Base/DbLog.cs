using System;
using Untility.Core.Base;

namespace Data.Core.Base
{
    internal static class DbLog
    {
        #region 数据库出错日志
        /// <summary>
        /// 数据库出错日志
        /// </summary>
        /// <param name="IsOutError"></param>
        /// <param name="IsAsyc"></param>
        /// <param name="dbType"></param>
        /// <param name="expContent"></param>
        public static void LogException<T>(bool IsOutError, string dbType, Exception ex, string CurrentMethod, string sql)
        {
            if (IsOutError)
            {
                var content = string.Format("方法：{0},对象：{1},{3}出错详情：{2}"
                                              , CurrentMethod
                                              , typeof(T).Name
                                              , ex.ToString()
                                              , sql == "" ? "" : string.Format("SQL：{0},", sql));

                BaseLog.SaveLog(content, string.Format("{0}_Error", dbType));
            }
        }
        #endregion 

        #region 数据库出错日志
        /// <summary>
        /// 数据库出错日志
        /// </summary>
        /// <param name="IsOutError"></param>
        /// <param name="IsAsyc"></param>
        /// <param name="dbType"></param>
        /// <param name="expContent"></param>
        public static void LogException(bool IsOutError, string dbType, Exception ex, string CurrentMethod, string sql)
        {
            if (IsOutError)
            {
                var content = string.Format("方法：{0},{2}出错详情：{1}"
                                              , CurrentMethod
                                              , ex.ToString()
                                              , sql == "" ? "" : string.Format("SQL：{0},", sql));

                BaseLog.SaveLog(content, string.Format("{0}_Error", dbType));
            }
        }
        #endregion 

        #region 数据库sql日志
        /// <summary>
        /// 数据库sql日志
        /// </summary>
        /// <param name="IsOutSql"></param>
        /// <param name="IsAsyc"></param>
        /// <param name="sql"></param>
        /// <param name="dbType"></param>
        public static void LogSql(bool IsOutSql, string sql, string dbType, double time,string type="")
        {
            if (IsOutSql)
            {
                if(type=="")
                    BaseLog.SaveLog(string.Format("{0}[{1}毫秒]", sql, time), string.Format("{0}_Sql", dbType));
                else
                    BaseLog.SaveLog(string.Format("{0}[{1}毫秒]", sql, time), string.Format("{1}_{0}_Sql", dbType,type));
            }
        }
        #endregion

        #region 数据库sql code first日志
        /// <summary>
        /// 数据库sql code first日志
        /// </summary>
        /// <param name="IsOutSql"></param>
        /// <param name="IsAsyc"></param>
        /// <param name="sql"></param>
        /// <param name="dbType"></param>
        public static void LogSql(bool IsOutSql, string sql, string dbType)
        {
            if (IsOutSql)
            {
                BaseLog.SaveLog(string.Format("{0}", sql), string.Format("{0}_CodeFirst_Sql", dbType));
            }
        }
        #endregion 
    }
}
