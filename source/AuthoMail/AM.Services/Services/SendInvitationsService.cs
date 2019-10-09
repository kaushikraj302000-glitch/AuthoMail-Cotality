using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public interface ISendInvitationsService
    {
        Task<bool> SendInvitations(int signingRoomId);
    }
    public class SendInvitationsService : ISendInvitationsService
    {
        public async Task<bool> SendInvitations(int signingRoomId)
        {
            bool isInvitationSent = false;
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

                    StringContent content = new StringContent("", Encoding.UTF8, "application/json");

                    //make request
                    HttpResponseMessage response = await client.PostAsync(string.Format(Config.E_API_SendInvitationURL, signingRoomId), content); //"api/SigningRoomManager/SigningRooms/{0}/Participants/SendInvitations"
                    if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.OK)
                        isInvitationSent = true;
                    return isInvitationSent;
                }
            }
            catch (Exception)
            {
                return isInvitationSent;
            }
        }
    }
}
