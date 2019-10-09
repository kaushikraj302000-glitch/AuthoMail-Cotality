using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public interface ISigningRoomActivityLogServices
    {
        Task<List<SigningRoomActivityLogResponse>> GetSigningRoomActivityLog(string signingRoomName);
    }
    public class SigningRoomActivityLogServices : ISigningRoomActivityLogServices
    {
        public async Task<List<SigningRoomActivityLogResponse>> GetSigningRoomActivityLog(string signingRoomName)
        {
            try
            {
                var token = await GenerateToken.GetToken();
                using (var client = new HttpClient())
                {
                    //setup client
                    client.BaseAddress = new Uri(Config.E_API_BaseURL); //"http://10.215.48.138:8080/"
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                    string json = @"{ 
                                        'SigningRoomName': '" + signingRoomName + "' " +
                                   "}";
                    StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(Config.E_API_SigningRoomActivityLogSearchURL, content);//"api/Reports/SigningRoomActivityLogSearch"
                    var responseString = await response.Content.ReadAsStringAsync();
                    List<SigningRoomActivityLogResponse> signingRoomActivityLogResponse = JsonConvert.DeserializeObject<List<SigningRoomActivityLogResponse>>(responseString);
                    return signingRoomActivityLogResponse;
                }
            }
            catch (Exception ex)
            {
                return new List<SigningRoomActivityLogResponse>()
                {
                    new SigningRoomActivityLogResponse
                    {
                        Error = string.Format("Unexpected error occured while getting signing room activity logs: {0}", ex.ToString())
                    }
                };
            }
        }
    }
}
