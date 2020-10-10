using StackExchange.Redis;

namespace VA.Framework.Common.RedisCaching.Definition
{
    public interface IRedisCacheConnectionPoolManager
    {
        IConnectionMultiplexer GetConnection();
    }
}
