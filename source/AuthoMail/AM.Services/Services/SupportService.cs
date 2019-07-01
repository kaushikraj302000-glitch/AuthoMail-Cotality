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
        List<DocTypeVM> GetDocTypesByClientId(int ClientId);
        List<DocTypeCoverLetterVM> GetDocTypeCoverLetterByDocTypeId(int Id);
        KeyValuePair<bool, string> SaveDocTypeCoverLetter(DocTypeCoverLetter dtcl);
        KeyValuePair<bool, string> ValidateDocTypeCoverLetter(DocTypeCoverLetter dtcl);
        KeyValuePair<bool, string> RemoveCoverletter(int docTypeId, int seqId, string filePath);
        KeyValuePair<bool, string> SaveDocTypePrintTrail(List<DocTypePrintTrailMap> model);
        KeyValuePair<bool, string> SavePrintTrail(PrintTrail model);
        PrintTrail GetPrintTrailById(int Id);
        KeyValuePair<bool, string> RemovePrintTrail(int Id);
        KeyValuePair<bool, string> SimulatePrintTrail(int Id, bool isSimulate);
        List<PrintTrail> GetPrintTrails();
        List<PrintVendor> GetPrintVendors();
        List<DocTypePrintTrailMap> GetDocTypePrintTrail();
        Tuple<DocType, List<DocTypePrintTrailMap>> GetDocTypePrintTrailByDocTypeId(int docTypeId);
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
        TrackingDetailViewModel GetTrackingDetailByTrackingNo(string trackingNo);
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
                var emailClient = db.QuerySingle("SELECT Email,ClientId FROM [Profile].[Users] WHERE Id= @userId", new { userId = userId });
                var mailAddress = new MailAddress(emailClient.Email);
                //if (isCLUserManager)
                //{
                //    string sql = "SELECT Id,UserName,Email,EmailConfirmed,FirstName,LastName,DisplayName,RequestedOn,Status FROM [Profile].[Users] WHERE Status = @status AND EmailConfirmed=1";
                //    return db.Query<UserRequestSignUp>(sql, new { status = AMUserStatus.Open }).ToList();
                //}
                //else
                //{
                var domains = db.Query<string>("SELECT AllowedDomain FROM [Common].[ClientDomains] WHERE ClientId=@clientId", new { clientId = emailClient.ClientId }).ToList();

                string emailQuery = "";
                int i = 0;
                foreach (var item in domains)
                {
                    i++;
                    emailQuery = domains.Count() == i ? string.Format("{0} Email LIKE '%{1}'", emailQuery, item) :
                        string.Format("{0} Email LIKE '%{1}' OR", emailQuery, item);

                }
                string sql = string.Format("SELECT Id,UserName,Email,EmailConfirmed,FirstName,LastName,DisplayName,RequestedOn,Status FROM [Profile].[Users] WHERE Status = @status AND EmailConfirmed=1 AND ({0})", emailQuery);
                return db.Query<UserRequestSignUp>(sql, new { status = AMUserStatus.Open }).ToList();
                //}
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
            if (client.Contact1Name.IsNullOrWhiteSpace()) return GetKeyValue(false, "Contact1 Name is required.");
            if (client.Contact1Phone.IsNullOrWhiteSpace()) return GetKeyValue(false, "Contact1 Phone is required.");
            if (client.Contact1Email.IsNullOrWhiteSpace()) return GetKeyValue(false, "Contact1 Email is required.");
            if (client.ITContactName.IsNullOrWhiteSpace()) return GetKeyValue(false, "ITContact Name is required.");
            if (client.ITContactEmail.IsNullOrWhiteSpace()) return GetKeyValue(false, "ITContact Email is required.");
            if (client.ITContactPhone.IsNullOrWhiteSpace()) return GetKeyValue(false, "ITContact Phone is required.");
            if (client.BillingContactName.IsNullOrWhiteSpace()) return GetKeyValue(false, "BillingContact Name is required.");
            if (client.BillingContactEmail.IsNullOrWhiteSpace()) return GetKeyValue(false, "BillingContact Email is required.");
            if (client.BillingContactPhone.IsNullOrWhiteSpace()) return GetKeyValue(false, "BillingContact Phone is required.");
            if (client.Id == 0)
            {
                int? isExist = db.QuerySingleOrDefault<int>("select 1 from Common.Clients where AppCode=@appCode", new { appCode = client.AppCode });
                if (isExist != null && isExist == 1) return GetKeyValue(true, "Client AppCode already exist.");

                string query = "INSERT INTO [Common].[Clients] ([Name], [AppCode], [BusinessUnit], [IsAMActive], [IsPnSActive], [TimeZone], [ModifiedOn], [CreatedOn], [CreatedBy] ,[ModifiedBy],[Address] ,[Contact1Name] ,[Contact1Phone] ,[Contact1Email] ,[Contact2Name] ,[Contact2Phone] ,[Contact2Email] ,[Contact3Name] ,[Contact3Phone] ,[Contact3Email] ,[ITContactName] ,[ITContactPhone] ,[ITContactEmail] ,[BillingContactName] ,[BillingContactPhone] ,[BillingContactEmail]) VALUES (@name,@appCode,@businessUnit,@isAMActive,@isPnSActive,@timeZone,@modifiedOn,@createdOn,@createdBy,@modifiedBy,@Address ,@Contact1Name ,@Contact1Phone ,@Contact1Email ,@Contact2Name ,@Contact2Phone ,@Contact2Email ,@Contact3Name ,@Contact3Phone ,@Contact3Email ,@ITContactName ,@ITContactPhone ,@ITContactEmail ,@BillingContactName ,@BillingContactPhone ,@BillingContactEmail)";
                client.ModifiedOn = client.CreatedOn = DateTime.UtcNow;
                client.ModifiedBy = client.CreatedBy = userId;
                int result = db.Execute(query, client);
                return result > 0 ? GetKeyValue(true, "Client successfully created.")
                    : GetKeyValue(false, "Invalid action performed.");
            }
            else
            {
                string query = "UPDATE [Common].[Clients] SET Name=@Name,AppCode=@AppCode,BusinessUnit=@BusinessUnit,TimeZone=@TimeZone,ModifiedBy=@ModifiedBy,ModifiedOn=@ModifiedOn,Address=@Address ,Contact1Name=@Contact1Name ,Contact1Phone=@Contact1Phone ,Contact1Email=@Contact1Email ,Contact2Name=@Contact2Name ,Contact2Phone=@Contact2Phone ,Contact2Email=@Contact2Email ,Contact3Name=@Contact3Name ,Contact3Phone=@Contact3Phone ,Contact3Email=@Contact3Email ,ITContactName=@ITContactName ,ITContactPhone=@ITContactPhone ,ITContactEmail=@ITContactEmail ,BillingContactName=@BillingContactName ,BillingContactPhone=@BillingContactPhone ,BillingContactEmail=@BillingContactEmail WHERE Id=@Id";
                client.ModifiedBy = userId;
                client.ModifiedOn = DateTime.UtcNow;
                int result = db.Execute(query, client);
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
            if (docTypes.DocTypeCode.IsNullOrWhiteSpace()) throw new Exception("Doc Type Code can't be null or empty.");
            if (docTypes.Description.IsNullOrWhiteSpace()) throw new Exception("Description can't be null or empty.");
            if (docTypes.PrintSize.IsNullOrWhiteSpace()) throw new Exception("PrintSize can't be null or empty.");
            if (docTypes.ShippingServiceType.IsNullOrWhiteSpace()) throw new Exception("Shipping Service Type can't be null or empty.");
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

            if (!docTypes.BizdayTimeSpan.IsNullOrWhiteSpace() && !IsTimeSpan(docTypes.BizdayTimeSpan)) throw new Exception("Invalid value BizdayTimeSpan.");
            if (!docTypes.SatTimeSpan.IsNullOrWhiteSpace() && !IsTimeSpan(docTypes.SatTimeSpan)) throw new Exception("Invalid value SatTimeSpan.");
            if (!docTypes.SunTimeSpan.IsNullOrWhiteSpace() && !IsTimeSpan(docTypes.SunTimeSpan)) throw new Exception("Invalid value SunTimeSpan.");


            if (docTypes.Id == 0)
            {
                int? isExist = db.QuerySingleOrDefault<int>("select 1 from AM.DocTypes where DocTypeCode=@code", new { code = docTypes.DocTypeCode });
                if (isExist != null && isExist == 1) return GetKeyValue(true, "DocType already exist.");

                string query = "INSERT INTO [AM].[DocTypes] ([DocTypeCode],[Description],[ClientId],[PrintSize],[ContentScalePercent],[PrintFormat],[QCPercent],[ShippingCarrier],[ShippingServiceType],[ShippingAccountCode],[IsReturnInfoSpecified],[RetRcptPrefix],[RetRcptFirstName],[RetRcptMiddleName],[RetRcptLastName],[RetRcptSuffix],[RetRcptPhone],[RetRcptOrganization],[RetRcptAddrOptionalLine1AboveStreet],[RetRcptAddrOptionalLine2AboveStreet],[RetRcptAddressStreet],[RetRcptAddressAptOrSuiteOrOther],[RetRcptAddrOptionalLine1BelowStreet],[RetRcptAddrOptionalLine2BelowStreet],[RetRcptAddressCity],[RetRcptAddressState],[RetRcptAddressZIP],[ClientEmails],[ClientContactNo],[SupportEmails],[SupportContactNo],[IsActive],[ModifiedOn],[CreatedOn],[CreatedBy],[ModifiedBy],[BizdayCutOff],[BizdaySLAType],[BizdayTimeSpan],[SatCutOff],[SatSLAType],[SatTimeSpan],[SunCutOff],[SunSLAType],[SunTimeSpan],[ShippingSpecialService],[IsBRE],[EnvelopeId],[BREEnvelopeId],[NoOfCopies]) VALUES (@docTypeCode,@description,@clientId,@printSize,@contentScalePercent,@printFormat,@qCPercent,@shippingCarrier,@shippingServiceType,@shippingAccountCode,@isReturnInfoSpecified,@retRcptPrefix,@retRcptFirstName,@retRcptMiddleName,@retRcptLastName,@retRcptSuffix,@retRcptPhone,@retRcptOrganization,@retRcptAddrOptionalLine1AboveStreet,@retRcptAddrOptionalLine2AboveStreet,@retRcptAddressStreet,@retRcptAddressAptOrSuiteOrOther,@retRcptAddrOptionalLine1BelowStreet,@retRcptAddrOptionalLine2BelowStreet,@retRcptAddressCity,@retRcptAddressState,@retRcptAddressZIP,@clientEmails,@clientContactNo,@supportEmails,@supportContactNo,@isActive,@modifiedOn,@createdOn,@createdBy,@modifiedBy,@bizdayCutOff,@bizdaySLAType,@bizdayTimeSpan,@satCutOff,@satSLAType,@satTimeSpan,@sunCutOff,@sunSLAType,@sunTimeSpan,@shippingSpecialService,@IsBRE,@EnvelopeId,@BREEnvelopeId,@NoOfCopies)";
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
                string query = "UPDATE [AM].[DocTypes] SET [DocTypeCode]=@docTypeCode ,[Description]=@description ,[ClientId]=@clientId ,[PrintSize]=@printSize ,[ContentScalePercent]=@contentScalePercent ,[PrintFormat]=@printFormat ,[QCPercent]=@qCPercent ,[ShippingCarrier]=@shippingCarrier ,[ShippingServiceType]=@shippingServiceType ,[ShippingAccountCode]=@shippingAccountCode ,[IsReturnInfoSpecified]=@isReturnInfoSpecified ,[RetRcptPrefix]=@retRcptPrefix ,[RetRcptFirstName]=@retRcptFirstName ,[RetRcptMiddleName]=@retRcptMiddleName ,[RetRcptLastName]=@retRcptLastName ,[RetRcptSuffix]=@retRcptSuffix ,[RetRcptPhone]=@retRcptPhone ,[RetRcptOrganization]=@retRcptOrganization ,[RetRcptAddrOptionalLine1AboveStreet]=@retRcptAddrOptionalLine1AboveStreet ,[RetRcptAddrOptionalLine2AboveStreet]=@retRcptAddrOptionalLine2AboveStreet ,[RetRcptAddressStreet]=@retRcptAddressStreet ,[RetRcptAddressAptOrSuiteOrOther]=@retRcptAddressAptOrSuiteOrOther ,[RetRcptAddrOptionalLine1BelowStreet]=@retRcptAddrOptionalLine1BelowStreet ,[RetRcptAddrOptionalLine2BelowStreet]=@retRcptAddrOptionalLine2BelowStreet ,[RetRcptAddressCity]=@retRcptAddressCity ,[RetRcptAddressState]=@retRcptAddressState ,[RetRcptAddressZIP]=@retRcptAddressZIP ,[ClientEmails]=@clientEmails ,[ClientContactNo]=@clientContactNo ,[SupportEmails]=@supportEmails ,[SupportContactNo]=@supportContactNo ,[ModifiedOn]=@modifiedOn,[ModifiedBy]=@modifiedBy,[BizdayCutOff]=@bizdayCutOff,[BizdaySLAType]=@bizdaySLAType,[BizdayTimeSpan]=@bizdayTimeSpan,[SatCutOff]=@satCutOff,[SatSLAType]=@satSLAType,[SatTimeSpan]=@satTimeSpan,[SunCutOff]=@sunCutOff, [SunSLAType]=@sunSLAType,[SunTimeSpan]=@sunTimeSpan,[ShippingSpecialService]=@shippingSpecialService,[IsBRE]=@IsBRE,[EnvelopeId]=@EnvelopeId,[BREEnvelopeId]=@BREEnvelopeId,[NoOfCopies]=@NoOfCopies WHERE Id=@id";
                docTypes.ModifiedOn = DateTime.UtcNow;
                docTypes.ModifiedBy = userId;
                int rows = db.Execute(query, docTypes);
                return rows > 0 ? GetKeyValue(true, "DocType saved successfully.") : GetKeyValue(false, "Invalid action performed.");
            }
        }

        public bool IsTimeSpan(string v)
        {
            try
            {
                TimeSpan.Parse(v);
                return true;
            }
            catch (Exception)
            {
                return false;
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
                MailAddress mailAddress = new MailAddress(email);
                var user = db.QuerySingleOrDefault<ProfileUser>("SELECT * FROM [Profile].[Users] WHERE Email=@email", new { email = email });
                if (user == null) return new KeyValuePair<bool, string>(false, "Email not found.");
                if (user.Status != AMUserStatus.Approved) return new KeyValuePair<bool, string>(false, "Registered email not yet approved.");
                var roles = db.Query<IdentityRole>("SELECT * FROM [Profile].[Roles] WHERE NAME IN @name", new { name = new List<string>() { AMRoles.ClientUserManager, AMRoles.CoreLogicUserManager } });



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
            catch (FormatException)
            {
                return new KeyValuePair<bool, string>(false, "Invalid email address.");
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
                string existRole = db.QuerySingleOrDefault<string>("SELECT UserId FROM [Profile].[UserRoles] WHERE UserId=@userId and RoleId=@roleId", new { userId = userId, roleId = roleId });
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
            var CLDomain = db.Query<EmailAllowedDomain>("Select Email,u.Id as [UserId],r.Id as [RoleId] from [Profile].[Users] u,[Profile].[UserRoles] ur,[Profile].[Roles] r where u.Id=ur.UserId and ur.RoleId=r.Id and r.Name=@rolename", new { rolename = AMRoles.CoreLogicUserManager }).ToList();

            if (CLDomain != null && CLDomain.Count() > 0)
            {
                CLDomain.ForEach(c => c.AllowedDomain = "corelogic.com");
                list.AddRange(CLDomain);
            }
            return list;
        }

        public string GetEmailsFromClientDomain(string clientEmail)
        {
            var roles = db.Query<IdentityRole>("SELECT * FROM [Profile].[Roles] WHERE NAME IN @name", new { name = new List<string>() { AMRoles.ClientUserManager, AMRoles.CoreLogicUserManager } });
            var mailAddress = new MailAddress(clientEmail);
            var emailList = new List<string>();

            //if (mailAddress.Host.Equals("corelogic.com", StringComparison.CurrentCultureIgnoreCase))
            //{
            string query = "SELECT Email FROM [Profile].[Users] u,[Profile].[UserRoles] ur WHERE u.Id=ur.UserId AND ur.RoleId=@roleId";
            var emails = db.Query<string>(query, new { roleId = roles.Single(r => r.Name == AMRoles.CoreLogicUserManager).Id }).ToList();
            emailList.AddRange(emails);
            //}
            //else
            //{
            var client = db.QuerySingle<ClientDomains>("SELECT * FROM [Common].[ClientDomains] WHERE AllowedDomain=@domain", new { domain = mailAddress.Host });
            if (client != null)
            {
                string queryOther = "SELECT Email FROM [Profile].[Users] u,[Profile].[UserRoles] ur WHERE u.Id=ur.UserId AND ur.RoleId=@roleId AND u.ClientId = @clientId";
                var emailsOther = db.Query<string>(queryOther, new { roleId = roles.Single(r => r.Name == AMRoles.ClientUserManager).Id, clientId = client.ClientId }).ToList();
                emailList.AddRange(emailsOther);
            }
            //}
            if (emailList == null || emailList.Count() == 0)
            {
                return Config.Web_RegistrationDefaultEmails;
            }
            return string.Join(",", emailList);
        }



        public TrackingDetailViewModel GetTrackingDetailByTrackingNo(string trackingNo)
        {
            TrackingDetailViewModel model = new TrackingDetailViewModel();
            model.TrackingNo = trackingNo;
            try
            {
                if (trackingNo.IsNullOrWhiteSpace()) throw new Exception("Tracking No can't be null.");
                string query = "SELECT m.[TrackingNo],d.ShippingCarrier,d.ShippingServiceType FROM [AM].[MailPieces] m INNER JOIN [AM].[DocTypes] d ON m.DocTypeId=d.Id WHERE m.TrackingNo=@trackingNo";
                model = db.QuerySingleOrDefault<TrackingDetailViewModel>(query, new { trackingNo = trackingNo });
                if (model == null)
                {
                    model = new TrackingDetailViewModel();
                    model.IsError = true;
                    model.Message = "NotFound";
                }
            }
            catch (System.InvalidOperationException)
            {
                model.IsError = true;
                model.Message = "Tracking Number contain more then one element.";
            }
            catch (Exception ex)
            {
                model.IsError = true;
                model.Message = ex.Message;
            }
            return model;
        }


        public List<DocTypeVM> GetDocTypesByClientId(int ClientId)
        {
            string q = "select d.Id,d.ClientId,d.DocTypeCode,c.Name as ClientName,c.AppCode from AM.DocTypes d,Common.Clients c where d.ClientId=c.Id and ClientId=@clientId";
            return db.Query<DocTypeVM>(q, new { clientId = ClientId }).ToList();
        }
        public List<DocTypeCoverLetterVM> GetDocTypeCoverLetterByDocTypeId(int Id)
        {
            string q = "select * from AM.DocTypeCoverLetters where DocTypeId=@id";
            return db.Query<DocTypeCoverLetterVM>(q, new { id = Id }).ToList();
        }
        public KeyValuePair<bool, string> SaveDocTypeCoverLetter(DocTypeCoverLetter dtcl)
        {
            try
            {
                int rows = db.Execute("insert into AM.DocTypeCoverLetters (DocTypeId,SeqId,CoverLetterTemplatePath) values (@docTypeId,@seqId,@coverLetterTemplatePath)", dtcl);
                if (rows == 1)
                    return GetKeyValue(true, "Saved Successfully.");
                else
                    return GetKeyValue(false, "Data can't be inserted.");
            }
            catch (Exception ex)
            {
                return GetKeyValue(false, ex.Message);
            }
        }

        public KeyValuePair<bool, string> ValidateDocTypeCoverLetter(DocTypeCoverLetter dtcl)
        {
            try
            {
                if (dtcl.CoverLetterTemplatePath.IsNullOrWhiteSpace())
                    return GetKeyValue(false, "Cover Letter Template Path can't be empty.");
                var isExists = db.QueryFirstOrDefault<string>("select CoverLetterTemplatePath from AM.DocTypeCoverLetters where CoverLetterTemplatePath=@filePath", new { filePath = dtcl.CoverLetterTemplatePath });
                if (isExists != null)
                    return GetKeyValue(false, "Cover Letter Template Path already exists.");
                return new KeyValuePair<bool, string>(true, "");
            }
            catch (Exception ex)
            {
                return GetKeyValue(false, ex.Message);
            }
        }
        //private string CalFileName(int docTypeId, int seq,int suffix=1)
        //{
        //    var docType = FindByIdDocType(docTypeId);
        //    var client = FindByClientId(docType.ClientId);
        //    return string.Format("{0}_{1}_{2}_{3}", client.AppCode, docType.ShippingCarrier, seq > 100 ? "EndPage" : "Generic", suffix);
        //}
        public KeyValuePair<bool, string> RemoveCoverletter(int docTypeId, int seqId, string filePath)
        {
            if (filePath.IsNullOrWhiteSpace())
                return GetKeyValue(false, "Cover Letter Template Path can't be empty.");
            int rows = db.Execute("delete from AM.DocTypeCoverLetters where DocTypeId = @docTypeId and SeqId=@seqId and CoverLetterTemplatePath = @coverLetterTemplatePath", new { docTypeId = docTypeId, seqId = seqId, coverLetterTemplatePath = filePath });
            if (rows == 1)
                return GetKeyValue(true, "Removed Successfully.");
            else
                return GetKeyValue(false, "Data can't be removed.");
        }



        public KeyValuePair<bool, string> SaveDocTypePrintTrail(List<DocTypePrintTrailMap> model)
        {
            var result = Has24HrsWithoutOverlap(model);
            if (!result.Key) return result;

            db.Open();
            var t = db.BeginTransaction();
            try
            {
                int docTypeId = model.Select(m => m.DocTypeId).First();
                db.Execute("delete from Common.DocTypePrintTrailMaps where doctypeid=@doctypeid", new { doctypeid = docTypeId }, transaction: t);

                string q = @"INSERT INTO Common.DocTypePrintTrailMaps (DocTypeId ,PrintTrailId ,BizDayStartTime ,BizDayEndTime ,SatStartTime ,SatEndTime ,SunStartTime ,SunEndTime) VALUES (@DocTypeId ,@PrintTrailId ,@BizDayStartTime ,@BizDayEndTime ,@SatStartTime ,@SatEndTime ,@SunStartTime ,@SunEndTime)";
                db.Execute(q, model, transaction: t);
                t.Commit();
                return new KeyValuePair<bool, string>(true, "DocType & Print Trail mapped successfully.");
            }
            catch (Exception ex)
            {
                t.Rollback();
                return new KeyValuePair<bool, string>(false, ex.Message);
            }
        }


        public KeyValuePair<bool, string> SavePrintTrail(PrintTrail model)
        {
            try
            {
                string q = model.Id == 0 ?
                    @"INSERT INTO Common.PrintTrails (TrailName ,PrintVendorId ,SimulateTrail) VALUES (@TrailName ,@PrintVendorId ,@SimulateTrail)" :
                    "UPDATE Common.PrintTrails SET TrailName = @TrailName,PrintVendorId = @PrintVendorId WHERE Id=@Id";
                db.Execute(q, model);
                return new KeyValuePair<bool, string>(true, "Print Trail saved successfully.");
            }
            catch (Exception ex)
            {
                return new KeyValuePair<bool, string>(false, ex.Message);
            }
        }
        public KeyValuePair<bool, string> RemovePrintTrail(int Id)
        {
            try
            {
                bool isExist = db.QuerySingle<bool>("select 1 from Common.DocTypePrintTrailMaps where PrintTrailId=@Id", new { Id = Id });
                if (isExist) new KeyValuePair<bool, string>(false, "Print trail mapped into doctype.");

                isExist = db.QuerySingle<bool>("select 1 from PnS.PrintTrailBatches where PrintTrailId=@Id", new { Id = Id });
                if (isExist) new KeyValuePair<bool, string>(false, "Print trail mapped into print trail batches.");

                db.Execute("delete from Common.PrintTrails WHERE Id=@Id", new { Id = Id });
                return new KeyValuePair<bool, string>(true, "Print Trail Removed.");
            }
            catch (Exception ex)
            {
                return new KeyValuePair<bool, string>(false, ex.Message);
            }
        }
        private KeyValuePair<bool, string> Has24HrsWithoutOverlap(List<DocTypePrintTrailMap> TSList)
        {
            if (TSList == null || TSList.Count == 0)
                return new KeyValuePair<bool, string>(false, "All times can't be null.");

            string biz = "BizDay times are either overlap or doesn't contain a full day.";
            string sat = "Saturday times are either overlap or doesn't contain a full day.";
            string sun = "Sunday times are either overlap or doesn't contain a full day.";
            double hrs = 0.0;

            if (TSList.Any(t => t.BizDayStartTime == null || t.BizDayEndTime == null) && TSList.Any(t => t.BizDayStartTime != null || t.BizDayEndTime != null))
                return new KeyValuePair<bool, string>(false, biz);
            if (TSList.Any(t => t.SatStartTime == null || t.SatEndTime == null) && TSList.Any(t => t.SatStartTime != null || t.SatEndTime != null))
                return new KeyValuePair<bool, string>(false, sat);
            if (TSList.Any(t => t.SunStartTime == null || t.SunEndTime == null) && TSList.Any(t => t.SunStartTime != null || t.SunEndTime != null))
                return new KeyValuePair<bool, string>(false, sun);

            bool IsAllTimeNull = true;

            TSList.ForEach(t =>
            {
                if (t.BizDayStartTime.HasValue && t.BizDayEndTime.HasValue) hrs += t.BizDayEndTime.Value.Subtract(t.BizDayStartTime.Value).TotalSeconds;
            });
            IsAllTimeNull = IsAllTimeNull && hrs == 0.0 ? true : false;
            if (!(TSList.Count(t => !t.BizDayEndTime.HasValue && !t.BizDayStartTime.HasValue) == TSList.Count) && (hrs + TSList.Count) != 86400)
                return new KeyValuePair<bool, string>(false, biz);

            hrs = 0.0;
            TSList.ForEach(t =>
            {
                if (t.SatStartTime.HasValue && t.SatEndTime.HasValue) hrs += t.SatEndTime.Value.Subtract(t.SatStartTime.Value).TotalSeconds;
            });
            IsAllTimeNull = IsAllTimeNull && hrs == 0.0 ? true : false;
            if (!(TSList.Count(t => !t.SatStartTime.HasValue && !t.SatEndTime.HasValue) == TSList.Count) && (hrs + TSList.Count) != 86400)
                return new KeyValuePair<bool, string>(false, sat);

            hrs = 0.0;
            TSList.ForEach(t =>
            {
                if (t.SunStartTime.HasValue && t.SunEndTime.HasValue) hrs += t.SunEndTime.Value.Subtract(t.SunStartTime.Value).TotalSeconds;
            });
            IsAllTimeNull = IsAllTimeNull && hrs == 0.0 ? true : false;

            if (!(TSList.Count(t => !t.SunEndTime.HasValue && !t.SunStartTime.HasValue) == TSList.Count) && (hrs + TSList.Count) != 86400)
                return new KeyValuePair<bool, string>(false, sun);

            if (IsAllTimeNull)
                return new KeyValuePair<bool, string>(false, "All times can't be null.");


            for (int i = 0; i < TSList.Count; i++)
            {
                for (int j = 0; j < TSList.Count; j++)
                {
                    if (i != j)
                    {
                        if ((TSList[j].BizDayStartTime.HasValue && TSList[j].BizDayEndTime.HasValue) && (TSList[j].BizDayStartTime <= TSList[i].BizDayStartTime && TSList[i].BizDayStartTime <= TSList[j].BizDayStartTime) || (TSList[j].BizDayStartTime <= TSList[i].BizDayEndTime && TSList[i].BizDayEndTime <= TSList[j].BizDayStartTime))
                            return new KeyValuePair<bool, string>(false, biz);
                        if ((TSList[j].SatStartTime.HasValue && TSList[j].SatEndTime.HasValue) && (TSList[j].SatStartTime <= TSList[i].SatStartTime && TSList[i].SatStartTime <= TSList[j].SatStartTime) || (TSList[j].SatStartTime <= TSList[i].SatEndTime && TSList[i].SatEndTime <= TSList[j].SatStartTime))
                            return new KeyValuePair<bool, string>(false, sat);
                        if ((TSList[j].SunStartTime.HasValue && TSList[j].SunEndTime.HasValue) && (TSList[j].SunStartTime <= TSList[i].SunStartTime && TSList[i].SunStartTime <= TSList[j].SunStartTime) || (TSList[j].SunStartTime <= TSList[i].SunEndTime && TSList[i].SunEndTime <= TSList[j].SunStartTime))
                            return new KeyValuePair<bool, string>(false, sun);
                    }
                }
            }

            return new KeyValuePair<bool, string>(true, "");
        }


        public List<PrintTrail> GetPrintTrails()
        {
            var model = db.Query<PrintTrail>("select pt.Id,pt.TrailName,pt.PrintVendorId,pt.SimulateTrail,pv.Name as PrintVendor from Common.PrintTrails pt inner join Common.PrintVendors pv on pt.PrintVendorId=pv.Id").ToList();
            return model;
        }

        public List<DocTypePrintTrailMap> GetDocTypePrintTrail()
        {
            var model = db.Query<DocTypePrintTrailMap>("select * from Common.DocTypePrintTrailMaps").ToList();
            return model;
        }


        public Tuple<DocType, List<DocTypePrintTrailMap>> GetDocTypePrintTrailByDocTypeId(int docTypeId)
        {
            var model = db.Query<DocTypePrintTrailMap>("select * from Common.DocTypePrintTrailMaps where doctypeid=@doctypeid", new { doctypeid = docTypeId }).ToList();
            var docType = db.QuerySingle<DocType>("select * from AM.DocTypes where id=@doctypeid", new { doctypeid = docTypeId });
            return Tuple.Create<DocType, List<DocTypePrintTrailMap>>(docType, model);
        }


        public List<PrintVendor> GetPrintVendors()
        {
            return db.Query<PrintVendor>("select * from common.PrintVendors").ToList();
        }


        public KeyValuePair<bool, string> SimulatePrintTrail(int Id, bool isSimulate)
        {
            try
            {
                db.Execute("update Common.PrintTrails set SimulateTrail=@SimulateTrail where Id=@id", new { SimulateTrail = isSimulate, id = @Id });
                return new KeyValuePair<bool, string>(true, "Simulate successfully.");
            }
            catch (Exception ex)
            {
                return new KeyValuePair<bool, string>(false, ex.Message);
            }
        }


        public PrintTrail GetPrintTrailById(int Id)
        {
            return db.QuerySingle<PrintTrail>("select * from Common.PrintTrails where Id=@Id", new { Id = Id });
        }
    }
}
