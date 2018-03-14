using AM.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace AM.Services
{
    public interface IChartDataService
    {
        List<WeeklyChart> GetWeeklyChartData(DateTime startDate, DateTime endDate, int clientId);
        List<TodayMailPieceChart> GetTodayMailPieceChartData(DateTime todayDate, int clientId);
        List<PullBackInfo> GetMailPieceByParam(int batchId, string status, int clientId);
        List<BatchListByChartData> GetBatchByParam(DateTime date, string status, int clientId);
        List<PullBackInfo> GetMailPieceByBatchId(int batchId,int clientId);
    }

    public class ChartDataService : ServiceBase, IChartDataService
    {
        public List<WeeklyChart> GetWeeklyChartData(DateTime startDate, DateTime endDate, int clientId)
        {
            var data = db.Query<WeeklyChart>(ChartDataServiceQuery.GetWeeklyChartData, new { startDate = startDate, endDate = endDate, clientId = clientId }).ToList();
            return data;
        }


        public List<TodayMailPieceChart> GetTodayMailPieceChartData(DateTime todayDate, int clientId)
        {
            var data = db.Query<TodayMailPieceChart>(ChartDataServiceQuery.GetTodayMailPieceChartData, new { todayDate = todayDate, clientId = clientId }).ToList();
            return data;
        }


        public List<PullBackInfo> GetMailPieceByParam(int batchId, string status, int clientId)
        {
            var data = db.Query<PullBackInfo>(ChartDataServiceQuery.GetMailPieceByParam, new { batchId = batchId, status = status, clientId = clientId }).ToList();
            return data;
        }

        public List<BatchListByChartData> GetBatchByParam(DateTime date, string status, int clientId)
        {
            var data = db.Query<BatchListByChartData>(ChartDataServiceQuery.GetBatchByParam, new { date = date, status = status, clientId = clientId }).ToList();
            return data;
        }


        public List<PullBackInfo> GetMailPieceByBatchId(int batchId,int clientId)
        {
            var data = db.Query<PullBackInfo>(ChartDataServiceQuery.GetMailPieceByBatchId, new { batchId = batchId, clientId = clientId }).ToList();
            return data;
        }
    }
    public static class ChartDataServiceQuery
    {
        public static string GetWeeklyChartData = "select * from dbo.ufn_GetWeeklyChartData(@startDate,@endDate,@clientId)";

        public static string GetTodayMailPieceChartData = "[dbo].[usp_TodayMailPieceChartData] @todayDate, @clientId";

        public static string GetMailPieceByParam = "[dbo].[usp_GetMailPieceByBatchIdAndStatus] @batchId, @status, @clientId";
        public static string GetBatchByParam = "select m.Id as [BatchId],m.ClientBatchId,m.[UniqueId] as [BatchUniqueId],m.BatchName,m.BatchStatus,m.DocumentOrPackageType,m.ReceivedOn, c.TimeZone as [ClientTimeZone] from [AM].[MailBatches] m left outer join [Common].[Clients] c ON m.ClientId=c.Id where BatchStatus=@status and ClientId=@clientId and cast(ReceivedOn as date)=cast(@date as date)";
        public static string GetMailPieceByBatchId = "[dbo].[usp_GetMailPieceByBatchId] @batchId, @clientId";
    }
}
