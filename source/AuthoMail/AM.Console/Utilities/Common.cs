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
    public static class Common
    {
        public static string AuthoMailRootFolder { get; set; }
        public static string SchemaPath { get; set; }
        public static string SchemaPathAMC { get; set; }
        public static SmtpClient Email_SMTPClient { get; private set; }
        public static string PnS_SFT_HostURL { get; private set; }
        public static string PnS_SFT_UserName { get; private set; }
        public static string PnS_SFT_Password { get; private set; }
        public static string PnS_SFT_RootDir { get; private set; }

        public static string DownloadEmailTemplate { get; private set; }
        public static string DownloadEmail_Subject { get; private set; }
        public static string ProccessEmail_Subject { get; private set; }
        public static string NoBatchReceivedNotification_Subject { get; set; }
        public static string PnSEmail_Subject { get; private set; }
        public static string Email_To { get; private set; }
        public static string Email_Cc { get; private set; }
        public static bool SendEmails { get; private set; }

        public static string ReportSFTUploadingPathAMC { get; private set; }
        public static string ReportSFTUploadingPathCL { get; private set; }

        public static readonly DateTime Now = DateTime.UtcNow;

        public static readonly DateTime Today = DateTime.UtcNow.Date;

        public static XmlSchemaSet schemaAM;
        public static XmlSchemaSet schemasAMC;

        public static Dictionary<string, string> ValidStatesForUSAA { get; set; }
        public static string StateListPathForUSAA { get; set; }

        static Common()
        {
            schemaAM = new XmlSchemaSet();
            schemasAMC = new XmlSchemaSet();
            AuthoMailRootFolder = Path.GetFullPath(ConfigurationManager.AppSettings["AuthoMailRootFolder"]);
            AuthoMailRootFolder.CreateDirectoryIfNotExist();
            SchemaPath = ConfigurationManager.AppSettings["SchemaPath"];
            schemaAM.Add("http://schemas.corelogic.com/ade/authomail/v1.18.0", Common.SchemaPath);

            SchemaPathAMC = Path.GetFullPath(ConfigurationManager.AppSettings["SchemaPathAMC"]);
            schemasAMC.Add("http://schemas.corelogic.com/2015/amc/v1/packages", Common.SchemaPathAMC);
            PnS_SFT_HostURL = ConfigurationManager.AppSettings["PnS_SFT_HostURL"];
            PnS_SFT_UserName = ConfigurationManager.AppSettings["PnS_SFT_UserName"];
            PnS_SFT_Password = ConfigurationManager.AppSettings["PnS_SFT_Password"];
            PnS_SFT_RootDir = ConfigurationManager.AppSettings["PnS_SFT_RootDir"];

            DownloadEmailTemplate = ConfigurationManager.AppSettings["DownloadEmailTemplate"];
            DownloadEmail_Subject = ConfigurationManager.AppSettings["DownloadEmail_Subject"];
            ProccessEmail_Subject = ConfigurationManager.AppSettings["ProccessEmail_Subject"];

            Email_SMTPClient = new SmtpClient();

            Email_To = ConfigurationManager.AppSettings["Email_To"];
            Email_Cc = ConfigurationManager.AppSettings["Email_Cc"];

            ValidStatesForUSAA = new Dictionary<string, string>();
            StateListPathForUSAA = Path.GetFullPath(ConfigurationManager.AppSettings["StateListPathForUSAA"]);
            if (!string.IsNullOrEmpty(StateListPathForUSAA))
            {
                if (File.Exists(StateListPathForUSAA))
                {
                    var stateList = File.ReadAllLines(StateListPathForUSAA);
                    foreach (string state in stateList)
                    {
                        string[] stateCombo = state.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        if (stateCombo.Count() > 0)
                        {
                            ValidStatesForUSAA.Add(stateCombo[0], stateCombo[1]);
                        }
                    }
                }
            }

            ReportSFTUploadingPathAMC = ConfigurationManager.AppSettings["ReportSFTUploadingPathAMC"];
            ReportSFTUploadingPathCL = ConfigurationManager.AppSettings["ReportSFTUploadingPathCL"];
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
            {
                yield return new ValidationResultAMC() { Code = OutComeCodes.Mailing_Address_Missing_Or_Invalid, ErrorMessage = "Recipient address details are missing in package." };
            }
            else
            {
                if (!errorMsg.Contains("Zip"))
                {
                    var recipientElement = xmlPiece.SelectSingleNode("./x:RecipientInfo", nsManager);
                    if (recipientElement != null)
                    {
                        var recipeintAddressElement = recipientElement.SelectSingleNode("./x:Address", nsManager);

                        if (recipeintAddressElement != null)
                        {
                            var stateValue = recipeintAddressElement.SelectSingleNode("./x:State/text()", nsManager).Value.Trim();
                            if (!ValidStatesForUSAA.Keys.Contains(stateValue))
                            {
                                yield return new ValidationResultAMC() { Code = OutComeCodes.Mailing_Address_Missing_Or_Invalid, ErrorMessage = "State is not from 50 states provided by client." };
                            }
                        }
                    }
                }
            }

            if (errorMsg.Contains("Recipient") || errorMsg.Contains("FirstName") || errorMsg.Contains("LastName"))
                yield return new ValidationResultAMC() { Code = OutComeCodes.Recipient_Details_Missing_Or_Invalid, ErrorMessage = "Recipient details are missing in package." };
            if (!string.IsNullOrEmpty(errorMsg))
                yield return new ValidationResultAMC() { Code = OutComeCodes.Invalid_XML, ErrorMessage = errorMsg };

            yield break;
        }

        public static ValidationResultAMC ValidateAMMailPiecesForUSAA(MailPiece piece)
        {
            StringBuilder sbErrorMessage = new StringBuilder();
            string outcomeCode = string.Empty;

            if (piece.RecipientInfo == null || piece.RecipientInfo.Recipient == null)
            {
                outcomeCode += OutComeCodes.Recipient_Details_Missing_Or_Invalid + "|";
                sbErrorMessage.AppendLine("Recipient details are missing in package.");
            }
            else if (string.IsNullOrEmpty(piece.RecipientInfo.Recipient.FirstName) || string.IsNullOrEmpty(piece.RecipientInfo.Recipient.LastName))
            {
                outcomeCode += OutComeCodes.Recipient_Details_Missing_Or_Invalid + "|";
                sbErrorMessage.AppendLine("First name or last name is missing from Recipient details.");
            }
            else if (piece.RecipientInfo.Address == null)
            {
                outcomeCode += OutComeCodes.Mailing_Address_Missing_Or_Invalid + "|";
                sbErrorMessage.AppendLine("Recipient address details are missing in package.");
            }

            if (string.IsNullOrEmpty(piece.DocumentOrPackageType))
            {
                outcomeCode += OutComeCodes.Invalid_XML + "|";
                sbErrorMessage.AppendLine("Package name is not defined in package.");
            }
            if (string.IsNullOrEmpty(piece.DocumentOrPackage.Name))
            {
                outcomeCode += OutComeCodes.Invalid_XML + "|";
                sbErrorMessage.AppendLine("File attribute is not defined in package.");
            }
            if (piece.ReturnInfo == null || piece.ReturnInfo.Recipient == null)
            {
                outcomeCode += OutComeCodes.Invalid_XML + "|";
                sbErrorMessage.AppendLine("Return info is not defined in package.");
            }
            if (piece.ReturnInfo.Address == null)
            {
                outcomeCode += OutComeCodes.Invalid_XML + "|";
                sbErrorMessage.AppendLine("Return address is not defined in package.");
            }
            if (piece.CustomInfo.Where(c => c.Key == "ShippingMethod").SingleOrDefault() == null)
            {
                outcomeCode += OutComeCodes.Invalid_XML + "|";
                sbErrorMessage.AppendLine("Shipping method is not defined in package.");
            }
            if (piece.CustomInfo.Where(c => c.Key == "ShippingCredCode").SingleOrDefault() == null)
            {
                outcomeCode += OutComeCodes.Invalid_XML + "|";
                sbErrorMessage.AppendLine("Shipping credential code is not defined in package.");
            }
            if (piece.CustomInfo.Where(c => c.Key == "RequestId").SingleOrDefault() == null)
            {
                outcomeCode += OutComeCodes.Invalid_XML + "|";
                sbErrorMessage.AppendLine("RequestId is not defined in package.");
            }
            if (piece.CustomInfo.Where(c => c.Key == "CustomerIdentifier").SingleOrDefault() == null)
            {
                outcomeCode += OutComeCodes.Invalid_XML + "|";
                sbErrorMessage.AppendLine("CustomerIdentifier is not defined in package.");
            }
            if (string.IsNullOrEmpty(outcomeCode))
                outcomeCode = OutComeCodes.No_Error;
            else
                outcomeCode = outcomeCode.TrimEnd('|');

            ValidationResultAMC returnCode = new ValidationResultAMC() { Code = outcomeCode, ErrorMessage = sbErrorMessage.ToString() };

            return returnCode;
        }

        public static StringBuilder USAAErrorReport(BatchData data, string sftChannelName)
        {
            StringBuilder errorReport = new StringBuilder("BatchID,ID,Channel,Package Name,Loan Number,File Name,Date Received,First Name,Last Name,Recipient Street Address,Recipient City,Recipient State,Recipient Zip,Error Code,Error Text,Shipping Method,Customer Identifier,RequestID");

            errorReport.AppendLine();

            // All the loans where outcome code is anything except no_error are processed invalid loans.
            foreach (MailPieceDTO mailPiece in data.MailPieces.Where(l => l.ErrorCode != OutComeCodes.No_Error))
            {
                errorReport.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17}",
                        data.MailBatch.BatchName == null ? string.Empty : data.MailBatch.BatchName,
                        mailPiece.UniqueId,
                        sftChannelName,
                        mailPiece.DocumentOrPackageType == null ? string.Empty : mailPiece.DocumentOrPackageType,
                        mailPiece.BusinessFriendlyIdValue == null ? string.Empty : mailPiece.BusinessFriendlyIdValue,
                        mailPiece.FileName == null ? string.Empty : mailPiece.FileName,
                        data.MailBatch.ReceivedOn == null ? string.Empty : data.MailBatch.ReceivedOn.Value.UtcToEst().ToString(),
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

        public static StringBuilder USAADailyDetailOperationReport(BatchData data, string sftChannelName)
        {
            StringBuilder deliveryReport = new StringBuilder("BatchID,ID,Channel,Package Name,Loan Number,File Name,Date Received,Date Printed,Date Mailed,Client Code,Tracking Number,Shipping Method,Customer Identifier,First Name,Last Name,Recipient Street Address,Recipient City,Recipient State,Recipient Zip,RequestID,Print Location,Pages Shipped");

            deliveryReport.AppendLine();

            // All the loans where outcome code is anything except no_error are processed invalid loans.
            foreach (MailPieceDTO mailPiece in data.MailPieces.Where(l => l.ShippingStatus == ShippingStatus.Shipped.ToString()))
            {
                deliveryReport.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21}",
                        data.MailBatch.BatchName == null ? string.Empty : data.MailBatch.BatchName,
                        mailPiece.UniqueId,
                        sftChannelName,
                        mailPiece.DocumentOrPackageType == null ? string.Empty : mailPiece.DocumentOrPackageType,
                        mailPiece.BusinessFriendlyIdValue == null ? string.Empty : mailPiece.BusinessFriendlyIdValue,
                        mailPiece.FileName == null ? string.Empty : mailPiece.FileName,
                        data.MailBatch.ReceivedOn == null ? string.Empty : data.MailBatch.ReceivedOn.Value.UtcToEst().ToString(),
                        mailPiece.PrintedOn == null ? string.Empty : mailPiece.PrintedOn.Value.UtcToEst().ToString(),
                        mailPiece.ShippedOn == null ? string.Empty : mailPiece.ShippedOn.Value.UtcToEst().ToString(),
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
