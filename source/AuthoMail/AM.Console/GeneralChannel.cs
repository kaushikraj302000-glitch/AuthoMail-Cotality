using AM.Console.Properties;
using AM.Entities;
using AM.Lib;
using AM.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace AM.Console
{
    class GeneralChannel : Channel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected IGeneralService generalSvc;

        private string channelType;

        private static XmlSchemaSet schemas;

        static GeneralChannel()
        {
            schemas = new XmlSchemaSet();
            schemas.Add("http://schemas.corelogic.com/ade/authomail/v1.18.0", XmlReader.Create(new StringReader(Resources.MailingRequest)));
        }

        public GeneralChannel(string channelCode, string channelType)
            : base(channelCode)
        {
            generalSvc = GetService<GeneralService>();
            this.channelType = channelType;
        }

        #region EnumarateBatches
        protected override void EnumerateBatches()
        {
            log.Debug("Enumerate Batches job queue for " + SFTChannel.Code + " is started.");

            List<string> triggerFilePaths = new List<string>();

            SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(SFTChannel.Protocol), SFTChannel.Host, SFTChannel.UserName, SFTChannel.Password, 300000);
            sftSite.Connect();

            triggerFilePaths = GetTriggerFilePaths(sftSite);

            foreach (string triggerFile in triggerFilePaths)
            {
                try
                {
                    string triggerFilePath = Path.Combine(SFTChannel.ToADEDir, triggerFile + "_Complete.go");

                    //Download and Insert the batch in database first always.
                    string batchName = triggerFile;

                    DateTime lastModifiedTime = Common.Now;
                    if (channelType == ChannelType.Trigger)
                    {
                        //TODO : Implement check for zip exist or not
                        lastModifiedTime = sftSite.LastModifiedTimeOfFile(triggerFilePath);
                    }

                    #region Create General Channel Directory.
                    //Here first we are cleaning AMC folder
                    string channelDownloadPath = Path.Combine(Config.Common_AuthoMailRootFolder, SFTChannel.Code.Split('_')[0]);

                    if (Directory.Exists(channelDownloadPath))
                    {
                        Directory.Delete(channelDownloadPath, true);
                    }

                    //To ensure that folder gets created.
                    Thread.Sleep(1000);
                    Directory.CreateDirectory(channelDownloadPath);

                    #endregion

                    #region Download Zip
                    string zipFileName = "{0}.zip".Fill(batchName);
                    string localZipFilePath = Path.Combine(channelDownloadPath, zipFileName);
                    string xmlFilePath = Path.Combine(channelDownloadPath, batchName + ".xml");
                    string ftpZipFilePath = Path.Combine(SFTChannel.ToADEDir, zipFileName);
                    string ftpTriggerFilePath = Path.Combine(SFTChannel.ToADEDir, "{0}_Complete.go".Fill(batchName));

                    if (!sftSite.IsFileAvailable(ftpZipFilePath))
                    {
                        var msg = "'{0}.zip' file is missing on SFT site.".Fill(batchName);
                        ReportInvalidBatch(InsertInvalidBatchInfo(batchName, lastModifiedTime, msg), "");
                        continue;
                    }

                    //Some time it is not creating AMC folder in flow so here we are again checking this if not created then below code will create
                    if (!Directory.Exists(channelDownloadPath))
                    {
                        log.Error("General Channel Folder was not created as expected.");
                        Directory.CreateDirectory(channelDownloadPath);
                    }

                    DownloadBatchZipFile(sftSite, ftpZipFilePath, ftpTriggerFilePath, localZipFilePath);

                    try
                    {
                        ZipFile.ExtractToDirectory(localZipFilePath, Path.GetDirectoryName(localZipFilePath));
                    }
                    catch (Exception ex)
                    {
                        var msg = "Unable to extract '{0}.zip' file.".Fill(batchName);
                        log.Error(msg, ex);
                        ReportInvalidBatch(InsertInvalidBatchInfo(batchName, lastModifiedTime, msg), localZipFilePath);
                        continue;
                    }
                    #endregion

                    ProcessBatch(xmlFilePath, localZipFilePath, batchName, channelDownloadPath, lastModifiedTime, sftSite);
                }
                catch (Exception ex)
                {
                    var msg = "Error occured in EnumerateBatches Method For BatchName: {0} with Error : {1}".Fill(triggerFile, ex.Message);
                    log.Error(msg, ex);
                    MailUtility.SendUnexpectedExceptionEmail(msg);
                    continue;
                }
            }
        }

        private void ProcessBatch(string xmlFilePath, string localZipFilePath, string batchName, string channelDownloadPath, DateTime receivedOnDateTime, SFTUtil sftSite)
        {
            List<MailBatchDTO> mailBatches = new List<MailBatchDTO>();

            XmlDocument batchXML = new XmlDocument();
            XmlNamespaceManager nsManager = null;
            XmlNodeList mailPieceNodes = null;
            try
            {
                batchXML.LoadXml(File.ReadAllText(xmlFilePath));
                nsManager = new XmlNamespaceManager(batchXML.NameTable);
                nsManager.AddNamespace("x", "http://schemas.corelogic.com/ade/authomail/v1.18.0");
                mailPieceNodes = batchXML.DocumentElement.ChildNodes;
            }
            catch (XmlException ex)
            {
                var msg = "'{0}.xml' is not a valid XML document.".Fill(batchName);
                log.Error(msg, ex);
                ReportInvalidBatch(InsertInvalidBatchInfo(batchName, receivedOnDateTime, msg), localZipFilePath);
                return;
            }
            catch (Exception ex)
            {
                var msg = "'{0}.xml' is not a valid XML document.".Fill(batchName);
                log.Error(msg, ex);
                ReportInvalidBatch(InsertInvalidBatchInfo(batchName, receivedOnDateTime, msg), localZipFilePath);
                return;
            }

            Client client;

            try
            {
                var clientCode = batchXML.DocumentElement.SelectSingleNode("//x:ClientCode/text()", nsManager) == null ? "InvalidCustId" : batchXML.DocumentElement.SelectSingleNode("//x:ClientCode/text()", nsManager).Value;

                client = clientSvc.GetClientByAppCode(clientCode);

                //TODO : Process for invalid ClientCode

                MailBatchDTO mailBatch = new MailBatchDTO();
                mailBatch.ClientId = client.Id;
                mailBatch.BatchName = batchName;
                mailBatch.SFTChannelId = SFTChannel.Id;
                mailBatch.ClientBatchId = batchName;
                mailBatch.UniqueId = Guid.NewGuid();
                mailBatch.Priority = Priority.Medium.ToString();
                mailBatch.ReceivedOn = receivedOnDateTime;
                mailBatch.PipelineStatus = BatchPipelineStatus.Received;
                mailBatch.BatchStatus = BatchStatus.InProgress.ToString();
                mailBatches.Add(mailBatch);
            }
            catch (Exception ex)
            {
                var msg = "Error occured while processing batch from Batch '{0}.xml'".Fill(batchName);
                log.Error(msg, ex);
                MailUtility.SendUnexpectedExceptionEmail(msg + "<br/> Error Message is {0}".Fill(ex));
                return;
            }

            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    mailBatches = mailBatchSvc.InsertBatchDetails(mailBatches);

                    foreach (var clientBatch in mailBatches)
                    {
                        MailBatchInfo mbi = new MailBatchInfo(clientBatch, client, Config.Common_AuthoMailRootFolder);

                        //Here we are moving whole directory so no need to create AMRequestPath directly.
                        Directory.Move(Path.Combine(channelDownloadPath, client.AppCode), mbi.AMRequestPath);
                        //Here we create folder structure
                        mbi.AMResponsePath.CreateDirectoryIfNotExist();
                        mbi.PnSRequestPath.CreateDirectoryIfNotExist();
                        mbi.PnSResponsePath.CreateDirectoryIfNotExist();


                        commonSvc.UpdateBatchPipeLineStatus(clientBatch.Id, BatchPipelineStatus.Downloaded, Common.Now);
                    }

                    //Here after moving all batches to particular client location , archive amc batch
                    string archiveZipPath = Path.Combine(Config.Common_AuthoMailRootFolder, "Archive", batchName + ".zip");

                    if (File.Exists(archiveZipPath))
                        File.Delete(archiveZipPath);

                    File.Move(localZipFilePath, archiveZipPath);

                    //Now delete/archive trigger and zip from SFTP
                    MoveBatchToBackupDir(sftSite, Path.Combine(SFTChannel.ToADEDir, batchName + ".zip"));
                    MoveBatchToBackupDir(sftSite, Path.Combine(SFTChannel.ToADEDir, batchName + "_Complete.go"));

                    //After complete process clean AMC folder
                    channelDownloadPath.CleanDirectory();

                    scope.Complete();

                }
                catch (Exception ex)
                {
                    var msg = "Error occured while inserting General Channel batches details for Batch '{0}.xml'".Fill(batchName);
                    log.Error(msg, ex);
                    MailUtility.SendUnexpectedExceptionEmail(msg + "<br/> Error Message is {0}".Fill(ex));
                }
            }
        }

        private string DownloadBatchZipFile(SFTUtil sftSite, string ftpZipFilePath, string ftpTriggerFilePath, string localZipFilePath)
        {
            sftSite.DownloadFile(ftpZipFilePath, localZipFilePath);

            return localZipFilePath;
        }

        private void ReportInvalidBatch(MailBatchDTO mailBatch, string localZipFilePath)
        {
            Client client = clientSvc.GetClientById(mailBatch.ClientId);

            Deliverable deliverable = commonSvc.GetDeliverableByReportType(client.Id, ReportTypes.Error);

            MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, Config.Common_AuthoMailRootFolder);

            mbi.AMRequestPath.CreateDirectoryIfNotExist();
            mbi.AMResponsePath.CreateDirectoryIfNotExist();

            if (!string.IsNullOrEmpty(localZipFilePath))
                File.Move(localZipFilePath, Path.Combine(mbi.AMRequestPath, mbi.BatchName + ".zip"));

            //For Invalid Batch, we will generate err file 
            string invalidBatchFileName = "{0}_InvalidZipfile.err".Fill(mailBatch.BatchName);

            string invalidBatchPath = Path.Combine(mbi.AMResponsePath, invalidBatchFileName);

            File.WriteAllText(invalidBatchPath, string.Empty);

            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportPrepped, Common.Now);

            string remoteFilePath = Path.Combine(deliverable.UploadPath, invalidBatchFileName);

            SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(SFTChannel.Protocol), SFTChannel.Host, SFTChannel.UserName, SFTChannel.Password, 300000);
            sftSite.Connect();

            sftSite.UploadFile(invalidBatchPath, remoteFilePath);
            if (sftSite.IsFileAvailable(Path.Combine(SFTChannel.ToADEDir, mbi.BatchName + ".zip")))
                MoveBatchToBackupDir(sftSite, Path.Combine(SFTChannel.ToADEDir, mbi.BatchName + ".zip"));

            MoveBatchToBackupDir(sftSite, Path.Combine(SFTChannel.ToADEDir, mbi.BatchName + "_Complete.go"));

            if (sftSite.IsConnected)
                sftSite.Disconnect();

            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportedBack, Common.Now);
        }

        private void MoveBatchToBackupDir(SFTUtil sftSite, string ftpFilePath)
        {
            string ftpBackUpFilePath = Path.Combine(SFTChannel.ArchiveDir, Path.GetFileName(ftpFilePath));

            if (SFTChannel.PostDownloadAction == PostDownloadAction.Archive)
            {
                sftSite.MoveFile(ftpFilePath, ftpBackUpFilePath);
            }
            else
                sftSite.DeleteFile(ftpFilePath);
        }

        private MailBatchDTO InsertInvalidBatchInfo(string batchName, DateTime receivedOn, string errorMessage)
        {
            MailBatchDTO mailBatch = new MailBatchDTO();
            mailBatch.UniqueId = Guid.NewGuid();
            mailBatch.ClientId = GetInvalidClient().Id;
            mailBatch.SFTChannelId = SFTChannel.Id;
            mailBatch.BatchName = batchName;
            mailBatch.ClientBatchId = batchName;
            mailBatch.Priority = Priority.Medium.ToString();
            mailBatch.Id = mailBatchSvc.InsertBatchDetails(mailBatch);
            commonSvc.UpdateBatchPipeLineStatus(mailBatch.Id, BatchPipelineStatus.Received, receivedOn);
            mailBatchSvc.MarkMailBatchAsInvalid(mailBatch.Id, errorMessage);
            commonSvc.UpdateBatchStatus(mailBatch.Id, BatchStatus.Failed.ToString());
            return mailBatch;
        }

        private Client GetInvalidClient()
        {
            return clientSvc.GetClientByAppCode("AMCInvalid");
        }

        private List<string> GetTriggerFilePaths(SFTUtil sftSite)
        {
            var regex = new Regex(@"^(\w*)_Complete.go$", RegexOptions.IgnoreCase);

            List<string> availableTriggerFilePaths = sftSite.ListDirectory(SFTChannel.ToADEDir)
                .Select(f => regex.Match(Path.GetFileName(f)))
                .Where(m => m.Success)
                .Select(m => m.Groups[1].Value)
                .ToList();

            if (availableTriggerFilePaths != null && availableTriggerFilePaths.Count > 0)
            {
                log.Debug(string.Format("{0} trigger file(s) found.", availableTriggerFilePaths.Count));
            }
            else
            {
                log.Error("No trigger files found hence no batches are available to download.");
            }

            return availableTriggerFilePaths;
        }

        private List<string> GetXmlFileNames(SFTUtil sftSite)
        {
            var regex = new Regex(@"^(.*).xml$", RegexOptions.IgnoreCase);

            List<string> availableTriggerFilePaths = sftSite.ListDirectory(SFTChannel.ToADEDir)
                .Select(f => regex.Match(Path.GetFileName(f)))
                .Where(m => m.Success)
                .Select(m => m.Groups[1].Value)
                .ToList();

            if (availableTriggerFilePaths != null && availableTriggerFilePaths.Count > 0)
            {
                log.Debug(string.Format("{0} xml file(s) found.", availableTriggerFilePaths.Count));
            }
            else
            {
                log.Error("No trigger files found hence no batches are available to download.");
            }

            return availableTriggerFilePaths;
        }
        #endregion

        protected override void DownloadBatches()
        {

        }

        #region Validate Batches
        protected override void ValidateBatches()
        {
            log.Debug("Validate job queue is started.");

            List<MailBatchDTO> downloadBatches = mailBatchSvc.GetBatchesByPipelineStatusAndSFTChannelId(BatchPipelineStatus.Downloaded, SFTChannel.Id);

            if (downloadBatches == null)
            {
                log.Debug("Validate job queue is empty.");
                return;
            }

            foreach (var mailBatch in downloadBatches)
            {
                try
                {
                    Client client = clientSvc.GetClientById(mailBatch.ClientId);

                    MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, Config.Common_AuthoMailRootFolder);


                    var xmlFilePath = Path.Combine(mbi.AMRequestPath, mbi.BatchName + ".xml");

                    XmlDocument batchXML = new XmlDocument();
                    XmlNamespaceManager nsManager = null;
                    XmlNodeList mailPieceNodes = null;

                    try
                    {
                        batchXML.LoadXml(File.ReadAllText(xmlFilePath));
                        nsManager = new XmlNamespaceManager(batchXML.NameTable);
                        nsManager.AddNamespace("x", "http://schemas.corelogic.com/ade/authomail/v1.18.0");
                        mailPieceNodes = batchXML.DocumentElement.ChildNodes;

                        mailBatch.UniqueId = batchXML.DocumentElement.Attributes["Id"] == null ? Guid.NewGuid() : Guid.Parse(batchXML.DocumentElement.Attributes["Id"].Value);
                        mailBatch.Priority = batchXML.DocumentElement.Attributes["Priority"] == null ? Priority.Medium.ToString() : batchXML.DocumentElement.Attributes["Priority"].Value;
                        mailBatch.DocumentOrPackageType = batchXML.DocumentElement.Attributes["DocumentOrPackageType"] == null ? null : batchXML.DocumentElement.Attributes["DocumentOrPackageType"].Value;
                    }
                    catch (XmlException ex)
                    {
                        var msg = "'{0}.xml' is not a valid XML document.".Fill(mbi.BatchName);
                        log.Error(msg, ex);

                        ReportInvalidBatch(mailBatch, "");

                        continue;
                    }
                    catch (Exception ex)
                    {
                        var msg = "'{0}.xml' is not a valid XML document.".Fill(mbi.BatchName);
                        log.Error(msg, ex);
                        ReportInvalidBatch(mailBatch, "");
                        continue;
                    }

                    foreach (XmlNode mailPieceNode in mailPieceNodes)
                    {
                        MailPieceDTO mailPieceDTO = new MailPieceDTO();
                        MailPieceCustomInfo customInfo = null;
                        List<MailPieceCustomInfo> mailPieceCustomInfo = new List<MailPieceCustomInfo>();

                        mailPieceDTO.ClientId = client.Id;
                        mailPieceDTO.MailBatchId = mbi.MailBatch.Id;
                        mailPieceDTO.ShippingStatus = ShippingStatus.InProgress.ToString();

                        try
                        {
                            var requests = XmlUtil.Deserialize<MailPiece>(mailPieceNode.OuterXml);

                            mailPieceDTO.BusinessFriendlyIdName = requests.BusinessFriendlyIdName;

                            //Here we are inserting BusinessIdentifier for Client if not exist.
                            ClientBusinessIdentifier clientBusinessIdDetails = commonSvc.GetBusinessIdNameFromClientId(client.Id);
                            if (clientBusinessIdDetails == null)
                            {
                                commonSvc.InsertBusinessIdentifierName(client.Id, mailPieceDTO.BusinessFriendlyIdName);
                            }

                            mailPieceDTO.BusinessFriendlyIdValue = requests.BusinessFriendlyIdValue;
                            mailPieceDTO.DocumentOrPackageType = requests.DocumentOrPackageType;
                            mailPieceDTO.UniqueId = requests.Id == null ? Guid.NewGuid() : requests.Id.Value;
                            mailPieceDTO.FileName = requests.DocumentOrPackage.Name;
                            mailPieceDTO.FilePattern = requests.DocumentOrPackage.Pattern;

                            //TODO : What to do with Base64String?

                            if (requests.RecipientInfo != null)
                            {
                                if (requests.RecipientInfo.Recipient != null)
                                {
                                    mailPieceDTO.IsRcptInfoSpecified = true;

                                    mailPieceDTO.RcptFirstName = requests.RecipientInfo.Recipient.FirstName;
                                    mailPieceDTO.RcptLastName = requests.RecipientInfo.Recipient.LastName;
                                    mailPieceDTO.RcptMiddleName = requests.RecipientInfo.Recipient.MiddleName;
                                    mailPieceDTO.RcptOrganization = requests.RecipientInfo.Recipient.Organization;
                                    mailPieceDTO.RcptPrefix = requests.RecipientInfo.Recipient.Prefix;
                                    mailPieceDTO.RcptSuffix = requests.RecipientInfo.Recipient.Suffix;
                                    mailPieceDTO.RcptPhone = requests.RecipientInfo.Recipient.Phone;
                                }
                                else
                                    mailPieceDTO.IsRcptInfoSpecified = false;

                                if (requests.RecipientInfo.Corecipient != null)
                                {
                                    mailPieceDTO.CoRcptFirstName = requests.RecipientInfo.Corecipient.FirstName;
                                    mailPieceDTO.CoRcptLastName = requests.RecipientInfo.Corecipient.LastName;
                                    mailPieceDTO.CoRcptMiddleName = requests.RecipientInfo.Corecipient.MiddleName;
                                    mailPieceDTO.CoRcptOrganization = requests.RecipientInfo.Corecipient.Organization;
                                    mailPieceDTO.CoRcptPrefix = requests.RecipientInfo.Corecipient.Prefix;
                                    mailPieceDTO.CoRcptSuffix = requests.RecipientInfo.Corecipient.Suffix;
                                    mailPieceDTO.CoRcptPhone = requests.RecipientInfo.Corecipient.Phone;
                                }

                                if (requests.RecipientInfo.Address != null)
                                {
                                    mailPieceDTO.RcptAddrOptionalLine1AboveStreet = requests.RecipientInfo.Address.OptionalLine1AboveStreet;
                                    mailPieceDTO.RcptAddrOptionalLine2AboveStreet = requests.RecipientInfo.Address.OptionalLine2AboveStreet;

                                    mailPieceDTO.RcptAddressStreet = requests.RecipientInfo.Address.Street;

                                    mailPieceDTO.RcptAddrOptionalLine1BelowStreet = requests.RecipientInfo.Address.OptionalLine1BelowStreet;
                                    mailPieceDTO.RcptAddrOptionalLine2BelowStreet = requests.RecipientInfo.Address.OptionalLine2BelowStreet;

                                    mailPieceDTO.RcptAddressCity = requests.RecipientInfo.Address.City;
                                    mailPieceDTO.RcptAddressState = requests.RecipientInfo.Address.State;
                                    mailPieceDTO.RcptAddressZIP = requests.RecipientInfo.Address.Zip;
                                }
                            }
                            else
                            {
                                mailPieceDTO.IsRcptInfoSpecified = false;
                            }

                            if (requests.ReturnInfo == null)
                            {
                                mailPieceDTO.IsReturnInfoSpecified = false;
                            }
                            else
                            {
                                mailPieceDTO.IsReturnInfoSpecified = true;
                                if (requests.ReturnInfo.Recipient != null)
                                {
                                    mailPieceDTO.RetRcptFirstName = requests.ReturnInfo.Recipient.FirstName;

                                    mailPieceDTO.RetRcptLastName = requests.ReturnInfo.Recipient.LastName;
                                    mailPieceDTO.RetRcptMiddleName = requests.ReturnInfo.Recipient.MiddleName;
                                    mailPieceDTO.RetRcptOrganization = requests.ReturnInfo.Recipient.Organization;
                                    mailPieceDTO.RetRcptPrefix = requests.ReturnInfo.Recipient.Prefix;
                                    mailPieceDTO.RetRcptSuffix = requests.ReturnInfo.Recipient.Suffix;
                                    mailPieceDTO.RetRcptPhone = requests.ReturnInfo.Recipient.Phone;
                                }

                                if (requests.ReturnInfo.Address != null)
                                {
                                    mailPieceDTO.RetRcptAddrOptionalLine1AboveStreet = requests.ReturnInfo.Address.OptionalLine1AboveStreet;
                                    mailPieceDTO.RetRcptAddrOptionalLine2AboveStreet = requests.ReturnInfo.Address.OptionalLine2AboveStreet;

                                    mailPieceDTO.RetRcptAddressStreet = requests.ReturnInfo.Address.Street;

                                    mailPieceDTO.RetRcptAddrOptionalLine1BelowStreet = requests.ReturnInfo.Address.OptionalLine1BelowStreet;
                                    mailPieceDTO.RetRcptAddrOptionalLine2BelowStreet = requests.ReturnInfo.Address.OptionalLine2BelowStreet;

                                    mailPieceDTO.RetRcptAddressCity = requests.ReturnInfo.Address.City;
                                    mailPieceDTO.RetRcptAddressState = requests.ReturnInfo.Address.State;
                                    mailPieceDTO.RetRcptAddressZIP = requests.ReturnInfo.Address.Zip;
                                }
                            }

                            if (requests.CustomInfo != null)
                            {
                                foreach (var cstmInfo in requests.CustomInfo)
                                {
                                    customInfo = new MailPieceCustomInfo() { MailPieceId = 0, Key = cstmInfo.Key, DataType = cstmInfo.DataType, Value = cstmInfo.Value };

                                    mailPieceCustomInfo.Add(customInfo);
                                }
                            }
                            //First System will check for schema validation
                            MailPieceValidationResult schemaValidationResult = ValidateMailPieceNode(mailPieceNode);
                            if (schemaValidationResult.Code == OutComeCodes.No_Error)
                            {
                                // System will check for client specific validations
                                MailPieceValidationResult clientSpecificValidationResult = ValidateClientSpecificRules(mailPieceNode);
                                if (clientSpecificValidationResult.Code == OutComeCodes.No_Error)
                                {
                                    //in last system will check for general validations
                                    string filePath = Path.Combine(mbi.AMRequestPath, mailPieceDTO.FileName);

                                    string documentOrPackageType = mailPieceDTO.DocumentOrPackageType;

                                    //Document Or Package Type validation
                                    var docType = docTypeSvc.GetDocTypeByDocTypeName(mailPieceDTO.ClientId, documentOrPackageType);

                                    if (docType == null)
                                    {
                                        mailPieceDTO.ErrorCode = OutComeCodes.Invalid_PackageType;
                                        mailPieceDTO.ErrorDesc = "The specified document or package type '{0}' is not configured.".Fill(mailPieceDTO.DocumentOrPackageType);
                                        mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                                    }
                                    else if (!File.Exists(filePath))
                                    {
                                        mailPieceDTO.ErrorCode = OutComeCodes.Missing_PDF_File;
                                        mailPieceDTO.ErrorDesc = "Pdf file is not available for this package.";
                                        mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                                    }
                                    else
                                    {
                                        mailPieceDTO.ErrorCode = OutComeCodes.No_Error;
                                        mailPieceDTO.DocTypeId = docType.Id;
                                    }
                                }
                                else
                                {
                                    mailPieceDTO.ErrorCode = clientSpecificValidationResult.Code;
                                    mailPieceDTO.ErrorDesc = clientSpecificValidationResult.ErrorMessage;
                                    mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                                }
                            }
                            else
                            {
                                mailPieceDTO.ErrorCode = schemaValidationResult.Code;
                                mailPieceDTO.ErrorDesc = schemaValidationResult.ErrorMessage;
                                mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                            }
                        }
                        catch (Exception ex)
                        {
                            var msg = "Unable to process package info node for mail batch {0}".Fill(mbi.BatchName);

                            log.Error(msg, ex);

                            mailPieceDTO.ErrorCode = OutComeCodes.Unknown_Error;
                            mailPieceDTO.ErrorDesc = msg + Environment.NewLine + ex.Message;
                            mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                        }

                        try
                        {
                            mailPieceSvc.InsertBatchMailPieceDetails(mailPieceDTO, mailPieceCustomInfo);
                        }
                        catch (SqlException ex)
                        {
                            //2601 error code used for duplicate unique key insert 
                            if (ex.Number == 2601)
                            {
                                var msg = "{0} key id is duplicate in package info node.".Fill(mailPieceDTO.UniqueId);

                                log.Error(msg, ex);
                                mailPieceDTO.UniqueId = Guid.NewGuid();
                                mailPieceDTO.ErrorCode = OutComeCodes.Invalid_DuplicateKey;
                                mailPieceDTO.ErrorDesc = msg + Environment.NewLine + ex.Message;
                                mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                                mailPieceSvc.InsertBatchMailPieceDetails(mailPieceDTO, mailPieceCustomInfo);
                            }
                        }
                    }

                    //TODO: Do we need to check all mail pieces are valid, partially valid or failed and update batch status according to that?

                    var data = mailPieceSvc.GetMailPiecesByMailBatchId(mbi.MailBatch.Id);
                    if (data != null)
                    {
                        var validMailPieces = data.Where(c => c.ErrorCode == OutComeCodes.No_Error).ToList();

                        if (validMailPieces.Count == 0)
                        {
                            mbi.MailBatch.ValidationStatus = BatchValidationStatus.Invalid.ToString();
                            mbi.MailBatch.ValidationErrors = "Xml does not contain any valid package info nodes.";
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

                        commonSvc.UpdateBatchValidationStatus(mbi.MailBatch.Id, mbi.MailBatch.ValidationStatus, mbi.MailBatch.ValidationErrors);
                    }

                    commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Validated, Common.Now);
                }
                catch (Exception ex)
                {
                    var msg = "Error occured in ValidateBatches Method For Mail Batch Id: {0} with Error : {1}".Fill(mailBatch.Id, ex.Message);
                    log.Error(msg, ex);
                    MailUtility.SendUnexpectedExceptionEmail(msg);
                    continue;
                }
            }
            log.Debug("Validate job queue is completed.");
        }

        private MailPieceValidationResult ValidateClientSpecificRules(XmlNode mailPieceNode)
        {
            throw new NotImplementedException();
        }

        private MailPieceValidationResult ValidateMailPieceNode(XmlNode xmlPiece)
        {
            var errors = GetAllMailPieceErrors(xmlPiece).ToList();

            if (errors.Count > 0)
            {
                return new MailPieceValidationResult() { Code = errors.First().Code, ErrorMessage = string.Join(" | ", errors.Select(row => row.ErrorMessage)) };
            }

            return new MailPieceValidationResult() { Code = OutComeCodes.No_Error };
        }

        private IEnumerable<MailPieceValidationResult> GetAllMailPieceErrors(XmlNode xmlPiece)
        {
            XDocument xDoc = XDocument.Parse(xmlPiece.OuterXml);
            StringBuilder sbErrorMessage = new StringBuilder();

            xDoc.Validate(schemas, (o, e) =>
            {
                sbErrorMessage.Append(e.Message);
            });

            string errorMsg = sbErrorMessage.ToString();

            if (errorMsg.Contains("Address") || errorMsg.Contains("City") || errorMsg.Contains("State") || errorMsg.Contains("Zip"))
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Mailing_Address_Missing_Or_Invalid, ErrorMessage = "Recipient address details are missing or invalid in package." };

            if (errorMsg.Contains("Recipient") || errorMsg.Contains("FirstName") || errorMsg.Contains("LastName"))
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Recipient_Details_Missing_Or_Invalid, ErrorMessage = "Recipient details are missing or invalid in package." };

            if (!string.IsNullOrEmpty(errorMsg))
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Invalid_XML_File, ErrorMessage = errorMsg };

            yield break;
        }
        #endregion

        protected override void ValidateAddress()
        {
            Channel.ValidateAddressType(SFTChannel.Id);
        }

        protected override void CalculateDueDateAndPrintLocation()
        {
            Channel.CalculateDueDateAndPrintLocation(SFTChannel.Id);
        }

        protected override void AcknowledgeBatches()
        {
            log.Debug("Acknowledge job queue is started.");

            List<MailBatchDTO> validatedBatches = mailBatchSvc.GetBatchesByPipelineStatusAndSFTChannelId(BatchPipelineStatus.Validated, SFTChannel.Id);

            if (validatedBatches == null)
            {
                log.Debug("Acknowledge job queue is empty.");
                return;
            }

            foreach (var mailBatch in validatedBatches)
            {
                try
                {
                    Client client = clientSvc.GetClientById(mailBatch.ClientId);

                    Deliverable deliverable = commonSvc.GetDeliverableByReportType(client.Id, ReportTypes.Error);

                    MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, Config.Common_AuthoMailRootFolder);

                    List<MailPieces> mailPieces;
                    using (IReportService reportSvc = new ReportService())
                    {
                        mailPieces = reportSvc.GetMailPiecesDataFromMailBatchId(mbi.MailBatch.Id);
                    }

                    if (mailPieces != null)
                    {
                        try
                        {
                            //Here we are doing generic flow for AMC.
                            UploadBaseReportToClient(mbi, mailPieces, deliverable);

                            //Here we have to provide clientwise notification so only reporting functionality will be customized over here
                            var emailErrorReportPath = GenerateClientWiseAcknowledgementReport(mbi, mailPieces, deliverable);

                            MailUtility.SendDownloadNotificationEmail(mbi, mailPieces, emailErrorReportPath, deliverable.EmailChannel);
                            //After upload change extension of local file Path
                            if (File.Exists(emailErrorReportPath))
                                File.Move(emailErrorReportPath, Path.ChangeExtension(emailErrorReportPath, ".uploaded"));
                        }
                        catch (Exception ex)
                        {
                            log.Error("Error occured while acknowleding for Batch-Id : {0}".Fill(mbi.BatchName), ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    var msg = "Error occured in AcknowledgeBatches Method For Mail Batch Id: {0} with Error : {1}".Fill(mailBatch.Id, ex.Message);
                    log.Error(msg, ex);
                    MailUtility.SendUnexpectedExceptionEmail(msg);
                    continue;
                }
            }

            log.Debug("Acknowledge job queue is completed.");
        }

        private string GenerateClientWiseAcknowledgementReport(MailBatchInfo mbi, List<MailPieces> mailPieces, Deliverable reportDeliverable)
        {
            string errorReportPath = string.Empty;
            string errorReportFileName = string.Empty;
            StringBuilder errorReportData = new StringBuilder();

            switch (mbi.Client.AppCode)
            {
                case "HSBC":
                    using (IHSBCService hsbcSvc = new HSBCService())
                    {
                        errorReportFileName = "{0}.ErrorReport_{1}.csv".Fill(mbi.BatchName.Split('[')[0], mbi.MailBatch.ReceivedOn.Value.UtcToClientTimeZone(mbi.Client.TimeZone).ToString("htt"));

                        errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);

                        //Here we are generating error report even though it does not have any errored records
                        errorReportData = hsbcSvc.HSBCErrorReport(mailPieces, mbi.Client.TimeZone, reportDeliverable.Delimeter);

                        File.WriteAllText(errorReportPath, errorReportData.ToString());
                    }
                    break;
                default:
                    errorReportFileName = "Error-{0}-{1}-{2}-{3}.txt".Fill(mbi.BatchName, mbi.Client.AppCode, mbi.MailBatch.Id, Common.Now.ToString("yyyyMMdd-HHmmss"));
                    errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);
                    errorReportData = generalSvc.GeneralErrorReport(mailPieces, reportDeliverable.Delimeter);
                    if (errorReportData != null)
                    {
                        File.WriteAllText(errorReportPath, errorReportData.ToString());
                    }
                    break;
            }

            return errorReportPath;
        }

        private void UploadBaseReportToClient(MailBatchInfo mbi, List<MailPieces> mailPieces, Deliverable deliverable)
        {
            string errorReportFileName = "Error-{0}-{1}-{2}-{3}.txt".Fill(mbi.BatchName, mbi.Client.AppCode, mbi.MailBatch.Id, Common.Now.ToString("yyyyMMdd-HHmmss"));
            string errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);

            string errorGoFileName = "ErrReport.go";
            string errorGoFilePath = Path.Combine(mbi.AMResponsePath, errorGoFileName);
            string ftpErrorGoUploadPath = Path.Combine(deliverable.UploadPath, errorGoFileName);

            if (mbi.MailBatch.ValidationStatus == BatchValidationStatus.Invalid)
            {
                StringBuilder errorData = generalSvc.GeneralErrorReport(mailPieces, deliverable.Delimeter);
                if (errorData != null)
                {
                    File.WriteAllText(errorReportPath, errorData.ToString());

                    SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(SFTChannel.Protocol), SFTChannel.Host, SFTChannel.UserName, SFTChannel.Password, 300000);
                    sftSite.Connect();

                    var ftpUploadPath = Path.Combine(deliverable.UploadPath, errorReportFileName);

                    sftSite.UploadFile(errorReportPath, ftpUploadPath);

                    // Upload Error.GO file after uploading error report
                    File.WriteAllText(errorGoFilePath, string.Empty);

                    sftSite.UploadFile(errorGoFilePath, ftpErrorGoUploadPath);

                    if (sftSite.IsConnected)
                        sftSite.Disconnect();

                    // Batch status Reported Back
                    commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportedBack, Common.Now);
                }
            }
            else if (mbi.MailBatch.ValidationStatus == BatchValidationStatus.PartiallyValid)
            {
                StringBuilder errorData = generalSvc.GeneralErrorReport(mailPieces, deliverable.Delimeter);

                if (errorData != null)
                {
                    File.WriteAllText(errorReportPath, errorData.ToString());

                    SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(SFTChannel.Protocol), SFTChannel.Host, SFTChannel.UserName, SFTChannel.Password, 300000);
                    sftSite.Connect();

                    var ftpUploadPath = Path.Combine(deliverable.UploadPath, errorReportFileName);

                    sftSite.UploadFile(errorReportPath, ftpUploadPath);

                    // Upload Error.GO file after uploading error report
                    File.WriteAllText(errorGoFilePath, string.Empty);

                    sftSite.UploadFile(errorGoFilePath, ftpErrorGoUploadPath);

                    if (sftSite.IsConnected)
                        sftSite.Disconnect();

                }
                commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Acknowledged, Common.Now);
            }
            else
            {
                commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Acknowledged, Common.Now);
            }
        }

        private void ReportInvalidBatch(MailBatchInfo mbi, string msg, Deliverable deliverables)
        {
            MailUtility.SendNoDataNotificationEmail(mbi, deliverables.EmailChannel, msg);
            //For Invalid Batch, we will generate error File
            mailBatchSvc.MarkMailBatchAsInvalid(mbi.MailBatch.Id, msg);

            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportedBack, Common.Now);

            commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, BatchStatus.Failed.ToString());
        }
    }
}
