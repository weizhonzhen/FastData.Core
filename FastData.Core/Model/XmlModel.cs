using System.Collections.Generic;

namespace FastData.Core.Model
{
    public class XmlModel
    {
        public List<string> key { get; set; } = new List<string>();

        public List<string> sql { get; set; } = new List<string>();

        public Dictionary<string, object> db { get; set; } = new Dictionary<string, object>();

        public Dictionary<string, object> type { get; set; } = new Dictionary<string, object>();

        public Dictionary<string, object> view { get; set; } = new Dictionary<string, object>();

        public Dictionary<string, object> param { get; set; } = new Dictionary<string, object>();

        public Dictionary<string, object> check { get; set; } = new Dictionary<string, object>();

        public Dictionary<string, object> name { get; set; } = new Dictionary<string, object>();

        public Dictionary<string, object> parameName { get; set; } = new Dictionary<string, object>();

        public bool isSuccess { get; set; }
    }
}
