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
            log.Debug("EDelivery Check ActivityLogs job queue is started.");

            using (IEDeliveryService eDeliverySvc = new EDeliveryService())
            {
                List<EDMailPiece> invitationSentMailPieces = eDeliverySvc.GetBatchesByPipelineStatusAndSFTChannelId(EDPipelineStatus.InvitationSent);

                if (invitationSentMailPieces.Count == 0)
                {
                    log.Debug("EDelivery Check ActivityLogs job queue is empty.");
                    log.Debug("EDelivery Check ActivityLogs job queue is completed.");
                    return;
                }

                foreach (EDMailPiece mp in invitationSentMailPieces)
                {
                    try
                    {
                        IEDeliveryAPIService eDeliveryAPISvc = new EDeliveryAPIService();

                        var activityLogs = eDeliveryAPISvc.GetSigningRoomActivityLog(mp.SigningRoomName).Result;
                        foreach (var actlog in activityLogs)
                        {
                            switch (actlog.EDeliveryStatus)
                            {
                                case ActivityStatus.ESignConsentAccepted:
                                    mp.IsConsented = true;
                                    mp.ConsentedOn = actlog.UpdatedDate.PstToUtc();
                                    eDeliverySvc.UpdateEDMailPieceActivityStatus(mp.Id, EDeliveryStatus.Consented, actlog.UpdatedDate);
                                    break;
                                case ActivityStatus.ESignConsentDeclined:
                                    mp.IsNotConsented = true;
                                    mp.NotConsentedOn = actlog.UpdatedDate.PstToUtc();
                                    eDeliverySvc.UpdateEDMailPieceActivityStatus(mp.Id, EDeliveryStatus.NotConsented, actlog.UpdatedDate);
                                    break;
                                case ActivityStatus.DocumentReviewed:
                                    mp.IsDocReviewed = true;
                                    mp.DocReviewedOn = actlog.UpdatedDate.PstToUtc();
                                    eDeliverySvc.UpdateEDMailPieceActivityStatus(mp.Id, EDeliveryStatus.DocReviewed, actlog.UpdatedDate);
                                    break;
                                case ActivityStatus.DocumentDownloaded:
                                    mp.IsDocDownloaded = true;
                                    mp.DocDownloadedOn = actlog.UpdatedDate.PstToUtc();
                                    eDeliverySvc.UpdateEDMailPieceActivityStatus(mp.Id, EDeliveryStatus.DocDownloaded, actlog.UpdatedDate);
                                    break;
                                default:
                                    break;
                            }
                        }

                        if (mp.IsConsented && mp.IsDocDownloaded && mp.ExpiryDate.Value.Date <= mp.ConsentedOn.Value.Date && mp.ExpiryDate.Value.Date <= mp.DocDownloadedOn.Value.Date)
                        {
                            eDeliverySvc.UpdateEDMailPiecePipelineStatus(mp.Id, EDPipelineStatus.eDelivered, Common.Now);
                        }

                        string signingRoomStatus = eDeliveryAPISvc.GetSigningRoomStatus(mp.SigningRoomId.Value).Result;

                        //NOTE : Here system will check if signing room is expired and activity logs have 3 activities only which were created at signing room creating process then system will consider this as Expired.
                        if (signingRoomStatus == SigningRoomStatus.Expired && Common.Today > mp.ExpiryDate.Value.Date)
                        {
                            //Do we need to update error code and description? ask to ravisir
                            eDeliverySvc.UpdateEDMailPiecePipelineStatus(mp.Id, EDPipelineStatus.Expired, Common.Now);
                        }
                    }
                    catch (Exception ex)
                    {
                        var msg = "Error occured while checking for activity logs of Id {0} for client {1} with below error <br/>{2}".Fill(mp.Id, mp.ClientCode, ex);
                        log.Error(msg);
                        MailUtility.SendUnexpectedExceptionEmail(msg);
                        continue;
                    }
                }
            }

            log.Debug("EDelivery Check ActivityLogs job queue is started.");
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
