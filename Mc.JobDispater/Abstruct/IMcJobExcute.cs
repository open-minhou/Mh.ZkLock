using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mc.JobDispater.Abstruct
{
    public interface IMcJobExcute<T>
    {
        Task<List<T>> GetWattingTasks();
        Task<bool> ExcuteTask(T task);
    }
}
