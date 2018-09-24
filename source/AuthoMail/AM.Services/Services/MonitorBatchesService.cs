using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AM.Lib;
using AM.Entities;
using AM.Services.Services;

namespace AM.Services
{
    public interface IMonitorBatchesService : IDisposable
    {
        List<dynamic> CheckSFTChannelsOfPendingBatches();
        MonitorReport GetPendingBatchesToSubmit(int cutOffMinutes);
        List<dynamic> GetPendingSweepBatchesToSubmit();
        List<dynamic> GetPendingBatchesToReportBack();
        void SaveEmailSentTimeStamp(int mailBatchId, bool IsSubmitted);
        int CheckPrintBatchIdExist(int mailBatchId, bool IsSubmitted);
        int CheckLastEmailSentTime(int mailBatchId, bool IsSubmitted);
        void UpdateEmailTimeStamp(int mailBatchId, bool IsSubmitted);
    }
    public class MonitorBatchesService : ServiceBase, IMonitorBatchesService
    {
        public List<dynamic> CheckSFTChannelsOfPendingBatches()
        {
            return db.Query("SELECT X.PrintBatchId,X.AppCode,X.PrintLocation,BS.PrintFileName,BS.PipelineStatus,X.ReceivedOn,X.DownloadedOn,X.PreppedOn,X.SFTChannelId ,BS.IsSubmitted,BS.IsReportedBack FROM (SELECT DISTINCT						  P.PrintBatchId,PB.UniqueId,B.PipelineStatus,B.ReceivedOn,B.DownloadedOn,B.PreppedOn,PB.AppCode,B.SFTChannelId,P.PrintLocation FROM	AM.MailPieces P  INNER JOIN AM.MailBatches B ON B.Id=P.MailBatchID INNER JOIN AM.PrintBatches PB ON PB.Id=P.PrintBatchId)X LEFT OUTER JOIN PnS.BatchSummary BS ON X.UniqueId = BS.BatchIdentifier WHERE (ISNULL(BS.IsSubmitted,0) = 0) AND DATEDIFF(minute, CASE WHEN DATENAME(DW, X.ReceivedOn) = 'Sunday' THEN X.DownloadedOn ELSE X.ReceivedOn END, ISNULL(BS.SubmittedOn,GETUTCDATE()) ) > 20").ToList();
        }

        public MonitorReport GetPendingBatchesToSubmit(int cutOffMinutes)
        {

            string qry = "[dbo].[uspalert_GetDelayedBatchesToSubmit] @cutOffMinutes";
            var timings = db.Query<Sweep>("select * from ufn_GetPCIBundleSweepTimings()").ToList();
            var pendingMPsToSubmit = db.Query<PendingMailPieceInfo>(qry, new { cutOffMinutes = cutOffMinutes }).ToList();

            var shippingStatusNullMailPieces = pendingMPsToSubmit.Where(p => p.ShippingStatus.IsNullOrEmpty()).ToList();
            var clientIdNullMailPieces = pendingMPsToSubmit.Where(p => !p.ClientId.HasValue).ToList();
            var delayedErrorMailPieces = pendingMPsToSubmit.Where(p => p.ShippingStatus == ShippingStatus.Error.ToString()).ToList();
            var delayedPulledBackMailPieces = pendingMPsToSubmit.Where(p => p.ShippingStatus == ShippingStatus.PulledBack.ToString());
            var delayedShippedMailPieces = pendingMPsToSubmit.Where(m => (m.ShippingStatus == ShippingStatus.Shipped.ToString()));
            var pendingInProgressMailPieces = pendingMPsToSubmit.Where(m => (m.ShippingStatus == ShippingStatus.InProgress.ToString()));
            var delayedEmptyPrintLocation = pendingInProgressMailPieces.Where(p => p.PrintLocation.IsNullOrEmpty()).ToList();

            var delayedDallasMailPieces = pendingInProgressMailPieces.Where(p => p.PrintLocation == Config.Common_PrintLocationCL).ToList();

            var pendingPCIMailPieces = pendingInProgressMailPieces.Where(p => p.PrintLocation == Config.Common_PrintLocationPCI).ToList();

            var delayedPCIMailPieces = pendingInProgressMailPieces.Where(mp => mp.PrintLocation == Config.Common_PrintLocationPCI && Common.Now > mp.GetEstimatedSubmittedOn(timings, cutOffMinutes + 15)).ToList();

            var delayedMailPieces = delayedDallasMailPieces.Concat(delayedEmptyPrintLocation).Concat(delayedShippedMailPieces).Concat(delayedPulledBackMailPieces).Concat(delayedErrorMailPieces).Concat(clientIdNullMailPieces).Concat(shippingStatusNullMailPieces).Concat(delayedPCIMailPieces).ToList();

            var delayedBatches = delayedMailPieces
                .GroupBy(mp => new { mp.Id, mp.AppCode, mp.BatchName, mp.ClientId, mp.PipelineStatus, mp.ReceivedOn })
                .Select(g => new BatchInfo { Id = g.Key.Id, AppCode = g.Key.AppCode, BatchName = g.Key.BatchName, ClientId = g.Key.ClientId, PipelineStatus = g.Key.PipelineStatus, ReceivedOn = g.Key.ReceivedOn, DelayedMailPieces = g.ToList() }).ToList();

            return new MonitorReport { DelayedBatches = delayedBatches, DelayedMailPieces = delayedMailPieces };
        }


