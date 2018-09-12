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
        List<NotReportedBackMailPieces> GetBatchSummarybyDate(DateTime date, int clientId);
        List<MailPieceByPrintBatch> GetMailPieceWithBatchSummaryByPrintBatch(int printBatch, int clientId);
        List<MailBatchesSupport> GetMailBatchesbyDate(DateTime date, int clientId);
        List<MailPieceByPrintBatch> GetMailPieceByBatchId(int batchId, int clientId);

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


        public List<NotReportedBackMailPieces> GetBatchSummarybyDate(DateTime date, int clientId)
        {
            date = clientId != 104 ? date.Date + new TimeSpan(20, 00, 00) : date.Date + new TimeSpan(20, 15, 00);
            date = date.EstToUtc();
            string query = "select bs.PrintBatch,bs.PrintLocation,bs.SubmittedOn,bs.JobName,bs.PrintFileName,bs.PipelineStatus, c.AppCode as ClientCode,d.DocTypeCode, d.ShippingCarrier,d.ShippingServiceType,Count(m.Id) as TotalPackages,Sum(m.TotalPages) as TotalPages from PnS.MRDFRequest rq inner join PnS.BatchSummary bs on rq.JobId=bs.PrintBatch inner join Common.Clients c on bs.ClientId=c.Id inner join AM.DocTypes d on bs.DocTypeId=d.Id inner join AM.MailPieces m on rq.UniqueId=m.UniqueId where c.Id=@clientId and bs.SubmittedOn <= @date group by bs.PrintBatch,bs.PrintLocation,bs.SubmittedOn,bs.JobName,bs.PrintFileName,bs.PipelineStatus, c.AppCode,d.DocTypeCode, d.ShippingCarrier,d.ShippingServiceType";
            return db.Query<NotReportedBackMailPieces>(query,
                new
                {
                    ClientId = clientId,
                    date = date
                }).ToList();
        }


        public List<MailPieceByPrintBatch> GetMailPieceWithBatchSummaryByPrintBatch(int printBatch, int clientId)
        {
            string query = "select rq.PieceId,rq.PDFFileName,rq.UniqueId,rq.MailPieceStatus,rs.ProcessingTimeStamp,rs.Disposition,m.Id as MailPieceId,m.BusinessFriendlyIdValue as LoanNumber,m.DocumentOrPackageType,m.EffectiveReceivedOnEst, d.ShippingCarrier,d.ShippingServiceType,m.ShippingStatus,m.ErrorCode,m.ErrorDesc,m.PrintedOn,m.ShippedOn,m.TotalPages,m.PrintLocation from PnS.BatchSummary bs inner join [PnS].[MRDFRequest] rq on bs.PrintBatch=rq.JobId left outer join PnS.MRDFResponse rs on rq.PieceId=rs.PieceId and rq.JobId=rs.JobId inner join AM.MailPieces m on rq.UniqueId=m.UniqueId inner join AM.DocTypes d on m.DocTypeId=d.Id where bs.ClientId=@clientId and bs.PrintBatch=@printBatch";
            return db.Query<MailPieceByPrintBatch>(query, new { printBatch = printBatch, clientId = clientId }).ToList();
        }


        public List<MailBatchesSupport> GetMailBatchesbyDate(DateTime date, int clientId)
        {
            string query = "select mb.Id as MailBatchId,mb.BatchName as MailBatchName,c.Name as Client,mb.PipelineStatus,mb.BatchStatus, mb.ValidationStatus,mb.ValidationErrors,c.AppCode,mb.ReceivedOn,mb.DownloadedOn from AM.MailBatches mb inner join Common.Clients c on mb.ClientId=c.Id inner join AM.MailBatches mb on m.MailBatchId=mb.Id where c.Id=@clientId and cast(mb.ReceivedOn as date) = cast(@date as date)";
            return db.Query<MailBatchesSupport>(query, new { date = date, clientId = clientId }).ToList();
        }


        public List<MailPieceByPrintBatch> GetMailPieceByBatchId(int batchId, int clientId)
        {
            string query = "select m.Id as MailPieceId,m.BusinessFriendlyIdValue as LoanNumber,m.DocumentOrPackageType,m.EffectiveReceivedOnEst,d.ShippingCarrier,d.ShippingServiceType,b.PrintBatchId,b.PipelineStatus,m.ErrorCode,m.ErrorDesc,m.PrintedOn,m.ShippedOn,m.TotalPages,m.PrintLocation from AM.MailPieces m inner join AM.DocTypes d on m.DocTypeId=d.Id inner join PnS.MRDFRequest r on m.UniqueId=r.UniqueId inner join PnS.BatchSummary b on  r.JobId=b.PrintBatch where m.ClientId=@clientId and m.MailBatchId=@batchId";
            return db.Query<MailPieceByPrintBatch>(query, new { batchId = batchId, clientId = clientId }).ToList();
        }
    }
}
