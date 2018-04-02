using AM.Entities;
using AM.Lib;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace AM.Console
{
    class AMCChannel : Channel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public AMCChannel(string channelCode)
            : base(channelCode) { }

        #region EnumerateBatches
        protected override void EnumerateBatches()
        {
            log.Debug("EnumerateBatches job queue is started.");

            List<string> triggerFilePaths = new List<string>();

            Client client = clientSvc.GetClientByAppCode("USAA");

            SFTUtil sftSite = null;
            try
            {
                sftSite = new SFTUtil(Common.GetProtocolType(SFTChannel.Protocol), SFTChannel.Host, SFTChannel.UserName, SFTChannel.Password, 300000);
                sftSite.Connect();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            if (sftSite.IsDownloadDirectoryAvailable(SFTChannel.ToADEDir))
            {
                triggerFilePaths = GetTriggerFilePaths(sftSite, SFTChannel);
                if (triggerFilePaths != null)
                {
                    foreach (string triggerFile in triggerFilePaths)
                    {
                        MailBatchDTO mailBatch = null;

                        string zipFileFTPPath = Path.Combine(SFTChannel.ToADEDir, Regex.Replace(Path.ChangeExtension(triggerFile, ".zip"), @"(w*)_Complete", string.Empty));

                        string triggerFilePath = Path.Combine(SFTChannel.ToADEDir, triggerFile);

                        //Download and Insert the batch in database first always.
                        string batchName = Path.GetFileNameWithoutExtension(zipFileFTPPath);

                        string appDirRemotePath = "{0}/{1}".Fill(Common.AuthoMailRootFolder, client.AppCode);
                        string batchPDFsDirRemotePath = "{0}/{1}".Fill(appDirRemotePath, batchName);

                        mailBatch = new MailBatchDTO();
                        mailBatch.ClientId = client.Id;
                        mailBatch.BatchName = batchName;
                        mailBatch.SFTChannelId = SFTChannel.Id;
                        mailBatch.ClientBatchId = batchName;
                        mailBatch.UniqueId = Guid.NewGuid();
                        mailBatch.Priority = Priority.Medium.ToString();
                        mailBatch.PrintLocation = "Fort Mill SC";

                        //Insert New Batch Entry
                        mailBatch.Id = mailBatchSvc.InsertBatchDetails(mailBatch);

                        //Update Pipeline Status of Batch Received
                        commonSvc.UpdateBatchPipeLineStatus(mailBatch.Id, BatchPipelineStatus.Received, sftSite.LastModifiedTimeOfFile(triggerFilePath));
                        commonSvc.UpdateBatchStatus(mailBatch.Id, BatchStatus.InProgress.ToString());
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

        private List<string> GetTriggerFilePaths(SFTUtil sftSite, SFTChannel sftChannelDetails)
        {
            if (sftSite.IsConnected == false)
                sftSite.Connect();

            List<string> availableTriggerFilePaths = sftSite.ListDirectory(sftChannelDetails.ToADEDir).Where(f => Path.GetExtension(f).Equals(".go", StringComparison.CurrentCultureIgnoreCase) && Path.GetFileNameWithoutExtension(f).EndsWith("_Complete")).ToList();

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
        #endregion

        #region DownloadBatches
        protected override void DownloadBatches()
        {
            log.Debug("Download job queue is started.");

            List<MailBatchDTO> downloadBatches = mailBatchSvc.GetBatchesByStatus(BatchPipelineStatus.Downloaded);
            if (downloadBatches == null)
            {
                log.Debug("Download job queue is empty.");
                return;
            }
            SFTUtil sftSite = null;
            try
            {
                sftSite = new SFTUtil(Common.GetProtocolType(SFTChannel.Protocol), SFTChannel.Host, SFTChannel.UserName, SFTChannel.Password, 300000);
                sftSite.Connect();
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return;
            }

            foreach (var batch in downloadBatches)
            {
                //TODO: Can we make client as common object to use?
                Client client = clientSvc.GetClientById(batch.ClientId);
                MailBatchInfo mbi = new MailBatchInfo(batch, client, null, Common.AuthoMailRootFolder);

                //TODO: Do we need to validate FTP directory exist or not?

                #region Create directory structure.

                Common.CleanDirectory(mbi.BatchRepo);
                mbi.AMRequestPath.CreateDirectoryIfNotExist();
                mbi.PnSRequestPath.CreateDirectoryIfNotExist();
                mbi.PnSResponsePath.CreateDirectoryIfNotExist();
                mbi.AMResponsePath.CreateDirectoryIfNotExist();

                #endregion

                string localZipFilePath = string.Empty;

                string ftpZipFilePath = Path.Combine(SFTChannel.ToADEDir, "{0}.zip".Fill(mbi.BatchName));
                string ftpTriggerFilePath = Path.Combine(SFTChannel.ToADEDir, "{0}_Complete.go".Fill(mbi.BatchName));

                if (sftSite.IsFileAvailable(ftpZipFilePath))
                {
                    try
                    {
                        localZipFilePath = DownloadBatchZipFile(sftSite, ftpZipFilePath, ftpTriggerFilePath, mbi);

                        //First update download status then try to extract zip file and if any issue occured, we can fail that batch
                        commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Downloaded, Common.Now);

                        using (Ionic.Zip.ZipFile zipFile = Ionic.Zip.ZipFile.Read(localZipFilePath))
                        {
                            foreach (ZipEntry e in zipFile)
                            {
                                e.Extract(Path.GetDirectoryName(localZipFilePath), ExtractExistingFileAction.OverwriteSilently);
                            }
                        }
                    }
                    catch (ZipException err)
                    {
                        var error = "Following error occured while extracting zip file for Batch ID : {0} ".Fill(mbi.MailBatch.Id) + Environment.NewLine + err.ToString();

                        log.Error(error);

                        ReportInvalidBatch(mbi.MailBatch, error);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        var error = "Error occured while downloading batch from sft site for Batch-Id : {0}".Fill(mbi.MailBatch.Id) + Environment.NewLine + ex.ToString();

                        log.Error(error);

                        ReportInvalidBatch(mbi.MailBatch, error);
                        continue;
                    }
                }
                else
                {
                    ReportInvalidBatch(mbi.MailBatch, "Zip file is missing on Sft site.");
                }
            }
            if (sftSite.IsConnected)
                sftSite.Disconnect();
        }

        private string DownloadBatchZipFile(SFTUtil sftSite, string ftpZipFilePath, string ftpTriggerFilePath, MailBatchInfo mbi)
        {
            if (sftSite.IsConnected == false)
                sftSite.Connect();

            string triggerFileName = Path.GetFileName(ftpTriggerFilePath);
            string zipFileName = Path.GetFileName(ftpZipFilePath);

            string localZipFilePath = Path.Combine(mbi.AMRequestPath, zipFileName);

            if (File.Exists(localZipFilePath))
                File.Delete(localZipFilePath);

            sftSite.DownloadFile(ftpZipFilePath, localZipFilePath);

            //Based on post download action zip file and trigger file will get moved or delete from that location
            if (mbi.SFTChannel.PostDownloadAction == PostDownloadAction.Archive)
            {
                //Move trigger file
                string archiveFTPTriggerFilePath = Path.Combine(SFTChannel.ArchiveDir, triggerFileName);

                if (sftSite.IsFileAvailable(archiveFTPTriggerFilePath))
                    sftSite.DeleteFile(archiveFTPTriggerFilePath);

                sftSite.MoveFile(ftpTriggerFilePath, archiveFTPTriggerFilePath);

                //Move zip file
                string archiveFTPZipFilePath = Path.Combine(SFTChannel.ArchiveDir, zipFileName);

                if (sftSite.IsFileAvailable(archiveFTPZipFilePath))
                    sftSite.DeleteFile(archiveFTPZipFilePath);

                sftSite.MoveFile(ftpZipFilePath, archiveFTPZipFilePath);
            }
            else
            {
                sftSite.DeleteFile(ftpTriggerFilePath);
                sftSite.DeleteFile(ftpZipFilePath);
            }

            return localZipFilePath;
        }
        #endregion

        private void ReportInvalidBatch(MailBatchDTO mailBatch, string error)
        {
            // If the zip file is not available in the FTP folder then log that error.
            mailBatch.ValidationStatus = BatchValidationStatus.Invalid;
            mailBatch.ValidationErrors = error;
            mailBatch.PrintLocation = string.Empty;
            //Update Batch Details
            mailBatchSvc.UpdateBatchDetails(mailBatch);

            //TODO: Ask to Ravi Sir for Batch level notification And how to manage status for this

            //Update Batch Status
            commonSvc.UpdateBatchStatus(mailBatch.Id, BatchStatus.Failed.ToString());
        }

        protected override void ValidateBatches()
        {
            log.Debug("Validate job queue is started.");

            List<MailBatchDTO> downloadBatches = mailBatchSvc.GetBatchesByStatus(BatchPipelineStatus.Downloaded);
            if (downloadBatches == null)
            {
                log.Debug("Validate job queue is empty.");
                return;
            }

            foreach (var batch in downloadBatches)
            {
                Client client = clientSvc.GetClientById(batch.ClientId);
                MailBatchInfo mbi = new MailBatchInfo(batch, client, null, Common.AuthoMailRootFolder);

                //TODO: Can we have multiple business identifier in client business?
                ClientBusinessIdentifier clientBusinessIdDetails = commonSvc.GetBusinessIdNameFromClientId(client.Id);

                if (clientBusinessIdDetails == null)
                {
                    log.Error("Business identifier is not assigned to client : {0}".Fill(client.Name));

                    //TODO: Ask to Ravi Sir if no data found then how to insert data and which value
                    continue;
                }

                var xmlFilePath = Path.Combine(mbi.AMRequestPath, mbi.BatchName + ".xml");

                if (!File.Exists(xmlFilePath))
                {
                    var error = "XML file for Batch : {0} does not exist".Fill(mbi.BatchName);

                    log.Error(error);

                    ReportInvalidBatch(batch, error);

                    continue;
                }

                XmlDocument batchXML = new XmlDocument();
                XmlNamespaceManager nsManager = null;
                XmlNodeList mailPieceNodes = null;

                try
                {
                    batchXML.LoadXml(File.ReadAllText(xmlFilePath));
                    nsManager = new XmlNamespaceManager(batchXML.NameTable);
                    nsManager.AddNamespace("x", "http://schemas.corelogic.com/2015/amc/v1/packages");
                    mailPieceNodes = batchXML.DocumentElement.ChildNodes;
                }
                catch (XmlException err)
                {
                    log.Error(err);

                    ReportInvalidBatch(batch, err.ToString());

                    continue;
                }

                if (mailPieceNodes == null || mailPieceNodes.Count == 0)
                {
                    ReportInvalidBatch(batch, "Xml does not contain any package info nodes.");

                    continue;
                }

                foreach (XmlNode mailPieceNode in mailPieceNodes)
                {
                    MailPieceDTO mailPieceDTO = new MailPieceDTO();
                    MailPieceCustomInfo customInfo = null;
                    List<MailPieceCustomInfo> mailPieceCustomInfo = new List<MailPieceCustomInfo>();

                    mailPieceDTO.ClientId = client.Id;
                    mailPieceDTO.MailBatchId = mbi.MailBatch.Id;

                    try
                    {
                        mailPieceDTO.BusinessFriendlyIdName = clientBusinessIdDetails.BusinessIdName;
                        mailPieceDTO.BusinessFriendlyIdValue = mailPieceNode.SelectSingleNode("./x:LoanNbr/text()", nsManager) == null ? string.Empty : mailPieceNode.SelectSingleNode("./x:LoanNbr/text()", nsManager).Value;
                        mailPieceDTO.DocumentOrPackageType = mailPieceNode.SelectSingleNode("./x:PackageName/text()", nsManager) == null ? null : mailPieceNode.SelectSingleNode("./x:PackageName/text()", nsManager).Value;
                        mailPieceDTO.UniqueId = mailPieceNode.SelectSingleNode("./x:Id/text()", nsManager) == null ? Guid.Empty : Guid.Parse(mailPieceNode.SelectSingleNode("./x:Id/text()", nsManager).Value);
                        mailPieceDTO.FileName = mailPieceNode.SelectSingleNode("./x:File/text()", nsManager) == null ? null : mailPieceNode.SelectSingleNode("./x:File/text()", nsManager).Value;

                        var recipientElement = mailPieceNode.SelectSingleNode("./x:RecipientInfo", nsManager);
                        if (recipientElement != null)
                        {
                            if (recipientElement.SelectNodes("./x:Recipient", nsManager) != null)
                            {
                                mailPieceDTO.IsRcptInfoSpecified = true;
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
                            else
                                mailPieceDTO.IsRcptInfoSpecified = false;

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
                        else
                        {
                            mailPieceDTO.IsRcptInfoSpecified = false;
                        }

                        var returnInfoElement = mailPieceNode.SelectSingleNode("./x:ReturnInfo", nsManager);
                        if (returnInfoElement != null)
                        {
                            mailPieceDTO.IsReturnInfoSpecified = true;
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
                                {
                                    customInfo = new MailPieceCustomInfo() { MailPieceId = 0, Key = "ShippingCredCode", DataType = DataType.String, Value = shippingCredElement.SelectSingleNode("./x:ClientCode/text()", nsManager).Value };

                                    mailPieceCustomInfo.Add(customInfo);
                                }
                            }
                        }
                        else
                            mailPieceDTO.IsReturnInfoSpecified = false;


                        ValidationResultAMC validationResult = Common.ValidateXMLNode(mailPieceNode, nsManager);
                        if (validationResult.Code == OutComeCodes.No_Error)
                        {
                            string filePath = Path.Combine(mbi.AMRequestPath, mailPieceDTO.FileName);
                            if (!File.Exists(filePath))
                            {
                                mailPieceDTO.ErrorCode = OutComeCodes.Missing_PDF;
                                mailPieceDTO.ErrorDesc = "Pdf file is not available for this package.";
                                mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
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

                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);

                        mailPieceDTO.ErrorCode = OutComeCodes.Invalid_XML;
                        mailPieceDTO.ErrorDesc = ex.ToString();
                        mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                    }

                    mailPieceSvc.InsertBatchMailPieceDetails(mailPieceDTO, mailPieceCustomInfo);
                }

                //TODO: Do we need to check all mail pieces are valid, partially valid or failed and update batch status according to that?
                var data = mailPieceSvc.GetMailPiecesByMailBatchId(mbi.MailBatch.Id);
                if (data != null)
                {
                    var validMailPieces = data.Where(c => c.ErrorCode == OutComeCodes.No_Error).ToList();

                    if (validMailPieces.Count == 0)
                    {
                        mbi.MailBatch.ValidationStatus = BatchValidationStatus.Invalid.ToString();
                        mbi.MailBatch.ValidationErrors = "Xml does not contain any package info nodes.";
                        commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, BatchStatus.Failed.ToString());
                    }
                    else if (validMailPieces.Count < data.Count)
                    {
                        mbi.MailBatch.ValidationStatus = BatchValidationStatus.PartiallyValid.ToString();
                    }
                    else
                    {
                        mbi.MailBatch.ValidationStatus = BatchValidationStatus.Valid.ToString();
                    }

                    mailBatchSvc.UpdateBatchDetails(mbi.MailBatch);
                }

                commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Validated, Common.Now);
                commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.ValidationEnd, Common.Now);
            }
        }

        protected override void AcknowledgeBatches()
        {
            log.Debug("Acknowledge job queue is started.");

            List<MailBatchDTO> validatedBatches = mailBatchSvc.GetBatchesByStatus(BatchPipelineStatus.Validated);

            if (validatedBatches == null)
            {
                log.Debug("Acknowledge job queue is empty.");
                return;
            }

            foreach (var batch in validatedBatches)
            {
                Client client = clientSvc.GetClientById(batch.ClientId);
                MailBatchInfo mbi = new MailBatchInfo(batch, client, null, Common.AuthoMailRootFolder);

                var data = mailBatchSvc.GetMailBatchDetailsByBatchId(mbi.MailBatch.Id);
                if (data != null)
                {
                    MailUtility.SendDownloadNotificationEmail(mbi, data);

                    commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Acknowledged, Common.Now);
                }
            }
        }
    }
}
