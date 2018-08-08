using AM.Console.Properties;
using AM.Entities;
using AM.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace AM.Console
{
    class TestHSBCChannel : HSBCChannel
    {
        public TestHSBCChannel(string channelCode, string channelType)
            : base(channelCode, channelType)
        {

        }
    }
}
