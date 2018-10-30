using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AM.Entities;
using AM.Lib;

namespace AM.Services
{
    public interface IMailBatchService : IDisposable
    {
        int InsertBatchDetails(MailBatchDTO mailBatch);
        List<MailBatchDTO> InsertBatchDetails(List<MailBatchDTO> mailBatches);
        int DeleteBatchDetails(int mailBatchId);
        int UpdateBatchDetails(MailBatchDTO mailBatch);
        int MarkMailBatchAsInvalid(int id, string errorDesc);
        List<MailBatchDTO> GetBatchesByPipelineStatusAndClientId(string batchPipelineStatus, int? clientId = null);
        List<MailBatchDTO> GetBatchesByPipelineStatusAndSFTChannelId(string batchPipelineStatus, int? sftChannelId = null);
        BatchData GetMailBatchDetailsByBatchId(int mailBatchId);
        MailBatchDTO GetMailBatchDetailByBatchId(int mailBatchId);
        int InsertPrintBatchDetails(PrintBatch printBatch);
        List<SubmittedBatch> GetSubmittedPrintBatchDetails();
        List<SubmittedBatch> GetResponseReceivedPrintBatchDetails();
        List<SubmittedBatch> GetReportPreppedPrintBatchDetails();
        int GetMailBatchesReceivedCount(DateTime startTime, DateTime endTime, string status);
        int UpdatePrintBatchPipelineStatus(int printBatchId, string pipeLineStatus, DateTime timestamp);
        bool GetReportedBackPrintBatchesFromMailBatchId(int mailBatchId);
    }
    public class MailBatchService : ServiceBase, IMailBatchService //It implements Interfaces 
    {
        public int InsertBatchDetails(MailBatchDTO mailBatch)
        {
            string query = @"INSERT INTO [AM].[MailBatches](UniqueId, ClientId, DocumentOrPackageType, SFTChannelId, ClientBatchId, BatchName, [Priority]) Output Inserted.Id VALUES (@UniqueId, @ClientId, @DocumentOrPackageType, @SFTChannelId, @ClientBatchId, @BatchName, @Priority)";

            return db.ExecuteScalar<int>(query, mailBatch);
        }

        public List<MailBatchDTO> InsertBatchDetails(List<MailBatchDTO> mailBatches)
        {
            //Insert batch record, mark batch as received and inprogress
            foreach (var mailBatch in mailBatches)
            {
                string query = @"INSERT INTO [AM].[MailBatches](UniqueId, ClientId, DocumentOrPackageType, SFTChannelId, ClientBatchId, BatchName, [Priority],[ReceivedOn], [PipelineStatus], [BatchStatus]) Output Inserted.Id VALUES (@UniqueId, @ClientId, @DocumentOrPackageType, @SFTChannelId, @ClientBatchId, @BatchName, @Priority, @ReceivedOn, @PipelineStatus, @BatchStatus)";

                mailBatch.Id = db.ExecuteScalar<int>(query, mailBatch);
            }

            return mailBatches;
        }

        public int DeleteBatchDetails(int mailBatchId)
        {
            //Sql Query
            string query = @"DELETE FROM [AM].[MailPieceCustomInfo] WHERE MailPieceId IN (SELECT Id FROM [AM].[MailPieces] WHERE MailBatchId=@Id)
                            DELETE FROM [AM].[MailPieces] WHERE MailBatchId=@Id 
                            DELETE FROM [AM].[LogBatchTimeLine] WHERE MailBatchId=@Id
                            DELETE FROM [AM].[MailBatchCustomInfo] WHERE MailBatchId=@Id
                            DELETE FROM [AM].[MailBatches] WHERE Id=@Id;";

            return db.Execute(query, new { Id = mailBatchId });
        }

        public int UpdateBatchDetails(MailBatchDTO mailBatch)
        {
            string query = "UPDATE [AM].[MailBatches] SET [UniqueId] = @UniqueId,[ClientId] = @ClientId,[DocumentOrPackageType] = @DocumentOrPackageType,[BatchName] = @BatchName ,[ValidationStatus] = @ValidationStatus ,[ValidationErrors] = @ValidationErrors,[ClientBatchId] = @ClientBatchId ,[IsReturnInfoSpecified] = IsReturnInfoSpecified,[RetRcptPrefix] = @RetRcptPrefix ,[RetRcptFirstName] =@RetRcptFirstName ,[RetRcptMiddleName] = @RetRcptMiddleName ,[RetRcptLastName] = @RetRcptLastName ,[RetRcptSuffix] = @RetRcptSuffix ,[RetRcptPhone] = @RetRcptPhone ,[RetRcptOrganization] = @RetRcptOrganization ,[RetRcptAddrOptionalLine1AboveStreet] = @RetRcptAddrOptionalLine1AboveStreet ,[RetRcptAddrOptionalLine2AboveStreet] = @RetRcptAddrOptionalLine2AboveStreet ,[RetRcptAddressStreet] = @RetRcptAddressStreet ,[RetRcptAddressAptOrSuiteOrOther] = @RetRcptAddressAptOrSuiteOrOther ,[RetRcptAddrOptionalLine1BelowStreet] = RetRcptAddrOptionalLine1BelowStreet ,[RetRcptAddrOptionalLine2BelowStreet] = RetRcptAddrOptionalLine2BelowStreet ,[RetRcptAddressCity] = @RetRcptAddressCity ,[RetRcptAddressState] = @RetRcptAddressState ,[RetRcptAddressZIP] = @RetRcptAddressZIP ,[Priority] = @Priority ,[SFTChannelId] = @SFTChannelId WHERE Id=@Id";

            return db.Execute(query, mailBatch);
        }

