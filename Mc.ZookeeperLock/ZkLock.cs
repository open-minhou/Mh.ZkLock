using System;
using System.Threading.Tasks;

namespace Mc.ZookeeperLock
{
    /// <summary>
    /// 分布式锁接口定义
    /// </summary>
    public interface IDistributeLock : IDisposable
    {
        /// <summary>
        /// lock's id 
        /// </summary>
        string Id { get; set; }
        /// <summary>
        /// the lock's value
        /// </summary>
        string LockValue { get; set; }
        /// <summary>
        /// 是否获得锁
        /// </summary>
        bool Locked { get; set; }
        /// <summary>
        /// 释放锁
        /// </summary>
        Task Release();
    }

    /// <summary>
    /// zookeeper分布式锁
    /// </summary>
    public class ZkLock : IDistributeLock
    {
        readonly IDistributeLockFactory _factory;

        public ZkLock(IDistributeLockFactory factory, bool locked = false)
        {
            _factory = factory;
            Locked = locked;
        }

        public async void Dispose()
        {
            await Release();
        }
        public string LockValue { get; set; }
        public string Id { get; set; }
        public bool Locked { get; set; }
        public async Task Release()
        {
            await _factory.Release(this);
        }
    }
}
