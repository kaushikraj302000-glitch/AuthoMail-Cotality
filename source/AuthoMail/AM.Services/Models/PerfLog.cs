using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class PerfLog
    {
        public string UniqueId { get; set; }
        public string Status { get; set; }
        public string ErrorLog { get; set; }

        public DateTime ProcessedOn
        {
            get { return DateTime.Now; }
        }

        public float SizeCheck { get; set; }
        public float Download { get; set; }
        public float Validate { get; set; }
        public float Merge { get; set; }
        public float PostBatch { get; set; }
        public float Cleanup { get; set; }
        public float TotalTime { get; set; }
    }
}
