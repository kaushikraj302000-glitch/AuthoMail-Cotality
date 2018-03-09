using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AM.Entities;

namespace AM.Services
{
    public interface IBatchService
    {
        int InsertBatchDetails(MailBatchDTO mailBatch);
        int DeleteBatchDetails(int mailBatchId);
        int UpdateBatchDetails(MailBatchDTO mailBatch);
        List<MailBatchDTO> GetBatchesByStatus(string batchPipelineStatus, string batchStatus = null);
        BatchData GetMailBatchDetailsByBatchId(int mailBatchId);
    }
    public class BatchService : ServiceBase, IBatchService //It implements Interfaces 
    {
        public int InsertBatchDetails(MailBatchDTO mailBatch)
        {
            //Sql Query
            string query = "INSERT INTO [AM].[MailBatches](UniqueId, ClientId, SFTChannelId, ClientBatchId, BatchName, DocumentOrPackageType, ReceivedOn, DownloadedOn, ValidatedOn, AcknowledgedOn, PreppedOn, SubmittedToShippingOn, ShippingResponseReceivedOn,  ReportPreppedOn, ReportedBackOn, Priority, BatchStatus, ValidationStatus, ValidationErrors, PipelineStatus, IsReturnInfoSpecified, RetRcptPrefix, RetRcptFirstName, RetRcptMiddleName, RetRcptLastName, RetRcptSuffix, RetRcptPhone, RetRcptOrganization, RetRcptAddrOptionalLine1AboveStreet, RetRcptAddrOptionalLine2AboveStreet, RetRcptAddressStreet, RetRcptAddressAptOrSuiteOrOther, RetRcptAddrOptionalLine1BelowStreet, RetRcptAddrOptionalLine2BelowStreet, RetRcptAddressCity, RetRcptAddressState, RetRcptAddressZIP, PrintLocation) Output Inserted.Id VALUES (@UniqueId, @ClientId, @SFTChannelId, @ClientBatchId, @BatchName, @DocumentOrPackageType, @ReceivedOn, @DownloadedOn, @ValidatedOn, @AcknowledgedOn, @PreppedOn, @SubmittedToShippingOn, @ShippingResponseReceivedOn, @ReportPreppedOn, @ReportedBackOn, @Priority, @BatchStatus, @ValidationStatus, @ValidationErrors, @PipelineStatus, @IsReturnInfoSpecified, @RetRcptPrefix, @RetRcptFirstName, @RetRcptMiddleName, @RetRcptLastName, @RetRcptSuffix, @RetRcptPhone, @RetRcptOrganization, @RetRcptAddrOptionalLine1AboveStreet, @RetRcptAddrOptionalLine2AboveStreet, @RetRcptAddressStreet, @RetRcptAddressAptOrSuiteOrOther, @RetRcptAddrOptionalLine1BelowStreet, @RetRcptAddrOptionalLine2BelowStreet, @RetRcptAddressCity, @RetRcptAddressState, @RetRcptAddressZIP, @PrintLocation)";

            var data = db.ExecuteScalar(query, mailBatch);
            return Convert.ToInt32(data);
        }

        public int DeleteBatchDetails(int mailBatchId)
        {
            //Sql Query
            string query = @"DELETE FROM [AM].[LogBatchTimeLine] WHERE MailBatchId=@Id
                             DELETE FROM [AM].[MailBatchCustomInfo] WHERE MailBatchId=@Id
                             DELETE FROM [AM].[MailBatches] WHERE Id=@Id;";

            var data = db.Execute(query, new { Id = mailBatchId });
            return data;
        }

