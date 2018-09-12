using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AM.Services;
using AM.Lib;

namespace AM.Services
{
    public interface ISupportReportService : IDisposable
    {
        List<NotReportedBackMailPieces> GetNotReportedBackMailPieces(int clientId, string printLocation, DateTime date);
        List<MailPieceByPrintBatch> GetMailPiecebyPrintBatch(int printBatch, int clientId);
        List<NotReportedBackMailPieces> GetBatchsummarybyPrintBatch(int printBatch, int clientId);
    }
    public class SupportReportService : ServiceBase, ISupportReportService
    {
        public List<NotReportedBackMailPieces> GetNotReportedBackMailPieces(int clientId, string printLocation, DateTime date)
        {
            date = clientId != 104 ? date.Date + new TimeSpan(20, 00, 00) : date.Date + new TimeSpan(20, 15, 00);
            date = date.EstToUtc();
            string query = "select bs.PrintBatch,bs.PrintLocation,bs.SubmittedOn,bs.JobName,bs.PrintFileName,bs.PipelineStatus, c.AppCode as ClientCode,d.DocTypeCode, d.ShippingCarrier,d.ShippingServiceType,Count(m.Id) as TotalPackages,Sum(m.TotalPages) as TotalPages from PnS.MRDFRequest rq inner join PnS.BatchSummary bs on rq.JobId=bs.PrintBatch inner join Common.Clients c on bs.ClientId=c.Id inner join AM.DocTypes d on bs.DocTypeId=d.Id inner join AM.MailPieces m on rq.UniqueId=m.UniqueId where c.Id=@clientId and bs.PrintLocation=ISNULL(@printLocation,bs.PrintLocation) and bs.PipelineStatus not in ('ReportedBack','Cancelled') and bs.SubmittedOn <= @date group by bs.PrintBatch,bs.PrintLocation,bs.SubmittedOn,bs.JobName,bs.PrintFileName,bs.PipelineStatus, c.AppCode,d.DocTypeCode, d.ShippingCarrier,d.ShippingServiceType";
            return db.Query<NotReportedBackMailPieces>(query,
                new
                {
                    ClientId = clientId,
                    PrintLocation = printLocation,
                    date = date
                }).ToList();
        }


        public List<MailPieceByPrintBatch> GetMailPiecebyPrintBatch(int printBatch, int clientId)
        {
            string query = "select rq.PieceId,rq.PDFFileName,rq.UniqueId,rq.MailPieceStatus,rs.ProcessingTimeStamp,rs.Disposition from PnS.BatchSummary bs inner join [PnS].[MRDFRequest] rq on bs.PrintBatch=rq.JobId left outer join [PnS].[MRDFResponse] rs on rq.PieceId=rs.PieceId and rq.JobId=rs.JobId where bs.ClientId=@clientId and bs.PrintBatch=@printBatch";
            return db.Query<MailPieceByPrintBatch>(query, new { printBatch = printBatch, clientId = clientId }).ToList();
        }


        public List<NotReportedBackMailPieces> GetBatchsummarybyPrintBatch(int printBatch, int clientId)
        {
            string query = "select bs.PrintBatch,bs.PrintLocation,bs.SubmittedOn,bs.JobName,bs.PrintFileName,bs.PipelineStatus, c.AppCode as ClientCode,d.DocTypeCode, d.ShippingCarrier,d.ShippingServiceType,Count(m.Id) as TotalPackages,Sum(m.TotalPages) as TotalPages from PnS.MRDFRequest rq inner join PnS.BatchSummary bs on rq.JobId=bs.PrintBatch inner join Common.Clients c on bs.ClientId=c.Id inner join AM.DocTypes d on bs.DocTypeId=d.Id inner join AM.MailPieces m on rq.UniqueId=m.UniqueId where c.Id=@clientId and bs.PrintBatch=@printBatch group by bs.PrintBatch,bs.PrintLocation,bs.SubmittedOn,bs.JobName,bs.PrintFileName,bs.PipelineStatus, c.AppCode,d.DocTypeCode, d.ShippingCarrier,d.ShippingServiceType";
            return db.Query<NotReportedBackMailPieces>(query,
                new
                {
                    ClientId = clientId,
                    printBatch = printBatch
                }).ToList();
        }
    }
}
