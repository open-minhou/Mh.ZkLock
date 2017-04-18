using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mc.ZookeeperLock;

namespace Mc.JobDispater.Abstruct
{
    public interface IJobProvider
    {
        IDistributeLockFactory GetLockFactory(string connSettingName, string systemId);
        IMessageQueue<T> GetQueue<T>(string conneSettingName, string queueName);
        ICacheClient GetCacheClient(string conn, int db);
    }
}
