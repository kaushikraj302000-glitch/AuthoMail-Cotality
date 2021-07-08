using AM.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AM.Web.Models;
using AM.Entities;
using AutoMapper;
using System.Threading.Tasks;
using System.Net.Http;
using DocumentFormat.OpenXml.Drawing;
using System.Text;
using System.IO;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Net;
using Newtonsoft.Json.Linq;
using AM.Lib;
using AM.Services.Services;

namespace AM.Web.Controllers
{
    [Authorize(Roles = AMRoles.Admin + "," + AMRoles.BA)]
    public class SupportController : BaseController
    {
        ISupportReportService supportSvc;
        IChartDataService chartDataSvc;
        IReportService reportSvc;
        IFedExService fedexSvc;
        IHomeService homeSvc;
        ISupportService supSvc;
        IPnSAPIService pnSAPISvc;
        IDocTypeService docTypeSvc;
        IMailPieceService mailpieceSvc;
        public SupportController(ISupportReportService supportSvc, IChartDataService chartDataSvc, IReportService reportSvc, IFedExService fedexSvc, IHomeService homeSvc, ISupportService supSvc, IPnSAPIService pnsapiSvc, IDocTypeService docTypeSvc, IMailPieceService mailpieceSvc)
        {
            this.supportSvc = supportSvc;
            this.chartDataSvc = chartDataSvc;
            this.reportSvc = reportSvc;
            this.fedexSvc = fedexSvc;
            this.homeSvc = homeSvc;
            this.supSvc = supSvc;
            this.pnSAPISvc = pnsapiSvc;
            this.docTypeSvc = docTypeSvc;
            this.mailpieceSvc = mailpieceSvc;
        }
        // GET: Support
        public ActionResult NotReportedBackOfBatchSummary()
        {
            return View();
        }
        [HttpPost]
        public ActionResult _GetNotReportedBackOfBatchSummary(string date, string printLocation)
        {
            try
            {

                var exactDate = DateTime.ParseExact(date, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
                var data = supportSvc.GetNotReportedBackMailPieces(printLocation, exactDate);
                return Json(JsonResponseFactory.SuccessResponse(new { model = data.OrderByDescending(s => s.SubmittedOn) }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.ToString()), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetDataByPrintBatchId(int printBatchId)
        {
            try
            {
                var data = supportSvc.GetMailPiecebyPrintBatch(printBatchId, User.Identity.GetClientId().Value);
                return Json(JsonResponseFactory.SuccessResponse(new { model = data }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.ToString()), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult LoanDetailsBySubmittedOn()
        {
            return View();
        }

        public ActionResult _GetLoanDetailsBySubmittedOn(string date)
        {
            try
            {
                var exactDate = DateTime.ParseExact(date, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
                var data = supportSvc.GetBatchSummarybyDate(exactDate, User.Identity.GetClientId().Value);
                return Json(JsonResponseFactory.SuccessResponse(new { model = data.OrderByDescending(s => s.SubmittedOn) }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.ToString()), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetMailPieceWithBatchSummaryByPrintBatch(int printBatchId)
        {

            try
            {
                var data = supportSvc.GetMailPieceWithBatchSummaryByPrintBatch(printBatchId, User.Identity.GetClientId().Value);
                return Json(JsonResponseFactory.SuccessResponse(new { model = data }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.ToString()), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult NotReportedBackOfMailBatches()
        {
            return View();
        }

        public ActionResult _GetNotReportedBackOfMailBatches(string date)
        {
            try
            {
                var exactDate = DateTime.ParseExact(date, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
                var data = supportSvc.GetMailBatchesbyDate(exactDate, User.Identity.GetClientId().Value);
                return Json(JsonResponseFactory.SuccessResponse(new { model = data }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.ToString()), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetMailPieceByBatchId(int batchId)
        {
            try
            {
                var data = supportSvc.GetMailPieceByBatchId(batchId, User.Identity.GetClientId().Value);
                return Json(JsonResponseFactory.SuccessResponse(new { model = data }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.ToString()), JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize(Roles = AMRoles.Admin + "," + AMRoles.BA)]
        [HttpGet]
        public ActionResult DelayedBatches()
        {
            return View();
        }
        [HttpGet]
        public ActionResult AllClientsPackages()
        {
            return View();
        }
        [HttpPost]
        public ActionResult TodayChartDetails(string status, int batchId)
        {
            var data = chartDataSvc.GetMailPieceByParam(batchId, status);

            var model = Mapper.Map<List<MailPieceViewModel>>(data);
            return Json(JsonResponseFactory.SuccessResponse(new { model = model.OrderByDescending(s => s.ReceivedOn) }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetMailPieceByBatchIdChart(int batchId)
        {
            var data = chartDataSvc.GetMailPieceByBatchId(batchId);
            var model = Mapper.Map<List<MailPieceViewModel>>(data);
            return Json(JsonResponseFactory.SuccessResponse(new { model = model.OrderByDescending(s => s.ReceivedOn) }), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult WeeklyChartDetailsChart(string status, string date)
        {
            var exactDate = DateTime.ParseExact(date, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            var model = chartDataSvc.GetBatchByParam(exactDate, status);
            return Json(JsonResponseFactory.SuccessResponse(new { model = model.OrderByDescending(s => s.ReceivedOn) }), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetMailPieceDataByStatusAndBatchIdChart(string status, int batchId)
        {
            var model = chartDataSvc.GetMailPieceByParam(batchId, status);
            return Json(model.OrderByDescending(s => s.MailBatches.Id).ToList(), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult GetPickedUpMailPieces()
        {
            var model = reportSvc.GetFedExData();
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult CallFedExService()
        {
            try
            {
                fedexSvc.UpdateFedExTrackingInfo();
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
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
        public async Task<ActionResult> SingleLoanEntry(MailPiece mailPiece, HttpPostedFileBase pdffile, DateTime? ExpiryDate)
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
                    if (pdffile == null)
                    {
                        ModelState.AddModelError("", "Please upload PDF file.");
                        goto Error;
                    }

                    var documentOrPackage = new DocumentOrPackage();
                    documentOrPackage.Name = pdffile.FileName;

                    mailPiece.ClientCode = supSvc.FindByClientId(User.Identity.GetClientId().Value).AppCode;
                    // convert stream to string
                    Byte[] byteStream = null;
                    using (var memoryStream = new MemoryStream())
                    {
                        await pdffile.InputStream.CopyToAsync(memoryStream);
                        byteStream = memoryStream.ToArray();
                    }
                    documentOrPackage.Base64String = Convert.ToBase64String(byteStream);
                    mailPiece.DocumentOrPackage = documentOrPackage;

                    var mailRequestController = new MailRequestController(pnSAPISvc, docTypeSvc,mailpieceSvc);
                    var result = await mailRequestController.CreateAPIMailRequestAsync(mailPiece);

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