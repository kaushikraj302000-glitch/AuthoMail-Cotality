using AM.Entities;
using AM.Services;
using System;
using System.Collections.Generic;

namespace AM.EDelivery
{
    class EDGeneralChannel : EDChannel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected IGeneralService generalSvc;

        private string channelType;

        public EDGeneralChannel(string channelCode, string channelType)
            : base(channelCode)
        {
            generalSvc = GetService<GeneralService>();
            this.channelType = channelType;
        }

        protected override void EnumerateBatches()
        {
            //No need to implement
        }

        protected override void DownloadBatches()
        {
            //No need to implement
        }

        protected override void ValidateBatches()
        {
            log.Debug("EDelivery Validate job queue is started.");

           
            log.Debug("EDelivery Validate job queue is completed.");
        }

        protected override void FallBackBatches()
        {
            log.Debug("EDelivery FallBack job queue is started.");

            List<EDMailPiece> downloadedMailPieces = eDeliverySvc.GetBatchesByPipelineStatusAndSFTChannelId(EDPipelineStatus.FallBack);
            if (downloadedMailPieces == null)
            {
                log.Debug("EDelivery FallBack job queue is empty.");
                return;
            }

            log.Debug("EDelivery FallBack job queue is started.");
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
