using FastData.Core.Repository;
using FastUntility.Core;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FastDataExtension
    {
        public static IServiceCollection AddFastData(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IFastRepository, FastRepository>();
            ServiceContext.Init(new ServiceEngine(serviceCollection.BuildServiceProvider()));
            return serviceCollection;
        }
    }
}
