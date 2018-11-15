using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AM.Lib;
using AM.Entities;
using System.Text.RegularExpressions;
using System.IO;
using System.Net.Mail;

namespace AM.Services
{
    public interface IMonitorBatchesService : IDisposable
    {
        MonitorReport GetPendingBatchesToSubmit(int cutOffMinutes);
        List<dynamic> GetPendingBatchesToReportBack();
        void SaveEmailSentTimeStamp(int mailBatchId, bool IsSubmitted);
        int CheckPrintBatchIdExist(int mailBatchId, bool IsSubmitted);
        int CheckLastEmailSentTime(int mailBatchId, bool IsSubmitted);
        void UpdateEmailTimeStamp(int mailBatchId, bool IsSubmitted);

        bool GetDelayedPackagesStatus(int effectiveCutOff);
        void SendUnexpectedExceptionEmail(string exception);
    }
    public class MonitorBatchesService : ServiceBase, IMonitorBatchesService
    {
        public MonitorReport GetPendingBatchesToSubmit(int cutOffMinutes)
        {

            string qry = "[dbo].[uspalert_GetDelayedBatchesToSubmit] @cutOffMinutes";
            List<Sweep> timings = new List<Sweep>();
            bool isReceivedDelay = false;
            bool isProcessDelay = false;
            bool isReportBackDelay = false;
            bool isPrinterDelay = false;
            var lastBatchReceivedOn = new BatchInfo();
            var delayedBatches = new List<BatchInfo>();
            var delayedMailPieces = new List<PendingMailPieceInfo>();
            var delayedAMReportedBackBatches = new List<BatchInfo>();
            var delayedPrinterBatches = new List<BatchInfo>();
            var delayedReportedBackMailPiecesByAM = new List<PendingMailPieceInfo>();
            var delayedMailPiecesByPrinter = new List<PendingMailPieceInfo>();
            using (ICommonService commonSvc = new CommonService())
            {
                try
                {
                    timings = commonSvc.GetSweepTimings();
                }
                catch (Exception)
                {
                    isProcessDelay = true;
                }
            }

            try
            {
                lastBatchReceivedOn = db.QuerySingle<BatchInfo>("select top 1 ReceivedOn from AM.MailBatches where IsReceived = 1 order by ReceivedOn desc");
                if ((Common.Now - lastBatchReceivedOn.ReceivedOn).TotalMinutes > 60)
                {
                    var triggerFiles = EnumerateBatches();
                    if (triggerFiles.Count > 0)
                    {
                        isReceivedDelay = true;
                    }
                }
            }
            catch (Exception)
            {
                isReceivedDelay = true;
            }
            try
            {
                var pendingMPsToSubmit = db.Query<PendingMailPieceInfo>(qry, new { cutOffMinutes = cutOffMinutes }).ToList();

                var shippingStatusNullMailPieces = pendingMPsToSubmit.Where(p => p.ShippingStatus.IsNullOrEmpty()).ToList();
                var clientIdNullMailPieces = pendingMPsToSubmit.Where(p => !p.ClientId.HasValue).ToList();
                var delayedErrorMailPieces = pendingMPsToSubmit.Where(p => p.ShippingStatus == ShippingStatus.Error.ToString()).ToList();
                var delayedPulledBackMailPieces = pendingMPsToSubmit.Where(p => p.ShippingStatus == ShippingStatus.PulledBack.ToString()).ToList();
                var delayedShippedMailPieces = pendingMPsToSubmit.Where(m => (m.ShippingStatus == ShippingStatus.Shipped.ToString())).ToList();
                var pendingInProgressMailPieces = pendingMPsToSubmit.Where(m => (m.ShippingStatus == ShippingStatus.InProgress.ToString())).ToList();
                var delayedEmptyPrintLocation = pendingInProgressMailPieces.Where(p => p.PrintLocation.IsNullOrEmpty()).ToList();

                var delayedDallasMailPieces = pendingInProgressMailPieces.Where(p => p.VendorCode == "CL_DALLAS").ToList();

                var delayedPCIMailPieces = pendingInProgressMailPieces.Where(mp => mp.VendorCode == "PCI" && Common.Now > mp.GetEstimatedSubmittedOn(timings, cutOffMinutes + 15)).ToList();

                delayedMailPieces = delayedDallasMailPieces.Concat(delayedEmptyPrintLocation).Concat(delayedShippedMailPieces).Concat(delayedPulledBackMailPieces).Concat(delayedErrorMailPieces).Concat(clientIdNullMailPieces).Concat(shippingStatusNullMailPieces).Concat(delayedPCIMailPieces).ToList();

                delayedBatches = delayedMailPieces
                    .GroupBy(mp => new { mp.MailBatchId, mp.AppCode, mp.BatchName, mp.ClientId, mp.PipelineStatus, mp.ReceivedOn })
                    .Select(g => new BatchInfo { Id = g.Key.MailBatchId, AppCode = g.Key.AppCode, BatchName = g.Key.BatchName, ClientId = g.Key.ClientId, PipelineStatus = g.Key.PipelineStatus, ReceivedOn = g.Key.ReceivedOn, DelayedMailPieces = g.ToList() }).ToList();

                if (delayedMailPieces.Count > 0 || delayedBatches.Count > 0)
                {
                    isProcessDelay = true;
                }

            }
            catch (Exception)
            {
                isProcessDelay = true;
            }

            try
            {
                delayedReportedBackMailPiecesByAM = db.Query<PendingMailPieceInfo>("Select MP.Id,MP.MailBatchId,C.AppCode,MP.ClientId,MB.BatchName,MB.ReceivedOn,MB.PipelineStatus,MP.PrintLocation from AM.MailPieces MP inner join Common.Clients C on MP.ClientId = C.Id inner join AM.MailBatches MB on MP.MailBatchId = MB.Id inner join PnS.MrdfRequest MR on MP.UniqueId = MR.UniqueId inner join PnS.BatchSummary BS on MR.JobId = BS.PrintBatch where BS.IsSubmitted = 1 and MP.SLADays < 0 and BS.IsReportedBack = 1 and BS.PipelineStatus <> 'Cancelled' and MB.IsReportedBack = 0").ToList();

                delayedAMReportedBackBatches = delayedReportedBackMailPiecesByAM.GroupBy(mp => new { mp.MailBatchId, mp.AppCode, mp.BatchName, mp.ClientId, mp.PipelineStatus, mp.ReceivedOn }).Select(g => new BatchInfo { Id = g.Key.MailBatchId, AppCode = g.Key.AppCode, BatchName = g.Key.BatchName, ClientId = g.Key.ClientId, PipelineStatus = g.Key.PipelineStatus, ReceivedOn = g.Key.ReceivedOn, DelayedMailPieces = g.ToList() }).ToList();

                if (delayedReportedBackMailPiecesByAM.Count > 0 || delayedAMReportedBackBatches.Count > 0)
                {
                    isReportBackDelay = true;
                }
            }
            catch (Exception)
            {
                isReportBackDelay = true;
            }

            try
            {
                delayedMailPiecesByPrinter = db.Query<PendingMailPieceInfo>("Select MP.Id,MP.MailBatchId,C.AppCode,MP.ClientId,MB.BatchName,MB.ReceivedOn,MB.PipelineStatus,MP.PrintLocation from AM.MailPieces MP inner join Common.Clients C on MP.ClientId = C.Id inner join AM.MailBatches MB on MP.MailBatchId = MB.Id inner join PnS.MrdfRequest MR on MP.UniqueId = MR.UniqueId inner join PnS.BatchSummary BS on MR.JobId = BS.PrintBatch where BS.IsSubmitted = 1 and MP.SLADays < 0 and BS.IsReportedBack = 0 and BS.PipelineStatus <> 'Cancelled'").ToList();

                delayedPrinterBatches = delayedMailPiecesByPrinter.GroupBy(mp => new { mp.MailBatchId, mp.AppCode, mp.BatchName, mp.ClientId, mp.PipelineStatus, mp.ReceivedOn }).Select(g => new BatchInfo { Id = g.Key.MailBatchId, AppCode = g.Key.AppCode, BatchName = g.Key.BatchName, ClientId = g.Key.ClientId, PipelineStatus = g.Key.PipelineStatus, ReceivedOn = g.Key.ReceivedOn, DelayedMailPieces = g.ToList() }).ToList();

                if (delayedMailPiecesByPrinter.Count > 0 || delayedPrinterBatches.Count > 0)
                {
                    isPrinterDelay = true;
                }
            }
            catch (Exception)
            {
                isPrinterDelay = true;
            }

            return new MonitorReport { DelayedBatches = delayedBatches, DelayedMailPieces = delayedMailPieces, IsReceivedDelay = isReceivedDelay, DelayedAMReportBackBatches = delayedAMReportedBackBatches, DelayedPrinterBatches = delayedPrinterBatches, DelayedAMReportBackMailPieces = delayedReportedBackMailPiecesByAM, DelayedPrinterMailPieces = delayedMailPiecesByPrinter, LastBatchReceivedOn = lastBatchReceivedOn.ReceivedOn, IsProcessDelay = isProcessDelay, IsPrinterDelay = isPrinterDelay, IsReportBackDelay = isReportBackDelay };
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

        public bool GetDelayedPackagesStatus(int effectiveCutOff)
        {
            int result = db.Execute("dbo.uspalert_GetDelayedPackagesStatus @p1", new { p1 = effectiveCutOff });
            return result > 0 ? false : true;
        }

        public List<string> EnumerateBatches()
        {
            List<string> triggerFilePaths = new List<string>();
            SFTChannel sftChannel;
            using (IClientService clientSvc = new ClientService())
            {
                sftChannel = clientSvc.GetSFTChannelDetailsBySFTChannelCode("AMC_" + Config.Common_Environment);
            }
            SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(sftChannel.Protocol), sftChannel.Host, sftChannel.UserName, sftChannel.Password, 300000);
            sftSite.Connect();

            triggerFilePaths = GetTriggerFilePaths(sftSite, sftChannel.ToADEDir);

            if (sftSite.IsConnected)
                sftSite.Disconnect();
            return triggerFilePaths;
        }

