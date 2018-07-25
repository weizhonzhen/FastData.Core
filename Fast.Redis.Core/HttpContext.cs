using System;

namespace Fast.Redis.Core.Base
{
    /// <summary>
    /// 启动注入services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>()
    /// </summary>
    internal static class HttpContext
    {
        public static IServiceProvider ServiceProvider;
        public static Microsoft.AspNetCore.Http.HttpContext Current
        { 
            get
            {
                object factory = ServiceProvider.GetService(typeof(Microsoft.AspNetCore.Http.IHttpContextAccessor));
                var context = ((Microsoft.AspNetCore.Http.HttpContextAccessor)factory).HttpContext;
                return context;
            }
        }
    }
}
