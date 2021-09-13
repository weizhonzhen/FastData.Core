using System.Collections.Generic;
using System.Data.Common;

namespace FastData.Core.Aop
{
    public class MapBeforeContext
    {
        public string dbType { get; internal set; }

        public string sql { get; internal set; }

        public string mapName { get; internal set; }

        public List<DbParameter> param { get; internal set; } = new List<DbParameter>();

        public AopType type { get; internal set; }
    }
}
