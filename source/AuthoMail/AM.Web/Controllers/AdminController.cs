using AM.Entities;
using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
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
        public ActionResult GetAllSystemSettings()
        {
            var model = confSvc.GetSystemSettings();
            return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ResetSetting(string settingId)
        {
            try
            {
                var model = confSvc.ResetValueSystemSetting(User.Identity.GetUserId(), settingId);
                return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult UpdateValueOrDescription(string settingId, string value, string description)
        {
            try
            {
                var model = confSvc.UpdateValueOrDescription(User.Identity.GetUserId(), settingId, value, description);
                return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Manage Business Unit

        public ActionResult ManageBusinessUnits()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetBusinessUnits()
        {
            try
            {
                var model = supportSvc.GetBusinessUnit();
                return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult CreateOrEditBusinessUnit(BusinessUnitModel businessUnit)
        {
            try
            {
                var model = supportSvc.CreateOrEditBusinessUnit(businessUnit);
                return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetBusinessUnitById(int id)
        {
            return id == 0 ? PartialView("_GetBusinessUnit", new BusinessUnitModel()) : PartialView("_GetBusinessUnit", supportSvc.FindByIdBusinessUnit(id));
        }

        [HttpPost]
        public ActionResult MakeActiveOrInActiveBusinessUnit(bool isActive, int Id)
        {
            try
            {
                var model = supportSvc.MakeActiveOrInActiveBusinessUnit(isActive, Id);
                return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}