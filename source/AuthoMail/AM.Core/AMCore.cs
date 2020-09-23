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
using System.IO.Compression;

namespace AM.Core
{
    static class AMCore
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static Random random = new Random();
        private static readonly List<string> fallBackCodes = new List<string>() { "3051", "3049", "3052" };
        private static readonly List<string> fallBackDescriptions = new List<string>() { "Recipient Postal Code not Served", "Recipient Postal Code not found", "Recipient Postal-State Mismatch" };
        private static readonly List<string> fedExExceptionCodes = new List<string>() { "3022", "1000", "2998", "Unknown_Error" };
        private static readonly List<string> fedExExceptionDescriptions = new List<string>() { "General Failure" };
        private static readonly List<string> uspsExceptionCodes = new List<string>() { "80040B18", "Unknown_Error" };
        private static readonly List<string> uspsExceptionDescriptions = new List<string>() { "error has occurred with the service", "connected party did not properly respond", "existing connection was forcibly closed" };

        #region PrepPnSJob
        internal static bool PrepPnSJobs()
        {
            log.Debug("PrepPnSJob started");

            using (IMailBatchService mailBatchService = new MailBatchService())
            {
                List<MailBatchDTO> mailBatches = mailBatchService.GetBatchesByPipelineStatusAndClientId(BatchPipelineStatus.Acknowledged);

                if (mailBatches.Count == 0)
                {
                    log.Debug("Prep Job queue is empty.");
                    return false;
                }
                Parallel.ForEach(mailBatches, (mailBatch) =>
                {
                    using (IMailBatchService mailBatchSvc = new MailBatchService())
                    using (IClientService clientSvc = new ClientService())
                    using (ICommonService commonSvc = new CommonService())
                    using (IMailPieceService mailPieceSvc = new MailPieceService())
                    using (IDocTypeService docTypeSvc = new DocTypeService())
                    using (IFedExService fedExSvc = new FedExService())
                    {
                        try
                        {
                            Client client = clientSvc.GetClientById(mailBatch.ClientId);
                            MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, Config.Common_AuthoMailRootFolder);

                            var batchData = mailBatchSvc.GetMailBatchDetailsByBatchId(mbi.MailBatch.Id);

                            List<MailPieceDTO> validMailPieces = batchData.MailPieces.Where(c => c.ErrorCode == OutComeCodes.No_Error && c.ShippingStatus == ShippingStatus.InProgress.ToString() && ((string.IsNullOrWhiteSpace(c.TrackingNo)) || c.IsPrintReady)).ToList();
                            List<MailPieceDTO> errorMailPieces = batchData.MailPieces.Where(c => c.ShippingStatus == ShippingStatus.Error.ToString()).ToList();
                            foreach (MailPieceDTO mailPiece in validMailPieces)
                            {
                                int attemptCount = 0;

                                DocType docType = docTypeSvc.GetDocTypeById(mailPiece.DocTypeId.Value, mailPiece.PnSConfigDetailId.Value);

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
                                mailPieceData.Add("FromAddress", fromAddress.Trim());
                                mailPieceData.Add("ToAddress", toAddress.Trim());
                                mailPieceData.Add("MailPieceId", mailPieceId.ToString());
                                mailPieceData.Add("Date", Common.Now.UtcToEst().ToString("MMMM dd, yyyy"));//USAA Specific
                                mailPieceData.Add("Free3OF9Barcode", "*" + mailPiece.Id.ToString() + "*");//Barcode for non-IMBarcode cover letter
                                mailPieceData.Add("UniqueId", "*" + mailPieceId.ToString() + "*");//Humanreadable code which is used for barcode
                                mailPieceData.Add("MailPieceZip", mailPiece.RcptAddressZIP);
                                mailPieceData.Add("LoanNbr", mailPiece.BusinessFriendlyIdValue);
                                #endregion

                                List<PnSConfigDetailCoverLetter> coverLetters = docTypeSvc.GetDocumentCoverLetterTemplates(mailPiece.PnSConfigDetailId.Value);

                                SmartDocs doc = new SmartDocs();

                                var docTypeLetter = coverLetters.SingleOrDefault();
                                //foreach (var docTypeLetter in frontCoverLetters)
                                //{
                                //Folder structure and file naming convention should be Root/AppCode/AppCode_DocTypeCode_SeqNo.pdf
                                string coverLetterTemplatePath = docTypeLetter == null ? string.Empty : Path.Combine(Config.Core_CoverTemplateFolder, client.AppCode, docTypeLetter.CoverLetterTemplatePath);

                                SmartDocResponse response = new SmartDocResponse();

                                if (docType.IsPrintReady)
                                {
                                    //Do nothing
                                }
                                else if (docType.ShippingCarrier == Carrier.USPS.ToString())
                                {
                                    if (docType.ShippingServiceType == AM.USPSLib.ServiceType.PRIORITY_EXPRESS.ToString())
                                    {
                                        int reTrycnt = 0;
                                        while (reTrycnt < Config.Core_NoOfAttemptOnFailure)
                                        {
                                            eVSLabelRequest eVSReq = GenerateEVSLabelRequest(mailPiece, client.Name, docType.ShippingServiceType.ParseEnum<AM.USPSLib.ServiceType>());

                                            response = doc.GenerateeVSPriorityExpressCoverLetter(commonSvc.GetSystemUserId(), mbi.PnSRequestPath, mailPiece.Id.ToString(), eVSReq, mailPiece.EnvelopeName);


                                            if (response.HasError && (uspsExceptionCodes.Any(code => response.ExceptionCode.Contains(code)) && uspsExceptionDescriptions.Any(desc => response.ExceptionDetail.Contains(desc))))
                                            {
                                                //NOTE: Here system is check for unusal error like connection issue or timeout issue if occure then system will retry to generate label
                                                log.Error("Priority Express had some unexpected error {0}".Fill(response.ExceptionDetail));
                                                System.Threading.Thread.Sleep(Config.Core_FailureAttemptInterval);
                                                reTrycnt++;
                                            }
                                            else if (response.HasError)
                                            {
                                                var msg = "USPS Priority Express is not supporting shipping to provided address for MailPieceId : {0} with Error :{1}. So AM is doing fallback to other shipping service.".Fill(mailPiece.Id, response.ExceptionDetail);
                                                log.Debug(msg);
                                                //NOTE: Here system is check for not deliverable or any other error if occure then system will fall back to USPS Priority service and try to generate label
                                                var fbPriority = docTypeSvc.GetFallBackDocTypeByPnSConfigId(mailPiece.ClientId, mailPiece.DocTypeId.Value, mailBatch.ReceivedOn.Value.UtcToEst(), Carrier.USPS.ToString(), ShippingServiceType.PRIORITY.ToString());

                                                if (fbPriority == null)
                                                {
                                                    //Consider as an Error Because system didn't find Fallback document type apart from USPS Priority
                                                    break;
                                                }

                                                //NOTE :Need to update Document Type level details like DueDate, PrintLocation, PrintTrailId because system is changing document type and if may have vary configurations.
                                                docType = fbPriority;

                                                commonSvc.GetUpdatedDueDateAndPrintLocation(mailBatch.ReceivedOn.Value, mailPiece, docType, client);

                                                eVSReq = GenerateEVSLabelRequest(mailPiece, client.Name, AM.USPSLib.ServiceType.PRIORITY);

                                                response = doc.GenerateeVSPriorityCoverLetter(commonSvc.GetSystemUserId(), mbi.PnSRequestPath, mailPiece.Id.ToString(), eVSReq, mailPiece.EnvelopeName);
                                                break;
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }

                                        if (response.HasError)
                                        {
                                            mailPiece.TotalPages = 0;
                                            mailPiece.ShippingStatus = ShippingStatus.Error.ToString();
                                            mailPiece.ErrorDesc = response.ExceptionDetail;
                                            mailPiece.ErrorCode = OutComeCodes.Invalid_ShippingDetail;
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
                                    else if (docType.ShippingServiceType == AM.USPSLib.ServiceType.PRIORITY.ToString())
                                    {
                                        int reTrycnt = 0;
                                        while (reTrycnt < Config.Core_NoOfAttemptOnFailure)
                                        {
                                            eVSLabelRequest eVSReq = GenerateEVSLabelRequest(mailPiece, client.Name, AM.USPSLib.ServiceType.PRIORITY);

                                            response = doc.GenerateeVSPriorityCoverLetter(commonSvc.GetSystemUserId(), mbi.PnSRequestPath, mailPiece.Id.ToString(), eVSReq, mailPiece.EnvelopeName);

                                            if (response.HasError && (uspsExceptionCodes.Any(code => response.ExceptionCode.Contains(code)) || uspsExceptionDescriptions.Any(desc => response.ExceptionDetail.Contains(desc))))
                                            {
                                                //NOTE: Here system is check for unusal error like connection issue or timeout issue if occure then system will retry to generate label
                                                log.Error("Priority had some unexpected error {0}".Fill(response.ExceptionDetail));
                                                System.Threading.Thread.Sleep(Config.Core_FailureAttemptInterval);
                                                reTrycnt++;
                                            }
                                            else
                                            {
                                                break;
                                            }
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
                                    else if (docType.ShippingServiceType == ShippingServiceType.IMB.ToString())
                                    {
                                        if (Path.GetExtension(coverLetterTemplatePath) == ".docx")
                                        {
                                            var coversheetData = mailPieceSvc.GetCoversheetData(mailPieceId);
                                            //Call ASPOSE API For Generating IMB Cover Letter
                                            response = doc.GenerateUSPSIMBCoverLetterDocx(client.AppCode, coverLetterTemplatePath, mbi.PnSRequestPath, coversheetData);
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

                                    // Shipping Service Type is First Class or  client is print ready then we don't need to do anything.
                                }
                                else if (docType.ShippingCarrier == Carrier.FedEx.ToString())
                                {
                                    FedExAccountInfo fedExAccInfo = fedExSvc.GetFedExAccountDetailsByAccountCode(docType.ShippingAccountCode);
                                    ShipperInformation shipperInfo = commonSvc.GetShipperInformationByPrintLocation(mailPiece.PrintLocation, mailPiece.PrintTrailId.Value);
                                    CreateFedExLabel createFedExLabel = new CreateFedExLabel();
                                    while (attemptCount < Config.Core_NoOfAttemptOnFailure)
                                    {
                                        response = createFedExLabel.Create(mailPiece, fedExAccInfo, docType, mbi, shipperInfo);
                                        if (!response.HasError)
                                        {
                                            coverLetterPdfPaths.Add(response.CoverLetterPath);
                                            mailPiece.TrackingNo = response.TrackingCode;
                                            mailPiece.ShippingCost = response.PostageCost;
                                            break;
                                        }
                                        else if (response.HasError && (fedExExceptionCodes.Any(code => response.ExceptionCode == code) || fedExExceptionDescriptions.Any(desc => response.ExceptionDetail.Contains(desc))))
                                        {
                                            System.Threading.Thread.Sleep(Config.Core_FailureAttemptInterval);
                                            attemptCount++;
                                            var msg = string.Format("{0}|{1}", OutComeCodes.Unknown_Error, response.ExceptionDetail);
                                            //Setting as an error suppose FedEx is failing continueously then System will consider as an unknown error for that mail piece
                                            mailPiece.ShippingStatus = ShippingStatus.Error.ToString();
                                            mailPiece.ErrorCode = OutComeCodes.Unknown_Error;
                                            mailPiece.ErrorDesc = msg;
                                            log.Error(msg + "For MailPiece Id - {0}".Fill(mailPiece.Id));
                                            SendUnexpectedExceptionEmail("Error occured while generating FedEx label for MailPiece Id {0}<br/> System will try again...<br/>".Fill(mailPiece.Id) + msg);
                                        }
                                        else
                                        {
                                            if (response.HasError && (fallBackCodes.Any(code => code == response.ExceptionCode) || fallBackDescriptions.Any(desc => response.ExceptionDetail.Contains(desc))))
                                            {
                                                var msg = "FedEx is not supporting shipping to provided address for MailPieceId : {0} with Error :{1}. So AM is doing fallback to other shipping carrier.".Fill(mailPiece.Id, response.ExceptionDetail);
                                                log.Debug(msg);
                                                //First try to generate eVS label with USPS Priority Express
                                                var fbPriorityExpress = docTypeSvc.GetFallBackDocTypeByPnSConfigId(mailPiece.ClientId, mailPiece.DocTypeId.Value, mailBatch.ReceivedOn.Value.UtcToEst(), Carrier.USPS.ToString(), ShippingServiceType.PRIORITY_EXPRESS.ToString());

                                                if (fbPriorityExpress != null)
                                                {
                                                    docType = fbPriorityExpress;

                                                    mailPiece.DocTypeId = docType.Id;
                                                    //Update Due Date and its other calculation based on updated document type.
                                                    commonSvc.GetUpdatedDueDateAndPrintLocation(mailBatch.ReceivedOn.Value, mailPiece, docType, client);

                                                    eVSLabelRequest eVSReq = GenerateEVSLabelRequest(mailPiece, client.Name, docType.ShippingServiceType.ParseEnum<AM.USPSLib.ServiceType>());

                                                    response = doc.GenerateeVSPriorityExpressCoverLetter(commonSvc.GetSystemUserId(), mbi.PnSRequestPath, mailPiece.Id.ToString(), eVSReq, mailPiece.EnvelopeName);

                                                    if (response.HasError)
                                                    {
                                                        msg = "USPS Priority Express is not supporting shipping to provided address for MailPieceId : {0} with Error :{1}.".Fill(mailPiece.Id, response.ExceptionDetail);
                                                        log.Debug(msg);
                                                        var fbPriority = docTypeSvc.GetFallBackDocTypeByPnSConfigId(mailPiece.ClientId, mailPiece.DocTypeId.Value, mailBatch.ReceivedOn.Value.UtcToEst(), Carrier.USPS.ToString(), ShippingServiceType.PRIORITY.ToString());

                                                        if (fbPriority == null)
                                                        {
                                                            //Consider as an Error package
                                                            break;
                                                        }

                                                        docType = fbPriority;

                                                        mailPiece.DocTypeId = docType.Id;

                                                        //Update Due Date and its other calculation based on updated document type.
                                                        commonSvc.GetUpdatedDueDateAndPrintLocation(mailBatch.ReceivedOn.Value, mailPiece, docType, client);

                                                        eVSReq = GenerateEVSLabelRequest(mailPiece, client.Name, AM.USPSLib.ServiceType.PRIORITY);

                                                        response = doc.GenerateeVSPriorityCoverLetter(commonSvc.GetSystemUserId(), mbi.PnSRequestPath, mailPiece.Id.ToString(), eVSReq, mailPiece.EnvelopeName);
                                                    }
                                                }
                                                else
                                                {
                                                    var fbPriority = docTypeSvc.GetFallBackDocTypeByPnSConfigId(mailPiece.ClientId, mailPiece.DocTypeId.Value, mailBatch.ReceivedOn.Value.UtcToEst(), Carrier.USPS.ToString(), ShippingServiceType.PRIORITY_EXPRESS.ToString());

                                                    if (fbPriority == null)
                                                    {
                                                        //Consider as an Error package
                                                        break;
                                                    }

                                                    docType = fbPriority;

                                                    mailPiece.DocTypeId = docType.Id;

                                                    //Update Due Date and its other calculation based on updated document type.
                                                    commonSvc.GetUpdatedDueDateAndPrintLocation(mailBatch.ReceivedOn.Value, mailPiece, docType, client);

                                                    eVSLabelRequest eVSReq = GenerateEVSLabelRequest(mailPiece, client.Name, AM.USPSLib.ServiceType.PRIORITY);

                                                    response = doc.GenerateeVSPriorityCoverLetter(commonSvc.GetSystemUserId(), mbi.PnSRequestPath, mailPiece.Id.ToString(), eVSReq, mailPiece.EnvelopeName);
                                                }

                                                if (response.HasError)
                                                {
                                                    msg = "After FallBack, USPS Priority is not supporting shipping to provided address for MailPieceId : {0} with Error :{1}.".Fill(mailPiece.Id, response.ExceptionDetail);
                                                    log.Debug(msg);
                                                    //Consider as an Error package
                                                    break;
                                                }

                                                var sweepTimings = commonSvc.GetSweepTimings();

                                                coverLetterPdfPaths.Add(response.CoverLetterPath);
                                                mailPiece.TrackingNo = response.TrackingCode;
                                                mailPiece.ShippingCost = response.PostageCost;

                                                break;
                                            }
                                            //Consider as an Error package
                                            break;
                                        }
                                    }

                                    if (response.HasError)
                                    {
                                        //NOTE : If result false then mark as an error and update status for that mail piece
                                        mailPiece.TotalPages = 0;
                                        mailPiece.PrintLocation = null;
                                        mailPiece.PrintTrailId = null;
                                        mailPiece.DueDate = null;
                                        mailPiece.ShippingStatus = ShippingStatus.Error.ToString();
                                        mailPiece.ErrorCode = OutComeCodes.Invalid_ShippingDetail;
                                        mailPiece.ErrorDesc = string.Format("{0}|{1}", OutComeCodes.Invalid_ShippingDetail, response.ExceptionDetail);
                                        mailPieceSvc.UpdateBatchMailPieceDetails(mailPiece);
                                        continue;
                                    }
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

                                //Here system will add rear cover page only when print location is Dallas CL
                                List<PnSConfigDetailCoverLetter> rearCoverLetters = docTypeSvc.GetDocumentRearLetterTemplates(mailPiece.PnSConfigDetailId.Value);

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

                                if (!File.Exists(pdfFilePath) && docType.AllowInsertsOnly)
                                {
                                    if (File.Exists(outputFilePath))
                                        File.Delete(outputFilePath);

                                    File.Move(coverLetterPdfPaths.First(), outputFilePath);
                                    mailPiece.TotalPages = PdfUtil.GetPageCount(outputFilePath);
                                }
                                else
                                {
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
                                }

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
                                        GenerateImmidiateReports(clientSvc, commonSvc, mbi, mailPieces, reportDeliverable);

                                        mailBatchSvc.MarkMailBatchAsInvalid(mbi.MailBatch.Id, (mbi.MailBatch.ValidationErrors + Environment.NewLine + "Some documents could not be processed.")).Wait();
                                        commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, AM.Entities.BatchStatus.Failed.ToString()).Wait();

                                        //NOTE: Inserting Error Notification in this because system is not going to update any further status for batch due to full batch is invalid
                                        commonSvc.InsertErrorNotifications(mbi.MailBatch.Id);
                                        SendSubscribedErrorNotificationEmail(mbi, mailPieces, commonSvc.GetErrorNotificationEmailSubscriberList(mbi.Client.Id));
                                    }
                                    else if (validMailPiecesList.Count < mailPieces.Count)
                                    {
                                        var errMpIds = mailPieces.Select(mp => mp.Id).Except(errorMailPieces.Select(errMp => errMp.Id));
                                        GenerateLabelErrorReport(clientSvc, commonSvc, mbi, mailPieces.Where(mp => errMpIds.Contains(mp.Id)).ToList(), reportDeliverable);
                                        commonSvc.UpdateBatchValidationStatus(mbi.MailBatch.Id, BatchValidationStatus.PartiallyValid, mailBatch.ValidationErrors);
                                        commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Prepped, Common.Now).Wait();
                                    }
                                    else
                                    {
                                        commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Prepped, Common.Now).Wait();
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
                        }
                    }
                });
            }
            log.Debug("PrepPnSJob Completed");
            return true;
        }

        private static void GenerateLabelErrorReport(IClientService clientSvc, ICommonService commonSvc, MailBatchInfo mbi, List<MailPieces> mailPieces, Deliverable deliverable)
        {
            var erroredMPs = mailPieces.Where(mp => mp.ShippingStatus == ShippingStatus.Error.ToString()).ToList();
            //If System do not find new errors in mail pieces apart from already provided error reports then it will just return.
            if (erroredMPs.Count() == 0)
                return;


            SFTChannel sftChannel = clientSvc.GetSFTChannelDetailsBySFTChannelId(mbi.MailBatch.SFTChannelId).Result;
            SFTUtil sftSite = new SFTUtil(Common.GetProtocolType(sftChannel.Protocol), sftChannel.Host, sftChannel.UserName, sftChannel.Password, 300000);
            sftSite.Connect();
            switch (sftChannel.Code.Split('_')[0])
            {
                case "ALASKA":
                    break;
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

                            sftSite.UploadFile(localErrorReportPath, ftpErrorReportPath);

                            // Upload Error.GO file after uploading error report
                            File.WriteAllText(errorGoFilePath, string.Empty);
                            sftSite.UploadFile(errorGoFilePath, ftpErrorGoUploadPath);

                            if (File.Exists(localErrorReportPath))
                                File.Move(localErrorReportPath, Path.ChangeExtension(localErrorReportPath, ".Uploaded"));
                        }
                    }
                    break;
                case "PENFED":
                    using (IGeneralService generalSvc = new GeneralService())
                    {
                        //NOTE : Below zip file naming convention is EDG Naming convention.
                        string zipFileName = "{0}FAGOS.ZYONT.N0010362.PenFedPNSOUTPUT.D{1}.T{2}.zip".Fill(Config.Common_EnvironmentInitial, Common.Now.ToString("yyyyMMdd"), Common.Now.ToString("hhmmss"));
                        string zipFilePath = Path.Combine(mbi.AMResponsePath, zipFileName);
                        string errorReportFileName = "{0}-{1}-{2}.xml".Fill(mbi.Client.AppCode, mbi.MailBatch.Id, commonSvc.GetReportingSequence());
                        string localErrorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);

                        string ftpUploadPath = Path.Combine(deliverable.UploadPath, zipFileName);
                        string responseReport = generalSvc.GeneralXMLReport(erroredMPs, mbi.MailBatch, mbi.Client.AppCode);
                        if (!string.IsNullOrWhiteSpace(responseReport))
                        {
                            File.WriteAllText(localErrorReportPath, responseReport);
                        }

                        using (ZipArchive newFile = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                        {
                            newFile.CreateEntryFromFile(localErrorReportPath, errorReportFileName);
                        }
                        sftSite.UploadFile(zipFilePath, ftpUploadPath);
                        Thread.Sleep(1000);//This line is added to avoid any duplication of file name.

                        File.Move(zipFilePath, Path.ChangeExtension(zipFilePath, ".ZipUploaded"));

                        if (File.Exists(localErrorReportPath))
                            File.Move(localErrorReportPath, Path.ChangeExtension(localErrorReportPath, ".Uploaded"));
                    }
                    break;
                case "BECU":
                    using (IGeneralService generalSvc = new GeneralService())
                    {
                        //NOTE : Below zip file naming convention is EDG Naming convention.
                        string zipFileName = "{0}FAGOS.ZYONT.N0010376.BECUPNSOUTPUT.D{1}.T{2}.zip".Fill(Config.Common_EnvironmentInitial, Common.Now.ToString("yyyyMMdd"), Common.Now.ToString("hhmmss"));
                        string zipFilePath = Path.Combine(mbi.AMResponsePath, zipFileName);
                        string errorReportFileName = "{0}-{1}-{2}.xml".Fill(mbi.Client.AppCode, mbi.MailBatch.Id, commonSvc.GetReportingSequence());
                        string localErrorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);

                        string ftpUploadPath = Path.Combine(deliverable.UploadPath, zipFileName);
                        string responseReport = generalSvc.GeneralXMLReport(erroredMPs, mbi.MailBatch, mbi.Client.AppCode);
                        if (!string.IsNullOrWhiteSpace(responseReport))
                        {
                            File.WriteAllText(localErrorReportPath, responseReport);
                        }

                        using (ZipArchive newFile = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                        {
                            newFile.CreateEntryFromFile(localErrorReportPath, errorReportFileName);
                        }
                        sftSite.UploadFile(zipFilePath, ftpUploadPath);
                        Thread.Sleep(1000);//This line is added to avoid any duplication of file name.

                        File.Move(zipFilePath, Path.ChangeExtension(zipFilePath, ".ZipUploaded"));

                        if (File.Exists(localErrorReportPath))
                            File.Move(localErrorReportPath, Path.ChangeExtension(localErrorReportPath, ".Uploaded"));
                    }
                    break;
                default:
                    using (IGeneralService generalSvc = new GeneralService())
                    {
                        string errorReportFileName = "{0}-{1}-{2}.xml".Fill(mbi.Client.AppCode, mbi.MailBatch.Id, commonSvc.GetReportingSequence());
                        string errorGoFileName = "ErrReport.go";
                        string errorGoFilePath = Path.Combine(mbi.AMResponsePath, errorGoFileName);
                        string ftpErrorGoUploadPath = Path.Combine(deliverable.UploadPath, errorGoFileName);

                        string localErrorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);
                        string ftpErrorReportPath = Path.Combine(deliverable.UploadPath, errorReportFileName);

                        string responseReport = generalSvc.GeneralXMLReport(erroredMPs, mbi.MailBatch, mbi.Client.AppCode);
                        if (!string.IsNullOrWhiteSpace(responseReport))
                        {
                            File.WriteAllText(localErrorReportPath, responseReport);

                            sftSite.UploadFile(localErrorReportPath, ftpErrorReportPath);

                            // Upload Error.GO file after uploading error report
                            File.WriteAllText(errorGoFilePath, string.Empty);
                            sftSite.UploadFile(errorGoFilePath, ftpErrorGoUploadPath);

                            if (File.Exists(localErrorReportPath))
                                File.Move(localErrorReportPath, Path.ChangeExtension(localErrorReportPath, ".Uploaded"));
                        }
                    }
                    break;
            }

            if (sftSite.IsConnected)
                sftSite.Disconnect();

            //TODO: Remove after proper naming convention
            Thread.Sleep(1000);
        }

        private static eVSLabelRequest GenerateEVSLabelRequest(MailPieceDTO mailPiece, string clientName, AM.USPSLib.ServiceType serviceType)
        {
            eVSLabelRequest eVSReq = new eVSLabelRequest();
            //General settings
            eVSReq.AllowNonCleansedDestAddr = true;
            eVSReq.AllowNonCleansedOriginAddr = true;
            eVSReq.ServiceType = serviceType;
            eVSReq.Container = Container.FLAT_RATE_ENVELOPE;
            eVSReq.ShipDate = mailPiece.EffectiveReceivedOnEst.Value;
            eVSReq.ReceiptOption = ReceiptOption.SEPARATE_PAGE;
            eVSReq.WeightInOunces = "48";
            eVSReq.ImageType = ImageType.PDF;
            eVSReq.POZipCode = "75099";
            eVSReq.SenderName = clientName;

            if (Config.Common_LandscapeUSPSLabels.Contains(mailPiece.EnvelopeName))
            {
                eVSReq.LabelParameters = new LabelParameters() { Sequence = new LabelSequence() { PackageNumber = 1, TotalPackages = 1 }, Size = LabelSize.SEPARATECONTINUEPAGE };
            }
            else
            {
                if (mailPiece.RcptAddressTypes.Contains(AddressType.Military.ToString()))
                    eVSReq.LabelParameters = new LabelParameters() { Sequence = new LabelSequence() { PackageNumber = 1, TotalPackages = 1 }, Size = LabelSize.LABEL4X6L };
                else
                    eVSReq.LabelParameters = new LabelParameters() { XCoordinate = 450, YCoordinate = 500, Sequence = new LabelSequence() { PackageNumber = 1, TotalPackages = 1 }, Size = LabelSize.LABEL4X6P };
            }


            // From Address
            eVSReq.CustomerRefNo = mailPiece.Id.ToString();
            eVSReq.FromName = (mailPiece.RetRcptFirstName + " " + mailPiece.RetRcptLastName).Trim();
            
            eVSReq.FromAptOrSuiteorOther = mailPiece.RetRcptAddressAptOrSuiteOrOther;
            eVSReq.FromStreetAddress = (mailPiece.RetRcptAddressStreet + " " + mailPiece.RetRcptAddrOptionalLine1BelowStreet).Trim();
            eVSReq.FromFirm = mailPiece.RetRcptOrganization;
            if (String.IsNullOrWhiteSpace(eVSReq.FromName) && String.IsNullOrWhiteSpace(eVSReq.FromFirm))
            {
                eVSReq.FromFirm = clientName;
            }
            
            eVSReq.FromCity = mailPiece.RetRcptAddressCity;
            eVSReq.FromState = mailPiece.RetRcptAddressState;
            eVSReq.FromZip4 = mailPiece.RetRcptAddressZIP.SeparateZip4And5(4);
            eVSReq.FromZip5 = mailPiece.RetRcptAddressZIP.SeparateZip4And5(5);
            eVSReq.FromPhone = string.IsNullOrWhiteSpace(mailPiece.RetRcptPhone) ? new string('0', 10) : mailPiece.RetRcptPhone.RemoveFormatting();

            // To Address
            eVSReq.ToFirm = string.IsNullOrEmpty(mailPiece.RcptOrganization) ? string.Empty : mailPiece.RcptOrganization;
            eVSReq.ToName = (mailPiece.RcptFirstName + " " + mailPiece.RcptLastName).Trim();
            eVSReq.ToAptOrSuiteorOther = mailPiece.RcptAddressAptOrSuiteOrOther;
            eVSReq.ToStreetAddress = (mailPiece.RcptAddressStreet + " " + mailPiece.RcptAddrOptionalLine1BelowStreet).Trim();
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

        private static bool SendSubscribedErrorNotificationEmail(MailBatchInfo mbi, List<MailPieces> mailPieces, string emailList)
        {
            bool isEmailSent = false;

            try
            {
                if (string.IsNullOrWhiteSpace(emailList))
                {
                    return true;
                }

                using (SmtpClient smtpClient = new SmtpClient())
                {
                    using (MailMessage msg = new MailMessage())
                    {
                        msg.IsBodyHtml = true;
                        msg.To.Add(emailList);
                        msg.CC.Add(Config.Common_ErrorSubscribedEmailCc);
                        msg.Subject = "{0} : {1} {2}".Fill(Config.Common_Environment, mbi.Client.AppCode, Config.Common_ErrorSubscribedEmailSubject);
                        msg.Priority = MailPriority.High;

                        if (mailPieces.Count > 0)
                        {
                            #region Prepare email body

                            string downloadEmail_TD_SrNoStyle = "style='width:10%;border:solid windowtext 1.0pt;border-top:none;padding:0in 5.4pt 0in 5.4pt' valign='top' width='213'><p>";
                            string downloadEmail_TD_DescriptionStyle = "style='width:45%;border-top:none;border-left:none;border-bottom:solid windowtext 1.0pt;border-right:solid windowtext 1.0pt;padding:0in 5.4pt 0in 5.4pt'' valign='top' width='213'><p>";

                            StringBuilder sb = new StringBuilder(File.ReadAllText(Config.Common_ErrorSubscribedEmailTemplate));
                            sb = sb.Replace("<%FileName%>", mbi.MailBatch.BatchName);
                            sb = sb.Replace("<%yyyymmdd%>", Common.Now.UtcToClientTimeZone(mbi.Client.TimeZone).ToString("MM/dd/yyyy"));
                            sb = sb.Replace("<%hhmm%>", Common.Now.UtcToClientTimeZone(mbi.Client.TimeZone).ToString("hh:mm tt"));

                            var errorMPs = mailPieces.Where(mp => mp.ShippingStatus == ShippingStatus.Error.ToString());
                            StringBuilder errData = new StringBuilder();
                            foreach (var errMp in errorMPs)
                            {
                                errData.AppendLine("<tr><td {0}>{1}</td><td {2}>{3}</td><td{4}>{5}</td></tr>".Fill(downloadEmail_TD_SrNoStyle, errMp.BusinessFriendlyIdValue, downloadEmail_TD_DescriptionStyle, errMp.ErrorCode, downloadEmail_TD_DescriptionStyle, errMp.ErrorDesc));
                            }
                            sb = sb.Replace("<%trdata%>", errData.ToString());
                            msg.Body = sb.ToString();

                            #endregion
                        }

                        if (msg.Body.Length > 0)
                        {
                            smtpClient.Send(msg);
                            isEmailSent = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            return isEmailSent;
        }

        private static void GenerateImmidiateReports(IClientService clientSvc, ICommonService commonSvc, MailBatchInfo mbi, List<MailPieces> mailPieces, Deliverable deliverable)
        {

            SFTChannel sftChannel = clientSvc.GetSFTChannelDetailsBySFTChannelId(mbi.MailBatch.SFTChannelId).Result;

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

                            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportPrepped, Common.Now).Wait();
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
                        commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportPrepped, Common.Now).Wait();
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
                        commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportPrepped, Common.Now).Wait();
                    }
                    break;
                default:
                    using (IGeneralService generalSvc = new GeneralService())
                    {
                        var errorOrPullbackMps = mailPieces.Where(mp => mp.ShippingStatus == ShippingStatus.Error.ToString() || mp.ShippingStatus == ShippingStatus.PulledBack.ToString()).ToList();
                        string responseReport = generalSvc.GeneralXMLReport(errorOrPullbackMps, mbi.MailBatch, mbi.Client.AppCode);

                        if (!string.IsNullOrWhiteSpace(responseReport))
                        {
                            string deliveryReportFileName = "{0}-{1}-{2}.xml".Fill(mbi.Client.AppCode, mbi.MailBatch.Id, commonSvc.GetReportingSequence());
                            string deliveryReportPath = Path.Combine(mbi.AMResponsePath, deliveryReportFileName);

                            File.WriteAllText(deliveryReportPath, responseReport);
                        }

                        commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportPrepped, Common.Now).Wait();
                    }
                    break;
            }

            //TODO: Remove after proper naming convention
            Thread.Sleep(1000);
        }

        #endregion

        #region SubmitBatchesDocTypeWise
        internal static bool SubmitBatchesDocTypeWiseToPrintVendor()
        {
            using (IMailBatchService mailBatchService = new MailBatchService())
            {
                log.Debug("Submitted Batches job started.");

                List<MailBatchDTO> preppedBatches = mailBatchService.GetBatchesByPipelineStatusAndClientId(BatchPipelineStatus.Prepped);

                if (preppedBatches.Count == 0)
                {
                    log.Debug("Submitted Batches Job queue is empty.");
                    return false;
                }

                Parallel.ForEach(preppedBatches, (mailBatch) =>
                {
                    using (IMailBatchService mailBatchSvc = new MailBatchService())
                    using (IClientService clientSvc = new ClientService())
                    using (ICommonService commonSvc = new CommonService())
                    using (IMailPieceService mailPieceSvc = new MailPieceService())
                    using (IDocTypeService docTypeSvc = new DocTypeService())
                    {
                        try
                        {
                            Client client = clientSvc.GetClientById(mailBatch.ClientId);
                            MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, Config.Common_AuthoMailRootFolder);

                            var batchData = mailBatchSvc.GetMailBatchDetailsByBatchId(mbi.MailBatch.Id);

                            GeneratePrintBatches(mailBatchSvc, clientSvc, commonSvc, mailPieceSvc, docTypeSvc, batchData);

                            //After processing mail batch for PrintBatches we will check for each and every mailpieces are having printbatchid in it or not then update SubmittedToShipping status in MailBatches table
                            var mailPiecesList = mailPieceSvc.GetMailPiecesByMailBatchId(mailBatch.Id);
                            var validPieces = mailPiecesList.Count(c => c.ErrorCode == OutComeCodes.No_Error && c.PrintBatchId != null);
                            var errorPieces = mailPiecesList.Count(c => c.ErrorCode != OutComeCodes.No_Error && c.PrintBatchId == null);
                            var earlyPulledBackPieces = mailPiecesList.Count(c => c.ErrorCode == OutComeCodes.No_Error && c.PrintBatchId == null && c.IsPulledBack);

                            if (mailPiecesList.Count != (validPieces + errorPieces + earlyPulledBackPieces))
                            {
                                return;
                            }

                            commonSvc.UpdateBatchPipeLineStatus(mailBatch.Id, BatchPipelineStatus.SubmittedToShipping, Common.Now).Wait();

                            //NOTE: Inserting Error Notification in this because system is not going to update any further status for batch due to full batch is invalid
                            commonSvc.InsertErrorNotifications(mbi.MailBatch.Id);
                            List<MailPieces> mailPieces;
                            using (IReportService reportSvc = new ReportService())
                            {
                                mailPieces = reportSvc.GetMailPiecesDataFromMailBatchId(mbi.MailBatch.Id);
                            }
                            SendSubscribedErrorNotificationEmail(mbi, mailPieces, commonSvc.GetErrorNotificationEmailSubscriberList(mbi.Client.Id));
                        }
                        catch (Exception ex)
                        {
                            var msg = "Error occured in SubmitBatchesDocTypeWiseToPrintVendor Method For Mail Batch Id: {0} with Error : {1}".Fill(mailBatch.Id, ex.Message);
                            log.Error(msg, ex);
                            SendUnexpectedExceptionEmail(msg);
                        }
                    }
                });

                log.Debug("Submitted Batches job completed.");
            }
            return true;
        }

