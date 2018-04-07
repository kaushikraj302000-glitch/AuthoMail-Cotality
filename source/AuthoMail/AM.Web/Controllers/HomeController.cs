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
using System.Globalization;

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
        public ActionResult GetMailPieceIdByLoanNumber(string BusinessFriendlyIdName, string BusinessFriendlyIdValue, string startDate, string endDate)
        {
            try
            {
                var exactStartDate = DateTime.ParseExact(startDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
                var exactEndDate = DateTime.ParseExact(endDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
                var model = homeSvc.GetMailPieceIdFromBusinessFriendlyValue(BusinessFriendlyIdName, BusinessFriendlyIdValue, User.Identity.GetClientId().Value);

                if (model.Count > 1)
                {
                    return Json(JsonResponseFactory.SuccessResponse("PullBack"), JsonRequestBehavior.AllowGet);
                }
                else if (model.Count == 1)
                {
                    var pullbackRequestData = pullBackSvc.GetPullBackRequestList(BusinessFriendlyIdName, BusinessFriendlyIdValue, null, exactStartDate, exactEndDate, User.Identity.GetClientId().Value);
                    return Json(JsonResponseFactory.SuccessResponse(new { model = pullbackRequestData.OrderBy(s => s.MailBatches.ReceivedOn) }), JsonRequestBehavior.AllowGet);
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
        [HttpPost]
        public ActionResult _GetSearchData(string BusinessFriendlyIdName, string BusinessFriendlyIdValue, string receivedOnStart, string receivedOnEnd, string status, string channel, string docType)
        {
            var exactStartDate = DateTime.ParseExact(receivedOnStart, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            var exactEndDate = DateTime.ParseExact(receivedOnEnd, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            var data = serSvc.GetMailPieceByParam(User.Identity.GetClientId().Value, BusinessFriendlyIdName, BusinessFriendlyIdValue, exactStartDate, exactEndDate, docType, status, channel);

            return Json(JsonResponseFactory.SuccessResponse(new { model = data.OrderBy(s => s.MailBatches.ReceivedOn) }), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult TodayChartDetails(string status, int batchId)
        {
            var model = chartDataSvc.GetMailPieceByParam(batchId, status, User.Identity.GetClientId().Value);
            return Json(JsonResponseFactory.SuccessResponse(new { model = model.OrderBy(s => s.MailBatches.ReceivedOn) }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetMailPieceByBatchId(int batchId)
        {
            var model = chartDataSvc.GetMailPieceByBatchId(batchId, User.Identity.GetClientId().Value);
            return Json(JsonResponseFactory.SuccessResponse(new { model = model.OrderBy(s => s.MailBatches.ReceivedOn) }), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult WeeklyChartDetails(string status, string date)
        {
            var exactDate = DateTime.ParseExact(date, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            var model = chartDataSvc.GetBatchByParam(exactDate, status, User.Identity.GetClientId().Value);
            return Json(JsonResponseFactory.SuccessResponse(new { model = model.OrderBy(s => s.ReceivedOn) }), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetMailPieceDataByStatusAndBatchId(string status, int batchId)
        {
            var model = chartDataSvc.GetMailPieceByParam(batchId, status, User.Identity.GetClientId().Value);
            return Json(model.OrderByDescending(s => s.MailBatches.Id).ToList(), JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
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