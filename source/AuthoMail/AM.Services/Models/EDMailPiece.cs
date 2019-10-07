using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;
using System.ComponentModel.DataAnnotations;
namespace AM.Services
{

    public class EDMailPiece
    {
        public int Id { get; set; }
        public Guid UniqueId { get; set; }
        public int? ClientId { get; set; }
        public int? ChannelId { get; set; }
        public Guid BatchUniqueId { get; set; }
        public string BatchName { get; set; }
        public string BusinessFriendlyIdName { get; set; }
        public string BusinessFriendlyIdValue { get; set; }
        public string FileName { get; set; }
        public string FilePattern { get; set; }
        public string DocumentOrPackageType { get; set; }
        public int? DocTypeId { get; set; }
        public int? ClientDocTypeId { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string PipelineStatus { get; set; }
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
        public int? SigningRoomId { get; set; }
        public Guid SigningRoomUniqueId { get; set; }
        public string SigningRoomName { get; set; }
        public int? DocumentId { get; set; }
        public DateTime? ReceivedOn { get; set; }
        public int IsReceived { get; set; }
        public DateTime? DownloadedOn { get; set; }
        public int IsDownloaded { get; set; }
        public DateTime? SigningRoomCreatedOn { get; set; }
        public int IsSigningRoomCreated { get; set; }
        public DateTime? InvitationSentOn { get; set; }
        public int IsInvitationSent { get; set; }
        public DateTime? eDeliveredOn { get; set; }
        public int IseDelivered { get; set; }
        public DateTime? ConsentedOn { get; set; }
        public int IsConsented { get; set; }
        public DateTime? NotConsentedOn { get; set; }
        public int IsNotConsented { get; set; }
        public DateTime? DocReviewedOn { get; set; }
        public int IsDocReviewed { get; set; }
        public DateTime? DocDownloadedOn { get; set; }
        public int IsDocDownloaded { get; set; }
        public DateTime? ESignedOn { get; set; }
        public int IsESigned { get; set; }
        public Int64 PDFFileSize { get; set; }
        public Clients Clients { get; set; }
        public DocTypes DocTypes { get; set; }
        public SFTChannels SFTChannel { get; set; }
    }
}
