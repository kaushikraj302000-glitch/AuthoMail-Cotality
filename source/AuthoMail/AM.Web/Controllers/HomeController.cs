using AM.Services.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Security.Claims;

namespace AM.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        IHomeService homeSvc;
        IReportService repoSvc;
        public HomeController(IHomeService homeSvc, IReportService repoSvc)
        {
            this.homeSvc = homeSvc;
            this.repoSvc = repoSvc;
        }
        public ActionResult Index()
        {
            string clientId = ((ClaimsIdentity)User.Identity).FindFirst("clientId") != null ? ((ClaimsIdentity)User.Identity).FindFirst("clientId").Value : null;
            return View();
        }

        [HttpPost]
        public ActionResult GetMailPieceIdByLoanNumber(string loanNumber)
        {
            var model = repoSvc.GetMailPieceIdFromLoanNo(loanNumber);
            return Json(model, JsonRequestBehavior.AllowGet);
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