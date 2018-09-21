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
                string triggerFilePath = Path.Combine(SFTChannel.ToADEDir, triggerFile + "_Complete.go");

                //Download and Insert the batch in database first always.
                string batchName = triggerFile;

                DateTime lastModifiedTime = Common.Now;
                if (channelType == ChannelType.Trigger)
                {
                    lastModifiedTime = sftSite.LastModifiedTimeOfFile(triggerFilePath);
                }

                #region Create AMC Directory.
                //Here first we are cleaning AMC folder
                string amcDownloadPath = Path.Combine(Config.Common_AuthoMailRootFolder, SFTChannel.Code.Split('_')[0]);

                if (Directory.Exists(amcDownloadPath))
                {
                    Directory.Delete(amcDownloadPath, true);
                }

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
            }
            catch (Exception ex)
            {
                var msg = "'{0}.xml' is not a valid XML document.".Fill(batchName);
                log.Error(msg, ex);
                ReportInvalidBatch(InsertInvalidBatchInfo(batchName, receivedOn, msg), localZipFilePath);
            }

            try
            {
                PackageRequests requests = null;
                PackageRequests amcRequest = new PackageRequests();

                requests = XmlUtil.Deserialize<PackageRequests>(batchXML.OuterXml);

                List<PackageInfo> invalidPackages = requests.Packages.FindAll(p => !amcClientMaps.Select(c => c.AMCCustId).Contains(p.CustomerIdentifier));
                amcRequest.Packages = invalidPackages;

                if (invalidPackages.Count > 0)
                {
                    SeparateXMLsAndPDFsClientWise(amcRequest, amcDownloadPath, GetInvalidClient().AppCode, batchName);

                    MailBatchDTO invalidMailBatch = new MailBatchDTO();
                    invalidMailBatch.ClientId = GetInvalidClient().Id;
                    invalidMailBatch.BatchName = batchName;
                    invalidMailBatch.SFTChannelId = SFTChannel.Id;
                    invalidMailBatch.ClientBatchId = batchName;
                    invalidMailBatch.UniqueId = Guid.NewGuid();
                    invalidMailBatch.Priority = Priority.Medium.ToString();
                    invalidMailBatch.ReceivedOn = receivedOn;

                    mailBatches.Add(invalidMailBatch);

                    requests.Packages.RemoveAll(p => !amcClientMaps.Select(c => c.AMCCustId).Contains(p.CustomerIdentifier));
                }

                if (requests.Packages.Count > 0)
                {
                    foreach (var map in amcClientMaps)
                    {
                        Client client = clientSvc.GetClientById(map.ClientId);

                        List<PackageInfo> packages = requests.Packages.FindAll(p => p.CustomerIdentifier == map.AMCCustId);

                        if (packages != null && packages.Count > 0)
                        {
                            amcRequest.Packages = packages;

                            SeparateXMLsAndPDFsClientWise(amcRequest, amcDownloadPath, client.AppCode, batchName);

                            MailBatchDTO mailBatch = new MailBatchDTO();
                            mailBatch.ClientId = client.Id;
                            mailBatch.BatchName = batchName;
                            mailBatch.SFTChannelId = SFTChannel.Id;
                            mailBatch.ClientBatchId = batchName;
                            mailBatch.UniqueId = Guid.NewGuid();
                            mailBatch.Priority = Priority.Medium.ToString();
                            mailBatch.ReceivedOn = receivedOn;
                            mailBatches.Add(mailBatch);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var msg = "Error occured while separating AMC batches from Batch '{0}.xml'".Fill(batchName);
                log.Error(msg, ex);
                MailUtility.SendUnexpectedExceptionEmail(msg + "<br/> Error Message is {0}".Fill(ex));
            }

            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    var insertedList = mailBatchSvc.InsertBatchDetails(mailBatches);

                    if (insertedList.Count > 0)
                    {
                        foreach (var clientBatch in insertedList)
                        {
                            Client client = clientSvc.GetClientById(clientBatch.ClientId);
                            MailBatchInfo mbi = new MailBatchInfo(clientBatch, client, Config.Common_AuthoMailRootFolder);

                            //Here we create folder structure
                            mbi.AMRequestPath.CreateDirectoryIfNotExist();
                            mbi.AMResponsePath.CreateDirectoryIfNotExist();
                            mbi.PnSRequestPath.CreateDirectoryIfNotExist();
                            mbi.PnSRequestPath.CreateDirectoryIfNotExist();

                            var fileList = Directory.GetFiles(Path.Combine(amcDownloadPath, client.AppCode));
                            foreach (var srcFilePath in fileList)
                            {
                                string destFilePath = Path.Combine(mbi.AMRequestPath, Path.GetFileName(srcFilePath));
                                if (File.Exists(destFilePath))
                                    File.Delete(destFilePath);

                                File.Move(srcFilePath, destFilePath);
                            }

                            commonSvc.UpdateBatchPipeLineStatus(clientBatch.Id, BatchPipelineStatus.Downloaded, Common.Now);
                        }
                    }

                    //Here after moving all batches to particular client location , archive amc batch
                    string archiveZipPath = Path.Combine(Config.Common_AuthoMailRootFolder, "Archive", batchName + ".zip");
                    File.Move(localZipFilePath, archiveZipPath);

                    //Now delete/archive trigger and zip from SFTP
                    MoveBatchToBackupDir(sftSite, Path.Combine(SFTChannel.ToADEDir, batchName + ".zip"));
                    MoveBatchToBackupDir(sftSite, Path.Combine(SFTChannel.ToADEDir, batchName + "_Complete.go"));

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

        private void SeparateXMLsAndPDFsClientWise(PackageRequests request, string amcDownloadPath, string clientCode, string batchName)
        {
            string clientBatchDirectory = Path.Combine(amcDownloadPath, clientCode);
            clientBatchDirectory.CleanDirectory();
            string amcFilePath = Path.Combine(clientBatchDirectory, string.Format("{0}.xml", batchName));
            List<PackageInfo> packages = request.Packages;
            foreach (PackageInfo package in packages)
            {
                if (File.Exists(Path.Combine(amcDownloadPath, package.File)))
                {
                    string sourceFile = Path.Combine(amcDownloadPath, package.File);
                    string destFile = Path.Combine(clientBatchDirectory, package.File);
                    File.Move(sourceFile, destFile);
                }
            }

            File.WriteAllText(amcFilePath, XmlUtil.Serialize<PackageRequests>(request));
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
            string triggerFileName = Path.GetFileName(ftpTriggerFilePath);
            string zipFileName = Path.GetFileName(ftpZipFilePath);

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
                File.Move(localZipFilePath, mbi.AMRequestPath);

            //For Invalid Batch, we will generate err file 
            string invalidBatchFileName = "{0}_InvalidZipfile.err".Fill(mailBatch.BatchName);

            string invalidBatchPath = Path.Combine(mbi.AMResponsePath, invalidBatchFileName);

            File.WriteAllText(invalidBatchPath, string.Empty);

            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportPrepped, Common.Now);

            string remoteFilePath = Path.Combine(deliverable.UploadPath, invalidBatchFileName);

            SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(SFTChannel.Protocol), SFTChannel.Host, SFTChannel.UserName, SFTChannel.Password, 300000);
            sftSite.Connect();

            sftSite.UploadFile(invalidBatchPath, remoteFilePath);

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

                #region Address Validation Logic
                //Get validate Addresses for each records
                List<AddressResponse> addressResponses = new List<AddressResponse>();
                List<AddressRequest> addressRequestes = new List<AddressRequest>();
                foreach (XmlNode mailPieceNode in mailPieceNodes)
                {
                    string uniqueId = mailPieceNode.SelectSingleNode("./x:Id/text()", nsManager) == null ? string.Empty : mailPieceNode.SelectSingleNode("./x:Id/text()", nsManager).Value;

                    string strRcptAddressStreet = string.Empty;
                    string strRcptAddrOptionalLine1BelowStreet = string.Empty;
                    string strRcptAddressCity = string.Empty;
                    string strRcptAddressState = string.Empty;
                    string strRcptAddressZIP = string.Empty;
                    string addressZip5 = string.Empty;
                    string addressZip4 = string.Empty;

                    var recipientElement = mailPieceNode.SelectSingleNode("./x:RecipientInfo", nsManager);
                    if (recipientElement != null)
                    {
                        var recipeintAddressElement = recipientElement.SelectSingleNode("./x:Address", nsManager);

                        if (recipeintAddressElement != null)
                        {
                            strRcptAddressStreet = recipeintAddressElement.SelectSingleNode("./x:Line1/text()", nsManager) == null ? null : recipeintAddressElement.SelectSingleNode("./x:Line1/text()", nsManager).Value;

                            strRcptAddrOptionalLine1BelowStreet = recipeintAddressElement.SelectSingleNode("./x:Line2/text()", nsManager) == null ? null : recipeintAddressElement.SelectSingleNode("./x:Line2/text()", nsManager).Value;

                            strRcptAddressCity = recipeintAddressElement.SelectSingleNode("./x:City/text()", nsManager) == null ? null : recipeintAddressElement.SelectSingleNode("./x:City/text()", nsManager).Value;

                            strRcptAddressState = recipeintAddressElement.SelectSingleNode("./x:State/text()", nsManager) == null ? null : recipeintAddressElement.SelectSingleNode("./x:State/text()", nsManager).Value;

                            strRcptAddressZIP = recipeintAddressElement.SelectSingleNode("./x:Zip/text()", nsManager) == null ? null : recipeintAddressElement.SelectSingleNode("./x:Zip/text()", nsManager).Value;
                        }
                    }

                    AddressRequest addressRequest = new AddressRequest() { Id = uniqueId, StreetAddress = strRcptAddressStreet + " " + strRcptAddrOptionalLine1BelowStreet, City = strRcptAddressCity, State = strRcptAddressState, Zip4 = strRcptAddressZIP.SeparateZip4And5(4), Zip5 = strRcptAddressZIP.SeparateZip4And5(5) };

                    addressRequestes.Add(addressRequest);
                }

                int requestAddressCount = addressRequestes.Count;
                int maxAddressVerification = 5;
                int verifiedAddressCount = 0;
                double loopCount = Math.Ceiling(requestAddressCount / (float)maxAddressVerification);

                for (double i = 0; i < loopCount; i++)
                {
                    var takenAddressesList = addressRequestes.Skip(verifiedAddressCount).Take(maxAddressVerification).ToList();
                    VerifierAddressRequest verifierAddRequest = new VerifierAddressRequest() { AddressRequests = takenAddressesList };
                    verifiedAddressCount += maxAddressVerification;
                    VerifierAddressResponse verifierAddResponse;

                    using (IUSPSService uspsSvc = new USPSService())
                    {
                        verifierAddResponse = uspsSvc.VerifyAddressAsync(commonSvc.GetSystemUserId(), verifierAddRequest).Result;
                    }

                    if (verifierAddResponse != null)
                    {
                        //if any error at VerifierAddressResponse level then throw unexpected error and exit from loop
                        if (verifierAddResponse.HasError)
                        {
                            throw new Exception("Error occured while validating Addresses : {0}".Fill(verifierAddResponse.Error));
                        }

                        addressResponses.AddRange(verifierAddResponse.AddressResponses);
                    }
                }
                #endregion

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

                            string documentOrPackageType = mailPieceDTO.DocumentOrPackageType;

                            //Document Or Package Type validation
                            var docType = docTypeSvc.GetDocTypeByDocTypeName(mailPieceDTO.ClientId, documentOrPackageType);

                            if (docType != null)
                            {
                                List<AddressType> addressTypes = addressResponses.Single(c => c.Id.ToLower() == mailPieceDTO.UniqueId.ToString().ToLower()).AddressTypes;
                                if (docType.ShippingCarrier == Carrier.FedEx.ToString())
                                {
                                    if (!(addressTypes.All(c => c == AddressType.DHAM)))
                                    {
                                        documentOrPackageType = documentOrPackageType + Config.Monitor_ExpressMailingText;

                                        docType = docTypeSvc.GetDocTypeByDocTypeName(mailPieceDTO.ClientId, documentOrPackageType);
                                    }
                                }
                                mailPieceDTO.RcptAddressTypes = string.Join(",", addressTypes);
                                mailPieceDTO.DocTypeId = docType.Id;
                            }


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
                            mailPieceDTO.PrintLocation = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        var msg = "Unable to process package info node.".Fill(mbi.BatchName);

                        log.Error(msg, ex);

                        mailPieceDTO.ErrorCode = OutComeCodes.Unknown_Error;
                        mailPieceDTO.ErrorDesc = msg + Environment.NewLine + ex.Message;
                        mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                        mailPieceDTO.PrintLocation = null;
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

            foreach (var batch in validatedBatches)
            {
                Client client = clientSvc.GetClientById(batch.ClientId);

                Deliverable deliverable = commonSvc.GetDeliverableByReportType(client.Id, ReportTypes.Error);

                MailBatchInfo mbi = new MailBatchInfo(batch, client, Config.Common_AuthoMailRootFolder);

                var data = mailBatchSvc.GetMailBatchDetailsByBatchId(mbi.MailBatch.Id);
                if (data != null)
                {
                    try
                    {
                        string errorReportFileName = "Error-{0}.txt".Fill(Common.Now.ToString("yyyyMMdd-HHmmss"));
                        //string errorReportFileName = "Error-{0}-{1}-{2}-{3}.txt".Fill(mbi.BatchName, mbi.Client.AppCode, mbi.MailBatch.Id, Common.Now.ToString("yyyyMMdd-HHmmss"));
                        string errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);

                        string errorGoFileName = "ErrReport.go";
                        string errorGoFilePath = Path.Combine(mbi.AMResponsePath, errorGoFileName);
                        string ftpErrorGoUploadPath = Path.Combine(deliverable.UploadPath, errorGoFileName);

                        // valid: ack, pvalid: err rpt upload, invalid: Generate Error Report, ReportPrepped

                        List<MailPieces> mailPieces;
                        using (IReportService reportSvc = new ReportService())
                        {
                            mailPieces = reportSvc.GetMailPiecesDataFromMailBatchId(mbi.MailBatch.Id);
                        }

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
