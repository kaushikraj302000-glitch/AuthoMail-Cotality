using AM.Services.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AM.Web.Controllers
{
    public class PullBackRequestController : Controller
    {
        // GET: PullBackRequest
        IHomeService homeSvc;
        IReportService repoSvc;
        public PullBackRequestController(IHomeService homeSvc, IReportService repoSvc)
        {
            this.homeSvc = homeSvc;
            this.repoSvc = repoSvc;
        }
        public ActionResult PullBack()
        {
            ViewBag.DocType = new SelectList(repoSvc.GetDocTypeList(), "Id", "DocTypeCode");
            return View();
        }
        public ActionResult _GetMailPieceList(string loanNumber, string docType, DateTime startDate, DateTime endDate)
        {
            ViewBag.Date = string.Format("{0} - {1}", startDate.ToShortDateString(), endDate.ToShortDateString());

            var model = repoSvc.GetMailPieceByParam(loanNumber, docType, startDate, endDate);
            return PartialView(model);
        }
    }
}