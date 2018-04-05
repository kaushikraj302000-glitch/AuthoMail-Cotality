using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AM.Entities.IdentityModels;
using AM.Web.Models;
using System.Globalization;
using AM.Lib;
namespace AM.Web.Controllers
{
    [Authorize]
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
            return View();
        }
        [HttpPost]
        public ActionResult _GetSummaryControlReport(string startDate, string endDate, string channel)
        {
            var exactStartDate = DateTime.ParseExact(startDate, "MM/dd/yy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            var exactEndDate = DateTime.ParseExact(endDate, "MM/dd/yy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);

            ViewBag.Date = string.Format("{0} to {1}", exactStartDate.ToString("MM/dd/yy HH:mm:ss"), exactEndDate.ToString("MM/dd/yy HH:mm:ss"));
            var model = repoSvc.GetSummaryControlReport(User.Identity.GetClientId().Value, exactStartDate, exactEndDate, channel);
            var reportViewModel = GetBatchNameAndTime(model);
            //ViewBag.BatchList = new SelectList(reportViewModel, "Key", "Value");
            //return PartialView(model);
            return Json(JsonResponseFactory.SuccessResponse(new { BatchList = reportViewModel, model = model.OrderBy(s => s.MailBatches.ReceivedOn) }), JsonRequestBehavior.AllowGet);
        }

        private static List<ReportViewModel> GetBatchNameAndTime(List<MailPieces> model)
        {
            List<ReportViewModel> reportViewModel = new List<ReportViewModel>();
            foreach (var item in model)
            {
                var reportModel = (new ReportViewModel
                {
                    Key = item.MailBatches.Id,
                    Value = string.Format("{0} - {1}", item.MailBatches.ReceivedOn.Value.ToString("H:mm"), item.MailBatches.BatchName)
                });
                if (!reportViewModel.Any(s => s.Value == reportModel.Value))
                {
                    reportViewModel.Add(reportModel);
                }
            }
            return reportViewModel;
        }
        public ActionResult _GetMailPieceList(List<MonthlyReconReport> model)
        {
            return PartialView(model);
        }
        public ActionResult MonthlyReconReport()
        {
            var dateList = new List<string>();

            for (var dt = DateTime.Now.Date; dt.Date >= DateTime.Now.AddMonths(-6).Date; dt = dt.AddMonths(-1).Date)
            {
                dateList.Add(string.Format("{0}/26/{1} - {2}/25/{3}", dt.Month, dt.Year, dt.AddMonths(1).Month, dt.AddMonths(1).Year));
            }
            ViewBag.dateList = dateList;
            return View();
        }
        [HttpPost]
        public ActionResult _MonthReconReport(string loanNumber, string docType, string startDate, string endDate, string channel)
        {
            var exactStartDate = DateTime.ParseExact(startDate, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            var exactEndDate = DateTime.ParseExact(endDate, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);

            ViewBag.Date = string.Format("{0} to {1}", exactStartDate.ToString("MM/dd/yy"), exactEndDate.ToString("MM/dd/yy"));
            var model = repoSvc.GetMonthlyReconReport(User.Identity.GetClientId().Value, exactStartDate, exactEndDate, channel);
            return Json(JsonResponseFactory.SuccessResponse(new { model = model.OrderBy(s => s.MailBatches.ReceivedOn) }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult MailPieceByBatchId(int batchId)
        {
            var model = homeSvc.GetMailPieceByBatchId(batchId, User.Identity.GetClientId().Value);
            return View(model);
        }

        public ActionResult DeliveryReport()
        {
            return View();
        }
        [HttpPost]
        public ActionResult _GetDeliveryReport(string loanNumber, string docType, DateTime startDate, DateTime endDate)
        {
            ViewBag.Date = string.Format("{0} to {1}", startDate.ToString("MM/dd/yy"), endDate.ToString("MM/dd/yy"));

            var model = repoSvc.GetDailyDetailOperationsByDateOrBatch(User.Identity.GetClientId().Value, startDate, null, null);
            return PartialView(model);
        }
        public ActionResult ErrorsReport()
        {
            return View();
        }
        [HttpPost]
        public ActionResult _GetErrorsReport(string date, string channel)
        {

            try
            {
                var exactDate = DateTime.ParseExact(date, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
                ViewBag.Date = exactDate.ToString("MM/dd/yy");
                var model = repoSvc.GetErrorReportByDateOrBatch(User.Identity.GetClientId().Value, exactDate, null, channel);
                var reportViewModel = GetBatchNameAndTime(model);
                return Json(JsonResponseFactory.SuccessResponse(new { BatchList = reportViewModel, model = model.OrderBy(s => s.MailBatches.ReceivedOn) }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                 return Json(JsonResponseFactory.ErrorResponse(e.ToString()), JsonRequestBehavior.AllowGet);
            }
            //ViewBag.BatchList = new SelectList(reportViewModel, "Key", "Value");
            //return PartialView(model);
        }

        public ActionResult ErrorSLAReport()
        {
            ViewBag.DocType = new SelectList(homeSvc.GetDocTypeList(User.Identity.GetClientId().Value), "Id", "DocTypeCode");
            return View();
        }
        [HttpPost]
        public ActionResult _GetErrorsSLAReport(string date, string filterBy, string docType)
        {
            var exactDate = DateTime.ParseExact(date, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            ViewBag.Date = exactDate.ToString("MM/dd/yy");

            //Update startDate and endDate
            var model = repoSvc.GetSLAMissErrorReportByDateOrBatch(User.Identity.GetClientId().Value, exactDate, null, docType, filterBy);
            var reportViewModel = GetBatchNameAndTime(model);
            return Json(JsonResponseFactory.SuccessResponse(new { BatchList = reportViewModel, model = model.OrderBy(s => s.MailBatches.ReceivedOn) }), JsonRequestBehavior.AllowGet);
        }
        public ActionResult DailyDetailOperationsReport()
        {
            return View();
        }
        [HttpPost]
        public ActionResult _GetDailyDetailOperationsReport(string date, string channel)
        {
            try
            {
                var exactDate = DateTime.ParseExact(date, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
                ViewBag.Date = exactDate.ToString("MM/dd/yy");
                var model = repoSvc.GetDailyDetailOperationsByDateOrBatch(User.Identity.GetClientId().Value, exactDate, null, channel);
                var reportViewModel = GetBatchNameAndTime(model);
                return Json(JsonResponseFactory.SuccessResponse(new { BatchList = reportViewModel, model = model.OrderBy(s => s.MailBatches.ReceivedOn) }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.ToString()), JsonRequestBehavior.AllowGet);
            }
            //return PartialView(model);
        }

        public ActionResult PullbackRequestReport()
        {
            var estToday = DateTime.UtcNow.UtcToEst().Date;
            // EST Week
            ViewBag.startDate = (estToday.AddDays(-7)).ToString("MM/dd/yy HH:mm:ss");
            ViewBag.endDate = (estToday + new TimeSpan(23, 59, 59)).ToString("MM/dd/yy HH:mm:ss");
            return View();
        }
        [HttpPost]
        public ActionResult _GetPullbackRequestReport(string startDate, string endDate)
        {
            var exactStartDate = DateTime.ParseExact(startDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            var exactEndDate = DateTime.ParseExact(endDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            ViewBag.Date = string.Format("{0} to {1}", exactStartDate.ToString("MM/dd/yy"), exactEndDate.ToString("MM/dd/yy"));
            //return PartialView(repoSvc.GetPullbackRequestReport(User.Identity.GetClientId().Value, exactStartDate, exactEndDate));
            var model = repoSvc.GetPullbackRequestReport(User.Identity.GetClientId().Value, exactStartDate, exactEndDate);
            return Json(JsonResponseFactory.SuccessResponse(new { model = model.OrderBy(s => s.RequestedOn) }), JsonRequestBehavior.AllowGet);
        }
    }
}