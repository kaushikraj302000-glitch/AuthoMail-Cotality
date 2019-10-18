using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace AM.Services
{

    public class EDMailPiece
    {
        public int Id { get; set; }
        public Guid UniqueId { get; set; }
        public int? ClientId { get; set; }
        public string ClientCode { get; set; }
        public int? ChannelId { get; set; }
        public string ChannelCode { get; set; }
        public int MailBatchId { get; set; }
        public string BatchName { get; set; }
        public string BusinessFriendlyIdName { get; set; }
        public string BusinessFriendlyIdValue { get; set; }
        public string FileName { get; set; }
        public string DocumentOrPackageType { get; set; }
        public int? DocTypeId { get; set; }
        public int? ClientDocTypeId { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string PipelineStatus { get; set; }
        public string RcptFirstName { get; set; }
        public string RcptMiddleName { get; set; }
        public string RcptLastName { get; set; }
        public string RcptEmailId { get; set; }
        public string RcptName
        {
            get
            {
                string nm = string.Format("{0} {1} {2}", this.RcptFirstName, this.RcptMiddleName, this.RcptLastName);
                return nm.IsNullOrWhiteSpace() ? null : nm.Replace("  ", " ");
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
        public bool IsFallBackRequested { get; set; }
        public int? SigningRoomId { get; set; }
        public Guid? SigningRoomUniqueId { get; set; }
        public string SigningRoomName { get; set; }
        public int? DocumentId { get; set; }
        public DateTime? ReceivedOn { get; set; }
        public int IsReceived { get; set; }
        public DateTime? DownloadedOn { get; set; }
        public int IsDownloaded { get; set; }
        public DateTime? ValidatedOn { get; set; }
        public int IsValidated { get; set; }
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
        public bool IsExpired { get; set; }
        public Int64 PDFFileSize { get; set; }
    }

    public class MailPieceInfo
    {
        public string BatchRepo { get; private set; }
        public string AMRequestPath { get; private set; }
        public string PnSRequestPath { get; private set; }
        public string PnSResponsePath { get; private set; }
        public string AMResponsePath { get; private set; }
        public string BatchName { get; private set; }
        public Clients Client { get; private set; }

        public MailPieceInfo(string clientCode, string amRootFolder, int mailBatchId, string batchName)
        {
            BatchName = batchName.Trim();
            //Combined path
            BatchRepo = Path.Combine(amRootFolder, clientCode, "{0}".Fill(mailBatchId));

            AMRequestPath = Path.Combine(BatchRepo, "AMRequest");
            PnSRequestPath = Path.Combine(BatchRepo, "PnSRequest");
            PnSResponsePath = Path.Combine(BatchRepo, "PnSResponse");
            AMResponsePath = Path.Combine(BatchRepo, Util.AMResponseFolderName);
        }
    }
}
