using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PCI.Service.Utilities;
using System.Net.Mail;
using PCI.Service;
using System.Data;

namespace PCI.Service
{
    public static class Common
    {
        public static bool Remove_ImmediateErrFiles { get; private set; }
        public static bool RemoveBatchFiles_PnS_SFT { get; private set; }
        public static bool Remove_MRDFOUT_Of_PCI_SFT { get; private set; }
        public static bool Remove_PDF_Of_PCI_SFT { get; private set; }

        public static string PCIRootFolder { get; set; }

        public static string PCIRootReturnPDFsZipFolder { get; set; }

        public static SFT_ProtocolType PnS_SFT_Protocol { get; private set; }
        public static SFT_ProtocolType PCI_SFT_Protocol { get; private set; }
        public static SFT_ProtocolType CL_SFT_Protocol { get; private set; }

        public static string PnS_SFT_HostURL { get; private set; }
        public static string PnS_SFT_UserName { get; private set; }
        public static string PnS_SFT_Password { get; private set; }
        public static string PnS_SFT_RootDir { get; private set; }

        public static string PCI_SFT_Host { get; private set; }
        public static string PCI_SFT_Username { get; private set; }
        public static string PCI_SFT_Password { get; private set; }
        public static string PCI_SFT_SubmitDir { get; private set; }
        public static string PCI_SFT_ResponseDir { get; private set; }
        public static string PCI_SFT_ReturnPDFDir { get; private set; }

        public static string CL_SFT_Host { get; private set; }
        public static string CL_SFT_Username { get; private set; }
        public static string CL_SFT_Password { get; private set; }
        public static string CL_SFT_SubmitDir { get; private set; }
        public static string CL_SFT_ReturnPDFDir { get; private set; }

        public static int PollingInterval { get; private set; }

        public static SmtpClient Email_SMTPClient { get; private set; }
        public static string Email_ToPCI { get; private set; }
        public static string Email_CcPCI { get; private set; }

        public static string Email_ToCL { get; private set; }
        public static string Email_CcCL { get; private set; }

        public static string SubmitBatchEmailSubject { get; private set; }
        public static string SubmitBatchEmailTemplate { get; private set; }

        public static string SuspendBatchEmailSubject { get; private set; }
        public static string SuspendBatchEmailTemplate { get; private set; }
        public static string SuspendEmail_To { get; private set; }
        public static string SuspendEmail_Cc { get; private set; }
        public static int MRDFResponseLength { get; private set; }

        public static string NoRecordsInMRDFSubject { get; private set; }
        public static string MRDFIssue_Email_To { get; private set; }
        public static string MRDFIssue_Email_Cc { get; private set; }
        public static string ApplicationEnvironment { get; private set; }
        public static string PrintLocationCL { get; private set; }
        public static string PrintLocationPCI { get; private set; }

        public static string UnexpectedEmail_Subject { get; private set; }
        public static string UnexpectedEmail_ToEmail { get; private set; }
        public static string UnexpectedEmail_CcEmail { get; private set; }

