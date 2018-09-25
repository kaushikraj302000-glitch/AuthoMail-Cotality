using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.Data;

namespace PCI.Service
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
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
                    PnSCoreService.ProcessPCIReturnPDFsInfo();
                    break;
                case "PCIJob":
                    PCIJobMonitor.SubmitBatchToPCI();
                    PCIJobMonitor.GetResponseMRDF();
                    PCIJobMonitor.GetReturnPDFsZip();
                    break;
                default:
                    break;
            }
        }
    }
}
