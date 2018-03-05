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
        ICommonService commonSvc;
        public DownloadJob()
        {
            clientSvc = new ClientService();
            batchSvc = new BatchService();
            commonSvc = new CommonService();
        }

        #region Download New Batch
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

                    if (sftSite.IsDownloadDirectoryAvailable(sftChannelDetails.ADERequestDir))
                    {
                        MailBatchDTO mailBatch = null;

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

                            MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, sftChannelDetails);
                            DownloadBatchFromFTP(sftSite, zipFileFTPPath, triggerFilePath, client, sftChannelDetails, mbi);
                        }
                        else
                        {
                            log.Debug("Batches are not available to download.");
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
                    commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Downloaded, Common.Now);

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
                        mbi.MailBatch.ValidationStatus = BatchValidationStatus.Invalid;
                        mbi.MailBatch.ValidationErrors = "Following error occured while extracting zip file :" + Environment.NewLine + err.ToString();

                        //Update Batch Details
                        batchSvc.UpdateBatchDetails(mbi.MailBatch);

                        //Update Batch Status
                        commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, BatchStatus.Failed.ToString());
                    }

                    //Process trigger file as per PostDownloadAction Setup for particular SFTChannel
                    //MoveTriggerFileToBackupDir(sftSite, triggerFilePath, channelDetails);
                }
            }
            else
            {
                // If the zip file is not available in the FTP folder then log that error.
                mbi.MailBatch.ValidationStatus = BatchValidationStatus.Invalid;
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
            DateTime pingTime = Common.Now;
            List<string> avialableTriggerFiles = sftp.ListDirectory(channelDetails.ADERequestDir).Where(f => Path.GetExtension(f).Equals(".go", StringComparison.CurrentCultureIgnoreCase) && Path.GetFileNameWithoutExtension(f).EndsWith("_Complete")).ToList();

            if (avialableTriggerFiles != null && avialableTriggerFiles.Count > 0)
            {
                ftpTriggerFilePath = avialableTriggerFiles.First();
                log.Debug(string.Format("Trigger file {0} found.", Path.GetFileName(ftpTriggerFilePath)));
                return Path.Combine(channelDetails.ADERequestDir, ftpTriggerFilePath);
            }
            else
            {
                log.Error("Trigger file not found.");
            }

            return "";
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
            commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.DownloadStart, Common.Now);
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
            commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.DownloadEnd, Common.Now);

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
        #endregion

        #region Convert AMC To AuthoMail Xml
        public void ConvertBatchIntoAMXml()
        {
            List<MailBatchDTO> downloadBatches = batchSvc.GetBatchesByStatus(BatchPipelineStatus.Downloaded);

            if (downloadBatches == null)
            {
                log.Debug("Process batches job queue is empty.");
                return;
            }

            foreach (MailBatchDTO batch in downloadBatches)
            {
                Client client = clientSvc.GetClientDetails(batch.ClientId).Single();
                SFTChannel sftChannelDetails = clientSvc.GetSFTChannelDetails(batch.SFTChannelId);

                MailBatchInfo mbi = new MailBatchInfo(batch, client, sftChannelDetails);

                var xmlFilePath = Path.Combine(mbi.AMRequestPath, mbi.BatchName, mbi.BatchName + ".xml");

                if (!File.Exists(xmlFilePath))
                {
                    mbi.MailBatch.ValidationStatus = BatchValidationStatus.Invalid;
                    mbi.MailBatch.ValidationErrors = "No XML file is available in {0} batch.".Fill(mbi.MailBatch.BatchName);

                    batchSvc.UpdateBatchDetails(mbi.MailBatch);

                    commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, BatchStatus.Failed.ToString());
                    continue;
                }

                MailBatch amParsedData = null;

                try
                {
                    amParsedData = ConvertAMCschemaToAMschema(xmlFilePath, mbi);

                    if (amParsedData != null)
                    {
                        File.WriteAllText(Path.Combine(mbi.AMRequestPath, mbi.BatchName, mbi.BatchName + "_AuthoMail.xml"), XmlUtil.Serialize<MailBatch>(amParsedData));
                    }
                }
                catch (Exception ex)
                {
                    mbi.MailBatch.ValidationStatus = BatchValidationStatus.Invalid;
                    mbi.MailBatch.ValidationErrors = ex.Message;

                    batchSvc.UpdateBatchDetails(mbi.MailBatch);

                    commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, BatchStatus.Failed.ToString());
                    continue;
                }
            }
        }

        private MailBatch ConvertAMCschemaToAMschema(string xmlFilePath, MailBatchInfo mbi)
        {
            PackageRequests packageRequest = null;
            try
            {
                XmlDocument batchXML = new XmlDocument();
                batchXML.LoadXml(File.ReadAllText(xmlFilePath));
                packageRequest = XmlUtil.Deserialize<PackageRequests>(batchXML.OuterXml);
            }
            catch (XmlException ex)
            {
                mbi.MailBatch.ValidationStatus = OutComeCodes.Invalid_XML;
                mbi.MailBatch.ValidationErrors = ex.Message;

                batchSvc.UpdateBatchDetails(mbi.MailBatch);

                commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, BatchStatus.Failed.ToString());
            }

            MailBatch amParsedBatch = new MailBatch();
            amParsedBatch.ChannelCode = mbi.SFTChannel.Code;
            amParsedBatch.ClientBatchId = mbi.BatchName;
            amParsedBatch.Id = Guid.NewGuid();
            amParsedBatch.Priority = Priority.High;

            List<MailPiece> amParsedPieces = new List<MailPiece>();
            foreach (var package in packageRequest.Packages)
            {
                // If the XML is valid and found any child nodes
                if (package != null)
                {
                    try
                    {
                        if (package != null)
                        {
                            MailPiece amPiece = new MailPiece();
                            amPiece.BusinessFriendlyIdName = "LoanNbr";
                            amPiece.BusinessFriendlyIdValue = package.LoanNbr;
                            amPiece.DocumentOrPackage.Name = package.File;
                            amPiece.DocumentOrPackageType = package.PackageName;
                            amPiece.Id = package.Id;
                            if (package.RecipientInfo != null)
                            {
                                if (package.RecipientInfo.Recipients.Count > 1)
                                {
                                    amPiece.RecipientInfo.Recipient.FirstName = package.RecipientInfo.Recipients[0].FirstName;
                                    amPiece.RecipientInfo.Recipient.LastName = package.RecipientInfo.Recipients[0].LastName;
                                    amPiece.RecipientInfo.Corecipient.FirstName = package.RecipientInfo.Recipients[1].FirstName;
                                    amPiece.RecipientInfo.Corecipient.LastName = package.RecipientInfo.Recipients[1].LastName;
                                }
                                else
                                {
                                    amPiece.RecipientInfo.Recipient.FirstName = package.RecipientInfo.Recipients[0].FirstName;
                                    amPiece.RecipientInfo.Recipient.LastName = package.RecipientInfo.Recipients[0].LastName;
                                }
                                amPiece.RecipientInfo.Recipient.Phone = package.RecipientInfo.RecipientPhoneNo;
                            }

                            if (package.RecipientInfo.RecipientAddress != null)
                            {
                                amPiece.RecipientInfo.Address.AptOrSuiteOrOther = package.RecipientInfo.RecipientAddress.Line2;
                                amPiece.RecipientInfo.Address.Street = package.RecipientInfo.RecipientAddress.Line1;
                                amPiece.RecipientInfo.Address.State = package.RecipientInfo.RecipientAddress.State;
                                amPiece.RecipientInfo.Address.Zip = package.RecipientInfo.RecipientAddress.Zip;
                            }

                            if (package.ReturnInfo != null)
                            {
                                amPiece.ReturnInfo.Recipient.FirstName = package.ReturnInfo.FirstName;
                                amPiece.ReturnInfo.Recipient.LastName = package.ReturnInfo.LastName;
                                amPiece.ReturnInfo.Recipient.Phone = package.ReturnInfo.ReturnPhoneNo;

                                if (package.ReturnInfo.ReturnAddress != null)
                                {
                                    amPiece.ReturnInfo.Address.AptOrSuiteOrOther = package.RecipientInfo.RecipientAddress.Line2;
                                    amPiece.ReturnInfo.Address.Street = package.RecipientInfo.RecipientAddress.Line1;
                                    amPiece.ReturnInfo.Address.State = package.RecipientInfo.RecipientAddress.State;
                                    amPiece.ReturnInfo.Address.Zip = package.RecipientInfo.RecipientAddress.Zip;
                                }
                            }

                            amPiece.CustomInfo = new List<Metadata>()
                            {
                                new Metadata() { Key = "ReceivedDate", Value = package.ReceivedDate.HasValue?package.ReceivedDate.Value.ToString():"" },
                                new Metadata() { Key = "RequestId", Value = package.RequestId},
                                new Metadata() { Key = "ShippingCredCode", Value = package.ShippingCredCode },
                                new Metadata() { Key = "ShippingMethod", Value = package.ShippingMethod.ToString() },
                                new Metadata() { Key = "CustomerIdentifier", Value = package.CustomerIdentifier }
                            };

                            amParsedPieces.Add(amPiece);
                        }
                    }
                    catch (Exception)
                    {
                        //TODO : Need to consult with Ravi Sir 
                        throw;
                    }
                    amParsedBatch.MailPieces = amParsedPieces;

                    return amParsedBatch;
                }
            }

            return null;
        } 
        #endregion
    }
}
