using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Entities;
using AM.Lib;
using AM.Services;
using System.Configuration;

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
                    DateTime deploymentDateTime = DateTime.Parse(ConfigurationManager.AppSettings["FedExDeployment"]).EstToUtc();
                    DateTime todayDate = Common.Today;
                    TimeSpan cutOff = todayDate.DayOfWeek == DayOfWeek.Saturday ? new TimeSpan(12, 00, 00) : new TimeSpan(19, 00, 00);
                    DateTime todayCutOffTime = todayDate + cutOff;
                    mailPieces = reportSvc.GetFedExPickedUpReport(deploymentDateTime, todayCutOffTime, 0);
                    if (mailPieces != null && mailPieces.Count > 0)
                    {
                        CheckFedExTrackingInfo checkTrackingInfo = new CheckFedExTrackingInfo();
                        bool isChecked = checkTrackingInfo.GetTrackingInformation(mailPieces);
                    }

                    //Get all records after updating pickedup information and generate report
                    mailPieces = reportSvc.GetFedExPickedUpReport(deploymentDateTime, todayCutOffTime);
                    if (mailPieces != null && mailPieces.Count > 0)
                    {
                        //generate using report format and send it to CL team
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error occured while checkign for FedEx packages picked up or not", ex);
            }
        }
    }
}
