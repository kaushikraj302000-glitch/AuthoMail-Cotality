using AM.Console.Properties;
using AM.Entities;
using AM.Lib;
using AM.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Data.SqlClient;

namespace AM.Console
{
    class AMCChannel : Channel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected IAMCService amcSvc;

        protected Client client;

        private static XmlSchemaSet schemas;

        static AMCChannel()
        {
            schemas = new XmlSchemaSet();
            schemas.Add("http://schemas.corelogic.com/2015/amc/v1/packages", XmlReader.Create(new StringReader(Resources.PackageInfoSchema)));
        }


        public AMCChannel(string channelCode)
            : base(channelCode)
        {
            amcSvc = GetService<AMCService>();
            client = clientSvc.GetClientByAppCode("USAA");
        }

        #region EnumerateBatches
        protected override void EnumerateBatches()
        {
            log.Debug("EnumerateBatches job queue is started.");

            List<string> triggerFilePaths = new List<string>();

            SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(SFTChannel.Protocol), SFTChannel.Host, SFTChannel.UserName, SFTChannel.Password, 300000);
            sftSite.Connect();

            triggerFilePaths = GetTriggerFilePaths(sftSite);

            foreach (string triggerFile in triggerFilePaths)
            {
                MailBatchDTO mailBatch = null;

                string triggerFilePath = Path.Combine(SFTChannel.ToADEDir, triggerFile + "_Complete.go");

                //Download and Insert the batch in database first always.
                string batchName = triggerFile;

                mailBatch = new MailBatchDTO();
                mailBatch.ClientId = client.Id;
                mailBatch.BatchName = batchName;
                mailBatch.SFTChannelId = SFTChannel.Id;
                mailBatch.ClientBatchId = batchName;
                mailBatch.UniqueId = Guid.NewGuid();
                mailBatch.Priority = Priority.Medium.ToString();
                mailBatch.PrintLocation = Config.Common_PrintLocation;

                //Insert New Batch Entry
                mailBatch.Id = mailBatchSvc.InsertBatchDetails(mailBatch);

                //Update Pipeline Status of Batch Received
                commonSvc.UpdateBatchPipeLineStatus(mailBatch.Id, BatchPipelineStatus.Received, sftSite.LastModifiedTimeOfFile(triggerFilePath));
                commonSvc.UpdateBatchStatus(mailBatch.Id, BatchStatus.InProgress.ToString());

                //Delete or move trigger file after status update as Received
                MoveTriggerFileToBackupDir(sftSite, triggerFilePath);
            }

