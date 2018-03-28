using AM.Entities;
using AM.Lib;
using AM.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Console
{
    abstract class Channel : Disposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public SFTChannel SFTChannel { get; private set; }

        protected ClientService clientSvc;
        protected MailBatchService mailBatchSvc;
        protected CommonService commonSvc;
        protected MailPieceService mailPieceSvc;
        protected DocTypeService docTypeSvc;

        protected List<ServiceBase> services;

        protected Channel(string channelCode)
        {
            services = new List<ServiceBase>();

            // Add services here
            clientSvc = GetService<ClientService>();
            mailBatchSvc = GetService<MailBatchService>();
            commonSvc = GetService<CommonService>();
            mailPieceSvc = GetService<MailPieceService>();
            docTypeSvc = GetService<DocTypeService>();

            // Get Channel by code
            SFTChannel = clientSvc.GetSFTChannelDetailsBySFTChannelCode(channelCode);

        }

        private T GetService<T>() where T : ServiceBase, new()
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
                this.SeparateBatches();
                this.ValidateBatches();
                this.AcknowledgeBatches();
            }
            catch (Exception ex)
            {
                // Log exception
                log.Error("Unexpected error.", ex);

                // Send email to support team
                MailUtility.SendUnexpectedExcelptionEmail(ex.ToString());
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
        protected abstract void SeparateBatches();
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
            log.Debug("ReportedBack job is started.");
            using (IMailBatchService mailBatchSvc = new MailBatchService())
            using (IClientService clientSvc = new ClientService())
            using (ICommonService commonSvc = new CommonService())
            {
                List<MailBatchDTO> reportPreppedBatches = mailBatchSvc.GetBatchesByPipelineStatus(BatchPipelineStatus.ReportPrepped);
                if (reportPreppedBatches == null)
                {
                    log.Debug("Uploading report job queue is empty.");
                    return;
                }

                foreach (var batch in reportPreppedBatches)
                {
                    Client client = clientSvc.GetClientById(batch.ClientId);

                    string uploadPath = Config.Monitor_ReportSFTUploadingPathAMC;

                    MailBatchInfo mbi = new MailBatchInfo(batch, client, Config.Common_AuthoMailRootFolder);

                    var reportList = Directory.EnumerateFiles(mbi.AMResponsePath, "*.csv").Where(f => Path.GetFileNameWithoutExtension(f).StartsWith("Delivery-") || Path.GetFileNameWithoutExtension(f).StartsWith("Error-")).ToList();

                    try
                    {
                        if (reportList != null && reportList.Count > 0)
                        {
                            SFTUtil sftSite = null;

                            SFTChannel sftChannelDetails = clientSvc.GetSFTChannelDetailsBySFTChannelId(batch.SFTChannelId);
                            if (sftChannelDetails != null)
                            {
                                try
                                {
                                    sftSite = new SFTUtil(cfg.GetProtocolType(sftChannelDetails.Protocol), sftChannelDetails.Host, sftChannelDetails.UserName, sftChannelDetails.Password, 300000);
                                    sftSite.Connect();

                                    if (sftSite.IsDownloadDirectoryAvailable(uploadPath))
                                    {
                                        foreach (string filePath in reportList)
                                        {
                                            string fileName = Path.GetFileName(filePath);
                                            string ftpUploadPath = Path.Combine(uploadPath, fileName);
                                            sftSite.UploadFile(filePath, ftpUploadPath);
                                        }
                                        commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportedBack, Common.Now);
                                    }
                                    else
                                    {
                                        log.Error("Upload path does not exist for Client-{0} and SFT Channel Id-{1}".Fill(client.Name, sftChannelDetails.Code));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    log.Error(ex);
                                }
                                sftSite.Disconnect();
                            }
                            else
                            {
                                log.Error("SFT site is not available or not enabled to upload reports.");
                            }
                        }
                        else
                        {
                            log.Error("Reports does not exist to upload for BatchId-{0}".Fill(batch.Id));
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("Error occured while uploading report to Client's SFT site for Batch : {0}. Please see error :" + Environment.NewLine + ex);
                    }
                }
            }
            log.Debug("ReportedBack job is completed.");
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
