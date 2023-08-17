using AM.Lib;
using AM.Lib.Utilities;
using AM.Services;
using AM.Services.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace AM.Reporting
{
    class WizardReport
    {
        private static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal static void GenerateWizardReport()
        {
            try
            {
                log.Debug($"Wizard report Execution Triggered at {Common.Now.UtcToEst()} Est");
                List<ReportWizard> reportDetails;
                DateTime endDate = Common.EstToday.AddDays(-1);
                DateTime startDate = endDate.AddDays(-9);
                using (IReportService reportSvc = new ReportService())
                {
                    //NOTE : Please change UTC to EST code when Daylight saving is off in service call
                    reportDetails = reportSvc.GetReportWizard(new List<int>() { }, null, null, startDate, endDate, null, null, null, null, null);

                    reportDetails.ForEach(m =>
                    {
                        m.ReceivedOn = m.ReceivedOn.HasValue ? m.ReceivedOn.Value.UtcToEst() : m.ReceivedOn;
                        m.PrintedOn = m.PrintedOn.HasValue ? m.PrintedOn.Value.UtcToEst() : m.PrintedOn;
                        m.MailedOn = m.MailedOn.HasValue ? m.MailedOn.Value.UtcToEst() : m.MailedOn;
                        m.DeliveryDate = m.DeliveryDate.HasValue ? m.DeliveryDate.Value.UtcToEst() : m.DeliveryDate;
                        m.ReturnDeliveryDate = m.ReturnDeliveryDate.HasValue ? m.ReturnDeliveryDate.Value.UtcToEst() : m.ReturnDeliveryDate;
                    });
                    var dataTable = ExcelHelper.ToDataTable<ReportWizard>(reportDetails);
                    var memoryStream = ExcelHelper.ExportDataSet(dataTable);

                    if (reportDetails.Count > 0)
                    {
                        string reportFileName = $"ReportWizard_{startDate.ToString("MMddyyyy")}_{endDate.ToString("MMddyyyy")}.xlsx";
                        SendEmailNotification(reportDetails, memoryStream, reportFileName, AppConfig.ReportWizardEMailSubject.Fill(startDate.ToString("MMddyyyy"), endDate.ToString("MMddyyyy")));
                    }
                }
                log.Debug($"Wizard report Execution completed at {Common.Now.UtcToEst()} Est");
            }
            catch (Exception ex)
            {
                log.Error("Error occured while generating Wizard report batch", ex);
            }
        }

        private static bool SendEmailNotification(List<ReportWizard> reportDetails, MemoryStream fileData, string reportFileName, string subject)
        {
            bool isEmailSent = false;

            try
            {
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = true;
                        msg.To.Add(AppConfig.ReportWizardEMailTo);
                        msg.CC.Add(AppConfig.ReportWizardEMailCc);
                        msg.Subject = $"{Config.Common_Environment} : {subject}";
                        msg.Priority = MailPriority.Normal;
                        msg.Body = subject;

                        if (fileData != null)
                            msg.Attachments.Add(new Attachment(fileData, reportFileName));

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
