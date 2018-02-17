using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Security.Claims;
using AM.Entities.IdentityModels;

namespace AM.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        IHomeService homeSvc;
        IReportService repoSvc;
        IPullBackService pullBackSvc;
        IChartDataService chartDataSvc;
        public HomeController(IHomeService homeSvc, IReportService repoSvc, IPullBackService pullBackSvc, IChartDataService chartDataSvc)
        {
            this.homeSvc = homeSvc;
            this.repoSvc = repoSvc;
            this.pullBackSvc = pullBackSvc;
            this.chartDataSvc = chartDataSvc;
        }
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetMailPieceIdByLoanNumber(string BusinessFriendlyIdName, string BusinessFriendlyIdValue, DateTime startDate, DateTime endDate)
        {
            var model = repoSvc.GetMailPieceIdFromLoanNo(BusinessFriendlyIdValue, User.Identity.GetClientId().Value);

            if (model.Count > 1)
            {

                return Json("PullBack", JsonRequestBehavior.AllowGet);
            }
            else if (model.Count == 1)
            {
                var pullbackRequestData = pullBackSvc.GetPullBackRequestList(BusinessFriendlyIdValue, null, startDate, endDate, User.Identity.GetClientId().Value);
                return PartialView("_PullBackRequestData", pullbackRequestData);
            }
            else
            {
                return Json("AdvancePullBack", JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult Search()
        {
            return View();
        }

        [HttpPost]
        public ActionResult TodayChartDetails(string status, int batchId)
        {
            var model = chartDataSvc.GetMailPieceByParam(batchId, status, User.Identity.GetClientId().Value);
            return PartialView("_GetChartMailPieceData",model);
        }

        public ActionResult GetMailPieceByBatchId(int batchId)
        {
            var model = chartDataSvc.GetMailPieceByBatchId(batchId, User.Identity.GetClientId().Value);
            return PartialView("_GetChartMailPieceData", model);
        }
        [HttpPost]
        public ActionResult WeeklyChartDetails(string status, DateTime date)
        {
            var model = chartDataSvc.GetBatchByParam(date, status, User.Identity.GetClientId().Value);
            return PartialView(model);
        }
        [HttpPost]
        public ActionResult GetWeeklyChartData(DateTime startDate, DateTime endDate)
        {
            var model = chartDataSvc.GetWeeklyChartData(startDate, endDate, User.Identity.GetClientId().Value);
            return Json(model.OrderByDescending(s => s.Date).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetTodayChartData(DateTime date)
        {
            var model = chartDataSvc.GetTodayMailPieceChartData(date, User.Identity.GetClientId().Value);
            return Json(model.OrderByDescending(s => s.BatchId).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetMailPieceDataByStatusAndBatchId(string status, int batchId)
        {
            var model = chartDataSvc.GetMailPieceByParam(batchId, status, User.Identity.GetClientId().Value);
            return Json(model.OrderByDescending(s => s.BatchId).ToList(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}