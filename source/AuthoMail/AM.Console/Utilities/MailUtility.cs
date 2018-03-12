using AM.Entities;
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
        public static bool SendBatchSuspendedNotificationEmail(int printBatchId, string errorMsg)
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
        public static bool SendNoBatchReceivedNotificationEmail(DateTime cutOffTime)
        {
            bool isEmailSent = false;

            try
            {
                using (MailMessage msg = new MailMessage())
                {
                    msg.IsBodyHtml = true;
                    msg.To.Add(Common.Email_To);
                    msg.CC.Add(Common.Email_Cc);
                    msg.Subject = Common.NoBatchReceivedNotification_Subject;
                    msg.Priority = MailPriority.Normal;
                    msg.Body = string.Format("No batch available today till cutOffTime {0}.", cutOffTime.ToString("HH:mm:ss"));
                    if (Common.SendEmails)
                        Common.Email_SMTPClient.Send(msg);

                    isEmailSent = true;
                }
            }
            catch (Exception err)
            {
                log.Error(err);
            }

            return isEmailSent;
        }

        public static bool SendDownloadNotificationEmail(MailBatchInfo mbi, List<MailPieceDTO> mailPieces, List<MailPieceCustomInfo> mailPiecesCustomInfo)
        {
            bool isEmailSent = false;

            try
            {
                using (MailMessage msg = new MailMessage())
                {
                    msg.IsBodyHtml = true;
                    msg.To.Add(Common.Email_To);
                    msg.CC.Add(Common.Email_Cc);
                    msg.Subject = Common.DownloadEmail_Subject;
                    msg.Priority = MailPriority.Normal;

                    if (mailPieces.Count > 0)
                    {
                        #region Prepare email body

                        string downloadEmail_TD_SrNoStyle = "style='width:10%;border:solid windowtext 1.0pt;border-top:none;padding:0in 5.4pt 0in 5.4pt' valign='top' width='213'><p>";
                        string downloadEmail_TD_DescriptionStyle = "style='width:80%;border-top:none;border-left:none;border-bottom:solid windowtext 1.0pt;border-right:solid windowtext 1.0pt;padding:0in 5.4pt 0in 5.4pt'' valign='top' width='213'><p>";
                        string downloadEmail_TD_CountStyle = "style='width:10%;border-top:none;border-left:none;border-bottom:solid windowtext 1.0pt;border-right:solid windowtext 1.0pt;padding:0in 5.4pt 0in 5.4pt' valign='top' width='213'><p>";

                        StringBuilder sb = new StringBuilder(File.ReadAllText(Common.DownloadEmailTemplate));
                        sb = sb.Replace("<%FileName%>", mbi.MailBatch.BatchName);
                        sb = sb.Replace("<%yyyymmdd%>", Common.UtcToClientTimeZone(DateTime.UtcNow, mbi.Client.TimeZone).ToString("MM/dd/yyyy"));
                        sb = sb.Replace("<%hhmm%>", Common.UtcToClientTimeZone(DateTime.UtcNow, mbi.Client.TimeZone).ToString("hh:mm tt"));

                        StringBuilder data = new StringBuilder();
                        data.AppendLine(string.Format("<tr> <td {0} {1} <u></u><u></u></p> </td>", downloadEmail_TD_SrNoStyle, "1"));
                        data.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p> </td>", downloadEmail_TD_DescriptionStyle, "TOTAL LOANS RECEIVED"));
                        data.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p> </td> </tr>", downloadEmail_TD_CountStyle, mailPieces.Count));
                        data.AppendLine(string.Format("<tr> <td {0} {1} <u></u><u></u></p> </td>", downloadEmail_TD_SrNoStyle, "2"));
                        data.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p>  </td>", downloadEmail_TD_DescriptionStyle, "VALID LOANS"));
                        data.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p> </td> </tr>", downloadEmail_TD_CountStyle, mailPieces.Where(l => l.ErrorCode == OutComeCodes.No_Error).Count()));
                        data.AppendLine(string.Format("<tr> <td {0} {1} <u></u><u></u></p> </td>", downloadEmail_TD_SrNoStyle, "3"));
                        data.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p>  </td>", downloadEmail_TD_DescriptionStyle, "INVALID LOANS"));
                        data.AppendLine(string.Format("<td {0} {1} <u></u><u></u></p> </td> </tr>", downloadEmail_TD_CountStyle, mailPieces.Where(l => l.ErrorCode != OutComeCodes.No_Error).Count()));

                        sb = sb.Replace("<%trdata%>", data.ToString());
                        msg.Body = sb.ToString();

                        #endregion
                    }
                    else if (mailPieces.Count == 0)
                    {
                        msg.Body = string.Format("No records available in the batch file {0}.", mbi.MailBatch.BatchName);
                    }

                    if (msg.Body.Length > 0)
                    {
                        mbi.MailBatch.ReceivedOn = Common.UtcToClientTimeZone(mbi.MailBatch.ReceivedOn.Value, mbi.Client.TimeZone);

                        // Generate Error Report
                        List<MailPieceDTO> errorMailPieces = mailPieces.Where(p => p.ErrorCode != OutComeCodes.No_Error).ToList();

                        List<MailPieceCustomInfo> filteredCustomInfo = mailPiecesCustomInfo.Where(c => errorMailPieces.Select(data => data.Id).Contains(c.MailPieceId)).ToList();

                        if (errorMailPieces != null && errorMailPieces.Count > 0)
                        {
                            StringBuilder errorReport = GetErrorReport(mbi.MailBatch, errorMailPieces, filteredCustomInfo);
                            string errorReportFileName = string.Format("Error-{0}.txt", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));
                            msg.Attachments.Add(new Attachment(
                                new MemoryStream(
                                    Encoding.UTF8.GetBytes(errorReport.ToString().ToCharArray())), errorReportFileName));
                        }

                        Common.Email_SMTPClient.Send(msg);
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

        private static StringBuilder GetErrorReport(MailBatchDTO mailBatch, List<MailPieceDTO> mailPiecesWithErr, List<MailPieceCustomInfo> mailPiecesCustomInfo)
        {
            StringBuilder errorReport = new StringBuilder();

            if (mailPiecesWithErr.Count > 0)
            {
                errorReport = new StringBuilder("BatchID\tID\tPackage Name\tLoan Number\tFile Name\tDate Received\tFirst Name\tLast Name\tRecipient Street Address\tRecipient City\tRecipient State\tRecipient Zip\tError Code\tError Text\tShipping Method\tCustomer Identifier\tRequestID");
                //return errorReport;
                errorReport.AppendLine();
                foreach (MailPieceDTO mailPiece in mailPiecesWithErr)
                {
                    errorReport.AppendLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8} {9}\t{10}\t{11}\t{12}\t{13}\t{14}\t{15}\t{16}",
                        mailBatch.BatchName == null ? string.Empty : mailBatch.BatchName, mailPiece.UniqueId,
                        mailPiece.DocumentOrPackageType == null ? string.Empty : mailPiece.DocumentOrPackageType,
                        mailPiece.BusinessFriendlyIdValue == null ? string.Empty : mailPiece.BusinessFriendlyIdValue,
                        mailPiece.FileName == null ? string.Empty : mailPiece.FileName,
                        mailBatch.ReceivedOn == null ? string.Empty : mailBatch.ReceivedOn.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                        mailPiece.RcptFirstName == null ? string.Empty : mailPiece.RcptFirstName,
                        mailPiece.RcptLastName == null ? string.Empty : mailPiece.RcptLastName,
                        mailPiece.RcptAddressStreet == null ? string.Empty : mailPiece.RcptAddressStreet,
                        mailPiece.RcptAddressCity == null ? string.Empty : mailPiece.RcptAddressCity,
                        mailPiece.RcptAddressState == null ? string.Empty : mailPiece.RcptAddressState,
                        mailPiece.RcptAddressZIP == null ? string.Empty : mailPiece.RcptAddressZIP,
                        mailPiece.ErrorCode,
                        mailPiece.ErrorDesc == null ? string.Empty : mailPiece.ErrorDesc.Replace(',', ' ').Replace("\r\n", " "),
                        mailPiecesCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "ShippingMethod").FirstOrDefault() == null ? string.Empty : mailPiecesCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "ShippingMethod").FirstOrDefault().Value,
                        mailPiecesCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "CustomerIdentifier").FirstOrDefault() == null ? string.Empty : mailPiecesCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "CustomerIdentifier").FirstOrDefault().Value,
                        mailPiecesCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "RequestId").FirstOrDefault() == null ? string.Empty : mailPiecesCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "RequestId").FirstOrDefault().Value));
                }
            }

            return errorReport;
        }
    }
}
