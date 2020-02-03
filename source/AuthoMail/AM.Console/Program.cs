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

            if (args.Length > 0)
            {
                if (args[0] == "config")
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
                else if (args[0] == "EffectiveReceivedOnCalculate")
                {
                    System.Console.WriteLine("Calculation Start");

                    ReCalculateDueDate.UpdateErrorEffectiveReceivedOn();
                    System.Console.WriteLine("Calculation Completed");
                    return;
                }
                else if (args[0] == "UpdatePnSConfigDetailId")
                {
                    System.Console.WriteLine("Update PnSConfigDetailId Start");

                    ReCalculateDueDate.UpdatePnSConfigDetailId();

                    System.Console.WriteLine("Update PnSConfigDetailId Completed");
                    return;
                }
                else if (args[0] == "BatchReporting")
                {
                    //This will be used as How system will report back to the client. means based on response of printbatches table or full response so Here user needs to pass "Incremental" Or "Full" then comma separated SFTChannel codes
                    string reportType = args.Length > 2 ? args[1] : string.Empty;

                    if (string.IsNullOrEmpty(reportType))
                        return;


                    string[] channelList = args[2].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    Channel.ReportBack(channelList, reportType);
                }
                else if (args[0] == "Purging")
                {
                    Purging.StartPurging();
                }
                else if (args[0] == "Delete")
                {
                    Purging.DeleteFiles();
                }
                else if (args[0] == "PurgeDBRecords")
                {
                    Purging.PurgeDBRecords();
                }
                else
                {
                    using (var channel = ChannelFactory.Create(args))
                    {
                        if (channel == null)
                        {
                            log.Error("Channel prefix '{0}' is invalid.".Fill(args[0]));
                            return;
                        }

                        channel.ProcessBatches();
                    }
                }
            }
        }
    }
}