        public List<PendingMailPieceInfo> EmptyPrintLocationPendingBatches(List<PendingMailPieceInfo> pendingEmptyPrintLocation)
        {
            var pendingEmptyPLBatches = pendingEmptyPrintLocation.GroupBy(g => g.Id).Select(s => new PendingMailPieceInfo
            {
                Id = s.First().Id,
                AppCode = s.First().AppCode,
                BatchName = s.First().BatchName,
                ReceivedOn = s.First().ReceivedOn,
                PipelineStatus = s.First().PipelineStatus,
                ShippingStatus = s.First().ShippingStatus,
                PrintLocation = s.First().PrintLocation,
                PendingMailPieces = s.Count(),
                SweepName = s.First().SweepName
            }).ToList();
            return pendingEmptyPLBatches;
        }

        public List<PendingMailPieceInfo> DallasPendingBatches(List<PendingMailPieceInfo> pendingDallasMailPieces)
        {
            var pendingDallasBatches = pendingDallasMailPieces.GroupBy(g => g.Id).Select(s => new PendingMailPieceInfo
            {
                Id = s.First().Id,
                AppCode = s.First().AppCode,
                BatchName = s.First().BatchName,
                ReceivedOn = s.First().ReceivedOn,
                PipelineStatus = s.First().PipelineStatus,
                ShippingStatus = s.First().ShippingStatus,
                PrintLocation = s.First().PrintLocation,
                PendingMailPieces = s.Count(),
                SweepName = s.First().SweepName
            }).ToList();
            return pendingDallasBatches;
        }
        public List<PendingMailPieceInfo> PCIPendingBatches(List<PendingMailPieceInfo> pendingPCIMailPieces)
        {
            List<int> batchIds = new List<int>();
            //foreach (var bt in pendingPCIMailPieces.GroupBy(g => g.Id).Distinct())
            //{
            //    var qry = "select * from ufn_GetPCIBundleSweepTimings(@date)";
            //    var timings = db.Query<PCIBundleSweepTiming>(qry, new { date = bt.First().ReceivedOn.Value.Date }).ToList();

            //    var thisBatchSweep = timings.Single(t => t.StartTime <= bt.First().ReceivedOn && t.EndTime >= bt.First().ReceivedOn);
            //    if (DateTime.UtcNow.UtcToEst() > thisBatchSweep.SweepTime) batchIds.Add(bt.First().Id);
            //}





            //var thisBatchSweep = timings.Single(t => t.StartTime <= bt.First().ReceivedOn && t.EndTime >= bt.First().ReceivedOn);
            //if (DateTime.UtcNow.UtcToEst() > thisBatchSweep.SweepTime) batchIds.Add(bt.First().Id);

            var pendingPCIBatches = pendingPCIMailPieces.Where(p => batchIds.Any(b => b == p.Id)).GroupBy(g => g.Id).Select(s => new PendingMailPieceInfo
            {
                Id = s.First().Id,
                AppCode = s.First().AppCode,
                BatchName = s.First().BatchName,
                ReceivedOn = s.First().ReceivedOn,
                PipelineStatus = s.First().PipelineStatus,
                ShippingStatus = s.First().ShippingStatus,
                PrintLocation = s.First().PrintLocation,
                PendingMailPieces = s.Count(),
                SweepName = s.First().SweepName
            }).ToList();
            return pendingPCIBatches;
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

        public void SaveEmailSentTimeStamp(int mailBatchId, bool IsSubmitted)
        {
            db.Execute("INSERT INTO [Common].[MonitorBatches] ([MailBatchId],[Email_TimeStamp],[IsSubmitted]) VALUES (@MailBatchId,@Email_TimeStamp,@IsSubmitted)", new { @MailBatchId = mailBatchId, @Email_TimeStamp = DateTime.UtcNow, @IsSubmitted = IsSubmitted });
        }
        public int CheckPrintBatchIdExist(int mailBatchId, bool IsSubmitted)
        {
            return Convert.ToInt32(db.ExecuteScalar("SELECT Count(MailBatchId) FROM Common.MonitorBatches WHERE MailBatchId = @MailBatchId AND IsSubmitted = @IsSubmitted", new { @MailBatchId = mailBatchId, @IsSubmitted = IsSubmitted }).ToString());
        }
        public int CheckLastEmailSentTime(int mailBatchId, bool IsSubmitted)
        {
            return Convert.ToInt32(db.ExecuteScalar("SELECT DATEDIFF(hour, Email_TimeStamp, GETUTCDATE()) AS EmailTimeStamp FROM Common.MonitorBatches WHERE MailBatchId = @MailBatchId AND IsSubmitted = @IsSubmitted", new { @MailBatchId = mailBatchId, @IsSubmitted = IsSubmitted }).ToString());
        }
        public void UpdateEmailTimeStamp(int mailBatchId, bool IsSubmitted)
        {
            db.Execute("UPDATE Common.MonitorBatches SET Email_TimeStamp = GETUTCDATE(),IsSubmitted = @IsSubmitted WHERE MailBatchId = @MailBatchId", new { @MailBatchId = mailBatchId, @IsSubmitted = IsSubmitted });
        }


    }
}

namespace AM.Services
{
    public class Sweep
    {
        public int SrNo { get; set; }
        public string WeekDayType { get; set; }
        public TimeSpan SweepTime { get; set; }
    }
}
