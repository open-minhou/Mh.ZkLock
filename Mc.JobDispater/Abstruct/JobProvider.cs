using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mc.JobDispater.Queue;
using Mc.JobDispater.Redis;
using Mc.ZookeeperLock;

namespace Mc.JobDispater.Abstruct
{
    public class JobProvider:IJobProvider
    {
        static readonly ConcurrentDictionary<string,IDistributeLockFactory> LockFactories=new ConcurrentDictionary<string, IDistributeLockFactory>(); 
      
        public virtual IDistributeLockFactory GetLockFactory(string connSettingName,string systemId)
        {
            IDistributeLockFactory factory;
            if (!LockFactories.TryGetValue(connSettingName, out factory))
            {
                var connstring = ConfigurationManager.AppSettings[connSettingName];
                factory = new ZkLockFactory(connstring, 10000,systemId);
                LockFactories[connSettingName] = factory;
            }
            return factory;
        }

        public virtual IMessageQueue<T> GetQueue<T>(string conneSettingName,string queueName)
        {
            return new RabbitQueue<T>(queueName,conneSettingName);
        }

        public ICacheClient GetCacheClient(string conn, int db)
        {
            return new RedisClient(conn,db);
        }
    }
}
