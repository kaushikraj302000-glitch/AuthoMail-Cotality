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
        public const string Invalid_Zip_File = "Invalid_Zip_File";
        public const string Invalid_XML = "Invalid_XML";
        public const string Invalid_PDF = "Invalid_PDF";
        public const string Missing_PDF = "Missing_PDF";
        public const string Mailing_Address_Missing_Or_Invalid = "Mailing_Address_Missing_Or_Invalid";
        public const string Recipient_Name_Missing_Or_Invalid = "Recipient_Name_Missing_Or_Invalid";
        public const string Unknown_Error = "Unknown_Error";
    }

    public enum ShippingStatus
    {
        InProgress,
        Aborted,
        PulledBack,
        Error,
        Shipped,
        Unknown
    }
}
