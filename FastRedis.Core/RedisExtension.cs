using System;
using FastRedis.Core;
using FastRedis.Core.Repository;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RedisExtension
    {
        public static IServiceCollection AddFastRedis(this IServiceCollection serviceCollection, Action<ConfigModel> optionsAction = null)
        {
            serviceCollection.Configure(optionsAction);
            serviceCollection.AddSingleton<IRedisRepository, RedisRepository>();
            return serviceCollection;
        }
    }
}
