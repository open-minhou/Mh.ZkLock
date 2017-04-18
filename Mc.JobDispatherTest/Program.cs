using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mc.JobDispater;
using Mc.JobDispater.Abstruct;
using Mc.ZookeeperLock;

namespace Mc.JobDispatherTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var setting=new JobSetting();
            setting.QueueName = "queue1";
            setting.QueueConnectionString = "JobCenter";
            setting.SystemId = "JobTest";
            setting.ZkConnectionString = "zkconn";
            setting.RedisConnection = "RedisConn";
            setting.RedisDb = 1;
            var engine=new JobEngine<TestTask1>(setting,new Test1Job(),log:new Log());
            var zkconn = ConfigurationManager.AppSettings["zkconn"];
            SlaveUtil.Start(zkconn,2,"JobTest",new Log());
            //setting.QueueName = "queue2";
            //var engine2= new JobEngine<TestTask1>(setting, new Test1Job());
            do
            {
                engine.Execute().Wait();
                //engine2.Execute();
                Task.Delay(5000).Wait();
            } while (true);
        }
    }

    class Log : ILog
    {
        public void Info(string msg)
        {
            Console.WriteLine(msg);
        }

        public void Error(string msg)
        {
            Console.WriteLine(msg);
        }

        public void Error(Exception ex)
        {
            Console.WriteLine(ex);
        }

        public void Error(Exception ex, string msg)
        {
            Console.WriteLine($"{msg}:{ex}");
        }
    }
}
