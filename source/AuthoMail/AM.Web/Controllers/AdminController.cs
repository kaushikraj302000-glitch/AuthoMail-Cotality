using AM.Entities;
using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Text;
using System.Threading.Tasks;
namespace AM.Web.Controllers
{
    [Authorize(Roles = AMRoles.Admin + "," + AMRoles.ClientUserManager + "," + AMRoles.CoreLogicUserManager)]
    public class AdminController : Controller
    {
        private AMUserManager _userManager;
        ISupportService supportSvc;
        IHomeService homeSvc;
        IConfigService confSvc;
        public AdminController(AMUserManager userManager, ISupportService supportSvc, IHomeService homeSvc, IConfigService confSvc)
        {
            UserManager = userManager;
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
        public ActionResult GetSingUpRequestedList()
        {
            string userId = User.IsInRole(AMRoles.Admin) ? null : User.Identity.GetUserId();
            var model = supportSvc.GetUsersRequestSignUp(userId);
            return Json(JsonResponseFactory.SuccessResponse(new { model = model }), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public async Task<ActionResult> UpdateUsersRequestSignUp(string userId, string status)
        {
            try
            {
                var model = supportSvc.UpdateUsersRequestSignUp(userId, status, User.Identity.GetUserId());
                var user = UserManager.FindById(userId);

                string template = status.Equals(AMUserStatus.Approved) ? "ApproveRegistration.html" : "RejectRegistration.html";
                StringBuilder sb = new StringBuilder(System.IO.File.ReadAllText(Server.MapPath(@"~\Template\" + template)));
                sb.Replace("{{name}}", user.FirstName);
                sb.Replace("{{email}}", user.Email);

                string AuthoMailURL = "";
                if (Config.Common_Environment == Env.Prod)
                {
                    AuthoMailURL = "https://authomail.corelogic.com";
                }
                else if (Config.Common_Environment == Env.Staging)
                {
                    AuthoMailURL = "https://authomailuat.corelogic.com";
                }
                else
                {
                    AuthoMailURL = Request.Url.ToString().Replace(Request.RawUrl.ToString(), Url.Action("Login", "Account"));
                }
                sb.Replace("{{AuthoMailURL}}", AuthoMailURL);
                string emails = supportSvc.GetEmailsFromClientDomain(user.Email);
                await InternalEmail.SendMail(user.Email, null, emails, status.Equals(AMUserStatus.Approved) ? "AuthoMail registration request approved." : "AuthoMail registration request rejected.", sb.ToString());

                return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Manage Domain
        [Authorize(Roles = AMRoles.Admin), HttpGet]
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
        [Authorize(Roles = AMRoles.Admin), HttpGet]
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
        [Authorize(Roles = AMRoles.Admin)]
        public ActionResult ManageDomain()
        {
            var model = homeSvc.GetAllActiveClient();

            return View();
        }

        [Authorize(Roles = AMRoles.Admin), HttpPost]
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
        [Authorize(Roles = AMRoles.Admin), HttpGet]
        public ActionResult SystemSettings()
        {
            return View();
        }
        [Authorize(Roles = AMRoles.Admin)]
        public ActionResult GetAllSystemSettings()
        {
            var model = confSvc.GetSystemSettings();
            return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = AMRoles.Admin)]
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
        [Authorize(Roles = AMRoles.Admin), HttpPost]
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
        [Authorize(Roles = AMRoles.Admin)]
        public ActionResult ManageBusinessUnits()
        {
            return View();
        }

        [Authorize(Roles = AMRoles.Admin), HttpGet]
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

        [Authorize(Roles = AMRoles.Admin), HttpPost]
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

        [Authorize(Roles = AMRoles.Admin), HttpGet]
        public ActionResult GetBusinessUnitById(int id)
        {
            return id == 0 ? PartialView("_AddOrEditBusinessUnit", new BusinessUnitModel()) : PartialView("_AddOrEditBusinessUnit", supportSvc.FindByIdBusinessUnit(id));
        }

        [Authorize(Roles = AMRoles.Admin), HttpPost]
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
        [Authorize(Roles = AMRoles.Admin)]
        public ActionResult ManageClients()
        {
            return View();
        }

        [Authorize(Roles = AMRoles.Admin), HttpGet]
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
        [Authorize(Roles = AMRoles.Admin), HttpPost]
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

        [Authorize(Roles = AMRoles.Admin), HttpPost]
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

        [Authorize(Roles = AMRoles.Admin), HttpGet]
        public ActionResult GetClientById(int id)
        {

            if (id != 0)
            {
                Clients client = supportSvc.FindByClientId(id);
                ViewBag.BusinessUnitList = new SelectList(supportSvc.GetBusinessUnitActive(), "Id", "BusinessUnit", client.BusinessUnit);
                ViewBag.TimezoneList = new SelectList(TimeZoneInfo.GetSystemTimeZones().OrderBy(s => s.StandardName).ToList(), "StandardName", "StandardName", client.TimeZone);
                return PartialView("_AddOrEditClient", client);
            }
            else
            {
                ViewBag.BusinessUnitList = new SelectList(supportSvc.GetBusinessUnitActive(), "Id", "BusinessUnit");
                ViewBag.TimezoneList = new SelectList(TimeZoneInfo.GetSystemTimeZones().OrderBy(s => s.StandardName).ToList(), "StandardName", "StandardName");
                return PartialView("_AddOrEditClient", new Clients());
            }
        }

        #endregion

        #region Manage SFTChannel
        [Authorize(Roles = AMRoles.Admin)]
        public ActionResult ManageSFTChannels()
        {
            return View();
        }

        [Authorize(Roles = AMRoles.Admin), HttpGet]
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

        [Authorize(Roles = AMRoles.Admin), HttpPost]
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

        [Authorize(Roles = AMRoles.Admin), HttpGet]
        public ActionResult GetSFTChannelById(int id)
        {
            var ProtocolList = new string[] { "ftp", "sftp", "unc" };
            var PostDownloadActionList = new string[] { "Archive", "Delete" };

            if (id != 0)
            {
                SFTChannels model = supportSvc.FindByIdSFTChannels(id);
                ViewBag.ProtocoldropDown = ProtocolList.Select(s => new SelectListItem() { Text = s, Value = s, Selected = model.Protocol == s }).ToList();
                ViewBag.PostDownloadActiondropDown = PostDownloadActionList.Select(s => new SelectListItem() { Text = s, Value = s, Selected = model.PostDownloadAction == s }).ToList();
                return PartialView("_AddOrEditSFTChannel", model);
            }
            else
            {
                ViewBag.ProtocoldropDown = ProtocolList.Select(s => new SelectListItem() { Text = s, Value = s }).ToList();
                ViewBag.PostDownloadActiondropDown = PostDownloadActionList.Select(s => new SelectListItem() { Text = s, Value = s }).ToList();
                return PartialView("_AddOrEditSFTChannel", new SFTChannels());
            }
        }
        [Authorize(Roles = AMRoles.Admin), HttpPost]
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
        [Authorize(Roles = AMRoles.Admin)]
        public ActionResult ManageDocumentTypes()
        {
            return View();
        }

        [Authorize(Roles = AMRoles.Admin), HttpGet]
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
        [Authorize(Roles = AMRoles.Admin), HttpGet]
        public ActionResult GetDocTypeById(int id)
        {
            var PrintSizeList = new string[] { "MixMedia", "LETTER-IP", "GLT-20-DOP-17.5", "LEGAL-IP", "WC24#LTR-PERF", "LEGAL-PERF-IP" };
            var PrintFormatList = new string[] { "Simplex", "Duplex" };
            var ShippingCarrierList = new string[] { "USPS", "FedEx", "UPS" };
            var ShippingServiceTypeList = new string[] { "USPS", "FEDEX_2_DAY", "FEDEX_EXPRESS_SAVER", "FEDEX_GROUND", "FIRST_OVERNIGHT", "GROUND_HOME_DELIVERY", "PRIORITY_OVERNIGHT", "STANDARD_OVERNIGHT" };
            if (id != 0)
            {
                DocTypes model = supportSvc.FindByIdDocType(id);
                ViewBag.Clients = new SelectList(homeSvc.GetAllActiveClient(), "Id", "Name", model.ClientId);
                ViewBag.PrintSizedropDown = PrintSizeList.Select(s => new SelectListItem() { Text = s, Value = s, Selected = model.PrintSize == s }).ToList();

                ViewBag.PrintFormatdropDown = PrintFormatList.Select(s => new SelectListItem() { Text = s, Value = s, Selected = model.PrintFormat == s }).ToList();

                ViewBag.ShippingCarrierdropDown = ShippingCarrierList.Select(s => new SelectListItem() { Text = s, Value = s, Selected = model.ShippingCarrier == s }).ToList();
                ViewBag.ShippingServiceTypedropDown = ShippingServiceTypeList.Select(s => new SelectListItem() { Text = s, Value = s, Selected = model.ShippingServiceType == s }).ToList();
                return PartialView("_AddOrEditDocType", model);
            }
            else
            {
                ViewBag.PrintFormatdropDown = PrintFormatList.Select(s => new SelectListItem() { Text = s, Value = s }).ToList();
                ViewBag.PrintSizedropDown = PrintSizeList.Select(s => new SelectListItem() { Text = s, Value = s }).ToList();
                ViewBag.ShippingCarrierdropDown = ShippingCarrierList.Select(s => new SelectListItem() { Text = s, Value = s }).ToList();
                ViewBag.ShippingServiceTypedropDown = ShippingServiceTypeList.Select(s => new SelectListItem() { Text = s, Value = s }).ToList();
                ViewBag.Clients = new SelectList(homeSvc.GetAllActiveClient(), "Id", "Name");
                return PartialView("_AddOrEditDocType", new DocTypes());
            }
        }

        [Authorize(Roles = AMRoles.Admin), HttpPost]
        public ActionResult MakeActiveOrDeactiveDocType(bool isActive, int Id)
        {
            try
            {
                var model = supportSvc.MakeActivateOrDeactivateDocType(Id, isActive, User.Identity.GetUserId());
                return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize(Roles = AMRoles.Admin), HttpPost]
        public ActionResult CreateOrEditDocType(DocTypes docType)
        {
            try
            {
                var model = supportSvc.CreateOrEditDocType(docType, User.Identity.GetUserId());
                return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.Message.ToString()), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Assige Role 
        [HttpGet]
        public ActionResult GetUserManager()
        {
            return View();
        }
        [HttpGet]
        public JsonResult GetUserManagerList()
        {
            try
            {
                var users = supportSvc.GetUserManagerList();
                return Json(JsonResponseFactory.SuccessResponse(users), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(JsonResponseFactory.ErrorResponse(ex.Message), JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]
        public JsonResult AssignUserManagerRole(string email)
        {
            try
            {
                var result = supportSvc.AssignUserManagerRole(email);
                if (result.Key)
                {
                    return Json(JsonResponseFactory.SuccessResponse(result.Value), JsonRequestBehavior.AllowGet);
                }
                return Json(JsonResponseFactory.ErrorResponse(result.Value), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(JsonResponseFactory.ErrorResponse(ex.Message), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult RemoveUserManagerRole(string userId, string roleId)
        {
            try
            {
                var result = supportSvc.RemoveUserManagerRole(userId, roleId);
                if (result.Key)
                {
                    return Json(JsonResponseFactory.SuccessResponse(result.Value), JsonRequestBehavior.AllowGet);
                }
                return Json(JsonResponseFactory.ErrorResponse(result.Value), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(JsonResponseFactory.ErrorResponse(ex.Message), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion 
    }
}