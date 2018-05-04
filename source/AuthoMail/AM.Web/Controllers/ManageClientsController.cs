using AM.Entities;
using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AM.Web.Controllers
{
    [Authorize(Roles=AMRoles.Admin)]
    public class ManageClientsController : Controller
    {
        ISupportService supportSvc;

        public ManageClientsController(ISupportService supportSvc)
        {
            this.supportSvc = supportSvc;
        }
        // GET: ManageClients
        public ActionResult SingUpRequest()
        {

            return View();
        }
    }
}