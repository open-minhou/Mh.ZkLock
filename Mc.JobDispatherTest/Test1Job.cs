using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mc.JobDispater.Abstruct;

namespace Mc.JobDispatherTest
{
    public class Test1Job:IMcJobExcute<TestTask1>
    {
        public async Task<List<TestTask1>> GetWattingTasks()
        {
            return new List<TestTask1> {new TestTask1 {Id=1,State = "Watting"}, new TestTask1 {Id=2, State = "Watting" }, };
        }

        public async Task<bool> ExcuteTask(TestTask1 task)
        {
            task.State = "Done";
            return true;
        }

        
    }
}
