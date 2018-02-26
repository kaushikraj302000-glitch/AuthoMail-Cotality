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
        IPullBackService pullBackSvc;
        public HomeController(IHomeService homeSvc, IReportService repoSvc, IPullBackService pullBackSvc)
        {
            this.homeSvc = homeSvc;
            this.repoSvc = repoSvc;
            this.pullBackSvc = pullBackSvc;
        }
        public ActionResult Index()
        {
            string clientId = ((ClaimsIdentity)User.Identity).FindFirst("clientId") != null ? ((ClaimsIdentity)User.Identity).FindFirst("clientId").Value : null;
            return View();
        }

        [HttpPost]
        public ActionResult GetMailPieceIdByLoanNumber(string loanNumber)
        {
            var model = repoSvc.GetMailPieceIdFromLoanNo(loanNumber, 3);
            if (model.Count > 1)
            //if (model.Count != 1)
            {
                return Json("PullBack", JsonRequestBehavior.AllowGet);
            }
            else
            {
                PullBackApiController pullBackAPI = new PullBackApiController(pullBackSvc);
                var data = pullBackAPI.GeneratePullBackRequest(model);
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