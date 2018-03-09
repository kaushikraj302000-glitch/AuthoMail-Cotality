using AM.Entities;
using AM.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Core
{
    static class AMCore
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static IBatchService mailBatchSvc = new BatchService();
        private static IClientService clientSvc = new ClientService();
        private static ICommonService commonSvc = new CommonService();
        private static IMailPieceService mailPieceSvc = new MailPieceService();
        private static IDocTypeService docTypeSvc = new DocTypeService();

        internal static void PrepPnSJobs()
        {
            List<MailBatchDTO> mailBatches = mailBatchSvc.GetBatchesByStatus(BatchPipelineStatus.Acknowledged);

            if (mailBatches == null || mailBatches.Count == 0)
            {
                log.Debug("Prep Job queue is empty.");
                return;
            }

            foreach (MailBatchDTO mailBatch in mailBatches)
            {
                SmartDocs doc = new SmartDocs();
                Client client = clientSvc.GetClientDetails(mailBatch.ClientId).Single();
                MailBatchInfo mbi = new MailBatchInfo(mailBatch, client);

                var batchData = mailBatchSvc.GetMailBatchDetailsByBatchId(mbi.MailBatch.Id);

                List<MailPieceDTO> validMailPieces = batchData.MailPieces.Where(c => c.ErrorCode == OutComeCodes.No_Error || c.ErrorCode == null).ToList();
                commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.PreppedStart, Common.Now);
                foreach (MailPieceDTO mailPiece in validMailPieces)
                {
                    try
                    {
                        DocType docType = docTypeSvc.GetDocTypesByDocTypeName(mbi.Client.Id, mailPiece.DocumentOrPackageType.Trim());

                        if (docType == null)
                        {
                            //TODO: Ask to Ravi Sir
                            continue;
                        }

                        DocTypeCoverLetter docTypeLetter = docTypeSvc.GetDocumentCoverLetterTemplates(docType.Id).SingleOrDefault();

                        if (docTypeLetter == null)
                        {
                            //TODO: Ask to Ravi Sir
                            continue;
                        }

                        int mailPieceId = mailPiece.Id;
                        string coverLetterTemplatePath = Path.Combine(Common.CoverTemplateFolder, docTypeLetter.CoverLetterTemplatePath);
                        string pdfFilePath = Path.Combine(mbi.AMRequestPath, mailPiece.FileName);
                        string outputFilePath = Path.Combine(mbi.PnSRequestPath, "{0}_{1}".Fill(mailPieceId, mailPiece.FileName));
                        string coverLetterPath = Path.Combine(mbi.PnSRequestPath, "{0}_CoverLetter.pdf".Fill(mailPieceId));

                        #region Prepare dictionary of placeholders/tags to fill the PDF template.

                        Dictionary<string, string> mailPieceData = new Dictionary<string, string>();
                        mailPieceData.Add("FromAddress", mailPiece.RetRcptAddressStreet + "\n" + mailPiece.RetRcptAddressAptOrSuiteOrOther + "\n" + mailPiece.RetRcptAddressCity + ", " + mailPiece.RetRcptAddressState + ", " + mailPiece.RetRcptAddressZIP);

                        mailPieceData.Add("ToAddress", mailPiece.RcptFirstName + " " + mailPiece.RcptLastName + "\n" + (string.IsNullOrEmpty(mailPiece.RcptAddressAptOrSuiteOrOther) ? mailPiece.RcptAddressStreet : string.Format("{0}\n{1}", mailPiece.RcptAddressStreet, mailPiece.RcptFirstName)) + "\n" + mailPiece.RcptAddressCity + ", " + mailPiece.RcptAddressState + ", " + mailPiece.RcptAddressZIP);

                        mailPieceData.Add("MailPieceId", mailPiece.Id.ToString());
                        mailPieceData.Add("MailPieceZip", mailPiece.RcptAddressZIP);
                        mailPieceData.Add("LoanNbr", mailPiece.BusinessFriendlyIdValue);

                        #endregion

                        if (docType.ShippingCarrier == Carrier.USPS.ToString())
                        {
                            // Create final document using SmartDocs library.
                            bool isDocumentGenerated = doc.GenerateDoc(coverLetterTemplatePath, pdfFilePath, outputFilePath, mailPieceData, docType.ContentScalePercent);

                            if (isDocumentGenerated)
                            {
                                mailPiece.TrackingNo = doc.TrackingCode;
                                mailPiece.ShippingStatus = ShippingStatus.InProgress.ToString();
                            }
                            else
                            {
                                if (doc.ExceptionDetail.Contains("InvalidPdfException"))
                                {
                                    mailPiece.ShippingStatus = ShippingStatus.Error.ToString();
                                    mailPiece.ErrorCode = OutComeCodes.Invalid_PDF;
                                    mailPiece.ErrorDesc = doc.ExceptionDetail;
                                }
                                else
                                {
                                    mailPiece.ShippingStatus = ShippingStatus.Error.ToString();
                                    mailPiece.ErrorCode = OutComeCodes.Unknown_Error;
                                    mailPiece.ErrorDesc = doc.ExceptionDetail;
                                }
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        mailPiece.ShippingStatus = ShippingStatus.Error.ToString();
                        mailPiece.ErrorCode = OutComeCodes.Unknown_Error;
                        mailPiece.ErrorDesc = "{0}\n{1}".Fill(err.Message, err.ToString());
                    }
                    mailPieceSvc.UpdateBatchMailPieceDetails(mailPiece);
                }

                commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Prepped, Common.Now);
                commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.PreppedEnd, Common.Now);
            }
        }

        internal static void SubmitBatchesDocTypeWiseToPCI()
        {
            List<MailBatchDTO> mailBatches = mailBatchSvc.GetBatchesByStatus(BatchPipelineStatus.Prepped);

            if (mailBatches == null || mailBatches.Count == 0)
            {
                log.Debug("Prep Job queue is empty.");
                return;
            }

            foreach (MailBatchDTO mailBatch in mailBatches)
            {
                Client client = clientSvc.GetClientDetails(mailBatch.ClientId).Single();
                MailBatchInfo mbi = new MailBatchInfo(mailBatch, client);

                var batchData = mailBatchSvc.GetMailBatchDetailsByBatchId(mbi.MailBatch.Id);

                List<MailPieceDTO> validMailPieces = batchData.MailPieces.Where(c => c.ErrorCode == OutComeCodes.No_Error || c.ErrorCode == null).ToList();
            }
        }

        internal static void ProcessPnSResponses()
        {
            log.Debug("Swaminarayan");
            //throw new NotImplementedException();
        }
    }
}
