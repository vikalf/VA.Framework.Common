using StackExchange.Redis;

namespace VA.Framework.Common.RedisCaching.Definition
{
    public interface IRedisConnectionFactory
    {
        ConnectionMultiplexer Connection();
        void ForceReconnect();
    }
}
