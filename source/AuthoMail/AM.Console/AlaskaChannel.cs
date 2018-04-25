using AM.Console.Properties;
using AM.Entities;
using AM.Lib;
using AM.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace AM.Console
{
    class AlaskaChannel : Channel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static XmlSchemaSet schemas;

        protected Client client;

        protected IAlaskaService alaskaSvc;

        static AlaskaChannel()
        {
            schemas = new XmlSchemaSet();
            schemas.Add(string.Empty, XmlReader.Create(new StringReader(Resources.RespaControlSchema)));
        }

        public AlaskaChannel(string channelCode)
            : base(channelCode)
        {
            alaskaSvc = GetService<AlaskaService>();
            client = clientSvc.GetClientByAppCode("ALASKA");
        }


        protected override void EnumerateBatches()
        {
            log.Debug("Enumerate Batches job queue for Alaska is started.");

            List<string> xmlFileNames = new List<string>();

            SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(SFTChannel.Protocol), SFTChannel.Host, SFTChannel.UserName, SFTChannel.Password, 300000);
            sftSite.Connect();

            xmlFileNames = GetXmlFileNames(sftSite);

            foreach (string xmlFile in xmlFileNames)
            {
                string xmlFilePath = Path.Combine(SFTChannel.ToADEDir, xmlFile + ".xml");

                //Download and Insert the batch in database first always.
                string batchName = xmlFile;

                //Here we consider Batch name as Document or Package type at batch level
                string shortBatchName = batchName.Replace("PSManifest", "");
                string documentOrPackageType = shortBatchName.Substring(0, shortBatchName.IndexOf("["));

                MailBatchDTO mailBatch = new MailBatchDTO();
                mailBatch.ClientId = client.Id;
                mailBatch.BatchName = batchName;
                mailBatch.SFTChannelId = SFTChannel.Id;
                mailBatch.ClientBatchId = batchName;
                mailBatch.UniqueId = Guid.NewGuid();
                mailBatch.Priority = Priority.Medium.ToString();
                mailBatch.PrintLocation = Config.Common_PrintLocation;
                mailBatch.DocumentOrPackageType = documentOrPackageType;

                //Insert New Batch Entry
                mailBatch.Id = mailBatchSvc.InsertBatchDetails(mailBatch);

                //Update Pipeline Status of Batch Received
                commonSvc.UpdateBatchPipeLineStatus(mailBatch.Id, BatchPipelineStatus.Received, sftSite.LastModifiedTimeOfFile(xmlFilePath));

                commonSvc.UpdateBatchStatus(mailBatch.Id, BatchStatus.InProgress.ToString());
            }

            if (sftSite.IsConnected)
                sftSite.Disconnect();
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

        protected override void DownloadBatches()
        {
            log.Debug("Alaska download job is started.");

            List<MailBatchDTO> downloadBatches = mailBatchSvc.GetBatchesByPipelineStatus(BatchPipelineStatus.Received, client.Id);
            if (downloadBatches.Count == 0)
            {
                log.Debug("Alaska download job queue is empty.");
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

                    string xmlFileName = "{0}.xml".Fill(mbi.BatchName);

                    string localXmlFilePath = Path.Combine(mbi.AMRequestPath, xmlFileName);
                    string ftpXmlFilePath = Path.Combine(SFTChannel.ToADEDir, xmlFileName);

                    sftSite.DownloadFile(ftpXmlFilePath, localXmlFilePath);

                    //Based on post download action xml file will get moved or delete from that location
                    if (SFTChannel.PostDownloadAction == PostDownloadAction.Archive)
                    {
                        string archiveFTPXmlFilePath = Path.Combine(SFTChannel.ArchiveDir, xmlFileName);

                        sftSite.MoveFile(ftpXmlFilePath, archiveFTPXmlFilePath);
                    }
                    else
                    {
                        sftSite.DeleteFile(ftpXmlFilePath);
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
        }

        protected override void ValidateBatches()
        {
            log.Debug("Validate job queue is started.");

            List<MailBatchDTO> downloadBatches = mailBatchSvc.GetBatchesByPipelineStatus(BatchPipelineStatus.Downloaded, client.Id);
            if (downloadBatches.Count == 0)
            {
                log.Debug("Validate job queue is empty.");
                return;
            }

            foreach (var mailBatch in downloadBatches)
            {
                MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, Config.Common_AuthoMailRootFolder);

                ClientBusinessIdentifier clientBusinessIdDetails = commonSvc.GetBusinessIdNameFromClientId(client.Id);

                var xmlFilePath = Path.Combine(mbi.AMRequestPath, mbi.BatchName + ".xml");

                XmlDocument mailBatchXML = new XmlDocument();
                XmlNodeList mailPieceNodes = null;

                try
                {
                    mailBatchXML.LoadXml(File.ReadAllText(xmlFilePath));
                    mailPieceNodes = mailBatchXML.DocumentElement.ChildNodes;
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
                    ReportInvalidBatch(mbi, "'{0}.xml' does not contain any valid node(s).".Fill(mbi.BatchName));

                    continue;
                }

                SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(SFTChannel.Protocol), SFTChannel.Host, SFTChannel.UserName, SFTChannel.Password, 300000);
                sftSite.Connect();

                List<string> fileListToMoveOrDelete = new List<string>();

                foreach (XmlNode mailPieceNode in mailPieceNodes)
                {
                    MailPieceDTO mailPieceDTO = new MailPieceDTO();
                    MailPieceCustomInfo mailPieceCustomInfo = null;
                    List<MailPieceCustomInfo> mailPieceCustomInfoList = new List<MailPieceCustomInfo>();

                    try
                    {
                        mailPieceDTO.ClientId = client.Id;
                        mailPieceDTO.UniqueId = Guid.NewGuid();
                        mailPieceDTO.MailBatchId = mbi.MailBatch.Id;
                        mailPieceDTO.ShippingStatus = ShippingStatus.InProgress.ToString();


                        ALKRespaControl node = XmlUtil.Deserialize<ALKRespaControl>(mailPieceNode.OuterXml);

                        mailPieceDTO.BusinessFriendlyIdName = clientBusinessIdDetails.BusinessIdName;
                        mailPieceDTO.BusinessFriendlyIdValue = node.LoanNbr;
                        mailPieceDTO.RcptFirstName = node.BorrowerFirstName;
                        mailPieceDTO.RcptLastName = node.BorrowerLastName;
                        mailPieceDTO.DocumentOrPackageType = mbi.MailBatch.DocumentOrPackageType;
                        mailPieceDTO.FileName = node.File;

                        mailPieceDTO.RcptAddressStreet = node.Address;
                        mailPieceDTO.RcptAddressAptOrSuiteOrOther = node.Address2;
                        mailPieceDTO.RcptAddressCity = node.City;
                        mailPieceDTO.RcptAddressState = node.State;
                        mailPieceDTO.RcptAddressZIP = node.Zip;
                        mailPieceDTO.IsRcptInfoSpecified = true;

                        if (node.ReturnInfo == null || node.ReturnInfo.Address == null)
                        {
                            mailPieceDTO.IsReturnInfoSpecified = false;
                        }
                        else
                        {
                            mailPieceDTO.IsReturnInfoSpecified = true;
                            mailPieceDTO.RetRcptAddrOptionalLine1AboveStreet = node.ReturnInfo.Address.Line1;
                            mailPieceDTO.RetRcptAddrOptionalLine2AboveStreet = node.ReturnInfo.Address.Line2;
                            mailPieceDTO.RetRcptAddressStreet = node.ReturnInfo.Address.Line3;
                            mailPieceDTO.RetRcptAddressCity = node.ReturnInfo.Address.City;
                            mailPieceDTO.RetRcptAddressState = node.ReturnInfo.Address.State;
                            mailPieceDTO.RetRcptAddressZIP = node.ReturnInfo.Address.Zip;
                        }

                        if (node.StatusID != null)
                        {
                            mailPieceCustomInfo = new MailPieceCustomInfo() { MailPieceId = 0, Key = "StatusID", DataType = DataType.String, Value = node.StatusID };
                            mailPieceCustomInfoList.Add(mailPieceCustomInfo);
                        }


                        MailPieceValidationResult validationResult = ValidateRespaControlNode(mailPieceNode);
                        if (validationResult.Code == OutComeCodes.No_Error)
                        {
                            //Now Check mentioned file is exist on sft or not that needs to be verified
                            string sftFilePath = Path.Combine(SFTChannel.ToADEDir, mailPieceDTO.FileName);
                            string localFilePath = Path.Combine(mbi.AMRequestPath, mailPieceDTO.FileName);

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
                            else if (!sftSite.IsFileAvailable(sftFilePath))
                            {
                                mailPieceDTO.ErrorCode = OutComeCodes.Missing_PDF_File;
                                mailPieceDTO.ErrorDesc = "Pdf attachment is not available.";
                                mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                            }
                            else
                            {
                                if (!File.Exists(localFilePath))
                                {
                                    //Downloading matching file name from client SFT
                                    sftSite.DownloadFile(sftFilePath, localFilePath);
                                }

                                //Creating collection for moving or deleting files because it may have same file for multiple nodes so can't move or delete at these level.
                                fileListToMoveOrDelete.Add(sftFilePath);

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
                        var msg = "Unable to process node for batch {0}.".Fill(mbi.BatchName);

                        log.Error(msg, ex);

                        mailPieceDTO.ErrorCode = OutComeCodes.Unknown_Error;
                        mailPieceDTO.ErrorDesc = msg + Environment.NewLine + ex.Message;
                        mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                    }


                    try
                    {
                        mailPieceSvc.InsertBatchMailPieceDetails(mailPieceDTO, mailPieceCustomInfoList);
                    }
                    catch (SqlException ex)
                    {
                        //2601 error code used for duplicate unique key insert 
                        if (ex.Number == 2601)
                        {
                            var msg = "[{0}] key id is duplicate in package info node.".Fill(mailPieceDTO.UniqueId);

                            log.Error(msg, ex);
                            mailPieceDTO.UniqueId = Guid.NewGuid();
                            mailPieceDTO.ErrorCode = OutComeCodes.Invalid_DuplicateKey;
                            mailPieceDTO.ErrorDesc = msg + Environment.NewLine + ex.Message;
                            mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                            mailPieceSvc.InsertBatchMailPieceDetails(mailPieceDTO, mailPieceCustomInfoList);
                        }
                    }
                }

                //Based on post download action pdf file will get move or delete from that location
                if (SFTChannel.PostDownloadAction == PostDownloadAction.Archive)
                {
                    foreach (var sftFilePath in fileListToMoveOrDelete.Distinct())
                    {
                        string archiveFTPPDFFilePath = Path.Combine(SFTChannel.ArchiveDir, Path.GetFileName(sftFilePath));

                        sftSite.MoveFile(sftFilePath, archiveFTPPDFFilePath);
                    }
                }
                else
                {
                    foreach (var sftFilePath in fileListToMoveOrDelete.Distinct())
                    {
                        sftSite.DeleteFile(sftFilePath);
                    }
                }

                if (sftSite.IsConnected)
                    sftSite.Disconnect();

                //After processing all nodes of a batch for validation status
                var mailPieceData = mailPieceSvc.GetMailPiecesByMailBatchId(mbi.MailBatch.Id);
                if (mailPieceData != null)
                {
                    var validMailPieces = mailPieceData.Where(c => c.ErrorCode == OutComeCodes.No_Error).ToList();

                    if (validMailPieces.Count == 0)
                    {
                        mbi.MailBatch.ValidationStatus = BatchValidationStatus.Invalid.ToString();
                        mbi.MailBatch.ValidationErrors = "Xml does not contain any valid respacontrol nodes.";
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

                    commonSvc.UpdateBatchValidationStatus(mbi.MailBatch.Id, mbi.MailBatch.ValidationStatus, mbi.MailBatch.ValidationErrors);
                }

                commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Validated, Common.Now);
            }
        }

        private MailPieceValidationResult ValidateRespaControlNode(XmlNode mailPieceNode)
        {
            var errors = GetAllRespaControlErrors(mailPieceNode).ToList();

            if (errors.Count > 0)
            {
                return new MailPieceValidationResult() { Code = errors.First().Code, ErrorMessage = string.Join(" | ", errors.Select(row => row.ErrorMessage)) };
            }

            return new MailPieceValidationResult() { Code = OutComeCodes.No_Error };
        }

        private IEnumerable<MailPieceValidationResult> GetAllRespaControlErrors(XmlNode mailPieceNode)
        {
            XDocument xDoc = XDocument.Parse(mailPieceNode.OuterXml);
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

            if (errorMsg.Contains("LoanNbr"))
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Invalid_XML_File, ErrorMessage = "Loan number is missing from respa control." };

            else if (errorMsg.Contains("RespaControl"))
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Invalid_XML_File, ErrorMessage = "Invalid RespaControl in xml." };

            if (!string.IsNullOrEmpty(errorMsg))
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Invalid_XML_File, ErrorMessage = errorMsg };

            yield break;
        }

        private void ReportInvalidBatch(MailBatchInfo mbi, string msg)
        {
            //For Invalid Batch, we will generate error File
            mailBatchSvc.MarkMailBatchAsInvalid(mbi.MailBatch.Id, msg);

            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportPrepped, Common.Now);

            commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, BatchStatus.Failed.ToString());
        }

        protected override void AcknowledgeBatches()
        {
            log.Debug("Alaska acknowledge job queue is started.");

            List<MailBatchDTO> validatedBatches = mailBatchSvc.GetBatchesByPipelineStatus(BatchPipelineStatus.Validated, client.Id);

            if (validatedBatches.Count == 0)
            {
                log.Debug("Alaska acknowledge job queue is empty.");
                return;
            }

            foreach (var batch in validatedBatches)
            {
                Deliverable reportDeliverable = commonSvc.GetDeliverableByReportType(client.Id, ReportTypes.Error);

                MailBatchInfo mbi = new MailBatchInfo(batch, client, Config.Common_AuthoMailRootFolder);

                var mailBatchData = mailBatchSvc.GetMailBatchDetailsByBatchId(mbi.MailBatch.Id);
                if (mailBatchData != null)
                {
                    try
                    {
                        string errorReportFileName = reportDeliverable.FileNameExpression;

                        Regex regEx = new Regex(@"{(.+?)}");

                        var param = regEx.Matches(errorReportFileName);
                        for (int i = 0; i < param.Count; i++)
                        {
                            string propertyName = param[i].Groups[1].Value;

                            errorReportFileName = errorReportFileName.Replace("{" + propertyName + "}", mbi.MailBatch.GetValueOfPropertyFromPropertyName(propertyName).ToString());
                        }

                        string errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);

                        if (mbi.MailBatch.ValidationStatus == BatchValidationStatus.Invalid)
                        {
                            StringBuilder errorData = alaskaSvc.AlaskaErrorReport(mailBatchData, mbi.Client.TimeZone, reportDeliverable.Delimeter);
                            if (errorData != null)
                            {
                                File.WriteAllText(errorReportPath, errorData.ToString());
                            }

                            // Batch status Prepped
                            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportPrepped, Common.Now);
                        }
                        else if (mbi.MailBatch.ValidationStatus == BatchValidationStatus.PartiallyValid)
                        {
                            StringBuilder errorData = alaskaSvc.AlaskaErrorReport(mailBatchData, mbi.Client.TimeZone, reportDeliverable.Delimeter);

                            if (errorData != null)
                            {
                                File.WriteAllText(errorReportPath, errorData.ToString());

                                if (reportDeliverable.IsSFTUploadRequired)
                                {
                                    SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(SFTChannel.Protocol), SFTChannel.Host, SFTChannel.UserName, SFTChannel.Password, 300000);
                                    sftSite.Connect();

                                    var ftpUploadPath = Path.Combine(reportDeliverable.UploadPath, errorReportFileName);

                                    sftSite.UploadFile(errorReportPath, ftpUploadPath);

                                    if (sftSite.IsConnected)
                                        sftSite.Disconnect();
                                }
                            }
                            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Acknowledged, Common.Now);
                        }
                        else
                        {
                            // Batch is valid
                            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Acknowledged, Common.Now);
                        }

                        if (reportDeliverable.EmailChannel != null)
                        {
                            MailUtility.SendDownloadNotificationEmail(mbi, mailBatchData, errorReportPath, reportDeliverable.EmailChannel);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("Error occured while acknowleding for Batch-Id : {0}".Fill(mbi.BatchName), ex);
                    }
                }
            }

            log.Debug("Alaska acknowledge job queue is completed.");
        }
    }
}