        private static void GeneratePrintBatches(IMailBatchService mailBatchSvc, IClientService clientSvc, ICommonService commonSvc, IMailPieceService mailPieceSvc, IDocTypeService docTypeSvc, BatchData mailBatchData)
        {
            List<MailPieceDTO> validMailPieces = mailBatchData.MailPieces.Where(c => c.ErrorCode == OutComeCodes.No_Error && c.ShippingStatus == ShippingStatus.InProgress.ToString() && c.PrintBatchId == null).ToList();

            if (validMailPieces.Count == 0)
                return;

            List<PrintBatch> printBatches = new List<PrintBatch>();

            var docTypes = validMailPieces.Select(row => new { DocTypeId = row.DocTypeId.Value, PnSConfigDetailId = row.PnSConfigDetailId.Value }).Distinct();
            foreach (var docTypePnSConfig in docTypes)
            {
                DocType docType = docTypeSvc.GetDocTypeById(docTypePnSConfig.DocTypeId, docTypePnSConfig.PnSConfigDetailId);

                var filteredMailPiecesDocTypeWise = validMailPieces.Where(c => c.DocTypeId == docTypePnSConfig.DocTypeId && c.PnSConfigDetailId == docTypePnSConfig.PnSConfigDetailId).ToList();

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
                    printBatch.PnSConfigDetailId = docType.PnSConfigDetailId.Value;
                    printBatch.PrintTrailId = printTrailId.Value;
                    printBatch.PrintLocation = commonSvc.GetShipperInformation(commonSvc.GetPrintVendorDetailsByPrintTrailId(printTrailId.Value).ShipperId).PrintLocation;
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
                PnSRequest request = new PnSRequest();
                try
                {
                    remoteBatchPath.CleanDirectory();
                    request.AppCode = printBatchData.AppCode;
                    request.BatchIdentifier = printBatchData.UniqueId;
                    request.BatchName = batchName;
                    request.DocTypeId = printBatchData.DocTypeId;
                    request.PnSConfigDetailId = printBatchData.PnSConfigDetailId;
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
                catch (Exception ex)
                {
                    var msg = "Error occured while generating PnSService batch for MailBatchId {0} and PrintBatchId {1} <br/> Error Message <br/>{2}".Fill(mailBatchData.MailBatch.Id, printBatchData.Id, ex);
                    log.Error(msg, ex);

                    SendUnexpectedExceptionEmail(msg);

                    if (Directory.Exists(remoteBatchPath))
                        Directory.Delete(remoteBatchPath, true);

                    if (File.Exists(remoteXMLPath))
                        File.Delete(remoteXMLPath);

                    if (File.Exists(remoteGOPath))
                        File.Delete(remoteGOPath);

                    mailPieceSvc.DeletePrintBatchDetails(printBatchData.Id);
                }
            }
        }
        #endregion

        internal static bool GetSubmittedPnSResponses()
        {
            var processedCount = 0;

            using (IMailBatchService mailBatchService = new MailBatchService())
            {
                log.Debug("Get submitted job started.");
                var submittedPrintBatches = mailBatchService.GetSubmittedPrintBatchDetails();

                if (submittedPrintBatches.Count == 0)
                {
                    log.Debug("Response download Job queue is empty.");
                    return false;
                }

                Parallel.ForEach(submittedPrintBatches, (printBatch) =>
                {
                    using (ICommonService commonSvc = new CommonService())
                    {
                        try
                        {
                            string triggerFilePath = Path.Combine(Config.Core_PnS_SFT_RootDir, printBatch.AppCode, $"{printBatch.SubmittedBatchName}.rpt");
                            string responseFileName = $"{printBatch.SubmittedBatchName}-res.xml";
                            string responseFilePath = Path.Combine(Config.Core_PnS_SFT_RootDir, printBatch.AppCode, responseFileName);

                            if (!File.Exists(triggerFilePath))
                            {
                                return;
                            }

                            string amPnSResponseDir = Path.Combine(Config.Common_AuthoMailRootFolder, printBatch.AppCode, Util.PnSResponsesFolderName);
                            string amPnSResponseFilePath = Path.Combine(amPnSResponseDir, responseFileName);

                            DateTime responseReceivedOn = File.GetLastWriteTimeUtc(responseFilePath);

                            Directory.CreateDirectory(amPnSResponseDir);

                            File.Copy(responseFilePath, amPnSResponseFilePath, true);

                            File.Delete(triggerFilePath);

                            File.Delete(responseFilePath);

                            commonSvc.UpdatePrintBatchPipeLineStatus(printBatch.PrintBatchId, BatchPipelineStatus.ShippingResponseReceived, responseReceivedOn);

                            Interlocked.Increment(ref processedCount);
                        }
                        catch (Exception ex)
                        {
                            var errorMsg = "Error occured while downloading response from PnS Service : {0} ".Fill(ex);
                            log.Error(errorMsg, ex);
                            SendUnexpectedExceptionEmail(errorMsg);
                        }
                    }
                });
                log.Debug("Get submitted job completed.");
            }
            return processedCount > 0;
        }

        internal static bool ProcessPnSResponses()
        {
            log.Debug("Process PnS response job is started.");

            using (IMailBatchService mailBatchService = new MailBatchService())
            {

                var responseReceivedPrintBatches = mailBatchService.GetResponseReceivedPrintBatchDetails();

                if (responseReceivedPrintBatches.Count == 0)
                {
                    log.Debug("Response process job queue is empty.");
                    return false;
                }

                Parallel.ForEach(responseReceivedPrintBatches, (printBatch) =>
                {
                    using (IMailBatchService mailBatchSvc = new MailBatchService())
                    using (IClientService clientSvc = new ClientService())
                    using (ICommonService commonSvc = new CommonService())
                    using (IMailPieceService mailPieceSvc = new MailPieceService())
                    using (IReportService reportSvc = new ReportService())
                    {
                        try
                        {
                            var printBatchMailPieces = reportSvc.GetMailPiecesDataFromPrintBatchId(printBatch.PrintBatchId);

                            // Here we have changed path of pnsresponse to process from "AMRoot/ClientCode/PnSResponses/AppCode_PrintBatchId-res.xml" because now we have multiple batches mail pieces in printbatch table

                            string printbatchResponseFilePath = Path.Combine(Config.Common_AuthoMailRootFolder, printBatch.AppCode, Util.PnSResponsesFolderName, $"{printBatch.SubmittedBatchName}-res.xml");

                            var pnsResponse = PnSResponse.LoadXml(File.ReadAllText(printbatchResponseFilePath));

                            var responseLookup = new Dictionary<Guid, PCI.Lib.MailPieceReport>();

                            pnsResponse.MailPackageReport.ForEach(mpr => responseLookup.Add(mpr.UniqueId, mpr));

                            printBatchMailPieces.ForEach(row =>
                            {
                                var mailPackage = responseLookup[row.UniqueId];

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
                                    row.MissedSLAReason = GetMissedSLAReason(mailPieceSvc, commonSvc, row);
                                }
                                row.ShippingStatus = mailPackage.MailPieceStatus.ToString();

                                mailPieceSvc.UpdateBatchMailPieceShippingStatusDetails(row);
                            });

                            //Updates PrintBatches Status values
                            commonSvc.UpdatePrintBatchStatus(printBatch.PrintBatchId, pnsResponse.BatchStatus.ToString(), pnsResponse.StatusDescription);

                            var increamentalReportingChannels = Config.Core_IncrementalReportingChannelCodes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                            increamentalReportingChannels = increamentalReportingChannels.Select(c => c + "_" + Config.Common_Environment).ToList();

                            string channelCode = clientSvc.GetSFTChannelDetailsBySFTChannelId(printBatch.SFTChannelId).Result.Code;

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
                                        using (IGeneralService generalSvc = new GeneralService())
                                        {
                                            string responseReport = generalSvc.GeneralXMLReport(printBatchMailPieces, mailBatch);

                                            if (!string.IsNullOrWhiteSpace(responseReport))
                                            {
                                                string deliveryReportFileName = "{0}-{1}-{2}.xml".Fill(mailBatch.Clients.AppCode, mailBatch.Id, commonSvc.GetReportingSequence());
                                                string deliveryReportPath = Path.Combine(amResponsePath, deliveryReportFileName);

                                                File.WriteAllText(deliveryReportPath, responseReport);
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
                                    commonSvc.UpdateBatchStatus(printBatch.MailBatchId, AM.Entities.BatchStatus.ShippedAll.ToString()).Wait();
                                }
                                else if (mailPiecesData.All(data => data.ShippingStatus == ShippingStatus.PulledBack.ToString()))
                                {
                                    commonSvc.UpdateBatchStatus(printBatch.MailBatchId, AM.Entities.BatchStatus.PulledBack.ToString()).Wait();
                                }
                                else if (mailPiecesData.All(data => data.ShippingStatus == ShippingStatus.Error.ToString()))
                                {
                                    commonSvc.UpdateBatchStatus(printBatch.MailBatchId, AM.Entities.BatchStatus.Failed.ToString()).Wait();
                                }
                                else
                                {
                                    commonSvc.UpdateBatchStatus(printBatch.MailBatchId, AM.Entities.BatchStatus.ShippedPartial.ToString()).Wait();
                                }

                                commonSvc.UpdateBatchPipeLineStatus(printBatch.MailBatchId, BatchPipelineStatus.ShippingResponseReceived, Common.Now).Wait();
                            }
                        }
                        catch (Exception ex)
                        {
                            var errorMsg = "Error occured while processing PnS response for PrintBatchId : {0} And Error : {1} ".Fill(printBatch.PrintBatchId, ex);
                            log.Error(errorMsg, ex);
                            SendUnexpectedExceptionEmail(errorMsg);
                        }
                    }
                });
            }
            log.Debug("Process PnS response job is completed.");
            return true;
        }

