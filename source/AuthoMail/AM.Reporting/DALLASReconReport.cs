using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;
using AM.Entities;
using System.IO;
using System.Net.Mail;

namespace AM.Reporting
{
    class DALLASReconReport
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal static void GenerateDALLASReconReport()
        {
            try
            {
                log.Debug("DALLAS recon report Execution Triggered at {0} Est".Fill(Common.Now.UtcToEst()));
                List<DALLASSubmittedBatchDetail> dallasSubmittedBatchDetails;
                using (IReportService reportSvc = new ReportService())
                {
                    DateTime startDate = Common.EstToday;
                    DateTime endDate = startDate.AddDays(1);

                    //NOTE : Please change UTC to EST code when Daylight saving is off in service call
                    dallasSubmittedBatchDetails = reportSvc.GetDALLASReconReport(startDate, endDate);

                    if (dallasSubmittedBatchDetails.Count > 0)
                    {
                        SendEmailNotificationToPCI(dallasSubmittedBatchDetails, startDate);
                    }
                }
                log.Debug("DALLAS recon report Execution Completed at {0} Est".Fill(Common.Now.UtcToEst()));
            }
            catch (Exception ex)
            {
                log.Error("Error occured while generating Reconcilation Report for DALLAS", ex);
            }
        }

        private static void SendEmailNotificationToPCI(List<DALLASSubmittedBatchDetail> dallasSubmittedBatchDetails, DateTime currentTime)
        {
            var columnName = String.Join(",", "Client", "Carrier", "Service Type", "Package Count", "Total Pages");

            StringBuilder reconReport = new StringBuilder(columnName);

            reconReport.AppendLine();
            foreach (var batch in dallasSubmittedBatchDetails)
            {
                var line = String.Join(",", batch.AppCode, batch.ShippingCarrier, batch.ShippingServiceType, batch.PackageCount, batch.TotalPages);
                reconReport.AppendLine(line);
            }
            string reportFilePath = Path.Combine(AppConfig.PCIReconReportPath, "DALLASReconReport_" + currentTime.ToString("MMddyyyy") + ".csv");
            File.WriteAllText(reportFilePath, reconReport.ToString());

            SendReportToCLTeam(reportFilePath, currentTime);
        }

        private static bool SendReportToCLTeam(string reconReportPath, DateTime todayCutOffTime)
        {
            bool isEmailSent = false;

            try
            {
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = false;
                        msg.To.Add(AppConfig.DALLASReconEMailTo);
                        msg.CC.Add(AppConfig.DALLASReconEMailCc);
                        msg.Subject = "{0} : {1}".Fill(Config.Common_Environment, AppConfig.DALLASReconEMailSubject.Fill(todayCutOffTime.ToString("MM/dd/yyyy")));
                        msg.Priority = MailPriority.Normal;
                        msg.Body = "DALLAS Recon Report For : {0}".Fill(todayCutOffTime.ToString("MM/dd/yyyy"));

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

    }
}
