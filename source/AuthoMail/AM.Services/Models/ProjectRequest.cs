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
        public string FileTransmittedVia { get; set; }
        public string NoOfEstSweepTimes { get; set; }
        public string ESTSweepTime { get; set; }
        public string TypeOfFiles { get; set; }
        public string TransmittedTime { get; set; }
        public string ServiceLevel { get; set; }
        public int? EstimatedPagesPerPackage { get; set; }
        public int? EstimatedPackagesDaily { get; set; }
        public int? EstimatedPackagesMonthly { get; set; }
        public int? EstimatedPackagesAnnually { get; set; }
        public string DailyPrintSchedule { get; set; }
        public string PaperPrintRequirements { get; set; }
        public string PackagesSentToCustomer { get; set; }
        public bool IsSampleEnvelopeAttached { get; set; }
        public string Envelopes { get; set; }
        public bool IsBRERequired { get; set; }
        public string BusinessReplyMailEnvelope { get; set; }
        public bool IsCustomerSupplyEnvelope { get; set; }
        public bool IsCustomerProvideArtworkEnvelope { get; set; }
        public bool IsClientPostageAccount { get; set; }
        public bool IsScanInAuthoMailDallas { get; set; }

        public string FilesTransmittedOther { get; set; }
        public string TypeOfFilesOther { get; set; }
        public string PaperAndPrintDetails { get; set; }
        public string ShippingMailingDetail { get; set; }
        public string MailingEnvelopeDetail { get; set; }
        public string BusinessReplyMailEnvelopeDetail { get; set; }
        public string ListOfDocTypesAndPages { get; set; }
        public string PDFSample { get; set; }
        public bool InsertRequired { get; set; }
        public bool BookletRequired { get; set; }
        public bool BusinessReplyMailEnvelopeRequired { get; set; }
        public bool OtherMaterialRequired { get; set; }
        public string InsertMaterialOrderedBy { get; set; }

        public string AdditionalInfo { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
    }
    //public class ProjectRequestFormHeaders
    //{
    //    public string HeaderCode { get; set; }
    //    public string HeaderTitle { get; set; }
    //    public string AlphaSeqCode { get; set; }
    //}
    //public class ProjectRequestLookupForm
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //    public string FieldName { get; set; }
    //    public string DisplayHeader
    //    {
    //        get
    //        {
    //            if (ProjectRequestFormHeaders == null) return null;
    //            return string.Format("{0}. {1}", this.ProjectRequestFormHeaders.AlphaSeqCode, this.ProjectRequestFormHeaders.HeaderTitle);
    //        }
    //    }
    //    public ProjectRequestFormHeaders ProjectRequestFormHeaders { get; set; }
    //    public List<ProjectRequestLookupFormValue> ProjectRequestLookupFormValueList { get; set; }
    //}
    //public class ProjectRequestLookupFormValue
    //{
    //    public int Id { get; set; }
    //    public string Title { get; set; }
    //    public int ProjectRequestLookupFormId { get; set; }
    //}
    //public class ProjectRequestValueMapping
    //{
    //    public int ProjectRequestId { get; set; }
    //    public int ProjectRequestLookupFormValueId { get; set; }
    //    public string Value { get; set; }
    //}

    public class Envelopes
    {
        public string Id { get; set; }
        public int CurrentQuantity { get; set; }
        public bool IsBREEnvelope { get; set; }
    }
    public class EnvelopeTransactions
    {
        public int Id { get; set; }
        public string EnvelopeId { get; set; }
        public string Description { get; set; }
        public DateTime ReceivedOn { get; set; }
        public int ReceivedQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public Envelopes Envelopes { get; set; }
    }
}
