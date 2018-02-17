using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Entities
{
    public class AMMailPiece
    {
        public Guid Id { get; set; }
        public int BatchClustId { get; set; }
        public int? PrintBatchClustId { get; set; }
        public string BusinessFriendlyIdName { get; set; }
        public string BusinessFriendlyIdValue { get; set; }
        public string FileName { get; set; }
        public string FilePattern { get; set; }
        public bool IsRcptInfoSpecified { get; set; }
        public string RcptPrefix { get; set; }
        public string RcptFirstName { get; set; }
        public string RcptMiddleName { get; set; }
        public string RcptLastName { get; set; }
        public string RcptSuffix { get; set; }
        public string RcptPhone { get; set; }
        public string RcptOrganization { get; set; }
        public string CoRcptPrefix { get; set; }
        public string CoRcptFirstName { get; set; }
        public string CoRcptMiddleName { get; set; }
        public string CoRcptLastName { get; set; }
        public string CoRcptSuffix { get; set; }
        public string CoRcptPhone { get; set; }
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
        public bool IsReturnInfoSpecified { get; set; }
        public string RetRcptPrefix { get; set; }
        public string RetRcptFirstName { get; set; }
        public string RetRcptMiddleName { get; set; }
        public string RetRcptLastName { get; set; }
        public string RetRcptSuffix { get; set; }
        public string RetRcptPhone { get; set; }
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
        public string PreppedFilePath { get; set; }
        public int TotalPages { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDesc { get; set; }
        public string TrackingNo { get; set; }
        public bool IsPullBackRequested { get; set; }
        public DateTime PullBackRequestedOn { get; set; }
        public bool IsPulledBack { get; set; }
        public DateTime PulledBackOn { get; set; }
        public bool IsShipped { get; set; }
        public DateTime ShippedOn { get; set; }
        public decimal ShippingCost { get; set; }
        public string ShippingStatus { get; set; }
        public bool IsQCRequested { get; set; }
        public bool IsQCPerformed { get; set; }
    }

    public class MailPieceCustomInfo
    {
        public Guid MailPieceId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
