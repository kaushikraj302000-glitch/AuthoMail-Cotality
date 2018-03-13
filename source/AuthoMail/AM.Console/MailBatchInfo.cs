using AM.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;
namespace AM.Console
{
    public class MailBatchInfo
    {
        public string BatchRepo { get; private set; }
        public string AMRequestPath { get; private set; }
        public string PnSRequestPath { get; private set; }
        public string PnSResponsePath { get; private set; }
        public string AMResponsePath { get; private set; }
        public string BatchName { get; private set; }
        public MailBatchDTO MailBatch { get; private set; }
        public Client Client { get; private set; }
        public DocType DocType { get; set; }
        public SFTChannel SFTChannel { get; private set; }

        public MailBatchInfo(MailBatchDTO mailBatch, Client client, SFTChannel sftChannel)
        {
            if (client == null || mailBatch == null) throw new ArgumentNullException("Client or MailBatch cannot be null.");

            MailBatch = mailBatch;
            Client = client;
            SFTChannel = sftChannel;

            Initialize(client, mailBatch);
        }

        private void Initialize(Client client, MailBatchDTO mailBatch)
        {
            BatchName = mailBatch.BatchName.Trim();
            //Combined path
            BatchRepo = Path.Combine(Common.AuthoMailRootFolder, client.AppCode, "{0}".Fill(mailBatch.Id));

            AMRequestPath = Path.Combine(BatchRepo, "AMRequest");
            PnSRequestPath = Path.Combine(BatchRepo, "PnSRequest");
            AMResponsePath = Path.Combine(BatchRepo, "AMResponse");
            PnSResponsePath = Path.Combine(BatchRepo, "PnSResponse");
        }
    }
}
