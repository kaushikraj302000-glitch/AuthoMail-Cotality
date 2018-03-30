using AM.Entities;
using AM.Lib;
using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Console
{
    abstract class Channel : Disposable
    {
        public SFTChannel SFTChannel { get; private set; }

        protected ClientService clientSvc;

        protected List<ServiceBase> services;

        protected Channel(string channelCode)
        {
            // Get Channel by code
            SFTChannel = new SFTChannel();

            services = new List<ServiceBase>();

            // Add services here
            clientSvc = GetService<ClientService>();
        }

        private T GetService<T> ()  where T : ServiceBase, new()
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
