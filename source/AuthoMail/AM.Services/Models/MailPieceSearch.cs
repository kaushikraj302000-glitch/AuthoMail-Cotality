using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;
namespace AM.Services
{

    public class MailPieces
    {
        public int Id { get; set; }
        public Guid UniqueId { get; set; }
        public int? ClientId { get; set; }
        public int MailBatchId { get; set; }
        public int? PrintBatchId { get; set; }
        public string BusinessFriendlyIdName { get; set; }
        public string BusinessFriendlyIdValue { get; set; }
        public string FileName { get; set; }
        public string FilePattern { get; set; }
        public string DocumentOrPackageType { get; set; }
        public int? SLADays { get; set; }
        public DateTime? DueDate { get; set; }
        public string ShippingStatus { get; set; }
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

        public int TotalPages { get; set; }
        public int TotalSheets { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDesc { get; set; }
        public string ErrorCodeDesc
        {
            get
            {
                return string.Format("{0}{1}", this.ErrorCode, this.ErrorDesc.IsNullOrWhiteSpace() ? null : (" <br/>  " + this.ErrorDesc));
            }
        }
        public string TrackingNo { get; set; }
        public int IsPullBackRequested { get; set; }
        public DateTime? PullBackRequestedOn { get; set; }
        public int IsPulledBack { get; set; }
        public DateTime? PulledBackOn { get; set; }
        public string PulledBackRequestedBy { get; set; }
        public int IsPrinted { get; set; }
        public DateTime? PrintedOn { get; set; }
        public int IsShipped { get; set; }
        public DateTime? ShippedOn { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal? Weight { get; set; }
        public bool IsQCRequested { get; set; }
        public bool IsQCPerformed { get; set; }
        public int? DocTypeId { get; set; }
        public MailBatches MailBatches { get; set; }
        public Clients Clients { get; set; }
        public DocTypes DocTypes { get; set; }
        public Users Users { get; set; }

    }
    public class MailBatches
    {
        public int Id { get; set; }
        public Guid UniqueId { get; set; }
        public int? ClientId { get; set; }
        public int? SFTChannelId { get; set; }
        public string ClientBatchId { get; set; }
        public string BatchName { get; set; }
        public string DocumentOrPackageType { get; set; }
        public int IsReceived { get; set; }
        public DateTime? ReceivedOn { get; set; }
        public int IsDownloaded { get; set; }
        public DateTime? DownloadedOn { get; set; }
        public int IsAcknowledged { get; set; }
        public DateTime? AcknowledgedOn { get; set; }
        public int IsValidated { get; set; }
        public DateTime? ValidatedOn { get; set; }
        public int IsPrepped { get; set; }
        public DateTime? PreppedOn { get; set; }
        public int IsSubmittedToShipping { get; set; }
        public DateTime? SubmittedToShippingOn { get; set; }
        public int IsShippingResponseReceived { get; set; }
        public DateTime? ShippingResponseReceivedOn { get; set; }
        public int IsReportPrepped { get; set; }
        public DateTime? ReportPreppedOn { get; set; }
        public int IsReportedBack { get; set; }
        public DateTime? ReportedBackOn { get; set; }
        public string Priority { get; set; }
        public string BatchStatus { get; set; }
        public string ValidationStatus { get; set; }
        public string ValidationErrors { get; set; }
        public string PipelineStatus { get; set; }
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
        public string PrintLocation { get; set; }
        public SFTChannels SFTChannels { get; set; }
    }
    public class Clients
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AppCode { get; set; }
        public int? BusinessUnit { get; set; }
        public bool IsAMActive { get; set; }
        public bool IsPnSActive { get; set; }
        public string TimeZone { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
    }
    public class DocTypes
    {
        public int Id { get; set; }
        public string DocTypeCode { get; set; }
        public string Description { get; set; }
        public int ClientId { get; set; }
        public string PrintSize { get; set; }
        public decimal ContentScalePercent { get; set; }
        public string PrintFormat { get; set; }
        public decimal QCPercent { get; set; }
        public int? SLADays { get; set; }
        public TimeSpan SLACutOffTime { get; set; }
        public string ShippingCarrier { get; set; }
        public string ShippingServiceType { get; set; }
        public string ShippingAccountCode { get; set; }
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
        public string ClientEmails { get; set; }
        public string ClientContactNo { get; set; }
        public string SupportEmails { get; set; }
        public string SupportContactNo { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public string CreatedBy { get; set; }
    }
    public class SFTChannels
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Protocol { get; set; }
        public string Host { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ToADEDir { get; set; }
        public string FromADEDir { get; set; }
        public string ArchiveDir { get; set; }
        public string PostDownloadAction { get; set; }
        public bool IsActive { get; set; }
    }
    public class Users
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public int? ClientId { get; set; }
    }

    public class PullBackRequests
    {
        public int Id { get; set; }
        public int? ClientId { get; set; }
        public int MailBatchId { get; set; }
        public int MailPieceId { get; set; }
        public Guid MailPieceUniqueId { get; set; }
        public string Status { get; set; }
        public DateTime RequestedOn { get; set; }
        public string RequestedBy { get; set; }
        public MailPieces MailPieces { get; set; }
        public Users Users { get; set; }
    }
}
