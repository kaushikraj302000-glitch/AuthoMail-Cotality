using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Entities;
using Dapper;

namespace AM.Services
{
    public interface IClientPermissionService : IDisposable
    {
        Task<IEnumerable<ClientIdNameVM>> GetClientDomainByDomainName(string domainName);
        Task InsertClientIdAndUserId(ClientUserMapping clientUserMapping);
        Task<IEnumerable<ClientUserMapping>> ApproveOrDeclinedClientsForUser(string userId, List<int> approveclientIds, string modifiedById);
        Task<IEnumerable<ClientUserMapping>> GetClientDomainByUserId(string userId);
        Task<bool> IsUserClientRequestPending(string userId);
    }
    public class ClientPermissionService : ServiceBase, IClientPermissionService
    {
        public async Task<IEnumerable<ClientUserMapping>> ApproveOrDeclinedClientsForUser(string userId, List<int> approveclientIds, string modifiedById)
        {
            //Approve client request
            string sql = @"update Common.ClientUserMapping
set Status=@status,
ModifiedOn=@now,
ModifiedById=@ModifiedById
where UserId=@userId and ClientId in @clientIds";
            await db.ExecuteAsync(sql, new
            {
                userId = userId,
                status = ClientUserMappingStatusTypes.Approved,
                ModifiedById = modifiedById,
                now = DateTime.UtcNow,
                clientIds = approveclientIds
            });
            //Decline client request
            sql = @"update Common.ClientUserMapping
set Status=@status,
ModifiedOn=@now,
ModifiedById=@ModifiedById
where UserId=@userId and Status=@pendingStatus";
            await db.ExecuteAsync(sql, new
            {
                userId = userId,
                status = ClientUserMappingStatusTypes.Declined,
                ModifiedById = modifiedById,
                now = DateTime.UtcNow,
                pendingStatus = ClientUserMappingStatusTypes.Pending
            });
            return await GetClientDomainByUserId(userId);
        }

        public async Task<IEnumerable<ClientUserMapping>> GetClientDomainByUserId(string userId)
        {
            string sql = @"select * from Common.ClientUserMapping where UserId=@userId";
            return await db.QueryAsync<ClientUserMapping>(sql, userId);
        }
        public async Task<IEnumerable<ClientIdNameVM>> GetClientDomainByDomainName(string domainName)
        {
            string sql = @"SELECT c.Id,c.Name,c.AppCode FROM Common.ClientDomains cd
inner join Common.Clients c on cd.ClientId=c.Id
where AllowedDomain=@domainName";
            var output = await db.QueryAsync<ClientIdNameVM>(sql, domainName);
            return output;
        }

        public async Task InsertClientIdAndUserId(ClientUserMapping clientUserMapping)
        {
            string isExistSql = "select 1 from Common.ClientUserMapping where UserId=@userId and ClientId=@clientId";
            int? isExist = await db.QuerySingleOrDefaultAsync<int?>(isExistSql, new
            {
                userId = clientUserMapping.UserId,
                clientId = clientUserMapping.ClientId
            });
            if (!isExist.HasValue)
            {
                string sql = @"insert into Common.ClientUserMapping (ClientId,UserId,Status,CreatedOn,ModifiedOn,ModifiedById) 
values(@ClientId,@UserId,@Status,@CreatedOn,@ModifiedOn,@ModifiedById)";
                clientUserMapping.CreatedOn = clientUserMapping.ModifiedOn = DateTime.UtcNow;
                await db.ExecuteAsync(sql, clientUserMapping);
            }
        }

        public async Task<bool> IsUserClientRequestPending(string userId)
        {
            string sql = "select 1 from Common.ClientUserMapping where UserId=@userId";
            int? isExist = await db.QuerySingleOrDefaultAsync<int?>(sql, userId);
            return isExist.HasValue;
        }
    }
}
