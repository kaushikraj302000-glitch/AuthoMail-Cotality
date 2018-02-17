using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace AM.Console
{
    public class MailUtility
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MailUtility));
        public static bool SendBatchSuspendedNotificationEmail(int printBatchId,string errorMsg)
        {
            bool isEmailSent = false;

            try
            {
                using (MailMessage msg = new MailMessage())
                {
                    msg.IsBodyHtml = true;
                    msg.To.Add("");
                    msg.CC.Add("");
                    msg.Subject = "URGENT: Batch Suspended For Batch-ID: " + printBatchId;
                    msg.Priority = MailPriority.Normal;

                    StringBuilder sb = new StringBuilder(File.ReadAllText(""));
                    sb = sb.Replace("<%Reason%>", errorMsg);                 

                    msg.Body = sb.ToString();
                    Common.Email_SMTPClient.Send(msg);

                    isEmailSent = true;
                }
            }
            catch (Exception err)
            {
                log.Error("Error occured while sending batch submitted notification to PCI", err);
            }

            return isEmailSent;
        }
    }
}
