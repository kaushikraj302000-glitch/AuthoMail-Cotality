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
        private static IBatchService mailBatchSvc = new BatchService();
        private static IClientService clientSvc = new ClientService();
        private static ICommonService commonSvc = new CommonService();
        private static IMailPieceService mailPieceSvc = new MailPieceService();
        private static IDocTypeService docTypeSvc = new DocTypeService();

        private static Random random = new Random();
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
                MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, null, Common.AuthoMailRootFolder);

                var batchData = mailBatchSvc.GetMailBatchDetailsByBatchId(mbi.MailBatch.Id);

                List<MailPieceDTO> validMailPieces = batchData.MailPieces.Where(c => c.ErrorCode == OutComeCodes.No_Error).ToList();
                commonSvc.AMLogBatchTimeLine(mbi.MailBatch.Id, AMLogBatchTimeLine.PreppedStart, Common.Now);

                foreach (MailPieceDTO mailPiece in validMailPieces)
                {
                    try
                    {
                        DocType docType = docTypeSvc.GetDocTypesByDocTypeName(mbi.Client.Id, "", mailPiece.DocumentOrPackageType.Trim());

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

                        #region Prepare dictionary of placeholders/tags to fill the PDF template.

                        Dictionary<string, string> mailPieceData = new Dictionary<string, string>();
                        mailPieceData.Add("FromAddress", mailPiece.RetRcptAddressStreet + "\n" + mailPiece.RetRcptAddressAptOrSuiteOrOther + "\n" + mailPiece.RetRcptAddressCity + ", " + mailPiece.RetRcptAddressState + ", " + mailPiece.RetRcptAddressZIP);

                        mailPieceData.Add("ToAddress", mailPiece.RcptFirstName + " " + mailPiece.RcptLastName + "\n" + (string.IsNullOrEmpty(mailPiece.RcptAddressAptOrSuiteOrOther) ? mailPiece.RcptAddressStreet : string.Format("{0}\n{1}", mailPiece.RcptAddressStreet, mailPiece.RcptFirstName)) + "\n" + mailPiece.RcptAddressCity + ", " + mailPiece.RcptAddressState + ", " + mailPiece.RcptAddressZIP);

                        mailPieceData.Add("MailPieceId", mailPiece.Id.ToString());
                        mailPieceData.Add("Date", DateTime.UtcNow.UtcToEst().ToString("MMMM dd, yyyy"));
                        mailPieceData.Add("MailPieceZip", mailPiece.RcptAddressZIP);
                        mailPieceData.Add("LoanNbr", mailPiece.BusinessFriendlyIdValue);

                        #endregion

                        if (docType.ShippingCarrier == Carrier.USPS.ToString())
                        {
                            DocTypeCoverLetter docTypeLetter = docTypeSvc.GetDocumentCoverLetterTemplates(docType.Id).SingleOrDefault();

                            if (docTypeLetter == null)
                            {
                                mailPiece.ErrorCode = OutComeCodes.Unknown_Error;
                                mailPiece.ErrorDesc = "Cover letter is not exist for DocumentType : {0}".Fill(mailPiece.DocumentOrPackageType);
                                mailPiece.ShippingStatus = ShippingStatus.Error.ToString();
                                mailPieceSvc.UpdateBatchMailPieceDetails(mailPiece);
                                continue;
                            }

                            string coverLetterTemplatePath = Path.Combine(Common.CoverTemplateFolder, docTypeLetter.CoverLetterTemplatePath);

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

        internal static void SubmitBatchesDocTypeWiseToPCI()
        {
            List<MailBatchDTO> mailBatches = mailBatchSvc.GetBatchesByStatus(BatchPipelineStatus.Prepped);

            if (mailBatches == null || mailBatches.Count == 0)
            {
                log.Debug("Submit batch Job queue is empty.");
                return;
            }

            foreach (MailBatchDTO mailBatch in mailBatches)
            {
                Client client = clientSvc.GetClientDetails(mailBatch.ClientId).SingleOrDefault(c => c.IsPnSActive == true);
                if (client == null)
                {
                    log.Error("Client of Batch : {0}, is not enabled for PnS service".Fill(mailBatch.BatchName));
                    continue;
                }

                MailBatchInfo mbi = new MailBatchInfo(mailBatch, client, null, Common.AuthoMailRootFolder);

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
                        DocType docType = docTypeSvc.GetDocTypesByDocTypeName(mbi.Client.Id, "", documentOrPackageType);

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

                        string pnsRootFolder = Common.PnS_SFT_RootDir;
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

        internal static void GetSubmittedPnSResponses()
        {
            var submittedPrintBatches = mailBatchSvc.GetSubmittedPrintBatchDetails();

            if (submittedPrintBatches == null || submittedPrintBatches.Count == 0)
            {
                log.Debug("Response download Job queue is empty.");
                return;
            }

            SFTUtil sftSite = new SFTUtil(SFT_ProtocolType.unc, Common.PnS_SFT_HostURL, Common.PnS_SFT_UserName, Common.PnS_SFT_Password, 300000);

            sftSite.Connect();

            CheckAndDownloadResponseFiles(sftSite, submittedPrintBatches);

            sftSite.Disconnect();
        }

        private static void CheckAndDownloadResponseFiles(SFTUtil sftSite, List<SubmittedBatch> submittedPrintBatches)
        {
            List<string> responseFiles;
            var clientAppCodes = submittedPrintBatches.Select(client => client.AppCode).Distinct();
            foreach (var appCode in clientAppCodes)
            {
                Client client = clientSvc.GetClientDetails(null, appCode).Single();

                responseFiles = sftSite.ListDirectory(Path.Combine(Common.PnS_SFT_RootDir, client.AppCode));

                var responseTriggerFiles = responseFiles
                .Where(file => Path.GetExtension(file).ToLower() == ".rpt")
                .Select(file => new KeyValuePair<string, string>(Path.GetFileNameWithoutExtension(file), file))
                .ToList();

                foreach (var triggerFile in responseTriggerFiles)
                {
                    string pnsResponseFilePath = Path.Combine(Common.PnS_SFT_RootDir, client.AppCode, triggerFile.Key + "-res.xml");
                    string pnsResponseTriggerFilePath = Path.Combine(Common.PnS_SFT_RootDir, client.AppCode, triggerFile.Value);
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
                        string amRootFilePath = Path.Combine(Common.AuthoMailRootFolder, responseFileName);

                        DateTime mrdfDownloadStartTime = Common.Now;
                        DateTime responseReceivedOn = sftSite.LastModifiedTimeOfFile(responseFilePath);

                        sftSite.DownloadFile(responseFilePath, amRootFilePath, true, false);
                        sftSite.DeleteFile(responseTriggerFilePath);

                        if (File.Exists(amRootFilePath))
                        {
                            var batchData = mailBatchSvc.GetMailBatchDetailsByBatchId(downloadableBatch.MailBatchId);
                            var mbi = new MailBatchInfo(batchData.MailBatch, client, null, Common.AuthoMailRootFolder);

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

        internal static void ProcessPnSResponses()
        {
            var responseReceivedPrintBatches = mailBatchSvc.GetResponseReceivedPrintBatchDetails();

            if (responseReceivedPrintBatches == null || responseReceivedPrintBatches.Count == 0)
            {
                log.Debug("Response process job queue is empty.");
                return;
            }

            foreach (var printBatch in responseReceivedPrintBatches)
            {
                Client client = clientSvc.GetClientDetails(null, printBatch.AppCode).Single();
                BatchData batchData = mailBatchSvc.GetMailBatchDetailsByBatchId(printBatch.MailBatchId);
                MailBatchInfo mbi = new MailBatchInfo(batchData.MailBatch, client, null, Common.AuthoMailRootFolder);

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
                catch (Exception)
                {
                    //TODO: Ask To Ravi Sir
                    throw;
                }
            }
        }
    }
}
