using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AM.Entities;

namespace AM.Services
{
    public interface IPendingBatchesService : IDisposable
    {
        List<MailBatches> GetPendingBatches(int clientId);
        List<MailBatches> GetPendingBatchFromPnS(int clientId);
    }
    public class PendingBatchesService : ServiceBase, IPendingBatchesService
    {
        public List<MailBatches> GetPendingBatches(int clientId)
        {
            //Id,UniqueId,ClientId,ClientBatchId,ReceivedOn,BatchStatus,PipelineStatus
            string query = "SELECT * FROM [AM].[MailBatches] WHERE [ClientId]=@clientId AND ([BatchStatus] = @batchStatus OR [PipelineStatus] <> @pipelineStatus)";

            var data = db.Query<MailBatches>(query, new { clientId = clientId, batchStatus = BatchStatus.InProgress.ToString(), pipelineStatus = BatchPipelineStatus.ReportedBack.ToString() }).ToList();
            return data;
        }


        public List<MailBatches> GetPendingBatchFromPnS(int clientId)
        {
            string query = "select * from [AM].[MailBatches] where [BatchStatus] =@batchStatus AND Id in (select MailBatchId from [AM].[PrintBatches] where Id in ( select PrintBatch from [PnS].[BatchSummary] where PipelineStatus='Submitted'))";

            var data = db.Query<MailBatches>(query, new { clientId = clientId, batchStatus = BatchStatus.InProgress.ToString() }).ToList();
            return data;
        }
    }
}
