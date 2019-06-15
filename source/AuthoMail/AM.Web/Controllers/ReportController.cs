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
            ViewBag.dateList = repoSvc.GetMonthlyReconDate(User.Identity.GetClientId().Value);
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

        public ActionResult DownloadMonthlyReconReport(string startDate, string endDate, string channel, string fileName, int timezoneOffset)
        {
            var exactStartDate = DateTime.ParseExact(startDate, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            var exactEndDate = DateTime.ParseExact(endDate, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            var data = repoSvc.GetMonthlyReconReport(User.Identity.GetClientId().Value, exactStartDate, exactEndDate, channel);
            var model = Mapper.Map<List<MonthlyReconReportViewModel>>(data);
            model.ForEach(m =>
            {
                m.ReceivedOn = m.ReceivedOn.HasValue ? m.ReceivedOn.Value.AddMinutes(-timezoneOffset) : m.ReceivedOn;
                m.ShippedOn = m.ShippedOn.HasValue ? m.ShippedOn.Value.AddMinutes(-timezoneOffset) : m.ShippedOn;
            });
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
            if (User.IsInRole(AMRoles.Admin))
            {
                ViewBag.ClientId = new SelectList(homeSvc.GetAllActiveClient(), "Id", "Name");
            }
            else
            {
                ViewBag.ClientId = User.Identity.GetClientId();// new SelectList(homeSvc.GetAllActiveClient().Where(s => s.Id == User.Identity.GetClientId()), "Id", "Name", );
            }
            return View();
        }
        [HttpPost]
        public JsonResult _ReportWizard(List<int> clientId, string businessFriendlyIdName, string businessFriendlyIdValue, DateTime? startDate, DateTime? endDate, List<int> docType, string status, string channel, List<string> shippingMethod, string trackingNo)
        {

            try
            {
                var result = repoSvc.GetReportWizard(clientId, businessFriendlyIdName, businessFriendlyIdValue, startDate, endDate, docType, status, channel, shippingMethod, trackingNo);
                return Json(JsonResponseFactory.SuccessResponse(new { model = result }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(JsonResponseFactory.ErrorResponse(ex.Message), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult _DownloadReportWizard(List<int> clientId, string businessFriendlyIdName, string businessFriendlyIdValue, string startDate, string endDate, List<int> docType, string status, string channel, List<string> shippingMethod, string trackingNo, int timezoneOffset)
        {
            try
            {
                var exactStartDate = startDate != string.Empty ? DateTime.ParseExact(startDate, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces) : (DateTime?)null;
                var exactEndDate = endDate != string.Empty ? DateTime.ParseExact(endDate, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces) : (DateTime?)null;
                var result = repoSvc.GetReportWizard(clientId, businessFriendlyIdName, businessFriendlyIdValue, exactStartDate, exactEndDate, docType, status, channel, shippingMethod, trackingNo);

                result.ForEach(m =>
                {
                    m.ReceivedOn = m.ReceivedOn.HasValue ? m.ReceivedOn.Value.AddMinutes(-timezoneOffset) : m.ReceivedOn;
                    m.PrintedOn = m.PrintedOn.HasValue ? m.PrintedOn.Value.AddMinutes(-timezoneOffset) : m.PrintedOn;
                    m.MailedOn = m.MailedOn.HasValue ? m.MailedOn.Value.AddMinutes(-timezoneOffset) : m.MailedOn;
                });
                var dataTable = ExcelHelper.ToDataTable<ReportWizard>(result);
                var memoryStream = ExcelHelper.ExportDataSet(dataTable);
                memoryStream.Seek(0, SeekOrigin.Begin);

                string fileGuid = Guid.NewGuid().ToString();
                TempData[fileGuid] = memoryStream.ToArray();
                string fileName = string.Empty;
                if (!startDate.IsNullOrWhiteSpace())
                {
                    string[] fileNameArray = { "ReportWizard", exactStartDate.Value.ToString("MMddyyyy"), "To", exactEndDate.Value.ToString("MMddyyyy") };
                    fileName = string.Join("_", fileNameArray);
                }
                else
                {
                    string[] fileNameArray = { "ReportWizard", DateTime.UtcNow.AddMinutes(-timezoneOffset).ToString("MMddyyyy") };
                    fileName = string.Join("_", fileNameArray);
                }
                fileName = string.Format("{0}.xlsx", fileName);
                return Json(JsonResponseFactory.SuccessResponse(new { model = new { FileGuid = fileGuid, FileName = fileName } }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult DownloadReportWizard(string fileGuid, string fileName)
        {
            if (TempData[fileGuid] != null)
            {
                byte[] data = TempData[fileGuid] as byte[];
                return File(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            else
            {
                return new EmptyResult();
            }

        }
    }
}