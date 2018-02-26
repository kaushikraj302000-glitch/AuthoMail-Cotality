using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Hosting;

namespace AM.Web
{
    public static class Common
    {
        static Common()
        {
            OutputDir = HostingEnvironment.MapPath(ConfigurationManager.AppSettings["OutputDir"]);
            PullBackFormTemplatePath = HostingEnvironment.MapPath(ConfigurationManager.AppSettings["PullBackFormTemplatePath"]);
            ClientTimeZone = ConfigurationManager.AppSettings["ClientTimeZone"];
            Email_SMTPClient = new SmtpClient();
            Email_From = ConfigurationManager.AppSettings["Email_From"];
            Email_To = ConfigurationManager.AppSettings["Email_To"];
            Email_Cc = ConfigurationManager.AppSettings["Email_Cc"];
        }

        public static readonly DateTime Now = DateTime.Now;

        public static readonly DateTime Today = DateTime.Today;

        public static string OutputDir { get; set; }

        public static string PullBackFormTemplatePath { get; set; }

        public static string Email_From { get; set; }

        public static string Email_To { get; set; }

        public static string Email_Cc { get; set; }

        public static string ClientTimeZone { get; set; }

        public static SmtpClient Email_SMTPClient { get; set; }

    }
}