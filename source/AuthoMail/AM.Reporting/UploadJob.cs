using AM.Entities;
using AM.Services;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;
using System.Reflection;
using System.Net.Mail;
using System.Configuration;

namespace AM.Reporting
{
    class UploadJob
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void ReportPreppedProcess()
        {
            //For now we are using 101 for USAA
            try
            {
                using (IClientService clientSvc = new ClientService())
                using (IReportService reportSvc = new ReportService())
                using (ICommonService commonSvc = new CommonService())
                {
                    Client client = clientSvc.GetClientByAppCode("USAA");

                    if (client != null)
                    {
                        List<ClientSFTChannel> sftChannelList = clientSvc.GetClientSFTChannelMapDetails(client.Id);

                        if (sftChannelList != null && sftChannelList.Count > 0)
                        {
                            foreach (var sftChannel in sftChannelList)
                            {
                                SFTChannel sftChannelDetails = clientSvc.GetSFTChannelDetailsBySFTChannelId(sftChannel.SFTChannelId);

                                if (sftChannelDetails == null)
                                {
                                    log.Debug("No records found for Client : {0) And SFTChannel-Id : {1}".Fill(client.AppCode, sftChannel.SFTChannelId));
                                    continue;
                                }

                                var startDate = (DateTime.UtcNow.UtcToEst().Date.AddDays(-1) + new TimeSpan(19, 45, 01)).EstToUtc();
                                var endDate = (DateTime.UtcNow.UtcToEst().Date + new TimeSpan(19, 45, 00)).EstToUtc();

                                var reportData = reportSvc.GetSummaryControlReport(client.Id, startDate, endDate, sftChannelDetails.Code).OrderBy(s => s.BatchId).ToList();

                                if (reportData != null && reportData.Count > 0)
                                {
                                    StringBuilder summaryReport = new StringBuilder("BatchID,Id,Channel,Package Name,Loan Number,File Name, Date Received,Date Printed,Date Mailed,Client Code,Tracking Number,Shipping Method,Customer Identifier,First Name,Last Name,Recipient Street Address,Recipient City,Recipient State,Recipient Zip,RequestId,Error Code,Error Text,Print Location,Pages Shipped");
                                    summaryReport.AppendLine();

                                    foreach (var mailPiece in reportData)
                                    {
                                        summaryReport.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23}",
                                            mailPiece.BatchId,
                                            mailPiece.BatchUniqueId,
                                            sftChannelDetails.Title,
                                            mailPiece.DocumentOrPackageType,
                                            mailPiece.BusinessFriendlyIdValue,
                                            mailPiece.FileName,
                                            mailPiece.ReceivedOnClient,
                                            mailPiece.PrintedOnClient,
                                            mailPiece.MailedOnClient,
                                            mailPiece.ClientCode,
                                            mailPiece.TrackingNo,
                                            mailPiece.ShippingMethod,
                                            mailPiece.CustomerIdentifier,
                                            mailPiece.RcptFirstName,
                                            mailPiece.RcptLastName,
                                            mailPiece.RcptAddressStreet,
                                            mailPiece.RcptAddressCity,
                                            mailPiece.RcptAddressState,
                                            mailPiece.RcptAddressZIP,
                                            mailPiece.RequestId,
                                            mailPiece.ErrorCode,
                                            mailPiece.ErrorDesc,
                                            mailPiece.PrintLocation.Replace(",", " "),
                                            mailPiece.PagesShipped));
                                    }

                                    string summaryReportFileName = "{0}_{1}_{2}_{3}.csv".Fill("SummaryControlReport", client.AppCode, sftChannelDetails.Code, DateTime.UtcNow.UtcToClientTimeZone(client.TimeZone).ToString("MM-dd-yyyy"));


                                    string summaryReportPath = Path.Combine(Common.AuthoMailRootFolder, summaryReportFileName);
                                    try
                                    {
                                        using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(summaryReport.ToString())))
                                        {
                                            using (FileStream file = new FileStream(summaryReportPath, FileMode.Create, FileAccess.Write))
                                            {
                                                ms.WriteTo(file);
                                            }
                                        }

                                        SFTUtil sftSite = null;

                                        sftSite = new SFTUtil(Common.GetProtocolType(sftChannelDetails.Protocol), sftChannelDetails.Host, sftChannelDetails.UserName, sftChannelDetails.Password, 300000);
                                        sftSite.Connect();

                                        if (sftSite.IsDownloadDirectoryAvailable(Common.ReportUploadPath))
                                        {
                                            string ftpUuploadPath = Path.Combine(Common.ReportUploadPath, summaryReportFileName);
                                            sftSite.UploadFile(summaryReportPath, ftpUuploadPath);
                                            log.Debug("{0}-File successfully uploaded to client's location.".Fill(summaryReportFileName));
                                        }
                                        else
                                        {
                                            log.Error("Upload path does not exist for Client-{0} and SFT Channel Id-{1}".Fill(client.Name, sftChannelDetails.Code));
                                        }

                                        SendDownloadNotificationEmail(summaryReportPath, client.TimeZone, startDate, endDate);
                                    }
                                    catch (Exception ex)
                                    {
                                        log.Error("Error occured while uploading Summary control report :" + ex);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error occured while uploading Summary report on SFT site.", ex);
            }
        }

        public bool SendDownloadNotificationEmail(string reportPath, string clienttimeZone, DateTime startDate, DateTime endDate)
        {
            bool isEmailSent = false;

            try
            {
                string subject = ConfigurationManager.AppSettings["SummaryNotificationSubject"];
                string email_To = ConfigurationManager.AppSettings["SummaryNotification_Email_To"];
                string email_Cc = ConfigurationManager.AppSettings["SummaryNotification_Email_Cc"];
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = true;
                        msg.To.Add(email_To);
                        msg.CC.Add(email_Cc);
                        msg.Subject = subject + DateTime.UtcNow.UtcToClientTimeZone(clienttimeZone).ToString("MM-dd-yyyy");
                        msg.Priority = MailPriority.Normal;
                        msg.Body = "Summary Control Report From Date {0} to {1}".Fill(startDate.UtcToClientTimeZone(clienttimeZone), endDate.UtcToClientTimeZone(clienttimeZone));

                        msg.Attachments.Add(new Attachment(reportPath));

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
    }
}
