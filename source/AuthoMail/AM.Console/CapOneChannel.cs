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
    class CapOneChannel : AMSchemaChannel
    {
        public CapOneChannel(string channelCode, string channelType)
            : base(channelCode, channelType)
        {

        }

        protected override void EnumerateBatches()
        {
            //Download File logic and separate according to clientcode in csv
        }

        protected override void DownloadBatches()
        {
            //Generate template logic and xml
        }

        protected override void ValidateBatches()
        {
            base.ValidateBatches();
        }

        protected override void AcknowledgeBatches()
        {
            base.AcknowledgeBatches();
        }
    }
}
