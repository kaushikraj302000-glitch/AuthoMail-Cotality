using AM.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AM.Lib;
using AM.Entities;

namespace AM.Services
{
    public interface IChartDataService
    {
        List<WeeklyChart> GetWeeklyChartData(int clientId);
        List<TodayMailPieceChart> GetTodayMailPieceChartData(DateTime todayDate, int clientId);
        List<PullBackInfo> GetMailPieceByParam(int batchId, string status, int clientId);
        List<BatchListByChartData> GetBatchByParam(DateTime date, string status, int clientId);
        List<PullBackInfo> GetMailPieceByBatchId(int batchId, int clientId);
    }

    public class ChartDataService : ServiceBase, IChartDataService
    {
        public List<WeeklyChart> GetWeeklyChartData(int clientId)
        {
            var estToday = DateTime.UtcNow.UtcToEst().Date;
            // EST Week
            var startDate = (estToday.AddDays(-7)).EstToUtc();
            var endDate = (estToday + new TimeSpan(23, 59, 59)).EstToUtc();
            var data = db.Query<WeeklyChartDTO>(ChartDataServiceQuery.GetWeeklyChartData, new { startDate = startDate, endDate = endDate, clientId = clientId }).ToList();

            // Convert received on date to EST timezone
            data.ForEach(s => s.ReceivedOn = s.ReceivedOn.UtcToEst());

            var model = data.GroupBy(d => d.ReceivedOn.Date,
                (key, g) => new WeeklyChart
                {
                    Date = key,
                    Weekday = key == estToday ? string.Format("{0} [{1}]", "Today", key.ToString("dd")) : string.Format("{0} [{1}]", key.ToString("ddd"), key.ToString("dd")),
                    Failed = g.Count(s => s.BatchStatus == BatchStatus.Failed.ToString()),
                    InProgress = g.Count(s => s.BatchStatus == BatchStatus.InProgress.ToString()),
                    ShippedAll = g.Count(s => s.BatchStatus == BatchStatus.ShippedAll.ToString()),
                    ShippedPartial = g.Count(s => s.BatchStatus == BatchStatus.ShippedPartial.ToString())
                }).ToList();

            //DateTime dt = DateTime.Today; dt > DateTime.Today.AddDays(-8); dt = dt.AddDays(-1)
            for (DateTime ed = estToday; ed >= startDate.UtcToEst().Date; ed = ed.AddDays(-1))
            {
                if (!model.Any(m => m.Date == ed))
                {
                    model.Add(new WeeklyChart
                    {
                        Date = ed,
                        Weekday = ed == estToday ? string.Format("{0} [{1}]", "Today", ed.ToString("dd")) : string.Format("{0} [{1}]", ed.ToString("ddd"), ed.ToString("dd")),
                        Failed = 0,
                        InProgress = 0,
                        ShippedAll = 0,
                        ShippedPartial = 0
                    });
                }
            }

            model = model.OrderByDescending(m => m.Date).ToList();

            return model;
        }


        public List<TodayMailPieceChart> GetTodayMailPieceChartData(DateTime todayDate, int clientId)
        {
            var data = db.Query<TodayMailPieceChart>(ChartDataServiceQuery.GetTodayMailPieceChartData, new { todayDate = todayDate.EstToUtc(), clientId = clientId }).ToList();
            return data;
        }


        public List<PullBackInfo> GetMailPieceByParam(int batchId, string status, int clientId)
        {
            var data = db.Query<PullBackInfo>(ChartDataServiceQuery.GetMailPieceByParam, new { batchId = batchId, status = status, clientId = clientId }).ToList();
            return data;
        }

        public List<BatchListByChartData> GetBatchByParam(DateTime date, string status, int clientId)
        {
            var data = db.Query<BatchListByChartData>(ChartDataServiceQuery.GetBatchByParam, new { date = date.EstToUtc(), status = status, clientId = clientId }).ToList();
            return data;
        }


        public List<PullBackInfo> GetMailPieceByBatchId(int batchId, int clientId)
        {
            var data = db.Query<PullBackInfo>(ChartDataServiceQuery.GetMailPieceByBatchId, new { batchId = batchId, clientId = clientId }).ToList();
            return data;
        }
    }
    public static class ChartDataServiceQuery
    {
        public static string GetWeeklyChartData = "select Id,BatchStatus,BatchName,ReceivedOn from AM.MailBatches Where ReceivedOn between @startDate and @endDate and ClientId=@clientId AND PipelineStatus !='Cancelled'";

        public static string GetTodayMailPieceChartData = "[dbo].[usp_TodayMailPieceChartData] @todayDate, @clientId";

        public static string GetMailPieceByParam = "[dbo].[usp_GetMailPieceByBatchIdAndStatus] @batchId, @status, @clientId";
        public static string GetBatchByParam = "select m.Id as [BatchId],m.ClientBatchId,m.[UniqueId] as [BatchUniqueId],m.BatchName,m.BatchStatus,m.DocumentOrPackageType,m.ReceivedOn, c.TimeZone as [ClientTimeZone] from [AM].[MailBatches] m left outer join [Common].[Clients] c ON m.ClientId=c.Id where BatchStatus=@status and ClientId=@clientId and cast(ReceivedOn as date)=cast(@date as date)";
        public static string GetMailPieceByBatchId = "[dbo].[usp_GetMailPieceByBatchId] @batchId, @clientId";
    }
}
