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
        public static void SubmitBatchToPCI(string printTrailNames)
        {

            //TODO : Sequence Change 
            string[] printTrails = printTrailNames.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var pieceIdFormat = MRDFRequestSchema.Single(f => f.FieldName == "PieceId").DataFormat;
            foreach (string printTrailName in printTrails)
            {


                var printTrailDetail = DAL.GetPrintTrailDetailByPrintTrailName(printTrailName);

                if (printTrailDetail.PrintVendorName == Common.PrintLocationPCI)
                {
                    DateTime startDateTime = Common.EstToUtc(Common.EstToday);
                    DateTime toDateTime = Common.GetCurrentTime();
                    DateTime endDateTime = new DateTime(toDateTime.Year, toDateTime.Month, toDateTime.Day, toDateTime.Hour, toDateTime.Minute, 0);

                    //Waiting Code for AuthoMail as well as BatchSummary
                    while (!DAL.CheckAllBatchStatusCompleted(startDateTime, endDateTime))
                    {
                        Thread.Sleep(6000);
                    }
                }

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
                                if (mp.MailPieceStatus == MailPieceStatus.PulledBack.ToString() || mp.MailPieceStatus == MailPieceStatus.Error.ToString())
                                    mp.SequenceNo = -1;
                                else
                                {
                                    mp.SequenceNo = sequenceNo++;
                                    mp.Address1 = "Job: {0} Seq: {1}".Fill(printTrailBatchDetail.Id, pieceIdFormat.Fill(mp.SequenceNo));
                                }
                            });

                            var maxSequenceNo = mailPieceDetails.Max(p => p.SequenceNo);

                            mailPieceDetails.ForEach(mrdf =>
                            {
                                if (mrdf.SequenceNo == -1)
                                {
                                    mrdf.SequenceNo = ++maxSequenceNo;
                                    mrdf.Address1 = "Job: {0} Seq: {1}".Fill(printTrailBatchDetail.Id, pieceIdFormat.Fill(mrdf.SequenceNo));
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

                    var validMailPieceDetails = mailPieceDetails.Where(mp => mp.MailPieceStatus == MailPieceStatus.InProgress.ToString()).ToList();

                    //NOTE: Bundling folder structure will be like : BundlingRoot/PrintTrailBatchId/Request & Response
                    var pciRequestPath = Path.Combine(Common.BundlingRootFolder, printTrailBatchDetail.Id.ToString(), Common.BundlingRequestFolderName);
                    var pciResponsePath = Path.Combine(Common.BundlingRootFolder, printTrailBatchDetail.Id.ToString(), Common.BundlingResponseFolderName);

                    pciRequestPath.CreateDirectoryIfNotExist();
                    Thread.Sleep(1000);
                    pciResponsePath.CreateDirectoryIfNotExist();

                    if (Common.ApplicationEnvironment == Env.Prod || printTrailDetail.SimulateTrail == false)
                    {
                        if (printTrailDetail.PrintVendorName == Common.PrintLocationCL)
                        {
                            UploadBatchesToInternalPrintAndShip(validMailPieceDetails);
                        }
                        else
                        {
                            var zipFilePath = Path.Combine(pciRequestPath, printTrailBatchDetail.PrintBatchName + ".zip");
                            var mrdfFilePath = Path.Combine(pciRequestPath, printTrailBatchDetail.PrintBatchName + ".dat");

                            CreateZipAndUploadPCIBatch(validMailPieceDetails, printTrailBatchDetail, zipFilePath, mrdfFilePath);

                            //After complete uploading send email to printing team
                            MailUtility.SendBatchSubmittedNotificationEmail(printTrailDetail.PrintVendorName, zipFilePath, validMailPieceDetails.Count, validMailPieceDetails.Sum(c => c.TotalPages));
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

        private static void CreateZipAndUploadPCIBatch(List<MailPieceDetail> mailPieceDetails, PrintTrailBatch printTrailBatchDetail, string zipFilePath, string mrdfFilePath)
        {
            var mrdfContent = new StringBuilder();

            foreach (MailPieceDetail row in mailPieceDetails)
            {
                foreach (var f in MRDFRequestSchema)
                {
                    if (f.FieldName == "JobId")
                    {
                        mrdfContent.AppendFormat(f.DataFormat, row.GetType().GetProperty("PrintTrailBatchId").GetValue(row, null));
                    }
                    else
                    {
                        mrdfContent.AppendFormat(f.DataFormat, row.GetType().GetProperty(f.FieldName).GetValue(row, null));
                    }
                }
                mrdfContent.AppendLine();
            }

            File.WriteAllText(mrdfFilePath, mrdfContent.ToString());

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

            SFTUtil sftSite = new SFTUtil(Common.PCI_SFT_Protocol, Common.PCI_SFT_Host, Common.PCI_SFT_Username, Common.PCI_SFT_Password,
                300000);

            sftSite.Connect();

            UploadFileOnPCISFTSite(zipFilePath, sftSite, Common.PCI_SFT_SubmitDir);

            sftSite.Disconnect();
        }

        private static void UploadBatchesToInternalPrintAndShip(List<MailPieceDetail> mailPieceDetails)
        {
            SFTUtil sftSite = new SFTUtil(Common.CL_SFT_Protocol, Common.CL_SFT_Host, Common.CL_SFT_Username, Common.CL_SFT_Password,
                300000);

            sftSite.Connect();
            var sw = new Stopwatch();
            sw.Start();
            UploadFileOnInternalPrintingSFTSite(Common.PCIRootFolder, sftSite, mailPieceDetails);
            sw.Stop();
            var time = sw.Elapsed;
            //log.DebugFormat("Total time spent : {0} For Batch : ", time, Path.GetFileNameWithoutExtension(bi.BatchSummary.PrintFileName));
            sftSite.Disconnect();
        }

        private static void UploadFileOnPCISFTSite(string localFilePath, SFTUtil sftSite, string submitDir)
        {
            var sw = new Stopwatch();
            sw.Start();

            sftSite.UploadFile(localFilePath, string.Format("{0}/{1}.uploading", submitDir, Path.GetFileNameWithoutExtension(localFilePath)), false, false);
            sftSite.RenameFile(string.Format("{0}/{1}.uploading", submitDir, Path.GetFileNameWithoutExtension(localFilePath)), string.Format("{0}/{1}.zip", submitDir, Path.GetFileNameWithoutExtension(localFilePath)));

            sw.Stop();
            var time = sw.Elapsed;
            log.DebugFormat("Total time spent : {0} For Batch : ", time, Path.GetFileNameWithoutExtension(localFilePath));
        }

        private static void UploadFileOnInternalPrintingSFTSite(string pciRootPath, SFTUtil sftSite, List<MailPieceDetail> mailPiecesDetails)
        {
            var batchTimings = DAL.GetDallasBatchTimings(Common.EstToday);

            //Reason for doing this because we can avoid multiple call from database for each mail piece it may contains multiple clients for same print trails

            var clientIds = mailPiecesDetails.Select(m => m.ClientId).Distinct();
            foreach (var clientId in clientIds)
            {
                Client client = DAL.GetClientAppDetailsById(clientId);

                var mailPiecesByClient = mailPiecesDetails.Where(c => c.ClientId == clientId);

                foreach (MailPieceDetail mailPiece in mailPiecesByClient)
                {
                    string localFilePath = Path.Combine(pciRootPath, client.AppCode, mailPiece.JobId.ToString(), Common.PCIRequestFolderName, mailPiece.PDFFileName);
                    //NOTE : Here Feeder4_Sel field of MRDFRequest table contains detail about mail packages is BRE or not and Because of that we have used this over here.

                    string folderBRE = mailPiece.Feeder4_Sel == 1 ? "_BRE" : "";
                    var effectiveReceivedOnEst = mailPiece.EffectiveReceivedOnEst;

                    var dallasBatches = batchTimings.Select(t => new DallasBatch
                    {
                        AppCode = t.AppCode,
                        BatchName = t.BatchName,
                        StartDate = effectiveReceivedOnEst.Date + t.StartTime,
                        EndDate = effectiveReceivedOnEst.Date + t.EndTime,
                        ShippingCarrier = t.ShippingCarrier,
                        ShippingServiceType = t.ShippingServiceType
                    });

                    var currentBatch = dallasBatches.Single(b => (b.AppCode == client.AppCode && effectiveReceivedOnEst >= b.StartDate && effectiveReceivedOnEst <= b.EndDate) && b.ShippingCarrier == mailPiece.ShippingCarrier && b.ShippingServiceType == mailPiece.ShippingServiceType);

                    string submitDir = Path.Combine(Common.CL_SFT_SubmitDir, client.AppCode, effectiveReceivedOnEst.Date.ToString("MMddyyyy"), currentBatch.BatchName, currentBatch.ShippingCarrier + "_" + currentBatch.ShippingServiceType + folderBRE);

                    if (!sftSite.IsDirectoryAvailable(submitDir))
                        sftSite.CreateDirectory(submitDir);

                    sftSite.UploadFile(localFilePath, Path.Combine(submitDir, mailPiece.PDFFileName));
                }
            }
        }

        #endregion

        #region Download Response MRDF and update batch status in DB.
        public static void GetResponseMRDF()
        {
            log.Debug("MRDF download job started.");
            List<SubmittedBatches> submittedPrintBatches;
            submittedPrintBatches = DAL.GetSubmittedBatches();
            if (submittedPrintBatches.Count == 0)
            {
                log.Debug("MRDF download job queue is empty.");
            }
            else
            {
                SFTUtil sftSite = new SFTUtil(Common.PCI_SFT_Protocol, Common.PCI_SFT_Host, Common.PCI_SFT_Username, Common.PCI_SFT_Password, 300000);

                sftSite.Connect();

                CheckAndDownloadMRDFOUTFile(sftSite, submittedPrintBatches);

                sftSite.Disconnect();
            }

            log.Debug("MRDF download job ended.");
        }

        private static void CheckAndDownloadMRDFOUTFile(SFTUtil sftSite, List<SubmittedBatches> submittedPrintBatches)
        {
            List<string> outFiles;
            outFiles = sftSite.ListDirectory(Common.PCI_SFT_ResponseDir);

            var mrdfOutFiles = outFiles
                .Where(file => Path.GetExtension(file).ToLower() == ".zip")
                .Select(file => new KeyValuePair<string, string>(Path.GetFileNameWithoutExtension(file), file))
                .ToList();

            foreach (var batch in submittedPrintBatches)
            {
                batch.ReceivedFileList.AddRange(mrdfOutFiles.Where(outfile => outfile.Key.StartsWith(batch.PrintBatchName, StringComparison.CurrentCultureIgnoreCase)).Select(outfile => outfile.Value));
            }

            var downloadableFiles = submittedPrintBatches.Where(c => c.ReceivedFileList != null && c.ReceivedFileList.Count > 0).ToList();

            if (downloadableFiles.Count == 0)
            {
                log.Debug("MRDF files are not available for download.");
            }
            else
            {
                #region Download MRDF

                foreach (var downloadableBatch in downloadableFiles)
                {
                    log.Debug("Downloading MRDF.OUT For Batch-ID : " + downloadableBatch.PrintTrailBatchId.ToString());
                    foreach (string filePath in downloadableBatch.ReceivedFileList)
                    {
                        log.Debug("Downloading PCI Response Job Started For Batch {0}".Fill(Path.GetFileNameWithoutExtension(filePath)));
                        string localMRDFResponseZipFilePath = Path.Combine(Common.PCIRootFolder, filePath);
                        string localMRDFResponseFilePath = Path.Combine(Common.PCIRootFolder, string.Format("{0}.OUT", Path.GetFileNameWithoutExtension(localMRDFResponseZipFilePath)));
                        string remoteMRDFPath = Path.Combine(Common.PCI_SFT_ResponseDir, filePath);
                        DateTime mrdfReceivedOn = sftSite.LastModifiedTimeOfFile(remoteMRDFPath);
                        DateTime mrdfDownloadStartTime = Common.GetCurrentTime();
                        sftSite.DownloadFile(localMRDFResponseZipFilePath, remoteMRDFPath, Common.Remove_MRDFOUT_Of_PCI_SFT, false);

                        if (!Common.Remove_MRDFOUT_Of_PCI_SFT)
                        {
                            // Comment below line during production.
                            sftSite.MoveFile(remoteMRDFPath, Path.Combine(Common.PCI_SFT_ResponseDir, "Archive", filePath));
                        }

                        using (ZipArchive archive = ZipFile.OpenRead(localMRDFResponseZipFilePath))
                        {
                            foreach (ZipArchiveEntry entry in archive.Entries)
                            {
                                if (entry.FullName.ToUpper().EndsWith(".OUT", StringComparison.OrdinalIgnoreCase))
                                {
                                    entry.ExtractToFile(localMRDFResponseFilePath, true);
                                }
                            }
                        }

                        IdentifyAndUpdateBatchStatus(localMRDFResponseZipFilePath, localMRDFResponseFilePath, mrdfReceivedOn, downloadableBatch.PrintTrailBatchId);

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
        }

        private static void IdentifyAndUpdateBatchStatus(string localMRDFResponseZipFilePath, string localMRDFResponseFilePath, DateTime mrdfReceivedOn, int printTrailBatchId)
        {
            string fileName = Path.GetFileNameWithoutExtension(localMRDFResponseFilePath);
            string outFileName = Path.GetFileNameWithoutExtension(localMRDFResponseFilePath);

            PrintTrailBatch printBatchDetail = DAL.GetPrintBatchDetailByPrintBatchTrailId(printTrailBatchId);

            if (printBatchDetail != null)
            {
                var bundlingRootPath = Path.Combine(Common.BundlingRootFolder, printTrailBatchId.ToString(), Common.BundlingResponseFolderName);

                if (!Directory.Exists(bundlingRootPath))
                {
                    bundlingRootPath.CreateDirectoryIfNotExist();
                }

                if (File.Exists(Path.Combine(bundlingRootPath, Path.GetFileName(localMRDFResponseZipFilePath))))
                    File.Delete(Path.Combine(bundlingRootPath, Path.GetFileName(localMRDFResponseZipFilePath)));

                //This will move zip file from root folder of pci response folder also.
                File.Move(localMRDFResponseZipFilePath, Path.Combine(bundlingRootPath, Path.GetFileName(localMRDFResponseZipFilePath)));


                if (File.Exists(Path.Combine(bundlingRootPath, Path.GetFileName(localMRDFResponseFilePath))))
                    File.Delete(Path.Combine(bundlingRootPath, Path.GetFileName(localMRDFResponseFilePath)));

                File.Move(localMRDFResponseFilePath, Path.Combine(bundlingRootPath, Path.GetFileName(localMRDFResponseFilePath)));

                File.Delete(localMRDFResponseFilePath);
                log.DebugFormat("Response MRDF moved in proper batch folder : {0} after batch identified.", printTrailBatchId);

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
