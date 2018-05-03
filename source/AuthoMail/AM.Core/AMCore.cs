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

                    List<MailPieceDTO> validMailPieces = batchData.MailPieces.Where(c => c.ErrorCode == OutComeCodes.No_Error).ToList();

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

                            mailPiece.DueDate = GetDueDate(docType.SLADays, docType.SLACutOffTime, mailBatch.ReceivedOn.Value);
                            //mailPiece.DocTypeId = docType.Id;

                            int mailPieceId = mailPiece.Id;

                            string pdfFilePath = Path.Combine(mbi.AMRequestPath, mailPiece.FileName);
                            string outputFilePath = Path.Combine(mbi.PnSRequestPath, "{0}_{1}".Fill(mailPieceId, mailPiece.FileName));
                            string coverLetterPdfPath = string.Empty;

                            if (docType.ShippingCarrier == Carrier.USPS.ToString())
                            {
                                #region Prepare dictionary of placeholders/tags to fill the PDF template.
                                string fromAddress = string.Empty;
                                if (mailPiece.IsReturnInfoSpecified)
                                {
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

                                Dictionary<string, string> mailPieceData = new Dictionary<string, string>();
                                mailPieceData.Add("FromAddress", fromAddress);

                                mailPieceData.Add("ToAddress", mailPiece.RcptFirstName + " " + mailPiece.RcptLastName + "\n" + (string.IsNullOrEmpty(mailPiece.RcptAddressAptOrSuiteOrOther) ? mailPiece.RcptAddressStreet : string.Format("{0}\n{1}", mailPiece.RcptAddressStreet, mailPiece.RcptAddressAptOrSuiteOrOther)) + "\n" + mailPiece.RcptAddressCity + ", " + mailPiece.RcptAddressState + ", " + mailPiece.RcptAddressZIP);

                                mailPieceData.Add("MailPieceId", mailPiece.Id.ToString());
                                mailPieceData.Add("Date", Common.Now.UtcToEst().ToString("MMMM dd, yyyy"));//USAA Specific
                                mailPieceData.Add("MailPieceZip", mailPiece.RcptAddressZIP);
                                mailPieceData.Add("LoanNbr", mailPiece.BusinessFriendlyIdValue);

                                #endregion

                                DocTypeCoverLetter docTypeLetter = docTypeSvc.GetDocumentCoverLetterTemplates(docType.Id).SingleOrDefault();

                                if (docTypeLetter == null)
                                {
                                    mailPiece.ErrorCode = OutComeCodes.Unknown_Error;
                                    mailPiece.ErrorDesc = "Cover letter does not exist for DocumentType : {0}".Fill(mailPiece.DocumentOrPackageType);
                                    mailPiece.ShippingStatus = ShippingStatus.Error.ToString();
                                    mailPiece.TotalPages = 0;
                                    mailPieceSvc.UpdateBatchMailPieceDetails(mailPiece);
                                    continue;
                                }

                                //Folder structure and file naming convention should be Root/AppCode/AppCode_DocTypeCode_SeqNo.pdf
                                string coverLetterTemplatePath = Path.Combine(Config.Core_CoverTemplateFolder, client.AppCode, docTypeLetter.CoverLetterTemplatePath);

                                // Create final document using SmartDocs library.
                                coverLetterPdfPath = doc.GenerateUSPSCoverLetter(client.AppCode, coverLetterTemplatePath, mbi.PnSRequestPath, mailPieceData);

                                if (string.IsNullOrEmpty(coverLetterPdfPath))
                                {
                                    mailPiece.ErrorCode = OutComeCodes.Unknown_Error;
                                    mailPiece.TotalPages = 0;
                                    mailPiece.ShippingStatus = ShippingStatus.Error.ToString();
                                    mailPiece.ErrorDesc = doc.ExceptionDetail;
                                    mailPieceSvc.UpdateBatchMailPieceDetails(mailPiece);
                                    continue;
                                }

                                mailPiece.TrackingNo = doc.TrackingCode;
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
                                    else if (!result && mailPiece.ErrorDesc.Contains("General Failure"))
                                    {
                                        System.Threading.Thread.Sleep(Config.Core_FedexServiceAttemptInterval);
                                        attemptCount++;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                if (result)
                                {
                                    coverLetterPdfPath = createFedExLabel.GeneratedFedExLabelPath;
                                    mailPiece.TrackingNo = Path.GetFileNameWithoutExtension(coverLetterPdfPath);
                                }

                                #endregion
                            }

                            if (File.Exists(coverLetterPdfPath))
                            {
                                int pageCount = doc.GenerateDoc(coverLetterPdfPath, pdfFilePath, outputFilePath, docType.ContentScalePercent);
                                if (pageCount > 0)
                                {
                                    mailPiece.TotalPages = pageCount;
                                }
                                else
                                {
                                    mailPiece.ErrorCode = OutComeCodes.Invalid_PDF_File;
                                    mailPiece.TotalPages = 0;
                                    mailPiece.ShippingStatus = ShippingStatus.Error.ToString();
                                    mailPiece.ErrorDesc = doc.ExceptionDetail;
                                }
                            }
                            mailPieceSvc.UpdateBatchMailPieceDetails(mailPiece);
                        }
                        catch (Exception ex)
                        {
                            log.Error("Error occured while processing mail pieces {0}".Fill(mailPiece.Id), ex);
                            continue;
                        }
                    }

                    Deliverable reportDeliverable = commonSvc.GetDeliverableByReportType(client.Id, ReportTypes.Error);

                    var data = mailBatchSvc.GetMailBatchDetailsByBatchId(mbi.MailBatch.Id);
                    if (data != null)
                    {
                        var validMailPiecesList = data.MailPieces.Where(c => c.ErrorCode == OutComeCodes.No_Error).ToList();

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

        private static void GenerateImmidiateReports(MailBatchInfo mbi, BatchData mailBatchData, Deliverable deliverable)
        {
            switch (mbi.Client.AppCode)
            {
                case "USAA":
                    using (IAMCService amcSvc = new AMCService())
                    {
                        string errorReportFileName = "Error-{0}.csv".Fill(Common.Now.ToString("yyyyMMdd-HHmmss"));
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
                        string errorReportFileName = "{0}.ErrorReport_{1}.csv".Fill(mbi.BatchName, mbi.MailBatch.ReceivedOn.Value.UtcToClientTimeZone(mbi.Client.TimeZone).ToString("htt"));
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

        private static DateTime GetDueDate(int slaDays, TimeSpan estSLACutOffTime, DateTime utcReceivedOn)
        {
            //ReceivedOn Time of Batch
            var estReceivedOn = utcReceivedOn.UtcToEst();

            var estReceivedOnDate = estReceivedOn.Date;

            //Should be in cut off Date time
            var estSLACutOffDateTime = (estReceivedOnDate + estSLACutOffTime);

            if (estReceivedOn > estSLACutOffDateTime)
            {
                //Here we have actual SLA days for DocType and added 1 because batch received after cut off time.
                slaDays += 1;
            }

            //Here we just add actual SLA days because batch received before cut off time
            return (estReceivedOnDate.AddDays(slaDays) + new TimeSpan(23, 59, 59)).EstToUtc();
        }
        #endregion

        internal static void SubmitBatchesDocTypeWiseToPCI()
        {
            using (IMailBatchService mailBatchSvc = new MailBatchService())
            using (IClientService clientSvc = new ClientService())
            using (ICommonService commonSvc = new CommonService())
            using (IMailPieceService mailPieceSvc = new MailPieceService())
            using (IDocTypeService docTypeSvc = new DocTypeService())
            {
                log.Debug("Submitted Batches job started.");

                var mailBatches = mailBatchSvc.GetBatchesByPipelineStatus(BatchPipelineStatus.Prepped);

                if (mailBatches.Count == 0)
                {
                    log.Debug("Submit batch Job queue is empty.");
                    return;
                }

                foreach (MailBatchDTO mailBatch in mailBatches)
                {
                    Client client = clientSvc.GetClientById(mailBatch.ClientId);
                    MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, Config.Common_AuthoMailRootFolder);

                    var batchData = mailBatchSvc.GetMailBatchDetailsByBatchId(mbi.MailBatch.Id);

                    List<MailPieceDTO> validMailPieces = batchData.MailPieces.Where(c => c.ErrorCode == OutComeCodes.No_Error).ToList();

                    //TODO : look into this logic
                    if (validMailPieces.Count == 0)
                    {
                        mailBatch.ValidationStatus = BatchValidationStatus.Invalid;
                        mailBatch.ValidationErrors = "Batch does not contain any valid mail pieces to process further.";

                        mailBatchSvc.UpdateBatchDetails(mailBatch);
                        commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, AM.Entities.BatchStatus.Failed.ToString());
                        continue;
                    }

                    try
                    {
                        List<PrintBatch> printBatches = new List<PrintBatch>();

                        var docTypes = validMailPieces.Select(row => row.DocTypeId.Value).Distinct();
                        foreach (var docTypeId in docTypes)
                        {
                            DocType docType = docTypeSvc.GetDocTypeById(docTypeId);

                            PrintBatch printBatch = new PrintBatch();
                            printBatch.AppCode = mbi.Client.AppCode;
                            printBatch.UniqueId = Guid.NewGuid();
                            printBatch.MailBatchId = mbi.MailBatch.Id;
                            printBatch.DocTypeId = docType.Id;

                            var filteredMailPieces = validMailPieces.Where(c => c.DocTypeId == docTypeId).ToList();

                            #region QC Request Logic
                            if (docType.QCPercent > 0)
                            {
                                double qcCount = 0;
                                float qcPercentage = docType.QCPercent / 100;
                                qcCount = Math.Ceiling(filteredMailPieces.Count * qcPercentage);

                                var randomIndexes = new List<int>();
                                while (randomIndexes.Count < qcCount)
                                {
                                    var index = random.Next(0, filteredMailPieces.Count - 1);
                                    if (!randomIndexes.Contains(index))
                                    {
                                        randomIndexes.Add(index);
                                    }
                                }

                                randomIndexes.ForEach(index =>
                                {
                                    filteredMailPieces[index].IsQCRequested = true;
                                });
                            }
                            #endregion

                            printBatch.MailPieces = filteredMailPieces;

                            printBatches.Add(printBatch);
                        }

                        foreach (var printBatchData in printBatches)
                        {
                            printBatchData.Id = mailBatchSvc.InsertPrintBatchDetails(printBatchData);

                            string pnsRootFolder = Config.Core_PnS_SFT_RootDir;

                            //Batch naming convention will be AppCode_PrintBatchId for Batch folder, XML and Go file
                            string batchName = "{0}_{1}".Fill(mbi.Client.AppCode, printBatchData.Id);
                            string xmlFileName = batchName + ".xml";
                            string goFileName = batchName + ".go";

                            string remoteXMLPath = Path.Combine(pnsRootFolder, mbi.Client.AppCode, xmlFileName);
                            string remoteGOPath = Path.Combine(pnsRootFolder, mbi.Client.AppCode, goFileName);
                            string remoteBatchPath = Path.Combine(pnsRootFolder, mbi.Client.AppCode, batchName);

                            remoteBatchPath.CleanDirectory();

                            PnSRequest request = new PnSRequest();
                            request.AppCode = printBatchData.AppCode;
                            request.BatchIdentifier = printBatchData.UniqueId;
                            request.BatchName = batchName;
                            request.DocTypeId = printBatchData.DocTypeId;
                            request.SLADays = 0;
                            request.OnValidationErrors = OnValidationErrors.ProcessValidFiles;

                            List<MailPieceData> mailPackage = new List<MailPieceData>();
                            foreach (var piece in printBatchData.MailPieces)
                            {
                                string fileName = "{0}_{1}".Fill(piece.Id, piece.FileName);
                                string localFilePath = Path.Combine(mbi.PnSRequestPath, fileName);
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
                        //TODO : Can we check condition for every mail piece has printbatch id and 
                        commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.SubmittedToShipping, Common.Now);
                    }
                    catch (Exception ex)
                    {
                        log.Error("Error occured while generaing PnSRequest :" + ex);
                    }
                }
            }
        }

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
                            string pnsResponseTriggerFilePath = Path.Combine(Config.Core_PnS_SFT_RootDir, client.AppCode, triggerFile.Value);
                            if (File.Exists(pnsResponseFilePath))
                            {
                                submittedPrintBatches.ForEach(row =>
                                {
                                    if (row.SubmittedBatchName == triggerFile.Key)
                                    {
                                        row.ResponseFilePath = pnsResponseFilePath;
                                        row.ResponseTriggerFilePath = pnsResponseTriggerFilePath;
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


                            string amRootFilePath = Path.Combine(Config.Common_AuthoMailRootFolder, responseFileName);

                            DateTime mrdfDownloadStartTime = Common.Now;
                            DateTime responseReceivedOn = File.GetLastWriteTimeUtc(responseFilePath);

                            File.Copy(responseFilePath, amRootFilePath, true);

                            if (File.Exists(responseTriggerFilePath))
                                File.Delete(responseTriggerFilePath);

                            if (File.Exists(amRootFilePath))
                            {
                                var batchData = mailBatchSvc.GetMailBatchDetailsByBatchId(downloadableBatch.MailBatchId);
                                var mbi = new MailBatchInfo(batchData.MailBatch, client, Config.Common_AuthoMailRootFolder);

                                string amPnSResponsePath = Path.Combine(mbi.PnSResponsePath, responseFileName);

                                File.Move(amRootFilePath, amPnSResponsePath);

                                commonSvc.UpdatePrintBatchPipeLineStatus(downloadableBatch.PrintBatchId, BatchPipelineStatus.ShippingResponseReceived, responseReceivedOn);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error occured while downloading response from PnS Service :", ex);
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

                        BatchData batchData = mailBatchSvc.GetMailBatchDetailsByBatchId(printBatch.MailBatchId);

                        MailBatchInfo mbi = new MailBatchInfo(batchData.MailBatch, client, Config.Common_AuthoMailRootFolder);

                        var printbatchResponseFilePath = Path.Combine(mbi.PnSResponsePath, printBatch.SubmittedBatchName + "-res.xml");

                        var pnsResponse = PnSResponse.LoadXml(File.ReadAllText(printbatchResponseFilePath));

                        foreach (var mailPackage in pnsResponse.MailPackageReport)
                        {
                            batchData.MailPieces.ForEach(row =>
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
                                    }
                                    row.ShippingStatus = mailPackage.MailPieceStatus.ToString();

                                    mailPieceSvc.UpdateBatchMailPieceShippingStatusDetails(row);
                                }
                            });
                        }

                        //Updates PrintBatches Status values
                        commonSvc.UpdatePrintBatchStatus(printBatch.PrintBatchId, pnsResponse.BatchStatus.ToString(), pnsResponse.StatusDescription);

                        //                InProgress,
                        //PulledBack,
                        //Error,
                        //Shipped,

                        //                 InProgress,
                        //Failed,
                        //ShippedPartial,
                        //ShippedAll,
                        //PulledBack
                        //Check is there any remaining data for response which is in progress then after update whole mail batch status
                        batchData = mailBatchSvc.GetMailBatchDetailsByBatchId(printBatch.MailBatchId);
                        var pendingMailPieces = batchData.MailPieces.Count(data => data.ShippingStatus == ShippingStatus.InProgress.ToString());

                        if (pendingMailPieces == 0)
                        {

                            if (batchData.MailPieces.All(data => data.ShippingStatus == ShippingStatus.Shipped.ToString()))
                            {
                                commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, AM.Entities.BatchStatus.ShippedAll.ToString());
                            }
                            else if (batchData.MailPieces.All(data => data.ShippingStatus == ShippingStatus.PulledBack.ToString()))
                            {
                                commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, AM.Entities.BatchStatus.PulledBack.ToString());
                            }
                            else if (batchData.MailPieces.All(data => data.ShippingStatus == ShippingStatus.Error.ToString()))
                            {
                                commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, AM.Entities.BatchStatus.Failed.ToString());
                            }
                            else
                            {
                                commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, AM.Entities.BatchStatus.ShippedPartial.ToString());
                            }

                            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ShippingResponseReceived, Common.Now);
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error occured while processing PnS response.", ex);
                }
            }
            log.Debug("Process PnS response job is completed.");
        }

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
                            log.Error("Error occured while generating reports : {0}".Fill(ex.ToString()));
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
                            string deliveryReportFileName = "Delivery-{0}.csv".Fill(Common.Now.ToString("yyyyMMdd-HHmmss"));
                            string deliveryReportPath = Path.Combine(mbi.AMResponsePath, deliveryReportFileName);

                            File.WriteAllText(deliveryReportPath, deliveryReport.ToString());
                            isReportGenerated = true;
                        }

                        StringBuilder errorReport = amcSvc.AMCErrorReport(mailBatchData);

                        if (errorReport != null)
                        {
                            string errorReportFileName = "Error-{0}.csv".Fill(Common.Now.ToString("yyyyMMdd-HHmmss"));
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
                    using (IHSBCService hsbcSvc = new HSBCService())
                    {
                        StringBuilder deliveryReport = hsbcSvc.HSBCDeliveryReport(mailBatchData, mbi.Client.TimeZone, deliverable.Delimeter);

                        string deliveryReportFileName = "{0}.DeliveryReport_{1}.csv".Fill(mbi.BatchName, mbi.MailBatch.ReceivedOn.Value.UtcToClientTimeZone(mbi.Client.TimeZone).ToString("htt"));
                        string deliveryReportPath = Path.Combine(mbi.AMResponsePath, deliveryReportFileName);

                        File.WriteAllText(deliveryReportPath, deliveryReport.ToString());


                        StringBuilder errorReport = hsbcSvc.HSBCErrorReport(mailBatchData, mbi.Client.TimeZone, deliverable.Delimeter);

                        string errorReportFileName = "{0}.ErrorReport_{1}.csv".Fill(mbi.BatchName, mbi.MailBatch.ReceivedOn.Value.UtcToClientTimeZone(mbi.Client.TimeZone).ToString("htt"));
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
    }
}
