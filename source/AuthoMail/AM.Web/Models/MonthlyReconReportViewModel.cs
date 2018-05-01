using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AM.Web.Models
{
    public class MonthlyReconReportViewModel:MailPieceCommonViewModel
    {
        public string DocType { get; set; }
        public string Channel { get; set; }
        public string PrintLocation { get; set; }
        public string ShippingMethod { get; set; }
        public string Status { get; set; }
        public string TrackingNo { get; set; }
        public int TotalPages { get; set; }
        public DateTime? PrintedOn { get; set; }
        public DateTime? ShippedOn { get; set; }
    }
}