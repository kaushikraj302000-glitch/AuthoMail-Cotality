using AM.Services;
using AM.Web.USPSIMService;
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
        [HttpPost]
        public ActionResult USPSTracking(string TrackingNo, string ShippingMethod)
        {
            USPSIMClient uspsClient = new USPSIMClient();
            USPSTrackingInfo USPSInfo = uspsClient.GetShipmentStatus(TrackingNo);
            return PartialView("_TrackingInfo", USPSInfo);

        }
    }
}