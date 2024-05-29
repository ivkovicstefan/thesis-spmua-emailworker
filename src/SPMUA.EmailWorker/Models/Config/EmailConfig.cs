using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPMUA.EmailWorker.Models.Config
{
    internal class EmailConfig
    {
        public string FromEmail { get; set; } = String.Empty;
        public string Password { get; set; } = String.Empty;
        public string SmtpClientHost { get; set; } = String.Empty;
        public int SmtpClientPort { get; set; }
    }
}
