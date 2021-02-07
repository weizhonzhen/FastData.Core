using FastData.Core;
using FastData.Core.Repository;
using FastUntility.Core;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FastDataExtension
    {
        public static IServiceCollection AddFastData(this IServiceCollection serviceCollection, ConfigModel config)
        {
            if (config.IsResource)
                FastMap.InstanceMapResource(config.dbKey, config.dbFile, config.mapFile);
            else
                FastMap.InstanceMap(config.dbKey, config.dbFile, config.mapFile);

            if (config.IsCodeFirst && !string.IsNullOrEmpty(config.NamespaceCodeFirst))
                FastMap.InstanceTable(config.NamespaceCodeFirst, config.dbKey, config.dbFile);

            if (!string.IsNullOrEmpty(config.NamespaceProperties))
            {
                AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(Assembly.GetCallingAssembly().GetName().Name)).GetTypes()
              .Where(a => a.Namespace != null && a.Namespace.Contains(config.NamespaceProperties)).ToList().ForEach(b =>
              {
                  FastMap.InstanceProperties(b.Namespace, config.dbFile);
              });
            }

            serviceCollection.AddTransient<IFastRepository, FastRepository>();
            ServiceContext.Init(new ServiceEngine(serviceCollection.BuildServiceProvider()));
            return serviceCollection;
        }
    }

    public class ConfigModel
    {
        public bool IsResource { get; set; }

        public bool IsCodeFirst { get; set; }

        public string NamespaceCodeFirst { get; set; }


        public string NamespaceProperties { get; set; }

        public string dbKey { get; set; }

        public string dbFile { get; set; } = "db.json";

        public string mapFile { get; set; } = "map.json";
    }
}
