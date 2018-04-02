using AM.Entities;
using AM.Lib;
using AM.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Console
{
    abstract class Channel : Disposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public SFTChannel SFTChannel { get; private set; }

        protected ClientService clientSvc;
        protected MailBatchService mailBatchSvc;
        protected CommonService commonSvc;
        protected MailPieceService mailPieceSvc;

        protected List<ServiceBase> services;

        protected Channel(string channelCode)
        {
            services = new List<ServiceBase>();

            // Add services here
            clientSvc = GetService<ClientService>();
            mailBatchSvc = GetService<MailBatchService>();
            commonSvc = GetService<CommonService>();
            mailPieceSvc = GetService<MailPieceService>();

            // Get Channel by code
            SFTChannel = clientSvc.GetSFTChannelDetailsBySFTChannelCode(channelCode);

        }

        private T GetService<T>() where T : ServiceBase, new()
        {
            var service = new T();
            services.Add(service);
            return service;
        }

        public void ProcessBatches()
        {
            this.EnumerateBatches();
            this.DownloadBatches();
            this.ValidateBatches();
            this.AcknowledgeBatches();
        }

        /// <summary>
        /// Checks for available batches and marks them as Received.
        /// </summary>
        protected abstract void EnumerateBatches();
        /// <summary>
        /// Downloads all batches which are queued as Received
        /// </summary>
        protected abstract void DownloadBatches();
        /// <summary>
        /// Validates all batches which are queued as Downloaded
        /// </summary>
        protected abstract void ValidateBatches();
        /// <summary>
        /// Acknowledge all batches which are queued as Validated
        /// </summary>
        protected abstract void AcknowledgeBatches();

        /// <summary>
        /// Sends batch level reports of all batches which are queued as ReportPrepped
        /// </summary>
        public static void ReportBack()
        {
            
        }

        protected override void DisposeManagedObjects()
        {
            services.ForEach(svc => svc.Dispose());
        }

        protected override void DisposeUnManagedObjects()
        {
            return;
        }
    }
}
