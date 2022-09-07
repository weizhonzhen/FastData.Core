﻿using FastData.Core;
using FastData.Core.Aop;
using FastData.Core.Base;
using FastData.Core.Filter;
using FastData.Core.Model;
using FastData.Core.Proxy;
using FastData.Core.Repository;
using FastRedis.Core.Repository;
using FastUntility.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            if (!config.IsResource && DataConfig.Get(config.dbKey, null, config.dbFile).CacheType == CacheType.Redis && ServiceContext.Engine.Resolve<IRedisRepository>() == null)
                throw new System.Exception("ConfigureServices First add services.AddFastRedis(); Second add services.AddFastData()");

            if (DataConfig.Get(config.dbKey, Assembly.GetCallingAssembly().GetName().Name, config.dbFile).CacheType == CacheType.Redis && ServiceContext.Engine.Resolve<IRedisRepository>() == null)
                throw new System.Exception("ConfigureServices First add services.AddFastRedis(); Second add services.AddFastData()");

            if (config.ServiceLifetime == ServiceLifetime.Scoped)
                serviceCollection.AddScoped<IFastRepository, FastRepository>();

            if (config.ServiceLifetime == ServiceLifetime.Transient)
                serviceCollection.AddTransient<IFastRepository, FastRepository>();

            if (config.ServiceLifetime == ServiceLifetime.Singleton)
                serviceCollection.AddSingleton<IFastRepository, FastRepository>();

            Assembly.GetCallingAssembly().GetReferencedAssemblies().ToList().ForEach(a =>
            {
                if (!AppDomain.CurrentDomain.GetAssemblies().ToList().Exists(b => b.GetName().Name == a.Name))
                    try { Assembly.Load(a.Name); } catch (Exception ex) { }
            });

            if (config.aop != null)
                serviceCollection.AddSingleton<IFastAop>(config.aop);

            if (!string.IsNullOrEmpty(config.NamespaceService))
                FastMap.InstanceService(serviceCollection, config.NamespaceService);

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

            if (config.IsCodeFirst && !string.IsNullOrEmpty(config.NamespaceCodeFirst) && !config.IsResource)
            {
                FastMap.InstanceProperties(config.NamespaceCodeFirst, config.dbFile);
                FastMap.InstanceTable(config.NamespaceCodeFirst, config.dbKey, config.dbFile);
            }

            if (!string.IsNullOrEmpty(config.NamespaceProperties) && config.IsResource)
                FastMap.InstanceProperties(config.NamespaceProperties, config.dbFile, projectName);

            if (!string.IsNullOrEmpty(config.NamespaceProperties) && !config.IsResource)
                FastMap.InstanceProperties(config.NamespaceProperties, config.dbFile);

            return serviceCollection;
        }

        public static IServiceCollection AddFastDataFilter<T>(this IServiceCollection serviceCollection, Expression<Func<T, bool>> predicate, FilterType type)
        {
            if (config != null)
            {
                ConfigModel item;
                string projectName = null;
                if (config.IsResource)
                    projectName = Assembly.GetCallingAssembly().GetName().Name;

                item = DataConfig.Get(config.dbKey, projectName, config.dbFile);
                var query = new DataQuery();
                query.Table.Add(typeof(T).Name);
                query.Config = item;
                query.TableAsName.Add(typeof(T).Name, predicate.Parameters[0].Name);
                var model = VisitExpression.LambdaWhere<T>(predicate, query);

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

        public static IServiceCollection AddFastDataKey(this IServiceCollection serviceCollection, Action<ConfigKey> action)
        {
            var model = new ConfigKey();
            action(model);

            var key = $"FastData.Key.{typeof(ConfigKey).Name}";
            DbCache.Set<ConfigKey>(CacheType.Web, key, model);
            return serviceCollection;
        }

        public static IServiceCollection AddFastDataGeneric(this IServiceCollection serviceCollection, Action<ConfigRepository> repository)
        {
            var configRepository = new ConfigRepository();
            repository(configRepository);

            if (string.IsNullOrEmpty(configRepository.NameSpaceServie))
                return serviceCollection;

            if (string.IsNullOrEmpty(configRepository.NameSpaceModel))
                return serviceCollection;

            InitModelType(configRepository.NameSpaceModel).ForEach(m =>
            {
                var type = typeof(IFastRepository<>).MakeGenericType(new Type[1] { m });
                var obj = Activator.CreateInstance(type);

                if (configRepository.ServiceLifetime == ServiceLifetime.Scoped)
                    serviceCollection.AddScoped(type, s => { return obj; });

                if (configRepository.ServiceLifetime == ServiceLifetime.Transient)
                    serviceCollection.AddTransient(type, s => { return obj; });

                if (configRepository.ServiceLifetime == ServiceLifetime.Singleton)
                    serviceCollection.AddSingleton(type, s => { return obj; });
            });

            var aopType = configRepository.Aop != null ? configRepository.Aop.GetType() : null;
            serviceCollection.AddFastAopGeneric(configRepository.NameSpaceServie, configRepository.NameSpaceModel, aopType, configRepository.ServiceLifetime);
            serviceCollection.AddFastAop(configRepository.NameSpaceServie, aopType, configRepository.ServiceLifetime);

            ServiceContext.Init(new ServiceEngine(serviceCollection.BuildServiceProvider()));
            return serviceCollection;
        }

        private static List<Type> InitModelType(string nameSpaceModel)
        {
            var list = new List<Type>();
            if (string.IsNullOrEmpty(nameSpaceModel))
                return list;

            AppDomain.CurrentDomain.GetAssemblies().ToList().ForEach(assembly =>
            {
                if (assembly.IsDynamic)
                    return;

                assembly.ExportedTypes.Where(a => a.Namespace != null && a.Namespace.Contains(nameSpaceModel)).ToList().ForEach(b =>
                {
                    if (b.IsPublic && b.IsClass && !b.IsAbstract && !b.IsGenericType)
                        list.Add(b);
                });
            });

            return list;
        }
    }

    public class ConfigKey
    {
        public string dbKey { get; set; }
    }
}
