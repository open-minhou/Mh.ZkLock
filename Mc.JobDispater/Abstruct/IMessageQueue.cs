using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mc.JobDispater.Abstruct
{
    public interface IMessageQueue<T>
    {
        void Enqueue(T item);
        void OnReceive(Func<T, bool> doJob);
    }
}
