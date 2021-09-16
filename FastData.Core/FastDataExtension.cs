using FastData.Core;
using FastData.Core.Aop;
using FastData.Core.Base;
using FastData.Core.Filter;
using FastData.Core.Model;
using FastData.Core.Repository;
using FastUntility.Core;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Loader;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FastDataExtension
    {
        private static ConfigData config;

        public static IServiceCollection AddFastData(this IServiceCollection serviceCollection, Action<ConfigData> action)
        {
            config = new ConfigData();
            action(config);

            serviceCollection.AddTransient<IFastRepository, FastRepository>();

            if (config.aop != null)
                serviceCollection.AddSingleton<IFastAop>(config.aop);

            ServiceContext.Init(new ServiceEngine(serviceCollection.BuildServiceProvider()));

            var projectName = Assembly.GetCallingAssembly().GetName().Name;
            if (config.IsResource)
                FastMap.InstanceMapResource(config.dbKey, config.dbFile, config.mapFile, projectName);
            else
                FastMap.InstanceMap(config.dbKey, config.dbFile, config.mapFile);

            if (config.IsCodeFirst && !string.IsNullOrEmpty(config.NamespaceCodeFirst) && config.IsResource)
            {
                FastMap.InstanceProperties(config.NamespaceCodeFirst, config.dbFile, projectName);
                FastMap.InstanceTable(config.NamespaceCodeFirst, config.dbKey, config.dbFile, projectName);
            }
            else if (config.IsCodeFirst && !string.IsNullOrEmpty(config.NamespaceCodeFirst))
            {
                FastMap.InstanceProperties(config.NamespaceCodeFirst, config.dbFile);
                FastMap.InstanceTable(config.NamespaceCodeFirst, config.dbKey, config.dbFile);
            }

            if (!string.IsNullOrEmpty(config.NamespaceProperties))
            {
                AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(Assembly.GetCallingAssembly().GetName().Name)).GetTypes()
              .Where(a => a.Namespace != null && a.Namespace.Contains(config.NamespaceProperties)).ToList().ForEach(b =>
              {
                  if (config.IsResource)
                      FastMap.InstanceProperties(b.Namespace, config.dbFile, projectName);
                  else
                      FastMap.InstanceProperties(b.Namespace, config.dbFile);
              });
            }

            return serviceCollection;
        }

        public static IServiceCollection AddFastFilter<T>(this IServiceCollection serviceCollection, Expression<Func<T, bool>> predicate, FilterType type)
        {
            if (config != null)
            {
                ConfigModel item;
                string projectName = null;
                if (config.IsResource)
                    projectName = Assembly.GetCallingAssembly().GetName().Name;

                item = DataConfig.Get(config.dbKey, projectName, config.dbFile);
                var model = VisitExpression.LambdaWhere<T>(predicate, item);

                if (predicate.Parameters.Count > 0)
                {
                    var flag = string.Format("{0}.", (predicate.Parameters[0] as ParameterExpression).Name);
                    model.Where = model.Where.Replace(flag, "");
                }

                var key = $"Filter.{typeof(T).Name}.{type.ToString()}";
                DbCache.Set<VisitModel>(CacheType.Web, key, model);
            }

            return serviceCollection;
        }
    }

    public class ConfigData
    {
        public bool IsResource { get; set; }

        public bool IsCodeFirst { get; set; }

        public string NamespaceCodeFirst { get; set; }

        public string NamespaceProperties { get; set; }

        public string dbKey { get; set; }

        public string dbFile { get; set; } = "db.json";

        public string mapFile { get; set; } = "map.json";

        public IFastAop aop { get; set; }
    }
}
