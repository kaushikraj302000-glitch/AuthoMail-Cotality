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

        protected IClientService clientSvc;
        protected IEDeliveryService eDeliverySvc;
        protected ICommonService commonSvc;
        protected IDocTypeService docTypeSvc;

        protected List<ServiceBase> services;

        protected EDChannel()
        {
            services = new List<ServiceBase>();

            // Add services here
            clientSvc = GetService<ClientService>();
            commonSvc = GetService<CommonService>();
            eDeliverySvc = GetService<EDeliveryService>();
            docTypeSvc = GetService<DocTypeService>();
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
                this.CreateSigningRoom();
                this.SendInvitations();
                this.EDelivered();
                this.Expired();
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
        /// Create signing room for all mailpieces which are validated successfully
        /// </summary>
        protected abstract void CreateSigningRoom();
        /// <summary>
        /// Send invitaions to all recipients for which signing rooms are created successfully
        /// </summary>
        protected abstract void SendInvitations();       
        /// <summary>
        /// Take all eDelivered records and generate reports
        /// </summary>
        protected abstract void EDelivered();
        /// <summary>
        /// Take all Expired records and generate reports
        /// </summary>
        protected abstract void Expired();

        #region Check Activity Logs
        private static void CheckActivityLogs()
        {
            
        }
        #endregion

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
