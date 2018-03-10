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
using System.Xml.Linq;
using System.Xml.Schema;

namespace AM.Console
{
    public class DownloadJob
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DownloadJob));

        IClientService clientSvc;
        IBatchService mailBatchSvc;
        ICommonService commonSvc;
        IMailPieceService mailPieceSvc;
        public DownloadJob()
        {
            clientSvc = new ClientService();
            mailBatchSvc = new BatchService();
            commonSvc = new CommonService();
            mailPieceSvc = new MailPieceService();
        }

        #region Download New Batch
        public void DownloadNewBatch()
        {
            log.Debug("Download Batch Process Started");

            Client client = clientSvc.GetClientDetails(null).Single(x => x.ClientName == "USAA");

            ClientSFTChannel sftChannelMap = clientSvc.GetClientSFTChannelMapDetails(client.Id).SingleOrDefault();
            List<string> triggerFilePaths = new List<string>();
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

                    if (sftSite.IsDownloadDirectoryAvailable(sftChannelDetails.ToADEDir))
                    {
                        triggerFilePaths = GetTriggerFilePaths(sftSite, sftChannelDetails);
                        if (triggerFilePaths != null || triggerFilePaths.Count > 0)
                        {
                            MailBatchDTO mailBatch = null;
                            foreach (string triggerFile in triggerFilePaths)
                            {
                                string zipFileFTPPath = Path.Combine(sftChannelDetails.ToADEDir, Regex.Replace(Path.ChangeExtension(triggerFile, ".zip"), @"(w*)_Complete", string.Empty));
                                string triggerFilePath = Path.Combine(sftChannelDetails.ToADEDir, triggerFile);
                                //Download and Insert the batch in database first always.
                                string batchName = Path.GetFileNameWithoutExtension(zipFileFTPPath);

                                string appDirRemotePath = "{0}/{1}".Fill(Common.AuthoMailRootFolder, client.AppCode);
                                string batchPDFsDirRemotePath = "{0}/{1}".Fill(appDirRemotePath, batchName);

                                mailBatch = new MailBatchDTO();
                                mailBatch.ClientId = client.Id;
                                mailBatch.BatchName = batchName;
                                mailBatch.SFTChannelId = sftChannelDetails.Id;
                                mailBatch.ClientBatchId = batchName;
                                mailBatch.UniqueId = Guid.NewGuid();
                                //Insert New Batch Entry
                                mailBatch.Id = mailBatchSvc.InsertBatchDetails(mailBatch);
                                //Update Pipeline Status of Batch Received
                                commonSvc.UpdateBatchPipeLineStatus(mailBatch.Id, BatchPipelineStatus.Received, sftSite.LastModifiedTimeOfFile(triggerFilePath));
                                //Now Download Zip File Path

                                MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, sftChannelDetails);
                                DownloadBatchFromFTP(sftSite, zipFileFTPPath, triggerFilePath, client, sftChannelDetails, mbi);
                                //ProcessForUSAAClient(sftSite, triggerFilePath, mbi, sftChannelDetails);
                            }
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
                        mailBatchSvc.UpdateBatchDetails(mbi.MailBatch);

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
                mbi.MailBatch.ValidationStatus = BatchValidationStatus.Invalid;
                mbi.MailBatch.ValidationErrors = "Zip file is missing on Sft site.";

                //Update Batch Details
                mailBatchSvc.UpdateBatchDetails(mbi.MailBatch);

                //Update Batch Status
                commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, BatchStatus.Failed.ToString());
            }
        }

        private static List<string> GetTriggerFilePaths(SFTUtil sftp, SFTChannel channelDetails)
        {
            if (sftp.IsConnected == false)
                sftp.Connect();

            DateTime pingTime = Common.Now;

            List<string> availableTriggerFilePaths = sftp.ListDirectory(channelDetails.ToADEDir).Where(f => Path.GetExtension(f).Equals(".go", StringComparison.CurrentCultureIgnoreCase) && Path.GetFileNameWithoutExtension(f).EndsWith("_Complete")).ToList();

            if (availableTriggerFilePaths != null && availableTriggerFilePaths.Count > 0)
            {
                log.Debug(string.Format("{0} Trigger file found.", availableTriggerFilePaths.Count));

                return availableTriggerFilePaths;
            }
            else
            {
                log.Error("Trigger file not found.");
            }

            return null;
        }

        private void MoveTriggerFileToBackupDir(SFTUtil sftSite, string triggerFTPFilePath, SFTChannel channelDetails)
        {
            if (!sftSite.IsConnected)
                sftSite.Connect();

            string ftpBackupDir = channelDetails.ArchiveDir;

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
                string archiveFTPFilePath = Path.Combine(mbi.SFTChannel.ArchiveDir, zipFileName);
               
                if (sftSite.IsFileAvailable(archiveFTPFilePath))
                    sftSite.DeleteFile(archiveFTPFilePath);

                sftSite.MoveFile(zipFileFTPPath, archiveFTPFilePath);
            }
            else
            {
                sftSite.DeleteFile(zipFileFTPPath);
            }

            return downloadedZipFilePath;
        }

        //public void ProcessForUSAAClient(SFTUtil sftSite, string triggerFilePath, MailBatchInfo mbi, SFTChannel channelDetails)
        //{
        //    var xmlFilePath = Path.Combine(mbi.AMRequestPath, mbi.BatchName, mbi.BatchName + ".xml");

        //    if (!File.Exists(xmlFilePath))
        //    {
        //        DeleteBatchDetailsAndFilesFromFolder(mbi);
        //    }

        //    XmlDocument batchXML = new XmlDocument();
        //    XmlNamespaceManager nsManager = null;
        //    XmlNodeList mailPieceNodes = null;
        //    // Try to parse the XML we got from the zip file. If XML is invalid then log error.
        //    try
        //    {
        //        batchXML.LoadXml(File.ReadAllText(xmlFilePath));
        //        nsManager = new XmlNamespaceManager(batchXML.NameTable);
        //        nsManager.AddNamespace("x", "http://schemas.corelogic.com/2015/amc/v1/packages");
        //        mailPieceNodes = batchXML.DocumentElement.ChildNodes;
        //    }
        //    catch (XmlException err)
        //    {
        //        #region Invalid XML file of the batch so just save error details
        //        log.Error(err);
        //        DeleteBatchDetailsAndFilesFromFolder(mbi);
        //        #endregion
        //    }

        //    if (mailPieceNodes != null)
        //    {
        //        List<string> custIdList = new List<string>();
        //        foreach (XmlNode mailPieceNode in mailPieceNodes)
        //        {
        //            string customerIdentifierCode = mailPieceNode.SelectSingleNode("./x:CustomerIdentifier/text()", nsManager) == null ? string.Empty : mailPieceNode.SelectSingleNode("./x:CustomerIdentifier/text()", nsManager).Value;

        //            custIdList.Add(customerIdentifierCode.Trim());
        //        }

        //        if (custIdList.Distinct().Count() == 1 && custIdList.Distinct().Single() == "166")
        //        {
        //            mbi.MailBatch.UniqueId = Guid.NewGuid();
        //            mbi.MailBatch.ClientBatchId = mbi.BatchName;
        //            mailBatchSvc.UpdateBatchDetails(mbi.MailBatch);

        //            MoveTriggerFileToBackupDir(sftSite, triggerFilePath, channelDetails);
        //        }
        //        else
        //        {
        //            DeleteBatchDetailsAndFilesFromFolder(mbi);
        //        }
        //    }
        //}

        private void DeleteBatchDetailsAndFilesFromFolder(MailBatchInfo mbi)
        {
            Directory.Delete(mbi.BatchRepo, true);
            mailBatchSvc.DeleteBatchDetails(mbi.MailBatch.Id);
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
                mbi.MailBatch.ValidationStatus = BatchValidationStatus.Invalid;
                mbi.MailBatch.ValidationErrors = ex.Message;

                mailBatchSvc.UpdateBatchDetails(mbi.MailBatch);

                commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, BatchStatus.Failed.ToString());
            }

            //We can do logic implementation for getting only 166 CustomerIdentifier's batch

            MailBatch amParsedBatch = new MailBatch();
            amParsedBatch.ChannelCode = mbi.SFTChannel.Code;
            amParsedBatch.ClientBatchId = mbi.BatchName;
            amParsedBatch.Id = Guid.NewGuid();
            amParsedBatch.Priority = Priority.High;
            amParsedBatch.ClientCode = mbi.Client.AppCode;

            List<MailPiece> amParsedPieces = new List<MailPiece>();
            foreach (var package in packageRequest.Packages)
            {
                // If the XML is valid and found any child nodes
                if (package != null)
                {
                    try
                    {
                        MailPiece amPiece = new MailPiece();
                        amPiece.BusinessFriendlyIdName = "LoanNbr";
                        amPiece.BusinessFriendlyIdValue = package.LoanNbr;
                        if (package.File != null)
                        {
                            amPiece.DocumentOrPackage = new DocumentOrPackage();
                            amPiece.DocumentOrPackage.Name = package.File;
                        }
                        amPiece.DocumentOrPackageType = package.PackageName;
                        amPiece.Id = package.Id;
                        if (package.RecipientInfo != null)
                        {
                            amPiece.RecipientInfo = new Entities.RecipientInfo();

                            if (package.RecipientInfo.Recipients != null && package.RecipientInfo.Recipients.Count > 0)
                            {
                                amPiece.RecipientInfo.Recipient = new Entities.Recipient();
                                if (package.RecipientInfo.Recipients.Count > 1)
                                {
                                    amPiece.RecipientInfo.Corecipient = new Entities.Recipient();
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
                                amPiece.RecipientInfo.Address = new Entities.Address();
                                amPiece.RecipientInfo.Address.AptOrSuiteOrOther = package.RecipientInfo.RecipientAddress.Line2;
                                amPiece.RecipientInfo.Address.Street = package.RecipientInfo.RecipientAddress.Line1;
                                amPiece.RecipientInfo.Address.City = package.RecipientInfo.RecipientAddress.City;
                                amPiece.RecipientInfo.Address.State = package.RecipientInfo.RecipientAddress.State;
                                amPiece.RecipientInfo.Address.Zip = package.RecipientInfo.RecipientAddress.Zip;
                            }

                        }

                        if (package.ReturnInfo != null)
                        {
                            amPiece.ReturnInfo = new Entities.ReturnInfo();
                            amPiece.ReturnInfo.Recipient = new Entities.Recipient();

                            amPiece.ReturnInfo.Recipient.FirstName = package.ReturnInfo.FirstName;
                            amPiece.ReturnInfo.Recipient.LastName = package.ReturnInfo.LastName;
                            amPiece.ReturnInfo.Recipient.Phone = package.ReturnInfo.ReturnPhoneNo;

                            if (package.ReturnInfo.ReturnAddress != null)
                            {
                                amPiece.ReturnInfo.Address = new Entities.Address();

                                amPiece.ReturnInfo.Address.AptOrSuiteOrOther = package.ReturnInfo.ReturnAddress.Line2;
                                amPiece.ReturnInfo.Address.Street = package.ReturnInfo.ReturnAddress.Line1;
                                amPiece.ReturnInfo.Address.City = package.ReturnInfo.ReturnAddress.City;
                                amPiece.ReturnInfo.Address.State = package.ReturnInfo.ReturnAddress.State;
                                amPiece.ReturnInfo.Address.Zip = package.ReturnInfo.ReturnAddress.Zip;
                            }
                        }

                        amPiece.CustomInfo = new List<Metadata>()
                        {
                            new Metadata() { Key = "RequestId",DataType=DataType.String, Value = package.RequestId},
                            new Metadata() { Key = "CustomerIdentifier",DataType=DataType.String, Value = package.CustomerIdentifier },
                            new Metadata() { Key = "ShippingCredCode",DataType=DataType.String, Value = package.ShippingCredCode },
                            new Metadata() { Key = "ShippingMethod",DataType=DataType.String, Value = package.ShippingMethod.ToString() },
                            new Metadata() { Key = "ReceivedDate",DataType=DataType.DateTime, Value = package.ReceivedDate.HasValue?package.ReceivedDate.Value.ToString():null }
                        };

                        amParsedPieces.Add(amPiece);
                    }
                    catch (Exception)
                    {
                        //TODO : Need to consult with Ravi Sir 
                        throw;
                    }
                }
            }

            amParsedBatch.MailPieces = amParsedPieces;

            return amParsedBatch;
        }
        #endregion

        #region Validate And Acknowledge
        public void ValidateBatches()
        {
            List<MailBatchDTO> downloadBatches = mailBatchSvc.GetBatchesByStatus(BatchPipelineStatus.Downloaded);
            if (downloadBatches == null)
            {
                log.Debug("Process batches job queue is empty.");
                return;
            }

            foreach (var batch in downloadBatches)
            {
                Client client = clientSvc.GetClientDetails(batch.ClientId).Single();
                SFTChannel sftChannelDetails = clientSvc.GetSFTChannelDetails(batch.SFTChannelId);

                MailBatchInfo mbi = new MailBatchInfo(batch, client, sftChannelDetails);

                var fullBatchPath = Path.Combine(mbi.AMRequestPath, mbi.BatchName);
                var xmlFilePath = Path.Combine(fullBatchPath, mbi.BatchName + ".xml");

                XmlDocument batchXML = new XmlDocument();
                XmlNamespaceManager nsManager = null;
                XmlNodeList mailPieceNodes = null;
                // Try to parse the XML we got from the zip file. If XML is invalid then log error.
                try
                {
                    commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.ValidationStart, Common.Now);
                    batchXML.LoadXml(File.ReadAllText(xmlFilePath));
                    nsManager = new XmlNamespaceManager(batchXML.NameTable);
                    nsManager.AddNamespace("x", "http://schemas.corelogic.com/2015/amc/v1/packages");
                    mailPieceNodes = batchXML.DocumentElement.ChildNodes;
                }
                catch (XmlException err)
                {
                    #region Invalid XML file of the batch so just save error details
                    log.Error(err);
                    batch.ValidationStatus = BatchValidationStatus.Invalid;
                    batch.ValidationErrors = err.ToString();

                    mailBatchSvc.UpdateBatchDetails(batch);
                    commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.ValidationEnd, Common.Now);
                    commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Validated, Common.Now);
                    commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, BatchStatus.Failed.ToString());
                    DeleteBatchDetailsAndFilesFromFolder(mbi);
                    #endregion
                }
                if (mailPieceNodes != null)
                {
                    foreach (XmlNode mailPieceNode in mailPieceNodes)
                    {
                        MailPieceDTO mailPieceDTO = new MailPieceDTO();
                        MailPieceCustomInfo customInfo = null;
                        List<MailPieceCustomInfo> mailPieceCustomInfo = new List<MailPieceCustomInfo>();
                        mailPieceDTO.ClientId = client.Id;
                        mailPieceDTO.DueDate = Common.Today;
                        mailPieceDTO.MailBatchId = mbi.MailBatch.Id;


                        mailPieceDTO.BusinessFriendlyIdName = "LoanNbr";
                        mailPieceDTO.BusinessFriendlyIdValue = mailPieceNode.SelectSingleNode("./x:LoanNbr/text()", nsManager) == null ? string.Empty : mailPieceNode.SelectSingleNode("./x:LoanNbr/text()", nsManager).Value;
                        mailPieceDTO.DocumentOrPackageType = mailPieceNode.SelectSingleNode("./x:PackageName/text()", nsManager) == null ? null : mailPieceNode.SelectSingleNode("./x:PackageName/text()", nsManager).Value;
                        mailPieceDTO.UniqueId = mailPieceNode.SelectSingleNode("./x:Id/text()", nsManager) == null ? Guid.Empty : Guid.Parse(mailPieceNode.SelectSingleNode("./x:Id/text()", nsManager).Value);
                        mailPieceDTO.FileName = mailPieceNode.SelectSingleNode("./x:File/text()", nsManager) == null ? null : mailPieceNode.SelectSingleNode("./x:File/text()", nsManager).Value;



                        var recipientElement = mailPieceNode.SelectSingleNode("./x:RecipientInfo", nsManager);
                        if (recipientElement != null)
                        {
                            if (recipientElement.SelectNodes("./x:Recipient", nsManager) != null)
                            {
                                var recipientsData = recipientElement.SelectNodes("./x:Recipient", nsManager);

                                if (recipientsData.Count > 1)
                                {
                                    mailPieceDTO.RcptFirstName = recipientsData.Item(0).SelectSingleNode("./x:FirstName/text()", nsManager) == null ? null : recipientsData.Item(0).SelectSingleNode("./x:FirstName/text()", nsManager).Value;
                                    mailPieceDTO.RcptLastName = recipientsData.Item(0).SelectSingleNode("./x:LastName/text()", nsManager) == null ? null : recipientsData.Item(0).SelectSingleNode("./x:LastName/text()", nsManager).Value;

                                    mailPieceDTO.CoRcptFirstName = recipientsData.Item(1).SelectSingleNode("./x:FirstName/text()", nsManager) == null ? null : recipientsData.Item(1).SelectSingleNode("./x:FirstName/text()", nsManager).Value;
                                    mailPieceDTO.CoRcptLastName = recipientsData.Item(1).SelectSingleNode("./x:LastName/text()", nsManager) == null ? null : recipientsData.Item(1).SelectSingleNode("./x:LastName/text()", nsManager).Value;
                                }
                                else
                                {
                                    mailPieceDTO.RcptFirstName = recipientsData.Item(0).SelectSingleNode("./x:FirstName/text()", nsManager) == null ? null : recipientsData.Item(0).SelectSingleNode("./x:FirstName/text()", nsManager).Value;
                                    mailPieceDTO.RcptLastName = recipientsData.Item(0).SelectSingleNode("./x:LastName/text()", nsManager) == null ? null : recipientsData.Item(0).SelectSingleNode("./x:LastName/text()", nsManager).Value;
                                }
                            }


                            mailPieceDTO.RcptPhone = recipientElement.SelectSingleNode("./x:RecipientPhoneNo/text()", nsManager) == null ? null : recipientElement.SelectSingleNode("./x:RecipientPhoneNo/text()", nsManager).Value;

                            var recipeintAddressElement = recipientElement.SelectSingleNode("./x:Address", nsManager);

                            if (recipeintAddressElement != null)
                            {
                                mailPieceDTO.RcptAddressStreet = recipeintAddressElement.SelectSingleNode("./x:Line1/text()", nsManager) == null ? null : recipeintAddressElement.SelectSingleNode("./x:Line1/text()", nsManager).Value;

                                mailPieceDTO.RcptAddressAptOrSuiteOrOther = recipeintAddressElement.SelectSingleNode("./x:Line2/text()", nsManager) == null ? null : recipeintAddressElement.SelectSingleNode("./x:Line2/text()", nsManager).Value;

                                mailPieceDTO.RcptAddressCity = recipeintAddressElement.SelectSingleNode("./x:City/text()", nsManager) == null ? null : recipeintAddressElement.SelectSingleNode("./x:City/text()", nsManager).Value;

                                mailPieceDTO.RcptAddressState = recipeintAddressElement.SelectSingleNode("./x:State/text()", nsManager) == null ? null : recipeintAddressElement.SelectSingleNode("./x:State/text()", nsManager).Value;

                                mailPieceDTO.RcptAddressZIP = recipeintAddressElement.SelectSingleNode("./x:Zip/text()", nsManager) == null ? null : recipeintAddressElement.SelectSingleNode("./x:Zip/text()", nsManager).Value;
                            }
                        }

                        var returnInfoElement = mailPieceNode.SelectSingleNode("./x:ReturnInfo", nsManager);
                        if (returnInfoElement != null)
                        {

                            mailPieceDTO.RetRcptFirstName = returnInfoElement.SelectSingleNode("./x:FirstName/text()", nsManager) == null ? null : returnInfoElement.SelectSingleNode("./x:FirstName/text()", nsManager).Value;

                            mailPieceDTO.RetRcptLastName = returnInfoElement.SelectSingleNode("./x:LastName/text()", nsManager) == null ? null : returnInfoElement.SelectSingleNode("./x:LastName/text()", nsManager).Value;


                            mailPieceDTO.RetRcptPhone = returnInfoElement.SelectSingleNode("./x:ReturnPhoneNo/text()", nsManager) == null ? null : returnInfoElement.SelectSingleNode("./x:ReturnPhoneNo/text()", nsManager).Value;

                            var returnAddressElement = returnInfoElement.SelectSingleNode("./x:Address", nsManager);
                            if (returnAddressElement != null)
                            {
                                mailPieceDTO.RetRcptAddressStreet = returnAddressElement.SelectSingleNode("./x:Line1/text()", nsManager) == null ? null : returnAddressElement.SelectSingleNode("./x:Line1/text()", nsManager).Value;

                                mailPieceDTO.RetRcptAddressAptOrSuiteOrOther = returnAddressElement.SelectSingleNode("./x:Line2/text()", nsManager) == null ? null : returnAddressElement.SelectSingleNode("./x:Line2/text()", nsManager).Value;

                                mailPieceDTO.RetRcptAddressCity = returnAddressElement.SelectSingleNode("./x:City/text()", nsManager) == null ? null : returnAddressElement.SelectSingleNode("./x:City/text()", nsManager).Value;

                                mailPieceDTO.RetRcptAddressState = returnAddressElement.SelectSingleNode("./x:State/text()", nsManager) == null ? null : returnAddressElement.SelectSingleNode("./x:State/text()", nsManager).Value;

                                mailPieceDTO.RetRcptAddressZIP = returnAddressElement.SelectSingleNode("./x:Zip/text()", nsManager) == null ? null : returnAddressElement.SelectSingleNode("./x:Zip/text()", nsManager).Value;
                            }

                            if (mailPieceNode.SelectSingleNode("./x:ShippingMethod", nsManager) != null)
                            {
                                customInfo = new MailPieceCustomInfo() { MailPieceId = 0, Key = "ShippingMethod", DataType = DataType.String, Value = mailPieceNode.SelectSingleNode("./x:ShippingMethod/text()", nsManager).Value };

                                mailPieceCustomInfo.Add(customInfo);
                            }

                            if (mailPieceNode.SelectSingleNode("./x:CustomerIdentifier", nsManager) != null)
                            {
                                customInfo = new MailPieceCustomInfo() { MailPieceId = 0, Key = "CustomerIdentifier", DataType = DataType.String, Value = mailPieceNode.SelectSingleNode("./x:CustomerIdentifier/text()", nsManager).Value };

                                mailPieceCustomInfo.Add(customInfo);
                            }

                            if (mailPieceNode.SelectSingleNode("./x:RequestId", nsManager) != null)
                            {
                                customInfo = new MailPieceCustomInfo() { MailPieceId = 0, Key = "RequestId", DataType = DataType.String, Value = mailPieceNode.SelectSingleNode("./x:RequestId/text()", nsManager).Value };

                                mailPieceCustomInfo.Add(customInfo);
                            }

                            if (mailPieceNode.SelectSingleNode("./x:ReceivedDate", nsManager) != null)
                            {
                                customInfo = new MailPieceCustomInfo() { MailPieceId = 0, Key = "ReceivedDate", DataType = DataType.DateTime, Value = mailPieceNode.SelectSingleNode("./x:ReceivedDate/text()", nsManager).Value };

                                mailPieceCustomInfo.Add(customInfo);
                            }

                            var shippingCredElement = mailPieceNode.SelectSingleNode("./x:ShippingCredentials", nsManager);
                            if (shippingCredElement != null)
                            {
                                if (shippingCredElement.SelectSingleNode("./x:ClientCode", nsManager) != null)
                                    customInfo = new MailPieceCustomInfo() { MailPieceId = 0, Key = "ShippingCredCode", DataType = DataType.String, Value = shippingCredElement.SelectSingleNode("./x:ClientCode/text()", nsManager).Value };

                                mailPieceCustomInfo.Add(customInfo);
                            }
                        }

                        try
                        {
                            ValidationResultAMC validationResult = Common.ValidateXMLNode(mailPieceNode);
                            if (validationResult.Code == OutComeCodes.No_Error)
                            {
                                string filePath = Path.Combine(mbi.AMRequestPath, mbi.BatchName, mailPieceDTO.FileName);
                                if (!File.Exists(filePath))
                                {
                                    mailPieceDTO.ErrorCode = OutComeCodes.Missing_PDF;
                                    mailPieceDTO.ErrorDesc = "Pdf file is not available for this package.";
                                }
                                else
                                {
                                    mailPieceDTO.ErrorCode = OutComeCodes.No_Error;
                                }
                            }
                            else
                            {
                                mailPieceDTO.ErrorCode = validationResult.Code;
                                mailPieceDTO.ErrorDesc = validationResult.ErrorMessage;
                                mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                            }

                            mailPieceSvc.InsertBatchMailPieceDetails(mailPieceDTO, mailPieceCustomInfo);
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                }
                commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Validated, Common.Now);
                commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.ValidationStart, Common.Now);
            }
        }

        public void AcknowledgeBatches()
        {
            List<MailBatchDTO> validatedBatches = mailBatchSvc.GetBatchesByStatus(BatchPipelineStatus.Validated);
            if (validatedBatches == null)
            {
                log.Debug("Process batches job queue is empty.");
                return;
            }

            foreach (var batch in validatedBatches)
            {
                Client client = clientSvc.GetClientDetails(batch.ClientId).Single();
                SFTChannel sftChannelDetails = clientSvc.GetSFTChannelDetails(batch.SFTChannelId);

                MailBatchInfo mbi = new MailBatchInfo(batch, client, sftChannelDetails);

                var data = mailBatchSvc.GetMailBatchDetailsByBatchId(mbi.MailBatch.Id);

                if (data != null)
                {
                    commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.AcknowledgeStart, Common.Now);

                    MailUtility.SendDownloadNotificationEmail(mbi, data.MailPieces, data.MailPieceCustomInfo);

                    commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Acknowledged, Common.Now);
                    commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.AcknowledgeEnd, Common.Now);
                }
            }
        }
        #endregion
    }
}
