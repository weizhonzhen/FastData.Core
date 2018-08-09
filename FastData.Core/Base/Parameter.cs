using System;
using System.Collections.Generic;
using System.Data.Common;
using FastData.Core.Model;

namespace FastData.Core.Base
{
    /// <summary>
    /// 标签：2015.9.6，魏中针
    /// 说明：parameter操作共用类    
    /// </summary>
    internal static class Parameter
    {
        #region DbParameter重先生成
        /// <summary>
        /// DbParameter重先生成
        /// </summary>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        /// <returns></returns>
        public static List<DbParameter> ReNewParam(List<DbParameter> param, ConfigModel config)
        {
            var list = new List<DbParameter>();

            if (param != null)
            {
                foreach (var item in param)
                {
                    var temp = DbProviderFactories.GetFactory(config).CreateParameter();
                    temp.Value = item.Value == null ? DBNull.Value : item.Value;
                    temp.ParameterName = item.ParameterName;
                    temp.Direction = item.Direction;
                    temp.DbType = item.DbType;
                    list.Add(temp);
                }
            }

            return list;
        }
        #endregion

        #region DbParameter合并
        /// <summary>
        /// DbParameter合并
        /// </summary>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        /// <returns></returns>
        public static List<DbParameter> ParamMerge(List<DbParameter> param1, List<DbParameter> param2)
        {
            if (param2.Count != 0)
                 param1.AddRange(param2);

            return param1;
        }
        #endregion
    }
}
