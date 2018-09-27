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

        public string RcptName
        {
            get
            {
                string nm = string.Format("{0} {1} {2}", this.RcptFirstName, this.RcptMiddleName, this.RcptLastName);
                return nm.IsNullOrWhiteSpace() ? null : nm.Replace("  ", " ");
            }
        }

        public string CoRcptPrefix { get; set; }
        public string CoRcptFirstName { get; set; }
        public string CoRcptMiddleName { get; set; }
        public string CoRcptLastName { get; set; }
        public string CoRcptSuffix { get; set; }
        public string CoRcptName
        {
            get
            {
                string nm = string.Format("{0} {1} {2}", this.CoRcptFirstName, this.CoRcptMiddleName, this.CoRcptLastName);
                return nm.IsNullOrWhiteSpace() ? null : nm.Replace("  ", " ");
            }
        }
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

        public string RcptAddress
        {
            get
            {
                string add = string.Format("{0} {1} {2} {3} {4} {5}", this.RcptAddrOptionalLine1AboveStreet, this.RcptAddrOptionalLine2AboveStreet, this.RcptAddressStreet, this.RcptAddressAptOrSuiteOrOther, this.RcptAddrOptionalLine1BelowStreet, this.RcptAddrOptionalLine2BelowStreet);
                return add;
            }
        }

        public bool IsReturnInfoSpecified { get; set; }
        public string RetRcptPrefix { get; set; }
        public string RetRcptFirstName { get; set; }
        public string RetRcptMiddleName { get; set; }
        public string RetRcptLastName { get; set; }
        public string RetRcptSuffix { get; set; }

        public string RetRcptName
        {
            get
            {
                string nm = string.Format("{0} {1} {2}", this.RetRcptFirstName, this.RetRcptMiddleName, this.RetRcptLastName);
                return nm.IsNullOrWhiteSpace() ? null : nm.Replace("  ", " ");
            }
        }

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

        public string RetRcptAddress
        {
            get
            {
                string add = string.Format("{0} {1} {2} {3} {4} {5}", this.RetRcptAddrOptionalLine1AboveStreet, this.RetRcptAddrOptionalLine2AboveStreet, this.RetRcptAddressStreet, this.RetRcptAddressAptOrSuiteOrOther, this.RetRcptAddrOptionalLine1BelowStreet, this.RetRcptAddrOptionalLine2BelowStreet);
                return add;
            }
        }

        public int TotalPages { get; set; }
        public int TotalSheets { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDesc { get; set; }
        public string ErrorDescription
        {
            get
            {
                if (!ErrorDesc.IsNullOrWhiteSpace()) return ErrorDesc.Replace("\r", string.Empty).Replace("\n", string.Empty);
                return ErrorDesc;
            }
        }
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
        public int IsPickedUp { get; set; }
        public DateTime? PickedUpOn { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal? Weight { get; set; }
        public bool IsQCRequested { get; set; }
        public bool IsQCPerformed { get; set; }
        public int? DocTypeId { get; set; }
        public string PrintLocation { get; set; }
        public DateTime? StdSubmissionTime { get; set; }
        public DateTime? MaxSubmissionTime { get; set; }
        public string MissedSLAReason { get; set; }
        public MailBatches MailBatches { get; set; }
        public Clients Clients { get; set; }
        public DocTypes DocTypes { get; set; }
        public Users Users { get; set; }
        public string PdfReceivedPath
        {
            get
            {
                if (this.MRDFRequests == null || this.Clients == null) return null;

                return string.Format(@"ReturnPdfs\{0}\{1}\{2}-{3}-{4}_{5}", this.Clients.AppCode, this.MRDFRequests.JobId, this.MRDFRequests.JobId, this.MRDFRequests.PieceId, this.Id, this.FileName);
            }
        }
        public List<MailPieceCustomInfoTest> MailPieceCustomInfoTest { get; set; }
        public MRDFRequests MRDFRequests { get; set; }

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
        public SFTChannels SFTChannels { get; set; }
        public Clients Clients { get; set; }
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
        public BusinessUnitModel BusinessUnitModel { get; set; }
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
        public Clients Client { get; set; }
        public string BizdayCutOff { get; set; }
        public string BizdaySLAType { get; set; }
        public string BizdayTimeSpan { get; set; }
        public string SatCutOff { get; set; }
        public string SatSLAType { get; set; }
        public string SatTimeSpan { get; set; }
        public string SunCutOff { get; set; }
        public string SunSLAType { get; set; }
        public string SunTimeSpan { get; set; }
        public string ShippingSpecialService { get; set; }

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

    public class MailPieceCustomInfoTest
    {
        public int MailPieceId { get; set; }
        public string Key { get; set; }
        public string DataType { get; set; }
        public string Value { get; set; }
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
    public class BusinessUnitModel
    {
        public int Id { get; set; }
        public string BusinessUnit { get; set; }
        public string CustCode { get; set; }
        public bool IsActive { get; set; }
    }
    public class MRDFRequests
    {
        public Guid UniqueId { get; set; }
        public int JobId { get; set; }
        public int PieceId { get; set; }
        public DateTime Date { get; set; }
        public int? TotalSheets { get; set; }
        public string InputWeight { get; set; }
        public string Feeder3_Sel { get; set; }
        public string Feeder4_Sel { get; set; }
        public string Feeder5_Sel { get; set; }
        public string LowCap_Sel { get; set; }
        public string NoSeal_Sel { get; set; }
        public string Feeder2_Sel { get; set; }
        public string ZipMark_Sel { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Zip5 { get; set; }
        public string Zip4 { get; set; }
        public string Zip2 { get; set; }
        public string DivertVS2 { get; set; }
        public string DivertVS3 { get; set; }
        public string DivertVS4 { get; set; }
        public string ReturnLogoBitMap { get; set; }
        public string ReturnLine1 { get; set; }
        public string ReturnLine2 { get; set; }
        public string ReturnLine3 { get; set; }
        public string ReturnLine4 { get; set; }
        public string ReturnLine5 { get; set; }
        public string ReturnLine6 { get; set; }
        public string GroupNumber { get; set; }
        public string SocialSecNumber { get; set; }
        public string CheckAmount { get; set; }
        public string PassThru { get; set; }
        public string Expansion { get; set; }
        public string EndofRecord { get; set; }
        public string PDFFileName { get; set; }
        public int? ErrId { get; set; }
        public string ErrDesc { get; set; }
        public string MailPieceStatus { get; set; }
        public bool IsPDFReceived { get; set; }
        public string PrintFileName { get; set; }
    }

    public class SearchViewModel
    {
        public string BusinessIdValue { get; set; }
        public string Status { get; set; }
        public Guid UniqueId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string City { get; set; }
        public string ZIP { get; set; }
        public string State { get; set; }
        public DateTime? ReceivedOn { get; set; }
        public DateTime? ShippedOn { get; set; }
        public string TrackingNo { get; set; }
        public string DocType { get; set; }
        public DateTime? DueDate { get; set; }
        public string Channel { get; set; }
        public string ShippingMethod { get; set; }
        public string ShippingCarrier { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDesc { get; set; }
        public string PrintLocation { get; set; }

    }

    public class MonthlyReconReportViewModel
    {
        public int MailPieceId { get; set; }
        public string BusinessIdValue { get; set; }
        public string FileName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZIP { get; set; }
        public DateTime? ReceivedOn { get; set; }
        public string DocType { get; set; }
        public string Channel { get; set; }
        public string PrintLocation { get; set; }
        public string ShippingMethod { get; set; }
        public string Status { get; set; }
        public string TrackingNo { get; set; }
        public int TotalPages { get; set; }
        public DateTime? PrintedOn { get; set; }
        public DateTime? ShippedOn { get; set; }
        public string AppCode { get; set; }
        public int? JobId { get; set; }
        public int? PieceId { get; set; }
        public string PdfReceivedPath
        {
            get
            {
                if (this.JobId == null || this.PieceId == null) return null;
                return string.Format(@"ReturnPdfs\{0}\{1}\{2}-{3}-{4}_{5}", this.AppCode, this.JobId, this.JobId, this.PieceId, this.MailPieceId, this.FileName);
            }
        }
    }
}
