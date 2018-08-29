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
                List<DallasBatchTimings> batchesTimings;

                using (ICommonService commonSvc = new CommonService())
                {
                    batchesTimings = commonSvc.GetDallasBatchTimings();
                }

                var currentEstDateTime = Common.Now.UtcToEst();

                DateTime startDateTime = new DateTime();
                DateTime endDateTime = new DateTime();
                string sweepName = string.Empty;

                var currentBatch = batchesTimings.OrderByDescending(b => b.EndDate).FirstOrDefault(b => b.BatchName != "Backlog" && currentEstDateTime > b.EndDate);

                if (currentBatch == null) return;


                startDateTime = currentBatch.StartDate.EstToUtc();
                endDateTime = currentBatch.EndDate.EstToUtc();

                while (!CheckAllBatchStatusCompleted(startDateTime, endDateTime))
                {
                    Thread.Sleep(6000);
                }

                List<DallasMailPieceData> dallasMailPieces;
                using (IMailPieceService mailPieceSvc = new MailPieceService())
                {
                    dallasMailPieces = mailPieceSvc.GetDallasMailPiecesByTimingsAndClient(currentBatch.BatchName);
                }

                if (dallasMailPieces != null)
                {
                    var clients = dallasMailPieces.Select(c => c.AppCode).Distinct();
                    List<Notification> notificationList = new List<Notification>();
                    foreach (string clientCode in clients)
                    {
                        var mailPiecesByClient = dallasMailPieces.Where(f => f.AppCode == clientCode).ToList();

                        var shippingServiceTypes = mailPiecesByClient.Select(c => c.ShippingServiceType).Distinct();

                        foreach (string shippingServiceType in shippingServiceTypes)
                        {
                            string shippingCarrier = shippingServiceType;
                            if (shippingServiceType == "PRIORITY_OVERNIGHT")
                                shippingCarrier = "FedEx";

                            var mailPiecesByShippingServiceType = mailPiecesByClient.Where(f => f.ShippingServiceType == shippingServiceType).ToList();

                            var totalPages = mailPiecesByClient.Where(f => f.ShippingServiceType == shippingServiceType).Sum(c => c.TotalPages);

                            notificationList.Add(new Notification() { ClientCode = clientCode, ShippingType = shippingCarrier, MailPieceCount = mailPiecesByShippingServiceType.Count, TotalPagesCount = totalPages });
                        }
                    }

                    SendEmailNotification(notificationList, endDateTime.UtcToEst(), sweepName);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private static bool CheckAllBatchStatusCompleted(DateTime startDateTime, DateTime endDateTime)
        {
            List<MailBatches> mailBatches;
            int receivedCount = 0;
            int completedCount = 0;


            using (IMailBatchService mailBatchSvc = new MailBatchService())
            {
                mailBatches = mailBatchSvc.GetMailBatchesDataByDallasCutOff(startDateTime, endDateTime);
            }

            receivedCount = mailBatches.Where(f => f.IsReceived == 1).ToList().Count;
            completedCount = mailBatches.Where(f => f.IsAcknowledged == 1 || f.IsReportPrepped == 1 || f.IsReportedBack == 1).ToList().Count;

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
                        msg.Subject = "{0} : {1} {2}".Fill(Config.Common_Environment, AppConfig.DallasEMailSubject, currentEstDate.AddHours(-1).ToString("hhtt"));
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
                                fullContent.AppendLine(@"<tr><td style='width:6.65in;border:solid windowtext 1.0pt;padding:0in 6pt 0in 6pt;text-align:center;font-size:14.0pt' valign='top' width='638'>Shipping Method</td><td style='width:6.65in;border:solid windowtext 1.0pt;padding:0in 6pt 0in 6pt;text-align:center;font-size:14.0pt' valign='top' width='638'>Total Packages</td><td style='width:6.65in;border:solid windowtext 1.0pt;padding:0in 6pt 0in 6pt;text-align:center;font-size:14.0pt' valign='top' width='638'>Total Pages</td></tr>");

                                foreach (var filterd in notificationList.Where(f => f.ClientCode == client))
                                {
                                    fullContent.AppendLine(@"<tr><td><p style='text-align:center' align='center'><span style='font-size:14.0pt'>{0}</span></p></td><td><p style='text-align:center' align='center'>
                                <span style='font-size:14.0pt'>{1}</span></p></td><td><p style='text-align:center' align='center'>
                                <span style='font-size:14.0pt'>{2}</span></p></td></tr>".Fill(filterd.ShippingType, filterd.MailPieceCount, filterd.TotalPagesCount));
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
        public string ShippingType { get; set; }
        public int MailPieceCount { get; set; }
        public int TotalPagesCount { get; set; }
    }
}
