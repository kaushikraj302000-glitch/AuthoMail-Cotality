using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Security.Claims;
using AM.Entities.IdentityModels;
using AM.Services.Services;

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
        public ActionResult GetMailPieceIdByLoanNumber(string loanNumber)
        {
            var model = repoSvc.GetMailPieceIdFromLoanNo(loanNumber, User.Identity.GetClientId().Value);
            if (model.Count > 1)
            {
                return Json("PullBack", JsonRequestBehavior.AllowGet);
            }
            else
            {
                PullBackApiController pullBackAPI = new PullBackApiController(pullBackSvc);
                var data = pullBackAPI.GeneratePullBackRequest(model.ToList());
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Search()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ChartDetails(string status, List<int> batchIds)
        {
            return PartialView();
        }

        public ActionResult GetWeeklyChartData(DateTime startDate,DateTime endDate)
        {
            var model = chartDataSvc.GetWeeklyChartData(startDate, endDate, User.Identity.GetClientId().Value);
            return Json(true, JsonRequestBehavior.AllowGet);
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