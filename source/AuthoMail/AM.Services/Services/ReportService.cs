using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using AM.Services.Models;
using AM.Entities;
using AM.Lib;
namespace AM.Services
{
    public interface IReportService
    {
        List<DailyDetailOperationsReport> GetDailyDetailOperationsByDateOrBatch(int clientId, DateTime? date, int? batchId, string channel);
        List<ErrorReport> GetErrorReportByDateOrBatch(int clientId, DateTime? date, int? batchId, string channel);
        List<SummaryControlReport> GetSummaryControlReport(int clientId, DateTime startDate, DateTime endDate, string channel);
        List<ErrorReport> GetSLAMissErrorReportByDateOrBatch(int clientId, DateTime date, int? batchId, string docType, string filterBy);
        List<MonthlyReconReport> GetMonthlyReconReport(int clientId, DateTime startDate, DateTime endDate, string channel);
        List<ChannelCodeTitle> GetChannelTitle(List<string> channelCodes);
    }
    public class ReportService : ServiceBase, IReportService
    {
        public List<DailyDetailOperationsReport> GetDailyDetailOperationsByDateOrBatch(int clientId, DateTime? date, int? batchId, string channel)
        {
            string query = ReportServiceQuery.GetDailyDetailOperationsByDateOrBatch;
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@clientId", clientId);
            parameters.Add("@shippingstatus", ShippingStatus.Shipped.ToString());
            if (date.HasValue)
            {
                DateTime startDate = (date.Value + new TimeSpan(00, 00, 00));
                DateTime endDate = (date.Value + new TimeSpan(23, 59, 59));
                query = string.Format("{0} AND ReceivedOn between @startDate and @endDate", query);
                parameters.Add("@startDate", startDate.EstToUtc());
                parameters.Add("@endDate", endDate.EstToUtc());
            }
            if (batchId.HasValue)
            {
                query = string.Format("{0} AND BatchId=@batchId", query);
                parameters.Add("@batchId", batchId);
            }
            if (!channel.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} AND ChannelCode=@channel", query);
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
                DateTime startDate = (date.Value + new TimeSpan(00, 00, 00));
                DateTime endDate = (date.Value + new TimeSpan(23, 59, 59));
                query = string.Format("{0} AND ReceivedOn between @startDate and @endDate", query);
                parameters.Add("@startDate", startDate.EstToUtc());
                parameters.Add("@endDate", endDate.EstToUtc());
            }

            if (batchId.HasValue)
            {
                query = string.Format("{0} AND BatchId=@batchId", query);
                parameters.Add("@batchId", batchId);
            }
            if (!channel.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} AND ChannelCode=@channel", query);
                parameters.Add("@channel", channel);
            }
            var data = db.Query<ErrorReport>(query, parameters).ToList();
            return data;
        }

        public List<SummaryControlReport> GetSummaryControlReport(int clientId, DateTime startDate, DateTime endDate, string channel)
        {
            string query = ReportServiceQuery.GetSummaryControlReport;
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@clientId", clientId);
            query = string.Format("{0} AND (ReceivedOn >= @startDate) AND (ReceivedOn <= @endDate)", query);
            parameters.Add("@startDate", startDate.EstToUtc());
            parameters.Add("@endDate", endDate.EstToUtc());
            if (!channel.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} AND ChannelCode=@channel", query);
                parameters.Add("@channel", channel);
            }
            var data = db.Query<SummaryControlReport>(query, parameters).ToList();
            return data;
        }

        public List<ErrorReport> GetSLAMissErrorReportByDateOrBatch(int clientId, DateTime date, int? batchId, string docType, string filterBy)
        {
            string query = ReportServiceQuery.GetSLAMissErrorReportByDateOrBatch;
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@clientId", clientId);
            //parameters.Add("@shippingstatus", ShippingStatus.Error.ToString());//Need to verify

            DateTime startDate = (date + new TimeSpan(00, 00, 00));
            DateTime endDate = (date + new TimeSpan(23, 59, 59));
            query = string.Format("{0} AND {1} between @startDate and @endDate", query, filterBy);
            parameters.Add("@startDate", startDate.EstToUtc());
            parameters.Add("@endDate", endDate.EstToUtc());

            //Also verify need to define time in below clause. also check time?
            if (batchId.HasValue)
            {
                query = string.Format("{0} AND BatchId=@batchId", query);
                parameters.Add("@batchId", batchId);
            }
            if (!docType.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} AND DocumentOrPackageType=@docType", query);
                parameters.Add("@docType", docType);
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
            parameters.Add("@startDate", startDate.EstToUtc());
            parameters.Add("@endDate", endDate.EstToUtc());
            if (!channel.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} AND ChannelCode=@channel", query);
                parameters.Add("@channel", channel);
            }
            var data = db.Query<MonthlyReconReport>(query, parameters).ToList();
            return data;
        }

        public List<ChannelCodeTitle> GetChannelTitle(List<string> channelCodes)
        {
            var channelList = db.Query<ChannelCodeTitle>(ReportServiceQuery.GetChannelTitle, new { channelCodes = channelCodes }).ToList();
            return channelList;
        }
    }
    public static class ReportServiceQuery
    {
        public static string GetDailyDetailOperationsByDateOrBatch = "select * from [dbo].[view_DailyDetailOperationsReport] WHERE ClientId=@clientId AND ShippingStatus=@shippingStatus";

        public static string GetErrorReportByDateOrBatch = "select * from [dbo].[view_ErrorReport] WHERE ClientId=@clientId AND ShippingStatus=@shippingstatus";

        public static string GetSummaryControlReport = "select * from [dbo].[view_SummaryControlReport] WHERE ClientId=@clientId";

        public static string GetSLAMissErrorReportByDateOrBatch = "select * from [dbo].[view_ErrorReport] WHERE ClientId=@clientId AND SLADays < 0";

        public static string GetMonthlyReconReport = "select * from [dbo].[view_MonthlyReconReport] WHERE ClientId=@clientId AND (cast(ReceivedOn as date) >= cast(@startDate as date) and cast(ReceivedOn as date) <= cast(@endDate as date)) AND ShippingStatus=@shippingStatus";//AND SLADays >= 0

        public static string GetChannelTitle = "SELECT Code,Title FROM [Common].[SFTChannels] WHERE IsActive=1 AND Code IN @channelCodes";

    }
}
