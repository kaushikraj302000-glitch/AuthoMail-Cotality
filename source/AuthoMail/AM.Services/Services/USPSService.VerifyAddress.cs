using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;
using System.Net;
using Dapper;
using System.Net.Http;

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
            var addressRequestsXML = addressRequests.Serialize();
            //int insertedID = 0;
            //check flag and call to db
            //if (Config.USPS_IsDbLoggingEnabled)
            //var recordRequestId = db.ExecuteScalar<int>("Insert into [API].[USPS] (API,UserID,APIRequest,APIRequestDateTime) OUTPUT INSERTED.Id VALUES(@API, @UserID, @APIRequest, @APIRequestDateTime);", new { API = "Verify", UserID = "2", APIRequest = "213<xml>", APIRequestDateTime = DateTime.Now });
            {
                //TODO take it from db
                var maxAddressCount = 5; //Config.USPS_AddressCount;

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
                        addresses.Add(new Address
                        {
                            //index 1 base
                            ID = response.Id,
                            FirmName = request.FirmName,
                            Address2 = "{0} {1}".Fill(request.Line1, request.Line2).Trim(),
                            City = request.City,
                            State = request.State,
                            Zip5 = request.Zip5,
                            Zip4 = request.Zip4
                        });
                    }
                    addressResponses.Add(response);
                }

                List<string> addressTypes = new List<string>();
                FormUrlEncodedContent payload = null;
                Task<string> uspsReq = null;

                try
                {
                    using (HttpClient httpClient = new HttpClient())
                    {
                        //Todo take userid,apilink,apiname take from db
                        //var request = new AddressValidateRequest(Config.USPS_UserId, "1");
                        var request = new AddressValidateRequest("719MSTC04390", "1");//"719MSTC04390", "1");
                        request.AddressList.AddRange(addresses);

                        //string url = Config.USPS_URL;
                        string url = @"https://secure.shippingapis.com/ShippingAPI.dll";//["USPSAPI"];

                        //TODO Record this input xml to db
                        var uspsInpurXML = request.Serialize();

                        //var uspsRequest = db.ExecuteAsync("update [API].[USPS] set USPSRequest = @USPSRequest ,USPSRequestDateTime = @USPSRequestDateTime where Id = @Id", new { USPSRequest = uspsInpurXML, USPSRequestDateTime = DateTime.Now, Id = recordRequestId });

                        payload = new FormUrlEncodedContent(new[] 
                        {
                            //new KeyValuePair<string, string>("API",Config.USPS_VerifyAPIName),
                            new KeyValuePair<string, string>("API","Verify"),
                            new KeyValuePair<string, string>("XML",uspsInpurXML)
                        });

                        var payloadResult = await payload.ReadAsStringAsync();
                        uspsReq = httpClient.GetStringAsync("{0}?{1}".Fill(url, payloadResult));

                        //TODO Record this output xml to db
                        var uspsResponseString = await uspsReq;

                        Address verifiedAddress = new Address();

                        if (uspsResponseString.Contains("AddressValidateResponse"))
                        {
                            var response = uspsResponseString.Deserialize<AddressValidateResponse>();

                            for (int i = 0; i < response.AddressList.Count; i++)
                            {
                                verifiedAddress = response.AddressList[i];
                                var order = i;
                                addressTypes.Clear();

                                CheckAddressType(verifiedAddress, ref addressTypes);//, ref stateType);

                                var responseAdd = addressResponses.Single(id => id.Id == verifiedAddress.ID);

                                responseAdd.Id = responseAdd.Id.Replace("{0}{1}".Fill(order + 1, ResponseSource.CLSVC), "");
                                responseAdd.FirmName = verifiedAddress.FirmName;
                                responseAdd.Line1 = verifiedAddress.Address1;
                                responseAdd.Line2 = verifiedAddress.Address2;
                                responseAdd.Line2Abbr = verifiedAddress.AddressLine2Abbreviation;
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
                                //responseAdd.IsAddressVerified = !verifiedAddress.HasError;
                                responseAdd.AddressTypes = addressTypes;
                                responseAdd.Verifier = ResponseSource.USPS;
                                responseAdd.VerifierDesc = verifiedAddress.HasError ? verifiedAddress.Error.Description : string.Empty;

                                //index 1 base
                                //CreateAddressResponse(verifiedAddress, addressRequests[int.Parse(verifiedAddress.ID)], addressTypes, source, sourceDesc);
                            }
                        }
                        else
                        {
                            var error = uspsResponseString.Deserialize<Error>();
                            verifiedAddress = new Address();
                            verifiedAddress.Error = error;
                            //to do when this called
                            throw new Exception(error.Description);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //fall back by USPS
                    foreach (Address verifiedAddress in addresses)
                    {

                        addressTypes.Clear();
                        CheckAddressType(verifiedAddress, ref addressTypes);

                        var responseAdd = addressResponses.Single(i => i.Id == verifiedAddress.ID);

                        responseAdd.FirmName = verifiedAddress.FirmName;
                        responseAdd.Line1 = verifiedAddress.Address1;
                        responseAdd.Line2 = verifiedAddress.Address2;
                        responseAdd.Line2Abbr = verifiedAddress.AddressLine2Abbreviation;
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
                        //responseAdd.IsAddressVerified = !verifiedAddress.HasError;
                        responseAdd.AddressTypes = addressTypes;
                        //responseAdd.StateType = stateType;
                        responseAdd.Verifier = ResponseSource.CLSVC;
                        responseAdd.VerifierDesc = "USPS Failure : {0}".Fill(ex.ToString());

                        //addressResponses.Add(CreateAddressResponse(add, addressRequests[int.Parse(add.ID)], addressTypes, source, sourceDesc));
                    }
                }
                finally
                {
                    uspsReq.Dispose();
                    payload.Dispose();
                }

                //TODO record this response xml to db
                //var addressResponseXML = addressResponses.Serialize();

                return await Task.Run(() => addressResponses);
            }
        }

        private static void CheckAddressType(Address address, ref List<string> addressType)//, ref string stateType)
        {
            //var militarystates = Config.USPS_MilitaryStates.Split(',');
            var militarystates = new string[] { "AA", "AE", "AP" };
            //var militaryCities = Config.USPS_MilitaryCities.Split(',');
            var militaryCities = new string[] { "APO", "DPO", "FPO" };
            //var territory = Config.USPS_Territory.Split(',');
            var territory = new string[] { "AS", "FM", "GU", "MH", "MP", "PR", "PW", "VI" };

            if (address.Address2.ToLower().Contains("po box") || address.Address2.ToLower().Contains("pobox"))
                addressType.Add(AddressTypes.POBOX);

            if (militarystates.Contains(address.State) && militaryCities.Contains(address.City))
                addressType.Add(AddressTypes.Military);
            else
            {
                if (territory.Contains(address.State))
                    addressType.Add(AddressTypes.Territory);
                else
                    addressType.Add(AddressTypes.DHAM);
            }
        }

        //private AddressResponse CreateAddressResponse(Address verifiedAddress, AddressRequest addressrequest, List<string> addressTypes, string source, string sourceDesc)
        //{


        //    return new AddressResponse(addressrequest)
        //    {
        //        FirmName = verifiedAddress.FirmName,
        //        AddressLine1 = verifiedAddress.Address1,
        //        AddressLine2 = verifiedAddress.Address2,
        //        AddressLine2Abbreviation = verifiedAddress.AddressLine2Abbreviation,
        //        City = verifiedAddress.City,
        //        CityAbbreviation = verifiedAddress.CityAbbreviation,
        //        State = verifiedAddress.State,
        //        Zip5 = verifiedAddress.Zip5,
        //        Zip4 = verifiedAddress.Zip4,
        //        Urbanization = verifiedAddress.Urbanization,
        //        CarrierRoute = verifiedAddress.CarrierRoute,
        //        DeliveryPoint = verifiedAddress.DeliveryPoint,
        //        ReturnText = verifiedAddress.ReturnText,
        //        Error = verifiedAddress.Error,
        //        //IsAddressVerified = !verifiedAddress.HasError,
        //        AddressTypes = addressTypes,
        //        //StateType = stateType,
        //        Verifier = source,
        //        VerifierDesc = sourceDesc
        //    };
        //}
    }
}
