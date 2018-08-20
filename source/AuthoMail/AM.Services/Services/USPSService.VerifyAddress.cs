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
        public VerifierAddressResponse VerifyAddress(VerifierAddressRequest addressRequest)
        {
            return this.VerifyAddressAsync(addressRequest.UserId, addressRequest.AddressRequests).Result;
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

        private async Task<VerifierAddressResponse> VerifyAddressAsync(string userId, List<AddressRequest> addressRequests)
        {
            try
            {
                int logId = 0;

                Task<int> dbLog = null;
                var maxAddressCount = Config.Web_API_USPS_AddressCount;
                var isAPIError = false;
                string apiErrorMSG = null;

                //response field and api flag , Validate list length
                if (addressRequests == null || addressRequests.Count < 1 || addressRequests.Count > maxAddressCount)
                {
                    isAPIError = true;
                    apiErrorMSG = "In addressRequests At least one and at most 5 addresses must be specified.";
                }

                if (userId.IsNullOrWhiteSpace())
                {
                    isAPIError = true;
                    apiErrorMSG = string.Join("\r\n", new string[] { apiErrorMSG, "For addressRequests UserId must be specified." }.Where(msg => !msg.IsNullOrWhiteSpace()));
                }

                //check flag and call to db
                if (Config.Web_API_USPS_IsDbLoggingEnabled)
                {
                    var addressRequestsXML = addressRequests.Serialize();

                    dbLog = db.ExecuteScalarAsync<int>("Insert into [API].[USPS] ([APIName], [UserId], [APIRequest], [APIRequestedOn],[IsAPIError],[IsUSPSError],[APIResponse], [APIResponseOn]) OUTPUT INSERTED.Id VALUES(@APIName, @UserId, @APIRequest, @APIRequestedOn, @IsAPIError, @IsUSPSError, @APIResponse, @APIResponseOn);", new { APIName = Config.Web_API_USPS_VerifyAPIName, UserId = userId, APIRequest = addressRequestsXML, APIRequestedOn = Common.Now, IsAPIError = isAPIError, IsUSPSError = false, APIResponse = apiErrorMSG, APIResponseOn = isAPIError ? Common.Now : (DateTime?)null });
                }

                if (isAPIError)
                {
                    if (Config.Web_API_USPS_IsDbLoggingEnabled)
                    {
                        var dbLogawait = await dbLog;
                    }

                    throw new ArgumentException(apiErrorMSG);
                }

                List<AddressResponse> addressResponses = new List<AddressResponse>(maxAddressCount);
                List<Address> addresses = new List<Address>(maxAddressCount);

                for (int i = 0; i < addressRequests.Count; i++)
                {
                    var request = addressRequests[i];
                    var validationErrors = ValidateAddress(request);

                    //Id index 1 base
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
                            Address1 = string.Empty,
                            Address2 = add2,
                            City = request.City.IsNullOrWhiteSpace() ? string.Empty : request.City.Trim(),
                            State = request.State.IsNullOrWhiteSpace() ? string.Empty : request.State.Trim(),
                            Zip5 = request.Zip5.IsNullOrWhiteSpace() ? string.Empty : request.Zip5.Trim(),
                            Zip4 = request.Zip4.IsNullOrWhiteSpace() ? string.Empty : request.Zip4.Trim(),
                            Urbanization = request.Urbanization.IsNullOrWhiteSpace() ? string.Empty : request.Urbanization.Trim()
                        });
                    }

                    response.AddressTypes = new List<string>();

                    addressResponses.Add(response);
                }

                FormUrlEncodedContent payload = null;

                Task<string> uspsReq = null;

                using (HttpClient httpClient = new HttpClient())
                {
                    var request = new AddressValidateRequest(Config.Web_API_USPS_UserId, "1");
                    //var request = new AddressValidateRequest("719MSTC04390", "1");//"719MSTC04390", "1");
                    request.AddressList.AddRange(addresses);

                    string url = Config.Web_API_USPS_URL;
                    //string url = @"https://secure.shippingapis.com/ShippingAPI.dll";//["USPSAPI"];

                    var uspsInputXML = request.Serialize();

                    payload = new FormUrlEncodedContent(new[] 
                        {
                            new KeyValuePair<string, string>("API",Config.Web_API_USPS_VerifyAPIName),
                            //new KeyValuePair<string, string>("API","Verify"),
                            new KeyValuePair<string, string>("XML",uspsInputXML)
                        });

                    var payloadResult = await payload.ReadAsStringAsync();
                    uspsReq = httpClient.GetStringAsync("{0}?{1}".Fill(url, payloadResult));

                    Task<int> uspsRequestLog = null;

                    if (Config.Web_API_USPS_IsDbLoggingEnabled)
                    {
                        logId = await dbLog;
                        uspsRequestLog = db.ExecuteAsync("Update [API].[USPS] set USPSRequest = @USPSRequest, USPSRequestedOn = @USPSRequestedOn where Id = @Id", new { USPSRequest = uspsInputXML, USPSRequestedOn = Common.Now, Id = logId });
                    }

                    string uspsResponseString = null;
                    bool isGenericUSPSError = false;

                    try
                    {
                        uspsResponseString = await uspsReq;
                        isGenericUSPSError = !uspsResponseString.Contains("AddressValidateResponse");

                        if (isGenericUSPSError)
                        {
                            var uspsError = uspsResponseString.Deserialize<Error>();

                            addresses.ForEach(add =>
                            {
                                add.Error = uspsError;
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        isGenericUSPSError = true;

                        // Store exception details received while accessing USPS API in addresses collection
                        addresses.ForEach(add =>
                        {
                            add.Error = new Error
                            {
                                Source = ResponseSource.USPS,
                                Description = ex.ToString()
                            };
                        });

                        uspsResponseString = ex.ToString();

                        //SetResponseValues(addressResponses, idRegex, addresses, ex);
                    }

                    Task<int> uspsResponseLog = null;

                    if (Config.Web_API_USPS_IsDbLoggingEnabled)
                    {
                        var uspsRequestLogAwait = await uspsRequestLog;

                        uspsResponseLog = db.ExecuteAsync("Update [API].[USPS] set USPSResponse = @USPSResponse ,  USPSResponseOn = @USPSResponseOn, IsUSPSError = @IsUSPSError, IsAPIError = @IsAPIError where Id = @Id", new { USPSResponse = uspsResponseString, USPSResponseOn = Common.Now, IsUSPSError = isGenericUSPSError ? true : false, IsAPIError = false, Id = logId });
                    }

                    if (isGenericUSPSError)
                    {
                        SetResponseValues(addressResponses, addresses);
                    }
                    else
                    {
                        var response = uspsResponseString.Deserialize<AddressValidateResponse>();

                        SetResponseValues(addressResponses, response.AddressList);
                    }

                    if (Config.Web_API_USPS_IsDbLoggingEnabled)
                    {
                        var addressResponseXML = addressResponses.Serialize();

                        var uspsResponseLogAwait = await uspsResponseLog;

                        var responseLog = db.ExecuteAsync("Update [API].[USPS] set APIResponse = @APIResponse ,  APIResponseOn = @APIResponseOn, IsAPIError = @IsAPIError Where Id = @Id", new { APIResponse = addressResponseXML, APIResponseOn = Common.Now, IsAPIError = addressResponses.Any(a => a.AddressTypes.Count == 0), Id = logId });

                        await responseLog;
                    }
                }

                return await Task.Run(() => new VerifierAddressResponse(addressResponses)
                {
                    Error = string.Join("\r\n", addressResponses.Where(e => e.Error != null).Select(e => e.Error.Description))
                });
            }
            catch (Exception ex)
            {
                return new VerifierAddressResponse(null)
                  {
                      Error = ex.ToString()
                  };
            }
        }

        private static void SetResponseValues(List<AddressResponse> addressResponses, List<Address> addresses)
        {
            var idRegex = new Regex(@"(\d+{0})(.*)".Fill(ResponseSource.CLSVC));

            foreach (Address address in addresses)
            {
                var responseAdd = addressResponses.Single(res => res.Id == address.ID);

                responseAdd.Id = idRegex.Match(responseAdd.Id).Groups[2].Value;

                //if error by USPS then assign requested add to respond with source CLSVC
                if (address.HasError)
                {
                    var add2 = "{0} {1}".Fill(responseAdd.AddressRequest.Line1, responseAdd.AddressRequest.Line2).Trim();

                    responseAdd.FirmName = responseAdd.AddressRequest.FirmName;
                    responseAdd.Line1 = add2;
                    responseAdd.City = responseAdd.AddressRequest.City;
                    responseAdd.State = responseAdd.AddressRequest.State;
                    responseAdd.Zip5 = responseAdd.AddressRequest.Zip5;
                    responseAdd.Zip4 = responseAdd.AddressRequest.Zip4;
                    responseAdd.Urbanization = responseAdd.AddressRequest.Urbanization;
                    responseAdd.City = responseAdd.AddressRequest.City;
                    responseAdd.AddressTypes = CheckAddressType(new Address { Address2 = add2, State = responseAdd.State, City = responseAdd.City });

                    responseAdd.Error = address.Error;
                    responseAdd.Verifier = ResponseSource.CLSVC;
                    responseAdd.VerifierDesc = "USPS Failure : {0}".Fill(address.Error.Description);

                    if (responseAdd.AddressTypes.Count == 0)
                    {
                        responseAdd.VerifierDesc = "{0}\r\n* AddressType Not Found : '{1}' state is not in USA mainland or territory list*".Fill(responseAdd.VerifierDesc, responseAdd.State);
                    }
                }
                else
                {
                    responseAdd.FirmName = address.FirmName;
                    responseAdd.Line1 = address.Address2;
                    responseAdd.Line1Abbr = address.AddressLine2Abbreviation;
                    responseAdd.City = address.City;
                    responseAdd.CityAbbr = address.CityAbbreviation;
                    responseAdd.State = address.State;
                    responseAdd.Zip5 = address.Zip5;
                    responseAdd.Zip4 = address.Zip4;
                    responseAdd.Urbanization = address.Urbanization;
                    responseAdd.CarrierRoute = address.CarrierRoute;
                    responseAdd.DeliveryPoint = address.DeliveryPoint;
                    responseAdd.ReturnText = address.ReturnText;
                    responseAdd.Error = address.Error;
                    responseAdd.AddressTypes = CheckAddressType(address);

                    responseAdd.Verifier = ResponseSource.USPS;
                    responseAdd.VerifierDesc = address.ReturnText.IsNullOrWhiteSpace() ? string.Empty : address.ReturnText;
                }
            }
        }

        private static List<string> CheckAddressType(Address address)
        {
            var addressTypes = new List<string>();
            //var dhamStates = new string[] { "AK", "AL", "AR", "AZ", "CA", "CO", "CT", "DC", "DE", "FL", "GA", "HI", "IA", "ID", "IL", "IN", "KS", "KY", "LA", "MA", "MD", "ME", "MI", "MN", "MO", "MS", "MT", "NC", "ND", "NE", "NH", "NJ", "NM", "NV", "NY", "OH", "OK", "OR", "PA", "RI", "SC", "SD", "TN", "TX", "UT", "VA", "VT", "WA", "WI", "WV", "WY" };
            var dhamStates = Config.Web_API_USPS_MainlandStates.Split(',').Select(ms => ms.Trim());

            var militarystates = Config.Web_API_USPS_MilitaryStates.Split(',').Select(ms => ms.Trim()); ;
            //var militarystates = new string[] { "AA", "AE", "AP" };
            var militaryCities = Config.Web_API_USPS_MilitaryCities.Split(',').Select(mc => mc.Trim()); ;
            //var militaryCities = new string[] { "APO", "DPO", "FPO" };
            var territory = Config.Web_API_USPS_Territory.Split(',').Select(t => t.Trim());
            //var territory = new string[] { "AS", "FM", "GU", "MH", "MP", "PR", "PW", "VI" };

            if (address.Address2.ToLower().Contains("po box") || address.Address2.ToLower().Contains("pobox"))
                addressTypes.Add(AddressTypes.POBOX);

            if (militarystates.Contains(address.State.ToUpper()) && militaryCities.Contains(address.City.ToUpper()))
                addressTypes.Add(AddressTypes.Military);

            if (territory.Contains(address.State.ToUpper()))
                addressTypes.Add(AddressTypes.Territory);

            if (dhamStates.Contains(address.State.ToUpper()))
                addressTypes.Add(AddressTypes.DHAM);

            return addressTypes;
        }
    }
}
