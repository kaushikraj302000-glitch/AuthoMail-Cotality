using AM.Lib;
using AM.Services;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace AM.Reporting
{
    class GOSReports
    {
        private static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal static void GenerateGOSReceivedBatchReport()
        {
            try
            {
                log.Debug("GOS received batch report Execution Triggered at {0} Est".Fill(Common.Now.UtcToEst()));
                List<GOSMailPieceData> gosBatchDetails;
                DateTime currentTime = Common.EstToday;
                using (IReportService reportSvc = new ReportService())
                {
                    //NOTE : Please change UTC to EST code when Daylight saving is off in service call
                    gosBatchDetails = reportSvc.GetGOSDailyReconReport(currentTime);

                    if (gosBatchDetails.Count > 0)
                    {
                        string reportFilePath = Path.Combine(AppConfig.GOSReconReportPath, "GOSBatchReceivedReport_" + currentTime.ToString("MMddyyyy") + ".csv");
                        SendEmailNotificationToGOS(gosBatchDetails, currentTime, reportFilePath, AppConfig.GOSReceivedBatchEMailSubject);
                    }
                }
                log.Debug("GOS received batch report Execution completed at {0} Est".Fill(Common.Now.UtcToEst()));
            }
            catch (Exception ex)
            {
                log.Error("Error occured while generating GOS received batch", ex);
            }
        }

        private static void SendEmailNotificationToGOS(List<GOSMailPieceData> gosBatchDetails, DateTime currentTime, string reportFilePath, string subject)
        {
            var columnName = String.Join(",", "Client Code", "Batch Name", "Received On (Est)", "Loan Number", "Document Type", "File Name", "Tracking Number", "Shipping Status", "Total Pages", "Shipped On (Est)", "Print Location", "Error Description");

            StringBuilder reconReport = new StringBuilder(columnName);

            reconReport.AppendLine();
            foreach (var batch in gosBatchDetails)
            {
                var line = String.Join(",", batch.AppCode, batch.BatchName, batch.ReceivedOnEst, batch.LoanNumber, batch.DocumentOrPackageType, batch.FileName, batch.TrackingNo, batch.ShippingStatus, batch.TotalPages, batch.ShippedOnEst, batch.PrintLocation, batch.ErrorDesc);
                reconReport.AppendLine(line);
            }

            File.WriteAllText(reportFilePath, reconReport.ToString());

            SendReportToCLTeam(reportFilePath, currentTime, subject, $"GOS Report For : {currentTime:MM/dd/yyyy}");
        }

        private static bool SendReportToCLTeam(string reconReportPath, DateTime todayCutOffTime, string subject, string body)
        {
            bool isEmailSent = false;

            try
            {
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = true;
                        msg.To.Add(AppConfig.GOSReconEMailTo);
                        msg.CC.Add(AppConfig.GOSReconEMailCc);
                        msg.Subject = $"{Config.Common_Environment} : {subject.Fill(todayCutOffTime.ToString("MM/dd/yyyy"))}";
                        msg.Priority = MailPriority.Normal;
                        msg.Body = body;

                        if (File.Exists(reconReportPath))
                            msg.Attachments.Add(new Attachment(reconReportPath));

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

        internal static void GenerateGOSReconReport()
        {
            try
            {
                log.Debug("GOS recon report Execution Triggered at {0} Est".Fill(Common.Now.UtcToEst()));
                List<GOSMailPieceData> gosBatchDetails;
                DateTime currentTime = Common.EstToday.AddDays(-1);
                using (IReportService reportSvc = new ReportService())
                {

                    //NOTE : Please change UTC to EST code when Daylight saving is off in service call
                    gosBatchDetails = reportSvc.GetGOSDailyReconReport(currentTime);

                    if (gosBatchDetails.Count > 0)
                    {
                        string reportFilePath = Path.Combine(AppConfig.GOSReconReportPath, "GOSReconReport_" + currentTime.ToString("MMddyyyy") + ".csv");
                        SendEmailNotificationToGOS(gosBatchDetails, currentTime, reportFilePath, AppConfig.GOSReconEMailSubject);
                    }
                }
                log.Debug("GOS recon report Execution completed at {0} Est".Fill(Common.Now.UtcToEst()));
            }
            catch (Exception ex)
            {
                log.Error("Error occured while generating GOS recon report", ex);
            }
        }

        internal static void GenerateGOSDataPointReport()
        {
            try
            {
                log.Debug("GOS Data Point report Execution Triggered at {0} Est".Fill(Common.Now.UtcToEst()));
                List<GOSMailPieceData> gosBatchDetails;
                DateTime endTime = Common.EstToday.AddDays(-1);
                DateTime startTime = endTime.AddDays(-7);
                using (IReportService reportSvc = new ReportService())
                {
                    CarrierTrackingInfo.CheckForShipmentTracking("GOS");
                    CarrierTrackingInfo.CheckForUSPSeVSShipmentTracking("GOS");
                    //NOTE : Please change UTC to EST code when Daylight saving is off in service call
                    gosBatchDetails = reportSvc.GetGOSDataPointReport(startTime, endTime);

                    if (gosBatchDetails.Count > 0)
                    {
                        string reportFilePath = Path.Combine(AppConfig.GOSReconReportPath, $"GOSDataPointReport_{startTime:MMddyyyy}_To_{endTime:MMddyyyy}.csv");
                        SendEmailNotificationToGOSDataPoints(gosBatchDetails, endTime, reportFilePath, AppConfig.GOSDataPointEMailSubject);
                    }
                }
                log.Debug("GOS Data Point report Execution completed at {0} Est".Fill(Common.Now.UtcToEst()));
            }
            catch (Exception ex)
            {
                log.Error("Error occured while generating GOS recon report", ex);
            }
        }

        private static void SendEmailNotificationToGOSDataPoints(List<GOSMailPieceData> gosBatchDetails, DateTime currentTime, string reportFilePath, string subject)
        {
            var columnName = String.Join(",", "Client Code", "Batch Name", "Received On (Est)", "Loan Number", "Product Code", "Document Type", "File Name", "Tracking Number", "Shipping Status", "Total Pages", "Shipped On (Est)", "Print Location", "Delivery Status", "Delivery Date", "Shipment Status", "Return Tracking Number", "Return Delivery Status", "Return Delivery Date", "Error Description");

            StringBuilder reconReport = new StringBuilder(columnName);

            reconReport.AppendLine();
            foreach (var batch in gosBatchDetails)
            {
                var line = String.Join(",", batch.AppCode, batch.BatchName, batch.ReceivedOnEst, batch.LoanNumber, batch.ProductCode, batch.DocumentOrPackageType, batch.FileName, batch.TrackingNo, batch.ShippingStatus, batch.TotalPages, batch.ShippedOnEst, batch.PrintLocation, batch.DeliveryStatus, batch.DeliveryDateTime, batch.ShipmentStatus, batch.ReturnTrackingNo, batch.ReturnDeliveryStatus, batch.ReturnDeliveryDate, batch.ErrorDesc);
                reconReport.AppendLine(line);
            }

            File.WriteAllText(reportFilePath, reconReport.ToString());


            StringBuilder sb = new StringBuilder(File.ReadAllText(AppConfig.GOSDataPointEMailTemplate));
            var fedexOWRecords = gosBatchDetails.Where(x => x.ShippingCarrier == "FEDEX");
            var uspsOWRecords = gosBatchDetails.Where(x => x.ShippingCarrier == "USPS");

            var fedexRetRecords = gosBatchDetails.Where(x => x.ReturnShippingCarrier == "FEDEX");
            var uspsRetRecords = gosBatchDetails.Where(x => x.ReturnShippingCarrier == "USPS");

            sb = sb.Replace("<%FedEx_ODelivered%>", fedexOWRecords.Count(t => t.DeliveryStatus.ToLower().Contains("delivered")).ToString());
            sb = sb.Replace("<%FedEx_OInTransit%>", fedexOWRecords.Count(t => !t.DeliveryStatus.ToLower().Contains("delivered") && !t.DeliveryStatus.ToLower().Contains("created")).ToString());
            sb = sb.Replace("<%FedEx_OPending%>", fedexOWRecords.Count(t => t.DeliveryStatus.ToLower().Contains("created")).ToString());
            sb = sb.Replace("<%FedEx_RDelivered%>", fedexRetRecords.Count(t => t.ReturnDeliveryStatus.ToLower().Contains("delivered")).ToString());
            sb = sb.Replace("<%FedEx_RInTransit%>", fedexRetRecords.Count(t => !t.ReturnDeliveryStatus.ToLower().Contains("delivered") && !t.DeliveryStatus.ToLower().Contains("created")).ToString());
            sb = sb.Replace("<%FedEx_RPending%>", fedexRetRecords.Count(t => t.ReturnDeliveryStatus.ToLower().Contains("created")).ToString());

            sb = sb.Replace("<%USPS_ODelivered%>", uspsOWRecords.Count(t => t.DeliveryStatus.ToLower().Contains("delivered")).ToString());
            sb = sb.Replace("<%USPS_OInTransit%>", uspsOWRecords.Count(t => !t.DeliveryStatus.ToLower().Contains("delivered") && !t.DeliveryStatus.ToLower().Contains("created")).ToString());
            sb = sb.Replace("<%USPS_OPending%>", uspsOWRecords.Count(t => t.DeliveryStatus.ToLower().Contains("created")).ToString());
            sb = sb.Replace("<%USPS_RDelivered%>", uspsRetRecords.Count(t => t.ReturnDeliveryStatus.ToLower().Contains("delivered")).ToString());
            sb = sb.Replace("<%USPS_RInTransit%>", uspsRetRecords.Count(t => !t.ReturnDeliveryStatus.ToLower().Contains("delivered") && !t.DeliveryStatus.ToLower().Contains("created")).ToString());
            sb = sb.Replace("<%USPS_RPending%>", uspsRetRecords.Count(t => t.ReturnDeliveryStatus.ToLower().Contains("created")).ToString());

            SendReportToCLTeam(reportFilePath, currentTime, subject, sb.ToString());
        }
    }
}
