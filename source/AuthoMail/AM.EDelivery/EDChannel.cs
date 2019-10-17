using AM.Entities;
using AM.Lib;
using AM.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace AM.EDelivery
{
    abstract class EDChannel : Disposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public SFTChannel SFTChannel { get; private set; }

        protected IClientService clientSvc;
        protected IEDeliveryService eDeliverySvc;
        protected ICommonService commonSvc;
        protected IDocTypeService docTypeSvc;

        protected List<ServiceBase> services;

        protected EDChannel(string channelCode)
        {
            services = new List<ServiceBase>();

            // Add services here
            clientSvc = GetService<ClientService>();
            commonSvc = GetService<CommonService>();
            eDeliverySvc = GetService<EDeliveryService>();
            docTypeSvc = GetService<DocTypeService>();

            if (channelCode != "GENERAL")
            {
                SFTChannel = clientSvc.GetSFTChannelDetailsBySFTChannelCode(channelCode);
            }
            // Get Channel by code
        }

        protected T GetService<T>() where T : ServiceBase, new()
        {
            var service = new T();
            services.Add(service);
            return service;
        }

        public void ProcessBatches()
        {
            try
            {
                this.EnumerateBatches();
                this.DownloadBatches();
                this.ValidateBatches();
                this.CreateSigningRoom();
                this.SendInvitations();
                this.CheckActivityLogs();
            }
            catch (Exception ex)
            {
                var msg = "Unexpected error occured in EDChannel : {0}".Fill(ex);

                // Log exception
                log.Error(msg);

                // Send email to support team
                MailUtility.SendUnexpectedExceptionEmail(msg);
            }
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
        /// Create signing room for all mailpieces which are validated successfully
        /// </summary>
        protected abstract void CreateSigningRoom();
        /// <summary>
        /// Send invitaions to all recipients for which signing rooms are created successfully
        /// </summary>
        protected abstract void SendInvitations();
        /// <summary>
        /// Send invitaions to all recipients for which signing rooms are created successfully
        /// </summary>
        protected abstract void CheckActivityLogs();


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
