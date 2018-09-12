using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class NotReportedBackMailPieces
    {
        public int PrintBatch { get; set; }
        public string PrintLocation { get; set; }
        public DateTime? SubmittedOn { get; set; }
        public string JobName { get; set; }
        public string PrintFileName { get; set; }
        public string PipelineStatus { get; set; }
        public string ClientCode { get; set; }
        public string DocTypeCode { get; set; }
        public string ShippingCarrier { get; set; }
        public string ShippingServiceType { get; set; }
        public int TotalPackages { get; set; }
        public int TotalPages { get; set; }
    }
    public class MailPieceByPrintBatch
    {
        public int PieceId { get; set; }
        public string PDFFileName { get; set; }
        public Guid UniqueId { get; set; }
        public string MailPieceStatus { get; set; }
        public string ProcessingTimeStamp { get; set; }
        public string Disposition { get; set; }
        public int MailPieceId { get; set; }
    }
    
}
