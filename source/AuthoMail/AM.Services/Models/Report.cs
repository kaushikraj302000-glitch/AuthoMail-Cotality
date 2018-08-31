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
        public string DocTypeCode { get; set; }
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
    }

    public class DallasBatch
    {
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
        public string MailPieceStatus { get; set; }
        public DateTime ProcessingTimeStamp { get; set; }
        public string ShippingCarrier { get; set; }
        public string ShippingServiceType { get; set; }
    }
}
