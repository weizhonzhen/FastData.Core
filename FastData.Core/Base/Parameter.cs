using System.Collections.Generic;
using System.Data.Common;

namespace FastData.Core.Base
{
    /// <summary>
    /// 标签：2015.9.6，魏中针
    /// 说明：parameter操作共用类    
    /// </summary>
    internal static class Parameter
    {
        #region DbParameter合并
        /// <summary>
        /// DbParameter合并
        /// </summary>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        /// <returns></returns>
        public static List<DbParameter> ParamMerge(List<DbParameter> param1, List<DbParameter> param2)
        {
            var result = new List<DbParameter>();

            if (param1.Count != 0)
                result.AddRange(param1);

            if (param2.Count != 0)
                result.AddRange(param2);

            return result;
        }
        #endregion
    }
}
