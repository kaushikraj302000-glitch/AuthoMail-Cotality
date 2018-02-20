using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace AM.Services
{
    public interface ICommonService
    {
        int UpdateBatchPipeLineStatus(int batchClustId, string pipeLineStatus, DateTime timestamp);
        int UpdateBatchStatus(int batchClustId, string batchStatus);
    }
    public class CommonService : ServiceBase, ICommonService //It implements Interfaces 
    {
        public int UpdateBatchPipeLineStatus(int batchClustId, string pipeLineStatus, DateTime timestamp)
        {
            string query = "UPDATE [AM].[Batch] SET PipelineStatus = @pipelineStatus, {0}On = @timestamp WHERE BatchClustId = @batchClustId".Fill(pipeLineStatus);

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@pipelineStatus", pipeLineStatus);
            parameters.Add("@timestamp", timestamp);
            parameters.Add("@batchClustId", batchClustId);

            return db.Execute(query, parameters);
        }

        public int UpdateBatchStatus(int batchClustId, string batchStatus)
        {
            string query = "UPDATE [AM].[Batch] SET BatchStatus = @batchStatus WHERE BatchClustId = @batchClustId";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@batchStatus", batchStatus);
            parameters.Add("@batchClustId", batchClustId);

            return db.Execute(query, parameters);
        }
    }
}
