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
        int InsertBatchDetails(Batch mailBatch);
        int UpdateBatchDetails(Batch mailBatch);
        List<Batch> GetBatchesByStatus(string batchPipelineStatus);
    }
    public class BatchService : ServiceBase, IBatchService //It implements Interfaces 
    {
        public int InsertBatchDetails(Batch mailBatch)
        {
            //Sql Query
            string query = "INSERT INTO [AM].[Batch](Id, ClientId, DocumentOrPackageType, BatchName, PipelineStatus, ValidationStatus, ValidationErrors, BatchStatus, ReceivedOn, DownloadedOn, AcknowledgedOn, ValidatedOn, PreppedOn, SubmittedToShippingOn, ShippingResponseReceivedOn, ReportPreppedOn, ReportedBackOn, BusinessFriendlyIdName, BusinessFriendlyIdValue, IsReturnInfoSpecified, RetRcptPrefix, RetRcptFirstName, RetRcptMiddleName, RetRcptLastName, RetRcptSuffix, RetRcptPhone, RetRcptOrganization, RetRcptAddrOptionalLine1AboveStreet, RetRcptAddrOptionalLine2AboveStreet, RetRcptAddressStreet, RetRcptAddressAptOrSuiteOrOther, RetRcptAddrOptionalLine1BelowStreet, RetRcptAddrOptionalLine2BelowStreet, RetRcptAddressCity,RetRcptAddressState, RetRcptAddressZIP, BatchFilePath, Priority,ChannelCode) Output Inserted.BatchClustId VALUES (@Id, @ClientId, @DocumentOrPackageType, @BatchName, @PipelineStatus, @ValidationStatus, @ValidationErrors, @BatchStatus, @ReceivedOn, @DownloadedOn, @AcknowledgedOn, @ValidatedOn, @PreppedOn, @SubmittedToShippingOn, @ShippingResponseReceivedOn, @ReportPreppedOn, @ReportedBackOn, @BusinessFriendlyIdName, @BusinessFriendlyIdValue, @IsReturnInfoSpecified, @RetRcptPrefix, @RetRcptFirstName, @RetRcptMiddleName, @RetRcptLastName, @RetRcptSuffix, @RetRcptPhone, @RetRcptOrganization, @RetRcptAddrOptionalLine1AboveStreet, @RetRcptAddrOptionalLine2AboveStreet, @RetRcptAddressStreet, @RetRcptAddressAptOrSuiteOrOther, @RetRcptAddrOptionalLine1BelowStreet, @RetRcptAddrOptionalLine2BelowStreet, @RetRcptAddressCity, @RetRcptAddressState, @RetRcptAddressZIP, @BatchFilePath, @Priority,@ChannelCode)";


            //Assign value to parameters It may one or more. Note:
            //List of parameters declaration
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@Id", mailBatch.Id);
            parameters.Add("@ClientId", mailBatch.ClientId);
            parameters.Add("@DocumentOrPackageType", mailBatch.DocumentOrPackageType);
            parameters.Add("@BatchName", mailBatch.BatchName);
            parameters.Add("@PipelineStatus", mailBatch.PipelineStatus);
            parameters.Add("@ValidationStatus", mailBatch.ValidationStatus);
            parameters.Add("@ValidationErrors", mailBatch.ValidationErrors);
            parameters.Add("@BatchStatus", mailBatch.BatchStatus);
            parameters.Add("@ReceivedOn", mailBatch.ReceivedOn);
            parameters.Add("@DownloadedOn", mailBatch.DownloadedOn);
            parameters.Add("@AcknowledgedOn", mailBatch.AcknowledgedOn);
            parameters.Add("@ValidatedOn", mailBatch.ValidatedOn);
            parameters.Add("@PreppedOn", mailBatch.PreppedOn);
            parameters.Add("@SubmittedToShippingOn", mailBatch.SubmittedToShippingOn);
            parameters.Add("@ShippingResponseReceivedOn", mailBatch.ShippingResponseReceivedOn);
            parameters.Add("@ReportPreppedOn", mailBatch.ReportPreppedOn);
            parameters.Add("@ReportedBackOn", mailBatch.ReportedBackOn);
            parameters.Add("@BusinessFriendlyIdName", mailBatch.BusinessFriendlyIdName);
            parameters.Add("@BusinessFriendlyIdValue", mailBatch.BusinessFriendlyIdValue);
            parameters.Add("@IsReturnInfoSpecified", mailBatch.IsReturnInfoSpecified);
            parameters.Add("@RetRcptPrefix", mailBatch.RetRcptPrefix);
            parameters.Add("@RetRcptFirstName", mailBatch.RetRcptFirstName);
            parameters.Add("@RetRcptMiddleName", mailBatch.RetRcptMiddleName);
            parameters.Add("@RetRcptLastName", mailBatch.RetRcptLastName);
            parameters.Add("@RetRcptSuffix", mailBatch.RetRcptSuffix);
            parameters.Add("@RetRcptPhone", mailBatch.RetRcptPhone);
            parameters.Add("@RetRcptOrganization", mailBatch.RetRcptOrganization);
            parameters.Add("@RetRcptAddrOptionalLine1AboveStreet", mailBatch.RetRcptAddrOptionalLine1AboveStreet);
            parameters.Add("@RetRcptAddrOptionalLine2AboveStreet", mailBatch.RetRcptAddrOptionalLine2AboveStreet);
            parameters.Add("@RetRcptAddressStreet", mailBatch.RetRcptAddressStreet);
            parameters.Add("@RetRcptAddressAptOrSuiteOrOther", mailBatch.RetRcptAddressAptOrSuiteOrOther);
            parameters.Add("@RetRcptAddrOptionalLine1BelowStreet", mailBatch.RetRcptAddrOptionalLine1BelowStreet);
            parameters.Add("@RetRcptAddrOptionalLine2BelowStreet", mailBatch.RetRcptAddrOptionalLine2BelowStreet);
            parameters.Add("@RetRcptAddressCity", mailBatch.RetRcptAddressCity);
            parameters.Add("@RetRcptAddressState", mailBatch.RetRcptAddressState);
            parameters.Add("@RetRcptAddressZIP", mailBatch.RetRcptAddressZIP);
            parameters.Add("@BatchFilePath", mailBatch.BatchFilePath);
            parameters.Add("@Priority", mailBatch.Priority);
            parameters.Add("@ChannelCode", mailBatch.ChannelCode);

            /*
             * Execution of query with parateters.
             * Without Dapper you can't create following code. So just Add "using Dapper" namespace at begin.
            */
            var data = db.ExecuteScalar(query, parameters);
            return Convert.ToInt32(data);
        }

        public int UpdateBatchDetails(Batch mailBatch)
        {
            string query = "UPDATE [AuthoMail].[AM].[Batch] SET [Id] = @Id,[ClientId] = @ClientId,[DocumentOrPackageType] = @DocumentOrPackageType,[BatchName] = @BatchName,[PipelineStatus] = @PipelineStatus ,[ValidationStatus] = @ValidationStatus ,[ValidationErrors] = @ValidationErrors,[BatchStatus] = @BatchStatus ,[ReceivedOn] = @ReceivedOn,[DownloadedOn] = @DownloadedOn,[AcknowledgedOn] = @AcknowledgedOn,[ValidatedOn] = @ValidatedOn,[PreppedOn] = @PreppedOn,[SubmittedToShippingOn] = @SubmittedToShippingOn,[ShippingResponseReceivedOn] =<ShippingResponseReceivedOn,[ReportPreppedOn] = @ReportPreppedOn,[ReportedBackOn] = @ReportedBackOn,[BusinessFriendlyIdName] = @BusinessFriendlyIdName ,[BusinessFriendlyIdValue] = @BusinessFriendlyIdValue ,[IsReturnInfoSpecified] = IsReturnInfoSpecified,[RetRcptPrefix] = @RetRcptPrefix ,[RetRcptFirstName] =<RetRcptFirstName ,[RetRcptMiddleName] = @RetRcptMiddleName ,[RetRcptLastName] = @RetRcptLastName ,[RetRcptSuffix] = @RetRcptSuffix ,[RetRcptPhone] = @RetRcptPhone ,[RetRcptOrganization] = @RetRcptOrganization ,[RetRcptAddrOptionalLine1AboveStreet] = @RetRcptAddrOptionalLine1AboveStreet ,[RetRcptAddrOptionalLine2AboveStreet] = @RetRcptAddrOptionalLine2AboveStreet ,[RetRcptAddressStreet] = @RetRcptAddressStreet ,[RetRcptAddressAptOrSuiteOrOther] = @RetRcptAddressAptOrSuiteOrOther ,[RetRcptAddrOptionalLine1BelowStreet] = RetRcptAddrOptionalLine1BelowStreet ,[RetRcptAddrOptionalLine2BelowStreet] = RetRcptAddrOptionalLine2BelowStreet ,[RetRcptAddressCity] = @RetRcptAddressCity ,[RetRcptAddressState] = @RetRcptAddressState ,[RetRcptAddressZIP] = @RetRcptAddressZIP ,[BatchFilePath] = @BatchFilePath ,[Priority] = @Priority ,[ChannelCode] = @ChannelCode WHERE BatchClustId=@BatchClustId";

            var data = db.ExecuteScalar(query, mailBatch);
            return Convert.ToInt32(data);
        }

        public List<Batch> GetBatchesByStatus(string batchPipelineStatus)
        {
            //Sql Query
            string query = "SELECT * FROM [AM].[Batch] WHERE PipelineStatus= @batchPipelineStatus";

            switch (batchPipelineStatus.ToString())
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
            parameters.Add("@batchPipelineStatus", batchPipelineStatus.ToString());

            /* Execution of query with parateters.
              Without Dapper you can't create following code. So just Add "using Dapper" namespace at begin.
            */
            var data = db.Query<Batch>(query, parameters).ToList();
            return data;
        }
    }
}
