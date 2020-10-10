using System;
using System.Threading.Tasks;

namespace VA.Framework.Common.RedisCaching.Definition
{
    public interface IRedisObjectStore<T>
    {
        /// <summary>
        /// Get an object stored in redis by key
        /// </summary>
        /// <param name="key">The key used to stroe object</param>
        Task<T> Get(string key);

        /// <summary>
        /// Save an object in redis
        /// </summary>
        /// <param name="key">The key to stroe object against</param>
        /// <param name="obj">The object to store</param>
        void Save(string key, T obj);

        /// <summary>
        /// Save and object in redis with expiration
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="exp"></param>
        void Save(string key, T obj, TimeSpan exp);

        /// <summary>
        /// Delete an object from redis using a key
        /// </summary>
        /// <param name="key">The key the object is stored using</param>
        void Delete(string key);

        /// <summary>
        /// Indicates whether any servers are connected
        /// </summary>
        /// <returns></returns>
        bool IsConnected();
    }
}
