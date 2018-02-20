using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace AM.Web
{
    public class Helper
    {
        public static void MailMerge(string templatePath, Dictionary<string, string> data, string outputFilePath)
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
    }
}