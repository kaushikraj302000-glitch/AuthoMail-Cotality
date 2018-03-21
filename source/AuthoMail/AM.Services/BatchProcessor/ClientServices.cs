using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AM.Entities;
using AM.Lib;

namespace AM.Services
{
    public interface IClientService : IDisposable//Home Service interface that has number of methods
    {
        List<Client> GetClientDetails(int? clientId, string appCode = "");
        List<ClientSFTChannel> GetClientSFTChannelMapDetails(int clientId);
        SFTChannel GetSFTChannelDetailsBySFTChannelId(int sftChannelId);
        SFTChannel GetSFTChannelDetailsBySFTChannelCode(string sftChannelCode);
    }
    public class ClientService : ServiceBase, IClientService //It implements Interfaces 
    {
        public List<Client> GetClientDetails(int? clientId, string appCode)
        {
            string query = "SELECT * FROM [Common].[Clients] WHERE IsAMActive=1 ";

            if (clientId.HasValue)
            {
                query = query + " AND Id={0}".Fill(clientId.Value);
            }

            if (!string.IsNullOrEmpty(appCode))
            {
                query = query + " AND AppCode='{0}'".Fill(appCode);
            }

            var data = db.Query<Client>(query).ToList();
            return data;
        }

        public List<ClientSFTChannel> GetClientSFTChannelMapDetails(int clientId)
        {
            string query = "SELECT ClientId, SFTChannelId FROM [Common].[ClientSFTChannels] WHERE ClientId=@clientId";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@clientId", clientId);

            var data = db.Query<ClientSFTChannel>(query, parameters).ToList();
            return data;
        }

        public SFTChannel GetSFTChannelDetailsBySFTChannelId(int sftChannelId)
        {
            string query = "SELECT Id, Code, Description, Title, Protocol, Host, UserName, Password, ToADEDir, FromADEDir, ArchiveDir, PostDownloadAction FROM [Common].[SFTChannels] WHERE Id=@sftChannelId";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@sftChannelId", sftChannelId);

            var data = db.Query<SFTChannel>(query, parameters).SingleOrDefault();
            return data;
        }

        public SFTChannel GetSFTChannelDetailsBySFTChannelCode(string sftChannelCode)
        {
            string query = "SELECT Id, Code, Description, Title, Protocol, Host, UserName, Password, ToADEDir, FromADEDir, ArchiveDir, PostDownloadAction FROM [Common].[SFTChannels] WHERE Code=@sftChannelCode";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@sftChannelCode", sftChannelCode);

            var data = db.Query<SFTChannel>(query, parameters).SingleOrDefault();
            return data;
        }
    }
}
