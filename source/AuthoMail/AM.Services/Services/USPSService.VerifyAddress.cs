using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;
using System.Net;
using Dapper;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace AM.Services
{
    public partial class USPSService : ServiceBase, IUSPSService
    {
        public AddressResponse VerifyAddress(string userId, AddressRequest addressRequest)
        {
            return this.VerifyAddress(userId, new List<AddressRequest>() { addressRequest }).Single();
        }

        public List<AddressResponse> VerifyAddress(string userId, List<AddressRequest> requests)
        {
            return VerifyAddressAsync(userId, requests).Result;
        }

        private IEnumerable<string> ValidateAddress(AddressRequest address)
        {
            // Validate if address is null
            if (address == null)
            {
                yield return "Address cannot be NULL.";
            }

            // Validate address line
            if ("{0} {1}".Fill(address.Line1, address.Line2).IsNullOrWhiteSpace())
            {
                yield return "Either address line 1 or line 2 must be specified.";
            }

            if (address.Zip5.IsNullOrWhiteSpace() && (address.City.IsNullOrWhiteSpace() || address.State.IsNullOrWhiteSpace()))
            {
                yield return "The address MUST contain either one of the following: 1. ZIP or 2. City and State.";
            }
        }

        private async Task<List<AddressResponse>> VerifyAddressAsync(string userId, List<AddressRequest> addressRequests)
        {
            try
            {

                int insertedID = 0;
                Task<int> recordRequestId = null;
                //check flag and call to db
                if (Config.Web_API_USPS_IsDbLoggingEnabled)
                {
                    var addressRequestsXML = addressRequests.Serialize();

                    recordRequestId = db.ExecuteScalarAsync<int>("Insert into [API].[USPS] ([APIName], [UserId], [APIRequest], [APIRequestedOn]) OUTPUT INSERTED.Id VALUES(@APIName, @UserId, @APIRequest, @APIRequestDateTime);", new { APIName = Config.Web_API_USPS_VerifyAPIName, UserId = userId, APIRequest = addressRequestsXML, APIRequestDateTime = DateTime.Now });
                }

                var maxAddressCount = Config.Web_API_USPS_AddressCount;

                // Validate list length
                if (addressRequests == null || addressRequests.Count < 1 || addressRequests.Count > maxAddressCount)
                    throw new ArgumentException("addressRequests", "At least one and at most 5 addresses must be specified.");

                List<AddressResponse> addressResponses = new List<AddressResponse>(maxAddressCount);
                List<Address> addresses = new List<Address>(maxAddressCount);

                for (int i = 0; i < addressRequests.Count; i++)
                {
                    var request = addressRequests[i];
                    var validationErrors = ValidateAddress(request);

                    var response = new AddressResponse(request, i + 1);

                    if (validationErrors.Any())
                    {
                        response.Error = new Error
                        {
                            Source = ResponseSource.CLSVC,
                            Description = String.Join(Environment.NewLine, validationErrors)
                        };
                    }
                    else
                    {
                        var add2 = "{0} {1}".Fill(request.Line1, request.Line2).Trim();

                        addresses.Add(new Address
                        {
                            //index 1 base
                            ID = response.Id,
                            FirmName = request.FirmName,
                            Address2 = add2.IsNullOrWhiteSpace() ? string.Empty : add2,
                            City = request.City.IsNullOrWhiteSpace() ? string.Empty : request.City,
                            State = request.State.IsNullOrWhiteSpace() ? string.Empty : request.State,
                            Zip5 = request.Zip5.IsNullOrWhiteSpace() ? string.Empty : request.Zip5,
                            Zip4 = request.Zip4.IsNullOrWhiteSpace() ? string.Empty : request.Zip4
                        });
                    }
                    addressResponses.Add(response);
                }

                FormUrlEncodedContent payload = null;
                Task<string> uspsReq = null;
                var idRegex = new Regex(@"(\d+{0})(.*)".Fill(ResponseSource.CLSVC));

                try
                {
                    using (HttpClient httpClient = new HttpClient())
                    {
                        var request = new AddressValidateRequest(Config.Web_API_USPS_UserId, "1");
                        //var request = new AddressValidateRequest("719MSTC04390", "1");//"719MSTC04390", "1");
                        request.AddressList.AddRange(addresses);

                        string url = Config.Web_API_USPS_URL;
                        //string url = @"https://secure.shippingapis.com/ShippingAPI.dll";//["USPSAPI"];

                        var uspsInpurXML = request.Serialize();


                        if (Config.Web_API_USPS_IsDbLoggingEnabled)
                        {
                            insertedID = await recordRequestId;
                            var uspsRequestLog = db.ExecuteAsync("Update [API].[USPS] set USPSRequest = @USPSRequest ,USPSRequestedOn = @USPSRequestedOn where Id = @Id", new { USPSRequest = uspsInpurXML, USPSRequestedOn = DateTime.Now, Id = insertedID });
                        }

                        payload = new FormUrlEncodedContent(new[] 
                        {
                            new KeyValuePair<string, string>("API",Config.Web_API_USPS_VerifyAPIName),
                            //new KeyValuePair<string, string>("API","Verify"),
                            new KeyValuePair<string, string>("XML",uspsInpurXML)
                        });

                        var payloadResult = await payload.ReadAsStringAsync();
                        uspsReq = httpClient.GetStringAsync("{0}?{1}".Fill(url, payloadResult));

                        var uspsResponseString = await uspsReq;
                        if (Config.Web_API_USPS_IsDbLoggingEnabled)
                        {
                            var uspsResponseLog = db.ExecuteAsync("Update [API].[USPS] set USPSResponse = @USPSResponse ,  USPSResponseOn = @USPSResponseOn where Id = @Id", new { USPSResponse = uspsResponseString, USPSResponseOn = DateTime.Now, Id = insertedID });
                        }
                        if (uspsResponseString.Contains("AddressValidateResponse"))
                        {
                            var response = uspsResponseString.Deserialize<AddressValidateResponse>();

                            SetResponseValues(addressResponses, idRegex, response.AddressList);
                        }
                        else
                        {
                            var error = uspsResponseString.Deserialize<Error>();
                            //var verifiedaddress = new address();
                            //verifiedAddress.Error = error;
                            //to do when this called
                            throw new InvalidOperationException(error.Description);
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    SetResponseValues(addressResponses, idRegex, addresses, ex);
                }



                if (Config.Web_API_USPS_IsDbLoggingEnabled)
                {
                    var addressResponseXML = addressResponses.Serialize();
                    var responseLog = db.ExecuteAsync("Update [API].[USPS] set APIResponse = @APIResponse ,  APIResponseOn = @APIResponseOn Where Id = @Id", new { APIResponse = addressResponseXML, APIResponseOn = DateTime.Now, Id = insertedID });
                    await responseLog;
                }


                return await Task.Run(() => addressResponses);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void SetResponseValues(List<AddressResponse> addressResponses, Regex idRegex, List<Address> addresses)
        {
            SetResponseValues(addressResponses, idRegex, addresses, null);
        }

        private static void SetResponseValues(List<AddressResponse> addressResponses, Regex idRegex, List<Address> addresses, Exception ex)
        {
            foreach (Address address in addresses)
            {
                var verifiedAddress = address;

                var responseAdd = addressResponses.Single(res => res.Id == verifiedAddress.ID);

                //if error by USPS then assign requested add to respond with source CLSVC
                if (verifiedAddress.HasError)
                {
                    verifiedAddress.FirmName = responseAdd.AddressRequest.FirmName;
                    verifiedAddress.Address1 = responseAdd.AddressRequest.Line1;
                    verifiedAddress.Address2 = responseAdd.AddressRequest.Line2;
                    verifiedAddress.City = responseAdd.AddressRequest.City;
                    verifiedAddress.State = responseAdd.AddressRequest.State;
                    verifiedAddress.Zip5 = responseAdd.AddressRequest.Zip5;
                    verifiedAddress.Zip4 = responseAdd.AddressRequest.Zip4;
                    verifiedAddress.Urbanization = responseAdd.AddressRequest.Urbanization;
                }

                responseAdd.Id = idRegex.Match(responseAdd.Id).Groups[2].Value;
                responseAdd.FirmName = verifiedAddress.FirmName;
                responseAdd.Line1 = verifiedAddress.Address2;
                //responseAdd.Line2 = verifiedAddress.Address2;
                responseAdd.Line1Abbr = verifiedAddress.AddressLine2Abbreviation;
                responseAdd.City = verifiedAddress.City;
                responseAdd.CityAbbr = verifiedAddress.CityAbbreviation;
                responseAdd.State = verifiedAddress.State;
                responseAdd.Zip5 = verifiedAddress.Zip5;
                responseAdd.Zip4 = verifiedAddress.Zip4;
                responseAdd.Urbanization = verifiedAddress.Urbanization;
                responseAdd.CarrierRoute = verifiedAddress.CarrierRoute;
                responseAdd.DeliveryPoint = verifiedAddress.DeliveryPoint;
                responseAdd.ReturnText = verifiedAddress.ReturnText;
                responseAdd.Error = verifiedAddress.Error;
                responseAdd.AddressTypes = CheckAddressType(verifiedAddress);

                if (!verifiedAddress.HasError)
                {
                    responseAdd.Verifier = ResponseSource.USPS;
                }
                else
                {
                    responseAdd.Verifier = ResponseSource.CLSVC;
                    responseAdd.VerifierDesc = "USPS Failure : {0}".Fill(ex != null ? ex.ToString() : verifiedAddress.Error.Description);
                }
            }
        }

        private static List<string> CheckAddressType(Address address)
        {
            var addressTypes = new List<string>();
            var militarystates = Config.Web_API_USPS_MilitaryStates.Split(',');
            //var militarystates = new string[] { "AA", "AE", "AP" };
            var militaryCities = Config.Web_API_USPS_MilitaryCities.Split(',');
            //var militaryCities = new string[] { "APO", "DPO", "FPO" };
            var territory = Config.Web_API_USPS_Territory.Split(',');
            //var territory = new string[] { "AS", "FM", "GU", "MH", "MP", "PR", "PW", "VI" };

            if (address.Address2.ToLower().Contains("po box") || address.Address2.ToLower().Contains("pobox"))
                addressTypes.Add(AddressTypes.POBOX);

            if (militarystates.Contains(address.State) && militaryCities.Contains(address.City))
                addressTypes.Add(AddressTypes.Military);
            else
            {
                if (territory.Contains(address.State))
                    addressTypes.Add(AddressTypes.Territory);
                else
                    addressTypes.Add(AddressTypes.DHAM);
            }

            return addressTypes;
        }
    }
}
