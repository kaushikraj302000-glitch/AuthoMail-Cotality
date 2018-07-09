using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AM.Web.Controllers
{
    public class TrackController : BaseController
    {
        // GET: Track
        
        public ActionResult Index(string id)
        {
            return View();
        }
    }
}