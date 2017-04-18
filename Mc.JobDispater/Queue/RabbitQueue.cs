using System;
using Mc.JobDispater.Abstruct;

namespace Mc.JobDispater.Queue
{
    public class RabbitQueue<T> : IMessageQueue<T> 
    {
        readonly RabbitTaskChannel _channel;
        readonly string _queueName;

        public RabbitQueue(string queueName,string connString)
        {
            _channel = new RabbitTaskChannel(connString);
            _queueName = queueName;
        }

        public void Enqueue(T item)
        {
            _channel.Enqueue(_queueName,0, item);
        }

        public void OnReceive(Func<T, bool> doJob)
        {
            _channel.OnReceive(_queueName, doJob);
        }
    }
}
