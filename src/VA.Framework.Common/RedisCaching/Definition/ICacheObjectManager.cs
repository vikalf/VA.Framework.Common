using System;
using System.Threading.Tasks;

namespace VA.Framework.Common.RedisCaching.Definition
{
    public interface ICacheObjectManager
    {
        /// <summary>
        /// Get an item from cache synchronously
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<T> GetAsync<T>(string key);
        /// <summary>
        /// Add an item to cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key of the item</param>
        /// <param name="value">The value of the Item</param>
        /// <returns></returns>
        Task<bool> AddAsync<T>(string key, T value);
        /// <summary>
        ///  Add item to cache asynchronously with expiration at expiresAT
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiresAt"></param>
        /// <returns></returns>
        Task<bool> AddAsync<T>(string key, T value, DateTimeOffset expiresAt);
        /// <summary>
        /// Remove Item from Cache
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<bool> RemoveAsync(string key);

        /// <summary>
        /// Get an item from cache synchronously
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="keyPrefix">the keyPrefix</param>
        /// <returns></returns>
        Task<T> GetAsync<T>(string key, string keyPrefix);
        /// <summary>
        /// Add an item to cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key of the item</param>
        /// <param name="value">The value of the Item</param>
        /// <param name="keyPrefix">the keyPrefix</param>
        /// <returns></returns>
        Task<bool> AddAsync<T>(string key, T value, string keyPrefix);
        /// <summary>
        ///  Add item to cache asynchronously with expiration at expiresAT
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiresAt"></param>
        /// <param name="keyPrefix"></param>
        /// <returns></returns>
        Task<bool> AddAsync<T>(string key, T value, DateTimeOffset expiresAt, string keyPrefix);
        /// <summary>
        /// Remove Item from Cache
        /// </summary>
        /// <param name="key"></param>
        /// <param name="keyPrefix"></param>
        /// <returns></returns>
        Task<bool> RemoveAsync(string key, string keyPrefix);

    }
}
