using AM.Lib;
using AM.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Reporting
{
    class AlaskaReports
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static void GenerateDailyOperationReport()
        {
            log.Debug($"Alaska Daily Operation report Execution Triggered at {Common.Now.UtcToEst()} Est");

            using (IReportService reportSvc = new ReportService())
            {
                var currentDate = Common.EstToday;
                var reportData = reportSvc.GetDailyDetailOperationsByDateOrBatch(102, currentDate, null, null);

                var columnName = String.Join(",", "Batch Name", "Unique Id", "Loan Number", "Document Type", "First Name", "Last Name", "Street", "City", "State", "Zip", "Received On (Est)", "Due Date (Est)", "Shipped On (Est)", "File Name", "Carrier", "Service Type", "Total Pages", "Total Blank Pages");

                StringBuilder reconReport = new StringBuilder(columnName);

                reconReport.AppendLine();
                foreach (var mp in reportData)
                {
                    var line = String.Join(",", mp.MailBatches.BatchName.ToCsvString(), mp.UniqueId, mp.BusinessFriendlyIdValue, mp.DocumentOrPackageType, mp.RcptFirstName, mp.RcptLastName, mp.RcptAddressStreet.ToCsvString(), mp.RcptAddressCity.ToCsvString(), mp.RcptAddressState.ToCsvString(), mp.RcptAddressZIP.ToCsvString(), mp.MailBatches.ReceivedOn.Value.UtcToEst().ToString("MM-dd-yyyy hh:mm:ss"), mp.DueDate.HasValue ? mp.DueDate.Value.UtcToEst().ToString("MM-dd-yyyy hh:mm:ss") : "", mp.ShippedOn.HasValue ? mp.ShippedOn.Value.UtcToEst().ToString("MM-dd-yyyy hh:mm:ss") : "", mp.FileName, mp.DocTypes.ShippingCarrier, mp.DocTypes.ShippingServiceType, mp.TotalPages, mp.TotalBlankPages);
                    reconReport.AppendLine(line);
                }
                string reportFilePath = Path.Combine(AppConfig.PCIReconReportPath, "AlaskaDailyOperationReport_" + Common.Now.UtcToEst().ToString("MMddyyyy") + ".csv");
                File.WriteAllText(reportFilePath, reconReport.ToString());
            }
            log.Debug($"Alaska Daily Operation report Execution Completed at {Common.Now.UtcToEst()} Est");
        }
    }
}
