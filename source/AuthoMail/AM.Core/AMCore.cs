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
                List<MailBatchDTO> mailBatches = mailBatchSvc.GetBatchesByPipelineStatus(BatchPipelineStatus.Acknowledged);

                if (mailBatches.Count == 0)
                {
                    log.Debug("Prep Job queue is empty.");
                    return;
                }

                foreach (MailBatchDTO mailBatch in mailBatches)
                {
                    SmartDocs doc = new SmartDocs();
                    Client client = clientSvc.GetClientById(mailBatch.ClientId);
                    MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, Config.Common_AuthoMailRootFolder);

                    var batchData = mailBatchSvc.GetMailBatchDetailsByBatchId(mbi.MailBatch.Id);

                    List<MailPieceDTO> validMailPieces = batchData.MailPieces.Where(c => c.ErrorCode == OutComeCodes.No_Error && c.ShippingStatus == ShippingStatus.InProgress.ToString()).ToList();

                    if (validMailPieces.Count == 0)
                    {
                        // TODO: Ask Ravi Sir -  Report Invalid or Keep Failed at Acknowledge level (generate report at Acknowledge level and make report prepped status)
                        continue;
                    }

                    foreach (MailPieceDTO mailPiece in validMailPieces)
                    {
                        int attemptCount = 0;
                        try
                        {
                            DocType docType = docTypeSvc.GetDocTypeById(mailPiece.DocTypeId.Value);

                            int mailPieceId = mailPiece.Id;

                            string pdfFilePath = Path.Combine(mbi.AMRequestPath, mailPiece.FileName);
                            string outputFilePath = Path.Combine(mbi.PnSRequestPath, "{0}_{1}".Fill(mailPieceId, mailPiece.FileName));
                            string coverLetterPdfPath = string.Empty;
                            string endPagePdfPath = string.Empty;
                            string endPageTemplatePath = Path.Combine(Config.Core_CoverTemplateFolder, Config.Core_EndPageTemplatePath);

                            #region Prepare dictionary of placeholders/tags to fill the PDF template.
                            string fromAddress = string.Empty;
                            #region From Address
                            if (mailPiece.IsReturnInfoSpecified)
                            {
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

                            string toAddress = string.Empty;
                            #region To Address
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

                            if (docType.ShippingCarrier == Carrier.USPS.ToString())
                            {
                                DocTypeCoverLetter docTypeLetter = docTypeSvc.GetDocumentCoverLetterTemplates(docType.Id).SingleOrDefault();

                                if (docTypeLetter == null)
                                {
                                    mailPiece.ErrorCode = OutComeCodes.Unknown_Error;
                                    mailPiece.ErrorDesc = "Cover letter does not exist for DocumentType : {0}".Fill(mailPiece.DocumentOrPackageType);
                                    mailPiece.ShippingStatus = ShippingStatus.Error.ToString();
                                    mailPiece.TotalPages = 0;
                                    mailPiece.DueDate = null;
                                    mailPiece.PrintLocation = null;
                                    mailPieceSvc.UpdateBatchMailPieceDetails(mailPiece);
                                    continue;
                                }

                                //Folder structure and file naming convention should be Root/AppCode/AppCode_DocTypeCode_SeqNo.pdf
                                string coverLetterTemplatePath = Path.Combine(Config.Core_CoverTemplateFolder, client.AppCode, docTypeLetter.CoverLetterTemplatePath);

                                if (docType.ShippingServiceType == AM.USPSLib.ServiceType.PRIORITY_EXPRESS.ToString())
                                {
                                    eVSLabelRequest eVSReq = GenerateEVSLabelRequest(mailPiece, client.Name);
                                    coverLetterPdfPath = doc.GenerateEVSCoverLetter(client.AppCode, commonSvc.GetSystemUserId(), mbi.PnSRequestPath, mailPiece.Id.ToString(), eVSReq);
                                    //coverLetterPdfPath = doc.GenerateUSPSCoverLetter(client.AppCode, coverLetterTemplatePath, mbi.PnSRequestPath, mailPieceData);
                                }
                                else
                                {
                                    //Create Cover letter with IMBarcode - USPS First Class
                                    coverLetterPdfPath = doc.GenerateUSPSIMBCoverLetter(client.AppCode, coverLetterTemplatePath, mbi.PnSRequestPath, mailPieceData);
                                }

                                if (string.IsNullOrEmpty(coverLetterPdfPath))
                                {
                                    mailPiece.ErrorCode = OutComeCodes.Unknown_Error;
                                    mailPiece.TotalPages = 0;
                                    mailPiece.ShippingStatus = ShippingStatus.Error.ToString();
                                    mailPiece.ErrorDesc = doc.ExceptionDetail;
                                    mailPiece.DueDate = null;
                                    mailPiece.PrintLocation = null;
                                    mailPieceSvc.UpdateBatchMailPieceDetails(mailPiece);
                                    continue;
                                }

                                mailPiece.TrackingNo = doc.TrackingCode;
                                mailPiece.ShippingCost = doc.PostageCost;

                                //For now reverse barcoding will be mail piece id
                                if (mailPiece.PrintLocation == Config.Common_PrintLocationCL)
                                {
                                    mailPieceData.Add("Free3OF9ReverseBarcode", "*" + new string((mailPiece.TrackingNo).Reverse().ToArray()) + "*");

                                    endPagePdfPath = doc.GenerateLastPageWithBarcode(endPageTemplatePath, mbi.PnSRequestPath, mailPieceData);
                                }
                            }
                            else if (docType.ShippingCarrier == Carrier.FedEx.ToString())
                            {
                                DocTypeCoverLetter docTypeLetter = docTypeSvc.GetDocumentCoverLetterTemplates(docType.Id).SingleOrDefault();

                                if (docTypeLetter == null)
                                {
                                    //TODO : Cover letter check for FedEx if not continue with Shipping label only
                                }

                                #region  Call FedEx web service and get create shipping label

                                FedExAccountInfo fedExAccInfo = fedExSvc.GetFedExAccountDetailsByAccountCode(docType.ShippingAccountCode);

                                CreateFedExLabel createFedExLabel = new CreateFedExLabel();
                                bool result = false;
                                while (attemptCount < Config.Core_NoofTime_AttemptFedexService)
                                {
                                    result = createFedExLabel.Create(mailPiece, fedExAccInfo, docType, mbi);
                                    if (result)
                                    {
                                        break;
                                    }
                                    else if (!result && (mailPiece.ErrorDesc.Contains("General Failure") || mailPiece.ErrorDesc.Contains("Unknown_Error")))
                                    {
                                        System.Threading.Thread.Sleep(Config.Core_FedexServiceAttemptInterval);
                                        attemptCount++;
                                    }
                                    else
                                    {
                                        mailPiece.TotalPages = 0;
                                        mailPiece.PrintLocation = null;
                                        mailPiece.DueDate = null;
                                        break;
                                    }
                                }

                                if (result)
                                {
                                    coverLetterPdfPath = createFedExLabel.GeneratedFedExLabelPath;
                                    mailPiece.TrackingNo = Path.GetFileNameWithoutExtension(coverLetterPdfPath);

                                    if (mailPiece.PrintLocation == Config.Common_PrintLocationCL)
                                    {
                                        mailPieceData.Add("Free3OF9ReverseBarcode", "*" + new string((mailPiece.TrackingNo).Reverse().ToArray()) + "*");

                                        endPagePdfPath = doc.GenerateLastPageWithBarcode(endPageTemplatePath, mbi.PnSRequestPath, mailPieceData);
                                    }
                                }

                                #endregion
                            }

                            if (File.Exists(coverLetterPdfPath))
                            {
                                int pageCount = doc.GenerateDoc(coverLetterPdfPath, pdfFilePath, outputFilePath, docType.ContentScalePercent, endPagePdfPath);
                                if (pageCount > 0)
                                {
                                    mailPiece.TotalPages = pageCount;
                                }
                                else
                                {
                                    mailPiece.ErrorCode = OutComeCodes.Invalid_PDF_File;
                                    mailPiece.TotalPages = 0;
                                    mailPiece.DueDate = null;
                                    mailPiece.ShippingStatus = ShippingStatus.Error.ToString();
                                    mailPiece.ErrorDesc = doc.ExceptionDetail;
                                }
                            }
                            mailPieceSvc.UpdateBatchMailPieceDetails(mailPiece);
                        }
                        catch (Exception ex)
                        {
                            log.Error("Error occured while processing mail pieces {0}".Fill(mailPiece.Id), ex);

                            // Send email to support team
                            SendUnexpectedExceptionEmail(ex.ToString());
                            continue;
                        }
                    }

                    Deliverable reportDeliverable = commonSvc.GetDeliverableByReportType(client.Id, ReportTypes.Error);

                    var data = mailBatchSvc.GetMailBatchDetailsByBatchId(mbi.MailBatch.Id);
                    if (data != null)
                    {
                        var validMailPiecesList = data.MailPieces.Where(c => c.ErrorCode == OutComeCodes.No_Error && c.ShippingStatus == ShippingStatus.InProgress.ToString()).ToList();

                        if (validMailPiecesList.Count == 0)
                        {
                            //TODO: Remove after proper logic implementation client wise switch case implemented
                            GenerateImmidiateReports(mbi, data, reportDeliverable);
                            // Batch status Prepped
                            mailBatchSvc.MarkMailBatchAsInvalid(mbi.MailBatch.Id, (mbi.MailBatch.ValidationErrors + Environment.NewLine + "Some documents could not be processed."));
                            commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, AM.Entities.BatchStatus.Failed.ToString());
                            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportPrepped, Common.Now);
                        }
                        else if (validMailPiecesList.Count < data.MailPieces.Count)
                        {
                            commonSvc.UpdateBatchValidationStatus(mbi.MailBatch.Id, BatchValidationStatus.PartiallyValid, data.MailBatch.ValidationErrors);
                            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Prepped, Common.Now);
                        }
                        else
                        {
                            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Prepped, Common.Now);
                        }
                    }
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
            //eVSReq.POZipCode = "75099";
            eVSReq.LabelParameters = new LabelParameters() { Sequence = new LabelSequence() { PackageNumber = 1, TotalPackages = 1 }, Size = LabelSize.CROP };//TODO : Change if required 50

            // From Address
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
            eVSReq.ToName = mailPiece.RcptFirstName + " " + mailPiece.RcptLastName;
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

        private static void GenerateImmidiateReports(MailBatchInfo mbi, BatchData mailBatchData, Deliverable deliverable)
        {
            switch (mbi.Client.AppCode)
            {
                case "USAA":
                    using (IAMCService amcSvc = new AMCService())
                    {
                        string errorReportFileName = "Error-{0}.txt".Fill(Common.Now.ToString("yyyyMMdd-HHmmss"));

                        //TODO: Comment below and uncomment above if don't want to move new naming convention and tab delimited report to Production environment
                        //string errorReportFileName = "Error-{0}-{1}-{2}-{3}.txt".Fill(mbi.BatchName, mbi.Client.AppCode, mbi.MailBatch.Id, Common.Now.ToString("yyyyMMdd-HHmmss"));
                        string errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);
                        StringBuilder errorData = amcSvc.AMCErrorReport(mailBatchData);
                        if (errorData != null)
                        {
                            File.WriteAllText(errorReportPath, errorData.ToString());
                        }
                    }
                    break;
                case "ALASKA":
                    using (IAlaskaService alaskaSvc = new AlaskaService())
                    {
                        string errorReportFileName = "ER_{0}.csv".Fill(mbi.BatchName);
                        string errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);
                        StringBuilder errorData = alaskaSvc.AlaskaErrorReport(mailBatchData, mbi.Client.TimeZone, deliverable.Delimeter);
                        if (errorData != null)
                        {
                            File.WriteAllText(errorReportPath, errorData.ToString());
                        }
                    }
                    break;
                case "HSBC":
                    using (IHSBCService hsbcSvc = new HSBCService())
                    {
                        string errorReportFileName = "{0}.ErrorReport_{1}.csv".Fill(mbi.BatchName.Split('[')[0], mbi.MailBatch.ReceivedOn.Value.UtcToClientTimeZone(mbi.Client.TimeZone).ToString("htt"));
                        string errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);
                        StringBuilder errorData = hsbcSvc.HSBCErrorReport(mailBatchData, mbi.Client.TimeZone, deliverable.Delimeter);
                        if (errorData != null)
                        {
                            File.WriteAllText(errorReportPath, errorData.ToString());
                        }
                    }
                    break;
                default:
                    throw new Exception("Invalid App Code");
            }
        }

        #endregion

        #region SubmitBatchesDocTypeWise
        internal static void SubmitBatchesDocTypeWiseToPCI(string arg)
        {
            using (IMailBatchService mailBatchSvc = new MailBatchService())
            using (IClientService clientSvc = new ClientService())
            using (ICommonService commonSvc = new CommonService())
            using (IMailPieceService mailPieceSvc = new MailPieceService())
            {
                log.Debug("Submitted Batches job started.");

                List<MailBatchDTO> mailBatches = null;
                List<MailPieceDTO> validMailPieces = new List<MailPieceDTO>();

                int[] channelIds = Array.ConvertAll(Config.Core_BundleSweepClientChannelIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries), s => int.Parse(s));

                if (arg == "BUNDLE")
                {
                    mailBatches = mailBatchSvc.GetBatchesByPipelineStatus(BatchPipelineStatus.Prepped).Where(c => channelIds.Contains(c.SFTChannelId)).ToList();

                    if (mailBatches.Count == 0)
                    {
                        log.Debug("Bundle sweep job queue is empty.");
                        return;
                    }

                    foreach (MailBatchDTO mailBatch in mailBatches)
                    {
                        var mailPieces = mailPieceSvc.GetMailPiecesByMailBatchId(mailBatch.Id);
                        validMailPieces.AddRange(mailPieces.Where(c => c.ErrorCode == OutComeCodes.No_Error && c.ShippingStatus == ShippingStatus.InProgress.ToString()).ToList());
                    }
                    try
                    {
                        GeneratePrintBatches(validMailPieces);

                        //After processing all mail batches for PrintBatches we will check for each and every mailpieces are having printbatchid in it or not then update SubmittedToShipping status in MailBatches table
                        foreach (MailBatchDTO mailBatch in mailBatches)
                        {
                            var mailPiecesList = mailPieceSvc.GetMailPiecesByMailBatchId(mailBatch.Id);
                            var validPieces = mailPiecesList.Where(c => c.ErrorCode == OutComeCodes.No_Error && c.PrintBatchId != null).ToList().Count;
                            var errorPieces = mailPiecesList.Where(c => c.ErrorCode != OutComeCodes.No_Error && c.PrintBatchId == null).ToList().Count;

                            if (mailPiecesList.Count == (validPieces + errorPieces))
                            {
                                commonSvc.UpdateBatchPipeLineStatus(mailBatch.Id, BatchPipelineStatus.SubmittedToShipping, Common.Now);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMsg = "Error occured while generationg Bundle Batches IDs : {0} and Error : {1}".Fill(string.Join(",", mailBatches.Select(b => b.Id)), ex);
                        log.Error(errorMsg, ex);

                        SendUnexpectedExceptionEmail(errorMsg);
                    }
                }
                else
                {
                    mailBatches = mailBatchSvc.GetBatchesByPipelineStatus(BatchPipelineStatus.Prepped).Where(c => !channelIds.Contains(c.SFTChannelId)).ToList();

                    foreach (MailBatchDTO mailBatch in mailBatches)
                    {
                        Client client = clientSvc.GetClientById(mailBatch.ClientId);
                        MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, Config.Common_AuthoMailRootFolder);

                        var batchData = mailBatchSvc.GetMailBatchDetailsByBatchId(mbi.MailBatch.Id);

                        validMailPieces = batchData.MailPieces.Where(c => c.ErrorCode == OutComeCodes.No_Error && c.ShippingStatus == ShippingStatus.InProgress.ToString()).ToList();
                        try
                        {
                            GeneratePrintBatches(validMailPieces);

                            //After processing mail batch for PrintBatches we will check for each and every mailpieces are having printbatchid in it or not then update SubmittedToShipping status in MailBatches table
                            var mailPiecesList = mailPieceSvc.GetMailPiecesByMailBatchId(mailBatch.Id);
                            var validPieces = mailPiecesList.Where(c => c.ErrorCode == OutComeCodes.No_Error && c.PrintBatchId != null).ToList().Count;
                            var errorPieces = mailPiecesList.Where(c => c.ErrorCode != OutComeCodes.No_Error && c.PrintBatchId == null).ToList().Count;

                            if (mailPiecesList.Count == (validPieces + errorPieces))
                            {
                                commonSvc.UpdateBatchPipeLineStatus(mailBatch.Id, BatchPipelineStatus.SubmittedToShipping, Common.Now);
                            }
                        }
                        catch (Exception ex)
                        {
                            var errorMsg = "Error occured while generationg PnS Batch - MailBatchId-{0} and Error : {1}".Fill(mailBatch.Id, ex);
                            log.Error(errorMsg, ex);
                            SendUnexpectedExceptionEmail(errorMsg);
                            commonSvc.UpdateBatchPipeLineStatus(mailBatch.Id, BatchPipelineStatus.Suspended, Common.Now);
                        }
                    }
                }
            }
        }

        private static void GeneratePrintBatches(List<MailPieceDTO> validMailPieces)
        {
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

                    var printLocations = filteredMailPiecesDocTypeWise.Select(c => c.PrintLocation).Distinct();

                    foreach (var printLocation in printLocations)
                    {
                        PrintBatch printBatch = new PrintBatch();
                        printBatch.AppCode = clientSvc.GetClientById(docType.ClientId).AppCode;
                        printBatch.UniqueId = Guid.NewGuid();
                        printBatch.DocTypeId = docType.Id;
                        printBatch.PrintLocation = printLocation;
                        var filteredMailPiecesPrintLocationWise = filteredMailPiecesDocTypeWise.Where(c => c.PrintLocation == printLocation).ToList();

                        #region QC Request Logic
                        if (docType.QCPercent > 0)
                        {
                            double qcCount = 0;
                            float qcPercentage = docType.QCPercent / 100;
                            qcCount = Math.Ceiling(filteredMailPiecesPrintLocationWise.Count * qcPercentage);

                            var randomIndexes = new List<int>();
                            while (randomIndexes.Count < qcCount)
                            {
                                var index = random.Next(0, filteredMailPiecesPrintLocationWise.Count - 1);
                                if (!randomIndexes.Contains(index))
                                {
                                    randomIndexes.Add(index);
                                }
                            }

                            randomIndexes.ForEach(index =>
                            {
                                filteredMailPiecesPrintLocationWise[index].IsQCRequested = true;
                            });
                        }
                        #endregion

                        printBatch.MailPieces = filteredMailPiecesPrintLocationWise;

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
                    List<string> responseFiles;
                    var clientAppCodes = submittedPrintBatches.Select(client => client.AppCode).Distinct();
                    foreach (var appCode in clientAppCodes)
                    {
                        Client client = clientSvc.GetClientByAppCode(appCode);

                        responseFiles = Directory.EnumerateFiles(Path.Combine(Config.Core_PnS_SFT_RootDir, client.AppCode), "*.rpt").ToList();

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

                        var downloadableFiles = submittedPrintBatches.Where(c => !string.IsNullOrEmpty(c.ResponseFilePath)).ToList();

                        if (downloadableFiles.Count == 0)
                        {
                            log.Debug("Response files are not available to download.");
                            continue;
                        }

                        foreach (var downloadableBatch in downloadableFiles)
                        {
                            string responseTriggerFilePath = downloadableBatch.ResponseTriggerFilePath;
                            string responseFileName = Path.GetFileName(downloadableBatch.ResponseFilePath);
                            string responseFilePath = downloadableBatch.ResponseFilePath;

                            string amPnSResponseDir = Path.Combine(Config.Common_AuthoMailRootFolder, downloadableBatch.AppCode, "PnSResponses");
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
                }
                catch (Exception ex)
                {
                    var errorMsg = "Error occured while downloading response from PnS Service : {0) ".Fill(ex);
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
            {
                try
                {
                    var responseReceivedPrintBatches = mailBatchSvc.GetResponseReceivedPrintBatchDetails();

                    if (responseReceivedPrintBatches.Count == 0)
                    {
                        log.Debug("Response process job queue is empty.");
                        return;
                    }

                    foreach (var printBatch in responseReceivedPrintBatches)
                    {
                        Client client = clientSvc.GetClientByAppCode(printBatch.AppCode);

                        var printBatchMailPieces = mailPieceSvc.GetMailPiecesByPrintBatchId(printBatch.PrintBatchId);

                        string printbatchResponseFilePath = Path.Combine(Config.Common_AuthoMailRootFolder, printBatch.AppCode, "PnSResponses", printBatch.SubmittedBatchName + "-res.xml");

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
                                    }
                                    else if (mailPackage.MailPieceStatus == MailPieceStatus.PulledBack)
                                    {
                                        row.PulledBackOn = mailPackage.PulledBackOn;
                                    }
                                    else if (mailPackage.MailPieceStatus == MailPieceStatus.Shipped)
                                    {
                                        row.PrintedOn = mailPackage.PrintedOn;
                                        row.ShippedOn = mailPackage.ShippedOn;
                                        row.ShippingCost = mailPackage.PostageCost;
                                        row.TotalSheets = mailPackage.TotalSheets;
                                        row.Weight = mailPackage.WeightMeasured;
                                        row.IsQCPerformed = mailPackage.IsQCPerformed;
                                        if (!string.IsNullOrEmpty(mailPackage.TrackingNumber))
                                        {
                                            row.TrackingNo = mailPackage.TrackingNumber;
                                        }
                                    }
                                    row.ShippingStatus = mailPackage.MailPieceStatus.ToString();

                                    mailPieceSvc.UpdateBatchMailPieceShippingStatusDetails(row);

                                    //because of removed MailBatchId we need to check each and every mailpiece' batchid to check we have received response or not for submitted batch to PCI
                                    var batchData = mailBatchSvc.GetMailBatchDetailsByBatchId(row.MailBatchId);
                                    var pendingMailPieces = batchData.MailPieces.Count(data => data.ShippingStatus == ShippingStatus.InProgress.ToString());

                                    if (pendingMailPieces == 0)
                                    {
                                        if (batchData.MailPieces.All(data => data.ShippingStatus == ShippingStatus.Shipped.ToString()))
                                        {
                                            commonSvc.UpdateBatchStatus(row.MailBatchId, AM.Entities.BatchStatus.ShippedAll.ToString());
                                        }
                                        else if (batchData.MailPieces.All(data => data.ShippingStatus == ShippingStatus.PulledBack.ToString()))
                                        {
                                            commonSvc.UpdateBatchStatus(row.MailBatchId, AM.Entities.BatchStatus.PulledBack.ToString());
                                        }
                                        else if (batchData.MailPieces.All(data => data.ShippingStatus == ShippingStatus.Error.ToString()))
                                        {
                                            commonSvc.UpdateBatchStatus(row.MailBatchId, AM.Entities.BatchStatus.Failed.ToString());
                                        }
                                        else
                                        {
                                            commonSvc.UpdateBatchStatus(row.MailBatchId, AM.Entities.BatchStatus.ShippedPartial.ToString());
                                        }

                                        commonSvc.UpdateBatchPipeLineStatus(row.MailBatchId, BatchPipelineStatus.ShippingResponseReceived, Common.Now);
                                    }
                                }
                            });
                        }

                        //Updates PrintBatches Status values
                        commonSvc.UpdatePrintBatchStatus(printBatch.PrintBatchId, pnsResponse.BatchStatus.ToString(), pnsResponse.StatusDescription);
                    }
                }
                catch (Exception ex)
                {
                    var errorMsg = "Error occured while processing PnS response : {0} ".Fill(ex);
                    log.Error(errorMsg, ex);
                    SendUnexpectedExceptionEmail(errorMsg);
                }
            }
            log.Debug("Process PnS response job is completed.");
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
                List<MailBatchDTO> resReceivedBatches = mailBatchSvc.GetBatchesByPipelineStatus(BatchPipelineStatus.ShippingResponseReceived);
                if (resReceivedBatches.Count == 0)
                {
                    log.Debug("Report prepped job queue is empty.");
                    return;
                }

                foreach (var batch in resReceivedBatches)
                {
                    Client client = clientSvc.GetClientById(batch.ClientId);

                    Deliverable deliverable = commonSvc.GetDeliverableByReportType(client.Id, ReportTypes.Delivery);

                    MailBatchInfo mbi = new MailBatchInfo(batch, client, Config.Common_AuthoMailRootFolder);

                    var data = mailBatchSvc.GetMailBatchDetailsByBatchId(mbi.MailBatch.Id);
                    if (data != null)
                    {
                        try
                        {
                            bool isReportGenerated = false;

                            isReportGenerated = GenerateFinalReports(mbi, data, deliverable);

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
            }

            log.Debug("Report prepped job is completed.");
        }

        private static bool GenerateFinalReports(MailBatchInfo mbi, BatchData mailBatchData, Deliverable deliverable)
        {
            bool isReportGenerated = false;
            switch (mbi.Client.AppCode)
            {
                case "USAA":
                    using (IAMCService amcSvc = new AMCService())
                    {
                        StringBuilder deliveryReport = amcSvc.AMCDeliveryReport(mailBatchData);

                        if (deliveryReport != null)
                        {
                            string deliveryReportFileName = "Delivery-{0}.txt".Fill(Common.Now.ToString("yyyyMMdd-HHmmss"));

                            //TODO: Comment below and uncomment above if don't want to move new naming convention and tab delimited report to Production environment
                            //string deliveryReportFileName = "Delivery-{0}-{1}-{2}-{3}.txt".Fill(mbi.BatchName, mbi.Client.AppCode, mbi.MailBatch.Id, Common.Now.ToString("yyyyMMdd-HHmmss"));
                            string deliveryReportPath = Path.Combine(mbi.AMResponsePath, deliveryReportFileName);

                            File.WriteAllText(deliveryReportPath, deliveryReport.ToString());
                            isReportGenerated = true;
                        }

                        StringBuilder errorReport = amcSvc.AMCErrorReport(mailBatchData);

                        if (errorReport != null)
                        {
                            string errorReportFileName = "Error-{0}.txt".Fill(Common.Now.ToString("yyyyMMdd-HHmmss"));

                            //TODO: Comment below and uncomment above if don't want to move new naming convention and tab delimited report to Production environment
                            //string errorReportFileName = "Error-{0}-{1}-{2}-{3}.txt".Fill(mbi.BatchName, mbi.Client.AppCode, mbi.MailBatch.Id, Common.Now.ToString("yyyyMMdd-HHmmss"));
                            string errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);

                            File.WriteAllText(errorReportPath, errorReport.ToString());
                            isReportGenerated = true;
                        }
                    }
                    break;
                case "ALASKA":
                    using (IAlaskaService alaskaSvc = new AlaskaService())
                    {
                        StringBuilder deliveryReport = alaskaSvc.AlaskaDeliveryReport(mailBatchData, mbi.Client.TimeZone, deliverable.Delimeter);

                        if (deliveryReport != null)
                        {
                            string deliveryReportFileName = "DR_{0}.csv".Fill(mbi.BatchName);
                            string deliveryReportPath = Path.Combine(mbi.AMResponsePath, deliveryReportFileName);

                            File.WriteAllText(deliveryReportPath, deliveryReport.ToString());
                            isReportGenerated = true;
                        }

                        StringBuilder errorReport = alaskaSvc.AlaskaErrorReport(mailBatchData, mbi.Client.TimeZone, deliverable.Delimeter);

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
                case "AMTEST":
                    using (IHSBCService hsbcSvc = new HSBCService())
                    {
                        StringBuilder deliveryReport = hsbcSvc.HSBCDeliveryReport(mailBatchData, mbi.Client.TimeZone, deliverable.Delimeter);

                        string deliveryReportFileName = "{0}.DeliveryReport_{1}.csv".Fill(mbi.BatchName.Split('[')[0], mbi.MailBatch.ReceivedOn.Value.UtcToClientTimeZone(mbi.Client.TimeZone).ToString("htt"));
                        string deliveryReportPath = Path.Combine(mbi.AMResponsePath, deliveryReportFileName);

                        File.WriteAllText(deliveryReportPath, deliveryReport.ToString());


                        StringBuilder errorReport = hsbcSvc.HSBCErrorReport(mailBatchData, mbi.Client.TimeZone, deliverable.Delimeter);

                        string errorReportFileName = "{0}.ErrorReport_{1}.csv".Fill(mbi.BatchName.Split('[')[0], mbi.MailBatch.ReceivedOn.Value.UtcToClientTimeZone(mbi.Client.TimeZone).ToString("htt"));
                        string errorReportPath = Path.Combine(mbi.AMResponsePath, errorReportFileName);

                        File.WriteAllText(errorReportPath, errorReport.ToString());


                        isReportGenerated = true;
                    }
                    break;
                default:
                    throw new Exception("Invalid App Code");
            }
            return isReportGenerated;
        }
        #endregion
    }
}
