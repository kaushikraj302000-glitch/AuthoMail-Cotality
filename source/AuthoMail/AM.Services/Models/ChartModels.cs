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
        public int InProgress { get; set; }
        public int ShippedPartial { get; set; }
        public int ShippedAll { get; set; }
        public int Failed { get; set; }
    }
    public class TodayMailPieceChart
    {
        public string BatchName { get; set; }
        public int InProgress { get; set; }
        public int PulledBack { get; set; }
        public int Error { get; set; }
        public int Shipped { get; set; }
    }
}
