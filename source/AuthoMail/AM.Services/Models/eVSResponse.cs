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
    [XmlRoot(ElementName = "eVSResponse")]
    public class eVSResponse : IeVSResponse
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
        public List<ExtraService> Extraservices { get; set; }

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
