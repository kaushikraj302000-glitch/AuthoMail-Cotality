using AM.Console.Properties;
using AM.Entities;
using AM.Lib;
using AM.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace AM.Console
{
    class EDAMSchemaChannel : EDChannel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected IGeneralService generalSvc;

        private string channelType;

        private static XmlSchemaSet schemas;

        static EDAMSchemaChannel()
        {
            schemas = new XmlSchemaSet();
            schemas.Add("http://schemas.corelogic.com/ade/authomail/v1.18.0", XmlReader.Create(new StringReader(Resources.AMMailPieceSchema)));
        }

        public EDAMSchemaChannel(string channelCode, string channelType)
            : base(channelCode)
        {
            generalSvc = GetService<GeneralService>();
            this.channelType = channelType;
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

        protected override void CreateSigningRoom()
        {
            throw new NotImplementedException();
        }

        protected override void SendInvitations()
        {
            throw new NotImplementedException();
        }
    }
}
