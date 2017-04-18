using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Mc.JobDispater.Abstruct
{
    public interface ICacheClient
    {
        Task<bool> SetIfNotExist(string key, string value);
        Task<bool> Remove(string key);
    }
}
