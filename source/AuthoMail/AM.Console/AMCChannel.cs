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
using AM.USPSLib;
using System.Transactions;
using System.Threading;

namespace AM.Console
{
    class AMCChannel : Channel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected IAMCService amcSvc;

        private string channelType;

        private static XmlSchemaSet schemas;

        static AMCChannel()
        {
            schemas = new XmlSchemaSet();
            schemas.Add("http://schemas.corelogic.com/2015/amc/v1/packages", XmlReader.Create(new StringReader(Resources.PackageInfoSchema)));
        }

        public AMCChannel(string channelCode, string channelType)
            : base(channelCode)
        {
            amcSvc = GetService<AMCService>();
            this.channelType = channelType;
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

                    #region Create AMC Directory.
                    //Here first we are cleaning AMC folder
                    string amcDownloadPath = Path.Combine(Config.Common_AuthoMailRootFolder, SFTChannel.Code.Split('_')[0]);

                    if (Directory.Exists(amcDownloadPath))
                    {
                        Directory.Delete(amcDownloadPath, true);
                    }

                    //To ensure that folder gets created.
                    Thread.Sleep(1000);
                    Directory.CreateDirectory(amcDownloadPath);

                    #endregion

                    #region Download Zip
                    string zipFileName = "{0}.zip".Fill(batchName);
                    string localZipFilePath = Path.Combine(amcDownloadPath, zipFileName);
                    string xmlFilePath = Path.Combine(amcDownloadPath, batchName + ".xml");
                    string ftpZipFilePath = Path.Combine(SFTChannel.ToADEDir, zipFileName);
                    string ftpTriggerFilePath = Path.Combine(SFTChannel.ToADEDir, "{0}_Complete.go".Fill(batchName));

                    if (!sftSite.IsFileAvailable(ftpZipFilePath))
                    {
                        var msg = "'{0}.zip' file is missing on SFT site.".Fill(batchName);
                        ReportInvalidBatch(InsertInvalidBatchInfo(batchName, lastModifiedTime, msg), "");
                        continue;
                    }

                    //Some time it is not creating AMC folder in flow so here we are again checking this if not created then below code will create
                    if (!Directory.Exists(amcDownloadPath))
                    {
                        log.Error("AMC Folder was not created as expected.");
                        Directory.CreateDirectory(amcDownloadPath);
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

                    SeparateAMCBatchClientWise(xmlFilePath, localZipFilePath, batchName, amcDownloadPath, lastModifiedTime, sftSite);
                }
                catch (Exception ex)
                {
                    var msg = "Error occured in EnumerateBatches Method For BatchName: {0} with Error : {1}".Fill(triggerFile, ex.Message);
                    log.Error(msg, ex);
                    MailUtility.SendUnexpectedExceptionEmail(msg);
                    continue;
                }
            }

            if (sftSite.IsConnected)
                sftSite.Disconnect();
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

        private void SeparateAMCBatchClientWise(string xmlFilePath, string localZipFilePath, string batchName, string amcDownloadPath, DateTime receivedOn, SFTUtil sftSite)
        {
            List<ClientAMCCustIdMap> amcClientMaps = clientSvc.GetAMCClientIdMaps();
            List<MailBatchDTO> mailBatches = new List<MailBatchDTO>();

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
                var msg = "'{0}.xml' is not a valid XML document.".Fill(batchName);
                log.Error(msg, ex);
                ReportInvalidBatch(InsertInvalidBatchInfo(batchName, receivedOn, msg), localZipFilePath);
                return;
            }
            catch (Exception ex)
            {
                var msg = "'{0}.xml' is not a valid XML document.".Fill(batchName);
                log.Error(msg, ex);
                ReportInvalidBatch(InsertInvalidBatchInfo(batchName, receivedOn, msg), localZipFilePath);
                return;
            }

            //Here we are caching all clients details 
            var clients = new List<Client>();

