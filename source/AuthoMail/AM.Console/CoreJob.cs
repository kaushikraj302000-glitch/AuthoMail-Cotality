using AM.Entities;
using AM.Services;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Console
{
    public class CoreJob
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CoreJob));
        IClientService clientSvc;
        IBatchService batchSvc;
        IMailPieceService mailPieceSvc;
        ICommonService commonSvc;
        IDocTypeService docTypeSvc;
        public CoreJob()
        {
            clientSvc = new ClientService();
            batchSvc = new BatchService();
            mailPieceSvc = new MailPieceService();
            commonSvc = new CommonService();
            docTypeSvc = new DocTypeService();
        }

        public void ProcessBatches()
        {
            List<MailBatchDTO> downloadBatches = batchSvc.GetBatchesByStatus(BatchPipelineStatus.Downloaded);

            if (downloadBatches == null)
            {
                log.Debug("Process batches job queue is empty.");
                return;
            }

            foreach (MailBatchDTO batch in downloadBatches)
            {
                Client client = clientSvc.GetClientDetails(batch.ClientId).Single();
                List<DocType> docTypes = docTypeSvc.GetDocTypesByClientIdOrByDocTypeName(client.Id, string.Empty);

                List<MailPieceDTO> mailPieces = mailPieceSvc.GetMailPiecesByBatchClustId(batch.Id);

                if (docTypes == null || docTypes.Count == 0)
                {
                    log.Error("Document type is not assigned for client : " + client.AppCode);
                    continue;
                }

                bool isBatchDocTypeAvailable = string.IsNullOrEmpty(batch.DocumentOrPackageType);
                DocType batchDocType;
                if (isBatchDocTypeAvailable)
                    batchDocType = docTypes.Single(x => x.DocTypeCode.Contains(batch.DocumentOrPackageType));

                if (mailPieces == null || mailPieces.Count == 0)
                {
                    log.Error("Mail pieces doen not found for Batch : " + batch.BatchName);
                    continue;
                }

                var errorMailPieces = mailPieces.Where(x => x.ErrorCode != OutComeCodes.No_Error.ToString()).ToList();
                if ((errorMailPieces != null && errorMailPieces.Count > 0))
                {
                    log.Error("All mail pieces have error in documents for Batch :" + batch.BatchName);
                    //TODO: mail to client or internal team
                }

                if (errorMailPieces != null && errorMailPieces.Count == mailPieces.Count)
                {
                    log.Error("All mail pieces have error in documents for Batch :" + batch.BatchName);
                    //TODO: Update Batch tables and mail to client or internal team

                    continue;
                }

                var noErrorMailPieces = mailPieces.Where(x => x.ErrorCode == OutComeCodes.No_Error.ToString()).ToList();

                foreach (MailPieceDTO mailPiece in noErrorMailPieces)
                {

                }

            }
        }
    }
}
