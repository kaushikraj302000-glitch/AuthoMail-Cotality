using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Reporting
{
    class Program
    {
        static void Main(string[] args)
        {
            var argString = String.Join(",", args);
            log4net.LogicalThreadContext.Properties["args"] = argString;
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(ConfigurationManager.AppSettings["log4net.Config"]));


            switch (args[0])
            {
                case "HSBC":
                    HSBCChannel.GenerateConsolidatedReport();
                    break;
                case "AMC":
                    AMCChannel.GenerateSummaryReport();
                    break;
                case "FedExPickUp":
                    FedExPickUp.CheckPickedUpPackages();
                    break;
                case "FedExTracking":
                    CarrierTrackingInfo.CheckForShipmentTracking();
                    break;
                case "USPSTracking":
                    CarrierTrackingInfo.CheckForUSPSeVSShipmentTracking();
                    break;
                case "DallasNotification":
                    DallasNotification.GenerateEmailNotification();
                    break;
                case "PCIRecon":
                    PCIReconReport.GeneratePCIReconReport();
                    break;
                case "DALLASRecon":
                    DALLASReconReport.GenerateDALLASReconReport();
                    break;
                case "GOS":
                    switch (args[1])
                    {
                        case "ReceivedBatch":
                            GOSReports.GenerateGOSReceivedBatchReport();
                            break;
                        case "Recon":
                            GOSReports.GenerateGOSReconReport();
                            break;
                        case "DataPoints":
                            GOSReports.GenerateGOSDataPointReport();
                            break;
                        default:
                            break;
                    }
                    break;
                case "USAAReport":
                    switch (args[1])
                    {
                        case "MonthlyRecon":
                            USAAReporting.GenerateMonthlyReconReport();
                            break;
                        case "ErrorReport":
                            USAAReporting.GenerateErrorReport();
                            break;
                        case "WeeklyReport":
                            USAAReporting.GenerateWeeklyReport();
                            break;
                        default:
                            break;
                    }
                    break;
                case "USAAMTD":
                    USAADailyMTDVolumeReporting.GenerateMTDReport();
                    break;
                default:
                    break;
            }
        }
    }
}
