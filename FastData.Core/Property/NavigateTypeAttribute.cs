using System;

namespace FastData.Core.Property
{
    /// <summary>
    /// 字典导航属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class NavigateTypeAttribute : Attribute
    {
        /// <summary>
        /// 类型
        /// </summary>
        public System.Type Type { get; set; }
    }
}
