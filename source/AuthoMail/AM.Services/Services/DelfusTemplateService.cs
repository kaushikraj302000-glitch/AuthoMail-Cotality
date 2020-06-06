using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace AM.Services.Services
{
    public interface IDelfusTemplateService : IDisposable
    {
        Task<KeyValuePair<bool, string>> SaveTemplate(DelfusTemplate delfusTemplate);
        Task<IEnumerable<DelfusTemplate>> DelfusTemplates();
        Task<DelfusTemplate> DelfusTemplateById(int id);
        Task RemoveDelfusTemplateById(int id, string userId);
    }
    public class DelfusTemplateService : ServiceBase, IDelfusTemplateService
    {
        public async Task<KeyValuePair<bool, string>> SaveTemplate(DelfusTemplate delfusTemplate)
        {
            try
            {
                delfusTemplate.UploadedOn = DateTime.UtcNow;
                string query = @"INSERT INTO Delfus.Templates (FileName ,FilePath ,UploadedById ,UploadedOn ,IsRemoved ,RemovedById ,RemovedOn)
                    VALUES 
                  (@FileName ,@FilePath ,@UploadedById ,@UploadedOn ,@IsRemoved ,@RemovedById ,@RemovedOn)";
                await db.ExecuteAsync(query, delfusTemplate);
                return new KeyValuePair<bool, string>(true, "File uploaded successfully.");
            }
            catch (Exception ex)
            {
                return new KeyValuePair<bool, string>(false, ex.Message);
            }
        }
        public async Task<IEnumerable<DelfusTemplate>> DelfusTemplates()
        {
            return await db.QueryAsync<DelfusTemplate>(@"
            select t.*,u.DisplayName as UploadedBy,r.DisplayName as RemovedBy
            from Delfus.Templates t
            inner join Profile.Users u on t.UploadedById=u.Id
            left outer join Profile.Users r on t.RemovedById=r.Id");
        }

        public async Task<DelfusTemplate> DelfusTemplateById(int id)
        {
            return await db.QuerySingleAsync<DelfusTemplate>(@"
            select t.*,u.DisplayName as UploadedBy,r.DisplayName as RemovedBy
            from Delfus.Templates t
            inner join Profile.Users u on t.UploadedById=u.Id
            left outer join Profile.Users r on t.RemovedById=r.Id where t.Id=@Id", new { Id = id });
        }

        public async Task RemoveDelfusTemplateById(int id, string userId)
        {
            await db.ExecuteAsync(@"update delfus.Templates
            set IsRemoved=@IsRemoved, RemovedById=@RemovedById, RemovedOn=@RemovedOn
            where Id=@Id", new { id = id, IsRemoved = true, RemovedById = userId, RemovedOn = DateTime.UtcNow });
        }
    }
}
