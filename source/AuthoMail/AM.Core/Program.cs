using AM.Entities;
using AM.Services;
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
            bool prepFlag, submitFlag, submittedResFlag, parseResFlag, reportPrepResFlag;

            List<int> channelIds = new List<int>();
           
            using (IClientService commonSvc = new ClientService())
            {
                var channels = commonSvc.GetSFTChannelDetailsByServerIP(Config.Common_ServerIP).Result;

                if (channels != null && channels.Count() > 0)
                {
                    channelIds = channels.Select(x => x.Id).ToList();
                }
                else
                {
                    return;
                }
            }

            do
            {
                prepFlag = AMCore.PrepPnSJobs(channelIds);
                submitFlag = AMCore.SubmitBatchesDocTypeWiseToPrintVendor(channelIds);
                submittedResFlag = AMCore.GetSubmittedPnSResponses(channelIds);
                parseResFlag = AMCore.ProcessPnSResponses(channelIds);
                reportPrepResFlag = AMCore.ReportPreppedProcess(channelIds);

            } while (prepFlag || submitFlag || submittedResFlag || parseResFlag || reportPrepResFlag);
        }
    }
}
