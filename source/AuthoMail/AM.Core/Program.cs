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
            AMCore.PrepPnSJobs();
            AMCore.SubmitBatchesDocTypeWiseToPrintVendor();
            AMCore.GetSubmittedPnSResponses();
            AMCore.ProcessPnSResponses();
            AMCore.ReportPreppedProcess();
        }
    }
}
