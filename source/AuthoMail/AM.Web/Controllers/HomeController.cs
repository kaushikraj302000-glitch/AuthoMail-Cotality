using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Security.Claims;
using AM.Entities;
using AM.Web.USPSIMService;
using System.Globalization;
using AM.Lib;
using AM.Web.Models;
using AutoMapper;
using AM.Web.AMHub;
using System.Threading.Tasks;
using System.IO;

namespace AM.Web.Controllers
{
    [Authorize(Roles = AMRoles.Admin + "," + AMRoles.BA + "," + AMRoles.Client + "," + AMRoles.ClientUserManager + "," + AMRoles.CoreLogicUserManager + "," + AMRoles.PnSOperator)]
    [RoutePrefix("AuthoMailHealth")]
    public class HomeController : BaseController
    {
        IHomeService homeSvc;
        IPullBackService pullBackSvc;
        IChartDataService chartDataSvc;
        ISearchService serSvc;
        public HomeController(IHomeService homeSvc, IPullBackService pullBackSvc, IChartDataService chartDataSvc, ISearchService serSvc)
        {
            this.homeSvc = homeSvc;
            this.pullBackSvc = pullBackSvc;
            this.chartDataSvc = chartDataSvc;
            this.serSvc = serSvc;
        }
        public ActionResult Index()
        {
            ViewBag.BusinessId = homeSvc.GetBusinessFriendlyIdNameByclientId(User.Identity.GetClientId().Value);
            return View();
        }

