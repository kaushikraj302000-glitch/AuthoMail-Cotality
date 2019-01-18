using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Entities
{
    public class DocType
    {
        public int Id { get; set; }
        public string DocTypeCode { get; set; }
        public string Description { get; set; }
        public bool IsBRE { get; set; }
        public string PrintSize { get; set; }
        public int ContentScalePercent { get; set; }
        public bool IsActive { get; set; }
        public string ShippingCarrier { get; set; }
        public string ShippingServiceType { get; set; }
        public string ShippingAccountCode { get; set; }
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
        public string ClientEmails { get; set; }
        public string ClientContactNo { get; set; }
        public string SupportEmails { get; set; }
        public string SupportContactNo { get; set; }
        public int ClientId { get; set; }
        public float QCPercent { get; set; }
        public string BizdayCutOff { get; set; }
        public string BizdaySLAType { get; set; }
        public string BizdayTimeSpan { get; set; }
        public string SatCutOff { get; set; }
        public string SatSLAType { get; set; }
        public string SatTimeSpan { get; set; }
        public string SunCutOff { get; set; }
        public string SunSLAType { get; set; }
        public string SunTimeSpan { get; set; }
        public DateTime ModifiedOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public string CreatedBy { get; set; }
        public string ShippingSpecialService { get; set; }
        public string EnvelopeId { get; set; }
        public string BREEnvelopeId { get; set; }
        public int NoOfCopies { get; set; }
    }

    public class DocTypeCoverLetter
    {
        public int DocTypeId { get; set; }
        public int SeqId { get; set; }
        public string CoverLetterTemplatePath { get; set; }
    }

    public class PrintVendor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int SFTChannelId { get; set; }
        public string BusinessCode { get; set; }
        public string VendorCode { get; set; }
    }

    public class PrintTrail
    {
        public int Id { get; set; }
        public string TrailName { get; set; }
        public int PrintVendorId { get; set; }
    }

    public class DocTypePrintTrailMap
    {
        public int DocTypeId { get; set; }
        public int PrintTrailId { get; set; }
        public TimeSpan BizDayStartTime { get; set; }
        public TimeSpan BizDayEndTime { get; set; }
        public TimeSpan SatStartTime { get; set; }
        public TimeSpan SatEndTime { get; set; }
        public TimeSpan SunStartTime { get; set; }
        public TimeSpan SunEndTime { get; set; }
    }
}
