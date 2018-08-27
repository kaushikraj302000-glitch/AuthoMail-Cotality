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
        public LabelSize Size { get; set; }

        [XmlElement(ElementName = "XCoordinate")]
        public int XCoordinate { get; set; }

        [XmlElement(ElementName = "YCoordinate")]
        public int YCoordinate { get; set; }

        [XmlElement(ElementName = "LabelSequence")]
        public LabelSequence Sequence { get; set; }
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
        public DeliveryOption DeliveryOption { get; set; }

        [XmlElement(ElementName = "WaiverOfSignature")]
        public bool WaiverOfSignature { get; set; }
    }

    public class Content
    {
        [XmlElement(ElementName = "ContentType")]
        public ContentType ContentType { get; set; }

        public ContentDescription ContentDescription { get; set; }
    }

    public class ItemDetail
    {
        public int MyProperty { get; set; }
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
}
