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

        #region SubmitBatch To PCI or Corelogic
        public static void SubmitBatchToPrintVendor(bool isBundle, string printTrailNames)
        {
            //TODO : Sequence Change 
            string[] printTrails = printTrailNames.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var pieceIdFormat = MRDFRequestSchema.Single(f => f.FieldName == "PieceId").DataFormat;

            if (isBundle)
            {
                DateTime startDateTime = Common.EstToUtc(Common.EstToday);
                DateTime toDateTime = Common.GetCurrentTime();
                DateTime endDateTime = new DateTime(toDateTime.Year, toDateTime.Month, toDateTime.Day, toDateTime.Hour, toDateTime.Minute, 0);

                //Waiting Code for AuthoMail as well as BatchSummary
                while (!DAL.CheckBatchesInQueue(startDateTime, endDateTime))
                {
                    log.Debug("Waiting to Complete Process");
                    Thread.Sleep(6000);
                }
            }

            foreach (string printTrailName in printTrails)
            {
                var printTrailDetail = DAL.GetPrintTrailDetailByPrintTrailName(printTrailName);

                var mailPieceDetails = DAL.GetMRDFRequestDataByPrintTrailId(printTrailDetail.PrintTrailId);

                if (mailPieceDetails == null || mailPieceDetails.Count == 0)
                {
                    log.Debug("Submit batches job queue is empty for PrintTrail {0}".Fill(printTrailName));
                    continue;
                }

                try
                {
                    //For divert primary key issue we are using starting number as 90001
                    var sequenceNo = 90001;
                    PrintTrailBatch printTrailBatchDetail = new PrintTrailBatch()
                    {
                        PrintTrailId = printTrailDetail.PrintTrailId
                    };

                    using (TransactionScope scope = new TransactionScope())
                    {
                        try
                        {
                            printTrailBatchDetail.Id = DAL.InsertPrintTrailBatchDetail(printTrailBatchDetail);

                            mailPieceDetails.ForEach(mp =>
                            {
                                mp.PrintTrailBatchId = printTrailBatchDetail.Id;
                                if (mp.MailPieceStatus == MailPieceStatus.PulledBack.ToString() || mp.MailPieceStatus == MailPieceStatus.Error.ToString())
                                    mp.SequenceNo = -1;
                                else
                                {
                                    mp.SequenceNo = sequenceNo++;
                                }
                            });

                            var maxSequenceNo = mailPieceDetails.Max(p => p.SequenceNo);

                            mailPieceDetails.ForEach(mrdf =>
                            {
                                if (mrdf.SequenceNo == -1)
                                {
                                    mrdf.SequenceNo = ++maxSequenceNo;
                                }
                            });

                            DAL.UpdatedPrintTrailBatchIdInMRDFRequest(printTrailBatchDetail.Id, mailPieceDetails);

                            printTrailBatchDetail.PrintBatchName = "{0}-{1}-{2}".Fill(printTrailDetail.BusinessCode, printTrailBatchDetail.Id, printTrailDetail.TrailName);

                            DAL.UpdatePrintTrailBatchDetail(printTrailBatchDetail);

                            scope.Complete();
                        }
                        catch (Exception ex)
                        {
                            var msg = "Error occured while updating print trail batch id in mrdfrequest for PrintTrail - {0}".Fill(printTrailDetail.TrailName);
                            log.Error(msg, ex);
                            MailUtility.SendUnexpectedNotificationEmail(msg + "<br/> Error Message is {0}".Fill(ex));
                            continue;
                        }
                    }

                    var validMailPieceDetails = DAL.GetMRDFRequestListByPrintTrailBatchId(printTrailBatchDetail.Id);

                    //NOTE: Bundling folder structure will be like : BundlingRoot/PrintTrailBatchId/Request & Response
                    var pciRequestPath = Path.Combine(Common.BundlingRootFolder, printTrailBatchDetail.Id.ToString(), Common.BundlingRequestFolderName);
                    var pciResponsePath = Path.Combine(Common.BundlingRootFolder, printTrailBatchDetail.Id.ToString(), Common.BundlingResponseFolderName);

                    pciRequestPath.CreateDirectoryIfNotExist();
                    Thread.Sleep(1000);
                    pciResponsePath.CreateDirectoryIfNotExist();

                    if (Common.ApplicationEnvironment == Env.Prod || printTrailDetail.SimulateTrail == false)
                    {
                        if (printTrailDetail.VendorCode == "CL_DALLAS")
                        {
                            UploadBatchesToInternalPrintAndShip(validMailPieceDetails, printTrailDetail);
                        }
                        else if (printTrailDetail.VendorCode == "PCI")
                        {
                            var zipFilePath = Path.Combine(pciRequestPath, printTrailBatchDetail.PrintBatchName + ".zip");
                            var mrdfFilePath = Path.Combine(pciRequestPath, printTrailBatchDetail.PrintBatchName + ".dat");

                            CreateZipAndUploadPCIBatch(validMailPieceDetails, printTrailBatchDetail, zipFilePath, mrdfFilePath, printTrailDetail);

                            //After complete uploading send email to printing team
                            MailUtility.SendBatchSubmittedNotificationEmail(printTrailDetail.VendorCode, zipFilePath, validMailPieceDetails.Count, validMailPieceDetails.Sum(c => c.TotalPages));
                        }
                        else if (printTrailDetail.VendorCode == "NORDIS")
                        {
                            var zipFilePath = Path.Combine(pciRequestPath, printTrailBatchDetail.PrintBatchName + ".zip");
                            var xmlFilePath = Path.Combine(pciRequestPath, printTrailBatchDetail.PrintBatchName + ".xml");

                            CreateZipAndUploadNordisBatch(validMailPieceDetails, printTrailBatchDetail, zipFilePath, xmlFilePath, printTrailDetail);

                            //After complete uploading send email to printing team
                            MailUtility.SendBatchSubmittedNotificationEmail(printTrailDetail.VendorCode, zipFilePath, validMailPieceDetails.Count, validMailPieceDetails.Sum(c => c.TotalPages));
                        }
                    }

                    //Reason for maintaining this time because it should reflect same for 
                    var currentTime = Common.GetCurrentTime();
                    using (TransactionScope scope = new TransactionScope())
                    {
                        try
                        {
                            DAL.UpdatePrintTrailBatchStatus(printTrailBatchDetail.Id, BatchPipelineStatus.Submitted, currentTime);

                            var jobIds = mailPieceDetails.Select(b => b.JobId).Distinct();
                            foreach (var jobId in jobIds)
                            {
                                DAL.UpdateBatchPipeLineStatus(jobId, BatchPipelineStatus.Submitted, currentTime);
                            }
                            scope.Complete();
                        }
                        catch (Exception ex)
                        {
                            var msg = "Error occured while updating submitted status for print trail batch id {0}".Fill(printTrailBatchDetail.Id);
                            log.Error(msg, ex);
                            MailUtility.SendUnexpectedNotificationEmail(msg + "<br/> Error Message is {0}".Fill(ex));
                        }
                    }
                }
                catch (Exception ex)
                {
                    var errorMsg = "Error occured while submitting Print Trail Name {0} to PrintVendor {1} with Error {2}".Fill(printTrailDetail.TrailName, printTrailDetail.PrintVendorName, ex);
                    log.Error(errorMsg);

                    MailUtility.SendUnexpectedNotificationEmail(errorMsg);
                    continue;
                }
            }
        }

        private static void CreateZipAndUploadNordisBatch(List<MailPieceDetail> mailPieceDetails, PrintTrailBatch printTrailBatchDetail, string zipFilePath, string xmlFilePath, PrintTrailDetail printTrailDetail)
        {
            List<PackageData> packageData = new List<PackageData>();
            foreach (MailPieceDetail package in mailPieceDetails)
            {
                packageData.Add(new PackageData() { PieceId = package.PieceId, PDFFileName = package.PDFFileName, DueDate = package.MailPieceDueDate, TotalSheets = package.TotalSheets, BRERequired = package.Feeder4_Sel == 1 ? true : false, QCReqd = package.DivertVS3 == 1 ? true : false });
            }
            BatchRequest batchRequestXml = new BatchRequest() { JobId = printTrailBatchDetail.Id, PackagesData = packageData };

            File.WriteAllText(xmlFilePath, batchRequestXml.ToXml());

            using (ZipArchive newFile = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
            {
                foreach (var mailPiece in mailPieceDetails)
                {
                    //NOTE:Here we are directly creating Zip with Bundling into another path
                    //We have folder structure like PCIRoot/AppCode/JobId/PCIRequest
                    var localFilePath = Path.Combine(Common.PCIRootFolder, mailPiece.AppCode, mailPiece.JobId.ToString(), Common.PCIRequestFolderName, mailPiece.PDFFileName);
                    newFile.CreateEntryFromFile(localFilePath, mailPiece.PDFFileName);
                }

                //make an entry for mrdf file after making entries of files into zip
                newFile.CreateEntryFromFile(xmlFilePath, Path.GetFileName(xmlFilePath));
            }
            SFTUtil sftSite = new SFTUtil(printTrailDetail.Protocol.ParseEnum<SFT_ProtocolType>(), printTrailDetail.Host, printTrailDetail.UserName, printTrailDetail.Password,
                300000);

            sftSite.Connect();

            log.DebugFormat("Uploading started For PrintTrailBatchId : {0}", printTrailBatchDetail.Id);
            UploadFileOnPrintVendorSFTSite(zipFilePath, sftSite, printTrailDetail.FromADEDir);
            log.DebugFormat("Uploading completed For PrintTrailBatchId : {0}", printTrailBatchDetail.Id);

            sftSite.Disconnect();
        }

        private static void CreateZipAndUploadPCIBatch(List<MailPieceDetail> mailPieceDetails, PrintTrailBatch printTrailBatchDetail, string zipFilePath, string mrdfFilePath, PrintTrailDetail printTrailDetail)
        {
            var mrdfContent = new StringBuilder();

            foreach (MailPieceDetail row in mailPieceDetails)
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
                foreach (var mailPiece in mailPieceDetails)
                {
                    //NOTE:Here we are directly creating Zip with Bundling into another path
                    //We have folder structure like PCIRoot/AppCode/JobId/PCIRequest
                    var localFilePath = Path.Combine(Common.PCIRootFolder, mailPiece.AppCode, mailPiece.JobId.ToString(), Common.PCIRequestFolderName, mailPiece.PDFFileName);
                    newFile.CreateEntryFromFile(localFilePath, mailPiece.PDFFileName);
                }

                //make an entry for mrdf file after making entries of files into zip
                newFile.CreateEntryFromFile(mrdfFilePath, Path.GetFileName(mrdfFilePath));
            }
            SFTUtil sftSite = new SFTUtil(printTrailDetail.Protocol.ParseEnum<SFT_ProtocolType>(), printTrailDetail.Host, printTrailDetail.UserName, printTrailDetail.Password,
                300000);

            sftSite.Connect();

            log.DebugFormat("Uploading started For PrintTrailBatchId : {0}", printTrailBatchDetail.Id);
            UploadFileOnPrintVendorSFTSite(zipFilePath, sftSite, printTrailDetail.FromADEDir);
            log.DebugFormat("Uploading completed For PrintTrailBatchId : {0}", printTrailBatchDetail.Id);

            sftSite.Disconnect();
        }

        private static void UploadFileOnPrintVendorSFTSite(string localFilePath, SFTUtil sftSite, string submitDir)
        {
            var sw = new Stopwatch();
            sw.Start();

            sftSite.UploadFile(localFilePath, string.Format("{0}/{1}.uploading", submitDir, Path.GetFileNameWithoutExtension(localFilePath)), false, false);
            sftSite.RenameFile(string.Format("{0}/{1}.uploading", submitDir, Path.GetFileNameWithoutExtension(localFilePath)), string.Format("{0}/{1}.zip", submitDir, Path.GetFileNameWithoutExtension(localFilePath)));

            sw.Stop();
            var time = sw.Elapsed;
            log.DebugFormat("Total time spent : {0} For Batch : {1}", time, Path.GetFileNameWithoutExtension(localFilePath));
        }

        private static void UploadBatchesToInternalPrintAndShip(List<MailPieceDetail> mailPieceDetails, PrintTrailDetail printTrailDetail)
        {
            SFTUtil sftSite = new SFTUtil(printTrailDetail.Protocol.ParseEnum<SFT_ProtocolType>(), printTrailDetail.Host, printTrailDetail.UserName, printTrailDetail.Password,
                300000);

            sftSite.Connect();
            var sw = new Stopwatch();
            sw.Start();
            UploadFileOnInternalPrintingSFTSite(Common.PCIRootFolder, sftSite, mailPieceDetails, printTrailDetail);
            sw.Stop();
            var time = sw.Elapsed;
            //log.DebugFormat("Total time spent : {0} For Batch : ", time, Path.GetFileNameWithoutExtension(bi.BatchSummary.PrintFileName));
            sftSite.Disconnect();
        }

        private static void UploadFileOnInternalPrintingSFTSite(string pciRootPath, SFTUtil sftSite, List<MailPieceDetail> mailPiecesDetails, PrintTrailDetail printTrailDetail)
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

                    string submitDir = Path.Combine(printTrailDetail.FromADEDir, client.AppCode, effectiveReceivedOnEst.Value.Date.ToString("MMddyyyy"), currentBatch.BatchName, currentBatch.ShippingCarrier + "_" + currentBatch.ShippingServiceType + folderBRE);

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

                SFTUtil sftSite = new SFTUtil(printVendorDetail.Protocol.ParseEnum<SFT_ProtocolType>(), printVendorDetail.Host, printVendorDetail.UserName, printVendorDetail.Password, 300000);

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
                    string remoteVendorResponsePath = Path.Combine(Common.PCI_SFT_ResponseDir, filePath);

                    DateTime responseReceivedOn = sftSite.LastModifiedTimeOfFile(remoteVendorResponsePath);
                    DateTime responseDownloadStartTime = Common.GetCurrentTime();

                    bool responseDelete = printVendorDetail.PostDownloadAction == "Archive" ? false : true;

                    sftSite.DownloadFile(localPrintVendorResponseZipFilePath, remoteVendorResponsePath, responseDelete, false);

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
            #region Original Code
            //foreach (string mrdfOutFileName in responseFiles)
            //{
            //    string localMRDFResponseZipFilePath = Path.Combine(Common.PCIRootFolder, mrdfOutFileName);
            //    string localMRDFResponseFilePath = Path.Combine(Common.PCIRootFolder, string.Format("{0}.OUT", Path.GetFileNameWithoutExtension(localMRDFResponseZipFilePath)));
            //    string remoteMRDFPath = Path.Combine(Common.PCI_SFT_ResponseDir, mrdfOutFileName);
            //    DateTime mrdfReceivedOn = sftSite.LastModifiedTimeOfFile(remoteMRDFPath);
            //    DateTime mrdfDownloadStartTime = Common.GetCurrentTime();

            //    sftSite.DownloadFile(localMRDFResponseZipFilePath, remoteMRDFPath, Common.Remove_MRDFOUT_Of_PCI_SFT, false);

            //    if (!Common.Remove_MRDFOUT_Of_PCI_SFT)
            //    {
            //        // Comment below line during production.
            //        sftSite.MoveFile(remoteMRDFPath, Path.Combine(Common.PCI_SFT_ResponseDir, "Archive", mrdfOutFileName));
            //    }
            //    using (ZipArchive archive = ZipFile.OpenRead(localMRDFResponseZipFilePath))
            //    {
            //        foreach (ZipArchiveEntry entry in archive.Entries)
            //        {
            //            if (entry.FullName.ToUpper().EndsWith(".OUT", StringComparison.OrdinalIgnoreCase))
            //            {
            //                entry.ExtractToFile(localMRDFResponseFilePath);
            //            }
            //        }
            //    }
            //    BatchInfo bi = IdentifyAndUpdateBatchStatus(localMRDFResponseZipFilePath, localMRDFResponseFilePath, mrdfReceivedOn);

            //    if (bi != null)
            //    {
            //        DAL.LogBatchTimeLine(bi.BatchSummary.PrintBatch, LogBatchTimeLine.MRDFDownloadStart, mrdfDownloadStartTime);
            //        DAL.LogBatchTimeLine(bi.BatchSummary.PrintBatch, LogBatchTimeLine.MRDFDownloadEnd, Common.GetCurrentTime());
            //    }
            //} 
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
