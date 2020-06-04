using AM.Console.Properties;
using AM.Entities;
using AM.Lib;
using AM.Services;
using Aspose.Words;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
    class DELFUSChannel : AMSchemaChannel
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string channelType;

        private static XmlSchemaSet schemas;

        static DELFUSChannel()
        {
            schemas = new XmlSchemaSet();
            schemas.Add("http://schemas.corelogic.com/ade/authomail/v1.18.0", XmlReader.Create(new StringReader(Resources.AMMailPieceSchema)));
        }
        public DELFUSChannel(string channelCode, string channelType)
            : base(channelCode, channelType)
        {
            generalSvc = GetService<GeneralService>();
            this.channelType = channelType;
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
                    //NOTE: Here system will delete if any existing records are there with MailBatchId in mail pieces table
                    mailBatchSvc.DeleteMailPiecesDetailsByMailBatchId(mailBatch.Id);

                    Client client = clientSvc.GetClientById(mailBatch.ClientId);

                    MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, Config.Common_AuthoMailRootFolder);

                    var xmlFilePath = Path.Combine(mbi.AMRequestPath, mbi.BatchName + ".xml");
                    int totalNodes = 0;

                    MailBatch mbRequest;
                    try
                    {
                        mbRequest = XmlUtil.Deserialize<MailBatch>(File.ReadAllText(xmlFilePath));
                        totalNodes = mbRequest.MailPieces.Count;
                    }
                    catch (Exception ex)
                    {
                        var msg = "'{0}.xml' is not a valid XML document.".Fill(mbi.BatchName);
                        log.Error(msg, ex);
                        ReportInvalidBatch(mailBatch, "");
                        continue;
                    }

                    foreach (var mp in mbRequest.MailPieces)
                    {
                        //mailPieceNode.Normalize();
                        MailPieceDTO mailPieceDTO = new MailPieceDTO();
                        MailPieceCustomInfo customInfo = null;
                        List<MailPieceCustomInfo> mailPieceCustomInfo = new List<MailPieceCustomInfo>();

                        mailPieceDTO.ClientId = client.Id;
                        mailPieceDTO.MailBatchId = mbi.MailBatch.Id;
                        mailPieceDTO.ShippingStatus = ShippingStatus.InProgress.ToString();

                        try
                        {
                            mailPieceDTO.BusinessFriendlyIdName = mp.BusinessFriendlyIdName.TrimIfNotNull();

                            //Here we are inserting BusinessIdentifier for Client if not exist.
                            ClientBusinessIdentifier clientBusinessIdDetails = commonSvc.CheckBusinessIdNameAlreadyExist(client.Id, mailPieceDTO.BusinessFriendlyIdName);
                            if (clientBusinessIdDetails == null)
                            {
                                commonSvc.InsertBusinessIdentifierName(client.Id, mailPieceDTO.BusinessFriendlyIdName.TrimIfNotNull());
                            }

                            mailPieceDTO.BusinessFriendlyIdValue = mp.BusinessFriendlyIdValue.TrimIfNotNull();
                            mailPieceDTO.DocumentOrPackageType = mp.DocumentOrPackageType == null ? mailBatch.DocumentOrPackageType : mp.DocumentOrPackageType;
                            mailPieceDTO.UniqueId = mp.Id == null || mp.Id == Guid.Empty ? Guid.NewGuid() : mp.Id;

                            mailPieceDTO.FileName = mp.DocumentOrPackage.Name.TrimIfNotNull();

                            if (mp.RecipientInfo != null)
                            {
                                if (mp.RecipientInfo.Recipient != null)
                                {
                                    mailPieceDTO.IsRcptInfoSpecified = true;

                                    mailPieceDTO.RcptFirstName = mp.RecipientInfo.Recipient.FirstName.TrimIfNotNull();
                                    mailPieceDTO.RcptLastName = mp.RecipientInfo.Recipient.LastName.TrimIfNotNull();
                                    mailPieceDTO.RcptMiddleName = mp.RecipientInfo.Recipient.MiddleName.TrimIfNotNull();
                                    mailPieceDTO.RcptOrganization = mp.RecipientInfo.Recipient.Organization.TrimIfNotNull();
                                    mailPieceDTO.RcptPrefix = mp.RecipientInfo.Recipient.Prefix.TrimIfNotNull();
                                    mailPieceDTO.RcptSuffix = mp.RecipientInfo.Recipient.Suffix.TrimIfNotNull();
                                    mailPieceDTO.RcptPhone = mp.RecipientInfo.Recipient.Phone.TrimIfNotNull();
                                    mailPieceDTO.RcptEmailId = mp.RecipientInfo.Recipient.EmailAddress.TrimIfNotNull();
                                }
                                else
                                    mailPieceDTO.IsRcptInfoSpecified = false;

                                if (mp.RecipientInfo.Address != null)
                                {
                                    mailPieceDTO.IsRcptInfoSpecified = true;

                                    mailPieceDTO.RcptAddrOptionalLine1AboveStreet = mp.RecipientInfo.Address.OptionalLine1AboveStreet.TrimIfNotNull();
                                    mailPieceDTO.RcptAddrOptionalLine2AboveStreet = mp.RecipientInfo.Address.OptionalLine2AboveStreet.TrimIfNotNull();

                                    mailPieceDTO.RcptAddressStreet = mp.RecipientInfo.Address.Street.TrimIfNotNull();
                                    mailPieceDTO.RcptAddressAptOrSuiteOrOther = mp.RecipientInfo.Address.AptOrSuiteOrOther.TrimIfNotNull();

                                    mailPieceDTO.RcptAddrOptionalLine1BelowStreet = mp.RecipientInfo.Address.OptionalLine1BelowStreet.TrimIfNotNull();
                                    mailPieceDTO.RcptAddrOptionalLine2BelowStreet = mp.RecipientInfo.Address.OptionalLine2BelowStreet.TrimIfNotNull();

                                    mailPieceDTO.RcptAddressCity = mp.RecipientInfo.Address.City.TrimIfNotNull();
                                    mailPieceDTO.RcptAddressState = mp.RecipientInfo.Address.State.TrimIfNotNull();
                                    mailPieceDTO.RcptAddressZIP = mp.RecipientInfo.Address.Zip.TrimIfNotNull();
                                    mailPieceDTO.RcptAddressCountry = mp.RecipientInfo.Address.Country.TrimIfNotNull();
                                }
                            }
                            else
                            {
                                mailPieceDTO.IsRcptInfoSpecified = false;
                            }

                            mailPieceDTO.IsEDeliveryRequested = false;
                            mailPieceDTO.IsFallBackRequested = false;

                            if (mp.ReturnInfo == null)
                            {
                                mailPieceDTO.IsReturnInfoSpecified = false;
                            }
                            else
                            {
                                if (mp.ReturnInfo.Recipient != null)
                                {
                                    mailPieceDTO.IsReturnInfoSpecified = true;
                                    mailPieceDTO.RetRcptFirstName = mp.ReturnInfo.Recipient.FirstName.TrimIfNotNull();
                                    mailPieceDTO.RetRcptLastName = mp.ReturnInfo.Recipient.LastName.TrimIfNotNull();

                                    mailPieceDTO.RetRcptMiddleName = mp.ReturnInfo.Recipient.MiddleName.TrimIfNotNull();
                                    mailPieceDTO.RetRcptOrganization = mp.ReturnInfo.Recipient.Organization.TrimIfNotNull();
                                    mailPieceDTO.RetRcptPrefix = mp.ReturnInfo.Recipient.Prefix.TrimIfNotNull();
                                    mailPieceDTO.RetRcptSuffix = mp.ReturnInfo.Recipient.Suffix.TrimIfNotNull();
                                    mailPieceDTO.RetRcptPhone = mp.ReturnInfo.Recipient.Phone.TrimIfNotNull();
                                }
                                else
                                    mailPieceDTO.IsReturnInfoSpecified = false;

                                if (mp.ReturnInfo.Address != null)
                                {
                                    mailPieceDTO.IsReturnInfoSpecified = true;
                                    mailPieceDTO.RetRcptAddrOptionalLine1AboveStreet = mp.ReturnInfo.Address.OptionalLine1AboveStreet.TrimIfNotNull();
                                    mailPieceDTO.RetRcptAddrOptionalLine2AboveStreet = mp.ReturnInfo.Address.OptionalLine2AboveStreet.TrimIfNotNull();

                                    mailPieceDTO.RetRcptAddressStreet = mp.ReturnInfo.Address.Street.TrimIfNotNull();
                                    mailPieceDTO.RetRcptAddressAptOrSuiteOrOther = mp.ReturnInfo.Address.AptOrSuiteOrOther.TrimIfNotNull();

                                    mailPieceDTO.RetRcptAddrOptionalLine1BelowStreet = mp.ReturnInfo.Address.OptionalLine1BelowStreet.TrimIfNotNull();
                                    mailPieceDTO.RetRcptAddrOptionalLine2BelowStreet = mp.ReturnInfo.Address.OptionalLine2BelowStreet.TrimIfNotNull();

                                    mailPieceDTO.RetRcptAddressCity = mp.ReturnInfo.Address.City.TrimIfNotNull();
                                    mailPieceDTO.RetRcptAddressState = mp.ReturnInfo.Address.State.TrimIfNotNull();
                                    mailPieceDTO.RetRcptAddressZIP = mp.ReturnInfo.Address.Zip.TrimIfNotNull();
                                }
                            }

                            if (mp.CustomInfo != null)
                            {
                                foreach (var cstmInfo in mp.CustomInfo)
                                {
                                    customInfo = new MailPieceCustomInfo() { MailPieceId = 0, Key = cstmInfo.Key.TrimIfNotNull(), DataType = cstmInfo.DataType, Value = cstmInfo.Value.TrimIfNotNull() };

                                    mailPieceCustomInfo.Add(customInfo);
                                }
                                mailPieceDTO.MailPieceCustomInfo = mailPieceCustomInfo;
                            }
                            //System will check for common validations
                            MailPieceValidationResult commonValidationResult = ValidateCommonValidation(mailPieceDTO, mbi);
                            if (commonValidationResult.Code == OutComeCodes.No_Error)
                            {
                                mailPieceDTO.ErrorCode = OutComeCodes.No_Error;
                                mailPieceDTO.ErrorDesc = "";
                            }
                            else
                            {
                                mailPieceDTO.ErrorCode = commonValidationResult.Code;
                                mailPieceDTO.ErrorDesc = commonValidationResult.ErrorMessage;
                                mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                            }
                        }
                        catch (Exception ex)
                        {
                            var msg = "Unable to process package info node for mail batch {0}".Fill(mbi.BatchName);

                            log.Error(msg, ex);

                            mailPieceDTO.ErrorCode = OutComeCodes.Unknown_Error;
                            mailPieceDTO.ErrorDesc = msg + Environment.NewLine + ex.ToString();
                            mailPieceDTO.ShippingStatus = ShippingStatus.Error.ToString();
                        }

                        try
                        {
                            mailPieceSvc.InsertBatchMailPieceDetails(mailPieceDTO, mailPieceCustomInfo, mbi.MailBatch.ReceivedOn, mbi.MailBatch.DownloadedOn);
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

                    var mailPieceList = mailPieceSvc.GetMailPiecesByMailBatchId(mbi.MailBatch.Id);
                    if (mailPieceList != null)
                    {
                        var validMailPieces = mailPieceList.Where(c => c.ErrorCode == OutComeCodes.No_Error).ToList();

                        //Here System will validate Addresses for valid mail pieces
                        //if (validMailPieces.Count > 0)
                        //    Channel.ValidateAddressType(mailBatch, validMailPieces);

                        if (validMailPieces.Count == 0)
                        {
                            mbi.MailBatch.ValidationStatus = BatchValidationStatus.Invalid.ToString();
                            mbi.MailBatch.ValidationErrors = "Xml does not contain any valid mail piece nodes.";
                            commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, BatchStatus.Failed.ToString()).Wait();
                        }
                        else if (validMailPieces.Count < mailPieceList.Count)
                        {
                            mbi.MailBatch.ValidationStatus = BatchValidationStatus.PartiallyValid.ToString();
                        }
                        else
                        {
                            mbi.MailBatch.ValidationStatus = BatchValidationStatus.Valid.ToString();
                        }

                        commonSvc.UpdateBatchValidationStatus(mbi.MailBatch.Id, mbi.MailBatch.ValidationStatus, mbi.MailBatch.ValidationErrors);
                    }

                    if (totalNodes != mailPieceList.Count)
                    {
                        throw new Exception("Mail pieces count and received xml count is mismatched for Batch-Id : {0}<br/>".Fill(mailBatch.Id) + "Mail Piece count is {0} and received xml nodes count is {1}".Fill(mailPieceList.Count, totalNodes));
                    }
                    Channel.CalculateDueDateAndPrintLocation(mailBatch, mailPieceList);

                    if (mailPieceList.All(mp => mp.IsEDeliveryRequested && mp.ShippingStatus != ShippingStatus.Error.ToString()))
                    {
                        commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.SubmittedToEDelivery, Common.Now).Wait();
                        commonSvc.UpdateEDeliveryPipelineStatus(mbi.MailBatch.Id, EDPipelineStatus.Validated, Common.Now);
                    }
                    else
                        commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Validated, Common.Now).Wait();
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

            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportPrepped, Common.Now).Wait();

            string remoteFilePath = Path.Combine(deliverable.UploadPath, invalidBatchFileName);

            var mailBatchSFTChannel = clientSvc.GetSFTChannelDetailsBySFTChannelId(mailBatch.SFTChannelId).Result;

            SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(mailBatchSFTChannel.Protocol), mailBatchSFTChannel.Host, mailBatchSFTChannel.UserName, mailBatchSFTChannel.Password, 300000);
            sftSite.Connect();

            sftSite.UploadFile(invalidBatchPath, remoteFilePath);

            if (sftSite.IsFileAvailable(Path.Combine(mailBatchSFTChannel.ToADEDir, mbi.BatchName + ".zip")))
                MoveBatchToBackupDir(sftSite, Path.Combine(mailBatchSFTChannel.ToADEDir, mbi.BatchName + ".zip"));

            MoveBatchToBackupDir(sftSite, Path.Combine(mailBatchSFTChannel.ToADEDir, mbi.BatchName + ".go"));

            if (sftSite.IsConnected)
                sftSite.Disconnect();

            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportedBack, Common.Now).Wait();
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

        private MailPieceValidationResult ValidateCommonValidation(MailPieceDTO mailPieceDTO, MailBatchInfo mbi)
        {
            //in last system will check for general validations
            string localFilePath = Path.Combine(mbi.AMRequestPath, mailPieceDTO.FileName);

            if (!File.Exists(localFilePath))
            {
                return new MailPieceValidationResult() { Code = OutComeCodes.Missing_PDF_File, ErrorMessage = "Pdf file is not available for this package." };
            }

            int pageCount = 0;
            List<int> blankPageNos = new List<int>();
            using (iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(localFilePath))
            {
                pageCount = reader.NumberOfPages;
                mailPieceDTO.TotalPages = pageCount;//System is going to store TotalPages only for AM.EDMailPieces table from here
                blankPageNos = Channel.CheckIfPdfBlank(pageCount, blankPageNos, localFilePath, reader);
            }

            //NOTE : Here system is storing file size of received pdf in KB
            FileInfo fileInfo = new FileInfo(localFilePath);
            mailPieceDTO.PDFFileSize = (fileInfo.Length / 1024);

            //Document Or Package Type validation
            var docType = docTypeSvc.GetDocTypeByDocTypeName(mailPieceDTO.ClientId, mailPieceDTO.DocumentOrPackageType, pageCount, mbi.MailBatch.ReceivedOn.Value.UtcToEst());

            if (docType != null)
            {
                mailPieceDTO.DocTypeId = docType.Id;
                mailPieceDTO.PnSConfigDetailId = docType.PnSConfigDetailId;
                mailPieceDTO.PrintTrailId = docType.PrintTrailId;
            }

            if (docType == null)
            {
                return new MailPieceValidationResult() { Code = OutComeCodes.Invalid_PackageType, ErrorMessage = "Either the specified document or package type '{0}' is not configured or no matching print configuration found for page count ({1}).".Fill(mailPieceDTO.DocumentOrPackageType, mailPieceDTO.TotalPages) };
            }
            else if (pageCount == blankPageNos.Count())
            {
                mailPieceDTO.TotalBlankPages = blankPageNos.Count;
                mailPieceDTO.BlankPageNumbers = string.Join(",", blankPageNos);
                return new MailPieceValidationResult() { Code = OutComeCodes.Invalid_PDF_File, ErrorMessage = "All pages are blank in received pdf file." };
            }
            else if (!mailPieceDTO.IsReturnInfoSpecified && !mbi.MailBatch.IsReturnInfoSpecified && !docType.IsReturnInfoSpecified)
            {
                return new MailPieceValidationResult() { Code = OutComeCodes.Mailing_Address_Missing_Or_Invalid, ErrorMessage = "Return mailing address is not specified for this package." };
            }
            else
            {
                //if at mail piece level or at mail batch level, system didn't find return info then it will check for MailBatch level if exist system will use that in mail piece level 
                mailPieceDTO.TotalBlankPages = blankPageNos.Count;
                mailPieceDTO.BlankPageNumbers = string.Join(",", blankPageNos);
                if (!mailPieceDTO.IsReturnInfoSpecified && !mbi.MailBatch.IsReturnInfoSpecified && docType.IsReturnInfoSpecified)
                {
                    mailPieceDTO.RetRcptFirstName = docType.RetRcptFirstName;
                    mailPieceDTO.RetRcptLastName = docType.RetRcptLastName;

                    mailPieceDTO.RetRcptMiddleName = docType.RetRcptMiddleName;
                    mailPieceDTO.RetRcptOrganization = docType.RetRcptOrganization;
                    mailPieceDTO.RetRcptPrefix = docType.RetRcptPrefix;
                    mailPieceDTO.RetRcptSuffix = docType.RetRcptSuffix;
                    mailPieceDTO.RetRcptPhone = docType.RetRcptPhone;

                    mailPieceDTO.RetRcptAddrOptionalLine1AboveStreet = docType.RetRcptAddrOptionalLine1AboveStreet;
                    mailPieceDTO.RetRcptAddrOptionalLine2AboveStreet = docType.RetRcptAddrOptionalLine2AboveStreet;

                    mailPieceDTO.RetRcptAddressStreet = docType.RetRcptAddressStreet;
                    mailPieceDTO.RetRcptAddressAptOrSuiteOrOther = docType.RetRcptAddressAptOrSuiteOrOther;

                    mailPieceDTO.RetRcptAddrOptionalLine1BelowStreet = docType.RetRcptAddrOptionalLine1BelowStreet;
                    mailPieceDTO.RetRcptAddrOptionalLine2BelowStreet = docType.RetRcptAddrOptionalLine2BelowStreet;

                    mailPieceDTO.RetRcptAddressCity = docType.RetRcptAddressCity;
                    mailPieceDTO.RetRcptAddressState = docType.RetRcptAddressState;
                    mailPieceDTO.RetRcptAddressZIP = docType.RetRcptAddressZIP;
                }
            }

            return new MailPieceValidationResult() { Code = OutComeCodes.No_Error };
        }
    }
}
