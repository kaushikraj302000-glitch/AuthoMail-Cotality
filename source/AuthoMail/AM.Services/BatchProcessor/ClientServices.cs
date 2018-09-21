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
        Client GetClientById(int clientId);
        Client GetClientByAppCode(string appCode);
        List<ClientSFTChannel> GetClientSFTChannelMapDetails(int clientId);
        SFTChannel GetSFTChannelDetailsBySFTChannelId(int sftChannelId);
        SFTChannel GetSFTChannelDetailsBySFTChannelCode(string sftChannelCode);
        List<ClientAMCCustIdMap> GetAMCClientIdMaps();
    }
    public class ClientService : ServiceBase, IClientService //It implements Interfaces 
    {
        public Client GetClientById(int clientId)
        {
            string query = "SELECT * FROM [Common].[Clients] WHERE IsAMActive=1 and Id = @Id ";
            var data = db.QuerySingleOrDefault<Client>(query, new { Id = clientId });
            return data;
        }

        public Client GetClientByAppCode(string appCode)
        {
            string query = "SELECT * FROM [Common].[Clients] WHERE IsAMActive=1 and AppCode = @AppCode";
            var data = db.QuerySingleOrDefault<Client>(query, new { AppCode = appCode });
            return data;
        }

        public List<ClientSFTChannel> GetClientSFTChannelMapDetails(int clientId)
        {
            string query = "SELECT ClientId, SFTChannelId FROM [Common].[ClientSFTChannels] WHERE ClientId=@clientId";

            var data = db.Query<ClientSFTChannel>(query, new { clientId = clientId }).ToList();
            return data;
        }

        public SFTChannel GetSFTChannelDetailsBySFTChannelId(int sftChannelId)
        {
            string query = "SELECT Id, Code, Description, Title, Protocol, Host, UserName, Password, ToADEDir, FromADEDir, ArchiveDir, PostDownloadAction FROM [Common].[SFTChannels] WHERE Id=@sftChannelId AND IsActive=1";

            var data = db.QuerySingleOrDefault<SFTChannel>(query, new { sftChannelId = sftChannelId });
            return data;
        }

        public SFTChannel GetSFTChannelDetailsBySFTChannelCode(string sftChannelCode)
        {
            string query = "SELECT Id, Code, Description, Title, Protocol, Host, UserName, Password, ToADEDir, FromADEDir, ArchiveDir, PostDownloadAction FROM [Common].[SFTChannels] WHERE Code=@sftChannelCode AND IsActive=1";

            var data = db.QuerySingleOrDefault<SFTChannel>(query, new { sftChannelCode = sftChannelCode });
            return data;
        }

        public List<ClientAMCCustIdMap> GetAMCClientIdMaps()
        {
            string query = "SELECT [ClientId],[AMCCustId] FROM [AM].[ClientAMCCustIdMap]";

            return db.Query<ClientAMCCustIdMap>(query, null).ToList();
        }
    }
}
