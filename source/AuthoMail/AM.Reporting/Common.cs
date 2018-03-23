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

        static Common()
        {
            AuthoMailRootFolder = ConfigurationManager.AppSettings["AuthoMailRootFolder"];
        }
    }
}
