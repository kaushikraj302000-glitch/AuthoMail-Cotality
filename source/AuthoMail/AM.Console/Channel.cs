using AM.Entities;
using AM.Lib;
using AM.Services;
using AM.USPSLib;
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
                this.ValidateAddress();
                this.CalculateDueDateAndPrintLocation();
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
        /// <summary>
        /// Validates address types
        /// </summary>
        protected abstract void ValidateAddress();
        /// <summary>
        /// Calculate effective receivedon date, printlocation and due date all valid batches which are queued as ValidateBatches
        /// </summary>
        protected abstract void CalculateDueDateAndPrintLocation();
        /// <summary>
        /// Acknowledge all batches which are queued as Validated
        /// </summary>
        protected abstract void AcknowledgeBatches();

        #region Reported Back logic
        /// <summary>
        /// Sends batch level reports of all batches which are queued as ReportPrepped
        /// </summary>
        public static void ReportBack(string channelCode)
        {
            log.Debug("ReportedBack job is started.");

            switch (channelCode)
            {
                case "AMC":
                    ProcessAMCReportedBack(channelCode);
                    break;
                case "HSBC":
                case "ALASKA":
                default:
                    ProcessGeneralReportedBack(channelCode);
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
                List<MailBatchDTO> reportPreppedBatches = mailBatchSvc.GetBatchesByPipelineStatusAndClientId(BatchPipelineStatus.ReportPrepped);
                if (reportPreppedBatches == null)
                {
                    log.Debug("Uploading report job queue is empty.");
                    return;
                }

                SFTChannel channel = clientSvc.GetSFTChannelDetailsBySFTChannelCode(channelCode + "_" + Config.Common_Environment);

                reportPreppedBatches = reportPreppedBatches.Where(b => b.SFTChannelId == channel.Id).ToList();

                foreach (var batch in reportPreppedBatches)
                {
                    Client client = clientSvc.GetClientById(batch.ClientId);

                    List<Deliverable> deliverables = commonSvc.GetDeliverablesByClientId(client.Id);

                    MailBatchInfo mbi = new MailBatchInfo(batch, client, Config.Common_AuthoMailRootFolder);

                    List<MailPieces> mailPieces;
                    using (IReportService reportSvc = new ReportService())
                    {
                        mailPieces = reportSvc.GetMailPiecesDataFromMailBatchId(mbi.MailBatch.Id);
                    }

                    SFTChannel sftChannelDetails = clientSvc.GetSFTChannelDetailsBySFTChannelId(batch.SFTChannelId);
                    if (sftChannelDetails != null)
                    {
                        try
                        {
                            SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(sftChannelDetails.Protocol), sftChannelDetails.Host, sftChannelDetails.UserName, sftChannelDetails.Password, 300000);
                            sftSite.Connect();

                            //TODO: Remove switch case after proper logic implementation of deliverables
                            UploadReportsToClients(mbi, sftSite, deliverables, mailPieces);

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
        }

        private static void ProcessAMCReportedBack(string channelCode)
        {
            using (IMailBatchService mailBatchSvc = new MailBatchService())
            using (IMailPieceService mailPieceSvc = new MailPieceService())
            using (IClientService clientSvc = new ClientService())
            using (ICommonService commonSvc = new CommonService())
            using (IReportService reportSvc = new ReportService())
            {
                SFTChannel sftChannelDetails = clientSvc.GetSFTChannelDetailsBySFTChannelCode(channelCode + "_" + Config.Common_Environment);
                var reportPreppedBatches = mailBatchSvc.GetReportPreppedPrintBatchDetails();
                SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(sftChannelDetails.Protocol), sftChannelDetails.Host, sftChannelDetails.UserName, sftChannelDetails.Password, 300000);
                sftSite.Connect();
                foreach (var item in reportPreppedBatches)
                {
                    try
                    {
                        Client client = clientSvc.GetClientByAppCode(item.AppCode);

                        Deliverable deliverable = commonSvc.GetDeliverableByReportType(client.Id, ReportTypes.Delivery);

                        string localReportPath = Path.Combine(Config.Common_AuthoMailRootFolder, client.AppCode, Util.AMResponseFolderName, item.PrintBatchId.ToString());

                        //TODO: Remove switch case after proper logic implementation of deliverables
                        UploadReportsToAMC(deliverable, localReportPath, sftSite, false);

                        mailBatchSvc.UpdatePrintBatchPipelineStatus(item.PrintBatchId, BatchPipelineStatus.ReportedBack, Common.Now);

                        var mailBatchIds = reportSvc.GetMailPiecesDataFromPrintBatchId(item.PrintBatchId).Select(b => b.MailBatchId).Distinct().ToList();

                        foreach (int mailBatchId in mailBatchIds)
                        {
                            var mailBatch = mailBatchSvc.GetMailBatchDetailByBatchId(mailBatchId);
                            var mailPieceCount = mailPieceSvc.GetMailPiecesByMailBatchId(mailBatchId).Count(status => status.ShippingStatus == ShippingStatus.InProgress.ToString());

                            if (mailPieceCount > 0 || mailBatch.PipelineStatus == BatchPipelineStatus.ReportedBack)
                                continue;

                            MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, Config.Common_AuthoMailRootFolder);
                            var mailPieceData = reportSvc.GetMailPiecesDataFromMailBatchId(mailBatchId);

                            string errorReportLocalPath = Path.Combine(Config.Common_AuthoMailRootFolder, client.AppCode, mailBatchId.ToString(), Util.AMResponseFolderName);
                            GenerateErrorReport(mailBatchId, mailPieceData, errorReportLocalPath);
                            UploadReportsToAMC(deliverable, errorReportLocalPath, sftSite, true);


                            GenerateAndSendProductionSummaryNotification(mbi, mailPieceData, deliverable);
                            commonSvc.UpdateBatchPipeLineStatus(mailBatchId, BatchPipelineStatus.ReportedBack, Common.Now);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                    }
                }

                if (sftSite.IsConnected)
                    sftSite.Disconnect();
            }
        }

        private static void GenerateAndSendProductionSummaryNotification(MailBatchInfo mbi, List<MailPieces> mailPieces, Deliverable deliverable)
        {
            string errorReportPath = string.Empty;
            string errorReportFileName = string.Empty;
            StringBuilder errorReportData = new StringBuilder();
            string deliveryReportPath = string.Empty;
            string deliveryReportFileName = string.Empty;
            StringBuilder deliveryReportData = new StringBuilder();
            var reportToMail = new List<string>();
            switch (mbi.Client.AppCode)
            {
                case "HSBC":
                    using (IHSBCService hsbcSvc = new HSBCService())
                    {
                        StringBuilder deliveryReport = hsbcSvc.HSBCDeliveryReport(mailPieces, mbi.Client.TimeZone, deliverable.Delimeter);

                        deliveryReportFileName = "{0}.DeliveryReport_{1}.csv".Fill(mbi.BatchName.Split('[')[0], mbi.MailBatch.ReceivedOn.Value.UtcToClientTimeZone(mbi.Client.TimeZone).ToString("htt"));
                        deliveryReportPath = Path.Combine(mbi.AMResponsePath, deliveryReportFileName);

                        File.WriteAllText(deliveryReportPath, deliveryReport.ToString());
                        reportToMail.Add(deliveryReportPath);

                        StringBuilder errorReport = hsbcSvc.HSBCErrorReport(mailPieces, mbi.Client.TimeZone, deliverable.Delimeter);

                        errorReportFileName = "{0}.ErrorReport_{1}.csv".Fill(mbi.BatchName.Split('[')[0], mbi.MailBatch.ReceivedOn.Value.UtcToClientTimeZone(mbi.Client.TimeZone).ToString("htt"));
                        errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);

                        File.WriteAllText(errorReportPath, errorReport.ToString());
                        reportToMail.Add(errorReportPath);
                    }
                    break;
                default:
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
            }

            MailUtility.SendProductionNotificationEmail(mbi, mailPieces, reportToMail, deliverable.EmailChannel);
        }

        private static void GenerateErrorReport(int mailBatchId, List<MailPieces> mailPieceData, string localErrorReportPath)
        {
            using (IAMCService amcSvc = new AMCService())
            {
                StringBuilder errorReport = amcSvc.AMCErrorReport(mailPieceData);

                if (errorReport != null)
                {
                    string errorReportFileName = "Error-{0}.txt".Fill(Common.Now.ToString("yyyyMMdd-HHmmss"));

                    //TODO: Comment below and uncomment above if don't want to move new naming convention and tab delimited report to Production environment
                    //string errorReportFileName = "Error-{0}-{1}-{2}-{3}.txt".Fill(mbi.BatchName, mbi.Client.AppCode, mbi.MailBatch.Id, Common.Now.ToString("yyyyMMdd-HHmmss"));
                    string errorReportPath = Path.Combine(localErrorReportPath, errorReportFileName);

                    File.WriteAllText(errorReportPath, errorReport.ToString());
                }
            }
        }

        private static void UploadReportsToAMC(Deliverable deliverable, string localReportPath, SFTUtil sftSite, bool isErrorReport)
        {
            string uploadPath = deliverable.UploadPath;
            if (sftSite.IsDownloadDirectoryAvailable(uploadPath))
            {
                //TODO: Replace *.csv in place of *.txt if we don't move new naming convention and tab delimited reports to production environment
                if (isErrorReport)
                {
                    var errorReportList = Directory.EnumerateFiles(localReportPath, "*.txt").Where(f => Path.GetFileNameWithoutExtension(f).StartsWith("Error-")).ToList();

                    foreach (string filePath in errorReportList)
                    {
                        string fileName = Path.GetFileName(filePath);
                        string ftpUploadPath = Path.Combine(uploadPath, fileName);
                        sftSite.UploadFile(filePath, ftpUploadPath);
                    }

                    if (errorReportList.Count(f => Path.GetFileNameWithoutExtension(f).StartsWith("Error-")) > 0)
                    {
                        string errorGoFilePath = Path.Combine(localReportPath, "ErrReport.go");
                        string ftpErrorGoUploadPath = Path.Combine(uploadPath, "ErrReport.go");
                        File.WriteAllText(errorGoFilePath, string.Empty);

                        sftSite.UploadFile(errorGoFilePath, ftpErrorGoUploadPath);
                    }
                    return;
                }

                var deliveryReportList = Directory.EnumerateFiles(localReportPath, "*.txt").Where(f => Path.GetFileNameWithoutExtension(f).StartsWith("Delivery-")).ToList();


                foreach (string filePath in deliveryReportList)
                {
                    string fileName = Path.GetFileName(filePath);
                    string ftpUploadPath = Path.Combine(uploadPath, fileName);
                    sftSite.UploadFile(filePath, ftpUploadPath);
                }

                if (deliveryReportList.Count(f => Path.GetFileNameWithoutExtension(f).StartsWith("Delivery-")) > 0)
                {
                    string deliveryGoFilePath = Path.Combine(localReportPath, "DlvrReport.go");
                    string ftpDeliveryGoUploadPath = Path.Combine(uploadPath, "DlvrReport.go");
                    File.WriteAllText(deliveryGoFilePath, string.Empty);

                    sftSite.UploadFile(deliveryGoFilePath, ftpDeliveryGoUploadPath);
                }
            }
            else
            {
                log.Error("Upload path does not exist for Client-{0} and SFT Channel Path-'{1}'".Fill("AMC", uploadPath));
            }
        }

        private static void UploadReportsToClients(MailBatchInfo mbi, SFTUtil sftSite, List<Deliverable> deliverables, List<MailPieces> mailPieceData)
        {
            switch (mbi.Client.AppCode)
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
                    break;
                case "HSBC":
                case "AMTEST":
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
                    break;
                default:
                    throw new Exception("Invalid App Code");

            }
        }
        #endregion

        #region EffectiveReceivedOn DueDate PrintLocation Logic
        public static void CalculateDueDateAndPrintLocation(int sftChannelId)
        {
            log.Debug("Calculation of Due Date and Print location job is started.");
            using (IMailBatchService mailBatchSvc = new MailBatchService())
            using (IMailPieceService mailPieceSvc = new MailPieceService())
            using (IDocTypeService docTypeSvc = new DocTypeService())
            using (ICommonService commonSvc = new CommonService())
            using (IClientService clientSvc = new ClientService())
            {

                List<MailBatchDTO> validatedBatches = mailBatchSvc.GetBatchesByPipelineStatusAndSFTChannelId(BatchPipelineStatus.Validated, sftChannelId);
                if (validatedBatches.Count == 0)
                {
                    log.Debug("Calculation of Due Date and Print location job queue is empty.");
                    return;
                }

                var sweepTimings = commonSvc.GetPCIBundleSweepTimings();

                foreach (var mailBatch in validatedBatches)
                {
                    Client client = clientSvc.GetClientById(mailBatch.ClientId);

                    var mailPieceData = mailPieceSvc.GetMailPiecesByMailBatchId(mailBatch.Id);

                    List<MailPieceDTO> validMailPieces = mailPieceData.Where(c => c.ErrorCode == OutComeCodes.No_Error && c.ShippingStatus == ShippingStatus.InProgress.ToString()).ToList();

                    if (validMailPieces.Count == 0)
                    {
                        continue;
                    }

                    foreach (MailPieceDTO mailPiece in validMailPieces)
                    {
                        DocType docType = docTypeSvc.GetDocTypeById(mailPiece.DocTypeId.Value);

                        //Here first we will calculate effective received on date and based on that we will calculate due date for that
                        DateTime effectiveEstReceivedOn = Channel.GetEffectiveEstReceivedOn(docType, mailBatch.ReceivedOn.Value);
                        //Here storing effective receivedon in UTC format as per rule
                        mailPiece.EffectiveReceivedOnEst = effectiveEstReceivedOn;
                        //Calculate Due Date for Mail Pieces doc type wise
                        mailPiece.DueDate = Channel.GetDueDate(docType, effectiveEstReceivedOn);
                        //Set print location where mail piece will go for print and ship
                        mailPiece.PrintTrailId = Channel.GetPrintTrail(docType, effectiveEstReceivedOn, client);
                        mailPiece.PrintLocation = commonSvc.GetPrintVendorDetailsByPrintTrailId(mailPiece.PrintTrailId.Value).Name;
                        //This field will be used for monitoring data
                        mailPiece.StdSubmissionTime = commonSvc.GetStandardSubmissionTime(client.AppCode, mailPiece.PrintLocation, mailBatch.ReceivedOn.Value, sweepTimings);
                        mailPiece.MaxSubmissionTime = Channel.GetMaximumSubmissionTime(docType, effectiveEstReceivedOn);
                        mailPieceSvc.UpdateBatchMailPieceDetails(mailPiece);
                    }
                }
            }
            log.Debug("Calculation of Due Date and Print location Job is ended.");
        }

        private static DateTime GetMaximumSubmissionTime(DocType docType, DateTime effectiveReceivedOnEst)
        {
            DayOfWeek dayOfWeek = effectiveReceivedOnEst.DayOfWeek;

            string cutOffTime = string.Empty;

            switch (dayOfWeek)
            {
                case DayOfWeek.Saturday:
                    cutOffTime = docType.SatCutOff;
                    break;
                case DayOfWeek.Sunday:
                    cutOffTime = docType.SunCutOff;
                    break;
                default:
                    cutOffTime = docType.BizdayCutOff;
                    break;
            }

            var effectiveReceivedOnEstDate = effectiveReceivedOnEst.Date;
            var maxSubmissionDateTime = effectiveReceivedOnEstDate + TimeSpan.Parse(cutOffTime);

            return maxSubmissionDateTime.EstToUtc();
        }

        public static DateTime GetEffectiveEstReceivedOn(DocType docType, DateTime utcReceivedOn)
        {
            var estReceivedOn = utcReceivedOn.UtcToEst();
            var estReceivedOnDate = estReceivedOn.Date;
            var effectiveEstReceivedOn = estReceivedOn;

            bool isSatWorkDay = docType.SatSLAType == SLAType.NA ? false : true;
            bool isSunWorkDay = docType.SunSLAType == SLAType.NA ? false : true;

            if (IsBusinessDay(estReceivedOn, Config.Common_Holidays, isSatWorkDay, isSunWorkDay))
            {
                DayOfWeek dayOfWeek = estReceivedOn.DayOfWeek;

                string slaCutOff = string.Empty;
                string slaType = string.Empty;

                if (dayOfWeek == DayOfWeek.Saturday)
                {
                    slaCutOff = docType.SatCutOff;
                    slaType = docType.SatSLAType;
                }

                if (dayOfWeek == DayOfWeek.Sunday)
                {
                    slaCutOff = docType.SunCutOff;
                    slaType = docType.SunSLAType;
                }

                if (dayOfWeek != DayOfWeek.Sunday && dayOfWeek != DayOfWeek.Saturday)
                {
                    slaCutOff = docType.BizdayCutOff;
                    slaType = docType.BizdaySLAType;
                }

                if (slaType == SLAType.FixedTime)
                {
                    var estSLACutOffDateTime = (estReceivedOnDate + TimeSpan.Parse(slaCutOff));

                    if (estReceivedOn > estSLACutOffDateTime)
                    {
                        effectiveEstReceivedOn = GetNextWorkingDay(estReceivedOnDate, Config.Common_Holidays, isSatWorkDay, isSunWorkDay);
                    }
                }
                else
                {
                    effectiveEstReceivedOn = estReceivedOn;
                }
            }
            else
            {
                effectiveEstReceivedOn = GetNextWorkingDay(estReceivedOnDate, Config.Common_Holidays, isSatWorkDay, isSunWorkDay);
            }

            return effectiveEstReceivedOn;
        }

        private static DateTime GetNextWorkingDay(DateTime estDateTime, List<Holiday> HolidayList, bool isSatWorkDay, bool isSunWorkDay)
        {
            do
            {
                estDateTime = estDateTime.AddDays(1);
            }
            while (!IsBusinessDay(estDateTime, HolidayList, isSatWorkDay, isSunWorkDay));

            return estDateTime;
        }

        public static DateTime GetDueDate(DocType docType, DateTime estEffectiveReceivedOn)
        {
            bool isSatWorkDay = docType.SatSLAType == SLAType.NA ? false : true;
            bool isSunWorkDay = docType.SunSLAType == SLAType.NA ? false : true;

            var estEffectiveReceivedOnDate = estEffectiveReceivedOn.Date;

            DayOfWeek dayOfWeek = estEffectiveReceivedOnDate.DayOfWeek;

            string slaTimeSpan = string.Empty;
            string slaType = string.Empty;

            DateTime dueDate = estEffectiveReceivedOn;

            if (dayOfWeek == DayOfWeek.Saturday)
            {
                slaTimeSpan = docType.SatTimeSpan;
                slaType = docType.SatSLAType;
            }

            if (dayOfWeek == DayOfWeek.Sunday)
            {
                slaTimeSpan = docType.SunTimeSpan;
                slaType = docType.SunSLAType;
            }

            if (dayOfWeek != DayOfWeek.Sunday && dayOfWeek != DayOfWeek.Saturday)
            {
                slaTimeSpan = docType.BizdayTimeSpan;
                slaType = docType.BizdaySLAType;
            }

            TimeSpan timeSpan = TimeSpan.Parse(slaTimeSpan);
            if (slaType == SLAType.FixedTime)
            {
                //Here we are taking only time from full time span which includes dates
                dueDate = estEffectiveReceivedOnDate + TimeSpan.Parse(string.Format("{0:hh\\:mm\\:ss}", timeSpan));

                for (int i = 0; i < timeSpan.Days; i++)
                {
                    dueDate = GetNextWorkingDay(dueDate, Config.Common_Holidays, isSatWorkDay, isSunWorkDay);
                }
            }
            else
            {
                dueDate = estEffectiveReceivedOn + TimeSpan.Parse(string.Format("{0:hh\\:mm\\:ss}", timeSpan));
                for (int i = 0; i < timeSpan.Days; i++)
                {
                    dueDate = GetNextWorkingDay(dueDate, Config.Common_Holidays, isSatWorkDay, isSunWorkDay);
                }
            }


            return dueDate.EstToUtc();
        }

        public static int GetPrintTrail(DocType docType, DateTime estEffectiveReceivedOn, Client client)
        {
            DocTypePrintTrailMap docTypePrintTrail;

            using (ICommonService commonSvc = new CommonService())
            {
                List<DocTypePrintTrailMap> docTypePrintTrailMaps = commonSvc.GetTrailMapDetailsByDocTypeId(docType.Id);
                var dayofWeek = estEffectiveReceivedOn.DayOfWeek;

                if (dayofWeek == DayOfWeek.Saturday)
                {
                    docTypePrintTrail = docTypePrintTrailMaps.Single(c => estEffectiveReceivedOn.TimeOfDay >= c.SatStartTime && estEffectiveReceivedOn.TimeOfDay <= c.SatEndTime);
                }
                else if (dayofWeek == DayOfWeek.Sunday)
                {
                    docTypePrintTrail = docTypePrintTrailMaps.Single(c => estEffectiveReceivedOn.TimeOfDay >= c.SunStartTime && estEffectiveReceivedOn.TimeOfDay <= c.SunEndTime);
                }
                else
                {
                    docTypePrintTrail = docTypePrintTrailMaps.Single(c => estEffectiveReceivedOn.TimeOfDay >= c.BizDayStartTime && estEffectiveReceivedOn.TimeOfDay <= c.BizDayEndTime);
                }
            }

            return docTypePrintTrail.PrintTrailId;
        }

        private static bool IsBusinessDay(DateTime dueDate, List<Holiday> holidays, bool isSatWorkDay, bool isSunWorkDay)
        {
            if (holidays.Select(c => c.HolidayDate).Contains(dueDate.Date))
                return false;

            if (dueDate.DayOfWeek == DayOfWeek.Saturday)
                return isSatWorkDay;

            if (dueDate.DayOfWeek == DayOfWeek.Sunday)
                return isSunWorkDay;

            return true;
        }
        #endregion

        #region AddressType Validation
        public static void ValidateAddressType(int sftChannelId)
        {
            log.Debug("Address type validation job is started.");
            using (IMailBatchService mailBatchSvc = new MailBatchService())
            using (IMailPieceService mailPieceSvc = new MailPieceService())
            using (IDocTypeService docTypeSvc = new DocTypeService())
            using (ICommonService commonSvc = new CommonService())
            using (IClientService clientSvc = new ClientService())
            {

                List<MailBatchDTO> validatedBatches = mailBatchSvc.GetBatchesByPipelineStatusAndSFTChannelId(BatchPipelineStatus.Validated, sftChannelId);
                if (validatedBatches.Count == 0)
                {
                    log.Debug("Address type validation job queue is empty.");
                    return;
                }

                foreach (var mailBatch in validatedBatches)
                {
                    Client client = clientSvc.GetClientById(mailBatch.ClientId);

                    var mailPieceData = mailPieceSvc.GetMailPiecesByMailBatchId(mailBatch.Id);

                    List<MailPieceDTO> validMailPieces = mailPieceData.Where(c => c.ErrorCode == OutComeCodes.No_Error && c.ShippingStatus == ShippingStatus.InProgress.ToString()).ToList();

                    if (validMailPieces.Count == 0)
                    {
                        continue;
                    }

                    #region Address Validation Logic
                    //Get validate Addresses for each records
                    List<AddressResponse> addressResponses = new List<AddressResponse>();
                    List<AddressRequest> addressRequestes = new List<AddressRequest>();
                    foreach (var mailPiece in validMailPieces)
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
                    foreach (MailPieceDTO mailPiece in validMailPieces)
                    {
                        string documentOrPackageType = mailPiece.DocumentOrPackageType;

                        //Document Or Package Type validation
                        var docType = docTypeSvc.GetDocTypeByDocTypeName(mailPiece.ClientId, documentOrPackageType);

                        var addressResponse = addressResponses.Single(c => c.Id.ToLower() == mailPiece.UniqueId.ToString().ToLower());

                        if (docType != null)
                        {
                            if (docType.ShippingCarrier == Carrier.FedEx.ToString())
                            {
                                if (!(addressResponse.AddressTypes.All(c => c == AddressType.DHAM)))
                                {
                                    documentOrPackageType = documentOrPackageType + Config.Monitor_ExpressMailingText;

                                    docType = docTypeSvc.GetDocTypeByDocTypeName(mailPiece.ClientId, documentOrPackageType);
                                }
                            }
                            mailPiece.RcptAddressTypes = string.Join(",", addressResponse.AddressTypes);
                            mailPiece.DocTypeId = docType.Id;
                        }

                        if (addressResponse.AddressTypes.Any(addr => addr == AddressType.INVALID))
                        {
                            mailPiece.ShippingStatus = ShippingStatus.Error.ToString();
                            mailPiece.ErrorCode = OutComeCodes.Mailing_Address_Missing_Or_Invalid;
                            mailPiece.ErrorDesc = !string.IsNullOrEmpty(addressResponse.VerifierDesc) ? addressResponse.VerifierDesc : addressResponse.Error.Description;
                        }
                        else if (docType == null)
                        {
                            mailPiece.ErrorCode = OutComeCodes.Invalid_PackageType;
                            mailPiece.ErrorDesc = "The specified document or package type '{0}' is not configured.".Fill(mailPiece.DocumentOrPackageType);
                            mailPiece.ShippingStatus = ShippingStatus.Error.ToString();
                        }
                        mailPieceSvc.UpdateBatchMailPieceDetails(mailPiece);
                    }
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
