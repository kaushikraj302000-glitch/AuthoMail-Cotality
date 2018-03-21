using AM.Entities;
using AM.Services;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;

namespace AM.Reporting
{
    class UploadJob
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
                Client client = clientSvc.GetClientDetails(batch.ClientId).Single();

                MailBatchInfo mbi = new MailBatchInfo(batch, client);
                var data = mailBatchSvc.GetMailBatchDetailsByBatchId(mbi.MailBatch.Id);
                if (data != null)
                {
                    try
                    {
                        StringBuilder deliveryReport = Common.USAADailyDetailOperationReport(data);
                        string deliveryReportFileName = "{0}_{1}.csv".Fill("DR", data.MailBatch.BatchName);
                        using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(deliveryReport.ToString())))
                        {
                            using (FileStream file = new FileStream(Path.Combine(mbi.AMResponsePath, deliveryReportFileName), FileMode.Create, FileAccess.Write))
                            {
                                ms.WriteTo(file);
                            }
                        }

                        StringBuilder errorReport = Common.USAADailyDetailOperationReport(data);
                        string errorReportFileName = "{0}_{1}.csv".Fill("ER", data.MailBatch.BatchName);
                        using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(errorReport.ToString())))
                        {
                            using (FileStream file = new FileStream(Path.Combine(mbi.AMResponsePath, errorReportFileName), FileMode.Create, FileAccess.Write))
                            {
                                ms.WriteTo(file);
                            }
                        }

                        commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportPrepped, Common.Now);
                    }
                    catch (Exception ex)
                    {
                        log.Error("Error occured while generating reports : {0}".Fill(ex.ToString()));
                    }
                }
            }
        }

        public void ReportBackedProcess()
        {
            List<MailBatchDTO> resReceivedBatches = mailBatchSvc.GetBatchesByStatus(BatchPipelineStatus.ReportPrepped);
            if (resReceivedBatches == null)
            {
                log.Debug("Uploading report job queue is empty.");
                return;
            }

            foreach (var batch in resReceivedBatches)
            {
                Client client = clientSvc.GetClientDetails(batch.ClientId).Single();
                SFTChannel sftChannelDetails = clientSvc.GetSFTChannelDetailsBySFTChannelCode("USAA_CL");
                MailBatchInfo mbi = new MailBatchInfo(batch, client);
                var data = mailBatchSvc.GetMailBatchDetailsByBatchId(mbi.MailBatch.Id);
                if (data != null)
                {
                    try
                    {
                        StringBuilder deliveryReport = Common.USAADailyDetailOperationReport(data);
                        string deliveryReportFileName = "{0}_{1}.csv".Fill("DR", data.MailBatch.BatchName);
                        using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(deliveryReport.ToString())))
                        {
                            using (FileStream file = new FileStream(Path.Combine(mbi.AMResponsePath, deliveryReportFileName), FileMode.Create, FileAccess.Write))
                            {
                                ms.WriteTo(file);
                            }
                        }

                        StringBuilder errorReport = Common.USAADailyDetailOperationReport(data);
                        string errorReportFileName = "{0}_{1}.csv".Fill("ER", data.MailBatch.BatchName);
                        using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(errorReport.ToString())))
                        {
                            using (FileStream file = new FileStream(Path.Combine(mbi.AMResponsePath, errorReportFileName), FileMode.Create, FileAccess.Write))
                            {
                                ms.WriteTo(file);
                            }
                        }

                        commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportPrepped, Common.Now);
                    }
                    catch (Exception ex)
                    {
                        log.Error("Error occured while generating reports : {0}".Fill(ex.ToString()));
                    }
                }
            }
        }
    }
}
