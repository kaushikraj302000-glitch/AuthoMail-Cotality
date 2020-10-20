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
                    DateTime dueDateStartTime = Common.Now.UtcToEst().AddDays(-10).ToStartDateTime();
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
    }
}
