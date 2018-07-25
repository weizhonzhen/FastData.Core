using System.Collections.Generic;
using System.Data.Common;

namespace Fast.Data.Core.Model
{
    /// <summary>
    /// 增加 修改
    /// </summary>
    internal class OptionModel
    {
        private List<DbParameter> _Param = new List<DbParameter>();

        /// <summary>
        /// 参数
        /// </summary>
        public List<DbParameter> Param
        {
            set { _Param = value; }
            get { return _Param; }
        }

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
