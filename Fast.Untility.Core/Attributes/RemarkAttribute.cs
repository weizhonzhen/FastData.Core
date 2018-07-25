using System;

namespace Fast.Untility.Core.Attributes
{
    /// <summary>
    /// 特性
    /// </summary>
    public sealed class RemarkAttribute : Attribute
    {
        private string m_value = "";

        /// <summary>
        /// 特性备注
        /// </summary>
        /// <param name="value"></param>
        public RemarkAttribute(string value)
        {
            m_value = value;
        }

        /// <summary>
        /// 特性备注
        /// </summary>
        public string Remark
        {
            get { return m_value; }
        }
    }
}
