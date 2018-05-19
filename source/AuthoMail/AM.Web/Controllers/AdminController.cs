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
        [HttpGet]
        public ActionResult GetDomainByName(string domainName)
        {

            if (!string.IsNullOrWhiteSpace(domainName))
            {
                ClientDomains domain = supportSvc.FingByDomainName(domainName);
                ViewBag.Clients = new SelectList(homeSvc.GetAllActiveClient(), "Id", "Name", domain.ClientId);
                return PartialView("_AddOrEditDomain", domain);
            }
            else
            {
                ViewBag.Clients = new SelectList(homeSvc.GetAllActiveClient(), "Id", "Name");
                return PartialView("_AddOrEditDomain", new ClientDomains());
            }
        }

        public ActionResult ManageDomain()
        {
            var model = homeSvc.GetAllActiveClient();

            return View();
        }

        [HttpPost]
        public ActionResult CreateOrEditDomain(int clientId, string domainName, string oldDomain)
        {

            try
            {
                int model = 0;
                if (string.IsNullOrWhiteSpace(oldDomain))
                {
                    model = supportSvc.CreateClientDomain(clientId, domainName);
                    return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    model = supportSvc.UpdateClientDomain(domainName, oldDomain);
                    return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
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
                var model = supportSvc.GetBusinessUnit().OrderByDescending(s => s.IsActive).ToList();
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
            return id == 0 ? PartialView("_AddOrEditBusinessUnit", new BusinessUnitModel()) : PartialView("_AddOrEditBusinessUnit", supportSvc.FindByIdBusinessUnit(id));
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

        #region Manage Client

        public ActionResult ManageClients()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetClients()
        {
            try
            {
                var model = supportSvc.GetClients().OrderByDescending(s => s.IsAMActive).ThenByDescending(s => s.IsPnSActive).ToList();
                return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult MakeActiveOrDeactiveClient(bool isActive, int Id, bool isAM)
        {
            try
            {
                if (isAM)
                {
                    var model = supportSvc.MakeClientAMActiveOrInActive(Id, isActive, User.Identity.GetUserId());
                    return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var model = supportSvc.MakeClientPnSActiveOrInActive(Id, isActive, User.Identity.GetUserId());
                    return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult CreateOrEditClient(Clients client)
        {
            try
            {
                var model = supportSvc.CreateEditClient(client, User.Identity.GetUserId());
                return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetClientById(int id)
        {

            if (id != 0)
            {
                Clients client = supportSvc.FindByClientId(id);
                ViewBag.BusinessUnitList = new SelectList(supportSvc.GetBusinessUnitActive(), "Id", "BusinessUnit", client.BusinessUnit);
                return PartialView("_AddOrEditClient", client);
            }
            else
            {
                ViewBag.BusinessUnitList = new SelectList(supportSvc.GetBusinessUnitActive(), "Id", "BusinessUnit");
                return PartialView("_AddOrEditClient", new Clients());
            }
        }

        #endregion

        #region Manage SFTChannel

        public ActionResult ManageSFTChannels()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetSFTChannels()
        {
            try
            {
                var model = supportSvc.GetSFTChannels();
                return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult MakeActiveOrDeactiveSFTChannel(bool isActive, int Id)
        {
            try
            {
                var model = supportSvc.MakeActivateOrDeactivateSFTChannel(Id, isActive);
                return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetSFTChannelById(int id)
        {

            if (id != 0)
            {
                SFTChannels model = supportSvc.FindByIdSFTChannels(id);
                return PartialView("_AddOrEditSFTChannel", model);
            }
            else
            {

                return PartialView("_AddOrEditSFTChannel", new SFTChannels());
            }
        }
        [HttpPost]
        public ActionResult CreateOrEditSFTChannel(SFTChannels sftChannel)
        {
            try
            {
                var model = supportSvc.CreateOrEditSFTChannel(sftChannel);
                return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Manage DocType

        public ActionResult ManageDocumentTypes()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetDocTypes()
        {
            try
            {
                var model = supportSvc.GetDocTypes();
                return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetDocTypeById(int id)
        {
            if (id != 0)
            {
                DocTypes model = supportSvc.FindByIdDocType(id);
                ViewBag.Clients = new SelectList(homeSvc.GetAllActiveClient(), "Id", "Name", model.ClientId);
                return PartialView("_AddOrEditDocType", model);
            }
            else
            {
                ViewBag.Clients = new SelectList(homeSvc.GetAllActiveClient(), "Id", "Name");
                return PartialView("_AddOrEditDocType", new DocTypes());
            }
        }

        #endregion
    }
}