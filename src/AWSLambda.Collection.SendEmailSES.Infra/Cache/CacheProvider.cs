using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AWSLambda.Collection.SendEmailSES.Infra.Cache
{
    /// <summary>
    /// Cache Provider
    /// </summary>
    public class CacheProvider : ICacheProvider
    {
        private readonly IDistributedCache _cache;

        /// <summary>
        /// Ctor for cache
        /// </summary>
        /// <param name="cache"></param>
        public CacheProvider(IDistributedCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Get from cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<T> GetFromCache<T>(string key) where T : class
        {
            var result = await _cache.GetStringAsync(key);
            return result == null ? null : JsonSerializer.Deserialize<T>(result);
        }

        /// <summary>
        /// Set cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task SetCache<T>(string key, T value, DistributedCacheEntryOptions options) where T : class
        {
            var users = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, users, options);
        }

        /// <summary>
        /// Remove from cache with key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task ClearCache(string key)
        {
            await _cache.RemoveAsync(key);
        }
    }
}
