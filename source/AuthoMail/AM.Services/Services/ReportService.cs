using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using AM.Services.Models;
using AM.Entities;

namespace AM.Services
{
    public interface IReportService
    {
        List<DailyDetailOperationsReport> GetDailyDetailOperationsByDateOrBatch(int clientId, DateTime? date, string clientBatchId);
        List<ErrorReport> GetErrorReportByDateOrBatch(int clientId, DateTime? date, string clientBatchId);
        List<SummaryControlReport> GetSummaryControlReport(int clientId, DateTime date);
        List<ErrorReport> GetSLAMissErrorReportByDateOrBatch(int clientId, DateTime? date, string clientBatchId);

        List<MonthlyReconReport> GetMonthlyReconReport(int clientId, DateTime startDate, DateTime endDate);

        //Nedd to move
        List<MailPieceSearch> GetMailPieceByBatchId(int batchId, int clientId);
        List<DocumentType> GetDocTypeList(int clientId);
        List<string> GetMailPieceIdFromLoanNo(string loanNo, int clientId);

    }
    public class ReportService : ServiceBase, IReportService
    {
        public List<DailyDetailOperationsReport> GetDailyDetailOperationsByDateOrBatch(int clientId, DateTime? date, string clientBatchId)
        {
            string query = ReportServiceQuery.GetDailyDetailOperationsByDateOrBatch;
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@clientId", clientId);
            if (date.HasValue)
            {
                query = string.Format("{0} AND cast(ReceivedOn as date) = cast(@date as date)", query);
                parameters.Add("@date", date);
            }
            if (!clientBatchId.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} AND ClientBatchId=@clientBatchId", query);
                parameters.Add("@ClientBatchId", clientBatchId);
            }
            var data = db.Query<DailyDetailOperationsReport>(query, parameters).ToList();
            return data;
        }
        public List<ErrorReport> GetErrorReportByDateOrBatch(int clientId, DateTime? date, string clientBatchId)
        {
            string query = ReportServiceQuery.GetErrorReportByDateOrBatch;

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@clientId", clientId);
            parameters.Add("@shippingstatus", ShippingStatus.Error);
            if (date.HasValue)
            {
                query = string.Format("{0} AND cast(ReceivedOn as date) = cast(@date as date)", query);
                parameters.Add("@date", date);
            }

            if (!clientBatchId.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} AND ClientBatchId=@clientBatchId", query);
                parameters.Add("@ClientBatchId", clientBatchId);
            }
            var data = db.Query<ErrorReport>(query, parameters).ToList();
            return data;
        }

        public List<SummaryControlReport> GetSummaryControlReport(int clientId, DateTime date)
        {
            string query = ReportServiceQuery.GetSummaryControlReport;
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@clientId", clientId);
            query = string.Format("{0} AND (cast(ReceivedOn as date) = cast(@date as date) AND CONVERT(VARCHAR(5),ReceivedOn,108)  < @time)", query);
            parameters.Add("@date", date);
            parameters.Add("@time", "19:45");
            var data = db.Query<SummaryControlReport>(query, parameters).ToList();
            return data;
        }

        public List<ErrorReport> GetSLAMissErrorReportByDateOrBatch(int clientId, DateTime? date, string clientBatchId)
        {
            string query = ReportServiceQuery.GetSLAMissErrorReportByDateOrBatch;
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@clientId", clientId);
            parameters.Add("@shippingstatus", ShippingStatus.Error);//Need to varify
            if (date.HasValue)
            {
                query = string.Format("{0} AND (cast(ReceivedOn as date) = cast(@date as date) AND CONVERT(VARCHAR(5),ReceivedOn,108)  < @time)", query);
                parameters.Add("@date", date);
                parameters.Add("@time", "19:45");
            }
            //Also verify need to define time in below clause
            if (!clientBatchId.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} AND ClientBatchId=@clientBatchId", query);
                parameters.Add("@ClientBatchId", clientBatchId);
            }
            var data = db.Query<ErrorReport>(query, new { clientId = clientId }).ToList();
            return data;
        }

        public List<MonthlyReconReport> GetMonthlyReconReport(int clientId, DateTime startDate, DateTime endDate)
        {
            string query = ReportServiceQuery.GetMonthlyReconReport;
            //date must be in 26th to 25th
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@clientId", clientId);
            parameters.Add("@startDate", startDate);
            parameters.Add("@endDate", endDate);
            parameters.Add("@shippingStatus", ShippingStatus.Shipped);

            var data = db.Query<MonthlyReconReport>(query, parameters).ToList();
            return data;
        }


        public List<MailPieceSearch> GetMailPieceByBatchId(int batchId, int clientId)
        {
            string query = ReportServiceQuery.GetMailPieceByBatchId;
            var data = db.Query<MailPieceSearch>(query, new { batchId = batchId, clientId = clientId }).ToList();
            return data;
        }



        public List<DocumentType> GetDocTypeList(int clientId)
        {
            string query = ReportServiceQuery.GetDocTypeList;
            var data = db.Query(query, new { clientId = clientId }).ToList();
            var list = new List<DocumentType>();
            foreach (var item in data)
            {
                list.Add(new DocumentType { Id = item.Id, DocTypeCode = item.DocTypeCode });
            }
            return list;
        }


        public List<string> GetMailPieceIdFromLoanNo(string loanNo, int clientId)
        {
            string query = ReportServiceQuery.GetMailPieceIdFromLoanNo;
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@loanNo", loanNo);
            parameters.Add("@clientId", clientId);
            parameters.Add("@todayDate", Common.Today.Date);
            parameters.Add("@shippingStatus", ShippingStatus.InProgress);
            parameters.Add("@pipelineStatus", BatchPipelineStatus.SubmittedToShipping);

            var mailPieces = db.Query<string>(query, parameters).ToList();
            return mailPieces;
        }


    }
    public static class ReportServiceQuery
    {
        public static string GetDailyDetailOperationsByDateOrBatch = "select * from [dbo].[view_DailyDetailOperationsReport] WHERE ClientId=@clientId";

        public static string GetErrorReportByDateOrBatch = "select * from [dbo].[view_ErrorReport] WHERE ClientId=@clientId AND ShippingStatus=@shippingstatus";

        public static string GetSummaryControlReport = "select * from [dbo].[view_SummaryControlReport] WHERE ClientId=@clientId";

        public static string GetSLAMissErrorReportByDateOrBatch = "select * from [dbo].[view_ErrorReport] WHERE ClientId=@clientId AND ShippingStatus=@shippingstatus AND SLADays < 0";

        public static string GetMonthlyReconReport = "select * from [dbo].[view_MonthlyReconReport] WHERE ClientId=@clientId AND (cast(ReceivedOn as date) >= cast(@startDate as date) and cast(ReceivedOn as date)<= cast(@endDate as date)) AND ShippingStatus=@shippingStatus";

        public static string GetMailPieceByBatchId = "select m.* from [AM].[MailPieces] m,[AM].[MailBatches] b WHERE m.BatchId=b.Id AND (BatchId=@batchId AND b.ClientId=@clientId)";

        public static string GetDocTypeList = "select Id,DocTypeCode FROM [AM].[DocTypes] WHERE ClientId=@clientId";

        public static string GetMailPieceIdFromLoanNo = "select m.[UniqueId] as [MailPieceId] from [AM].[MailBatches] b,[AM].[MailPieces] m where b.Id = m.BatchId and ((cast(ReceivedOn as date)=cast(@todayDate as date) or (m.ShippingStatus=@shippingStatus and b.PipelineStatus=@pipelineStatus))) and b.BusinessFriendlyIdValue=@loanNo AND b.ClientId=@clientId";


    }
}
