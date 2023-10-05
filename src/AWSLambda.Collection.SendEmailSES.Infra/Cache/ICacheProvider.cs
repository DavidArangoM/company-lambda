using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambda.Collection.SendEmailSES.Infra.Cache
{
    /// <summary>
    /// Interface for Cache
    /// </summary>
    public interface ICacheProvider
    {
        /// <summary>
        /// Task get from cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<T> GetFromCache<T>(string key) where T : class;

        /// <summary>
        /// Task SetCache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        Task SetCache<T>(string key, T value, DistributedCacheEntryOptions options) where T : class;

        /// <summary>
        /// Clear Cache
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task ClearCache(string key);

    }
}
