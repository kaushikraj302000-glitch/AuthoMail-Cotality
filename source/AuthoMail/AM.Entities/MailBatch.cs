using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AM.Entities
{
    [Serializable]
    [XmlRoot(Namespace = "http://schemas.corelogic.com/ade/authomail/v1.18.0")]
    public class MailBatch
    {
        [XmlAttribute]
        public Guid Id { get; set; }

        [XmlAttribute]
        public string BusinessFriendlyIdName { get; set; }

        [XmlAttribute]
        public string BusinessFriendlyIdValue { get; set; }

        [XmlAttribute]
        public string ChannelCode { get; set; }

        [XmlAttribute]
        public Priority Priority { get; set; }

        [XmlAttribute]
        public string DocumentOrPackageType { get; set; }

        [XmlElement(ElementName = "ReturnInfo")]
        public ReturnInfo ReturnInfo { get; set; }

        [XmlArrayItem]
        public List<MailPiece> MailPieces { get; set; }

        [XmlArrayItem]
        public List<Metadata> CustomInfo { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://schemas.corelogic.com/ade/authomail/v1.18.0")]
    public class MailPiece
    {
        public MailPiece()
        {
            DocumentOrPackage = new DocumentOrPackage();
            RecipientInfo = new RecipientInfo();
            ReturnInfo = new ReturnInfo();
        }

        [XmlAttribute]
        public Guid Id { get; set; }

        [XmlAttribute]
        public string BusinessFriendlyIdName { get; set; }

        [XmlAttribute]
        public string BusinessFriendlyIdValue { get; set; }

        [XmlAttribute]
        public string DocumentOrPackageType { get; set; }

        [XmlElement(ElementName = "DocumentOrPackage")]
        public DocumentOrPackage DocumentOrPackage { get; set; }

        [XmlElement(ElementName = "RecipientInfo")]
        public RecipientInfo RecipientInfo { get; set; }

        [XmlElement(ElementName = "ReturnInfo")]
        public ReturnInfo ReturnInfo { get; set; }

        [XmlArrayItem]
        public List<Metadata> CustomInfo { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://schemas.corelogic.com/ade/authomail/v1.18.0")]
    public class DocumentOrPackage
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Pattern { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://schemas.corelogic.com/ade/authomail/v1.18.0")]
    public class RecipientInfo
    {
        public RecipientInfo()
        {
            Recipient = new Recipient();
            Corecipient = new Recipient();
            Address = new Address();
        }
        [XmlElement(ElementName = "Recipient")]
        public Recipient Recipient { get; set; }

        [XmlElement(ElementName = "Corecipient")]
        public Recipient Corecipient { get; set; }

        [XmlElement(ElementName = "Address")]
        public Address Address { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://schemas.corelogic.com/ade/authomail/v1.18.0")]
    public class Recipient
    {

        [XmlAttribute]
        public string Organization { get; set; }

        [XmlAttribute]
        public string Prefix { get; set; }

        [XmlAttribute]
        public string FirstName { get; set; }

        [XmlAttribute]
        public string MiddleName { get; set; }

        [XmlAttribute]
        public string LastName { get; set; }

        [XmlAttribute]
        public string Suffix { get; set; }

        [XmlAttribute]
        public string Phone { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://schemas.corelogic.com/ade/authomail/v1.18.0")]
    public class Address
    {
        [XmlAttribute]
        public string OptionalLine1AboveStreet { get; set; }

        [XmlAttribute]
        public string OptionalLine2AboveStreet { get; set; }

        [XmlAttribute]
        public string Street { get; set; }

        [XmlAttribute]
        public string AptOrSuiteOrOther { get; set; }

        [XmlAttribute]
        public string OptionalLine1BelowStreet { get; set; }

        [XmlAttribute]
        public string OptionalLine2BelowStreet { get; set; }

        [XmlAttribute]
        public string City { get; set; }

        [XmlAttribute]
        public string State { get; set; }

        [XmlAttribute]
        public string Zip { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://schemas.corelogic.com/ade/authomail/v1.18.0")]
    public class ReturnInfo
    {
        public ReturnInfo()
        {
            Recipient = new Recipient();
            Address = new Address();
        }
        [XmlElement(ElementName = "Recipient")]
        public Recipient Recipient { get; set; }

        [XmlElement(ElementName = "Address")]
        public Address Address { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://schemas.corelogic.com/ade/authomail/v1.18.0")]
    public class Metadata
    {
        [XmlAttribute]
        public string Key { get; set; }

        [XmlAttribute]
        public DataType DataType { get; set; }

        [XmlAttribute]
        public string Value { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://schemas.corelogic.com/ade/authomail/v1.18.0")]
    public enum Priority
    {
        High,
        Medium,
        Low
    }

    [Serializable]
    [XmlType(Namespace = "http://schemas.corelogic.com/ade/authomail/v1.18.0")]
    public enum DataType
    {
        String,
        DateTime,
        Boolean,
        Integer,
        Float
    }
}
