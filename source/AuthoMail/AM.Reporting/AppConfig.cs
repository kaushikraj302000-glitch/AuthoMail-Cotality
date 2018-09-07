using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Reporting
{
    static class AppConfig
    {
        public static string DallasEMailTo { get; private set; }
        public static string DallasEMailCc { get; private set; }
        public static string DallasEMailSubject { get; private set; }
        public static string DallasEMailTemplate { get; private set; }

        public static string FedExReport_Email_To { get; private set; }
        public static string FedExReport_Email_Cc { get; private set; }
        public static string FedExReportSubject { get; private set; }
        public static string FedExAccountCode { get; private set; }

        public static string PCIReconEMailTo { get; private set; }
        public static string PCIReconEMailCc { get; private set; }
        public static string PCIReconEMailSubject { get; private set; }
        public static string PCIReconClientId { get; private set; }
        public static TimeSpan PCIReconCutOff { get; set; }
        public static string PCIReconReportPath { get; private set; }

        static AppConfig()
        {
            DallasEMailTo = ConfigurationManager.AppSettings["DallasEMailTo"].Trim();
            DallasEMailCc = ConfigurationManager.AppSettings["DallasEMailCc"].Trim();
            DallasEMailSubject = ConfigurationManager.AppSettings["DallasEMailSubject"].Trim();
            DallasEMailTemplate = ConfigurationManager.AppSettings["DallasEMailTemplate"].Trim();

            FedExReport_Email_To = ConfigurationManager.AppSettings["FedExReport_Email_To"].Trim();
            FedExReport_Email_Cc = ConfigurationManager.AppSettings["FedExReport_Email_Cc"].Trim();
            FedExReportSubject = ConfigurationManager.AppSettings["FedExReportSubject"].Trim();
            FedExAccountCode = ConfigurationManager.AppSettings["FedExAccountCode"].Trim();

            PCIReconEMailTo = ConfigurationManager.AppSettings["PCIReconEMailTo"].Trim();
            PCIReconEMailCc = ConfigurationManager.AppSettings["PCIReconEMailCc"].Trim();
            PCIReconEMailSubject = ConfigurationManager.AppSettings["PCIReconEMailSubject"].Trim();
            PCIReconClientId = ConfigurationManager.AppSettings["PCIReconClientId"].Trim();
            PCIReconCutOff = TimeSpan.Parse(ConfigurationManager.AppSettings["PCIReconCutOff"].Trim());
            PCIReconReportPath = ConfigurationManager.AppSettings["PCIReconReportPath"].Trim();
        }
    }
}
