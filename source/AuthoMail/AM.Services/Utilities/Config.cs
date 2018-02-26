using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public static class Config
    {
        public static Dictionary<string,string> Msg {get; private set;}
        public static void GetMessages()
        {
            ConfigService cs = new ConfigService();
            cs.GetSharedStrings();
        }
    }
}
