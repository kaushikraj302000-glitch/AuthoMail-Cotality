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
            channelCode = channelPrefix + "_" + Config.Common_Environment;
            switch (channelPrefix)
            {
                case "AMC":
                    return new AMCChannel(channelCode);

                case "ALASKA":
                    return new AlaskaChannel(channelCode);

                case "HSBC":
                    return new HSBCChannel(channelCode);

                //// AuthoMail XML schema
                case "AmeriHome":
                case "USAA":
                    // XML client code , get id
                    return new GeneralChannel(channelCode);

                //case "BatchReporting":

                //    break;
                default:
                    return null;
            }
        }
    }
}
