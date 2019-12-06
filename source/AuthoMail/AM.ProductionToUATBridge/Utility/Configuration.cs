using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.ProductionToUATBridge
{
    public static class Configuration
    {
        public static string AMRootPath { get; set; }
        public static string DestinationPath { get; set; }
        public static string AMRequestFolder { get; set; }
        public static string[] ProductionVolumnRatio { get; set; }
        public static string UATChannelCode { get; set; }
        public static string PrintReadyUATChannelCode { get; set; }
        public static string Monitor_Exception_Email_To { get; set; }
        public static string Monitor_Exception_Email_Cc { get; set; }
        public static string Monitor_ExceptionEmail_Subject { get; set; }
        public static string[] PrintReadyClientCodes { get; set; }

        static Configuration()
        {
            AMRootPath = ConfigurationManager.AppSettings["AMRootPath"].Trim();
            DestinationPath = ConfigurationManager.AppSettings["DestinationPath"].Trim();
            AMRequestFolder = ConfigurationManager.AppSettings["AMRequestFolder"].Trim();
            ProductionVolumnRatio = ConfigurationManager.AppSettings["ProductionVolumnRatio"].Trim().Split(',');
            UATChannelCode = ConfigurationManager.AppSettings["UATChannelCode"];
            PrintReadyUATChannelCode = ConfigurationManager.AppSettings["PrintReadyUATChannelCode"].Trim();
            PrintReadyClientCodes = ConfigurationManager.AppSettings["PrintReadyClientCodes"].Trim().Split(',');
        }
    }
}
