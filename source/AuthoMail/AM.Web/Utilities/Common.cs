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
            
        }

        public static readonly DateTime Now = DateTime.UtcNow;

        public static readonly DateTime Today = DateTime.Today;

    }
}