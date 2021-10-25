using System.Collections.Generic;
using System.Data.Common;

namespace FastData.Core.Aop
{
    public class BeforeContext
    {
        public string dbType { get; internal set; }

        public List<string> tableName { get; internal set; } = new List<string>();

        public string sql { get; internal set; }

        public List<DbParameter> param { get; internal set; } = new List<DbParameter>();

        public bool isRead { get; internal set; } = false;

        public bool isWrite { get; internal set; } = false;

        public AopType type { get; internal set; }

        public object model { get; internal set; }
    }
}