        public int MarkMailBatchAsInvalid(int id, string errorDesc)
        {
            string query = "UPDATE [AM].[MailBatches] SET [ValidationStatus] = @ValidationStatus, [ValidationErrors] = @ValidationErrors WHERE Id=@Id";

            return db.Execute(query, new { ValidationStatus = BatchValidationStatus.Invalid, ValidationErrors = errorDesc, Id = id });
        }

        public List<MailBatchDTO> GetBatchesByPipelineStatusAndClientId(string batchPipelineStatus, int? clientId)
        {
            //Sql Query
            string query = "SELECT * FROM [AM].[MailBatches] WHERE PipelineStatus= @batchPipelineStatus";

            if (clientId.HasValue)
                query += " AND ClientId= @ClientId";

            switch (batchPipelineStatus)
            {
                case BatchPipelineStatus.Received: query += " AND IsReceived=1 AND IsDownloaded=0";
                    break;
                case BatchPipelineStatus.Downloaded: query += " AND IsDownloaded=1 AND IsValidated=0";
                    break;
                case BatchPipelineStatus.Validated: query += " AND IsValidated=1 AND IsAcknowledged=0";
                    break;
                case BatchPipelineStatus.Acknowledged: query += " AND IsAcknowledged=1 AND IsPrepped=0";
                    break;
                case BatchPipelineStatus.Prepped: query += " AND IsPrepped=1 AND IsSubmittedToShipping=0";
                    break;
                case BatchPipelineStatus.SubmittedToShipping: query += " AND IsSubmittedToShipping=1 AND IsShippingResponseReceived=0";
                    break;
                case BatchPipelineStatus.ShippingResponseReceived: query += " AND IsShippingResponseReceived=1 AND IsReportPrepped=0";
                    break;
                case BatchPipelineStatus.ReportPrepped: query += " AND IsReportPrepped=1 AND IsReportedBack=0";
                    break;
                default:
                    break;
            }

            var data = db.Query<MailBatchDTO>(query, new { batchPipelineStatus = batchPipelineStatus, ClientId = clientId }).ToList();
            return data;
        }

