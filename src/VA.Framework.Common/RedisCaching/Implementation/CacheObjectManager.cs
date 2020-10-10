using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using StackExchange.Redis.KeyspaceIsolation;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using VA.Framework.Common.RedisCaching.Definition;

namespace VA.Framework.Common.RedisCaching.Implementation
{
    public class CacheObjectManager : ICacheObjectManager
    {
        private readonly ILogger<CacheObjectManager> _logger;
        protected readonly IRedisCacheConnectionPoolManager _redisConnPool;
        private readonly BinarySerializer _binarySerializer;
        const int OBJECT_DATABASE = 1;
        private readonly string _keyPrefix;

        public CacheObjectManager(IRedisCacheConnectionPoolManager redisCacheConnectionPool, ILogger<CacheObjectManager> logger, string keyPrefix = "")
        {
            _redisConnPool = redisCacheConnectionPool;

            _logger = logger;
            _binarySerializer = new BinarySerializer();
            _keyPrefix = keyPrefix;
        }

        public T Get<T>(string key)
        {
            T result = default;

            try
            {
                var db = GetDatabase();
                var item = db.StringGet(key);
                if (!item.HasValue)
                    return default;

                result = _binarySerializer.Deserialize<T>(item);

            }
            catch (Exception exc)
            {

                if (exc is RedisConnectionException || exc is SocketException)
                {
                    _logger.LogWarning("Get<T>({key}) - Will Reconnect", key);
                }
                else if (exc is RedisTimeoutException)
                    _logger.LogError(exc, "Get<T>({key})", key);

            }
            return result;
        }

        public async Task<T> GetAsync<T>(string key)
        {
            T result = default;

            try
            {
                var db = GetDatabase();
                var item = await db.StringGetAsync(key);

                if (!item.HasValue)
                    return default;

                result = await _binarySerializer.DeserializeAsync<T>(item);

            }
            catch (Exception exc)
            {
                if (exc is RedisConnectionException || exc is SocketException)
                {
                    _logger.LogWarning("GetAsync<T>({key}) - Will Reconnect", key);
                }
                else if (exc is RedisTimeoutException)
                    _logger.LogError(exc, "GetAsync<T>({key})", key);
            }
            return result;
        }

        public async Task<bool> AddAsync<T>(string key, T value)
        {
            bool success = false;

            try
            {
                var db = GetDatabase();
                var entryBytes = await _binarySerializer.SerializeAsync(value);

                return await db.StringSetAsync(key, entryBytes, null, When.Always, CommandFlags.FireAndForget);
            }
            catch (Exception exc)
            {
                if (exc is RedisConnectionException || exc is SocketException)
                {
                    _logger.LogWarning("GetAsync<T>({key}) - Will Reconnect", key);
                }
                else if (exc is RedisTimeoutException || exc is RedisServerException)
                    _logger.LogError(exc, "AddAsync<T>({key})", key);

            }
            return success;
        }

        public async Task<bool> AddAsync<T>(string key, T value, DateTimeOffset expiresAt)
        {
            bool success = false;

            try
            {
                var entryBytes = await _binarySerializer.SerializeAsync(value);
                var db = GetDatabase();
                success = await db.StringSetAsync(key, entryBytes, expiresAt.Subtract(DateTimeOffset.Now), When.Always, CommandFlags.FireAndForget);
            }
            catch (Exception exc)
            {
                if (exc is RedisConnectionException || exc is SocketException)
                {
                    _logger.LogWarning("AddAsync<T>({key}, {expiresAt}) - Will Reconnect", key, expiresAt);
                }
                else if (exc is RedisTimeoutException || exc is RedisServerException)
                    _logger.LogError(exc, "AddAsync<T>({key})", key);
            }
            return success;
        }

        public bool Exists(string key)
        {
            bool success = false;

            try
            {
                var db = GetDatabase();
                success = db.KeyExists(key);
            }
            catch (Exception exc)
            {
                if (exc is RedisConnectionException || exc is SocketException)
                {
                    _logger.LogWarning("Exists({key}) - Will Reconnect", key);
                }
                else if (exc is RedisTimeoutException || exc is RedisServerException)
                    _logger.LogError(exc, "Exists({key})", key);
            }
            return success;
        }

        public async Task<bool> ExistsAsync(string key)
        {
            bool success = false;

            try
            {
                var db = GetDatabase();
                success = await db.KeyExistsAsync(key);
            }
            catch (Exception exc)
            {
                if (exc is RedisConnectionException || exc is SocketException)
                {
                    _logger.LogWarning("ExistsAsync({key}) - Will Reconnect", key);
                }
                else if (exc is RedisTimeoutException || exc is RedisServerException)
                    _logger.LogError(exc, "ExistsAsync({key})", key);
            }
            return success;
        }

