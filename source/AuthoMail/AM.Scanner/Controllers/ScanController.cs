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
        HttpCookie scanDocument = new HttpCookie("scannedDocument");
        public ActionResult Index(string clientCode)
        {
            return View();
        }
        public string GetIPAddress()
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
        
        [HttpPost]
        public ActionResult RegisterScan(string barcode, int totalScanned, string clientCode)
        {
            string machineName = GetIPAddress();
            ScanDetails scanDetails = DBHelper.RegisterScan(barcode, totalScanned, clientCode, User.Identity.Name, machineName);
            if (scanDetails != null)
            {
                if (scanDetails.Code == "Valid" || scanDetails.Code == "QC" || scanDetails.Code == "Pulledback")
                {
                    string code = string.Empty;
                    if (scanDetails.Code == "Valid")
                    {
                        code = "0";
                    }
                    if (scanDetails.Code == "QC")
                    {
                        code = "1";
                    }
                    if (scanDetails.Code == "Pulledback")
                    {
                        code = "2";
                    }
                    if (Request.Cookies["scannedDocument"] != null && !string.IsNullOrEmpty(Request.Cookies["scannedDocument"].Value))
                    {

                        scanDocument.Value += JsonConvert.SerializeObject(scanDetails);
                        // Request.Form[0].ToString() + "|";
                    }
                    else
                    {
                        scanDocument.Value = JsonConvert.SerializeObject(scanDetails);
                        // Request.Form[0].ToString() + "|";
                    }
                    Response.Cookies.Add(scanDocument);
                }
            }
            return Json(scanDetails, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetScanDocumentFromCookies()
        {
            if (Request.Cookies["scannedDocument"] != null && !string.IsNullOrEmpty(Request.Cookies["scannedDocument"].Value))
            {
                return Json(Request.Cookies["scannedDocument"].Value, JsonRequestBehavior.AllowGet);
            }
            return Json("ScanDocumentEmpty", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult NextBatch()
        {
            Response.Cookies.Remove("scannedDocument");
            Response.Cookies.Remove("scannedDocument");
            scanDocument = new HttpCookie("scannedDocument");
            scanDocument.Expires = DateTime.Now.AddDays(-10);
            scanDocument.Value = null;
            Response.SetCookie(scanDocument);
            return Json("ReloadPage", JsonRequestBehavior.AllowGet);
        }

    }
}