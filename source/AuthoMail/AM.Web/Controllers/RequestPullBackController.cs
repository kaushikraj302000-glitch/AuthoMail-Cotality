using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AM.Entities.IdentityModels;
namespace AM.Web.Controllers
{
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
            ViewBag.DocType = new SelectList(homeSvc.GetDocTypeList(User.Identity.GetClientId().Value), "DocTypeCode", "DocTypeCode");
            ViewBag.BusinessId = homeSvc.GetBusinessFriendlyIdNameByclientId(User.Identity.GetClientId().Value);
            ViewBag.BusinessFriendlyIdName = BusinessFriendlyIdName;
            ViewBag.BusinessFriendlyIdValue = BusinessFriendlyIdValue;
            return View();
        }
        [HttpPost]
        public ActionResult _GetMailPieceList(string businessFriendlyIdName, string businessFriendlyIdValue, string docType, DateTime startDate, DateTime endDate)
        {
            ViewBag.Date = string.Format("{0} To {1}", startDate.ToString("MM/dd/yyyy"), endDate.ToString("MM/dd/yyyy"));
            //Ref only. remove it. also check doctype is required or not.
            var model = pullBackSvc.GetPullBackRequestList(businessFriendlyIdName, businessFriendlyIdValue, docType, startDate, endDate, User.Identity.GetClientId().Value);
            return PartialView(model);
        }
    }
}