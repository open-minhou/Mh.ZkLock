using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mc.JobDispater.Abstruct;
using StackExchange.Redis;

namespace Mc.JobDispater.Redis
{
    public class RedisClient:ICacheClient
    {
        readonly IDatabase _client;
        static readonly object LockObj = new object();
        static readonly ConcurrentDictionary<string, ConnectionMultiplexer> ConnectionFactory = new ConcurrentDictionary<string, ConnectionMultiplexer>();
        public RedisClient(string connection, int db)
        {
            _client = GetConnection(connection).GetDatabase(db);
        }

        ConnectionMultiplexer GetConnection(string connection)
        {
            ConnectionMultiplexer conn;
            if (ConnectionFactory.TryGetValue(connection, out conn))
                return conn;
            lock (LockObj)
            {
                var connStr = ConfigurationManager.AppSettings[connection];
                conn = ConnectionMultiplexer.ConnectAsync(connStr).Result;
                ConnectionFactory[connection] = conn;
            }
            return conn;
        }

        public Task<bool> SetIfNotExist(string key, string value)
        {
            return _client.StringSetAsync(key, value,null,When.NotExists);
        }

        public Task<bool> Remove(string key)
        {
            return _client.KeyDeleteAsync(key);
        }
    }
}
