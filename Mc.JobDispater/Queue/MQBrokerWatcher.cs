using System;
using System.Configuration;
using Raven.Message.RabbitMQ.Abstract;

namespace Mc.JobDispater.Queue
{
    public class MqBrokerWatcher : IBrokerWatcher
    {
        public event EventHandler<BrokerChangeEventArg> BrokerUriChanged;

        public string GetBrokerUri(string brokerName)
        {
            return ConfigurationManager.AppSettings[brokerName];
        }
    }
}
