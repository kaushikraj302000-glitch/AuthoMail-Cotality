using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AM.Lib;
using System.Net.Mail;
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
        List<UserRequestSignUp> GetUsersRequestSignUp();
        int RemoveUsersRequestSignUp(string email);
        bool ApproveUsersRequestSignUp(string email);
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
            return db.QuerySingleOrDefault<Clientdomains>(query, new { domainName = domainName });
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
            return db.QuerySingleOrDefault<Clients>(query, new { appCode = appCode });
        }

        public List<UserRequestSignUp> GetUsersRequestSignUp()
        {
            string query = "SELECT UserName,Email,EmailConfirmed,FirstName,LastName,DisplayName FROM [Profile].[Users] WHERE ClientId IS NULL";
            return db.Query<UserRequestSignUp>(query).ToList();
        }


        public int RemoveUsersRequestSignUp(string email)
        {
            string query = "DELETE FROM [Profile].[Users] WHERE Email=@email";
            return db.Execute(query, new { email = email });
        }


        public bool ApproveUsersRequestSignUp(string email)
        {
            string getUser = "SELECT Id FROM [Profile].[Users] WHERE Email=@email";
            string userId = db.QuerySingleOrDefault<string>(getUser, new { email = email });
            MailAddress mAdd = new MailAddress(email);
            string getClientId = "SELECT ClientId FROM [Common].[ClientDomains] WHERE AllowedDomain=@domain";
            int? clientId = db.QuerySingleOrDefault<int>(getClientId, new { domain = mAdd.Host });
            if (userId.IsNullOrWhiteSpace() || clientId == null)
            {
                throw new ArgumentNullException("User or Client not found.");
            }
            string setClientToUser = "UPDATE [Profile].[Users] SET ClientId=@clientId WHERE Id=@userId";
            int result = db.Execute(setClientToUser, new { userId = userId });
            return result > 1 ? true : false;
        }
    }
}
