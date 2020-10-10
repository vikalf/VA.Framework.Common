using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using VA.Framework.Common.RedisCaching.Definition;

namespace VA.Framework.Common.RedisCaching.Implementation
{
    class RedisCacheConnectionPoolManager : IRedisCacheConnectionPoolManager
    {
        private const int POOL_SIZE = 10;
        private static ConcurrentBag<Lazy<ConnectionMultiplexer>> connections = new ConcurrentBag<Lazy<ConnectionMultiplexer>>();
        const string REDIS_CONNECTIONSTRING_TLS = "REDIS_CONNECTIONSTRING_TLS";
        const string REDIS_PASSWORD = "REDIS_PASSWORD";

        private readonly ILogger<RedisCacheConnectionPoolManager> _logger;

        public RedisCacheConnectionPoolManager(ILogger<RedisCacheConnectionPoolManager> logger)
        {
            _logger = logger;
            var configurationOptions = GetRedisConfigurationOptions();

            for (int i = 0; i < POOL_SIZE; i++)
            {
                var multiplexer = CreateMultiplexer(configurationOptions);
                connections.Add(multiplexer);
            }
        }
        public IConnectionMultiplexer GetConnection()
        {
            Lazy<ConnectionMultiplexer> response;
            var loadedLazys = connections.Where(lazy => lazy.IsValueCreated);
            if (loadedLazys.Count() == connections.Count)
            {
                var minValue = connections.Min(lazy => lazy.Value.GetCounters().TotalOutstanding);
                response = connections.First(lazy => lazy.Value.GetCounters().TotalOutstanding == minValue);
            }
            else
            {
                _logger.LogTrace("Creating a new connection to Redis");
                response = connections.First(lazy => !lazy.IsValueCreated);
            }
            return response.Value;
        }

        private Lazy<ConnectionMultiplexer> CreateMultiplexer(ConfigurationOptions configurationOptions)
        {
            return new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(configurationOptions));
        }

        private ConfigurationOptions GetRedisConfigurationOptions()
        {
            var redisHost = System.Environment.GetEnvironmentVariable(REDIS_CONNECTIONSTRING_TLS);

            if (redisHost == null)
                throw new KeyNotFoundException($"Environment variable for {REDIS_CONNECTIONSTRING_TLS} was not found.");

            var password = System.Environment.GetEnvironmentVariable(REDIS_PASSWORD);

            if (password == null)
                throw new KeyNotFoundException($"Environment variable for {REDIS_PASSWORD} was not found.");

            var configurationOptions = new ConfigurationOptions()
            {
                SyncTimeout = 1000,
                AsyncTimeout = 1000,
                ConnectTimeout = 15000,
                AbortOnConnectFail = false,
                AllowAdmin = false,
                Ssl = true,
                SslProtocols = System.Security.Authentication.SslProtocols.Tls12,
                Password = password,
            };

            configurationOptions.EndPoints.Add(redisHost);

            return configurationOptions;
        }
    }
}
