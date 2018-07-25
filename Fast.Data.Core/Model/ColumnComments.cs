namespace Fast.Data.Core.Model
{
    /// <summary>
    /// 列备注
    /// </summary>
    internal class ColumnComments
    {
        /// <summary>
        /// 备注
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// 列名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public ColumnType Type { get; set; }
    }
}
