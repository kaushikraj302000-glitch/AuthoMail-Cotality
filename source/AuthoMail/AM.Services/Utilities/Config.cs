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
        static Config()
        {
            Msg = new Dictionary<string, string>();
            ConfigService cs = new ConfigService();
            var messages = cs.GetSharedStrings();
            foreach (var message in messages)
            {
                Msg.Add(message.Key, message.Value);
            }            
        }
    }
}
