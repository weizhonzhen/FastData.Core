using System;
using System.Collections.Generic;

namespace Data.Core.CacheModel
{
    /// <summary>
    /// xml
    /// </summary>
    internal class MapXmlModel
    {
        /// <summary>
        /// 上次修改时间
        /// </summary>
        public DateTime LastWrite { get; set; }

        /// <summary>
        /// xml键
        /// </summary>
        public List<string> FileKey { get; set; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }
    }
}
