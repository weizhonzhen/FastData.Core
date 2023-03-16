
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
        private DataContext _Context;

        public DataContext Context
        {
            set { _Context = value; }
            get
            {
                if (_Context != null && !_Context.isDispose)
                    return _Context;
                else
                {
                    _Context = new DataContext(DataConfig.List[0].Key);
                    return _Context;
                }
            }
        }

        private readonly ConcurrentDictionary<string, DataContext> list = new ConcurrentDictionary<string, DataContext>();

        public UnitOfWorK()
        {
            foreach (var item in DataConfig.List)
            {
                list.TryAdd(item.Key, new DataContext(item.Key));
            }
            _Context = list[DataConfig.List[0].Key];
        }

        public void Dispose()
        {
            foreach (var item in list)
            {
                if (!item.Value.isDispose)
                    _Context?.Dispose();
                item.Value?.Dispose();
            }
            list.Clear();
        }

        public DataContext Contexts(string key)
        {
            if (!DataConfig.List.Exists(a => a.Key == key) && DataConfig.List.Count > 1)
                throw new Exception($"不存在数据库Key:{key}");

            if (string.IsNullOrEmpty(key))
                key = DataConfig.List[0].Key;

            DataContext data;
            list.TryGetValue(key, out data);
            if (data == null || data?.isDispose == true)
            {
                list.TryRemove(key, out data);
                data = new DataContext(key);
                list.TryAdd(key, data);
            }
            return data;
        }
    }
}