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

namespace AM.Console
{
    public class CoreJob
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CoreJob));
        IClientService clientSvc;
        IBatchService batchSvc;
        ICommonService commonSvc;
        public CoreJob()
        {
            clientSvc = new ClientService();
            batchSvc = new BatchService();
            commonSvc = new CommonService();
        }

        public void ParseAuthoMailSchema()
        {
            List<MailBatchDTO> downloadBatches = batchSvc.GetBatchesByStatus(BatchPipelineStatus.Downloaded);
            if (downloadBatches == null)
            {
                log.Debug("Process batches job queue is empty.");
                return;
            }
        }
    }
}
