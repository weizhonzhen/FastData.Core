using System;

namespace Data.Core.Property
{
    /// <summary>
    /// 字段属性
    /// </summary>
    public class ColumnAttribute : Attribute
    {
        /// <summary>
        /// 主键
        /// </summary>
        public bool IsKey { get; set; }

        /// <summary>
        /// 长度
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// 精度
        /// </summary>
        public int Precision { get; set; }

        /// <summary>
        /// 小数点位数
        /// </summary>
        public int Scale { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// 是否空
        /// </summary>
        public bool IsNull { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Comments { get; set; }
    }
}
