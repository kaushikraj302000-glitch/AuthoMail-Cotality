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
        public static TimeSpan FedExFullReportTiming { get; private set; }

        public static string PCIReconEMailTo { get; private set; }
        public static string PCIReconEMailCc { get; private set; }
        public static string PCIReconEMailSubject { get; private set; }
        public static string PCIReconReportPath { get; private set; }
        public static string DALLASReconEMailTo { get; private set; }
        public static string DALLASReconEMailCc { get; private set; }
        public static string DALLASReconEMailSubject { get; private set; }
        public static string DALLASReconReportPath { get; private set; }

        public static string USAAEMailTo { get; private set; }
        public static string USAAEMailCc { get; private set; }
        public static string USAAMonthlyReconEMailSubject { get; private set; }
        public static string USAAMonthlyReconReportPath { get; private set; }
        public static string USAAErrorReportEMailSubject { get; private set; }
        public static string USAAErrorReportPath { get; private set; }
        public static string USAAWeeklyReportEMailSubject { get; private set; }
        public static string USAAWeeklyReportPath { get; private set; }

        public static string GOSReconEMailTo { get; private set; }
        public static string GOSReconEMailCc { get; private set; }
        public static string GOSReconEMailSubject { get; private set; }
        public static string GOSReceivedBatchEMailSubject { get; private set; }
        public static string GOSReconReportPath { get; private set; }


        public static string USAAMTDEmailTo { get; private set; }
        public static string USAAMTDEmailCc { get; private set; }
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
            FedExFullReportTiming = TimeSpan.Parse(ConfigurationManager.AppSettings["FedExFullReportTiming"].Trim());

            PCIReconEMailTo = ConfigurationManager.AppSettings["PCIReconEMailTo"].Trim();
            PCIReconEMailCc = ConfigurationManager.AppSettings["PCIReconEMailCc"].Trim();
            PCIReconEMailSubject = ConfigurationManager.AppSettings["PCIReconEMailSubject"].Trim();
            PCIReconReportPath = ConfigurationManager.AppSettings["PCIReconReportPath"].Trim();

            DALLASReconEMailTo = ConfigurationManager.AppSettings["DALLASReconEMailTo"].Trim();
            DALLASReconEMailCc = ConfigurationManager.AppSettings["DALLASReconEMailCc"].Trim();
            DALLASReconEMailSubject = ConfigurationManager.AppSettings["DALLASReconEMailSubject"].Trim();
            DALLASReconReportPath = ConfigurationManager.AppSettings["DALLASReconReportPath"].Trim();

            USAAEMailTo = ConfigurationManager.AppSettings["USAAEMailTo"].Trim();
            USAAEMailCc = ConfigurationManager.AppSettings["USAAEMailCc"].Trim();
            USAAMonthlyReconEMailSubject = ConfigurationManager.AppSettings["USAAMonthlyReconEMailSubject"].Trim();
            USAAMonthlyReconReportPath = ConfigurationManager.AppSettings["USAAMonthlyReconReportPath"].Trim();
            USAAErrorReportEMailSubject = ConfigurationManager.AppSettings["USAAErrorReportEMailSubject"].Trim();
            USAAErrorReportPath = ConfigurationManager.AppSettings["USAAErrorReportPath"].Trim();
            USAAWeeklyReportEMailSubject = ConfigurationManager.AppSettings["USAAWeeklyReportEMailSubject"].Trim();
            USAAWeeklyReportPath = ConfigurationManager.AppSettings["USAAWeeklyReportPath"].Trim();

            GOSReconEMailTo = ConfigurationManager.AppSettings["GOSReconEMailTo"].Trim();
            GOSReconEMailCc = ConfigurationManager.AppSettings["GOSReconEMailCc"].Trim();
            GOSReconEMailSubject = ConfigurationManager.AppSettings["GOSReconEMailSubject"].Trim();
            GOSReceivedBatchEMailSubject = ConfigurationManager.AppSettings["GOSReceivedBatchEMailSubject"].Trim();
            GOSReconReportPath = ConfigurationManager.AppSettings["GOSReconReportPath"].Trim();

            USAAMTDEmailTo = ConfigurationManager.AppSettings["USAAMTDEmailTo"].Trim();
            USAAMTDEmailCc = ConfigurationManager.AppSettings["USAAMTDEmailCc"].Trim();
        }
    }
}
