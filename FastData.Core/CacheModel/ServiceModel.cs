using System;
using System.Collections.Generic;

namespace FastData.Core.CacheModel
{
    internal class ServiceModel
    {
        public Dictionary<string, object> param { get; set; } = new Dictionary<string, object>();

        public string sql { get; set; }

        public bool isWrite { get; set; }

        public bool isList { get; set; }

        public System.Type type { get; set; }

        public String dbKey { get; set; }

        public bool isSysType { get; set; }

        public bool isDic { get; set; }

        public bool isPage { get; set; }
    }
}
