using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public static class ReqStatus
    {
        public const string Verifying = "Verifying";
        public const string BadRequest = "BadRequest";
        public const string Queued = "Queued";
    }

    public class PerfLog
    {
        public Guid MailPieceId { get; set; }
        public string Status { get; set; }
        public string ErrorLog { get; set; }

        public DateTime ReceivedOn { get; set; }

        public DateTime ProcessedOn
        {
            get { return Common.Now; }
        }

        public double InitialCheck { get; set; }
        public double? Download { get; set; }
        public double? Validate { get; set; }
        public double? Merge { get; set; }
        public double? PostBatch { get; set; }
        public double? Cleanup { get; set; }

        public double TotalTime
        {
            get { return (double)ProcessedOn.Subtract(ReceivedOn).TotalSeconds; }
        }
    }
}
