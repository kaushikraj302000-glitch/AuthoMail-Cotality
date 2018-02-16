using AM.Services.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AM.Web.Controllers
{
    public class HomeController : Controller
    {
        IHomeService homeSvc;
        public HomeController(IHomeService homeSvc)
        {
            this.homeSvc = homeSvc;
        }
        [Authorize]
        public ActionResult Index()
        {
            var data = homeSvc.GetDeliveryReportByBatchClustId("");
            return View();
        }
        public ActionResult MailPiece()
        {
            //var data = homeSvc.GetDeliveryReportByBatchId("");
            return View();
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