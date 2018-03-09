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

        public bool GenerateDoc(string coverTemplatePDFPath, string pdfAttachmentPath, string outputDocumentPath, Dictionary<string, string> data, string mailPieceId, string recipientZip)
        {
            TrackingCode = string.Empty;
            string outputDirPath = Path.GetDirectoryName(outputDocumentPath);
            bool isDocGenerated = false;
            ExceptionDetail = string.Empty;

            try
            {
                #region Generate CoverLetter

                USPSIMClient client = new USPSIMClient();
                IMBarcodeInfo IMBarcode = client.GenerateIMBarcode("TEST", mailPieceId, recipientZip);

                if (IMBarcode.Status == true)
                {
                    data.Add("Barcode", IMBarcode.BarcodeString);
                    TrackingCode = IMBarcode.TrackingCode;
                }
                else
                {
                    ExceptionDetail = IMBarcode.ErrorMessage;
                    return false;
                }

                string coverLetterPDFPath = Path.Combine(outputDirPath, string.Format("CoverLetter_{0}.pdf", mailPieceId));

                CoverLetterGen(coverTemplatePDFPath, data, coverLetterPDFPath);

                #region Generate EndPage with barcode

                string endpagePDFPath = Path.Combine(outputDirPath, string.Format("EndPage_{0}.pdf", mailPieceId));

                GenEndPage(Common.PDFEndPageTemplatePath, data, endpagePDFPath);

                #endregion

                //#region Shrink by 96%
                //var tempShrinkedFilePath = Path.Combine(Path.GetDirectoryName(pdfAttachmentPath), Path.GetFileNameWithoutExtension(pdfAttachmentPath) + "_shrink.pdf");
                //ShrinkAndCenterPDF(pdfAttachmentPath, tempShrinkedFilePath);
                //#endregion

                string[] mergePDFArray = { coverLetterPDFPath, pdfAttachmentPath, endpagePDFPath, outputDocumentPath };
                PdfUtil.MergePDFs(mergePDFArray);
                #endregion

                if (!File.Exists(coverLetterPDFPath))
                {
                    ExceptionDetail = "Unable to generate coverletter.";
                    return false;
                }
                File.Delete(coverLetterPDFPath);

                //if (File.Exists(tempShrinkedFilePath))
                //{
                //    File.Delete(tempShrinkedFilePath);
                //}

                if (File.Exists(endpagePDFPath))
                {
                    File.Delete(endpagePDFPath);
                }

                isDocGenerated = true;
            }
            catch (Exception err)
            {
                ExceptionDetail = err.ToString();
            }

            return isDocGenerated;
        }

        public void ShrinkAndCenterPDF(string src, string dest)
        {
            PdfReader reader = new PdfReader(src);
            PdfStamper stamper = new PdfStamper(reader, new FileStream(dest, FileMode.Create));
            int n = reader.NumberOfPages;
            float percentage = 0.96f;
            for (int p = 1; p <= n; p++)
            {
                float offsetX = (reader.GetPageSize(p).Width * (1 - percentage)) / 2;
                float offsetY = (reader.GetPageSize(p).Height * (1 - percentage)) / 2;
                stamper.GetUnderContent(p).SetLiteral(String.Format("\nq {0} 0 0 {0} {1} {2} cm\nq\n", percentage, offsetX, offsetY));
                stamper.GetOverContent(p).SetLiteral("\nQ\nQ\n");
            }
            stamper.Close();
            reader.Close();
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
                    //IList<AcroFields.FieldPosition> fieldPositions = form.GetFieldPositions(model.Key);
                    //if (fieldPositions == null || fieldPositions.Count <= 0) throw new ApplicationException("Error locating field");
                    //AcroFields.FieldPosition fieldPosition = fieldPositions[0];
                    //fieldPosition.position.Left = Utilities.InchesToPoints(5.88f);
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
                    //IList<AcroFields.FieldPosition> fieldPositions = form.GetFieldPositions(model.Key);
                    //if (fieldPositions == null || fieldPositions.Count <= 0) throw new ApplicationException("Error locating field");
                    //AcroFields.FieldPosition fieldPosition = fieldPositions[0];
                    //fieldPosition.position.Left = Utilities.InchesToPoints(5.88f);
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
