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
                SFTChannel sftChannelDetails = clientSvc.GetSFTChannelDetails(mailBatch.SFTChannelId);
                MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, sftChannelDetails);

                var batchData = mailBatchSvc.GetMailBatchDetailsByBatchId(mbi.MailBatch.Id);

                List<MailPieceDTO> validMailPieces = batchData.MailPieces.Where(c => c.ErrorCode == OutComeCodes.No_Error || c.ErrorCode == null).ToList();

                foreach (MailPieceDTO mailPiece in validMailPieces)
                {
                    try
                    {
                        int mailPieceId = mailPiece.Id;
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

                        // Create final document using SmartDocs library.
                        bool isDocumentGenerated = doc.GenerateDoc(pdfFilePath, outputFilePath, mailPieceData);

                        if (isDocumentGenerated)
                        {
                            // Update IsDocGenerated flag in DB.
                            loan.OutputPDFFilePath = outputDocumentPath;
                            loan.TrackingNo = doc.TrackingCode;
                            loan.OutcomeCode = OutComeCodes.No_error;
                            loan.DocGeneratedOn = DateTime.UtcNow;
                        }
                        else
                        {
                            // Update error in DB.                        
                            if (doc.ExceptionDetail.Contains("InvalidPdfException"))
                            {
                                loan.OutcomeCode = OutComeCodes.Invalid_PDF_file;
                                loan.ErrorDetail = doc.ExceptionDetail;
                                batch.TotalLoans = batch.TotalLoans - 1;
                            }
                            else
                            {
                                loan.ErrorDetail = doc.ExceptionDetail;
                                loan.OutcomeCode = OutComeCodes.UNKNOWN_error;
                            }

                            Common.LogError(string.Format("Document not generated for loan {0}:\n{1}", loan.LoanNbr, loan.ErrorDetail));
                        }

                        db.SaveChanges();
                    }
                    catch (Exception err)
                    {
                        loan.ErrorDetail = string.Format("{0}|{1}", err.Message, err.ToString());
                        loan.OutcomeCode = OutComeCodes.UNKNOWN_error;
                        db.SaveChanges();
                        Common.LogError(err.ToString(), true);
                    }
                }

            }
        }

        internal static void ProcessPnSResponses()
        {
            log.Debug("Swaminarayan");
            //throw new NotImplementedException();
        }
    }
}
