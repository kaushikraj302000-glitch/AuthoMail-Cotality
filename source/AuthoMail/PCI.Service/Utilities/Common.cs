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
using log4net;

namespace PCI.Service
{
    public static class Common
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Common));
        public static bool Remove_ImmediateErrFiles { get; private set; }
        public static string FontPathDir { get; private set; }
        public static string PCIRootFolder { get; set; }
        public static string BundlingRootFolder { get; set; }
        public static string PCIRootReturnPDFsZipFolder { get; set; }
        public static string PnS_SFTChannelCode { get; private set; }
        public static int MRDFResponseLength { get; private set; }
        public static SmtpClient Email_SMTPClient { get; private set; }

        public static string SubmitPCIBatchEmailSubject { get; private set; }
        public static string SubmitPCIBatchEmailTemplate { get; private set; }
        public static string Email_ToPCI { get; private set; }
        public static string Email_CcPCI { get; private set; }

        public static string SubmitNordisBatchEmailSubject { get; private set; }
        public static string SubmitNordisBatchEmailTemplate { get; private set; }
        public static string Email_ToNordis { get; private set; }
        public static string Email_CcNordis { get; private set; }

        public static string SubmitDELNordisBatchEmailSubject { get; private set; }
        public static string SubmitDELNordisBatchEmailTemplate { get; private set; }
        public static string Email_ToDELNordis { get; private set; }
        public static string Email_CcDELNordis { get; private set; }

        public static string SuspendBatchEmailSubject { get; private set; }
        public static string SuspendBatchEmailTemplate { get; private set; }
        public static string SuspendEmail_To { get; private set; }
        public static string SuspendEmail_Cc { get; private set; }


        public static string PCI_Issue_Subject { get; private set; }
        public static string PCI_Issue_Email_To { get; private set; }
        public static string PCI_Issue_Email_Cc { get; private set; }

        public static string NORDIS_Issue_Subject { get; private set; }
        public static string NORDIS_Issue_Email_To { get; private set; }
        public static string NORDIS_Issue_Email_Cc { get; private set; }

        public static string ApplicationEnvironment { get; private set; }
        public static string PrintLocationCL { get; private set; }
        public static string PrintLocationPCI { get; private set; }

        public static string BundlingRequestFolderName { get; private set; }
        public static string BundlingResponseFolderName { get; private set; }

        public static string PnSRequestFolderName { get; private set; }
        public static string PnSResponseFolderName { get; private set; }
        public static string PCIRequestFolderName { get; private set; }
        public static string PCIResponseFolderName { get; private set; }

        public static string UnexpectedEmail_Subject { get; private set; }
        public static string UnexpectedEmail_ToEmail { get; private set; }
        public static string UnexpectedEmail_CcEmail { get; private set; }

        static Common()
        {
            try
            {
                DatabaseFactory.SetDatabaseProviderFactory(new DatabaseProviderFactory());
                Remove_ImmediateErrFiles = DAL.GetSystemSettingsValue("PnS_Remove_ImmediateErrFiles").Trim() == "0" ? false : true;
                FontPathDir = DAL.GetSystemSettingsValue("PnS_FontsDirPath").Trim();
                PCIRootFolder = DAL.GetSystemSettingsValue("PnS_PCIRootFolder").Trim();
                PCIRootFolder.CreateDirectoryIfNotExist();

                BundlingRootFolder = Path.Combine(PCIRootFolder, "BundlingRoot");
                BundlingRootFolder.CreateDirectoryIfNotExist();
                BundlingRequestFolderName = "Request";
                BundlingResponseFolderName = "Response";

                PnSRequestFolderName = "PnSRequest";
                PnSResponseFolderName = "PnSResponse";
                PCIRequestFolderName = "PCIRequest";
                PCIResponseFolderName = "PCIResponse";

                PCIRootReturnPDFsZipFolder = DAL.GetSystemSettingsValue("PnS_PCIRootReturnPDFsZipFolder").Trim();
                PCIRootReturnPDFsZipFolder.CreateDirectoryIfNotExist();
                MRDFResponseLength = int.Parse(DAL.GetSystemSettingsValue("PnS_MRDFResponseLength").Trim());                

                ApplicationEnvironment = DAL.GetSystemSettingsValue("Common_Environment");

                PnS_SFTChannelCode = DAL.GetSystemSettingsValue("PnS_SFTChannelCode").Trim() + "_" + ApplicationEnvironment;
                Email_SMTPClient = new SmtpClient();

                var suspendedEmailChannel = DAL.GetEmailChannelByTitleName("PnS_SuspendEmail");
                SuspendEmail_To = suspendedEmailChannel.To;
                SuspendEmail_Cc = suspendedEmailChannel.Cc;
                SuspendBatchEmailSubject = suspendedEmailChannel.Subject;
                SuspendBatchEmailTemplate = suspendedEmailChannel.BodyTemplate;

                var submitNordisBatchEmailChannel = DAL.GetEmailChannelByTitleName("PnS_SubmitNordisBatchEmail");
                SubmitNordisBatchEmailSubject = submitNordisBatchEmailChannel.Subject;
                SubmitNordisBatchEmailTemplate = submitNordisBatchEmailChannel.BodyTemplate;
                Email_ToNordis = submitNordisBatchEmailChannel.To;
                Email_CcNordis = submitNordisBatchEmailChannel.Cc;

                var submitDELNordisBatchEmailChannel = DAL.GetEmailChannelByTitleName("PnS_DELSubmitNordisBatchEmail");
                SubmitDELNordisBatchEmailSubject = submitDELNordisBatchEmailChannel.Subject;
                SubmitDELNordisBatchEmailTemplate = submitDELNordisBatchEmailChannel.BodyTemplate;
                Email_ToDELNordis = submitDELNordisBatchEmailChannel.To;
                Email_CcDELNordis = submitDELNordisBatchEmailChannel.Cc;

                var submitPCIBatchEmailChannel = DAL.GetEmailChannelByTitleName("PnS_SubmitPCIBatchEmail");
                SubmitPCIBatchEmailSubject = submitPCIBatchEmailChannel.Subject;
                SubmitPCIBatchEmailTemplate = submitPCIBatchEmailChannel.BodyTemplate;
                Email_ToPCI = submitPCIBatchEmailChannel.To;
                Email_CcPCI = submitPCIBatchEmailChannel.Cc;


                var invalidPCIResponseEmailChannel = DAL.GetEmailChannelByTitleName("PnS_PCIIssueEmail");
                PCI_Issue_Subject = invalidPCIResponseEmailChannel.Subject;
                PCI_Issue_Email_To = invalidPCIResponseEmailChannel.To;
                PCI_Issue_Email_Cc = invalidPCIResponseEmailChannel.Cc;

                var invalidNordisResponseEmailChannel = DAL.GetEmailChannelByTitleName("PnS_NORDISIssueEmail");
                NORDIS_Issue_Subject = invalidNordisResponseEmailChannel.Subject;
                NORDIS_Issue_Email_To = invalidNordisResponseEmailChannel.To;
                NORDIS_Issue_Email_Cc = invalidNordisResponseEmailChannel.Cc;

                //var unexpectedEmailChannel = DAL.GetEmailChannelByTitleName("PnS_UnexpectedEmail");
                UnexpectedEmail_Subject = ConfigurationManager.AppSettings["UnexpectedEmail_Subject"].Trim();
                UnexpectedEmail_ToEmail = ConfigurationManager.AppSettings["UnexpectedEmail_ToEmail"].Trim();
                UnexpectedEmail_CcEmail = ConfigurationManager.AppSettings["UnexpectedEmail_CcEmail"].Trim();
            }
            catch (Exception ex)
            {
                log.Error("Error occured while initializing Common Properties :", ex);
                MailUtility.SendUnexpectedNotificationEmail(ex.Message);
            }
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
