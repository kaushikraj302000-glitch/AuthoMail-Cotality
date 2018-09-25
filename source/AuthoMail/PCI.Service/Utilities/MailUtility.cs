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
        public static bool SendBatchSubmittedNotificationEmail(BatchInfo batchInfo, int mailPiecesCount)
        {
            bool isEmailSent = false;

            try
            {
                using (MailMessage msg = new MailMessage())
                {
                    msg.IsBodyHtml = true;

                    msg.To.Add(batchInfo.BatchSummary.PrintLocation == Common.PrintLocationCL ? Common.Email_ToCL : Common.Email_ToPCI);
                    msg.CC.Add(batchInfo.BatchSummary.PrintLocation == Common.PrintLocationCL ? Common.Email_CcCL : Common.Email_CcPCI);
                    //Here subject string contains "Batch Submitted To {Location} : {FileName} at {Time} Total Files : {FileCount} Total Page Count : {PageCount}"
                    msg.Subject = Common.SubmitBatchEmailSubject.Fill(batchInfo.BatchSummary.PrintLocation == Common.PrintLocationCL ? "Dallas CoreLogic" : "PCI", Path.GetFileName(batchInfo.BatchZipFilePath), Common.UtcToEst(DateTime.UtcNow).ToString("MM/dd/yyyy hh:mm tt") + " EST", mailPiecesCount, batchInfo.BatchSummary.Images);

                    msg.Priority = MailPriority.Normal;

                    StringBuilder sb = new StringBuilder(File.ReadAllText(Common.SubmitBatchEmailTemplate));
                    sb = sb.Replace("<%FileName%>", Path.GetFileName(batchInfo.BatchZipFilePath));
                    sb = sb.Replace("<%PrintLocation%>", batchInfo.BatchSummary.PrintLocation == Common.PrintLocationCL ? "Dallas CoreLogic" : "PCI");
                    sb = sb.Replace("<%yyyymmdd%>", Common.UtcToEst(DateTime.UtcNow).ToString("MM/dd/yyyy"));
                    sb = sb.Replace("<%hhmm%>", Common.UtcToEst(DateTime.UtcNow).ToString("hh:mm tt") + " EST");
                    sb = sb.Replace("<%TotalMailPieces%>", mailPiecesCount.ToString());
                    sb = sb.Replace("<%TotalPageCount%>", batchInfo.BatchSummary.Images.ToString());

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

        public static bool SendInvalidMRDFOutFoundNotificationPCIEmail(BatchSummary batchSummary, string buName, string fileName, string errorMsg)
        {
            bool isEmailSent = false;

            try
            {
                using (MailMessage msg = new MailMessage())
                {
                    msg.IsBodyHtml = true;
                    msg.To.Add(Common.MRDFIssue_Email_To);
                    msg.CC.Add(Common.MRDFIssue_Email_Cc);
                    msg.Subject = buName + " " + Common.NoRecordsInMRDFSubject;
                    msg.Priority = MailPriority.High;

                    StringBuilder sb = new StringBuilder(File.ReadAllText(Common.SuspendBatchEmailTemplate));
                    sb = sb.Replace("<%Reason%>", errorMsg + "<br/><br/>Batch Name: {0} <br/><br/>".Fill(Path.GetFileNameWithoutExtension(batchSummary.PrintFileName)) + "MRDF OUT File Name : {0}".Fill(fileName));

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
                    msg.To.Add(Common.MRDFIssue_Email_To);
                    msg.CC.Add(Common.MRDFIssue_Email_Cc);
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
