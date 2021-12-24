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

        /// <summary>
        ///  否增加
        /// </summary>
        public bool IsAdd { get; set; }

        /// <summary>
        /// 是否修改
        /// </summary>
        public bool IsUpdate { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDel { get; set; }
    }
}
