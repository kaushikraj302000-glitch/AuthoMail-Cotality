using AM.Services.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AM.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        IHomeService homeSvc;
        public HomeController(IHomeService homeSvc)
        {
            this.homeSvc = homeSvc;
        }
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult _GetDeliveryReport(DateTime startDate, DateTime endDate, string documentType)
        {
            if (startDate < endDate)
            {
                ViewBag.Message = "Invalid date selection.";
                return PartialView(null);
            }
            startDate = startDate.ToUniversalTime();
            endDate = endDate.ToUniversalTime();
            var data = homeSvc.GetDeliveryReportByDateAndDocumentType(startDate, endDate, documentType);
            return PartialView(data);
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