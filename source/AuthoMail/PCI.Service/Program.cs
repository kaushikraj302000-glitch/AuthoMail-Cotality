using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.Data;
using log4net;
using System.Configuration;

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
            var argString = String.Join(",", args);

            if (args.Length > 0 && args[0] == "PCIJob")
            {
                string jobName = args[0];
                string pvCode = args[1];

                if (pvCode == "PCI" || pvCode.Contains("NORDIS"))
                {
                    argString = "{0}_{1}_{2}".Fill(jobName, pvCode, Common.UtcToEst(Common.GetCurrentTime()).ToString("hhmm"));
                }
                else
                {
                    argString = "{0}_{1}".Fill(jobName, pvCode);
                }
            }
            //Code Added to create argument wise log file
            log4net.LogicalThreadContext.Properties["args"] = argString;
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(ConfigurationManager.AppSettings["log4net.Config"]));
            var type = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType;
            log = log4net.LogManager.GetLogger(type);


            log.InfoFormat("Command line arguments : {0}", String.Join(",", args));

            try
            {
                log.Info($"{string.Join(",",args)} started.");
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
                        List<string> ptNames;
                        var isBundle = args.Length > 3 ? true : false;
                        log.Info("PCIJob Started");
                        if (isBundle)
                        {
                            log.Info("PCIJob isBundle True..Started Submitting..");
                            //NOTE : Total 4 arguments will be passed for bundling PCIJob PrintVendorCode Bundle [PrintTrailNames]
                            ptNames = args[3].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                            PCIJobMonitor.SubmitBatchToPrintVendor(args[1], isBundle, ptNames);
                            log.Info("PCIJob isBundle True..Completed Submitting..");
                        }
                        else
                        {
                            log.Info("PCIJob isBundle False..Started Submitting..");
                            //NOTE : Total 3 arguments will be passed for regular flow PCIJob PrintVendorCode [PrintTrailNames]
                            ptNames = args[2].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                            PCIJobMonitor.SubmitBatchToPrintVendor(args[1], isBundle, ptNames);
                            log.Info("PCIJob isBundle False..Completed Submitting..");
                        }
                        log.Info("PCIJob Completed");
                        break;
                    case "DownloadPrinterReport":
                        PCIJobMonitor.GetPrintVendorResponse(args.Length > 1 ? args[1] : "");
                        //PCIJobMonitor.GetReturnPDFsZip();
                        break;
                    default:
                        break;
                }
                log.Info($"{string.Join(",", args)} completed.");
            }
            catch (Exception ex)
            {
                var msg = "Error occured while executing {0} <br/> <b> Error Message </b><br/>{1}".Fill(args[0], ex);
                log.Error(msg);
                MailUtility.SendUnexpectedNotificationEmail(msg);
            }
        }
    }
}
