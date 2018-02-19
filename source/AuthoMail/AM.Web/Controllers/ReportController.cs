using AM.Services.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AM.Web.Controllers
{
    public class ReportController : Controller
    {
        // GET: Report
        IHomeService homeSvc;
        IReportService repoSvc;
        public ReportController(IHomeService homeSvc, IReportService repoSvc)
        {
            this.homeSvc = homeSvc;
            this.repoSvc = repoSvc;
        }
        public ActionResult BatchReport()
        {
            ViewBag.DocType = new SelectList(repoSvc.GetDocTypeList(), "Id", "DocTypeCode");
            return View();
        }
        public ActionResult _GetBatchList(string loanNumber, string docType, DateTime startDate, DateTime endDate)
        {
            return PartialView();
        }
        public ActionResult MailPieceReport()
        {
            //homeSvc.GetMailPieceByBatchId(batchId);
            return View();
        }
    }
}