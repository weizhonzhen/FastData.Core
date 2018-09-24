using System.Collections.Generic;
namespace FastData.Core.Model
{
    /// <summary>
    /// 字段返回结果
    /// </summary>
    internal class FieldModel
    {
        /// <summary>
        /// 字段名
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// 字段别名
        /// </summary>
        public List<string> AsName { set; get; } = new List<string>();
    }
}
