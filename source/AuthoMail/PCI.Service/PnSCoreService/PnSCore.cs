using iTextSharp.text;
using iTextSharp.text.exceptions;
using iTextSharp.text.pdf;
using log4net;
using Microsoft.Practices.EnterpriseLibrary.Data;
using PCI.Lib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace PCI.Service
{
    static class PnSCoreService
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PnSCoreService));
        private static Database db = DatabaseFactory.CreateDatabase();

        private static List<PCIField> BatchSummarySchema { get; set; }
        private static List<PCIField> MRDFRequestSchema { get; set; }
        private static List<PCIField> MRDFResponseSchema { get; set; }

        static PnSCoreService()
        {
            BatchSummarySchema = DAL.GetBatchSummarySchema();
            MRDFRequestSchema = DAL.GetMRDFRequestSchema();
            MRDFResponseSchema = DAL.GetMRDFResponseSchema();
        }

        #region PrepPCIJobs

        public static void PrepPCIJobs()
        {
            log.Debug("Started preparing PCI jobs");

            List<int> batchIds = DAL.GetNextBatchIdByJobName(JobTypes.PnSCoreService, OperationTypes.Prep);

            if (batchIds == null || batchIds.Count == 0)
            {
                log.Debug("Prepped job queue is empty.");
                return;
            }

            foreach (var batchId in batchIds)
            {
                try
                {
                    DAL.LogBatchTimeLine(batchId, LogBatchTimeLine.PCIBatchPrepStart, Common.GetCurrentTime());

                    BatchSummary batchSummary = DAL.GetBatchSummaryById(batchId);
                    Client clientDetail = DAL.GetClientAppDetailsById(batchSummary.ClientId);

                    var bi = new BatchInfo(batchSummary, clientDetail);

                    log.Debug("Prepped Job Started For Batch {0}".Fill(bi.BatchName));

                    // Clean up
                    Common.CleanDirectory(bi.PCIRequestPath);

                    PnSRequest pnsRequest = null;

                    try
                    {
                        pnsRequest = PnSRequest.LoadXml(File.ReadAllText(bi.RequestXmlPath));
                    }
                    catch (XmlException ex)
                    {
                        var msg = "The request file is not a valid XML.";
                        ReportInvalidBatch(bi, msg, ex);
                        continue;
                    }
                    catch (XmlSchemaValidationException ex)
                    {
                        var msg = "The request XML has some invalid data.";
                        ReportInvalidBatch(bi, msg, ex);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        var msg = "Ensure that the request XML complies with the request schema.";
                        ReportInvalidBatch(bi, msg, ex);
                        continue;
                    }

                    // Load PnSRequest into batch summary
                    batchSummary.JulianDate = DateTime.Today.Year * 1000 + DateTime.Today.DayOfYear;
                    batchSummary.DueDate = pnsRequest.DueDate;
                    batchSummary.OnValidationErrors = pnsRequest.OnValidationErrors.ToString();
                    batchSummary.SLADays = pnsRequest.SLADays;
                    batchSummary.BatchIdentifier = pnsRequest.BatchIdentifier;
                    batchSummary.PrintLocation = pnsRequest.PrintLocation.IsNullOrEmpty() ? Common.PrintLocationPCI : pnsRequest.PrintLocation;

                    var docTypes = DAL.GetDocTypeDetailsByAppId(clientDetail.Id);
                    if (docTypes == null)
                    {
                        var msg = "Client does not have allocated any document types.";
                        ReportInvalidBatch(bi, msg);
                        continue;
                    }
                    var validDocType = docTypes.Where(x => x.DocTypeId == pnsRequest.DocTypeId).FirstOrDefault();
                    if (validDocType == null)
                    {
                        var msg = "The specified document type id '{0}' is not valid.".Fill(pnsRequest.DocTypeId);
                        ReportInvalidBatch(bi, msg);
                        continue;
                    }
                    batchSummary.DocTypeId = pnsRequest.DocTypeId;

                    //Ensure that the DocTypeCode is valid
                    var printTrail = DAL.GetPrintTrailDetailByPrintTrailId(pnsRequest.PrintTrailId);
                    if (printTrail == null)
                    {
                        var msg = "Client does not have allocated print trail.";
                        ReportInvalidBatch(bi, msg);
                        continue;
                    }
                    batchSummary.PrintTrailId = pnsRequest.PrintTrailId;

                    // Update database
                    DAL.UpdatePnSRequestDataInBatchSummary(batchSummary);

                    // Ensure that the app code is valid
                    if (clientDetail.AppCode != pnsRequest.AppCode)
                    {
                        var msg = "The specified application code '{0}' is not valid.".Fill(pnsRequest.AppCode);
                        ReportInvalidBatch(bi, msg);
                        continue;
                    }

                    // Batch xml file name MUST match with batch name mentioned in the file name.
                    if (pnsRequest.BatchName != bi.BatchName)
                    {
                        var msg = "The XML file name '{0}' must match with batch name ('{1}') attribute of the XML.".Fill(bi.BatchName, pnsRequest.BatchName);
                        ReportInvalidBatch(bi, msg);
                        continue;
                    }

                    // Ensure that files in XML and in batch folder match

                    var validationResult = ValidateMailPieces(pnsRequest, bi);

                    if (validationResult.HasErrors)
                    {
                        ReportInvalidBatch(bi, validationResult.Errors);
                        continue;
                    }

                    //If any extra attachments needs to send to PCI then it will validate Attachements attribute of XML and copy to PCI Request folder 
                    if (pnsRequest.Attachments != null && pnsRequest.Attachments.Count > 0)
                    {
                        string jobName = bi.BatchSummary.JobName.Substring(bi.BatchSummary.JobName.IndexOf('-') + 1, bi.BatchSummary.JobName.Length - (bi.BatchSummary.JobName.IndexOf('-') + 1));
                        foreach (var attachment in pnsRequest.Attachments)
                        {
                            if (File.Exists(Path.Combine(bi.PnSRequestPath, jobName, attachment.Name)))
                            {
                                File.Copy(Path.Combine(bi.PnSRequestPath, jobName, attachment.Name), Path.Combine(bi.PCIRequestPath, attachment.Name));
                            }
                            else
                            {
                                var msg = "Mentioned attachment file '{0}' was not found.".Fill(attachment.Name);
                                ReportInvalidBatch(bi, msg);
                                continue;
                            }
                        }
                    }

                    ProcessPDFsForPCI(pnsRequest, bi, validDocType);

                    DAL.LogBatchTimeLine(batchId, LogBatchTimeLine.PCIBatchPrepEnd, Common.GetCurrentTime());
                }
                catch (Exception ex)
                {
                    var msg = "Unexpected error occured in PrepPCIJobs()";
                    log.Error(msg, ex);
                    MailUtility.SendUnexpectedNotificationEmail(msg + ex.Message);
                }
            }
            log.Debug("Completed preparing PCI jobs");
        }

        private static void ReportInvalidBatch(BatchInfo bi, string msg)
        {
            ReportInvalidBatch(bi, msg, null, null);
        }

        private static void ReportInvalidBatch(BatchInfo bi, string msg, Exception ex)
        {
            bi.BatchSummary.BatchIdentifier = Guid.NewGuid();
            ReportInvalidBatch(bi, msg, null, ex);
        }

        private static void ReportInvalidBatch(BatchInfo bi, string msg, List<MailPieceReport> mailPackageReport)
        {
            ReportInvalidBatch(bi, msg, mailPackageReport, null);
        }

        private static void ReportInvalidBatch(BatchInfo bi, string msg, List<MailPieceReport> mailPackageReport, Exception ex)
        {
            log.Error(msg, ex);

            MailUtility.SendUnexpectedNotificationEmail(msg);

            DAL.LogBatchTimeLine(bi.BatchSummary.PrintBatch, LogBatchTimeLine.PCIBatchPrepStart, Common.GetCurrentTime());

            DAL.LogBatchTimeLine(bi.BatchSummary.PrintBatch, LogBatchTimeLine.ReportPrepStart, Common.GetCurrentTime());

            bi.BatchSummary.ValidationStatus = bi.BatchSummary.Images > 0 ? BatchValidationStatus.PartiallyValid : BatchValidationStatus.Invalid;
            bi.BatchSummary.ValidationErrors = ex == null ? msg : msg + Environment.NewLine + Environment.NewLine + ex.ToString();

            var pciBatchNameFormat = BatchSummarySchema.Single(f => f.FieldName == "PrintBatch").DataFormat;

            PnSResponse pnsResponse = new PnSResponse()
            {
                AppCode = bi.Client.AppCode,
                BatchName = bi.BatchName,
                BatchStatus = BatchStatus.Failed,
                MailPackageReport = mailPackageReport,
                StatusDescription = bi.BatchSummary.ValidationErrors,
                PCIBatchName = pciBatchNameFormat.Fill(bi.BatchSummary.PrintBatch),
                ReceivedOn = bi.BatchSummary.ReceivedOn
            };

            // Clean up
            Common.CleanDirectory(bi.PnSResponsePath);

            File.WriteAllText(Path.Combine(bi.PnSResponsePath, "{0}-res.xml".Fill(bi.BatchName)), pnsResponse.ToXml());
            File.WriteAllText(Path.Combine(bi.PnSResponsePath, "{0}.err".Fill(bi.BatchName)), "");

            DAL.LogBatchTimeLine(bi.BatchSummary.PrintBatch, LogBatchTimeLine.ReportPrepStart, Common.GetCurrentTime());

            DAL.UpdatePnSRequestDataInBatchSummary(bi.BatchSummary);
            DAL.UpdateBatchStatus(bi.BatchSummary.PrintBatch, BatchStatus.Failed.ToString());
            DAL.UpdateBatchPipeLineStatus(bi.BatchSummary.PrintBatch, BatchPipelineStatus.ReportPrepped, Common.GetCurrentTime());
        }

        private static void ReportInvalidMailPieces(BatchInfo bi, string msg, List<MailPieceReport> mailPackageReport)
        {
            log.Error(msg);

            DAL.LogBatchTimeLine(bi.BatchSummary.PrintBatch, LogBatchTimeLine.IEReportPrepStart, Common.GetCurrentTime());

            bi.BatchSummary.ValidationStatus = BatchValidationStatus.PartiallyValid;
            bi.BatchSummary.ValidationErrors = msg;

            var pciBatchNameFormat = BatchSummarySchema.Single(f => f.FieldName == "PrintBatch").DataFormat;

            PnSResponse pnsResponse = new PnSResponse()
            {
                AppCode = bi.Client.AppCode,
                BatchName = bi.BatchName,
                BatchStatus = BatchStatus.InProgress,
                MailPackageReport = mailPackageReport,
                StatusDescription = bi.BatchSummary.ValidationErrors,
                PCIBatchName = pciBatchNameFormat.Fill(bi.BatchSummary.PrintBatch),
                ReceivedOn = bi.BatchSummary.ReceivedOn
            };

            var responseXml = pnsResponse.ToXml();

            //One Copy goes to PnSResponse folder and another copy will go to PCIRoot folder with *.err trigger file.
            File.WriteAllText(Path.Combine(bi.PnSResponsePath, "{0}-res.xml.immediate".Fill(bi.BatchName)), responseXml);
            File.WriteAllText(Path.Combine(Common.PCIRootFolder, "{0}-res.xml".Fill(bi.BatchName)), responseXml);
            File.WriteAllText(Path.Combine(Common.PCIRootFolder, "{0}.err.{1}".Fill(bi.BatchName, bi.Client.AppCode)), "");

            DAL.LogBatchTimeLine(bi.BatchSummary.PrintBatch, LogBatchTimeLine.IEReportPrepEnd, Common.GetCurrentTime());

            DAL.UpdatePnSRequestDataInBatchSummary(bi.BatchSummary);
        }

        private static ValidationResult ValidateMailPieces(PnSRequest pnsRequest, BatchInfo bi)
        {
            var validationResult = new ValidationResult { HasErrors = false, Errors = null };

            var errors = new StringBuilder();

            var pdfs = Directory.GetFiles(bi.PnSRequestPDFsPath, "*.pdf", SearchOption.TopDirectoryOnly).ToList();

            var xmlMinusActual = pnsRequest.MailPackageData
                .Select(mp => mp.PDFFileName)
                .Except(pdfs.Select(f => Path.GetFileName(f)))
                .ToList();

            if (xmlMinusActual.Count > 0)
            {
                validationResult.HasErrors = true;
                errors.AppendLine("The following files are missing in '{0}' folder".Fill(bi.BatchName));
                errors.AppendLine(String.Join(", ", xmlMinusActual));
            }

            var actualMinusXml = pdfs.Select(f => Path.GetFileName(f))
                .Except(pnsRequest.MailPackageData
                .Select(mp => mp.PDFFileName))
                .ToList();

            if (actualMinusXml.Count > 0)
            {
                validationResult.HasErrors = true;
                errors.AppendLine("The following files are not mentioned in the request XML {0} : ".Fill(bi.BatchName));
                errors.AppendLine(String.Join(", ", actualMinusXml));
            }

            validationResult.Errors = errors.ToString();

            return validationResult;
        }

        private static void ProcessPDFsForPCI(PnSRequest pnsRequest, BatchInfo bi, DocType docType)
        {
            log.Debug("Started merging PDFs");
            var mrdfReq = new List<MRDFRequest>();
            var pieceIdFormat = MRDFRequestSchema.Single(f => f.FieldName == "PieceId").DataFormat;
            var totalSheetFormat = MRDFRequestSchema.Single(f => f.FieldName == "TotalSheets").DataFormat;

            // Resize and Barcode all PDFs
            var pieceId = 1;
            foreach (var mailPiece in pnsRequest.MailPackageData)
            {
                var mrdf = new MRDFRequest
                {
                    JobId = bi.BatchSummary.PrintBatch,
                    PieceId = pieceId,
                    Date = DateTime.Today,
                    UniqueId = mailPiece.UniqueId,
                    PDFFileName = mailPiece.PDFFileName,
                    DivertVS3 = mailPiece.PerformQC ? 1 : 0,
                    Address1 = "Job: {0} Seq: {1}".Fill(bi.BatchSummary.PrintBatch, pieceIdFormat.Fill(pieceId)),
                    Feeder4_Sel = mailPiece.IsBRE ? 1 : 0
                };

                var rawPdfPath = Path.Combine(bi.PnSRequestPDFsPath, mailPiece.PDFFileName);
                var processedPdfPath = Path.Combine(bi.PCIRequestPath, mailPiece.PDFFileName);

                var pageCountBySize = ProcessPDFWithBarcodeOnRotatedAndOrientation(rawPdfPath, processedPdfPath, mrdf, mailPiece, docType);

                DAL.InsertPageSizeWisePageCounts(pageCountBySize, mrdf.UniqueId);

                // Sending total number of pages instead of total sheets in input MRDF
                mrdf.TotalSheets = mrdf.TotalPages;

                // Fix sequence in case of errors
                if (mrdf.HasError)
                {
                    mrdf.PieceId = -1;
                }
                else
                {
                    pieceId++;
                }

                mrdfReq.Add(mrdf);
            }

            #region Pullback logic
            var pullbackData = DAL.GetPullBackRequestDataByJobId(bi.BatchSummary.PrintBatch).AsEnumerable().ToList();

            if (pullbackData.Count > 0)
            {
                mrdfReq.ForEach(mrdf =>
                {
                    if (pullbackData.Select(data => data.Field<Guid>("MailPieceUniqueId")).Contains(mrdf.UniqueId))
                    {
                        mrdf.PieceId = -1;
                        mrdf.MailPieceStatus = MailPieceStatus.PulledBack.ToString();
                        mrdf.ErrId = -4;//Because we are not sending this mail piece to PCI. It will be directly pullbed back by AuthoMail, We can identify that It was pullbed back directly. that's we are updating as pulled back
                        mrdf.ErrDesc = "Mail piece was pulled back by client directly.";
                        mrdf.TotalSheets = 0;
                    }
                });

                //Because we have pull back requests so we need to reorder pieces index so we are starting with 1 again.          
                pieceId = 1;

                mrdfReq.ForEach(mrdf =>
                {
                    if (mrdf.PieceId != -1)
                    {
                        mrdf.PieceId = pieceId++;
                        mrdf.Address1 = "Job: {0} Seq: {1}".Fill(bi.BatchSummary.PrintBatch, pieceIdFormat.Fill(pieceId));
                    }
                });

                //Because we are pulling back records so we don't need to send pdf in the zip and thats why we are deleting from PCIRequest folder
                var pulledbackFiles = mrdfReq.Where(row => row.PieceId == -1).Select(c => c.PDFFileName);
                foreach (var fileName in pulledbackFiles)
                {
                    string filePath = Path.Combine(bi.PCIRequestPath, fileName);
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }
            }
            #endregion

            var maxPieceId = mrdfReq.Max(mrdf => mrdf.PieceId);

            mrdfReq.ForEach(mrdf =>
            {
                if (mrdf.PieceId == -1)
                {
                    mrdf.PieceId = ++maxPieceId;
                    mrdf.Address1 = "Job: {0} Seq: {1}".Fill(bi.BatchSummary.PrintBatch, pieceIdFormat.Fill(mrdf.PieceId));
                }
            });

            mrdfReq = mrdfReq.OrderBy(mrdf => mrdf.PieceId).ToList();

            var pdfsWithError = mrdfReq.Where(mrdf => mrdf.HasError).ToList();
            var pdfsToMerge = mrdfReq.Where(mrdf => !mrdf.HasError).ToList();

            var totalActualPages = pdfsToMerge.Sum(mrdf => mrdf.TotalPages);

            bi.BatchSummary.Images = totalActualPages;
            bi.BatchSummary.PrintFileName = Path.GetFileName(bi.BatchZipFilePath);

            if (pdfsToMerge.Count == 0)
            {
                bi.BatchSummary.ValidationStatus = BatchValidationStatus.Invalid;
            }
            else if (pdfsWithError.Count > 0)
            {
                bi.BatchSummary.ValidationStatus = pnsRequest.OnValidationErrors == OnValidationErrors.CancelRequest ? BatchValidationStatus.Invalid : BatchValidationStatus.PartiallyValid;
            }
            else
            {
                bi.BatchSummary.ValidationStatus = BatchValidationStatus.Valid;
            }

            var batchSummarySchema = BatchSummarySchema.Select(f => f.Clone()).ToList();

            DAL.UpdateBatchSummaryData(BatchSummarySchema, bi.BatchSummary);

            DAL.DeleteMRDFRequestByJobId(bi.BatchSummary.PrintBatch);

            mrdfReq.ForEach(mrdf =>
            {
                if (mrdf.MailPieceStatus != MailPieceStatus.Error.ToString() && mrdf.MailPieceStatus != MailPieceStatus.PulledBack.ToString())
                {
                    mrdf.MailPieceStatus = bi.BatchSummary.ValidationStatus == BatchValidationStatus.Invalid ? MailPieceStatus.Aborted.ToString() : MailPieceStatus.InProgress.ToString();
                }
            });

            var mrdfRequestSchema = MRDFRequestSchema.Select(f => f.Clone()).ToList();

            DAL.InsertMRDFRequestData(mrdfRequestSchema, mrdfReq);

            if (bi.BatchSummary.ValidationStatus == BatchValidationStatus.Invalid)
            {
                var mailPackageReport = mrdfReq.Select(mrdf => new MailPieceReport
                {
                    UniqueId = mrdf.UniqueId,
                    PDFFileName = mrdf.PDFFileName,
                    MailPieceStatus = mrdf.HasError ? mrdf.ErrId == -4 ? MailPieceStatus.PulledBack : MailPieceStatus.Error : MailPieceStatus.Aborted,
                    ErrId = mrdf.HasError ? mrdf.ErrId : default(int),
                    ErrDesc = mrdf.ErrDesc
                }).ToList();

                ReportInvalidBatch(bi, "Some PDFs could not be processed. Check detailed description.", mailPackageReport);
                return;
            }


            if (pdfsWithError.Count > 0)
            {
                var mailPackageReport = mrdfReq.Select(mrdf => new MailPieceReport
                {
                    UniqueId = mrdf.UniqueId,
                    PDFFileName = mrdf.PDFFileName,
                    MailPieceStatus = mrdf.HasError ? mrdf.ErrId == -4 ? MailPieceStatus.PulledBack : MailPieceStatus.Error : MailPieceStatus.InProgress,
                    ErrId = mrdf.HasError ? mrdf.ErrId : default(int),
                    ErrDesc = mrdf.ErrDesc
                }).ToList();

                ReportInvalidMailPieces(bi, "Some PDFs could not be processed. Check detailed description.", mailPackageReport);
            }

            DAL.UpdateBatchStatus(bi.BatchSummary.PrintBatch, BatchStatus.InProgress.ToString());
            DAL.UpdateBatchPipeLineStatus(bi.BatchSummary.PrintBatch, BatchPipelineStatus.Prepped, Common.GetCurrentTime());
            log.Debug("Completed preparing PCI jobs");
        }

        //private static decimal[] GetPageSizeForXnY(string firstPage, BarcodeLocation location)
        //{
        //    decimal[] pageSizeXY = { 0, 0 };
        //    if (firstPage == "LETTER" || firstPage == "LETTER-IP" || firstPage == "WC24#LTR-PERF")
        //    {
        //        if (location == BarcodeLocation.OnFrontSide)//For now we are considering Top-Right
        //        {
        //            pageSizeXY[0] = Math.Round(Convert.ToDecimal(612f - (BarcodeInfo.RightMargin + BarcodeInfo.TemplateSize)), 2);
        //            pageSizeXY[1] = Math.Round(Convert.ToDecimal(BarcodeInfo.TopMargin), 3);
        //        }
        //        else //For now we are considering Bottom-Left
        //        {
        //            pageSizeXY[0] = Math.Round(Convert.ToDecimal(BarcodeInfo.LeftMargin), 3);
        //            pageSizeXY[1] = Math.Round(Convert.ToDecimal(792f - (BarcodeInfo.BottomMargin + BarcodeInfo.TemplateSize)), 2);
        //        }
        //    }
        //    else if (firstPage == "LEGAL" || firstPage == "LEGAL-IP" || firstPage == "LEGAL-PERF-IP")
        //    {
        //        if (location == BarcodeLocation.OnFrontSide)//For now we are considering Top-Right
        //        {
        //            pageSizeXY[0] = Math.Round(Convert.ToDecimal(612f - (BarcodeInfo.RightMargin + BarcodeInfo.TemplateSize)), 2);
        //            pageSizeXY[1] = Math.Round(Convert.ToDecimal(BarcodeInfo.TopMargin), 3);
        //        }
        //        else //For now we are considering Bottom-Left
        //        {
        //            pageSizeXY[0] = Math.Round(Convert.ToDecimal(BarcodeInfo.LeftMargin), 3);
        //            pageSizeXY[1] = Math.Round(Convert.ToDecimal(1008f - (BarcodeInfo.BottomMargin + BarcodeInfo.TemplateSize)), 2);
        //        }
        //    }
        //    return pageSizeXY;
        //}

        public static iTextSharp.text.Image GenerateBarcode(string batchNumber, string pieceId, string curPagecount)
        {
            int size = BarcodeInfo.TemplateSize;
            BarcodeDatamatrix barcodeDatamatrix = new BarcodeDatamatrix();
            barcodeDatamatrix.Options = BarcodeDatamatrix.DM_ASCII;
            barcodeDatamatrix.Height = size;
            barcodeDatamatrix.Width = size;
            barcodeDatamatrix.Ws = BarcodeInfo.WhiteSpace;
            barcodeDatamatrix.Generate(batchNumber + pieceId + curPagecount);
            return barcodeDatamatrix.CreateImage();
        }

        //Get Page Size for PDF
        private static Rectangle GetPageSize(string pageSize)
        {
            Rectangle size = null;
            switch (pageSize)
            {
                case "GLT-20-DOP-17-5":
                    size = iTextSharp.text.PageSize.LEDGER;
                    break;
                case "LETTER-IP":
                    size = iTextSharp.text.PageSize.LETTER;
                    break;
                case "LEGAL-IP":
                    size = iTextSharp.text.PageSize.LEGAL;
                    break;
                case "WC24-LTR-PERF":
                    size = iTextSharp.text.PageSize.LETTER;
                    break;
                case "LEGAL-PERF-IP":
                    size = iTextSharp.text.PageSize.LEGAL;
                    break;
            }
            return size;
        }

        private static Dictionary<string, int> ProcessPDFWithBarcodeOnRotatedAndOrientation(string src, string dest, MRDFRequest mrdf, MailPieceData mailPiece, DocType docType)
        {
            try
            {
                Dictionary<string, int> pageCountBySize = new Dictionary<string, int>();
                List<int> checksPageNumbers = new List<int>();
                string inPDF = src;
                string outPDF = dest;

                #region Resize and Bookmarking
                ScaleEvent evn;
                if (docType.PrintSize == "MixMedia")
                {
                    evn = new ScaleEvent(1f, 1f);
                }
                else
                    evn = new ScaleEvent(new PdfRectangle(GetPageSize(docType.PrintSize)));

                using (PdfReader reader = new PdfReader(inPDF))
                {
                    int totalPages = reader.NumberOfPages;

                    #region Validation PDF Level
                    //Validation of total pages of client and downloaded PDF page counts
                    if (mailPiece.TotalPages > 0)
                    {
                        if (mailPiece.TotalPages != totalPages)
                        {
                            mrdf.ErrId = (int)ErrorTypes.PageCountMismatch;
                            throw new Exception("Client specified total page count is mismatched with downloaded file's page count.");
                        }
                    }

                    if (mailPiece.Bookmarks.Count > 0)
                    {
                        //Validation of Specified checks last page number of client and downloaded PDF total Page Counts
                        var checksBookMark = mailPiece.Bookmarks.Where(x => x.Name == "Check").SingleOrDefault();
                        if (checksBookMark != null)
                        {
                            checksPageNumbers = checksBookMark.Pages.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(Int32.Parse).ToList();
                            if (checksPageNumbers.Count > 0 && checksPageNumbers.Max() > totalPages && checksPageNumbers.Min() < 1)
                            {
                                mrdf.ErrId = (int)ErrorTypes.PageCountMismatch;
                                throw new Exception("Pages to be bookmarked in file '{0}' must fall within the range of 1 to {1}.".Fill(mailPiece.PDFFileName, totalPages));
                            }
                        }
                    }
                    #endregion

                    using (Document document = new Document())
                    using (PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(outPDF, FileMode.Create)))
                    {
                        mrdf.TotalPages = totalPages;
                        evn.SetPageDict(reader.GetPageN(1));
                        writer.PageEvent = evn;
                        document.Open();
                        iTextSharp.text.Image page;
                        for (int p = 1; (p <= totalPages); p++)
                        {
                            int rotation = reader.GetPageRotation(p);
                            float pageWidth = reader.GetPageSizeWithRotation(p).Width;
                            float pageHeight = reader.GetPageSizeWithRotation(p).Height;

                            page = iTextSharp.text.Image.GetInstance(writer.GetImportedPage(reader, p));

                            page.SetAbsolutePosition(0, 0);

                            if (rotation == 270 || rotation == 90)
                            {
                                page.ScalePercent(evn.HScale * 100, evn.WScale * 100);
                            }
                            else
                            {
                                page.ScalePercent(evn.WScale * 100, evn.HScale * 100);
                            }

                            document.Add(page);

                            #region Mix Media Bookmarking

                            string pageSize = GetPageSizeInString(page.ScaledWidth, page.ScaledHeight);
                            if (checksPageNumbers.Contains(p))
                            {
                                AddBookmark(writer, p, "Check");
                            }
                            else if (docType.PrintSize == "MixMedia")
                            {
                                AddBookmark(writer, p, pageSize);
                            }

                            //Pagewise Page Counts
                            if (pageCountBySize.Keys.Contains(pageSize))
                            {
                                pageCountBySize[pageSize] += 1;
                            }
                            else
                            {
                                pageCountBySize.Add(pageSize, 1);
                            }

                            #endregion

                            if ((p < totalPages))
                            {
                                evn.SetPageDict(reader.GetPageN((p + 1)));
                            }

                            #region Top Right Barcode
                            //iTextSharp.text.Image image = GenerateBarcode("{0:d10}".Fill(mrdf.JobId), "{0:d4}".Fill(mrdf.PieceId), "{0:d3}".Fill(p));
                            //if (rotation == 90)
                            //{
                            //    image.SetAbsolutePosition(BarcodeInfo.TopMargin.ToPoint() - BarcodeInfo.ScaledWhiteSpace, page.ScaledHeight - (BarcodeInfo.RightMargin + BarcodeInfo.ActualSizeInch + BarcodeInfo.ScaledWhiteSpace.ToInch()).ToPoint());
                            //}
                            //else if (rotation == 180)
                            //{
                            //    image.SetAbsolutePosition(BarcodeInfo.RightMargin.ToPoint() - BarcodeInfo.ScaledWhiteSpace, BarcodeInfo.TopMargin.ToPoint() - BarcodeInfo.ScaledWhiteSpace);
                            //}
                            //else if (rotation == 270)
                            //{
                            //    image.SetAbsolutePosition(page.ScaledWidth - (BarcodeInfo.TopMargin + BarcodeInfo.ActualSizeInch + BarcodeInfo.ScaledWhiteSpace.ToInch()).ToPoint(), BarcodeInfo.RightMargin.ToPoint() - BarcodeInfo.ScaledWhiteSpace);
                            //}
                            //else
                            //{
                            //    image.SetAbsolutePosition(page.ScaledWidth - (BarcodeInfo.RightMargin + BarcodeInfo.ActualSizeInch + BarcodeInfo.ScaledWhiteSpace.ToInch()).ToPoint(), page.ScaledHeight - (BarcodeInfo.TopMargin + BarcodeInfo.ActualSizeInch + BarcodeInfo.ScaledWhiteSpace.ToInch()).ToPoint());
                            //}

                            //image.ScalePercent(BarcodeInfo.ScalePercent);

                            //writer.DirectContent.AddImage(image);
                            #endregion

                            #region Bottom Left Barcode
                            //iTextSharp.text.Image image2 = GenerateBarcode("{0:d10}".Fill(mrdf.JobId), "{0:d4}".Fill(mrdf.PieceId), "{0:d3}".Fill(p));
                            //if (rotation == 90)
                            //{
                            //    image2.SetAbsolutePosition(page.ScaledWidth - (BarcodeInfo.BottomMargin + BarcodeInfo.ActualSizeInch + BarcodeInfo.ScaledWhiteSpace.ToInch()).ToPoint(), (BarcodeInfo.LeftMargin.ToPoint() - BarcodeInfo.ScaledWhiteSpace));
                            //}
                            //else if (rotation == 180)
                            //{
                            //    image2.SetAbsolutePosition(page.ScaledWidth - (BarcodeInfo.LeftMargin + BarcodeInfo.ActualSizeInch + BarcodeInfo.ScaledWhiteSpace.ToInch()).ToPoint(), page.ScaledHeight - (BarcodeInfo.BottomMargin + BarcodeInfo.ActualSizeInch + BarcodeInfo.ScaledWhiteSpace.ToInch()).ToPoint());
                            //}
                            //else if (rotation == 270)
                            //{
                            //    image2.SetAbsolutePosition(BarcodeInfo.BottomMargin.ToPoint() - BarcodeInfo.ScaledWhiteSpace, page.ScaledHeight - (BarcodeInfo.LeftMargin + BarcodeInfo.ActualSizeInch + BarcodeInfo.ScaledWhiteSpace.ToInch()).ToPoint());
                            //}
                            //else
                            //{
                            //    image2.SetAbsolutePosition(BarcodeInfo.LeftMargin.ToPoint() - BarcodeInfo.ScaledWhiteSpace, BarcodeInfo.BottomMargin.ToPoint() - BarcodeInfo.ScaledWhiteSpace);
                            //}

                            //image2.ScalePercent(BarcodeInfo.ScalePercent);
                            //writer.DirectContent.AddImage(image2);
                            #endregion

                            document.NewPage();
                        }
                        document.Close();
                    }
                }
                #endregion

                return pageCountBySize;
            }
            catch (Exception ex)
            {
                var errorMsg = "Error occured while processing PDF file for Batch-Id {0} and File Name - {1} with Error {2}".Fill(mrdf.JobId, mrdf.PDFFileName, ex);
                log.Error(errorMsg);

                MailUtility.SendUnexpectedNotificationEmail(errorMsg);

                mrdf.ErrId = mrdf.ErrId == 0 ? (int)ErrorTypes.CannotResize : mrdf.ErrId;
                mrdf.MailPieceStatus = MailPieceStatus.Error.ToString();
                mrdf.ErrDesc = ex.ToString();
                // clean up
                if (File.Exists(dest)) File.Delete(dest);
            }
            return null;
        }

        private static void AddBookmark(PdfWriter writer, int pageNumber, string bookmarkName)
        {
            PdfOutline bookmark = new PdfOutline(writer.DirectContent.RootOutline, PdfAction.GotoLocalPage(pageNumber, new PdfDestination(pageNumber), writer), bookmarkName);
        }

        private static string GetPageSizeInString(float width, float height)
        {
            if ((width == 612f && height == 792f) || (width == 792f && height == 612f))
                return "LETTER";
            else if ((width == 612f && height == 1008f) || (width == 1008f && height == 612f))
                return "LEGAL";
            else if ((width == 595.32f && height == 841.92f) || (width == 841.92f && height == 595.32f) || (width == 595f && height == 842f) || (width == 842f && height == 595f))
                return "A4";
            return "Unknown";
        }

        #endregion

        #region ProcessPCIResponse
        public static void ProcessPCIResponse()
        {
            log.Debug("Started Processing PCI Response");

            List<SubmittedBatches> submittedPrintBatches;
            submittedPrintBatches = DAL.GetSubmittedBatches();
            if (submittedPrintBatches.Count == 0)
            {
                log.Debug("Processing PCI Response job queue is empty.");
                return;
            }

            //Looping Distinct BatchId because in PrintBatchId will have same PrintBatchIDs( mainly beacause of Reprint functionality )
            foreach (var printTrailBatchId in submittedPrintBatches.Select(x => x.PrintTrailBatchId))
            {
                var printTrailBatchDetail = DAL.GetPrintBatchDetailByPrintBatchTrailId(printTrailBatchId);

                var bundlingRootPath = Path.Combine(Common.BundlingRootFolder, printTrailBatchId.ToString(), Common.BundlingResponseFolderName);

                //Check if directory exist or not
                if (!Directory.Exists(bundlingRootPath))
                {
                    log.Error("PCIResponsePath is not found for Batch-ID : {0}".Fill(printTrailBatchDetail.PrintBatchName));
                    continue;
                }

                #region By Pass PCI
                //If we are bypass PCI then we will generate PCI response over here.
                //if (bi.Client.SimulatePCI && (Common.ApplicationEnvironment == Env.Internal || Common.ApplicationEnvironment == Env.Staging || Common.ApplicationEnvironment == Env.QA))
                //{
                //    var mrdfResData = DAL.GetMRDFResponseDummyDataByJobId(bi.BatchSummary.PrintBatch);

                //    var mrdfResContent = new StringBuilder();
                //    var mrdfs = new List<string>();
                //    foreach (DataRow row in mrdfResData.Rows)
                //    {
                //        string mrd = string.Empty;

                //        foreach (var f in DAL.GetMRDFRequestSchema())
                //        {
                //            mrd += string.Format(f.DataFormat, row[f.FieldName]);
                //        }

                //        foreach (var f in DAL.GetMRDFResponseSchema())
                //        {
                //            if (f.StartPosition < 800)
                //            {
                //                mrd = mrd.Remove(f.StartPosition, f.Length);
                //                mrd = mrd.Insert(f.StartPosition, string.Format(f.DataFormat, row[f.FieldName]));
                //            }
                //            else
                //            {
                //                mrd += string.Format(f.DataFormat, row[f.FieldName]);
                //            }
                //        }
                //        mrdfs.Add(mrd.Remove(mrd.Length - 1, 1));
                //    }
                //    File.WriteAllLines(bi.ResponseMRDFPath, mrdfs.ToArray());
                //}
                #endregion

                //Check for OUT files only
                List<string> outFileList = Directory.GetFiles(Path.Combine(bundlingRootPath), "*.out").ToList();
                if (outFileList.Count == 0)
                {
                    continue;
                }

                try
                {
                    log.Debug("Print Trail Batch-Id : " + printTrailBatchId.ToString() + " have out file(s) to process.");

                    //Get MRDF Request Data List
                    var mrdfRequestData = DAL.GetMRDFRequestDataByPrintTrailId(printTrailBatchId);

                    foreach (string outFilePath in outFileList)
                    {
                        bool isSuspend = false;
                        bool isREPFile = false;

                        string outFileName = Path.GetFileName(outFilePath);

                        ////This logic is used for checking if outfile name is starting with <CustCode>-<REPXXXXXXX>.
                        //if (Path.GetFileNameWithoutExtension(outFilePath).StartsWith(bi.Client.CustCode + "-" + "REP", StringComparison.CurrentCultureIgnoreCase))
                        //{
                        //    isREPFile = true;
                        //}

                        //Read Out file from PCIResponse path
                        string[] mrdfOut = File.ReadAllLines(outFilePath);

                        var suspensionReason = "";

                        if (mrdfOut.Length == 0)
                        {
                            suspensionReason = "Batch is suspended due to return file contains no records.";
                            PCINotificationEmail(printTrailBatchDetail.PrintBatchName, suspensionReason, outFileName);

                            //Renaming file so that if we want to continue by reverting status and want to process new file then system will not pick up that file again and again
                            File.Move(outFilePath, Path.Combine(bundlingRootPath, outFileName + ".Suspended"));

                            break;
                        }

                        List<PCIFieldData> fieldDataCollection = new List<PCIFieldData>();

                        foreach (string record in mrdfOut)
                        {
                            if (record.Length != Common.MRDFResponseLength)
                            {
                                isSuspend = true;
                                suspensionReason = "Record length is '{0}'. It should be '{1}' as per the MRDF response schema. ".Fill(record.Length, Common.MRDFResponseLength);
                                break;
                            }

                            var pciFields = ParseMRDFResponse(record, isREPFile);
                            string mrdfJobId = pciFields.Single(x => x.FieldName == "JobId").StrValue.TrimStart('0').Trim();
                            if (mrdfJobId != printTrailBatchDetail.Id.ToString())
                            {
                                isSuspend = true;
                                suspensionReason = "Batch is suspended due to invalid JobId : {0}".Fill(mrdfJobId);
                                break;
                            }

                            string mrdfPieceId = pciFields.Single(x => x.FieldName == "PieceId").StrValue.TrimStart('0').Trim();
                            var maxPieceId = mrdfRequestData.Max(x => x.PieceId);
                            int pieceId;
                            bool isPieceIdValid = int.TryParse(mrdfPieceId, out pieceId);
                            if (!(isPieceIdValid && pieceId > 0 && pieceId <= maxPieceId))
                            {
                                isSuspend = true;
                                suspensionReason = "Batch is suspended due to invalid PieceId : {0}".Fill(mrdfPieceId);
                                break;
                            }

                            fieldDataCollection.Add(new PCIFieldData() { PCIFieldColl = pciFields, PieceId = pieceId });
                        }

                        if (isSuspend)
                        {
                            SuspendBatchAndSendPCIMail(printTrailBatchDetail.PrintBatchName, suspensionReason, outFileName);
                            //Renaming files so that if we want to continue by reverting status then system will not pick up that file again and again
                            File.Move(outFilePath, Path.Combine(bundlingRootPath, outFileName + ".Suspended"));
                            break;
                        }

                        //Get MRDF Response Piece ID

                        // Persist MRDF response
                        DAL.InsertMRDFResponseData(printTrailBatchDetail.Id, fieldDataCollection);

                        #region Response Validation

                        var resPieceIdData = fieldDataCollection.Select(c => c.PieceId).ToList();
                        var mrdfResponseData = DAL.GetMRDFResponseDataByPrintTrailBatchId(printTrailBatchId).AsEnumerable();

                        string validationErrors = null;

                        resPieceIdData.ForEach(pieceId =>
                        {
                            var errors = ValidateMRDFResponse(mrdfRequestData, mrdfResponseData, pieceId, printTrailBatchDetail.SubmittedOn.Value);

                            if (!errors.IsNullOrWhiteSpace())
                            {
                                validationErrors += Environment.NewLine + errors;
                            }
                        });

                        if (!validationErrors.IsNullOrWhiteSpace())
                        {
                            SuspendBatchAndSendPCIMail(printTrailBatchDetail.PrintBatchName, validationErrors, outFileName);

                            //Renaming file so that if we want to continue by reverting status and want to process new file then system will not pick up that file again and again
                            File.Move(outFilePath, Path.Combine(bundlingRootPath, outFileName + ".Suspended"));
                            break;
                        }
                        #endregion

                        // Update MRDF request based on MRDF response - MailPieceStatus : Shipped, PulledBack, Error, ErrId, ErrDesc

                        #region Update MRDF Request MailPieceStatus

                        DAL.UpdateMRDFRequestStatusFromResponse(mrdfResponseData, resPieceIdData);

                        #endregion


                        //If out file is processed successfully than create .Complete File So that if any new file come in it than it will not process it again and again
                        string processedFilePath = Path.Combine(bundlingRootPath, Path.GetFileName(outFilePath) + ".complete");
                        if (File.Exists(processedFilePath))
                            File.Delete(processedFilePath);
                        File.Move(outFilePath, processedFilePath);

                    }

                    #region Final Update After Receiving All MRDF Response

                    mrdfRequestData = DAL.GetMRDFRequestDataByPrintTrailId(printTrailBatchId);
                    //Here checking that whatever piece ids in mrdf we sent to PCI, all are received or not. if some fot them are remaining (means remaining count is not 0) than we are not going to generate report.

                    var inProgressMailPieces = mrdfRequestData.Where(row => row.MailPieceStatus == MailPieceStatus.InProgress.ToString());

                    if (inProgressMailPieces.Count() > 0)
                        continue;

                    //First update MRDF Received status because we have received all MRDF responses
                    DAL.UpdatePrintTrailBatchStatus(printTrailBatchId, BatchPipelineStatus.MRDFReceived, Common.GetCurrentTime());

                    var jobIds = mrdfRequestData.Select(p => p.JobId).Distinct().ToList();
                    foreach (var jobId in jobIds)
                    {
                        var inProgressMailPiecesForJob = DAL.GetMRDFRequestDataByJobId(jobId).AsEnumerable().Where(row => row.Field<string>("MailPieceStatus") == MailPieceStatus.InProgress.ToString());

                        if (inProgressMailPiecesForJob.Count() > 0)
                            continue;

                        DAL.UpdateBatchPipeLineStatus(jobId, BatchPipelineStatus.MRDFReceived, Common.GetCurrentTime());
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    log.Error("Error occured while processing mrdf out file {0}".Fill(Path.GetFileName(printTrailBatchDetail.PrintBatchName)), ex);

                    string validationErrors = "Batch suspended due to error while processing OUT file :  " + Environment.NewLine + ex.ToString();

                    SuspendBatchAndSendInternalMail(printTrailBatchId, validationErrors);
                }
            }
            log.Debug("ended preparing pns response");
        }

        private static string ValidateMRDFResponse(List<MailPieceDetail> reqMRDF, IEnumerable<DataRow> resMRDF, int pieceId, DateTime submittedOn)
        {
            var req = reqMRDF.Single(row => row.PieceId == pieceId);
            var res = resMRDF.Single(row => row.Field<int>("PieceId") == pieceId);

            var errors = ValidateMRDFResponse(req, res, submittedOn).ToList();

            if (errors.Count > 0)
            {
                var errDesc = "Errors in PieceId : {0}".Fill(pieceId);

                errors.ForEach(e =>
                {
                    errDesc = errDesc + Environment.NewLine + e;
                });

                errDesc = errDesc + Environment.NewLine;

                return errDesc;
            }

            return null;
        }

        private static IEnumerable<string> ValidateMRDFResponse(MailPieceDetail req, DataRow res, DateTime submittedOn)
        {
            if (!req.PDFFileName.Trim().Equals(res.Field<string>("PDFFileName").Trim(), StringComparison.CurrentCultureIgnoreCase))
                yield return "PDFFileName : Request({0}), Response({1})".Fill(req.PDFFileName.Trim(), res.Field<string>("PDFFileName").Trim());

            if (req.TotalSheets != res.Field<int>("TotalSheets"))
                yield return "TotalSheets : Request({0}), Response({1})".Fill(req.TotalSheets, res.Field<int>("TotalSheets"));

            if (res.Field<string>("Disposition").Trim() == "6")
                yield return "Disposition : Response(6)";

            if (res.Field<DateTime>("ProcessingTimeStamp") < submittedOn)
                yield return "ProcessingTimeStamp : Submitted({0}), ProcessingTimeStamp({1})".Fill(submittedOn, res.Field<DateTime>("ProcessingTimeStamp"));
            //if (res.Field<string>("Disposition").Trim() == "7")
            //    yield return "Disposition : Response(7)";

            //if ((req.Field<string>("DivertVS3").Trim() == "1") && (res.Field<string>("Disposition").Trim() != "8"))
            //    yield return "Disposition : Expected(8), Actual({0})".Fill(res.Field<string>("Disposition").Trim());

            yield break;
        }

        private static List<PCIField> ParseMRDFResponse(string record, bool isReprint)
        {
            var mrdfResponseFields = MRDFResponseSchema.Select(f => f.Clone()).ToList();

            foreach (var res in mrdfResponseFields)
            {
                res.StrValue = record.Substring(res.StartPosition, res.Length);

                if (res.FieldName == "ProcessingTimeStamp")
                    res.StrValue = Common.EstToUtc(res.Value).ToString(res.DataFormatTemplate.Replace("{0:", "").Replace("}", ""));


                if (isReprint)
                {
                    if (res.FieldName == "JobId")
                    {
                        res.StrValue = res.StrValue.Replace("REP", "000");
                    }
                    if (res.FieldName == "Expansion")
                    {
                        mrdfResponseFields.Single(x => x.FieldName == "PieceId").StrValue = res.StrValue;
                    }
                }
            }
            return mrdfResponseFields;
        }

        private static void SuspendBatchAndSendPCIMail(string printFileName, string suspensionReason, string outFileName)
        {
            //DAL.LogBatchTimeLine(batchSummary.PrintBatch, LogBatchTimeLine.ReportPrepEnd, Common.GetCurrentTime());

            //batchSummary.ValidationErrors = batchSummary.ValidationErrors + Environment.NewLine + suspensionReason;
            //batchSummary.ValidationErrors = batchSummary.ValidationErrors.Trim();

            //DAL.UpdatePnSRequestDataInBatchSummary(batchSummary);
            ////This is need to be updated first and then quickly change it to suspend
            //DAL.UpdateBatchPipeLineStatus(batchSummary.PrintBatch, BatchPipelineStatus.ReportPrepped, Common.GetCurrentTime());
            ////suspend entire batch
            //DAL.UpdateBatchPipeLineStatus(batchSummary.PrintBatch, BatchPipelineStatus.Suspended, Common.GetCurrentTime());

            PCINotificationEmail(printFileName, suspensionReason, outFileName);

        }

        public static void PCINotificationEmail(string printFileName, string suspensionReason, string outFileName)
        {
            MailUtility.SendInvalidMRDFOutFoundNotificationPCIEmail(printFileName, outFileName, suspensionReason);
        }
        #endregion

        #region GeneratePnSResponse

        public static void GeneratePnSResponse()
        {
            log.Debug("Started Generating PnSResponse");

            List<int> batchIds = DAL.GetNextBatchIdByJobName(JobTypes.PnSCoreService, OperationTypes.ProcessMRDF);

            if (batchIds == null || batchIds.Count == 0)
            {
                log.Debug("Generate PnS response job queue is empty.");
                return;
            }

            foreach (var batchId in batchIds)
            {
                DAL.LogBatchTimeLine(batchId, LogBatchTimeLine.ReportPrepStart, Common.GetCurrentTime());

                BatchSummary batchSummary = DAL.GetBatchSummaryById(batchId);
                Client clientDetail = DAL.GetClientAppDetailsById(batchSummary.ClientId);

                var bi = new BatchInfo(batchSummary, clientDetail);
                log.Debug("Report Prepped Job Started For Batch {0}".Fill(bi.BatchName));

                try
                {
                    //Check if directory exist or not
                    if (!Directory.Exists(Path.Combine(bi.PnSResponsePath)))
                    {
                        log.Error("PnSResponsePath is not found for Batch-ID : {0}".Fill(bi.BatchSummary.PrintBatch));
                        continue;
                    }

                    //Here we will get merged data of request and response to put it in mail piece data
                    DataTable dtResData = DAL.GetFinalMRDFResponseDataByJobId(bi.BatchSummary.PrintBatch);
                    var mailPieceReport = DAL.GetMailPieceReportList(dtResData);
                    var printBatchFormat = BatchSummarySchema.Single(c => c.FieldName == "PrintBatch").DataFormat;
                    var shippedCount = mailPieceReport.Count(c => c.MailPieceStatus == MailPieceStatus.Shipped);
                    var batchStatus = BatchStatus.ShippedAll;
                    var statusDesc = string.Empty;

                    if (shippedCount == 0)
                    {
                        // Check if all mail piece status is pulled back then Batch status will be pulled back.
                        var allPulledBack = mailPieceReport.All(c => c.MailPieceStatus == MailPieceStatus.PulledBack);
                        if (allPulledBack)
                        {
                            batchStatus = BatchStatus.PulledBack;
                            statusDesc = "All mail pieces were pulled back, check mail piece report for details";
                        }
                        else
                        {
                            batchStatus = BatchStatus.Failed;
                            statusDesc = "No mail pieces were shipped, check mail piece report for details";
                        }
                    }
                    else if (shippedCount < mailPieceReport.Count())
                    {
                        batchStatus = BatchStatus.ShippedPartial;
                        statusDesc = "Some mail pieces were not shipped, check mail piece report for details";
                    }
                    else
                    {
                        batchStatus = BatchStatus.ShippedAll;
                        statusDesc = "All mail pieces were printed and shipped successfully.";
                    }

                    PnSResponse pnsResponse = new PnSResponse()
                    {
                        BatchIdentifier = bi.BatchSummary.BatchIdentifier,
                        AppCode = clientDetail.AppCode,
                        BatchName = bi.BatchName,
                        BatchStatus = batchStatus,
                        MailPackageReport = mailPieceReport,
                        PCIBatchName = printBatchFormat.Fill(bi.BatchSummary.PrintBatch),
                        ReceivedOn = DateTime.SpecifyKind(bi.BatchSummary.ReceivedOn, DateTimeKind.Utc),
                        StatusDescription = statusDesc
                    };

                    File.WriteAllText(Path.Combine(bi.PnSResponsePath, "{0}-res.xml".Fill(bi.BatchName)), pnsResponse.ToXml());
                    File.WriteAllText(Path.Combine(bi.PnSResponsePath, "{0}.rpt".Fill(bi.BatchName)), "");

                    DAL.LogBatchTimeLine(batchId, LogBatchTimeLine.ReportPrepEnd, Common.GetCurrentTime());

                    DAL.UpdateBatchStatus(batchId, batchStatus.ToString());

                    #region Reprint Check
                    var containsReprint = mailPieceReport.Where(mp => mp.ErrId == (int)ErrorTypes.Reprint).ToList();
                    if (containsReprint.Count > 0)
                    {
                        SuspendBatchAndSendInternalMail(bi.BatchSummary.PrintBatch, "Batch is suspended because response mrdf contains reprint codes.");
                        continue;
                    }
                    #endregion

                    #region QCDiverted Validations
                    var qcRecords = dtResData.AsEnumerable().Where(x => x.Field<string>("DivertVS3").Trim() == "1");
                    var isQCPerformed = qcRecords.All(x => x.Field<string>("Disposition").Trim() == "8" || x.Field<string>("Disposition").Trim() == "4");

                    if (!isQCPerformed)
                    {
                        SuspendBatchAndSendInternalMail(bi.BatchSummary.PrintBatch, "Batch is suspended because QC Divert is not confirmed.");
                        continue;
                    }
                    #endregion

                    DAL.UpdateBatchPipeLineStatus(bi.BatchSummary.PrintBatch, BatchPipelineStatus.ReportPrepped, Common.GetCurrentTime());

                    log.Debug("Completed Generating PnSResponse");
                }
                catch (Exception ex)
                {
                    var msg = "Error occured while generating PnSResponse file {0} with Error {1}".Fill(Path.GetFileName(bi.ResponseMRDFPath), ex.Message);
                    log.Error(msg, ex);

                    MailUtility.SendUnexpectedNotificationEmail(msg);
                    continue;
                }
            }
        }

        private static void SuspendBatchAndSendInternalMail(int printTrailBatchId, string suspensionReason)
        {
            //DAL.LogBatchTimeLine(batchSummary.PrintBatch, LogBatchTimeLine.ReportPrepEnd, Common.GetCurrentTime());

            //batchSummary.ValidationErrors = batchSummary.ValidationErrors + Environment.NewLine + suspensionReason;

            //DAL.UpdatePnSRequestDataInBatchSummary(batchSummary);
            ////This is need to be updated first and then quickly change it to suspend
            //DAL.UpdateBatchPipeLineStatus(batchSummary.PrintBatch, BatchPipelineStatus.ReportPrepped, Common.GetCurrentTime());
            ////suspend entire batch
            //DAL.UpdateBatchPipeLineStatus(batchSummary.PrintBatch, BatchPipelineStatus.Suspended, Common.GetCurrentTime());

            MailUtility.SendBatchSuspendedNotificationInternalEmail(printTrailBatchId, suspensionReason);
        }
        #endregion

        #region Process Return PDFs information
        //public static void ProcessPCIReturnPDFsInfo()
        //{
        //    log.Debug("Started processing return pdfs information");

        //    List<SubmittedBatches> receivedPDFs;
        //    receivedPDFs = DAL.GetMRDFReceivedBatches();
        //    if (receivedPDFs.Count == 0)
        //    {
        //        log.Debug("Process PCI return PDFs job queue is empty.");
        //        return;
        //    }

        //    //Looping Distinct BatchId because in PrintBatchId will have same PrintBatchIDs( mainly beacause of Reprint functionality )
        //    foreach (var batchId in receivedPDFs.Select(x => x.PrintTrailBatchId).Distinct())
        //    {
        //        BatchSummary batchSummary = DAL.GetBatchSummaryById(batchId);
        //        Client clApp = DAL.GetClientAppDetailsById(batchSummary.ClientId);

        //        var bi = new BatchInfo(batchSummary, clApp);

        //        log.Debug("PCI return PDFs Job Started For Batch {0}".Fill(bi.BatchName));

        //        var docTypesMaps = DAL.GetDocTypeDetailsByAppId(clApp.Id).Where(x => x.DocTypeId == bi.BatchSummary.DocTypeId).Single();

        //        if (docTypesMaps != null)
        //            bi.InitializeDocTypeMaps(docTypesMaps, bi.BatchSummary.PrintBatch, bi.Client.AppCode, bi.Client.CustCode);

        //        //Check if directory exist or not
        //        if (!Directory.Exists(Path.Combine(bi.PCIResponseReturnPDFsPath)))
        //        {
        //            log.Error("PCIResponse return PDFs path is not found for Batch-ID : {0}".Fill(bi.BatchSummary.PrintBatch));
        //            continue;
        //        }

        //        //Check for pdf files only
        //        List<string> pdfFileList = Directory.GetFiles(Path.Combine(bi.PCIResponseReturnPDFsPath), "*.pdf").ToList();
        //        if (pdfFileList.Count == 0)
        //        {
        //            continue;
        //        }
        //        try
        //        {
        //            log.Debug("Batch-Id : " + batchId.ToString() + " have pdf file(s) to process.");

        //            //Get Only submitted MRDF Request Data to the PCI 
        //            var mrdfRequestData = DAL.GetMRDFRequestDataByJobId(bi.BatchSummary.PrintBatch, "INTERNAL").AsEnumerable().Select(c => new { JobId = c.Field<int>("JobId"), MailPieceId = c.Field<int>("PieceId"), FileName = c.Field<int>("JobId").ToString() + "-" + c.Field<int>("PieceId").ToString() + "-" + c.Field<string>("PDFFileName"), IsReceived = c.Field<bool>("IsPDFReceived") }).ToList();

        //            mrdfRequestData.ForEach(a =>
        //            {
        //                if (File.Exists(Path.Combine(bi.PCIResponseReturnPDFsPath, a.FileName)))
        //                {
        //                    DAL.UpdateFileReceivedStatus(a.JobId, a.MailPieceId);
        //                }
        //            });

        //            var updatedMrdfRequestData = DAL.GetMRDFRequestDataByJobId(bi.BatchSummary.PrintBatch, "INTERNAL").AsEnumerable().ToList();

        //            if (updatedMrdfRequestData.Count == updatedMrdfRequestData.Where(c => c.Field<bool>("IsPDFReceived") == true).Count())
        //            {
        //                DAL.UpdateFileReceivedBatchStatus(bi.BatchSummary.PrintBatch, Common.GetCurrentTime());
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            var msg = "Error occured while processing return pdf searching file {0} with Error {1}".Fill(Path.GetFileName(bi.PCIResponseReturnPDFsPath), ex.Message);
        //            log.Error(msg, ex);
        //            MailUtility.SendUnexpectedNotificationEmail(msg);
        //        }
        //    }
        //}
        #endregion
    }
}
