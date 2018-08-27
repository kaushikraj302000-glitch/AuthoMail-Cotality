using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AM.USPSLib;

namespace AM.Services
{
    [Serializable]
    [XmlRoot(ElementName = "eVSRequest")]
    public class eVSRequest : IeVSRequest
    {
        [XmlAttribute(AttributeName = "USERID")]
        public string USERID { get; set; }

        [XmlAttribute(AttributeName = "PASSWORD")]
        public string PASSWORD { get; set; }

        [XmlElement(ElementName = "ImageParameters")]
        public LabelParameters LabelParameters { get; set; }

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

        [XmlElement(ElementName = "ToContactPreference")]
        public ToContactPreference ToContactPreference { get; set; }

        [XmlElement(ElementName = "ToContactMessaging")]
        public string ToContactMessaging { get; set; }

        [XmlElement(ElementName = "ToContactEmail")]
        public string ToContactEmail { get; set; }

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

        [XmlElement(ElementName = "CustomerRefNo2")]
        public string CustomerRefNo2 { get; set; }

        [XmlArray(ElementName = "ExtraServices")]
        public List<ExtraService> ExtraServices { get; set; }

        [XmlElement(ElementName = "CRID")]
        public string CRID { get; set; }

        [XmlElement(ElementName = "MID")]
        public string MID { get; set; }

        [XmlElement(ElementName = "LogisticsManagerMID")]
        public string LogisticsManagerMID { get; set; }

        [XmlElement(ElementName = "VendorCode")]
        public string VendorCode { get; set; }

        [XmlElement(ElementName = "VendorProductVersionNumber")]
        public string VendorProductVersionNumber { get; set; }

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

        [XmlElement(ElementName = "CarrierRelease")]
        public bool CarrierRelease { get; set; }

        [XmlElement(ElementName = "DropOffTime")]
        public bool DropOffTime { get; set; }

        [XmlElement(ElementName = "ReturnCommitments")]
        public bool ReturnCommitments { get; set; }

        [XmlElement(ElementName = "PrintCustomerRefNo")]
        public bool PrintCustomerRefNo { get; set; }

        [XmlElement(ElementName = "Content")]
        public Content Content { get; set; }

        [XmlElement(ElementName = "Content")]
        public ActionCode ActionCode { get; set; }

        [XmlElement(ElementName = "OptOutOfSPE")]
        public bool OptOutOfSPE { get; set; }

        [XmlElement(ElementName = "SortationLevel")]
        public string SortationLevel { get; set; }

        [XmlElement(ElementName = "DestinationEntryFacilityType")]
        public DestinationEntryFacilityType DestinationEntryFacilityType { get; set; }

        [XmlElement(ElementName = "ShippingContents")]
        public List<ItemDetail> ShippingContents { get; set; }
    }
}
