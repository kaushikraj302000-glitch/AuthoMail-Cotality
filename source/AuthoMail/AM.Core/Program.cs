using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace AM.Core
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                if (args.Length > 0)
                {
                    //Here we need to execute only this method for generating pnsbatch for PCI
                    AMCore.SubmitBatchesDocTypeWiseToPrintVendor(args[0]);
                }
                else
                {
                    AMCore.PrepPnSJobs();
                    AMCore.SubmitBatchesDocTypeWiseToPrintVendor("");
                    AMCore.GetSubmittedPnSResponses();
                    AMCore.ProcessPnSResponses();
                    AMCore.ReportPreppedProcess();
                }
            }
            else
            {
                if (args.Length > 0)
                {
                    //Here we need to execute only this method for generating pnsbatch for PCI
                    AMCore.SubmitBatchesDocTypeWiseToPrintVendor(args[0]);
                }
                else
                {
                    AMCore.PrepPnSJobs();
                    AMCore.SubmitBatchesDocTypeWiseToPrintVendor("");
                    AMCore.GetSubmittedPnSResponses();
                    AMCore.ProcessPnSResponses();
                    AMCore.ReportPreppedProcess();
                }


                //ServiceBase[] ServicesToRun;
                //ServicesToRun = new ServiceBase[] 
                //{ 
                //    new AMCoreService() 
                //};
                //ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
