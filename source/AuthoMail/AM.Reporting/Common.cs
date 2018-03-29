using AM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;
using System.Configuration;

namespace AM.Reporting
{
    static class Common
    {
        public static readonly DateTime Now = DateTime.UtcNow;

        public static readonly DateTime Today = DateTime.UtcNow.Date;

        public static string AuthoMailRootFolder { get; set; }
        public static string ReportUploadPath { get; set; }

        static Common()
        {
            AuthoMailRootFolder = ConfigurationManager.AppSettings["AuthoMailRootFolder"];
            ReportUploadPath = ConfigurationManager.AppSettings["ReportUploadPath"];
        }

        public static SFT_ProtocolType GetProtocolType(string protocolType)
        {
            SFT_ProtocolType PnS_SFT_Protocol;
            switch (protocolType.ToLower())
            {
                case "ftp": PnS_SFT_Protocol = SFT_ProtocolType.ftp;
                    break;
                case "sftp": PnS_SFT_Protocol = SFT_ProtocolType.sftp;
                    break;
                case "unc": PnS_SFT_Protocol = SFT_ProtocolType.unc;
                    break;
                default: throw new Exception("Please provide valid protocol for 'PnS_SFT_Protocol_Value'.");
            }
            return PnS_SFT_Protocol;
        }
    }
}
