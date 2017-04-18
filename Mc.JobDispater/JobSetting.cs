using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Mc.JobDispater
{
   public class JobSetting
    {
       public string ZkConnectionString { get; set; }
       public string QueueConnectionString { get; set; }
       public string QueueName { get; set; }
       public string SystemId { get; set; }
       public string RedisConnection { get; set; }
       public int RedisDb { get; set; }
    }
}
