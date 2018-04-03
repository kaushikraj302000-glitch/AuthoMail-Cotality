using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using AM.Lib;
using System.ComponentModel;

namespace AM.Services
{
    public class Config
    {
        #region General
        public static string Environment { get; private set; }

        #endregion

        #region Console & Core
        public static string AuthoMailRootFolder { get; private set; }

        public static string PnS_SFT_HostURL { get; private set; }
        public static string PnS_SFT_UserName { get; private set; }
        public static string PnS_SFT_Password { get; private set; }
        public static string PnS_SFT_RootDir { get; private set; }

        public static string DSEmail_Subject { get; private set; }
        public static string DSEmailTemplate { get; private set; }
        public static string DS_Email_To { get; private set; }
        public static string DS_Email_Cc { get; private set; }

        public static string PSEmail_Subject { get; private set; }
        public static string PSEmailTemplate { get; private set; }
        public static string PS_Email_To { get; private set; }
        public static string PS_Email_Cc { get; private set; }

        public static string NoBatchReceivedNotification_Subject { get; set; }
        public static string NoBatch_Email_To { get; private set; }
        public static string NoBatch_Email_Cc { get; private set; }


        public static string PnSEmail_Subject { get; private set; }
        public static string PnS_Email_To { get; private set; }
        public static string PnS_Email_Cc { get; private set; }

        public static string ExceptionEmail_Subject { get; private set; }
        public static string Exception_Email_To { get; private set; }
        public static string Exception_Email_Cc { get; private set; }

        public static string ReportSFTUploadingChannelCode { get; private set; }
        public static string ReportSFTUploadingPathAMC { get; private set; }
        public static string USAACustIdentifier { get; set; }
        public static string AMCDirPath { get; set; }

        //Core
        public static string CoverTemplateFolder { get; private set; }

        public static string CountryCode { get; private set; }
        public static int NoofTime_AttemptFedexService { get; private set; }
        public static int FedexServiceAttemptInterval { get; private set; }
        public static string CustomerTransactionId { get; set; }
        #endregion

        #region PullBackRequestProps
        [Description("When mailpiece is aborted by client or for unknown reason.")]
        public static string Msg_API_PB_Aborted { get; private set; }
        [Description("If batchstatus is submitted then only mailpiece can be pullback.")]
        public static string Msg_API_PB_BatchStatusSubmitted { get; private set; }
        [Description("When no data found for specified unique id.")]
        public static string Msg_API_PB_DataNull { get; private set; }
        [Description("user have select atleast one mailpiece to request pullback.")]
        public static string Msg_API_PB_Empty { get; private set; }
        [Description("If mailpiece has some error.")]
        public static string Msg_API_PB_Error { get; private set; }
        [Description("If mailpiece has recorded successfully.")]
        public static string Msg_API_PB_Success { get; private set; }
        [Description("When error occured while generating pullback form.")]
        public static string Msg_API_PB_PBReqFormErr { get; private set; }
        [Description("When pullback is already requested by some user.")]
        public static string Msg_API_PB_PullBackAlreadyRequested { get; private set; }
        [Description("When mailpiece is pulledback.")]
        public static string Msg_API_PB_PulledBack { get; private set; }
        [Description("When mailpiece statis is shipped then it can not be pullback.")]
        public static string Msg_API_PB_Shipped { get; private set; }
        [Description("To define SMTPClient for email.")]
        public static SmtpClient API_Email_SMTPClient { get; private set; }
        [Description("To whom need to send email.")]
        public static string API_Email_To { get; private set; }
        [Description("To whom need to send email.")]
        public static string API_Email_Cc { get; private set; }
        [Description("Define client's local timezone.")]
        public static string API_PB_ClientTimeZone { get; private set; }
        [Description("Path of pdf template which is used to send when pullback is requested.")]
        public static string API_PB_PullBackFormTemplatePath { get; private set; }
        #endregion

