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

namespace PCI.Service
{
    public static class PCIJobMonitor
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PCIJobMonitor));
        private static Database db = DatabaseFactory.CreateDatabase();



        #region SubmitBatch To PCI or Corelogic
        public static void SubmitBatchToPCI()
        {
            List<int> batchIds = DAL.GetNextBatchIdByJobName(JobTypes.PCIJobMonitor, OperationTypes.Submit);

            if (batchIds == null || batchIds.Count == 0)
            {
                log.Debug("Submit batches job queue is empty.");
                return;
            }

            foreach (var batchId in batchIds)
            {
                BatchSummary bs = DAL.GetBatchSummaryById(batchId);
                try
                {
                    Client client = DAL.GetClientAppDetailsById(bs.ClientId);
                    BatchInfo bi = new BatchInfo(bs, client);
                    List<MailPieceReceivedOnData> mailPiecesReceivedOnData = DAL.GetMailBatchReceivedOnDataBasedFromPrintBatchId(bi.BatchSummary.PrintBatch);

                    if (!Directory.Exists(bi.BatchRepo))
                    {
                        throw new InvalidOperationException("Batch folder does not exist");
                    }

                    log.Debug("Submitting Batch To PCI Job Started For Batch {0}".Fill(bi.BatchName));

                    var docTypesMaps = DAL.GetDocTypeDetailsByAppId(client.Id).Where(x => x.DocTypeId == bi.BatchSummary.DocTypeId).First();
                    if (docTypesMaps != null)
                        bi.InitializeDocTypeMaps(docTypesMaps, bi.BatchSummary.PrintBatch, bi.Client.AppCode, bi.Client.CustCode);

                    //Get count for inprogress mail pieces
                    var mailPiecesCount = DAL.GetMRDFRequestDataByJobId(bi.BatchSummary.PrintBatch, MailPieceStatus.InProgress.ToString()).Rows.Count;
                    #region Upload batch to PCI

                    if (Directory.Exists(bi.PCIRequestPath))
                    {
                        if (bi.RequestMRDFPath.IsFileExists())
                        {
                            DAL.LogBatchTimeLine(bi.BatchSummary.PrintBatch, LogBatchTimeLine.SubmitToPCIStart, Common.GetCurrentTime());
                            if (!bi.Client.SimulatePCI || Common.ApplicationEnvironment == Env.Prod)
                            {
                                if (bi.BatchSummary.PrintLocation == Common.PrintLocationCL)
                                {
                                    UploadBatchesToInternalPrintAndShip(bi, mailPiecesReceivedOnData);
                                }
                                else
                                {
                                    CreateZipAndUploadPCIBatch(bi);
                                    //After complete uploading send email to printing team
                                    MailUtility.SendBatchSubmittedNotificationEmail(bi, mailPiecesCount);
                                }
                            }

                            DAL.UpdateBatchPipeLineStatus(batchId, BatchPipelineStatus.Submitted, Common.GetCurrentTime());
                            DAL.LogBatchTimeLine(bi.BatchSummary.PrintBatch, LogBatchTimeLine.SubmitToPCIEnd, Common.GetCurrentTime());
                        }
                        else
                        {
                            throw new Exception("Not all three files available to make PCI request. (Merged PDF, MRDF and Summary).");
                        }
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    var errorMsg = "Error occured while submitting batch id {0} to PrintVendor {1} with Error {2}".Fill(batchId, bs.PrintLocation, ex);
                    log.Error(errorMsg);

                    MailUtility.SendUnexpectedNotificationEmail(errorMsg);
                    continue;
                }
            }
        }

        private static void CreateZipAndUploadPCIBatch(BatchInfo bi)
        {
            if (File.Exists(bi.BatchZipFilePath))
            {
                File.Delete(bi.BatchZipFilePath);
            }
            using (ZipArchive newFile = ZipFile.Open(bi.BatchZipFilePath, ZipArchiveMode.Create))
            {
                foreach (string file in Directory.GetFiles(bi.PCIRequestPath).Where(fileType => !fileType.ToLower().EndsWith(".zip")))
                {
                    newFile.CreateEntryFromFile(file, Path.GetFileName(file));
                }
            }

            SFTUtil sftSite = new SFTUtil(Common.PCI_SFT_Protocol, Common.PCI_SFT_Host, Common.PCI_SFT_Username, Common.PCI_SFT_Password,
                300000);

            sftSite.Connect();

            UploadFileOnPCISFTSite(bi.BatchZipFilePath, sftSite, Common.PCI_SFT_SubmitDir);

            sftSite.Disconnect();
        }

        private static void UploadBatchesToInternalPrintAndShip(BatchInfo bi, List<MailPieceReceivedOnData> mailPiecesReceivedOnData)
        {
            SFTUtil sftSite = new SFTUtil(Common.CL_SFT_Protocol, Common.CL_SFT_Host, Common.CL_SFT_Username, Common.CL_SFT_Password,
                300000);

            sftSite.Connect();
            var sw = new Stopwatch();
            sw.Start();
            UploadFileOnInternalPrintingSFTSite(bi.PCIRequestPath, sftSite, mailPiecesReceivedOnData, bi.DocType, bi.Client);
            sw.Stop();
            var time = sw.Elapsed;
            log.DebugFormat("Total time spent : {0} For Batch : ", time, Path.GetFileNameWithoutExtension(bi.BatchSummary.PrintFileName));
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

        private static void UploadFileOnInternalPrintingSFTSite(string batchRootPath, SFTUtil sftSite, List<MailPieceReceivedOnData> mailPiecesReceivedOnData, DocType docType, Client client)
        {

            var batchTimings = DAL.GetDallasBatchTimings(Common.EstToday);
            foreach (MailPieceReceivedOnData mailPiece in mailPiecesReceivedOnData)
            {
                var effectiveReceivedOnEst = mailPiece.EffectiveReceivedOnEst;
                var dallasBatches = batchTimings.Select(t => new DallasBatch
                {
                    BatchName = t.BatchName,
                    StartDate = effectiveReceivedOnEst.Date + t.StartTime,
                    EndDate = effectiveReceivedOnEst.Date + t.EndTime,
                    ShippingCarrier = t.ShippingCarrier,
                    ShippingServiceType = t.ShippingServiceType
                });

                var currentBatch = dallasBatches.Single(b => (effectiveReceivedOnEst >= b.StartDate && effectiveReceivedOnEst <= b.EndDate) && b.ShippingCarrier == docType.ShippingCarrier && b.ShippingServiceType == docType.ShippingServiceType);

                string submitDir = Path.Combine(Common.CL_SFT_SubmitDir, client.AppCode, effectiveReceivedOnEst.Date.ToString("MMddyyyy"), currentBatch.BatchName, currentBatch.ShippingCarrier + "_" + currentBatch.ShippingServiceType);

                string localFilePath = Path.Combine(batchRootPath, mailPiece.MailPieceFileName);

                if (!sftSite.IsDirectoryAvailable(submitDir))
                    sftSite.CreateDirectory(submitDir);

                sftSite.UploadFile(localFilePath, Path.Combine(submitDir, Path.GetFileName(mailPiece.MailPieceFileName)));
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
                batch.ReceivedFileList.AddRange(mrdfOutFiles.Where(outfile => outfile.Key.StartsWith(batch.PatternBatchCode, StringComparison.CurrentCultureIgnoreCase)).Select(outfile => outfile.Value));
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
                    log.Debug("Downloading MRDF.OUT For Batch-ID : " + downloadableBatch.PrintBatchId.ToString());
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

                        BatchInfo bi = IdentifyAndUpdateBatchStatus(localMRDFResponseZipFilePath, localMRDFResponseFilePath, mrdfReceivedOn, downloadableBatch.PrintBatchId);

                        if (bi != null)
                        {
                            DAL.LogBatchTimeLine(bi.BatchSummary.PrintBatch, LogBatchTimeLine.MRDFDownloadStart, mrdfDownloadStartTime);
                            DAL.LogBatchTimeLine(bi.BatchSummary.PrintBatch, LogBatchTimeLine.MRDFDownloadEnd, Common.GetCurrentTime());
                        }
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

        private static BatchInfo IdentifyAndUpdateBatchStatus(string localMRDFResponseZipFilePath, string localMRDFResponseFilePath, DateTime mrdfReceivedOn, int printBatchId)
        {
            string fileName = Path.GetFileNameWithoutExtension(localMRDFResponseFilePath);
            string outFileName = Path.GetFileNameWithoutExtension(localMRDFResponseFilePath);

            BatchSummary bs = DAL.GetBatchSummaryById(printBatchId);

            if (bs != null)
            {
                Client app = DAL.GetClientAppDetailsById(bs.ClientId);
                BatchInfo bi = new BatchInfo(bs, app);

                var docTypesMaps = DAL.GetDocTypeDetailsByAppId(app.Id).Where(x => x.DocTypeId == bi.BatchSummary.DocTypeId).First();
                if (docTypesMaps != null)
                    bi.InitializeDocTypeMaps(docTypesMaps, bi.BatchSummary.PrintBatch, bi.Client.AppCode, bi.Client.CustCode);

                if (!Directory.Exists(bi.BatchRepo))
                {
                    string errorMsg = string.Format("Local directory does not exists for the batch : {0} for {1}", printBatchId, localMRDFResponseFilePath);
                    log.ErrorFormat(errorMsg);
                }
                else
                {
                    if (File.Exists(Path.Combine(bi.PCIResponsePath, Path.GetFileName(localMRDFResponseZipFilePath))))
                        File.Delete(Path.Combine(bi.PCIResponsePath, Path.GetFileName(localMRDFResponseZipFilePath)));

                    //This will move zip file from root folder of pci response folder also.
                    File.Move(localMRDFResponseZipFilePath, Path.Combine(bi.PCIResponsePath, Path.GetFileName(localMRDFResponseZipFilePath)));


                    if (File.Exists(Path.Combine(bi.PCIResponsePath, Path.GetFileName(localMRDFResponseFilePath))))
                        File.Delete(Path.Combine(bi.PCIResponsePath, Path.GetFileName(localMRDFResponseFilePath)));

                    File.Move(localMRDFResponseFilePath, Path.Combine(bi.PCIResponsePath, Path.GetFileName(localMRDFResponseFilePath)));

                    File.Delete(localMRDFResponseFilePath);
                    log.DebugFormat("Response MRDF moved in proper batch folder : {0} after batch identified.", printBatchId);

                    return bi;
                }
            }

            return null;
        }

        #endregion

        #region Return PDF Zip File Logic
        public static void GetReturnPDFsZip()
        {
            log.Debug("Return PDF job started.");
            List<SubmittedBatches> mrdfReceivedPrintBatches;
            mrdfReceivedPrintBatches = DAL.GetMRDFReceivedBatches();
            if (mrdfReceivedPrintBatches.Count == 0)
            {
                log.Debug("Return PDF job queue is empty.");
            }
            else
            {
                SFTUtil sftSite = new SFTUtil(Common.PCI_SFT_Protocol, Common.PCI_SFT_Host, Common.PCI_SFT_Username, Common.PCI_SFT_Password, 300000);

                sftSite.Connect();

                CheckAndDownloadReturnPDFZipFile(sftSite, mrdfReceivedPrintBatches);

                sftSite.Disconnect();
            }

            log.Debug("MRDF download job ended.");
        }

        private static void CheckAndDownloadReturnPDFZipFile(SFTUtil sftSite, List<SubmittedBatches> mrdfReceivedPrintBatches)
        {
            List<string> zipFiles;
            zipFiles = sftSite.ListDirectory(Common.PCI_SFT_ReturnPDFDir);

            var retZipFiles = zipFiles
                .Where(file => Path.GetExtension(file).ToLower() == ".zip")
                .Select(file => new KeyValuePair<string, string>(Path.GetFileNameWithoutExtension(file), file))
                .ToList();

            foreach (var batch in mrdfReceivedPrintBatches)
            {
                batch.ReceivedFileList.AddRange(retZipFiles.Where(zipfile => zipfile.Key.StartsWith(batch.PatternBatchCode, StringComparison.CurrentCultureIgnoreCase)).Select(zipfile => zipfile.Value));
            }

            var downloadableFiles = mrdfReceivedPrintBatches.Where(c => c.ReceivedFileList != null && c.ReceivedFileList.Count > 0).ToList();

            if (downloadableFiles.Count == 0)
            {
                log.Debug("Return PDFs zip files are not available for download.");
            }
            else
            {
                foreach (var downloadableBatch in downloadableFiles)
                {
                    log.Debug("Downloading return PDFs zip For Batch-ID : " + downloadableBatch.PrintBatchId.ToString());
                    foreach (string fileName in downloadableBatch.ReceivedFileList)
                    {
                        log.Debug("Downloading PCI Return PDF Job Started For Batch {0}".Fill(Path.GetFileNameWithoutExtension(fileName)));

                        //Reason for creating ReturnPDFs in PCI root folder because we are currenly processing return MRDF file in same root folder and naming convention of its zip file is same as return PDFs' zip file.
                        string localPDFsZipFilePath = Path.Combine(Common.PCIRootReturnPDFsZipFolder, fileName);

                        string remoteZipPath = Path.Combine(Common.PCI_SFT_ReturnPDFDir, fileName);
                        DateTime zipReceivedOn = sftSite.LastModifiedTimeOfFile(remoteZipPath);

                        sftSite.DownloadFile(localPDFsZipFilePath, remoteZipPath, Common.Remove_PDF_Of_PCI_SFT, false);

                        IdentifyAndMoveZip(localPDFsZipFilePath, zipReceivedOn, downloadableBatch.PrintBatchId);
                    }
                }
            }
        }

        private static void IdentifyAndMoveZip(string localPDFsZipFilePath, DateTime zipReceivedOn, int printBatchId)
        {
            BatchSummary bs = DAL.GetBatchSummaryById(printBatchId);

            if (bs != null)
            {
                Client app = DAL.GetClientAppDetailsById(bs.ClientId);
                BatchInfo bi = new BatchInfo(bs, app);

                var docTypesMaps = DAL.GetDocTypeDetailsByAppId(app.Id).Where(x => x.DocTypeId == bi.BatchSummary.DocTypeId).First();
                if (docTypesMaps != null)
                    bi.InitializeDocTypeMaps(docTypesMaps, bi.BatchSummary.PrintBatch, bi.Client.AppCode, bi.Client.CustCode);

                if (!Directory.Exists(bi.BatchRepo))
                {
                    string errorMsg = string.Format("Local directory does not exists for the batch : {0} for {1}", printBatchId, localPDFsZipFilePath);
                    log.ErrorFormat(errorMsg);
                }
                else
                {
                    if (!Directory.Exists(bi.PCIResponseReturnPDFsPath))
                        Directory.CreateDirectory(bi.PCIResponseReturnPDFsPath);

                    string batchFolderZipFilePath = Path.Combine(bi.PCIResponseReturnPDFsPath, Path.GetFileName(localPDFsZipFilePath));

                    if (File.Exists(batchFolderZipFilePath))
                        File.Delete(batchFolderZipFilePath);

                    //This will move zip file from root folder of pci response folder also.
                    File.Move(localPDFsZipFilePath, batchFolderZipFilePath);

                    //After moving to return pdf
                    using (ZipArchive archive = ZipFile.OpenRead(batchFolderZipFilePath))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            if (entry.FullName.ToUpper().EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                            {
                                entry.ExtractToFile(Path.Combine(bi.PCIResponseReturnPDFsPath, Path.GetFileName(entry.FullName)), true);
                            }
                        }
                    }

                    log.DebugFormat("Return PDFs zip is moved in proper batch folder : {0} after batch identified.", printBatchId);
                }
            }
        }
        #endregion

    }
}
