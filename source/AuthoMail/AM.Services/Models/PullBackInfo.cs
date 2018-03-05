using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class PullBackInfo
    {
        public string ClientBatchId { get; set; }
        public int MailPieceId { get; set; }
        public Guid MailPieceUniqueId { get; set; }
        public int BatchId { get; set; }
        public Guid BatchUniqueId { get; set; }
        public string BatchName { get; set; }
        public string BusinessFriendlyIdValue { get; set; }
        public string BusinessFriendlyIdName { get; set; }
        public string DocumentOrPackageName { get; set; }
        public string FileName { get; set; }
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
        public string RcptAddrOptionalLine1AboveStreet { get; set; }
        public string RcptAddrOptionalLine2AboveStreet { get; set; }
        public string RcptAddressStreet { get; set; }
        public string RcptAddressAptOrSuiteOrOther { get; set; }
        public string RcptAddrOptionalLine1BelowStreet { get; set; }
        public string RcptAddrOptionalLine2BelowStreet { get; set; }
        public string RcptAddressCity { get; set; }
        public string RcptAddressState { get; set; }
        public string RcptAddressZIP { get; set; }
        public string RCPTAddress
        {
            get
            {
                string address = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}",
                    RcptAddrOptionalLine1AboveStreet.IsNullOrWhiteSpace() ? null : RcptAddrOptionalLine1AboveStreet.Trim() + ", ",
                    RcptAddrOptionalLine2AboveStreet.IsNullOrWhiteSpace() ? null : RcptAddrOptionalLine2AboveStreet.Trim() + ", ",
                    RcptAddressStreet.IsNullOrWhiteSpace() ? null : RcptAddressStreet.Trim() + ", ",
                    RcptAddressAptOrSuiteOrOther.IsNullOrWhiteSpace() ? null : RcptAddressAptOrSuiteOrOther.Trim() + ", ",
                    RcptAddrOptionalLine1BelowStreet.IsNullOrWhiteSpace() ? null : RcptAddrOptionalLine1BelowStreet.Trim() + ", ",
                    RcptAddrOptionalLine2BelowStreet.IsNullOrWhiteSpace() ? null : RcptAddrOptionalLine2BelowStreet.Trim() + ", ",
                    RcptAddressCity.IsNullOrWhiteSpace() ? null : RcptAddressCity.Trim() + ", ",
                    RcptAddressState.IsNullOrWhiteSpace() ? null : RcptAddressState.Trim() + ", ",
                    RcptAddressZIP.IsNullOrWhiteSpace() ? null : RcptAddressZIP.Trim() + ", ");
                return address.IsNullOrWhiteSpace() ? address : address.Trim(',').Trim();
            }
        }
        public DateTime? ReceivedOn { get; set; }
        public DateTime? PrintedOn { get; set; }
        public int TotalPages { get; set; }//Pages Shipped
        public string ShippingMethod { get; set; }
        public string TrackingNo { get; set; }
        public string ShippingStatus { get; set; }

        public string RetRcptPrefix { get; set; }
        public string RetRcptFirstName { get; set; }
        public string RetRcptMiddleName { get; set; }
        public string RetRcptLastName { get; set; }
        public string RetRcptSuffix { get; set; }
        public string RetRecipientName
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
        public string RetRcptAddressZIP { get; set; }
        public string RetAddress
        {
            get
            {
                string address = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}",
                    RetRcptAddrOptionalLine1AboveStreet.IsNullOrWhiteSpace() ? null : RetRcptAddrOptionalLine1AboveStreet.Trim() + ", ",
                    RetRcptAddrOptionalLine2AboveStreet.IsNullOrWhiteSpace() ? null : RetRcptAddrOptionalLine2AboveStreet.Trim() + ", ",
                    RetRcptAddressStreet.IsNullOrWhiteSpace() ? null : RetRcptAddressStreet.Trim() + ", ",
                    RetRcptAddressAptOrSuiteOrOther.IsNullOrWhiteSpace() ? null : RetRcptAddressAptOrSuiteOrOther.Trim() + ", ",
                    RetRcptAddrOptionalLine1BelowStreet.IsNullOrWhiteSpace() ? null : RetRcptAddrOptionalLine1BelowStreet.Trim() + ", ",
                    RetRcptAddrOptionalLine2BelowStreet.IsNullOrWhiteSpace() ? null : RetRcptAddrOptionalLine2BelowStreet.Trim() + ", ",
                    RetRcptAddressCity.IsNullOrWhiteSpace() ? null : RetRcptAddressCity.Trim() + ", ",
                    RetRcptAddressState.IsNullOrWhiteSpace() ? null : RetRcptAddressState.Trim() + ", ",
                    RetRcptAddressZIP.IsNullOrWhiteSpace() ? null : RetRcptAddressZIP.Trim() + ", ");
                return address.IsNullOrWhiteSpace() ? address : address.Trim(',').Trim();
            }
        }
    }
    public class QuickPullBackResponse
    {
        public string ClientBatchId { get; set; }
        public int BatchId { get; set; }
        public Guid BatchUniqueId { get; set; }
        public string BatchName { get; set; }
        public int MailPieceId { get; set; }
        public Guid MailPieceUniqueId { get; set; }
        public DateTime ReceivedOn { get; set; }
        public string BusinessFriendlyIdName { get; set; }
        public string BusinessFriendlyIdValue { get; set; }
        public string FileName { get; set; }
        public string ShippingStatus { get; set; }
        public string PulledBackBy { get; set; }
        public DateTime? PulledBackOn { get; set; }
        public string PullBackMessage
        {
            get
            {
                if (PulledBackBy.IsNullOrWhiteSpace() || !PulledBackOn.HasValue) return null;

                return string.Format("{0} by {1} on {2}", Config.Msg_API_PB_PullBackAlreadyRequested, PulledBackBy, PulledBackOn.Value.ToString());
            }
        }
    }
}
