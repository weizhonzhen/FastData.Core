using System.Collections.Generic;

namespace FastData.Core.Model
{
    internal class XmlModel
    {
        public List<string> Key { get; set; } = new List<string>();

        public List<string> Sql { get; set; } = new List<string>();

        public Dictionary<string, object> Db { get; set; } = new Dictionary<string, object>();

        public Dictionary<string, object> Type { get; set; } = new Dictionary<string, object>();

        public Dictionary<string, object> View { get; set; } = new Dictionary<string, object>();

        public Dictionary<string, object> Param { get; set; } = new Dictionary<string, object>();

        public Dictionary<string, object> Check { get; set; } = new Dictionary<string, object>();

        public Dictionary<string, object> Name { get; set; } = new Dictionary<string, object>();

        public Dictionary<string, object> ParameName { get; set; } = new Dictionary<string, object>();

        public bool IsSuccess { get; set; }
    }
}
