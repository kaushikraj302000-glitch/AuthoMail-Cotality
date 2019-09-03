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
        public int? PrintTrailBatchId { get; set; }
        public int PrintBatch { get; set; }
        public string TrackingNo { get; set; }
        public string ShippingServiceType { get; set; }
        public string AMBatchPipelineStatus { get; set; }
        public string PnSBatchPipelineStatus { get; set; }
        public string AMMailPieceStatus { get; set; }
        public string PnSMailPieceStatus { get; set; }
        public string PrintLocation { get; set; }
        public string AMErrorDescription { get; set; }
        public string PnSErrorDescription { get; set; }
        public DateTime? PCIBatchPrepEnd { get; set; }
        public bool IsPullBackRequested { get; set; }
        public DateTime? PullBackRequestedOn { get; set; }
        public bool IsPulledBack { get; set; }
        public DateTime? PulledBackOn { get; set; }
        public string UserName { get; set; }
    }

    public class PullBackRequestForm
    {
        public string PrintFileName { get; set; }
        public DateTime? SubmittedOn { get; set; }
        public int PrintBatch { get; set; }
        public string PieceId { get; set; }
        public string Name { get; set; }
        public string VendorCode { get; set; }
    }
}
