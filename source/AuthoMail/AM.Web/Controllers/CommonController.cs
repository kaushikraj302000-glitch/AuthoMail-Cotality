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
using System.Net.Http;
using System.IO;
using AM.Lib;
using AM.Services.Services;

namespace AM.Web.Controllers
{
    [Authorize(Roles = AMRoles.Admin + "," + AMRoles.BA + "," + AMRoles.Client + "," + AMRoles.ClientUserManager + "," + AMRoles.CoreLogicUserManager + "," + AMRoles.PnSOperator + "," + AMRoles.Delfus + "," + AMRoles.BAOffShore)]
    public class CommonController : BaseController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        IReportService repoSvc;
        IHomeService homeSvc;
        ISupportService supSvc;
        IPnSAPIService pnSAPISvc;
        IDocTypeService docTypeSvc;
        IMailPieceService mailpieceSvc;
        private AMSignInManager _signInManager;
        private AMUserManager _userManager;
        //private readonly IClientPermissionService clientPermissionSvc;
        public CommonController(IReportService repoSvc, IHomeService homeSvc, ISupportService supSvc, IPnSAPIService pnsapiSvc, IDocTypeService docTypeSvc, IMailPieceService mailpieceSvc, AMSignInManager signInManager, AMUserManager userManager)
        {
            this.repoSvc = repoSvc;
            this.homeSvc = homeSvc;
            this.supSvc = supSvc;
            this.pnSAPISvc = pnsapiSvc;
            this.docTypeSvc = docTypeSvc;
            this.mailpieceSvc = mailpieceSvc;
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

        [Authorize(Roles = AMRoles.Admin + "," + AMRoles.BA + "," + AMRoles.Client + "," + AMRoles.BAOffShore)]
        [HttpGet]
        public async Task<ActionResult> GetClientMenu()
        {
            if (User.IsInRole(AMRoles.Client))
            {
                var result = await homeSvc.GetAllActiveAppCodeForClientRole(User.Identity.GetUserId());
                return Json(JsonResponseFactory.SuccessResponse(result.ToList()), JsonRequestBehavior.AllowGet);
            }
            var model = User.IsInRole(AMRoles.BA) || User.IsInRole(AMRoles.BAOffShore) ? homeSvc.GetAllActiveClient().Where(u => u.Id > 0).ToList() : homeSvc.GetAllActiveClient();

            return Json(JsonResponseFactory.SuccessResponse(model), JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = AMRoles.Admin + "," + AMRoles.BA + "," + AMRoles.Client + "," + AMRoles.BAOffShore)]
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

        [HttpGet]
        public async Task<ActionResult> DownloadPdf(string fileName, string appCode, int id)
        {
            try
            {
                var filePath = Path.Combine(Config.PnS_PCIRootFolder, HttpUtility.UrlEncode(appCode), id.ToString(), "PCIRequest", fileName);
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Id", "5b189031-eecc-42d7-8b6d-b531caf1036e");
                HttpResponseMessage response = new HttpResponseMessage();
                response = await client.GetAsync($@"{Config.Web_Pdf_API_Url}\DownloadPdf?fileName={filePath}");
                if (response.IsSuccessStatusCode)
                {
                    var stream = await response.Content.ReadAsByteArrayAsync();
                    //return Json(JsonResponseFactory.SuccessResponse(
                    //new
                    //{
                    //    fileName = Path.GetFileNameWithoutExtension(fileName),
                    //    stream = Convert.ToBase64String(stream)
                    //}
                    //));
                    var pdfFileName = Path.GetFileName(fileName);
                    return File(stream, "application/pdf", pdfFileName);
                }
                else
                {
                    return Json(JsonResponseFactory.ErrorResponse("File not found."));
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                return Json(JsonResponseFactory.ErrorResponse(ex.Message));
            }
        }

        [HttpGet]
        public ActionResult _PrintVendorList()
        {
            ViewBag.PrintVendors = new SelectList(repoSvc.GetActivePrintVendorsWithLocation(), "PrintVendorCode", "PrintVendorNameCode");
            return PartialView();
        }

        #region SingleLoanEntry
        [HttpGet]
        public ActionResult SingleLoanEntry(string msg)
        {
            ViewBag.msg = msg;
            ViewBag.BusinessId = homeSvc.GetBusinessFriendlyIdNameByclientId(User.Identity.GetClientId().Value);
            ViewBag.docTypes = new SelectList(homeSvc.GetDocTypeList(User.Identity.GetClientId().Value), "DocTypeName", "DocTypeName");
            return View();
        }


        [HttpPost]
        public async Task<ActionResult> SingleLoanEntry(MailPiece mailPiece, List<HttpPostedFileBase> pdffile, DateTime? ExpiryDate)
        {

            if (mailPiece.eDelivery != null)
            {
                //Email Validation
                if (mailPiece.eDelivery.PrintAndShipOnExpiry && ExpiryDate == null)
                {
                    ModelState.AddModelError("", "Expiry Date must be future date.");
                    goto Error;
                }
                if (mailPiece.eDelivery.PrintAndShipOnExpiry)
                {
                    mailPiece.eDelivery.ExpiryDate = ExpiryDate.Value;
                }
                else
                {
                    mailPiece.eDelivery = null;
                }
            }
            if (ModelState.IsValid)
            {
                try
                {
                    if (pdffile[0] == null)
                    {
                        ModelState.AddModelError("", "Please upload PDF file.");
                        goto Error;
                    }

                    // store files in temp folder
                    string root = Config.MailRequestAPI_FilesDownloadPath;
                    string downloadPath = System.IO.Path.Combine(root, "TempWebFiles");
                    List<string> downloadedFiles = new List<string>();
                    if (!Directory.Exists(downloadPath))
                        Directory.CreateDirectory(downloadPath);

                    // sequence validation
                    if (pdffile.Count > 1)
                    {
                        foreach (var file in pdffile)
                        {
                            int value;
                            if (!int.TryParse(file.FileName.Split('_')[0], out value))
                            {
                                ModelState.AddModelError("", $"The pdf {file.FileName} does not contain the order sequence number.");
                                goto Error;
                            }
                        }
                        var orderedPdfsList = pdffile.Select(x => new { FileObject = x, FileOrder = int.Parse(x.FileName.Split('_')[0]) }).OrderBy(x => x.FileOrder);
                        foreach (var file in orderedPdfsList)
                        {
                            string downloadfilePath = System.IO.Path.Combine(downloadPath, file.FileObject.FileName);
                            file.FileObject.SaveAs(downloadfilePath);
                            downloadedFiles.Add(downloadfilePath);
                        }
                    }
                    else
                    {
                        string downloadfilePath = System.IO.Path.Combine(downloadPath, pdffile[0].FileName);
                        pdffile[0].SaveAs(downloadfilePath);
                        downloadedFiles.Add(downloadfilePath);
                    }
                   
                    mailPiece.ClientCode = supSvc.FindByClientId(User.Identity.GetClientId().Value).AppCode;

                    var mailRequestController = new MailRequestController(pnSAPISvc, docTypeSvc, mailpieceSvc);
                    var result = await mailRequestController.CreateAPIMailRequestAsync(mailPiece, downloadedFiles);

                    var response = result.GetValueOfPropertyFromPropertyName("Value");

                    var mailAPIResponse = (MailAPIResponse)response;

                    if (mailAPIResponse.MailingStatus == "Error")
                    {
                        ModelState.AddModelError("", mailAPIResponse.ErrorCode);
                        ModelState.AddModelError("", mailAPIResponse.ErrorDesc);
                        goto Error;
                    }
                    return RedirectToAction("SingleLoanEntry", new { msg = mailAPIResponse.Message });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
        Error:
            ViewBag.BusinessId = homeSvc.GetBusinessFriendlyIdNameByclientId(User.Identity.GetClientId().Value);
            ViewBag.docTypes = new SelectList(homeSvc.GetDocTypeList(User.Identity.GetClientId().Value), "DocTypeName", "DocTypeName");
            return View(mailPiece);
        }

        #endregion SingleLoanEntry
    }
}