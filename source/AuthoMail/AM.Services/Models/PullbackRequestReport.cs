using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class PullbackRequestReport
    {
        public int Id { get; set; }
        public int MailBatchId { get; set; }
        public string ClientBatchId { get; set; }
        public int MailPieceId { get; set; }
        public Guid MailPieceUniqueId { get; set; }
        public string Status { get; set; }
        public DateTime RequestedOn { get; set; }
        public string RequestedBy { get; set; }
        public string UserName { get; set; }
    }
}
