using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using org.apache.zookeeper;

namespace Mc.ZookeeperLock
{
    /// <summary>
    /// Zk客户端
    /// </summary>
    public class ZkClient
    {
         ZooKeeper _zk;
        /// <summary>
        /// 当前连接持有锁的数量
        /// </summary>
        int _lockCount;

        /// <summary>
        /// 是否可以获取锁
        /// </summary>
        public bool LockedAble => _lockedAble;

        /// <summary>
        /// 是否可以获取锁
        /// </summary>
         bool _lockedAble;

        readonly string _connectionString;
        readonly int _timeOut;
        /// <summary>
        /// 创建一个ZK连接
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="sessionTimeout">会话超时时间</param>
        public ZkClient(string connectionString, int sessionTimeout)
        {
            _lockedAble = true;
            _connectionString = connectionString;
            _timeOut = sessionTimeout;
            _zk=new ZooKeeper(connectionString,sessionTimeout,null);
        }
        /// <summary>
        /// 判断文档目录节点是否存在
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public async Task<bool> ExistAsync(string node)
        {
            var result= await _zk.existsAsync(node);
            return result != null;
        }
        /// <summary>
        /// 创建一个持久节点
        /// </summary>
        /// <param name="path"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<string> CreatePersistentNode(string path, string value)
        {
            return
                await
                    _zk.createAsync(path, Encoding.UTF8.GetBytes(value), ZooDefs.Ids.OPEN_ACL_UNSAFE,
                        CreateMode.PERSISTENT);
        }
        /// <summary>
        /// 创建一个临时节点
        /// </summary>
        /// <param name="path"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<string> CreateEphemeralNode(string path, string value)
        {
            return
                await
                    _zk.createAsync(path, Encoding.UTF8.GetBytes(value), ZooDefs.Ids.OPEN_ACL_UNSAFE,
                        CreateMode.EPHEMERAL);
        }
        /// <summary>
        /// 创建临时有序节点
        /// </summary>
        /// <param name="path"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<string> CreateEphemeralSequentialNode(string path, string value)
        {
            return
                await
                    _zk.createAsync(path, Encoding.UTF8.GetBytes(value), ZooDefs.Ids.OPEN_ACL_UNSAFE,
                        CreateMode.EPHEMERAL_SEQUENTIAL);
        }
        /// <summary>
        /// 创建持久有序节点
        /// </summary>
        /// <param name="path"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<string> CreatePersistentSequentialNode(string path, string value)
        {
            return
                await
                    _zk.createAsync(path, Encoding.UTF8.GetBytes(value), ZooDefs.Ids.OPEN_ACL_UNSAFE,
                        CreateMode.PERSISTENT_SEQUENTIAL);
        }
        /// <summary>
        /// 监听节点变化
        /// </summary>
        /// <param name="path"></param>
        /// <param name="watcher"></param>
        /// <returns></returns>
        public async Task<string> WatchNode(string path, Watcher watcher)
        {
            var result= await _zk.getDataAsync(path, watcher);
            if (result == null)
                return "";
            return Encoding.UTF8.GetString(result.Data);
        }
        /// <summary>
        /// 获取子节点
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<List<string>> GetChildren(string path)
        {
            var result = await _zk.getChildrenAsync(path);
            return result?.Children;
        }
        /// <summary>
        /// 获取节点内容
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<string> GetNode(string path)
        {
            var result = await _zk.getDataAsync(path);
            if (result == null)
                return "";
            return Encoding.UTF8.GetString(result.Data);
        }
        /// <summary>
        /// 删除节点
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task Remove(string path)
        {
            await _zk.deleteAsync(path);
        }
        /// <summary>
        /// 断点连接使session过期
        /// </summary>
        /// <returns></returns>
        public async Task Disconnect()
        {
            _lockedAble = false;
            if (_lockCount == 0)
            {
                await _zk.closeAsync();
                _zk=new ZooKeeper(_connectionString,_timeOut,null);
                _lockedAble = true;
            }
        }
        /// <summary>
        /// 增加持有锁计数
        /// </summary>
        public void IncremLock()
        {
            Interlocked.Increment(ref _lockCount);
        }
        /// <summary>
        /// 减少持有锁计数
        /// </summary>
        public void DecremLock()
        {
            Interlocked.Decrement(ref _lockCount);
        }

        public async Task ReConnect()
        {
            await _zk.closeAsync();
            _zk = new ZooKeeper(_connectionString, _timeOut, null);
        }

    }
  
}
