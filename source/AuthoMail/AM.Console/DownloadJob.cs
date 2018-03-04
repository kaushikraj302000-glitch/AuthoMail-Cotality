using AM.Entities;
using AM.Services;
using Ionic.Zip;
using log4net;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace AM.Console
{
    public class DownloadJob
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DownloadJob));

        IClientService clientSvc;
        IBatchService batchSvc;
        IMailPieceService mailPieceSvc;
        ICommonService commonSvc;
        public DownloadJob()
        {
            clientSvc = new ClientService();
            batchSvc = new BatchService();
            mailPieceSvc = new MailPieceService();
            commonSvc = new CommonService();
        }

        public void DownloadNewBatch()
        {
            log.Debug("Download Batch Process Started");

            Client client = clientSvc.GetClientDetails(null).Single(x => x.ClientName == "USAA");

            ClientSFTChannel channelMap = clientSvc.GetClientSFTChannelMapDetails(client.Id).SingleOrDefault();
            string triggerFilePath = string.Empty;
            if (channelMap != null)
            {
                SFTChannel channelDetails = clientSvc.GetSFTChannelDetails(channelMap.SFTChannelId);
                if (channelDetails != null)
                {
                    SFTUtil sftSite = null;
                    try
                    {
                        sftSite = new SFTUtil(Common.GetProtocolType(channelDetails.Protocol), channelDetails.Host, channelDetails.UserName, channelDetails.Password, 300000);
                        sftSite.Connect();
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                    }

                    MailBatchDTO mailBatch = null;
                    if (sftSite.IsDownloadDirectoryAvailable(channelDetails.ADERequestDir))
                    {
                        triggerFilePath = GetTriggerFilePath(sftSite, channelDetails);
                        if (!string.IsNullOrEmpty(triggerFilePath))
                        {
                            // Download and Insert the batch in database first always.
                            string zipFileFTPPath = Regex.Replace(Path.ChangeExtension(triggerFilePath, ".zip"), @"(w*)_Complete", string.Empty);
                            mailBatch = GetNewBatchFromFTP(sftSite, zipFileFTPPath, triggerFilePath, client, channelDetails);
                        }
                        if (sftSite.IsConnected)
                            sftSite.Disconnect();
                    }
                }
            }
        }

        private MailBatchDTO GetNewBatchFromFTP(SFTUtil sftSite, string zipFileFTPPath, string triggerFilePath, Client client, SFTChannel channelDetails)
        {
            MailBatchDTO newBatch = null;
            string downloadedZipFilePath = string.Empty;

            if (sftSite.IsConnected == false)
                sftSite.Connect();

            // If batch zip file exists on FTP then download and process it.
            if (sftSite.IsFileAvailable(zipFileFTPPath))
            {
                downloadedZipFilePath = DownloadBatchZipFile(sftSite, zipFileFTPPath, client);

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
                        newBatch = new MailBatchDTO()
                        {
                            BatchName = Path.GetFileNameWithoutExtension(downloadedZipFilePath),
                            ReceivedOn = DateTime.UtcNow,
                            UniqueId = Guid.NewGuid(),
                            Priority = Priority.High.ToString(),
                            ClientId = client.Id
                        };

                        #endregion

                        MoveTriggerFileToBackupDir(sftSite, triggerFilePath, channelDetails);

                        log.Error(string.Format("Trigger file '{0}' and corresponding zip file deleted after downloading its ZIP file.", Path.GetFileName(triggerFilePath)));

                        int availableXMLFiles = Directory.GetFiles(Path.GetDirectoryName(downloadedZipFilePath), "*.xml").Count();
                        if (availableXMLFiles != 1)
                        {
                            newBatch.DownloadedOn = Common.GetCurrentTime();
                            newBatch.ValidationStatus = OutComeCodes.Invalid_Zip_File.ToString();
                            newBatch.ValidationErrors = string.Format("{0}|No XML file is available.", OutComeCodes.Invalid_Zip_File.ToString());
                        }

                        newBatch.Id = batchSvc.InsertBatchDetails(newBatch);
                        commonSvc.UpdateBatchPipeLineStatus(newBatch.Id, BatchPipelineStatus.Downloaded, Common.GetCurrentTime());
                        return newBatch;
                    }
                    catch (ZipException err)
                    {
                        // The zip file of batch is corrupt so log the error in batch table.
                        log.Error(err);
                        newBatch = new MailBatchDTO()
                        {
                            UniqueId = Guid.NewGuid(),
                            BatchName = Path.GetFileNameWithoutExtension(triggerFilePath).TrimEnd("_Complete".ToCharArray()).ToString(),
                            ReceivedOn = Common.GetCurrentTime(),
                            DownloadedOn = Common.GetCurrentTime(),
                            ValidationStatus = OutComeCodes.Invalid_Zip_File.ToString(),
                            ValidationErrors = string.Format("{0}|{1}", OutComeCodes.Invalid_Zip_File.ToString(), err.Message),
                        };

                        newBatch.Id = batchSvc.InsertBatchDetails(newBatch);

                        MoveTriggerFileToBackupDir(sftSite, triggerFilePath, channelDetails);
                    }
                }
            }
            // If the zip file is not available in the FTP folder then log that error.
            else
            {
                newBatch = new MailBatchDTO()
                {
                    BatchName = Path.GetFileNameWithoutExtension(triggerFilePath).TrimEnd("_Complete".ToCharArray()).ToString(),
                    ReceivedOn = DateTime.UtcNow,
                    ValidationStatus = OutComeCodes.Invalid_Zip_File.ToString(),
                    ValidationErrors = string.Format("{0}|Trigger file found but zip file is missing on FTP.", OutComeCodes.Invalid_Zip_File.ToString()),
                    DownloadedOn = Common.GetCurrentTime()
                };

                newBatch.Id = batchSvc.InsertBatchDetails(newBatch);
                log.Error(string.Format("Trigger file {0} found but zip file missing on FTP.", Path.GetFileName(triggerFilePath)));

                MoveTriggerFileToBackupDir(sftSite, triggerFilePath, channelDetails);
            }

            return null;
        }

        private static string GetTriggerFilePath(SFTUtil sftp, SFTChannel channelDetails)
        {
            if (sftp.IsConnected == false)
                sftp.Connect();

            string ftpTriggerFilePath = string.Empty;
            DateTime pingTime = Common.GetCurrentTime();
            List<string> avialableTriggerFiles = sftp.ListDirectory(channelDetails.ADERequestDir).Where(f => Path.GetExtension(f).Equals(".go", StringComparison.CurrentCultureIgnoreCase) && Path.GetFileNameWithoutExtension(f).EndsWith("_Complete")).ToList();

            if (avialableTriggerFiles != null && avialableTriggerFiles.Count > 0)
            {
                ftpTriggerFilePath = avialableTriggerFiles.First();
                log.Debug(string.Format("Trigger file {0} found.", Path.GetFileName(ftpTriggerFilePath)));
            }
            else
            {
                log.Error("Trigger file not found.");
            }

            return ftpTriggerFilePath;
        }

        private void MoveTriggerFileToBackupDir(SFTUtil sftSite, string triggerFilePath, SFTChannel channelDetails)
        {
            if (!sftSite.IsConnected)
                sftSite.Connect();

            string ftpBackupDir = channelDetails.ADERequestArchiveDir;

            //if (!sftp.Exists(ftpBackupDir))
            //    sftp.CreateDirectory(ftpBackupDir);

            string ftpBackupTriggerFilePath = Path.Combine(ftpBackupDir, Path.GetFileName(triggerFilePath));
            if (sftSite.IsFileAvailable(ftpBackupTriggerFilePath))
                sftSite.DeleteFile(ftpBackupTriggerFilePath);

            if (channelDetails.PostDownloadAction == PostDownloadAction.Archive)
            {
                sftSite.MoveFile(triggerFilePath, ftpBackupTriggerFilePath);
            }
            else
                sftSite.DeleteFile(triggerFilePath);
        }

        private string DownloadBatchZipFile(SFTUtil sftSite, string batchFileFTPPath, Client client)
        {
            if (sftSite.IsConnected == false)
                sftSite.Connect();

            string downloadedZipFilePath = string.Empty;
            if(sftSite.IsFileAvailable(batchFileFTPPath))
            {
                string batchName = Path.GetFileNameWithoutExtension(batchFileFTPPath);
                string batchLocalDir = Path.Combine(Common.AuthoMailRootFolder, client.AppCode, batchName);
                if (!Directory.Exists(batchLocalDir))
                    Directory.CreateDirectory(batchLocalDir);

                downloadedZipFilePath = Path.Combine(batchLocalDir, batchFileFTPPath);
                if (File.Exists(downloadedZipFilePath))
                    File.Delete(downloadedZipFilePath);
                File.WriteAllText(downloadedZipFilePath, "");
                Stream downloadedXMLStrm = File.OpenWrite(downloadedZipFilePath);
                sftSite.DownloadFile(batchFileFTPPath, downloadedZipFilePath);
                downloadedXMLStrm.Close();

                // After downloading the zip file delete it from FTP.
                //sftp.DeleteFile(batchFileFTPPath);
            }            

            return downloadedZipFilePath;
        }

        private void ProcessXML(SFTUtil sftSite, string xmlFilePath, MailBatchDTO mailBatch, Client client, string localDownloadPath, SFTChannel channelDetails)
        {
            string batchName = Path.GetFileNameWithoutExtension(xmlFilePath);
            //ValidationResult validationResult = XmlUtil.ValidateXml(Common.schemas, xmlFilePath);
            //if (!validationResult.HasErrors)
            //{

            MailBatch processXML = XmlUtil.Deserialize<MailBatch>(File.ReadAllText(xmlFilePath));
            if (processXML != null)
            {
                mailBatch.UniqueId = processXML.Id;
                mailBatch.Priority = processXML.Priority.GetStringValue();
                mailBatch.DocumentOrPackageType = processXML.DocumentOrPackageType;
                //mailBatch.BusinessFriendlyIdName = processXML.BusinessFriendlyIdName != null ? processXML.BusinessFriendlyIdName : "";
                //mailBatch.BusinessFriendlyIdValue = processXML.BusinessFriendlyIdValue != null ? processXML.BusinessFriendlyIdValue : "";
                mailBatch.BatchName = batchName;
                if (processXML.ReturnInfo != null)
                {
                    mailBatch.IsReturnInfoSpecified = true;

                    if (processXML.ReturnInfo.Address != null)
                    {
                        mailBatch.RetRcptAddressAptOrSuiteOrOther = processXML.ReturnInfo.Address.AptOrSuiteOrOther;
                        mailBatch.RetRcptAddressCity = processXML.ReturnInfo.Address.City;
                        mailBatch.RetRcptAddressState = processXML.ReturnInfo.Address.State;
                        mailBatch.RetRcptAddressStreet = processXML.ReturnInfo.Address.Street;
                        mailBatch.RetRcptAddressZIP = processXML.ReturnInfo.Address.Zip;
                        mailBatch.RetRcptAddrOptionalLine1AboveStreet = processXML.ReturnInfo.Address.OptionalLine1AboveStreet;
                        mailBatch.RetRcptAddrOptionalLine2AboveStreet = processXML.ReturnInfo.Address.OptionalLine2AboveStreet;
                        mailBatch.RetRcptAddrOptionalLine1BelowStreet = processXML.ReturnInfo.Address.OptionalLine1BelowStreet;
                        mailBatch.RetRcptAddrOptionalLine2BelowStreet = processXML.ReturnInfo.Address.OptionalLine2BelowStreet;
                    }

                    if (processXML.ReturnInfo.Recipient != null)
                    {
                        mailBatch.RetRcptFirstName = processXML.ReturnInfo.Recipient.FirstName;
                        mailBatch.RetRcptLastName = processXML.ReturnInfo.Recipient.LastName;
                        mailBatch.RetRcptMiddleName = processXML.ReturnInfo.Recipient.MiddleName;
                        mailBatch.RetRcptOrganization = processXML.ReturnInfo.Recipient.Organization;
                        mailBatch.RetRcptPhone = processXML.ReturnInfo.Recipient.Phone;
                        mailBatch.RetRcptPrefix = processXML.ReturnInfo.Recipient.Prefix;
                        mailBatch.RetRcptSuffix = processXML.ReturnInfo.Recipient.Suffix;
                    }
                }

                mailBatch.Id = batchSvc.InsertBatchDetails(mailBatch);

                if (processXML.MailPieces != null && processXML.MailPieces.Count > 0)
                {
                    List<MailPieceDTO> amMailPieces = new List<MailPieceDTO>();
                    Dictionary<string, string> archiveFileList = new Dictionary<string, string>();
                    foreach (MailPiece mailPiece in processXML.MailPieces)
                    {
                        MailPieceDTO amMailPiece = new MailPieceDTO();
                        amMailPiece.UniqueId = mailPiece.Id;
                        amMailPiece.BatchId = mailBatch.Id;
                        amMailPiece.BusinessFriendlyIdName = mailPiece.BusinessFriendlyIdName;
                        amMailPiece.BusinessFriendlyIdValue = mailPiece.BusinessFriendlyIdValue;
                        amMailPiece.DocumentOrPackageType = mailPiece.DocumentOrPackageType;

                        if (mailPiece.ReturnInfo != null)
                        {
                            amMailPiece.IsReturnInfoSpecified = true;

                            if (mailPiece.ReturnInfo.Address != null)
                            {
                                amMailPiece.RetRcptAddressAptOrSuiteOrOther = mailPiece.ReturnInfo.Address.AptOrSuiteOrOther;
                                amMailPiece.RetRcptAddressCity = mailPiece.ReturnInfo.Address.City;
                                amMailPiece.RetRcptAddressState = mailPiece.ReturnInfo.Address.State;
                                amMailPiece.RetRcptAddressStreet = mailPiece.ReturnInfo.Address.Street;
                                amMailPiece.RetRcptAddressZIP = mailPiece.ReturnInfo.Address.Zip;
                                amMailPiece.RetRcptAddrOptionalLine1AboveStreet = mailPiece.ReturnInfo.Address.OptionalLine1AboveStreet;
                                amMailPiece.RetRcptAddrOptionalLine2AboveStreet = mailPiece.ReturnInfo.Address.OptionalLine2AboveStreet;
                                amMailPiece.RetRcptAddrOptionalLine1BelowStreet = mailPiece.ReturnInfo.Address.OptionalLine1BelowStreet;
                                amMailPiece.RetRcptAddrOptionalLine2BelowStreet = mailPiece.ReturnInfo.Address.OptionalLine2BelowStreet;
                            }

                            if (mailPiece.ReturnInfo.Recipient != null)
                            {
                                amMailPiece.RetRcptFirstName = mailPiece.ReturnInfo.Recipient.FirstName;
                                amMailPiece.RetRcptLastName = mailPiece.ReturnInfo.Recipient.LastName;
                                amMailPiece.RetRcptMiddleName = mailPiece.ReturnInfo.Recipient.MiddleName;
                                amMailPiece.RetRcptOrganization = mailPiece.ReturnInfo.Recipient.Organization;
                                amMailPiece.RetRcptPhone = mailPiece.ReturnInfo.Recipient.Phone;
                                amMailPiece.RetRcptPrefix = mailPiece.ReturnInfo.Recipient.Prefix;
                                amMailPiece.RetRcptSuffix = mailPiece.ReturnInfo.Recipient.Suffix;
                            }
                        }

                        string validationOutCome = ValidationClientWise(client.AppCode, mailPiece);

                        if (validationOutCome == OutComeCodes.No_Error)
                        {
                            amMailPiece.IsRcptInfoSpecified = true;
                            amMailPiece.RcptFirstName = mailPiece.RecipientInfo.Recipient.FirstName;
                            amMailPiece.RcptLastName = mailPiece.RecipientInfo.Recipient.LastName;
                            amMailPiece.RcptMiddleName = mailPiece.RecipientInfo.Recipient.MiddleName;
                            amMailPiece.RcptOrganization = mailPiece.RecipientInfo.Recipient.Organization;
                            amMailPiece.RcptPhone = mailPiece.RecipientInfo.Recipient.Phone;
                            amMailPiece.RcptPrefix = mailPiece.RecipientInfo.Recipient.Prefix;
                            amMailPiece.RcptSuffix = mailPiece.RecipientInfo.Recipient.Suffix;

                            if (mailPiece.RecipientInfo.Corecipient != null)
                            {
                                amMailPiece.CoRcptFirstName = mailPiece.RecipientInfo.Corecipient.FirstName;
                                amMailPiece.CoRcptLastName = mailPiece.RecipientInfo.Corecipient.LastName;
                                amMailPiece.CoRcptMiddleName = mailPiece.RecipientInfo.Corecipient.MiddleName;
                                amMailPiece.CoRcptOrganization = mailPiece.RecipientInfo.Corecipient.Organization;
                                amMailPiece.CoRcptPhone = mailPiece.RecipientInfo.Corecipient.Phone;
                                amMailPiece.CoRcptPrefix = mailPiece.RecipientInfo.Corecipient.Prefix;
                                amMailPiece.CoRcptSuffix = mailPiece.RecipientInfo.Corecipient.Suffix;
                            }

                            if (mailPiece.RecipientInfo.Address != null)
                            {
                                amMailPiece.RcptAddressAptOrSuiteOrOther = mailPiece.RecipientInfo.Address.AptOrSuiteOrOther;
                                amMailPiece.RcptAddressCity = mailPiece.RecipientInfo.Address.City;
                                amMailPiece.RcptAddressState = mailPiece.RecipientInfo.Address.State;
                                amMailPiece.RcptAddressStreet = mailPiece.RecipientInfo.Address.Street;
                                amMailPiece.RcptAddressZIP = mailPiece.RecipientInfo.Address.Zip;
                                amMailPiece.RcptAddrOptionalLine1AboveStreet = mailPiece.RecipientInfo.Address.OptionalLine1AboveStreet;
                                amMailPiece.RcptAddrOptionalLine2AboveStreet = mailPiece.RecipientInfo.Address.OptionalLine2AboveStreet;
                                amMailPiece.RcptAddrOptionalLine1BelowStreet = mailPiece.RecipientInfo.Address.OptionalLine1BelowStreet;
                                amMailPiece.RcptAddrOptionalLine2BelowStreet = mailPiece.RecipientInfo.Address.OptionalLine2BelowStreet;
                            }

                            List<string> availablePDF = sftSite.ListDirectory(channelDetails.ADERequestDir).Where(x => Path.GetExtension(x).Equals(".pdf", StringComparison.CurrentCultureIgnoreCase)).ToList();
                            string pdfLocalFilePath = string.Empty;
                            string pdfServerFilePath = string.Empty;
                            string pdfServerArchiveFilePath = string.Empty;
                            if (availablePDF.Count > 0)
                            {
                                if (mailPiece.DocumentOrPackage.Name != null || string.IsNullOrEmpty(mailPiece.DocumentOrPackage.Name))
                                {
                                    amMailPiece.FileName = mailPiece.DocumentOrPackage.Name;

                                    if (availablePDF.Contains(mailPiece.DocumentOrPackage.Name))
                                    {
                                        pdfLocalFilePath = Path.Combine(localDownloadPath, mailPiece.DocumentOrPackage.Name);
                                        pdfServerFilePath = Path.Combine(channelDetails.ADERequestDir, mailPiece.DocumentOrPackage.Name);
                                        pdfServerArchiveFilePath = Path.Combine(channelDetails.ADERequestArchiveDir, mailPiece.DocumentOrPackage.Name);
                                        if (!archiveFileList.ContainsKey(pdfServerFilePath))
                                            archiveFileList.Add(pdfServerFilePath, pdfServerArchiveFilePath);

                                        sftSite.DownloadFile(pdfLocalFilePath, pdfServerFilePath);
                                        amMailPiece.ErrorCode = OutComeCodes.No_Error.ToString();
                                    }
                                    else
                                    {
                                        amMailPiece.ErrorCode = OutComeCodes.Missing_PDF.ToString();
                                        amMailPiece.ErrorDesc = "Pdf file is missing on server.";
                                    }
                                }
                                else
                                {
                                    amMailPiece.FilePattern = mailPiece.DocumentOrPackage.Pattern;

                                    if (availablePDF.Contains(mailPiece.DocumentOrPackage.Pattern))
                                    {
                                        string pdfFileName = availablePDF.Single(x => x.Contains(mailPiece.DocumentOrPackage.Pattern));

                                        pdfLocalFilePath = Path.Combine(localDownloadPath, pdfFileName);
                                        pdfServerFilePath = Path.Combine(channelDetails.ADERequestDir, pdfFileName);
                                        pdfServerArchiveFilePath = Path.Combine(channelDetails.ADERequestArchiveDir, pdfFileName);

                                        sftSite.DownloadFile(pdfLocalFilePath, pdfServerFilePath);

                                        if (!archiveFileList.ContainsKey(pdfServerFilePath))
                                            archiveFileList.Add(pdfServerFilePath, pdfServerArchiveFilePath);

                                        amMailPiece.ErrorCode = OutComeCodes.No_Error.ToString();
                                    }
                                    else
                                    {
                                        amMailPiece.ErrorCode = OutComeCodes.Missing_PDF.ToString();
                                        amMailPiece.ErrorDesc = "Pdf file is missing on server.";
                                    }
                                }
                            }
                        }
                        else
                        {
                            amMailPiece.ErrorCode = validationOutCome.ToString();
                            amMailPiece.ErrorDesc = "";
                        }
                        amMailPieces.Add(amMailPiece);
                    }
                    //mailPieceSvc.InsertBatchMailPieceDetails(amMailPieces);

                    #region Archive Files After Download
                    foreach (var archiveFile in archiveFileList)
                    {
                        sftSite.MoveFile(archiveFile.Key, archiveFile.Value);
                    }
                    #endregion
                    commonSvc.UpdateBatchPipeLineStatus(mailBatch.Id, BatchPipelineStatus.Downloaded, Common.GetCurrentTime());

                }
            }
            else
            {
                //TODO:Mail if found null
            }
            //}
            //else
            //{
            //    //TODO:Mail if found any XML level error
            //}
        }

        private string ValidationClientWise(string clientCode, MailPiece mailPiece)
        {
            string validationResultClientWise = OutComeCodes.No_Error;
            switch (clientCode)
            {
                case "USAA": validationResultClientWise = mailPiece.RecipientInfo.Recipient == null ? OutComeCodes.Recipient_Name_Missing_Or_Invalid : OutComeCodes.No_Error;
                    break;
                default:
                    validationResultClientWise = OutComeCodes.No_Error;
                    break;
            }
            return validationResultClientWise;
        }
    }
}
