using System.Collections.Generic;

namespace Data.Core.CacheModel
{
    /// <summary>
    /// 表实体
    /// </summary>
    internal class TableModel
    {
        /// <summary>
        /// 表名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// 列信息
        /// </summary>
        public List<ColumnModel> Column { get; set; }
    }
}