        #region WEB AccountController
        public static string Msg_Web_Login_InvalidUserName { get; private set; }
        public static string Msg_Web_Login_EmailConfirm { get; private set; }
        public static string Msg_Web_Login_Default { get; private set; }
        public static string Msg_Web_Login_Success { get; private set; }
        public static string MSG_WEB_Lockout { get; private set; }
        public static string MSG_WEB_Error { get; private set; }
        #endregion

        #region WEB Common Msg
        public static string Msg_Web_Ajax_CommonError { get; private set; }
        public static string Msg_Web_NoRecordFound { get; private set; }
        public static string Msg_Web_PullBackConfirmation { get; private set; }
        #endregion

        #region WEB RequestPullBackController
        public static string Msg_Web_NoMailPieceSelect { get; private set; }
        public static string Msg_Web_NoErrorDescription { get; private set; }
        #endregion

        #region WEB HomeController
        public static string Msg_Web_NoMailPieceReceivedToday { get; private set; }
        #endregion

        #region WEB ReportController
        public static string StartCutOffTime { get; private set; }
        public static string EndCutOffTime { get; private set; }
        #endregion

        private static List<string> errors;
        //public static Dictionary<string,string> Msg {get; private set;}
        static Config()
        {

            #region General
            Environment = "Staging";
            AuthoMailRootFolder = @"D:\MTC\USAA\Monitor\AuthoMailRoot";
            #endregion

            #region Console & Core
            PnS_SFT_HostURL = @"\\slxqn1vdappsr01.infosolco.net\FAGOSFTP\ADE_PnS";
            PnS_SFT_UserName = "user";
            PnS_SFT_Password = "";
            PnS_SFT_RootDir = @"\\slxqn1vdappsr01.infosolco.net\FAGOSFTP\ADE_PnS";

            DSEmailTemplate = @"D:\MTC\USAA\Monitor\HTMLTemplates\DET.html";
            DSEmail_Subject = "TEST USAA Download Summary";
            DS_Email_To = "brpatel@corelogic.com";
            DS_Email_Cc = "sgolvelkar@corelogic.com,avala@corelogic.com";

            PSEmailTemplate = @"D:\MTC\USAA\Monitor\HTMLTemplates\PLET.html";
            PSEmail_Subject = "TEST USAA Production Summary";
            PS_Email_To = "brpatel@corelogic.com";
            PS_Email_Cc = "sgolvelkar@corelogic.com,avala@corelogic.com";

            NoBatchReceivedNotification_Subject = "No USAA batch is available";
            NoBatch_Email_To = "brpatel@corelogic.com";
            NoBatch_Email_Cc = "sgolvelkar@corelogic.com,avala@corelogic.com";

            PnSEmail_Subject = "TEST USAA Status Update";
            PnS_Email_To = "sgolvelkar@corelogic.com";
            PnS_Email_Cc = "sgolvelkar@corelogic.com";

            ExceptionEmail_Subject = "Unexpected error occurred in AuthoMail";
            Exception_Email_To = "sgolvelkar@corelogic.com,avala@corelogic.com,vmungara@corelogic.com";
            Exception_Email_Cc = "nirav.barot@milestonetechnoconsulting.com,milan.patel@milestonetechnoconsulting.com";

            USAACustIdentifier = "166";
            AMCDirPath = @"\\slxqn1vdappsr01.infosolco.net\FAGOSFTP\ADE_AM\UAT\AMC";

            ReportSFTUploadingChannelCode = "AMC_Staging";
            ReportSFTUploadingPathAMC = @"/Reports";

            //Core Config

            CoverTemplateFolder = @"D:\MTC\USAA\Core\CoverTemplates";
            CountryCode = "US";
            FedexServiceAttemptInterval = 15000;
            NoofTime_AttemptFedexService = 5;
            CustomerTransactionId = "***Express Domestic Ship v7 Request using VC#***";
            #endregion

            #region PullBackMessages
            API_Email_SMTPClient = new SmtpClient();
            #endregion

            #region WEB AccountController
            //Account Controller Login Messages
            Msg_Web_Login_InvalidUserName = "Invalid username.";
            Msg_Web_Login_EmailConfirm = "Please confirm email address.";
            Msg_Web_Login_Default = "Invalid login attempt.";
            Msg_Web_Login_Success = "Login successful.";
            MSG_WEB_Lockout = "This account has been locked out, please try again later.";
            MSG_WEB_Error = "An error occurred while processing your request.";
            #endregion

            #region WEB Common Msg
            Msg_Web_Ajax_CommonError = "Communication error. Please check your internet connection and try again.";
            Msg_Web_NoRecordFound = "No Records";
            Msg_Web_PullBackConfirmation = "Click 'Yes' to confirm your pullback request.";
            #endregion

            #region WEB RequestPullBackController
            Msg_Web_NoMailPieceSelect = "Please select at least one mailpiece.";
            Msg_Web_NoErrorDescription = "No error description.";
            #endregion

            #region WEB HomeController
            Msg_Web_NoMailPieceReceivedToday = "No batches received today.";
            #endregion

            #region WEB ReportController
            StartCutOffTime = "19:45:01";
            EndCutOffTime = "19:45:00";
            #endregion

            // Initialize config settings
            InitializeProperties<Config>();

        }

