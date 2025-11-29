using System;
using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;
using SupportAgent.Application.Interfaces;

namespace SupportAgent.Infrastructure.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _redis;

        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var db = _redis.GetDatabase();
            var value = await db.StringGetAsync(key);

            if (value.IsNullOrEmpty)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(value.ToString());
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            var db = _redis.GetDatabase();
            var json = JsonSerializer.Serialize(value);
            if (expiration.HasValue)
            {
                await db.StringSetAsync(key, json, expiration.Value);
            }
            else
            {
                await db.StringSetAsync(key, json);
            }
        }

        public async Task RemoveAsync(string key)
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(key);
        }
    }
}
