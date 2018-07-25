using System.Collections.Generic;
using System.Data.Common;
using Fast.Data.Core.Model;

namespace Fast.Data.Core.Base
{
    /// <summary>
    /// 标签：2015.9.6，魏中针
    /// 说明：Parameter转sql
    /// </summary>
    internal static class ParameterToSql
    {
        #region object 转sql
         /// <summary>
        /// 标签：2015.9.6，魏中针
        /// 说明：DbParameter转sql
        /// </summary>
        /// <returns></returns>
        public static string ObjectParamToSql(List<DbParameter> param, string Sql, ConfigModel config)
        {
            if (param == null)
                return Sql;
            Sql = string.Format("sql:{0},param:", Sql);
            foreach (var item in param)
            {
                if (item != null)
                    Sql = string.Format("{0}{1}={2},", Sql, item.ParameterName, item.Value);                   
            }

            return Sql;
        }
        #endregion
    }
}
