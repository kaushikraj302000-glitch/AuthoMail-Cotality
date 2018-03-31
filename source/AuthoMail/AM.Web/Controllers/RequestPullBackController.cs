using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AM.Entities.IdentityModels;
using System.Globalization;
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
            return View();
        }
        [HttpPost]
        public ActionResult _GetMailPieceList(string businessFriendlyIdName, string businessFriendlyIdValue, string docType, string startDate, string endDate)
        {
            var exactStartDate = DateTime.ParseExact(startDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            var exactEndDate = DateTime.ParseExact(endDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            ViewBag.Date = string.Format("{0} to {1}", exactStartDate.ToString("MM/dd/yy"), exactEndDate.ToString("MM/dd/yy"));
            //Ref only. remove it. also check doctype is required or not.
            var model = pullBackSvc.GetPullBackRequestList(businessFriendlyIdName, businessFriendlyIdValue, docType, exactStartDate, exactEndDate, User.Identity.GetClientId().Value);
            return PartialView(model);
        }
    }
}