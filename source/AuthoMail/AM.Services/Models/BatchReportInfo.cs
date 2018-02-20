using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services.Models
{
    public class BatchReportInfo
    {
        public int BusinessId { get; set; }
        public int BatchClustId { get; set; }
        public string DocumentOrPackageType { get; set; }
        public string RetRcptPrefix { get; set; }
        public string RetRcptFirstName { get; set; }
        public string RetRcptMiddleName { get; set; }
        public string RetRcptLastName { get; set; }
        public string RetRcptSuffix { get; set; }
        public string RecipientName
        {
            get
            {
                string name = string.Format("{0} {1} {2} {3} {4}", RetRcptPrefix, RetRcptFirstName, RetRcptMiddleName, RetRcptLastName, RetRcptSuffix);
                return name.IsNullOrWhiteSpace() ? name : name.Trim();
            }
        }
        public string RetRcptOrganization { get; set; }
        public string RetRcptAddrOptionalLine1AboveStreet { get; set; }
        public string RetRcptAddrOptionalLine2AboveStreet { get; set; }
        public string RetRcptAddressStreet { get; set; }
        public string RetRcptAddressAptOrSuiteOrOther { get; set; }
        public string RetRcptAddrOptionalLine1BelowStreet { get; set; }
        public string RetRcptAddrOptionalLine2BelowStreet { get; set; }
        public string RetRcptAddressCity { get; set; }
        public string RetRcptAddressState { get; set; }
        public string RetRcptAddressZip { get; set; }
        public string Address
        {
            get
            {
                string address = string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8}",
                    RetRcptAddrOptionalLine1AboveStreet.IsNullOrWhiteSpace() ? null : RetRcptAddrOptionalLine1AboveStreet + ", ",
                    RetRcptAddrOptionalLine2AboveStreet.IsNullOrWhiteSpace() ? null : RetRcptAddrOptionalLine2AboveStreet + ", ",
                    RetRcptAddressStreet.IsNullOrWhiteSpace() ? null : RetRcptAddressStreet + ", ",
                    RetRcptAddressAptOrSuiteOrOther.IsNullOrWhiteSpace() ? null : RetRcptAddressAptOrSuiteOrOther + ", ",
                    RetRcptAddrOptionalLine1BelowStreet.IsNullOrWhiteSpace() ? null : RetRcptAddrOptionalLine1BelowStreet + ", ",
                    RetRcptAddrOptionalLine2BelowStreet.IsNullOrWhiteSpace() ? null : RetRcptAddrOptionalLine2BelowStreet + ", ",
                    RetRcptAddressCity.IsNullOrWhiteSpace() ? null : RetRcptAddressCity + ", ",
                    RetRcptAddressState.IsNullOrWhiteSpace() ? null : RetRcptAddressState + ", ",
                    RetRcptAddressZip.IsNullOrWhiteSpace() ? null : RetRcptAddressZip + ", ");
                return address.IsNullOrWhiteSpace() ? address : address.TrimEnd(',').Trim();
            }
        }
        public string CurrentStatus { get; set; }
        public DateTime? ShippedOn { get; set; }
        //public decimal ShippingCost { get; set; }
    }

    public class MailPieceReportInfo
    {
        //public int BusinessId { get; set; }
        public string BusinessName { get; set; }
        public string BusinessValue { get; set; }
        public string RcptPrefix { get; set; }
        public string RcptFirstName { get; set; }
        public string RcptMiddleName { get; set; }
        public string RcptLastName { get; set; }
        public string RcptSuffix { get; set; }
        public string RecipientName
        {
            get
            {
                string name = string.Format("{0} {1} {2} {3} {4}", RcptPrefix, RcptFirstName, RcptMiddleName, RcptLastName, RcptSuffix);
                return name.IsNullOrWhiteSpace() ? name : name.Trim();
            }
        }
        public string RcptOrganization { get; set; }
        public string RcptAddrOptionalLine1AboveStreet { get; set; }
        public string RcptAddrOptionalLine2AboveStreet { get; set; }
        public string RcptAddressStreet { get; set; }
        public string RcptAddressAptOrSuiteOrOther { get; set; }
        public string RcptAddrOptionalLine1BelowStreet { get; set; }
        public string RcptAddrOptionalLine2BelowStreet { get; set; }
        public string RcptAddressCity { get; set; }
        public string RcptAddressState { get; set; }
        public string RcptAddressZip { get; set; }
        public string Address
        {
            get
            {
                string address = string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8}",
                    RcptAddrOptionalLine1AboveStreet.IsNullOrWhiteSpace() ? null : RcptAddrOptionalLine1AboveStreet + ", ",
                    RcptAddrOptionalLine2AboveStreet.IsNullOrWhiteSpace() ? null : RcptAddrOptionalLine2AboveStreet + ", ",
                    RcptAddressStreet.IsNullOrWhiteSpace() ? null : RcptAddressStreet + ", ",
                    RcptAddressAptOrSuiteOrOther.IsNullOrWhiteSpace() ? null : RcptAddressAptOrSuiteOrOther + ", ",
                    RcptAddrOptionalLine1BelowStreet.IsNullOrWhiteSpace() ? null : RcptAddrOptionalLine1BelowStreet + ", ",
                    RcptAddrOptionalLine2BelowStreet.IsNullOrWhiteSpace() ? null : RcptAddrOptionalLine2BelowStreet + ", ",
                    RcptAddressCity.IsNullOrWhiteSpace() ? null : RcptAddressCity + ", ",
                    RcptAddressState.IsNullOrWhiteSpace() ? null : RcptAddressState + ", ",
                    RcptAddressZip.IsNullOrWhiteSpace() ? null : RcptAddressZip + ", ");
                return address.IsNullOrWhiteSpace() ? address : address.TrimEnd(',').Trim();
            }
        }
        public string CurrentStatus { get; set; }
        public DateTime? ShippedOn { get; set; }
        public decimal ShippingCost { get; set; }
        public string ShippingStatus { get; set; }
    }
}
