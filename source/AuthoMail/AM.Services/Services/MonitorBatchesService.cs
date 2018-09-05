using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace AM.Services
{
    public interface IMonitorBatchesService : IDisposable
    {
        List<dynamic> CheckSFTChannelsOfPendingBatches();
        List<dynamic> GetPendingBatchesToSubmit();
        List<dynamic> GetPendingSweepBatchesToSubmit();
        List<dynamic> GetPendingBatchesToReportBack();
        void SaveEmailSentTimeStamp(int printBatch, bool IsSubmitted);
        int CheckPrintBatchIdExist(int printBatch, bool IsSubmitted);
        int CheckLastEmailSentTime(int printBatch, bool IsSubmitted);
        void UpdateEmailTimeStamp(int printBatch, bool IsSubmitted);
    }
    public class MonitorBatchesService : ServiceBase, IMonitorBatchesService
    {
        public List<dynamic> CheckSFTChannelsOfPendingBatches()
        {
            return db.Query("SELECT X.PrintBatchId,X.AppCode,X.PrintLocation,BS.PrintFileName,BS.PipelineStatus,X.ReceivedOn,X.DownloadedOn,X.PreppedOn,X.SFTChannelId ,BS.IsSubmitted,BS.IsReportedBack FROM (SELECT DISTINCT						  P.PrintBatchId,PB.UniqueId,B.PipelineStatus,B.ReceivedOn,B.DownloadedOn,B.PreppedOn,PB.AppCode,B.SFTChannelId,P.PrintLocation FROM	AM.MailPieces P  INNER JOIN AM.MailBatches B ON B.Id=P.MailBatchID INNER JOIN AM.PrintBatches PB ON PB.Id=P.PrintBatchId)X LEFT OUTER JOIN PnS.BatchSummary BS ON X.UniqueId = BS.BatchIdentifier WHERE (ISNULL(BS.IsSubmitted,0) = 0) AND DATEDIFF(minute, CASE WHEN DATENAME(DW, X.ReceivedOn) = 'Sunday' THEN X.DownloadedOn ELSE X.ReceivedOn END, ISNULL(BS.SubmittedOn,GETUTCDATE()) ) > 20").ToList();
        }

        public List<dynamic> GetPendingBatchesToSubmit()
        {
            //return db.Query("SELECT PB.PrintBatch,CC.AppCode,PB.PrintFileName,PB.Pipelinestatus,PB.ReceivedOn,PB.DownloadedOn,PB.PreppedOn FROM PnS.BatchSummary PB INNER JOIN Common.Clients CC ON CC.Id = PB.ClientId WHERE PB.IsSubmitted = 0 AND PB.IsReportedBack = 0 AND DATEDIFF(minute, ReceivedOn, GETUTCDATE()) > 15").ToList();
            return db.Query("SELECT X.PrintBatchId,X.AppCode,BS.PrintFileName,X.PrintLocation,BS.PipelineStatus,X.ReceivedOn,X.DownloadedOn,X.PreppedOn,X.SFTChannelId ,BS.IsSubmitted,BS.IsReportedBack FROM(SELECT DISTINCT P.PrintBatchId,PB.UniqueId,B.ReceivedOn,B.DownloadedOn,B.PreppedOn,PB.AppCode,B.SFTChannelId,P.PrintLocation  FROM AM.MailPieces P  INNER JOIN AM.MailBatches B ON B.Id=P.MailBatchID INNER JOIN AM.PrintBatches PB ON PB.Id=P.PrintBatchId )X LEFT OUTER JOIN PnS.BatchSummary BS ON X.UniqueId = BS.BatchIdentifier WHERE (ISNULL(BS.IsSubmitted,0) = 0 AND ISNULL(BS.IsReportedBack,0) = 0) AND DATEDIFF(minute, CASE WHEN DATENAME(DW, X.ReceivedOn) = 'Sunday' THEN X.DownloadedOn ELSE X.ReceivedOn END, ISNULL(BS.SubmittedOn,GETUTCDATE()) ) > 20").ToList();

        }

        public List<dynamic> GetPendingSweepBatchesToSubmit()
        {
            return db.Query("SELECT X.PrintBatchId,X.AppCode,BS.PrintFileName,X.PrintLocation,BS.PipelineStatus,X.ReceivedOn,X.DownloadedOn,X.PreppedOn,X.SFTChannelId ,BS.IsSubmitted,BS.IsReportedBack FROM(SELECT DISTINCT P.PrintBatchId,PB.UniqueId,B.ReceivedOn,B.DownloadedOn,B.PreppedOn,B.SubmittedToShippingOn,PB.AppCode,B.SFTChannelId,P.PrintLocation  FROM AM.MailPieces P  INNER JOIN AM.MailBatches B ON B.Id=P.MailBatchID INNER JOIN AM.PrintBatches PB ON PB.Id=P.PrintBatchId )X LEFT OUTER JOIN PnS.BatchSummary BS ON X.UniqueId = BS.BatchIdentifier WHERE (ISNULL(BS.IsSubmitted,0) = 0 AND ISNULL(BS.IsReportedBack,0) = 0) AND DATEDIFF(minute, X.SubmittedToShippingOn, ISNULL(BS.SubmittedOn,GETUTCDATE()) ) > 20").ToList();
        }        

        public List<dynamic> GetPendingBatchesToReportBack()
        {
            //return db.Query("SELECT PB.PrintBatch,CC.AppCode,PB.PrintFileName,PB.Pipelinestatus,PB.ReceivedOn,PB.DownloadedOn,PB.PreppedOn,PB.SubmittedOn FROM PnS.BatchSummary PB LEFT OUTER JOIN AM.PrintBatches AP ON AP.UniqueId = PB.BatchIdentifier INNER JOIN Common.Clients CC ON CC.Id = PB.ClientId WHERE PB.IsSubmitted = 1 AND (PB.IsReportedBack = 0 OR AP.IsShippingResponseReceived = 0) AND DATEDIFF(HOUR, ReceivedOn, GETUTCDATE()) > 10 AND PB.PipelineStatus <> 'Cancelled'").ToList();
            return db.Query("SELECT X.PrintBatchId,X.AppCode,BS.PrintFileName,BS.PipelineStatus,X.ReceivedOn,X.DownloadedOn,BS.PreppedOn,BS.SubmittedOn,X.PrintLocation,X.IsShippingResponseReceived,BS.IsReportedBack FROM(	SELECT DISTINCT P.PrintBatchId,PB.UniqueId,B.ReceivedOn,B.DownloadedOn,B.PreppedOn,PB.AppCode,P.PrintLocation,PB.IsShippingResponseReceived FROM AM.MailPieces P  INNER JOIN AM.MailBatches B ON B.Id=P.MailBatchID INNER JOIN AM.PrintBatches PB ON PB.Id=P.PrintBatchId)X LEFT OUTER JOIN PnS.BatchSummary BS ON X.UniqueId = BS.BatchIdentifier WHERE BS.IsSubmitted = 1 AND (BS.IsReportedBack = 0 OR X.IsShippingResponseReceived = 0) AND DATEDIFF(HOUR, X.ReceivedOn, GETUTCDATE()) > 10 AND BS.PipelineStatus <> 'Cancelled'").ToList();
        }

        public void SaveEmailSentTimeStamp(int printBatch, bool IsSubmitted)
        {
            db.Execute("INSERT INTO [Common].[MonitorBatches] ([PrintBatch],[Email_TimeStamp],[IsSubmitted]) VALUES (@PrintBatch,@Email_TimeStamp,@IsSubmitted)", new { @PrintBatch = printBatch, @Email_TimeStamp = DateTime.UtcNow, @IsSubmitted = IsSubmitted });
        }
        public int CheckPrintBatchIdExist(int printBatch, bool IsSubmitted)
        {
            return Convert.ToInt32(db.ExecuteScalar("SELECT Count(PrintBatch) FROM Common.MonitorBatches WHERE PrintBatch = @PrintBatch AND IsSubmitted = @IsSubmitted", new { @PrintBatch = printBatch, @IsSubmitted = IsSubmitted }).ToString());
        }
        public int CheckLastEmailSentTime(int printBatch, bool IsSubmitted)
        {
            return Convert.ToInt32(db.ExecuteScalar("SELECT DATEDIFF(hour, Email_TimeStamp, GETUTCDATE()) AS EmailTimeStamp FROM Common.MonitorBatches WHERE PrintBatch = @PrintBatch AND IsSubmitted = @IsSubmitted", new { @PrintBatch = printBatch, @IsSubmitted = IsSubmitted }).ToString());
        }
        public void UpdateEmailTimeStamp(int printBatch, bool IsSubmitted)
        {
            db.Execute("UPDATE Common.MonitorBatches SET Email_TimeStamp = GETUTCDATE(),IsSubmitted = @IsSubmitted WHERE PrintBatch = @PrintBatch", new { @PrintBatch = printBatch, @IsSubmitted = IsSubmitted });
        }

        
    }
}
