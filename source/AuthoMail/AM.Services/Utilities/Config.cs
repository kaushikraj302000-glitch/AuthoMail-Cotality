using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace AM.Services
{
    public static class Config
    {
        #region General
        public static string Environment { get; private set; }

        #endregion

        #region Core & Console
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

        public static string ReportSFTUploadingChannelCode { get; private set; }
        public static string ReportSFTUploadingPathAMC { get; private set; }
        public static string USAACustIdentifier { get; set; }
        public static string AMCDirPath { get; set; }
        #endregion

        #region PullBackRequestProps
        public static string Msg_API_PB_Aborted { get; private set; }
        public static string Msg_API_PB_BatchStatusSubmitted { get; private set; }
        public static string Msg_API_PB_DataNull { get; private set; }
        public static string Msg_API_PB_Empty { get; private set; }
        public static string Msg_API_PB_Error { get; private set; }
        public static string Msg_API_PB_Success { get; private set; }
        public static string Msg_API_PB_PBReqFormErr { get; private set; }
        public static string Msg_API_PB_PullBackAlreadyRequested { get; private set; }
        public static string Msg_API_PB_PulledBack { get; private set; }
        public static string Msg_API_PB_Shipped { get; private set; }
        public static SmtpClient API_Email_SMTPClient { get; set; }
        public static string API_Email_To { get; private set; }
        public static string API_Email_Cc { get; private set; }
        public static string API_PB_ClientTimeZone { get; private set; }
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
        public static string Msg_Web_NoRecordFound { get; set; }
        public static string Msg_Web_PullBackConfirmation { get; set; }
        #endregion

        #region WEB RequestPullBackController
        public static string Msg_Web_NoMailPieceSelect { get; set; }
        public static string Msg_Web_NoErrorDescription { get; set; }
        #endregion

        #region WEB HomeController
        public static string Msg_Web_NoMailPieceReceivedToday { get; set; }
        #endregion

        #region WEB ReportController
        public static string StartCutOffTime { get; private set; }
        public static string EndCutOffTime { get; private set; }
        #endregion

        //public static Dictionary<string,string> Msg {get; private set;}
        static Config()
        {
            #region General
            Environment = "Internal";
            AuthoMailRootFolder = @"D:\AuthoMailGit\Repos\source\AuthoMail\AM.Console\bin\Debug\AuthoMailRoot";
            #endregion

            #region Console & Core
            PnS_SFT_HostURL = @"\\MTC-sagarg\c$\MSTC\PnS";
            PnS_SFT_UserName = "user";
            PnS_SFT_Password = "";
            PnS_SFT_RootDir = @"\\MTC-sagarg\c$\MSTC\PnS";

            DSEmailTemplate = @"D:\AuthoMailGit\Repos\source\AuthoMail\AM.Console\HTMLTemplates\DET.html";
            DSEmail_Subject = "TEST USAA Download Summary";
            DS_Email_To = "sgolvelkar@corelogic.com";
            DS_Email_Cc = "sgolvelkar@corelogic.com";

            PSEmailTemplate = @"D:\AuthoMailGit\Repos\source\AuthoMail\AM.Console\HTMLTemplates\PLET.html";
            PSEmail_Subject = "TEST USAA Production Summary";
            PS_Email_To = "sgolvelkar@corelogic.com";
            PS_Email_Cc = "sgolvelkar@corelogic.com";

            NoBatchReceivedNotification_Subject = "No USAA batch is available";
            NoBatch_Email_To = "sgolvelkar@corelogic.com";
            NoBatch_Email_Cc = "sgolvelkar@corelogic.com";

            PnSEmail_Subject = "TEST USAA Status Update";
            PnS_Email_To = "sgolvelkar@corelogic.com";
            PnS_Email_Cc = "sgolvelkar@corelogic.com";

            USAACustIdentifier = "166";
            AMCDirPath = @"\\MTC-sagarg\c$\MSTC\AM\AMC";

            ReportSFTUploadingChannelCode = "USAA_CL";
            ReportSFTUploadingPathAMC = @"/Unknown/USAA/Output";
            #endregion

            #region PullBackMessages
            Msg_API_PB_Aborted = "Mailpiece is aborted, it is not going to be printed.";
            Msg_API_PB_BatchStatusSubmitted = "Batch is being parsed, please try again after few minutes.";
            Msg_API_PB_DataNull = "Mailpiece with the specified unique id is not yet received.";
            Msg_API_PB_Empty = "At least one mailpiece id must be specified.";
            Msg_API_PB_Error = "Mailpiece has an error, it is not going to be printed. Check error report for details.";
            Msg_API_PB_Success = "Pullback request is recorded successfully.";
            Msg_API_PB_PBReqFormErr = "An error occured while generating pullback request form.";
            Msg_API_PB_PullBackAlreadyRequested = "Pullback is already requested for this mailpiece.";
            Msg_API_PB_PulledBack = "Mailpiece is already pulled back.";
            Msg_API_PB_Shipped = "Mailpiece is already shipped, pullback cannot be requested.";
            API_Email_SMTPClient = new SmtpClient();
            API_Email_To = "CoreLogic.PrintProduction@pcigroup.com";
            //API_Email_To = "avala@corelogic.com";
            API_Email_Cc = "avala@corelogic.com,sgolvelkar@corelogic.com,brpatel@corelogic.com";
            //API_Email_Cc = "avala@corelogic.com";
            API_PB_ClientTimeZone = "Eastern Standard Time";
            API_PB_PullBackFormTemplatePath = HostingEnvironment.MapPath("~\\Template\\PullBackForm.pdf");
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
            //Msg = new Dictionary<string, string>();
            //ConfigService cs = new ConfigService();
            //var messages = cs.GetSharedStrings();
            //foreach (var message in messages)
            //{
            //    Msg.Add(message.Key, message.Value);
            //}            
        }

    }
}
