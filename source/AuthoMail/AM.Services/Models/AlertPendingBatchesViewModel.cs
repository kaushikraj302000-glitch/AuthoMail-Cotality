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
        public string PrintLocation { get; set; }
        public int PendingMailPieces { get; set; }
        public string SweepName { get; set; }
    }
    public class PendingBatchesListByPrintLocation
    {
        public List<AlertPendingBatchesViewModel> Dallas { get; set; }
        public List<AlertPendingBatchesViewModel> PCI { get; set; }
        public List<AlertPendingBatchesViewModel> Empty { get; set; }
    }
    public class PCIBundleSweepTiming
    {
        public string SweepName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime SweepTime { get; set; }
    }
}
