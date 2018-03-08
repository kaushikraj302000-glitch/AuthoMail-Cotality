using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AM.Entities.IdentityModels;
using AM.Web.Models;
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
            ViewBag.DocType = new SelectList(homeSvc.GetDocTypeList(User.Identity.GetClientId().Value), "Id", "DocTypeCode");
            return View();
        }
        [HttpPost]
        public ActionResult _GetBatchList(string date)
        {
            ViewBag.Date = date;

            var model = repoSvc.GetSummaryControlReport(User.Identity.GetClientId().Value,Convert.ToDateTime(date));
            return PartialView(model);
        }
        public ActionResult _GetMailPieceList(List<MonthlyReconReport> model)
        {
            return PartialView(model);
        }
        public ActionResult MonthReconReport()
        {
            ViewBag.DocType = new SelectList(homeSvc.GetDocTypeList(User.Identity.GetClientId().Value), "Id", "DocTypeCode");
            return View();
        }
        [HttpPost]
        public ActionResult _MonthReconReport(string loanNumber, string docType, string startDate, string endDate)
        {
            ViewBag.Date = string.Format("{0} To {1}", startDate, endDate);
            var model = repoSvc.GetMonthlyReconReport(User.Identity.GetClientId().Value, Convert.ToDateTime(startDate), Convert.ToDateTime(endDate));
            return PartialView("_GetMailPieceList", model);
        }

        public ActionResult MailPieceByBatchId(int batchId)
        {
            var model = homeSvc.GetMailPieceByBatchId(batchId, User.Identity.GetClientId().Value);
            return View(model);
        }

        public ActionResult DeliveryReport()
        {
            ViewBag.DocType = new SelectList(homeSvc.GetDocTypeList(User.Identity.GetClientId().Value), "Id", "DocTypeCode");
            return View();
        }
        [HttpPost]
        public ActionResult _GetDeliveryReport(string loanNumber, string docType, string startDate, string endDate)
        {
            ViewBag.Date = string.Format("{0} To {1}", startDate, endDate);

            var model = repoSvc.GetDailyDetailOperationsByDateOrBatch(User.Identity.GetClientId().Value,Convert.ToDateTime( startDate), null);
            return PartialView(model);
        }
        public ActionResult ErrorsReport()
        {
            ViewBag.DocType = new SelectList(homeSvc.GetDocTypeList(User.Identity.GetClientId().Value), "Id", "DocTypeCode");
            return View();
        }
        [HttpPost]
        public ActionResult _GetErrorsReport(string date)
        {
            ViewBag.Date = date;
            var model = repoSvc.GetErrorReportByDateOrBatch(User.Identity.GetClientId().Value, Convert.ToDateTime(date), null);
            List<ReportViewModel> reportViewModel = new List<ReportViewModel>();
            foreach (var item in model)
            {
                reportViewModel.Add(new ReportViewModel
                {
                    Key = item.BatchId,
                    Value = string.Format("{0} - {1}", item.ReceivedOn.ToString("H:mm"), item.BatchName)
                });
            }
            ViewBag.BatchList = new SelectList(reportViewModel, "Key", "Value");
            return PartialView(model);
        }

        public ActionResult ErrorSLAReport()
        {
            return View();
        }
        [HttpPost]
        public ActionResult _GetErrorsSLAReport(string date)
        {
            ViewBag.Date = date;
            var model = repoSvc.GetErrorReportByDateOrBatch(User.Identity.GetClientId().Value, Convert.ToDateTime(date), null);
            List<ReportViewModel> reportViewModel = new List<ReportViewModel>();
            foreach (var item in model)
            {
                reportViewModel.Add(new ReportViewModel
                {
                    Key = item.BatchId,
                    Value = string.Format("{0} - {1}", item.ReceivedOn.ToString("H:mm"), item.BatchName)
                });
            }
            ViewBag.BatchList = new SelectList(reportViewModel, "Key", "Value");
            return PartialView(model);
        }
        public ActionResult DailyDetailOperationsReport()
        {
            ViewBag.DocType = new SelectList(homeSvc.GetDocTypeList(User.Identity.GetClientId().Value), "Id", "DocTypeCode");
            return View();
        }
        [HttpPost]
        public ActionResult _GetDailyDetailOperationsReport(string date)
        {
            ViewBag.Date = date;
            var model = repoSvc.GetDailyDetailOperationsByDateOrBatch(User.Identity.GetClientId().Value, Convert.ToDateTime(date), null);
            List<ReportViewModel> reportViewModel = new List<ReportViewModel>();
            foreach (var item in model)
            {
                reportViewModel.Add(new ReportViewModel { 
                Key=item.BatchId,
                Value = string.Format("{0} - {1}", item.ReceivedOn.ToString("H:mm"), item.BatchName)
                });
            }
            ViewBag.BatchList = new SelectList(reportViewModel, "Key", "Value");
            return PartialView(model);
        }
    }
}