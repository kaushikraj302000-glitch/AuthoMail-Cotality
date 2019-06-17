using AM.Scanner.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AM.Scanner.Models;
using System.IO;
using System.Configuration;
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
        [AllowAnonymous, HttpPost]
        public ActionResult SaveConverLetter()
        {
            try
            {

                if (Request.Params["param"].Replace("id=", "").Trim() == "c711c4aa-8559-44fa-9edc-88600f38b3c9")
                {
                    Request.Files[1].SaveAs(Path.Combine(ConfigurationManager.AppSettings["CoverLetterPath"].ToString(), Request.Files[0].FileName));
                    //System.IO.File.WriteAllBytes(Path.Combine(ConfigurationManager.AppSettings["CoverLetterPath"].ToString(), fileName), fileData);
                    return Json("File save successfully.", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Url is not valid.", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(e.Message, JsonRequestBehavior.AllowGet);
            }
        }
    }
}