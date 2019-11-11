using AM.Entities;
using AM.Lib;
using AM.Services;
using AM.USPSLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
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
                //this.CalculateDueDateAndPrintLocation();
                this.AcknowledgeBatches();
            }
            catch (Exception ex)
            {
                // Log exception
                log.Error("Unexpected error.", ex);

                // Send email to support team
                MailUtility.SendUnexpectedExceptionEmail(ex.ToString());
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
        ///// <summary>
        ///// Calculate effective receivedon date, printlocation and due date all valid batches which are queued as ValidateBatches
        ///// </summary>
        //protected abstract void CalculateDueDateAndPrintLocation();
        /// <summary>
        /// Acknowledge all batches which are queued as Validated
        /// </summary>
        protected abstract void AcknowledgeBatches();

        #region Reported Back logic
        /// <summary>
        /// Sends batch level reports of all batches which are queued as ReportPrepped
        /// </summary>
        public static void ReportBack(string[] channelCodes, string reportType)
        {
            log.Debug("ReportedBack job is started.");
            //TODO: Comma separated channels for Increamental reports
            switch (reportType)
            {
                case "Incremental":
                    foreach (var channelCode in channelCodes)
                    {
                        ProcessIncreamentalReportedBack(channelCode);
                        ProcessGeneralReportedBack(channelCode);
                    }
                    break;
                default:
                    foreach (var channelCode in channelCodes)
                    {
                        ProcessGeneralReportedBack(channelCode);
                    }
                    break;
            }

            log.Debug("ReportedBack job is completed.");
        }

        private static void ProcessGeneralReportedBack(string channelCode)
        {
            using (IMailBatchService mailBatchSvc = new MailBatchService())
            using (IClientService clientSvc = new ClientService())
            using (ICommonService commonSvc = new CommonService())
            {
                SFTChannel sftChannelDetails = clientSvc.GetSFTChannelDetailsBySFTChannelCode(channelCode + "_" + Config.Common_Environment);

                List<MailBatchDTO> reportPreppedBatches = mailBatchSvc.GetBatchesByPipelineStatusAndClientId(BatchPipelineStatus.ReportPrepped).Where(ch => ch.SFTChannelId == sftChannelDetails.Id).ToList();

                if (reportPreppedBatches == null || reportPreppedBatches.Count == 0)
                {
                    log.Debug("Uploading report job queue is empty.");
                    return;
                }

                foreach (var mailBatch in reportPreppedBatches)
                {
                    try
                    {
                        Client client = clientSvc.GetClientById(mailBatch.ClientId);

                        List<Deliverable> deliverables = commonSvc.GetDeliverablesByClientId(client.Id);

                        MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, Config.Common_AuthoMailRootFolder);

                        List<MailPieces> mailPieces;
                        using (IReportService reportSvc = new ReportService())
                        {
                            mailPieces = reportSvc.GetMailPiecesDataFromMailBatchId(mbi.MailBatch.Id);
                        }

                        if (sftChannelDetails != null)
                        {
                            try
                            {
                                SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(sftChannelDetails.Protocol), sftChannelDetails.Host, sftChannelDetails.UserName, sftChannelDetails.Password, 300000);
                                sftSite.Connect();

                                //TODO: Remove switch case after proper logic implementation of deliverables
                                bool isUploaded = UploadReportsToClients(mbi, sftSite, deliverables, mailPieces, sftChannelDetails);

                                if (isUploaded)
                                    commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportedBack, Common.Now);

                                if (sftSite.IsConnected)
                                    sftSite.Disconnect();
                            }
                            catch (Exception ex)
                            {
                                var msg = "Error occured while report uploading for MailBatchId - {0} with Error : {1}".Fill(mailBatch.Id, ex.Message);
                                log.Error(msg, ex);
                                MailUtility.SendUnexpectedExceptionEmail(msg);
                            }
                        }
                        else
                        {
                            var msg = "SFT site is not available or not enabled to upload reports.";
                            log.Error(msg);
                            MailUtility.SendUnexpectedExceptionEmail(msg);
                        }
                    }
                    catch (Exception ex)
                    {
                        var msg = "Error occured in ReportedBack Method For Mail Batch Id: {0} with Error : {1}".Fill(mailBatch.Id, ex.Message);
                        log.Error(msg, ex);
                        MailUtility.SendUnexpectedExceptionEmail(msg);
                        continue;
                    }
                }
            }
        }

        private static void ProcessIncreamentalReportedBack(string channelCode)
        {
            using (IMailBatchService mailBatchSvc = new MailBatchService())
            using (IMailPieceService mailPieceSvc = new MailPieceService())
            using (IClientService clientSvc = new ClientService())
            using (ICommonService commonSvc = new CommonService())
            using (IReportService reportSvc = new ReportService())
            {
                SFTChannel sftChannelDetails = clientSvc.GetSFTChannelDetailsBySFTChannelCode(channelCode + "_" + Config.Common_Environment);
                var reportPreppedBatches = mailBatchSvc.GetReportPreppedPrintBatchDetails().Where(c => c.SFTChannelId == sftChannelDetails.Id).ToList();
                SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(sftChannelDetails.Protocol), sftChannelDetails.Host, sftChannelDetails.UserName, sftChannelDetails.Password, 300000);
                sftSite.Connect();
                foreach (var preppedPrintBatch in reportPreppedBatches)
                {
                    try
                    {
                        Client client = clientSvc.GetClientByAppCode(preppedPrintBatch.AppCode);

                        List<Deliverable> deliverables = commonSvc.GetDeliverablesByClientId(client.Id);

                        string localReportPath = Path.Combine(Config.Common_AuthoMailRootFolder, client.AppCode, Util.AMResponseFolderName, preppedPrintBatch.PrintBatchId.ToString());
                        //Here we are passing localReportPath as PrintBatches folder structure AMRoot/AMResponses/PrintBatchId
                        UploadReportsToIncrementalClients(deliverables, localReportPath, sftSite, sftChannelDetails);

                        mailBatchSvc.UpdatePrintBatchPipelineStatus(preppedPrintBatch.PrintBatchId, BatchPipelineStatus.ReportedBack, Common.Now);
                    }
                    catch (Exception ex)
                    {
                        var msg = "Error occured while increamental report uploading for PrintBatchId - {0} with Error : {1}".Fill(preppedPrintBatch.PrintBatchId, ex.Message);
                        log.Error(msg, ex);
                        MailUtility.SendUnexpectedExceptionEmail(msg);
                    }
                }

                if (sftSite.IsConnected)
                    sftSite.Disconnect();
            }
        }

        private static void GenerateAndSendProductionSummaryNotification(MailBatchInfo mbi, List<MailPieces> mailPieces, List<Deliverable> deliverables, SFTChannel sftChannelDetail)
        {
            string errorReportPath = string.Empty;
            string errorReportFileName = string.Empty;
            StringBuilder errorReportData = new StringBuilder();
            string deliveryReportPath = string.Empty;
            string deliveryReportFileName = string.Empty;
            StringBuilder deliveryReportData = new StringBuilder();
            var reportToMail = new List<string>();
            var deliveryReportDeliverable = deliverables.SingleOrDefault(rt => rt.ReportType == ReportTypes.Delivery);
            var errorReportDeliverable = deliverables.SingleOrDefault(rt => rt.ReportType == ReportTypes.Error);

            //NOTE : If Email channel is not exist for both delivery report and Error report then no need to create reports and send an email.
            if (deliveryReportDeliverable.EmailChannel == null && errorReportDeliverable.EmailChannel == null)
                return;

            switch (sftChannelDetail.Code.Split('_')[0])
            {
                case "HSBC":
                    using (IHSBCService hsbcSvc = new HSBCService())
                    {

                        StringBuilder deliveryReport = hsbcSvc.HSBCDeliveryReport(mailPieces, mbi.Client.TimeZone, deliveryReportDeliverable.Delimeter);

                        deliveryReportFileName = "{0}.DeliveryReport_{1}.csv".Fill(mbi.BatchName.Split('[')[0], mbi.MailBatch.ReceivedOn.Value.UtcToClientTimeZone(mbi.Client.TimeZone).ToString("htt"));
                        deliveryReportPath = Path.Combine(mbi.AMResponsePath, deliveryReportFileName);

                        File.WriteAllText(deliveryReportPath, deliveryReport.ToString());
                        reportToMail.Add(deliveryReportPath);

                        StringBuilder errorReport = hsbcSvc.HSBCErrorReport(mailPieces, mbi.Client.TimeZone, errorReportDeliverable.Delimeter);

                        errorReportFileName = "{0}.ErrorReport_{1}.csv".Fill(mbi.BatchName.Split('[')[0], mbi.MailBatch.ReceivedOn.Value.UtcToClientTimeZone(mbi.Client.TimeZone).ToString("htt"));
                        errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);

                        File.WriteAllText(errorReportPath, errorReport.ToString());
                        reportToMail.Add(errorReportPath);
                    }
                    break;
                case "AMC":
                    using (IAMCService amcSvc = new AMCService())
                    {
                        StringBuilder deliveryReport = amcSvc.AMCDeliveryReport(mailPieces);

                        if (deliveryReport != null)
                        {
                            deliveryReportFileName = "Delivery-{0}.txt".Fill(Common.Now.ToString("yyyyMMdd-HHmmss"));

                            //TODO: Comment below and uncomment above if don't want to move new naming convention and tab delimited report to Production environment
                            //string deliveryReportFileName = "Delivery-{0}-{1}-{2}-{3}.txt".Fill(mbi.BatchName, mbi.Client.AppCode, mbi.MailBatch.Id, Common.Now.ToString("yyyyMMdd-HHmmss"));
                            deliveryReportPath = Path.Combine(mbi.AMResponsePath, deliveryReportFileName);

                            File.WriteAllText(deliveryReportPath, deliveryReport.ToString());
                            reportToMail.Add(deliveryReportPath);
                        }

                        StringBuilder errorReport = amcSvc.AMCErrorReport(mailPieces);

                        if (errorReport != null)
                        {
                            errorReportFileName = "Error-{0}.txt".Fill(Common.Now.ToString("yyyyMMdd-HHmmss"));

                            //TODO: Comment below and uncomment above if don't want to move new naming convention and tab delimited report to Production environment
                            //string errorReportFileName = "Error-{0}-{1}-{2}-{3}.txt".Fill(mbi.BatchName, mbi.Client.AppCode, mbi.MailBatch.Id, Common.Now.ToString("yyyyMMdd-HHmmss"));
                            errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);

                            File.WriteAllText(errorReportPath, errorReport.ToString());
                            reportToMail.Add(errorReportPath);
                        }
                    }
                    break;
                default:
                    using (IGeneralService generalSvc = new GeneralService())
                    using (ICommonService commonSvc = new CommonService())
                    {
                        string deliveryReport = generalSvc.GeneralXMLReport(mailPieces, mbi.MailBatch, mbi.Client.AppCode);
                        if (!string.IsNullOrWhiteSpace(deliveryReport))
                        {
                            deliveryReportFileName = "{0}-{1}-{2}.xml".Fill(mbi.Client.AppCode, mbi.MailBatch.Id, commonSvc.GetReportingSequence());
                            deliveryReportPath = Path.Combine(mbi.AMResponsePath, deliveryReportFileName);
                            File.WriteAllText(deliveryReportPath, deliveryReport.ToString());
                            reportToMail.Add(deliveryReportPath);
                        }
                    }
                    break;

            }

            //TODO: Remove after proper naming convention
            Thread.Sleep(1000);

            MailUtility.SendProductionNotificationEmail(mbi, mailPieces, reportToMail, deliveryReportDeliverable.EmailChannel);
            //After Sending An Email Change its extention to "EmailAttachment" to identify that report is sent as an attachments
            foreach (var reportPath in reportToMail)
            {
                File.Move(reportPath, Path.ChangeExtension(reportPath, ".Mailed"));
            }
        }

        private static void UploadReportsToIncrementalClients(List<Deliverable> deliverables, string localReportPath, SFTUtil sftSite, SFTChannel sftChannelDetails)
        {
            string deliveryUploadPath = deliverables.SingleOrDefault(up => up.ReportType == ReportTypes.Delivery).UploadPath;
            string errorUploadPath = deliverables.SingleOrDefault(up => up.ReportType == ReportTypes.Error).UploadPath;
            string ftpUploadPath = "";
            string sftChannelCode = sftChannelDetails.Code.Split('_')[0];
            switch (sftChannelCode)
            {
                case "AMC":

                    var deliveryReportList = Directory.EnumerateFiles(localReportPath, "*.txt").Where(f => Path.GetFileNameWithoutExtension(f).StartsWith("Delivery-")).ToList();

                    foreach (string filePath in deliveryReportList)
                    {
                        string fileName = Path.GetFileName(filePath);
                        ftpUploadPath = Path.Combine(deliveryUploadPath, fileName);
                        sftSite.UploadFile(filePath, ftpUploadPath);


                        //After upload change extention as a Complete
                        File.Move(filePath, Path.ChangeExtension(filePath, ".Complete"));
                    }

                    if (deliveryReportList.Count(f => Path.GetFileNameWithoutExtension(f).StartsWith("Delivery-")) > 0)
                    {
                        string deliveryGoFilePath = Path.Combine(localReportPath, "DlvrReport.go");
                        string ftpDeliveryGoUploadPath = Path.Combine(deliveryUploadPath, "DlvrReport.go");
                        File.WriteAllText(deliveryGoFilePath, string.Empty);

                        sftSite.UploadFile(deliveryGoFilePath, ftpDeliveryGoUploadPath);
                    }

                    var errorReportList = Directory.EnumerateFiles(localReportPath, "*.txt").Where(f => Path.GetFileNameWithoutExtension(f).StartsWith("Error-")).ToList();

                    foreach (string filePath in errorReportList)
                    {
                        string fileName = Path.GetFileName(filePath);
                        ftpUploadPath = Path.Combine(errorUploadPath, fileName);
                        sftSite.UploadFile(filePath, ftpUploadPath);


                        //After upload change extention as a Complete
                        File.Move(filePath, Path.ChangeExtension(filePath, ".Complete"));
                    }

                    if (errorReportList.Count(f => Path.GetFileNameWithoutExtension(f).StartsWith("Error-")) > 0)
                    {
                        string errorGoFilePath = Path.Combine(localReportPath, "ErrReport.go");
                        string ftpErrorGoUploadPath = Path.Combine(errorUploadPath, "ErrReport.go");
                        File.WriteAllText(errorGoFilePath, string.Empty);

                        sftSite.UploadFile(errorGoFilePath, ftpErrorGoUploadPath);
                    }
                    break;
                case "PENFED":

                    //NOTE : Below zip file naming convention is EDG Naming convention.
                    string zipFileName = "{0}FAGOS.ZYONT.N0010362.PenFedPNSOUTPUT.D{1}.T{2}.zip".Fill(Config.Common_EnvironmentInitial, Common.Now.ToString("yyyyMMdd"), Common.Now.ToString("hhmmss"));
                    string zipFilePath = Path.Combine(localReportPath, zipFileName);
                    ftpUploadPath = Path.Combine(deliveryUploadPath, zipFileName);
                    var resXmlList = Directory.EnumerateFiles(localReportPath, "*.xml").ToList();
                    using (ZipArchive newFile = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                    {
                        foreach (string filePath in resXmlList)
                        {
                            string fileName = Path.GetFileName(filePath);
                            newFile.CreateEntryFromFile(filePath, fileName);
                            //After upload change extention as a Complete
                            File.Move(filePath, Path.ChangeExtension(filePath, ".Complete"));
                        }
                    }
                    sftSite.UploadFile(zipFilePath, ftpUploadPath);
                    File.Move(zipFilePath, Path.ChangeExtension(zipFilePath, ".ZipUploaded"));
                    break;
                default:
                    resXmlList = Directory.EnumerateFiles(localReportPath, "*.xml").ToList();

                    foreach (string filePath in resXmlList)
                    {
                        string fileName = Path.GetFileName(filePath);
                        ftpUploadPath = Path.Combine(deliveryUploadPath, fileName);
                        sftSite.UploadFile(filePath, ftpUploadPath);


                        //After upload change extention as a Complete
                        File.Move(filePath, Path.ChangeExtension(filePath, ".Complete"));
                    }
                    break;
            }
        }

        private static bool UploadReportsToClients(MailBatchInfo mbi, SFTUtil sftSite, List<Deliverable> deliverables, List<MailPieces> mailPieceData, SFTChannel sftChannelDetail)
        {
            bool isUploaded = false;

            var increamentalReportingChannels = Config.Core_IncrementalReportingChannelCodes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            increamentalReportingChannels = increamentalReportingChannels.Select(c => c + "_" + Config.Common_Environment).ToList();

            switch (sftChannelDetail.Code.Split('_')[0].ToUpper())
            {
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

                    MailUtility.SendProductionNotificationEmail(mbi, mailPieceData, reportToMail, emailChannel);
                    isUploaded = true;
                    break;
                case "HSBC":
                    var reportToMailHSBC = new List<string>();
                    //TODO: handle multiple channel code implementation
                    var emailChannelHSBC = deliverables.SingleOrDefault(x => x.ReportType == ReportTypes.Delivery).EmailChannel;
                    foreach (var deliverable in deliverables)
                    {
                        if (deliverable.ReportType == ReportTypes.Delivery)
                        {
                            var deliveryReportName = "{0}.DeliveryReport_{1}.csv".Fill(mbi.BatchName.Split('[')[0], mbi.MailBatch.ReceivedOn.Value.UtcToClientTimeZone(mbi.Client.TimeZone).ToString("htt"));
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
                            var errorReportName = "{0}.ErrorReport_{1}.csv".Fill(mbi.BatchName.Split('[')[0], mbi.MailBatch.ReceivedOn.Value.UtcToClientTimeZone(mbi.Client.TimeZone).ToString("htt"));
                            string errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportName);
                            if (File.Exists(errorReportPath))
                            {
                                string ftpUploadPath = Path.Combine(deliverable.UploadPath, errorReportName);
                                sftSite.UploadFile(errorReportPath, ftpUploadPath);
                                reportToMailHSBC.Add(errorReportPath);
                            }
                        }
                    }

                    MailUtility.SendProductionNotificationEmail(mbi, mailPieceData, reportToMailHSBC, emailChannelHSBC);
                    isUploaded = true;
                    break;
                case "AMC":
                default:
                    var mailPieceCount = mailPieceData.Count(status => status.ShippingStatus == ShippingStatus.InProgress.ToString());

                    if (mailPieceCount > 0)
                        return isUploaded;

                    //NOTE : Here we have to check printbatches table for that MailBatchId because we can not updated reported back status until all print batches status are getting changed to ReportedBack.
                    int remainingPrintBatchReportedBackCount = 0;
                    if (increamentalReportingChannels.Contains(sftChannelDetail.Code))
                    {
                        using (IMailBatchService mailBatchSvc = new MailBatchService())
                        {
                            remainingPrintBatchReportedBackCount = mailBatchSvc.RemainingPrintBatchesReportedBackCount(mbi.MailBatch.Id);
                        }

                        if (remainingPrintBatchReportedBackCount > 0)
                            return isUploaded;
                    }

                    string reportPath = Path.Combine(Config.Common_AuthoMailRootFolder, mbi.Client.AppCode, mbi.MailBatch.Id.ToString(), Util.AMResponseFolderName);

                    //Here we are passing localReportPath as MailBatch folder structure AMRoot/USAA/MailBatchId/AMResponses
                    UploadReportsToIncrementalClients(deliverables, reportPath, sftSite, sftChannelDetail);

                    GenerateAndSendProductionSummaryNotification(mbi, mailPieceData, deliverables, sftChannelDetail);
                    isUploaded = true;
                    break;

            }
            return isUploaded;
        }
        #endregion

        #region EffectiveReceivedOn DueDate PrintLocation Logic
        public static void CalculateDueDateAndPrintLocation(MailBatchDTO mailBatch, List<MailPieceDTO> mailPieces)
        {
            log.Debug("Calculation of Due Date and Print location job is started.");
            using (IMailBatchService mailBatchSvc = new MailBatchService())
            using (IMailPieceService mailPieceSvc = new MailPieceService())
            using (IDocTypeService docTypeSvc = new DocTypeService())
            using (ICommonService commonSvc = new CommonService())
            using (IClientService clientSvc = new ClientService())
            {
                var sweepTimings = commonSvc.GetSweepTimings();

                Client client = clientSvc.GetClientById(mailBatch.ClientId);

                foreach (MailPieceDTO mailPiece in mailPieces)
                {
                    if (mailPiece.ShippingStatus == ShippingStatus.Error.ToString())
                    {
                        if (!mailPiece.DocTypeId.HasValue)
                        {
                            //NOTE : If System didn't find any doctypeid for particular mail piece then it will take default max cut-off time of that particular client for calculating effective received on date.
                            mailPiece.EffectiveReceivedOnEst = commonSvc.GetEffectiveEstReceivedOn(docTypeSvc.GetDocTypeWithMaximumCutOffTimeByClientId(client.Id), mailBatch.ReceivedOn.Value);
                        }
                        else
                        {
                            mailPiece.EffectiveReceivedOnEst = commonSvc.GetEffectiveEstReceivedOn(docTypeSvc.GetDocTypeById(mailPiece.DocTypeId.Value), mailBatch.ReceivedOn.Value);
                        }
                    }
                    else if (mailPiece.IsEDeliveryRequested && !mailPiece.IsFallBackRequested)
                    {
                        mailPiece.EffectiveReceivedOnEst = commonSvc.GetEffectiveEstReceivedOn(docTypeSvc.GetDocTypeById(mailPiece.DocTypeId.Value), mailBatch.ReceivedOn.Value);
                        mailPiece.StdSubmissionTime = mailPiece.EffectiveReceivedOnEst.Value.AddMinutes(30).EstToUtc();
                        mailPiece.MaxSubmissionTime = mailPiece.EffectiveReceivedOnEst.Value.AddHours(4).EstToUtc();
                    }
                    else
                    {
                        DocType docType = docTypeSvc.GetDocTypeById(mailPiece.DocTypeId.Value);

                        //Here first we will calculate effective received on date and based on that we will calculate due date,Print location, print trail id,etc. for that
                        //Here storing effective receivedon in EST format
                        mailPiece.EffectiveReceivedOnEst = commonSvc.GetEffectiveEstReceivedOn(docType, mailBatch.ReceivedOn.Value);
                        //Calculate Due Date for Mail Pieces doc type wise
                        mailPiece.DueDate = commonSvc.GetDueDate(docType, mailPiece.EffectiveReceivedOnEst.Value);
                        //Set print location where mail piece will go for print and ship
                        mailPiece.PrintTrailId = commonSvc.GetPrintTrail(docType, mailPiece.EffectiveReceivedOnEst.Value);
                        var printVendorDetails = commonSvc.GetPrintVendorDetailsByPrintTrailId(mailPiece.PrintTrailId.Value, mailPiece.EffectiveReceivedOnEst);
                        mailPiece.PrintLocation = printVendorDetails.PrintLocation;
                        //This field will be used for monitoring data    
                        mailPiece.StdSubmissionTime = commonSvc.GetStandardSubmissionTime(client.AppCode, printVendorDetails.VendorCode, mailBatch.ReceivedOn.Value, sweepTimings);
                        mailPiece.MaxSubmissionTime = commonSvc.GetMaximumSubmissionTime(docType, mailPiece.EffectiveReceivedOnEst.Value);
                    }
                    mailPieceSvc.UpdateBatchMailPieceDetails(mailPiece);
                }
            }
            log.Debug("Calculation of Due Date and Print location Job is ended.");
        }

        #endregion

        #region AddressType Validation
        public static void ValidateAddressType(MailBatchDTO mailBatch, List<MailPieceDTO> validMailPieces)
        {
            log.Debug("Address type validation job is started.");
            using (IMailBatchService mailBatchSvc = new MailBatchService())
            using (IMailPieceService mailPieceSvc = new MailPieceService())
            using (IDocTypeService docTypeSvc = new DocTypeService())
            using (ICommonService commonSvc = new CommonService())
            using (IClientService clientSvc = new ClientService())
            {
                try
                {
                    #region Address Validation Logic
                    //Get validate Addresses for each records
                    List<AddressResponse> addressResponses = new List<AddressResponse>();
                    List<AddressRequest> addressRequestes = new List<AddressRequest>();
                    foreach (var mailPiece in validMailPieces.Where(mp => (mp.IsEDeliveryRequested && mp.IsFallBackRequested) || !mp.IsEDeliveryRequested)) // Here system will verify only those address which have fallbackrequested=true or Edeliveryrequested=false
                    {
                        AddressRequest addressRequest = new AddressRequest() { Id = mailPiece.UniqueId.ToString(), StreetAddress = mailPiece.RcptAddressStreet + " " + mailPiece.RcptAddrOptionalLine1BelowStreet, City = mailPiece.RcptAddressCity, State = mailPiece.RcptAddressState, Zip4 = mailPiece.RcptAddressZIP.SeparateZip4And5(4), Zip5 = mailPiece.RcptAddressZIP.SeparateZip4And5(5) };

                        addressRequestes.Add(addressRequest);
                    }

                    int requestAddressCount = addressRequestes.Count;
                    int maxAddressVerification = 5;
                    int verifiedAddressCount = 0;
                    double loopCount = Math.Ceiling(requestAddressCount / (float)maxAddressVerification);

                    for (double i = 0; i < loopCount; i++)
                    {
                        var takenAddressesList = addressRequestes.Skip(verifiedAddressCount).Take(maxAddressVerification).ToList();
                        VerifierAddressRequest verifierAddRequest = new VerifierAddressRequest() { AddressRequests = takenAddressesList };
                        verifiedAddressCount += maxAddressVerification;
                        VerifierAddressResponse verifierAddResponse;

                        using (IUSPSService uspsSvc = new USPSService())
                        {
                            verifierAddResponse = uspsSvc.VerifyAddressAsync(commonSvc.GetSystemUserId(), verifierAddRequest).Result;
                        }

                        if (verifierAddResponse != null)
                        {
                            //if any error at VerifierAddressResponse level then throw unexpected error and exit from loop
                            if (verifierAddResponse.HasError)
                            {
                                throw new Exception("Error occured while validating Addresses : {0}".Fill(verifierAddResponse.Error));
                            }

                            addressResponses.AddRange(verifierAddResponse.AddressResponses);
                        }
                    }
                    #endregion
                    foreach (MailPieceDTO mailPiece in validMailPieces.Where(mp => (mp.IsEDeliveryRequested && mp.IsFallBackRequested) || !mp.IsEDeliveryRequested))
                    {
                        string documentOrPackageType = mailPiece.DocumentOrPackageType;

                        //Document Or Package Type validation
                        var docType = docTypeSvc.GetDocTypeById(mailPiece.DocTypeId.Value);

                        var addressResponse = addressResponses.Single(c => c.Id.ToLower() == mailPiece.UniqueId.ToString().ToLower());

                        mailPiece.RcptAddressTypes = string.Join(",", addressResponse.AddressTypes);

                        if (addressResponse.AddressTypes.Any(addr => addr == AddressType.INVALID))
                        {
                            mailPiece.ShippingStatus = ShippingStatus.Error.ToString();
                            mailPiece.ErrorCode = OutComeCodes.Mailing_Address_Missing_Or_Invalid;
                            mailPiece.ErrorDesc = !string.IsNullOrEmpty(addressResponse.VerifierDesc) ? addressResponse.VerifierDesc : addressResponse.Error.Description;
                            mailPieceSvc.UpdateBatchMailPieceDetails(mailPiece);
                            continue;
                        }

                        if (docType.ShippingCarrier == Carrier.FedEx.ToString())
                        {
                            if (!(addressResponse.AddressTypes.All(c => c == AddressType.DHAM)))
                            {
                                var fallBackDocTypes = docTypeSvc.GetFallBackDocTypeByClientDocTypeId(mailPiece.ClientId, docType.ClientDocTypeId);

                                if (fallBackDocTypes == null || fallBackDocTypes.Count == 0)
                                {
                                    mailPieceSvc.UpdateBatchMailPieceDetails(mailPiece);
                                    continue;
                                }

                                var priExDocType = fallBackDocTypes.SingleOrDefault(dt => dt.ShippingCarrier == Carrier.USPS.ToString() && dt.ShippingServiceType == AM.USPSLib.ServiceType.PRIORITY_EXPRESS.ToString());

                                if (priExDocType != null)
                                {
                                    docType = priExDocType;
                                }
                                else
                                {
                                    var priDocType = fallBackDocTypes.SingleOrDefault(dt => dt.ShippingCarrier == Carrier.USPS.ToString() && dt.ShippingServiceType == AM.USPSLib.ServiceType.PRIORITY.ToString());

                                    if (priDocType == null)
                                    {
                                        mailPieceSvc.UpdateBatchMailPieceDetails(mailPiece);
                                        continue;
                                    }

                                    docType = priDocType;
                                }

                                mailPiece.DocTypeId = docType.Id;

                                if (!mailPiece.IsReturnInfoSpecified && !mailBatch.IsReturnInfoSpecified && docType.IsReturnInfoSpecified)
                                {
                                    mailPiece.RetRcptFirstName = docType.RetRcptFirstName;
                                    mailPiece.RetRcptLastName = docType.RetRcptLastName;
                                    mailPiece.RetRcptMiddleName = docType.RetRcptMiddleName;
                                    mailPiece.RetRcptOrganization = docType.RetRcptOrganization;
                                    mailPiece.RetRcptPrefix = docType.RetRcptPrefix;
                                    mailPiece.RetRcptSuffix = docType.RetRcptSuffix;
                                    mailPiece.RetRcptPhone = docType.RetRcptPhone;

                                    mailPiece.RetRcptAddrOptionalLine1AboveStreet = docType.RetRcptAddrOptionalLine1AboveStreet;
                                    mailPiece.RetRcptAddrOptionalLine2AboveStreet = docType.RetRcptAddrOptionalLine2AboveStreet;
                                    mailPiece.RetRcptAddressStreet = docType.RetRcptAddressStreet;
                                    mailPiece.RetRcptAddressAptOrSuiteOrOther = docType.RetRcptAddressAptOrSuiteOrOther;
                                    mailPiece.RetRcptAddrOptionalLine1BelowStreet = docType.RetRcptAddrOptionalLine1BelowStreet;
                                    mailPiece.RetRcptAddrOptionalLine2BelowStreet = docType.RetRcptAddrOptionalLine2BelowStreet;
                                    mailPiece.RetRcptAddressCity = docType.RetRcptAddressCity;
                                    mailPiece.RetRcptAddressState = docType.RetRcptAddressState;
                                    mailPiece.RetRcptAddressZIP = docType.RetRcptAddressZIP;
                                }
                            }
                        }

                        mailPieceSvc.UpdateBatchMailPieceDetails(mailPiece);
                    }
                }
                catch (Exception ex)
                {
                    var msg = "Error occured in ValidateAddress Method For Mail Batch Id: {0} with Error : {1}".Fill(validMailPieces.Select(c => c.MailBatchId).Distinct().SingleOrDefault(), ex.Message);
                    log.Error(msg, ex);
                    MailUtility.SendUnexpectedExceptionEmail(msg);
                    throw ex;
                }
            }
            log.Debug("Address type validation job is completed.");
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
