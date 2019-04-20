using AM.Scanner.Common;
using AM.Scanner.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace AM.Scanner.Controllers
{
    [Authorize]
    public class ScanController : Controller
    {
        // GET: Scan
        //HttpCookie scanDocument = new HttpCookie("scannedDocument");
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        //private string machineName = string.Empty;
        public ScanController()
        {
            
        }
        public ActionResult Index(string clientCode, string batchTime)
        {
            return View();
        }

        public string GetIPAddress()
        {
            using (new PerfTimerLogger("GetIPAddress[ScanController]: "))
            {
                try
                {
                    IPHostEntry Host = default(IPHostEntry);
                    string Hostname = null;
                    Hostname = System.Environment.MachineName;
                    Host = Dns.GetHostEntry(Hostname);
                    string ip = string.Empty;
                    foreach (IPAddress IP in Host.AddressList)
                    {
                        if (IP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            ip = Convert.ToString(IP);
                        }
                    }
                    return ip;
                }
                catch (Exception e)
                {
                    logger.Error(e.Message);
                    return string.Empty;
                }
            }
        }

        [HttpPost]
        public ActionResult RegisterScan(string barcode, string clientCode, string batchName)
        {
            using (new PerfTimerLogger("RegisterScan[ScanController]: "))
            {
                string machineName = GetIPAddress();
                ScanDetails scanDetails = DBHelper.RegisterScan(barcode, clientCode, User.Identity.Name, machineName, batchName);
                return Json(new { model = scanDetails }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetScannedDocuments(string clientCode, string batchName)
        {
            using (new PerfTimerLogger("GetScannedDocuments[ScanController]: "))
            {
                var getScannedList = DBHelper.GetScannedMailPieces(clientCode, batchName);
                return Json(new { model = getScannedList }, JsonRequestBehavior.AllowGet);
            }
            //if (Request.Cookies["scannedDocument"] != null && !string.IsNullOrEmpty(Request.Cookies["scannedDocument"].Value))
            //{
            //    string getClientCode = Request.Cookies["scannedDocument"].Value.Split('$')[0].ToString().Trim();
            //    string getBatchTime = Request.Cookies["scannedDocument"].Value.Split('$')[1].ToString().Trim();
            //    if (getClientCode == clientCode && getBatchTime == batchTime)
            //    {
            //        return Json(new { model = Request.Cookies["scannedDocument"].Value }, JsonRequestBehavior.AllowGet);
            //    }
            //    else
            //    {
            //        return Json("ScanDocumentEmpty", JsonRequestBehavior.AllowGet);
            //    }
            //}
            //return Json("ScanDocumentEmpty", JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetScanCountByBatchName(string clientCode, string batchName)
        {
            using (new PerfTimerLogger("GetScanCountByBatchName[ScanController]: "))
            {
                var model = DBHelper.GetRecentBatchesStats(clientCode.Trim());
                ScanDetails scanDetails = new ScanDetails();
                scanDetails.AppCode = clientCode;
                if (model != null)
                {
                    var batchWiseCount = model.Where(s => s.BatchName == batchName).FirstOrDefault();
                    if (batchWiseCount != null)
                    {
                        scanDetails.ScanCount = Convert.ToInt32(batchWiseCount.ScannedLoans);
                        scanDetails.TotalCount = Convert.ToInt32(batchWiseCount.TotalLoans);
                    }
                    else
                    {
                        scanDetails.ScanCount = 0;
                        scanDetails.TotalCount = 0;
                    }

                }
                else
                {
                    scanDetails.ScanCount = 0;
                    scanDetails.TotalCount = 0;
                }
                return Json(scanDetails, JsonRequestBehavior.AllowGet);
            }
        }
        //[HttpGet]
        //public ActionResult NextBatch()
        //{
        //    Response.Cookies.Remove("scannedDocument");
        //    Response.Cookies.Remove("scannedDocument");
        //    scanDocument = new HttpCookie("scannedDocument");
        //    scanDocument.Expires = DateTime.Now.AddDays(-10);
        //    scanDocument.Value = null;
        //    Response.SetCookie(scanDocument);
        //    return Json("ReloadPage", JsonRequestBehavior.AllowGet);
        //}

        [HttpGet]
        public ActionResult USPSScan(string clientCode)
        {
            return View();
        }

        [HttpPost]
        public ActionResult RegisterUSPSScan(int mailPieceId, string trackingNo, string clientCode)
        {
            USPSScanDetails scanDetails = DBHelper.RegisterUSPSScan(mailPieceId, trackingNo, clientCode);
            return Json(new { model = scanDetails }, JsonRequestBehavior.AllowGet);
        }

        #region SingleScan
        public ActionResult SingleScan(string clientCode, string batchTime)
        {
            logger.Info("RegisterScan SingleScan");
            return View();
        }
        #endregion SingleScan
    }
}