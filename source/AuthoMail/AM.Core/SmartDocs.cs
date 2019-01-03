using AM.Core.USPSIMService;
using AM.Services;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;
using AM.USPSLib;

namespace AM.Core
{
    public class SmartDocs
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private BaseFont timesBaseFont = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, false);
        private BaseFont timesBold = BaseFont.CreateFont(BaseFont.TIMES_BOLD, BaseFont.CP1252, false);
        private BaseFont timesItalic = BaseFont.CreateFont(BaseFont.TIMES_ITALIC, BaseFont.CP1252, false);
        private Font arial = FontFactory.GetFont("Arial", 12.5f);
        private Font arialBold = FontFactory.GetFont("Arial", 12f, Font.BOLD);

        public SmartDocResponse GenerateUSPSIMBCoverLetter(string appCode, string coverTemplatePDFPath, string outputDir, Dictionary<string, string> mailPieceData)
        {
            appCode = Config.Common_Environment == Env.Prod ? appCode : "TEST";

            SmartDocResponse response = new SmartDocResponse();

            string coverLetterPDFPath = Path.Combine(outputDir, string.Format("CoverLetter_{0}.pdf", mailPieceData["MailPieceId"]));

            #region Generate Cover Letter

            USPSIMClient client = new USPSIMClient();
            IMBarcodeInfo IMBarcode = client.GenerateIMBarcode(appCode, mailPieceData["MailPieceId"], mailPieceData["MailPieceZip"]);

            if (IMBarcode.Status == true)
            {
                mailPieceData.Add("Barcode", IMBarcode.BarcodeString);
                response.TrackingCode = IMBarcode.TrackingCode;
                mailPieceData.Add("FrontBarcode", "*" + IMBarcode.TrackingCode + "*");
            }
            else
            {
                response.HasError = true;
                response.ExceptionDetail = "Error occured while generating IMBarcode :" + IMBarcode.ErrorMessage;
                return response;
            }

            try
            {
                PdfFormFiller(coverTemplatePDFPath, coverLetterPDFPath, mailPieceData);

                if (!File.Exists(coverLetterPDFPath))
                {
                    response.HasError = true;
                    response.ExceptionDetail = "Unable to generate cover letter.";
                    return response;
                }
                response.CoverLetterPath = coverLetterPDFPath;
            }
            catch (Exception ex)
            {
                var msg = "Error occured while generating cover letter for MailPiece-Id : {0}".Fill(mailPieceData["MailPieceId"]);
                log.Error(msg, ex);
                response.HasError = true;
                response.ExceptionDetail = msg;
                return response;
            }
            return response;

            #endregion
        }

        private void PdfFormFiller(string templatePDFPath, string filledPDFPath, Dictionary<string, string> mailPieceData)
        {
            using (var existingFileStream = new FileStream(templatePDFPath, FileMode.Open))
            using (var newFileStream = new FileStream(filledPDFPath, FileMode.Create))
            {
                // Open existing PDF
                var pdfReader = new PdfReader(existingFileStream);

                // PdfStamper, which will create
                var stamper = new PdfStamper(pdfReader, newFileStream);

                var form = stamper.AcroFields;
                foreach (var model in mailPieceData)
                {
                    form.SetField(model.Key, model.Value);
                }

                // "Flatten" the form so it wont be editable/usable anymore
                stamper.FormFlattening = true;

                // You can also specify fields to be flattened, which
                // leaves the rest of the form still be editable/usable
                // stamper.PartialFormFlattening("Address");

                stamper.Close();
                pdfReader.Close();
            }
        }

        public SmartDocResponse GenerateEVSCoverLetter(string appCode, string sysUserId, string outputDir, string mailPieceId, eVSLabelRequest eVSReq)
        {
            SmartDocResponse response = new SmartDocResponse();

            response.ShippingServiceType = eVSReq.ServiceType.Value;

            string coverLetterPDFPath = Path.Combine(outputDir, string.Format("eVSCoverLetter_{0}.pdf", mailPieceId));

            #region Generate Cover Letter

            try
            {
                eVSLabelResponse eVSRes;

                using (IUSPSService uspsSvc = new USPSService())
                {
                    eVSRes = uspsSvc.RequesteVSLabelAsync(sysUserId, eVSReq).Result;

                    if (eVSRes.HasError)
                    {
                        //If we get any error while generating USPS Priority express label then we are going to try for USPS Priority label
                        eVSReq.ServiceType = AM.USPSLib.ServiceType.PRIORITY;

                        eVSRes = uspsSvc.RequesteVSLabelAsync(sysUserId, eVSReq).Result;

                        if (eVSRes.HasError)
                        {
                            response.HasError = true;
                            response.ExceptionDetail = "Error occured while generating eVS label for MailPiece-Id:{0} with Exception : {1}".Fill(mailPieceId, eVSRes.Error.Description);
                            return response;
                        }
                    }
                    response.ShippingServiceType = eVSReq.ServiceType.Value;

                    File.WriteAllBytes(coverLetterPDFPath, Convert.FromBase64String(eVSRes.LabelImageString));
                    response.CoverLetterPath = coverLetterPDFPath;
                }

                response.TrackingCode = eVSRes.TrackingNumber;
                response.PostageCost = eVSRes.Postage;
            }
            catch (Exception ex)
            {
                var msg = "Error occured while generating cover letter for MailPiece-Id : {0} with Error : {1}".Fill(mailPieceId, ex.Message);
                log.Error(msg, ex);
                response.HasError = true;
                response.ExceptionDetail = msg;
                return response;
            }
            return response;

            #endregion
        }

        public SmartDocResponse GenerateDoc(string coverLetterPDFPath, string pdfAttachmentPath, string outputDocumentPath, float contentScalePercent, string endPagePDFPath = "")
        {
            SmartDocResponse response = new SmartDocResponse();

            string outputDirPath = Path.GetDirectoryName(outputDocumentPath);
            string flattenPDFPath = Path.Combine(outputDirPath, "Flatten_" + Path.GetFileName(pdfAttachmentPath));
            string shrinkPDFPath = Path.Combine(outputDirPath, "Shrinked_" + Path.GetFileName(pdfAttachmentPath));

            string mergeReadyFilePath = flattenPDFPath;

            try
            {
                #region FlattenCode
                PdfUtil.FlattenPDF(pdfAttachmentPath, flattenPDFPath);
                #endregion

                #region Shrink Document
                if (contentScalePercent != 100f)
                {
                    PdfUtil.ShrinkAndCenterPDF(flattenPDFPath, shrinkPDFPath, contentScalePercent);
                    mergeReadyFilePath = shrinkPDFPath;
                }
                #endregion

                #region Merge Final Document

                if (!string.IsNullOrEmpty(endPagePDFPath))
                {
                    PdfUtil.MergePDFs(coverLetterPDFPath, mergeReadyFilePath, endPagePDFPath, outputDocumentPath);
                }
                else if (!string.IsNullOrEmpty(coverLetterPDFPath))
                {
                    PdfUtil.MergePDFs(coverLetterPDFPath, mergeReadyFilePath, outputDocumentPath);
                }
                else
                {
                    //Here we are just renaming file as a output document because we are not merging any cover letter for it
                    File.Move(mergeReadyFilePath, outputDocumentPath);
                }

                #endregion

                #region Remove Extra Document After Merging
                if (File.Exists(coverLetterPDFPath))
                {
                    File.Delete(coverLetterPDFPath);
                }

                if (File.Exists(flattenPDFPath))
                {
                    File.Delete(flattenPDFPath);
                }

                if (File.Exists(shrinkPDFPath))
                {
                    File.Delete(shrinkPDFPath);
                }

                if (File.Exists(endPagePDFPath))
                {
                    File.Delete(endPagePDFPath);
                }
                #endregion

                response.TotalPages = PdfUtil.GetPageCount(outputDocumentPath);
            }
            catch (Exception ex)
            {
                var msg = "Error occured while generating final document '{0}.pdf'".Fill(Path.GetFileNameWithoutExtension(outputDocumentPath));

                log.Error(msg, ex);
                response.HasError = true;
                response.ExceptionDetail = msg;
                return response;
            }

            return response;
        }

        public SmartDocResponse GenerateLastPageWithBarcode(string endPageTemplatePath, string outputDocumentPath, Dictionary<string, string> mailPieceData)
        {
            SmartDocResponse response = new SmartDocResponse();

            string endPagePDFPath = Path.Combine(outputDocumentPath, string.Format("EndPage_{0}.pdf", mailPieceData["MailPieceId"]));
            try
            {
                PdfFormFiller(endPageTemplatePath, endPagePDFPath, mailPieceData);
                response.CoverLetterPath = endPagePDFPath;
            }
            catch (Exception ex)
            {
                var msg = "Error occured while generating end page document '{0}.pdf'".Fill(Path.GetFileNameWithoutExtension(outputDocumentPath));

                log.Error(msg, ex);
                response.HasError = true;
                response.ExceptionDetail = msg;
                return response;
            }

            return response;
        }

        public SmartDocResponse GenerateFrontBarcodeCoverLetter(string coverTemplatePDFPath, string outputDir, Dictionary<string, string> mailPieceData)
        {
            SmartDocResponse response = new SmartDocResponse();

            string coverLetterPDFPath = Path.Combine(outputDir, string.Format("CoverLetter_{0}.pdf", mailPieceData["MailPieceId"]));

            #region Generate Cover Letter
            try
            {
                PdfFormFiller(coverTemplatePDFPath, coverLetterPDFPath, mailPieceData);

                if (!File.Exists(coverLetterPDFPath))
                {
                    response.HasError = true;
                    response.ExceptionDetail = "Unable to generate cover letter.";
                    return response;
                }
                response.CoverLetterPath = coverLetterPDFPath;
            }
            catch (Exception ex)
            {
                var msg = "Error occured while generating cover letter for MailPiece-Id : {0}".Fill(mailPieceData["MailPieceId"]);
                log.Error(msg, ex);
                response.HasError = true;
                response.ExceptionDetail = msg;
                return response;
            }
            return response;

            #endregion
        }
    }

    public class SmartDocResponse
    {
        public string CoverLetterPath { get; set; }
        public string TrackingCode { get; set; }
        public ServiceType ShippingServiceType { get; set; }
        public decimal PostageCost { get; set; }
        public int TotalPages { get; set; }
        public bool HasError { get; set; }
        public string ExceptionDetail { get; set; }
    }
}
