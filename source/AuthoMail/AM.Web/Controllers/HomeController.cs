using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Security.Claims;
using AM.Entities.IdentityModels;
using AM.Web.USPSIMService;

namespace AM.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
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
        public ActionResult GetMailPieceIdByLoanNumber(string BusinessFriendlyIdName, string BusinessFriendlyIdValue, DateTime startDate, DateTime endDate)
        {
            var model = homeSvc.GetMailPieceIdFromBusinessFriendlyValue(BusinessFriendlyIdName, BusinessFriendlyIdValue, User.Identity.GetClientId().Value);

            if (model.Count > 1)
            {

                return Json("PullBack", JsonRequestBehavior.AllowGet);
            }
            else if (model.Count == 1)
            {
                var pullbackRequestData = pullBackSvc.GetPullBackRequestList(BusinessFriendlyIdName, BusinessFriendlyIdValue, null, startDate,endDate, User.Identity.GetClientId().Value);
                return PartialView("_PullBackRequestData", pullbackRequestData);
            }
            else
            {
                return Json("AdvancePullBack", JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult Search()
        {
            ViewBag.BusinessId = homeSvc.GetBusinessFriendlyIdNameByclientId(User.Identity.GetClientId().Value);
            ViewBag.DocType = new SelectList(homeSvc.GetDocTypeList(User.Identity.GetClientId().Value), "DocTypeCode", "DocTypeCode");
            return View();
        }
        [HttpPost]
        public ActionResult _GetSearchData(string BusinessFriendlyIdName, string BusinessFriendlyIdValue, DateTime receivedOnStart, DateTime receivedOnEnd,string status,string channel,string docType)
        {
            ViewBag.Date = string.Format("{0} To {1}", receivedOnStart.ToString("MM/dd/yyyy"), receivedOnEnd.ToString("MM/dd/yyyy"));
            var data = serSvc.GetMailPieceByParam(User.Identity.GetClientId().Value, BusinessFriendlyIdName, BusinessFriendlyIdValue,receivedOnStart,receivedOnEnd,docType,status,channel);
            //ViewBag.BusinessId = se.(User.Identity.GetClientId().Value);
            return PartialView(data);
        }
        [HttpPost]
        public ActionResult TodayChartDetails(string status, int batchId)
        {
            var model = chartDataSvc.GetMailPieceByParam(batchId, status, User.Identity.GetClientId().Value);
            return PartialView("_GetChartMailPieceData", model);
        }

        public ActionResult GetMailPieceByBatchId(int batchId)
        {
            var model = chartDataSvc.GetMailPieceByBatchId(batchId, User.Identity.GetClientId().Value);
            return PartialView("_GetChartMailPieceData", model);
        }
        [HttpPost]
        public ActionResult WeeklyChartDetails(string status, DateTime date)
        {
            var model = chartDataSvc.GetBatchByParam(date, status, User.Identity.GetClientId().Value);
            return PartialView(model);
        }
        [HttpPost]
        public ActionResult GetWeeklyChartData(DateTime startDate, DateTime endDate)
        {
            var model = chartDataSvc.GetWeeklyChartData(startDate, endDate, User.Identity.GetClientId().Value);
            return Json(model.OrderByDescending(s => s.Date).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetTodayChartData(DateTime date)
        {
            var model = chartDataSvc.GetTodayMailPieceChartData(date, User.Identity.GetClientId().Value);
            return Json(model.OrderByDescending(s => s.MailBatchId).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetMailPieceDataByStatusAndBatchId(string status, int batchId)
        {
            var model = chartDataSvc.GetMailPieceByParam(batchId, status, User.Identity.GetClientId().Value);
            return Json(model.OrderByDescending(s => s.BatchId).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult USPSTracking(string TrackingNo, string ShippingMethod)
        {
            if (ShippingMethod=="USPS")
            {
                 USPSIMClient uspsClient = new USPSIMClient();
                USPSTrackingInfo USPSInfo = uspsClient.GetShipmentStatus(TrackingNo);
                return Json(USPSInfo, JsonRequestBehavior.AllowGet);
            }
            return Json(Config.Msg_Web_NoRecordFound,JsonRequestBehavior.AllowGet);
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