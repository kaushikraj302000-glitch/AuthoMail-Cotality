using AM.Entities;
using AM.Services;
using PCI.Lib;
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
                        string pdfFilePath = Path.Combine(mbi.AMRequestPath, mbi.BatchName, mailPiece.FileName);
                        string outputFilePath = Path.Combine(mbi.PnSRequestPath, "{0}_{1}".Fill(mailPieceId, mailPiece.FileName));

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
                            int pageCount = doc.GenerateDoc(coverLetterTemplatePath, pdfFilePath, outputFilePath, mailPieceData, docType.ContentScalePercent);

                            if (pageCount > 0)
                            {
                                mailPiece.TotalPages = pageCount;
                                mailPiece.TrackingNo = doc.TrackingCode;
                                mailPiece.ShippingStatus = ShippingStatus.InProgress.ToString();
                            }
                            else
                            {
                                if (doc.ExceptionDetail.Contains("InvalidPdfException"))
                                {
                                    mailPiece.ErrorCode = OutComeCodes.Invalid_PDF;
                                }
                                else
                                {
                                    mailPiece.ErrorCode = OutComeCodes.Unknown_Error;
                                }
                                mailPiece.TotalPages = 0;
                                mailPiece.ShippingStatus = ShippingStatus.Error.ToString();
                                mailPiece.ErrorDesc = doc.ExceptionDetail;
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        mailPiece.TotalPages = 0;
                        mailPiece.ShippingStatus = ShippingStatus.Error.ToString();
                        mailPiece.ErrorCode = OutComeCodes.Unknown_Error;
                        mailPiece.ErrorDesc = "{0}\n{1}".Fill(err.Message, err.ToString());
                    }
                    mailPieceSvc.UpdateBatchMailPieceDetails(mailPiece);
                }

                commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Prepped, Common.Now);
                commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, AM.Entities.BatchStatus.InProgress.ToString());
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

                if (validMailPieces.Count == 0)
                {
                    continue;
                }

                commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.SubmittedToShippingStart, Common.Now);
                try
                {
                    List<PrintBatch> printBatches = new List<PrintBatch>();

                    PrintBatch printBatch = null;

                    var docTypes = validMailPieces.Select(row => row.DocumentOrPackageType.Trim()).Distinct();
                    foreach (var documentOrPackageType in docTypes)
                    {
                        DocType docType = docTypeSvc.GetDocTypesByDocTypeName(mbi.Client.Id, documentOrPackageType);

                        if (docType == null)
                        {
                            //TODO: Ask Ravi Sir
                            continue;
                        }

                        printBatch = new PrintBatch();
                        printBatch.AppCode = mbi.Client.AppCode;
                        printBatch.UniqueId = Guid.NewGuid();
                        printBatch.MailBatchId = mbi.MailBatch.Id;
                        printBatch.DocTypeId = docType.Id;
                        printBatch.MailPieces = validMailPieces.Where(c => c.DocumentOrPackageType == documentOrPackageType).ToList();

                        printBatches.Add(printBatch);
                    }

                    if (printBatches.Count == 0)
                    {
                        continue;
                    }

                    foreach (var batch in printBatches)
                    {
                        if (batch.MailPieces.Count == 0)
                        {
                            continue;
                        }

                        mailBatchSvc.InsertPrintBatchDetails(batch);
                        string pnsRootFolder = Common.PnS_SFT_RootDir;
                        string batchName = "{0}_{1}".Fill(mbi.MailBatch.BatchName, batch.DocTypeId);
                        string batchXMLSubmitPath = Path.Combine(pnsRootFolder, mbi.Client.AppCode, batchName + ".xml");
                        string batchGOSubmitPath = Path.Combine(pnsRootFolder, mbi.Client.AppCode, batchName + ".go");
                        string batchSubmitPath = Path.Combine(pnsRootFolder, mbi.Client.AppCode, batchName);

                        //TODO: Add Validation For Directory Full Access Read/Write/Delete

                        if (!Directory.Exists(batchSubmitPath))
                        {
                            Directory.CreateDirectory(batchSubmitPath);
                        }

                        PnSRequest request = new PnSRequest();
                        request.AppCode = batch.AppCode;
                        request.BatchIdentifier = batch.UniqueId;
                        request.BatchName = batchName;
                        request.DocTypeId = batch.DocTypeId;
                        request.DueDate = Common.Today;
                        request.OnValidationErrors = OnValidationErrors.ProcessValidFiles;

                        List<MailPieceData> mailPackage = new List<MailPieceData>();
                        foreach (var piece in batch.MailPieces)
                        {
                            string fileName = "{0}_{1}".Fill(piece.Id, piece.FileName);
                            File.Copy(Path.Combine(mbi.PnSRequestPath, fileName), Path.Combine(batchSubmitPath, fileName), true);

                            MailPieceData mailPieceData = new MailPieceData();
                            mailPieceData.UniqueId = piece.UniqueId;
                            mailPieceData.TotalPages = piece.TotalPages;
                            mailPieceData.PerformQC = piece.IsQCRequested;
                            mailPieceData.PDFFileName = fileName;

                            mailPackage.Add(mailPieceData);
                        }
                        request.MailPackageData = mailPackage;
                        File.WriteAllText(batchXMLSubmitPath, XmlUtil.Serialize<PnSRequest>(request));
                        File.WriteAllText(batchGOSubmitPath, "");
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.SubmittedToShippingEnd, Common.Now);
                }
                commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.SubmittedToShipping, Common.Now);
                commonSvc.UpdatePrintBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.SubmittedToShipping, Common.Now);
                commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.SubmittedToShippingEnd, Common.Now);
            }
        }

        internal static void ProcessPnSResponses()
        {
            log.Debug("Swaminarayan");
            //throw new NotImplementedException();
        }
    }
}