        static Common()
        {
            Remove_ImmediateErrFiles = ConfigurationManager.AppSettings["Remove_ImmediateErrFiles"].Trim() == "0" ? false : true;
            RemoveBatchFiles_PnS_SFT = ConfigurationManager.AppSettings["RemoveBatchFiles_PnS_SFT"].Trim() == "0" ? false : true;
            Remove_MRDFOUT_Of_PCI_SFT = ConfigurationManager.AppSettings["Remove_MRDFOUT_Of_PCI_SFT"].Trim() == "0" ? false : true;
            Remove_PDF_Of_PCI_SFT = ConfigurationManager.AppSettings["Remove_PDF_Of_PCI_SFT"].Trim() == "0" ? false : true;

            PCIRootFolder = Path.GetFullPath(ConfigurationManager.AppSettings["PCIRootFolder"]);
            PCIRootFolder.CreateDirectoryIfNotExist();

            PCIRootReturnPDFsZipFolder = Path.GetFullPath(ConfigurationManager.AppSettings["PCIRootReturnPDFsZipFolder"]);
            PCIRootReturnPDFsZipFolder.CreateDirectoryIfNotExist();

            string PnS_SFT_Protocol_Value = ConfigurationManager.AppSettings["PnS_SFT_Protocol_Value"].Trim().ToLower();
            switch (PnS_SFT_Protocol_Value)
            {
                case "ftp": PnS_SFT_Protocol = SFT_ProtocolType.ftp;
                    break;
                case "sftp": PnS_SFT_Protocol = SFT_ProtocolType.sftp;
                    break;
                case "unc": PnS_SFT_Protocol = SFT_ProtocolType.unc;
                    break;
                default: throw new Exception("Please provide valid protocol for 'PnS_SFT_Protocol_Value'.");
            }

            string PCI_SFT_Protocol_Value = ConfigurationManager.AppSettings["PCI_SFT_Protocol_Value"].Trim().ToLower();
            switch (PCI_SFT_Protocol_Value)
            {
                case "ftp": PCI_SFT_Protocol = SFT_ProtocolType.ftp;
                    break;
                case "sftp": PCI_SFT_Protocol = SFT_ProtocolType.sftp;
                    break;
                case "unc": PCI_SFT_Protocol = SFT_ProtocolType.unc;
                    break;
                default: throw new Exception("Please provide valid protocol for 'PCI_SFT_Protocol_Value'.");
            }

            string CL_SFT_Protocol_Value = ConfigurationManager.AppSettings["CL_SFT_Protocol_Value"].Trim().ToLower();
            switch (CL_SFT_Protocol_Value)
            {
                case "ftp": CL_SFT_Protocol = SFT_ProtocolType.ftp;
                    break;
                case "sftp": CL_SFT_Protocol = SFT_ProtocolType.sftp;
                    break;
                case "unc": CL_SFT_Protocol = SFT_ProtocolType.unc;
                    break;
                default: throw new Exception("Please provide valid protocol for 'CL_SFT_Protocol_Value'.");
            }

            PnS_SFT_HostURL = ConfigurationManager.AppSettings["PnS_SFT_HostURL"];
            PnS_SFT_UserName = ConfigurationManager.AppSettings["PnS_SFT_UserName"];
            PnS_SFT_Password = ConfigurationManager.AppSettings["PnS_SFT_Password"];
            PnS_SFT_RootDir = ConfigurationManager.AppSettings["PnS_SFT_RootDir"];

            PCI_SFT_Host = ConfigurationManager.AppSettings["PCI_SFT_Host"];
            PCI_SFT_Username = ConfigurationManager.AppSettings["PCI_SFT_Username"];
            PCI_SFT_Password = ConfigurationManager.AppSettings["PCI_SFT_Password"];
            PCI_SFT_SubmitDir = ConfigurationManager.AppSettings["PCI_SFT_SubmitDir"];
            PCI_SFT_ResponseDir = ConfigurationManager.AppSettings["PCI_SFT_ResponseDir"];
            PCI_SFT_ReturnPDFDir = ConfigurationManager.AppSettings["PCI_SFT_ReturnPDFDir"];

            CL_SFT_Host = ConfigurationManager.AppSettings["CL_SFT_Host"];
            CL_SFT_Username = ConfigurationManager.AppSettings["CL_SFT_Username"];
            CL_SFT_Password = ConfigurationManager.AppSettings["CL_SFT_Password"];
            CL_SFT_SubmitDir = ConfigurationManager.AppSettings["CL_SFT_SubmitDir"];
            CL_SFT_ReturnPDFDir = ConfigurationManager.AppSettings["CL_SFT_ReturnPDFDir"];

            PollingInterval = int.Parse(ConfigurationManager.AppSettings["PollingInterval"]) * 1000;
            DatabaseFactory.SetDatabaseProviderFactory(new DatabaseProviderFactory());

            ApplicationEnvironment = DAL.GetSystemSettingsValue("Common_Environment");
            PrintLocationCL = DAL.GetSystemSettingsValue("Common_PrintLocationCL");
            PrintLocationPCI = DAL.GetSystemSettingsValue("Common_PrintLocationPCI");

            Email_SMTPClient = new SmtpClient();

            Email_ToPCI = ConfigurationManager.AppSettings["Email_ToPCI"];
            Email_CcPCI = ConfigurationManager.AppSettings["Email_CcPCI"];

            Email_ToCL = ConfigurationManager.AppSettings["Email_ToCL"];
            Email_CcCL = ConfigurationManager.AppSettings["Email_CcCL"];

            SubmitBatchEmailTemplate = Path.GetFullPath(ConfigurationManager.AppSettings["SubmitBatchEmailTemplate"]);

            SuspendEmail_To = ConfigurationManager.AppSettings["SuspendEmail_To"];
            SuspendEmail_Cc = ConfigurationManager.AppSettings["SuspendEmail_Cc"];
            SuspendBatchEmailTemplate = Path.GetFullPath(ConfigurationManager.AppSettings["SuspendBatchEmailTemplate"]);

            MRDFResponseLength = int.Parse(ConfigurationManager.AppSettings["MRDFResponseLength"]);

            SubmitBatchEmailSubject = ConfigurationManager.AppSettings["SubmitBatchEmailSubject"];
            SuspendBatchEmailSubject = ConfigurationManager.AppSettings["SuspendBatchEmailSubject"];

            NoRecordsInMRDFSubject = ConfigurationManager.AppSettings["NoRecordsInMRDFSubject"];
            MRDFIssue_Email_To = ConfigurationManager.AppSettings["MRDFIssue_Email_To"];
            MRDFIssue_Email_Cc = ConfigurationManager.AppSettings["MRDFIssue_Email_Cc"];

            UnexpectedEmail_Subject = ConfigurationManager.AppSettings["UnexpectedEmail_Subject"];
            UnexpectedEmail_ToEmail = ConfigurationManager.AppSettings["UnexpectedEmail_ToEmail"];
            UnexpectedEmail_CcEmail = ConfigurationManager.AppSettings["UnexpectedEmail_CcEmail"];
        }

