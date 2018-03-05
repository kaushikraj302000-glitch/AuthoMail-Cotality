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

namespace AM.Console
{
    public class CoreJob
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CoreJob));
        IClientService clientSvc;
        IBatchService batchSvc;
        ICommonService commonSvc;
        public CoreJob()
        {
            clientSvc = new ClientService();
            batchSvc = new BatchService();
            commonSvc = new CommonService();
        }

        public void ParseAuthoMailSchema()
        {
            List<MailBatchDTO> downloadBatches = batchSvc.GetBatchesByStatus(BatchPipelineStatus.Downloaded);
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

                var amXmlFilePath = Path.Combine(mbi.AMRequestPath, mbi.BatchName, mbi.BatchName + "_AuthoMail.xml");

                if (!File.Exists(amXmlFilePath))
                {
                    mbi.MailBatch.ValidationStatus = BatchValidationStatus.Invalid;
                    mbi.MailBatch.ValidationErrors = "No AM XML file is available in {0} batch.".Fill(mbi.MailBatch.BatchName);

                    batchSvc.UpdateBatchDetails(mbi.MailBatch);

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

                    batchSvc.UpdateBatchDetails(mbi.MailBatch);

                    commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, BatchStatus.Failed.ToString());
                    continue;
                }

                if (amParsedBatch != null)
                {
                    mbi.MailBatch.Priority = amParsedBatch.Priority.ToString();
                    mbi.MailBatch.UniqueId = amParsedBatch.Id;
                    mbi.MailBatch.ClientBatchId = amParsedBatch.ClientBatchId;

                    batchSvc.UpdateBatchDetails(mbi.MailBatch);

                    if (amParsedBatch.MailPieces == null || amParsedBatch.MailPieces.Count == 0)
                    {
                        mbi.MailBatch.ValidationStatus = BatchValidationStatus.Invalid;
                        mbi.MailBatch.ValidationErrors = "No mailpiece found in xml for batch : {0}".Fill(mbi.MailBatch.Id);

                        batchSvc.UpdateBatchDetails(mbi.MailBatch);

                        commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, BatchStatus.Failed.ToString());
                        continue;
                    }
                    List<MailPieceDTO> mailPieces = new List<MailPieceDTO>();
                    foreach (var piece in amParsedBatch.MailPieces)
                    {
                        MailPieceDTO mailPiece = new MailPieceDTO();
                        List<MailPieceCustomInfo> mailPieceCustInfo = new List<MailPieceCustomInfo>();

                        mailPiece.BatchId = mbi.MailBatch.Id;

                        ValidationResultAMC validationResult = Common.ValidateAMMailPiecesForUSAA(piece);

                        if (validationResult.Code == OutComeCodes.No_Error)
                        {
                            mailPiece.BusinessFriendlyIdName = piece.BusinessFriendlyIdName;
                            mailPiece.BusinessFriendlyIdValue = piece.BusinessFriendlyIdValue;
                            mailPiece.DocumentOrPackageType = piece.DocumentOrPackageType;
                            mailPiece.FileName = piece.DocumentOrPackage.Name;
                            mailPiece.UniqueId = piece.Id;

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
                            

                        }
                    }
                }
            }
        }
    }
}
