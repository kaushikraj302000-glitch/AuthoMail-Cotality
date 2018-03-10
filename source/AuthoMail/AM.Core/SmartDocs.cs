using AM.Core.USPSIMService;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Core
{
    public class SmartDocs
    {
        private BaseFont timesBaseFont = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, false);
        private BaseFont timesBold = BaseFont.CreateFont(BaseFont.TIMES_BOLD, BaseFont.CP1252, false);
        private BaseFont timesItalic = BaseFont.CreateFont(BaseFont.TIMES_ITALIC, BaseFont.CP1252, false);
        private Font arial = FontFactory.GetFont("Arial", 12.5f);
        private Font arialBold = FontFactory.GetFont("Arial", 12f, Font.BOLD);

        public string ExceptionDetail { get; private set; }
        public string TrackingCode { get; private set; }

        public int GenerateDoc(string coverTemplatePDFPath, string pdfAttachmentPath, string outputDocumentPath, Dictionary<string, string> mailPieceData, float contentScalePercent)
        {
            TrackingCode = string.Empty;

            string outputDirPath = Path.GetDirectoryName(outputDocumentPath);
            string flattenPDFPath = Path.Combine(outputDirPath, "Flatten_" + Path.GetFileName(pdfAttachmentPath));
            string shrinkPDFPath = Path.Combine(outputDirPath, "Shrinked_" + Path.GetFileName(pdfAttachmentPath));
            string coverLetterPDFPath = Path.Combine(outputDirPath, string.Format("CoverLetter_{0}.pdf", mailPieceData["MailPieceId"]));
            string mergeReadyFilePath = flattenPDFPath;

            int totalPage = 0;
            ExceptionDetail = string.Empty;

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


                #region Generate Cover Letter

                USPSIMClient client = new USPSIMClient();
                IMBarcodeInfo IMBarcode = client.GenerateIMBarcode("TEST", mailPieceData["MailPieceId"], mailPieceData["MailPieceZip"]);

                if (IMBarcode.Status == true)
                {
                    mailPieceData.Add("Barcode", IMBarcode.BarcodeString);
                    TrackingCode = IMBarcode.TrackingCode;
                }
                else
                {
                    ExceptionDetail = IMBarcode.ErrorMessage;
                    return 0;
                }

                CoverLetterGen(coverTemplatePDFPath, mailPieceData, coverLetterPDFPath);

                if (!File.Exists(coverLetterPDFPath))
                {
                    ExceptionDetail = "Unable to generate coverletter.";
                    return 0;
                }

                #endregion

                string[] mergePDFArray = { coverLetterPDFPath, mergeReadyFilePath };

                PdfUtil.BookmarkedPdfFilesMerge(outputDocumentPath, mergePDFArray);

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

                totalPage = PdfUtil.GetPageCount(outputDocumentPath);
            }
            catch (Exception err)
            {
                ExceptionDetail = err.ToString();
            }

            return totalPage;
        }



        public static void CoverLetterGen(string templatePath, Dictionary<string, string> data, string outputFilePath)
        {
            using (var existingFileStream = new FileStream(templatePath, FileMode.Open))
            using (var newFileStream = new FileStream(outputFilePath, FileMode.Create))
            {
                // Open existing PDF
                var pdfReader = new PdfReader(existingFileStream);

                // PdfStamper, which will create
                var stamper = new PdfStamper(pdfReader, newFileStream);

                var form = stamper.AcroFields;
                foreach (var model in data)
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

        private void GenEndPage(string templatePath, Dictionary<string, string> data, string endpagePDFPath)
        {
            using (var existingFileStream = new FileStream(templatePath, FileMode.Open))
            using (var newFileStream = new FileStream(endpagePDFPath, FileMode.Create))
            {
                // Open existing PDF
                var pdfReader = new PdfReader(existingFileStream);

                // PdfStamper, which will create
                var stamper = new PdfStamper(pdfReader, newFileStream);

                var form = stamper.AcroFields;
                foreach (var model in data)
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

    }
}
