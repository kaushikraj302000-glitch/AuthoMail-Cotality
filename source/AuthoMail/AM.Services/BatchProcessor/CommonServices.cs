using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using AM.Lib;
using AM.Entities;

namespace AM.Services
{
    public interface ICommonService : IDisposable
    {
        int UpdateBatchPipeLineStatus(int batchId, string pipeLineStatus, DateTime timestamp);
        int UpdateBatchStatus(int batchId, string batchStatus);
        int AMLogBatchTimeLine(int batchId, string activityName, DateTime timeStamp);
        int UpdatePrintBatchStatus(int printBatchId, string batchStatus, string statusDescription = null);
        int UpdatePrintBatchPipeLineStatus(int printBatchId, string pipeLineStatus, DateTime timestamp);
        ClientBusinessIdentifier GetBusinessIdNameFromClientId(int clientId);
        int InsertBusinessIdentifierName(ClientBusinessIdentifier clientBusinessId);
    }
    public class CommonService : ServiceBase, ICommonService //It implements Interfaces 
    {
        public int UpdateBatchPipeLineStatus(int batchId, string pipeLineStatus, DateTime timestamp)
        {
            string query = "UPDATE [AM].[MailBatches] SET PipelineStatus = @pipelineStatus, {0}On = @timestamp WHERE Id = @batchId".Fill(pipeLineStatus);

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@pipelineStatus", pipeLineStatus);
            parameters.Add("@timestamp", timestamp);
            parameters.Add("@batchId", batchId);

            return db.Execute(query, parameters);
        }

        public int UpdateBatchStatus(int batchId, string batchStatus)
        {
            string query = "UPDATE [AM].[MailBatches] SET BatchStatus = @batchStatus WHERE Id = @batchId";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@batchStatus", batchStatus);
            parameters.Add("@batchId", batchId);

            return db.Execute(query, parameters);
        }

        public int AMLogBatchTimeLine(int batchId, string activityName, DateTime timeStamp)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@activityName", activityName);
            parameters.Add("@batchId", batchId);
            parameters.Add("@timeStamp", timeStamp);

            return db.Execute("usp_AMLogBatchTimeLine", parameters, commandType: CommandType.StoredProcedure);
        }

        public int UpdatePrintBatchStatus(int printBatchId, string batchStatus, string statusDescription = null)
        {
            string query = "UPDATE [AM].[PrintBatches] SET BatchStatus = @batchStatus,StatusDescription=@statusDescription WHERE Id = @printBatchId";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@batchStatus", batchStatus);
            parameters.Add("@printBatchId", printBatchId);
            parameters.Add("@statusDescription", statusDescription);

            return db.Execute(query, parameters);
        }

        public int UpdatePrintBatchPipeLineStatus(int printBatchId, string pipeLineStatus, DateTime timestamp)
        {
            string query = "UPDATE [AM].[PrintBatches] SET {0}On = @timestamp WHERE Id = @printBatchId".Fill(pipeLineStatus);

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@timestamp", timestamp);
            parameters.Add("@printBatchId", printBatchId);

            return db.Execute(query, parameters);
        }

        public ClientBusinessIdentifier GetBusinessIdNameFromClientId(int clientId)
        {
            string query = "SELECT * FROM [AM].[ClientBusinessIdentifiers] WHERE ClientId= @clientId ";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@clientId", clientId);

            return db.Query<ClientBusinessIdentifier>(query, parameters).SingleOrDefault();
        }

        public int InsertBusinessIdentifierName(ClientBusinessIdentifier clientBusinessId)
        {
            string query = "INSERT INTO [AM].[ClientBusinessIdentifiers] ( [ClientId], [BusinessIdName] ) VALUES (@ClientId, @BusinessIdName)";

            return db.Execute(query, clientBusinessId);
        }
    }
}
