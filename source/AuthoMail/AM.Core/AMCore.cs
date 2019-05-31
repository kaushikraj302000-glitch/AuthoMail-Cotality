using AM.Entities;
using AM.Services;
using PCI.Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;
using System.Threading;
using System.Net.Mail;
using AM.USPSLib;

namespace AM.Core
{
    static class AMCore
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static Random random = new Random();

        #region PrepPnSJob
        internal static void PrepPnSJobs()
        {
            log.Debug("PrepPnSJob started");

            using (IMailBatchService mailBatchSvc = new MailBatchService())
            using (IClientService clientSvc = new ClientService())
            using (ICommonService commonSvc = new CommonService())
            using (IMailPieceService mailPieceSvc = new MailPieceService())
            using (IDocTypeService docTypeSvc = new DocTypeService())
            using (IFedExService fedExSvc = new FedExService())
            {
                List<MailBatchDTO> mailBatches = mailBatchSvc.GetBatchesByPipelineStatusAndClientId(BatchPipelineStatus.Acknowledged);

                if (mailBatches.Count == 0)
                {
                    log.Debug("Prep Job queue is empty.");
                    return;
                }

                foreach (MailBatchDTO mailBatch in mailBatches)
                {
                    try
                    {
                        Client client = clientSvc.GetClientById(mailBatch.ClientId);
                        MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, Config.Common_AuthoMailRootFolder);

                        var batchData = mailBatchSvc.GetMailBatchDetailsByBatchId(mbi.MailBatch.Id);

                        List<MailPieceDTO> validMailPieces = batchData.MailPieces.Where(c => c.ErrorCode == OutComeCodes.No_Error && c.ShippingStatus == ShippingStatus.InProgress.ToString() && string.IsNullOrWhiteSpace(c.TrackingNo)).ToList();
                        List<MailPieceDTO> errorMailPieces = batchData.MailPieces.Where(c => c.ShippingStatus == ShippingStatus.Error.ToString()).ToList();
                        foreach (MailPieceDTO mailPiece in validMailPieces)
                        {
                            int attemptCount = 0;

                            DocType docType = docTypeSvc.GetDocTypeById(mailPiece.DocTypeId.Value);

                            int mailPieceId = mailPiece.Id;

                            string pdfFilePath = Path.Combine(mbi.AMRequestPath, mailPiece.FileName);
                            string outputFilePath = Path.Combine(mbi.PnSRequestPath, "{0}_{1}".Fill(mailPieceId, mailPiece.FileName));
                            List<string> coverLetterPdfPaths = new List<string>();
                            List<string> endLetterPdfPaths = new List<string>();
                            string endPageTemplatePath = string.Empty;


                            #region From Address
                            string fromAddress = string.Empty;
                            if (mailPiece.IsReturnInfoSpecified)
                            {
                                //This is compulsary
                                fromAddress = mailPiece.RetRcptFirstName + " " + mailPiece.RetRcptLastName + "\n";

                                if (!string.IsNullOrEmpty(mailPiece.RetRcptOrganization))
                                    fromAddress += mailPiece.RetRcptOrganization + "\n";
                                if (!string.IsNullOrEmpty(mailPiece.RetRcptAddrOptionalLine1AboveStreet))
                                    fromAddress += mailPiece.RetRcptAddrOptionalLine1AboveStreet + "\n";
                                if (!string.IsNullOrEmpty(mailPiece.RetRcptAddrOptionalLine2AboveStreet))
                                    fromAddress += mailPiece.RetRcptAddrOptionalLine2AboveStreet + "\n";

                                //Because Street address is must field
                                fromAddress += mailPiece.RetRcptAddressStreet + "\n";

                                if (!string.IsNullOrEmpty(mailPiece.RetRcptAddrOptionalLine1BelowStreet))
                                    fromAddress += mailPiece.RetRcptAddrOptionalLine1BelowStreet + "\n";
                                if (!string.IsNullOrEmpty(mailPiece.RetRcptAddrOptionalLine2BelowStreet))
                                    fromAddress += mailPiece.RetRcptAddrOptionalLine2BelowStreet + "\n";
                                if (!string.IsNullOrEmpty(mailPiece.RetRcptAddressAptOrSuiteOrOther))
                                    fromAddress += mailPiece.RetRcptAddressAptOrSuiteOrOther + "\n";

                                //Because City,state,zip are must field for all client
                                fromAddress += mailPiece.RetRcptAddressCity + " " + mailPiece.RetRcptAddressState + " " + mailPiece.RetRcptAddressZIP;
                            }
                            else if (mbi.MailBatch.IsReturnInfoSpecified)
                            {
                                fromAddress = mbi.MailBatch.RetRcptFirstName + " " + mbi.MailBatch.RetRcptLastName + "\n";

                                if (!string.IsNullOrEmpty(mbi.MailBatch.RetRcptOrganization))
                                    fromAddress += mbi.MailBatch.RetRcptOrganization + "\n";
                                if (!string.IsNullOrEmpty(mbi.MailBatch.RetRcptAddrOptionalLine1AboveStreet))
                                    fromAddress += mbi.MailBatch.RetRcptAddrOptionalLine1AboveStreet + "\n";
                                if (!string.IsNullOrEmpty(mailPiece.RetRcptAddrOptionalLine2AboveStreet))
                                    fromAddress += mbi.MailBatch.RetRcptAddrOptionalLine2AboveStreet + "\n";

                                //Because Street address is must field
                                fromAddress += mbi.MailBatch.RetRcptAddressStreet + "\n";

                                if (!string.IsNullOrEmpty(mbi.MailBatch.RetRcptAddrOptionalLine1BelowStreet))
                                    fromAddress += mbi.MailBatch.RetRcptAddrOptionalLine1BelowStreet + "\n";
                                if (!string.IsNullOrEmpty(mbi.MailBatch.RetRcptAddrOptionalLine2BelowStreet))
                                    fromAddress += mbi.MailBatch.RetRcptAddrOptionalLine2BelowStreet + "\n";
                                if (!string.IsNullOrEmpty(mbi.MailBatch.RetRcptAddressAptOrSuiteOrOther))
                                    fromAddress += mbi.MailBatch.RetRcptAddressAptOrSuiteOrOther + "\n";

                                fromAddress += mbi.MailBatch.RetRcptAddressCity + " " + mbi.MailBatch.RetRcptAddressState + " " + mbi.MailBatch.RetRcptAddressZIP;
                            }
                            else if (docType.IsReturnInfoSpecified)
                            {
                                fromAddress = docType.RetRcptFirstName + " " + docType.RetRcptLastName + "\n";

                                if (!string.IsNullOrEmpty(docType.RetRcptOrganization))
                                    fromAddress += docType.RetRcptOrganization + "\n";
                                if (!string.IsNullOrEmpty(docType.RetRcptAddrOptionalLine1AboveStreet))
                                    fromAddress += docType.RetRcptAddrOptionalLine1AboveStreet + "\n";
                                if (!string.IsNullOrEmpty(docType.RetRcptAddrOptionalLine2AboveStreet))
                                    fromAddress += docType.RetRcptAddrOptionalLine2AboveStreet + "\n";

                                //Because Street address is must field
                                fromAddress += docType.RetRcptAddressStreet + "\n";

                                if (!string.IsNullOrEmpty(docType.RetRcptAddrOptionalLine1BelowStreet))
                                    fromAddress += docType.RetRcptAddrOptionalLine1BelowStreet + "\n";
                                if (!string.IsNullOrEmpty(docType.RetRcptAddrOptionalLine2BelowStreet))
                                    fromAddress += docType.RetRcptAddrOptionalLine2BelowStreet + "\n";
                                if (!string.IsNullOrEmpty(docType.RetRcptAddressAptOrSuiteOrOther))
                                    fromAddress += docType.RetRcptAddressAptOrSuiteOrOther + "\n";

                                fromAddress += docType.RetRcptAddressCity + " " + docType.RetRcptAddressState + " " + docType.RetRcptAddressZIP;
                            }
                            else
                            {
                                //TODO: Generate error
                            }
                            #endregion

                            #region To Address
                            string toAddress = string.Empty;
                            if (mailPiece.IsRcptInfoSpecified)
                            {
                                //This is compulsary
                                toAddress = mailPiece.RcptFirstName + " " + mailPiece.RcptLastName + "\n";

                                if (!string.IsNullOrEmpty(mailPiece.RcptOrganization))
                                    toAddress += mailPiece.RcptOrganization + "\n";
                                if (!string.IsNullOrEmpty(mailPiece.RcptAddrOptionalLine1AboveStreet))
                                    toAddress += mailPiece.RcptAddrOptionalLine1AboveStreet + "\n";
                                if (!string.IsNullOrEmpty(mailPiece.RcptAddrOptionalLine2AboveStreet))
                                    toAddress += mailPiece.RcptAddrOptionalLine2AboveStreet + "\n";

                                //Because Street address is must field
                                toAddress += mailPiece.RcptAddressStreet + "\n";

                                if (!string.IsNullOrEmpty(mailPiece.RcptAddrOptionalLine1BelowStreet))
                                    toAddress += mailPiece.RcptAddrOptionalLine1BelowStreet + "\n";
                                if (!string.IsNullOrEmpty(mailPiece.RcptAddrOptionalLine2BelowStreet))
                                    toAddress += mailPiece.RcptAddrOptionalLine2BelowStreet + "\n";
                                if (!string.IsNullOrEmpty(mailPiece.RcptAddressAptOrSuiteOrOther))
                                    toAddress += mailPiece.RcptAddressAptOrSuiteOrOther + "\n";

                                //Because City,state,zip are must field for all client
                                toAddress += mailPiece.RcptAddressCity + " " + mailPiece.RcptAddressState + " " + mailPiece.RcptAddressZIP;
                            }
                            #endregion

                            #region Prepare dictionary of placeholders/tags to fill the PDF template.
                            Dictionary<string, string> mailPieceData = new Dictionary<string, string>();
                            mailPieceData.Add("FromAddress", fromAddress);
                            mailPieceData.Add("ToAddress", toAddress);
                            mailPieceData.Add("MailPieceId", mailPieceId.ToString());
                            mailPieceData.Add("Date", Common.Now.UtcToEst().ToString("MMMM dd, yyyy"));//USAA Specific
                            mailPieceData.Add("Free3OF9Barcode", "*" + mailPiece.Id.ToString() + "*");//Barcode for non-IMBarcode cover letter
                            mailPieceData.Add("UniqueId", "*" + mailPieceId.ToString() + "*");//Humanreadable code which is used for barcode
                            mailPieceData.Add("MailPieceZip", mailPiece.RcptAddressZIP);
                            mailPieceData.Add("LoanNbr", mailPiece.BusinessFriendlyIdValue);
                            #endregion

                            List<DocTypeCoverLetter> docTypeLetters = docTypeSvc.GetDocumentCoverLetterTemplates(docType.Id).OrderBy(o => o.SeqId).ToList();

                            var frontCoverLetters = docTypeLetters.Where(seq => seq.SeqId <= 100);
                            var rearCoverLetters = docTypeLetters.Where(seq => seq.SeqId > 100);

                            SmartDocs doc = new SmartDocs();

                            var docTypeLetter = frontCoverLetters.SingleOrDefault();
                            //foreach (var docTypeLetter in frontCoverLetters)
                            //{
                            //Folder structure and file naming convention should be Root/AppCode/AppCode_DocTypeCode_SeqNo.pdf
                            string coverLetterTemplatePath = docTypeLetter == null ? string.Empty : Path.Combine(Config.Core_CoverTemplateFolder, client.AppCode, docTypeLetter.CoverLetterTemplatePath);

                            SmartDocResponse response = new SmartDocResponse();

                            if (docType.ShippingCarrier == Carrier.USPS.ToString())
                            {
                                if (docType.ShippingServiceType == AM.USPSLib.ServiceType.PRIORITY_EXPRESS.ToString())
                                {
                                    int reTrycnt = 0;
                                    while (reTrycnt < Config.Core_NoofTime_AttemptFedexService)
                                    {
                                        eVSLabelRequest eVSReq = GenerateEVSLabelRequest(mailPiece, client.Name);

                                        response = doc.GenerateeVSPriorityExpressCoverLetter(client.AppCode, commonSvc.GetSystemUserId(), mbi.PnSRequestPath, mailPiece.Id.ToString(), eVSReq);

                                        if (!response.HasError)
                                        {
                                            //if we have service changed to priority then we are updating doctypeid and its reletad data in database also
                                            if (response.ShippingServiceType == AM.USPSLib.ServiceType.PRIORITY)
                                            {
                                                string priorityDocType = mailPiece.DocumentOrPackageType + Config.Monitor_PriorityMailingText;

                                                var docTypePRI = docTypeSvc.GetDocTypeByDocTypeName(client.Id, priorityDocType);

                                                if (docTypePRI != null)
                                                {
                                                    var sweepTimings = commonSvc.GetSweepTimings();

                                                    mailPiece.DocTypeId = docTypePRI.Id;

                                                    mailPiece.EffectiveReceivedOnEst = commonSvc.GetEffectiveEstReceivedOn(docTypePRI, mailBatch.ReceivedOn.Value);

                                                    //Calculate Due Date for Mail Pieces doc type wise
                                                    mailPiece.DueDate = commonSvc.GetDueDate(docTypePRI, mailPiece.EffectiveReceivedOnEst.Value);

                                                    //Set print location where mail piece will go for print and ship
                                                    mailPiece.PrintTrailId = commonSvc.GetPrintTrail(docTypePRI, mailPiece.EffectiveReceivedOnEst.Value);

                                                    var printVendorDetails = commonSvc.GetPrintVendorDetailsByPrintTrailId(mailPiece.PrintTrailId.Value);
                                                    mailPiece.PrintLocation = printVendorDetails.Name;

                                                    mailPiece.StdSubmissionTime = commonSvc.GetStandardSubmissionTime(client.AppCode, printVendorDetails.VendorCode, mailBatch.ReceivedOn.Value, sweepTimings);

                                                    mailPiece.MaxSubmissionTime = commonSvc.GetMaximumSubmissionTime(docTypePRI, mailPiece.EffectiveReceivedOnEst.Value);
                                                }
                                                else
                                                {
                                                    throw new Exception("Priority document type does not exist.");
                                                }
                                            }
                                            break;
                                        }
                                        else if (response.HasError && (response.ExceptionCode.Contains("80040B18") || response.ExceptionDetail.Contains("An error has occurred with the service.  Please contact the system administrator, or try again.") || response.ExceptionDetail.Contains("A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond")))
                                        {
                                            System.Threading.Thread.Sleep(Config.Core_FedexServiceAttemptInterval);
                                            reTrycnt++;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }
                                else if (docType.ShippingServiceType == AM.USPSLib.ServiceType.PRIORITY.ToString())
                                {
                                    int reTrycnt = 0;
                                    while (reTrycnt < Config.Core_NoofTime_AttemptFedexService)
                                    {
                                        eVSLabelRequest eVSReq = GenerateEVSLabelRequest(mailPiece, client.Name);
                                        eVSReq.ServiceType = AM.USPSLib.ServiceType.PRIORITY;
                                        response = doc.GenerateeVSPriorityCoverLetter(client.AppCode, commonSvc.GetSystemUserId(), mbi.PnSRequestPath, mailPiece.Id.ToString(), eVSReq);

                                        if (response.HasError && (response.ExceptionCode.Contains("80040B18") || response.ExceptionDetail.Contains("An error has occurred with the service.  Please contact the system administrator, or try again.") || response.ExceptionDetail.Contains("A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond")))
                                        {
                                            System.Threading.Thread.Sleep(Config.Core_FedexServiceAttemptInterval);
                                            reTrycnt++;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    //Create Cover letter with IMBarcode - USPS First Class
                                    response = doc.GenerateUSPSIMBCoverLetter(client.AppCode, coverLetterTemplatePath, mbi.PnSRequestPath, mailPieceData);
                                }

                                if (response.HasError)
                                {
                                    mailPiece.ErrorCode = OutComeCodes.Unknown_Error;
                                    mailPiece.TotalPages = 0;
                                    mailPiece.ShippingStatus = ShippingStatus.Error.ToString();
                                    mailPiece.ErrorDesc = response.ExceptionDetail;
                                    mailPiece.DueDate = null;
                                    mailPiece.PrintLocation = null;
                                    mailPiece.PrintTrailId = null;
                                    mailPieceSvc.UpdateBatchMailPieceDetails(mailPiece);
                                    continue;
                                }

                                coverLetterPdfPaths.Add(response.CoverLetterPath);
                                mailPiece.TrackingNo = response.TrackingCode;
                                mailPiece.ShippingCost = response.PostageCost;
                            }
                            else if (docType.ShippingCarrier == Carrier.FedEx.ToString())
                            {
                                #region  Call FedEx web service and get create shipping label

                                FedExAccountInfo fedExAccInfo = fedExSvc.GetFedExAccountDetailsByAccountCode(docType.ShippingAccountCode);

                                CreateFedExLabel createFedExLabel = new CreateFedExLabel();
                                response = new SmartDocResponse();
                                while (attemptCount < Config.Core_NoofTime_AttemptFedexService)
                                {
                                    response = createFedExLabel.Create(mailPiece, fedExAccInfo, docType, mbi);
                                    if (!response.HasError)
                                    {
                                        coverLetterPdfPaths.Add(response.CoverLetterPath);
                                        mailPiece.TrackingNo = response.TrackingCode;
                                        mailPiece.ShippingCost = response.PostageCost;
                                        break;
                                    }
                                    else if (response.HasError && (response.ExceptionCode == "3022" || response.ExceptionCode == "1000" || response.ExceptionCode == "2998" || response.ExceptionCode == "Unknown_Error" || response.ExceptionDetail.Contains("General Failure")))
                                    {
                                        System.Threading.Thread.Sleep(Config.Core_FedexServiceAttemptInterval);
                                        attemptCount++;

                                        //Setting as an error suppose FedEx is failing continueously then System will consider as an unknown error for that mail piece
                                        mailPiece.ShippingStatus = ShippingStatus.Error.ToString();
                                        mailPiece.ErrorCode = OutComeCodes.Unknown_Error;
                                        mailPiece.ErrorDesc = string.Format("{0}|{1}", OutComeCodes.Unknown_Error, response.ExceptionDetail);
                                    }
                                    else
                                    {
                                        if (response.HasError && (response.ExceptionCode == "3051" || response.ExceptionCode == "3049" || response.ExceptionCode == "3052" || response.ExceptionDetail.Contains("Recipient Postal Code not Served") || response.ExceptionDetail.Contains("Recipient Postal Code not found") || response.ExceptionDetail.Contains("Recipient Postal-State Mismatch")))
                                        {
                                            //First try to generate eVS label with USPS Priority Express
                                            string priorityExpressDocType = mailPiece.DocumentOrPackageType + Config.Monitor_ExpressMailingText;

                                            docType = docTypeSvc.GetDocTypeByDocTypeName(client.Id, priorityExpressDocType);
                                            if (docType != null)
                                            {
                                                mailPiece.DocTypeId = docType.Id;
                                            }

                                            eVSLabelRequest eVSReq = GenerateEVSLabelRequest(mailPiece, client.Name);

                                            response = doc.GenerateeVSPriorityExpressCoverLetter(client.AppCode, commonSvc.GetSystemUserId(), mbi.PnSRequestPath, mailPiece.Id.ToString(), eVSReq);

                                            //if eVS has changed service to USPS Priority then AM is updating doctypeid in database
                                            if (response.ShippingServiceType == AM.USPSLib.ServiceType.PRIORITY)
                                            {
                                                string priorityDocType = mailPiece.DocumentOrPackageType + Config.Monitor_PriorityMailingText;

                                                docType = docTypeSvc.GetDocTypeByDocTypeName(client.Id, priorityDocType);
                                                if (docType != null)
                                                {
                                                    mailPiece.DocTypeId = docType.Id;
                                                }
                                            }

                                            if (response.HasError)
                                            {
                                                mailPiece.ErrorCode = OutComeCodes.Unknown_Error;
                                                mailPiece.ShippingStatus = ShippingStatus.Error.ToString();
                                                mailPiece.ErrorDesc = response.ExceptionDetail;
                                                break;
                                            }

                                            var sweepTimings = commonSvc.GetSweepTimings();

                                            coverLetterPdfPaths.Add(response.CoverLetterPath);
                                            mailPiece.TrackingNo = response.TrackingCode;
                                            mailPiece.ShippingCost = response.PostageCost;
                                            mailPiece.EffectiveReceivedOnEst = commonSvc.GetEffectiveEstReceivedOn(docType, mailBatch.ReceivedOn.Value);

                                            //Calculate Due Date for Mail Pieces doc type wise
                                            mailPiece.DueDate = commonSvc.GetDueDate(docType, mailPiece.EffectiveReceivedOnEst.Value);

                                            //Set print location where mail piece will go for print and ship
                                            mailPiece.PrintTrailId = commonSvc.GetPrintTrail(docType, mailPiece.EffectiveReceivedOnEst.Value);

                                            var printVendorDetails = commonSvc.GetPrintVendorDetailsByPrintTrailId(mailPiece.PrintTrailId.Value);
                                            mailPiece.PrintLocation = printVendorDetails.Name;

                                            mailPiece.StdSubmissionTime = commonSvc.GetStandardSubmissionTime(client.AppCode, printVendorDetails.VendorCode, mailBatch.ReceivedOn.Value, sweepTimings);

                                            mailPiece.MaxSubmissionTime = commonSvc.GetMaximumSubmissionTime(docType, mailPiece.EffectiveReceivedOnEst.Value);
                                            break;
                                        }


                                        mailPiece.ShippingStatus = ShippingStatus.Error.ToString();
                                        mailPiece.ErrorCode = OutComeCodes.Invalid_ShippingDetail;
                                        mailPiece.ErrorDesc = string.Format("{0}|{1}", OutComeCodes.Invalid_ShippingDetail, response.ExceptionDetail);
                                        break;
                                    }
                                }

                                if (response.HasError)
                                {
                                    mailPiece.TotalPages = 0;
                                    mailPiece.PrintLocation = null;
                                    mailPiece.PrintTrailId = null;
                                    mailPiece.DueDate = null;
                                    //NOTE : If result false then mark as an error and update status for that mail piece
                                    mailPieceSvc.UpdateBatchMailPieceDetails(mailPiece);
                                    continue;
                                }

                                #endregion
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(coverLetterTemplatePath))
                                {
                                    response = doc.GenerateFrontBarcodeCoverLetter(coverLetterTemplatePath, mbi.PnSRequestPath, mailPieceData);
                                    if (response.HasError)
                                    {
                                        mailPiece.ErrorCode = OutComeCodes.Unknown_Error;
                                        mailPiece.TotalPages = 0;
                                        mailPiece.ShippingStatus = ShippingStatus.Error.ToString();
                                        mailPiece.ErrorDesc = response.ExceptionDetail;
                                        mailPiece.DueDate = null;
                                        mailPiece.PrintLocation = null;
                                        mailPiece.PrintTrailId = null;
                                        mailPieceSvc.UpdateBatchMailPieceDetails(mailPiece);
                                        continue;
                                    }

                                    mailPiece.TrackingNo = mailPiece.Id.ToString();
                                    coverLetterPdfPaths.Add(response.CoverLetterPath);
                                }

                            }
                            //}

                            foreach (var rearCoverLetter in rearCoverLetters)
                            {
                                endPageTemplatePath = Path.Combine(Config.Core_CoverTemplateFolder, client.AppCode, rearCoverLetter.CoverLetterTemplatePath);
                                //For now reverse barcoding will be mail piece id
                                mailPieceData.Add("Free3OF9ReverseBarcode", "*" + new string((mailPiece.TrackingNo).Reverse().ToArray()) + "*");

                                response = doc.GenerateLastPageWithBarcode(endPageTemplatePath, mbi.PnSRequestPath, mailPieceData);

                                if (response.HasError)
                                {
                                    mailPiece.ErrorCode = OutComeCodes.Unknown_Error;
                                    mailPiece.TotalPages = 0;
                                    mailPiece.ShippingStatus = ShippingStatus.Error.ToString();
                                    mailPiece.ErrorDesc = response.ExceptionDetail;
                                    mailPiece.DueDate = null;
                                    mailPiece.PrintLocation = null;
                                    mailPiece.PrintTrailId = null;
                                    mailPieceSvc.UpdateBatchMailPieceDetails(mailPiece);
                                    continue;
                                }

                                endLetterPdfPaths.Add(response.CoverLetterPath);
                            }

                            response = doc.GenerateDoc(coverLetterPdfPaths, pdfFilePath, outputFilePath, docType.ContentScalePercent, docType.NoOfCopies, endLetterPdfPaths);

                            if (response.HasError)
                            {
                                mailPiece.ErrorCode = OutComeCodes.Invalid_PDF_File;
                                mailPiece.TotalPages = 0;
                                mailPiece.DueDate = null;
                                mailPiece.PrintTrailId = null;
                                mailPiece.PrintLocation = null;
                                mailPiece.ShippingStatus = ShippingStatus.Error.ToString();
                                mailPiece.ErrorDesc = response.ExceptionDetail;
                            }

                            mailPiece.TotalPages = response.TotalPages;
                            mailPiece.TotalSheets = docType.PrintFormat == "Duplex" ? Convert.ToInt32(Math.Ceiling(mailPiece.TotalPages / 2f)) : mailPiece.TotalPages;

                            mailPieceSvc.UpdateBatchMailPieceDetails(mailPiece);

                        }

                        try
                        {
                            Deliverable reportDeliverable = commonSvc.GetDeliverableByReportType(client.Id, ReportTypes.Error);

                            List<MailPieces> mailPieces;
                            using (IReportService reportSvc = new ReportService())
                            {
                                mailPieces = reportSvc.GetMailPiecesDataFromMailBatchId(mbi.MailBatch.Id);
                            }

                            if (mailPieces != null)
                            {
                                var validMailPiecesList = mailPieces.Where(c => c.ErrorCode == OutComeCodes.No_Error && c.ShippingStatus == ShippingStatus.InProgress.ToString()).ToList();

                                if (validMailPiecesList.Count == 0)
                                {
                                    //TODO: Remove after proper logic implementation client wise switch case implemented
                                    GenerateImmidiateReports(mbi, mailPieces, reportDeliverable);

                                    mailBatchSvc.MarkMailBatchAsInvalid(mbi.MailBatch.Id, (mbi.MailBatch.ValidationErrors + Environment.NewLine + "Some documents could not be processed."));
                                    commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, AM.Entities.BatchStatus.Failed.ToString());
                                }
                                else if (validMailPiecesList.Count < mailPieces.Count)
                                {
                                    var errMpIds = mailPieces.Select(mp => mp.Id).Except(errorMailPieces.Select(errMp => errMp.Id));
                                    mailPieces = mailPieces.Where(mp => errMpIds.Contains(mp.Id)).ToList();
                                    GeneratePartialReports(mbi, mailPieces, reportDeliverable);
                                    commonSvc.UpdateBatchValidationStatus(mbi.MailBatch.Id, BatchValidationStatus.PartiallyValid, mailBatch.ValidationErrors);
                                    commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Prepped, Common.Now);
                                }
                                else
                                {
                                    commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Prepped, Common.Now);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            var msg = "Error occured while preparing batches for printing for Batch Id - {0} and error - {1}".Fill(mbi.MailBatch.Id, ex.Message);
                            log.Error(msg, ex);
                            SendUnexpectedExceptionEmail(msg);
                        }
                    }
                    catch (Exception ex)
                    {
                        var msg = "Error occured in PrepPnSJobs Method For Mail Batch Id: {0} with Error : {1}".Fill(mailBatch.Id, ex.Message);
                        log.Error(msg, ex);
                        SendUnexpectedExceptionEmail(msg);
                        continue;
                    }
                }
            }
        }

        private static void GeneratePartialReports(MailBatchInfo mbi, List<MailPieces> mailPieces, Deliverable deliverable)
        {
            SFTChannel sftChannel;
            using (IClientService clientSvc = new ClientService())
            using (IMailBatchService mailBatchSvc = new MailBatchService())
            using (ICommonService commonSvc = new CommonService())
            {
                sftChannel = clientSvc.GetSFTChannelDetailsBySFTChannelId(mbi.MailBatch.SFTChannelId);

                switch (sftChannel.Code.Split('_')[0])
                {
                    case "AMC":
                        using (IAMCService amcSvc = new AMCService())
                        {
                            string errorGoFileName = "ErrReport.go";
                            string errorGoFilePath = Path.Combine(mbi.AMResponsePath, errorGoFileName);
                            string ftpErrorGoUploadPath = Path.Combine(deliverable.UploadPath, errorGoFileName);

                            string errorReportFileName = "Error-{0}.txt".Fill(Common.Now.ToString("yyyyMMdd-HHmmss"));
                            //TODO: Comment below and uncomment above if don't want to move new naming convention and tab delimited report to Production environment
                            //string errorReportFileName = "Error-{0}-{1}-{2}-{3}.txt".Fill(mbi.BatchName, mbi.Client.AppCode, mbi.MailBatch.Id, Common.Now.ToString("yyyyMMdd-HHmmss"));
                            string localErrorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);
                            string ftpErrorReportPath = Path.Combine(deliverable.UploadPath, errorReportFileName);


                            StringBuilder errorData = amcSvc.AMCErrorReport(mailPieces);
                            if (errorData != null)
                            {
                                File.WriteAllText(localErrorReportPath, errorData.ToString());

                                SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(sftChannel.Protocol), sftChannel.Host, sftChannel.UserName, sftChannel.Password, 300000);
                                sftSite.Connect();

                                sftSite.UploadFile(localErrorReportPath, ftpErrorReportPath);

                                //NOTE: Temparary Code for Duplicate Report Upload 
                                if (sftSite.IsDownloadDirectoryAvailable(Config.Monitor_AMCBackupReportUploadPath))
                                {
                                    sftSite.UploadFile(localErrorReportPath, Path.Combine(Config.Monitor_AMCBackupReportUploadPath, errorReportFileName));
                                }

                                // Upload Error.GO file after uploading error report
                                File.WriteAllText(errorGoFilePath, string.Empty);
                                sftSite.UploadFile(errorGoFilePath, ftpErrorGoUploadPath);

                                if (sftSite.IsConnected)
                                    sftSite.Disconnect();

                                if (File.Exists(localErrorReportPath))
                                    File.Move(localErrorReportPath, Path.ChangeExtension(localErrorReportPath, ".Uploaded"));
                            }
                        }
                        break;
                    default:
                        using (IGeneralService generalSvc = new GeneralService())
                        {
                            string errorGoFileName = "ErrReport.go";
                            string errorGoFilePath = Path.Combine(mbi.AMResponsePath, errorGoFileName);
                            string ftpErrorGoUploadPath = Path.Combine(deliverable.UploadPath, errorGoFileName);

                            string errorReportFileName = "Error-{0}-{1}-{2}-{3}.txt".Fill(mbi.BatchName, mbi.Client.AppCode, mbi.MailBatch.Id, Common.Now.ToString("yyyyMMdd-HHmmss"));
                            string localErrorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);
                            string ftpErrorReportPath = Path.Combine(deliverable.UploadPath, errorReportFileName);

                            StringBuilder errorData = generalSvc.GeneralErrorReport(mailPieces, deliverable.Delimeter);
                            if (errorData != null)
                            {
                                File.WriteAllText(localErrorReportPath, errorData.ToString());

                                SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(sftChannel.Protocol), sftChannel.Host, sftChannel.UserName, sftChannel.Password, 300000);
                                sftSite.Connect();

                                sftSite.UploadFile(localErrorReportPath, ftpErrorReportPath);

                                //NOTE: Temparary Code for Duplicate Report Upload 
                                if (sftSite.IsDownloadDirectoryAvailable(Config.Monitor_AMCBackupReportUploadPath))
                                {
                                    sftSite.UploadFile(localErrorReportPath, Path.Combine(Config.Monitor_AMCBackupReportUploadPath, errorReportFileName));
                                }

                                // Upload Error.GO file after uploading error report
                                File.WriteAllText(errorGoFilePath, string.Empty);
                                sftSite.UploadFile(errorGoFilePath, ftpErrorGoUploadPath);

                                if (sftSite.IsConnected)
                                    sftSite.Disconnect();

                                if (File.Exists(localErrorReportPath))
                                    File.Move(localErrorReportPath, Path.ChangeExtension(localErrorReportPath, ".Uploaded"));
                            }
                        }
                        break;
                }
            }
        }

        private static eVSLabelRequest GenerateEVSLabelRequest(MailPieceDTO mailPiece, string clientName)
        {
            eVSLabelRequest eVSReq = new eVSLabelRequest();
            //General settings
            eVSReq.AllowNonCleansedDestAddr = true;
            eVSReq.AllowNonCleansedOriginAddr = true;
            eVSReq.ServiceType = AM.USPSLib.ServiceType.PRIORITY_EXPRESS;
            eVSReq.Container = Container.FLAT_RATE_ENVELOPE;
            eVSReq.ShipDate = mailPiece.EffectiveReceivedOnEst.Value;
            eVSReq.ReceiptOption = ReceiptOption.SEPARATE_PAGE;
            eVSReq.WeightInOunces = "48";
            eVSReq.ImageType = ImageType.PDF;
            eVSReq.POZipCode = "75099";
            eVSReq.SenderName = clientName;
            if (mailPiece.RcptAddressTypes.Contains(AddressType.Military.ToString()))
                eVSReq.LabelParameters = new LabelParameters() { Sequence = new LabelSequence() { PackageNumber = 1, TotalPackages = 1 }, Size = LabelSize.LABEL4X6L };
            else
                eVSReq.LabelParameters = new LabelParameters() { XCoordinate = 450, YCoordinate = 500, Sequence = new LabelSequence() { PackageNumber = 1, TotalPackages = 1 }, Size = LabelSize.LABEL4X6P_PAGE };

            // From Address
            eVSReq.CustomerRefNo = mailPiece.Id.ToString();
            eVSReq.FromName = (mailPiece.RetRcptFirstName + " " + mailPiece.RetRcptLastName).Trim();
            eVSReq.FromFirm = mailPiece.RetRcptOrganization ?? clientName;
            eVSReq.FromStreetAddress = (mailPiece.RetRcptAddressStreet + " " + mailPiece.RetRcptAddressAptOrSuiteOrOther + " " + mailPiece.RetRcptAddrOptionalLine1BelowStreet).Trim();
            eVSReq.FromCity = mailPiece.RetRcptAddressCity;
            eVSReq.FromState = mailPiece.RetRcptAddressState;
            eVSReq.FromZip4 = mailPiece.RetRcptAddressZIP.SeparateZip4And5(4);
            eVSReq.FromZip5 = mailPiece.RetRcptAddressZIP.SeparateZip4And5(5);
            eVSReq.FromPhone = string.IsNullOrWhiteSpace(mailPiece.RetRcptPhone) ? new string('0', 10) : mailPiece.RcptPhone.RemoveFormatting();

            // To Address
            eVSReq.ToFirm = string.IsNullOrEmpty(mailPiece.RcptOrganization) ? string.Empty : mailPiece.RcptOrganization;
            eVSReq.ToName = (mailPiece.RcptFirstName + " " + mailPiece.RcptLastName).Trim();
            eVSReq.ToStreetAddress = (mailPiece.RcptAddressStreet + " " + mailPiece.RcptAddressAptOrSuiteOrOther + " " + mailPiece.RcptAddrOptionalLine1BelowStreet).Trim();
            eVSReq.ToCity = mailPiece.RcptAddressCity;
            eVSReq.ToState = mailPiece.RcptAddressState;
            eVSReq.ToZip4 = mailPiece.RcptAddressZIP.SeparateZip4And5(4);
            eVSReq.ToZip5 = mailPiece.RcptAddressZIP.SeparateZip4And5(5);
            eVSReq.ToPhone = string.IsNullOrWhiteSpace(mailPiece.RcptPhone) ? new string('0', 10) : mailPiece.RcptPhone.RemoveFormatting();

            //Based on address types needs to specify some custom changes in request
            eVSReq.POBox = mailPiece.RcptAddressTypes.Contains(AddressType.POBOX.ToString());

            if (mailPiece.RcptAddressTypes.Contains(AddressType.Military.ToString()) || mailPiece.RcptAddressTypes.Contains(AddressType.Territory.ToString()))
            {
                eVSCustomsFormsData customesData = new eVSCustomsFormsData()
                {
                    ShippingContents = new List<ItemDetail>()
                    {
                        new ItemDetail()
                        {
                            CountryOfOrigin = "United States", Description = "Loan Document", Quantity = 1, NetPounds = 3, NetOunces = 0, Value = 10
                        }
                    },
                    CustomsContentType = CustomsContentType.DOCUMENTS
                };

                eVSReq.eVSCustomsFormsData = customesData;
            }

            return eVSReq;
        }

        private static void SendUnexpectedExceptionEmail(string exception)
        {
            try
            {
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = true;
                        msg.To.Add(Config.Monitor_Exception_Email_To);
                        msg.CC.Add(Config.Monitor_Exception_Email_Cc);
                        msg.Subject = "{0} : {1}".Fill(Config.Common_Environment, Config.Monitor_ExceptionEmail_Subject);
                        msg.Priority = MailPriority.High;
                        msg.Body = "Unexpected exception occured during processing documents for print and ship." + Environment.NewLine + exception;
                        smtpClient.Send(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private static void GenerateImmidiateReports(MailBatchInfo mbi, List<MailPieces> mailPieces, Deliverable deliverable)
        {
            SFTChannel sftChannel;
            using (IClientService clientSvc = new ClientService())
            using (IMailBatchService mailBatchSvc = new MailBatchService())
            using (ICommonService commonSvc = new CommonService())
            {
                sftChannel = clientSvc.GetSFTChannelDetailsBySFTChannelId(mbi.MailBatch.SFTChannelId);

                switch (sftChannel.Code.Split('_')[0])
                {
                    case "AMC":
                        using (IAMCService amcSvc = new AMCService())
                        {
                            string errorGoFileName = "ErrReport.go";
                            string errorGoFilePath = Path.Combine(mbi.AMResponsePath, errorGoFileName);

                            string errorReportFileName = "Error-{0}.txt".Fill(Common.Now.ToString("yyyyMMdd-HHmmss"));
                            //TODO: Comment below and uncomment above if don't want to move new naming convention and tab delimited report to Production environment
                            //string errorReportFileName = "Error-{0}-{1}-{2}-{3}.txt".Fill(mbi.BatchName, mbi.Client.AppCode, mbi.MailBatch.Id, Common.Now.ToString("yyyyMMdd-HHmmss"));

                            string errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);

                            StringBuilder errorData = amcSvc.AMCErrorReport(mailPieces);
                            if (errorData != null)
                            {
                                File.WriteAllText(errorReportPath, errorData.ToString());

                                commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportPrepped, Common.Now);
                            }
                        }
                        break;
                    case "ALASKA":
                        using (IAlaskaService alaskaSvc = new AlaskaService())
                        {
                            string errorReportFileName = "ER_{0}.csv".Fill(mbi.BatchName);
                            string errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);
                            StringBuilder errorData = alaskaSvc.AlaskaErrorReport(mailPieces, mbi.Client.TimeZone, deliverable.Delimeter);
                            if (errorData != null)
                            {
                                File.WriteAllText(errorReportPath, errorData.ToString());
                            }
                            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportPrepped, Common.Now);
                        }
                        break;
                    case "HSBC":
                        using (IHSBCService hsbcSvc = new HSBCService())
                        {
                            string errorReportFileName = "{0}.ErrorReport_{1}.csv".Fill(mbi.BatchName.Split('[')[0], mbi.MailBatch.ReceivedOn.Value.UtcToClientTimeZone(mbi.Client.TimeZone).ToString("htt"));
                            string errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);
                            StringBuilder errorData = hsbcSvc.HSBCErrorReport(mailPieces, mbi.Client.TimeZone, deliverable.Delimeter);
                            if (errorData != null)
                            {
                                File.WriteAllText(errorReportPath, errorData.ToString());
                            }
                            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportPrepped, Common.Now);
                        }
                        break;
                    default:
                        using (IGeneralService generalSvc = new GeneralService())
                        {
                            string errorGoFileName = "ErrReport.go";
                            string errorGoFilePath = Path.Combine(mbi.AMResponsePath, errorGoFileName);

                            string errorReportFileName = "Error-{0}-{1}-{2}-{3}.txt".Fill(mbi.BatchName, mbi.Client.AppCode, mbi.MailBatch.Id, Common.Now.ToString("yyyyMMdd-HHmmss"));

                            string errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);

                            StringBuilder errorData = generalSvc.GeneralErrorReport(mailPieces, deliverable.Delimeter);
                            if (errorData != null)
                            {
                                File.WriteAllText(errorReportPath, errorData.ToString());

                                commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportPrepped, Common.Now);
                            }
                        }
                        break;
                }
            }
        }

        #endregion

        #region SubmitBatchesDocTypeWise
        internal static void SubmitBatchesDocTypeWiseToPrintVendor()
        {
            using (IMailBatchService mailBatchSvc = new MailBatchService())
            using (IClientService clientSvc = new ClientService())
            using (ICommonService commonSvc = new CommonService())
            using (IMailPieceService mailPieceSvc = new MailPieceService())
            {
                log.Debug("Submitted Batches job started.");

                List<MailBatchDTO> mailBatches = mailBatchSvc.GetBatchesByPipelineStatusAndClientId(BatchPipelineStatus.Prepped);

                foreach (MailBatchDTO mailBatch in mailBatches)
                {
                    try
                    {
                        Client client = clientSvc.GetClientById(mailBatch.ClientId);
                        MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, Config.Common_AuthoMailRootFolder);

                        var batchData = mailBatchSvc.GetMailBatchDetailsByBatchId(mbi.MailBatch.Id);

                        GeneratePrintBatches(batchData);

                        //After processing mail batch for PrintBatches we will check for each and every mailpieces are having printbatchid in it or not then update SubmittedToShipping status in MailBatches table
                        var mailPiecesList = mailPieceSvc.GetMailPiecesByMailBatchId(mailBatch.Id);
                        var validPieces = mailPiecesList.Count(c => c.ErrorCode == OutComeCodes.No_Error && c.PrintBatchId != null);
                        var errorPieces = mailPiecesList.Count(c => c.ErrorCode != OutComeCodes.No_Error && c.PrintBatchId == null);
                        var earlyPulledBackPieces = mailPiecesList.Count(c => c.ErrorCode == OutComeCodes.No_Error && c.PrintBatchId == null && c.IsPulledBack);

                        if (mailPiecesList.Count != (validPieces + errorPieces + earlyPulledBackPieces))
                        {
                            return;
                        }

                        commonSvc.UpdateBatchPipeLineStatus(mailBatch.Id, BatchPipelineStatus.SubmittedToShipping, Common.Now);
                        //TODO : Logic add for checking submitted
                        //TODO : Also add logic for received response, check incremental reporting also
                    }
                    catch (Exception ex)
                    {
                        var msg = "Error occured in SubmitBatchesDocTypeWiseToPrintVendor Method For Mail Batch Id: {0} with Error : {1}".Fill(mailBatch.Id, ex.Message);
                        log.Error(msg, ex);
                        SendUnexpectedExceptionEmail(msg);
                        continue;
                    }
                }
            }
        }

        private static void GeneratePrintBatches(BatchData mailBatchData)
        {
            List<MailPieceDTO> validMailPieces = mailBatchData.MailPieces.Where(c => c.ErrorCode == OutComeCodes.No_Error && c.ShippingStatus == ShippingStatus.InProgress.ToString() && c.PrintBatchId == null).ToList();

            if (validMailPieces.Count == 0)
                return;

            using (IMailBatchService mailBatchSvc = new MailBatchService())
            using (IClientService clientSvc = new ClientService())
            using (ICommonService commonSvc = new CommonService())
            using (IMailPieceService mailPieceSvc = new MailPieceService())
            using (IDocTypeService docTypeSvc = new DocTypeService())
            {
                List<PrintBatch> printBatches = new List<PrintBatch>();

                var docTypes = validMailPieces.Select(row => row.DocTypeId.Value).Distinct();
                foreach (var docTypeId in docTypes)
                {
                    DocType docType = docTypeSvc.GetDocTypeById(docTypeId);

                    var filteredMailPiecesDocTypeWise = validMailPieces.Where(c => c.DocTypeId == docTypeId).ToList();

                    #region QC Request Logic
                    if (docType.QCPercent > 0)
                    {
                        double qcCount = 0;
                        float qcPercentage = docType.QCPercent / 100;
                        qcCount = Math.Ceiling(filteredMailPiecesDocTypeWise.Count * qcPercentage);

                        var randomIndexes = new List<int>();
                        while (randomIndexes.Count < qcCount)
                        {
                            var index = random.Next(0, filteredMailPiecesDocTypeWise.Count - 1);
                            if (!randomIndexes.Contains(index))
                            {
                                randomIndexes.Add(index);
                            }
                        }

                        randomIndexes.ForEach(index =>
                        {
                            filteredMailPiecesDocTypeWise[index].IsQCRequested = true;
                        });
                    }
                    #endregion

                    var printTrailIds = filteredMailPiecesDocTypeWise.Select(c => c.PrintTrailId).Distinct();

                    foreach (var printTrailId in printTrailIds)
                    {
                        PrintBatch printBatch = new PrintBatch();
                        printBatch.MailBatchId = mailBatchData.MailBatch.Id;
                        printBatch.AppCode = clientSvc.GetClientById(docType.ClientId).AppCode;
                        printBatch.UniqueId = Guid.NewGuid();
                        printBatch.DocTypeId = docType.Id;
                        printBatch.PrintTrailId = printTrailId.Value;
                        printBatch.PrintLocation = commonSvc.GetPrintVendorDetailsByPrintTrailId(printTrailId.Value).Name;
                        printBatch.MailPieces = filteredMailPiecesDocTypeWise.Where(c => c.PrintTrailId == printTrailId).ToList();

                        printBatches.Add(printBatch);
                    }
                }

                string pnsRootFolder = Config.Core_PnS_SFT_RootDir;

                foreach (var printBatchData in printBatches)
                {
                    //Insert data into PrintBatches table and update PrintBatchId in MailPieces table
                    printBatchData.Id = mailBatchSvc.InsertPrintBatchDetails(printBatchData);

                    //Batch naming convention will be AppCode_PrintBatchId for Batch folder, XML and Go file
                    string batchName = "{0}_{1}".Fill(printBatchData.AppCode, printBatchData.Id);
                    string xmlFileName = batchName + ".xml";
                    string goFileName = batchName + ".go";

                    string remoteXMLPath = Path.Combine(pnsRootFolder, printBatchData.AppCode, xmlFileName);
                    string remoteGOPath = Path.Combine(pnsRootFolder, printBatchData.AppCode, goFileName);
                    string remoteBatchPath = Path.Combine(pnsRootFolder, printBatchData.AppCode, batchName);

                    remoteBatchPath.CleanDirectory();

                    PnSRequest request = new PnSRequest();
                    request.AppCode = printBatchData.AppCode;
                    request.BatchIdentifier = printBatchData.UniqueId;
                    request.BatchName = batchName;
                    request.DocTypeId = printBatchData.DocTypeId;
                    request.PrintTrailId = printBatchData.PrintTrailId;
                    request.DueDate = printBatchData.MailPieces.OrderBy(x => x.DueDate).Select(x => x.DueDate).Distinct().First().Value.Date;
                    request.PrintLocation = printBatchData.PrintLocation;
                    request.OnValidationErrors = OnValidationErrors.ProcessValidFiles;

                    List<MailPieceData> mailPackage = new List<MailPieceData>();
                    foreach (var piece in printBatchData.MailPieces)
                    {
                        string batchPnSRequestPath = Path.Combine(Config.Common_AuthoMailRootFolder, printBatchData.AppCode, piece.MailBatchId.ToString(), "PnSRequest");
                        string fileName = "{0}_{1}".Fill(piece.Id, piece.FileName);
                        string localFilePath = Path.Combine(batchPnSRequestPath, fileName);
                        string remoteFilePath = Path.Combine(remoteBatchPath, fileName);

                        File.Copy(localFilePath, remoteFilePath, true);

                        MailPieceData mailPieceData = new MailPieceData();
                        mailPieceData.UniqueId = piece.UniqueId;
                        mailPieceData.TotalPages = piece.TotalPages;
                        mailPieceData.PerformQC = piece.IsQCRequested;
                        mailPieceData.PDFFileName = fileName;
                        mailPackage.Add(mailPieceData);
                    }
                    request.MailPackageData = mailPackage;

                    File.WriteAllText(remoteXMLPath, request.ToXml());
                    File.WriteAllText(remoteGOPath, "");

                    commonSvc.UpdatePrintBatchStatus(printBatchData.Id, AM.Entities.BatchStatus.InProgress.ToString());
                    commonSvc.UpdatePrintBatchPipeLineStatus(printBatchData.Id, BatchPipelineStatus.SubmittedToShipping, Common.Now);
                }
            }
        }
        #endregion

        internal static void GetSubmittedPnSResponses()
        {
            using (IMailBatchService mailBatchSvc = new MailBatchService())
            using (IClientService clientSvc = new ClientService())
            using (ICommonService commonSvc = new CommonService())
            {
                log.Debug("Get submitted job started.");
                var submittedPrintBatches = mailBatchSvc.GetSubmittedPrintBatchDetails();

                if (submittedPrintBatches.Count == 0)
                {
                    log.Debug("Response download Job queue is empty.");
                    return;
                }

                try
                {
                    var clientAppCodes = submittedPrintBatches.Select(client => client.AppCode).Distinct();
                    foreach (var appCode in clientAppCodes)
                    {
                        Client client = clientSvc.GetClientByAppCode(appCode);

                        List<string> responseFiles = Directory.EnumerateFiles(Path.Combine(Config.Core_PnS_SFT_RootDir, client.AppCode), "*.rpt").ToList();

                        var responseTriggerFiles = responseFiles
                        .Select(file => new KeyValuePair<string, string>(Path.GetFileNameWithoutExtension(file), file))
                        .ToList();

                        foreach (var triggerFile in responseTriggerFiles)
                        {
                            string pnsResponseFilePath = Path.Combine(Config.Core_PnS_SFT_RootDir, client.AppCode, triggerFile.Key + "-res.xml");
                            if (File.Exists(pnsResponseFilePath))
                            {
                                submittedPrintBatches.ForEach(row =>
                                {
                                    if (row.SubmittedBatchName == triggerFile.Key)
                                    {
                                        row.ResponseFilePath = pnsResponseFilePath;
                                        row.ResponseTriggerFilePath = triggerFile.Value;
                                    }
                                });
                            }
                        }
                    }

                    var downloadableFiles = submittedPrintBatches.Where(c => !string.IsNullOrEmpty(c.ResponseFilePath)).ToList();

                    if (downloadableFiles.Count == 0)
                    {
                        log.Debug("Response files are not available to download.");
                        return;
                    }

                    foreach (var downloadableBatch in downloadableFiles)
                    {
                        string responseTriggerFilePath = downloadableBatch.ResponseTriggerFilePath;
                        string responseFileName = Path.GetFileName(downloadableBatch.ResponseFilePath);
                        string responseFilePath = downloadableBatch.ResponseFilePath;

                        string amPnSResponseDir = Path.Combine(Config.Common_AuthoMailRootFolder, downloadableBatch.AppCode, Util.PnSResponsesFolderName);
                        string amPnSResponseFilePath = Path.Combine(amPnSResponseDir, responseFileName);

                        DateTime mrdfDownloadStartTime = Common.Now;
                        DateTime responseReceivedOn = File.GetLastWriteTimeUtc(responseFilePath);

                        if (!Directory.Exists(amPnSResponseDir))
                            Directory.CreateDirectory(amPnSResponseDir);

                        File.Copy(responseFilePath, amPnSResponseFilePath, true);

                        if (File.Exists(responseTriggerFilePath))
                            File.Delete(responseTriggerFilePath);

                        if (File.Exists(amPnSResponseFilePath))
                        {
                            //After checking response is copied to path system will remove response file from remote location
                            File.Delete(responseFilePath);

                            commonSvc.UpdatePrintBatchPipeLineStatus(downloadableBatch.PrintBatchId, BatchPipelineStatus.ShippingResponseReceived, responseReceivedOn);
                        }
                    }
                }
                catch (Exception ex)
                {
                    var errorMsg = "Error occured while downloading response from PnS Service : {0} ".Fill(ex);
                    log.Error(errorMsg, ex);
                    SendUnexpectedExceptionEmail(errorMsg);
                }
            }
        }

        internal static void ProcessPnSResponses()
        {
            log.Debug("Process PnS response job is started.");

            using (IMailBatchService mailBatchSvc = new MailBatchService())
            using (IClientService clientSvc = new ClientService())
            using (ICommonService commonSvc = new CommonService())
            using (IMailPieceService mailPieceSvc = new MailPieceService())
            using (IReportService reportSvc = new ReportService())
            {

                var responseReceivedPrintBatches = mailBatchSvc.GetResponseReceivedPrintBatchDetails();

                if (responseReceivedPrintBatches.Count == 0)
                {
                    log.Debug("Response process job queue is empty.");
                    return;
                }

                foreach (var printBatch in responseReceivedPrintBatches)
                {
                    try
                    {
                        var printBatchMailPieces = reportSvc.GetMailPiecesDataFromPrintBatchId(printBatch.PrintBatchId);

                        // Here we have changed path of pnsresponse to process from "AMRoot/ClientCode/PnSResponses/AppCode_PrintBatchId-res.xml" because now we have multiple batches mail pieces in printbatch table

                        string printbatchResponseFilePath = Path.Combine(Config.Common_AuthoMailRootFolder, printBatch.AppCode, Util.PnSResponsesFolderName, printBatch.SubmittedBatchName + "-res.xml");

                        var pnsResponse = PnSResponse.LoadXml(File.ReadAllText(printbatchResponseFilePath));

                        foreach (var mailPackage in pnsResponse.MailPackageReport)
                        {
                            printBatchMailPieces.ForEach(row =>
                            {
                                if (row.UniqueId == mailPackage.UniqueId)
                                {
                                    if (mailPackage.MailPieceStatus == MailPieceStatus.Error)
                                    {
                                        row.ErrorCode = Enum.ToObject(typeof(ErrorTypes), mailPackage.ErrId).ToString();
                                        row.ErrorDesc = mailPackage.ErrDesc;
                                        row.MissedSLAReason = MissedSLAReason.NA;
                                    }
                                    else if (mailPackage.MailPieceStatus == MailPieceStatus.PulledBack)
                                    {
                                        row.PulledBackOn = mailPackage.PulledBackOn;
                                        row.MissedSLAReason = MissedSLAReason.NA;
                                    }
                                    else if (mailPackage.MailPieceStatus == MailPieceStatus.Shipped)
                                    {
                                        row.PrintedOn = mailPackage.PrintedOn;
                                        row.ShippedOn = mailPackage.ShippedOn;
                                        row.ShippingCost = mailPackage.PostageCost != 0.00M ? mailPackage.PostageCost : row.ShippingCost;
                                        row.Weight = mailPackage.WeightMeasured;
                                        row.IsQCPerformed = mailPackage.IsQCPerformed;
                                        row.MissedSLAReason = GetMissedSLAReason(row);
                                    }
                                    row.ShippingStatus = mailPackage.MailPieceStatus.ToString();

                                    mailPieceSvc.UpdateBatchMailPieceShippingStatusDetails(row);
                                }
                            });
                        }

                        //Updates PrintBatches Status values
                        commonSvc.UpdatePrintBatchStatus(printBatch.PrintBatchId, pnsResponse.BatchStatus.ToString(), pnsResponse.StatusDescription);

                        var increamentalReportingChannels = Config.Core_DirectReportBackChannelCodes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                        increamentalReportingChannels = increamentalReportingChannels.Select(c => c + "_" + Config.Common_Environment).ToList();

                        string channelCode = clientSvc.GetSFTChannelDetailsBySFTChannelId(printBatch.SFTChannelId).Code;

                        MailBatches mailBatch = reportSvc.GetMailBatchDataFromMailBatchId(printBatch.MailBatchId);

                        if (increamentalReportingChannels.Contains(channelCode))
                        {
                            string amResponsePath = Path.Combine(Config.Common_AuthoMailRootFolder, printBatch.AppCode, Util.AMResponseFolderName, printBatch.PrintBatchId.ToString());

                            amResponsePath.CreateDirectoryIfNotExist();

                            switch (channelCode.Split('_')[0].ToUpper())
                            {
                                case "AMC":
                                    using (IAMCService amcSvc = new AMCService())
                                    {
                                        StringBuilder deliveryReport = amcSvc.AMCDeliveryReport(printBatchMailPieces);

                                        if (deliveryReport != null)
                                        {
                                            string deliveryReportFileName = "Delivery-{0}.txt".Fill(Common.Now.ToString("yyyyMMdd-HHmmss"));

                                            //TODO: Comment below and uncomment above if don't want to move new naming convention and tab delimited report to Production environment
                                            //string deliveryReportFileName = "Delivery-{0}-{1}-{2}-{3}.txt".Fill(mbi.BatchName, mbi.Client.AppCode, mbi.MailBatch.Id, Common.Now.ToString("yyyyMMdd-HHmmss"));
                                            string deliveryReportPath = Path.Combine(amResponsePath, deliveryReportFileName);

                                            File.WriteAllText(deliveryReportPath, deliveryReport.ToString());
                                        }
                                    }
                                    Thread.Sleep(1000);


                                    break;
                                default:
                                    var deliverables = commonSvc.GetDeliverableByReportType(mailBatch.ClientId.Value, ReportTypes.Delivery);
                                    using (IGeneralService generalSvc = new GeneralService())
                                    {
                                        StringBuilder deliveryReport = generalSvc.GeneralDeliveryReport(printBatchMailPieces, deliverables.Delimeter);

                                        if (deliveryReport != null)
                                        {
                                            string deliveryReportFileName = "Delivery-{0}-{1}-{2}-{3}.txt".Fill(mailBatch.BatchName, mailBatch.Clients.AppCode, mailBatch.Id, Common.Now.ToString("yyyyMMdd-HHmmss"));
                                            string deliveryReportPath = Path.Combine(amResponsePath, deliveryReportFileName);

                                            File.WriteAllText(deliveryReportPath, deliveryReport.ToString());
                                        }
                                    }
                                    break;
                            }
                            mailBatchSvc.UpdatePrintBatchPipelineStatus(printBatch.PrintBatchId, BatchPipelineStatus.ReportPrepped, Common.Now);
                        }

                        var mailPiecesData = mailPieceSvc.GetMailPiecesByMailBatchId(printBatch.MailBatchId);
                        var pendingMailPieces = mailPiecesData.Count(data => data.ShippingStatus == ShippingStatus.InProgress.ToString());

                        if (pendingMailPieces == 0)
                        {
                            if (mailPiecesData.All(data => data.ShippingStatus == ShippingStatus.Shipped.ToString()))
                            {
                                commonSvc.UpdateBatchStatus(printBatch.MailBatchId, AM.Entities.BatchStatus.ShippedAll.ToString());
                            }
                            else if (mailPiecesData.All(data => data.ShippingStatus == ShippingStatus.PulledBack.ToString()))
                            {
                                commonSvc.UpdateBatchStatus(printBatch.MailBatchId, AM.Entities.BatchStatus.PulledBack.ToString());
                            }
                            else if (mailPiecesData.All(data => data.ShippingStatus == ShippingStatus.Error.ToString()))
                            {
                                commonSvc.UpdateBatchStatus(printBatch.MailBatchId, AM.Entities.BatchStatus.Failed.ToString());
                            }
                            else
                            {
                                commonSvc.UpdateBatchStatus(printBatch.MailBatchId, AM.Entities.BatchStatus.ShippedPartial.ToString());
                            }

                            commonSvc.UpdateBatchPipeLineStatus(printBatch.MailBatchId, BatchPipelineStatus.ShippingResponseReceived, Common.Now);
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMsg = "Error occured while processing PnS response for PrintBatchId : {0} And Error : {1} ".Fill(printBatch.PrintBatchId, ex);
                        log.Error(errorMsg, ex);
                        SendUnexpectedExceptionEmail(errorMsg);
                    }
                }
            }
            log.Debug("Process PnS response job is completed.");
        }

        private static string GetMissedSLAReason(MailPieces mailPiece)
        {
            if (mailPiece.ShippedOn <= mailPiece.DueDate)
                return MissedSLAReason.NA;
            string vendorCode;
            BatchSummary pnsBatchSummary;
            using (IMailPieceService mailPieceSvc = new MailPieceService())
            {
                pnsBatchSummary = mailPieceSvc.GetBatchSummaryDetailByUniqueId(mailPiece.UniqueId);
            }
            using (ICommonService commonSvc = new CommonService())
            {
                vendorCode = commonSvc.GetPrintVendorDetailsByPrintTrailId(mailPiece.PrintTrailId.Value).VendorCode;
            }

            if ((pnsBatchSummary.SubmittedOn - mailPiece.MaxSubmissionTime.Value).TotalMinutes > Config.Common_ProcessingBufferTime)
                return MissedSLAReason.AM;

            if (vendorCode == "CL_DALLAS")
                return MissedSLAReason.DALLAS_CL;

            if (vendorCode == "PCI")
                return MissedSLAReason.PCI;

            return MissedSLAReason.AM;
        }

        #region ReportPreppedProcess
        internal static void ReportPreppedProcess()
        {
            log.Debug("Report prepped job is started.");

            using (IMailBatchService mailBatchSvc = new MailBatchService())
            using (IClientService clientSvc = new ClientService())
            using (ICommonService commonSvc = new CommonService())
            using (IAMCService amcSvc = new AMCService())
            {
                List<MailBatchDTO> resReceivedBatches = mailBatchSvc.GetBatchesByPipelineStatusAndClientId(BatchPipelineStatus.ShippingResponseReceived);
                if (resReceivedBatches.Count == 0)
                {
                    log.Debug("Report prepped job queue is empty.");
                    return;
                }

                foreach (var mailBatch in resReceivedBatches)
                {
                    try
                    {
                        Client client = clientSvc.GetClientById(mailBatch.ClientId);

                        List<Deliverable> deliverables = commonSvc.GetDeliverablesByClientId(client.Id);

                        MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, Config.Common_AuthoMailRootFolder);
                        SFTChannel sftChannel = clientSvc.GetSFTChannelDetailsBySFTChannelId(mbi.MailBatch.SFTChannelId);
                        List<MailPieces> mailPieces;
                        using (IReportService reportSvc = new ReportService())
                        {
                            mailPieces = reportSvc.GetMailPiecesDataFromMailBatchId(mbi.MailBatch.Id);
                        }

                        if (mailPieces != null)
                        {
                            try
                            {
                                bool isReportGenerated = false;

                                isReportGenerated = GenerateFinalReports(mbi, mailPieces, deliverables, sftChannel);

                                if (isReportGenerated)
                                    commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportPrepped, Common.Now);

                                //TODO: Remove after proper naming convention
                                Thread.Sleep(1000);
                            }
                            catch (Exception ex)
                            {
                                var errorMsg = "Error occured while generating reports : {0}".Fill(ex);
                                log.Error(errorMsg, ex);
                                SendUnexpectedExceptionEmail(errorMsg);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        var msg = "Error occured in ReportPreppedProcess Method For Mail Batch Id: {0} with Error : {1}".Fill(mailBatch.Id, ex.Message);
                        log.Error(msg, ex);
                        SendUnexpectedExceptionEmail(msg);
                        continue;
                    }
                }
            }

            log.Debug("Report prepped job is completed.");
        }

        private static bool GenerateFinalReports(MailBatchInfo mbi, List<MailPieces> mailPieces, List<Deliverable> deliverables, SFTChannel sftChannel)
        {
            bool isReportGenerated = false;
            string sftChannelCode = sftChannel.Code.Split('_')[0].ToUpper();
            switch (sftChannelCode)
            {
                case "ALASKA":
                    using (IAlaskaService alaskaSvc = new AlaskaService())
                    {
                        var deliverayReportDeliverable = deliverables.Single(r => r.ReportType == ReportTypes.Delivery);
                        StringBuilder deliveryReport = alaskaSvc.AlaskaDeliveryReport(mailPieces, mbi.Client.TimeZone, deliverayReportDeliverable.Delimeter);

                        if (deliveryReport != null)
                        {
                            string deliveryReportFileName = "DR_{0}.csv".Fill(mbi.BatchName);
                            string deliveryReportPath = Path.Combine(mbi.AMResponsePath, deliveryReportFileName);

                            File.WriteAllText(deliveryReportPath, deliveryReport.ToString());
                            isReportGenerated = true;
                        }

                        var errorReportDeliverable = deliverables.Single(r => r.ReportType == ReportTypes.Error);
                        StringBuilder errorReport = alaskaSvc.AlaskaErrorReport(mailPieces, mbi.Client.TimeZone, errorReportDeliverable.Delimeter);

                        if (errorReport != null)
                        {
                            string errorReportFileName = "ER_{0}.csv".Fill(mbi.BatchName);
                            string errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);

                            File.WriteAllText(errorReportPath, errorReport.ToString());
                            isReportGenerated = true;
                        }
                    }
                    break;
                case "HSBC":
                    using (IHSBCService hsbcSvc = new HSBCService())
                    {
                        var deliverayReportDeliverable = deliverables.Single(r => r.ReportType == ReportTypes.Delivery);
                        StringBuilder deliveryReport = hsbcSvc.HSBCDeliveryReport(mailPieces, mbi.Client.TimeZone, deliverayReportDeliverable.Delimeter);

                        string deliveryReportFileName = "{0}.DeliveryReport_{1}.csv".Fill(mbi.BatchName.Split('[')[0], mbi.MailBatch.ReceivedOn.Value.UtcToClientTimeZone(mbi.Client.TimeZone).ToString("htt"));
                        string deliveryReportPath = Path.Combine(mbi.AMResponsePath, deliveryReportFileName);

                        File.WriteAllText(deliveryReportPath, deliveryReport.ToString());


                        var errorReportDeliverable = deliverables.Single(r => r.ReportType == ReportTypes.Delivery);
                        StringBuilder errorReport = hsbcSvc.HSBCErrorReport(mailPieces, mbi.Client.TimeZone, errorReportDeliverable.Delimeter);

                        string errorReportFileName = "{0}.ErrorReport_{1}.csv".Fill(mbi.BatchName.Split('[')[0], mbi.MailBatch.ReceivedOn.Value.UtcToClientTimeZone(mbi.Client.TimeZone).ToString("htt"));
                        string errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);

                        File.WriteAllText(errorReportPath, errorReport.ToString());


                        isReportGenerated = true;
                    }
                    break;
                case "AMC":
                case "AMTEST":
                default:
                    var inProgressMailPiece = mailPieces.Where(m => m.ShippingStatus == ShippingStatus.InProgress.ToString()).Count();

                    if (inProgressMailPiece == 0)
                    {
                        switch (sftChannelCode)
                        {
                            case "AMC":
                                using (IAMCService amcSvc = new AMCService())
                                {
                                    var pulledBackMailPieces = mailPieces.Where(m => (m.ShippingStatus == ShippingStatus.PulledBack.ToString() && m.PrintBatchId == null)).ToList();
                                    StringBuilder deliveryReport = amcSvc.AMCDeliveryReport(pulledBackMailPieces);

                                    if (deliveryReport != null)
                                    {
                                        string deliveryReportFileName = "Delivery-{0}.txt".Fill(Common.Now.ToString("yyyyMMdd-HHmmss"));

                                        //TODO: Comment below and uncomment above if don't want to move new naming convention and tab delimited report to Production environment
                                        //string deliveryReportFileName = "Delivery-{0}-{1}-{2}-{3}.txt".Fill(mbi.BatchName, mbi.Client.AppCode, mbi.MailBatch.Id, Common.Now.ToString("yyyyMMdd-HHmmss"));
                                        string deliveryReportPath = Path.Combine(mbi.AMResponsePath, deliveryReportFileName);

                                        File.WriteAllText(deliveryReportPath, deliveryReport.ToString());
                                    }

                                    StringBuilder errorReport = amcSvc.AMCErrorReport(mailPieces);
                                    if (errorReport != null)
                                    {
                                        string errorReportFileName = "Error-{0}.txt".Fill(Common.Now.ToString("yyyyMMdd-HHmmss"));

                                        //TODO: Comment below and uncomment above if don't want to move new naming convention and tab delimited report to Production environment
                                        //string errorReportFileName = "Error-{0}-{1}-{2}-{3}.txt".Fill(mbi.BatchName, mbi.Client.AppCode, mbi.MailBatch.Id, Common.Now.ToString("yyyyMMdd-HHmmss"));
                                        string errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);

                                        File.WriteAllText(errorReportPath, errorReport.ToString());
                                    }
                                }
                                break;
                            default:
                                using (IGeneralService generalSvc = new GeneralService())
                                {
                                    var deliverayReportDeliverable = deliverables.Single(r => r.ReportType == ReportTypes.Delivery);

                                    var pulledBackMailPieces = mailPieces.Where(m => (m.ShippingStatus == ShippingStatus.PulledBack.ToString() && m.PrintBatchId == null)).ToList();
                                    StringBuilder deliveryReport = generalSvc.GeneralDeliveryReport(pulledBackMailPieces, deliverayReportDeliverable.Delimeter);
                                    if (deliveryReport != null)
                                    {
                                        string deliveryReportFileName = "Delivery-{0}-{1}-{2}-{3}.txt".Fill(mbi.BatchName, mbi.Client.AppCode, mbi.MailBatch.Id, Common.Now.ToString("yyyyMMdd-HHmmss"));
                                        string deliveryReportPath = Path.Combine(mbi.AMResponsePath, deliveryReportFileName);

                                        File.WriteAllText(deliveryReportPath, deliveryReport.ToString());
                                    }

                                    var errorReportDeliverable = deliverables.Single(r => r.ReportType == ReportTypes.Error);
                                    StringBuilder errorReport = generalSvc.GeneralErrorReport(mailPieces, errorReportDeliverable.Delimeter);

                                    if (errorReport != null)
                                    {
                                        string errorReportFileName = "Error-{0}-{1}-{2}-{3}.txt".Fill(mbi.BatchName, mbi.Client.AppCode, mbi.MailBatch.Id, Common.Now.ToString("yyyyMMdd-HHmmss"));
                                        string errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);

                                        File.WriteAllText(errorReportPath, errorReport.ToString());
                                    }
                                }
                                break;
                        }
                        isReportGenerated = true;
                    }
                    break;
            }
            return isReportGenerated;
        }
        #endregion
    }
}
