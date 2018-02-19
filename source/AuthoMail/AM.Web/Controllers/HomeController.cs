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
        IReportService repoSvc;
        public HomeController(IHomeService homeSvc, IReportService repoSvc)
        {
            this.homeSvc = homeSvc;
            this.repoSvc = repoSvc;
        }
        public ActionResult Index()
        {
            ViewBag.DocType = new SelectList(repoSvc.GetDocTypeList(), "Id", "DocTypeCode");
            return View();
        }
        public ActionResult _GetBatchList(string loanNumber, string docType, DateTime startDate, DateTime endDate)
        {
            return PartialView();
        }
        public ActionResult _GetBatchByDateRange()
        {
            DateTime startDate = DateTime.Now.AddDays(-5);
            DateTime endDate = DateTime.Now;
            if (startDate > endDate)
            {
                ViewBag.Message = "Invalid date selection.";
                return PartialView(null);
            }
            startDate = startDate.ToUniversalTime();
            endDate = endDate.ToUniversalTime();
            var q1 = repoSvc.GetBatchByParam(null, null, startDate, endDate);
            var q2 = repoSvc.GetMailPieceByBatchId(1000);
            var data = homeSvc.GetBatchReportInfoByDateRange(startDate, endDate);
            var d2 = homeSvc.GetMailPieceInfoByBatchId(0);
            return PartialView(data);
        }

        public ActionResult MailPiece()
        {
            //homeSvc.GetMailPieceByBatchId(batchId);
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