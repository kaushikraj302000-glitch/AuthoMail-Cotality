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
        [Description("To indicate on which environment job is running.")]
        public static string Monitor_Environment { get; private set; }

        #endregion

        #region Console & Core
        public static string Common_AuthoMailRootFolder { get; private set; }

        public static string Monitor_DSEmail_Subject { get; private set; }
        public static string Monitor_DSEmailTemplate { get; private set; }
        public static string Monitor_DS_Email_To { get; private set; }
        public static string Monitor_DS_Email_Cc { get; private set; }

        public static string Monitor_PSEmail_Subject { get; private set; }
        public static string Monitor_PSEmailTemplate { get; private set; }
        public static string Monitor_PS_Email_To { get; private set; }
        public static string Monitor_PS_Email_Cc { get; private set; }

        public static string Monitor_NoBatchReceivedNotification_Subject { get; set; }
        public static string Monitor_NoBatch_Email_To { get; private set; }
        public static string Monitor_NoBatch_Email_Cc { get; private set; }

        public static string Monitor_ExceptionEmail_Subject { get; private set; }
        public static string Monitor_Exception_Email_To { get; private set; }
        public static string Monitor_Exception_Email_Cc { get; private set; }
        public static string Monitor_ReportSFTUploadingPathAMC { get; private set; }
        [Description("This is unique customer identifier")]
        public static string Monitor_USAACustIdentifier { get; set; }
        public static string Monitor_AMCDirPath { get; set; }

        //Core
        public static string Core_PnS_SFT_HostURL { get; private set; }
        public static string Core_PnS_SFT_UserName { get; private set; }
        public static string Core_PnS_SFT_Password { get; private set; }
        public static string Core_PnS_SFT_RootDir { get; private set; }

        public static string Core_PnSEmail_Subject { get; private set; }
        public static string Core_PnS_Email_To { get; private set; }
        public static string Core_PnS_Email_Cc { get; private set; }

        public static string Core_CoverTemplateFolder { get; private set; }
        public static string Core_CountryCode { get; private set; }
        public static int Core_NoofTime_AttemptFedexService { get; private set; }
        public static int Core_FedexServiceAttemptInterval { get; private set; }
        public static string Core_CustomerTransactionId { get; set; }
        #endregion

        #region Reporting_ReadOnly
        public static readonly TimeSpan Reporting_USAA_StartCutOffTime=TimeSpan.Parse(Web_StartCutOffTime);
        
        public static readonly TimeSpan Reporting_USAA_EndCutOffTime= TimeSpan.Parse(Web_EndCutOffTime);
       
        #endregion Reporting_ReadOnly

        #region PullBackRequestProps
        [Description("When mailpiece is aborted by client or for unknown reason.")]
        public static string Web_Msg_API_PB_Aborted { get; private set; }
        [Description("If batchstatus is submitted then only mailpiece can be pullback.")]
        public static string Web_Msg_API_PB_BatchStatusSubmitted { get; private set; }
        [Description("When no data found for specified unique id.")]
        public static string Web_Msg_API_PB_DataNull { get; private set; }
        [Description("user have select atleast one mailpiece to request pullback.")]
        public static string Web_Msg_API_PB_Empty { get; private set; }
        [Description("If mailpiece has some error.")]
        public static string Web_Msg_API_PB_Error { get; private set; }
        [Description("If mailpiece has recorded successfully.")]
        public static string Web_Msg_API_PB_Success { get; private set; }
        [Description("When error occured while generating pullback form.")]
        public static string Web_Msg_API_PB_PBReqFormErr { get; private set; }
        [Description("When pullback is already requested by some user.")]
        public static string Web_Msg_API_PB_PullBackAlreadyRequested { get; private set; }
        [Description("When mailpiece is pulledback.")]
        public static string Web_Msg_API_PB_PulledBack { get; private set; }
        [Description("When mailpiece statis is shipped then it can not be pullback.")]
        public static string Web_Msg_API_PB_Shipped { get; private set; }
        [Description("To whom need to send email.")]
        public static string Web_API_Email_To { get; private set; }
        [Description("To whom need to send email.")]
        public static string Web_API_Email_Cc { get; private set; }
        public static string Web_API_Email_Subject { get; private set; }
        public static string Web_API_Email_BodyMsg { get; private set; }
        [Description("Define client's local timezone.")]
        public static string Web_API_PB_ClientTimeZone { get; private set; }
        [Description("Path of pdf template which is used to send when pullback is requested.")]
        public static string Web_API_PB_PullBackFormTemplatePath { get; private set; }
        #endregion

        #region WEB AccountController
        public static string Web_Msg_Login_InvalidUserName { get; private set; }
        public static string Web_Msg_Login_EmailConfirm { get; private set; }
        public static string Web_Msg_Login_Default { get; private set; }
        public static string Web_Msg_Login_Success { get; private set; }
        public static string Web_Msg_Lockout { get; private set; }
        public static string Web_Msg_Error { get; private set; }
        #endregion

        #region WEB Common Msg
        public static string Web_Msg_Ajax_CommonError { get; private set; }
        public static string Web_Msg_NoRecordFound { get; private set; }
        public static string Web_Msg_PullBackConfirmation { get; private set; }
        #endregion

        #region WEB RequestPullBackController
        public static string Web_Msg_NoMailPieceSelect { get; private set; }
        public static string Web_Msg_NoErrorDescription { get; private set; }
        #endregion

        #region WEB HomeController
        public static string Web_Msg_NoMailPieceReceivedToday { get; private set; }
        #endregion

        #region WEB ReportController
        [Description("Start cut off time for Mailpiece")]
        public static string Web_StartCutOffTime { get; private set; }
        [Description("End cut off time for Mailpiece")]
        public static string Web_EndCutOffTime { get; private set; }
        #endregion

        private static List<string> errors;
        static Config()
        {
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

                var authomailSetting = settings.SingleOrDefault(s => s.Id == p.Name);// db.ERPSettings.Find(p.Name);

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
            var authomailSetting = settings.SingleOrDefault(s => s.Id == key);   //db.ERPSettings.Find(key);

            var propValue = prop.PropertyType.GetConversionMethod().Invoke(null, new object[] { value });
            prop.SetValue(null, propValue);

            if (authomailSetting == null)
            {
                cs.InsertSystemSetting(new AMSetting
                {
                    Id = key,
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
                    ModifiedOn = Common.Now,
                    Id = key
                });
            }
        }

        public static void SaveAllSetting()
        {
            PropertyInfo[] propInfo = typeof(Config).GetProperties(BindingFlags.Static | BindingFlags.Public);
            ConfigService cs = new ConfigService();
            var settings = cs.GetSystemSettings();

            foreach (PropertyInfo p in propInfo)
            {
                var authomailSetting = settings.SingleOrDefault(s => s.Id == p.Name);
                if (authomailSetting != null)
                {
                    cs.UpdateSystemSetting(new AMSetting
                  {
                      DataType = p.PropertyType.Name,
                      Value = authomailSetting.Value,
                      Description = p.GetDescription(),
                      ModifiedOn = Common.Now,
                      Id = p.Name
                  });
                }
            }
        }
    }
}
