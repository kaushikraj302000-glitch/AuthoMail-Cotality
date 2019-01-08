using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AM.Web.Models
{
    public class ErrorSLAReportViewModel : MailPieceCommonViewModel
    {
        public string DocType { get; set; }
        public int BatchId { get; set; }
        public Guid UniqueId { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ShippedOnMap { get; set; }
        public DateTime? PickedUpOn { get; set; }
        public DateTime? ShippedOn
        {
            get
            {
                return string.IsNullOrWhiteSpace(this.ShippingCarrier) ? this.ShippedOnMap : this.ShippingCarrier.ToLower() == "fedex" ? this.PickedUpOn : this.ShippedOnMap;
            }
        }
        public string ShippingMethod { get; set; }
        public string ShippingCarrier { get; set; }
        public string SLAStatus { get; set; }
        public string TrackingNo { get; set; }
        public string Status { get; set; }
        public string Channel { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDesc { get; set; }
    }
}