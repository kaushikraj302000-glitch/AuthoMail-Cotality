using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Microsoft.Practices.EnterpriseLibrary.Data;
using PCI.Service.Utilities;
using System.Text.RegularExpressions;
using PCI.Lib;
using System.Data;
using System.Threading;
using System.Transactions;
using Nordis.Lib;

namespace PCI.Service
{
    public static class PCIJobMonitor
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PCIJobMonitor));
        private static Database db = DatabaseFactory.CreateDatabase();
        private static List<PCIField> MRDFRequestSchema { get; set; }

        static PCIJobMonitor()
        {
            MRDFRequestSchema = DAL.GetMRDFRequestSchema();
        }

        #region Submit Batches To Print Vendors

        public static void SubmitBatchToPrintVendor(string pvCode, bool isBundle, List<string> ptNames)
        {

            List<string> unprocessedPrintTrails = new List<string>();

            var pieceIdFormat = MRDFRequestSchema.Single(f => f.FieldName == "PieceId").DataFormat;

            if (isBundle)
            {
                try
                {
                    DateTime startDateTime = Common.EstToUtc(Common.EstToday);
                    DateTime toDateTime = Common.GetCurrentTime();
                    DateTime endDateTime = new DateTime(toDateTime.Year, toDateTime.Month, toDateTime.Day, toDateTime.Hour, toDateTime.Minute, 0);
                    int bundlingTime = 0;

                    //Waiting Code for AuthoMail as well as BatchSummary
                    while (!DAL.CheckBatchesInQueue(startDateTime, endDateTime))
                    {
                        log.Debug("Queue Check Waiting...");

                        Thread.Sleep(6000);

                        //NOTE: If bundling is continueously running for 6 minutes then system will continue further for submitting batches to print vendor.
                        bundlingTime += 6000;
                        if (bundlingTime >= 360000)
                            break;
                    }
                }
                catch (Exception ex)
                {
                    var msg = "SubmitBatchToPrintVendor : Error occured in PCIJob Monitor while checking for batches in queue.";
                    log.Error(msg, ex);
                    MailUtility.SendUnexpectedNotificationEmail(msg + "<br/> <b>Error Message</b> <br/> {0}".Fill(ex));

                    //NOTE: Reprocess if any exception occured while getting bundling queue data check
                    Thread.Sleep(9000);
                    SubmitBatchToPrintVendor(pvCode, isBundle, ptNames);
                    return;
                }
            }


            foreach (string ptName in ptNames)
            {
                try
                {
                    log.Info("Inserting Process Started For Print Trail {0}".Fill(ptName));
                    InsertStage(ptName);
                    log.Info("Inserting Process Completed For Print Trail {0}".Fill(ptName));
                }
                catch (Exception ex)
                {
                    var msg = "SubmitBatchToPrintVendor : Error occured in PCIJob Monitor while getting print trail batch details for PrintTrail - {0}".Fill(ptName);
                    log.Error(msg, ex);

                    MailUtility.SendUnexpectedNotificationEmail(msg + "<br/> <b>Error Message</b> <br/> {0}".Fill(ex));

                    //NOTE: Here System will add unprocessed print trails to process it once all other print trails are getting processed successfully.
                    unprocessedPrintTrails.Add(ptName);
                    continue;
                }
            }

            //NOTE: Here System will process unprocessed print trails.
            if (unprocessedPrintTrails.Count > 0)
            {
                log.Info("Processing Again Unprocessed PrintTrails {0}".Fill(unprocessedPrintTrails));
                SubmitBatchToPrintVendor(pvCode, isBundle, unprocessedPrintTrails);
                log.Info("Processing Completed Unprocessed PrintTrails {0}".Fill(unprocessedPrintTrails));
                return;
            }

            //NOTE : Here system will try to submit bundle batches 5 times if any error is coming during execution
            30.TimesAttempt(() => SubmitBundledBatches(pvCode));

        }

        public static void SubmitBundledBatches(string pvCode)
        {
            //NOTE : Here system will process not submitted batches if any error occured while getting any type of data. 
            //NOTE : It will be used specifically for PCI and NORDIS client because they all have fix sweeps.
            var notSubmittedList = DAL.GetNotSubmittedPrintBatchDetailByPrintVendorCode(pvCode);

            while (notSubmittedList.Count > 0)
            {
                log.Info("Submit Bundle Process Started");
                SubmitBatches(notSubmittedList);
                log.Info("Submit Bundle Process Completed");
                Thread.Sleep(12000);
                notSubmittedList = DAL.GetNotSubmittedPrintBatchDetailByPrintVendorCode(pvCode);
            }
        }

        private static void SubmitBatches(List<PrintTrailBatch> ptBatchList)
        {
            foreach (PrintTrailBatch ptBatch in ptBatchList)
            {
                try
                {
                    log.Info("Prepped Stage Execution Started For PrintTrailBatchId - {0}".Fill(ptBatch.Id));
                    PrepAndUploadStage(ptBatch);
                    log.Info("Prepped Stage Execution Completed For PrintTrailBatchId - {0}".Fill(ptBatch.Id));
                }
                catch (Exception ex)
                {
                    var msg = "SubmitBatchToPrintVendor : Error occured while uploading batch to print vendor for PrintTrailBatchId - {0}".Fill(ptBatch.Id);
                    log.Error(msg, ex);

                    MailUtility.SendUnexpectedNotificationEmail(msg + "<br/> <b>Error Message</b> <br/> {0}".Fill(ex));

                    continue;
                }

                try
                {
                    log.Info("Submitted Stage Execution Started For PrintTrailBatchId - {0}".Fill(ptBatch.Id));
                    SubmitStage(ptBatch);
                    log.Info("Submitted Stage Execution Completed For PrintTrailBatchId - {0}".Fill(ptBatch.Id));
                }
                catch (Exception ex)
                {
                    var msg = "SubmitBatchToPrintVendor : Error occured while updating Batch Status for PrintTrailBatchId - {0}".Fill(ptBatch.Id);
                    log.Error(msg, ex);

                    MailUtility.SendUnexpectedNotificationEmail(msg + "<br/> <b>Error Message</b> <br/> {0}".Fill(ex));

                    continue;
                }
            }
        }

        private static void InsertStage(string ptName)
        {
            //For divert primary key issue we are using starting number as 90001
            int sequenceNo = 90001;
            PrintTrailBatch ptBatchDetail;

            PrintTrailDetail ptDetail = DAL.GetPrintTrailDetailByPrintTrailName(ptName);

            List<MailPieceDetail> mpDetails = DAL.GetMRDFRequestDataByPrintTrailId(ptDetail.PrintTrailId);

            if (mpDetails == null || mpDetails.Count == 0)
            {
                log.Debug("SubmitBatchToPrintVendor : Submit Batches job queue is empty for Print Trail {0}".Fill(ptName));
                return;
            }

            ptBatchDetail = new PrintTrailBatch()
            {
                PrintTrailId = ptDetail.PrintTrailId
            };


            using (TransactionScope scope = new TransactionScope())
            {
                log.Info("Inserting Data Started.");
                ptBatchDetail.Id = DAL.InsertPrintTrailBatchDetail(ptBatchDetail);
                log.Info("Inserting Data Successfull.");

                mpDetails.ForEach(mp =>
                {
                    mp.PrintTrailBatchId = ptBatchDetail.Id;
                    if (mp.MailPieceStatus == MailPieceStatus.PulledBack.ToString() || mp.MailPieceStatus == MailPieceStatus.Error.ToString())
                        mp.SequenceNo = -1;
                    else
                    {
                        mp.SequenceNo = sequenceNo++;
                    }
                });

                var maxSequenceNo = mpDetails.Max(p => p.SequenceNo);

                mpDetails.ForEach(mrdf =>
                {
                    if (mrdf.SequenceNo == -1)
                    {
                        mrdf.SequenceNo = ++maxSequenceNo;
                    }
                });

                log.Info("Updating Data Started For MRDFRequest.");
                DAL.UpdatedPrintTrailBatchIdInMRDFRequest(ptBatchDetail.Id, mpDetails);
                log.Info("Updating Data Completed For MRDFRequest.");

                ptBatchDetail.PrintBatchName = "{0}-{1}-{2}".Fill(ptDetail.BusinessCode, ptBatchDetail.Id, ptDetail.TrailName);

                log.Info("Updating Data Started For PrintTrailBatch.");
                DAL.UpdatePrintTrailBatchDetail(ptBatchDetail);
                log.Info("Updating Data Started For PrintTrailBatch.");

                scope.Complete();
            }
        }

        private static void PrepAndUploadStage(PrintTrailBatch ptBatchDetails)
        {
            string batchPath = string.Empty;

            var ptDetails = DAL.GetPrintTrailDetailByPrintTrailId(ptBatchDetails.PrintTrailId);

            var mpList = DAL.GetMRDFRequestListByPrintTrailBatchId(ptBatchDetails.Id);

            if (Common.ApplicationEnvironment == Env.Prod || ptDetails.SimulateTrail == false)
            {
                if (ptDetails.VendorCode == "CL_DALLAS")
                {
                    log.InfoFormat("Uploading Started For PrintTrailBatchId : {0}", ptBatchDetails.Id);
                    CreateDallasBatch(mpList, ptDetails);
                    log.InfoFormat("Uploading Completd For PrintTrailBatchId : {0}", ptBatchDetails.Id);
                }
                else
                {
                    //NOTE: Bundling folder structure will be like : BundlingRoot/PrintTrailBatchId/Request & Response
                    var batchRequestPath = Path.Combine(Common.BundlingRootFolder, ptBatchDetails.Id.ToString(), Common.BundlingRequestFolderName);

                    var batchResponsePath = Path.Combine(Common.BundlingRootFolder, ptBatchDetails.Id.ToString(), Common.BundlingResponseFolderName);

                    batchRequestPath.CreateDirectoryIfNotExist();
                    Thread.Sleep(1000);
                    batchResponsePath.CreateDirectoryIfNotExist();

                    if (ptDetails.VendorCode == "PCI")
                    {
                        var zipFilePath = Path.Combine(batchRequestPath, ptBatchDetails.PrintBatchName + ".zip");
                        var mrdfFilePath = Path.Combine(batchRequestPath, ptBatchDetails.PrintBatchName + ".dat");

                        batchPath = CreatePCIBatch(mpList, ptBatchDetails, zipFilePath, mrdfFilePath, ptDetails);
                    }
                    else
                    {
                        //For NORDIS OR Any Other Print Vendor
                        var zipFilePath = Path.Combine(batchRequestPath, ptBatchDetails.PrintBatchName + ".zip");
                        var xmlFilePath = Path.Combine(batchRequestPath, ptBatchDetails.PrintBatchName + ".xml");

                        batchPath = CreateNordisBatch(mpList, ptBatchDetails, zipFilePath, xmlFilePath, ptDetails);
                    }

                    log.InfoFormat("Uploading Started For PrintTrailBatchId : {0}", ptBatchDetails.Id);
                    UploadBatchToPrintVendorSite(batchPath, ptDetails);
                    log.InfoFormat("Uploading Completd For PrintTrailBatchId : {0}", ptBatchDetails.Id);
                }
            }
        }

        private static void SubmitStage(PrintTrailBatch ptBatchDetails)
        {

            //Get total counts of envelope / BRE envelopes used for valid mail pieces.
            Dictionary<int?, int> envelopeData = new Dictionary<int?, int>();

            var ptDetails = DAL.GetPrintTrailDetailByPrintTrailId(ptBatchDetails.PrintTrailId);

            var mpList = DAL.GetMRDFRequestListByPrintTrailBatchId(ptBatchDetails.Id);

            #region Envelope Stock Update
            //After Updating status of Batch System is going to update envelope inventory stock.
            var envelopes = mpList.GroupBy(c => c.EnvelopeId);
            var breEnvelopes = mpList.GroupBy(c => c.BREEnvelopeId);
            foreach (var env in envelopes.Where(e => e.Key != null))
            {
                envelopeData.Add(env.Key, mpList.Count(mp => mp.EnvelopeId == env.Key));
            }
            foreach (var breEnv in breEnvelopes.Where(e => e.Key != null))
            {
                envelopeData.Add(breEnv.Key, mpList.Count(mp => mp.BREEnvelopeId == breEnv.Key));
            }
            #endregion

            SFTUtil sftSite = new SFTUtil(ptDetails.Protocol.ParseEnum<SFT_ProtocolType>(), ptDetails.Host, ptDetails.UserName, ptDetails.Password,
               60000);

            sftSite.Connect();

            log.DebugFormat("Updating status started For PrintTrailBatchId : {0}", ptBatchDetails.Id);

            using (TransactionScope scope = new TransactionScope())
            {
                //Reason for maintaining this time because it should reflect same for 
                var currentTime = Common.GetCurrentTime();

                DAL.UpdateSubmittedForAllResReceivedBatch(ptBatchDetails.Id, currentTime);

                DAL.UpdatePrintTrailBatchStatus(ptBatchDetails.Id, BatchPipelineStatus.Submitted, currentTime);

                //NOTE : Here we are inserting used envelope stock by PrintTrailBatch in EnvelopeUsage table
                DAL.UpdateEnvelopeInventory(ptBatchDetails, envelopeData);

                if (ptDetails.VendorCode != "CL_DALLAS")
                {
                    string serverFilePath = "{0}/{1}.uploading".Fill(ptDetails.FromADEDir, ptBatchDetails.PrintBatchName);
                    if (sftSite.IsFileAvailable(serverFilePath))
                        sftSite.RenameFile(serverFilePath, "{0}/{1}.zip".Fill(ptDetails.FromADEDir, ptBatchDetails.PrintBatchName));

                    MailUtility.SendBatchSubmittedNotificationEmail(ptDetails.VendorCode, "{0}.zip".Fill(ptBatchDetails.PrintBatchName), mpList.Count, mpList.Sum(c => c.TotalPages));
                }

                scope.Complete();
            }

            log.DebugFormat("Updating status completed For PrintTrailBatchId : {0}", ptBatchDetails.Id);

            sftSite.Disconnect();
        }

        private static void CreateDallasBatch(List<MailPieceDetail> mpList, PrintTrailDetail ptDetails)
        {
            SFTUtil sftSite = new SFTUtil(ptDetails.Protocol.ParseEnum<SFT_ProtocolType>(), ptDetails.Host, ptDetails.UserName, ptDetails.Password,
                60000);

            sftSite.Connect();
            var sw = new Stopwatch();
            sw.Start();

            UploadBatchToInternalPrintVendorSite(Common.PCIRootFolder, sftSite, mpList, ptDetails);

            sw.Stop();
            var time = sw.Elapsed;

            log.DebugFormat("Total time spent : {0} For Dallas Batch", time);
            sftSite.Disconnect();
        }

        private static string CreateNordisBatch(List<MailPieceDetail> mpList, PrintTrailBatch ptBatchDetails, string zipFilePath, string xmlFilePath, PrintTrailDetail ptDetails)
        {
            List<Package> packageData = new List<Package>();
            PrintRequest printRequestXml = new PrintRequest();
            printRequestXml.JobId = ptBatchDetails.Id;
            foreach (MailPieceDetail package in mpList)
            {
                printRequestXml.Packages.Add(
                    new Package()
                    {
                        PieceId = package.MailPieceId.Value,
                        FileName = package.PDFFileName,
                        DueDate = package.MailPieceDueDate,
                        TotalSheets = package.TotalSheets,
                        ClientCode = package.AppCode,
                        IsForeignAddress = false,
                        TotalImages = package.TotalPages,
                        PaperSize = package.PrintSize,
                        PrintFormat = package.PrintFormat.ParseEnum<PrintFormat>(),
                        EnvelopePartNo = package.EnvelopePartNo.ToString(),
                        EnvelopeDesc = package.EnvelopeDesc,
                        BREPartNo = package.BREPartNo.ToString(),
                        BREDesc = package.BREDesc,
                        QCReqd = package.DivertVS3 == 1 ? true : false,
                    });
            }

            File.WriteAllText(xmlFilePath, printRequestXml.ToXml());

            using (ZipArchive newFile = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
            {
                foreach (var mailPiece in mpList)
                {
                    //NOTE:Here we are directly creating Zip with Bundling into another path
                    //We have folder structure like PCIRoot/AppCode/JobId/PCIRequest
                    var localFilePath = Path.Combine(Common.PCIRootFolder, mailPiece.AppCode, mailPiece.JobId.ToString(), Common.PCIRequestFolderName, mailPiece.PDFFileName);
                    newFile.CreateEntryFromFile(localFilePath, mailPiece.PDFFileName);
                }

                //make an entry for mrdf file after making entries of files into zip
                newFile.CreateEntryFromFile(xmlFilePath, Path.GetFileName(xmlFilePath));
            }

            //NOTE : Here system is storing file size of received pdf in KB
            FileInfo fileInfo = new FileInfo(zipFilePath);
            Int64 fileSize = (fileInfo.Length / 1024);
            DAL.UpdatePrintTrailBatchSize(ptBatchDetails.Id, fileSize);

            return zipFilePath;
        }

        private static string CreatePCIBatch(List<MailPieceDetail> mpList, PrintTrailBatch ptBatchDetails, string zipFilePath, string mrdfFilePath, PrintTrailDetail ptDetails)
        {
            var mrdfContent = new StringBuilder();

            foreach (MailPieceDetail row in mpList)
            {
                foreach (var f in MRDFRequestSchema)
                {
                    if (f.FieldName == "JobId")
                    {
                        mrdfContent.AppendFormat(f.DataFormat, row.PrintTrailBatchId);
                    }
                    else
                    {
                        mrdfContent.AppendFormat(f.DataFormat, row.GetType().GetProperty(f.FieldName).GetValue(row, null));
                    }
                }
                mrdfContent.AppendLine();
            }

            File.WriteAllText(mrdfFilePath, mrdfContent.ToString());

            if (File.Exists(zipFilePath))
                File.Delete(zipFilePath);

            using (ZipArchive newFile = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
            {
                foreach (var mailPiece in mpList)
                {
                    //NOTE:Here we are directly creating Zip with Bundling into another path
                    //We have folder structure like PCIRoot/AppCode/JobId/PCIRequest
                    var localFilePath = Path.Combine(Common.PCIRootFolder, mailPiece.AppCode, mailPiece.JobId.ToString(), Common.PCIRequestFolderName, mailPiece.PDFFileName);
                    newFile.CreateEntryFromFile(localFilePath, mailPiece.PDFFileName);
                }

                //make an entry for mrdf file after making entries of files into zip
                newFile.CreateEntryFromFile(mrdfFilePath, Path.GetFileName(mrdfFilePath));
            }

            //NOTE : Here system is storing file size of received pdf in KB
            FileInfo fileInfo = new FileInfo(zipFilePath);
            Int64 fileSize = (fileInfo.Length / 1024);
            DAL.UpdatePrintTrailBatchSize(ptBatchDetails.Id, fileSize);

            return zipFilePath;
        }

        private static void UploadBatchToPrintVendorSite(string zipFilePath, PrintTrailDetail ptDetails)
        {
            SFTUtil sftSite = new SFTUtil(ptDetails.Protocol.ParseEnum<SFT_ProtocolType>(), ptDetails.Host, ptDetails.UserName, ptDetails.Password,
               60000);
            sftSite.Connect();

            var sw = new Stopwatch();
            sw.Start();

            sftSite.UploadFile(zipFilePath, string.Format("{0}/{1}.uploading", ptDetails.FromADEDir, Path.GetFileNameWithoutExtension(zipFilePath)), false, false);

            sw.Stop();
            var time = sw.Elapsed;
            log.DebugFormat("Total time spent : {0} For Batch : {1}", time, Path.GetFileNameWithoutExtension(zipFilePath));

            sftSite.Disconnect();
        }

        private static void UploadBatchToInternalPrintVendorSite(string pciRootPath, SFTUtil sftSite, List<MailPieceDetail> mailPiecesDetails, PrintTrailDetail printTrailDetail)
        {
            //Reason for doing this because we can avoid multiple call from database for each mail piece it may contains multiple clients for same print trails

            var clientIds = mailPiecesDetails.Select(m => m.ClientId).Distinct();
            foreach (var clientId in clientIds)
            {
                Client client = DAL.GetClientAppDetailsById(clientId);

                var mailPiecesByClient = mailPiecesDetails.Where(c => c.ClientId == clientId);

                foreach (MailPieceDetail mailPiece in mailPiecesByClient)
                {
                    //Here we are calling batch timings based on effective received on est date only.
                    var batchTimings = DAL.GetDallasBatchTimings(mailPiece.EffectiveReceivedOnEst.Value.Date);

                    string localFilePath = Path.Combine(pciRootPath, client.AppCode, mailPiece.JobId.ToString(), Common.PCIRequestFolderName, mailPiece.PDFFileName);
                    //NOTE : Here Feeder4_Sel field of MRDFRequest table contains detail about mail packages is BRE or not and Because of that we have used this over here.

                    string folderBRE = mailPiece.Feeder4_Sel == 1 ? "_BRE" : "";
                    var effectiveReceivedOnEst = mailPiece.EffectiveReceivedOnEst;

                    var dallasBatches = batchTimings.Select(t => new DallasBatch
                    {
                        AppCode = t.AppCode,
                        BatchName = t.BatchName,
                        StartDate = effectiveReceivedOnEst.Value.Date + t.StartTime,
                        EndDate = effectiveReceivedOnEst.Value.Date + t.EndTime,
                        ShippingCarrier = t.ShippingCarrier,
                        ShippingServiceType = t.ShippingServiceType
                    });

                    var currentBatch = dallasBatches.Single(b => (b.AppCode == client.AppCode && effectiveReceivedOnEst >= b.StartDate && effectiveReceivedOnEst <= b.EndDate) && b.ShippingCarrier == mailPiece.ShippingCarrier && b.ShippingServiceType == mailPiece.ShippingServiceType);
                    string submitDir = string.Empty;

                    if (client.AppCode == "HSBC" && mailPiece.TotalPages >= 65) //NOTE : Here for HSBC System is separating packages which have pages more than 65 pages with priority folder
                    {
                        submitDir = Path.Combine(printTrailDetail.FromADEDir, client.AppCode, effectiveReceivedOnEst.Value.Date.ToString("MMddyyyy"), currentBatch.BatchName, currentBatch.ShippingCarrier + "_" + currentBatch.ShippingServiceType + folderBRE, "PRIORITY");
                    }
                    else
                    {
                        submitDir = Path.Combine(printTrailDetail.FromADEDir, client.AppCode, effectiveReceivedOnEst.Value.Date.ToString("MMddyyyy"), currentBatch.BatchName, currentBatch.ShippingCarrier + "_" + currentBatch.ShippingServiceType + folderBRE);
                    }

                    if (!sftSite.IsDownloadDirectoryAvailable(submitDir))
                        sftSite.CreateDirectory(submitDir);

                    sftSite.UploadFile(localFilePath, Path.Combine(submitDir, mailPiece.PDFFileName));
                }
            }
        }

        #endregion

        #region Download Response MRDF and update batch status in DB.
        public static void GetPrintVendorResponse(string printVendorCodes)
        {
            log.Debug("Print vendor response download job started.");

            string[] vendorCodes = printVendorCodes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string printVendorCode in vendorCodes)
            {
                var printVendorDetail = DAL.GetPrintVendorDetailByVendorCode(printVendorCode);
                List<SubmittedBatches> submittedPrintBatches;
                submittedPrintBatches = DAL.GetSubmittedBatches(printVendorCode);
                if (submittedPrintBatches.Count == 0)
                {
                    log.Debug("Print vendor - {0}'s job queue is empty.".Fill(printVendorCode));
                    continue;
                }

                SFTUtil sftSite = new SFTUtil(printVendorDetail.Protocol.ParseEnum<SFT_ProtocolType>(), printVendorDetail.Host, printVendorDetail.UserName, printVendorDetail.Password, 60000);

                sftSite.Connect();

                CheckAndDownloadResponseFile(sftSite, submittedPrintBatches, printVendorDetail);

                sftSite.Disconnect();

                log.Debug("MRDF download job ended.");
            }
        }

        private static void CheckAndDownloadResponseFile(SFTUtil sftSite, List<SubmittedBatches> submittedPrintBatches, PrintVendor printVendorDetail)
        {
            List<string> responseFiles;
            responseFiles = sftSite.ListDirectory(printVendorDetail.ToADEDir);

            string printVendorResponseType = string.Empty; // This will be used for downloading main file format from print vendor's SFT site.
            string printVendorResponseFileExtention = string.Empty;// This will be used for looking for file extention format from provided zip file.

            switch (printVendorDetail.VendorCode)
            {
                case "PCI":
                    printVendorResponseType = ".zip";
                    printVendorResponseFileExtention = "OUT";
                    break;
                case "NORDIS":
                    printVendorResponseType = ".zip";
                    printVendorResponseFileExtention = "xml";
                    break;
                default:
                    break;
            }

            if (string.IsNullOrEmpty(printVendorResponseType) || string.IsNullOrEmpty(printVendorResponseFileExtention))
                return;

            var printVendorResponseFiles = responseFiles
                .Where(file => Path.GetExtension(file).ToLower() == printVendorResponseType)
                .Select(file => new KeyValuePair<string, string>(Path.GetFileNameWithoutExtension(file), file))
                .ToList();

            foreach (var batch in submittedPrintBatches)
            {
                batch.ReceivedFileList.AddRange(printVendorResponseFiles.Where(resfile => resfile.Key.StartsWith(batch.PrintBatchName, StringComparison.CurrentCultureIgnoreCase)).Select(resfile => resfile.Value));
            }

            var downloadableFiles = submittedPrintBatches.Where(c => c.ReceivedFileList != null && c.ReceivedFileList.Count > 0).ToList();

            if (downloadableFiles.Count == 0)
            {
                log.Debug("Print vendor {0}'s responses are not available for download.".Fill(printVendorDetail.VendorCode));
                return;
            }

            #region Download MRDF

            foreach (var downloadableBatch in downloadableFiles)
            {
                log.Debug("Downloading print response For Print Trail Batch-ID : " + downloadableBatch.PrintTrailBatchId.ToString());
                foreach (string filePath in downloadableBatch.ReceivedFileList)
                {
                    log.Debug("Downloading Print Vendor Response Job Started For Batch {0}".Fill(Path.GetFileNameWithoutExtension(filePath)));

                    string localPrintVendorResponseZipFilePath = Path.Combine(Common.PCIRootFolder, filePath);
                    string localPrintVendorResponseFilePath = Path.Combine(Common.PCIRootFolder, "{0}.{1}".Fill(Path.GetFileNameWithoutExtension(localPrintVendorResponseZipFilePath), printVendorResponseFileExtention));
                    string remoteVendorResponsePath = Path.Combine(printVendorDetail.ToADEDir, filePath);

                    DateTime responseReceivedOn = sftSite.LastModifiedTimeOfFile(remoteVendorResponsePath);
                    DateTime responseDownloadStartTime = Common.GetCurrentTime();

                    bool responseDelete = printVendorDetail.PostDownloadAction == "Archive" ? false : true;

                    sftSite.DownloadFile(remoteVendorResponsePath, localPrintVendorResponseZipFilePath, responseDelete, false);

                    if (!responseDelete)
                    {
                        sftSite.MoveFile(remoteVendorResponsePath, Path.Combine(printVendorDetail.ToADEDir, "Archive", filePath));
                    }

                    using (ZipArchive archive = ZipFile.OpenRead(localPrintVendorResponseZipFilePath))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            if (entry.FullName.ToUpper().EndsWith(".{0}".Fill(printVendorResponseFileExtention), StringComparison.OrdinalIgnoreCase))
                            {
                                entry.ExtractToFile(localPrintVendorResponseFilePath, true);
                            }
                        }
                    }

                    IdentifyAndUpdateBatchStatus(localPrintVendorResponseZipFilePath, localPrintVendorResponseFilePath, responseReceivedOn, downloadableBatch.PrintTrailBatchId);

                    //if (bi != null)
                    //{
                    //    DAL.LogBatchTimeLine(bi.BatchSummary.PrintBatch, LogBatchTimeLine.MRDFDownloadStart, mrdfDownloadStartTime);
                    //    DAL.LogBatchTimeLine(bi.BatchSummary.PrintBatch, LogBatchTimeLine.MRDFDownloadEnd, Common.GetCurrentTime());
                    //}
                }
            }

            #endregion
        }

        private static void IdentifyAndUpdateBatchStatus(string localResponseZipFilePath, string localResponseFilePath, DateTime responseReceivedOn, int printTrailBatchId)
        {
            string fileName = Path.GetFileNameWithoutExtension(localResponseFilePath);
            string outFileName = Path.GetFileNameWithoutExtension(localResponseFilePath);

            PrintTrailBatch printBatchDetail = DAL.GetPrintBatchDetailByPrintBatchTrailId(printTrailBatchId);

            if (printBatchDetail != null)
            {
                var bundlingRootPath = Path.Combine(Common.BundlingRootFolder, printTrailBatchId.ToString(), Common.BundlingResponseFolderName);

                if (!Directory.Exists(bundlingRootPath))
                {
                    bundlingRootPath.CreateDirectoryIfNotExist();
                }

                if (File.Exists(Path.Combine(bundlingRootPath, Path.GetFileName(localResponseZipFilePath))))
                    File.Delete(Path.Combine(bundlingRootPath, Path.GetFileName(localResponseZipFilePath)));

                //This will move zip file from root folder of pci response folder also.
                File.Move(localResponseZipFilePath, Path.Combine(bundlingRootPath, Path.GetFileName(localResponseZipFilePath)));


                if (File.Exists(Path.Combine(bundlingRootPath, Path.GetFileName(localResponseFilePath))))
                    File.Delete(Path.Combine(bundlingRootPath, Path.GetFileName(localResponseFilePath)));

                File.Move(localResponseFilePath, Path.Combine(bundlingRootPath, Path.GetFileName(localResponseFilePath)));

                log.DebugFormat("Response moved in proper batch folder : {0} after batch identified.", printTrailBatchId);
            }
        }

        #endregion

        #region Return PDF Zip File Logic
        //public static void GetReturnPDFsZip()
        //{
        //    log.Debug("Return PDF job started.");
        //    List<SubmittedBatches> mrdfReceivedPrintBatches;
        //    mrdfReceivedPrintBatches = DAL.GetMRDFReceivedBatches();
        //    if (mrdfReceivedPrintBatches.Count == 0)
        //    {
        //        log.Debug("Return PDF job queue is empty.");
        //    }
        //    else
        //    {
        //        SFTUtil sftSite = new SFTUtil(Common.PCI_SFT_Protocol, Common.PCI_SFT_Host, Common.PCI_SFT_Username, Common.PCI_SFT_Password, 300000);

        //        sftSite.Connect();

        //        CheckAndDownloadReturnPDFZipFile(sftSite, mrdfReceivedPrintBatches);

        //        sftSite.Disconnect();
        //    }

        //    log.Debug("MRDF download job ended.");
        //}

        //private static void CheckAndDownloadReturnPDFZipFile(SFTUtil sftSite, List<SubmittedBatches> mrdfReceivedPrintBatches)
        //{
        //    List<string> zipFiles;
        //    zipFiles = sftSite.ListDirectory(Common.PCI_SFT_ReturnPDFDir);

        //    var retZipFiles = zipFiles
        //        .Where(file => Path.GetExtension(file).ToLower() == ".zip")
        //        .Select(file => new KeyValuePair<string, string>(Path.GetFileNameWithoutExtension(file), file))
        //        .ToList();

        //    foreach (var batch in mrdfReceivedPrintBatches)
        //    {
        //        batch.ReceivedFileList.AddRange(retZipFiles.Where(zipfile => zipfile.Key.StartsWith(batch.PrintBatchName, StringComparison.CurrentCultureIgnoreCase)).Select(zipfile => zipfile.Value));
        //    }

        //    var downloadableFiles = mrdfReceivedPrintBatches.Where(c => c.ReceivedFileList != null && c.ReceivedFileList.Count > 0).ToList();

        //    if (downloadableFiles.Count == 0)
        //    {
        //        log.Debug("Return PDFs zip files are not available for download.");
        //    }
        //    else
        //    {
        //        foreach (var downloadableBatch in downloadableFiles)
        //        {
        //            log.Debug("Downloading return PDFs zip For Batch-ID : " + downloadableBatch.PrintTrailBatchId.ToString());
        //            foreach (string fileName in downloadableBatch.ReceivedFileList)
        //            {
        //                log.Debug("Downloading PCI Return PDF Job Started For Batch {0}".Fill(Path.GetFileNameWithoutExtension(fileName)));

        //                //Reason for creating ReturnPDFs in PCI root folder because we are currenly processing return MRDF file in same root folder and naming convention of its zip file is same as return PDFs' zip file.
        //                string localPDFsZipFilePath = Path.Combine(Common.PCIRootReturnPDFsZipFolder, fileName);

        //                string remoteZipPath = Path.Combine(Common.PCI_SFT_ReturnPDFDir, fileName);
        //                DateTime zipReceivedOn = sftSite.LastModifiedTimeOfFile(remoteZipPath);

        //                sftSite.DownloadFile(localPDFsZipFilePath, remoteZipPath, Common.Remove_PDF_Of_PCI_SFT, false);

        //                IdentifyAndMoveZip(localPDFsZipFilePath, zipReceivedOn, downloadableBatch.PrintTrailBatchId);
        //            }
        //        }
        //    }
        //}

        //private static void IdentifyAndMoveZip(string localPDFsZipFilePath, DateTime zipReceivedOn, int printBatchId)
        //{
        //    BatchSummary bs = DAL.GetBatchSummaryById(printBatchId);

        //    if (bs != null)
        //    {
        //        Client app = DAL.GetClientAppDetailsById(bs.ClientId);
        //        BatchInfo bi = new BatchInfo(bs, app);

        //        var docTypesMaps = DAL.GetDocTypeDetailsByAppId(app.Id).Where(x => x.DocTypeId == bi.BatchSummary.DocTypeId).First();
        //        if (docTypesMaps != null)
        //            bi.InitializeDocTypeMaps(docTypesMaps, bi.BatchSummary.PrintBatch, bi.Client.AppCode, bi.Client.CustCode);

        //        if (!Directory.Exists(bi.BatchRepo))
        //        {
        //            string errorMsg = string.Format("Local directory does not exists for the batch : {0} for {1}", printBatchId, localPDFsZipFilePath);
        //            log.ErrorFormat(errorMsg);
        //        }
        //        else
        //        {
        //            if (!Directory.Exists(bi.PCIResponseReturnPDFsPath))
        //                Directory.CreateDirectory(bi.PCIResponseReturnPDFsPath);

        //            string batchFolderZipFilePath = Path.Combine(bi.PCIResponseReturnPDFsPath, Path.GetFileName(localPDFsZipFilePath));

        //            if (File.Exists(batchFolderZipFilePath))
        //                File.Delete(batchFolderZipFilePath);

        //            //This will move zip file from root folder of pci response folder also.
        //            File.Move(localPDFsZipFilePath, batchFolderZipFilePath);

        //            //After moving to return pdf
        //            using (ZipArchive archive = ZipFile.OpenRead(batchFolderZipFilePath))
        //            {
        //                foreach (ZipArchiveEntry entry in archive.Entries)
        //                {
        //                    if (entry.FullName.ToUpper().EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        //                    {
        //                        entry.ExtractToFile(Path.Combine(bi.PCIResponseReturnPDFsPath, Path.GetFileName(entry.FullName)), true);
        //                    }
        //                }
        //            }

        //            log.DebugFormat("Return PDFs zip is moved in proper batch folder : {0} after batch identified.", printBatchId);
        //        }
        //    }
        //}
        #endregion
    }
}
