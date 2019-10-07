using AM.Console.Properties;
using AM.Entities;
using AM.Lib;
using AM.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Data.SqlClient;
using AM.USPSLib;
using System.Transactions;
using System.Threading;

namespace AM.Console
{
    class EDAMCChannel : EDChannel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected IAMCService amcSvc;

        private string channelType;

        private static XmlSchemaSet schemas;

        static EDAMCChannel()
        {
            schemas = new XmlSchemaSet();
            schemas.Add("http://schemas.corelogic.com/2015/amc/v1/packages", XmlReader.Create(new StringReader(Resources.PackageInfoSchema)));
        }

        public EDAMCChannel(string channelCode, string channelType)
            : base(channelCode)
        {
            amcSvc = GetService<AMCService>();
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
