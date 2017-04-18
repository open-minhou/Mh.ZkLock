using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using org.apache.zookeeper;

namespace Mc.ZookeeperLock
{
    public static class SlaveUtil
    {
        /// <summary>
        /// 本进程标识，需保证唯一性
        /// </summary>
        static readonly string _hostIdentity;
        /// <summary>
        /// Zk连接
        /// </summary>
        static ZkClient _client;
        /// <summary>
        /// 当前进程是否是主节点
        /// </summary>
        public static bool IsMaster { get; private set; }
        /// <summary>
        /// 是否已开启过
        /// </summary>
        static bool _isRunning;
        /// <summary>
        /// 锁对象
        /// </summary>
        static object _lockObj=new object();
        /// <summary>
        /// 本进程的zk里的key(path),同一服务多节点需保持一致
        /// </summary>
        static string _zkKey;
        /// <summary>
        /// 日志
        /// </summary>
        static ILog _log;
        static SlaveUtil()
        {
            //计算机名
            var hostName= Dns.GetHostName();
            //进程ID
            var pid = Process.GetCurrentProcess().Id;
            var time = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            _hostIdentity = $"{hostName}_{pid}_{time}";
        }

        static CancellationTokenSource _cancellation;
        /// <summary>
        /// 开始启动一个线程，轮询Zookeeper,争主节点
        /// </summary>
        /// <param name="zkConnection">zookeeper连接字符串</param>
        /// <param name="interval">轮询间隔时间(秒)</param>
        /// <param name="systemId">本进程的系统ID，多个节点必须保持一致，不能与其它JOB重复</param>
        /// <param name="log">日志实例</param>
        public static void Start(string zkConnection,int interval,string systemId,ILog log=null)
        {
            //如果多次调用直接返回，保证一个进程只能存在一个
            if (_isRunning)
                return;
            lock (_lockObj)
            {
                if (_isRunning)
                    return;
                _client=new ZkClient(zkConnection,10000);
                _isRunning = true;
                _zkKey = $"{ZookeeperConst.MasterRoot}/{systemId}";
                _log = log;
                _cancellation=new CancellationTokenSource();
                Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        await CompeteMaster(interval);
                    }
                    catch (Exception ex)
                    {
                        _log?.Error(ex);
                    }
                },TaskCreationOptions.LongRunning);
            }
        }
        /// <summary>
        /// 停止线程执行
        /// </summary>
        public static void Stop()
        {
            _isRunning = false;
            _cancellation.Cancel();
        }
        /// <summary>
        /// 竞争主节点
        /// </summary>
        /// <param name="interval">轮询间隔</param>
        /// <returns></returns>
        static async Task CompeteMaster(int interval)
        {
            while (!_cancellation.IsCancellationRequested)
            {
                //等待指定时间后再次执行
                await Task.Delay(TimeSpan.FromSeconds(interval)).ConfigureAwait(false);

                try
                {
                    if (!await _client.ExistAsync(ZookeeperConst.MasterRoot))
                        await _client.CreatePersistentNode(ZookeeperConst.MasterRoot,"masterlock");
                    //如果节点已存在，取值，看是否是本进程写入的
                    if (await _client.ExistAsync(_zkKey))
                    {
                        var value = await _client.GetNode(_zkKey);
                        IsMaster = value == _hostIdentity;
                    }
                    else
                    {
                        //尝试创建节点，如果成功则作为主节点，失败则会抛出异常
                        await _client.CreateEphemeralNode(_zkKey, _hostIdentity);
                        IsMaster = true;
                    }
                }
                catch (KeeperException.ConnectionLossException)
                {
                    //连接丢失时重连
                    IsMaster = false;
                    await _client.ReConnect();
                }
                catch (KeeperException.SessionExpiredException)
                {
                    //会话超时时重连
                    IsMaster = false;
                    await _client.ReConnect();
                }
                catch (KeeperException.NodeExistsException)
                {
                    //igore 节点已存在时不做处理
                    IsMaster = false;
                }
                catch (Exception ex)
                {
                    //其它异常情况记录日志后等待下次执行
                    IsMaster = false;
                    _log?.Error(ex);
                }
            }
        }
    }
}
