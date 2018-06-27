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
        List<ClientDomains> FindDomainByClientId(int clientId);
        ClientDomains FingByDomainName(string domainName);
        List<ClientDomains> GetClientDomains();
        bool HasClientDomain(int clientId, string domainName);
        int CreateClientDomain(int clientId, string domainName);
        int UpdateClientDomain(string domainName, string existingDomainName);
        List<UserRequestSignUp> GetUsersRequestSignUp(string userId);
        KeyValuePair<bool, string> UpdateUsersRequestSignUp(string Id, string status, string closedBy);
        //DocTypes
        List<DocTypes> GetDocTypes();
        //Clients
        List<Clients> GetClients();
        Clients FindByClientId(int Id);
        List<BusinessUnitModel> GetBusinessUnitActive();
        KeyValuePair<bool, string> CreateEditClient(Clients client, string userId);
        int MakeClientAMActiveOrInActive(int Id, bool isActive, string userId);
        int MakeClientPnSActiveOrInActive(int Id, bool isActive, string userId);
        //Business Unit
        List<BusinessUnitModel> GetBusinessUnit();
        BusinessUnitModel FindByIdBusinessUnit(int Id);
        int CreateOrEditBusinessUnit(BusinessUnitModel businessUnit);
        int MakeActiveOrInActiveBusinessUnit(bool isActive, int Id);
        //DocType
        KeyValuePair<bool, string> CreateOrEditDocType(DocTypes docTypes, string userId);
        int MakeActivateOrDeactivateDocType(int Id, bool isActive, string userId);
        DocTypes FindByIdDocType(int Id);
        //SFTChannel
        List<SFTChannels> GetSFTChannels();
        SFTChannels FindByIdSFTChannels(int Id);
        KeyValuePair<bool, string> CreateOrEditSFTChannel(SFTChannels sftChannel);
        int MakeActivateOrDeactivateSFTChannel(int Id, bool isActive);
        //Assign Or Remove ClientUserManager,CoreLogicUserManager role 
        KeyValuePair<bool, string> AssignUserManagerRole(string email);
        KeyValuePair<bool, string> RemoveUserManagerRole(string userId, string roleId);
        List<EmailAllowedDomain> GetUserManagerList();
        string GetEmailsFromClientDomain(string clientEmail);
    }
    public class SupportService : ServiceBase, ISupportService
    {
        public List<ClientDomains> FindDomainByClientId(int clientId)
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
            if (Uri.CheckHostName(domainName) != UriHostNameType.Dns) throw new Exception("Invalid domain name.");
            if (FingByDomainName(domainName) != null) throw new Exception("Domain name already exists.");
            if (FingByDomainName(existingDomainName) == null) throw new Exception("Existing domain name not found.");

            var usersCountQ = "SELECT TOP 1 Email FROM [Profile].[Users] WHERE Email Like @domain";
            string userEmail = db.QueryFirstOrDefault<string>(usersCountQ, new { domain = string.Format("%{0}", existingDomainName) });
            if (userEmail != null) throw new Exception("Domain name couldn't change because users are registered with this domain.");

            string query = "UPDATE [Common].[ClientDomains] SET AllowedDomain=@domainName WHERE AllowedDomain=@existingDomainName";
            return db.Execute(query, new { domainName = domainName, existingDomainName = existingDomainName });
        }
        public List<UserRequestSignUp> GetUsersRequestSignUp(string userId)
        {
            if (userId != null)
            {
                string email = db.QuerySingle<string>("SELECT Email FROM [Profile].[Users] WHERE Id= @userId", new { userId = userId });
                var mailAddress = new MailAddress(email);
                string sql = "SELECT Id,UserName,Email,EmailConfirmed,FirstName,LastName,DisplayName,RequestedOn,Status FROM [Profile].[Users] WHERE Status = @status AND EmailConfirmed=1 AND Email LIKE @email";
                return db.Query<UserRequestSignUp>(sql, new { status = AMUserStatus.Open, email = string.Format("%{0}", mailAddress.Host) }).ToList();
            }
            string query = "SELECT Id,UserName,Email,EmailConfirmed,FirstName,LastName,DisplayName,RequestedOn,Status FROM [Profile].[Users] WHERE Status = @status AND EmailConfirmed=1";
            return db.Query<UserRequestSignUp>(query, new { status = AMUserStatus.Open }).ToList();
        }

        public KeyValuePair<bool, string> UpdateUsersRequestSignUp(string Id, string status, string closedBy)
        {
            if (status.Equals(AMUserStatus.Approved, StringComparison.CurrentCultureIgnoreCase))
            {
                string emailQuery = "SELECT Email FROM [Profile].[Users] WHERE Id=@id";
                string email = db.QuerySingleOrDefault<string>(emailQuery, new { id = Id });
                if (email.IsNullOrWhiteSpace()) return GetKeyValue(false, "Email not found.");

                MailAddress mAdd = new MailAddress(email);
                var clientQuery = "SELECT ClientId FROM [Common].[ClientDomains] WHERE AllowedDomain=@domain";
                var client = db.QuerySingleOrDefault(clientQuery, new { domain = mAdd.Host });
                int? clientId = client.ClientId;

                if (clientId == null) return GetKeyValue(false, "Client not found.");
                bool IsAMActive = db.QuerySingle<bool>("SELECT IsAMActive FROM [Common].[Clients] WHERE Id=@clientId", new { clientId = clientId });
                if (!IsAMActive) return GetKeyValue(false, "Client is not activated.");

                string setClientToUser = "UPDATE [Profile].[Users] SET ClientId=@clientId,Status=@status,ClosedOn=@closedOn,ClosedBy=@closedBy WHERE Id=@userId";
                int result = db.Execute(setClientToUser, new { status = AMUserStatus.Approved, closedOn = DateTime.UtcNow, clientId = clientId, closedBy = closedBy, userId = Id });
                return result > 0 ? GetKeyValue(true, "User request has been approved.") : GetKeyValue(false, "User request not yet processed.");
            }
            if (status.Equals(AMUserStatus.Rejected, StringComparison.CurrentCultureIgnoreCase))
            {
                string setClientToUser = "UPDATE [Profile].[Users] SET Status=@status,ClosedOn=@closedOn,ClosedBy=@closedBy WHERE Id=@userId";
                int result = db.Execute(setClientToUser, new { status = AMUserStatus.Rejected, closedOn = DateTime.UtcNow, closedBy = closedBy, userId = Id });
                return result > 0 ? GetKeyValue(true, "User request has been rejected") : GetKeyValue(false, "User request not yet processed");
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
        public Clients FindByClientId(int Id)
        {
            string query = "SELECT * FROM [Common].[Clients] WHERE Id=@id";
            return db.QuerySingleOrDefault<Clients>(query, new { id = Id });
        }
        public List<BusinessUnitModel> GetBusinessUnitActive()
        {
            string query = "SELECT * FROM [Common].[BusinessUnit] WHERE IsActive=1";
            return db.Query<BusinessUnitModel>(query).ToList();
        }

        public KeyValuePair<bool, string> CreateEditClient(Clients client, string userId)
        {
            //Validation
            if (client.Name.IsNullOrWhiteSpace()) return GetKeyValue(false, "Name is required.");
            if (client.AppCode.IsNullOrWhiteSpace()) return GetKeyValue(false, "AppCode is required.");
            if (client.TimeZone.IsNullOrWhiteSpace()) return GetKeyValue(false, "TimeZone is required.");
            if (client.BusinessUnit == 0) return GetKeyValue(false, "BusinessUnit is required.");
            if (client.Id == 0)
            {
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
            else
            {
                string query = "UPDATE [Common].[Clients] SET Name=@name,AppCode=@appCode,BusinessUnit=@businessUnit,TimeZone=@timeZone,ModifiedBy=@modifiedBy,ModifiedOn=@modifiedOn WHERE Id=@id";
                int result = db.Execute(query, new { name = client.Name, appCode = client.AppCode, businessUnit = client.BusinessUnit, timeZone = client.TimeZone, modifiedBy = userId, modifiedOn = DateTime.UtcNow, id = client.Id });
                return result > 0 ? GetKeyValue(true, "Client updated successfully.")
                    : GetKeyValue(false, "Invalid action performed.");
            }
        }

        public int MakeClientAMActiveOrInActive(int Id, bool isActive, string userId)
        {
            string query = "UPDATE [Common].[Clients] SET IsAMActive=@isActive,ModifiedOn=@modifiedOn,ModifiedBy=@modifiedBy WHERE Id=@id";
            return db.Execute(query, new { isActive = isActive, modifiedOn = DateTime.UtcNow, modifiedBy = userId, id = Id });
        }

        public int MakeClientPnSActiveOrInActive(int Id, bool isActive, string userId)
        {
            string query = "UPDATE [Common].[Clients] SET IsPnSActive=@isActive,ModifiedOn=@modifiedOn,ModifiedBy=@modifiedBy WHERE Id=@id";
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
        public BusinessUnitModel FindByIdBusinessUnit(int Id)
        {
            string query = "SELECT * FROM [Common].[BusinessUnit] WHERE Id=@id";
            return db.QuerySingleOrDefault<BusinessUnitModel>(query, new { id = Id });
        }
        public int CreateOrEditBusinessUnit(BusinessUnitModel businessUnit)
        {
            if (businessUnit.Id == 0)
            {
                string query = "INSERT INTO [Common].[BusinessUnit] ([BusinessUnit], [CustCode], [IsActive]) VALUES (@businessUnit, @custCode, @isActive)";
                return db.Execute(query, new { businessUnit = businessUnit.BusinessUnit, custCode = businessUnit.CustCode, isActive = businessUnit.IsActive });
            }
            else
            {
                string query = "UPDATE [Common].[BusinessUnit] SET [BusinessUnit] = @businessUnit, [CustCode] = @custCode WHERE Id=@id";
                return db.Execute(query, businessUnit);
            }
        }
        public int MakeActiveOrInActiveBusinessUnit(bool isActive, int Id)
        {
            string query = "UPDATE [Common].[BusinessUnit] SET IsActive=@isActive WHERE Id=@id";
            return db.Execute(query, new { isActive = isActive, id = Id });
        }



        public KeyValuePair<bool, string> CreateOrEditDocType(DocTypes docTypes, string userId)
        {
            //Validation
            if (docTypes.DocTypeCode.IsNullOrWhiteSpace()) throw new Exception("DocTypeCode can't be null or empty.");
            if (docTypes.Description.IsNullOrWhiteSpace()) throw new Exception("Description can't be null or empty.");
            if (docTypes.PrintSize.IsNullOrWhiteSpace()) throw new Exception("PrintSize can't be null or empty.");
            if (FindByClientId(docTypes.ClientId) == null) throw new Exception("Client not found.");
            if (docTypes.IsReturnInfoSpecified)
            {
                if (docTypes.RetRcptFirstName.IsNullOrWhiteSpace()) throw new Exception("First name can't be null or empty.");
                if (docTypes.RetRcptMiddleName.IsNullOrWhiteSpace()) throw new Exception("Middle name can't be null or empty.");
                if (docTypes.RetRcptLastName.IsNullOrWhiteSpace()) throw new Exception("Last name can't be null or empty.");
                if (docTypes.RetRcptAddressCity.IsNullOrWhiteSpace()) throw new Exception("City can't be null or empty.");
                if (docTypes.RetRcptAddressState.IsNullOrWhiteSpace()) throw new Exception("State can't be null or empty.");
                if (docTypes.RetRcptAddressZIP.IsNullOrWhiteSpace()) throw new Exception("zip can't be null or empty.");
            }
            if (docTypes.Id == 0)
            {
                string query = "INSERT INTO [AM].[DocTypes] ([DocTypeCode] ,[Description] ,[ClientId] ,[PrintSize] ,[ContentScalePercent] ,[PrintFormat] ,[QCPercent] ,[SLADays] ,[SLACutOffTime] ,[ShippingCarrier] ,[ShippingServiceType] ,[ShippingAccountCode] ,[IsReturnInfoSpecified] ,[RetRcptPrefix] ,[RetRcptFirstName] ,[RetRcptMiddleName] ,[RetRcptLastName] ,[RetRcptSuffix] ,[RetRcptPhone] ,[RetRcptOrganization] ,[RetRcptAddrOptionalLine1AboveStreet] ,[RetRcptAddrOptionalLine2AboveStreet] ,[RetRcptAddressStreet] ,[RetRcptAddressAptOrSuiteOrOther] ,[RetRcptAddrOptionalLine1BelowStreet] ,[RetRcptAddrOptionalLine2BelowStreet] ,[RetRcptAddressCity] ,[RetRcptAddressState] ,[RetRcptAddressZIP] ,[ClientEmails] ,[ClientContactNo] ,[SupportEmails] ,[SupportContactNo] ,[IsActive] ,[ModifiedOn] ,[CreatedOn] ,[CreatedBy] ,[ModifiedBy]) VALUES (@docTypeCode,@description,@clientId,@printSize,@contentScalePercent,@printFormat,@qCPercent,@sLADays,@sLACutOffTime,@shippingCarrier,@shippingServiceType,@shippingAccountCode,@isReturnInfoSpecified,@retRcptPrefix,@retRcptFirstName,@retRcptMiddleName,@retRcptLastName,@retRcptSuffix,@retRcptPhone,@retRcptOrganization,@retRcptAddrOptionalLine1AboveStreet,@retRcptAddrOptionalLine2AboveStreet,@retRcptAddressStreet,@retRcptAddressAptOrSuiteOrOther,@retRcptAddrOptionalLine1BelowStreet,@retRcptAddrOptionalLine2BelowStreet,@retRcptAddressCity,@retRcptAddressState,@retRcptAddressZIP,@clientEmails,@clientContactNo,@supportEmails,@supportContactNo,@isActive,@modifiedOn,@createdOn,@createdBy,@modifiedBy)";
                docTypes.CreatedOn = DateTime.UtcNow;
                docTypes.CreatedBy = userId;
                docTypes.ModifiedOn = DateTime.UtcNow;
                docTypes.IsActive = false;
                docTypes.ModifiedBy = userId;
                int rows = db.Execute(query, docTypes);
                return rows > 0 ? GetKeyValue(true, "DocType saved successfully.") : GetKeyValue(false, "Invalid action performed.");
            }
            else
            {
                string query = "UPDATE [AM].[DocTypes] SET [DocTypeCode]=@docTypeCode ,[Description]=@description ,[ClientId]=@clientId ,[PrintSize]=@printSize ,[ContentScalePercent]=@contentScalePercent ,[PrintFormat]=@printFormat ,[QCPercent]=@qCPercent ,[SLADays]=@sLADays ,[SLACutOffTime]=@sLACutOffTime ,[ShippingCarrier]=@shippingCarrier ,[ShippingServiceType]=@shippingServiceType ,[ShippingAccountCode]=@shippingAccountCode ,[IsReturnInfoSpecified]=@isReturnInfoSpecified ,[RetRcptPrefix]=@retRcptPrefix ,[RetRcptFirstName]=@retRcptFirstName ,[RetRcptMiddleName]=@retRcptMiddleName ,[RetRcptLastName]=@retRcptLastName ,[RetRcptSuffix]=@retRcptSuffix ,[RetRcptPhone]=@retRcptPhone ,[RetRcptOrganization]=@retRcptOrganization ,[RetRcptAddrOptionalLine1AboveStreet]=@retRcptAddrOptionalLine1AboveStreet ,[RetRcptAddrOptionalLine2AboveStreet]=@retRcptAddrOptionalLine2AboveStreet ,[RetRcptAddressStreet]=@retRcptAddressStreet ,[RetRcptAddressAptOrSuiteOrOther]=@retRcptAddressAptOrSuiteOrOther ,[RetRcptAddrOptionalLine1BelowStreet]=@retRcptAddrOptionalLine1BelowStreet ,[RetRcptAddrOptionalLine2BelowStreet]=@retRcptAddrOptionalLine2BelowStreet ,[RetRcptAddressCity]=@retRcptAddressCity ,[RetRcptAddressState]=@retRcptAddressState ,[RetRcptAddressZIP]=@retRcptAddressZIP ,[ClientEmails]=@clientEmails ,[ClientContactNo]=@clientContactNo ,[SupportEmails]=@supportEmails ,[SupportContactNo]=@supportContactNo ,[ModifiedOn]=@modifiedOn,[ModifiedBy]=@modifiedBy WHERE Id=@id";
                docTypes.ModifiedOn = DateTime.UtcNow;
                docTypes.ModifiedBy = userId;
                int rows = db.Execute(query, docTypes);
                return rows > 0 ? GetKeyValue(true, "DocType saved successfully.") : GetKeyValue(false, "Invalid action performed.");
            }
        }

        public int MakeActivateOrDeactivateDocType(int Id, bool isActive, string userId)
        {
            string query = "UPDATE [AM].[DocTypes] SET [IsActive]=@isActive WHERE Id=@id";
            return db.Execute(query, new { isActive = @isActive, @id = @Id });
        }


        public DocTypes FindByIdDocType(int Id)
        {
            string query = "SELECT * FROM [AM].[DocTypes] WHERE Id=@id";
            return db.QuerySingleOrDefault<DocTypes>(query, new { id = Id });
        }

        public List<SFTChannels> GetSFTChannels()
        {
            string query = "SELECT * FROM [Common].[SFTChannels]";
            return db.Query<SFTChannels>(query).ToList();
        }
        public SFTChannels FindByIdSFTChannels(int Id)
        {
            string query = "SELECT * FROM [Common].[SFTChannels] WHERE Id=@id";
            return db.QuerySingleOrDefault<SFTChannels>(query, new { id = Id });
        }
        public KeyValuePair<bool, string> CreateOrEditSFTChannel(SFTChannels sftChannel)
        {
            if (sftChannel.Code.IsNullOrWhiteSpace()) throw new Exception("Code can't be null or empty.");
            if (sftChannel.Title.IsNullOrWhiteSpace()) throw new Exception("Title can't be null or empty.");
            if (sftChannel.Protocol.IsNullOrWhiteSpace()) throw new Exception("Protocol can't be null or empty.");
            if (sftChannel.Host.IsNullOrWhiteSpace()) throw new Exception("Host can't be null or empty.");

            if (sftChannel.Id == 0)
            {
                string query = "INSERT INTO [Common].[SFTChannels] ([Code] ,[Description] ,[Title] ,[Protocol] ,[Host] ,[UserName] ,[Password] ,[ToADEDir] ,[FromADEDir] ,[ArchiveDir] ,[PostDownloadAction] ,[IsActive]) VALUES (@code ,@description ,@title ,@protocol ,@host ,@userName ,@password ,@toADEDir ,@fromADEDir ,@archiveDir ,@postDownloadAction ,@isActive)";
                int rows = db.Execute(query, sftChannel);
                return rows > 0 ? GetKeyValue(true, "SFTChannel saved successfully.") : GetKeyValue(false, "Invalid action performed.");
            }
            else
            {
                string query = "UPDATE [Common].[SFTChannels] SET [Code] = @code ,[Description] =@description ,[Title] = @title ,[Protocol] = @protocol ,[Host] = @host ,[UserName] = @userName ,[Password] = @password ,[ToADEDir] = @toADEDir ,[FromADEDir] = @fromADEDir ,[ArchiveDir] = @archiveDir ,[PostDownloadAction] = @postDownloadAction WHERE [Id]=@id";
                int rows = db.Execute(query, sftChannel);
                return rows > 0 ? GetKeyValue(true, "SFTChannel saved successfully.") : GetKeyValue(false, "Invalid action performed.");
            }
        }

        public int MakeActivateOrDeactivateSFTChannel(int Id, bool isActive)
        {
            string query = "UPDATE [Common].[SFTChannels] SET [IsActive]=@isActive WHERE Id=@id";
            return db.Execute(query, new { isActive = @isActive, @id = @Id });
        }



        public KeyValuePair<bool, string> AssignUserManagerRole(string email)
        {
            try
            {
                //Need to think again
                var roles = db.Query<IdentityRole>("SELECT * FROM [Profile].[Roles] WHERE NAME IN @name", new { name = new List<string>() { AMRoles.ClientUserManager, AMRoles.Client } });

                MailAddress mailAddress = new MailAddress(email);
                var user = db.QuerySingle<ProfileUser>("SELECT * FROM [Profile].[Users] WHERE Email=@email", new { email = email });
                string roleId =
                (mailAddress.Host.Equals("corelogic.com", StringComparison.CurrentCultureIgnoreCase))
                ?
                     roles.Single(r => r.Name == AMRoles.CoreLogicUserManager).Id
                :
                    roles.Single(r => r.Name == AMRoles.ClientUserManager).Id;

                var userRole = db.Execute("select COUNT(UserId) from [Profile].[UserRoles] WHERE UserId=@userId AND RoleId=@roleId", new { userId = user.Id, roleId = roleId });
                if (userRole > 0) return new KeyValuePair<bool, string>(false, "Role already assigned.");

                int rows = db.Execute("INSERT INTO [Profile].[UserRoles] VALUES (@userId,@roleId)", new { userId = user.Id, roleId = roleId });
                if (rows == 1)
                    return new KeyValuePair<bool, string>(true, "Role assigned.");
                else
                    return new KeyValuePair<bool, string>(false, "Role not assigned. Please try again.");
            }
            catch (Exception ex)
            {
                return new KeyValuePair<bool, string>(false, ex.Message);
            }
        }

        public KeyValuePair<bool, string> RemoveUserManagerRole(string userId, string roleId)
        {

            try
            {
                string existRole = db.QuerySingleOrDefault<string>("SELECT Id FROM [Profile].[UserRoles] WHERE UserId=@userId and RoleId=@roleId", new { userId = userId, roleId = roleId });
                if (existRole.IsNullOrWhiteSpace())
                {
                    return new KeyValuePair<bool, string>(false, "Role not assigned to this user.");
                }
                int rows = db.Execute("DELETE FROM [Profile].[UserRoles] WHERE UserId=@userId and RoleId=@roleId", new { userId = userId, roleId = roleId });
                if (rows == 1)
                    return new KeyValuePair<bool, string>(true, "User removed from role.");
                else
                    return new KeyValuePair<bool, string>(false, "User and assigned role not exist.");
            }
            catch (Exception ex)
            {
                return new KeyValuePair<bool, string>(false, ex.Message);
            }
        }
        public List<EmailAllowedDomain> GetUserManagerList()
        {
            var list = new List<EmailAllowedDomain>();
            string query = "with users as ( select u.Email,u.ClientId,u.Id as [UserId],r.Id as [RoleId] from [Profile].[Users] u,[Profile].[UserRoles] ur,[Profile].[Roles] r where u.Id=ur.UserId AND r.Id=ur.RoleId and r.Name =@rolename ), client as ( select * from [Common].[ClientDomains] where ClientId in (select ClientId from users) ), userclient as ( select Email,AllowedDomain,UserId,RoleId from users u,client c where u.ClientId=c.ClientId ) SELECT Email,UserId,RoleId, AllowedDomain = STUFF((SELECT ', ' + AllowedDomain FROM userclient b WHERE b.email = a.email FOR XML PATH('')), 1, 2, '') FROM userclient a GROUP BY Email,UserId,RoleId";
            var nonCLDomain = db.Query<EmailAllowedDomain>(query, new { rolename = AMRoles.ClientUserManager }).ToList();
            if (nonCLDomain != null && nonCLDomain.Count() > 0)
            {
                list.AddRange(nonCLDomain);
            }
            var CLDomain = db.Query<EmailAllowedDomain>("Select Email,u.Id as [UserId],r.Id as [RoleId] from [Profile].[Users] u,[Profile].[UserRoles] ur,[Profile].[Roles] r where u.Id=ur.UserId and ur.RoleId=r.Id and r.Name=@rolename", new { rolename = AMRoles.ClientUserManager }).ToList();

            if (CLDomain != null && CLDomain.Count() > 0)
            {
                CLDomain.ForEach(c => c.AllowedDomain = AMRoles.CoreLogicUserManager);
                list.AddRange(CLDomain);
            }
            return list;
        }

        public string GetEmailsFromClientDomain(string clientEmail)
        {
            var roles = db.Query<IdentityRole>("SELECT * FROM [Profile].[Roles] WHERE NAME IN @name", new { name = new List<string>() { AMRoles.ClientUserManager, AMRoles.CoreLogicUserManager } });
            var mailAddress = new MailAddress(clientEmail);
            var emailList = new List<string>();

            if (mailAddress.Host.Equals("corelogic.com", StringComparison.CurrentCultureIgnoreCase))
            {
                string query = "SELECT Email FROM [Profile].[Users] u,[Profile].[UserRoles] ur WHERE u.Id=ur.UserId AND ur.RoleId=@roleId";
                var emails = db.Query<string>(query, new { roleId = roles.Single(r => r.Name == AMRoles.CoreLogicUserManager).Id }).ToList();
                emailList.AddRange(emails);
            }
            else
            {
                var client = db.QuerySingle<ClientDomains>("SELECT * FROM [Common].[ClientDomains] WHERE AllowedDomain=@domain", new { domain = mailAddress.Host });
                if (client != null)
                {
                    string query = "SELECT Email FROM [Profile].[Users] u,[Profile].[UserRoles] ur WHERE u.Id=ur.UserId AND ur.RoleId=@roleId AND u.ClientId = @clientId";
                    var emails = db.Query<string>(query, new { roleId = roles.Single(r => r.Name == AMRoles.ClientUserManager).Id, clientId = client.ClientId }).ToList();
                    emailList.AddRange(emails);
                }
            }
            if (emailList == null || emailList.Count() == 0)
            {
                return Config.Web_RegistrationDefaultEmails;
            }
            return string.Join(",", emailList);
        }

    }
}
