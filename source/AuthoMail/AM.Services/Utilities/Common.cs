using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
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
