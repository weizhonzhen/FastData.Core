using System;

namespace FastData.Core.Property
{
    [AttributeUsage(AttributeTargets.Method)]
    public class FastReadAttribute : Attribute
    {
        /// <summary>
        /// sql
        /// </summary>
        public string sql { get; set; }

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