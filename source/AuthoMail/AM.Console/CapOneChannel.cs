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
                    string localCsvFilePath = Path.Combine(channelDownloadPath, csvFileName);
                    string ftpCsvFilePath = Path.Combine(SFTChannel.ToADEDir, csvFileName);

                    if (!sftSite.IsFileAvailable(ftpCsvFilePath))
                    {
                        var msg = "'{0}.csv' file is missing on SFT site.".Fill(batchName);
                        //ReportInvalidBatch(InsertInvalidBatchInfo(batchName, lastModifiedTime, msg), "");
                        continue;
                    }

                    //Some time it is not creating AMC folder in flow so here we are again checking this if not created then below code will create
                    if (!Directory.Exists(channelDownloadPath))
                    {
                        log.Error("General Channel Folder was not created as expected.");
                        Directory.CreateDirectory(channelDownloadPath);
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
                    SeparatePackages(caponeClientData, localCsvFilePath, batchName, channelDownloadPath, lastModifiedTime, sftSite);
                    #endregion

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

        private void SeparatePackages(Dictionary<string, List<CapOneData>> caponeClientData, string localCsvFilePath, string batchName, string channelDownloadPath, DateTime lastModifiedTime, SFTUtil sftSite)
        {
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
                        ReportInvalidBatch(InsertInvalidBatchInfo(batchName, lastModifiedTime, msg), "as");
                        continue;
                    }

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

                    foreach (var loanInfo in caponeClientData[clKey])
                    {
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

                        mb.MailPieces.Add(mp);
                    }

                }
                catch (Exception ex)
                {
                    var msg = "Error occured while processing batch from Batch '{0}.xml'".Fill(batchName);
                    log.Error(msg, ex);
                    MailUtility.SendUnexpectedExceptionEmail(msg + "<br/> Error Message is {0}".Fill(ex));
                    return;
                }
            }

            //using (TransactionScope scope = new TransactionScope())
            //{
            //    try
            //    {
            //        mailBatch.Id = mailBatchSvc.InsertBatchDetails(mailBatch, mailBatchCustomInfoList);

            //        MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, Config.Common_AuthoMailRootFolder);

            //        //Here we create folder structure
            //        mbi.AMResponsePath.CreateDirectoryIfNotExist();
            //        mbi.PnSRequestPath.CreateDirectoryIfNotExist();
            //        mbi.PnSResponsePath.CreateDirectoryIfNotExist();
            //        //Here we are moving whole directory so no need to create AMRequestPath directly.
            //        Directory.Move(batchPath, mbi.AMRequestPath);

            //        commonSvc.UpdateBatchPipeLineStatus(mailBatch.Id, BatchPipelineStatus.Downloaded, Common.Now).Wait();

            //        scope.Complete();

            //    }
            //    catch (Exception ex)
            //    {
            //        var msg = "Error occured while inserting General Channel batches details for Batch '{0}.xml'".Fill(batchName);
            //        log.Error(msg, ex);
            //        MailUtility.SendUnexpectedExceptionEmail(msg + "<br/> Error Message is {0}".Fill(ex));
            //    }
            //}

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
            string[] values = csvLine.Split(',');
            CapOneData caponeValues = new CapOneData();
            caponeValues.BorrowerLastName = values[2];
            caponeValues.BorrowerFirstName = values[3];
            caponeValues.BorrowerStreetAddress = values[4];
            caponeValues.BorrowerCity = values[5];
            caponeValues.BorrowerState = values[6];
            caponeValues.BorrowerZip = values[7];
            caponeValues.DocumentDate = Convert.ToDateTime(values[8]);
            caponeValues.PropertyStreetAddress = values[9];
            caponeValues.PropertyState = values[10];
            caponeValues.PropertyCity = values[11];
            caponeValues.PropertyZip = values[12];
            caponeValues.CommunityName = values[13];
            caponeValues.LoanNumber = values[15];
            caponeValues.NoticeDate = Convert.ToDateTime(values[18]);
            caponeValues.ClientCode = values[22];
            return caponeValues;
        }

        private string DownloadBatchCsvFile(SFTUtil sftSite, string ftpCsvFilePath, string localCsvFilePath)
        {
            sftSite.DownloadFile(ftpCsvFilePath, localCsvFilePath);

            return localCsvFilePath;
        }

        public string ToXML(MailBatch mailBatch)
        {
            using (var stringwriter = new System.IO.StringWriter())
            {
                var serializer = new XmlSerializer(mailBatch.GetType());
                serializer.Serialize(stringwriter, mailBatch);
                return stringwriter.ToString();
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
            //Generate template logic and xml
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