        private List<string> GetTriggerFilePaths(SFTUtil sftSite, string toADEDir)
        {
            var regex = new Regex(@"^(\w*)_Complete.go$", RegexOptions.IgnoreCase);

            List<string> availableTriggerFilePaths = sftSite.ListDirectory(toADEDir)
                .Select(f => regex.Match(Path.GetFileName(f)))
                .Where(m => m.Success)
                .Select(m => m.Groups[1].Value)
                .ToList();
            return availableTriggerFilePaths;
        }

        public void SendUnexpectedExceptionEmail(string exception)
        {
            using (SmtpClient smtpClient = new SmtpClient())
            {
                using (MailMessage msg = new MailMessage())
                {
                    msg.IsBodyHtml = true;
                    msg.To.Add("akshar.vala@milestonetechnoconsulting.com");
                    msg.CC.Add("nirav.barot@milestonetechnoconsulting.com");
                    msg.Subject = "{0} : {1}".Fill(Config.Common_Environment, "Exception in Alert API");
                    msg.Priority = MailPriority.High;
                    msg.Body = "Unexpected exception occured during execution of Alert API." + Environment.NewLine + exception;
                    smtpClient.Send(msg);
                }
            }
        }
    }
}

namespace AM.Services
{
    public class Sweep
    {
        public int SrNo { get; set; }
        public string AppCode { get; set; }
        public string VendorCode { get; set; }
        public string WeekDayType { get; set; }
        public string SweepType { get; set; }
        public TimeSpan SweepTime { get; set; }
    }
}
