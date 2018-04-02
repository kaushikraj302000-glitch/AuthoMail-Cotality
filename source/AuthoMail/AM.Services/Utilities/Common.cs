using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public static class Common
    {
        public static DateTime Now { get { return DateTime.UtcNow; } }
        public static DateTime Today { get { return DateTime.UtcNow.Date; } }
    }
}
