using SPMUA.EmailWorker.Models.Dictionaries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPMUA.EmailWorker.Models
{
    public class EmailQueueItemDTO
    {
        public int EmailQueueId { get; set; }
        public string ToEmail { get; set; } = String.Empty;
        public string Subject { get; set; } = String.Empty;
        public string Body { get; set; } = String.Empty;
        public int NoOfAttempts { get; set; }
        public EmailQueueStatusEnum EmailQueueStatus { get; set; }
    }
}
