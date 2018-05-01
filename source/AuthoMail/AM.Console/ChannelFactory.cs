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
                    channelCode = channelPrefix + "_" + Config.Common_Environment;
                    return new AMCChannel(channelCode);

                case "ALASKA":
                    channelCode = channelPrefix + "_" + Config.Common_Environment;
                    return new AlaskaChannel(channelCode);

                case "HSBC":
                    channelCode = channelPrefix + "_" + Config.Common_Environment;
                    return new HSBCChannel(channelCode);

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
