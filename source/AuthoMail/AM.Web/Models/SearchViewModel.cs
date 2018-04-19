using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AM.Web.Models
{
    public class SearchViewModel
    {
        public string BusinessFriendlyIdValue { get; set; }
        public string ShippingStatus { get; set; }
        public string RcptFirstName { get; set; }
        public string RcptLastName { get; set; }
        public string RcptAddressCity { get; set; }
        public string RcptAddressZIP { get; set; }
        public DateTime? ReceivedOn { get; set; }
        public DateTime? ShippedOn { get; set; }
        public string TrackingNo { get; set; }
        public string DocumentOrPackageType { get; set; }
        public string Channel { get; set; }
        public string ShippingMethod { get; set; }

    }
}