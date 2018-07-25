using System.Collections.Generic;
using System.Data.Common;

namespace Fast.Data.Core.Model
{
    /// <summary>
    /// lambda查询
    /// </summary>
    internal class VisitModel
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
        /// 条件
        /// </summary>
        public string Where { get; set; }
    }
}
