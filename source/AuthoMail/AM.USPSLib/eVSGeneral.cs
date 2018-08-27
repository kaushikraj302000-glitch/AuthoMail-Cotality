using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AM.USPSLib
{
    public class LabelParameters
    {
        [XmlElement(ElementName = "ImageParameter")]
        public LabelSize? Size { get; set; }

        [XmlElement(ElementName = "XCoordinate")]
        public int XCoordinate { get; set; }

        [XmlElement(ElementName = "YCoordinate")]
        public int YCoordinate { get; set; }

        [XmlElement(ElementName = "LabelSequence")]
        public LabelSequence Sequence { get; set; }

        public bool ShouldSerializeSize() { return this.Size.HasValue; }
    }

    public class LabelSequence
    {
        [XmlElement(ElementName = "PackageNumber")]
        public int PackageNumber { get; set; }

        [XmlElement(ElementName = "TotalPackages")]
        public int TotalPackages { get; set; }
    }

    public class ExpressMailOptions
    {
        [XmlElement(ElementName = "DeliveryOption")]
        public DeliveryOption? DeliveryOption { get; set; }

        [XmlElement(ElementName = "WaiverOfSignature")]
        public bool WaiverOfSignature { get; set; }

        public bool ShouldSerializeDeliveryOption() { return this.DeliveryOption.HasValue; }
    }

    public class Content
    {
        [XmlElement(ElementName = "ContentType")]
        public ContentType? ContentType { get; set; }

        public ContentDescription? ContentDescription { get; set; }

        public bool ShouldSerializeContentType() { return this.ContentType.HasValue; }
        public bool ShouldSerializeContentDescription() { return this.ContentDescription.HasValue; }
    }

    public class ItemDetail
    {
        public string Description { get; set; }
        public int Quantity { get; set; }
        public Decimal Value { get; set; }
        public int NetPounds { get; set; }
        public Decimal NetOunces { get; set; }
        public string HSTariffNumber { get; set; }
        public string CountryofOrigin { get; set; }
    }

    public class Commitment
    {
        [XmlElement(ElementName = "CommitmentName")]
        public string CommitmentName { get; set; }

        [XmlElement(ElementName = "ScheduledDeliveryDate")]
        public string ScheduledDeliveryDate { get; set; }
    }

    public class CommitmentDetail
    {
        public string CommitmentName { get; set; }

        public DateTime ScheduledDeliveryDate { get; set; }
    }

    public static class AddressTypes
    {
        public const string POBOX = "POBOX";
        public const string Military = "Military";
        public const string Territory = "Territory";

        //DC,Hawaii,Alaska and All Mainlands
        public const string DHAM = "DHAM";
    }

    public static class ResponseSource
    {
        //Response by USPS
        public const string USPS = "USPS";
        //Response by This Service
        public const string CLSVC = "CLSVC";
    }
}
