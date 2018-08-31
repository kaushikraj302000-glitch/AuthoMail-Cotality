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
                this.CalculateDueDateAndPrintLocation();
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
        /// Calculate effective receivedon date, printlocation and due date all valid batches which are queued as ValidateBatches
        /// </summary>
        protected abstract void CalculateDueDateAndPrintLocation();
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

        public static void CalculateDueDateAndPrintLocation(int clientId)
        {
            log.Debug("Calculation of Due Date and Print location job is started.");
            using (IMailBatchService mailBatchSvc = new MailBatchService())
            using (IMailPieceService mailPieceSvc = new MailPieceService())
            using (IDocTypeService docTypeSvc = new DocTypeService())
            {
                List<MailBatchDTO> validatedBatches = mailBatchSvc.GetBatchesByPipelineStatus(BatchPipelineStatus.Validated, clientId);
                if (validatedBatches.Count == 0)
                {
                    log.Debug("Calculation of Due Date and Print location job queue is empty.");
                    return;
                }

                foreach (var mailBatch in validatedBatches)
                {
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
                        mailPiece.PrintLocation = Channel.GetPrintLocation(docType, effectiveEstReceivedOn);

                        mailPieceSvc.UpdateBatchMailPieceDetails(mailPiece);
                    }
                }
            }
            log.Debug("Calculation of Due Date and Print location Job is ended.");
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
                    MailUtility.SendProductionNotificationEmail(mbi, mailBatchData, reportToMailHSBC, emailChannelHSBC);
                    break;
                default:
                    throw new Exception("Invalid App Code");

            }
        }

        #region EffectiveReceivedOn DueDate PrintLocation Logic
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

        public static string GetPrintLocation(DocType docType, DateTime estEffectiveReceivedOn)
        {
            if (estEffectiveReceivedOn.DayOfWeek == DayOfWeek.Saturday && docType.ShippingCarrier == Carrier.FedEx.ToString())
                return Config.Common_PrintLocationCL;

            if (estEffectiveReceivedOn.DayOfWeek == DayOfWeek.Saturday)
                return Config.Common_PrintLocationPCI;

            if (docType.ShippingCarrier == Carrier.USPS.ToString() && docType.ShippingServiceType == AM.USPSLib.ServiceType.PRIORITY_EXPRESS.ToString())
                return Config.Common_PrintLocationCL;

            if (docType.ShippingCarrier == Carrier.FedEx.ToString() && (estEffectiveReceivedOn.TimeOfDay >= Config.Monitor_InternalPrintAndShipStartTime && estEffectiveReceivedOn.TimeOfDay < Config.Monitor_InternalPrintAndShipEndTime))
                return Config.Common_PrintLocationCL;

            return Config.Common_PrintLocationPCI;
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
