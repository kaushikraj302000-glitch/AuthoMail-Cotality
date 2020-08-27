using AM.Services;
using AM.Web.USPSIMService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using AM.Entities;
using System.Security.Claims;
using Microsoft.Owin.Security;
using System.Threading.Tasks;

namespace AM.Web.Controllers
{
    [Authorize(Roles = AMRoles.Admin + "," + AMRoles.BA + "," + AMRoles.Client + "," + AMRoles.ClientUserManager + "," + AMRoles.CoreLogicUserManager + "," + AMRoles.PnSOperator + "," + AMRoles.Delfus)]
    public class CommonController : BaseController
    {
        IReportService repoSvc;
        IHomeService homeSvc;
        private AMSignInManager _signInManager;
        private AMUserManager _userManager;
        //private readonly IClientPermissionService clientPermissionSvc;
        public CommonController(IReportService repoSvc, IHomeService homeSvc, AMSignInManager signInManager, AMUserManager userManager)
        {
            this.repoSvc = repoSvc;
            this.homeSvc = homeSvc;
            UserManager = userManager;
            SignInManager = signInManager;
            //this.clientPermissionSvc = clientPermissionSvc;
        }
        public AMUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<AMUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        public AMSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<AMSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
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
            var model = homeSvc.GetClientNameById(User.Identity.GetClientId().Value);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetHeadingForBusinessId()
        {
            var model = homeSvc.GetBusinessFriendlyIdNameByclientId(User.Identity.GetClientId().Value);
            return PartialView("_GetHeadingForBusinessId", model);
        }

        [Authorize(Roles = AMRoles.Admin + "," + AMRoles.BA + "," + AMRoles.Client)]
        [HttpGet]
        public async Task<ActionResult> GetClientMenu()
        {
            if (User.IsInRole(AMRoles.Client))
            {
                var result = await homeSvc.GetAllActiveAppCodeForClientRole(User.Identity.GetUserId());
                return Json(JsonResponseFactory.SuccessResponse(result.ToList()), JsonRequestBehavior.AllowGet);
            }
            var model = User.IsInRole(AMRoles.BA) ? homeSvc.GetAllActiveClient().Where(u => u.Id > 0).ToList() : homeSvc.GetAllActiveClient();

            return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = AMRoles.Admin + "," + AMRoles.BA + "," + AMRoles.Client)]
        [HttpPost]
        public ActionResult RefreshDataByClientId(int clientId)
        {
            int result = homeSvc.SetClientIdToUser(User.Identity.GetUserId(), clientId);

            if (result > 0)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                SignInManager.SignInAsync(user, false, false);
                return Json(JsonResponseFactory.SuccessResponse(result), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(JsonResponseFactory.ErrorResponse("You don't have access for this client."), JsonRequestBehavior.AllowGet);
            }

        }
    }
}