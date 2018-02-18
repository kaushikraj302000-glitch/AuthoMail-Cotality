using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services.Models
{
    public class MonthlyReconReport
    {
        public string LoanNo { get; set; }
        public string FileName { get; set; }
        public string DocumentOrPackageType { get; set; }
        public string RcptFirstName { get; set; }
        public string RcptMiddleName { get; set; }
        public string RcptLastName { get; set; }
        public string CoRcptOrganization { get; set; }
        public string RcptAddrOptionalLine1AboveStreet { get; set; }
        public string RcptAddrOptionalLine2AboveStreet { get; set; }
        public string RcptAddressStreet { get; set; }
        public string RcptAddressAptOrSuiteOrOther { get; set; }
        public string RcptAddrOptionalLine1BelowStreet { get; set; }
        public string RcptAddrOptionalLine2BelowStreet { get; set; }
        public string RcptAddressCity { get; set; }
        public string RcptAddressState { get; set; }
        public string RcptAddressZIP { get; set; }

        public DateTime? ReceivedOn { get; set; }
        public DateTime? PrintedOn { get; set; }
        public DateTime? MailedOn { get; set; }
        public int TotalPages { get; set; }//Pages Shipped
        public string ShippingMethod { get; set; }
        public int TrackingNo { get; set; }
        public string Status { get; set; }
        public string Channel { get; set; }//chk in report
        public string PrintLocation { get; set; }//chk in report

    }
    public class SummaryControlReport
    {
        public string BatchClustId { get; set; }
        public string Id { get; set; }
        public string DocumentOrPackageType { get; set; }
        public string LoanNo { get; set; }
        public string FileName { get; set; }
        public string RcptFirstName { get; set; }
        public string RcptMiddleName { get; set; }
        public string RcptLastName { get; set; }
        public string CoRcptOrganization { get; set; }
        public string RcptAddrOptionalLine1AboveStreet { get; set; }
        public string RcptAddrOptionalLine2AboveStreet { get; set; }
        public string RcptAddressStreet { get; set; }
        public string RcptAddressAptOrSuiteOrOther { get; set; }
        public string RcptAddrOptionalLine1BelowStreet { get; set; }
        public string RcptAddrOptionalLine2BelowStreet { get; set; }
        public string RcptAddressCity { get; set; }
        public string RcptAddressState { get; set; }
        public string RcptAddressZIP { get; set; }

        public DateTime? ReceivedOn { get; set; }
        public DateTime? PrintedOn { get; set; }
        public DateTime? MailedOn { get; set; }
        public int PageShipped { get; set; }//Total Pages
        public string ShippingMethod { get; set; }
        public int TrackingNo { get; set; }
        public int ErrorCode { get; set; }
        public string ErrorDesc { get; set; }
    }
    public class DocTypeIdDocTypeCode
    {
        public int Id { get; set; }
        public string DocTypeCode { get; set; }
    }
}
