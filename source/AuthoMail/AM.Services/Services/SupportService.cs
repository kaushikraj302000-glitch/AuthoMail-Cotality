using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AM.Lib;
using System.Net.Mail;
using AM.Entities;
namespace AM.Services
{
    public interface ISupportService : IDisposable
    {
        List<ClientDomains> FindByClientId(int clientId);
        ClientDomains FingByDomainName(string domainName);
        List<ClientDomains> GetClientDomains();
        bool HasClientDomain(int clientId, string domainName);
        int CreateClientDomain(int clientId, string domainName);
        List<Clients> GetClients();
        List<UserRequestSignUp> GetUsersRequestSignUp();
        KeyValuePair<bool, string> UpdateUsersRequestSignUp(string Id, string status);
        //DocTypes
        List<DocTypes> GetDocTypes();
    }
    public class SupportService : ServiceBase, ISupportService
    {
        public List<ClientDomains> FindByClientId(int clientId)
        {
            string query = "SELECT cd.*,c.Name as [ClientName] FROM [Common].[ClientDomains] cd,[Common].[Clients] c WHERE cd.[ClientId] = @clientId AND cd.ClientId=c.Id";
            return db.Query<ClientDomains>(query, new { clientId = clientId }).ToList();
        }

        public ClientDomains FingByDomainName(string domainName)
        {
            string query = "SELECT cd.*,c.Name as [ClientName] FROM [Common].[ClientDomains] cd,[Common].[Clients] c WHERE [AllowedDomain] = @domainName AND cd.ClientId=c.Id";
            return db.QuerySingleOrDefault<ClientDomains>(query, new { domainName = domainName });
        }

        public List<ClientDomains> GetClientDomains()
        {
            string query = "SELECT cd.*,c.Name as [ClientName] FROM [Common].[ClientDomains] cd,[Common].[Clients] c WHERE cd.ClientId=c.Id";
            return db.Query<ClientDomains>(query).ToList();
        }

        public bool HasClientDomain(int clientId, string domainName)
        {
            string query = "SELECT COUNT(ClientId) FROM [Common].[ClientDomains] WHERE [ClientId]=@clientId AND [AllowedDomain] = @domainName";
            int result = db.Execute(query, new { clientId = clientId, domainName = domainName });
            return result == 0 ? false : true;
        }


        public int CreateClientDomain(int clientId, string domainName)
        {
            if (domainName.IsNullOrWhiteSpace()) throw new ArgumentNullException("Domain name cannot be null.");
            if (FingByDomainName(domainName) != null) return 0;

            string query = "INSERT INTO [Common].[ClientDomains] ([ClientId],[AllowedDomain]) VALUES (@clientId,@domainName)";
            return db.Execute(query, new { clientId = clientId, domainName = domainName });
        }

        public List<Clients> GetClients()
        {
            string query = "SELECT * FROM [Common].[Clients]";
            return db.Query<Clients>(query).ToList();
        }

        public List<UserRequestSignUp> GetUsersRequestSignUp()
        {
            string query = "SELECT Id,UserName,Email,EmailConfirmed,FirstName,LastName,DisplayName,RequestedOn,Status FROM [Profile].[Users] WHERE Status = @status AND EmailConfirmed=1";
            return db.Query<UserRequestSignUp>(query, new { status = AMUserStatus.Open }).ToList();
        }

        public KeyValuePair<bool, string> UpdateUsersRequestSignUp(string Id, string status)
        {
            if (status.Equals(AMUserStatus.Approved, StringComparison.CurrentCultureIgnoreCase))
            {
                string emailQuery = "SELECT Email FROM [Profile].[Users] WHERE Id=@id";
                string email = db.QuerySingleOrDefault<string>(emailQuery, new { id = Id });
                if (email.IsNullOrWhiteSpace()) return new KeyValuePair<bool, string>(false, "Email not found.");

                MailAddress mAdd = new MailAddress(email);
                string getClientId = "SELECT ClientId FROM [Common].[ClientDomains] WHERE AllowedDomain=@domain";
                int? clientId = db.QuerySingleOrDefault<int>(getClientId, new { domain = mAdd.Host });
                if (clientId == null) return new KeyValuePair<bool, string>(false, "Client not found.");

                string setClientToUser = "UPDATE [Profile].[Users] SET ClientId=@clientId,Status=@status,ClosedOn=@closedOn WHERE Id=@userId";
                int result = db.Execute(setClientToUser, new { status = AMUserStatus.Approved, closedOn = DateTime.UtcNow, clientId = clientId, userId = Id });
                return result > 1 ? new KeyValuePair<bool, string>(true, "User request has been approve.") : new KeyValuePair<bool, string>(false, "User request not yet processed.");
            }
            if (status.Equals(AMUserStatus.Rejected, StringComparison.CurrentCultureIgnoreCase))
            {
                string setClientToUser = "UPDATE [Profile].[Users] SET Status=@status,ClosedOn=@closedOn WHERE Id=@userId";
                int result = db.Execute(setClientToUser, new { status = AMUserStatus.Rejected, closedOn = DateTime.UtcNow, userId = Id });
                return result > 1 ? new KeyValuePair<bool, string>(true, "User request has been reject") : new KeyValuePair<bool, string>(false, "User request not yet processed");
            }
            return new KeyValuePair<bool, string>(false, "Invalid action performed.");
        }


        public List<DocTypes> GetDocTypes()
        {
            string query = "SELECT d.*,c.* FROM [AM].[DocTypes] d,[Common].[Clients] c WHERE d.ClientId=c.Id";
            return db.Query<DocTypes, Clients, DocTypes>(query,
                (d, c) =>
                {
                    d.Client = c;
                    return d;
                }
                ).ToList();
        }

    }
}
