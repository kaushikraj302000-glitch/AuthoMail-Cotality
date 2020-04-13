using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using AM.Lib;
using AM.Services;
using AM.Entities;
using System.Net.Mail;
using System.IO;
using System.Threading;

namespace AM.Reporting
{
    class DallasNotification
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        internal static void GenerateEmailNotification()
        {
            try
            {
                log.Debug("Dallas Notification Execution Triggered at {0} Est".Fill(Common.Now.UtcToEst()));
                List<DallasBatch> dallasBatches;
                List<DallasNotificationMap> notificationSequence;
                //We have Batch timings in EST time Zone.
                using (ICommonService commonSvc = new CommonService())
                {
                    dallasBatches = commonSvc.GetDallasBatches(Common.EstToday);
                    notificationSequence = commonSvc.GetDallasNotificationMaps();
                }

                var currentEstDateTime = Common.Now.UtcToEst();

                var query = dallasBatches.Where(b => b.BatchName != "Backlog").GroupBy(b => new { b.ShippingCarrier, b.ShippingServiceType });

                var currentBatches = new List<DallasBatch>();

                foreach (var g in query)
                {
                    // Here we are checking time limit for sending dallas notification for 30 Minutes
                    var batch = g.OrderBy(b => b.EndDate).FirstOrDefault(b => currentEstDateTime >= b.EndDate && currentEstDateTime <= b.EndDate.AddMinutes(30));

                    if (batch != null)
                        currentBatches.Add(batch);
                }

                if (currentBatches == null || currentBatches.Count == 0) return;

                int bundlingTime = 0;
                while (!CheckAllBatchStatusCompleted(currentBatches.Min(b => b.StartDate), currentBatches.Max(b => b.EndDate)))
                {
                    log.Debug("CheckAllBatchStatusCompleted Queue Check Waiting...");

                    Thread.Sleep(6000);

                    //NOTE: If bundling is continueously running for 6 minutes then system will continue further for submitting batches to print vendor.
                    bundlingTime += 6000;
                    if (bundlingTime >= 360000)
                        break;
                }

                foreach (var batch in currentBatches.GroupBy(b => b.BatchName))
                {
                    List<DallasMailPieceData> dallasMailPieces;
                    using (IMailPieceService mailPieceSvc = new MailPieceService())
                    {
                        dallasMailPieces = mailPieceSvc.GetDallasMailPiecesByTimingsAndClient(batch.Key).Where(mp => mp.MailPieceStatus != ShippingStatus.PulledBack.ToString() || mp.ShippingStatus != ShippingStatus.PulledBack.ToString()).OrderByDescending(cl => cl.AppCode).ToList();
                    }

                    if (dallasMailPieces != null)
                    {
                        var batchSummary = dallasMailPieces.GroupBy(b => new { b.AppCode, b.ShippingCarrier, b.ShippingServiceType, b.IsBRE });
                        List<Notification> notificationList = new List<Notification>();
                        foreach (var g in batchSummary)
                        {
                            notificationList.Add(new Notification() { ClientCode = g.Key.AppCode, ShippingCarrier = g.Key.ShippingCarrier, ShippingServiceType = g.Key.ShippingServiceType, IsBRE = g.Key.IsBRE ? "Yes" : "No", MailPieceCount = g.Count(), TotalPagesCount = g.Sum(b => b.TotalPages), SeqNo = notificationSequence.SingleOrDefault(c => c.AppCode == g.Key.AppCode) == null ? 999 : notificationSequence.SingleOrDefault(c => c.AppCode == g.Key.AppCode).SeqNo });
                        }

                        SendEmailNotification(notificationList.OrderBy(cd => cd.SeqNo).ToList(), Common.EstToday, batch.Key);
                    }
                }
                log.Debug("Dallas Notification Execution Completed at {0} Est".Fill(Common.Now.UtcToEst()));
            }
            catch (Exception ex)
            {
                log.Error("Error occured while trying to generate packages count for dallas printing team ", ex);
            }
        }

        private static bool CheckAllBatchStatusCompleted(DateTime startDateTime, DateTime endDateTime)
        {
            int receivedCount = 0;
            int completedCount = 0;

            //Here we are passing record counts to UTC because we are storing that time in UTC format.
            using (IMailBatchService mailBatchSvc = new MailBatchService())
            {
                receivedCount = mailBatchSvc.GetMailBatchesReceivedCount(startDateTime.EstToUtc(), endDateTime.EstToUtc(), "Received");
                completedCount = mailBatchSvc.GetMailBatchesReceivedCount(startDateTime.EstToUtc(), endDateTime.EstToUtc(), "Completed");
            }

            return receivedCount == completedCount;
        }

        private static void SendEmailNotification(List<Notification> notificationList, DateTime currentEstDate, string sweepName)
        {
            try
            {
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = true;
                        msg.To.Add(AppConfig.DallasEMailTo);
                        msg.CC.Add(AppConfig.DallasEMailCc);
                        msg.Subject = "{0} : {1} {2}".Fill(Config.Common_Environment, AppConfig.DallasEMailSubject, sweepName);
                        msg.Priority = MailPriority.Normal;

                        StringBuilder sb = new StringBuilder(File.ReadAllText(AppConfig.DallasEMailTemplate));
                        if (notificationList.Count > 0)
                        {
                            var clientList = notificationList.Select(c => c.ClientCode).Distinct();

                            StringBuilder fullContent = new StringBuilder();

                            foreach (var client in clientList)
                            {
                                fullContent.AppendLine(@"<tr class='information'><td colspan='5'><span> {0} / {1} / {2}</td></tr>".Fill(client, currentEstDate.ToString("MMddyyyy"), sweepName));
                                fullContent.AppendLine(@"<tr class='title'><td>Shipping Method</td><td>Shipping Service</td><td>BRE</td><td>Total Packages</td><td>Total Pages</td></tr>");

                                var clientwiseCounts = notificationList.Where(f => f.ClientCode == client).ToList();
                                foreach (var filterd in clientwiseCounts)
                                {
                                    fullContent.AppendLine(@"<tr class='details'><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td></tr>".Fill(filterd.ShippingCarrier, filterd.ShippingServiceType, filterd.IsBRE, filterd.MailPieceCount, filterd.TotalPagesCount));
                                }

                                fullContent.AppendLine(@"<tr class='finalCount'><td colspan='3'>Total Counts</td><td>{0}</td><td>{1}</td></tr>".Fill(clientwiseCounts.Sum(c => c.MailPieceCount), clientwiseCounts.Sum(c => c.TotalPagesCount)));
                            }

                            sb = sb.Replace("<%REPLACE%>", fullContent.ToString());
                        }
                        else
                        {
                            sb = sb.Replace("<%REPLACE%>", @"<tr class='information'><td colspan='5'>{0} sweep does not contain any package to print and ship</td></tr>".Fill(sweepName));
                        }
                        msg.Body = sb.ToString();
                        smtpClient.Send(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error occured while sending an Email to Dallas Printing Team : ", ex);
            }
        }
    }

    class Notification
    {
        public string ClientCode { get; set; }
        public string ShippingCarrier { get; set; }
        public string ShippingServiceType { get; set; }
        public string IsBRE { get; set; }
        public int MailPieceCount { get; set; }
        public int TotalPagesCount { get; set; }
        public int SeqNo { get; set; }
    }
}
