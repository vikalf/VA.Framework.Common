using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading;
using VA.Framework.Common.RedisCaching.Definition;

namespace VA.Framework.Common.RedisCaching.Implementation
{
    public class RedisConnectionFactory : IRedisConnectionFactory
    {

        private long lastReconnectTicks = DateTimeOffset.MinValue.UtcTicks;
        private DateTimeOffset firstError = DateTimeOffset.MinValue;
        private DateTimeOffset previousError = DateTimeOffset.MinValue;
        private Lazy<ConnectionMultiplexer> multiplexer;
        private readonly ILogger<RedisConnectionFactory> _logger;

        private readonly object reconnectLock = new object();

        // In general, let StackExchange.Redis handle most reconnects, 
        // so limit the frequency of how often this will actually reconnect.
        private static readonly TimeSpan ReconnectMinFrequency = TimeSpan.FromSeconds(60);

        // if errors continue for longer than the below threshold, then the 
        // multiplexer seems to not be reconnecting, so re-create the multiplexer
        private static readonly TimeSpan ReconnectErrorThreshold = TimeSpan.FromSeconds(30);

        public RedisConnectionFactory(ILogger<RedisConnectionFactory> logger)
        {
            _logger = logger;
            multiplexer = CreateMultiplexer();
        }

        public ConnectionMultiplexer Connection()
        {
            return multiplexer.Value;
        }

        /// <summary>
        /// Force a new ConnectionMultiplexer to be created.  
        /// NOTES: 
        ///     1. Users of the ConnectionMultiplexer MUST handle ObjectDisposedExceptions, which can now happen as a result of calling ForceReconnect()
        ///     2. Don't call ForceReconnect for Timeouts, just for RedisConnectionExceptions or SocketExceptions
        ///     3. Call this method every time you see a connection exception, the code will wait to reconnect:
        ///         a. for at least the "ReconnectErrorThreshold" time of repeated errors before actually reconnecting
        ///         b. not reconnect more frequently than configured in "ReconnectMinFrequency"

        /// </summary>    
        public void ForceReconnect()
        {
            var utcNow = DateTimeOffset.UtcNow;
            var previousTicks = Interlocked.Read(ref lastReconnectTicks);
            var previousReconnect = new DateTimeOffset(previousTicks, TimeSpan.Zero);
            var elapsedSinceLastReconnect = utcNow - previousReconnect;

            // If mulitple threads call ForceReconnect at the same time, we only want to honor one of them.
            if (elapsedSinceLastReconnect > ReconnectMinFrequency)
            {
                lock (reconnectLock)
                {
                    utcNow = DateTimeOffset.UtcNow;
                    elapsedSinceLastReconnect = utcNow - previousReconnect;

                    if (firstError == DateTimeOffset.MinValue)
                    {
                        // We haven't seen an error since last reconnect, so set initial values.
                        firstError = utcNow;
                        previousError = utcNow;
                        return;
                    }

                    if (elapsedSinceLastReconnect < ReconnectMinFrequency)
                        return; // Some other thread made it through the check and the lock, so nothing to do.

                    var elapsedSinceFirstError = utcNow - firstError;
                    var elapsedSinceMostRecentError = utcNow - previousError;

                    var shouldReconnect =
                        elapsedSinceFirstError >= ReconnectErrorThreshold   // make sure we gave the multiplexer enough time to reconnect on its own if it can
                        && elapsedSinceMostRecentError <= ReconnectErrorThreshold; //make sure we aren't working on stale data (e.g. if there was a gap in errors, don't reconnect yet).

                    // Update the previousError timestamp to be now (e.g. this reconnect request)
                    previousError = utcNow;

                    if (shouldReconnect)
                    {
                        firstError = DateTimeOffset.MinValue;
                        previousError = DateTimeOffset.MinValue;

                        var oldMultiplexer = multiplexer;
                        CloseMultiplexer(oldMultiplexer);
                        multiplexer = CreateMultiplexer();
                        Interlocked.Exchange(ref lastReconnectTicks, utcNow.UtcTicks);
                    }
                }
            }
        }

        private Lazy<ConnectionMultiplexer> CreateMultiplexer()
        {
            var connectionString = System.Environment.GetEnvironmentVariable("REDIS_CONNECTIONSTRING");

            if (connectionString == null)
                throw new KeyNotFoundException($"Environment variable REDIS_CONNECTIONSTRING was not found.");

            var pwd = System.Environment.GetEnvironmentVariable("REDIS_PASSWORD");

            if (pwd == null)
                throw new KeyNotFoundException($"Environment variable REDIS_PASSWORD was not found.");

            var configurationOptions = new ConfigurationOptions()
            {
                SyncTimeout = 1000,
                AsyncTimeout = 1000,
                ConnectTimeout = 10000,
                AbortOnConnectFail = false,
                AllowAdmin = false,
                Ssl = false,
                Password = pwd,
            };

            configurationOptions.EndPoints.Add(connectionString);

            return new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(configurationOptions));
        }

        private void CloseMultiplexer(Lazy<ConnectionMultiplexer> oldMultiplexer)
        {
            if (oldMultiplexer != null)
            {
                try
                {
                    oldMultiplexer.Value.Close();
                }
                catch (Exception exc)
                {
                    _logger.LogError(exc, exc.Message);
                    // Example error condition: if accessing old.Value causes a connection attempt and that fails.
                }
            }
        }

    }
}
