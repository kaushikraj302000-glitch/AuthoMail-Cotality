using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace AM.Services.Services
{
    public interface IDelfusTemplateService : IDisposable
    {
        Task<KeyValuePair<bool, string>> SaveTemplate(DelfusTemplate delfusTemplate);
        Task<IEnumerable<DelfusTemplate>> DelfusTemplatesByFileName(string fileName);
        Task<IEnumerable<string>> GetDelfusMergeList(string connectionString);
    }
    public class DelfusTemplateService : ServiceBase, IDelfusTemplateService
    {
        public async Task<KeyValuePair<bool, string>> SaveTemplate(DelfusTemplate delfusTemplate)
        {
            try
            {
                string query = @"INSERT INTO Delfus.Templates (FileName ,FilePath ,ModifiedById ,ModifiedOn ,Status)
                    VALUES 
                  (@FileName ,@FilePath ,@ModifiedById ,@ModifiedOn ,@Status)";
                await db.ExecuteAsync(query, delfusTemplate);
                return new KeyValuePair<bool, string>(true, "File uploaded successfully.");
            }
            catch (Exception ex)
            {
                return new KeyValuePair<bool, string>(false, ex.Message);
            }
        }
        public async Task<IEnumerable<DelfusTemplate>> DelfusTemplatesByFileName(string fileName)
        {
            return await db.QueryAsync<DelfusTemplate>(@"
            select t.*,u.DisplayName as ModifiedBy
            from Delfus.Templates t 
            inner join Profile.Users u on t.ModifiedById=u.Id
            where t.FilePath=@FileName order by ModifiedOn desc", new { FileName=fileName });
        }

        public async Task<IEnumerable<string>> GetDelfusMergeList(string connectionString)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                return await db.QueryAsync<string>("SELECT TemplateFieldMapName FROM [Schema].[Request] WHERE RecordType IN ('L','C') AND TemplateFieldMapName IS NOT NULL");
            }
        }

    }
}
