using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Entities;
using AM.Lib;
using AM.Services;
using System.IO;

namespace AM.Reporting
{
    class PCIReconReport
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal static void GeneratePCIReconReport()
        {
            try
            {
                log.Debug("PCI recon report Execution Triggered at {0} Est".Fill(Common.Now.UtcToEst()));
                List<PCISubmittedBatchDetail> pciSubmittedBatchDetails;
                using (IReportService reportSvc = new ReportService())
                {
                    DateTime effectiveStartDateTime = Common.Now.UtcToEst().Date.AddDays(-1);
                    DateTime effectiveEndDateTime = effectiveStartDateTime.Date + AppConfig.PCIReconCutOff;

                    pciSubmittedBatchDetails = reportSvc.GetPCIReconReport(AppConfig.PCIReconClientId, effectiveStartDateTime, effectiveEndDateTime);

                    if (pciSubmittedBatchDetails.Count > 0)
                    {
                        SendEmailNotificationToPCI(pciSubmittedBatchDetails);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error occured while generating Reconcilation Report for PCI", ex);
            }
        }

        private static void SendEmailNotificationToPCI(List<PCISubmittedBatchDetail> pciSubmittedBatchDetails)
        {
            var columnName = String.Join(",", "BatchID", "Batch Name", "Submitted On", "Due Date", "Carrier", "Service Type", "Package Count", "Total Pages");

            StringBuilder reconReport = new StringBuilder(columnName);

            reconReport.AppendLine();
            foreach (var batch in pciSubmittedBatchDetails)
            {
                var line = String.Join(",", batch.PrintBatch, batch.PrintFileName, batch.SubmittedOn.UtcToEst(), batch.DueDate, batch.ShippingCarrier, batch.ShippingServiceType, batch.PackageCount, batch.TotalPages);
                reconReport.AppendLine(line);
            }
            string reportFilePath = Path.Combine(AppConfig.PCIReconReportPath, "PCIReconReport_" + Common.Now.UtcToEst().Date.ToString("MMddyyyy") + ".csv");
            File.WriteAllText(reportFilePath, reconReport.ToString());
        }
    }
}