        public static void CleanDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            Directory.CreateDirectory(path);
        }

        public static DateTime GetCurrentTime()
        {
            DateTime dtCurrentTime = DateTime.UtcNow;

            // Any conversion of time if needed.

            return dtCurrentTime;
        }

        public static DateTime EstToUtc(object dt)
        {
            //This is defined because we don't need to verify by DateTime.Parse. it is already in proper date.
            DateTime dtVal = Convert.ToDateTime(dt);
            // Treat date as Est irrecpective of client and server timezone and covert it to UTC

            dt = DateTime.SpecifyKind(dtVal, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTime(dtVal, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"), TimeZoneInfo.FindSystemTimeZoneById("UTC"));
        }

        public static DateTime UtcToEst(object dt)
        {
            //This is defined because we don't need to verify by DateTime.Parse. it is already in proper date.
            DateTime dtVal = Convert.ToDateTime(dt);
            // Treat date as Est irrecpective of client and server timezone and covert it to UTC

            dt = DateTime.SpecifyKind(dtVal, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTime(dtVal, TimeZoneInfo.FindSystemTimeZoneById("UTC"), TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
        }

        public static DateTime UtcToCst(this DateTime dt)
        {
            // Treat date as Est irrecpective of client and server timezone and covert it to UTC

            dt = DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTime(dt, TimeZoneInfo.FindSystemTimeZoneById("UTC"), TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
        }

        public static DateTime EstToday
        {
            get { return Common.UtcToEst(DateTime.UtcNow).Date; }
        }

    }
}
