using AM.Lib;
using AM.Services;
using AM.Services.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;

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

            SendReportToCLTeam(reportFilePath, subject.Fill(currentTime.ToString("MM/dd/yyyy")), $"GOS Report For : {currentTime:MM/dd/yyyy}", AppConfig.GOSReconEMailTo, AppConfig.GOSReconEMailCc);
        }

        private static bool SendReportToCLTeam(string reconReportPath, string subject, string body, string to, string cc)
        {
            bool isEmailSent = false;

            try
            {
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = true;
                        msg.To.Add(to);
                        msg.CC.Add(cc);
                        msg.Subject = $"{Config.Common_Environment} : {subject}";
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
                DateTime endTime = Common.EstToday.AddDays(-1);
                DateTime startTime = endTime.AddDays(-7);
                using (IReportService reportSvc = new ReportService())
                {
                    CarrierTrackingInfo.CheckForShipmentTracking("GOS");
                    CarrierTrackingInfo.CheckForUSPSeVSShipmentTracking("GOS");
                    //NOTE : Please change UTC to EST code when Daylight saving is off in service call
                    var gosBatchDetails = reportSvc.GetGOSDataPointReport(startTime, endTime);

                    var reportGOS = gosBatchDetails.Select(r => new { r.AppCode, r.BatchName, r.BorrowerName, r.LoanNumber, r.ProductCode, r.DocumentOrPackageType, r.FileName, r.ReceivedOnEst, r.ShippedOnEst, r.TotalPages, r.TrackingNo, r.DeliveryStatus, r.DeliveryDate, r.ReturnTrackingNo, r.ReturnDeliveryStatus, r.ReturnDeliveryDate, r.Week }).ToList();

                    if (gosBatchDetails.Count > 0)
                    {
                        string reportFilePath = Path.Combine(AppConfig.GOSReconReportPath, $"GOSDataPointReport_{startTime:MMddyyyy}_To_{endTime:MMddyyyy}.xls");

                        var dt = reportGOS.ToDataTable();

                        XLS excel = new XLS(AppConfig.GOSDataPointReportTemplate);

                        excel.LoadData("Sheet1", dt, true);
                        excel.SaveAs(reportFilePath);

                        StringBuilder sb = new StringBuilder(File.ReadAllText(AppConfig.GOSDataPointEMailTemplate));
                        var fedexOWRecords = gosBatchDetails.Where(x => x.ShippingCarrier == "FedEx");
                        var uspsOWRecords = gosBatchDetails.Where(x => x.ShippingCarrier == "USPS");

                        var fedexRetRecords = gosBatchDetails.Where(x => x.ReturnShippingCarrier == "FedEx");
                        var uspsRetRecords = gosBatchDetails.Where(x => x.ReturnShippingCarrier == "USPS");

                        sb = sb.Replace("<%FedEx_ODelivered%>", fedexOWRecords.Count(t => t.DeliveryStatus != null && t.DeliveryStatus.ToLower().Contains("delivered")).ToString());
                        sb = sb.Replace("<%FedEx_OInTransit%>", fedexOWRecords.Count(t => t.DeliveryStatus != null && !t.DeliveryStatus.ToLower().Contains("delivered")).ToString());
                        sb = sb.Replace("<%FedEx_OPending%>", fedexOWRecords.Count(t => t.DeliveryStatus.IsNullOrWhiteSpace()).ToString());

                        sb = sb.Replace("<%FedEx_RDelivered%>", fedexRetRecords.Count(t => t.ReturnDeliveryStatus != null && t.ReturnDeliveryStatus.ToLower().Contains("delivered")).ToString());
                        sb = sb.Replace("<%FedEx_RInTransit%>", fedexRetRecords.Count(t => t.ReturnDeliveryStatus != null && !t.ReturnDeliveryStatus.ToLower().Contains("delivered")).ToString());
                        sb = sb.Replace("<%FedEx_RPending%>", fedexRetRecords.Count(t => t.ReturnDeliveryStatus.IsNullOrWhiteSpace()).ToString());

                        sb = sb.Replace("<%USPS_ODelivered%>", uspsOWRecords.Count(t => t.DeliveryStatus != null && t.DeliveryStatus.ToLower().Contains("delivered")).ToString());
                        sb = sb.Replace("<%USPS_OInTransit%>", uspsOWRecords.Count(t => t.DeliveryStatus != null && !t.DeliveryStatus.ToLower().Contains("delivered")).ToString());
                        sb = sb.Replace("<%USPS_OPending%>", uspsOWRecords.Count(t => t.DeliveryStatus.IsNullOrWhiteSpace()).ToString());

                        sb = sb.Replace("<%USPS_RDelivered%>", uspsRetRecords.Count(t => t.ReturnDeliveryStatus != null && t.ReturnDeliveryStatus.ToLower().Contains("delivered")).ToString());
                        sb = sb.Replace("<%USPS_RInTransit%>", uspsRetRecords.Count(t => t.ReturnDeliveryStatus != null && !t.ReturnDeliveryStatus.ToLower().Contains("delivered")).ToString());
                        sb = sb.Replace("<%USPS_RPending%>", uspsRetRecords.Count(t => t.ReturnDeliveryStatus.IsNullOrWhiteSpace()).ToString());

                        SendReportToCLTeam(reportFilePath, $"GOS Tracking Status Report From {startTime:MM/dd/yyyy} To {endTime:MM/dd/yyyy}", sb.ToString(), AppConfig.GOSDataPointEMailTo, AppConfig.GOSDataPointEMailCc);
                    }
                }
                log.Debug("GOS Data Point report Execution completed at {0} Est".Fill(Common.Now.UtcToEst()));
            }
            catch (Exception ex)
            {
                log.Error("Error occured while generating GOS recon report", ex);
            }
        }

        internal static void GenerateGOSConsolidatedDataPointReport(string clientList)
        {
            try
            {
                log.Debug("GOS Consolidated report Execution Triggered at {0} Est".Fill(Common.Now.UtcToEst()));
                var today = Common.EstToday;
                var month = new DateTime(today.Year, today.Month, 1);
                var startTime = month.AddMonths(-1);
                var endTime = month.AddDays(-1);

                using (IReportService reportSvc = new ReportService())
                {
                    var clientCodes = clientList.IsNullOrWhiteSpace() ? null : clientList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    CarrierTrackingInfo.CheckForShipmentTracking(null, clientCodes, startTime, endTime);

                    CarrierTrackingInfo.CheckForUSPSeVSShipmentTracking(null, clientCodes, startTime, endTime);
                    //NOTE : Please change UTC to EST code when Daylight saving is off in service call
                    var gosBatchDetails = reportSvc.GetGOSDataPointReport(startTime, endTime, clientCodes);

                    var reportGOS = gosBatchDetails.Select(r => new { r.AppCode, r.BatchName, r.BorrowerName, r.LoanNumber, r.ProductCode, r.DocumentOrPackageType, r.FileName, r.ReceivedOnEst, r.ShippedOnEst, r.TotalPages, r.TrackingNo, r.ShipmentStatus, r.DeliveryStatus, r.DeliveryDate, r.ReturnTrackingNo, r.ReturnDeliveryStatus, r.ReturnDeliveryDate, r.Week }).ToList();

                    if (gosBatchDetails.Count > 0)
                    {
                        string reportFilePath = Path.Combine(AppConfig.GOSReconReportPath, $"GOSDataPointReport_{startTime:MMddyyyy}_To_{endTime:MMddyyyy}.xls");

                        var dt = reportGOS.ToDataTable();

                        XLS excel = new XLS(AppConfig.GOSDataPointReportTemplate);

                        excel.LoadData("Sheet1", dt, true);
                        excel.SaveAs(reportFilePath);

                        StringBuilder sb = new StringBuilder(File.ReadAllText(AppConfig.GOSDataPointEMailTemplate));
                        var fedexOWRecords = gosBatchDetails.Where(x => x.ShippingCarrier == "FedEx");
                        var uspsOWRecords = gosBatchDetails.Where(x => x.ShippingCarrier == "USPS");

                        var fedexRetRecords = gosBatchDetails.Where(x => x.ReturnShippingCarrier == "FedEx");
                        var uspsRetRecords = gosBatchDetails.Where(x => x.ReturnShippingCarrier == "USPS");

                        sb = sb.Replace("<%FedEx_ODelivered%>", fedexOWRecords.Count(t => t.DeliveryStatus != null && t.DeliveryStatus.ToLower().Contains("delivered")).ToString());
                        sb = sb.Replace("<%FedEx_OInTransit%>", fedexOWRecords.Count(t => t.DeliveryStatus != null && !t.DeliveryStatus.ToLower().Contains("delivered")).ToString());
                        sb = sb.Replace("<%FedEx_OPending%>", fedexOWRecords.Count(t => t.DeliveryStatus.IsNullOrWhiteSpace()).ToString());

                        sb = sb.Replace("<%FedEx_RDelivered%>", fedexRetRecords.Count(t => t.ReturnDeliveryStatus != null && t.ReturnDeliveryStatus.ToLower().Contains("delivered")).ToString());
                        sb = sb.Replace("<%FedEx_RInTransit%>", fedexRetRecords.Count(t => t.ReturnDeliveryStatus != null && !t.ReturnDeliveryStatus.ToLower().Contains("delivered")).ToString());
                        sb = sb.Replace("<%FedEx_RPending%>", fedexRetRecords.Count(t => t.ReturnDeliveryStatus.IsNullOrWhiteSpace()).ToString());

                        sb = sb.Replace("<%USPS_ODelivered%>", uspsOWRecords.Count(t => t.DeliveryStatus != null && t.DeliveryStatus.ToLower().Contains("delivered")).ToString());
                        sb = sb.Replace("<%USPS_OInTransit%>", uspsOWRecords.Count(t => t.DeliveryStatus != null && !t.DeliveryStatus.ToLower().Contains("delivered")).ToString());
                        sb = sb.Replace("<%USPS_OPending%>", uspsOWRecords.Count(t => t.DeliveryStatus.IsNullOrWhiteSpace()).ToString());

                        sb = sb.Replace("<%USPS_RDelivered%>", uspsRetRecords.Count(t => t.ReturnDeliveryStatus != null && t.ReturnDeliveryStatus.ToLower().Contains("delivered")).ToString());
                        sb = sb.Replace("<%USPS_RInTransit%>", uspsRetRecords.Count(t => t.ReturnDeliveryStatus != null && !t.ReturnDeliveryStatus.ToLower().Contains("delivered")).ToString());
                        sb = sb.Replace("<%USPS_RPending%>", uspsRetRecords.Count(t => t.ReturnDeliveryStatus.IsNullOrWhiteSpace()).ToString());

                        SendReportToCLTeam(reportFilePath, $"GOS Tracking Status Report From {startTime:MM/dd/yyyy} To {endTime:MM/dd/yyyy}", sb.ToString(), AppConfig.GOSDataPointEMailTo, AppConfig.GOSDataPointEMailCc);
                    }
                }
                log.Debug("GOS Consolidated report Execution completed at {0} Est".Fill(Common.Now.UtcToEst()));
            }
            catch (Exception ex)
            {
                log.Error("Error occured while generating GOS Consolidated Report", ex);
            }
        }

        internal static void GenerateGOSDailyDataPointReport(string clientList)
        {
            try
            {
                log.Debug("GOS Daily report Execution Triggered at {0} Est".Fill(Common.Now.UtcToEst()));

                var statusExcel = new XLS(AppConfig.GOSStatusMappingFile);

                var statusTable = statusExcel.GetData("Sheet1");

                List<StatusMap> statusMaps = new List<StatusMap>();

                foreach (DataRow item in statusTable.Rows)
                {
                    statusMaps.Add(new StatusMap() { OriginalStatus = item["Original"].ToString(), ConvertedStatus = item["Converted"].ToString() });
                }

                using (IReportService reportSvc = new ReportService())
                {
                    var clientCodes = clientList.IsNullOrWhiteSpace() ? null : clientList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    var _3monthClientCodes = clientCodes.Where(x => AppConfig.GOSDaily3MonthAppCodes.Contains(x)).ToList();

                    var otherClientCodes = clientCodes.Except(_3monthClientCodes).ToList();

                    var today = Common.EstToday;
                    var startTime = today.AddMonths(-1);
                    var endTime = today.AddSeconds(-1);

                    GenerateReport(reportSvc, otherClientCodes, startTime, endTime, statusMaps);

                    if (AppConfig.GOSDaily3MonthAppCodes != null && AppConfig.GOSDaily3MonthAppCodes.Count() > 0)
                    {
                        startTime = today.AddMonths(-3);
                        endTime = today.AddSeconds(-1);
                        GenerateReport(reportSvc, _3monthClientCodes, startTime, endTime, statusMaps);
                    }
                }
                log.Debug("GOS Daily report Execution completed at {0} Est".Fill(Common.Now.UtcToEst()));
            }
            catch (Exception ex)
            {
                log.Error("Error occured while generating GOS Consolidated Report", ex);
            }
        }

        private static void GenerateReport(IReportService reportSvc, List<string> clientCodes, DateTime startTime, DateTime endTime, List<StatusMap> statusMaps)
        {
            CarrierTrackingInfo.CheckForShipmentTracking(null, clientCodes, startTime, endTime);

            CarrierTrackingInfo.CheckForUSPSeVSShipmentTracking(null, clientCodes, startTime, endTime);

            var gosBatchDetails = reportSvc.GetGOSDataPointReport(startTime, endTime, clientCodes);

            gosBatchDetails.ForEach(x =>
            {
                if (statusMaps.Any(s => x.DeliveryStatus != null && x.DeliveryStatus.Contains(s.OriginalStatus)))
                {
                    x.ConvertedDeliveryStatus = statusMaps.Distinct().SingleOrDefault(c => x.DeliveryStatus.Contains(c.OriginalStatus)).ConvertedStatus.ToTitleCase();
                }
                else
                {
                    x.ConvertedDeliveryStatus = x.DeliveryStatus.IsNullOrWhiteSpace() ? "No Tracking Information" : x.DeliveryStatus.ToTitleCase();
                }

                if (statusMaps.Any(s => x.ReturnDeliveryStatus != null && x.ReturnDeliveryStatus.Contains(s.OriginalStatus)))
                {
                    x.ConvertedReturnDeliveryStatus = statusMaps.SingleOrDefault(c => x.ReturnDeliveryStatus.Contains(c.OriginalStatus)).ConvertedStatus.ToTitleCase();
                }
                else
                {
                    x.ConvertedReturnDeliveryStatus = x.ReturnDeliveryStatus.IsNullOrWhiteSpace() && !x.ReturnTrackingNo.IsNullOrWhiteSpace() ? "No Tracking Information" : x.ReturnDeliveryStatus.ToTitleCase();
                }
            });

            var appCodeEmailList = AppConfig.GOSDailyEmailListByAppCode;

            foreach (var appCode in clientCodes)
            {
                //NOTE : Please change UTC to EST code when Daylight saving is off in service call
                var appCodeWiseData = gosBatchDetails.Where(c => c.AppCode == appCode);

                var reportGOS = appCodeWiseData.Select(r => new { r.LoanNumber, r.BorrowerName, r.ProductCode, r.ReceivedOnEst, r.TrackingNo, r.ShippingCarrier, r.ConvertedDeliveryStatus, r.DeliveryStatus, r.DeliveryDate, r.ReturnTrackingNo, r.ReturnShippingCarrier, r.ConvertedReturnDeliveryStatus, r.ReturnDeliveryStatus, r.ReturnDeliveryDate, r.Week }).ToList();

                if (gosBatchDetails.Count > 0)
                {
                    string reportFilePath = Path.Combine(AppConfig.GOSReconReportPath, $"{appCode}_DataPointReport_{Common.EstToday:MMddyyyy}.xls");

                    var dt = reportGOS.ToDataTable();

                    XLS excel = new XLS(AppConfig.GOSDailyDataPointReportTemplate);

                    excel.LoadData("Sheet1", dt, true);

                    excel.SaveAs(reportFilePath);

                    StringBuilder sb = new StringBuilder(File.ReadAllText(AppConfig.GOSDailyDataPointEMailTemplate));

                    var lstByOnGoingProductCode = appCodeWiseData.GroupBy(x => x.ProductCode);

                    var lstByReturnProductCode = appCodeWiseData.Where(x => !x.ReturnTrackingNo.IsNullOrWhiteSpace()).GroupBy(x => x.ProductCode);

                    StringBuilder ongoingRows = new StringBuilder();
                    foreach (var ongoingProdlst in lstByOnGoingProductCode)
                    {
                        string tblRowData = "";
                        tblRowData += $"<td>{ongoingProdlst.Key}</td>";
                        tblRowData += $"<td>{ongoingProdlst.Count(x => x.ShippingCarrier == "FedEx")}</td>";
                        tblRowData += $"<td>{ongoingProdlst.Count(x => x.ShippingCarrier == "USPS" && x.ShippingServiceType == "PRIORITY")}</td>";
                        tblRowData += $"<td>{ongoingProdlst.Count(x => x.ShippingCarrier == "USPS" && (x.ShippingServiceType == "IMB" || x.ShippingServiceType == "FIRSTCLASS"))}</td>";
                        tblRowData += $"<td>{ongoingProdlst.Count()}</td>";

                        string tblStatusRowData = "";
                        foreach (var lstByStatus in ongoingProdlst.GroupBy(x => x.ConvertedDeliveryStatus.ToTitleCase()))
                        {
                            tblStatusRowData += $"<tr><td style='text-align: right;'>{lstByStatus.Key}</td>";
                            tblStatusRowData += $"<td>{lstByStatus.Count(x => x.ShippingCarrier == "FedEx")}</td>";
                            tblStatusRowData += $"<td>{lstByStatus.Count(x => x.ShippingCarrier == "USPS" && x.ShippingServiceType == "PRIORITY")}</td>";
                            tblStatusRowData += $"<td>{lstByStatus.Count(x => x.ShippingCarrier == "USPS" && (x.ShippingServiceType == "IMB" || x.ShippingServiceType == "FIRSTCLASS"))}</td>";
                            tblStatusRowData += $"<td>{lstByStatus.Count()}</td></tr>";
                        }
                        ongoingRows.AppendLine($"<tr class='trProduct'>{tblRowData}</tr>");
                        ongoingRows.AppendLine($"{tblStatusRowData}");
                    }

                    StringBuilder returnRows = new StringBuilder();
                    foreach (var returnProdlst in lstByReturnProductCode)
                    {
                        string tblRowData = "";
                        tblRowData += $"<td>{returnProdlst.Key}</td>";
                        tblRowData += $"<td>{returnProdlst.Count(x => x.ReturnShippingCarrier == "FedEx")}</td>";
                        tblRowData += $"<td>{returnProdlst.Count(x => x.ReturnShippingCarrier == "USPS" && x.ShippingServiceType == "PRIORITY")}</td>";
                        tblRowData += $"<td>{returnProdlst.Count(x => x.ReturnShippingCarrier == "USPS" && (x.ShippingServiceType == "IMB" || x.ShippingServiceType == "FIRSTCLASS"))}</td>";
                        tblRowData += $"<td>{returnProdlst.Count()}</td>";

                        string tblStatusRowData = "";
                        foreach (var lstByStatus in returnProdlst.GroupBy(x => x.ConvertedReturnDeliveryStatus.ToTitleCase()))
                        {
                            tblStatusRowData += $"<tr><td style='text-align: right;'>{lstByStatus.Key}</td>";
                            tblStatusRowData += $"<td>{lstByStatus.Count(x => x.ReturnShippingCarrier == "FedEx")}</td>";
                            tblStatusRowData += $"<td>{lstByStatus.Count(x => x.ReturnShippingCarrier == "USPS" && x.ShippingServiceType == "PRIORITY")}</td>";
                            tblStatusRowData += $"<td>{lstByStatus.Count(x => x.ReturnShippingCarrier == "USPS" && (x.ShippingServiceType == "IMB" || x.ShippingServiceType == "FIRSTCLASS"))}</td>";
                            tblStatusRowData += $"<td>{lstByStatus.Count()}</td></tr>";
                        }
                        returnRows.AppendLine($"<tr class='trProduct'>{tblRowData}</tr>");
                        returnRows.AppendLine($"{tblStatusRowData}");
                    }

                    sb = sb.Replace("<%AppCode%>", appCode);

                    sb = sb.Replace("{OngoingData}", ongoingRows.ToString());
                    sb = sb.Replace("{ReturnData}", returnRows.ToString());
                    sb = sb.Replace("{OngoingGrandTotal}", $"<tr><td>Grand Total</td><td>{gosBatchDetails.Count(x => x.ShippingCarrier == "FedEx" && x.AppCode == appCode)}</td><td>{gosBatchDetails.Count(x => (x.ShippingCarrier == "USPS" && x.ShippingServiceType == "PRIORITY") && x.AppCode == appCode)}</td><td>{gosBatchDetails.Count(x => (x.ShippingCarrier == "USPS" && (x.ShippingServiceType == "IMB" || x.ShippingServiceType == "FIRSTCLASS")) && x.AppCode == appCode)}</td><td>{gosBatchDetails.Count(x => (x.ShippingCarrier == "FedEx" || (x.ShippingCarrier == "USPS" && x.ShippingServiceType == "PRIORITY")) && x.AppCode == appCode)}</td></tr>");
                    sb = sb.Replace("{ReturnGrandTotal}", $"<tr><td>Grand Total</td><td>{gosBatchDetails.Count(x => x.ReturnShippingCarrier == "FedEx" && x.AppCode == appCode)}</td><td>{gosBatchDetails.Count(x => x.ReturnShippingCarrier == "USPS" && x.AppCode == appCode)}</td><td>{gosBatchDetails.Count(x => (x.ReturnShippingCarrier == "FedEx" || x.ReturnShippingCarrier == "USPS") && x.AppCode == appCode)}</td></tr>");


                    SendReportToCLTeam(reportFilePath, $"{appCode} MTD Delivery Status Report For Date {Common.EstToday.Date.ToString("MM-dd-yyyy")}", sb.ToString(), appCodeEmailList[appCode], AppConfig.GOSDailyDataPointEMailCc);
                }
            }
        }
    }

    internal class StatusMap
    {
        public string OriginalStatus { get; set; }
        public string ConvertedStatus { get; set; }
    }
}
