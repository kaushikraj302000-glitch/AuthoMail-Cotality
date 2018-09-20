using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class AlertPendingBatchesViewModel
    {
        public int Id { get; set; }
        public string AppCode { get; set; }
        public string BatchName { get; set; }
        public DateTime? ReceivedOn { get; set; }
        public string PipelineStatus { get; set; }
        public string BatchStatus { get; set; }
        public int TotalValidMailPieces { get; set; }
    }
}
