using System.Collections.Generic;
namespace Data.Core.Model
{
    /// <summary>
    /// 字段返回结果
    /// </summary>
    internal class FieldModel
    {
        private List<string> _asName = new List<string>();

        /// <summary>
        /// 字段名
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// 字段别名
        /// </summary>
        public List<string> AsName
        {
            set { _asName = value; }
            get { return _asName; }
        }
    }
}