        public List<MailBatchDTO> GetBatchesByPipelineStatusAndSFTChannelId(string batchPipelineStatus, int? sftChannelId)
        {
            //Sql Query
            string query = "SELECT * FROM [AM].[MailBatches] WHERE PipelineStatus= @batchPipelineStatus";

            if (sftChannelId.HasValue)
                query += " AND SFTChannelId= @SFTChannelId";

            switch (batchPipelineStatus)
            {
                case BatchPipelineStatus.Received: query += " AND IsReceived=1 AND IsDownloaded=0";
                    break;
                case BatchPipelineStatus.Downloaded: query += " AND IsDownloaded=1 AND IsValidated=0";
                    break;
                case BatchPipelineStatus.Validated: query += " AND IsValidated=1 AND IsAcknowledged=0";
                    break;
                case BatchPipelineStatus.Acknowledged: query += " AND IsAcknowledged=1 AND IsPrepped=0";
                    break;
                case BatchPipelineStatus.Prepped: query += " AND IsPrepped=1 AND IsSubmittedToShipping=0";
                    break;
                case BatchPipelineStatus.SubmittedToShipping: query += " AND IsSubmittedToShipping=1 AND IsShippingResponseReceived=0";
                    break;
                case BatchPipelineStatus.ShippingResponseReceived: query += " AND IsShippingResponseReceived=1 AND IsReportPrepped=0";
                    break;
                case BatchPipelineStatus.ReportPrepped: query += " AND IsReportPrepped=1 AND IsReportedBack=0";
                    break;
                default:
                    break;
            }

            return db.Query<MailBatchDTO>(query, new { batchPipelineStatus = batchPipelineStatus, SFTChannelId = sftChannelId }).ToList();
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

        public MailBatchDTO GetMailBatchDetailByBatchId(int mailBatchId)
        {
            string query = @"SELECT * FROM [AM].[MailBatches] WHERE Id=@MailBatchId";

            return db.QuerySingle<MailBatchDTO>(query, new { MailBatchId = mailBatchId });
        }

        public int InsertPrintBatchDetails(PrintBatch printBatch)
        {
            //Sql Query
            string query = "INSERT INTO [AM].[PrintBatches](UniqueId, AppCode, PrintTrailId) Output Inserted.Id VALUES (@UniqueId, @AppCode, @PrintTrailId)";

            var printBatchId = db.ExecuteScalar<int>(query, printBatch);
            foreach (var mailPiece in printBatch.MailPieces)
            {
                string updateQuery = "UPDATE [AM].[MailPieces] SET [PrintBatchId]=@PrintBatchId,IsQCRequested=@IsQCRequested WHERE Id=@Id";

                var data = db.Execute(updateQuery, new { PrintBatchId = printBatchId, Id = mailPiece.Id, IsQCRequested = mailPiece.IsQCRequested });
            }
            return printBatchId;

        }

        public List<SubmittedBatch> GetSubmittedPrintBatchDetails()
        {
            //Sql Query
            string query = @"SELECT PB.Id AS PrintBatchId, PB.AppCode +'_'+ CONVERT(VARCHAR(10),PB.Id) AS SubmittedBatchName,PB.UniqueId,PB.DocTypeId,PB.AppCode FROM AM.PrintBatches PB WHERE PB.IsSubmittedToShipping=1 AND PB.IsShippingResponseReceived=0";

            return db.Query<SubmittedBatch>(query).ToList();
        }

        public List<SubmittedBatch> GetResponseReceivedPrintBatchDetails()
        {
            //Sql Query
            string query = @"SELECT DISTINCT PB.Id AS PrintBatchId, PB.AppCode +'_'+ CONVERT(VARCHAR(10),PB.Id) AS SubmittedBatchName,PB.UniqueId,PB.DocTypeId,PB.AppCode,MB.SFTChannelId FROM AM.PrintBatches PB INNER JOIN AM.MailPieces MP ON PB.Id=MP.PrintBatchId INNER JOIN AM.MailBatches MB ON MP.MailBatchId=MB.Id WHERE PB.IsSubmittedToShipping=1 AND PB.IsShippingResponseReceived=1 AND PB.BatchStatus=@BatchStatus";

            return db.Query<SubmittedBatch>(query, new { BatchStatus = BatchStatus.InProgress.ToString() }).ToList();
        }

        public List<SubmittedBatch> GetReportPreppedPrintBatchDetails()
        {
            //Sql Query
            string query = @"SELECT DISTINCT PB.Id AS PrintBatchId, PB.AppCode +'_'+ CONVERT(VARCHAR(10),PB.Id) AS SubmittedBatchName,PB.UniqueId,PB.DocTypeId,PB.AppCode,MB.SFTChannelId FROM AM.PrintBatches PB INNER JOIN AM.MailPieces MP ON PB.Id=MP.PrintBatchId INNER JOIN AM.MailBatches MB ON MP.MailBatchId=MB.Id WHERE PB.IsSubmittedToShipping=1 AND PB.IsReportPrepped=1 AND PB.IsReportedBack=0";

            return db.Query<SubmittedBatch>(query, null).ToList();
        }

        public int GetMailBatchesReceivedCount(DateTime startDateTime, DateTime endDateTime, string status)
        {
            string query = @"SELECT COUNT(*) FROM [AM].[MailBatches] WHERE ReceivedOn >=@StartDate AND ReceivedOn <=@EndDate";

            if (status == "Received")
                query += " AND IsReceived=1";
            else
                query += " AND (IsPrepped = 1 OR IsReportPrepped = 1 OR IsReportedBack = 1)";

            return db.ExecuteScalar<int>(query, new { StartDate = startDateTime, EndDate = endDateTime });
        }

        public int UpdatePrintBatchPipelineStatus(int printBatchId, string pipeLineStatus, DateTime timestamp)
        {
            string query = "UPDATE [AM].[PrintBatches] SET [{0}On] = @TimeStamp WHERE Id=@Id".Fill(pipeLineStatus);

            return db.Execute(query, new { TimeStamp = timestamp, Id = printBatchId });
        }

        public bool GetReportedBackPrintBatchesFromMailBatchId(int mailBatchId)
        {
            string query = @"SELECT COUNT(*) FROM AM.MailPieces P INNER JOIN AM.PrintBatches PB ON P.PrintBatchId=PB.Id WHERE P.MailBatchId=@MailBatchId AND PB.IsReportedBack=0";

            int count = db.ExecuteScalar<int>(query, new { MailBatchId = mailBatchId });
            if (count == 0)
                return true;
            return false;
        }
    }
}
