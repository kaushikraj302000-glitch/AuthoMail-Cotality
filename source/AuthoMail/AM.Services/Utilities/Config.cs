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
        #endregion
        //public static Dictionary<string,string> Msg {get; private set;}
        static Config()
        {
            #region PullBackMessages
            Msg_API_PB_Aborted = "Mailpiece was already aborted so it cannot be pullback.";
            Msg_API_PB_BatchStatusSubmitted = "Batch is currently In Progress, please try again after few minutes.";
            Msg_API_PB_DataNull = "No mailpiece data found for given loan numbers.";
            Msg_API_PB_Empty = "Mailpieces can not be empty.";
            Msg_API_PB_Error = "Mailpiece has some error.";
            Msg_API_PB_Success = "Mailpiece pullback requested successfully.";
            Msg_API_PB_PBReqFormErr = "Error while generating Pullback request form.";
            Msg_API_PB_PullBackAlreadyRequested = "Pullback requested already for this mailpiece";
            Msg_API_PB_PulledBack = "Mailpiece was already pulledback.";
            Msg_API_PB_Shipped = "Mailpiece already shipped so pullback cannot be requested.";
            API_Email_SMTPClient = new SmtpClient();
            API_Email_To = "avala@corelogic.com";
            API_Email_Cc = "avala@corelogic.com";
            API_PB_ClientTimeZone = "Eastern Standard Time";
            API_PB_PullBackFormTemplatePath = HostingEnvironment.MapPath("~\\Template\\PullBackForm.pdf");
            #endregion

            #region WEB AccountController
            //Account Controller Login Messages
            Msg_Web_Login_InvalidUserName = "Invalid Username.";
            Msg_Web_Login_EmailConfirm = "Please confirm email address.";
            Msg_Web_Login_Default = "Invalid login attempt.";
            Msg_Web_Login_Success = "Login success.";
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
