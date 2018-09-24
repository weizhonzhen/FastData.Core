using System.Collections.Generic;
using System.Data.Common;

namespace FastData.Core.Model
{
    /// <summary>
    /// lambda查询
    /// </summary>
    internal class VisitModel
    {
        /// <summary>
        /// 参数
        /// </summary>
        public List<DbParameter> Param { set; get; } = new List<DbParameter>();

        /// <summary>
        /// 条件
        /// </summary>
        public string Where { get; set; }
    }
}
