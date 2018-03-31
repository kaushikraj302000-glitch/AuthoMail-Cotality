using AM.Entities;
using AM.Services;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AM.Lib;
namespace AM.Console
{
    public class UploadJob
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(UploadJob));
        IClientService clientSvc;
        IBatchService mailBatchSvc;
        IMailPieceService mailPieceSvc;
        ICommonService commonSvc;
        public UploadJob()
        {
            clientSvc = new ClientService();
            mailBatchSvc = new BatchService();
            commonSvc = new CommonService();
            mailPieceSvc = new MailPieceService();
        }

        public void ReportPreppedProcess()
        {
            List<MailBatchDTO> resReceivedBatches = mailBatchSvc.GetBatchesByStatus(BatchPipelineStatus.ShippingResponseReceived);
            if (resReceivedBatches == null)
            {
                log.Debug("Uploading report job queue is empty.");
                return;
            }

            foreach (var batch in resReceivedBatches)
            {
                Client client = clientSvc.GetClientById(batch.ClientId);
                SFTChannel sftChannelDetails = clientSvc.GetSFTChannelDetailsBySFTChannelId(batch.SFTChannelId);

                MailBatchInfo mbi = new MailBatchInfo(batch, client, sftChannelDetails, Common.AuthoMailRootFolder);
                var data = mailBatchSvc.GetMailBatchDetailsByBatchId(mbi.MailBatch.Id);
                if (data != null)
                {
                    commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.AcknowledgeStart, Common.Now);

                    //MailUtility.SendDailyDetailOperationReportEmail(mbi, data.MailPieces, data.MailPieceCustomInfo);

                    commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Acknowledged, Common.Now);
                }
            }
        }
    }
}
