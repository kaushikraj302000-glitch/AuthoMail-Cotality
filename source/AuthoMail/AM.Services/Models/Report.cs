using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;

namespace AM.Services
{
    public class DocumentType
    {
        public int Id { get; set; }
        public string DocTypeName { get; set; }
    }
    public class ChannelCodeTitle
    {
        public string Code { get; set; }
        public string Title { get; set; }
    }
    public class ClientIdName
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AppCode { get; set; }
        public string GroupAlias { get; set; }
    }

    public class DallasBatch
    {
        public string AppCode { get; set; }
        public string BatchName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ShippingCarrier { get; set; }
        public string ShippingServiceType { get; set; }
    }

    public class DallasBatchTiming
    {
        public string BatchName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string ShippingCarrier { get; set; }
        public string ShippingServiceType { get; set; }
    }

    public class DallasMailPieceData
    {
        public int ClientId { get; set; }
        public string AppCode { get; set; }
        public string ClientName { get; set; }
        public string LoanNumber { get; set; }
        public string PrintLocation { get; set; }
        public DateTime EffectiveReceivedOnEst { get; set; }
        public string TrackingNo { get; set; }
        public bool IsPullBackRequested { get; set; }
        public bool IsQCRequested { get; set; }
        public int TotalSheets { get; set; }
        public DateTime DueDate { get; set; }
        public int TotalPages { get; set; }
        public int PrintBatch { get; set; }
        public bool IsSubmitted { get; set; }
        public string PipelineStatus { get; set; }
        public string JobName { get; set; }
        public DateTime SubmittedOn { get; set; }
        public int PieceId { get; set; }
        public DateTime MRDFReqDate { get; set; }
        public string PDFFileName { get; set; }
        public Guid UniqueId { get; set; }
        public string ShippingStatus { get; set; }
        public string MailPieceStatus { get; set; }
        public DateTime ProcessingTimeStamp { get; set; }
        public string ShippingCarrier { get; set; }
        public string ShippingServiceType { get; set; }
        public bool IsBRE { get; set; }
    }

    public class GOSMailPieceData
    {
        public string AppCode { get; set; }
        public string BatchName { get; set; }
        public DateTime ReceivedOnEst { get; set; }
        public string LoanNumber { get; set; }
        public string DocumentOrPackageType { get; set; }
        public string FileName { get; set; }
        public string TrackingNo { get; set; }
        public int TotalPages { get; set; }
        public string ProductCode { get; set; }
        public string ShippingCarrier { get; set; }
        public string ShippingStatus { get; set; }
        public string ShipmentStatus { get; set; }
        public string DeliveryStatus { get; set; }
        public DateTime DeliveryDateTime { get; set; }
        public DateTime ShippedOnEst { get; set; }
        public string PrintLocation { get; set; }
        public string ErrorDesc { get; set; }
        public string ReturnTrackingNo { get; set; }
        public string ReturnDeliveryStatus { get; set; }
        public DateTime? ReturnDeliveryDate { get; set; }
        public string ReturnShippingCarrier
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ReturnTrackingNo))
                {
                    if (ReturnTrackingNo.StartsWith("7") || ReturnTrackingNo.StartsWith("3"))
                    {
                        return "FEDEX";
                    }
                    else
                    {
                        return "USPS";
                    }
                }

                return null;
            }
        }
    }
    public class ReportWizard
    {
        public int MailPieceId { get; set; }
        public string BatchId { get; set; }
        public string Channel { get; set; }
        public string DocumentOrPackageType { get; set; }
        public bool IsEDeliveryRequested { get; set; }
        //public string BusinessIdName { get; set; }
        //public string BusinessIdValue { get; set; }
        public string LoanNumber { get; set; }
        public string TrackingNo { get; set; }
        public DateTime? ReceivedOn { get; set; }
        public DateTime? PrintedOn { get; set; }
        public DateTime? MailedOn { get; set; }
        public string FileName { get; set; }
        public string ClientCode { get; set; }
        public string ShippingMethod { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string RcptAddressStreet { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZIP { get; set; }
        public string PrintLocation { get; set; }
        public int PagesShipped { get; set; }
        public int BlankPages { get; set; }
        public string Status { get; set; }
        public string Inserts { get; set; }
        public string PrintFileName { get; set; }
        public string EnvelopeName { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string DeliveryStatus { get; set; }
    }
}
