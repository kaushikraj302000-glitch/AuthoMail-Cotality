using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AM.Lib;

namespace AM.Services
{
    public class eVSLabelRequest
    {
        public string UserId { get; set; }

        public string Option { get; set; }

        public string Revision { get; set; }

        public ImageParameters ImageParameters { get; set; }

        public string FromName { get; set; }

        public string FromFirm { get; set; }

        public string FromAddress1 { get; set; }

        public string FromAddress2 { get; set; }

        public string FromCity { get; set; }

        public string FromState { get; set; }

        public string FromZip5 { get; set; }

        public string FromZip4 { get; set; }

        public string FromPhone { get; set; }

        public string POZipCode { get; set; }

        public bool AllowNonCleansedOriginAddr { get; set; }

        public string ToName { get; set; }

        public string ToFirm { get; set; }

        public string ToAddress1 { get; set; }

        public string ToAddress2 { get; set; }

        public string ToCity { get; set; }

        public string ToState { get; set; }

        public string ToZip5 { get; set; }

        public string ToZip4 { get; set; }

        public string ToPhone { get; set; }

        public bool POBox { get; set; }

        public bool AllowNonCleansedDestAddr { get; set; }

        public string WeightInOunces { get; set; }

        public ServiceType ServiceType { get; set; }

        public Container Container { get; set; }

        public decimal Width { get; set; }

        public decimal Length { get; set; }

        public decimal Height { get; set; }

        public Machinable Machinable { get; set; }

        public string ProcessingCategory { get; set; }

        public PriceOption PriceOptions { get; set; }

        public decimal InsuredAmount { get; set; }

        public bool AddressServiceRequested { get; set; }

        public ExpressMailOptions ExpressMailOptions { get; set; }

        public DateTime ShipDate { get; set; }

        public string CustomerRefNo { get; set; }

        public List<ExtraService> ExtraServices { get; set; }

        public string PermitNumber { get; set; }

        public string PermitZIPCode { get; set; }

        public string PermitHolderName { get; set; }

        public string CRID { get; set; }

        public string MID { get; set; }

        public string SenderName { get; set; }

        public string SenderEMail { get; set; }

        public string RecipientName { get; set; }

        public string RecipientEMail { get; set; }

        public ReceiptOption ReceiptOption { get; set; }

        public ImageType ImageType { get; set; }

        public HoldForManifest HoldForManifest { get; set; }

        public bool NineDigitRoutingZip { get; set; }

        public bool ShipInfo { get; set; }
    }

    public class eVSLabelResponse
    {
        public string BarcodeNumber { get; set; }

        public string LabelImageString { get; set; }

        public string ReceiptImageString { get; set; }

        public string ToName { get; set; }

        public string ToFirm { get; set; }

        public string ToAddress1 { get; set; }

        public string ToAddress2 { get; set; }

        public string ToCity { get; set; }

        public string ToState { get; set; }

        public string ToZip5 { get; set; }

        public string ToZip4 { get; set; }

        public string Postnet { get; set; }

        public string RDC { get; set; }

        public decimal Postage { get; set; }

        public List<ExtraServices> Extraservices { get; set; }

        public string Zone { get; set; }

        public string DimensionalWeight { get; set; }

        public string CarrierRoute { get; set; }

        public string PermitHolderName { get; set; }

        public string InductionType { get; set; }

        public string LogMessage { get; set; }

        public CommitmentDetail Commitment { get; set; }

        public USPSError Error { get; set; }

        [XmlIgnore]
        public bool HasError
        {
            get
            {
                if (this.Error == null) return false;
                return !(String.IsNullOrWhiteSpace(this.Error.Number) &&
                         String.IsNullOrWhiteSpace(this.Error.Description));
            }
            set { }
        }
    }

    public class CommitmentDetail
    {
        [XmlElement(ElementName = "CommitmentName")]
        public string CommitmentName { get; set; }

        [XmlElement(ElementName = "ScheduledDeliveryDate")]
        public DateTime ScheduledDeliveryDate { get; set; }
    }

    [XmlRoot(ElementName = "LabelSequence")]
    public class LabelSequence
    {
        [XmlElement(ElementName = "PackageNumber")]
        public int PackageNumber { get; set; }

        [XmlElement(ElementName = "TotalPackages")]
        public int TotalPackages { get; set; }
    }

    [XmlRoot(ElementName = "ImageParameters")]
    public class ImageParameters
    {
        [XmlElement(ElementName = "ImageParameter")]
        public LabelType LabelType { get; set; }

        [XmlElement(ElementName = "XCoordinate")]
        public int XCoordinate { get; set; }

        [XmlElement(ElementName = "YCoordinate")]
        public int YCoordinate { get; set; }

