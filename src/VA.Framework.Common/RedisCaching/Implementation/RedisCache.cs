using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VA.Framework.Common.RedisCaching.Implementation
{
    public static class RedisCache<T> where T : BaseCache
    {

        private static string ConnectionString;

        private static int DB;
        private static ILogger logger;

        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            // automatically disposed when the AppDomain is torn down
            var connection = ConnectionMultiplexer.Connect(ConnectionString);

            return connection;
        });

        private static Lazy<IDatabase> lazyCache = new Lazy<IDatabase>(() =>
        {
            return Connection.GetDatabase(db: DB);
        });

        private static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }

        private static IDatabase Cache
        {
            get
            {
                return lazyCache.Value;
            }
        }

        public static void Configure(int db, string connectionString, ILoggerFactory loggerFactory)
        {
            DB = db;
            ConnectionString = connectionString;
            logger = loggerFactory.CreateLogger(nameof(RedisCache<T>));
        }

        public static async Task<T> GetItemAsync(string id)
        {
            using (logger.BeginScope(nameof(GetItemAsync)))
            {
                logger.LogDebug("GetItemAsync({id})", id);

                logger.LogDebug("Start: reading value from Redis");
                var item = await Cache.StringGetAsync(id);
                logger.LogDebug("End: reading value from Redis");

                if (!item.HasValue)
                    return default;


                return await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<T>(item)).ConfigureAwait(continueOnCapturedContext: false);
            }
        }

        public static async Task<IEnumerable<T>> GetItemsAsync(IEnumerable<string> ids)
        {
            using (logger.BeginScope(nameof(GetItemsAsync)))
            {
                logger.LogDebug("GetItemAsync({ids})", ids);

                if (!ids.Any())
                    return default;

                IEnumerable<Task<T>> allTasks = ids.Select(id => GetItemAsync(id));
                // async + in parallel (non-blocking)
                IEnumerable<T> allResults = await Task.WhenAll(allTasks).ConfigureAwait(continueOnCapturedContext: false);

                return allResults.Where(s => s != null);
            }
        }

        public static async Task<bool> CreateItemAsync(T item)
        {
            using (logger.BeginScope(nameof(CreateItemAsync)))
            {
                logger.LogDebug("CreateItemAsync({item})", item);
                logger.LogDebug("Start: storing item in Redis");
                string jsonItem = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(item));
                await Cache.StringSetAsync(item.Key, jsonItem);
                var success = await Cache.KeyExpireAsync(item.Key, DateTime.MaxValue.ToUniversalTime()).ConfigureAwait(continueOnCapturedContext: false);
                logger.LogDebug("End: storing item in Redis");

                return success;
            }
        }

        public static async Task<bool> UpdateItemAsync(string id, T item)
        {
            using (logger.BeginScope(nameof(UpdateItemAsync)))
            {
                logger.LogDebug("UpdateItemAsync({id})", id);

                logger.LogDebug("Start: updating item in Redis");
                string jsonItem = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(item));
                var success = await Cache.StringSetAsync(id, jsonItem).ConfigureAwait(continueOnCapturedContext: false);
                logger.LogDebug("End: updating item in Redis");

                return success;
            }
        }

        public static async Task<bool> DeleteItemAsync(string id)
        {
            using (logger.BeginScope(nameof(DeleteItemAsync)))
            {
                logger.LogDebug("DeleteItemAsync({id})", id);

                logger.LogDebug("Start: updating item's TTL in Redis");
                var success = await Cache.KeyExpireAsync(id, DateTime.UtcNow.AddDays(7)).ConfigureAwait(continueOnCapturedContext: false);
                logger.LogDebug("End: updating item's TTL in Redis");

                return success;
            }
        }
    }
}
