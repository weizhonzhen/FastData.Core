using System.Collections.Generic;
using System.Data.Common;

namespace FastData.Core.Aop
{
    public class MapContext
    {
        public string dbType { get; set; }

        public string sql { get; set; }

        public string mapName { get; set; }

        public List<DbParameter> param { get; set; } = new List<DbParameter>();
}
}
