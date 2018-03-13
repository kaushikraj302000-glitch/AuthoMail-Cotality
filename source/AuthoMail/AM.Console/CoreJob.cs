using AM.Entities;
using AM.Services;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AM.Lib;
namespace AM.Console
{
    public class CoreJob
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CoreJob));
        IClientService clientSvc;
        IBatchService mailBatchSvc;
        IMailPieceService mailPieceSvc;
        ICommonService commonSvc;
        public CoreJob()
        {
            clientSvc = new ClientService();
            mailBatchSvc = new BatchService();
            commonSvc = new CommonService();
            mailPieceSvc = new MailPieceService();
        }

        public void ParseAuthoMailSchema()
        {
            List<MailBatchDTO> downloadBatches = mailBatchSvc.GetBatchesByStatus(BatchPipelineStatus.Downloaded);
            if (downloadBatches == null)
            {
                log.Debug("Process batches job queue is empty.");
                return;
            }

            foreach (var batch in downloadBatches)
            {
                Client client = clientSvc.GetClientDetails(batch.ClientId).Single();
                SFTChannel sftChannelDetails = clientSvc.GetSFTChannelDetails(batch.SFTChannelId);

                MailBatchInfo mbi = new MailBatchInfo(batch, client, sftChannelDetails);

                var fullBatchPath = Path.Combine(mbi.AMRequestPath, mbi.BatchName);
                var amXmlFilePath = Path.Combine(fullBatchPath, mbi.BatchName + "_AuthoMail.xml");

                if (!File.Exists(amXmlFilePath))
                {
                    mbi.MailBatch.ValidationStatus = BatchValidationStatus.Invalid;
                    mbi.MailBatch.ValidationErrors = "No AM XML file is available in {0} batch.".Fill(mbi.MailBatch.BatchName);

                    mailBatchSvc.UpdateBatchDetails(mbi.MailBatch);

                    commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, BatchStatus.Failed.ToString());
                    continue;
                }

                MailBatch amParsedBatch = null;
                try
                {
                    XmlDocument amXMLDoc = new XmlDocument();
                    amXMLDoc.LoadXml(File.ReadAllText(amXmlFilePath));
                    amParsedBatch = XmlUtil.Deserialize<MailBatch>(amXMLDoc.OuterXml);
                }
                catch (XmlException ex)
                {
                    mbi.MailBatch.ValidationStatus = BatchValidationStatus.Invalid;
                    mbi.MailBatch.ValidationErrors = ex.Message;

                    mailBatchSvc.UpdateBatchDetails(mbi.MailBatch);

                    commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, BatchStatus.Failed.ToString());
                    continue;
                }

                if (amParsedBatch != null)
                {
                    mbi.MailBatch.Priority = amParsedBatch.Priority.ToString();
                    mbi.MailBatch.UniqueId = amParsedBatch.Id;
                    mbi.MailBatch.ClientBatchId = amParsedBatch.ClientBatchId;

                    mailBatchSvc.UpdateBatchDetails(mbi.MailBatch);

                    if (amParsedBatch.MailPieces == null || amParsedBatch.MailPieces.Count == 0)
                    {
                        mbi.MailBatch.ValidationStatus = BatchValidationStatus.Invalid;
                        mbi.MailBatch.ValidationErrors = "No mailpiece found in xml for batch : {0}".Fill(mbi.MailBatch.Id);

                        mailBatchSvc.UpdateBatchDetails(mbi.MailBatch);

                        commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, BatchStatus.Failed.ToString());
                        continue;
                    }

                    commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.ValidationStart, Common.Now);
                    foreach (var piece in amParsedBatch.MailPieces)
                    {
                        MailPieceDTO mailPiece = new MailPieceDTO();
                        List<MailPieceCustomInfo> mailPieceCustInfo = new List<MailPieceCustomInfo>();

                        mailPiece.MailBatchId = mbi.MailBatch.Id;
                        mailPiece.ClientId = mbi.Client.Id;
                        try
                        {
                            ValidationResultAMC validationResult = Common.ValidateAMMailPiecesForUSAA(piece);

                            if (validationResult.Code == OutComeCodes.No_Error)
                            {
                                mailPiece.BusinessFriendlyIdName = piece.BusinessFriendlyIdName;
                                mailPiece.BusinessFriendlyIdValue = piece.BusinessFriendlyIdValue;
                                mailPiece.DocumentOrPackageType = piece.DocumentOrPackageType;
                                mailPiece.FileName = piece.DocumentOrPackage.Name;
                                mailPiece.UniqueId = piece.Id;
                                mailPiece.DueDate = Common.Today; //TODO : Need to change after discussing with Ravi Sir
                                mailPiece.IsRcptInfoSpecified = true;
                                mailPiece.RcptFirstName = piece.RecipientInfo.Recipient.FirstName;
                                mailPiece.RcptLastName = piece.RecipientInfo.Recipient.LastName;
                                if (piece.RecipientInfo.Corecipient != null)
                                {
                                    mailPiece.CoRcptFirstName = piece.RecipientInfo.Corecipient.FirstName;
                                    mailPiece.CoRcptLastName = piece.RecipientInfo.Corecipient.LastName;
                                }
                                mailPiece.RcptAddressStreet = piece.RecipientInfo.Address.Street;
                                mailPiece.RcptAddressCity = piece.RecipientInfo.Address.City;
                                mailPiece.RcptAddressState = piece.RecipientInfo.Address.State;
                                mailPiece.RcptAddressZIP = piece.RecipientInfo.Address.Zip;
                                mailPiece.RcptPhone = piece.RecipientInfo.Recipient.Phone;

                                mailPiece.IsReturnInfoSpecified = true;
                                mailPiece.RetRcptFirstName = piece.ReturnInfo.Recipient.FirstName;
                                mailPiece.RetRcptLastName = piece.ReturnInfo.Recipient.LastName;
                                mailPiece.RetRcptAddressStreet = piece.ReturnInfo.Address.Street;
                                mailPiece.RetRcptAddressCity = piece.ReturnInfo.Address.City;
                                mailPiece.RetRcptAddressState = piece.ReturnInfo.Address.State;
                                mailPiece.RetRcptAddressZIP = piece.ReturnInfo.Address.Zip;
                                mailPiece.RetRcptPhone = piece.ReturnInfo.Recipient.Phone;

                                //Check PDF file is available or not
                                if (!File.Exists(Path.Combine(fullBatchPath, mailPiece.FileName)))
                                {
                                    mailPiece.ErrorCode = OutComeCodes.Missing_PDF.ToString();
                                    mailPiece.ErrorDesc = "Pdf file is not available.";
                                }
                                else
                                {
                                    mailPiece.ErrorCode = OutComeCodes.No_Error;
                                }

                                if (piece.CustomInfo != null && piece.CustomInfo.Count > 0)
                                {
                                    mailPieceCustInfo = new List<MailPieceCustomInfo>();
                                    foreach (var item in piece.CustomInfo)
                                    {
                                        mailPieceCustInfo.Add(new MailPieceCustomInfo() { Key = item.Key, MailPieceId = 0, DataType = item.DataType, Value = item.Value });
                                    }
                                }
                            }
                            else
                            {
                                mailPiece.BusinessFriendlyIdName = piece.BusinessFriendlyIdName;
                                mailPiece.BusinessFriendlyIdValue = piece.BusinessFriendlyIdValue;
                                mailPiece.DocumentOrPackageType = piece.DocumentOrPackageType;
                                mailPiece.FileName = piece.DocumentOrPackage.Name;
                                mailPiece.UniqueId = piece.Id;

                                if (piece.RecipientInfo != null)
                                {
                                    if (piece.RecipientInfo.Recipient != null)
                                    {
                                        mailPiece.IsRcptInfoSpecified = true;
                                        mailPiece.RcptFirstName = piece.RecipientInfo.Recipient.FirstName;
                                        mailPiece.RcptLastName = piece.RecipientInfo.Recipient.LastName;
                                        if (piece.RecipientInfo.Corecipient != null)
                                        {
                                            mailPiece.CoRcptFirstName = piece.RecipientInfo.Corecipient.FirstName;
                                            mailPiece.CoRcptLastName = piece.RecipientInfo.Corecipient.LastName;
                                        }
                                    }

                                    if (piece.RecipientInfo.Address != null)
                                    {
                                        mailPiece.RcptAddressStreet = piece.RecipientInfo.Address.Street;
                                        mailPiece.RcptAddressCity = piece.RecipientInfo.Address.City;
                                        mailPiece.RcptAddressState = piece.RecipientInfo.Address.State;
                                        mailPiece.RcptAddressZIP = piece.RecipientInfo.Address.Zip;
                                        mailPiece.RcptPhone = piece.RecipientInfo.Recipient.Phone;
                                    }
                                }

                                if (piece.ReturnInfo != null)
                                {
                                    if (piece.ReturnInfo.Recipient != null)
                                    {
                                        mailPiece.IsReturnInfoSpecified = true;
                                        mailPiece.RetRcptFirstName = piece.ReturnInfo.Recipient.FirstName;
                                        mailPiece.RetRcptLastName = piece.ReturnInfo.Recipient.LastName;
                                    }

                                    if (piece.ReturnInfo.Address != null)
                                    {
                                        mailPiece.RetRcptAddressStreet = piece.ReturnInfo.Address.Street;
                                        mailPiece.RetRcptAddressCity = piece.ReturnInfo.Address.City;
                                        mailPiece.RetRcptAddressState = piece.ReturnInfo.Address.State;
                                        mailPiece.RetRcptAddressZIP = piece.ReturnInfo.Address.Zip;
                                        mailPiece.RetRcptPhone = piece.ReturnInfo.Recipient.Phone;
                                    }
                                }

                                mailPiece.ErrorCode = validationResult.Code.ToString();
                                mailPiece.ErrorDesc = validationResult.ErrorMessage;

                                if (piece.CustomInfo != null && piece.CustomInfo.Count > 0)
                                {
                                    mailPieceCustInfo = new List<MailPieceCustomInfo>();
                                    foreach (var item in piece.CustomInfo)
                                    {
                                        mailPieceCustInfo.Add(new MailPieceCustomInfo() { Key = item.Key, MailPieceId = 0, DataType = item.DataType, Value = item.Value });
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex);
                        }
                        //Insert MailPiece Data and It's custome info
                        mailPiece.Id = mailPieceSvc.InsertBatchMailPieceDetails(mailPiece, mailPieceCustInfo);
                    }

                    commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.ValidationEnd, Common.Now);
                    commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Validated, Common.Now);
                }
            }
        }

        public void AcknowledgeReportSend()
        {
            List<MailBatchDTO> validatedBatches = mailBatchSvc.GetBatchesByStatus(BatchPipelineStatus.Validated);
            if (validatedBatches == null)
            {
                log.Debug("Process batches job queue is empty.");
                return;
            }

            foreach (var mailBatch in validatedBatches)
            {
                Client client = clientSvc.GetClientDetails(mailBatch.ClientId).Single();
                SFTChannel sftChannelDetails = clientSvc.GetSFTChannelDetails(mailBatch.SFTChannelId);

                MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, sftChannelDetails);

                var data = mailBatchSvc.GetMailBatchDetailsByBatchId(mbi.MailBatch.Id);


            }
        }
    }
}
