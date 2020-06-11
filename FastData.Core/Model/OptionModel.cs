using System.Collections.Generic;
using System.Data;
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
        public bool IsSuccess { get; set; }
        
        /// <summary>
        /// 是否缓存
        /// </summary>
        public bool IsCache { get; set; }

        /// <summary>
        /// 出错信息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 数据表
        /// </summary>
        public DataTable table { get; set; }
    }
}
