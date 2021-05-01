using System;
using System.Collections.Generic;

namespace FastData.Core.CacheModel
{
    internal class MapConfigModel
    {
        /// <summary>
        /// 文件名带绝对路径
        /// </summary>
        public List<string> Path { get; set; } = new List<string>();

        /// <summary>
        /// 上次修改时间
        /// </summary>
        public DateTime LastWrite { get; set; }
    }
}
