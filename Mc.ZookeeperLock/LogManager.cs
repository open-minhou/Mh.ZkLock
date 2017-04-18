using System.Collections.Generic;
using System.Linq;

namespace Mc.ZookeeperLock
{
    public static class LogManager
    {
        static List<ILog> _logs=new List<ILog>(); 
        public static ILog GetLog()
        {
            return _logs.FirstOrDefault();
        }

        public static void AddLogger(ILog log)
        {
            _logs.Add(log);
        }
    }
}
