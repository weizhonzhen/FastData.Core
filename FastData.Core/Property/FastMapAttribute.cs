using System;
using System.Collections.Generic;
using System.Text;

namespace FastData.Core.Property
{
    [AttributeUsage(AttributeTargets.Method)]
    public class FastMapAttribute : Attribute
    {
        /// <summary>
        /// xml
        /// </summary>
        public string xml { get; set; }

        /// <summary>
        /// 数据库key
        /// </summary>
        public string dbKey { get; set; }

        /// <summary>
        /// 是否分页
        /// </summary>
        public bool isPage { get; set; }
    }
}
