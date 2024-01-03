using FastData.Core.Model;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

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

        public static DbParameter[] ToDbParameter(DbParameter[] param, ConfigModel config)
        {
            var list = new List<DbParameter>();
            if (param == null || param.Length == 0)
                return list.ToArray();
            else
            {
                if (param.ToList().Exists(a => a.GetType() == typeof(DataParameter)))
                {
                    param.ToList().ForEach(p =>
                    {
                        if (p.GetType() == typeof(DataParameter))
                        {
                            var info = DbProviderFactories.GetFactory(config).CreateParameter();
                            info.ParameterName = p.ParameterName;
                            info.Direction = p.Direction == 0 ? ParameterDirection.Input : p.Direction;
                            info.Value = p.Value;
                            info.DbType = p.DbType;
                            list.Add(info);
                        }
                        else
                            list.Add(p);
                    });

                    return list.ToArray();
                }
                else
                    return param;
            }
        }
    }
}
