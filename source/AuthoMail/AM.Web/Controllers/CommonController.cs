using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AM.Web.Controllers
{
    public class CommonController : Controller
    {
        IReportService repoSvc;
        public CommonController(IReportService repoSvc)
        {
            this.repoSvc = repoSvc;
        }
        // GET: Common
        [HttpGet]
        public ActionResult _Channel()
        {
            ViewBag.Channel = new SelectList(repoSvc.GetChannelTitle(new List<string> { "AMC_Internal", "USAA_CL" }), "Code", "Title");
            return PartialView();
        }
    }
}