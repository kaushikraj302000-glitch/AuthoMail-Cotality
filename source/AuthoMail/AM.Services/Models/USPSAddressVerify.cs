using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AM.Lib;

namespace AM.Services
{
    public class VerifierAddressRequest
    {
        public string UserId { get; set; }
        public List<AddressRequest> AddressRequests { get; set; }
    }

    public class VerifierAddressResponse
    {
        public List<AddressResponse> AddressResponses { get; private set; }
        public string Error { get; set; }
        public bool HasError { get { return !this.Error.IsNullOrWhiteSpace(); } }

        public VerifierAddressResponse(List<AddressResponse> responseList)
        {
            this.AddressResponses = responseList;
        }
    }

    public class AddressRequest
    {
        public string Id { get; set; }

        public string FirmName { get; set; }

        /// <summary>
        /// Address Line 1 is used to provide an apartment or suite number, if applicable
        /// </summary>
        public string Line1 { get; set; }

        /// <summary>
        /// Street address
        /// </summary>
        public string Line2 { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        /// <summary>
        /// For Puerto Rico addresses only.
        /// </summary>
        public string Urbanization { get; set; }

        public string Zip5 { get; set; }

        public string Zip4 { get; set; }

        public AddressRequest()
        {
            // Initialize all string properties to empty string. Required to have them serialized.
            foreach (var propInfo in typeof(AddressRequest).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (propInfo.PropertyType == typeof(string) && propInfo.CanWrite)
                {
                    propInfo.SetValue(this, string.Empty);
                }
            }
        }
    }

    public class AddressResponse
    {
        public string Id { get; set; }
        public string FirmName { get; set; }

        public string Line1 { get; set; }

        public string Line2 { get; set; }

        public string Line1Abbr { get; set; }

        public string City { get; set; }

        public string CityAbbr { get; set; }

        public string State { get; set; }

        public string Urbanization { get; set; }

        public string Zip5 { get; set; }

        public string Zip4 { get; set; }

        public string Zip { get { return string.Join("-", new string[] { Zip5, Zip4 }.Where(s => !s.IsNullOrWhiteSpace())); } }

        public string DeliveryPoint { get; set; }

        public string CarrierRoute { get; set; }

        public string ReturnText { get; set; }

        public Error Error { get; set; }

        //public bool IsAddressVerified { get; set; }

        public List<string> AddressTypes { get; set; }

        //public string StateType { get; set; }

        public string Verifier { get; set; }

        public string VerifierDesc { get; set; }

        public bool HasError
        {
            get
            {
                if (this.Error == null) return false;
                return !(String.IsNullOrWhiteSpace(this.Error.Number) &&
                         String.IsNullOrWhiteSpace(this.Error.Description));
            }
        }

        [XmlIgnore]
        public AddressRequest AddressRequest { get; private set; }

        public AddressResponse(AddressRequest request, int order)
        {
            this.AddressRequest = request;
            this.Id = "{0}{1}{2}".Fill(order, ResponseSource.CLSVC, request == null ? "" : request.Id);
        }
        private AddressResponse()
        {

        }
    }

    [DataContract]
    [Serializable]
    public class Error
    {
        [DataMember]
        [XmlElement(ElementName = "Number")]
        public string Number { get; set; }

        [DataMember]
        [XmlElement(ElementName = "Source")]
        public string Source { get; set; }

        [DataMember]
        [XmlElement(ElementName = "Description")]
        public string Description { get; set; }

        [DataMember]
        [XmlElement(ElementName = "HelpFile")]
        public string HelpFile { get; set; }

        [DataMember]
        [XmlElement(ElementName = "HelpContext")]
        public string HelpContext { get; set; }
    }

    [DataContract]
    [Serializable]
    public class Address
    {
        [DataMember]
        [XmlAttribute(AttributeName = "ID")]
        public string ID { get; set; }

        [XmlElement(ElementName = "FirmName")]
        public string FirmName { get; set; }

        [DataMember]
        [XmlElement(ElementName = "Address1")]
        public string Address1 { get; set; }

