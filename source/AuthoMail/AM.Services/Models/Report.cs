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
        public string DocumentOrPackageType { get; set; }
        public string BusinessFriendlyIdName { get; set; }
        public string BusinessFriendlyIdValue { get; set; }
        public string FileName { get; set; }
        public DateTime ReceivedOn { get; set; }
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
        public string DocumentOrPackageType { get; set; }
        public string BusinessFriendlyIdName { get; set; }
        public string BusinessFriendlyIdValue { get; set; }
        public int SLADays { get; set; }
        public DateTime? DueDate { get; set; }
        public string FileName { get; set; }
        public DateTime ReceivedOn { get; set; }
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
        public string DocumentOrPackageType { get; set; }
        public string BusinessFriendlyIdName { get; set; }
        public string BusinessFriendlyIdValue { get; set; }
        public string FileName { get; set; }
        public DateTime ReceivedOn { get; set; }
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
        public string ClientBatchId { get; set; }
        public string Channel { get; set; }
        public string DocumentOrPackageType { get; set; }
        public string BusinessFriendlyIdName { get; set; }
        public string BusinessFriendlyIdValue { get; set; }
        public string FileName { get; set; }
        public DateTime ReceivedOn { get; set; }
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
        public string ErrorCode { get; set; }
        public string ErrorDesc { get; set; }
        public string PrintLocation { get; set; }
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
