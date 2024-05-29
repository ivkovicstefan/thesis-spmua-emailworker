using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPMUA.EmailWorker.Models.Dictionaries
{
    public enum EmailQueueStatusEnum
    {
        Ready = 1,
        Sent = 2,
        Failed = 3
    }
}
