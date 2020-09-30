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
        Task<IEnumerable<string>> GetDelfusMergeList();
        Task<IEnumerable<DelfusTemplate>> DelfusTemplateSummary();
    }
    public class DelfusTemplateService : DelfusServiceBase, IDelfusTemplateService
    {
        public async Task<KeyValuePair<bool, string>> SaveTemplate(DelfusTemplate delfusTemplate)
        {
            try
            {
                string query = @"INSERT INTO Templates (FileName ,FilePath ,ModifiedBy ,ModifiedOn ,Status, Reason)
                    VALUES 
                  (@FileName ,@FilePath ,@ModifiedBy ,@ModifiedOn ,@Status, @Reason)";
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
            return await db.QueryAsync<DelfusTemplate>(@"select * from Templates
                    where FilePath=@FileName order by ModifiedOn desc", new { FileName = fileName });
        }

        public async Task<IEnumerable<string>> GetDelfusMergeList()
        {
             return await db.QueryAsync<string>("SELECT TemplateFieldMapName FROM [Schema].[Request] WHERE RecordType IN ('L','C') AND TemplateFieldMapName IS NOT NULL");
           
        }
        public async Task<IEnumerable<DelfusTemplate>> DelfusTemplateSummary()
        {
            return await db.QueryAsync<DelfusTemplate>(@"
            with cte as
(
	select max(Id) as Id from Templates
	group by FileName
)

         select * from Templates  
			where Id in (select Id from cte)
            order by ModifiedOn desc");
        }

    }
}
