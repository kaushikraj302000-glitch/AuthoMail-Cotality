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
        public ActionResult RegisterScan(string barcode, int scanCount, string clientCode)
        {
            string machineName = GetIPAddress();
            ScanDetails scanDetails = DBHelper.RegisterScan(barcode, scanCount, clientCode, User.Identity.Name, machineName);

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
                        string getClientCode = Request.Cookies["scannedDocument"].Value.Split('$')[0].ToString().Trim();
                        if (getClientCode == clientCode)
                        {
                            scanDocument.Value = string.Format("{0}{1},{2},{3}|", Request.Cookies["scannedDocument"].Value, scanDetails.TrackingNo, code, scanDetails.BatchName);

                        }
                        else
                        {
                            scanDocument.Value = string.Format("{3}${0},{1},{2}|", scanDetails.TrackingNo, code, scanDetails.BatchName, clientCode);
                        }
                    }
                    else
                    {
                        scanDocument.Value = string.Format("{3}${0},{1},{2}|", scanDetails.TrackingNo, code, scanDetails.BatchName, clientCode);
                    }
                    Response.Cookies.Add(scanDocument);
                }
            }
            return Json(new { model = scanDetails }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetScanDocumentFromCookies(string clientCode)
        {
            if (Request.Cookies["scannedDocument"] != null && !string.IsNullOrEmpty(Request.Cookies["scannedDocument"].Value))
            {
                string getClientCode = Request.Cookies["scannedDocument"].Value.Split('$')[0].ToString().Trim();
                if (getClientCode == clientCode)
                {
                    return Json(new { model = Request.Cookies["scannedDocument"].Value }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("ScanDocumentEmpty", JsonRequestBehavior.AllowGet);
                }
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