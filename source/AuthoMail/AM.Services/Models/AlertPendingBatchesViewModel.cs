using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;

namespace AM.Services
{
    public class PendingMailPieceInfo
    {
        public int Id { get; set; }
        public int MailBatchId { get; set; }
        public Guid? UniqueId { get; set; }
        public bool? IsSubmitted { get; set; }
        public string AppCode { get; set; }
        public string BatchName { get; set; }
        public DateTime ReceivedOn { get; set; }
        public string PipelineStatus { get; set; }
        public string ShippingStatus { get; set; }
        public string DocTypeCode { get; set; }
        public string ShippingCarrier { get; set; }
        public string ShippingServiceType { get; set; }
        public int? ClientId { get; set; }
        public string PrintLocation { get; set; }
        public DateTime? StdPrepStartTime { get; set; }
        public double MinRemainingHours
        {
            get
            {
                return StdPrepStartTime.Value.Subtract(Common.Now).TotalHours;
            }
        }

        public DateTime GetEstimatedSubmittedOn(List<Sweep> sweepTimings, int bufferTime)
        {
            using (ICommonService commonSvc = new CommonService())
            {
                return commonSvc.GetStandardSubmissionTime(this.ReceivedOn, sweepTimings).Add(new TimeSpan(0, bufferTime, 0));
            }
        }
    }
    public class BatchInfo
    {
        public int Id { get; set; }
        public string AppCode { get; set; }
        public string BatchName { get; set; }
        public DateTime ReceivedOn { get; set; }
        public string PipelineStatus { get; set; }
        public int? ClientId { get; set; }
        public List<PendingMailPieceInfo> DelayedMailPieces { get; set; }
    }
    public class MonitorReport
    {
        public List<BatchInfo> DelayedBatches { get; set; }
        public List<PendingMailPieceInfo> DelayedMailPieces { get; set; }
        public List<MonitorAppCodeCount> AppCodeCountByBatch
        {
            get
            {
                if (DelayedBatches == null) return null;
                return DelayedBatches.GroupBy(g => g.AppCode).Select(g => new MonitorAppCodeCount { AppCode = g.Key, Total = g.Count() }).ToList();
            }
        }
        public List<MonitorAppCodeCount> AppCodeCountByMailPiece
        {
            get
            {
                if (DelayedMailPieces == null) return null;
                return DelayedMailPieces.GroupBy(g => g.AppCode).Select(g => new MonitorAppCodeCount { AppCode = g.Key, Total = g.Count() }).ToList();
            }
        }
        public string Status
        {
            get
            {
                if (this.DelayedMailPieces.Count > 0 || this.DelayedBatches.Count > 0)
                    return "red";
                else
                    return "green";
            }
        }
    }
    public class MonitorAppCodeCount
    {
        public string AppCode { get; set; }
        public int Total { get; set; }
    }
}
