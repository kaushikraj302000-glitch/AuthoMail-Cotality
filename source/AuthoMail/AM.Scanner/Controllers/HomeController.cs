using AM.Scanner.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AM.Scanner.Models;
namespace AM.Scanner.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        //[HttpPost]
        //public ActionResult GetAllBatchForScan(string clientCode)
        //{
        //    var model = DBHelper.GetRecentBatchesStats(clientCode.Trim());
        //    return Json(model, JsonRequestBehavior.AllowGet);
        //}
        [HttpGet]
        public ActionResult GetScanCount(string clientCode)
        {
            using (new PerfTimerLogger("GetScanCount[HomeController]: "))
            {
                var model = DBHelper.GetScanStats(clientCode.Trim());
                return Json(model, JsonRequestBehavior.AllowGet);
            }
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