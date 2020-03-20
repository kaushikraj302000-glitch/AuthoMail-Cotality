using AM.Console.Properties;
using AM.Entities;
using AM.Lib;
using AM.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace AM.Console
{
    class CapOneChannel : AMSchemaChannel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string channelType;

        private static XmlSchemaSet schemas;

        static CapOneChannel()
        {
            schemas = new XmlSchemaSet();
            schemas.Add("http://schemas.corelogic.com/ade/authomail/v1.18.0", XmlReader.Create(new StringReader(Resources.AMMailPieceSchema)));
        }
        public CapOneChannel(string channelCode, string channelType)
            : base(channelCode, channelType)
        {
            generalSvc = GetService<GeneralService>();
            this.channelType = channelType;
        }

        protected override void EnumerateBatches()
        {
            //Download File logic and separate according to clientcode in csv
            log.Debug("Enumerate Batches job queue for " + SFTChannel.Code + " is started.");

            SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(SFTChannel.Protocol), SFTChannel.Host, SFTChannel.UserName, SFTChannel.Password, 300000);
            sftSite.Connect();

            List<string> filePaths = GetFilePaths(sftSite);

            foreach (string file in filePaths)
            {
                try
                {
                    string filePath = Path.Combine(SFTChannel.ToADEDir, file + ".csv");

                    //Download and Insert the batch in database first always.
                    string batchName = file;

                    DateTime lastModifiedTime = Common.Now;
                    if (channelType == ChannelType.Trigger)
                    {
                        //TODO : Implement check for zip exist or not
                        lastModifiedTime = sftSite.LastModifiedTimeOfFile(filePath);
                    }

                    #region Create General Channel Directory.
                    //Here first we are cleaning AMC folder
                    string channelDownloadPath = Path.Combine(Config.Common_AuthoMailRootFolder, "GENERAL", SFTChannel.Code.Split('_')[0]);

                    if (Directory.Exists(channelDownloadPath))
                    {
                        Directory.Delete(channelDownloadPath, true);
                    }

                    //To ensure that folder gets created.
                    Thread.Sleep(1000);
                    Directory.CreateDirectory(channelDownloadPath);

                    #endregion

                    #region Download Zip
                    string csvFileName = "{0}.csv".Fill(batchName);
                    string localCsvDirPath = Path.Combine(channelDownloadPath, Path.GetFileNameWithoutExtension(csvFileName));
                    string localCsvFilePath = Path.Combine(localCsvDirPath, csvFileName);
                    string ftpCsvFilePath = Path.Combine(SFTChannel.ToADEDir, csvFileName);

                    if (!sftSite.IsFileAvailable(ftpCsvFilePath))
                    {
                        var msg = "'{0}.csv' file is missing on SFT site.".Fill(batchName);
                        //ReportInvalidBatch(InsertInvalidBatchInfo(batchName, lastModifiedTime, msg), "");
                        continue;
                    }

                    //Some time it is not creating AMC folder in flow so here we are again checking this if not created then below code will create
                    if (!Directory.Exists(localCsvDirPath))
                    {
                        log.Error("General Channel Folder was not created as expected.");
                        Directory.CreateDirectory(localCsvDirPath);
                    }

                    DownloadBatchCsvFile(sftSite, ftpCsvFilePath, localCsvFilePath);

                    //Generate List from CSV 
                    Dictionary<string, List<CapOneData>> caponeClientData = new Dictionary<string, List<CapOneData>>();
                    List<CapOneData> data = File.ReadAllLines(localCsvFilePath).Skip(1).Select(v => FromCsv(v)).ToList();

                    var clData = data.GroupBy(cl => cl.ClientCode);
                    foreach (var clInfo in clData)
                    {
                        caponeClientData.Add(clInfo.Key, clInfo.Where(cl => cl.ClientCode == clInfo.Key).ToList());
                    }

                    bool isAllSuccess = SeparatePackages(caponeClientData, localCsvFilePath, batchName, localCsvDirPath, lastModifiedTime, sftSite);
                    #endregion

                    //Deleting Local Directory 
                    if (Directory.Exists(localCsvDirPath))
                        Directory.Delete(localCsvDirPath, true);


                    if (isAllSuccess)
                        MoveBatchToBackupDir(sftSite, ftpCsvFilePath);

                }
                catch (Exception ex)
                {
                    var msg = "Error occured in EnumerateBatches Method For BatchName: {0} with Error : {1}".Fill(file, ex.Message);
                    log.Error(msg, ex);
                    MailUtility.SendUnexpectedExceptionEmail(msg);
                    continue;
                }
            }
        }

        private bool SeparatePackages(Dictionary<string, List<CapOneData>> caponeClientData, string localCsvFilePath, string batchName, string localCsvDirPath, DateTime lastModifiedTime, SFTUtil sftSite)
        {
            bool isAllSuccess = false;
            List<MailBatchDTO> mbList = new List<MailBatchDTO>();
            foreach (var clKey in caponeClientData.Keys)
            {
                Client client;
                MailBatchDTO mailBatch;
                try
                {
                    var clientCode = clKey == null || string.IsNullOrWhiteSpace(clKey) ? "InvalidClientCode" : clKey;

                    if (clientCode == "InvalidClientCode")
                    {
                        var msg = "Client code is required in provided csv.".Fill(clientCode);
                        //ReportInvalidBatch(InsertInvalidBatchInfo(batchName, receivedOnDateTime, msg), xmlFilePath);
                        continue;
                    }

                    client = clientSvc.GetClientByAppCode(clientCode).Result;

                    if (client == null)
                    {
                        var msg = "Client code {0} is not valid from provided csv.".Fill(clientCode);
                        //ReportInvalidBatch(InsertInvalidBatchInfo(batchName, lastModifiedTime, msg), "as");
                        continue;
                    }

                    string batchPathByClient = Path.Combine(localCsvDirPath, client.AppCode);

                    if (!Directory.Exists(batchPathByClient))
                    {
                        log.Info("Directory path was not exist.So creating new directory based on client {0}".Fill(batchPathByClient));
                        Directory.CreateDirectory(batchPathByClient);
                    }

                    //NOTE : Path : {channelDownloadPath}/{localCsvFilePath}.FileNameWithOutExtention/{ClientCode}/{XML}

                    mailBatch = new MailBatchDTO
                    {
                        ClientId = client.Id,
                        BatchName = batchName,
                        SFTChannelId = SFTChannel.Id,
                        ReceivedOn = lastModifiedTime,
                        PipelineStatus = BatchPipelineStatus.Received,
                        BatchStatus = BatchStatus.InProgress.ToString(),
                        //Parse AM mail batch request element
                        UniqueId = Guid.NewGuid(),
                        Priority = Priority.Medium.ToString(),
                        ClientBatchId = batchName,
                        IsReturnInfoSpecified = false
                    };

                    MailBatch mb = new MailBatch()
                    {
                        ClientCode = clKey
                    };

                    int cnt = 0;
                    foreach (var loanInfo in caponeClientData[clKey])
                    {
                        cnt++;
                        MailPiece mp = new MailPiece()
                        {
                            BusinessFriendlyIdName = "LoanNumber",
                            BusinessFriendlyIdValue = loanInfo.LoanNumber,
                            DocumentOrPackageType = "Generic",
                            Id = Guid.NewGuid()
                        };

                        Entities.Address borrowerAddress = new Entities.Address();
                        borrowerAddress.Street = loanInfo.BorrowerStreetAddress;
                        borrowerAddress.City = loanInfo.BorrowerCity;
                        borrowerAddress.State = loanInfo.BorrowerState;
                        borrowerAddress.Zip = loanInfo.BorrowerZip;

                        Entities.Recipient borrowerName = new Entities.Recipient();
                        borrowerName.FirstName = loanInfo.BorrowerFirstName;
                        borrowerName.LastName = loanInfo.BorrowerLastName;

                        AM.Entities.RecipientInfo recipientInfo = new Entities.RecipientInfo();
                        recipientInfo.Recipient = borrowerName;
                        recipientInfo.Address = borrowerAddress;

                        List<AM.Entities.Metadata> mpCustInfo = new List<Metadata>();
                        mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "PropertyStreetAddress", Value = loanInfo.PropertyStreetAddress });
                        mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "PropertyCity", Value = loanInfo.PropertyCity });
                        mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "PropertyState", Value = loanInfo.PropertyState });
                        mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "PropertyZip", Value = loanInfo.PropertyZip });
                        mpCustInfo.Add(new Metadata() { DataType = DataType.DateTime, Key = "DocumentDate", Value = loanInfo.DocumentDate.ToString("MMMM dd, yyyy") });
                        mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "NoticeDate", Value = loanInfo.NoticeDate.ToString("MM/dd/yyyy") });
                        mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "CommunityName", Value = loanInfo.CommunityName });

                        mp.RecipientInfo = recipientInfo;
                        mp.CustomInfo = mpCustInfo;

                        DocumentOrPackage borrowerDocument = new DocumentOrPackage();
                        borrowerDocument.Name = "{0}_{1}.pdf".Fill(loanInfo.LoanNumber.Trim(), cnt);

                        mp.DocumentOrPackage = borrowerDocument;
                        //NOTE : CODE FOR Generating PDFs for selected templates Naming Conventions = {LoanNumber+ +_+{cnt}.pdf                       
                        mb.MailPieces.Add(mp);
                    }

                    //NOTE : Generate XML 

                    var xmlByClient = mb.Serialize<MailBatch>();

                    string xmlFilePath = Path.Combine(localCsvDirPath,client.AppCode, "{0}_{1}.xml".Fill(client.AppCode, lastModifiedTime.ToString("MMddyyyyhhmmss")));
                    File.WriteAllText(xmlFilePath, xmlByClient);

                    mbList.Add(mailBatch);
                }
                catch (Exception ex)
                {
                    var msg = "Error occured while processing batch from Batch '{0}.xml'".Fill(batchName);
                    log.Error(msg, ex);

                    if (Directory.Exists(localCsvDirPath))
                        Directory.Delete(localCsvDirPath, true);

                    MailUtility.SendUnexpectedExceptionEmail(msg + "<br/> Error Message is {0}".Fill(ex));
                    return isAllSuccess;
                }
            }

            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    foreach (var mb in mbList)
                    {

                        mb.Id = mailBatchSvc.InsertBatchDetails(mb);
                        Client client = clientSvc.GetClientById(mb.ClientId);
                        //NOTE : Generate Batch path
                        string channelBatchPath = Path.Combine(localCsvDirPath, client.AppCode);
                        MailBatchInfo mbi = new MailBatchInfo(mb, client, Config.Common_AuthoMailRootFolder);

                        //Here we create folder structure
                        mbi.AMResponsePath.CreateDirectoryIfNotExist();
                        mbi.PnSRequestPath.CreateDirectoryIfNotExist();
                        mbi.PnSResponsePath.CreateDirectoryIfNotExist();
                        //Here we are moving whole directory so no need to create AMRequestPath directly.
                        Directory.Move(channelBatchPath, mbi.AMRequestPath);

                        commonSvc.UpdateBatchPipeLineStatus(mb.Id, BatchPipelineStatus.Received, Common.Now).Wait();
                    }
                    scope.Complete();
                    isAllSuccess = true;
                }
                catch (Exception ex)
                {
                    var msg = "Error occured while inserting General Channel batches details for Batch '{0}.xml'".Fill(batchName);
                    log.Error(msg, ex);
                    MailUtility.SendUnexpectedExceptionEmail(msg + "<br/> Error Message is {0}".Fill(ex));

                    return isAllSuccess;
                }
            }
            return isAllSuccess;
        }

        private void ReportInvalidBatch(MailBatchDTO mailBatch, string localXMLFilePath)
        {
            Client client = clientSvc.GetClientById(mailBatch.ClientId);

            Deliverable deliverable = commonSvc.GetDeliverableByReportType(client.Id, ReportTypes.Error);

            MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, Config.Common_AuthoMailRootFolder);

            mbi.AMRequestPath.CreateDirectoryIfNotExist();
            mbi.AMResponsePath.CreateDirectoryIfNotExist();

            if (!string.IsNullOrEmpty(localXMLFilePath))
                File.Move(localXMLFilePath, Path.Combine(mbi.AMRequestPath, mbi.BatchName + ".zip"));

            //For Invalid Batch, we will generate err file 
            string invalidBatchFileName = "{0}_InvalidZipfile.err".Fill(mailBatch.BatchName);

            string invalidBatchPath = Path.Combine(mbi.AMResponsePath, invalidBatchFileName);

            File.WriteAllText(invalidBatchPath, string.Empty);

            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportPrepped, Common.Now).Wait();

            string remoteFilePath = Path.Combine(deliverable.UploadPath, invalidBatchFileName);

            var mailBatchSFTChannel = clientSvc.GetSFTChannelDetailsBySFTChannelId(mailBatch.SFTChannelId).Result;

            SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(mailBatchSFTChannel.Protocol), mailBatchSFTChannel.Host, mailBatchSFTChannel.UserName, mailBatchSFTChannel.Password, 300000);
            sftSite.Connect();

            sftSite.UploadFile(invalidBatchPath, remoteFilePath);

            if (sftSite.IsFileAvailable(Path.Combine(mailBatchSFTChannel.ToADEDir, mbi.BatchName + ".zip")))
                //MoveBatchToBackupDir(sftSite, Path.Combine(mailBatchSFTChannel.ToADEDir, mbi.BatchName + ".zip"));

                //MoveBatchToBackupDir(sftSite, Path.Combine(mailBatchSFTChannel.ToADEDir, mbi.BatchName + ".go"));

                if (sftSite.IsConnected)
                    sftSite.Disconnect();

            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportedBack, Common.Now).Wait();
        }

        private MailBatchDTO InsertInvalidBatchInfo(string batchName, DateTime receivedOn, string errorMessage)
        {
            MailBatchDTO mailBatch = new MailBatchDTO();
            mailBatch.UniqueId = Guid.NewGuid();
            mailBatch.ClientId = GetInvalidClient(SFTChannel.Code.Split('_')[0]).Id;
            mailBatch.SFTChannelId = SFTChannel.Id;
            mailBatch.BatchName = batchName;
            mailBatch.ClientBatchId = batchName;
            mailBatch.Priority = Priority.Low.ToString();
            mailBatch.Id = mailBatchSvc.InsertBatchDetails(mailBatch);
            commonSvc.UpdateBatchPipeLineStatus(mailBatch.Id, BatchPipelineStatus.Received, receivedOn).Wait();
            mailBatchSvc.MarkMailBatchAsInvalid(mailBatch.Id, errorMessage).Wait();
            commonSvc.UpdateBatchStatus(mailBatch.Id, BatchStatus.Failed.ToString()).Wait();
            return mailBatch;
        }

        private Client GetInvalidClient(string sftChannelCode)
        {
            return clientSvc.GetClientByAppCode(sftChannelCode + "_Invalid").Result;
        }

        private CapOneData FromCsv(string csvLine)
        {            
            List<string> values = csvLine.SplitCsv();
            CapOneData caponeValues = new CapOneData();
            caponeValues.BorrowerLastName = values[2].RemoveCSVStringFormat();
            caponeValues.BorrowerFirstName = values[3].RemoveCSVStringFormat();
            caponeValues.BorrowerStreetAddress = values[4].RemoveCSVStringFormat();
            caponeValues.BorrowerCity = values[5].RemoveCSVStringFormat();
            caponeValues.BorrowerState = values[6].RemoveCSVStringFormat();
            caponeValues.BorrowerZip = values[7].RemoveCSVStringFormat();
            caponeValues.DocumentDate = values[8].RemoveCSVStringFormat().AnyFormatToDateTime().Value;
            caponeValues.PropertyStreetAddress = values[9].RemoveCSVStringFormat();
            caponeValues.PropertyState = values[10].RemoveCSVStringFormat();
            caponeValues.PropertyCity = values[11].RemoveCSVStringFormat();
            caponeValues.PropertyZip = values[12].RemoveCSVStringFormat();
            caponeValues.CommunityName = values[13].RemoveCSVStringFormat();
            caponeValues.LoanNumber = values[15].RemoveCSVStringFormat();
            caponeValues.NoticeDate = values[18].RemoveCSVStringFormat().AnyFormatToDateTime().Value;
            caponeValues.ClientCode = values[22].RemoveCSVStringFormat();
            return caponeValues;
        }

        private string DownloadBatchCsvFile(SFTUtil sftSite, string ftpCsvFilePath, string localCsvFilePath)
        {
            sftSite.DownloadFile(ftpCsvFilePath, localCsvFilePath);

            return localCsvFilePath;
        }

        private void MoveBatchToBackupDir(SFTUtil sftSite, string ftpFilePath)
        {
            string ftpBackUpFilePath = Path.Combine(SFTChannel.ArchiveDir, Path.GetFileName(ftpFilePath));
            if (sftSite.IsFileAvailable(ftpFilePath))
            {
                if (SFTChannel.PostDownloadAction == PostDownloadAction.Archive)
                {
                    sftSite.MoveFile(ftpFilePath, ftpBackUpFilePath);
                }
                else
                    sftSite.DeleteFile(ftpFilePath);
            }
        }

        private List<string> GetFilePaths(SFTUtil sftSite)
        {
            var regex = new Regex(@"^(\S*).csv$", RegexOptions.IgnoreCase);

            List<string> availableFilePaths = sftSite.ListDirectory(SFTChannel.ToADEDir)
                .Select(f => regex.Match(Path.GetFileName(f)))
                .Where(m => m.Success)
                .Select(m => m.Groups[1].Value)
                .ToList();

            if (availableFilePaths != null && availableFilePaths.Count > 0)
            {
                log.Debug(string.Format("{0} trigger file(s) found.", availableFilePaths.Count));
            }
            else
            {
                log.Error("No trigger files found hence no batches are available to download.");
            }

            return availableFilePaths;
        }

        protected override void DownloadBatches()
        {
            log.Debug("CAPONE download job is started.");

            List<MailBatchDTO> downloadBatches = mailBatchSvc.GetBatchesByPipelineStatusAndSFTChannelId(BatchPipelineStatus.Received, SFTChannel.Id);
            if (downloadBatches.Count == 0)
            {
                log.Debug("CAPONE download job queue is empty.");
                return;
            }

            SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(SFTChannel.Protocol), SFTChannel.Host, SFTChannel.UserName, SFTChannel.Password, 300000);
            sftSite.Connect();

            foreach (var mailBatch in downloadBatches)
            {
                try
                {
                    Client client = clientSvc.GetClientById(mailBatch.ClientId);

                    MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, Config.Common_AuthoMailRootFolder);

                    CustomTemplate customTemplate = commonSvc.GetCustomTemplateByClientId(client.Id);

                    if (customTemplate == null)
                    {
                        log.Error("No Tempalte found");
                        //NOTE Report Invalid
                        continue;
                    }

                    var templateFilePath = Path.Combine(Config.Monitor_CustomTemplateFolder, client.AppCode, customTemplate.CustomTemplatePath);

                    var xmlFilePath = Directory.EnumerateFiles(mbi.AMRequestPath, "*.xml").SingleOrDefault();

                    if (string.IsNullOrWhiteSpace(xmlFilePath))
                    {
                        log.Error("File does not exist");
                        //Report Invalid
                    }

                    var mbData = File.ReadAllText(xmlFilePath).Deserialize<MailBatch>();

                    foreach (var mp in mbData.MailPieces)
                    {
                        Dictionary<string, string> templateFieldData = new Dictionary<string, string>();
                        templateFieldData.Add("LoanNumber", mp.BusinessFriendlyIdValue);
                        templateFieldData.Add("BorrowerName", mp.RecipientInfo != null ? mp.RecipientInfo.Recipient != null ? "{0} {1}".Fill(mp.RecipientInfo.Recipient.LastName, mp.RecipientInfo.Recipient.FirstName).Trim() : "" : "");
                        templateFieldData.Add("BorrowerAddress", mp.RecipientInfo != null ? mp.RecipientInfo.Address != null ? "{0}\n{1}".Fill(mp.RecipientInfo.Address.Street, "{0}, {1} {2}".Fill(mp.RecipientInfo.Address.City, mp.RecipientInfo.Address.State, mp.RecipientInfo.Address.Zip)).Trim() : "" : "");

                        if (mp.CustomInfo != null)
                        {
                            templateFieldData.Add("PropertyAddress1", mp.CustomInfo.SingleOrDefault(m => m.Key == "PropertyStreetAddress").Value);
                            templateFieldData.Add("PropertyAddress2", "{0}, {1} {2}".Fill(mp.CustomInfo.SingleOrDefault(m => m.Key == "PropertyCity").Value, mp.CustomInfo.SingleOrDefault(m => m.Key == "PropertyState").Value, mp.CustomInfo.SingleOrDefault(m => m.Key == "PropertyZip").Value));
                            templateFieldData.Add("DocumentDate", mp.CustomInfo.SingleOrDefault(m => m.Key == "DocumentDate").Value);
                            templateFieldData.Add("NoticeDate", mp.CustomInfo.SingleOrDefault(m => m.Key == "NoticeDate").Value);
                            templateFieldData.Add("CommunityName", mp.CustomInfo.SingleOrDefault(m => m.Key == "CommunityName").Value);
                        }

                        var printReadyTemplatePath = Path.Combine(mbi.AMRequestPath, mp.DocumentOrPackage.Name);

                        PdfUtil.PdfFormFiller(templateFilePath, printReadyTemplatePath, templateFieldData);
                    }

                    commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Downloaded, Common.Now).Wait();
                }
                catch (Exception ex)
                {
                    var msg = "Error occured in DownloadBatches Method For Mail Batch Id: {0} with Error : {1}".Fill(mailBatch.Id, ex.Message);
                    log.Error(msg, ex);
                    MailUtility.SendUnexpectedExceptionEmail(msg);
                    continue;
                }
            }

            if (sftSite.IsConnected)
                sftSite.Disconnect();

            log.Debug("Download job queue is completed.");
        }

        protected override void ValidateBatches()
        {
            base.ValidateBatches();
        }

        protected override void AcknowledgeBatches()
        {
            base.AcknowledgeBatches();
        }
    }
}
