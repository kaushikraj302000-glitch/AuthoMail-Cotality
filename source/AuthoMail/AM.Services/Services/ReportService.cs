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
        List<DailyDetailOperationsReport> GetDailyDetailOperationsByDateOrBatch(int clientId, DateTime? date, int? batchId, string channel);
        List<ErrorReport> GetErrorReportByDateOrBatch(int clientId, DateTime? date, int? batchId, string channel);
        List<SummaryControlReport> GetSummaryControlReport(int clientId, DateTime date, string channel);
        List<ErrorReport> GetSLAMissErrorReportByDateOrBatch(int clientId, DateTime? date, int? batchId);
        List<MonthlyReconReport> GetMonthlyReconReport(int clientId, DateTime startDate, DateTime endDate, string channel);
    }
    public class ReportService : ServiceBase, IReportService
    {
        public List<DailyDetailOperationsReport> GetDailyDetailOperationsByDateOrBatch(int clientId, DateTime? date, int? batchId, string channel)
        {
            string query = ReportServiceQuery.GetDailyDetailOperationsByDateOrBatch;
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@clientId", clientId);
            if (date.HasValue)
            {
                query = string.Format("{0} AND cast(ReceivedOn as date) = cast(@date as date)", query);
                parameters.Add("@date", date);
            }
            if (batchId.HasValue)
            {
                query = string.Format("{0} AND BatchId=@batchId", query);
                parameters.Add("@batchId", batchId);
            }
            if (!channel.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} AND Channel=@channel", query);
                parameters.Add("@channel", channel);
            }
            var data = db.Query<DailyDetailOperationsReport>(query, parameters).ToList();
            return data;
        }
        public List<ErrorReport> GetErrorReportByDateOrBatch(int clientId, DateTime? date, int? batchId, string channel)
        {
            string query = ReportServiceQuery.GetErrorReportByDateOrBatch;

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@clientId", clientId);
            parameters.Add("@shippingstatus", ShippingStatus.Error.ToString());
            if (date.HasValue)
            {
                query = string.Format("{0} AND cast(ReceivedOn as date) = cast(@date as date)", query);
                parameters.Add("@date", date);
            }

            if (batchId.HasValue)
            {
                query = string.Format("{0} AND BatchId=@batchId", query);
                parameters.Add("@batchId", batchId);
            }
            if (!channel.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} AND Channel=@channel", query);
                parameters.Add("@channel", channel);
            }
            var data = db.Query<ErrorReport>(query, parameters).ToList();
            return data;
        }

        public List<SummaryControlReport> GetSummaryControlReport(int clientId, DateTime date, string channel)
        {
            string query = ReportServiceQuery.GetSummaryControlReport;
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@clientId", clientId);
            query = string.Format("{0} AND (cast(ReceivedOn as date) = cast(@date as date) AND CONVERT(VARCHAR(5),ReceivedOn,108)  <= @time)", query);
            parameters.Add("@date", date);
            parameters.Add("@time", "19:45");
            if (!channel.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} AND Channel=@channel", query);
                parameters.Add("@channel", channel);
            }
            var data = db.Query<SummaryControlReport>(query, parameters).ToList();
            return data;
        }

        public List<ErrorReport> GetSLAMissErrorReportByDateOrBatch(int clientId, DateTime? date, int? batchId)
        {
            string query = ReportServiceQuery.GetSLAMissErrorReportByDateOrBatch;
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@clientId", clientId);
            parameters.Add("@shippingstatus", ShippingStatus.Error.ToString());//Need to verify
            if (date.HasValue)
            {
                query = string.Format("{0} AND (cast(ReceivedOn as date) = cast(@date as date) AND CONVERT(VARCHAR(5),ReceivedOn,108)  < @time)", query);
                parameters.Add("@date", date);
                parameters.Add("@time", "19:45");
            }
            //Also verify need to define time in below clause. also check time?
            if (batchId.HasValue)
            {
                query = string.Format("{0} AND BatchId=@batchId", query);
                parameters.Add("@batchId", batchId);
            }
            var data = db.Query<ErrorReport>(query, parameters).ToList();
            return data;
        }

        public List<MonthlyReconReport> GetMonthlyReconReport(int clientId, DateTime startDate, DateTime endDate, string channel)
        {
            string query = ReportServiceQuery.GetMonthlyReconReport;
            //date must be in 26th to 25th
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@clientId", clientId);
            parameters.Add("@shippingStatus", ShippingStatus.Shipped.ToString());
            parameters.Add("@startDate", startDate);
            parameters.Add("@endDate", endDate);
            if (!channel.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} AND Channel=@channel", query);
                parameters.Add("@channel", channel);
            }
            var data = db.Query<MonthlyReconReport>(query, parameters).ToList();
            return data;
        }

    }
    public static class ReportServiceQuery
    {
        public static string GetDailyDetailOperationsByDateOrBatch = "select * from [dbo].[view_DailyDetailOperationsReport] WHERE ClientId=@clientId";

        public static string GetErrorReportByDateOrBatch = "select * from [dbo].[view_ErrorReport] WHERE ClientId=@clientId AND ShippingStatus=@shippingstatus";

        public static string GetSummaryControlReport = "select * from [dbo].[view_SummaryControlReport] WHERE ClientId=@clientId";

        public static string GetSLAMissErrorReportByDateOrBatch = "select * from [dbo].[view_ErrorReport] WHERE ClientId=@clientId AND ShippingStatus=@shippingstatus AND SLADays < 0";

        public static string GetMonthlyReconReport = "select * from [dbo].[view_MonthlyReconReport] WHERE ClientId=@clientId AND (cast(ReceivedOn as date) >= cast(@startDate as date) and cast(ReceivedOn as date)<= cast(@endDate as date)) AND ShippingStatus=@shippingStatus";

    }
}
