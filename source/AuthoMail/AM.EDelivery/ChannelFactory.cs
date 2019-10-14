using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.EDelivery
{
    static class ChannelFactory
    {
        public static EDChannel Create(string channelPrefix, string channelType)
        {
            // Each channel is tied to one and ONLY one XML schema
            string channelCode = channelPrefix + "_" + Config.Common_Environment;

            switch (channelPrefix)
            {
                case "GENERAL"://This channel will be used for those client whose coming from existing print and ship clients
                    return new EDGeneralChannel(channelCode, channelType);
                //case "AMC":
                //    return new EDAMCChannel(channelCode, channelType);
                // Client is using Generic XML schema 
                default:
                    return null;// new EDAMSchemaChannel(channelCode, channelType);
            }
        }
    }
}
