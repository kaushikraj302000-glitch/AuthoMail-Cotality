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
    class FedExPickUp
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        internal static void CheckPickedUpPackages()
        {
            try
            {
                List<MailPieces> mailPieces = new List<MailPieces>();
                using (IReportService reportSvc = new ReportService())
                {
                    //Here we are passing dates in EST format because we are looking for est dates column
                    DateTime todayEffectiveDate = Common.EstToday;

                    mailPieces = reportSvc.GetFedExPickedUpReport(todayEffectiveDate, 0);
                    if (mailPieces != null && mailPieces.Count > 0)
                    {
                        CheckFedExTrackingInfo checkTrackingInfo = new CheckFedExTrackingInfo();
                        checkTrackingInfo.GetTrackingInformation(mailPieces);
                    }

                    //Get all records after updating pickedup information and generate report
                    mailPieces = reportSvc.GetFedExPickedUpReport(todayEffectiveDate);
                    if (mailPieces != null && mailPieces.Count > 0)
                    {
                        StringBuilder fedExReport = GetFedExReport(mailPieces);
                        string fedExReportPath = Path.Combine(Config.Common_AuthoMailRootFolder, "USAA", "FedExReport_{0}.csv".Fill(todayEffectiveDate.ToString("MMddyyyyhhmmtt")));
                        File.WriteAllText(fedExReportPath, fedExReport.ToString());
                        SendReportToCLTeam(fedExReportPath, todayEffectiveDate);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error occured while checkign for FedEx packages picked up or not", ex);
            }
        }

        private static StringBuilder GetFedExReport(List<MailPieces> mailPieces)
        {
            var columnName = String.Join(",", "Batch Name", "Loan Number", "Borrower Name", "Property Address", "Property City", "Property State", "Property Zip", "Tracking Number", "Received Date (Local)", "Document Type", "FedEx Status", "Status Time");

            //generate using report format and send it to CL team
            StringBuilder mailPiecesReport = new StringBuilder(columnName);
            mailPiecesReport.AppendLine();
            foreach (MailPieces mailPiece in mailPieces)
            {
                var line = String.Join(",",
                    mailPiece.MailBatches.BatchName,
                    mailPiece.BusinessFriendlyIdValue,
                    (mailPiece.RcptLastName + " " + mailPiece.RcptFirstName).ToCsvString(),
                    (mailPiece.RcptAddressStreet + " " + mailPiece.RcptAddressAptOrSuiteOrOther).ToCsvString(),
                    mailPiece.RcptAddressCity.ToCsvString(),
                    mailPiece.RcptAddressState,
                    mailPiece.RcptAddressZIP,
                    mailPiece.TrackingNo,
                    mailPiece.MailBatches.ReceivedOn.Value.UtcToEst(),
                    mailPiece.DocumentOrPackageType,
                    mailPiece.IsPickedUp == 1 ? "Picked Up" : "Not Picked",
                    mailPiece.PickedUpOn);
                mailPiecesReport.AppendLine(line);
            }

            return mailPiecesReport;
        }

        private static bool SendReportToCLTeam(string fedExReportPath, DateTime todayDate)
        {
            bool isEmailSent = false;

            try
            {
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = false;
                        msg.To.Add(AppConfig.FedExReport_Email_To);
                        msg.CC.Add(AppConfig.FedExReport_Email_Cc);
                        msg.Subject = "{0} : {1} {2}".Fill(Config.Common_Environment, "USAA", AppConfig.FedExReportSubject);
                        msg.Priority = MailPriority.Normal;
                        msg.Body = "FedEx Status Report From : {0} To {1}".Fill(todayDate.ToString("MM/dd/yyyy hh:mm tt"));

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
