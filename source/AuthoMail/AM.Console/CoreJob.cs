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

            foreach (var batch in downloadBatches)
            {
                Client client = clientSvc.GetClientDetails(batch.ClientId).Single();
                SFTChannel sftChannelDetails = clientSvc.GetSFTChannelDetails(batch.SFTChannelId);

                MailBatchInfo mbi = new MailBatchInfo(batch, client, sftChannelDetails);

                var amXmlFilePath = Path.Combine(mbi.AMRequestPath, mbi.BatchName, mbi.BatchName + "_AuthoMail.xml");

                if (!File.Exists(amXmlFilePath))
                {
                    mbi.MailBatch.ValidationStatus = BatchValidationStatus.Invalid;
                    mbi.MailBatch.ValidationErrors = "No AM XML file is available in {0} batch.".Fill(mbi.MailBatch.BatchName);

                    batchSvc.UpdateBatchDetails(mbi.MailBatch);

                    commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, BatchStatus.Failed.ToString());
                    continue;
                }

                MailBatch amParsedBatch = null;
                try
                {
                    XmlDocument amXMLDoc = new XmlDocument();
                    amXMLDoc.LoadXml(File.ReadAllText(amXmlFilePath));
                    amParsedBatch = XmlUtil.Deserialize<MailBatch>(amXMLDoc.OuterXml);
                }
                catch (XmlException ex)
                {
                    mbi.MailBatch.ValidationStatus = BatchValidationStatus.Invalid;
                    mbi.MailBatch.ValidationErrors = ex.Message;

                    batchSvc.UpdateBatchDetails(mbi.MailBatch);

                    commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, BatchStatus.Failed.ToString());
                }


                
            }
        }
    }
}
