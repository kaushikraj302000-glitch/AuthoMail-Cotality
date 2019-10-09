using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public interface ISigningRoomRequestService
    {
        Task<SigningRoomResponse> CreateSigningRoom(SigningRoomRequest signingRoomRequest);
    }
    public class SigningRoomRequestService : ISigningRoomRequestService
    {
        public async Task<SigningRoomResponse> CreateSigningRoom(SigningRoomRequest signingRoomRequest)
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

                    string json = JsonConvert.SerializeObject(signingRoomRequest);
                    StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(Config.E_API_CreateSigningRoomURL, content); //"api/SigningRoomManager/SigningRooms/CreateFromPackage"
                    var responseString = await response.Content.ReadAsStringAsync();
                    SigningRoomResponse signingRoomResponse = JsonConvert.DeserializeObject<SigningRoomResponse>(responseString);
                    return signingRoomResponse;
                }
            }
            catch (Exception ex)
            {
                return new SigningRoomResponse()
                {
                    Error = string.Format("Unexpected error occured while creating signing room : {0}", ex.ToString())
                };
            }
        }
    }
}

