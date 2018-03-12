using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
