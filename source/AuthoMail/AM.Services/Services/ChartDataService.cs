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
    public interface IChartDataService : IDisposable
    {
        List<WeeklyChart> GetWeeklyChartData(int clientId);
        List<TodayMailPieceChart> GetTodayMailPieceChartData(DateTime todayDate, int clientId);
        List<MailPieces> GetMailPieceByParam(int batchId, string status, int clientId);
        List<BatchListByChartData> GetBatchByParam(DateTime date, string status, int clientId);
        List<MailPieces> GetMailPieceByBatchId(int batchId, int clientId);
        //--------------Chart for all clients-----------------------------------------------------//
        List<WeeklyChart> GetWeeklyChartData();
        List<TodayMailPieceChart> GetTodayMailPieceChartData(DateTime todayDate);
        List<MailPieces> GetMailPieceByParam(int batchId, string status);
        List<BatchListByChartData> GetBatchByParam(DateTime date, string status);
        List<MailPieces> GetMailPieceByBatchId(int batchId);
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
                    Weekday = key == estToday ? string.Format("{0} ({1})", "Today", key.ToString("dd")) : string.Format("{0} ({1})", key.ToString("ddd"), key.ToString("dd")),
                    Failed = g.Count(s => s.BatchStatus == BatchStatus.Failed.ToString()),
                    InProgress = g.Count(s => s.BatchStatus == BatchStatus.InProgress.ToString()),
                    ShippedAll = g.Count(s => s.BatchStatus == BatchStatus.ShippedAll.ToString()),
                    ShippedPartial = g.Count(s => s.BatchStatus == BatchStatus.ShippedPartial.ToString()),
                    PulledBack = g.Count(s => s.BatchStatus == BatchStatus.PulledBack.ToString())
                }).ToList();

            //DateTime dt = DateTime.Today; dt > DateTime.Today.AddDays(-8); dt = dt.AddDays(-1)
            for (DateTime ed = estToday; ed >= startDate.UtcToEst().Date; ed = ed.AddDays(-1))
            {
                if (!model.Any(m => m.Date == ed))
                {
                    model.Add(new WeeklyChart
                    {
                        Date = ed,
                        Weekday = ed == estToday ? string.Format("{0} ({1})", "Today", ed.ToString("dd")) : string.Format("{0} ({1})", ed.ToString("ddd"), ed.ToString("dd")),
                        Failed = 0,
                        InProgress = 0,
                        ShippedAll = 0,
                        ShippedPartial = 0,
                        PulledBack = 0
                    });
                }
            }

            model = model.OrderByDescending(m => m.Date).ToList();

            return model;
        }


        public List<TodayMailPieceChart> GetTodayMailPieceChartData(DateTime todayDate, int clientId)
        {
            string query = "select * from(select b.ClientBatchId,m.MailBatchId as [MailBatchId], b.BatchName,m.Id,m.ShippingStatus, b.ReceivedOn from [AM].[MailPieces] m,[AM].[MailBatches] b where m.MailBatchId=b.Id AND b.ClientId=@clientId AND  b.ReceivedOn BETWEEN @startDate AND @endDate) s PIVOT (count(Id) FOR ShippingStatus IN ([InProgress],[PulledBack],[Error],[Shipped]))pvt";
            var data = db.Query<TodayMailPieceChart>(query, new { startDate = todayDate.ToStartDateTime().EstToUtc(), endDate = todayDate.ToEndDateTime().EstToUtc(), clientId = clientId }).ToList();
            return data;
        }


        public List<MailPieces> GetMailPieceByParam(int batchId, string status, int clientId)
        {
            string query = "[dbo].[uspweb_GetMailPieceByBatchIdAndStatus] @batchId, @status, @clientId";
            var data = db.Query<MailBatches, Clients, MailPieces, DocTypes, MailPieces>(query,
                (b, c, m, d) =>
                {
                    m.MailBatches = b;
                    m.Clients = c;
                    m.DocTypes = d;
                    return m;
                },
                new
                {
                    batchId = batchId,
                    status = status,
                    clientId = clientId
                }).ToList();
            return data;
        }

        public List<BatchListByChartData> GetBatchByParam(DateTime date, string status, int clientId)
        {
            var data = db.Query<BatchListByChartData>(ChartDataServiceQuery.GetBatchByParam, new { startDate = date.ToStartDateTime().EstToUtc(), endDate = date.ToEndDateTime().EstToUtc(), status = status, clientId = clientId }).ToList();
            return data;
        }


        public List<MailPieces> GetMailPieceByBatchId(int batchId, int clientId)
        {
            string query = "[dbo].[uspweb_GetMailPieceByBatchId] @batchId, @clientId";
            var data = db.Query<MailBatches, Clients, MailPieces, DocTypes, MailPieces>(query,
                (b, c, m, d) =>
                {
                    m.MailBatches = b;
                    m.Clients = c;
                    m.DocTypes = d;
                    return m;
                },
                new
                {
                    batchId = batchId,
                    clientId = clientId
                }).ToList();
            return data;
        }


        public List<WeeklyChart> GetWeeklyChartData()
        {
            var estToday = DateTime.UtcNow.UtcToEst().Date;
            // EST Week
            var startDate = (estToday.AddDays(-7)).EstToUtc();
            var endDate = (estToday + new TimeSpan(23, 59, 59)).EstToUtc();
            var data = db.Query<WeeklyChartDTO>("select Id,BatchStatus,BatchName,ReceivedOn from AM.MailBatches Where ReceivedOn between @startDate and @endDate", new { startDate = startDate, endDate = endDate}).ToList();

            // Convert received on date to EST timezone
            data.ForEach(s => s.ReceivedOn = s.ReceivedOn.UtcToEst());

            var model = data.GroupBy(d => d.ReceivedOn.Date,
                (key, g) => new WeeklyChart
                {
                    Date = key,
                    Weekday = key == estToday ? string.Format("{0} ({1})", "Today", key.ToString("dd")) : string.Format("{0} ({1})", key.ToString("ddd"), key.ToString("dd")),
                    Failed = g.Count(s => s.BatchStatus == BatchStatus.Failed.ToString()),
                    InProgress = g.Count(s => s.BatchStatus == BatchStatus.InProgress.ToString()),
                    ShippedAll = g.Count(s => s.BatchStatus == BatchStatus.ShippedAll.ToString()),
                    ShippedPartial = g.Count(s => s.BatchStatus == BatchStatus.ShippedPartial.ToString()),
                    PulledBack = g.Count(s => s.BatchStatus == BatchStatus.PulledBack.ToString())
                }).ToList();

            //DateTime dt = DateTime.Today; dt > DateTime.Today.AddDays(-8); dt = dt.AddDays(-1)
            for (DateTime ed = estToday; ed >= startDate.UtcToEst().Date; ed = ed.AddDays(-1))
            {
                if (!model.Any(m => m.Date == ed))
                {
                    model.Add(new WeeklyChart
                    {
                        Date = ed,
                        Weekday = ed == estToday ? string.Format("{0} ({1})", "Today", ed.ToString("dd")) : string.Format("{0} ({1})", ed.ToString("ddd"), ed.ToString("dd")),
                        Failed = 0,
                        InProgress = 0,
                        ShippedAll = 0,
                        ShippedPartial = 0,
                        PulledBack = 0
                    });
                }
            }

            model = model.OrderByDescending(m => m.Date).ToList();

            return model;
        }

        public List<TodayMailPieceChart> GetTodayMailPieceChartData(DateTime todayDate)
        {
            string query = "select * from(select b.ClientBatchId,m.MailBatchId as [MailBatchId], b.BatchName,m.Id,m.ShippingStatus, b.ReceivedOn from [AM].[MailPieces] m,[AM].[MailBatches] b where m.MailBatchId=b.Id AND  b.ReceivedOn BETWEEN @startDate AND @endDate) s PIVOT (count(Id) FOR ShippingStatus IN ([InProgress],[PulledBack],[Error],[Shipped]))pvt";
            var data = db.Query<TodayMailPieceChart>(query, new { startDate = todayDate.ToStartDateTime().EstToUtc(), endDate = todayDate.ToEndDateTime().EstToUtc() }).ToList();
            return data;
        }


        public List<MailPieces> GetMailPieceByParam(int batchId, string status)
        {
            //need to update
            string query = "[dbo].[uspweb_GetMailPieceByBatchIdAndStatus] @batchId, @status, @clientId";
            var data = db.Query<MailBatches, Clients, MailPieces, DocTypes, MailPieces>(query,
                (b, c, m, d) =>
                {
                    m.MailBatches = b;
                    m.Clients = c;
                    m.DocTypes = d;
                    return m;
                },
                new
                {
                    batchId = batchId,
                    status = status                    
                }).ToList();
            return data;
        }

        public List<BatchListByChartData> GetBatchByParam(DateTime date, string status)
        {
            var data = db.Query<BatchListByChartData>("select b.Id as [BatchId],b.ClientBatchId,b.[UniqueId] as [BatchUniqueId],b.BatchName,b.BatchStatus,b.DocumentOrPackageType,b.ReceivedOn,b.ValidationErrors, c.TimeZone as [ClientTimeZone],( select COUNT(Id) FROM [AM].[MailPieces]  WHERE [MailBatchId]=b.Id ) as MailPieceCount from [AM].[MailBatches] b left outer join [Common].[Clients] c ON b.ClientId=c.Id where BatchStatus=@status and ReceivedOn BETWEEN @startDate AND @enddate", new { startDate = date.ToStartDateTime().EstToUtc(), endDate = date.ToEndDateTime().EstToUtc(), status = status }).ToList();
            return data;
        }


        public List<MailPieces> GetMailPieceByBatchId(int batchId)
        {
            string query = "[dbo].[uspweb_GetMailPieceByBatchId] @batchId";
            var data = db.Query<MailBatches, Clients, MailPieces, DocTypes, MailPieces>(query,
                (b, c, m, d) =>
                {
                    m.MailBatches = b;
                    m.Clients = c;
                    m.DocTypes = d;
                    return m;
                },
                new
                {
                    batchId = batchId
                }).ToList();
            return data;
        }
    }
    public static class ChartDataServiceQuery
    {
        public static string GetWeeklyChartData = "select Id,BatchStatus,BatchName,ReceivedOn from AM.MailBatches Where ReceivedOn between @startDate and @endDate and ClientId=@clientId";

        public static string GetBatchByParam = "select b.Id as [BatchId],b.ClientBatchId,b.[UniqueId] as [BatchUniqueId],b.BatchName,b.BatchStatus,b.DocumentOrPackageType,b.ReceivedOn,b.ValidationErrors, c.TimeZone as [ClientTimeZone],( select COUNT(Id) FROM [AM].[MailPieces]  WHERE [MailBatchId]=b.Id ) as MailPieceCount from [AM].[MailBatches] b left outer join [Common].[Clients] c ON b.ClientId=c.Id where BatchStatus=@status and ClientId=@clientId and ReceivedOn BETWEEN @startDate AND @enddate";
    }
}
