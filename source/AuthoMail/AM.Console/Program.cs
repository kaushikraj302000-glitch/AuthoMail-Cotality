using AM.Lib;
using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Console
{
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            // Process mail batches based on channel code

            foreach (var arg in args)
            {
                if (arg == "BatchReporting")
                {
                    Channel.ReportBack();
                }
                else
                {
                    using (var channel = ChannelFactory.Create(arg))
                    {
                        if (channel == null)
                        {
                            log.Error("Channel prefix '{0}' is invalid.".Fill(arg));
                            continue;
                        }

                        channel.ProcessBatches();
                    }
                }
            }

            //DownloadJob downloadJob = new DownloadJob();
            //downloadJob.DownloadNewBatch();
            //downloadJob.SeparateUSAABatches();
            //downloadJob.ValidateBatches();
            //downloadJob.AcknowledgeBatches();
            //downloadJob.ReportPreppedProcess();
            //downloadJob.ReportBackProcess();
        }
    }
}
