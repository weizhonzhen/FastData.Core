using System;
using Microsoft.Extensions.DependencyInjection;

namespace FastUntility.Core
{
    public class ServiceEngine : IServiceEngine
    {
        private IServiceProvider serviceProvider;
        public ServiceEngine(IServiceProvider _serviceProvider)
        {
            this.serviceProvider = _serviceProvider;
        }

        public T Resolve<T>()
        {
            return serviceProvider.GetService<T>();
        }
    }

    public interface IServiceEngine
    {
        T Resolve<T>();
    }

    public class ServiceContext
    {
        private static IServiceEngine engine;
        public static IServiceEngine Init(IServiceEngine _engine)
        {
            if (engine == null)
                engine = _engine;
            return engine;
        }
        
        public static IServiceEngine Engine
        {
            get
            {
                return engine;
            }
        }
    }
}
