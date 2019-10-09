using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public interface ISigningRoomSummaryService
    {
        Task<string> GetSigningRoomStatus(int signingRoomId);
    }
    public class SigningRoomSummaryService : ISigningRoomSummaryService
    {
        public async Task<string> GetSigningRoomStatus(int signingRoomId)
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

                    //make request
                    HttpResponseMessage response = await client.GetAsync(string.Format(Config.E_API_SigningRoomSummaryURL, signingRoomId)); //"api/SigningRoomManager/SigningRooms/{0}/signingroomsummary"
                    var responseString = await response.Content.ReadAsStringAsync();
                    var jObject = JObject.Parse(responseString);
                    return jObject.GetValue("Status").ToString();
                }
            }
            catch (Exception ex)
            {
                return string.Format("Unexpected error occured while getting signing room summary : {0}", ex.ToString());
            }
        }
    }
}
