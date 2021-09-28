using System;
using System.Reflection;

namespace FastData.Core.Proxy
{
    public interface IProxyHandler
    {
        Object Invoke(Object proxy, MethodInfo method, Object[] args);
    }
}
