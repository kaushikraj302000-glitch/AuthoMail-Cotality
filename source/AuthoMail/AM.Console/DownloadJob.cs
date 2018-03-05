using AM.Entities;
using AM.Services;
using Ionic.Zip;
using log4net;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace AM.Console
{
    public class DownloadJob
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DownloadJob));

        IClientService clientSvc;
        IBatchService batchSvc;
        IMailPieceService mailPieceSvc;
        ICommonService commonSvc;
        public DownloadJob()
        {
            clientSvc = new ClientService();
            batchSvc = new BatchService();
            mailPieceSvc = new MailPieceService();
            commonSvc = new CommonService();
        }

        public void DownloadNewBatch()
        {
            log.Debug("Download Batch Process Started");

            Client client = clientSvc.GetClientDetails(null).Single(x => x.ClientName == "USAA");

            ClientSFTChannel sftChannelMap = clientSvc.GetClientSFTChannelMapDetails(client.Id).SingleOrDefault();
            string triggerFilePath = string.Empty;
            if (sftChannelMap != null)
            {
                SFTChannel sftChannelDetails = clientSvc.GetSFTChannelDetails(sftChannelMap.SFTChannelId);
                if (sftChannelDetails != null)
                {
                    SFTUtil sftSite = null;
                    try
                    {
                        sftSite = new SFTUtil(Common.GetProtocolType(sftChannelDetails.Protocol), sftChannelDetails.Host, sftChannelDetails.UserName, sftChannelDetails.Password, 300000);
                        sftSite.Connect();
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                    }

                    MailBatchDTO mailBatch = null;
                    if (sftSite.IsDownloadDirectoryAvailable(sftChannelDetails.ADERequestDir))
                    {

                        triggerFilePath = GetTriggerFilePath(sftSite, sftChannelDetails);
                        if (!string.IsNullOrEmpty(triggerFilePath))
                        {
                            // Download and Insert the batch in database first always.
                            string zipFileFTPPath = Path.Combine(sftChannelDetails.ADERequestDir, Regex.Replace(Path.ChangeExtension(triggerFilePath, ".zip"), @"(w*)_Complete", string.Empty));
                            string batchName = Path.GetFileNameWithoutExtension(zipFileFTPPath);

                            string appDirRemotePath = "{0}/{1}".Fill(Common.AuthoMailRootFolder, client.AppCode);
                            string batchPDFsDirRemotePath = "{0}/{1}".Fill(appDirRemotePath, batchName);

                            mailBatch = new MailBatchDTO();
                            mailBatch.ClientId = client.Id;
                            mailBatch.BatchName = batchName;
                            mailBatch.SFTChannelId = sftChannelDetails.Id;
                            //Insert New Batch Entry
                            mailBatch.Id = batchSvc.InsertBatchDetails(mailBatch);
                            //Update Pipeline Status of Batch Received
                            commonSvc.UpdateBatchPipeLineStatus(mailBatch.Id, BatchPipelineStatus.Received, sftSite.LastModifiedTimeOfFile(triggerFilePath));
                            //Now Download Zip File Path

                            MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, null, sftChannelDetails);
                            DownloadBatchFromFTP(sftSite, zipFileFTPPath, triggerFilePath, client, sftChannelDetails, mbi);
                        }
                        if (sftSite.IsConnected)
                            sftSite.Disconnect();
                    }
                }
            }
        }

        private void DownloadBatchFromFTP(SFTUtil sftSite, string zipFileFTPPath, string triggerFilePath, Client client, SFTChannel channelDetails, MailBatchInfo mbi)
        {
            string downloadedZipFilePath = string.Empty;

            if (sftSite.IsConnected == false)
                sftSite.Connect();

            // If batch zip file exists on FTP then download and process it.
            if (sftSite.IsFileAvailable(zipFileFTPPath))
            {
                downloadedZipFilePath = DownloadBatchZipFile(sftSite, zipFileFTPPath, client, mbi);

                if (!string.IsNullOrEmpty(downloadedZipFilePath))
                {
                    //After successfully download update downloaded pipeline status
                    commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Downloaded, Common.GetCurrentTime());

                    try
                    {
                        #region Try to extract zip file, insert batch in DB and delete trigger+zip files from FTP.

                        using (ZipFile zipFile = ZipFile.Read(downloadedZipFilePath))//Read file From the Zip.
                        {
                            foreach (ZipEntry e in zipFile)
                            {
                                e.Extract(Path.GetDirectoryName(downloadedZipFilePath), ExtractExistingFileAction.OverwriteSilently);
                            }
                        }

                        #endregion

                    }
                    catch (ZipException err)
                    {
                        // If the zip file is not available in the FTP folder then log that error.
                        mbi.MailBatch.ValidationStatus = OutComeCodes.Invalid_Zip_File;
                        mbi.MailBatch.ValidationErrors = "Following error occured while extracting zip file :" + Environment.NewLine + err.ToString();

                        //Update Batch Details
                        batchSvc.UpdateBatchDetails(mbi.MailBatch);

                        //Update Batch Status
                        commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, BatchStatus.Failed.ToString());
                    }

                    //Process trigger file as per PostDownloadAction Setup for particular SFTChannel
                    MoveTriggerFileToBackupDir(sftSite, triggerFilePath, channelDetails);
                }
            }
            else
            {
                // If the zip file is not available in the FTP folder then log that error.
                mbi.MailBatch.ValidationStatus = OutComeCodes.Missing_Zip_File;
                mbi.MailBatch.ValidationErrors = "Zip file is missing on Sft site.";

                //Update Batch Details
                batchSvc.UpdateBatchDetails(mbi.MailBatch);

                //Update Batch Status
                commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, BatchStatus.Failed.ToString());
            }
        }

        private static string GetTriggerFilePath(SFTUtil sftp, SFTChannel channelDetails)
        {
            if (sftp.IsConnected == false)
                sftp.Connect();

            string ftpTriggerFilePath = string.Empty;
            DateTime pingTime = Common.GetCurrentTime();
            List<string> avialableTriggerFiles = sftp.ListDirectory(channelDetails.ADERequestDir).Where(f => Path.GetExtension(f).Equals(".go", StringComparison.CurrentCultureIgnoreCase) && Path.GetFileNameWithoutExtension(f).EndsWith("_Complete")).ToList();

            if (avialableTriggerFiles != null && avialableTriggerFiles.Count > 0)
            {
                ftpTriggerFilePath = avialableTriggerFiles.First();
                log.Debug(string.Format("Trigger file {0} found.", Path.GetFileName(ftpTriggerFilePath)));
            }
            else
            {
                log.Error("Trigger file not found.");
            }

            return Path.Combine(channelDetails.ADERequestDir, ftpTriggerFilePath);
        }

        private void MoveTriggerFileToBackupDir(SFTUtil sftSite, string triggerFTPFilePath, SFTChannel channelDetails)
        {
            if (!sftSite.IsConnected)
                sftSite.Connect();

            string ftpBackupDir = channelDetails.ADERequestArchiveDir;

            string triggerFTPBackUpFilePath = Path.Combine(ftpBackupDir, Path.GetFileName(triggerFTPFilePath));
            if (sftSite.IsFileAvailable(triggerFTPBackUpFilePath))
                sftSite.DeleteFile(triggerFTPBackUpFilePath);

            if (channelDetails.PostDownloadAction == PostDownloadAction.Archive)
            {
                sftSite.MoveFile(triggerFTPFilePath, triggerFTPBackUpFilePath);
            }
            else
                sftSite.DeleteFile(triggerFTPFilePath);
        }

        private string DownloadBatchZipFile(SFTUtil sftSite, string zipFileFTPPath, Client client, MailBatchInfo mbi)
        {
            commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.DownloadStart, Common.GetCurrentTime());
            #region Create directory structure.

            Common.CleanDirectory(mbi.BatchRepo);
            mbi.AMRequestPath.CreateDirectoryIfNotExist();
            mbi.PnSRequestPath.CreateDirectoryIfNotExist();
            mbi.PnSResponsePath.CreateDirectoryIfNotExist();
            mbi.AMResponsePath.CreateDirectoryIfNotExist();

            #endregion

            if (sftSite.IsConnected == false)
                sftSite.Connect();

            string downloadedZipFilePath = string.Empty;
            string zipFileName = Path.GetFileName(zipFileFTPPath);
            string batchLocalDir = Path.Combine(mbi.AMRequestPath, mbi.BatchName);
            if (!Directory.Exists(batchLocalDir))
                Directory.CreateDirectory(batchLocalDir);

            downloadedZipFilePath = Path.Combine(batchLocalDir, zipFileName);
            if (File.Exists(downloadedZipFilePath))
                File.Delete(downloadedZipFilePath);
            //Download Zip File Path
            sftSite.DownloadFile(zipFileFTPPath, downloadedZipFilePath);

            //Update LogTime for Download complete
            commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.DownloadEnd, Common.GetCurrentTime());

            if (mbi.SFTChannel.PostDownloadAction == PostDownloadAction.Archive)
            {
                string archiveFTPFilePath = Path.Combine(mbi.SFTChannel.ADERequestArchiveDir, zipFileName);
                sftSite.MoveFile(zipFileFTPPath, archiveFTPFilePath);
            }
            else
            {
                sftSite.DeleteFile(zipFileFTPPath);
            }

            return downloadedZipFilePath;
        }       
    }
}
