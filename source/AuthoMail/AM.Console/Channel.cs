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

                    List<Deliverable> deliverables = commonSvc.GetDeliverablesByClientId(client.Id);

                    MailBatchInfo mbi = new MailBatchInfo(batch, client, Config.Common_AuthoMailRootFolder);

                    var data = mailBatchSvc.GetMailBatchDetailsByBatchId(mbi.MailBatch.Id);

                    SFTChannel sftChannelDetails = clientSvc.GetSFTChannelDetailsBySFTChannelId(batch.SFTChannelId);
                    if (sftChannelDetails != null)
                    {
                        try
                        {
                            SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(sftChannelDetails.Protocol), sftChannelDetails.Host, sftChannelDetails.UserName, sftChannelDetails.Password, 300000);
                            sftSite.Connect();
                            //TODO: Remove switch case after proper logic implementation of deliverables
                            UploadReportsToClients(mbi, sftSite, deliverables, data);

                            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportedBack, Common.Now);

                            if (sftSite.IsConnected)
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
            }
            log.Debug("ReportedBack job is completed.");
        }

        private static void UploadReportsToClients(MailBatchInfo mbi, SFTUtil sftSite, List<Deliverable> deliverables, BatchData mailBatchData)
        {

            switch (mbi.Client.AppCode)
            {
                case "USAA":
                    string uploadPath = deliverables.SingleOrDefault().UploadPath;
                    if (sftSite.IsDownloadDirectoryAvailable(uploadPath))
                    {
                        //TODO: Replace *.csv in place of *.txt if we don't move new naming convention and tab delimited reports to production environment
                        var reportList = Directory.EnumerateFiles(mbi.AMResponsePath, "*.txt").Where(f => Path.GetFileNameWithoutExtension(f).StartsWith("Delivery-") || Path.GetFileNameWithoutExtension(f).StartsWith("Error-")).ToList();

                        reportList.AddRange(Directory.EnumerateFiles(mbi.AMResponsePath, "*.err"));


                        if (reportList != null && reportList.Count > 0)
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
                                string ftpDeliveryGoUploadPath = Path.Combine(uploadPath, "DlvrReport.go");
                                File.WriteAllText(deliveryGoFilePath, string.Empty);

                                sftSite.UploadFile(deliveryGoFilePath, ftpDeliveryGoUploadPath);
                            }

                            if (reportList.Count(f => Path.GetFileNameWithoutExtension(f).StartsWith("Error-")) > 0)
                            {
                                string errorGoFilePath = Path.Combine(mbi.AMResponsePath, "ErrReport.go");
                                string ftpErrorGoUploadPath = Path.Combine(uploadPath, "ErrReport.go");
                                File.WriteAllText(errorGoFilePath, string.Empty);

                                sftSite.UploadFile(errorGoFilePath, ftpErrorGoUploadPath);
                            }

                            MailUtility.SendProductionNotificationEmail(mbi, mailBatchData, reportList, deliverables.SingleOrDefault().EmailChannel);
                        }
                    }
                    else
                    {
                        log.Error("Upload path does not exist for Client-{0} and SFT Channel Path-'{1}'".Fill(mbi.Client.Name, uploadPath));
                    }
                    break;
                case "ALASKA":

                    var reportToMail = new List<string>();
                    //TODO: handle multiple channel code implementation
                    var emailChannel = deliverables.SingleOrDefault(x => x.ReportType == ReportTypes.Delivery).EmailChannel;
                    foreach (var deliverable in deliverables)
                    {
                        if (deliverable.ReportType == ReportTypes.Delivery)
                        {
                            var deliveryReportName = "DR_{0}.csv".Fill(mbi.BatchName);
                            string deliveryReportPath = Path.Combine(mbi.AMResponsePath, deliveryReportName);
                            if (File.Exists(deliveryReportPath))
                            {
                                string ftpUploadPath = Path.Combine(deliverable.UploadPath, deliveryReportName);
                                sftSite.UploadFile(deliveryReportPath, ftpUploadPath);
                                reportToMail.Add(deliveryReportPath);
                            }
                        }
                        else if (deliverable.ReportType == ReportTypes.Error)
                        {
                            var errorReportName = "ER_{0}.csv".Fill(mbi.BatchName);
                            string errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportName);
                            if (File.Exists(errorReportPath))
                            {
                                string ftpUploadPath = Path.Combine(deliverable.UploadPath, errorReportName);
                                sftSite.UploadFile(errorReportPath, ftpUploadPath);
                                reportToMail.Add(errorReportPath);
                            }
                        }
                    }
                    MailUtility.SendProductionNotificationEmail(mbi, mailBatchData, reportToMail, emailChannel);
                    break;
                case "HSBC":

                    var reportToMailHSBC = new List<string>();
                    //TODO: handle multiple channel code implementation
                    var emailChannelHSBC = deliverables.SingleOrDefault(x => x.ReportType == ReportTypes.Delivery).EmailChannel;
                    foreach (var deliverable in deliverables)
                    {
                        if (deliverable.ReportType == ReportTypes.Delivery)
                        {
                            var deliveryReportName = "{0}.DeliveryReport_{1}.csv".Fill(mbi.BatchName, mbi.MailBatch.ReceivedOn.Value.UtcToClientTimeZone(mbi.Client.TimeZone).ToString("htt"));
                            string deliveryReportPath = Path.Combine(mbi.AMResponsePath, deliveryReportName);
                            if (File.Exists(deliveryReportPath))
                            {
                                string ftpUploadPath = Path.Combine(deliverable.UploadPath, deliveryReportName);
                                sftSite.UploadFile(deliveryReportPath, ftpUploadPath);
                                reportToMailHSBC.Add(deliveryReportPath);
                            }
                        }
                        else if (deliverable.ReportType == ReportTypes.Error)
                        {
                            var errorReportName = "{0}.ErrorReport_{1}.csv".Fill(mbi.BatchName, mbi.MailBatch.ReceivedOn.Value.UtcToClientTimeZone(mbi.Client.TimeZone).ToString("htt"));
                            string errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportName);
                            if (File.Exists(errorReportPath))
                            {
                                string ftpUploadPath = Path.Combine(deliverable.UploadPath, errorReportName);
                                sftSite.UploadFile(errorReportPath, ftpUploadPath);
                                reportToMailHSBC.Add(errorReportPath);
                            }
                        }
                    }
                    MailUtility.SendProductionNotificationEmail(mbi, mailBatchData, reportToMailHSBC, emailChannelHSBC);
                    break;
                default:
                    throw new Exception("Invalid App Code");

            }
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
