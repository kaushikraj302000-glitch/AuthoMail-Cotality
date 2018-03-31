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
    public interface IMailBatchService : IDisposable
    {
        int InsertBatchDetails(MailBatchDTO mailBatch);
        int DeleteBatchDetails(int mailBatchId);
        int UpdateBatchDetails(MailBatchDTO mailBatch);
        List<MailBatchDTO> GetBatchesByStatus(string batchPipelineStatus);
        BatchData GetMailBatchDetailsByBatchId(int mailBatchId);
        int InsertPrintBatchDetails(PrintBatch printBatch);
        List<SubmittedBatch> GetSubmittedPrintBatchDetails();
        List<SubmittedBatch> GetResponseReceivedPrintBatchDetails();
    }
    public class MailBatchService : ServiceBase, IMailBatchService //It implements Interfaces 
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
            string query = @"DELETE FROM [AM].[MailPieceCustomInfo] WHERE MailPieceId IN (SELECT Id FROM [AM].[MailPieces] WHERE MailBatchId=@Id)
                            DELETE FROM [AM].[MailPieces] WHERE MailBatchId=@Id              
                            DELETE FROM [AM].[PrintBatches] WHERE MailBatchId=@Id
                            DELETE FROM [AM].[LogBatchTimeLine] WHERE MailBatchId=@Id
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

        public List<MailBatchDTO> GetBatchesByStatus(string batchPipelineStatus)
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

            var data = db.Query<MailBatchDTO>(query, new { batchPipelineStatus = batchPipelineStatus }).ToList();
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

        public int InsertPrintBatchDetails(PrintBatch printBatch)
        {
            //Sql Query
            string query = "INSERT INTO [AM].[PrintBatches](UniqueId, MailBatchId, AppCode, DocTypeId) Output Inserted.Id VALUES (@UniqueId, @MailBatchId, @AppCode, @DocTypeId)";

            var printBatchId = db.ExecuteScalar(query, printBatch);

            if (printBatchId != null || Convert.ToInt32(printBatchId) != 0)
            {
                string updateQuery = "UPDATE [AM].[MailPieces] SET [PrintBatchId]=@Id WHERE MailBatchId=@mailBatchId";

                var data = db.Execute(updateQuery, new { Id = printBatchId, mailBatchId = printBatch.MailBatchId });
                return Convert.ToInt32(printBatchId);
            }
            return 0;
        }

        public List<SubmittedBatch> GetSubmittedPrintBatchDetails()
        {
            //Sql Query
            string query = @"SELECT PB.Id AS PrintBatchId, PB.MailBatchId, MB.BatchName,MB.BatchName+'_'+CONVERT(varchar(5),PB.DocTypeId) as SubmittedBatchName,PB.UniqueId,PB.DocTypeId,PB.AppCode FROM [AM].[PrintBatches] PB INNER JOIN [AM].[MailBatches] MB ON MB.Id= PB.MailBatchId WHERE PB.IsSubmittedToShipping=1 AND PB.IsShippingResponseReceived=0";

            var data = db.Query<SubmittedBatch>(query).ToList();

            return data;
        }

        public List<SubmittedBatch> GetResponseReceivedPrintBatchDetails()
        {
            //Sql Query
            string query = @"SELECT PB.Id AS PrintBatchId, PB.MailBatchId, MB.BatchName,MB.BatchName+'_'+CONVERT(varchar(5),PB.DocTypeId) as SubmittedBatchName,PB.UniqueId,PB.DocTypeId,PB.AppCode FROM [AM].[PrintBatches] PB INNER JOIN [AM].[MailBatches] MB ON MB.Id= PB.MailBatchId WHERE PB.IsSubmittedToShipping=1 AND PB.IsShippingResponseReceived=1";

            var data = db.Query<SubmittedBatch>(query).ToList();

            return data;
        }
    }
}
