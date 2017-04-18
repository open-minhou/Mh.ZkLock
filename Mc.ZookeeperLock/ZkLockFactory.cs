using System;
using System.Threading;
using System.Threading.Tasks;
using org.apache.zookeeper;

namespace Mc.ZookeeperLock
{
    public interface IDistributeLockFactory
    {
        /// <summary>
        /// get the distrubtelock 
        /// </summary>
        /// <param name="lockId"></param>
        /// <returns></returns>
        Task<IDistributeLock> GetLock(string lockId);

        Task<IDistributeLock> GetWaitLock(string lockId);
        /// <summary>
        /// release the distrubtelock
        /// </summary>
        /// <param name="dislock"></param>
        /// <returns></returns>
        Task Release(IDistributeLock dislock);
    }

    /// <summary>
    /// zookeeper lock Factory
    /// </summary>
    public class ZkLockFactory : IDistributeLockFactory
    {

        readonly ZkClient _zooKeeper;
        static readonly ILog _log = LogManager.GetLog();
        // /// <summary>
        // /// 保存每个ID最后一个锁的值，以避免服务节点挂掉后，无法清除最后一个锁
        // /// </summary>
        //// static ConcurrentDictionary<string, string> LastLockValue = new ConcurrentDictionary<string, string>();

        readonly string _oneLockRoot;
        readonly string _waitLockRoot;
        readonly string _systemid;

        public ZkLockFactory(string connectionStr, int timeout, string systemName = "job")
        {
            _zooKeeper = new ZkClient(connectionStr, timeout);
            _oneLockRoot = $"{ZookeeperConst.OneLockRoot}/{systemName}";
            _waitLockRoot = $"{ZookeeperConst.WaitLockRoot}/{systemName}";
            _systemid = systemName;

        }



        /// <summary>
        /// 同一时间只有一个客户端取到锁，取不到的直接返回
        /// 需要判断lock的Locked属性
        /// </summary>
        /// <param name="lockId">lock key</param>
        /// <returns></returns>
        public async Task<IDistributeLock> GetLock(string lockId)
        {
            var _lock = new ZkLock(this);
            if (!_zooKeeper.LockedAble)
            {
                return _lock;
            }
            try
            {
                await CreateLockNodeIfNotExist(ZookeeperConst.OneLockRoot, _systemid);
                _lock.Locked = false;
                //判断是否已锁，如果已有，则表示本次未取到，直接返回
                var lockExist = await _zooKeeper.ExistAsync($"{_oneLockRoot}/{lockId}");
                if (lockExist)
                    return _lock;
                //尝试写入节点，如果写入成功，表示成功获取到锁
                _lock.LockValue = Guid.NewGuid().ToString("N");
                _lock.Id = await _zooKeeper.CreateEphemeralNode($"{_oneLockRoot}/{lockId}", _lock.LockValue);
                //LastLockValue[_lock.Id] = _lock.LockValue;
                _lock.Locked = true;
                _zooKeeper.IncremLock();
                return _lock;
            }
            catch (KeeperException.NoNodeException)
            {
                try
                {
                    await CreateLockNodeIfNotExist(ZookeeperConst.WaitLockRoot, _systemid);
                }
                catch (Exception)
                {
                    //
                }
                return _lock;
            }
            catch (Exception ex)
            {
                _log?.Error(ex);
                return _lock;
            }
        }
        /// <summary>
        /// 获取一个等待锁
        /// </summary>
        /// <param name="lockId"></param>
        /// <returns></returns>
        public async Task<IDistributeLock> GetWaitLock(string lockId)
        {
            var _lock = new ZkLock(this);
            if (!_zooKeeper.LockedAble)
                return _lock;
            try
            {
                _lock.Locked = false;

                //尝试写入节点，如果写入成功，表示成功获取到锁
                _lock.LockValue = Guid.NewGuid().ToString("N");
                _lock.Id =
                    await
                        _zooKeeper.CreateEphemeralSequentialNode($"{_waitLockRoot}/{lockId}", _lock.LockValue);
                var children =
                    await _zooKeeper.GetChildren($"{_waitLockRoot}");
                children.Sort();
                _lock.Locked = true;
                var splitIndex = _lock.Id.LastIndexOf('/') + 1;
                var index = children.IndexOf(_lock.Id.Substring(splitIndex, _lock.Id.Length - splitIndex));
                //LastLockValue[_lock.Id] = _lock.LockValue;
                if (index == 0)
                {
                    _zooKeeper.IncremLock();
                    return _lock;
                }

                SemaphoreSlim slim = new SemaphoreSlim(0);
                await _zooKeeper.WatchNode($"{_waitLockRoot}/{children[index - 1]}", new NodeWatcher(slim));
                await slim.WaitAsync();
                _zooKeeper.IncremLock();
                return _lock;
            }
            catch (KeeperException.ConnectionLossException)
            {
                return _lock;
            }
            catch (KeeperException.NoNodeException)
            {
                try
                {
                    await CreateLockNodeIfNotExist(ZookeeperConst.WaitLockRoot, _systemid);
                }
                catch (Exception)
                {
                    //
                }
                return _lock;
            }
            catch (Exception ex)
            {
                _log?.Error(ex);
                await Release(_lock);
                throw;
            }
        }


        public async Task Release(IDistributeLock dislock)
        {
            try
            {
                if (dislock.Locked)
                {
                    _zooKeeper.DecremLock();
                    await _zooKeeper.Remove($"{dislock.Id}");
                }
            }
            catch (Exception ex)
            {
                _log?.Error(ex);
                try
                {
                    await _zooKeeper.Disconnect();
                }
                catch (Exception ex1)
                {
                    _log?.Error(ex1);
                }
            }
        }

        async Task CreateLockNodeIfNotExist(string root, string systemid)
        {
            if (!await _zooKeeper.ExistAsync(root))
                await _zooKeeper.CreatePersistentNode(root, "lock root");
            if (!await _zooKeeper.ExistAsync($"{root}/{systemid}"))
                await _zooKeeper.CreatePersistentNode($"{root}/{systemid}", "lock dir");
        }


        class NodeWatcher : Watcher
        {
            readonly SemaphoreSlim _slim;
            public NodeWatcher(SemaphoreSlim slim)
            {
                _slim = slim;
            }

            public override async Task process(WatchedEvent e)
            {
                _slim.Release();
            }
        }
    }
}
