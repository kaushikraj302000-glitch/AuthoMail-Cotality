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
        
        public DateTime GetEstimatedSubmittedOn(List<Sweep> sweepTimings, int bufferTime)
        {
            var revisedReceivedOnEst = this.ReceivedOn.UtcToEst();
            if (revisedReceivedOnEst.DayOfWeek == DayOfWeek.Saturday)
            {
                sweepTimings = sweepTimings.Where(s => s.WeekDayType == "Saturday").ToList();
            }
            else
            {
                sweepTimings = sweepTimings.Where(s => s.WeekDayType == "Weekday").ToList();
            }
            sweepTimings = sweepTimings.OrderBy(t => t.SrNo).ToList();


            // adjust for sunday
            revisedReceivedOnEst = revisedReceivedOnEst.DayOfWeek == DayOfWeek.Sunday ? revisedReceivedOnEst.AddDays(1) : revisedReceivedOnEst;

            var applicableSweep = sweepTimings.FirstOrDefault(s => s.SweepTime >= revisedReceivedOnEst.TimeOfDay);

            if (applicableSweep == null)
            {
                revisedReceivedOnEst = revisedReceivedOnEst.AddDays(1);
                // adjust for sunday
                revisedReceivedOnEst = revisedReceivedOnEst.DayOfWeek == DayOfWeek.Sunday ? revisedReceivedOnEst.AddDays(1) : revisedReceivedOnEst;

                applicableSweep = sweepTimings.First();
            }

            var estimatedSubmittedOn = (revisedReceivedOnEst.Date + applicableSweep.SweepTime.Add(new TimeSpan(0, bufferTime, 0))).EstToUtc();

            return estimatedSubmittedOn;
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
}
