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
        public ActionResult _GetSummaryControlReport(string startDate,string endDate, string channel)
        {
            ViewBag.Date = string.Format("{0} to {1}", startDate, endDate);
            //Update startdate and enddate
            var model = repoSvc.GetSummaryControlReport(User.Identity.GetClientId().Value, DateTime.Parse(startDate), DateTime.Parse(endDate), channel);
            List<ReportViewModel> reportViewModel = new List<ReportViewModel>();
            foreach (var item in model)
            {
                var reportModel = (new ReportViewModel
                {
                    Key = item.BatchId,
                    Value = string.Format("{0} - {1}", item.ReceivedOn.ToString("H:mm"), item.BatchName)
                });
                if (!reportViewModel.Any(s => s.Value == reportModel.Value))
                {
                    reportViewModel.Add(reportModel);
                }
            }
            ViewBag.BatchList = new SelectList(reportViewModel, "Key", "Value");
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
        public ActionResult _MonthReconReport(string loanNumber, string docType, DateTime startDate, DateTime endDate, string channel)
        {
            ViewBag.Date = string.Format("{0} to {1}", startDate.ToString("MM/dd/yyyy"), endDate.ToString("MM/dd/yyyy"));
            var model = repoSvc.GetMonthlyReconReport(User.Identity.GetClientId().Value, startDate, endDate,channel);
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
        public ActionResult _GetDeliveryReport(string loanNumber, string docType, DateTime startDate, DateTime endDate)
        {
            ViewBag.Date = string.Format("{0} to {1}", startDate.ToString("MM/dd/yyyy"), endDate.ToString("MM/dd/yyyy"));

            var model = repoSvc.GetDailyDetailOperationsByDateOrBatch(User.Identity.GetClientId().Value, startDate, null, null);
            return PartialView(model);
        }
        public ActionResult ErrorsReport()
        {
            ViewBag.DocType = new SelectList(homeSvc.GetDocTypeList(User.Identity.GetClientId().Value), "Id", "DocTypeCode");
            return View();
        }
        [HttpPost]
        public ActionResult _GetErrorsReport(DateTime date, string channel)
        {
            ViewBag.Date = date.ToString("MM/dd/yyyy");
            var model = repoSvc.GetErrorReportByDateOrBatch(User.Identity.GetClientId().Value, date, null, channel);
            List<ReportViewModel> reportViewModel = new List<ReportViewModel>();
            foreach (var item in model)
            {
                var reportModel = (new ReportViewModel
                {
                    Key = item.BatchId,
                    Value = string.Format("{0} - {1}", item.ReceivedOn.ToString("H:mm"), item.BatchName)
                });
                if (!reportViewModel.Any(s => s.Value == reportModel.Value))
                {
                    reportViewModel.Add(reportModel);
                }
            }
            ViewBag.BatchList = new SelectList(reportViewModel, "Key", "Value");
            return PartialView(model);
        }

        public ActionResult ErrorSLAReport()
        {
            ViewBag.DocType = new SelectList(homeSvc.GetDocTypeList(User.Identity.GetClientId().Value), "DocTypeCode", "DocTypeCode");
            return View();
        }
        [HttpPost]
        public ActionResult _GetErrorsSLAReport(DateTime date, string filterBy, string docType)
        {
            ViewBag.Date = date.ToString("MM/dd/yyyy");
            
            //Update startDate and endDate
            var model = repoSvc.GetSLAMissErrorReportByDateOrBatch(User.Identity.GetClientId().Value,date, null,docType,filterBy);
            List<ReportViewModel> reportViewModel = new List<ReportViewModel>();
            foreach (var item in model)
            {
                var reportModel = (new ReportViewModel
                {
                    Key = item.BatchId,
                    Value = string.Format("{0} - {1}", item.ReceivedOn.ToString("H:mm"), item.BatchName)
                });
                if (!reportViewModel.Any(s => s.Value == reportModel.Value))
                {
                    reportViewModel.Add(reportModel);
                }
            }
            ViewBag.BatchList = new SelectList(reportViewModel, "Key", "Value");
            return PartialView(model);
        }
        public ActionResult DailyDetailOperationsReport()
        {
            return View();
        }
        [HttpPost]
        public ActionResult _GetDailyDetailOperationsReport(DateTime date, string channel)
        {
            ViewBag.Date = date.ToString("MM/dd/yyyy");
            var model = repoSvc.GetDailyDetailOperationsByDateOrBatch(User.Identity.GetClientId().Value, date, null, channel);
            List<ReportViewModel> reportViewModel = new List<ReportViewModel>();
            foreach (var item in model)
            {
                var reportModel = (new ReportViewModel
                {
                    Key = item.BatchId,
                    Value = string.Format("{0} - {1}", item.ReceivedOn.ToString("H:mm"), item.BatchName)
                });
                if (!reportViewModel.Any(s=>s.Value==reportModel.Value))
                {
                    reportViewModel.Add(reportModel);
                }
               
            }
            ViewBag.BatchList = new SelectList(reportViewModel, "Key", "Value");
            return PartialView(model);
        }
    }
}