        private static void InitializeProperties<T>()
        {
            errors = new List<string>();

            // Load settings from database
            ConfigService cs = new ConfigService();
            var settings = cs.GetSystemSettings();

            PropertyInfo[] propInfo = typeof(T).GetProperties(BindingFlags.Static | BindingFlags.Public);

            foreach (PropertyInfo p in propInfo)
            {
                //var isRequired = p.CustomAttributes.Any(ca => ca.AttributeType == typeof(RequiredInConfigAttribute));

                var authomailSetting = settings.SingleOrDefault(s => s.Key == p.Name);// db.ERPSettings.Find(p.Name);

                // Ignore if not stored in the database
                if (authomailSetting == null) continue;


                object propValue;
                try
                {
                    propValue = p.PropertyType.GetConversionMethod().Invoke(null, new object[] { authomailSetting.Value });
                }
                catch (Exception ex)
                {
                    propValue = null;
                    errors.Add(string.Format("{0}, Type - {1} : {2}", p.Name, p.PropertyType.Name, ex.ToString()));
                }

                p.SetValue(null, propValue);
                //if (isRequired && (propValue == null || String.IsNullOrWhiteSpace(propValue.ToString())))
                //{
                //    errors.Add(p.Name);
                //}

                //else if (isRequired)
                //{
                //    errors.Add(p.Name);
                //}
            }

        }

        public static void SaveSetting(string key, string value)
        {
            SaveSetting(key, value, null, null);
        }

        public static void SaveSetting(string key, string value, bool? IsSystem, string DefaultValue)
        {

            var prop = typeof(Config).GetProperty(key, BindingFlags.Static | BindingFlags.Public);

            if (prop == null)
            {
                throw new ArgumentException("No such AM setting found. ", key);
            }

            ConfigService cs = new ConfigService();
            var settings = cs.GetSystemSettings();
            var authomailSetting = settings.SingleOrDefault(s => s.Key == key);   //db.ERPSettings.Find(key);

            var propValue = prop.PropertyType.GetConversionMethod().Invoke(null, new object[] { value });
            prop.SetValue(null, propValue);

            if (authomailSetting == null)
            {
                cs.InsertSystemSetting(new AMSetting
                {
                    Key = key,
                    Value = value,
                    IsSystem = IsSystem.HasValue ? IsSystem.Value : false,
                    DefaultValue = DefaultValue,
                    Description = prop.GetDescription(),
                    DataType = prop.PropertyType.Name
                });
            }
            else
            {
                cs.UpdateSystemSetting(new AMSetting
                {
                    DataType = prop.PropertyType.Name,
                    Value = value,
                    Description = prop.GetDescription(),
                    Key = key
                });
            }
        }
    }
}
