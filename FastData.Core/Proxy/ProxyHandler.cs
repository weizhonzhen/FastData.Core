using FastData.Core.Base;
using FastData.Core.CacheModel;
using FastData.Core.Context;
using FastUntility.Core.Base;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Linq;
using FastData.Core.Property;
using FastData.Core.Model;

namespace FastData.Core.Proxy
{
    internal class ProxyHandler : IProxyHandler
    {
        public object Invoke(object proxy, MethodInfo method, object[] args)
        {
            var param = new List<DbParameter>();
            var config = DataConfig.Get();
            var key = string.Format("{0}.{1}", method.DeclaringType.Name, method.Name);

            if (DbCache.Exists(config.CacheType, key))
            {
                var model = DbCache.Get<ServiceModel>(config.CacheType, key);
                config = DataConfig.Get(model.dbKey);

                if (model.isSysType)
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        var temp = DbProviderFactories.GetFactory(config).CreateParameter();
                        temp.ParameterName = model.param.GetValue(i.ToString()).ToStr();
                        temp.Value = args[i];
                        param.Add(temp);
                    }
                }
                else
                {
                    var dyn = new DynamicGet(Activator.CreateInstance(method.GetParameters()[0].ParameterType));
                    for (int i = 0; i < model.param.Count; i++)
                    {
                        var temp = DbProviderFactories.GetFactory(config).CreateParameter();
                        temp.ParameterName = model.param.GetValue(i.ToString()).ToStr();
                        temp.Value = dyn.GetValue(args[0], temp.ParameterName,true);
                        temp.ParameterName = temp.ParameterName.ToLower();
                        param.Add(temp);
                    }
                }

                using (var db = new DataContext(config.Key))
                {
                    if (model.isWrite)
                        return db.ExecuteSql(model.sql, param.ToArray(), Aop.AopType.FaseWrite).writeReturn;
                    else                    
                        return db.FastReadAttribute(model, param); 
                }
            }

            throw new Exception($"error: service {method.DeclaringType.Name} , method {method.Name} not exists");
        }
    }
}