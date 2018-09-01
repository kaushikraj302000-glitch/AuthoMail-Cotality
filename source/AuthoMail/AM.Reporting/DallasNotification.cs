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
                List<DallasBatch> dallasBatches;

                using (ICommonService commonSvc = new CommonService())
                {
                    dallasBatches = commonSvc.GetDallasBatches(Common.EstToday);
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


                while (!CheckAllBatchStatusCompleted(currentBatches.Min(b => b.StartDate), currentBatches.Max(b => b.EndDate)))
                {
                    Thread.Sleep(6000);
                }

                foreach (var batch in currentBatches.GroupBy(b => b.BatchName))
                {
                    List<DallasMailPieceData> dallasMailPieces;
                    using (IMailPieceService mailPieceSvc = new MailPieceService())
                    {
                        dallasMailPieces = mailPieceSvc.GetDallasMailPiecesByTimingsAndClient(batch.Key);
                    }

                    if (dallasMailPieces != null)
                    {
                        var batchSummary = dallasMailPieces.GroupBy(b => new { b.AppCode, b.ShippingCarrier, b.ShippingServiceType });
                        List<Notification> notificationList = new List<Notification>();
                        foreach (var g in batchSummary)
                        {

                            notificationList.Add(new Notification() { ClientCode = g.Key.AppCode, ShippingCarrier = g.Key.ShippingCarrier, ShippingServiceType = g.Key.ShippingServiceType, MailPieceCount = g.Count(), TotalPagesCount = g.Sum(b => b.TotalPages) });
                        }

                        SendEmailNotification(notificationList, Common.EstToday, batch.Key);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private static bool CheckAllBatchStatusCompleted(DateTime startDateTime, DateTime endDateTime)
        {
            int receivedCount = 0;
            int completedCount = 0;


            using (IMailBatchService mailBatchSvc = new MailBatchService())
            {
                receivedCount = mailBatchSvc.GetMailBatchesReceivedCount(startDateTime, endDateTime, "Received");
                completedCount = mailBatchSvc.GetMailBatchesReceivedCount(startDateTime, endDateTime, "Completed");
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
                            fullContent.AppendLine(@"<tr><td colspan='3' style='width:6.65in;border:solid windowtext 1.0pt;padding:0in 5.4pt 0in 5.4pt' valign='top' width='638'><p style='text-align:center' align='center'><span style='font-size:20.0pt'>Below Folder should have count of packages to be printed and shipped<u></u><u></u></span></p></td></tr>");

                            foreach (var client in clientList)
                            {
                                fullContent.AppendLine(@"<tr><td colspan='3' style='width:6.65in;border:solid windowtext 1.0pt;padding:0in 5.4pt 0in 5.4pt' valign='top' width='638'><p style='text-align:center' align='center'><span style='font-size:20.0pt'>
                                    {0} / {1} / {2}
                                    <u></u><u></u>
                                </span></p></td></tr>".Fill(client, currentEstDate.ToString("MMddyyyy"), sweepName));
                                fullContent.AppendLine(@"<tr><td style='width:6.65in;border:solid windowtext 1.0pt;padding:0in 6pt 0in 6pt;text-align:center;font-size:14.0pt' valign='top' width='638'>Shipping Method</td><td style='width:6.65in;border:solid windowtext 1.0pt;padding:0in 6pt 0in 6pt;text-align:center;font-size:14.0pt' valign='top' width='638'>Shipping Service</td><td style='width:6.65in;border:solid windowtext 1.0pt;padding:0in 6pt 0in 6pt;text-align:center;font-size:14.0pt' valign='top' width='638'>Total Packages</td><td style='width:6.65in;border:solid windowtext 1.0pt;padding:0in 6pt 0in 6pt;text-align:center;font-size:14.0pt' valign='top' width='638'>Total Pages</td></tr>");

                                foreach (var filterd in notificationList.Where(f => f.ClientCode == client))
                                {
                                    fullContent.AppendLine(@"<tr><td><p style='text-align:center' align='center'><span style='font-size:14.0pt'>{0}</span></p></td><p style='text-align:center' align='center'><span style='font-size:14.0pt'>{1}</span></p></td><td><p style='text-align:center' align='center'>
                                <span style='font-size:14.0pt'>{2}</span></p></td><td><p style='text-align:center' align='center'>
                                <span style='font-size:14.0pt'>{3}</span></p></td></tr>".Fill(filterd.ShippingCarrier, filterd.ShippingServiceType, filterd.MailPieceCount, filterd.TotalPagesCount));
                                }
                            }

                            sb = sb.Replace("<%REPLACE%>", fullContent.ToString());
                        }
                        else
                        {
                            sb = sb.Replace("<%REPLACE%>", @"<tr><td colspan='3' style='width:6.65in;border:solid windowtext 1.0pt;padding:0in 5.4pt 0in 5.4pt' valign='top' width='638'><p style='text-align:center' align='center'><span style='font-size:20.0pt'>{0} sweep does not contain any package to print and ship<u></u><u></u></span></p></td></tr>".Fill(sweepName));
                        }
                        msg.Body = sb.ToString();
                        smtpClient.Send(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error occured while sending an Email to Dallas Team : ", ex);
            }
        }
    }

    class Notification
    {
        public string ClientCode { get; set; }
        public string ShippingCarrier { get; set; }
        public string ShippingServiceType { get; set; }
        public int MailPieceCount { get; set; }
        public int TotalPagesCount { get; set; }
    }
}
