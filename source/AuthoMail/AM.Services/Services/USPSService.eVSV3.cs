using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services.V3
{
    public interface IUSPSAPIClient
    {
        Task<AddressResponseV3> GetAddress(Address request);
    }

    public class USPSAPIClient : IUSPSAPIClient, IDisposable
    {
        readonly RestClient _client;
        private string baseUSPSURL;

        public USPSAPIClient(string apiKey, string apiKeySecret)
        {
            baseUSPSURL = "https://api-cat.usps.com";

            var options = new RestClientOptions(baseUSPSURL)
            {
                Authenticator = new USPSAPIAuthenticator(baseUSPSURL, apiKey, apiKeySecret)
            };

            _client = new RestClient(options);
        }

        public async Task<AddressResponseV3> GetAddress(Address address)
        {
            try
            {
                RestRequest request = new RestRequest("addresses/v3/address", Method.GET);
                AddressResponseV3 response = new AddressResponseV3();

                // Add query parameters to the request URL
                request.AddParameter("streetAddress", address.Address1);
                
                if (!string.IsNullOrWhiteSpace(address.Address2))
                    request.AddParameter("secondaryAddress", address.Address2);
                
                if (!string.IsNullOrWhiteSpace(address.City))
                    request.AddParameter("city", address.City);
                
                request.AddParameter("state", address.State);
                
                request.AddParameter("ZIPCode", address.Zip5);
                
                if (!string.IsNullOrWhiteSpace(address.Zip4))
                    request.AddParameter("ZIPPlus4", address.Zip4);

                // Execute request and handle response
                var resp = await _client.ExecuteAsync(request).ConfigureAwait(false);

                if (resp != null && resp.IsSuccessful)
                {
                    response = JsonConvert.DeserializeObject<AddressResponseV3>(resp.Content);
                }
                else
                {
                    var err = JsonConvert.DeserializeObject<AddressErrorResponseV3>(resp.Content.Replace(@"\", @"\\"));
                    response.Error = err.error;
                }

                return response;//!.Data;
            }
            catch (Exception ex)
            {
                ex.ToString();
                return new AddressResponseV3();
            }
        }

        public void Dispose()
        {
            _client?.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    public class USPSAPIAuthenticator : AuthenticatorBase
    {
        readonly string _baseUrl;
        readonly string _clientId;
        readonly string _clientSecret;
        private DateTime expiredTime;

        public USPSAPIAuthenticator(string baseUrl, string clientId, string clientSecret) : base("")
        {
            _baseUrl = baseUrl;
            _clientId = clientId;
            _clientSecret = clientSecret;

            //Token = string.IsNullOrEmpty(Token) || expiredTime < DateTime.Now ? GetToken() : Token;

            ValidateToken();
        }

        protected override async ValueTask<Parameter> GetAuthenticationParameter(string accessToken)
        {
            Token = string.IsNullOrEmpty(Token) || expiredTime < DateTime.Now ? await GetToken() : Token;
            return new HeaderParameter(KnownHeaders.Authorization, Token);
        }

        public bool ValidateToken()
        {
            Token = string.IsNullOrEmpty(Token) || expiredTime < DateTime.Now ? GetToken().Result : Token;

            return string.IsNullOrWhiteSpace(Token);
        }


        async Task<string> GetToken()
        {
            {
                USPSTokenResponse response = new USPSTokenResponse();

                //var requestJson = new USPSTokenRequest { ClientId = "6oZjA4lgwGPfJCHSeHpnxitxupQ1qGi0", ClientPassword = "Kp1Alu4oHV9G6WB6" };

                var options = new RestClientOptions(_baseUrl)
                {
                    Authenticator = new HttpBasicAuthenticator(_clientId, _clientSecret),
                };

                using (var client = new RestClient(options))
                {
                    if (this._baseUrl.StartsWith("https", StringComparison.CurrentCultureIgnoreCase))
                    {
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    }

                    var requestJson = new USPSTokenRequest { client_id = _clientId, client_secret = _clientSecret };

                    var request = new RestRequest("oauth2/v3/token", Method.POST);
                    request.AddHeader("Content-Type", "application/json");

                    request.AddJsonBody(requestJson);

                    //var json = JsonConvert.SerializeObject(requestJson);

                    //request.AddJsonBody(json);

                    response = await client.PostAsync<USPSTokenResponse>(request).ConfigureAwait(false);

                    //if (resp != null && resp.IsSuccessful)
                    //{
                    //    response = JsonConvert.DeserializeObject<USPSTokenResponse>(resp.Content);
                    //}

                    if (response != null)
                    {
                        int expires_in = Convert.ToInt32(response.expires_in);
                        DateTime tokenLife = DateTime.Now.AddSeconds(expires_in);
                        expiredTime = tokenLife;
                    }
                }

                return $"{response?.token_type} {response?.access_token}";
            }
        }
    }  
}