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

        protected IClientService clientSvc;
        protected IMailBatchService mailBatchSvc;
        protected ICommonService commonSvc;
        protected IMailPieceService mailPieceSvc;
        protected IDocTypeService docTypeSvc;

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

                    var data = mailBatchSvc.GetMailBatchDetailsByBatchId(mbi.MailBatch.Id);

                    var reportList = Directory.EnumerateFiles(mbi.AMResponsePath, "*.csv").Where(f => Path.GetFileNameWithoutExtension(f).StartsWith("Delivery-") || Path.GetFileNameWithoutExtension(f).StartsWith("Error-") || Path.GetExtension(f).Equals(".err", StringComparison.CurrentCultureIgnoreCase)).ToList();


                    if (reportList != null && reportList.Count > 0)
                    {
                        SFTChannel sftChannelDetails = clientSvc.GetSFTChannelDetailsBySFTChannelId(batch.SFTChannelId);
                        if (sftChannelDetails != null)
                        {
                            try
                            {
                                SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(sftChannelDetails.Protocol), sftChannelDetails.Host, sftChannelDetails.UserName, sftChannelDetails.Password, 300000);
                                sftSite.Connect();

                                if (sftSite.IsDownloadDirectoryAvailable(uploadPath))
                                {
                                    foreach (string filePath in reportList)
                                    {
                                        string fileName = Path.GetFileName(filePath);
                                        string ftpUploadPath = Path.Combine(uploadPath, fileName);
                                        sftSite.UploadFile(filePath, ftpUploadPath);
                                    }

                                    if (reportList.Count(f => Path.GetFileNameWithoutExtension(f).StartsWith("Delivery-")) > 0)
                                    {
                                        string deliveryGoFilePath = Path.Combine(mbi.AMResponsePath, "DlvrReport.go");
                                        string ftpDeliveryGoUploadPath = Path.Combine(Config.Monitor_ReportSFTUploadingPathAMC, "DlvrReport.go");
                                        File.WriteAllText(deliveryGoFilePath, string.Empty);

                                        sftSite.UploadFile(deliveryGoFilePath, ftpDeliveryGoUploadPath);
                                    }

                                    if (reportList.Count(f => Path.GetFileNameWithoutExtension(f).StartsWith("Error-")) > 0)
                                    {
                                        string errorGoFilePath = Path.Combine(mbi.AMResponsePath, "ErrReport.go");
                                        string ftpErrorGoUploadPath = Path.Combine(Config.Monitor_ReportSFTUploadingPathAMC, "ErrReport.go");
                                        File.WriteAllText(errorGoFilePath, string.Empty);

                                        sftSite.UploadFile(errorGoFilePath, ftpErrorGoUploadPath);
                                    }

                                    MailUtility.SendProductionNotificationEmail(mbi, data, reportList);


                                    commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportedBack, Common.Now);
                                }
                                else
                                {
                                    log.Error("Upload path does not exist for Client-{0} and SFT Channel Id-{1}".Fill(client.Name, sftChannelDetails.Code));
                                }

                                sftSite.Disconnect();
                            }
                            catch (Exception ex)
                            {
                                log.Error(ex);
                            }
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
