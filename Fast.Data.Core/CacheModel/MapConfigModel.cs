using System;
using System.Collections.Generic;

namespace Fast.Data.Core.CacheModel
{
    internal class MapConfigModel
    {
        private List<string> _path = new List<string>();

        /// <summary>
        /// 文件名带绝对路径
        /// </summary>
        public List<string> Path
        {
            set { _path = value; }
            get { return _path; }
        }

        /// <summary>
        /// 上次修改时间
        /// </summary>
        public DateTime LastWrite { get; set; }
    }
}
