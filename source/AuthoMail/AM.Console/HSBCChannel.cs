using AM.Console.Properties;
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
    class HSBCChannel : Channel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static XmlSchemaSet schemas;

        protected IHSBCService hsbcSvc;

        static HSBCChannel()
        {
            schemas = new XmlSchemaSet();
            schemas.Add(string.Empty, XmlReader.Create(new StringReader(Resources.RespaControlSchema)));
        }

        public HSBCChannel(string channelCode)
            : base(channelCode)
        {
            hsbcSvc = GetService<HSBCService>();
        }

        protected override void EnumerateBatches()
        {
            throw new NotImplementedException();
        }

        protected override void DownloadBatches()
        {
            throw new NotImplementedException();
        }

        protected override void ValidateBatches()
        {
            throw new NotImplementedException();
        }

        protected override void AcknowledgeBatches()
        {
            throw new NotImplementedException();
        }
    }
}
