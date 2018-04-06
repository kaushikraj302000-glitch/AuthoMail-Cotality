using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Console
{
    static class ChannelFactory
    {
        public static Channel Create(string channelPrefix)
        {
            string channelCode;

            // Each channel is tied to one and ONLY one XML schema
            switch (channelPrefix)
            {
                case "AMC":
                    channelCode = channelPrefix + "_" + Config.Monitor_Environment;
                    return new AMCChannel(channelCode);

                //case "ALASKA_Staging":
                //    break;

                //case "HSBC_Staging":
                //    // HSBC
                //    // get channel id by code
                //    // get client id by channel id
                //    break;

                //// AuthoMail XML schema
                //case "AmeriHome_Staging":
                //case "USAA_Staging":
                //    // XML client code , get id
                //    break;

                //case "BatchReporting":

                //    break;
                default:
                    return null;
            }
        }
    }
}
