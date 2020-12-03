using AM.Console.Properties;
using AM.Entities;
using AM.Lib;
using AM.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
                    string channelDownloadPath = Path.Combine(Config.Common_AuthoMailRootFolder, "GENERAL", SFTChannel.GetChannelCode());

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
                        ReportInvalidBatch(InsertInvalidBatchInfo(batchName, lastModifiedTime, msg), localCsvFilePath);
                        continue;
                    }

                    //Some time it is not creating AMC folder in flow so here we are again checking this if not created then below code will create
                    if (!Directory.Exists(localCsvDirPath))
                    {
                        log.Error("General Channel Folder was not created as expected.");
                        Directory.CreateDirectory(localCsvDirPath);
                    }

                    DownloadBatchCsvFile(sftSite, ftpCsvFilePath, localCsvFilePath);
                    string localArchivePath = Path.Combine(Config.Common_AuthoMailRootFolder, "Archive", "{0}.csv".Fill(batchName));
                    //Generate List from CSV 
                    Dictionary<string, List<CapOneData>> caponeClientData = new Dictionary<string, List<CapOneData>>();
                    var lines = File.ReadAllLines(localCsvFilePath).ToList();
                    if (lines.Count <= 1)
                    {
                        MailUtility.SendUnexpectedExceptionEmail("No records found for received file {0}.csv".Fill(batchName));
                        if (File.Exists(localCsvFilePath))
                        {
                            if (Directory.Exists(Path.Combine(Config.Common_AuthoMailRootFolder, "Archive")))
                            {

                                if (File.Exists(localArchivePath))
                                    File.Delete(localArchivePath);

                                File.Move(localCsvFilePath, localArchivePath);
                            }
                        }
                        MoveBatchToBackupDir(sftSite, ftpCsvFilePath);
                        continue;
                    }

                    List<CapOneData> data = lines.Skip(1).Select(v => FromCsv(v)).ToList();

                    var clData = data.GroupBy(cl => cl.ClientCode);
                    foreach (var clInfo in clData)
                    {
                        caponeClientData.Add(clInfo.Key, clInfo.Where(cl => cl.ClientCode == clInfo.Key).ToList());
                    }

                    bool isAllSuccess = SeparatePackages(caponeClientData, localCsvFilePath, batchName, localCsvDirPath, lastModifiedTime, sftSite);
                    #endregion
                    //Here after moving all batches to particular client location , archive amc batch

                    if (File.Exists(localArchivePath))
                        File.Delete(localArchivePath);
                    if(File.Exists(localCsvFilePath))
                        File.Move(localCsvFilePath, localArchivePath);
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
                        ReportInvalidBatch(InsertInvalidBatchInfo(batchName, lastModifiedTime, msg), localCsvFilePath);
                        continue;
                    }

                    client = clientSvc.GetClientByName(clientCode).Result;

                    if (client == null)
                    {
                        var msg = "Client code {0} is not valid from provided csv.".Fill(clientCode);
                        ReportInvalidBatch(InsertInvalidBatchInfo(batchName, lastModifiedTime, msg), localCsvFilePath);
                        continue;
                    }

                    string batchPathByClient = Path.Combine(localCsvDirPath, client.AppCode);

                    if (!Directory.Exists(batchPathByClient))
                    {
                        Thread.Sleep(1000);
                        log.Info("Directory path was not exist.So creating new directory based on client {0}".Fill(batchPathByClient));
                        Directory.CreateDirectory(batchPathByClient);
                    }

                    if (!Directory.Exists(batchPathByClient))
                    {
                        Thread.Sleep(1000);
                        log.Info("Directory path was not exist.So creating new directory based on client {0}".Fill(batchPathByClient));
                        Directory.CreateDirectory(batchPathByClient);
                    }
                    log.Info("Local Folder is created");
                    //NOTE : Path : {channelDownloadPath}/{localCsvFilePath}.FileNameWithOutExtention/{ClientCode}/{XML}
                    string xmlName = "{0}_{1}".Fill(client.AppCode, lastModifiedTime.ToString("MMddyyyyhhmmss"));
                    var batchUniqueId = Guid.NewGuid();
                    mailBatch = new MailBatchDTO
                    {
                        ClientId = client.Id,
                        BatchName = xmlName,
                        SFTChannelId = SFTChannel.Id,
                        ReceivedOn = lastModifiedTime,
                        PipelineStatus = BatchPipelineStatus.Received,
                        BatchStatus = BatchStatus.InProgress.ToString(),
                        //Parse AM mail batch request element
                        UniqueId = batchUniqueId,
                        Priority = Priority.Medium.ToString(),
                        ClientBatchId = batchName,
                        IsReturnInfoSpecified = false
                    };

                    MailBatch mb = new MailBatch()
                    {
                        ClientCode = client.AppCode,
                        Id = batchUniqueId
                    };

                    int cnt = 0;
                    foreach (var loanInfo in caponeClientData[clKey])
                    {
                        cnt++;
                        MailPiece mp = new MailPiece()
                        {
                            BusinessFriendlyIdName = "Flood Cert Number",
                            BusinessFriendlyIdValue = loanInfo.FloodCertNumber.IsNullOrWhiteSpace() ? null : loanInfo.FloodCertNumber,
                            DocumentOrPackageType = docTypeSvc.GetDocTypeByDocTypeName(mailBatch.ClientId, loanInfo.ClientId.Trim(), 0).DocTypeName,
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
                        mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "LoanNumber", Value = loanInfo.LoanNumber });
                        mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "PropertyStreetAddress", Value = loanInfo.PropertyStreetAddress });
                        mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "PropertyCity", Value = loanInfo.PropertyCity });
                        mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "PropertyState", Value = loanInfo.PropertyState });
                        mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "PropertyZip", Value = loanInfo.PropertyZip });
                        mpCustInfo.Add(new Metadata() { DataType = DataType.DateTime, Key = "DocumentDate", Value = Convert.ToDateTime(loanInfo.DocumentDate).ToString("MMMM dd, yyyy") });
                        mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "NoticeDate", Value = Convert.ToDateTime(loanInfo.NoticeDate).ToString("MM/dd/yyyy") });
                        mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "CommunityName", Value = loanInfo.CommunityName });
                        mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "CommunityCode", Value = loanInfo.CommunityCode });
                        mpCustInfo.Add(new Metadata() { DataType = DataType.String, Key = "ClientId", Value = loanInfo.ClientId });

                        mp.RecipientInfo = recipientInfo;
                        mp.CustomInfo = mpCustInfo;

                        DocumentOrPackage borrowerDocument = new DocumentOrPackage();
                        borrowerDocument.Name = "{0}_{1}.pdf".Fill(loanInfo.FloodCertNumber.Trim(), cnt);

                        mp.DocumentOrPackage = borrowerDocument;
                        //NOTE : CODE FOR Generating PDFs for selected templates Naming Conventions = {LoanNumber+ +_+{cnt}.pdf                       
                        mb.MailPieces.Add(mp);
                    }

                    //NOTE : Generate XML 

                    var xmlByClient = mb.Serialize<MailBatch>();

                    string xmlFilePath = Path.Combine(localCsvDirPath, client.AppCode, "{0}.xml".Fill(xmlName));
                    File.WriteAllText(xmlFilePath, xmlByClient);
                    log.Info("Local XML File is created");
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

        private void ReportInvalidBatch(MailBatchDTO mailBatch, string localCsvFilePath)
        {
            Client client = clientSvc.GetClientById(mailBatch.ClientId);

            Deliverable deliverable = commonSvc.GetDeliverableByReportType(client.Id, ReportTypes.Error);

            MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, Config.Common_AuthoMailRootFolder);

            mbi.AMRequestPath.CreateDirectoryIfNotExist();
            mbi.AMResponsePath.CreateDirectoryIfNotExist();

            if (!string.IsNullOrEmpty(localCsvFilePath))
                File.Move(localCsvFilePath, Path.Combine(mbi.AMRequestPath, mbi.BatchName + ".csv"));

            //For Invalid Batch, we will generate err file 
            string invalidBatchFileName = "{0}_InvalidCsvfile.err".Fill(mailBatch.BatchName);

            string invalidBatchPath = Path.Combine(mbi.AMResponsePath, invalidBatchFileName);

            File.WriteAllText(invalidBatchPath, string.Empty);

            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportPrepped, Common.Now).Wait();

            string remoteFilePath = Path.Combine(deliverable.UploadPath, invalidBatchFileName);

            var mailBatchSFTChannel = clientSvc.GetSFTChannelDetailsBySFTChannelId(mailBatch.SFTChannelId).Result;

            SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(mailBatchSFTChannel.Protocol), mailBatchSFTChannel.Host, mailBatchSFTChannel.UserName, mailBatchSFTChannel.Password, 300000);
            sftSite.Connect();

            sftSite.UploadFile(invalidBatchPath, remoteFilePath);

            if (sftSite.IsFileAvailable(Path.Combine(mailBatchSFTChannel.ToADEDir, mbi.BatchName + ".csv")))
                MoveBatchToBackupDir(sftSite, Path.Combine(mailBatchSFTChannel.ToADEDir, mbi.BatchName + ".csv"));

            if (sftSite.IsConnected)
                sftSite.Disconnect();

            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportedBack, Common.Now).Wait();
        }

        private MailBatchDTO InsertInvalidBatchInfo(string batchName, DateTime receivedOn, string errorMessage)
        {
            MailBatchDTO mailBatch = new MailBatchDTO();
            mailBatch.UniqueId = Guid.NewGuid();
            mailBatch.ClientId = GetInvalidClient(SFTChannel.GetChannelCode()).Id;
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
            caponeValues.FloodCertNumber = values[0].RemoveCSVStringFormat();
            caponeValues.BorrowerLastName = values[2].RemoveCSVStringFormat();
            caponeValues.BorrowerFirstName = values[3].RemoveCSVStringFormat();
            caponeValues.BorrowerStreetAddress = values[4].RemoveCSVStringFormat();
            caponeValues.BorrowerCity = values[5].RemoveCSVStringFormat();
            caponeValues.BorrowerState = values[6].RemoveCSVStringFormat();
            caponeValues.BorrowerZip = values[7].RemoveCSVStringFormat();
            caponeValues.DocumentDate = values[8].RemoveCSVStringFormat().AnyFormatToDateTimeString();
            caponeValues.PropertyStreetAddress = values[9].RemoveCSVStringFormat();
            caponeValues.PropertyCity = values[10].RemoveCSVStringFormat();
            caponeValues.PropertyState = values[11].RemoveCSVStringFormat();
            caponeValues.PropertyZip = values[12].RemoveCSVStringFormat();
            caponeValues.CommunityName = values[13].RemoveCSVStringFormat();
            caponeValues.CommunityCode = values[14].RemoveCSVStringFormat();
            caponeValues.LoanNumber = values[15].RemoveCSVStringFormat();
            caponeValues.NoticeDate = values[18].RemoveCSVStringFormat().AnyFormatToDateTimeString();
            caponeValues.ClientId = values[22].RemoveCSVStringFormat();
            caponeValues.ClientCode = values[23].RemoveCSVStringFormat();
            return caponeValues;
        }

        private string DownloadBatchCsvFile(SFTUtil sftSite, string ftpCsvFilePath, string localCsvFilePath)
        {
            sftSite.DownloadFile(ftpCsvFilePath, localCsvFilePath);

            return localCsvFilePath;
        }

        private void MoveBatchToBackupDir(SFTUtil sftSite, string ftpFilePath)
        {
            if (sftSite.IsFileAvailable(ftpFilePath))
            {
                if (SFTChannel.PostDownloadAction == PostDownloadAction.Archive)
                {
                    string ftpBackUpFilePath = Path.Combine(SFTChannel.ArchiveDir, Path.GetFileName(ftpFilePath));
                    sftSite.MoveFile(ftpFilePath, ftpBackUpFilePath);
                }
                else
                    sftSite.DeleteFile(ftpFilePath);
            }
        }

        private List<string> GetFilePaths(SFTUtil sftSite)
        {
            var regex = new Regex(@"^([\s\S]*).csv", RegexOptions.IgnoreCase);

            List<string> availableFilePaths = sftSite.ListDirectory(SFTChannel.ToADEDir)
                .Select(f => regex.Match(Path.GetFileName(f)))
                .Where(m => m.Success)
                .Select(m => m.Groups[1].Value)
                .ToList();

            if (availableFilePaths != null && availableFilePaths.Count > 0)
            {
                log.Debug(string.Format("{0} file(s) found.", availableFilePaths.Count));
            }
            else
            {
                log.Error("No files found hence no batches are available to download.");
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

                    var xmlFilePath = Directory.EnumerateFiles(mbi.AMRequestPath, "*.xml").SingleOrDefault();

                    if (string.IsNullOrWhiteSpace(xmlFilePath))
                    {
                        log.Error("File does not exist");
                        MailUtility.SendUnexpectedExceptionEmail("No xml found for generating pdf for Batch Id {0} and client name : {1}".Fill(mbi.MailBatch.Id, client.AppCode));
                        continue;
                    }

                    var mbData = File.ReadAllText(xmlFilePath).Deserialize<MailBatch>();

                    foreach (var mp in mbData.MailPieces)
                    {
                        #region Create Template with iTextSharp 
                        //Dictionary<string, string> templateFieldData = new Dictionary<string, string>();
                        //templateFieldData.Add("LoanNumber", mp.BusinessFriendlyIdValue);
                        //templateFieldData.Add("BorrowerName", mp.RecipientInfo != null ? mp.RecipientInfo.Recipient != null ? "{0} {1}".Fill(mp.RecipientInfo.Recipient.LastName, mp.RecipientInfo.Recipient.FirstName).Trim() : "" : "");
                        //templateFieldData.Add("BorrowerAddress", mp.RecipientInfo != null ? mp.RecipientInfo.Address != null ? "{0}\n{1}".Fill(mp.RecipientInfo.Address.Street, "{0}, {1} {2}".Fill(mp.RecipientInfo.Address.City, mp.RecipientInfo.Address.State, mp.RecipientInfo.Address.Zip)).Trim() : "" : "");

                        //if (mp.CustomInfo != null)
                        //{
                        //    templateFieldData.Add("Subject", "Proposed loan to be made to {0} by Capital One Multifamily Finance, LLC, secured by real property known as {1}, {2}, {3} {4}".Fill(mp.RecipientInfo != null ? mp.RecipientInfo.Recipient != null ? "{0} {1}".Fill(mp.RecipientInfo.Recipient.LastName, mp.RecipientInfo.Recipient.FirstName).Trim() : "" : "", mp.CustomInfo.SingleOrDefault(m => m.Key == "PropertyStreetAddress").Value, mp.CustomInfo.SingleOrDefault(m => m.Key == "PropertyCity").Value, mp.CustomInfo.SingleOrDefault(m => m.Key == "PropertyState").Value, mp.CustomInfo.SingleOrDefault(m => m.Key == "PropertyZip").Value));
                        //    templateFieldData.Add("PropertyFullAddress", mp.CustomInfo.SingleOrDefault(m => m.Key == "PropertyStreetAddress").Value);
                        //    templateFieldData.Add("PropertyAddress1", mp.CustomInfo.SingleOrDefault(m => m.Key == "PropertyStreetAddress").Value);
                        //    templateFieldData.Add("PropertyAddress2", "{0}, {1} {2}".Fill(mp.CustomInfo.SingleOrDefault(m => m.Key == "PropertyCity").Value, mp.CustomInfo.SingleOrDefault(m => m.Key == "PropertyState").Value, mp.CustomInfo.SingleOrDefault(m => m.Key == "PropertyZip").Value));
                        //    templateFieldData.Add("DocumentDate", mp.CustomInfo.SingleOrDefault(m => m.Key == "DocumentDate").Value);
                        //    templateFieldData.Add("NoticeDate", mp.CustomInfo.SingleOrDefault(m => m.Key == "NoticeDate").Value);
                        //    templateFieldData.Add("CommunityName", mp.CustomInfo.SingleOrDefault(m => m.Key == "CommunityName").Value);
                        //}

                        //NOTE: move this line after selecting customTemplate
                        //PdfUtil.PdfFormFiller(templateFilePath, printReadyTemplatePath, templateFieldData);
                        #endregion

                        DocType docType = docTypeSvc.GetDocTypeByDocTypeName(mailBatch.ClientId, mp.DocumentOrPackageType, 0);

                        CustomTemplate customTemplate = commonSvc.GetCustomTemplateByDocTypeId(docType.Id, mp.CustomInfo.SingleOrDefault(m => m.Key == "CommunityCode").Value);
                        var printReadyTemplatePath = Path.Combine(mbi.AMRequestPath, mp.DocumentOrPackage.Name);

                        if (customTemplate == null)
                        {
                            log.Error("No Template found");
                            //NOTE Report Invalid                           

                            continue;
                        }

                        var templateFilePath = Path.Combine(Config.Monitor_CustomTemplateFolder, client.AppCode, customTemplate.CustomTemplatePath);
                        var base64Template = Convert.ToBase64String(File.ReadAllBytes(templateFilePath));
                        dynamic objFloodRequest = new ExpandoObject();
                        objFloodRequest.Data = new
                        {
                            BorrowerFullName = mp.RecipientInfo != null ? mp.RecipientInfo.Recipient != null ? "{0} {1}".Fill(mp.RecipientInfo.Recipient.FirstName, mp.RecipientInfo.Recipient.LastName).Trim() : "" : "",
                            BorrowerFullNameReverse = mp.RecipientInfo != null ? mp.RecipientInfo.Recipient != null ? "{0}, {1}".Fill(mp.RecipientInfo.Recipient.LastName, mp.RecipientInfo.Recipient.FirstName).FormattedName() : "" : "",
                            BorrFName = mp.RecipientInfo != null ? mp.RecipientInfo.Recipient != null ? "{0}".Fill(mp.RecipientInfo.Recipient.FirstName).Trim() : "" : "",
                            BorrLName = mp.RecipientInfo != null ? mp.RecipientInfo.Recipient != null ? "{0}".Fill(mp.RecipientInfo.Recipient.LastName).Trim() : "" : "",
                            BorrStreetAddress = mp.RecipientInfo != null ? mp.RecipientInfo.Address != null ? "{0}".Fill(mp.RecipientInfo.Address.Street).Trim() : "" : "",
                            BorrCity = mp.RecipientInfo != null ? mp.RecipientInfo.Address != null ? "{0}".Fill(mp.RecipientInfo.Address.City).Trim() : "" : "",
                            BorrState = mp.RecipientInfo != null ? mp.RecipientInfo.Address != null ? "{0}".Fill(mp.RecipientInfo.Address.State).Trim() : "" : "",
                            BorrZip = mp.RecipientInfo != null ? mp.RecipientInfo.Address != null ? "{0}".Fill(mp.RecipientInfo.Address.Zip).Trim() : "" : "",
                            LoanNumber = mp.CustomInfo == null ? "" : mp.CustomInfo.SingleOrDefault(m => m.Key == "LoanNumber").Value.IsNullOrWhiteSpace() ? "" : mp.CustomInfo.SingleOrDefault(m => m.Key == "LoanNumber").Value,
                            PropStreetAddress = mp.CustomInfo == null ? "" : mp.CustomInfo.SingleOrDefault(m => m.Key == "PropertyStreetAddress").Value,
                            PropCity = mp.CustomInfo == null ? "" : mp.CustomInfo.SingleOrDefault(m => m.Key == "PropertyCity")?.Value,
                            PropState = mp.CustomInfo == null ? "" : mp.CustomInfo.SingleOrDefault(m => m.Key == "PropertyState")?.Value,
                            PropZip = mp.CustomInfo == null ? "" : mp.CustomInfo.SingleOrDefault(m => m.Key == "PropertyZip")?.Value,
                            DocumentDate = mp.CustomInfo == null ? "" : mp.CustomInfo.SingleOrDefault(m => m.Key == "DocumentDate")?.Value,
                            NoticeDate = mp.CustomInfo == null ? "" : mp.CustomInfo.SingleOrDefault(m => m.Key == "NoticeDate")?.Value,
                            CommunityName = mp.CustomInfo == null ? "" : mp.CustomInfo.SingleOrDefault(m => m.Key == "CommunityName")?.Value
                        };

                        objFloodRequest.Settings = new
                        {
                            MailMergeOptions = "RemoveEmptyParagraphs",
                            ReportEngineOptions = "RemoveEmptyParagraphs",
                            OutputDocType = "pdf",
                            Template = base64Template
                        };

                        objFloodRequest.TransformData = null;

                        GenerateDoc(objFloodRequest, mp.DocumentOrPackage.Name, mbi).Wait();
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

        private async Task GenerateDoc(dynamic objFloodRequest, string pdfFileName, MailBatchInfo batchInfo)
        {
            using (var httpClient = new HttpClient())
            using (var stringContent = new StringContent(JsonConvert.SerializeObject(objFloodRequest), Encoding.UTF8, "application/json"))
            {
                HttpResponseMessage httpResponse = await httpClient.PostAsync(Config.Common_DocGen_API, stringContent);
                var responseString = await httpResponse.Content.ReadAsStringAsync();
                if (!httpResponse.IsSuccessStatusCode || httpResponse.StatusCode != HttpStatusCode.OK)
                {
                    //await _commonService.UpdateLienDisqualifyReason(lienRecord.Id, responseString);
                    return;
                }
                var docGenResponse = JsonConvert.DeserializeObject<DocGenResponse>(responseString);
                string finalPdfPath = Path.Combine(batchInfo.AMRequestPath, pdfFileName);

                File.WriteAllBytes(finalPdfPath, docGenResponse.DocumentData);
            }
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
