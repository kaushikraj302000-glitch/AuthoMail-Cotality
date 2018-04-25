using AM.Services;
using AM.Web.USPSIMService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using AM.Entities;

namespace AM.Web.Controllers
{
    [Authorize]
    public class CommonController : Controller
    {
        IReportService repoSvc;
        IHomeService homeSvc;
        public CommonController(IReportService repoSvc,IHomeService homeSvc)
        {
            this.repoSvc = repoSvc;
            this.homeSvc = homeSvc;
        }
        // GET: Common
        [HttpGet]
        public ActionResult _Channel()
        {
            ViewBag.Channel = new SelectList(repoSvc.GetChannelTitle(User.Identity.GetClientId().Value), "Code", "Title");
            return PartialView();
        }
        [HttpPost]
        public ActionResult USPSTracking(string TrackingNo, string ShippingMethod)
        {
            USPSIMClient uspsClient = new USPSIMClient();
            USPSTrackingInfo USPSInfo = uspsClient.GetShipmentStatus(TrackingNo);
            return PartialView("_TrackingInfo", USPSInfo);

        }
        [HttpGet]
        public ActionResult GetClientNameById()
        {
            var model=homeSvc.GetClientNameById(User.Identity.GetClientId().Value);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetHeadingForBusinessId()
        {
           var model= homeSvc.GetBusinessFriendlyIdNameByclientId(User.Identity.GetClientId().Value);
           return PartialView("_GetHeadingForBusinessId", model);
        }
    }
}