using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AM.Scanner.Controllers
{
    public class ScanController : Controller
    {
        // GET: Scan
        public ActionResult Index(string clientCode)
        {
            return View();
        }
    }
}