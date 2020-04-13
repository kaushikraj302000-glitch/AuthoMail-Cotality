using AM.Lib;
using AM.Services;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace AM.Reporting
{
    class GOSReports
    {
        private static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal static void GenerateGOSReceivedBatchReport()
        {
            try
            {
                log.Debug("GOS received batch report Execution Triggered at {0} Est".Fill(Common.Now.UtcToEst()));
                List<GOSMailPieceData> gosBatchDetails;
                DateTime currentTime = Common.EstToday;
                using (IReportService reportSvc = new ReportService())
                {
                    //NOTE : Please change UTC to EST code when Daylight saving is off in service call
                    gosBatchDetails = reportSvc.GetGOSDailyReconReport(currentTime);

                    if (gosBatchDetails.Count > 0)
                    {
                        string reportFilePath = Path.Combine(AppConfig.GOSReconReportPath, "GOSBatchReceivedReport_" + currentTime.ToString("MMddyyyy") + ".csv");
                        SendEmailNotificationToGOS(gosBatchDetails, currentTime, reportFilePath, AppConfig.GOSReceivedBatchEMailSubject);
                    }
                }
                log.Debug("GOS received batch report Execution completed at {0} Est".Fill(Common.Now.UtcToEst()));
            }
            catch (Exception ex)
            {
                log.Error("Error occured while generating GOS received batch", ex);
            }
        }

        private static void SendEmailNotificationToGOS(List<GOSMailPieceData> gosBatchDetails, DateTime currentTime, string reportFilePath, string subject)
        {
            var columnName = String.Join(",", "Client Code", "Batch Name", "Received On (Est)", "Loan Number", "Document Type", "File Name", "Tracking Number", "Shipping Status", "Total Pages", "Shipped On (Est)", "Print Location", "Error Description");

            StringBuilder reconReport = new StringBuilder(columnName);

            reconReport.AppendLine();
            foreach (var batch in gosBatchDetails)
            {
                var line = String.Join(",", batch.AppCode, batch.BatchName, batch.ReceivedOnEst, batch.LoanNumber, batch.DocumentOrPackageType, batch.FileName, batch.TrackingNo, batch.ShippingStatus, batch.TotalPages, batch.ShippedOnEst, batch.PrintLocation, batch.ErrorDesc);
                reconReport.AppendLine(line);
            }

            File.WriteAllText(reportFilePath, reconReport.ToString());

            SendReportToCLTeam(reportFilePath, currentTime, subject);
        }

        private static bool SendReportToCLTeam(string reconReportPath, DateTime todayCutOffTime, string subject)
        {
            bool isEmailSent = false;

            try
            {
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = false;
                        msg.To.Add(AppConfig.GOSReconEMailTo);
                        msg.CC.Add(AppConfig.GOSReconEMailCc);
                        msg.Subject = "{0} : {1}".Fill(Config.Common_Environment, subject.Fill(todayCutOffTime.ToString("MM/dd/yyyy")));
                        msg.Priority = MailPriority.Normal;
                        msg.Body = "GOS Report For : {0}".Fill(todayCutOffTime.ToString("MM/dd/yyyy"));

                        if (File.Exists(reconReportPath))
                            msg.Attachments.Add(new Attachment(reconReportPath));

                        smtpClient.Send(msg);
                        isEmailSent = true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            return isEmailSent;
        }

        internal static void GenerateGOSReconReport()
        {
            try
            {
                log.Debug("GOS recon report Execution Triggered at {0} Est".Fill(Common.Now.UtcToEst()));
                List<GOSMailPieceData> gosBatchDetails;
                DateTime currentTime = Common.EstToday.AddDays(-1);
                using (IReportService reportSvc = new ReportService())
                {

                    //NOTE : Please change UTC to EST code when Daylight saving is off in service call
                    gosBatchDetails = reportSvc.GetGOSDailyReconReport(currentTime);

                    if (gosBatchDetails.Count > 0)
                    {
                        string reportFilePath = Path.Combine(AppConfig.GOSReconReportPath, "GOSReconReport_" + currentTime.ToString("MMddyyyy") + ".csv");
                        SendEmailNotificationToGOS(gosBatchDetails, currentTime, reportFilePath, AppConfig.GOSReconEMailSubject);
                    }
                }
                log.Debug("GOS recon report Execution completed at {0} Est".Fill(Common.Now.UtcToEst()));
            }
            catch (Exception ex)
            {
                log.Error("Error occured while generating GOS recon report", ex);
            }
        }
    }
}
