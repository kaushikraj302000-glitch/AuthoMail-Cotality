using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class PullBackViewModel
    {
        public Guid MailPieceUniqueId { get; set; }
        public int PrintBatch { get; set; }
        public string PipelineStatus { get; set; }
        public string MailPieceStatus { get; set; }
        public bool IsPullBackRequested { get; set; }
        public DateTime? PullBackRequestedOn { get; set; }
        public string PulledBackRequestedBy { get; set; }
        public string UserName { get; set; }
    }

    public class PullBackRequestForm
    {
        public string PrintFileName { get; set; }
        public DateTime? SubmittedOn { get; set; }
        public int PrintBatch { get; set; }
        public string PieceId { get; set; }
        public string Name { get; set; }
        public string DocTypeCode { get; set; }
    }
}