        [XmlElement(ElementName = "LabelSequence")]
        public LabelSequence LabelSequence { get; set; }
    }

    [XmlRoot(ElementName = "ExpressMailOptions")]
    public class ExpressMailOptions
    {
        [XmlElement(ElementName = "DeliveryOption")]
        public DeliveryOption DeliveryOption { get; set; }

        [XmlElement(ElementName = "WaiverOfSignature")]
        public bool WaiverOfSignature { get; set; }
    }

    [Serializable]
    [XmlRoot(ElementName = "eVSRequest")]
    public class eVSRequest
    {
        [XmlIgnore]
        public string UserId { get; set; }

        [XmlAttribute(AttributeName = "USERID")]
        public string USPSUSERID { get; set; }

        [XmlElement(ElementName = "Option")]
        public string Option { get; set; }

        [XmlElement(ElementName = "Revision")]
        public string Revision { get; set; }

        [XmlElement(ElementName = "ImageParameters")]
        public ImageParameters ImageParameters { get; set; }

        [XmlElement(ElementName = "FromName")]
        public string FromName { get; set; }

        [XmlElement(ElementName = "FromFirm")]
        public string FromFirm { get; set; }

        [XmlElement(ElementName = "FromAddress1")]
        public string FromAddress1 { get; set; }

        [XmlElement(ElementName = "FromAddress2")]
        public string FromAddress2 { get; set; }

        [XmlElement(ElementName = "FromCity")]
        public string FromCity { get; set; }

        [XmlElement(ElementName = "FromState")]
        public string FromState { get; set; }

        [XmlElement(ElementName = "FromZip5")]
        public string FromZip5 { get; set; }

        [XmlElement(ElementName = "FromZip4")]
        public string FromZip4 { get; set; }

        [XmlElement(ElementName = "FromPhone")]
        public string FromPhone { get; set; }

        [XmlElement(ElementName = "POZipCode")]
        public string POZipCode { get; set; }

        [XmlElement(ElementName = "AllowNonCleansedOriginAddr")]
        public bool AllowNonCleansedOriginAddr { get; set; }

        [XmlElement(ElementName = "ToName")]
        public string ToName { get; set; }

        [XmlElement(ElementName = "ToFirm")]
        public string ToFirm { get; set; }

        [XmlElement(ElementName = "ToAddress1")]
        public string ToAddress1 { get; set; }

        [XmlElement(ElementName = "ToAddress2")]
        public string ToAddress2 { get; set; }

        [XmlElement(ElementName = "ToCity")]
        public string ToCity { get; set; }

        [XmlElement(ElementName = "ToState")]
        public string ToState { get; set; }

        [XmlElement(ElementName = "ToZip5")]
        public string ToZip5 { get; set; }

        [XmlElement(ElementName = "ToZip4")]
        public string ToZip4 { get; set; }

        [XmlElement(ElementName = "ToPhone")]
        public string ToPhone { get; set; }

        [XmlElement(ElementName = "POBox")]
        public bool POBox { get; set; }

        [XmlElement(ElementName = "AllowNonCleansedDestAddr")]
        public bool AllowNonCleansedDestAddr { get; set; }

        [XmlElement(ElementName = "WeightInOunces")]
        public string WeightInOunces { get; set; }

        [XmlElement(ElementName = "ServiceType")]
        public ServiceType ServiceType { get; set; }

        [XmlElement(ElementName = "Container")]
        public Container Container { get; set; }

        [XmlElement(ElementName = "Width")]
        public decimal Width { get; set; }

        [XmlElement(ElementName = "Length")]
        public decimal Length { get; set; }

        [XmlElement(ElementName = "Height")]
        public decimal Height { get; set; }

        [XmlElement(ElementName = "Machinable")]
        public Machinable Machinable { get; set; }

        [XmlElement(ElementName = "ProcessingCategory")]
        public string ProcessingCategory { get; set; }

        [XmlElement(ElementName = "PriceOptions")]
        public PriceOption PriceOptions { get; set; }

        [XmlElement(ElementName = "InsuredAmount")]
        public decimal InsuredAmount { get; set; }

        [XmlElement(ElementName = "AddressServiceRequested")]
        public bool AddressServiceRequested { get; set; }

        [XmlElement(ElementName = "ExpressMailOptions")]
        public ExpressMailOptions ExpressMailOptions { get; set; }

        [XmlElement(ElementName = "ShipDate")]
        public string ShipDate { get; set; }

        [XmlElement(ElementName = "CustomerRefNo")]
        public string CustomerRefNo { get; set; }

        [XmlArray(ElementName = "ExtraServices")]
        public List<ExtraService> ExtraServices { get; set; }

