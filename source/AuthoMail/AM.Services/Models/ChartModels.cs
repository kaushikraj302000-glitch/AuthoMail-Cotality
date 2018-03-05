using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services.Models
{
    public class WeeklyChart
    {
        public DateTime Date { get; set; }
        public string ShortDate
        {
            get
            {
                return this.Date.ToShortDateString();
            }
        }
        public int InProgress { get; set; }
        public int ShippedPartial { get; set; }
        public int ShippedAll { get; set; }
        public int Failed { get; set; }
    }
    public class TodayMailPieceChart
    {
        public int BatchId { get; set; }
        public string BatchName { get; set; }
        public int InProgress { get; set; }
        public int PulledBack { get; set; }
        public int Error { get; set; }
        public int Shipped { get; set; }
    }
    public class BatchListByChartData
    {
        public string ClientBatchId { get; set; }
        public int BatchId { get; set; }
        public Guid BatchUniqueId { get; set; }
        public string BatchName { get; set; }
        public string BatchStatus { get; set; }
        public string DocumentOrPackageType { get; set; }
        public DateTime? ReceivedOn { get; set; }
    }
}
