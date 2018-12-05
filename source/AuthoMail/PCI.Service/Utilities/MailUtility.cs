using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace PCI.Service
{
    public class MailUtility
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PnSCoreService));
        public static bool SendBatchSubmittedNotificationEmail(string printVendorCode, string fileName, int mailPiecesCount, int totalPages)
        {
            bool isEmailSent = false;

            try
            {
                using (MailMessage msg = new MailMessage())
                {
                    msg.IsBodyHtml = true;

                    msg.To.Add(printVendorCode == "NORDIS" ? Common.Email_ToCL : Common.Email_ToPCI);
                    msg.CC.Add(printVendorCode == "NORDIS" ? Common.Email_CcCL : Common.Email_CcPCI);
                    //Here subject string contains "Batch Submitted To {Location} : {FileName} at {Time} Total Files : {FileCount} Total Page Count : {PageCount}"
                    msg.Subject = Common.SubmitBatchEmailSubject.Fill(printVendorCode, Path.GetFileName(fileName), Common.UtcToEst(Common.GetCurrentTime()).ToString("MM/dd/yyyy hh:mm tt") + " EST", mailPiecesCount, totalPages);

                    msg.Priority = MailPriority.Normal;

                    StringBuilder sb = new StringBuilder(File.ReadAllText(Common.SubmitBatchEmailTemplate));
                    sb = sb.Replace("<%FileName%>", Path.GetFileName(fileName));
                    sb = sb.Replace("<%PrintLocation%>", printVendorCode);
                    sb = sb.Replace("<%yyyymmdd%>", Common.UtcToEst(Common.GetCurrentTime()).ToString("MM/dd/yyyy"));
                    sb = sb.Replace("<%hhmm%>", Common.UtcToEst(Common.GetCurrentTime()).ToString("hh:mm tt") + " EST");
                    sb = sb.Replace("<%TotalMailPieces%>", mailPiecesCount.ToString());
                    sb = sb.Replace("<%TotalPageCount%>", totalPages.ToString());

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
        public static bool SendBatchSuspendedNotificationInternalEmail(int printBatchId, string errorMsg)
        {
            bool isEmailSent = false;

            try
            {
                using (MailMessage msg = new MailMessage())
                {
                    msg.IsBodyHtml = true;
                    msg.To.Add(Common.SuspendEmail_To);
                    msg.CC.Add(Common.SuspendEmail_Cc);
                    msg.Subject = Common.SuspendBatchEmailSubject + printBatchId;
                    msg.Priority = MailPriority.Normal;

                    StringBuilder sb = new StringBuilder(File.ReadAllText(Common.SuspendBatchEmailTemplate));
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

        public static bool SendInvalidMRDFOutFoundNotificationPCIEmail(string printFileName, string fileName, string errorMsg)
        {
            bool isEmailSent = false;

            try
            {
                using (MailMessage msg = new MailMessage())
                {
                    msg.IsBodyHtml = true;
                    msg.To.Add(Common.MRDFIssue_Email_To);
                    msg.CC.Add(Common.MRDFIssue_Email_Cc);
                    msg.Subject = Common.NoRecordsInMRDFSubject;
                    msg.Priority = MailPriority.High;

                    StringBuilder sb = new StringBuilder(File.ReadAllText(Common.SuspendBatchEmailTemplate));
                    sb = sb.Replace("<%Reason%>", errorMsg + "<br/><br/>Batch Name: {0} <br/><br/>".Fill(Path.GetFileNameWithoutExtension(printFileName)) + "MRDF OUT File Name : {0}".Fill(fileName));

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

        public static bool SendUnexpectedNotificationEmail(string errorMsg)
        {
            bool isEmailSent = false;

            try
            {
                using (MailMessage msg = new MailMessage())
                {
                    msg.To.Add(Common.UnexpectedEmail_ToEmail);
                    msg.CC.Add(Common.UnexpectedEmail_CcEmail);
                    msg.Subject = Common.ApplicationEnvironment + " : " + Common.UnexpectedEmail_Subject;
                    msg.Priority = MailPriority.High;

                    msg.Body = errorMsg;

                    Common.Email_SMTPClient.Send(msg);

                    isEmailSent = true;
                }
            }
            catch (Exception err)
            {
                log.Error("Error occured while sending batch submitted notification to internal team", err);
            }

            return isEmailSent;
        }
    }
}
