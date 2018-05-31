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
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace AM.Console
{
    class GeneralChannel : Channel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static XmlSchemaSet schemas;
        protected Client client;

        static GeneralChannel()
        {
            schemas = new XmlSchemaSet();
            schemas.Add(string.Empty, XmlReader.Create(new StringReader(Resources.MailingRequest)));
        }

        public GeneralChannel(string channelCode)
            : base(channelCode)
        {
        }

        protected override void EnumerateBatches()
        {
            log.Debug("Enumerate Batches job queue for " + client.AppCode + " is started.");

            List<string> xmlFileNames = new List<string>();

            Deliverable deliverable = commonSvc.GetDeliverableByReportType(client.Id, ReportTypes.NoBatch);

            SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(SFTChannel.Protocol), SFTChannel.Host, SFTChannel.UserName, SFTChannel.Password, 300000);
            sftSite.Connect();

            xmlFileNames = GetXmlFileNames(sftSite);

            if (xmlFileNames.Count == 0)
            {
                MailUtility.SendNoBatchReceivedNotificationEmail(client, Common.Now, deliverable);
                return;
            }

            foreach (string xmlFile in xmlFileNames)
            {
                string xmlFilePath = Path.Combine(SFTChannel.ToADEDir, xmlFile + ".xml");

                //Download and Insert the batch in database first always.
                string batchName = xmlFile;

                MailBatchDTO mailBatch = new MailBatchDTO();
                mailBatch.ClientId = client.Id;
                mailBatch.BatchName = batchName;
                mailBatch.SFTChannelId = SFTChannel.Id;
                mailBatch.PrintLocation = Config.Common_PrintLocation;

                //Insert New Batch Entry
                mailBatch.Id = mailBatchSvc.InsertBatchDetails(mailBatch);

                //Update Pipeline Status of Batch Received
                commonSvc.UpdateBatchPipeLineStatus(mailBatch.Id, BatchPipelineStatus.Received, sftSite.LastModifiedTimeOfFile(xmlFilePath));

                commonSvc.UpdateBatchStatus(mailBatch.Id, BatchStatus.InProgress.ToString());
            }

            if (sftSite.IsConnected)
                sftSite.Disconnect();
            log.Debug("Enumerate Batches job queue for " + client.AppCode + " is completed.");
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
            log.Debug(client.AppCode + " download job is started.");

            List<MailBatchDTO> downloadBatches = mailBatchSvc.GetBatchesByPipelineStatus(BatchPipelineStatus.Received, client.Id);
            if (downloadBatches.Count == 0)
            {
                log.Debug(client.AppCode + " download job queue is empty.");
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

            log.Debug(client.AppCode + " download job is completed.");
        }

        protected override void ValidateBatches()
        {
            log.Debug(client.AppCode + " validate job queue is started.");

            List<MailBatchDTO> downloadBatches = mailBatchSvc.GetBatchesByPipelineStatus(BatchPipelineStatus.Downloaded, client.Id);
            if (downloadBatches.Count == 0)
            {
                log.Debug(client.AppCode + " validate job queue is empty.");
                return;
            }

            foreach (var mailBatch in downloadBatches)
            {
                MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, Config.Common_AuthoMailRootFolder);

                Deliverable deliverables = commonSvc.GetDeliverableByReportType(client.Id, ReportTypes.Error);

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

                    ReportInvalidBatch(mbi, msg, deliverables);

                    continue;
                }
                catch (Exception ex)
                {
                    var msg = "'{0}.xml' is not a valid XML document.".Fill(mbi.BatchName);
                    log.Error(msg, ex);
                    ReportInvalidBatch(mbi, ex.Message, deliverables);
                    continue;
                }

                if (mailPieceNodes == null || mailPieceNodes.Count == 0)
                {
                    ReportInvalidBatch(mbi, "'{0}.xml' does not contain any valid node(s).".Fill(mbi.BatchName), deliverables);

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


                        MailPiece node = XmlUtil.Deserialize<MailPiece>(mailPieceNode.OuterXml);

                        ClientBusinessIdentifier clientBusinessIdDetails = commonSvc.CheckBusinessIdNameAlreadyExist(client.Id, node.BusinessFriendlyIdName);

                        if (clientBusinessIdDetails == null)
                        {
                            commonSvc.InsertBusinessIdentifierName(client.Id, node.BusinessFriendlyIdName);
                        }

                        mailPieceDTO.BusinessFriendlyIdName = node.BusinessFriendlyIdName;
                        mailPieceDTO.BusinessFriendlyIdValue = node.BusinessFriendlyIdValue;
                        mailPieceDTO.DocumentOrPackageType = node.DocumentOrPackageType;

                        //Receipient Info
                        if (node.RecipientInfo == null || node.RecipientInfo.Recipient == null)
                        {
                            mailPieceDTO.IsRcptInfoSpecified = false;
                        }
                        else
                        {
                            mailPieceDTO.IsRcptInfoSpecified = true;

                            mailPieceDTO.RcptOrganization = node.RecipientInfo.Recipient.Organization;
                            mailPieceDTO.RcptPrefix = node.RecipientInfo.Recipient.Prefix;
                            mailPieceDTO.RcptFirstName = node.RecipientInfo.Recipient.FirstName;
                            mailPieceDTO.RcptMiddleName = node.RecipientInfo.Recipient.MiddleName;
                            mailPieceDTO.RcptLastName = node.RecipientInfo.Recipient.LastName;
                            mailPieceDTO.RcptSuffix = node.RecipientInfo.Recipient.Suffix;
                            mailPieceDTO.RcptPhone = node.RecipientInfo.Recipient.Phone;

                            mailPieceDTO.RcptAddrOptionalLine1AboveStreet = node.RecipientInfo.Address.OptionalLine1AboveStreet;
                            mailPieceDTO.RcptAddrOptionalLine2AboveStreet = node.RecipientInfo.Address.OptionalLine2AboveStreet;
                            mailPieceDTO.RcptAddrOptionalLine1BelowStreet = node.RecipientInfo.Address.OptionalLine1BelowStreet;
                            mailPieceDTO.RcptAddrOptionalLine2BelowStreet = node.RecipientInfo.Address.OptionalLine2BelowStreet;
                            mailPieceDTO.RcptAddressStreet = node.RecipientInfo.Address.Street;
                            mailPieceDTO.RcptAddressAptOrSuiteOrOther = node.RecipientInfo.Address.AptOrSuiteOrOther;
                            mailPieceDTO.RcptAddressCity = node.RecipientInfo.Address.City;
                            mailPieceDTO.RcptAddressState = node.RecipientInfo.Address.State;
                            mailPieceDTO.RcptAddressZIP = node.RecipientInfo.Address.Zip;
                        }

                        //Co-Receipient Info
                        mailPieceDTO.CoRcptOrganization = node.RecipientInfo.Corecipient.Organization;
                        mailPieceDTO.CoRcptPrefix = node.RecipientInfo.Corecipient.Prefix;
                        mailPieceDTO.CoRcptFirstName = node.RecipientInfo.Corecipient.FirstName;
                        mailPieceDTO.CoRcptMiddleName = node.RecipientInfo.Corecipient.MiddleName;
                        mailPieceDTO.CoRcptLastName = node.RecipientInfo.Corecipient.LastName;
                        mailPieceDTO.CoRcptSuffix = node.RecipientInfo.Corecipient.Suffix;
                        mailPieceDTO.CoRcptPhone = node.RecipientInfo.Corecipient.Phone;

                        //File info
                        mailPieceDTO.FileName = node.DocumentOrPackage.Name;
                        mailPieceDTO.FilePattern = node.DocumentOrPackage.Pattern;

                        if (node.ReturnInfo == null || node.ReturnInfo.Address == null)
                        {
                            mailPieceDTO.IsReturnInfoSpecified = false;
                        }
                        else
                        {
                            //Return info
                            mailPieceDTO.IsReturnInfoSpecified = true;

                            mailPieceDTO.RetRcptOrganization = node.ReturnInfo.Recipient.Organization;
                            mailPieceDTO.RetRcptPrefix = node.ReturnInfo.Recipient.Prefix;
                            mailPieceDTO.RetRcptFirstName = node.ReturnInfo.Recipient.FirstName;
                            mailPieceDTO.RetRcptMiddleName = node.ReturnInfo.Recipient.MiddleName;
                            mailPieceDTO.RetRcptLastName = node.ReturnInfo.Recipient.LastName;
                            mailPieceDTO.RetRcptSuffix = node.ReturnInfo.Recipient.Suffix;
                            mailPieceDTO.RetRcptPhone = node.ReturnInfo.Recipient.Phone;

                            mailPieceDTO.RetRcptAddrOptionalLine1AboveStreet = node.ReturnInfo.Address.OptionalLine1AboveStreet;
                            mailPieceDTO.RetRcptAddrOptionalLine2AboveStreet = node.ReturnInfo.Address.OptionalLine2AboveStreet;
                            mailPieceDTO.RetRcptAddrOptionalLine1BelowStreet = node.ReturnInfo.Address.OptionalLine1BelowStreet;
                            mailPieceDTO.RetRcptAddrOptionalLine2BelowStreet = node.ReturnInfo.Address.OptionalLine2BelowStreet;
                            mailPieceDTO.RetRcptAddressStreet = node.ReturnInfo.Address.Street;
                            mailPieceDTO.RetRcptAddressAptOrSuiteOrOther = node.ReturnInfo.Address.AptOrSuiteOrOther;
                            mailPieceDTO.RetRcptAddressCity = node.ReturnInfo.Address.City;
                            mailPieceDTO.RetRcptAddressState = node.ReturnInfo.Address.State;
                            mailPieceDTO.RetRcptAddressZIP = node.ReturnInfo.Address.Zip;
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
                            var msg = "{0} key id is duplicate in package info node.".Fill(mailPieceDTO.UniqueId);

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
            log.Debug(client.AppCode + " Validate job queue is completed.");
        }

        private MailPieceValidationResult ValidateRespaControlNode(XmlNode mailPieceNode)
        {
            throw new NotImplementedException();
        }

        private void ReportInvalidBatch(MailBatchInfo mbi, string msg, Deliverable deliverables)
        {
            MailUtility.SendNoDataNotificationEmail(mbi, deliverables.EmailChannel, msg);
            //For Invalid Batch, we will generate error File
            mailBatchSvc.MarkMailBatchAsInvalid(mbi.MailBatch.Id, msg);

            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportedBack, Common.Now);

            commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, BatchStatus.Failed.ToString());
        }

        protected override void AcknowledgeBatches()
        {
            throw new NotImplementedException();
        }
    }
}
