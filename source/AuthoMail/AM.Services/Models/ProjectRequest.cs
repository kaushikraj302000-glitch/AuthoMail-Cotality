using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class ProjectRequest
    {
        public int Id { get; set; }
        public string ClientName { get; set; }
        public DateTime? SubmittedOn { get; set; }
        public string SubmittedBy { get; set; }
        public DateTime? UATStartOn { get; set; }
        public DateTime? ProdStartOn { get; set; }
        public string Description { get; set; }
        public string ESTSweepTime { get; set; }
        public string TransmittedTime { get; set; }
        public int EstimatedPagesPerPackage { get; set; }
        public int EstimatedPackagesDaily { get; set; }
        public int EstimatedPackagesMonthly { get; set; }
        public int EstimatedPackagesAnnually { get; set; }
        public bool IsBRERequired { get; set; }
        public bool IsCustomerSupplyEnvelope { get; set; }
        public bool IsCustomerProvideArtworkEnvelope { get; set; }
        public bool IsClientPostageAccount { get; set; }
        public bool IsScanInAuthoMailDallas { get; set; }
        public string AdditionalInfo { get; set; }
        public List<ProjectRequestLookupForm> ProjectRequestLookupFormList { get; set; }
        public List<ProjectRequestValueMapping> ProjectRequestValueMappingList { get; set; }
    }

    public class ProjectRequestLookupForm
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FieldName { get; set; }
        public List<ProjectRequestLookupFormValue> ProjectRequestLookupFormValueList { get; set; }
    }
    public class ProjectRequestLookupFormValue
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int ProjectRequestLookupFormId { get; set; }
    }
    public class ProjectRequestValueMapping
    {
        public int ProjectRequestId { get; set; }
        public int ProjectRequestLookupFormValueId { get; set; }
        public string Value { get; set; }
    }
}