        private static string GetMissedSLAReason(IMailPieceService mailPieceSvc, ICommonService commonSvc, MailPieces mailPiece)
        {
            if (mailPiece.ShippedOn <= mailPiece.DueDate)
                return MissedSLAReason.NA;

            BatchSummary pnsBatchSummary = mailPieceSvc.GetBatchSummaryDetailByUniqueId(mailPiece.UniqueId);

            string vendorCode = commonSvc.GetPrintVendorDetailsByPrintTrailId(mailPiece.PrintTrailId.Value).VendorCode;

            if ((pnsBatchSummary.SubmittedOn - mailPiece.MaxSubmissionTime.Value).TotalMinutes > Config.Common_ProcessingBufferTime)
                return MissedSLAReason.AM;

            if (vendorCode == "CL_DALLAS")
                return MissedSLAReason.DALLAS_CL;

            if (vendorCode == "PCI")
                return MissedSLAReason.PCI;

            if (vendorCode == "NORDIS")
                return MissedSLAReason.NORDIS;

            return MissedSLAReason.AM;
        }

        #region ReportPreppedProcess
        internal static bool ReportPreppedProcess()
        {
            log.Debug("Report prepped job is started.");

            using (IMailBatchService mailBatchService = new MailBatchService())
            {
                List<MailBatchDTO> resReceivedBatches = mailBatchService.GetBatchesByPipelineStatusAndClientId(BatchPipelineStatus.ShippingResponseReceived);
                if (resReceivedBatches.Count == 0)
                {
                    log.Debug("Report prepped job queue is empty.");
                    return false;
                }

                Parallel.ForEach(resReceivedBatches, (mailBatch) =>
                {
                    using (IMailBatchService mailBatchSvc = new MailBatchService())
                    using (IClientService clientSvc = new ClientService())
                    using (ICommonService commonSvc = new CommonService())
                    using (IAMCService amcSvc = new AMCService())
                    using (IReportService reportSvc = new ReportService())
                    {
                        try
                        {
                            Client client = clientSvc.GetClientById(mailBatch.ClientId);

                            List<Deliverable> deliverables = commonSvc.GetDeliverablesByClientId(client.Id);

                            MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, Config.Common_AuthoMailRootFolder);
                            SFTChannel sftChannel = clientSvc.GetSFTChannelDetailsBySFTChannelId(mbi.MailBatch.SFTChannelId).Result;

                            List<MailPieces> mailPieces = reportSvc.GetMailPiecesDataFromMailBatchId(mbi.MailBatch.Id);

                            if (mailPieces != null)
                            {
                                try
                                {
                                    bool isReportGenerated = false;

                                    isReportGenerated = GenerateFinalReports(commonSvc, mbi, mailPieces, deliverables, sftChannel);

                                    if (isReportGenerated)
                                        commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportPrepped, Common.Now).Wait();

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
                        }
                    }
                });
            }

            log.Debug("Report prepped job is completed.");
            return true;
        }

