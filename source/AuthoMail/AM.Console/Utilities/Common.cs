using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Xml.Schema;
using System.Xml;
using System.Xml.Linq;
using AM.Entities;
using AM.Lib;
namespace AM.Console
{
    public static class cfg
    {
        public static SmtpClient Email_SMTPClient { get; private set; }
        public static string SchemaPath { get; set; }
        public static string SchemaPathAMC { get; set; }
        public static XmlSchemaSet schemaAM;
        public static XmlSchemaSet schemasAMC;

        static cfg()
        {
            schemaAM = new XmlSchemaSet();
            schemasAMC = new XmlSchemaSet();
            SchemaPath = ConfigurationManager.AppSettings["SchemaPath"];
            schemaAM.Add("http://schemas.corelogic.com/ade/authomail/v1.18.0", cfg.SchemaPath);

            SchemaPathAMC = Path.GetFullPath(ConfigurationManager.AppSettings["SchemaPathAMC"]);
            schemasAMC.Add("http://schemas.corelogic.com/2015/amc/v1/packages", cfg.SchemaPathAMC);
           
            Email_SMTPClient = new SmtpClient();
        }

        public static void CleanDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            Directory.CreateDirectory(path);
        }

        public static SFT_ProtocolType GetProtocolType(string protocolType)
        {
            SFT_ProtocolType PnS_SFT_Protocol;
            switch (protocolType.ToLower())
            {
                case "ftp": PnS_SFT_Protocol = SFT_ProtocolType.ftp;
                    break;
                case "sftp": PnS_SFT_Protocol = SFT_ProtocolType.sftp;
                    break;
                case "unc": PnS_SFT_Protocol = SFT_ProtocolType.unc;
                    break;
                default: throw new Exception("Please provide valid protocol for 'PnS_SFT_Protocol_Value'.");
            }
            return PnS_SFT_Protocol;
        }

        public static ValidationResultAMC ValidateAMCXMLNode(XmlNode loan)
        {
            XDocument xDoc = XDocument.Parse(loan.OuterXml);
            StringBuilder sbErrorMessage = new StringBuilder();
            string outcomeCode = OutComeCodes.No_Error;

            xDoc.Validate(schemasAMC, (o, e) =>
            {
                sbErrorMessage.Append(e.Message);
            });

            string errorMsg = sbErrorMessage.ToString();
            if (errorMsg.Contains("Address") || errorMsg.Contains("City") || errorMsg.Contains("State") || errorMsg.Contains("Zip"))
                outcomeCode = OutComeCodes.Mailing_Address_Missing_Or_Invalid;
            else if (errorMsg.Contains("Recipient") || errorMsg.Contains("FirstName") || errorMsg.Contains("LastName"))
                outcomeCode = OutComeCodes.Recipient_Details_Missing_Or_Invalid;
            else if (!string.IsNullOrEmpty(errorMsg))
                outcomeCode = OutComeCodes.Invalid_XML;

            ValidationResultAMC returnCode = new ValidationResultAMC() { Code = outcomeCode, ErrorMessage = errorMsg };

            return returnCode;
        }

        public static ValidationResultAMC ValidateXMLNode(XmlNode xmlPiece, XmlNamespaceManager nsManager)
        {
            var errors = ValidateAMXMLForUSAA(xmlPiece, nsManager).ToList();

            if (errors.Count > 0)
            {
                return new ValidationResultAMC() { Code = string.Join("|", errors.Select(row => row.Code)), ErrorMessage = string.Join("|", errors.Select(row => row.ErrorMessage)) };
            }

            return new ValidationResultAMC() { Code = OutComeCodes.No_Error };
        }

        private static IEnumerable<ValidationResultAMC> ValidateAMXMLForUSAA(XmlNode xmlPiece, XmlNamespaceManager nsManager)
        {
            XDocument xDoc = XDocument.Parse(xmlPiece.OuterXml);
            StringBuilder sbErrorMessage = new StringBuilder();

            xDoc.Validate(schemasAMC, (o, e) =>
            {
                sbErrorMessage.Append(e.Message);
            });

            string errorMsg = sbErrorMessage.ToString();

            if (errorMsg.Contains("Address") || errorMsg.Contains("City") || errorMsg.Contains("State") || errorMsg.Contains("Zip"))
                yield return new ValidationResultAMC() { Code = OutComeCodes.Mailing_Address_Missing_Or_Invalid, ErrorMessage = "Recipient address details are missing or invalid in package." };

            if (errorMsg.Contains("Recipient") || errorMsg.Contains("FirstName") || errorMsg.Contains("LastName"))
                yield return new ValidationResultAMC() { Code = OutComeCodes.Recipient_Details_Missing_Or_Invalid, ErrorMessage = "Recipient details are missing or invalid in package." };

            if (!string.IsNullOrEmpty(errorMsg))
                yield return new ValidationResultAMC() { Code = OutComeCodes.Invalid_XML, ErrorMessage = errorMsg };

            yield break;
        }

        public static StringBuilder USAAErrorReport(BatchData data, string sftChannelName, string timeZone)
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

        public static StringBuilder USAADailyDetailOperationReport(BatchData data, string sftChannelName, string timeZone)
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

    public class ValidationResultAMC
    {
        public string Code { get; set; }
        public string ErrorMessage { get; set; }
    }
}
