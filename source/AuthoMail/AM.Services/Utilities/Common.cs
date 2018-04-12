using AM.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public static class Common
    {
        public static DateTime Now
        {
            get { return DateTime.UtcNow; }
        }

        public static DateTime Today
        {
            get { return DateTime.UtcNow.Date; }
        }

        public static SFT_ProtocolType GetProtocolType(string protocolType)
        {
            SFT_ProtocolType SFT_Protocol;
            switch (protocolType.ToLower())
            {
                case "ftp": SFT_Protocol = SFT_ProtocolType.ftp;
                    break;
                case "sftp": SFT_Protocol = SFT_ProtocolType.sftp;
                    break;
                case "unc": SFT_Protocol = SFT_ProtocolType.unc;
                    break;
                default: throw new Exception("Please provide valid protocol for 'SFT_Protocol_Value'.");
            }
            return SFT_Protocol;
        }
    }

    public static class Env
    {
        public static string Prod = "Prod";
        public static string Staging = "Staging";
        public static string QA = "QA";
        public static string Internal = "Internal";
    }
}
