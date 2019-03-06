using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AM.Entities;
using AM.Web.Models;
using System.Globalization;
using AM.Lib;
using AutoMapper;
using System.IO;
namespace AM.Web.Controllers
{
    [Authorize(Roles = AMRoles.Admin + "," + AMRoles.BA + "," + AMRoles.Client + "," + AMRoles.ClientUserManager + "," + AMRoles.CoreLogicUserManager + "," + AMRoles.PnSOperator)]
    public class ReportController : BaseController
    {
        IHomeService homeSvc;
        IReportService repoSvc;
        IInventoryService invSvc;
        public ReportController(IHomeService homeSvc, IReportService repoSvc, IInventoryService invSvc)
        {
            this.homeSvc = homeSvc;
            this.repoSvc = repoSvc;
            this.invSvc = invSvc;
        }
        public ActionResult SummaryControlReport()
        {
            return View();
        }
        [HttpPost]
        public ActionResult _GetSummaryControlReport(string startDate, string endDate, string channel)
        {
            var exactStartDate = DateTime.ParseExact(startDate, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            var exactEndDate = DateTime.ParseExact(endDate, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            var data = repoSvc.GetSummaryControlReport(User.Identity.GetClientId().Value, exactStartDate, exactEndDate, channel);

            var reportViewModel = GetBatchNameAndTime(data);
            var model = Mapper.Map<List<SummaryControlViewModel>>(data);

            return Json(JsonResponseFactory.SuccessResponse(new { BatchList = reportViewModel, model = model.OrderByDescending(s => s.ReceivedOn) }), JsonRequestBehavior.AllowGet);
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

        public ActionResult MonthlyReconReport()
        {
            var dateList = new List<string>();

            var currDate = DateTime.UtcNow.UtcToEst();
            if (currDate.Day <= 25) currDate = currDate.AddMonths(-1);

            for (var dt = currDate.Date; dt.Date >= currDate.AddMonths(-6).Date; dt = dt.AddMonths(-1).Date)
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

            var data = repoSvc.GetMonthlyReconReport(User.Identity.GetClientId().Value, exactStartDate, exactEndDate, channel);
            //var model=Mapper.Map<List<MonthlyReconReportViewModel>>(data);
            return Json(JsonResponseFactory.SuccessResponse(new { model = data.OrderByDescending(s => s.ReceivedOn) }), JsonRequestBehavior.AllowGet);

        }

        public ActionResult DownloadMonthlyReconReport(string startDate, string endDate, string channel, string fileName)
        {
            var exactStartDate = DateTime.ParseExact(startDate, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            var exactEndDate = DateTime.ParseExact(endDate, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            var data = repoSvc.GetMonthlyReconReport(User.Identity.GetClientId().Value, exactStartDate, exactEndDate, channel);
            var model = Mapper.Map<List<MonthlyReconReportViewModel>>(data);
            var dataTable = ExcelHelper.ToDataTable<MonthlyReconReportViewModel>(model);
            var memoryStream = ExcelHelper.ExportDataSet(dataTable);
            memoryStream.Seek(0, SeekOrigin.Begin);

            var fileNameArry = new string[] { fileName, exactStartDate.ToString("MMddyyy"), "TO", exactEndDate.ToString("MMddyyy"), channel };
            fileName = string.Join("_", fileNameArry.Where(s => s != "").ToArray());
            fileName = string.Format("{0}.xlsx", fileName);
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
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
                var data = repoSvc.GetErrorReportByDateOrBatch(User.Identity.GetClientId().Value, exactDate, null, channel);
                var reportViewModel = GetBatchNameAndTime(data);
                var model = Mapper.Map<List<ErrorReportViewModel>>(data);
                return Json(JsonResponseFactory.SuccessResponse(new { BatchList = reportViewModel, model = model.OrderByDescending(s => s.ReceivedOn) }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.ToString()), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ErrorSLAReport()
        {
            ViewBag.DocType = new SelectList(homeSvc.GetDocTypeList(User.Identity.GetClientId().Value), "Id", "DocTypeCode");
            return View();
        }
        [HttpPost]
        public ActionResult _GetErrorsSLAReport(string startDate, string endDate, string filterBy, string docType)
        {
            var exactStartDate = DateTime.ParseExact(startDate, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            var exactEndDate = DateTime.ParseExact(endDate, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            //Update startDate and endDate
            var data = repoSvc.GetSLAMissErrorReportByDateOrBatch(User.Identity.GetClientId().Value, exactStartDate, exactEndDate, null, docType, filterBy);
            var reportViewModel = GetBatchNameAndTime(data);
            var model = Mapper.Map<List<ErrorSLAReportViewModel>>(data);
            return Json(JsonResponseFactory.SuccessResponse(new { BatchList = reportViewModel, model = model.OrderByDescending(s => s.ReceivedOn) }), JsonRequestBehavior.AllowGet);
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
                var data = repoSvc.GetDailyDetailOperationsByDateOrBatch(User.Identity.GetClientId().Value, exactDate, null, channel);
                var reportViewModel = GetBatchNameAndTime(data);
                var model = Mapper.Map<List<DailyDetailReportViewModel>>(data);
                return Json(JsonResponseFactory.SuccessResponse(new { BatchList = reportViewModel, model = model.OrderByDescending(s => s.ReceivedOn) }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.ToString()), JsonRequestBehavior.AllowGet);
            }
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
            var model = repoSvc.GetPullbackRequestReport(User.Identity.GetClientId().Value, exactStartDate, exactEndDate);
            return Json(JsonResponseFactory.SuccessResponse(new { model = model.OrderByDescending(s => s.RequestedOn) }), JsonRequestBehavior.AllowGet);
        }
        public ActionResult SupplyInventoryReport()
        {
            ViewBag.Envelopes = new SelectList(invSvc.GetEnvelopes(), "Id", "Id");
            return View();
        }
        [HttpPost]
        public ActionResult _SupplyInventoryReport(List<string> envelopes)
        {
            var model = repoSvc.SupplyInventoryReport(envelopes);
            foreach (var item in model)
            {
                item.StockUsed = model.Where(m => m.EnvelopeId == item.EnvelopeId).Sum(m => m.ReceivedQuantity) - item.Envelopes.CurrentQuantity;
            }
            return Json(JsonResponseFactory.SuccessResponse(new { model = model }), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult ReportWizard()
        {
            ViewBag.BusinessId = homeSvc.GetBusinessFriendlyIdNameByclientId(User.Identity.GetClientId().Value);
            ViewBag.DocType = new SelectList(homeSvc.GetDocTypeList(User.Identity.GetClientId().Value), "Id", "DocTypeCode");
            return View();
        }
        [HttpGet]
        public JsonResult _ReportWizard(string businessFriendlyIdName, string businessFriendlyIdValue, DateTime? startDate, DateTime? endDate, List<int> docType, List<string> status, string channel, List<string> shippingMethod, string trackingNo)
        {
            var result = repoSvc.GetReportWizard(User.Identity.GetClientId().Value, businessFriendlyIdName, businessFriendlyIdValue, startDate, endDate, docType, status, channel, shippingMethod, trackingNo);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}