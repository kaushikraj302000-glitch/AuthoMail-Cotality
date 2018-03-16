using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AM.Entities.IdentityModels;
using AM.Web.Models;
using AM.Lib;
namespace AM.Web.Controllers
{
    [RoutePrefix("api/chart")]
    public class ChartApiController : ApiController
    {
        IChartDataService chartDataSvc;
        public ChartApiController(IChartDataService chartDataSvc)
        {
            this.chartDataSvc = chartDataSvc;
        }
        [Route("GetDateTime")]
        [HttpPost]
        public object GetDateTime()
        {
            return DateTime.Now;
        }
        [Route("GetWeeklyChartData")]
        [HttpPost]
        public object GetWeeklyChartData()
        {
            var estToday = DateTime.UtcNow.UtcToEst().Date;
            // EST Week
            var estWeekStart = (estToday.AddDays(-7)).EstToUtc();
            var estWeekEnd = (estToday + new TimeSpan(23, 59, 59)).EstToUtc();
            var model = chartDataSvc.GetWeeklyChartData(estWeekStart, estWeekEnd, User.Identity.GetClientId().Value);
            return model.OrderByDescending(s => s.Date).ToList();
        }

        [Route("GetTodayChartData")]
        [HttpPost]
        public object GetTodayChartData()
        {
            var estToday = DateTime.UtcNow.UtcToEst().Date;
            var model = chartDataSvc.GetTodayMailPieceChartData(estToday, User.Identity.GetClientId().Value);
            return model.OrderByDescending(s => s.MailBatchId).ToList();
        }
    }
}
