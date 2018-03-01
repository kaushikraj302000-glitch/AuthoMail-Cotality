using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AM.Entities.IdentityModels;
namespace AM.Web.Controllers
{
    public class PullBackRequestController : Controller
    {
        // GET: PullBackRequest
        IHomeService homeSvc;
        IReportService repoSvc;
        IPullBackService pullBackSvc;
        public PullBackRequestController(IHomeService homeSvc, IReportService repoSvc, IPullBackService pullBackSvc)
        {
            this.homeSvc = homeSvc;
            this.repoSvc = repoSvc;
            this.pullBackSvc = pullBackSvc;
        }
        public ActionResult PullBack(string loanNumber)
        {
            ViewBag.DocType = new SelectList(repoSvc.GetDocTypeList(User.Identity.GetClientId().Value), "Id", "DocTypeCode");
            ViewBag.loanNumber = loanNumber;
            return View();
        }
        [HttpPost]
        public ActionResult _GetMailPieceList(string loanNumber, string docType, DateTime startDate, DateTime endDate)
        {
            ViewBag.Date = string.Format("{0} To {1}", startDate.ToString("MM/dd/yyyy"), endDate.ToString("MM/dd/yyyy"));

            var model = pullBackSvc.GetPullBackRequestList(loanNumber, docType, startDate, endDate, User.Identity.GetClientId().Value);
            return PartialView(model);
        }
    }
}