            try
            {
                Dictionary<string, List<XmlNode>> inputXmlByCustId = new Dictionary<string, List<XmlNode>>();

                foreach (XmlNode node in batchXML.DocumentElement.SelectNodes("//x:PackageInfo", nsManager))
                {
                    var custIdNode = node.SelectSingleNode("x:CustomerIdentifier", nsManager);

                    string custId = custIdNode == null || !amcClientMaps.Any(c => c.AMCCustId == custIdNode.InnerXml) ? "InvalidCustId" : custIdNode.InnerXml;

                    if (!inputXmlByCustId.ContainsKey(custId))
                    {
                        inputXmlByCustId.Add(custId, new List<XmlNode>());
                    }

                    inputXmlByCustId[custId].Add(node);
                }

                foreach (var map in amcClientMaps)
                {
                    if (!inputXmlByCustId.ContainsKey(map.AMCCustId))
                        continue;

                    Client client = clientSvc.GetClientById(map.ClientId);

                    clients.Add(client);

                    var packages = inputXmlByCustId[map.AMCCustId];

                    if (packages.Count > 0)
                    {
                        SeparateXMLsAndPDFsClientWise(packages, nsManager, amcDownloadPath, client.AppCode, batchName);

                        MailBatchDTO mailBatch = new MailBatchDTO();
                        mailBatch.ClientId = client.Id;
                        mailBatch.BatchName = batchName;
                        mailBatch.SFTChannelId = SFTChannel.Id;
                        mailBatch.ClientBatchId = batchName;
                        mailBatch.UniqueId = Guid.NewGuid();
                        mailBatch.Priority = Priority.Medium.ToString();
                        mailBatch.ReceivedOn = receivedOn;
                        mailBatch.PipelineStatus = BatchPipelineStatus.Received;
                        mailBatch.BatchStatus = BatchStatus.InProgress.ToString();
                        mailBatch.IsReturnInfoSpecified = false;
                        mailBatches.Add(mailBatch);
                    }
                }
            }
            catch (Exception ex)
            {
                var msg = "Error occured while separating AMC batches from Batch '{0}.xml'".Fill(batchName);
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
                        Client client = clients.Single(c => c.Id == clientBatch.ClientId);
                        MailBatchInfo mbi = new MailBatchInfo(clientBatch, client, Config.Common_AuthoMailRootFolder);

                        //Here we create folder structure
                        //mbi.AMRequestPath.CreateDirectoryIfNotExist();
                        mbi.AMResponsePath.CreateDirectoryIfNotExist();
                        mbi.PnSRequestPath.CreateDirectoryIfNotExist();
                        mbi.PnSResponsePath.CreateDirectoryIfNotExist();

                        Directory.Move(Path.Combine(amcDownloadPath, client.AppCode), mbi.AMRequestPath);

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
                    amcDownloadPath.CleanDirectory();

                    scope.Complete();

                }
                catch (Exception ex)
                {
                    var msg = "Error occured while inserting AMC batches details for Batch '{0}.xml'".Fill(batchName);
                    log.Error(msg, ex);
                    MailUtility.SendUnexpectedExceptionEmail(msg + "<br/> Error Message is {0}".Fill(ex));
                }
            }
        }

        private Client GetInvalidClient()
        {
            return clientSvc.GetClientByAppCode("AMCInvalid");
        }

        private void SeparateXMLsAndPDFsClientWise(List<XmlNode> packages, XmlNamespaceManager nsManager, string amcDownloadPath, string clientCode, string batchName)
        {
            string clientBatchDirectory = Path.Combine(amcDownloadPath, clientCode);

            clientBatchDirectory.CleanDirectory();

            string amcFilePath = Path.Combine(clientBatchDirectory, string.Format("{0}.xml", batchName));

            string reqRootXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><PackageRequests xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.corelogic.com/2015/amc/v1/packages\">{0}</PackageRequests>";

            var packageInfoNodes = string.Join(Environment.NewLine, packages.Select(nodes => nodes.OuterXml));

            foreach (var package in packages)
            {
                var fileNameAttr = package.SelectSingleNode("x:File", nsManager);
                string fileName = fileNameAttr == null ? string.Empty : fileNameAttr.InnerXml;

                if (!string.IsNullOrEmpty(fileName) && File.Exists(Path.Combine(amcDownloadPath, fileName)))
                {
                    string sourceFile = Path.Combine(amcDownloadPath, fileName);
                    string destFile = Path.Combine(clientBatchDirectory, fileName);
                    File.Move(sourceFile, destFile);
                }
            }

            File.WriteAllText(amcFilePath, reqRootXml.Fill(packageInfoNodes));
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

        private string DownloadBatchZipFile(SFTUtil sftSite, string ftpZipFilePath, string ftpTriggerFilePath, string localZipFilePath)
        {
            sftSite.DownloadFile(ftpZipFilePath, localZipFilePath);

            return localZipFilePath;
        }
        #endregion

        #region DownloadBatches
        protected override void DownloadBatches()
        {
        }

        #endregion

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
                    int totalNodes = 0;
                    try
                    {
                        batchXML.LoadXml(File.ReadAllText(xmlFilePath));
                        nsManager = new XmlNamespaceManager(batchXML.NameTable);
                        nsManager.AddNamespace("x", "http://schemas.corelogic.com/2015/amc/v1/packages");
                        mailPieceNodes = batchXML.DocumentElement.ChildNodes;
                        totalNodes = mailPieceNodes.Count;
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

                                    mailPieceDTO.RcptAddrOptionalLine1BelowStreet = recipeintAddressElement.SelectSingleNode("./x:Line2/text()", nsManager) == null ? null : recipeintAddressElement.SelectSingleNode("./x:Line2/text()", nsManager).Value;

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

                                    mailPieceDTO.RetRcptAddrOptionalLine1BelowStreet = returnAddressElement.SelectSingleNode("./x:Line2/text()", nsManager) == null ? null : returnAddressElement.SelectSingleNode("./x:Line2/text()", nsManager).Value;

                                    mailPieceDTO.RetRcptAddressCity = returnAddressElement.SelectSingleNode("./x:City/text()", nsManager) == null ? null : returnAddressElement.SelectSingleNode("./x:City/text()", nsManager).Value;

                                    mailPieceDTO.RetRcptAddressState = returnAddressElement.SelectSingleNode("./x:State/text()", nsManager) == null ? null : returnAddressElement.SelectSingleNode("./x:State/text()", nsManager).Value;

                                    mailPieceDTO.RetRcptAddressZIP = returnAddressElement.SelectSingleNode("./x:Zip/text()", nsManager) == null ? null : returnAddressElement.SelectSingleNode("./x:Zip/text()", nsManager).Value;
                                }
                            }
                            else
                                mailPieceDTO.IsReturnInfoSpecified = false;

                            if (mailPieceNode.SelectSingleNode("./x:ShippingMethod", nsManager) != null)
                            {
                                customInfo = new MailPieceCustomInfo() { MailPieceId = 0, Key = "ShippingMethod", DataType = DataType.String, Value = mailPieceNode.SelectSingleNode("./x:ShippingMethod/text()", nsManager) == null ? string.Empty : mailPieceNode.SelectSingleNode("./x:ShippingMethod/text()", nsManager).Value };

                                mailPieceCustomInfo.Add(customInfo);
                            }

                            if (mailPieceNode.SelectSingleNode("./x:CustomerIdentifier", nsManager) != null)
                            {
                                customInfo = new MailPieceCustomInfo() { MailPieceId = 0, Key = "CustomerIdentifier", DataType = DataType.String, Value = mailPieceNode.SelectSingleNode("./x:CustomerIdentifier/text()", nsManager) == null ? string.Empty : mailPieceNode.SelectSingleNode("./x:CustomerIdentifier/text()", nsManager).Value };

                                mailPieceCustomInfo.Add(customInfo);
                            }

                            if (mailPieceNode.SelectSingleNode("./x:RequestId", nsManager) != null)
                            {
                                customInfo = new MailPieceCustomInfo() { MailPieceId = 0, Key = "RequestId", DataType = DataType.String, Value = mailPieceNode.SelectSingleNode("./x:RequestId/text()", nsManager) == null ? string.Empty : mailPieceNode.SelectSingleNode("./x:RequestId/text()", nsManager).Value };

                                mailPieceCustomInfo.Add(customInfo);
                            }

                            if (mailPieceNode.SelectSingleNode("./x:ReceivedDate", nsManager) != null)
                            {
                                customInfo = new MailPieceCustomInfo() { MailPieceId = 0, Key = "ReceivedDate", DataType = DataType.DateTime, Value = mailPieceNode.SelectSingleNode("./x:ReceivedDate/text()", nsManager) == null ? null : mailPieceNode.SelectSingleNode("./x:ReceivedDate/text()", nsManager).Value };

                                mailPieceCustomInfo.Add(customInfo);
                            }

                            var shippingCredElement = mailPieceNode.SelectSingleNode("./x:ShippingCredentials", nsManager);
                            if (shippingCredElement != null)
                            {
                                if (shippingCredElement.SelectSingleNode("./x:ClientCode", nsManager) != null)
                                {
                                    customInfo = new MailPieceCustomInfo() { MailPieceId = 0, Key = "ShippingCredCode", DataType = DataType.String, Value = shippingCredElement.SelectSingleNode("./x:ClientCode/text()", nsManager) == null ? string.Empty : shippingCredElement.SelectSingleNode("./x:ClientCode/text()", nsManager).Value };

                                    mailPieceCustomInfo.Add(customInfo);
                                }
                            }

                            #endregion

                            MailPieceValidationResult validationResult = ValidatePackgeInfoNode(mailPieceNode);
                            if (validationResult.Code == OutComeCodes.No_Error)
                            {
                                string filePath = Path.Combine(mbi.AMRequestPath, mailPieceDTO.FileName);

                                string documentOrPackageType = mailPieceDTO.DocumentOrPackageType;

                                //Document Or Package Type validation
                                var docType = docTypeSvc.GetDocTypeByDocTypeName(mailPieceDTO.ClientId, documentOrPackageType);

                                if (docType == null)
                                {
                                    mailPieceDTO.ErrorCode = OutComeCodes.Invalid_PackageType;
                                    mailPieceDTO.ErrorDesc = client.AppCode == GetInvalidClient().AppCode ? "The specified customer identifier {0} is not configured.".Fill(mailPieceCustomInfo.Single(c => c.Key == "CustomerIdentifier").Value) : "The specified document or package type '{0}' is not configured.".Fill(mailPieceDTO.DocumentOrPackageType);
                                    mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                                }
                                else if (!File.Exists(filePath))
                                {
                                    mailPieceDTO.ErrorCode = OutComeCodes.Missing_PDF_File;
                                    mailPieceDTO.ErrorDesc = "Pdf file is not available for this package.";
                                    mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                                }
                                else if (!mailPieceDTO.IsReturnInfoSpecified && !mailBatch.IsReturnInfoSpecified && !docType.IsReturnInfoSpecified)
                                {
                                    mailPieceDTO.ErrorCode = OutComeCodes.Mailing_Address_Missing_Or_Invalid;
                                    mailPieceDTO.ErrorDesc = "Return mailing address is not specified for this package.";
                                    mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                                }
                                else
                                {
                                    mailPieceDTO.ErrorCode = OutComeCodes.No_Error;
                                    mailPieceDTO.DocTypeId = docType.Id;

                                    if (!mailPieceDTO.IsReturnInfoSpecified && !mailBatch.IsReturnInfoSpecified && docType.IsReturnInfoSpecified)
                                    {
                                        mailPieceDTO.RetRcptFirstName = docType.RetRcptFirstName;

                                        mailPieceDTO.RetRcptLastName = docType.RetRcptLastName;

                                        mailPieceDTO.RetRcptPhone = docType.RetRcptPhone;

                                        mailPieceDTO.RetRcptAddressStreet = docType.RetRcptAddressStreet;

                                        mailPieceDTO.RetRcptAddrOptionalLine1BelowStreet = docType.RetRcptAddrOptionalLine1BelowStreet;

                                        mailPieceDTO.RetRcptAddressCity = docType.RetRcptAddressCity;

                                        mailPieceDTO.RetRcptAddressState = docType.RetRcptAddressState;

                                        mailPieceDTO.RetRcptAddressZIP = docType.RetRcptAddressZIP;
                                    }
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
                            log.Error("Error occcured while inserting MailPiece details :", ex);
                            MailUtility.SendUnexpectedExceptionEmail("Error occcured while inserting MailPiece details :" + ex.Message);
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

                    var mailPieceData = mailPieceSvc.GetMailPiecesByMailBatchId(mbi.MailBatch.Id);
                    if (mailPieceData != null)
                    {
                        var validMailPieces = mailPieceData.Where(c => c.ErrorCode == OutComeCodes.No_Error).ToList();

                        //Here System will validate Addresses for valid mail pieces
                        if (validMailPieces.Count > 0)
                            Channel.ValidateAddressType(mailBatch, validMailPieces);

                        if (validMailPieces.Count == 0)
                        {
                            mbi.MailBatch.ValidationStatus = BatchValidationStatus.Invalid.ToString();
                            mbi.MailBatch.ValidationErrors = "Xml does not contain any valid package info nodes.";
                            commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, BatchStatus.Failed.ToString());
                        }
                        else if (validMailPieces.Count < mailPieceData.Count)
                        {
                            mbi.MailBatch.ValidationStatus = BatchValidationStatus.PartiallyValid.ToString();
                        }
                        else
                        {
                            mbi.MailBatch.ValidationStatus = BatchValidationStatus.Valid.ToString();
                        }

                        if (totalNodes != mailPieceData.Count)
                            MailUtility.SendUnexpectedExceptionEmail("Mail pieces count and received xml count is mismatched for Batch-Id : {0}<br/>".Fill(mailBatch.Id) + "Mail Piece count is {0} and received xml nodes count is {1}".Fill(mailPieceData.Count, totalNodes));

                        commonSvc.UpdateBatchValidationStatus(mbi.MailBatch.Id, mbi.MailBatch.ValidationStatus, mbi.MailBatch.ValidationErrors);
                    }
                    else
                    {
                        MailUtility.SendUnexpectedExceptionEmail("Mail Pieces list is null for Batch-Id {0}".Fill(mailBatch.Id));
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
                            UploadBaseReportToAMC(mbi, mailPieces, deliverable);

                            //Here we have to provide clientwise notification so only reporting functionality will be customized over here
                            var emailErrorReportPath = GenerateClientWiseAcknowledgementReport(mbi, mailPieces, deliverable);

                            MailUtility.SendDownloadNotificationEmail(mbi, mailPieces, emailErrorReportPath, deliverable.EmailChannel);
                            //After upload change extension of local file Path
                            if (File.Exists(emailErrorReportPath))
                                File.Move(emailErrorReportPath, Path.ChangeExtension(emailErrorReportPath, ".uploaded"));
                        }
                        catch (Exception ex)
                        {
                            var msg = "Error occured while acknowleding for Batch-Id : {0}".Fill(mbi.BatchName);
                            log.Error(msg, ex);
                            MailUtility.SendUnexpectedExceptionEmail(msg + ex.Message);
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

        private void UploadBaseReportToAMC(MailBatchInfo mbi, List<MailPieces> mailPieces, Deliverable deliverable)
        {
            string errorReportFileName = "Error-{0}.txt".Fill(Common.Now.ToString("yyyyMMdd-HHmmss"));
            //string errorReportFileName = "Error-{0}-{1}-{2}-{3}.txt".Fill(mbi.BatchName, mbi.Client.AppCode, mbi.MailBatch.Id, Common.Now.ToString("yyyyMMdd-HHmmss"));
            string errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);

            string errorGoFileName = "ErrReport.go";
            string errorGoFilePath = Path.Combine(mbi.AMResponsePath, errorGoFileName);
            string ftpErrorGoUploadPath = Path.Combine(deliverable.UploadPath, errorGoFileName);

            if (mbi.MailBatch.ValidationStatus == BatchValidationStatus.Invalid)
            {
                StringBuilder errorData = amcSvc.AMCErrorReport(mailPieces);
                if (errorData != null)
                {
                    File.WriteAllText(errorReportPath, errorData.ToString());

                    SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(SFTChannel.Protocol), SFTChannel.Host, SFTChannel.UserName, SFTChannel.Password, 300000);
                    sftSite.Connect();

                    var ftpUploadPath = Path.Combine(deliverable.UploadPath, errorReportFileName);

                    sftSite.UploadFile(errorReportPath, ftpUploadPath);

                    //NOTE: Temparary Code for Duplicate Report Upload 
                    if (sftSite.IsDownloadDirectoryAvailable(Config.Monitor_AMCBackupReportUploadPath))
                    {
                        sftSite.UploadFile(errorReportPath, Path.Combine(Config.Monitor_AMCBackupReportUploadPath, errorReportFileName));
                    }

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
                StringBuilder errorData = amcSvc.AMCErrorReport(mailPieces);

                if (errorData != null)
                {
                    File.WriteAllText(errorReportPath, errorData.ToString());

                    SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(SFTChannel.Protocol), SFTChannel.Host, SFTChannel.UserName, SFTChannel.Password, 300000);
                    sftSite.Connect();

                    var ftpUploadPath = Path.Combine(deliverable.UploadPath, errorReportFileName);

                    sftSite.UploadFile(errorReportPath, ftpUploadPath);

                    //NOTE: Temparary Code for Duplicate Report Upload 
                    if (sftSite.IsDownloadDirectoryAvailable(Config.Monitor_AMCBackupReportUploadPath))
                    {
                        sftSite.UploadFile(errorReportPath, Path.Combine(Config.Monitor_AMCBackupReportUploadPath, errorReportFileName));
                    }

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
                    errorReportFileName = "Error-{0}.txt".Fill(Common.Now.ToString("yyyyMMdd-HHmmss"));
                    //string errorReportFileName = "Error-{0}-{1}-{2}-{3}.txt".Fill(mbi.BatchName, mbi.Client.AppCode, mbi.MailBatch.Id, Common.Now.ToString("yyyyMMdd-HHmmss"));
                    errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);
                    errorReportData = amcSvc.AMCErrorReport(mailPieces);
                    if (errorReportData != null)
                    {
                        File.WriteAllText(errorReportPath, errorReportData.ToString());
                    }
                    break;
            }

            return errorReportPath;
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
