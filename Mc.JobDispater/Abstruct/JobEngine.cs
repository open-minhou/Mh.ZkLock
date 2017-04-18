using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mc.ZookeeperLock;

namespace Mc.JobDispater.Abstruct
{
    public class JobEngine<T>
    {
        static IDistributeLockFactory _lockFactory;
        static IMessageQueue<T> _queue;
        static IMcJobExcute<T> _worker; 
        static ILog _log;
        static bool _isInitlize;
        static  int _currentTaskCount;
        readonly ICacheClient _cacheClient;
         static bool IsRunning => _currentTaskCount > 0;

        public JobEngine(JobSetting setting, IMcJobExcute<T> worker, IJobProvider provider=null, ILog log = null)
        {
            lock (GetType())
            {
                if (!_isInitlize)
                {
                    provider = provider ?? new JobProvider();
                    _lockFactory = provider.GetLockFactory(setting.ZkConnectionString,setting.SystemId);
                    _log = log;
                    _queue = provider.GetQueue<T>(setting.QueueConnectionString, setting.QueueName);
                    _worker = worker;
                    _cacheClient = provider.GetCacheClient(setting.RedisConnection, setting.RedisDb);
                    Task.Factory.StartNew(() =>
                    {
                        _queue.OnReceive(ExcuteTask);
                    }, TaskCreationOptions.LongRunning).ConfigureAwait(false);
                    _isInitlize = true;
                }
            }
        }
        /// <summary>
        /// 执行
        /// </summary>
        /// <returns></returns>
        public async Task Execute()
        {
            _log?.Info("开始执行。。。。");
            if (IsRunning)
                return;
            //using (var _lock = await _lockFactory.GetLock(typeof(T).Name))
            //{
            //    if (!_lock.Locked)
            //    {
            //        _log?.Info("get no lock");
            //        return;
            //    }
            //    _log?.Info("get lock");
            //    var list = await _worker.GetWattingTasks();
            //    if (list.Count > 0)
            //    {
            //        foreach (var task in list)
            //        {
            //            _log?.Info($"dispather task type:{typeof(T).Name},task:{task.ToString()}");
            //            _queue.Enqueue(task);
            //        }
            //    }
            //}
            if (!SlaveUtil.IsMaster)
            {
                _log?.Info("not master");
                return;
            }
            _log?.Info("master");
            var list = await _worker.GetWattingTasks();
            if (list.Count > 0)
            {
                foreach (var task in list)
                {
                    _log?.Info($"dispather task type:{typeof(T).Name},task:{task.ToString()}");
                    try
                    {
                        if (!await _cacheClient.SetIfNotExist(GetCacheKey(task), "dispatcher"))
                            continue;
                        _queue.Enqueue(task);
                    }
                    catch (Exception ex)
                    {
                        _log?.Error(ex);
                    }
                    
                }
            }
        }
        /// <summary>
        /// 对任务执行操作
        /// </summary>
        /// <param name="task"></param>
         bool ExcuteTask(T task)
        {
            bool result=false;
            Interlocked.Increment(ref _currentTaskCount);
            try
            {
                result = _worker.ExcuteTask(task).Result;
                _cacheClient.Remove(GetCacheKey(task));
                _log?.Info($"task excuted,type:{typeof(T).Name},task:{task}");
            }
            catch (Exception ex)
            {
                _log?.Error(ex);
            }
            finally
            {
                Interlocked.Decrement(ref _currentTaskCount);
            }
            return result;
        }

        string GetCacheKey(T task)
        {
            var name = task.GetType().Name;
            var id = task.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(a => a.Name.ToUpper() == "ID")?.GetValue(task);
            return $"{name}_{id}";
        }
    }
}
