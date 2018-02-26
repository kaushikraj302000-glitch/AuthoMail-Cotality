using AM.Services.Models;
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
        public ActionResult SummaryControlReport()
        {
            ViewBag.DocType = new SelectList(repoSvc.GetDocTypeList(), "Id", "DocTypeCode");
            return View();
        }
        [HttpPost]
        public ActionResult _GetBatchList(string loanNumber, string docType, DateTime startDate, DateTime endDate)
        {
            ViewBag.BatchId = loanNumber;
            ViewBag.Date = string.Format("{0} - {1}", startDate.ToString("MM/dd/yyyy"), endDate.ToString("MM/dd/yyyy"));

            var model = repoSvc.GetBatchByParam(loanNumber, docType, startDate, endDate);
            return PartialView(model);
        }
        public ActionResult _GetMailPieceList(List<MonthlyReconReport> model)
        {
            return PartialView(model);
        }
        public ActionResult MonthReconReport()
        {
            ViewBag.DocType = new SelectList(repoSvc.GetDocTypeList(), "Id", "DocTypeCode");
            return View();
        }
        [HttpPost]
        public ActionResult _MonthReconReport(string loanNumber, string docType, DateTime startDate, DateTime endDate)
        {
            ViewBag.Date = string.Format("{0} - {1}", startDate.ToString("MM/dd/yyyy"), endDate.ToString("MM/dd/yyyy"));
            var model = repoSvc.GetMailPieceByParam(loanNumber,docType,startDate,endDate);
            return PartialView("_GetMailPieceList", model);
        }

        public ActionResult MailPieceByBatchId(int batchId)
        {
            var model=repoSvc.GetMailPieceByBatchId(batchId);
            return View(model);
        }

        public ActionResult DeliveryReport()
        {
            ViewBag.DocType = new SelectList(repoSvc.GetDocTypeList(), "Id", "DocTypeCode");
            return View();
        }
        [HttpPost]
        public ActionResult _GetDeliveryReport(string loanNumber, string docType, DateTime startDate, DateTime endDate)
        {
            ViewBag.Date = string.Format("{0} - {1}", startDate.ToString("MM/dd/yyyy"), endDate.ToString("MM/dd/yyyy"));
            var model = repoSvc.GetDeliveryByParam(loanNumber, docType, startDate, endDate);
            return PartialView(model);
        }
        public ActionResult ErrorsReport()
        {
            ViewBag.DocType = new SelectList(repoSvc.GetDocTypeList(), "Id", "DocTypeCode");
            return View();
        }
        [HttpPost]
        public ActionResult _GetErrorsReport(string loanNumber, string docType, DateTime startDate, DateTime endDate)
        {
            ViewBag.Date = string.Format("{0} - {1}", startDate.ToString("MM/dd/yyyy"), endDate.ToString("MM/dd/yyyy"));
            var model = repoSvc.GetErroReportByParam(loanNumber, docType, startDate, endDate);
            return PartialView(model);
        }
        
    }
}