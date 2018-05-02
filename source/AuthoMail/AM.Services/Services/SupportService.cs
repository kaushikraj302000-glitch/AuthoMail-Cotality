using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AM.Lib;
namespace AM.Services
{
    public interface ISupportService : IDisposable
    {
        List<Clientdomains> FindByClientId(int clientId);
        Clientdomains FingByDomainName(string domainName);
        bool HasClientDomain(int clientId, string domainName);
        int CreateClientDomain(int clientId, string domainName);
        List<Clients> GetClients();
        Clients FindClientByAppCode(string appCode);
    }
    public class SupportService : ServiceBase, ISupportService
    {
        public List<Clientdomains> FindByClientId(int clientId)
        {
            string query = "SELECT * FROM [Common].[ClientDomains] WHERE [ClientId] = @clientId";
            return db.Query<Clientdomains>(query, new { clientId = clientId }).ToList();
        }

        public Clientdomains FingByDomainName(string domainName)
        {
            string query = "SELECT * FROM [Common].[ClientDomains] WHERE [AllowedDomain] = @domainName";
            return db.QuerySingle<Clientdomains>(query, new { domainName = domainName });
        }

        public bool HasClientDomain(int clientId, string domainName)
        {
            string query = "SELECT COUNT(ClientId) FROM [Common].[ClientDomains] WHERE [ClientId]=@clientId AND [AllowedDomain] = @domainName";
            int result = db.Execute(query, new { clientId = clientId, domainName = domainName });
            return result == 0 ? false : true;
        }


        public int CreateClientDomain(int clientId, string domainName)
        {
            if (domainName.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException("Domain name cannot be null.");
            }
            string query = "INSERT INTO [Common].[ClientDomains] ([ClientId],[AllowedDomain]) VALUES (@clientId,'@domainName')";
            return db.Execute(query, new { clientId = clientId, domainName = domainName });
        }

        public List<Clients> GetClients()
        {
            string query = "SELECT * FROM [Common].[Clients]";
            return db.Query<Clients>(query).ToList();
        }

        public Clients FindClientByAppCode(string appCode)
        {
            string query = "select * from [Common].[Clients] WHERE AppCode=@appCode";
            return db.QuerySingle<Clients>(query, new { appCode = appCode });
        }
    }
}
