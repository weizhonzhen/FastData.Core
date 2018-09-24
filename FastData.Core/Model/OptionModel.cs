using System.Collections.Generic;
using System.Data.Common;

namespace FastData.Core.Model
{
    /// <summary>
    /// 增加 修改
    /// </summary>
    internal class OptionModel
    {

        /// <summary>
        /// 参数
        /// </summary>
        public List<DbParameter> Param { set; get; } = new List<DbParameter>();

        /// <summary>
        /// sql
        /// </summary>
        public string Sql { get; set; }
        
        /// <summary>
        /// 结果状态
        /// </summary>
        public bool Result { get; set; }
        
        /// <summary>
        /// 是否缓存
        /// </summary>
        public bool IsCache { get; set; }
    }
}
