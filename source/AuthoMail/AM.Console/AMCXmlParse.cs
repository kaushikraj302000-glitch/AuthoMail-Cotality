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
            PackageRequests batch = null;
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
                            BatchFilePath = downloadedZipFilePath
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

                        batchSvc.InsertBatchDetails(newBatch);

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

                        batchSvc.InsertBatchDetails(newBatch);

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
                    ValidationStatus=OutComeCodes.Invalid_Zip_File.ToString(),
                    ValidationErrors = string.Format("{0}|Trigger file found but zip file is missing on FTP.", OutComeCodes.Invalid_Zip_File.ToString()),
                    DownloadedOn=Common.GetCurrentTime()
                };

                batchSvc.InsertBatchDetails(newBatch);
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

        //private void ParseLoans(string xmlFilePath, string localDownloadPath)
        //{
        //    XmlNamespaceManager nsManager = null;
        //    XmlNodeList loans = null;

        //    if (!string.IsNullOrEmpty(xmlFilePath))
        //    {
        //        #region Parse XML file
        //        Batch batch = null;
        //        XmlDocument batchXML = new XmlDocument();
        //        int batchClustId = 0;
        //        // Try to parse the XML we got from the zip file. If XML is invalid then log error.
        //        try
        //        {
        //            batchXML.LoadXml(File.ReadAllText(xmlFilePath));
        //            nsManager = new XmlNamespaceManager(batchXML.NameTable);
        //            nsManager.AddNamespace("x", "http://schemas.corelogic.com/2015/amc/v1/packages");
        //            loans = batchXML.DocumentElement.ChildNodes;
        //        }
        //        catch (XmlException err)
        //        {
        //            #region Invalid XML file of the batch so just save error details
        //            log.Error(err);
        //            batch.BatchName = Path.GetFileNameWithoutExtension(xmlFilePath);
        //            batch.Id = Guid.NewGuid();
        //            batch.ReceivedOn = Common.GetCurrentTime();
        //            batch.DownloadedOn = Common.GetCurrentTime();
        //            batch.ValidationStatus = OutComeCodes.Invalid_XML_file.ToString();
        //            batch.ValidationErrors = err.Message;
        //            batchClustId = batchSvc.InsertBatchDetails(batch);
        //            commonSvc.UpdateBatchStatus(batchClustId, BatchValidationStatus.Invalid);

        //            //TODO: Mail For Invalid XML

        //            commonSvc.UpdateBatchPipeLineStatus(batchClustId, BatchPipelineStatus.ReportedBack, Common.GetCurrentTime());
        //            #endregion
        //        }

        //        // If the XML is valid and found any child nodes then save those loans in DB.
        //        #region Process loans of XML

        //        if (loans != null)
        //        {
        //            foreach (XmlNode loan in loans)
        //            {
        //                AMMailPiece mailPiece = null;
        //                PackageInfo package = null;
        //                ValidationResultAMC parsingResult = null;

        //                try
        //                {
        //                    // Validate each child node of XML and store in DB along with error detail if any.
        //                    parsingResult = Common.ValidateAMCXMLNode(loan);

        //                    #region De-serialize XML nodes.

        //                    if (parsingResult.Code == OutComeCodes.No_error)
        //                    {
        //                        package = XmlUtil.Deserialize<PackageInfo>(loan.OuterXml);

        //                        // Check if the PDF file is available or not for loan.
        //                        if (File.Exists(Path.Combine(batchLocalDir, package.File)))
        //                        {
        //                            package.OutcomeCode = OutComeCodes.No_error;
        //                        }
        //                        else
        //                        {
        //                            package.OutcomeCode = OutComeCodes.Missing_PDF_file;
        //                            package.ErrorDetail = "PDF file missing.";
        //                        }
        //                    }

        //                    #endregion

        //                }
        //                catch (XmlException err)
        //                {
        //                    #region Save error details

        //                    Common.LogError(err.ToString(), true);
        //                    if (package != null)
        //                    {
        //                        package.ErrorDetail = err.Message;
        //                        package.OutcomeCode = OutComeCodes.Invalid_XML_File;
        //                    }
        //                    else
        //                    {
        //                        string loanNbr = loan.SelectSingleNode("./x:LoanNbr/text()", nsManager) == null ? string.Empty :
        //                            loan.SelectSingleNode("./x:LoanNbr/text()", nsManager).Value;
        //                        package = new PackageInfo()
        //                        {
        //                            LoanNbr = loanNbr,
        //                            OutcomeCode = OutComeCodes.Invalid_XML_File,
        //                            ErrorDetail = err.Message
        //                        };
        //                    }

        //                    #endregion
        //                }
        //                catch (Exception err)
        //                {
        //                    #region Save error details

        //                    Common.LogError(err.ToString(), true);
        //                    if (package != null)
        //                    {
        //                        package.ErrorDetail = err.Message;
        //                        package.OutcomeCode = OutComeCodes.Unknown_Error;
        //                    }
        //                    else
        //                    {
        //                        string loanNbr = loan.SelectSingleNode("./x:LoanNbr/text()", nsManager) == null ? string.Empty :
        //                            loan.SelectSingleNode("./x:LoanNbr/text()", nsManager).Value;
        //                        package = new PackageInfo()
        //                        {
        //                            LoanNbr = loanNbr,
        //                            OutcomeCode = OutComeCodes.Invalid_XML_File,
        //                            ErrorDetail = err.Message
        //                        };
        //                    }

        //                    #endregion
        //                }
        //                finally
        //                {
        //                    // if the validation works or fails save some information in loan table.
        //                    if (parsingResult.Code != OutComeCodes.No_Error)
        //                    {
        //                        package = new PackageInfo()
        //                        {
        //                            LoanNbr = loan.SelectSingleNode("./x:LoanNbr/text()", nsManager) == null ? string.Empty : loan.SelectSingleNode("./x:LoanNbr/text()", nsManager).Value,
        //                            Id = loan.SelectSingleNode("./x:Id/text()", nsManager) == null ? Guid.Empty : Guid.Parse(loan.SelectSingleNode("./x:Id/text()", nsManager).Value)
        //                        ,
        //                            RequestId = loan.SelectSingleNode("./x:RequestId/text()", nsManager) == null ? string.Empty : loan.SelectSingleNode("./x:RequestId/text()", nsManager).Value
        //                        ,
        //                            ErrorDetail = parsingResult.ErrorMessage
        //                        ,
        //                            OutcomeCode = parsingResult.Code
        //                        };
        //                    }

        //                    package.ReceivedOn = DateTime.UtcNow;
        //                    batch.Packages.Add(package);
        //                }
        //            }

        //            db.SaveChanges();
        //        }

        //        // TotalLoans is valid loans of this batch. Update the count and add barcode for each loan.
        //        if (batch.Packages.Count > 0)
        //        {
        //            batch.TotalLoans = batch.Packages.Where(loan => loan.OutcomeCode == OutComeCodes.No_Error).Count();
        //            batch.Packages.ForEach(l => l.BarcodeValue = string.Format("AMC{0}", l.LoanID.ToString("D7")));
        //            db.SaveChanges();
        //        }

        //        if (batch.Packages.Count > 0 && batch.TotalLoans == 0)
        //        {
        //            // Count of NoError and UnknownError both is 0. So, all are invalid loans in the package.
        //            int downloadedLoans = batch.Packages.Where(loan => loan.OutcomeCode == OutComeCodes.Unknown_Error).Count();
        //            if (downloadedLoans == 0)
        //            {
        //                batch.IsPrinted = true;
        //                batch.IsCompleted = true;
        //                batch.IsShipped = true;
        //                db.SaveChanges();
        //            }
        //        }

        //        // Send download summary email if not sent.
        //        if (batch.IsDSEmailSent == false)
        //        {
        //            if (MailUtility.SendDownloadNotificationEmail(batch) && UploadInitialErrorReportOnFTP(batch))
        //            {
        //                batch.IsDSEmailSent = true;
        //                db.SaveChanges();
        //            }
        //        }

        //        #endregion

        //        // Now as the XML file parsed and loans saved in DB rename that XML so it will not be used in resume batch.
        //        string parsedXMLFilePath = Path.Combine(batchLocalDir, string.Format("{0}.parsed", Path.GetFileNameWithoutExtension(xmlFilePath)));

        //        if (File.Exists(parsedXMLFilePath) == true)
        //            File.Delete(parsedXMLFilePath);
        //        File.Move(xmlFilePath, parsedXMLFilePath);

        //        #endregion
        //    }
        //    else
        //    {
        //        string parsedXMLFilePath = Directory.GetFiles(batchLocalDir, "*.parsed").FirstOrDefault();
        //        if (batch.IsDSEmailSent == false)
        //        {
        //            if (MailUtility.SendDownloadNotificationEmail(batch) && UploadInitialErrorReportOnFTP(batch))
        //            {
        //                batch.IsDSEmailSent = true;
        //                db.SaveChanges();
        //            }
        //        }

        //        if (string.IsNullOrEmpty(parsedXMLFilePath))
        //        {
        //            // If no XML file inside zip then log that error.
        //            batch.ErrorDetail += string.Format("{0}|XML file not found.", OutComeCodes.Invalid_XML_File.ToString());
        //            batch.IsCompleted = true;
        //            db.SaveChanges();
        //        }
        //    }
        //}
    }
}
