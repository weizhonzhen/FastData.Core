using System;

namespace FastData.Core.Property
{
    /// <summary>
    /// 表属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class  TableAttribute : Attribute
    {
        /// <summary>
        /// 备注
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// 表名
        /// </summary>
        public string Name { get; set; }
    }
}
