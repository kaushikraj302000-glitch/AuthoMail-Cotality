using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AM.Entities;
using System.Globalization;
using AM.Lib;
namespace AM.Web.Controllers
{
    [Authorize]
    public class RequestPullBackController : Controller
    {
        // GET: PullBackRequest
        IHomeService homeSvc;
        IReportService repoSvc;
        IPullBackService pullBackSvc;
        public RequestPullBackController(IHomeService homeSvc, IReportService repoSvc, IPullBackService pullBackSvc)
        {
            this.homeSvc = homeSvc;
            this.repoSvc = repoSvc;
            this.pullBackSvc = pullBackSvc;
        }
        public ActionResult PullBack(string BusinessFriendlyIdName,string BusinessFriendlyIdValue)
        {
            ViewBag.DocType = new SelectList(homeSvc.GetDocTypeList(User.Identity.GetClientId().Value), "Id", "DocTypeCode");
            ViewBag.BusinessId = homeSvc.GetBusinessFriendlyIdNameByclientId(User.Identity.GetClientId().Value);
            ViewBag.BusinessFriendlyIdName = BusinessFriendlyIdName;
            ViewBag.BusinessFriendlyIdValue = BusinessFriendlyIdValue;
            var estToday = DateTime.UtcNow.UtcToEst().Date;
            // EST Week
            ViewBag.startDate = (estToday + new TimeSpan(0, 0, 0)).ToString("MM/dd/yy");
            ViewBag.endDate = (estToday + new TimeSpan(23, 59, 59)).ToString("MM/dd/yy");
            return View();
        }
        [HttpPost]
        public ActionResult _GetMailPieceList(string businessFriendlyIdName, string businessFriendlyIdValue, string docType, string startDate, string endDate)
        {
            var exactStartDate = DateTime.ParseExact(startDate, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            var exactEndDate = DateTime.ParseExact(endDate, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
           
            //Ref only. remove it. also check doctype is required or not.
            var model = pullBackSvc.GetPullBackRequestList(businessFriendlyIdName, businessFriendlyIdValue, docType, exactStartDate, exactEndDate, User.Identity.GetClientId().Value);
            //return PartialView(model);
            return Json(JsonResponseFactory.SuccessResponse(new { model = model.OrderByDescending(s => s.ShippingStatus == "InProgress").ThenBy(s => s.IsPullBackRequested).ThenByDescending(s => s.ShippingStatus == "PulledBack").ThenByDescending(s => s.MailBatches.ReceivedOn) }), JsonRequestBehavior.AllowGet);
        }
    }
}