            if (sftSite.IsConnected)
                sftSite.Disconnect();
        }

        private void MoveTriggerFileToBackupDir(SFTUtil sftSite, string triggerFTPFilePath)
        {
            string triggerFTPBackUpFilePath = Path.Combine(SFTChannel.ArchiveDir, Path.GetFileName(triggerFTPFilePath));

            if (SFTChannel.PostDownloadAction == PostDownloadAction.Archive)
            {
                sftSite.MoveFile(triggerFTPFilePath, triggerFTPBackUpFilePath);
            }
            else
                sftSite.DeleteFile(triggerFTPFilePath);
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
        #endregion

        #region DownloadBatches
        protected override void DownloadBatches()
        {
            log.Debug("Download job queue is started.");

            List<MailBatchDTO> downloadBatches = mailBatchSvc.GetBatchesByPipelineStatus(BatchPipelineStatus.Received, client.Id);
            if (downloadBatches == null)
            {
                log.Debug("Download job queue is empty.");
                return;
            }

            SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(SFTChannel.Protocol), SFTChannel.Host, SFTChannel.UserName, SFTChannel.Password, 300000);
            sftSite.Connect();


            foreach (var batch in downloadBatches)
            {
                MailBatchInfo mbi = new MailBatchInfo(batch, client, Config.Common_AuthoMailRootFolder);

                try
                {
                    #region Create directory structure.

                    mbi.BatchRepo.CleanDirectory();
                    mbi.AMRequestPath.CreateDirectoryIfNotExist();
                    mbi.PnSRequestPath.CreateDirectoryIfNotExist();
                    mbi.PnSResponsePath.CreateDirectoryIfNotExist();
                    mbi.AMResponsePath.CreateDirectoryIfNotExist();

                    #endregion

                    string localZipFilePath = string.Empty;
                    string zipFileName = "{0}.zip".Fill(mbi.BatchName);
                    string ftpZipFilePath = Path.Combine(SFTChannel.ToADEDir, zipFileName);
                    string ftpTriggerFilePath = Path.Combine(SFTChannel.ToADEDir, "{0}_Complete.go".Fill(mbi.BatchName));

                    if (!sftSite.IsFileAvailable(ftpZipFilePath))
                    {
                        ReportInvalidBatch(mbi, "'{0}.zip' file is missing on SFT site.".Fill(mbi.BatchName));
                        continue;
                    }

                    localZipFilePath = DownloadBatchZipFile(sftSite, ftpZipFilePath, ftpTriggerFilePath, mbi);

                    try
                    {
                        ZipFile.ExtractToDirectory(localZipFilePath, mbi.AMRequestPath);
                    }
                    catch (Exception ex)
                    {
                        var msg = "Unable to extract '{0}.zip' file. Batch ID : {1} ".Fill(mbi.BatchName, mbi.MailBatch.Id);

                        log.Error(msg, ex);

                        ReportInvalidBatch(mbi, msg);
                        continue;
                    }

                    //Based on post download action zip file and trigger file will get moved or delete from that location
                    if (SFTChannel.PostDownloadAction == PostDownloadAction.Archive)
                    {
                        //Move zip file
                        string archiveFTPZipFilePath = Path.Combine(SFTChannel.ArchiveDir, zipFileName);

                        sftSite.MoveFile(ftpZipFilePath, archiveFTPZipFilePath);
                    }
                    else
                    {
                        sftSite.DeleteFile(ftpZipFilePath);
                    }

                    commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Downloaded, Common.Now);
                }
                catch (Exception ex)
                {
                    var error = "Error occured while downloading {0} batch from SFT site for Batch-Id : {1}".Fill(mbi.BatchName, mbi.MailBatch.Id) + Environment.NewLine + ex.ToString();

                    log.Error(error);

                    continue;
                }
            }
            if (sftSite.IsConnected)
                sftSite.Disconnect();

            log.Debug("Download job queue is completed.");

            //if (Config.Common_Environment != Env.Prod)
            //{
            // Separate batches for USAA from AMC
            SeparateBatches();
            //}
        }

        private string DownloadBatchZipFile(SFTUtil sftSite, string ftpZipFilePath, string ftpTriggerFilePath, MailBatchInfo mbi)
        {
            string triggerFileName = Path.GetFileName(ftpTriggerFilePath);
            string zipFileName = Path.GetFileName(ftpZipFilePath);

            string localZipFilePath = Path.Combine(mbi.AMRequestPath, zipFileName);

            sftSite.DownloadFile(ftpZipFilePath, localZipFilePath);

            return localZipFilePath;
        }
        #endregion

        private void ReportInvalidBatch(MailBatchInfo mbi, string error)
        {
            //For Invalid Batch, we will generate err file 
            string invalidBatchFileName = "{0}_InvalidZipfile.err".Fill(mbi.BatchName);
            string invalidBatchPath = Path.Combine(mbi.AMResponsePath, invalidBatchFileName);

            File.WriteAllText(invalidBatchPath, string.Empty);

            mailBatchSvc.MarkMailBatchAsInvalid(mbi.MailBatch.Id, error);

            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportPrepped, Common.Now);

            commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, BatchStatus.Failed.ToString());
        }

        #region Separate Batches
        protected void SeparateBatches()
        {
            log.Debug("Separate batches job queue is started.");
            List<MailBatchDTO> downloadBatches = mailBatchSvc.GetBatchesByPipelineStatus(BatchPipelineStatus.Downloaded, client.Id);
            if (downloadBatches == null)
            {
                log.Debug("Separating job queue is empty.");
                return;
            }

            foreach (var batch in downloadBatches)
            {
                MailBatchInfo mbi = new MailBatchInfo(batch, client, Config.Common_AuthoMailRootFolder);

                var xmlFilePath = Path.Combine(mbi.AMRequestPath, mbi.BatchName + ".xml");
                if (!File.Exists(xmlFilePath))
                {
                    ReportInvalidBatch(mbi, "Xml file does not exist in Batch-{0}".Fill(batch.BatchName));

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
                catch (XmlException ex)
                {
                    var msg = "'{0}.xml' is not a valid XML document.".Fill(mbi.BatchName);
                    log.Error(msg, ex);

                    ReportInvalidBatch(mbi, msg);

                    continue;
                }
                catch (Exception ex)
                {
                    var msg = "'{0}.xml' is not a valid XML document.".Fill(mbi.BatchName);
                    log.Error(msg, ex);
                    ReportInvalidBatch(mbi, ex.Message);
                    continue;
                }

                if (mailPieceNodes == null)
                {
                    ReportInvalidBatch(mbi, "Xml does not contain any package info nodes.");

                    continue;
                }

                try
                {
                    PackageRequests requests = null;
                    PackageRequests amcRequest = new PackageRequests();

                    requests = XmlUtil.Deserialize<PackageRequests>(batchXML.OuterXml);

                    List<PackageInfo> packages = requests.Packages.FindAll(p => p.CustomerIdentifier != Config.Monitor_USAACustIdentifier);

                    if (packages != null && packages.Count > 0)
                    {
                        amcRequest.Packages = packages;
                        SplitXMLAndPDFS(amcRequest, Path.GetDirectoryName(xmlFilePath), mbi.BatchName);
                        requests.Packages.RemoveAll(p => p.CustomerIdentifier != Config.Monitor_USAACustIdentifier);
                        //After removing other document types from package request we are checking for any remaining USAA packages are there or not if not then we don't need to process that batch further so we will remove that batch's records and files from database and from folder
                        if (requests.Packages.Count == 0)
                        {
                            //TODO: Ask to Ravi Sir
                            DeleteBatchInfo(mbi.MailBatch.Id, mbi.BatchRepo);
                            continue;
                        }

                        if (File.Exists(xmlFilePath))
                            File.Delete(xmlFilePath);

                        File.WriteAllText(xmlFilePath, XmlUtil.Serialize<PackageRequests>(requests));
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error occured while separating AMC batches from Batch '{0}.xml'".Fill(mbi.BatchName), ex);
                }
            }
            log.Debug("Separate batches job queue is completed.");
        }

        private void DeleteBatchInfo(int mailBatchId, string mailBatchRepo)
        {
            int retVal = 0;
            try
            {
                retVal = mailBatchSvc.DeleteBatchDetails(mailBatchId);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void SplitXMLAndPDFS(PackageRequests request, string PDFsDir, string batchName)
        {
            //Create folder for Separate batch for AMC in UNC path
            string batchDirectoryPath = Path.Combine(Config.Monitor_AMCDirPath, batchName);

            batchDirectoryPath.CleanDirectory();

            //Xml and pdf files will be copied to batchname folder.
            string amcFilePath = Path.Combine(batchDirectoryPath, string.Format("{0}.xml", batchName));

            string zipFilePath = Path.Combine(Config.Monitor_AMCDirPath, string.Format("{0}.zip", batchName));
            string triggerFilePath = Path.Combine(Config.Monitor_AMCDirPath, string.Format("{0}_Complete.go", batchName));

            List<PackageInfo> packages = request.Packages;
            if (packages != null)
            {
                foreach (PackageInfo package in packages)
                {
                    if (File.Exists(Path.Combine(PDFsDir, package.File)))
                    {
                        string sourceFile = Path.Combine(PDFsDir, package.File);
                        string destFile = Path.Combine(batchDirectoryPath, package.File);

                        if (File.Exists(destFile))
                            File.Delete(destFile);

                        File.Copy(sourceFile, destFile);
                    }
                }

                if (File.Exists(amcFilePath))
                    File.Delete(amcFilePath);

                File.WriteAllText(amcFilePath, XmlUtil.Serialize<PackageRequests>(request));

                if (File.Exists(zipFilePath))
                    File.Delete(zipFilePath);

                using (ZipArchive newFile = System.IO.Compression.ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                {
                    foreach (string file in Directory.GetFiles(batchDirectoryPath).Where(fileType => !fileType.ToLower().EndsWith(".zip")))
                    {
                        newFile.CreateEntryFromFile(file, Path.GetFileName(file));
                    }
                }

                //After creating zip remove batch folder
                Directory.Delete(batchDirectoryPath, true);


                if (File.Exists(triggerFilePath))
                    File.Delete(triggerFilePath);

                File.Create(triggerFilePath);
            }
        }
        #endregion

        protected override void ValidateBatches()
        {
            log.Debug("Validate job queue is started.");

            List<MailBatchDTO> downloadBatches = mailBatchSvc.GetBatchesByPipelineStatus(BatchPipelineStatus.Downloaded, client.Id);
            if (downloadBatches == null)
            {
                log.Debug("Validate job queue is empty.");
                return;
            }

            foreach (var batch in downloadBatches)
            {
                MailBatchInfo mbi = new MailBatchInfo(batch, client, Config.Common_AuthoMailRootFolder);

                //TODO: Can we have multiple business identifier in client business?
                ClientBusinessIdentifier clientBusinessIdDetails = commonSvc.GetBusinessIdNameFromClientId(client.Id);

                if (clientBusinessIdDetails == null)
                {
                    log.Error("Business identifier is not assigned to client : {0}".Fill(client.Name));

                    //TODO: Ask to Ravi Sir if no data found then how to insert data and which value
                    continue;
                }

                var xmlFilePath = Path.Combine(mbi.AMRequestPath, mbi.BatchName + ".xml");

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
                catch (XmlException ex)
                {
                    var msg = "'{0}.xml' is not a valid XML document.".Fill(mbi.BatchName);
                    log.Error(msg, ex);

                    ReportInvalidBatch(mbi, msg);

                    continue;
                }
                catch (Exception ex)
                {
                    var msg = "'{0}.xml' is not a valid XML document.".Fill(mbi.BatchName);
                    log.Error(msg, ex);
                    ReportInvalidBatch(mbi, ex.Message);
                    continue;
                }

                if (mailPieceNodes == null || mailPieceNodes.Count == 0)
                {
                    ReportInvalidBatch(mbi, "'{0}.xml' does not contain any package info node(s).".Fill(mbi.BatchName));

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
                        #region Reading nodes using XPath
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
                        #endregion

                        MailPieceValidationResult validationResult = ValidatePackgeInfoNode(mailPieceNode);
                        if (validationResult.Code == OutComeCodes.No_Error)
                        {
                            string filePath = Path.Combine(mbi.AMRequestPath, mailPieceDTO.FileName);

                            var validStateList = Resources.ValidStatesForUSAA
                                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(state => state.Trim())
                                .ToList();

                            //Document Or Package Type validation
                            var docType = docTypeSvc.GetDocTypeByDocTypeName(mailPieceDTO.ClientId, mailPieceDTO.DocumentOrPackageType);

                            if (docType != null)
                                mailPieceDTO.DocTypeId = docType.Id;

                            if (docType == null)
                            {
                                mailPieceDTO.ErrorCode = OutComeCodes.Invalid_PackageType;
                                mailPieceDTO.ErrorDesc = "The specified document or package type '{0}' is not configured.".Fill(mailPieceDTO.DocumentOrPackageType);
                                mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                            }
                            else if (!validStateList.Contains(mailPieceDTO.RcptAddressState))
                            {
                                mailPieceDTO.ErrorCode = OutComeCodes.Mailing_Address_Missing_Or_Invalid;
                                mailPieceDTO.ErrorDesc = "State '{0}' is not amongst the valid states in receipient mailing address.".Fill(mailPieceDTO.RcptAddressState);
                                mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                            }
                            else if (!validStateList.Contains(mailPieceDTO.RetRcptAddressState))
                            {
                                mailPieceDTO.ErrorCode = OutComeCodes.Mailing_Address_Missing_Or_Invalid;
                                mailPieceDTO.ErrorDesc = "State '{0}' is not amongst the valid states in return mailing address.".Fill(mailPieceDTO.RetRcptAddressState);
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
                        var msg = "Unable to process package info node.".Fill(mbi.BatchName);

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
            log.Debug("Validate job queue is completed.");
        }

        protected override void AcknowledgeBatches()
        {
            log.Debug("Acknowledge job queue is started.");

            List<MailBatchDTO> validatedBatches = mailBatchSvc.GetBatchesByPipelineStatus(BatchPipelineStatus.Validated, client.Id);

            if (validatedBatches == null)
            {
                log.Debug("Acknowledge job queue is empty.");
                return;
            }

            foreach (var batch in validatedBatches)
            {
                Deliverable deliverable = commonSvc.GetDeliverableByReportType(client.Id, ReportTypes.Error);

                MailBatchInfo mbi = new MailBatchInfo(batch, client, Config.Common_AuthoMailRootFolder);

                var data = mailBatchSvc.GetMailBatchDetailsByBatchId(mbi.MailBatch.Id);
                if (data != null)
                {
                    try
                    {
                        string errorReportFileName = string.Format("Error-{0}.csv", Common.Now.ToString("yyyyMMdd-HHmmss"));
                        string errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);

                        string errorGoFileName = "ErrReport.go";
                        string errorGoFilePath = Path.Combine(mbi.AMResponsePath, errorGoFileName);
                        string ftpErrorGoUploadPath = Path.Combine(deliverable.UploadPath, errorGoFileName);

                        // valid: ack, pvalid: err rpt upload, invalid: Generate Error Report, ReportPrepped

                        if (mbi.MailBatch.ValidationStatus == BatchValidationStatus.Invalid)
                        {
                            StringBuilder errorData = amcSvc.AMCErrorReport(data);
                            if (errorData != null)
                            {
                                File.WriteAllText(errorReportPath, errorData.ToString());
                            }

                            // Batch status Prepped
                            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportPrepped, Common.Now);

                        }
                        else if (mbi.MailBatch.ValidationStatus == BatchValidationStatus.PartiallyValid)
                        {
                            StringBuilder errorData = amcSvc.AMCErrorReport(data);

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

                            MailUtility.SendDownloadNotificationEmail(mbi, data, errorReportPath);
                            //After upload delete from local
                            File.Move(errorReportPath, Path.ChangeExtension(errorReportPath, ".uploaded"));
                        }
                        else
                        {
                            // Batch is valid
                            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Acknowledged, Common.Now);

                            MailUtility.SendDownloadNotificationEmail(mbi, data, errorReportPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("Error occured while acknowleding for Batch-Id : {0}".Fill(mbi.BatchName), ex);
                    }
                }
            }

            log.Debug("Acknowledge job queue is completed.");
        }

        private MailPieceValidationResult ValidatePackgeInfoNode(XmlNode xmlPiece)
        {
            var errors = GetAllPackageInfoErrors(xmlPiece).ToList();

            if (errors.Count > 0)
            {
                return new MailPieceValidationResult() { Code = errors.First().Code, ErrorMessage = string.Join(" | ", errors.Select(row => row.ErrorMessage)) };
            }

            return new MailPieceValidationResult() { Code = OutComeCodes.No_Error };
        }

        private IEnumerable<MailPieceValidationResult> GetAllPackageInfoErrors(XmlNode xmlPiece)
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
    }
}
