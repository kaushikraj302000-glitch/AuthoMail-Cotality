using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;
using System.IO;
namespace AM.Entities
{
    public static class BatchValidationStatus
    {
        public const string Valid = "Valid";
        public const string PartiallyValid = "PartiallyValid";
        public const string Invalid = "Invalid";
    }

    public enum BatchStatus
    {
        InProgress,
        Failed,
        ShippedPartial,
        ShippedAll
    }

    public static class BatchPipelineStatus
    {
        public const string Received = "Received";
        public const string Downloaded = "Downloaded";
        public const string Validated = "Validated";
        public const string Acknowledged = "Acknowledged";
        public const string Prepped = "Prepped";
        public const string SubmittedToShipping = "SubmittedToShipping";
        public const string ShippingResponseReceived = "ShippingResponseReceived";
        public const string ReportPrepped = "ReportPrepped";
        public const string ReportedBack = "ReportedBack";
        public const string Suspended = "Suspended";
        public const string Cancelled = "Cancelled";
    }

    public static class OutComeCodes
    {
        public const string No_Error = "No_Error";
        public const string Missing_Zip_File = "Missing_Zip_File";
        public const string Invalid_Zip_File = "Invalid_Zip_File";
        public const string Invalid_XML = "Invalid_XML";
        public const string Missing_XML = "Missing_XML";
        public const string Invalid_PDF = "Invalid_PDF";
        public const string Missing_PDF = "Missing_PDF";
        public const string Mailing_Address_Missing_Or_Invalid = "Mailing_Address_Missing_Or_Invalid";
        public const string Recipient_Details_Missing_Or_Invalid = "Recipient_Details_Missing_Or_Invalid";
        public const string Unknown_Error = "Unknown_Error";
        public const string Invalid_PackageType = "Invalid_PackageType";
        public const string Invalid_ShippingDetails = "Invalid_ShippingDetails";
        public const string Invalid_FedExAccount = "Invalid_FedExAccount";
    }

    public enum ShippingStatus
    {
        InProgress,
        PulledBack,
        Error,
        Shipped,
        Unknown
    }

    public static class PostDownloadAction
    {
        public const string Delete = "Delete";
        public const string Archive = "Archive";
    }

    public static class AMLogBatchTimeLine
    {
        public const string DownloadStart = "DownloadStart";
        public const string DownloadEnd = "DownloadEnd";
        public const string ValidationStart = "ValidationStart";
        public const string ValidationEnd = "ValidationEnd";
        public const string AcknowledgeStart = "AcknowledgeStart";
        public const string AcknowledgeEnd = "AcknowledgeEnd";
        public const string PreppedStart = "PreppedStart";
        public const string PreppedEnd = "PreppedEnd";
        public const string SubmittedToShippingStart = "SubmittedToShippingStart";
        public const string SubmittedToShippingEnd = "SubmittedToShippingEnd";
        public const string ShippingResponseReceivedStart = "ShippingResponseReceivedStart";
        public const string ShippingResponseReceivedEnd = "ShippingResponseReceivedEnd";
        public const string ReportPrepStart = "ReportPrepStart";
        public const string ReportPrepEnd = "ReportPrepEnd";
        public const string ReportBackStart = "ReportBackStart";
        public const string ReportBackEnd = "ReportBackEnd";
    }

    public enum ErrorTypes
    {
        IO = -1,
        CannotResize = -2,
        PageCountMismatch = -3,
        Reprint = 7
    }

    public enum ShippingServiceType
    {
        USPS = 0,
        FEDEX_2_DAY = 1,
        FEDEX_EXPRESS_SAVER = 2,
        FEDEX_GROUND = 3,
        FIRST_OVERNIGHT = 4,
        GROUND_HOME_DELIVERY = 5,
        PRIORITY_OVERNIGHT = 6,
        STANDARD_OVERNIGHT = 7
    }

    public class MailBatchInfo
    {
        public string BatchRepo { get; private set; }
        public string AMRequestPath { get; private set; }
        public string PnSRequestPath { get; private set; }
        public string PnSResponsePath { get; private set; }
        public string AMResponsePath { get; private set; }
        public string BatchName { get; private set; }
        public MailBatchDTO MailBatch { get; private set; }
        public Client Client { get; private set; }
        public DocType DocType { get; set; }
        public SFTChannel SFTChannel { get; private set; }

        public MailBatchInfo(MailBatchDTO mailBatch, Client client, SFTChannel sftChannel = null, string authoMailRootFolder = "")
        {
            if (client == null || mailBatch == null) throw new ArgumentNullException("Client or MailBatch cannot be null.");

            MailBatch = mailBatch;
            Client = client;
            SFTChannel = sftChannel;

            Initialize(client, mailBatch, authoMailRootFolder);
        }

        private void Initialize(Client client, MailBatchDTO mailBatch, string authoMailRootFolder)
        {
            BatchName = mailBatch.BatchName.Trim();
            //Combined path
            BatchRepo = Path.Combine(authoMailRootFolder, client.AppCode, "{0}".Fill(mailBatch.Id));

            AMRequestPath = Path.Combine(BatchRepo, "AMRequest");
            PnSRequestPath = Path.Combine(BatchRepo, "PnSRequest");
            AMResponsePath = Path.Combine(BatchRepo, "AMResponse");
            PnSResponsePath = Path.Combine(BatchRepo, "PnSResponse");
        }
    }
}
