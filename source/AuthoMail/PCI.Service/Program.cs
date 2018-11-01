using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.Data;
using log4net;

namespace PCI.Service
{
    static class Program
    {
        private static ILog log;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {

            switch (args[0])
            {
                case "PnSJob":
                    log = LogManager.GetLogger("PCI.Service.PnSJobMonitor");
                    break;
                case "PnSCore":
                    log = LogManager.GetLogger("PCI.Service.PnSCoreService");
                    break;
                case "PCIJob":
                    log = LogManager.GetLogger("PCI.Service.PCIJobMonitor");
                    break;
                default:
                    break;
            }

            try
            {
                Common.PCIRootFolder.IsNullOrEmpty();
                switch (args[0])
                {
                    case "PnSJob":
                        PnSJobMonitor.GetNewPnSBatch();
                        PnSJobMonitor.SubmitImmediateErrorReport();
                        PnSJobMonitor.SubmitFinalXMLReport();
                        break;
                    case "PnSCore":
                        PnSCoreService.PrepPCIJobs();
                        PnSCoreService.ProcessPCIResponse();
                        PnSCoreService.GeneratePnSResponse();
                        //PnSCoreService.ProcessPCIReturnPDFsInfo();
                        break;
                    case "PCIJob":
                        var printTrails = args.Length > 1 ? args[1] : "";
                        PCIJobMonitor.SubmitBatchToPCI(printTrails);
                        PCIJobMonitor.GetResponseMRDF();
                        //PCIJobMonitor.GetReturnPDFsZip();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                var msg = "Error occured while executing PnSService";
                log.Error(msg, ex);

                MailUtility.SendUnexpectedNotificationEmail(msg + ex.Message);
            }
        }
    }
}
