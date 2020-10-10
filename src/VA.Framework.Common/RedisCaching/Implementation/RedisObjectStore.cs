using StackExchange.Redis;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using VA.Framework.Common.RedisCaching.Definition;

namespace VA.Framework.Common.RedisCaching.Implementation
{
    public class RedisObjectStore<T> : IRedisObjectStore<T>
    {
        private readonly IDatabase _database;
        private readonly IConnectionMultiplexer _connection;
        public RedisObjectStore(IDatabase database, IConnectionMultiplexer connection)
        {
            _connection = connection;
            _database = database;
        }

        public async Task<T> Get(string key)
        {
            key = GenerateKey(key);
            var hash = await _database.HashGetAllAsync(key);

            return MapFromHash(hash);
        }

        public void Save(string key, T obj)
        {
            if (obj != null)
            {
                var hash = GenerateRedisHash(obj);
                key = GenerateKey(key);

                _database.HashSetAsync(key, hash);
            }
        }

        public void Save(string key, T obj, TimeSpan exp)
        {
            if (obj != null)
            {
                var hash = GenerateRedisHash(obj);
                key = GenerateKey(key);

                _database.HashSetAsync(key, hash);
                _database.KeyExpire(key, exp);
            }
        }

        public void Delete(string key)
        {
            if (string.IsNullOrWhiteSpace(key) || key.Contains(":"))
                throw new ArgumentException("invalid key");

            key = GenerateKey(key);
            _database.KeyDeleteAsync(key);
        }

        public bool IsConnected()
        {
            return _connection.IsConnected;
        }

        #region Helpers

        //generate a key from a given key and the class name of the object we are storing
        string GenerateKey(string key) =>
            string.Concat(key.ToLower(), ":", NameOfT.ToLower());

        //create a hash entry array from object using reflection
        HashEntry[] GenerateRedisHash(T obj)
        {
            var props = obj.GetType().GetProperties();
            var hash = new HashEntry[props.Count()];

            for (int i = 0; i < props.Count(); i++)
            {

                if (props[i].GetValue(obj) != null)
                    hash[i] = new HashEntry(props[i].Name, props[i].GetValue(obj).ToString());
            }
            return hash;
        }

        //build object from hash entry array using reflection
        T MapFromHash(HashEntry[] hash)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            var obj = (T)Activator.CreateInstance(TypeOfT);//new instance of T

            foreach (var property in properties)
            {
                HashEntry entry = hash.FirstOrDefault(g => g.Name.ToString().Equals(property.Name));
                if (entry.Equals(new HashEntry())) continue;
                property.SetValue(obj, Convert.ChangeType(entry.Value.ToString(), property.PropertyType));
            }



            return obj;
        }

        Type TypeOfT { get { return typeof(T); } }

        string NameOfT { get { return TypeOfT.FullName; } }

        #endregion

    }
}
