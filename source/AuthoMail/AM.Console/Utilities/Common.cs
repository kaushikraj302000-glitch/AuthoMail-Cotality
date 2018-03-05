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

        public static readonly DateTime Now = DateTime.UtcNow;

        public static readonly DateTime Today = DateTime.UtcNow.Date;

        public static XmlSchemaSet schemas;
        public static XmlSchemaSet schemasAMC;

        static Common()
        {
            schemas = new XmlSchemaSet();
            schemasAMC = new XmlSchemaSet();
            AuthoMailRootFolder = Path.GetFullPath(ConfigurationManager.AppSettings["AuthoMailRootFolder"]);
            AuthoMailRootFolder.CreateDirectoryIfNotExist();
            SchemaPath = ConfigurationManager.AppSettings["SchemaPath"];
            schemas.Add("http://schemas.corelogic.com/ade/authomail/v1.18.0", Common.SchemaPath);

            SchemaPathAMC = Path.GetFullPath(ConfigurationManager.AppSettings["SchemaPathAMC"]);
            schemasAMC.Add("http://schemas.corelogic.com/2015/amc/v1/packages", Common.SchemaPathAMC);
            PnS_SFT_HostURL = ConfigurationManager.AppSettings["PnS_SFT_HostURL"];
            PnS_SFT_UserName = ConfigurationManager.AppSettings["PnS_SFT_UserName"];
            PnS_SFT_Password = ConfigurationManager.AppSettings["PnS_SFT_Password"];
            PnS_SFT_RootDir = ConfigurationManager.AppSettings["PnS_SFT_RootDir"];

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

        public static DateTime EstToUtc(object dt)
        {
            //This is defined because we don't need to verify by DateTime.Parse. it is already in proper date.
            DateTime dtVal = Convert.ToDateTime(dt);
            // Treat date as Est irrecpective of client and server timezone and covert it to UTC

            dt = DateTime.SpecifyKind(dtVal, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTime(dtVal, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"), TimeZoneInfo.FindSystemTimeZoneById("UTC"));
        }

        public static DateTime UtcToEst(object dt)
        {
            //This is defined because we don't need to verify by DateTime.Parse. it is already in proper date.
            DateTime dtVal = Convert.ToDateTime(dt);
            // Treat date as Est irrecpective of client and server timezone and covert it to UTC

            dt = DateTime.SpecifyKind(dtVal, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTime(dtVal, TimeZoneInfo.FindSystemTimeZoneById("UTC"), TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
        }
        public static DateTime UtcToClientTimeZone(DateTime dt, string clientTimeZone)
        {
            dt = DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
            // Treat date as IST irrecpective of client and server timezone and covert it to UTC
            return TimeZoneInfo.ConvertTime(dt, TimeZoneInfo.FindSystemTimeZoneById("UTC"), TimeZoneInfo.FindSystemTimeZoneById(clientTimeZone));
        }
        public static DateTime EstToUtc(DateTime dt)
        {
            dt = DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
            // Treat date as IST irrecpective of client and server timezone and covert it to UTC
            return TimeZoneInfo.ConvertTime(dt, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"), TimeZoneInfo.FindSystemTimeZoneById("UTC"));
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
                outcomeCode = OutComeCodes.Recipient_Name_Missing_Or_Invalid;
            else if (!string.IsNullOrEmpty(errorMsg))
                outcomeCode = OutComeCodes.Invalid_XML;

            ValidationResultAMC returnCode = new ValidationResultAMC() { Code = outcomeCode, ErrorMessage = errorMsg };

            return returnCode;
        }
        public static ValidationResultAMC ValidateAMMailPiecesForUSAA(MailPiece piece)
        {
            StringBuilder sbErrorMessage = new StringBuilder();
            string outcomeCode = OutComeCodes.No_Error;
            string errorMsg = sbErrorMessage.ToString();

            if (piece.RecipientInfo == null || piece.RecipientInfo.Recipient == null)
            {
                outcomeCode = OutComeCodes.Recipient_Name_Missing_Or_Invalid;
                sbErrorMessage.AppendLine("Recipient details are missing in package.");
            }
            else if (string.IsNullOrEmpty(piece.RecipientInfo.Recipient.FirstName) || string.IsNullOrEmpty(piece.RecipientInfo.Recipient.LastName))
            {
                outcomeCode = OutComeCodes.Recipient_Name_Missing_Or_Invalid;
                sbErrorMessage.AppendLine("First name or last name is missing from Recipient details.");
            }
            else if (piece.RecipientInfo.Address == null)
            {
                outcomeCode = OutComeCodes.Mailing_Address_Missing_Or_Invalid;
                sbErrorMessage.AppendLine("Recipient address details are missing in package.");
            }
            else if (string.IsNullOrEmpty(piece.RecipientInfo.Address.Street) || string.IsNullOrEmpty(piece.RecipientInfo.Address.City) || string.IsNullOrEmpty(piece.RecipientInfo.Address.State) || string.IsNullOrEmpty(piece.RecipientInfo.Address.Zip))
            {
                outcomeCode = OutComeCodes.Mailing_Address_Missing_Or_Invalid;
                sbErrorMessage.AppendLine("Street or City or State or Zip is missing from recipient address in package.");
            }
            else if (string.IsNullOrEmpty(piece.DocumentOrPackageType))
            {
                outcomeCode = OutComeCodes.Invalid_XML;
                sbErrorMessage.AppendLine("Package name is not defined in package.");
            }
            else if (string.IsNullOrEmpty(piece.DocumentOrPackage.Name))
            {
                outcomeCode = OutComeCodes.Invalid_XML;
                sbErrorMessage.AppendLine("File attribute is not defined in package.");
            }
            else if (piece.ReturnInfo == null || piece.ReturnInfo.Recipient == null)
            {
                outcomeCode = OutComeCodes.Invalid_XML;
                sbErrorMessage.AppendLine("Return info is not defined in package.");
            }
            else if (piece.ReturnInfo == null || piece.ReturnInfo.Recipient == null)
            {
                outcomeCode = OutComeCodes.Invalid_XML;
                sbErrorMessage.AppendLine("Return info is not defined in package.");
            }
            else if (piece.ReturnInfo.Address == null || piece.ReturnInfo.Recipient == null)
            {
                outcomeCode = OutComeCodes.Invalid_XML;
                sbErrorMessage.AppendLine("Return address is not defined in package.");
            }
            else if (string.IsNullOrEmpty(piece.ReturnInfo.Address.Street) || string.IsNullOrEmpty(piece.ReturnInfo.Address.City) || string.IsNullOrEmpty(piece.ReturnInfo.Address.State) || string.IsNullOrEmpty(piece.ReturnInfo.Address.Zip))
            {
                outcomeCode = OutComeCodes.Invalid_XML;
                sbErrorMessage.AppendLine("Street or City or State or Zip is missing from return address in package.");
            }
            else if (piece.CustomInfo.Where(c => c.Key == "ShippingMethod").SingleOrDefault() == null)
            {
                outcomeCode = OutComeCodes.Invalid_XML;
                sbErrorMessage.AppendLine("Shipping method is not defined in package.");
            }
            else if (piece.CustomInfo.Where(c => c.Key == "ShippingCredentials").SingleOrDefault() == null)
            {
                outcomeCode = OutComeCodes.Invalid_XML;
                sbErrorMessage.AppendLine("Shipping method is not defined in package.");
            }
            else if (piece.CustomInfo.Where(c => c.Key == "ShippingCredCode").SingleOrDefault() == null)
            {
                outcomeCode = OutComeCodes.Invalid_XML;
                sbErrorMessage.AppendLine("Shipping credential code is not defined in package.");
            }
            else if (piece.CustomInfo.Where(c => c.Key == "RequestId").SingleOrDefault() == null)
            {
                outcomeCode = OutComeCodes.Invalid_XML;
                sbErrorMessage.AppendLine("RequestId is not defined in package.");
            }
            else if (piece.CustomInfo.Where(c => c.Key == "CustomerIdentifier").SingleOrDefault() == null)
            {
                outcomeCode = OutComeCodes.Invalid_XML;
                sbErrorMessage.AppendLine("CustomerIdentifier is not defined in package.");
            }

            ValidationResultAMC returnCode = new ValidationResultAMC() { Code = outcomeCode, ErrorMessage = errorMsg };

            return returnCode;
        }

    }

    public class ValidationResultAMC
    {
        public string Code { get; set; }
        public string ErrorMessage { get; set; }
    }
}
