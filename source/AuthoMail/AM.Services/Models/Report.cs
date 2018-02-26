using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services.Models
{
    public class MonthlyReconReport
    {
        public Guid BatchId { get; set; }
        public string BatchClustId { get; set; }
        public DateTime ReceiveDate { get; set; }
        public string BatchDocumentPackageName {
            get
            {
                return BatchDocumentPackageName.IsNullOrWhiteSpace() ? MailPieceDocumentPackageName : BatchDocumentPackageName;
            }
            set; }
        public string PipelineStatus { get; set; }
        public int ClientId { get; set; }
        public Guid MailPieceId { get; set; }
        public string LoanNumber { get; set; }
        public string FileName { get; set; }
        public string BusinessFriendlyIdName { get; set; }
        public DateTime? PrintedOn { get; set; }
        public DateTime? MailedOn { get; set; }
        public int TrackingNo { get; set; }
        public string RcptPrefix { get; set; }
        public string RcptFirstName { get; set; }
        public string RcptMiddleName { get; set; }
        public string RcptLastName { get; set; }
        public string RcptSuffix { get; set; }
        public string RcptFullName
        {
            get
            {
                return string.Format("{0}{1}{2}{3}{4}", RcptPrefix, RcptFirstName, RcptMiddleName, RcptLastName, RcptSuffix);
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
        public string RcptAddressZIP { get; set; }
        public string RcptFullAddress
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
                    RcptAddressZIP.IsNullOrWhiteSpace() ? null : RcptAddressZIP.Trim() + ",");
                return address.IsNullOrWhiteSpace() ? address : address.Trim().Trim(',');
            }
        }
        public string MailPieceDocumentPackageName { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDesc { get; set; }
        public int TotalPages { get; set; }
        public string CustomerIdentifier { get; set; }
        public string ShippingMethod { get; set; }
        public string Channel { get; set; }//chk in report
        public string PrintLocation { get; set; }//chk in report

    }
    public class SummaryControlReport
    {
        public Guid BatchId { get; set; }
        public string BatchClustId { get; set; }
        public DateTime ReceiveDate { get; set; }
        public string BatchDocumentPackageName
        {
            get
            {
                return BatchDocumentPackageName.IsNullOrWhiteSpace() ? MailPieceDocumentPackageName : BatchDocumentPackageName;
            }
            set;
        }
        public string BatchName { get; set; }
        public string PipelineStatus { get; set; }
        public int ClientId { get; set; }
        public Guid MailPieceId { get; set; }
        public string LoanNumber { get; set; }
        public string FileName { get; set; }
        public string BusinessFriendlyIdName { get; set; }
        public DateTime? PrintedOn { get; set; }
        public DateTime? MailedOn { get; set; }
        public int TrackingNo { get; set; }
        public string RcptPrefix { get; set; }
        public string RcptFirstName { get; set; }
        public string RcptMiddleName { get; set; }
        public string RcptLastName { get; set; }
        public string RcptSuffix { get; set; }
        public string RcptFullName
        {
            get
            {
                return string.Format("{0}{1}{2}{3}{4}", RcptPrefix, RcptFirstName, RcptMiddleName, RcptLastName, RcptSuffix);
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
        public string RcptAddressZIP { get; set; }
        public string RcptFullAddress
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
                    RcptAddressZIP.IsNullOrWhiteSpace() ? null : RcptAddressZIP.Trim() + ",");
                return address.IsNullOrWhiteSpace() ? address : address.Trim().Trim(',');
            }
        }
        public string MailPieceDocumentPackageName { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDesc { get; set; }
        public int TotalPages { get; set; }
        public string CustomerIdentifier { get; set; }
        public string ShippingMethod { get; set; }
    }
    public class DocumentType
    {
        public int Id { get; set; }
        public string DocTypeCode { get; set; }
    }
    public class ErrorReport
    {
        public Guid BatchId { get; set; }
        public string BatchClustId { get; set; }
        public DateTime ReceiveDate { get; set; }
        public string BatchDocumentPackageName {
            get
            {
                return BatchDocumentPackageName.IsNullOrWhiteSpace() ? MailPieceDocumentPackageName : BatchDocumentPackageName;
            }
            set; }
        public string BatchName { get; set; }
        public string PipelineStatus { get; set; }
        public int ClientId { get; set; }
        public Guid MailPieceId { get; set; }
        public string LoanNumber { get; set; }
        public string FileName { get; set; }
        public string BusinessFriendlyIdName { get; set; }
        public DateTime? PrintedOn { get; set; }
        public DateTime? MailedOn { get; set; }
        public int TrackingNo { get; set; }
        public string RcptPrefix { get; set; }
        public string RcptFirstName { get; set; }
        public string RcptMiddleName { get; set; }
        public string RcptLastName { get; set; }
        public string RcptSuffix { get; set; }
        public string RcptFullName
        {
            get
            {
                return string.Format("{0}{1}{2}{3}{4}", RcptPrefix, RcptFirstName, RcptMiddleName, RcptLastName, RcptSuffix);
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
        public string RcptAddressZIP { get; set; }
        public string RcptFullAddress
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
                    RcptAddressZIP.IsNullOrWhiteSpace() ? null : RcptAddressZIP.Trim() + ",");
                return address.IsNullOrWhiteSpace() ? address : address.Trim().Trim(',');
            }
        }
        public string MailPieceDocumentPackageName { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDesc { get; set; }
        public int TotalPages { get; set; }
        public string CustomerIdentifier { get; set; }
        public string ShippingMethod { get; set; }

    }

    public class DeliveryReport
    {
        public Guid BatchId { get; set; }
        public string BatchClustId { get; set; }
        public DateTime ReceiveDate { get; set; }
        public string BatchDocumentPackageName {
            get
            {
                return BatchDocumentPackageName.IsNullOrWhiteSpace() ? MailPieceDocumentPackageName : BatchDocumentPackageName;
            }
            set; }
        public int ClientId { get; set; }
        public Guid MailPieceId { get; set; }
        public string LoanNumber { get; set; }
        public string FileName { get; set; }
        public string BusinessFriendlyIdName { get; set; }
        public DateTime? PrintedOn { get; set; }
        public DateTime? MailedOn { get; set; }
        public int TrackingNo { get; set; }
        public string RcptPrefix { get; set; }
        public string RcptFirstName { get; set; }
        public string RcptMiddleName { get; set; }
        public string RcptLastName { get; set; }
        public string RcptSuffix { get; set; }
        public string RcptFullName
        {
            get
            {
                return string.Format("{0}{1}{2}{3}{4}", RcptPrefix, RcptFirstName, RcptMiddleName, RcptLastName, RcptSuffix);
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
        public string RcptAddressZIP { get; set; }
        public string RcptFullAddress
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
                    RcptAddressZIP.IsNullOrWhiteSpace() ? null : RcptAddressZIP.Trim() + ",");
                return address.IsNullOrWhiteSpace() ? address : address.Trim().Trim(',');
            }
        }
        public string MailPieceDocumentPackageName { get; set; }
        public int TotalPages { get; set; }
        public string CustomerIdentifier { get; set; }
        public string ShippingMethod { get; set; }
    }
    public class SupplyInventory
    {
        public DateTime ReceivedOn { get; set; }
        public string InventoryId { get; set; }
        public string EnvelopType { get; set; }
        public string Description { get; set; }
        public decimal UnitPrice { get; set; }
        public int StockReceivedQuantity { get; set; }
        public int StockUsed { get; set; }
        public int CurrentlyInStock { get; set; }
    }
}
