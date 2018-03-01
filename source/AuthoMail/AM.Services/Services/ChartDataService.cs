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
    }
    public class ChartDataService : ServiceBase, IChartDataService
    {
        public List<WeeklyChart> GetWeeklyChartData(DateTime startDate, DateTime endDate, int clientId)
        {
            var data = db.Query<WeeklyChart>(ChartDataServiceQuery.GetWeeklyChartData, new { startDate = startDate, endDate = endDate, clientId = clientId }).ToList();
            return data;
        }
    }
    public static class ChartDataServiceQuery
    {
        public static string GetWeeklyChartData = "select * from dbo.ufn_GetWeeklyChartData(@startDate,@endDate,@clientId)";
    }
}
