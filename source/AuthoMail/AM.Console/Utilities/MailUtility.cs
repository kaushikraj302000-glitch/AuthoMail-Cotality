using AM.Entities;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;
using AM.Services;

namespace AM.Console
{
    public class MailUtility
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MailUtility));

        public static bool SendBatchSuspendedNotificationEmail(int printBatchId, string errorMsg)
        {
            bool isEmailSent = false;

            try
            {
                using (SmtpClient smtpClient = new SmtpClient())
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
                        smtpClient.Send(msg);

                        isEmailSent = true;
                    }
                }
            }
            catch (Exception err)
            {
                log.Error("Error occured while sending batch submitted notification to PCI", err);
            }

            return isEmailSent;
        }

        public static bool SendNoBatchReceivedNotificationEmail(DateTime cutOffTime)
        {
            bool isEmailSent = false;

            try
            {
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = true;
                        msg.To.Add(Config.Monitor_NoBatch_Email_To);
                        msg.CC.Add(Config.Monitor_NoBatch_Email_Cc);
                        msg.Subject = Config.Monitor_NoBatchReceivedNotification_Subject;
                        msg.Priority = MailPriority.Normal;
                        msg.Body = string.Format("No batch available today till cutOffTime {0}.", cutOffTime.ToString("HH:mm:ss"));
                        smtpClient.Send(msg);
                        isEmailSent = true;
                    }
                }
            }
            catch (Exception err)
            {
                log.Error(err);
            }

            return isEmailSent;
        }

        public static void SendUnexpectedExcelptionEmail(string exception)
        {
            try
            {
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = true;
                        msg.To.Add(Config.Monitor_Exception_Email_To);
                        msg.CC.Add(Config.Monitor_Exception_Email_Cc);
                        msg.Subject = Config.Monitor_ExceptionEmail_Subject;
                        msg.Priority = MailPriority.High;
                        msg.Body = "Unexpected exception occured during execution." + Environment.NewLine + exception;
                        smtpClient.Send(msg);
                    }
                }
            }
            catch (Exception err)
            {
                log.Error(err);
            }
        }

        public static bool SendDownloadNotificationEmail(MailBatchInfo mbi, BatchData batchData, string errorReportPath)
        {
            bool isEmailSent = false;

            try
            {
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = true;
                        msg.To.Add(Config.Monitor_DS_Email_To);
                        msg.CC.Add(Config.Monitor_DS_Email_Cc);
                        msg.Subject = Config.Monitor_DSEmail_Subject;
                        msg.Priority = MailPriority.Normal;

                        if (batchData.MailPieces.Count > 0)
                        {
                            #region Prepare email body

                            string downloadEmail_TD_SrNoStyle = "style='width:10%;border:solid windowtext 1.0pt;border-top:none;padding:0in 5.4pt 0in 5.4pt' valign='top' width='213'><p>";
                            string downloadEmail_TD_DescriptionStyle = "style='width:80%;border-top:none;border-left:none;border-bottom:solid windowtext 1.0pt;border-right:solid windowtext 1.0pt;padding:0in 5.4pt 0in 5.4pt'' valign='top' width='213'><p>";
                            string downloadEmail_TD_CountStyle = "style='width:10%;border-top:none;border-left:none;border-bottom:solid windowtext 1.0pt;border-right:solid windowtext 1.0pt;padding:0in 5.4pt 0in 5.4pt' valign='top' width='213'><p>";

                            StringBuilder sb = new StringBuilder(File.ReadAllText(Config.Monitor_DSEmailTemplate));
                            sb = sb.Replace("<%FileName%>", mbi.MailBatch.BatchName);
                            sb = sb.Replace("<%yyyymmdd%>", DateTime.UtcNow.UtcToClientTimeZone(mbi.Client.TimeZone).ToString("MM/dd/yyyy"));
                            sb = sb.Replace("<%hhmm%>", DateTime.UtcNow.UtcToClientTimeZone(mbi.Client.TimeZone).ToString("hh:mm tt"));

                            StringBuilder data = new StringBuilder();
                            data.AppendLine(string.Format("<tr> <td {0} {1} <u></u><u></u></p> </td>", downloadEmail_TD_SrNoStyle, "1"));
                            data.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p> </td>", downloadEmail_TD_DescriptionStyle, "TOTAL LOANS RECEIVED"));
                            data.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p> </td> </tr>", downloadEmail_TD_CountStyle, batchData.MailPieces.Count));
                            data.AppendLine(string.Format("<tr> <td {0} {1} <u></u><u></u></p> </td>", downloadEmail_TD_SrNoStyle, "2"));
                            data.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p>  </td>", downloadEmail_TD_DescriptionStyle, "VALID LOANS"));
                            data.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p> </td> </tr>", downloadEmail_TD_CountStyle, batchData.MailPieces.Where(l => l.ErrorCode == OutComeCodes.No_Error).Count()));
                            data.AppendLine(string.Format("<tr> <td {0} {1} <u></u><u></u></p> </td>", downloadEmail_TD_SrNoStyle, "3"));
                            data.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p>  </td>", downloadEmail_TD_DescriptionStyle, "INVALID LOANS"));
                            data.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p> </td> </tr>", downloadEmail_TD_CountStyle, batchData.MailPieces.Where(l => l.ErrorCode != OutComeCodes.No_Error).Count()));

                            sb = sb.Replace("<%trdata%>", data.ToString());
                            msg.Body = sb.ToString();

                            #endregion
                        }
                        else if (batchData.MailPieces.Count == 0)
                        {
                            msg.Body = string.Format("No records available in the batch file {0}.", mbi.MailBatch.BatchName);
                        }

                        if (msg.Body.Length > 0)
                        {

                            if (!string.IsNullOrEmpty(errorReportPath))
                                msg.Attachments.Add(new Attachment(errorReportPath));

                            smtpClient.Send(msg);
                            isEmailSent = true;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                log.Error(err);
            }

            return isEmailSent;
        }
    }
}
