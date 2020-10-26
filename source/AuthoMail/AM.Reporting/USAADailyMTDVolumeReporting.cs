using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;
using AM.Services;

namespace AM.Reporting
{
    class USAADailyMTDVolumeReporting
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        internal static void GenerateMTDReport()
        {
            log.Debug("USAA MTD Report Execution Triggered at {0} Est".Fill(Common.Now.UtcToEst()));
            DateTime startDate = new DateTime(Common.EstToday.Year, Common.EstToday.Month, 1);
            DateTime EndDate = Common.EstToday;
            startDate = startDate.EstToUtc();
            EndDate = EndDate.EstToUtc();
            try
            {
                using (IReportService reportSvc = new ReportService())
                {
                    //var sourceFilePath = @"USAAMTDVolumeReport\USAAMTDVolumeReportTemplate.xlsx";
                    var output = reportSvc.USAAMTDVolumeReport(startDate, EndDate);
                    var byteArr = output.ToArray();
                    var ms = new MemoryStream(byteArr, true);
                    ms.Write(byteArr, 0, byteArr.Length);
                    ms.Position = 0;
                    SendMTDReportToUSAATeam(ms, $"USAA MTD Volume Report {startDate.ToString("MMddyy")}To{EndDate.ToString("MMddyy")}.csv", "USAA daily MTD volume report");
                    //var output = reportSvc.USAAMTDVolumeReport(startDate, EndDate, sourceFilePath);
                    //var byteArr = output.ToArray();
                    //var ms = new MemoryStream(byteArr, true);
                    //ms.Write(byteArr, 0, byteArr.Length);
                    //ms.Position = 0;
                    //SendMTDReportToUSAATeam(ms, $"USAA MTD Volume Report {startDate.ToString("MMddyy")}To{EndDate.ToString("MMddyy")}.csv", "USAA daily MTD volume report");

                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private static void SendMTDReportToUSAATeam(MemoryStream fileStream, string fileName, string subject)
        {
            try
            {
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = false;
                        msg.To.Add(AppConfig.USAAMTDEmailTo);
                        if (!AppConfig.USAAMTDEmailCc.IsNullOrWhiteSpace())
                            msg.CC.Add(AppConfig.USAAMTDEmailCc);
                        msg.Subject = subject;
                        msg.Priority = MailPriority.Normal;
                        msg.Body = EmailBody();
                        msg.IsBodyHtml = true;
                        msg.Attachments.Add(new Attachment(fileStream, fileName));

                        smtpClient.Send(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error occured while sending USAA MTD Report :", ex);
            }
        }
        private static string EmailBody()
        {
            return $@"
<html>
<head></head>
<body>
<p>
<br/>
 Please find the attached month to date MTD volume report.
<br/>
</p>
<br/>
<br/>
Thank you,
<br/>
<b>AuthoMail Team</b>
</body>
</html>
";
        }
    }
}
