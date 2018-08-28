using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;
using System.Xml.Serialization;

namespace AM.USPSLib
{
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

        public string Warning { get; set; }

        public USPSError Error { get; set; }

        //public bool IsAddressVerified { get; set; }

        public List<AddressType> AddressTypes { get; set; }

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
}
