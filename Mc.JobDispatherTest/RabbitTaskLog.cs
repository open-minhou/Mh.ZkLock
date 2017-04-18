using System;
using Raven.Message.RabbitMQ.Abstract;

namespace Mc.JobDispatherTest
{
    public class RabbitTaskLog : ILog
    {
        //Log _log = new Log("RabbitTaskLog");
        public void LogDebug(string info, object dataObj)
        {
#if DEBUG
           // _log.Info("{0}{1}{2}", info, Environment.NewLine, dataObj);
           //Console.WriteLine(info);
#endif
        }

        public void LogError(string errorMessage, Exception ex, object dataObj)
        {
            //Console.WriteLine($"{errorMessage}:{ex}");
            //_log.Error("{0}{1}{2}{1}{3}", errorMessage, Environment.NewLine, ex, dataObj);
        }
    }
}