        [DataMember]
        [XmlElement(ElementName = "Address2")]
        public string Address2 { get; set; }

        [DataMember]
        [XmlElement(ElementName = "AddressLine2Abbreviation")]
        public string AddressLine2Abbreviation { get; set; }

        [DataMember]
        [XmlElement(ElementName = "City")]
        public string City { get; set; }

        [DataMember]
        [XmlElement(ElementName = "CityAbbreviation")]
        public string CityAbbreviation { get; set; }

        [DataMember]
        [XmlElement(ElementName = "State")]
        public string State { get; set; }

        [DataMember]
        [XmlElement(ElementName = "Urbanization")]
        public string Urbanization { get; set; }

        [DataMember]
        [XmlElement(ElementName = "Zip5")]
        public string Zip5 { get; set; }

        [DataMember]
        [XmlElement(ElementName = "Zip4")]
        public string Zip4 { get; set; }

        [DataMember]
        [XmlElement(ElementName = "DeliveryPoint")]
        public string DeliveryPoint { get; set; }

        [DataMember]
        [XmlElement(ElementName = "CarrierRoute")]
        public string CarrierRoute { get; set; }

        [XmlIgnore]
        [DataMember]
        [XmlElement(ElementName = "ReturnText")]
        public string ReturnText { get; set; }

        [DataMember]
        [XmlElement(ElementName = "Error")]
        public Error Error { get; set; }

        [DataMember]
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

        private string _oldAddress;
        [DataMember]
        public string FullAddress
        {
            get
            {
                return String.IsNullOrWhiteSpace(_oldAddress) ?
                       String.Join(", ", new[] { this.Address1, this.Address2, this.City, this.State, string.Format("{0}-{1}", this.Zip5, this.Zip4).Trim('-') }.Where(s => !String.IsNullOrWhiteSpace(s))).Trim()
                       : _oldAddress;
            }
            set { _oldAddress = value; }
        }

        public Address()
        {
            // Initialize all string properties to empty string. Required to have them serialized.
            foreach (var propInfo in typeof(Address).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (propInfo.PropertyType == typeof(string) && propInfo.CanWrite)
                {
                    propInfo.SetValue(this, string.Empty);
                }
            }
        }

        //Ignore all extra properties
        public bool ShouldSerializeAddressLine2Abbreviation() { return false; }
        public bool ShouldSerializeCityAbbreviation() { return false; }
        public bool ShouldSerializeDeliveryPoint() { return false; }
        public bool ShouldSerializeCarrierRoute() { return false; }
        public bool ShouldSerializeError() { return false; }
        public bool ShouldSerializeHasError() { return false; }
        public bool ShouldSerializeFullAddress() { return false; }
    }

    [Serializable]
    [XmlRoot(ElementName = "AddressValidateRequest")]
    public class AddressValidateRequest
    {
        [XmlAttribute(AttributeName = "USERID")]
        public string USERID { get; set; }

        [XmlElement(ElementName = "Revision")]
        public string Revision { get; set; }

        [XmlElement(ElementName = "IncludeOptionalElements")]
        public bool IncludeOptionalElements { get; set; }

        [XmlElement(ElementName = "ReturnCarrierRoute")]
        public bool ReturnCarrierRoute { get; set; }

        [XmlElement(ElementName = "Address")]
        public List<Address> AddressList { get; set; }

        public AddressValidateRequest(string userid, string revision)
        {
            this.USERID = userid; Revision = revision;
            this.IncludeOptionalElements = true;
            this.ReturnCarrierRoute = true;
            this.AddressList = new List<Address>();
        }

        public AddressValidateRequest()
        {
            this.USERID = "819MILES3278"; Revision = "1";
            this.IncludeOptionalElements = true;
            this.ReturnCarrierRoute = true;
            this.AddressList = new List<Address>();
        }
    }

    [Serializable]
    [XmlRoot(ElementName = "AddressValidateResponse")]
    public class AddressValidateResponse
    {
        [XmlElement(ElementName = "Address")]
        public List<Address> AddressList { get; set; }
        public bool IsError { get { return AddressList.Any(s => s.HasError); } }
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
