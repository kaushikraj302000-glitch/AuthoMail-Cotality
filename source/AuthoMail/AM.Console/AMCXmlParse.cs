using AM.Entities;
using AM.Services;
using Ionic.Zip;
using log4net;
using Renci.SshNet;
using Renci.SshNet.Sftp;
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
    public class AMCXmlParse
    {
        IClientService clientSvc;
        IBatchService batchSvc;
        IMailPieceService mailPieceSvc;
        ICommonService commonSvc;
        public AMCXmlParse()
        {
            clientSvc = new ClientService();
            batchSvc = new BatchService();
            mailPieceSvc = new MailPieceService();
            commonSvc = new CommonService();
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(AMCXmlParse));
        public void DownloadAMCXml()
        {
            Batch mailBatch = null;
            string triggerFilePath = string.Empty;
            Client client = clientSvc.GetClientDetails(null, true).Single(x => x.ClientName == "USAA");
            using (SftpClient sftp = new SftpClient(client.SFTHost, client.SFTUserName, client.SFTPassword))
            {
                sftp.KeepAliveInterval = TimeSpan.FromSeconds(20);
                sftp.Connect();

                // Check if trigger file is available or not on FTP folder.
                triggerFilePath = GetTriggerFilePath(sftp, client);

                if (!string.IsNullOrEmpty(triggerFilePath))
                {
                    // Download and Insert the batch in database first always.
                    string zipFileFTPPath = Regex.Replace(Path.ChangeExtension(triggerFilePath, ".zip"), @"(w*)_Complete", string.Empty);
                    mailBatch = GetNewBatchFromFTP(sftp, zipFileFTPPath, triggerFilePath, client);
                }
                if (sftp.IsConnected)
                    sftp.Disconnect();
            }

            if (mailBatch != null)
            {
                ParseLoans(mailBatch, client);
                //ProcessLoans(batch);
                //UploadPDFsToPnSFTP(batch); // If all PDFs uploaded successfully then batch is completed.

                //if (batch.IsCompleted && batch.TotalLoans > 0)
                //{
                //    MailUtility.SendMailToPNSDept(string.Format("{0}/{1}", batch.ReceivedOn.ToString("yyyyMMdd"), batch.BatchName), GetDocumentsCountSentForPrint(batch));
                //}

                //Common.ServiceLog(string.Format("AMC batch processed : ", batch.BatchName));
            }
        }

        private Batch GetNewBatchFromFTP(SftpClient sftp, string zipFileFTPPath, string triggerFilePath, Client client)
        {
            Batch newBatch = null;
            string downloadedZipFilePath = string.Empty;

            if (sftp.IsConnected == false)
                sftp.Connect();

            // If batch zip file exists on FTP then download and process it.
            if (sftp.Exists(zipFileFTPPath))
            {
                downloadedZipFilePath = DownloadBatchZipFile(sftp, zipFileFTPPath, client);

                if (!string.IsNullOrEmpty(downloadedZipFilePath))
                {
                    // Process downloaded zip file.
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

                        // Insert a new batch and get batchID, set batchDate
                        newBatch = new Batch()
                        {
                            BatchName = Path.GetFileNameWithoutExtension(downloadedZipFilePath),
                            ReceivedOn = DateTime.UtcNow,
                            Id = Guid.NewGuid(),
                            BatchFilePath = downloadedZipFilePath,
                            Priority = Priority.High.ToString()
                        };

                        #endregion

                        MoveTriggerFileToBackupDir(sftp, triggerFilePath, client);

                        log.Error(string.Format("Trigger file '{0}' and corresponding zip file deleted after downloading its ZIP file.", Path.GetFileName(triggerFilePath)));

                        int availableXMLFiles = Directory.GetFiles(Path.GetDirectoryName(downloadedZipFilePath), "*.xml").Count();
                        if (availableXMLFiles != 1)
                        {
                            newBatch.DownloadedOn = Common.GetCurrentTime();
                            newBatch.ValidationStatus = OutComeCodes.Invalid_Zip_File.ToString();
                            newBatch.ValidationErrors = string.Format("{0}|No XML file is available.", OutComeCodes.Invalid_Zip_File.ToString());
                        }

                        newBatch.BatchClustId = batchSvc.InsertBatchDetails(newBatch);
                        commonSvc.UpdateBatchPipeLineStatus(newBatch.BatchClustId, BatchPipelineStatus.Downloaded, Common.GetCurrentTime());
                        return newBatch;
                    }
                    catch (ZipException err)
                    {
                        // The zip file of batch is corrupt so log the error in batch table.
                        log.Error(err);
                        newBatch = new Batch()
                        {
                            Id = Guid.NewGuid(),
                            BatchName = Path.GetFileNameWithoutExtension(triggerFilePath).TrimEnd("_Complete".ToCharArray()).ToString(),
                            ReceivedOn = Common.GetCurrentTime(),
                            DownloadedOn = Common.GetCurrentTime(),
                            ValidationStatus = OutComeCodes.Invalid_Zip_File.ToString(),
                            ValidationErrors = string.Format("{0}|{1}", OutComeCodes.Invalid_Zip_File.ToString(), err.Message),
                            BatchFilePath = downloadedZipFilePath
                        };

                        newBatch.BatchClustId = batchSvc.InsertBatchDetails(newBatch);

                        MoveTriggerFileToBackupDir(sftp, triggerFilePath, client);
                    }
                }
            }
            // If the zip file is not available in the FTP folder then log that error.
            else
            {
                newBatch = new Batch()
                {
                    BatchName = Path.GetFileNameWithoutExtension(triggerFilePath).TrimEnd("_Complete".ToCharArray()).ToString(),
                    ReceivedOn = DateTime.UtcNow,
                    ValidationStatus = OutComeCodes.Invalid_Zip_File.ToString(),
                    ValidationErrors = string.Format("{0}|Trigger file found but zip file is missing on FTP.", OutComeCodes.Invalid_Zip_File.ToString()),
                    DownloadedOn = Common.GetCurrentTime()
                };

                newBatch.BatchClustId = batchSvc.InsertBatchDetails(newBatch);
                log.Error(string.Format("Trigger file {0} found but zip file missing on FTP.", Path.GetFileName(triggerFilePath)));

                MoveTriggerFileToBackupDir(sftp, triggerFilePath, client);
            }

            return null;
        }

        private void MoveTriggerFileToBackupDir(SftpClient sftp, string triggerFilePath, Client client)
        {
            if (!sftp.IsConnected)
                sftp.Connect();

            string ftpBackupDir = client.SFTADERequestArchiveDir;

            //if (!sftp.Exists(ftpBackupDir))
            //    sftp.CreateDirectory(ftpBackupDir);

            string ftpBackupTriggerFilePath = Path.Combine(ftpBackupDir, Path.GetFileName(triggerFilePath));
            if (sftp.Exists(ftpBackupTriggerFilePath))
                sftp.DeleteFile(ftpBackupTriggerFilePath);
            SftpFile triggerFile = sftp.Get(triggerFilePath);

            //triggerFile.MoveTo(ftpBackupTriggerFilePath);
            if (sftp.Exists(triggerFilePath))
                triggerFile.Delete();

        }

        private string DownloadBatchZipFile(SftpClient sftp, string batchFileFTPPath, Client client)
        {
            if (sftp.IsConnected == false)
                sftp.Connect();

            string downloadedZipFilePath = string.Empty;
            SftpFile sourceZip = sftp.Get(batchFileFTPPath);
            string batchName = Path.GetFileNameWithoutExtension(sourceZip.FullName);
            string batchLocalDir = Path.Combine(Common.AuthoMailRootFolder, client.AppCode, batchName);
            if (!Directory.Exists(batchLocalDir))
                Directory.CreateDirectory(batchLocalDir);

            downloadedZipFilePath = Path.Combine(batchLocalDir, sourceZip.Name);
            if (File.Exists(downloadedZipFilePath))
                File.Delete(downloadedZipFilePath);
            File.WriteAllText(downloadedZipFilePath, "");
            Stream downloadedXMLStrm = File.OpenWrite(downloadedZipFilePath);
            sftp.DownloadFile(sourceZip.FullName, downloadedXMLStrm);
            downloadedXMLStrm.Close();

            // After downloading the zip file delete it from FTP.
            sftp.DeleteFile(batchFileFTPPath);

            return downloadedZipFilePath;
        }

        private static string GetTriggerFilePath(SftpClient sftp, Client client)
        {
            if (sftp.IsConnected == false)
                sftp.Connect();

            string ftpTriggerFilePath = string.Empty;
            DateTime pingTime = DateTime.UtcNow;
            List<SftpFile> avialableTriggerFiles = sftp.ListDirectory(client.SFTADERequestDir).Where(f => f.IsRegularFile && Path.GetExtension(f.FullName).Equals(".go", StringComparison.CurrentCultureIgnoreCase) && Path.GetFileNameWithoutExtension(f.Name).EndsWith("_Complete")).ToList<SftpFile>();

            if (avialableTriggerFiles != null && avialableTriggerFiles.Count > 0)
            {
                ftpTriggerFilePath = avialableTriggerFiles.First().FullName;
                log.Error(string.Format("Trigger file {0} found.", Path.GetFileName(ftpTriggerFilePath)));
            }
            else
            {
                log.Error("Trigger file not found.");
            }

            return ftpTriggerFilePath;
        }

        private void ParseLoans(Batch batch, Client client)
        {
            #region Converting AMCXml to AuthoMailXml
            MailBatch amMailBatch = new MailBatch();
            List<MailPiece> amMailPieces = new List<MailPiece>();
            amMailBatch.Id = Guid.NewGuid();
            amMailBatch.Priority = Priority.High;
            amMailBatch.ChannelCode = "Mortgage Cadence Integration";
            #endregion
            XmlNamespaceManager nsManager = null;
            XmlNodeList loans = null;
            string batchDirectoryPath = Path.Combine(Common.AuthoMailRootFolder, client.AppCode, batch.BatchName);
            string xmlFilePath = Path.Combine(Common.AuthoMailRootFolder, client.AppCode, batch.BatchName, batch.BatchName + ".xml");
            if (!string.IsNullOrEmpty(xmlFilePath))
            {
                XmlDocument batchXML = new XmlDocument();
                int batchClustId = 0;
                // Try to parse the XML we got from the zip file. If XML is invalid then log error.
                try
                {
                    batchXML.LoadXml(File.ReadAllText(xmlFilePath));
                    nsManager = new XmlNamespaceManager(batchXML.NameTable);
                    nsManager.AddNamespace("x", "http://schemas.corelogic.com/2015/amc/v1/packages");
                    loans = batchXML.DocumentElement.ChildNodes;
                }
                catch (XmlException err)
                {
                    #region Invalid XML file of the batch so just save error details
                    log.Error(err);
                    batch.ValidationStatus = OutComeCodes.Invalid_XML_file.ToString();
                    batch.ValidationErrors = err.Message;
                    batchSvc.UpdateBatchDetails(batch);
                    commonSvc.UpdateBatchStatus(batchClustId, BatchValidationStatus.Invalid);

                    //TODO: Mail For Invalid XML

                    commonSvc.UpdateBatchPipeLineStatus(batchClustId, BatchPipelineStatus.ReportedBack, Common.GetCurrentTime());
                    #endregion
                }

                // If the XML is valid and found any child nodes then save those loans in DB.
                #region Process loans of XML

                if (loans != null)
                {
                    foreach (XmlNode loan in loans)
                    {
                        AMMailPiece mailPiece = null;
                        List<MailPieceCustomInfo> mailPieceCustomInfos = null;
                        PackageInfo package = null;
                        ValidationResultAMC parsingResult = null;

                        try
                        {
                            // Validate each child node of XML and store in DB along with error detail if any.
                            parsingResult = Common.ValidateAMCXMLNode(loan);

                            #region De-serialize XML nodes.

                            if (parsingResult.Code == OutComeCodes.No_error)
                            {
                                package = XmlUtil.Deserialize<PackageInfo>(loan.OuterXml);

                                #region PackageInfo To MailPiece for AuthoMail
                                MailPiece amMailPiece = new MailPiece();
                                amMailPiece.BusinessFriendlyIdName = "LoanNbr";
                                amMailPiece.BusinessFriendlyIdValue = package.LoanNbr;
                                amMailPiece.DocumentOrPackage.Name = package.File;
                                amMailPiece.DocumentOrPackageType = package.PackageName;
                                amMailPiece.Id = package.Id;
                                amMailPiece.RecipientInfo.Recipient.Phone = package.RecipientInfo.RecipientPhoneNo;
                                if (package.RecipientInfo.Recipients.Count > 1)
                                {
                                    amMailPiece.RecipientInfo.Recipient.FirstName = package.RecipientInfo.Recipients[0].FirstName;
                                    amMailPiece.RecipientInfo.Recipient.LastName = package.RecipientInfo.Recipients[0].LastName;
                                    amMailPiece.RecipientInfo.Corecipient.FirstName = package.RecipientInfo.Recipients[1].FirstName;
                                    amMailPiece.RecipientInfo.Corecipient.LastName = package.RecipientInfo.Recipients[2].LastName;
                                }
                                else
                                {
                                    amMailPiece.RecipientInfo.Recipient.FirstName = package.RecipientInfo.Recipients[0].FirstName;
                                    amMailPiece.RecipientInfo.Recipient.LastName = package.RecipientInfo.Recipients[0].LastName;
                                }

                                amMailPiece.RecipientInfo.Address.AptOrSuiteOrOther = package.RecipientInfo.RecipientAddress.Line2;
                                amMailPiece.RecipientInfo.Address.Street = package.RecipientInfo.RecipientAddress.Line1;
                                amMailPiece.RecipientInfo.Address.State = package.RecipientInfo.RecipientAddress.State;
                                amMailPiece.RecipientInfo.Address.Zip = package.RecipientInfo.RecipientAddress.Zip;
                                amMailPiece.ReturnInfo.Recipient.FirstName = package.ReturnInfo.FirstName;
                                amMailPiece.ReturnInfo.Recipient.LastName = package.ReturnInfo.LastName;
                                amMailPiece.ReturnInfo.Recipient.Phone = package.ReturnInfo.ReturnPhoneNo;
                                amMailPiece.ReturnInfo.Address.AptOrSuiteOrOther = package.ReturnInfo.ReturnAddress.Line2;
                                amMailPiece.ReturnInfo.Address.Street = package.ReturnInfo.ReturnAddress.Line1;
                                amMailPiece.ReturnInfo.Address.City = package.ReturnInfo.ReturnAddress.City;
                                amMailPiece.ReturnInfo.Address.State = package.ReturnInfo.ReturnAddress.State;
                                amMailPiece.ReturnInfo.Address.Zip = package.ReturnInfo.ReturnAddress.Zip;
                                amMailPiece.CustomInfo = new List<Metadata>() 
                                { 
                                new Metadata() { Key = "ReceivedDate", Value = package.ReceivedDate.HasValue?package.ReceivedDate.Value.ToString():"" },
                                new Metadata() { Key = "RequestId", Value = package.RequestId},
                                new Metadata() { Key = "ReceivedDate", Value = package.ShippingCredCode },
                                new Metadata() { Key = "CustomerIdentifier", Value = package.CustomerIdentifier }
                                };

                                amMailPieces.Add(amMailPiece);
                                #endregion

                                mailPiece = new AMMailPiece();
                                mailPiece.BatchClustId = batch.BatchClustId;
                                mailPiece.Id = package.Id;
                                mailPiece.FileName = package.File;
                                mailPiece.DocumentOrPackageType = package.PackageName;
                                mailPiece.BusinessFriendlyIdName = "LoanNbr";
                                mailPiece.BusinessFriendlyIdValue = package.LoanNbr;

                                if (package.RecipientInfo.Recipients.Count > 1)
                                {
                                    mailPiece.RcptFirstName = package.RecipientInfo.Recipients[0].FirstName;
                                    mailPiece.RcptLastName = package.RecipientInfo.Recipients[0].LastName;
                                    mailPiece.CoRcptFirstName = package.RecipientInfo.Recipients[1].FirstName;
                                    mailPiece.CoRcptLastName = package.RecipientInfo.Recipients[1].LastName;
                                }
                                else if (package.RecipientInfo.Recipients.Count == 1)
                                {
                                    mailPiece.RcptFirstName = package.RecipientInfo.Recipients[0].FirstName;
                                    mailPiece.RcptLastName = package.RecipientInfo.Recipients[0].LastName;
                                }

                                mailPiece.RcptAddressAptOrSuiteOrOther = package.RecipientInfo.RecipientAddress.Line2;
                                mailPiece.RcptAddressStreet = package.RecipientInfo.RecipientAddress.Line1;
                                mailPiece.RcptAddressCity = package.RecipientInfo.RecipientAddress.City;
                                mailPiece.RcptAddressState = package.RecipientInfo.RecipientAddress.State;
                                mailPiece.RcptAddressZIP = package.RecipientInfo.RecipientAddress.Zip;
                                mailPiece.RcptPhone = package.RecipientInfo.RecipientPhoneNo;

                                mailPiece.IsReturnInfoSpecified = true;
                                mailPiece.RetRcptFirstName = package.ReturnInfo.FirstName;
                                mailPiece.RetRcptLastName = package.ReturnInfo.LastName;
                                mailPiece.RetRcptAddressStreet = package.ReturnInfo.ReturnAddress.Line1;
                                mailPiece.RetRcptAddressAptOrSuiteOrOther = package.ReturnInfo.ReturnAddress.Line2;
                                mailPiece.RetRcptAddressCity = package.ReturnInfo.ReturnAddress.City;
                                mailPiece.RetRcptAddressState = package.ReturnInfo.ReturnAddress.State;
                                mailPiece.RetRcptAddressZIP = package.ReturnInfo.ReturnAddress.Zip;
                                // Check if the PDF file is available or not for loan.
                                if (File.Exists(Path.Combine(batchDirectoryPath, package.File)))
                                {
                                    mailPiece.DownloadedFilePath = Path.Combine(batchDirectoryPath, package.File);
                                    mailPiece.ErrorCode = OutComeCodes.No_error.ToString();
                                }
                                else
                                {
                                    mailPiece.ErrorCode = OutComeCodes.Missing_PDF_file.ToString();
                                    mailPiece.ErrorDesc = "PDF file missing.";
                                }

                                mailPieceSvc.InsertBatchMailPieceDetails(mailPiece);

                                mailPieceCustomInfos = new List<MailPieceCustomInfo>();
                                MailPieceCustomInfo customInfoCustomerIdentifier = new MailPieceCustomInfo();
                                customInfoCustomerIdentifier.MailPieceId = mailPiece.Id;
                                customInfoCustomerIdentifier.Key = "CustomerIdentifier";
                                customInfoCustomerIdentifier.Value = package.CustomerIdentifier.ToString();
                                mailPieceCustomInfos.Add(customInfoCustomerIdentifier);

                                MailPieceCustomInfo customInfoReceivedDate = new MailPieceCustomInfo();
                                customInfoReceivedDate.MailPieceId = mailPiece.Id;
                                customInfoReceivedDate.Key = "ReceivedDate";
                                customInfoReceivedDate.Value = package.ReceivedDate.HasValue == true ? package.ReceivedDate.Value.ToString() : "";
                                mailPieceCustomInfos.Add(customInfoReceivedDate);

                                MailPieceCustomInfo customInfoRequestId = new MailPieceCustomInfo();
                                customInfoRequestId.MailPieceId = mailPiece.Id;
                                customInfoRequestId.Key = "RequestId";
                                customInfoRequestId.Value = package.RequestId;
                                mailPieceCustomInfos.Add(customInfoRequestId);

                                MailPieceCustomInfo customInfoShippingCredCode = new MailPieceCustomInfo();
                                customInfoShippingCredCode.MailPieceId = mailPiece.Id;
                                customInfoShippingCredCode.Key = "ShippingCredCode";
                                customInfoShippingCredCode.Value = package.ShippingCredCode;
                                mailPieceCustomInfos.Add(customInfoShippingCredCode);

                                MailPieceCustomInfo customInfoShippingMethod = new MailPieceCustomInfo();
                                customInfoShippingMethod.MailPieceId = mailPiece.Id;
                                customInfoShippingMethod.Key = "ShippingMethod";
                                customInfoShippingMethod.Value = package.ShippingMethod.ToString();
                                mailPieceCustomInfos.Add(customInfoShippingMethod);

                                mailPieceSvc.InsertBatchMailPieceCustomDetails(mailPieceCustomInfos);
                            }
                            #endregion
                        }
                        catch (XmlException err)
                        {
                            #region Save error details

                            log.Error(err);
                            if (mailPiece != null)
                            {
                                if (loan.SelectSingleNode("./x:LoanNbr/text()", nsManager) == null)
                                {
                                    mailPiece.BusinessFriendlyIdName = "LoanNbr";
                                    mailPiece.BusinessFriendlyIdValue = string.Empty;
                                }
                                else
                                {
                                    mailPiece.BusinessFriendlyIdValue = loan.SelectSingleNode("./x:LoanNbr/text()", nsManager).Value;
                                }

                                if (loan.SelectSingleNode("./x:Id/text()", nsManager) == null)
                                {
                                    mailPiece.Id = Guid.Empty;
                                }
                                else
                                {
                                    mailPiece.Id = Guid.Parse(loan.SelectSingleNode("./x:Id/text()", nsManager).Value);
                                }

                                if (loan.SelectSingleNode("./x:LoanNbr/text()", nsManager) == null)
                                {
                                    mailPiece.BusinessFriendlyIdName = "LoanNbr";
                                    mailPiece.BusinessFriendlyIdValue = string.Empty;
                                }
                                else
                                {
                                    mailPiece.BusinessFriendlyIdName = "LoanNbr";
                                    mailPiece.BusinessFriendlyIdValue = loan.SelectSingleNode("./x:LoanNbr/text()", nsManager).Value;
                                }
                                mailPiece.ErrorCode = OutComeCodes.Invalid_XML_file.ToString();
                                mailPiece.ErrorDesc = err.Message;
                                mailPiece.BatchClustId = batch.BatchClustId;
                            }
                            else
                            {
                                mailPiece = new AMMailPiece();
                                mailPiece.ErrorCode = OutComeCodes.Invalid_XML_file.ToString();
                                mailPiece.ErrorDesc = err.Message;
                                mailPiece.BatchClustId = batch.BatchClustId;
                                if (loan.SelectSingleNode("./x:LoanNbr/text()", nsManager) == null)
                                {
                                    mailPiece.BusinessFriendlyIdName = "LoanNbr";
                                    mailPiece.BusinessFriendlyIdValue = string.Empty;
                                }
                                else
                                {
                                    mailPiece.BusinessFriendlyIdValue = loan.SelectSingleNode("./x:LoanNbr/text()", nsManager).Value;
                                }

                                if (loan.SelectSingleNode("./x:Id/text()", nsManager) == null)
                                {
                                    mailPiece.Id = Guid.Empty;
                                }
                                else
                                {
                                    mailPiece.Id = Guid.Parse(loan.SelectSingleNode("./x:Id/text()", nsManager).Value);
                                }

                                if (loan.SelectSingleNode("./x:LoanNbr/text()", nsManager) == null)
                                {
                                    mailPiece.BusinessFriendlyIdName = "LoanNbr";
                                    mailPiece.BusinessFriendlyIdValue = string.Empty;
                                }
                                else
                                {
                                    mailPiece.BusinessFriendlyIdName = "LoanNbr";
                                    mailPiece.BusinessFriendlyIdValue = loan.SelectSingleNode("./x:LoanNbr/text()", nsManager).Value;
                                }
                            }

                            #endregion
                        }
                        catch (Exception err)
                        {
                            #region Save error details

                            log.Error(err);
                            if (mailPiece != null)
                            {
                                if (loan.SelectSingleNode("./x:LoanNbr/text()", nsManager) == null)
                                {
                                    mailPiece.BusinessFriendlyIdName = "LoanNbr";
                                    mailPiece.BusinessFriendlyIdValue = string.Empty;
                                }
                                else
                                {
                                    mailPiece.BusinessFriendlyIdValue = loan.SelectSingleNode("./x:LoanNbr/text()", nsManager).Value;
                                }

                                if (loan.SelectSingleNode("./x:Id/text()", nsManager) == null)
                                {
                                    mailPiece.Id = Guid.Empty;
                                }
                                else
                                {
                                    mailPiece.Id = Guid.Parse(loan.SelectSingleNode("./x:Id/text()", nsManager).Value);
                                }

                                if (loan.SelectSingleNode("./x:LoanNbr/text()", nsManager) == null)
                                {
                                    mailPiece.BusinessFriendlyIdName = "LoanNbr";
                                    mailPiece.BusinessFriendlyIdValue = string.Empty;
                                }
                                else
                                {
                                    mailPiece.BusinessFriendlyIdName = "LoanNbr";
                                    mailPiece.BusinessFriendlyIdValue = loan.SelectSingleNode("./x:LoanNbr/text()", nsManager).Value;
                                }
                                mailPiece.ErrorCode = OutComeCodes.UNKNOWN_error.ToString();
                                mailPiece.ErrorDesc = err.Message;
                                mailPiece.BatchClustId = batch.BatchClustId;
                            }
                            else
                            {
                                mailPiece = new AMMailPiece();
                                mailPiece.ErrorCode = OutComeCodes.UNKNOWN_error.ToString();
                                mailPiece.ErrorDesc = err.Message;
                                mailPiece.BatchClustId = batch.BatchClustId;
                                if (loan.SelectSingleNode("./x:LoanNbr/text()", nsManager) == null)
                                {
                                    mailPiece.BusinessFriendlyIdName = "LoanNbr";
                                    mailPiece.BusinessFriendlyIdValue = string.Empty;
                                }
                                else
                                {
                                    mailPiece.BusinessFriendlyIdValue = loan.SelectSingleNode("./x:LoanNbr/text()", nsManager).Value;
                                }

                                if (loan.SelectSingleNode("./x:Id/text()", nsManager) == null)
                                {
                                    mailPiece.Id = Guid.Empty;
                                }
                                else
                                {
                                    mailPiece.Id = Guid.Parse(loan.SelectSingleNode("./x:Id/text()", nsManager).Value);
                                }

                                if (loan.SelectSingleNode("./x:LoanNbr/text()", nsManager) == null)
                                {
                                    mailPiece.BusinessFriendlyIdName = "LoanNbr";
                                    mailPiece.BusinessFriendlyIdValue = string.Empty;
                                }
                                else
                                {
                                    mailPiece.BusinessFriendlyIdName = "LoanNbr";
                                    mailPiece.BusinessFriendlyIdValue = loan.SelectSingleNode("./x:LoanNbr/text()", nsManager).Value;
                                }
                            }

                            #endregion
                        }
                        finally
                        {
                            // if the validation works or fails save some information in loan table.
                            if (parsingResult.Code != OutComeCodes.No_error)
                            {
                                mailPiece = new AMMailPiece();
                                mailPiece.ErrorCode = OutComeCodes.UNKNOWN_error.ToString();
                                mailPiece.ErrorDesc = parsingResult.ErrorMessage;
                                mailPiece.BatchClustId = batch.BatchClustId;
                                if (loan.SelectSingleNode("./x:LoanNbr/text()", nsManager) == null)
                                {
                                    mailPiece.BusinessFriendlyIdName = "LoanNbr";
                                    mailPiece.BusinessFriendlyIdValue = string.Empty;
                                }
                                else
                                {
                                    mailPiece.BusinessFriendlyIdValue = loan.SelectSingleNode("./x:LoanNbr/text()", nsManager).Value;
                                }

                                if (loan.SelectSingleNode("./x:Id/text()", nsManager) == null)
                                {
                                    mailPiece.Id = Guid.Empty;
                                }
                                else
                                {
                                    mailPiece.Id = Guid.Parse(loan.SelectSingleNode("./x:Id/text()", nsManager).Value);
                                }

                                if (loan.SelectSingleNode("./x:LoanNbr/text()", nsManager) == null)
                                {
                                    mailPiece.BusinessFriendlyIdName = "LoanNbr";
                                    mailPiece.BusinessFriendlyIdValue = string.Empty;
                                }
                                else
                                {
                                    mailPiece.BusinessFriendlyIdName = "LoanNbr";
                                    mailPiece.BusinessFriendlyIdValue = loan.SelectSingleNode("./x:LoanNbr/text()", nsManager).Value;
                                }
                            }

                            mailPieceSvc.InsertBatchMailPieceDetails(mailPiece);
                        }
                    }
                }
                #endregion
                List<AMMailPiece> mailPieces = mailPieceSvc.GetMailPiecesByBatchClustId(batch.BatchClustId);
                //// TotalLoans is valid loans of this batch. Update the count and add barcode for each loan.
                //if (mailPieces.Count > 0)
                //{
                //    batch.TotalLoans = batch.Packages.Where(loan => loan.OutcomeCode == OutComeCodes.No_Error).Count();
                //    batch.Packages.ForEach(l => l.BarcodeValue = string.Format("AMC{0}", l.LoanID.ToString("D7")));
                //    db.SaveChanges();
                //}

                //if (batch.Packages.Count > 0 && batch.TotalLoans == 0)
                //{
                //    // Count of NoError and UnknownError both is 0. So, all are invalid loans in the package.
                //    int downloadedLoans = batch.Packages.Where(loan => loan.OutcomeCode == OutComeCodes.Unknown_Error).Count();
                //    if (downloadedLoans == 0)
                //    {
                //        batch.IsPrinted = true;
                //        batch.IsCompleted = true;
                //        batch.IsShipped = true;
                //        db.SaveChanges();
                //    }
                //}

                //// Send download summary email if not sent.
                //if (batch.IsDSEmailSent == false)
                //{
                //    if (MailUtility.SendDownloadNotificationEmail(batch) && UploadInitialErrorReportOnFTP(batch))
                //    {
                //        batch.IsDSEmailSent = true;
                //        db.SaveChanges();
                //    }
                //}

                //#endregion

                //// Now as the XML file parsed and loans saved in DB rename that XML so it will not be used in resume batch.
                //string parsedXMLFilePath = Path.Combine(batchLocalDir, string.Format("{0}.parsed", Path.GetFileNameWithoutExtension(xmlFilePath)));

                //if (File.Exists(parsedXMLFilePath) == true)
                //    File.Delete(parsedXMLFilePath);
                //File.Move(xmlFilePath, parsedXMLFilePath);

                //#endregion
            }
            else
            {
                //string parsedXMLFilePath = Directory.GetFiles(batchLocalDir, "*.parsed").FirstOrDefault();
                //if (batch.IsDSEmailSent == false)
                //{
                //    if (MailUtility.SendDownloadNotificationEmail(batch) && UploadInitialErrorReportOnFTP(batch))
                //    {
                //        batch.IsDSEmailSent = true;
                //        db.SaveChanges();
                //    }
                //}

                //if (string.IsNullOrEmpty(parsedXMLFilePath))
                //{
                //    // If no XML file inside zip then log that error.
                //    batch.ErrorDetail += string.Format("{0}|XML file not found.", OutComeCodes.Invalid_XML_File.ToString());
                //    batch.IsCompleted = true;
                //    db.SaveChanges();
                //}
            }
        }
    }
}