        [HttpPost]
        public ActionResult GetMailPieceIdByLoanNumber(string BusinessFriendlyIdName, string BusinessFriendlyIdValue)
        {
            try
            {
                var estToday = DateTime.UtcNow.UtcToEst().Date;
                // EST Start End
                var startDate = (estToday + new TimeSpan(0, 0, 0));
                var endDate = (estToday + new TimeSpan(23, 59, 59));
                var model = homeSvc.GetMailPieceIdFromBusinessFriendlyValue(BusinessFriendlyIdName, BusinessFriendlyIdValue, User.Identity.GetClientId().Value);

                if (model.Count > 1)
                {
                    return Json(JsonResponseFactory.SuccessResponse("PullBack"), JsonRequestBehavior.AllowGet);
                }
                else if (model.Count == 1)
                {
                    var pullbackRequestData = pullBackSvc.GetPullBackRequestList(BusinessFriendlyIdName, BusinessFriendlyIdValue, null, startDate, endDate, User.Identity.GetClientId().Value);
                    var data = Mapper.Map<List<MailPieceViewModel>>(pullbackRequestData);
                    return Json(JsonResponseFactory.SuccessResponse(new { model = data.OrderBy(s => s.ReceivedOn) }), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(JsonResponseFactory.SuccessResponse("AdvancePullBack"), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.ToString()), JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult Search()
        {
            ViewBag.BusinessId = homeSvc.GetBusinessFriendlyIdNameByclientId(User.Identity.GetClientId().Value);
            ViewBag.DocType = new SelectList(homeSvc.GetDocTypeList(User.Identity.GetClientId().Value), "Id", "DocTypeCode");
            return View();
        }
        [HttpGet]
        public ActionResult GetMailPieceByUniqueId(string mailPieceId)
        {
            var data = serSvc.GetMailPieceByUniqueId(new Guid(mailPieceId));
            var model = Mapper.Map<MailPieceDetailsViewModel>(data);
            return Json(JsonResponseFactory.SuccessResponse(new { model = model }), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult _GetSearchData(string BusinessFriendlyIdName, string BusinessFriendlyIdValue, string receivedOnStart, string receivedOnEnd, string status, string channel, List<int> docType, string trackingNumber, string clientBatchId, string batchId, List<string> errorCode)
        {
            try
            {
                int? mailBatchId = batchId.IsNullOrWhiteSpace() ? (int?)null : Convert.ToInt32(batchId);
                var exactStartDate = receivedOnStart != string.Empty ? DateTime.ParseExact(receivedOnStart, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces) : (DateTime?)null;
                var exactEndDate = receivedOnEnd != string.Empty ? DateTime.ParseExact(receivedOnEnd, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces) : (DateTime?)null;
                var data = serSvc.GetMailPieceByParam(User.Identity.GetClientId().Value, BusinessFriendlyIdName, BusinessFriendlyIdValue, exactStartDate, exactEndDate, docType, status, channel, clientBatchId, mailBatchId, trackingNumber, errorCode,false);
                
                //var model = Mapper.Map<List<SearchViewModel>>(data);

                return Json(JsonResponseFactory.SuccessResponse(new { model = data }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.ToString()), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult _DownloadSearchData(string BusinessFriendlyIdName, string BusinessFriendlyIdValue, string receivedOnStart, string receivedOnEnd, string status, string channel, List<int> docType, string trackingNumber, string clientBatchId, string batchId, List<string> errorCode)
        {
            try
            {
                int? mailBatchId = batchId.IsNullOrWhiteSpace() ? (int?)null : Convert.ToInt32(batchId);
                var exactStartDate = receivedOnStart != string.Empty ? DateTime.ParseExact(receivedOnStart, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces) : (DateTime?)null;
                var exactEndDate = receivedOnEnd != string.Empty ? DateTime.ParseExact(receivedOnEnd, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces) : (DateTime?)null;
                var data = serSvc.GetMailPieceByParam(User.Identity.GetClientId().Value, BusinessFriendlyIdName, BusinessFriendlyIdValue, exactStartDate, exactEndDate, docType, status, channel, clientBatchId, mailBatchId, trackingNumber, errorCode,true);
                string fileGuid = Guid.NewGuid().ToString();
                var model = Mapper.Map<List<SearchViewModel>>(data.Value);
                var dataTable = ExcelHelper.ToDataTable<SearchViewModel>(model);
                using (MemoryStream memoryStream = ExcelHelper.ExportDataSet(dataTable))
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    TempData[fileGuid] = memoryStream.ToArray();
                }  
                
                return Json(JsonResponseFactory.SuccessResponse(new { model = new { FileGuid = fileGuid, FileName = "Search.xlsx" } }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.ToString()), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult DownloadSearch(string fileGuid, string fileName)
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
        [HttpPost]
        public ActionResult TodayChartDetails(string status, int batchId)
        {
            var data = chartDataSvc.GetMailPieceByParam(batchId, status, User.Identity.GetClientId().Value);

            var model = Mapper.Map<List<MailPieceViewModel>>(data);
            return Json(JsonResponseFactory.SuccessResponse(new { model = model.OrderByDescending(s => s.ReceivedOn) }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetMailPieceByBatchId(int batchId)
        {
            var data = chartDataSvc.GetMailPieceByBatchId(batchId, User.Identity.GetClientId().Value);
            var model = Mapper.Map<List<MailPieceViewModel>>(data);
            return Json(JsonResponseFactory.SuccessResponse(new { model = model.OrderByDescending(s => s.ReceivedOn) }), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult WeeklyChartDetails(string status, string date)
        {
            var exactDate = DateTime.ParseExact(date, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            var model = chartDataSvc.GetBatchByParam(exactDate, status, User.Identity.GetClientId().Value);
            return Json(JsonResponseFactory.SuccessResponse(new { model = model.OrderByDescending(s => s.ReceivedOn) }), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetMailPieceDataByStatusAndBatchId(string status, int batchId)
        {
            var model = chartDataSvc.GetMailPieceByParam(batchId, status, User.Identity.GetClientId().Value);
            return Json(model.OrderByDescending(s => s.MailBatches.Id).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DailySummaryChartDetails(string status, string docTypeCode)
        {
            var estToday = DateTime.UtcNow.UtcToEst().Date;
            var data = chartDataSvc.GetMailPiecesByDocTypeIdAndStatus(estToday, status, User.Identity.GetClientId().Value, docTypeCode);
            var model = Mapper.Map<List<MailPieceViewModel>>(data);
            return Json(JsonResponseFactory.SuccessResponse(new { model = model.OrderByDescending(s => s.ReceivedOn) }), JsonRequestBehavior.AllowGet);
        }


        [AllowAnonymous, Route("37f9ac3e-40b3-4ae8-ac9a-d84634638065")]
        public ActionResult AMHealth()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpGet, Route("getamstatusdetail")]
        public JsonResult AMStatusDetail()
        {
            try
            {
                return Json(AMAlert.Instance.GetGlobalStatus, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
                //return Json(GetData(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet, Route("getamstatus")]
        public JsonResult AMStatus()
        {
            try
            {
                return Json(AMAlert.Instance.GetAMHealth, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
                //var _model = GetData();
                //return Json(new AMHealth { AlertLevel = _model.AlertLevel, IsAMDelay = _model.IsAMDelay, LastUpdatedOn = _model.LastUpdatedOn }, JsonRequestBehavior.AllowGet);
            }
        }
        private AMGlobalStatus GetData()
        {
            using (IMonitorBatchesService alertSvc = new MonitorBatchesService())
            {
                var data = alertSvc.GetPendingBatchesToSubmit(30);
                return new AMGlobalStatus { DelayedProcessBatches = data.DelayedBatches.Count(), DelayedProcessMailPieces = data.DelayedMailPieces.Count(), DelayedAMReportBackBatches = data.DelayedAMReportBackBatches.Count(), DelayedAMReportBackMailPieces = data.DelayedAMReportBackMailPieces.Count(), DelayedPrinterBatches = data.DelayedPrinterBatches.Count(), DelayedPickUpMailPieces = data.DelayedPrinterMailPieces.Count(), MinHour = data.MinHour, MaxHour = data.MaxHour, LastBatchReceivedOn = data.LastBatchReceivedOn, LastUpdatedOn = data.LastUpdatedOn, IsReportBackDelay = data.IsReportBackDelay, IsPrinterDelay = data.IsPrinterDelay, IsReceivedDelay = data.IsReceivedDelay, IsProcessDelay = data.IsProcessDelay, AlertLevel = data.AlertLevel, IsAMDelay = data.IsAMDelay };
            }
        }
    }
}