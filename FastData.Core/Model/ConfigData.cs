using FastData.Core.Aop;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FastData.Core.Model
{
    public class ConfigData
    {
        public bool IsResource { get; set; }

        public bool IsCodeFirst { get; set; }

        public string NamespaceCodeFirst { get; set; }

        public string NamespaceProperties { get; set; }

        public string DbKey { get; set; }

        public string DbFile { get; set; } = "db.json";

        public string MapFile { get; set; } = "map.json";

        public string NamespaceService { get; set; }

        public IFastAop Aop { get; set; }

        public Assembly Current { get; set; }
    }
}
