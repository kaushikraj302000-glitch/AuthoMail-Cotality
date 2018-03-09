using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Xml.Schema;
using System.Xml;
using System.Xml.Linq;
using AM.Entities;
using AM.Lib;

namespace AM.Core
{
    public static class Common
    {
        public static string AuthoMailRootFolder { get; set; }
        public static SmtpClient Email_SMTPClient { get; private set; }
        public static string PnS_SFT_HostURL { get; private set; }
        public static string PnS_SFT_UserName { get; private set; }
        public static string PnS_SFT_Password { get; private set; }
        public static string PnS_SFT_RootDir { get; private set; }
        public static string CoverTemplateFolder { get; private set; }

        public static readonly DateTime Now = DateTime.UtcNow;

        public static readonly DateTime Today = DateTime.UtcNow.Date;

        static Common()
        {
            AuthoMailRootFolder = ConfigurationManager.AppSettings["AuthoMailRootFolder"];

            PnS_SFT_HostURL = ConfigurationManager.AppSettings["PnS_SFT_HostURL"];
            PnS_SFT_UserName = ConfigurationManager.AppSettings["PnS_SFT_UserName"];
            PnS_SFT_Password = ConfigurationManager.AppSettings["PnS_SFT_Password"];
            PnS_SFT_RootDir = ConfigurationManager.AppSettings["PnS_SFT_RootDir"];

            CoverTemplateFolder = Path.GetFullPath(ConfigurationManager.AppSettings["CoverTemplateFolder"]);

            Email_SMTPClient = new SmtpClient();
        }

        public static void CleanDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            Directory.CreateDirectory(path);
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
        public static DateTime UtcToClientTimeZone(DateTime dt, string clientTimeZone)
        {
            dt = DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
            // Treat date as IST irrecpective of client and server timezone and covert it to UTC
            return TimeZoneInfo.ConvertTime(dt, TimeZoneInfo.FindSystemTimeZoneById("UTC"), TimeZoneInfo.FindSystemTimeZoneById(clientTimeZone));
        }
        public static DateTime EstToUtc(DateTime dt)
        {
            dt = DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
            // Treat date as IST irrecpective of client and server timezone and covert it to UTC
            return TimeZoneInfo.ConvertTime(dt, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"), TimeZoneInfo.FindSystemTimeZoneById("UTC"));
        }
    }
}
