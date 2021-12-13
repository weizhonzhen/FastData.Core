using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace FastRedis.Core.Repository
{
    public interface IRedisRepository
    {
        bool Exists(string key, int db = 0);


        Task<bool> ExistsAsy(string key, int db = 0);

        bool Set<T>(string key, T model, int hours = 24 * 30 * 12, int db = 0);

        bool Set<T>(string key,T model, TimeSpan timeSpan, int db = 0);


        Task<bool> SetAsy<T>(string key, T model, int hours = 24 * 30 * 12, int db = 0);

        Task<bool> SetAsy<T>(string key, T model, TimeSpan timeSpan, int db = 0);


        bool Set(string key, string model, int hours = 24 * 30 * 12, int db = 0);

        bool Set(string key, string model, TimeSpan timeSpan, int db = 0);


        Task<bool> SetAsy(string key, string model, int hours = 24 * 30 * 12, int db = 0);

        Task<bool> SetAsy(string key, string model, TimeSpan timeSpan, int db = 0);


        string Get(string key, int db = 0);

        Task<string> GetAsy(string key, int db = 0);

        T Get<T>(string key, int db = 0) where T : class, new();

        Task<T> GetAsy<T>(string key, int db = 0) where T : class, new();

        bool Remove(string key, int db = 0);

        Task<bool> RemoveAsy(string key, int db = 0);

        RedisResult Execute(string command, int db = 0, params object[] args);

        Task<RedisResult> ExecuteAsy(string command, int db = 0, params object[] args);
    }
}
