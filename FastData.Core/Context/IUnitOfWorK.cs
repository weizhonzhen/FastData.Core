
using FastData.Core.Base;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace FastData.Core.Context
{
    public interface IUnitOfWorK
    {
        DataContext Contexts(string key);

        DataContext Context { get; set; }
    }

    internal class UnitOfWorK : IUnitOfWorK, IDisposable
    {
        public DataContext Context { get; set; }
        private readonly ConcurrentDictionary<string, DataContext> list = new ConcurrentDictionary<string, DataContext>();

        public UnitOfWorK()
        {
            if (DataConfig.List.Count == 1)
                Context = new DataContext(DataConfig.List[0].Key);
            else
                foreach (var item in DataConfig.List)
                {
                    list.TryAdd(item.Key, new DataContext(item.Key));
                }
        }

        public void Dispose()
        {
            Context?.Dispose();
            foreach (var item in list)
            {
                item.Value?.Dispose();
            }
        }

        public DataContext Contexts(string key)
        {
            if (!DataConfig.List.Exists(a => a.Key == key))
                throw new Exception($"不存在数据库Key:{key}");

            DataContext data;
            list.TryGetValue(key, out data);
            if (data == null)
            {
                list.TryRemove(key, out data);
                data = new DataContext(key);
                list.TryAdd(key, data);
            }
            return data;
        }
    }
}