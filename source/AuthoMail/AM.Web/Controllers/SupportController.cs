using AM.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AM.Web.Models;
using AM.Entities;
using AM.Services;
namespace AM.Web.Controllers
{
    [Authorize(Roles = AMRoles.Admin + "," + AMRoles.BA + "," +  AMRoles.PnSOperator)]
    public class SupportController : BaseController
    {
        ISupportReportService supportSvc;
        public SupportController(ISupportReportService supportSvc)
        {
            this.supportSvc = supportSvc;
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
                var data = supportSvc.GetNotReportedBackMailPieces(User.Identity.GetClientId().Value, printLocation, exactDate);
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


                //var data = serSvc.GetMailPieceByParam(User.Identity.GetClientId().Value, BusinessFriendlyIdName, BusinessFriendlyIdValue, exactStartDate, exactEndDate, docType, status, channel, clientBatchId, mailBatchId, trackingNumber);

                //var model = Mapper.Map<List<SearchViewModel>>(data);

                //return Json(JsonResponseFactory.SuccessResponse(new { model = data.OrderByDescending(s => s.ReceivedOn) }), JsonRequestBehavior.AllowGet);
                return null;
            }
            catch (Exception e)
            {
                return Json(JsonResponseFactory.ErrorResponse(e.ToString()), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult LoanDetailsByPrintBatchId()
        {
            return View();
        }

        public ActionResult _GetLoanDetailsByPrintBatchId(int printBatchId)
        {
            return View();
        }

        public ActionResult NotReportedBackOfMailBatches()
        {
            return View();
        }

        public ActionResult _GetNotReportedBackOfMailBatches(string date)
        {
            return View();
        }

    }
}