        public int UpdateBatchDetails(MailBatchDTO mailBatch)
        {
            string query = "UPDATE [AM].[MailBatches] SET [UniqueId] = @UniqueId,[ClientId] = @ClientId,[DocumentOrPackageType] = @DocumentOrPackageType,[BatchName] = @BatchName ,[ValidationStatus] = @ValidationStatus ,[ValidationErrors] = @ValidationErrors,[ClientBatchId] = @ClientBatchId ,[IsReturnInfoSpecified] = IsReturnInfoSpecified,[RetRcptPrefix] = @RetRcptPrefix ,[RetRcptFirstName] =@RetRcptFirstName ,[RetRcptMiddleName] = @RetRcptMiddleName ,[RetRcptLastName] = @RetRcptLastName ,[RetRcptSuffix] = @RetRcptSuffix ,[RetRcptPhone] = @RetRcptPhone ,[RetRcptOrganization] = @RetRcptOrganization ,[RetRcptAddrOptionalLine1AboveStreet] = @RetRcptAddrOptionalLine1AboveStreet ,[RetRcptAddrOptionalLine2AboveStreet] = @RetRcptAddrOptionalLine2AboveStreet ,[RetRcptAddressStreet] = @RetRcptAddressStreet ,[RetRcptAddressAptOrSuiteOrOther] = @RetRcptAddressAptOrSuiteOrOther ,[RetRcptAddrOptionalLine1BelowStreet] = RetRcptAddrOptionalLine1BelowStreet ,[RetRcptAddrOptionalLine2BelowStreet] = RetRcptAddrOptionalLine2BelowStreet ,[RetRcptAddressCity] = @RetRcptAddressCity ,[RetRcptAddressState] = @RetRcptAddressState ,[RetRcptAddressZIP] = @RetRcptAddressZIP ,[Priority] = @Priority ,[SFTChannelId] = @SFTChannelId WHERE Id=@Id";

            var data = db.ExecuteScalar(query, mailBatch);
            return Convert.ToInt32(data);
        }

        public List<MailBatchDTO> GetBatchesByStatus(string batchPipelineStatus, string batchStatus)
        {
            //Sql Query
            string query = "SELECT * FROM [AM].[MailBatches] WHERE PipelineStatus= @batchPipelineStatus ";

            switch (batchPipelineStatus)
            {
                case BatchPipelineStatus.Downloaded: query += " AND IsValidated=0";
                    break;
                case BatchPipelineStatus.Validated: query += " AND IsAcknowledged=0";
                    break;
                case BatchPipelineStatus.Acknowledged: query += " AND IsPrepped=0";
                    break;
                case BatchPipelineStatus.Prepped: query += " AND IsSubmittedToShipping=0";
                    break;
                case BatchPipelineStatus.SubmittedToShipping: query += " AND IsShippingResponseReceived=0";
                    break;
                case BatchPipelineStatus.ShippingResponseReceived: query += " AND IsReportPrepped=0";
                    break;
                case BatchPipelineStatus.ReportPrepped: query += " AND IsReportedBack=0";
                    break;
                default:
                    break;
            }

            //Assign value to parameters It may one or more. Note:
            //List of parameters declaration
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@batchPipelineStatus", batchPipelineStatus);

            if (!string.IsNullOrEmpty(batchStatus))
            {
                parameters.Add("@batchStatus", batchStatus);
                query += " AND BatchStatus=@batchStatus";
            }

            /* Execution of query with parateters.
              Without Dapper you can't create following code. So just Add "using Dapper" namespace at begin.
            */
            var data = db.Query<MailBatchDTO>(query, parameters).ToList();
            return data;
        }

        public BatchData GetMailBatchDetailsByBatchId(int mailBatchId)
        {
            BatchData data = null;
            string query = @"SELECT * FROM [AM].[MailBatches] WHERE Id=@mailBatchId 
                            SELECT * FROM [AM].[MailBatchCustomInfo] WHERE MailBatchId=@mailBatchId
                            SELECT * FROM [AM].[MailPieces] WHERE MailBatchId=@mailBatchId
                            SELECT * FROM [AM].[MailPieceCustomInfo] WHERE MailPieceId IN (SELECT Id FROM [AM].[MailPieces] WHERE MailBatchId=@mailBatchId)";

            using (var multi = db.QueryMultiple(query, new { mailBatchId = mailBatchId }))
            {
                if (multi != null)
                {
                    data = new BatchData();
                    data.MailBatch = multi.Read<MailBatchDTO>().Single();
                    data.MailBatchCustomInfo = multi.Read<MailBatchCustomInfo>().ToList();
                    data.MailPieces = multi.Read<MailPieceDTO>().ToList();
                    data.MailPieceCustomInfo = multi.Read<MailPieceCustomInfo>().ToList();
                }
            }

            return data;
        }
    }
}