        [XmlElement(ElementName = "PermitNumber")]
        public string PermitNumber { get; set; }

        [XmlElement(ElementName = "PermitZIPCode")]
        public string PermitZIPCode { get; set; }

        [XmlElement(ElementName = "PermitHolderName")]
        public string PermitHolderName { get; set; }

        [XmlElement(ElementName = "CRID")]
        public string CRID { get; set; }

        [XmlElement(ElementName = "MID")]
        public string MID { get; set; }

        [XmlElement(ElementName = "SenderName")]
        public string SenderName { get; set; }

        [XmlElement(ElementName = "SenderEMail")]
        public string SenderEMail { get; set; }

        [XmlElement(ElementName = "RecipientName")]
        public string RecipientName { get; set; }

        [XmlElement(ElementName = "RecipientEMail")]
        public string RecipientEMail { get; set; }

        [XmlElement(ElementName = "ReceiptOption")]
        public ReceiptOption ReceiptOption { get; set; }

        [XmlElement(ElementName = "ImageType")]
        public ImageType ImageType { get; set; }

        [XmlElement(ElementName = "HoldForManifest")]
        public HoldForManifest HoldForManifest { get; set; }

        [XmlElement(ElementName = "NineDigitRoutingZip")]
        public bool NineDigitRoutingZip { get; set; }

        [XmlElement(ElementName = "ShipInfo")]
        public bool ShipInfo { get; set; }
    }

    [XmlRoot(ElementName = "ExtraServices")]
    public class ExtraServices
    {
        [XmlElement(ElementName = "ServiceID")]
        public ExtraService ServiceID { get; set; }

        [XmlElement(ElementName = "ServiceName")]
        public string ServiceName { get; set; }

        [XmlElement(ElementName = "Price")]
        public decimal Price { get; set; }
    }

    [XmlRoot(ElementName = "Commitment")]
    public class Commitment
    {
        [XmlElement(ElementName = "CommitmentName")]
        public string CommitmentName { get; set; }

        [XmlElement(ElementName = "ScheduledDeliveryDate")]
        public string ScheduledDeliveryDate { get; set; }
    }

    [Serializable]
    [XmlRoot(ElementName = "eVSResponse")]
    public class eVSResponse
    {
        [XmlElement(ElementName = "BarcodeNumber")]
        public string BarcodeNumber { get; set; }

        [XmlElement(ElementName = "LabelImage")]
        public string LabelImageString { get; set; }

        [XmlElement(ElementName = "ReceiptImage")]
        public string ReceiptImageString { get; set; }

        [XmlElement(ElementName = "ToName")]
        public string ToName { get; set; }

        [XmlElement(ElementName = "ToFirm")]
        public string ToFirm { get; set; }

        [XmlElement(ElementName = "ToAddress1")]
        public string ToAddress1 { get; set; }

        [XmlElement(ElementName = "ToAddress2")]
        public string ToAddress2 { get; set; }

        [XmlElement(ElementName = "ToCity")]
        public string ToCity { get; set; }

        [XmlElement(ElementName = "ToState")]
        public string ToState { get; set; }

        [XmlElement(ElementName = "ToZip5")]
        public string ToZip5 { get; set; }

        [XmlElement(ElementName = "ToZip4")]
        public string ToZip4 { get; set; }

        [XmlElement(ElementName = "Postnet")]
        public string Postnet { get; set; }

        [XmlElement(ElementName = "RDC")]
        public string RDC { get; set; }

        [XmlElement(ElementName = "Postage")]
        public decimal Postage { get; set; }

        [XmlElement(ElementName = "Extraservices")]
        public List<ExtraServices> Extraservices { get; set; }

        [XmlElement(ElementName = "Zone")]
        public string Zone { get; set; }

        [XmlElement(ElementName = "DimensionalWeight")]
        public string DimensionalWeight { get; set; }

        [XmlElement(ElementName = "CarrierRoute")]
        public string CarrierRoute { get; set; }

        [XmlElement(ElementName = "PermitHolderName")]
        public string PermitHolderName { get; set; }

        [XmlElement(ElementName = "InductionType")]
        public string InductionType { get; set; }

        [XmlElement(ElementName = "LogMessage")]
        public string LogMessage { get; set; }

        [XmlElement(ElementName = "Commitment")]
        public Commitment Commitment { get; set; }

        [XmlElement(ElementName = "Error")]
        public USPSError Error { get; set; }

        [XmlIgnore]
        public bool HasError
        {
            get
            {
                if (this.Error == null) return false;
                return !(String.IsNullOrWhiteSpace(this.Error.Number) &&
                         String.IsNullOrWhiteSpace(this.Error.Description));
            }
            set { }
        }
    }
}
