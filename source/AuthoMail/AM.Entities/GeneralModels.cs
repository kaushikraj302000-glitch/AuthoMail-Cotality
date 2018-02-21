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

    public static class BatchPipelineStatus
    {
        public const string Received = "Received";
        public const string Downloaded = "Downloaded";
        public const string Acknowledged = "Acknowledged";
        public const string Validated = "Validated";
        public const string Prepped = "Prepped";
        public const string SubmittedToShipping = "SubmittedToShipping";
        public const string ShippingResponseReceived = "ShippingResponseReceived";
        public const string ReportPrepped = "ReportPrepped";
        public const string ReportedBack = "ReportedBack";
    }
    public enum OutComeCodes
    {
        No_error,
        Invalid_Zip_File,
        Invalid_XML_file,
        Invalid_PDF_file, 
        Missing_PDF_file,
        Mailing_address_missing_or_invalid,
        Recipient_name_missing_or_invalid,
        UNKNOWN_error,
    }
}
