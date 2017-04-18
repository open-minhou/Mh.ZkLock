using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mc.JobDispatherTest
{
    public class TestTask1
    {
        public int Id { get; set; }
        public string State { get; set; }
        public override string ToString()
        {
            return $"Id:{Id},State:{State}";
        }
    }
}
