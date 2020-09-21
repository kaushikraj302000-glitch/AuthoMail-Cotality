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
        public string UniqueId { get; set; }
        public string Status { get; set; }
        public string ErrorLog { get; set; }

        public DateTime ReceivedOn { get; set; }

        public DateTime ProcessedOn
        {
            get { return Common.Now; }
        }

        public float SizeCheck { get; set; }
        public float? Download { get; set; }
        public float? Validate { get; set; }
        public float? Merge { get; set; }
        public float? PostBatch { get; set; }
        public float? Cleanup { get; set; }

        public double TotalTime
        {
            get { return (float)ProcessedOn.Subtract(ReceivedOn).TotalSeconds; }
        }
    }
}