        private static bool GenerateFinalReports(ICommonService commonSvc, MailBatchInfo mbi, List<MailPieces> mailPieces, List<Deliverable> deliverables, SFTChannel sftChannel)
        {
            // #ChannelCode
            string sftChannelCode = sftChannel.GetChannelCode().ToUpper();
            var increamentalReportingChannels = Config.Core_IncrementalReportingChannelCodes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            increamentalReportingChannels = increamentalReportingChannels.Select(c => c + "_" + Config.Common_Environment).ToList();
            var inProgressMailPiece = mailPieces.Where(m => m.ShippingStatus == ShippingStatus.InProgress.ToString()).Count();

            if (inProgressMailPiece == 0)
            {
                if (increamentalReportingChannels.Contains(sftChannel.Code))
                {
                    switch (sftChannelCode)
                    {
                        case "AMC":
                            using (IAMCService amcSvc = new AMCService())
                            {
                                var pulledBackMailPieces = mailPieces.Where(m => (m.ShippingStatus == ShippingStatus.PulledBack.ToString() && m.PrintBatchId == null)).ToList();
                                if (pulledBackMailPieces.Count > 0)
                                {
                                    StringBuilder deliveryReport = amcSvc.AMCDeliveryReport(pulledBackMailPieces);

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
                                var filteredMPs = mailPieces.Where(m => (m.ShippingStatus == ShippingStatus.PulledBack.ToString() && m.PrintBatchId == null) || (m.ShippingStatus == ShippingStatus.Error.ToString())).ToList();

                                if (filteredMPs.Count == 0)
                                    return true;

                                string responseReport = generalSvc.GeneralXMLReport(filteredMPs, mbi.MailBatch, mbi.Client.AppCode);

                                if (!string.IsNullOrWhiteSpace(responseReport))
                                {
                                    string deliveryReportFileName = "{0}-{1}-{2}.xml".Fill(mbi.Client.AppCode, mbi.MailBatch.Id, commonSvc.GetReportingSequence());
                                    string deliveryReportPath = Path.Combine(mbi.AMResponsePath, deliveryReportFileName);

                                    File.WriteAllText(deliveryReportPath, responseReport);
                                }
                            }
                            break;
                    }
                }
                else
                {
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
                                }

                                var errorReportDeliverable = deliverables.Single(r => r.ReportType == ReportTypes.Error);
                                StringBuilder errorReport = alaskaSvc.AlaskaErrorReport(mailPieces, mbi.Client.TimeZone, errorReportDeliverable.Delimeter);

                                if (errorReport != null)
                                {
                                    string errorReportFileName = "ER_{0}.csv".Fill(mbi.BatchName);
                                    string errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);

                                    File.WriteAllText(errorReportPath, errorReport.ToString());
                                }
                            }
                            break;
                        case "API_ELLIEMAE":
                            using (IGeneralService generalSvc = new GeneralService())
                            {
                                //NOTE : Reason to use FirstOrDefault : AM will get only single request using API
                                string responseReport = generalSvc.GeneralJsonReport(mailPieces.FirstOrDefault().UniqueId.ToString());

                                if (!string.IsNullOrWhiteSpace(responseReport))
                                {
                                    string deliveryReportFileName = "{0}-{1}-{2}.json".Fill(mbi.Client.AppCode, mbi.MailBatch.Id, commonSvc.GetReportingSequence());
                                    string deliveryReportPath = Path.Combine(mbi.AMResponsePath, deliveryReportFileName);

                                    File.WriteAllText(deliveryReportPath, responseReport);
                                }
                            }
                            break;
                        default:
                            using (IGeneralService generalSvc = new GeneralService())
                            {
                                string responseReport = generalSvc.GeneralXMLReport(mailPieces, mbi.MailBatch, mbi.Client.AppCode);

                                if (!string.IsNullOrWhiteSpace(responseReport))
                                {
                                    string deliveryReportFileName = "{0}-{1}-{2}.xml".Fill(mbi.Client.AppCode, mbi.MailBatch.Id, commonSvc.GetReportingSequence());
                                    string deliveryReportPath = Path.Combine(mbi.AMResponsePath, deliveryReportFileName);

                                    File.WriteAllText(deliveryReportPath, responseReport);
                                }
                            }
                            break;
                    }
                }
                return true;
            }

            return false;
        }
        #endregion
    }
}
