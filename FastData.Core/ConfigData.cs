using FastData.Core.Aop;

namespace FastData.Core
{
    public class ConfigData
    {
        public bool IsResource { get; set; }

        public bool IsCodeFirst { get; set; }

        public string NamespaceCodeFirst { get; set; }

        public string NamespaceProperties { get; set; }

        public string dbKey { get; set; }

        public string dbFile { get; set; } = "db.json";

        public string mapFile { get; set; } = "map.json";

        public string NamespaceService { get; set; }

        public IFastAop aop { get; set; }
    }
}
