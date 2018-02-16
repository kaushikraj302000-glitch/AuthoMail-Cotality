using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services.Models
{
    public class DeliveryReportVM
    {
        public string BatchNumber { get; set; }
        public string Id { get; set; }
        public string DocumentOrPackageType { get; set; }
        public string LoanNumber { get; set; }
        public string FileName { get; set; }
        public DateTime ReceivedOn { get; set; }
        public DateTime PrintedOn { get; set; }
        public DateTime MailedOn { get; set; }
        public string ClientCode { get; set; }//AppCode
        public int TrackingNo { get; set; }
        public string ShippingMethod { get; set; }
        public string CustomerIdentifier { get; set; }
        public string BorrowerFirstName { get; set; }
        public string BorrowerLastName { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string RequestId { get; set; }
        public int PagesShipped { get; set; }//TotalPages
    }
    public class ErrorReportVM
    {
        public string BatchNumber { get; set; }
        public string Id { get; set; }
        public string DocumentOrPackageType { get; set; }
        public string LoanNumber { get; set; }
        public string FileName { get; set; }
        public DateTime ReceivedOn { get; set; }
        public string BorrowerFirstName { get; set; }
        public string BorrowerLastName { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public int ErrorCode { get; set; }
        public string ErrorText { get; set; }
    }
}
