using AM.Lib;
using AM.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Console
{
    class Program
    {
        private static log4net.ILog log;
        static void Main(string[] args)
        {
            //Code Added to create argument wise log file
            var argString = String.Join(",", args);
            log4net.LogicalThreadContext.Properties["args"] = argString;
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(ConfigurationManager.AppSettings["log4net.Config"]));
            var type = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType;
            log = log4net.LogManager.GetLogger(type);

            log.InfoFormat("Command line arguments : {0}", String.Join(",", args));
            
            foreach (var arg in args)
            {
                if (arg == "config")
                {
                    System.Console.WriteLine("Seeding Start");

                    using (var cfgSvc = new ConfigService())
                    {
                        cfgSvc.SeedAMSettings();
                    }
                    Config.SaveAllSetting();
                    System.Console.WriteLine("Seed Completed.");
                    return;
                }
                else if (arg == "BatchReporting")
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
