using AM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using AM.Services;

namespace AM.Web.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    //[Authorize(Roles = AMRoles.API)]
    [RoutePrefix("api/alert")]
    public class AlertApiController : ApiController
    {
        IMonitorBatchesService alertSvc;
        public AlertApiController(IMonitorBatchesService alertSvc)
        {
            this.alertSvc = alertSvc;
        }
        [Route("GetPendingBatches")]
        public PendingBatchesListByPrintLocation GetPendingBatches()
        {
            return alertSvc.GetPendingBatchesToSubmit(Config.Alert_CutOffMinutes);
        }
    }
}
