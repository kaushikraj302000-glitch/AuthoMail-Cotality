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
        int UpdateClientDomain(string domainName, string existingDomainName);
        List<UserRequestSignUp> GetUsersRequestSignUp();
        KeyValuePair<bool, string> UpdateUsersRequestSignUp(string Id, string status);
        //DocTypes
        List<DocTypes> GetDocTypes();
        //Clients
        List<Clients> GetClients();
        List<BusinessUnitModel> GetBusinessUnitActive();
        KeyValuePair<bool, string> CreateClient(Clients client, string userId);
        int MakeClientAMActiveOrInActive(int Id, bool isActive, string userId);
        int MakeClientPnSActiveOrInActive(int Id, bool isActive, string userId);
        //Business Unit
        List<BusinessUnitModel> GetBusinessUnit();
        int CreateBusinessUnit(BusinessUnitModel businessUnit);
        int MakeActiveOrInActiveBusinessUnit(bool isActive, int Id);
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
            if (domainName.IsNullOrWhiteSpace()) throw new Exception("Domain name cannot be null.");
            if (FingByDomainName(domainName) != null) throw new Exception("Domain name already exists.");
            if (Uri.CheckHostName(domainName) != UriHostNameType.Dns) throw new Exception("Invalid domain name.");
            string query = "INSERT INTO [Common].[ClientDomains] ([ClientId],[AllowedDomain]) VALUES (@clientId,@domainName)";
            return db.Execute(query, new { clientId = clientId, domainName = domainName });
        }
        public int UpdateClientDomain(string domainName, string existingDomainName)
        {
            if (domainName.IsNullOrWhiteSpace()) throw new Exception("Domain name cannot be null.");
            if (FingByDomainName(domainName) != null) throw new Exception("Domain name already exists.");
            if (Uri.CheckHostName(domainName) != UriHostNameType.Dns) throw new Exception("Invalid domain name.");
            string query = "UPDATE [Common].[ClientDomains] SET AllowedDomain=@doaminName WHERE AllowedDomain=@existingDomainName";
            return db.Execute(query, new { domainName = domainName, existingDomainName = existingDomainName });
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
                if (email.IsNullOrWhiteSpace()) return GetKeyValue(false, "Email not found.");

                MailAddress mAdd = new MailAddress(email);
                string getClientId = "SELECT ClientId FROM [Common].[ClientDomains] WHERE AllowedDomain=@domain";
                int? clientId = db.QuerySingleOrDefault<int>(getClientId, new { domain = mAdd.Host });
                if (clientId == null) return GetKeyValue(false, "Client not found.");

                string setClientToUser = "UPDATE [Profile].[Users] SET ClientId=@clientId,Status=@status,ClosedOn=@closedOn WHERE Id=@userId";
                int result = db.Execute(setClientToUser, new { status = AMUserStatus.Approved, closedOn = DateTime.UtcNow, clientId = clientId, userId = Id });
                return result > 0 ? GetKeyValue(true, "User request has been approve.") : GetKeyValue(false, "User request not yet processed.");
            }
            if (status.Equals(AMUserStatus.Rejected, StringComparison.CurrentCultureIgnoreCase))
            {
                string setClientToUser = "UPDATE [Profile].[Users] SET Status=@status,ClosedOn=@closedOn WHERE Id=@userId";
                int result = db.Execute(setClientToUser, new { status = AMUserStatus.Rejected, closedOn = DateTime.UtcNow, userId = Id });
                return result > 0 ? GetKeyValue(true, "User request has been reject") : GetKeyValue(false, "User request not yet processed");
            }
            return GetKeyValue(false, "Invalid action performed.");
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

        public List<Clients> GetClients()
        {
            string query = "SELECT c.*,b.* FROM [Common].[Clients] c,[Common].[BusinessUnit] b WHERE c.BusinessUnit=b.Id";
            return db.Query<Clients, BusinessUnitModel, Clients>(query,
                (c, b) =>
                {
                    c.BusinessUnitModel = b;
                    return c;
                }
                ).ToList();
        }
        public List<BusinessUnitModel> GetBusinessUnitActive()
        {
            string query = "SELECT * FROM [Common].[BusinessUnit] WHERE IsActive=1";
            return db.Query<BusinessUnitModel>(query).ToList();
        }

        public KeyValuePair<bool, string> CreateClient(Clients client, string userId)
        {
            //Validation
            if (client.Name.IsNullOrWhiteSpace()) return GetKeyValue(false, "Name is required.");
            if (client.AppCode.IsNullOrWhiteSpace()) return GetKeyValue(false, "AppCode is required.");
            if (client.TimeZone.IsNullOrWhiteSpace()) return GetKeyValue(false, "TimeZone is required.");
            if (client.BusinessUnit == 0) return GetKeyValue(false, "BusinessUnit is required.");
            string query = "INSERT INTO [Common].[Clients] ([Name], [AppCode], [BusinessUnit], [IsAMActive], [IsPnSActive], [TimeZone], [ModifiedOn], [CreatedOn], [CreatedBy] ,[ModifiedBy]) VALUES (@name,@appCode,@businessUnit,@isAMActive,@isPnSActive,@timeZone,@modifiedOn,@createdOn,@createdBy,@modifiedBy)";
            int result = db.Execute(query, new
            {
                name = client.Name,
                appcode = client.AppCode,
                businessUnit = client.BusinessUnit,
                isAMActive = client.IsAMActive,
                isPnSActive = client.IsPnSActive,
                timeZone = client.TimeZone,
                modifiedOn = DateTime.UtcNow,
                createdOn = DateTime.UtcNow,
                createdBy = userId,
                modifiedBy = userId
            });
            return result > 0 ? GetKeyValue(true, "Client successfully created.")
                : GetKeyValue(false, "Invalid action performed.");
        }
        public int MakeClientAMActiveOrInActive(int Id, bool isActive, string userId)
        {
            string query = "UPDATE [Common].[Clients] SET IsAMActive=@isActive,ModifiedOn=@modifiedOn,ModifiedBy=@modifiedBy WHERE Id=@id";
            return db.Execute(query, new { isActive = isActive, modifiedOn = DateTime.UtcNow, modifiedBy = userId, id = Id });
        }

        public int MakeClientPnSActiveOrInActive(int Id, bool isActive, string userId)
        {
            string query = "UPDATE [Common].[Clients] SET IsAMActive=@isActive,ModifiedOn=@modifiedOn,ModifiedBy=@modifiedBy WHERE Id=@id";
            return db.Execute(query, new { isActive = isActive, modifiedOn = DateTime.UtcNow, modifiedBy = userId, id = Id });
        }

        private KeyValuePair<bool, string> GetKeyValue(bool key, string value)
        {
            return new KeyValuePair<bool, string>(key, value);
        }

        public List<BusinessUnitModel> GetBusinessUnit()
        {
            string query = "SELECT * FROM [Common].[BusinessUnit]";
            return db.Query<BusinessUnitModel>(query).ToList();
        }
        public int CreateBusinessUnit(BusinessUnitModel businessUnit)
        {
            string query = "INSERT INTO [Common].[BusinessUnit] ([BusinessUnit], [CustCode], [IsActive]) VALUES (@businessUnit, @custCode, @isActive)";
            return db.Execute(query, new { businessUnit = businessUnit.BusinessUnit, custCode = businessUnit.CustCode, isActive = businessUnit.IsActive });
        }
        public int MakeActiveOrInActiveBusinessUnit(bool isActive, int Id)
        {
            string query = "UPDATE [Common].[BusinessUnit] SET IsActive=@isActive WHERE Id=@id";
            return db.Execute(query, new { isActive = isActive, id = Id });
        }

    }
}
