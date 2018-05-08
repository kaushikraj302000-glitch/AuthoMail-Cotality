using AM.Entities;
using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AM.Web.Controllers
{
    [Authorize(Roles = AMRoles.Admin)]
    public class AdminController : Controller
    {
        ISupportService supportSvc;
        IHomeService homeSvc;
        IConfigService confSvc;
        public AdminController(ISupportService supportSvc, IHomeService homeSvc, IConfigService confSvc)
        {
            this.supportSvc = supportSvc;
            this.homeSvc = homeSvc;
            this.confSvc = confSvc;
        }
        // GET: Admin
        #region SignUp Request
        public ActionResult SignUpRequest()
        {
            return View();
        }

        public ActionResult GetSingUpRequestedList()
        {
            var model = supportSvc.GetUsersRequestSignUp();
            return Json(JsonResponseFactory.SuccessResponse(new { model = model }), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult UpdateUsersRequestSignUp(string userId, string status)
        {
            try
            {
                var model = supportSvc.UpdateUsersRequestSignUp(userId, status);
                return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Manage Domain
        [HttpGet]
        public ActionResult GetDomains()
        {
            try
            {
                var model = supportSvc.GetClientDomains();

                return Json(JsonResponseFactory.SuccessResponse(new { model = model }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult ManageDomain()
        {
            var model = homeSvc.GetAllActiveClient();
            ViewBag.Clients = new SelectList(homeSvc.GetAllActiveClient(), "Id", "Name");
            return View();
        }

        [HttpPost]
        public ActionResult AddDomain(int clientId, string domainName)
        {

            try
            {
                var model = supportSvc.CreateClientDomain(clientId, domainName);
                if (model > 0)
                {

                    return Json(JsonResponseFactory.SuccessResponse(), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(JsonResponseFactory.ErrorResponse("Error while adding domain name."), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region System Settings
        [HttpGet]
        public ActionResult SystemSettings()
        {
            return View();
        }
        public JsonResult GetAllSystemSettings()
        {
            var model = confSvc.GetSystemSettings();
            return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}