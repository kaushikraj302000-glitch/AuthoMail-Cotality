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

                if (mailBatches == null || mailBatches.Count == 0)
                {
                    log.Debug("Prep Job queue is empty.");
                    return;
                }

                foreach (MailBatchDTO mailBatch in mailBatches)
                {
                    SmartDocs doc = new SmartDocs();
                    Client client = clientSvc.GetClientById(mailBatch.ClientId);
                    MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, Config.AuthoMailRootFolder);

                    var batchData = mailBatchSvc.GetMailBatchDetailsByBatchId(mbi.MailBatch.Id);

                    List<MailPieceDTO> validMailPieces = batchData.MailPieces.Where(c => c.ErrorCode == OutComeCodes.No_Error).ToList();

                    foreach (MailPieceDTO mailPiece in validMailPieces)
                    {
                        int count = 0;
                        try
                        {
                            DocType docType = docTypeSvc.GetDocTypesByDocTypeName(mbi.Client.Id, mailPiece.DocumentOrPackageType.Trim());

                            if (docType == null)
                            {
                                mailPiece.ErrorCode = OutComeCodes.Invalid_PackageType;
                                mailPiece.ErrorDesc = "Document Or Package type does not exist.";
                                mailPiece.ShippingStatus = ShippingStatus.Error.ToString();
                                mailPieceSvc.UpdateBatchMailPieceDetails(mailPiece);
                                continue;
                            }

                            mailPiece.DueDate = GetDueDate(docType.SLADays, docType.SLACutOffTime, mailBatch.ReceivedOn.Value);
                            mailPiece.DocTypeId = docType.Id;

                            int mailPieceId = mailPiece.Id;

                            string pdfFilePath = Path.Combine(mbi.AMRequestPath, mailPiece.FileName);
                            string outputFilePath = Path.Combine(mbi.PnSRequestPath, "{0}_{1}".Fill(mailPieceId, mailPiece.FileName));
                            string coverLetterPdfPath = string.Empty;

                            if (docType.ShippingCarrier == Carrier.USPS.ToString())
                            {
                                #region Prepare dictionary of placeholders/tags to fill the PDF template.

                                Dictionary<string, string> mailPieceData = new Dictionary<string, string>();
                                mailPieceData.Add("FromAddress", mailPiece.RetRcptAddressStreet + "\n" + mailPiece.RetRcptAddressAptOrSuiteOrOther + "\n" + mailPiece.RetRcptAddressCity + ", " + mailPiece.RetRcptAddressState + ", " + mailPiece.RetRcptAddressZIP);

                                mailPieceData.Add("ToAddress", mailPiece.RcptFirstName + " " + mailPiece.RcptLastName + "\n" + (string.IsNullOrEmpty(mailPiece.RcptAddressAptOrSuiteOrOther) ? mailPiece.RcptAddressStreet : string.Format("{0}\n{1}", mailPiece.RcptAddressStreet, mailPiece.RcptFirstName)) + "\n" + mailPiece.RcptAddressCity + ", " + mailPiece.RcptAddressState + ", " + mailPiece.RcptAddressZIP);

                                mailPieceData.Add("MailPieceId", mailPiece.Id.ToString());
                                mailPieceData.Add("Date", DateTime.UtcNow.UtcToEst().ToString("MMMM dd, yyyy"));
                                mailPieceData.Add("MailPieceZip", mailPiece.RcptAddressZIP);
                                mailPieceData.Add("LoanNbr", mailPiece.BusinessFriendlyIdValue);

                                #endregion

                                DocTypeCoverLetter docTypeLetter = docTypeSvc.GetDocumentCoverLetterTemplates(docType.Id).SingleOrDefault();

                                if (docTypeLetter == null)
                                {
                                    mailPiece.ErrorCode = OutComeCodes.Unknown_Error;
                                    mailPiece.ErrorDesc = "Cover letter does not exist for DocumentType : {0}".Fill(mailPiece.DocumentOrPackageType);
                                    mailPiece.ShippingStatus = ShippingStatus.Error.ToString();
                                    mailPieceSvc.UpdateBatchMailPieceDetails(mailPiece);
                                    continue;
                                }

                                //Folder structure and file naming convention should be Root/AppCode/DocTypeId/AppCode_DocTypeId_SeqNo.pdf
                                string coverLetterTemplatePath = Path.Combine(Config.CoverTemplateFolder, client.AppCode, docType.Id.ToString(), docTypeLetter.CoverLetterTemplatePath);

                                // Create final document using SmartDocs library.
                                coverLetterPdfPath = doc.GenerateUSPSCoverLetter(coverLetterTemplatePath, mbi.PnSRequestPath, mailPieceData);

                                if (string.IsNullOrEmpty(coverLetterPdfPath))
                                {
                                    mailPiece.ErrorCode = OutComeCodes.Unknown_Error;
                                    mailPiece.TotalPages = 0;
                                    mailPiece.ShippingStatus = ShippingStatus.Error.ToString();
                                    mailPiece.ErrorDesc = doc.ExceptionDetail;
                                }
                                else
                                {
                                    mailPiece.TrackingNo = doc.TrackingCode;
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

                                if (fedExAccInfo != null)
                                {
                                    CreateFedExLabel createFedExLabel = new CreateFedExLabel();
                                    bool result = false;
                                    while (count < Config.NoofTime_AttemptFedexService)
                                    {
                                        result = createFedExLabel.Create(mailPiece, fedExAccInfo, docType, mbi);
                                        if (result)
                                        {
                                            break;
                                        }
                                        else if (result == false && mailPiece.ErrorDesc == "Invalid_shipping_detail|General Failure")
                                        {
                                            System.Threading.Thread.Sleep(Config.FedexServiceAttemptInterval);
                                            count++;
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
                                }
                                else
                                {
                                    // FedEx account information not found; so report it as an error.
                                    mailPiece.ErrorCode = OutComeCodes.Invalid_FedExAccount;
                                    mailPiece.ErrorDesc += "FedEx account does not exist for provided client code.";
                                }

                                #endregion
                            }

                            if (!string.IsNullOrEmpty(coverLetterPdfPath))
                            {
                                int pageCount = doc.GenerateDoc(coverLetterPdfPath, pdfFilePath, outputFilePath, docType.ContentScalePercent);
                                if (pageCount > 0)
                                {
                                    mailPiece.TotalPages = pageCount;
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
                            log.Error(err);
                        }
                        mailPieceSvc.UpdateBatchMailPieceDetails(mailPiece);
                    }

                    commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.Prepped, Common.Now);
                    commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, AM.Entities.BatchStatus.InProgress.ToString());
                    commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.PreppedEnd, Common.Now);
                }
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
            using (IFedExService fedExSvc = new FedExService())
            {
                log.Debug("Submitted Batches job started.");
                List<MailBatchDTO> mailBatches = mailBatchSvc.GetBatchesByPipelineStatus(BatchPipelineStatus.Prepped).Where(x => x.BatchStatus != AM.Entities.BatchStatus.Failed.ToString()).ToList();

                if (mailBatches == null || mailBatches.Count == 0)
                {
                    log.Debug("Submit batch Job queue is empty.");
                    return;
                }

                foreach (MailBatchDTO mailBatch in mailBatches)
                {
                    Client client = clientSvc.GetClientById(mailBatch.ClientId);
                    if (client == null)
                    {
                        log.Error("Client of Batch : {0}, is not enabled for PnS service".Fill(mailBatch.BatchName));
                        continue;
                    }

                    MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, Config.AuthoMailRootFolder);

                    var batchData = mailBatchSvc.GetMailBatchDetailsByBatchId(mbi.MailBatch.Id);

                    List<MailPieceDTO> validMailPieces = batchData.MailPieces.Where(c => c.ErrorCode == OutComeCodes.No_Error).ToList();

                    commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.SubmittedToShippingStart, Common.Now);

                    if (validMailPieces.Count == 0)
                    {
                        mailBatch.ValidationStatus = BatchValidationStatus.Invalid;
                        mailBatch.ValidationErrors = "Batch does not contain any valid mail pieces to process further.";

                        mailBatchSvc.UpdateBatchDetails(mailBatch);
                        commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.ValidationEnd, Common.Now);
                        commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, AM.Entities.BatchStatus.Failed.ToString());
                        commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.SubmittedToShippingEnd, Common.Now);
                        continue;
                    }

                    try
                    {
                        List<PrintBatch> printBatches = new List<PrintBatch>();

                        PrintBatch printBatch = null;

                        var docTypes = validMailPieces.Select(row => row.DocumentOrPackageType.Trim()).Distinct();
                        foreach (var documentOrPackageType in docTypes)
                        {
                            DocType docType = docTypeSvc.GetDocTypesByDocTypeName(mbi.Client.Id, documentOrPackageType);

                            //Commented because the same thing we are validating when generating document

                            //if (docType == null)
                            //{
                            //    commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.SubmittedToShippingEnd, Common.Now);
                            //    //TODO: Ask Ravi Sir
                            //    continue;
                            //}

                            printBatch = new PrintBatch();
                            printBatch.AppCode = mbi.Client.AppCode;
                            printBatch.UniqueId = Guid.NewGuid();
                            printBatch.MailBatchId = mbi.MailBatch.Id;
                            printBatch.DocTypeId = docType.Id;

                            var filteredMailPieces = validMailPieces.Where(c => c.DocumentOrPackageType == documentOrPackageType).ToList();

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

                        if (printBatches.Count == 0)
                        {
                            log.Error("No batches are available for submit to PCI.");

                            commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.SubmittedToShippingEnd, Common.Now);
                            continue;
                        }

                        foreach (var printBatchData in printBatches)
                        {
                            if (printBatchData.MailPieces.Count == 0)
                            {
                                log.Error("{0} : Print batch is not containing any mail pieces.".Fill(printBatchData.UniqueId));
                                commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.SubmittedToShippingEnd, Common.Now);
                                continue;
                            }

                            printBatchData.Id = mailBatchSvc.InsertPrintBatchDetails(printBatchData);

                            string pnsRootFolder = Config.PnS_SFT_RootDir;
                            string batchName = "{0}_{1}".Fill(mbi.MailBatch.BatchName, printBatchData.DocTypeId);
                            string batchXMLSubmitPath = Path.Combine(pnsRootFolder, mbi.Client.AppCode, batchName + ".xml");
                            string batchGOSubmitPath = Path.Combine(pnsRootFolder, mbi.Client.AppCode, batchName + ".go");
                            string batchSubmitPath = Path.Combine(pnsRootFolder, mbi.Client.AppCode, batchName);

                            try
                            {
                                //TODO: Add Validation For Directory Full Access Read/Write/Delete
                                batchSubmitPath.CleanDirectory();
                            }
                            catch (Exception ex)
                            {
                                commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.SubmittedToShippingEnd, Common.Now);
                                log.Error("Error occured while creating directory :" + ex);
                            }

                            try
                            {
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
                                commonSvc.UpdatePrintBatchStatus(printBatchData.Id, AM.Entities.BatchStatus.InProgress.ToString());
                                commonSvc.UpdatePrintBatchPipeLineStatus(printBatchData.Id, BatchPipelineStatus.SubmittedToShipping, Common.Now);
                            }
                            catch (Exception ex)
                            {
                                commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.SubmittedToShippingEnd, Common.Now);
                                log.Error("Error occured while generaing PnSRequest :" + ex);
                            }
                        }
                        commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.SubmittedToShipping, Common.Now);
                        commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.SubmittedToShippingEnd, Common.Now);
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                        commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.SubmittedToShippingEnd, Common.Now);
                    }
                }
            }
        }

        internal static void GetSubmittedPnSResponses()
        {
            using (IMailBatchService mailBatchSvc = new MailBatchService())
            using (IClientService clientSvc = new ClientService())
            using (ICommonService commonSvc = new CommonService())
            using (IMailPieceService mailPieceSvc = new MailPieceService())
            using (IDocTypeService docTypeSvc = new DocTypeService())
            using (IFedExService fedExSvc = new FedExService())
            {
                log.Debug("Get submitted job started.");
                var submittedPrintBatches = mailBatchSvc.GetSubmittedPrintBatchDetails();

                if (submittedPrintBatches == null || submittedPrintBatches.Count == 0)
                {
                    log.Debug("Response download Job queue is empty.");
                    return;
                }

                SFTUtil sftSite = new SFTUtil(SFT_ProtocolType.unc, Config.PnS_SFT_HostURL, Config.PnS_SFT_UserName, Config.PnS_SFT_Password, 300000);

                sftSite.Connect();

                CheckAndDownloadResponseFiles(sftSite, submittedPrintBatches);

                sftSite.Disconnect();
            }
        }

        private static void CheckAndDownloadResponseFiles(SFTUtil sftSite, List<SubmittedBatch> submittedPrintBatches)
        {
            using (IMailBatchService mailBatchSvc = new MailBatchService())
            using (IClientService clientSvc = new ClientService())
            using (ICommonService commonSvc = new CommonService())
            using (IMailPieceService mailPieceSvc = new MailPieceService())
            using (IDocTypeService docTypeSvc = new DocTypeService())
            using (IFedExService fedExSvc = new FedExService())
            {
                List<string> responseFiles;
                var clientAppCodes = submittedPrintBatches.Select(client => client.AppCode).Distinct();
                foreach (var appCode in clientAppCodes)
                {
                    Client client = clientSvc.GetClientByAppCode(appCode);

                    responseFiles = sftSite.ListDirectory(Path.Combine(Config.PnS_SFT_RootDir, client.AppCode));

                    var responseTriggerFiles = responseFiles
                    .Where(file => Path.GetExtension(file).ToLower() == ".rpt")
                    .Select(file => new KeyValuePair<string, string>(Path.GetFileNameWithoutExtension(file), file))
                    .ToList();

                    foreach (var triggerFile in responseTriggerFiles)
                    {
                        string pnsResponseFilePath = Path.Combine(Config.PnS_SFT_RootDir, client.AppCode, triggerFile.Key + "-res.xml");
                        string pnsResponseTriggerFilePath = Path.Combine(Config.PnS_SFT_RootDir, client.AppCode, triggerFile.Value);
                        if (sftSite.IsFileAvailable(pnsResponseFilePath))
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
                    #region Download Response Files

                    foreach (var downloadableBatch in downloadableFiles)
                    {
                        string responseTriggerFilePath = downloadableBatch.ResponseTriggerFilePath;
                        string responseFileName = Path.GetFileName(downloadableBatch.ResponseFilePath);
                        string responseFilePath = downloadableBatch.ResponseFilePath;

                        if (sftSite.IsFileAvailable(responseFilePath))
                        {
                            string amRootFilePath = Path.Combine(Config.AuthoMailRootFolder, responseFileName);

                            DateTime mrdfDownloadStartTime = Common.Now;
                            DateTime responseReceivedOn = sftSite.LastModifiedTimeOfFile(responseFilePath);

                            sftSite.DownloadFile(responseFilePath, amRootFilePath, true, false);
                            sftSite.DeleteFile(responseTriggerFilePath);

                            if (File.Exists(amRootFilePath))
                            {
                                var batchData = mailBatchSvc.GetMailBatchDetailsByBatchId(downloadableBatch.MailBatchId);
                                var mbi = new MailBatchInfo(batchData.MailBatch, client, Config.AuthoMailRootFolder);

                                string amPnSResponsePath = Path.Combine(mbi.PnSResponsePath, responseFileName);

                                File.Move(amRootFilePath, amPnSResponsePath);

                                commonSvc.UpdatePrintBatchPipeLineStatus(downloadableBatch.PrintBatchId, BatchPipelineStatus.ShippingResponseReceived, responseReceivedOn);
                            }
                        }
                        else
                        {
                            log.Error("Trigger file is available but xml file is not available to download For PrintBatch-Id :{0}".Fill(downloadableBatch.PrintBatchId));
                        }
                    }
                    #endregion
                }
            }
        }

        internal static void ProcessPnSResponses()
        {
            using (IMailBatchService mailBatchSvc = new MailBatchService())
            using (IClientService clientSvc = new ClientService())
            using (ICommonService commonSvc = new CommonService())
            using (IMailPieceService mailPieceSvc = new MailPieceService())
            using (IDocTypeService docTypeSvc = new DocTypeService())
            using (IFedExService fedExSvc = new FedExService())
            {
                var responseReceivedPrintBatches = mailBatchSvc.GetResponseReceivedPrintBatchDetails();

                if (responseReceivedPrintBatches == null || responseReceivedPrintBatches.Count == 0)
                {
                    log.Debug("Response process job queue is empty.");
                    return;
                }

                foreach (var printBatch in responseReceivedPrintBatches)
                {
                    Client client = clientSvc.GetClientByAppCode(printBatch.AppCode);
                    BatchData batchData = mailBatchSvc.GetMailBatchDetailsByBatchId(printBatch.MailBatchId);
                    MailBatchInfo mbi = new MailBatchInfo(batchData.MailBatch, client, Config.AuthoMailRootFolder);

                    var printbatchResponseFilePath = Path.Combine(mbi.PnSResponsePath, printBatch.SubmittedBatchName + "-res.xml");
                    if (!File.Exists(printbatchResponseFilePath))
                    {
                        log.Error("Response xml file is not exist for PrintBatch ID : {0}".Fill(printBatch.PrintBatchId));
                        continue;
                    }

                    try
                    {
                        var pnsResponse = XmlUtil.Deserialize<PnSResponse>(File.ReadAllText(printbatchResponseFilePath));
                        if (pnsResponse != null)
                        {
                            commonSvc.UpdatePrintBatchStatus(printBatch.PrintBatchId, pnsResponse.BatchStatus.ToString(), pnsResponse.StatusDescription);

                            if (pnsResponse.MailPackageReport != null && pnsResponse.MailPackageReport.Count > 0)
                            {
                                foreach (var mailPackage in pnsResponse.MailPackageReport)
                                {
                                    batchData.MailPieces.ForEach(row =>
                                    {
                                        if (row.UniqueId == mailPackage.UniqueId)
                                        {
                                            if (mailPackage.MailPieceStatus == MailPieceStatus.Error)
                                            {
                                                row.ErrorCode = Convert.ToString((ErrorTypes)mailPackage.ErrId);
                                                row.ErrorDesc = mailPackage.ErrDesc;
                                            }
                                            else if (mailPackage.MailPieceStatus == MailPieceStatus.PulledBack && row.IsPullBackRequested == true)
                                            {
                                                row.IsPulledBack = true;
                                                row.PulledBackOn = mailPackage.PulledBackOn;
                                            }
                                            else if (mailPackage.MailPieceStatus == MailPieceStatus.Shipped)
                                            {
                                                row.PrintedOn = mailPackage.PrintedOn;
                                                row.ShippedOn = mailPackage.ShippedOn;
                                                row.ShippingCost = mailPackage.PostageCost;
                                                row.TotalSheets = mailPackage.TotalSheets;
                                                row.Weight = mailPackage.WeightMeasured;
                                            }
                                            row.ShippingStatus = mailPackage.MailPieceStatus.ToString();

                                            mailPieceSvc.UpdateBatchMailPieceDetails(row);
                                        }
                                    });
                                }

                                var remainingData = batchData.MailPieces.Where(data => data.ShippingStatus == ShippingStatus.InProgress.ToString());
                                if (remainingData.Count() == 0)
                                {
                                    var shippedCount = batchData.MailPieces.Where(data => data.ShippingStatus == ShippingStatus.Shipped.ToString());
                                    if (shippedCount.Count() == 0)
                                    {
                                        commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, AM.Entities.BatchStatus.Failed.ToString());
                                    }
                                    else if (shippedCount.Count() < batchData.MailPieces.Count)
                                    {
                                        commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, AM.Entities.BatchStatus.ShippedPartial.ToString());
                                    }
                                    else
                                    {
                                        commonSvc.UpdateBatchStatus(mbi.MailBatch.Id, AM.Entities.BatchStatus.ShippedAll.ToString());
                                    }

                                    commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ShippingResponseReceived, Common.Now);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //TODO: Ask To Ravi Sir
                        log.Error(ex);
                    }
                }
            }
        }

        internal static void ReportPreppedProcess()
        {
            using (IMailBatchService mailBatchSvc = new MailBatchService())
            using (IClientService clientSvc = new ClientService())
            using (ICommonService commonSvc = new CommonService())
            using (IMailPieceService mailPieceSvc = new MailPieceService())
            using (IDocTypeService docTypeSvc = new DocTypeService())
            using (IFedExService fedExSvc = new FedExService())
            {
                List<MailBatchDTO> resReceivedBatches = mailBatchSvc.GetBatchesByPipelineStatus(BatchPipelineStatus.ShippingResponseReceived);
                if (resReceivedBatches == null)
                {
                    log.Debug("Report prepped job queue is empty.");
                    return;
                }

                foreach (var batch in resReceivedBatches)
                {
                    Client client = clientSvc.GetClientById(batch.ClientId);
                    SFTChannel sftChannelDetails = clientSvc.GetSFTChannelDetailsBySFTChannelId(batch.SFTChannelId);
                    MailBatchInfo mbi = new MailBatchInfo(batch, client, Config.AuthoMailRootFolder);
                    var data = mailBatchSvc.GetMailBatchDetailsByBatchId(mbi.MailBatch.Id);
                    if (data != null)
                    {
                        try
                        {
                            StringBuilder deliveryReport = USAADailyDetailOperationReport(data, sftChannelDetails.Title, client.TimeZone);

                            if (deliveryReport != null)
                            {
                                string deliveryReportFileName = "{0}_{1}.csv".Fill("DR", data.MailBatch.BatchName);
                                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(deliveryReport.ToString())))
                                {
                                    using (FileStream file = new FileStream(Path.Combine(mbi.AMResponsePath, deliveryReportFileName), FileMode.Create, FileAccess.Write))
                                    {
                                        ms.WriteTo(file);
                                    }
                                }
                            }

                            StringBuilder errorReport = USAAErrorReport(data, sftChannelDetails.Title, client.TimeZone);

                            if (errorReport != null)
                            {
                                string errorReportFileName = "{0}_{1}.csv".Fill("ER", data.MailBatch.BatchName);
                                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(errorReport.ToString())))
                                {
                                    using (FileStream file = new FileStream(Path.Combine(mbi.AMResponsePath, errorReportFileName), FileMode.Create, FileAccess.Write))
                                    {
                                        ms.WriteTo(file);
                                    }
                                }
                            }

                            commonSvc.UpdateBatchPipeLineStatus(mbi.MailBatch.Id, BatchPipelineStatus.ReportPrepped, Common.Now);
                        }
                        catch (Exception ex)
                        {
                            log.Error("Error occured while generating reports : {0}".Fill(ex.ToString()));
                        }
                    }
                }
            }
        }
        internal static StringBuilder USAAErrorReport(BatchData data, string sftChannelName, string timeZone)
        {
            StringBuilder errorReport = new StringBuilder("BatchID,ID,Channel,Package Name,Loan Number,File Name,Date Received,First Name,Last Name,Recipient Street Address,Recipient City,Recipient State,Recipient Zip,Error Code,Error Text,Shipping Method,Customer Identifier,RequestID");

            errorReport.AppendLine();

            var erroredData = data.MailPieces.Where(l => l.ErrorCode != OutComeCodes.No_Error).ToList();

            if (erroredData == null || erroredData.Count == 0)
            {
                return null;
            }

            // All the loans where outcome code is anything except no_error are processed invalid loans.
            foreach (MailPieceDTO mailPiece in erroredData)
            {
                errorReport.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17}",
                        data.MailBatch.BatchName == null ? string.Empty : data.MailBatch.BatchName,
                        mailPiece.UniqueId,
                        sftChannelName,
                        mailPiece.DocumentOrPackageType == null ? string.Empty : mailPiece.DocumentOrPackageType,
                        mailPiece.BusinessFriendlyIdValue == null ? string.Empty : mailPiece.BusinessFriendlyIdValue,
                        mailPiece.FileName == null ? string.Empty : mailPiece.FileName,
                        data.MailBatch.ReceivedOn == null ? string.Empty : data.MailBatch.ReceivedOn.Value.UtcToClientTimeZone(timeZone).ToString(),
                        mailPiece.RcptFirstName == null ? string.Empty : mailPiece.RcptFirstName,
                        mailPiece.RcptLastName == null ? string.Empty : mailPiece.RcptLastName,
                        mailPiece.RcptAddressStreet == null ? string.Empty : mailPiece.RcptAddressStreet,
                        mailPiece.RcptAddressCity == null ? string.Empty : mailPiece.RcptAddressCity,
                        mailPiece.RcptAddressState == null ? string.Empty : mailPiece.RcptAddressState,
                        mailPiece.RcptAddressZIP == null ? string.Empty : mailPiece.RcptAddressZIP,
                        mailPiece.ErrorCode,
                        mailPiece.ErrorDesc == null ? string.Empty : mailPiece.ErrorDesc.Replace(',', ' ').Replace("\r\n", " "),
                        data.MailPieceCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "ShippingMethod").FirstOrDefault() == null ? string.Empty : data.MailPieceCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "ShippingMethod").FirstOrDefault().Value,
                        data.MailPieceCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "CustomerIdentifier").FirstOrDefault() == null ? string.Empty : data.MailPieceCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "CustomerIdentifier").FirstOrDefault().Value,
                        data.MailPieceCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "RequestId").FirstOrDefault() == null ? string.Empty : data.MailPieceCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "RequestId").FirstOrDefault().Value));
            }

            return errorReport;
        }

        internal static StringBuilder USAADailyDetailOperationReport(BatchData data, string sftChannelName, string timeZone)
        {
            StringBuilder deliveryReport = new StringBuilder("BatchID,ID,Channel,Package Name,Loan Number,File Name,Date Received,Date Printed,Date Mailed,Client Code,Tracking Number,Shipping Method,Customer Identifier,First Name,Last Name,Recipient Street Address,Recipient City,Recipient State,Recipient Zip,RequestID,Print Location,Pages Shipped");

            deliveryReport.AppendLine();

            var shippedData = data.MailPieces.Where(l => l.ShippingStatus == ShippingStatus.Shipped.ToString()).ToList();

            if (shippedData == null || shippedData.Count == 0)
            {
                return null;
            }

            // All the loans where outcome code is anything except no_error are processed invalid loans.
            foreach (MailPieceDTO mailPiece in shippedData)
            {
                deliveryReport.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21}",
                        data.MailBatch.BatchName == null ? string.Empty : data.MailBatch.BatchName,
                        mailPiece.UniqueId,
                        sftChannelName,
                        mailPiece.DocumentOrPackageType == null ? string.Empty : mailPiece.DocumentOrPackageType,
                        mailPiece.BusinessFriendlyIdValue == null ? string.Empty : mailPiece.BusinessFriendlyIdValue,
                        mailPiece.FileName == null ? string.Empty : mailPiece.FileName,
                        data.MailBatch.ReceivedOn == null ? string.Empty : data.MailBatch.ReceivedOn.Value.UtcToClientTimeZone(timeZone).ToString(),
                        mailPiece.PrintedOn == null ? string.Empty : mailPiece.PrintedOn.Value.UtcToClientTimeZone(timeZone).ToString(),
                        mailPiece.ShippedOn == null ? string.Empty : mailPiece.ShippedOn.Value.UtcToClientTimeZone(timeZone).ToString(),
                        data.MailPieceCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "ShippingCredCode").FirstOrDefault() == null ? string.Empty : data.MailPieceCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "ShippingCredCode").FirstOrDefault().Value,
                        mailPiece.TrackingNo == null ? string.Empty : mailPiece.TrackingNo,
                        data.MailPieceCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "ShippingMethod").FirstOrDefault() == null ? string.Empty : data.MailPieceCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "ShippingMethod").FirstOrDefault().Value,
                        data.MailPieceCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "CustomerIdentifier").FirstOrDefault() == null ? string.Empty : data.MailPieceCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "CustomerIdentifier").FirstOrDefault().Value,
                        mailPiece.RcptFirstName == null ? string.Empty : mailPiece.RcptFirstName,
                        mailPiece.RcptLastName == null ? string.Empty : mailPiece.RcptLastName,
                        mailPiece.RcptAddressStreet == null ? string.Empty : mailPiece.RcptAddressStreet,
                        mailPiece.RcptAddressCity == null ? string.Empty : mailPiece.RcptAddressCity,
                        mailPiece.RcptAddressState == null ? string.Empty : mailPiece.RcptAddressState,
                        mailPiece.RcptAddressZIP == null ? string.Empty : mailPiece.RcptAddressZIP,
                        data.MailPieceCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "RequestId").FirstOrDefault() == null ? string.Empty : data.MailPieceCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "RequestId").FirstOrDefault().Value,
                        data.MailBatch.PrintLocation,
                        mailPiece.TotalSheets));
            }

            return deliveryReport;
        }
    }
}
