using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Entities
{
    public class Batch
    {
        public Guid Id { get; set; }
        public int ClientId { get; set; }
        public int BatchClustId { get; set; }
        public string DocumentOrPackageType { get; set; }
        public string BatchName { get; set; }
        public string PipelineStatus { get; set; }
        public string ValidationStatus { get; set; }
        public string ValidationErrors { get; set; }
        public string BatchStatus { get; set; }
        public bool IsReceived { get; set; }
        public DateTime ReceivedOn { get; set; }
        public bool IsDownloaded { get; set; }
        public DateTime DownloadedOn { get; set; }
        public bool IsAcknowledged { get; set; }
        public DateTime AcknowledgedOn { get; set; }
        public bool IsValidated { get; set; }
        public DateTime ValidatedOn { get; set; }
        public bool IsPrepped { get; set; }
        public DateTime PreppedOn { get; set; }
        public bool IsSubmittedToShipping { get; set; }
        public DateTime SubmittedToShippingOn { get; set; }
        public bool IsShippingResponseReceived { get; set; }
        public DateTime ShippingResponseReceivedOn { get; set; }
        public bool IsReportPrepped { get; set; }
        public DateTime ReportPreppedOn { get; set; }
        public bool IsReportedBack { get; set; }
        public DateTime ReportedBackOn { get; set; }
        public DateTime DueDate { get; set; }
        public int SLADays { get; set; }
        public string BusinessFriendlyIdName { get; set; }
        public string BusinessFriendlyIdValue { get; set; }
        public bool IsReturnInfoSpecified { get; set; }
        public string RetRcptPrefix { get; set; }
        public string RetRcptFirstName { get; set; }
        public string RetRcptMiddleName { get; set; }
        public string RetRcptLastName { get; set; }
        public string RetRcptSuffix { get; set; }
        public string RetRcptPhone { get; set; }
        public string RetRcptOrganization { get; set; }
        public string RetRcptAddrOptionalLine1AboveStreet { get; set; }
        public string RetRcptAddrOptionalLine2AboveStreet { get; set; }
        public string RetRcptAddressStreet { get; set; }
        public string RetRcptAddressAptOrSuiteOrOther { get; set; }
        public string RetRcptAddrOptionalLine1BelowStreet { get; set; }
        public string RetRcptAddrOptionalLine2BelowStreet { get; set; }
        public string RetRcptAddressCity { get; set; }
        public string RetRcptAddressState { get; set; }
        public string RetRcptAddressZIP { get; set; }
    }

    public class BatchCustomInfo
    {
        public Guid BatchId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
