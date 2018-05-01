using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AM.Web.Models
{
    public class DailyDetailReportViewModel:MailPieceCommonViewModel
    {
        public Guid UniqueId { get; set; }
        public string TrackingNo { get; set; }
        public int TotalPages { get; set; }
        public string ClientBatchId { get; set; }
        public string Channel { get; set; }
        public string DocType { get; set; }
        public int BatchId { get; set; }
        public DateTime? PrintedOn { get; set; }
        public DateTime? ShippedOn { get; set; }
        public string ClientCode { get; set; }//Client Code
        public string ShippingMethod { get; set; }
        public string PrintLocation { get; set; }
        public string Status { get; set; }

    }
}