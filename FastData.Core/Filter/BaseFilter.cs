using FastData.Core.Base;
using FastData.Core.Model;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Linq;

namespace FastData.Core.Filter
{
    internal static class BaseFilter
    {
        #region Filter
        public static void Filter(List<DbParameter> param, FilterType type, List<string> table, ConfigModel config, StringBuilder sql)
        {
            for (var i = 0; i < table.Count; i++)
            {
                var key = $"Filter.{table[i]}.{type.ToString()}";

                if (DbCache.Exists(CacheType.Web, key))
                {
                    param = param ?? new List<DbParameter>();
                    var model = DbCache.Get<VisitModel>(CacheType.Web, key);
                    if (model.IsSuccess)
                    {
                        sql.AppendFormat(" and {0}", model.Where);
                        param.AddRange(model.Param.ToArray());
                    }
                }
            }
        }
        #endregion

        #region Filter
        public static void Filter(DbParameter[] param, FilterType type, List<string> table, ConfigModel config, ref string sql)
        {
            for (var i = 0; i < table.Count; i++)
            {
                var key = $"Filter.{table[i]}.{type.ToString()}";

                if (DbCache.Exists(CacheType.Web, key))
                {
                    param = param ?? (new List<DbParameter>()).ToArray();
                    var model = DbCache.Get<VisitModel>(CacheType.Web, key);
                    sql = string.Format("{0} and {1}", sql, model.Where);

                    param.ToList().AddRange(model.Param.ToArray());
                }
            }
        }
        #endregion
    }
}
