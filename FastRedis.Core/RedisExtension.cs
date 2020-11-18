using System;
using FastRedis.Core;
using FastRedis.Core.Repository;
using FastUntility.Core;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RedisExtension
    {
        public static IServiceCollection AddFastRedis(this IServiceCollection serviceCollection, Action<ConfigModel> optionsAction = null)
        {
            serviceCollection.Configure(optionsAction);
            serviceCollection.AddSingleton<IRedisRepository, RedisRepository>();
            ServiceContext.Init(new ServiceEngine(serviceCollection.BuildServiceProvider()));
            return serviceCollection;
        }
    }
}
