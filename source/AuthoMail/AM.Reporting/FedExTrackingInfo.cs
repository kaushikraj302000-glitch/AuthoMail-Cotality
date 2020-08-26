using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Entities;
using AM.Lib;
using AM.Services;
using System.Configuration;
using System.Net.Mail;
using System.IO;

namespace AM.Reporting
{
    class FedExTrackingInfo
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        internal static void CheckForShipmentTracking()
        {
            try
            {
                log.Info("FedEx Pickup Process Started");
                List<FedExReport> mailPieces = new List<FedExReport>();
                using (IFedExService fedExTrackingSvc = new FedExService())
                using (IReportService reportSvc = new ReportService())
                {
                    DateTime dueDateStartTime = Common.Now.UtcToEst().AddDays(-7).ToStartDateTime();
                    //Here we are passing dates in EST format because we are looking for est dates column
                    DateTime dueDateEndTime = Common.Now.UtcToEst().ToEndDateTime();
                    log.Info("FedEx Tracking Process For Date And Time {0}".Fill(dueDateEndTime));
                    //UPDATE Pickupon list before generating reports.
                    fedExTrackingSvc.UpdateFedExShipmentInfo(dueDateStartTime, dueDateEndTime);
                }
                log.Info("FedEx Pickup Process Completed");
            }
            catch (Exception ex)
            {
                log.Error("Error occured while checkign for FedEx packages picked up or not", ex);
            }
        }

        private static string GenerateNotPickedEmailBody(List<FedExReport> notPickedMailPieces)
        {
            string bodyContent = @"<center><b>Not Picked Up Loans Details</b></center><br/><table border='1'><tbody><%REPLACE%></tbody></table>";
            StringBuilder fullContent = new StringBuilder();
            fullContent.AppendLine(@"<tr><td>Loan Number</td><td>Borrower Name</td><td>Document Type</td><td>Received On</td><td>Tracking Number</td><td>Due Date</td><td>Shipped On</td></tr>");
            foreach (var mailPiece in notPickedMailPieces)
            {
                fullContent.AppendLine(@"<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td><td>{6}</td></tr>".Fill(mailPiece.LoanNumber, (mailPiece.RcptLastName + " " + mailPiece.RcptFirstName), mailPiece.DocumentOrPackageType, mailPiece.ReceivedOn.Value.UtcToEst(), mailPiece.TrackingNo, mailPiece.DueDate.Value.UtcToEst(), mailPiece.ShippedOn.HasValue ? mailPiece.ShippedOn.Value.UtcToEst().ToString() : ""));
            }
            return bodyContent.Replace("<%REPLACE%>", fullContent.ToString());
        }

        private static StringBuilder GetFedExReport(List<FedExReport> mailPieces)
        {
            var columnName = String.Join(",", "Batch Name", "Loan Number", "Borrower Name", "Property Address", "Property City", "Property State", "Property Zip", "Tracking Number", "Received Date (Local)", "Document Type", "FedEx Status", "Status Time");

            //generate using report format and send it to CL team
            StringBuilder mailPiecesReport = new StringBuilder(columnName);
            mailPiecesReport.AppendLine();
            foreach (FedExReport mailPiece in mailPieces)
            {
                var line = String.Join(",",
                    mailPiece.BatchName,
                    mailPiece.LoanNumber.ToCsvString(),
                    (mailPiece.RcptLastName + " " + mailPiece.RcptFirstName).ToCsvString(),
                    (mailPiece.RcptAddressStreet + " " + mailPiece.RcptAddressAptOrSuiteOrOther).ToCsvString(),
                    mailPiece.RcptAddressCity.ToCsvString(),
                    mailPiece.RcptAddressState,
                    mailPiece.RcptAddressZIP,
                    mailPiece.TrackingNo.ToCsvString(),
                    mailPiece.ReceivedOn.Value.UtcToEst(),
                    mailPiece.DocumentOrPackageType,
                    mailPiece.IsPickedUp == 1 ? "Picked Up" : "Not Picked",
                    mailPiece.PickedUpOn.HasValue ? mailPiece.PickedUpOn.Value.UtcToEst() : mailPiece.PickedUpOn);
                mailPiecesReport.AppendLine(line);
            }

            return mailPiecesReport;
        }

        private static bool SendReportToCLTeam(string fedExReportPath, string tableBody, DateTime todayDate)
        {
            bool isEmailSent = false;

            try
            {
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = true;
                        msg.To.Add(AppConfig.FedExReport_Email_To);
                        msg.CC.Add(AppConfig.FedExReport_Email_Cc);
                        msg.Subject = "{0} : {1} {2} {3}".Fill(Config.Common_Environment, "USAA", AppConfig.FedExReportSubject, todayDate.ToString("MM-dd-yyyy hh:mm"));
                        msg.Priority = MailPriority.Normal;

                        string bodyTags = @"<!DOCTYPE html><html xmlns='http://www.w3.org/1999/xhtml'><head><meta http-equiv='Content-Style-Type' content='text/css'></head><body><center><%REPLACE%></center></body></html>";

                        msg.Body = bodyTags.Replace("<%REPLACE%>", tableBody);

                        if (File.Exists(fedExReportPath))
                            msg.Attachments.Add(new Attachment(fedExReportPath));

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
