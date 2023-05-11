using AM.Lib;
using AM.Services;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace AM.Reporting
{
    class CSSReporting
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        internal static void GenerateErrorReport()
        {
            try
            {
                log.Debug("CSS Error Report Execution Triggered at {0} Est".Fill(Common.Now.UtcToEst()));
                List<MailPieces> errorReportData;
                using (IClientService clientSvc = new ClientService())
                using (IReportService reportSvc = new ReportService())
                {
                    var clientInfo = clientSvc.GetClientsByAlias("CSS Invoices");
                    DateTime previousDate = Common.EstToday.AddDays(-1);
                    //NOTE : Please change UTC to EST code when Daylight saving is off in service call
                    
                    errorReportData = reportSvc.GetErrorReportByDateOrBatch(clientInfo.Select(x => x.Id).FirstOrDefault(), previousDate, previousDate, null, "");

                    if (errorReportData.Count > 0)
                    {
                        GenerateErrorReport(errorReportData, previousDate);
                    }
                }
                log.Debug("CSS Error Report Execution Completed at {0} Est".Fill(Common.Now.UtcToEst()));
            }
            catch (Exception ex)
            {
                log.Error("Error occured while generating CSS Error Report", ex);
            }
        }
        private static void GenerateErrorReport(List<MailPieces> errorReportData, DateTime previousDate)
        {
            string subject = "{0} : {1}".Fill(Config.Common_Environment, AppConfig.CSSErrorReportEMailSubject.Fill(previousDate.ToString("MM/dd/yyyy")));

            string body = "CSS Error Report For : {0}".Fill(previousDate.ToString("MM/dd/yyyy"));

            var columnName = String.Join(",", "Loan Number", "Document Type", "Error Reason", "Receive Date");

            StringBuilder reconReport = new StringBuilder(columnName);

            reconReport.AppendLine();
            foreach (var mp in errorReportData)
            {
                var line = String.Join(",", mp.BusinessFriendlyIdValue.ToCsvString(), mp.DocumentOrPackageType, mp.ErrorDesc, mp.MailBatches.ReceivedOn.Value.UtcToEst().ToString("yyyy-MM-dd hh:mm:ss"));
                reconReport.AppendLine(line);
            }
            string reportFilePath = Path.Combine(AppConfig.CSSErrorReportPath, "CSSErrorReport_" + previousDate.ToString("MM-dd-yyyy") + ".csv");
            File.WriteAllText(reportFilePath, reconReport.ToString());

            SendReportToCSSTeam(reportFilePath, subject, body);
        }
        private static void SendReportToCSSTeam(string reportFilePath, string subject, string body)
        {
            try
            {
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = false;
                        msg.To.Add(AppConfig.CSSErrorEMailTo);
                        msg.CC.Add(AppConfig.CSSErrorEMailCc);
                        msg.Subject = subject;
                        msg.Priority = MailPriority.Normal;
                        msg.Body = body;

                        if (File.Exists(reportFilePath))
                            msg.Attachments.Add(new Attachment(reportFilePath));

                        smtpClient.Send(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error occured while sending CSS Report :", ex);
            }
        }


    }
}
