using System;

namespace Mc.ZookeeperLock
{
    public interface ILog
    {
        void Info(string msg);
        void Error(string msg);
        void Error(Exception ex);
        void Error(Exception ex, string msg);
    }
}
