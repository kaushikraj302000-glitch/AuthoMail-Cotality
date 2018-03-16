using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AM.Entities.IdentityModels;
using AM.Web.Models;

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
        public object GetWeeklyChartData([FromBody] ChartViewModel chartViewModel)
        {
            var model = chartDataSvc.GetWeeklyChartData(chartViewModel.StartDate, chartViewModel.EndDate, User.Identity.GetClientId().Value);
            return model.OrderByDescending(s => s.Date).ToList();
        }

        [Route("GetTodayChartData")]
        [HttpPost]
        public object GetTodayChartData([FromBody] DateTime date)
        {
            var model = chartDataSvc.GetTodayMailPieceChartData(date, User.Identity.GetClientId().Value);
            return model.OrderByDescending(s => s.MailBatchId).ToList();
        }
    }
}
