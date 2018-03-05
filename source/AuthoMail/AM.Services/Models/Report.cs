using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class DailyDetailOperationsReport
    {
        public int BatchId { get; set; }
        public Guid BatchUniqueId { get; set; }
        public int MailPieceId { get; set; }
        public Guid MailPieceUniqueId { get; set; }
        public string ClientBatchId { get; set; }
        public string Channel { get; set; }
        public string DocumentOrPackageName { get; set; }
        public string BusinessFriendlyIdName { get; set; }
        public string BusinessFriendlyIdValue { get; set; }
        public string FileName { get; set; }
        public DateTime ReceiveOn { get; set; }
        public DateTime? PrintedOn { get; set; }
        public DateTime? MailedOn { get; set; }
        public int ClientId { get; set; }
        public string ClientCode { get; set; }
        public string TrackingNo { get; set; }
        public string ShippingMethod { get; set; }
        public string CustomerIdentifier { get; set; }

        public string RcptFirstName { get; set; }

        public string RcptLastName { get; set; }


        public string RcptAddressStreet { get; set; }

        public string RcptAddressCity { get; set; }
        public string RcptAddressState { get; set; }
        public string RcptAddressZIP { get; set; }

        public string RequestId { get; set; }
        public string PrintLocation { get; set; }
        public int PagesShipped { get; set; }
    }

    public class ErrorReport
    {
        public int BatchId { get; set; }
        public Guid BatchUniqueId { get; set; }
        public int MailPieceId { get; set; }
        public Guid MailPieceUniqueId { get; set; }
        public string ClientBatchId { get; set; }
        public string Channel { get; set; }
        public string DocumentOrPackageName { get; set; }
        public string BusinessFriendlyIdName { get; set; }
        public string BusinessFriendlyIdValue { get; set; }
        public int SLADays { get; set; }
        public DateTime? DueDate { get; set; }
        public string FileName { get; set; }
        public DateTime ReceiveOn { get; set; }
        public int ClientId { get; set; }
        public string ShippingMethod { get; set; }
        public string CustomerIdentifier { get; set; }
        public string RcptFirstName { get; set; }
        public string RcptLastName { get; set; }
        public string RcptAddressStreet { get; set; }
        public string RcptAddressCity { get; set; }
        public string RcptAddressState { get; set; }
        public string RcptAddressZIP { get; set; }
        public string RequestId { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDesc { get; set; }
    }

    public class MonthlyReconReport
    {
        public int MailPieceId { get; set; }
        public Guid MailPieceUniqueId { get; set; }
        public string ClientBatchId { get; set; }
        public string Channel { get; set; }
        public string DocumentOrPackageName { get; set; }
        public string BusinessFriendlyIdName { get; set; }
        public string BusinessFriendlyIdValue { get; set; }
        public string FileName { get; set; }
        public DateTime ReceiveOn { get; set; }
        public DateTime? PrintedOn { get; set; }
        public DateTime? MailedOn { get; set; }
        public int ClientId { get; set; }
        public string TrackingNo { get; set; }
        public string ShippingMethod { get; set; }
        public string RcptFirstName { get; set; }
        public string RcptLastName { get; set; }
        public string RcptOrganization { get; set; }
        public string RcptAddressStreet { get; set; }
        public string RcptAddressCity { get; set; }
        public string RcptAddressState { get; set; }
        public string RcptAddressZIP { get; set; }
        public string PrintLocation { get; set; }
        public int PagesShipped { get; set; }
        public string ShippingStatus { get; set; }
    }
    public class SummaryControlReport
    {
        public int BatchId { get; set; }
        public Guid BatchUniqueId { get; set; }
        public int MailPieceId { get; set; }
        public Guid MailPieceUniqueId { get; set; }
        public string Channel { get; set; }//Todo
        public string BatchDocumentOrPackageType { get; set; }
        public string MailPieceDocumentOrPackageType { get; set; }
        public string DocumentOrPackageName
        {
            get
            {
                return BatchDocumentOrPackageType.IsNullOrWhiteSpace() ? MailPieceDocumentOrPackageType : BatchDocumentOrPackageType;
            }
        }
        public string BusinessFriendlyIdName { get; set; }
        public string BusinessFriendlyIdValue { get; set; }
        public string FileName { get; set; }
        public DateTime ReceiveOn { get; set; }
        public DateTime? PrintedOn { get; set; }
        public DateTime? MailedOn { get; set; }
        public int ClientId { get; set; }
        public string ClientCode { get; set; }
        public string TrackingNo { get; set; }
        public string ShippingMethod { get; set; }
        public string CustomerIdentifier { get; set; }
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
        public string RequestId { get; set; }//ToDo
        public string ErrorCode { get; set; }
        public string ErrorDesc { get; set; }
        public string PrintLocation { get; set; }//ToDo
        public int PagesShipped { get; set; }
    }
    public class DocumentType
    {
        public int Id { get; set; }
        public string DocTypeCode { get; set; }
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