        public async Task<bool> RemoveAsync(string key)
        {
            bool success = false;

            try
            {
                var db = GetDatabase();
                success = await db.KeyDeleteAsync(key);
            }
            catch (Exception exc)
            {
                if (exc is RedisConnectionException || exc is SocketException)
                {
                    _logger.LogWarning("RemoveAsync({key}) - Will Reconnect", key);
                }
                else if (exc is RedisTimeoutException || exc is RedisServerException)
                    _logger.LogError(exc, "RemoveAsync({key})", key);
            }
            return success;
        }

        public async Task<T> GetAsync<T>(string key, string keyPrefix)
        {
            T result = default;

            try
            {
                var db = GetDatabase(keyPrefix);
                var item = await db.StringGetAsync(key);

                if (!item.HasValue)
                    return default;

                result = await _binarySerializer.DeserializeAsync<T>(item);

            }
            catch (Exception exc)
            {
                if (exc is RedisConnectionException || exc is SocketException)
                {
                    _logger.LogWarning("GetAsync<T>({key}, {keyPrefix}) - Will Reconnect", key, keyPrefix);
                }
                else if (exc is RedisTimeoutException)
                    _logger.LogError(exc, "GetAsync<T>({key}, {keyPrefix})", key, keyPrefix);
            }
            return result;
        }

        public async Task<bool> AddAsync<T>(string key, T value, string keyPrefix)
        {
            bool success = false;

            try
            {
                var db = GetDatabase(keyPrefix);
                var entryBytes = await _binarySerializer.SerializeAsync(value);

                return await db.StringSetAsync(key, entryBytes, null, When.Always, CommandFlags.FireAndForget);
            }
            catch (Exception exc)
            {
                if (exc is RedisConnectionException || exc is SocketException)
                {
                    _logger.LogWarning("GetAsync<T>({key}, {keyPrefix}) - Will Reconnect", key, keyPrefix);
                }
                else if (exc is RedisTimeoutException || exc is RedisServerException)
                    _logger.LogError(exc, "AddAsync<T>({key}, {keyPrefix})", key, keyPrefix);

            }
            return success;
        }

        public async Task<bool> AddAsync<T>(string key, T value, DateTimeOffset expiresAt, string keyPrefix)
        {
            bool success = false;

            try
            {
                var entryBytes = await _binarySerializer.SerializeAsync(value);
                var db = GetDatabase(keyPrefix);
                success = await db.StringSetAsync(key, entryBytes, expiresAt.Subtract(DateTimeOffset.Now), When.Always, CommandFlags.FireAndForget);
            }
            catch (Exception exc)
            {
                if (exc is RedisConnectionException || exc is SocketException)
                {
                    _logger.LogWarning("AddAsync<T>({key}, {expiresAt}, {keyPrefix}) - Will Reconnect", key, expiresAt, keyPrefix);
                }
                else if (exc is RedisTimeoutException || exc is RedisServerException)
                    _logger.LogError(exc, "AddAsync<T>({key}, {expiresAt}, {keyPrefix})", key, expiresAt, keyPrefix);
            }
            return success;
        }

        public async Task<bool> RemoveAsync(string key, string keyPrefix)
        {
            bool success = false;

            try
            {
                var db = GetDatabase(keyPrefix);
                success = await db.KeyDeleteAsync(key);
            }
            catch (Exception exc)
            {
                if (exc is RedisConnectionException || exc is SocketException)
                {
                    _logger.LogWarning("RemoveAsync({key}, {keyPrefix}) - Will Reconnect", key, keyPrefix);
                }
                else if (exc is RedisTimeoutException || exc is RedisServerException)
                    _logger.LogError(exc, "RemoveAsync({key}, {keyPrefix})", key, keyPrefix);
            }
            return success;
        }

        #region "Helper Method"
        private IDatabase GetDatabase()
        {
            if (!string.IsNullOrWhiteSpace(_keyPrefix))
                return _redisConnPool.GetConnection().GetDatabase(OBJECT_DATABASE).WithKeyPrefix(_keyPrefix + ":");
            else
                return _redisConnPool.GetConnection().GetDatabase(OBJECT_DATABASE);
        }

        private IDatabase GetDatabase(string keyPrefix)
        {
            if (string.IsNullOrWhiteSpace(keyPrefix))
                throw new ArgumentException("keyPrefix is not specified");

            return _redisConnPool.GetConnection().GetDatabase(OBJECT_DATABASE).WithKeyPrefix(keyPrefix + ":");
        }

        #endregion
    }
}
