using AM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using AM.Services;
using AM.Lib;

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
        public MonitorReport GetPendingBatches([FromBody]string cutOff)
        {
            int calculatedCutOff = 30;
            calculatedCutOff = int.TryParse(cutOff, out calculatedCutOff) ? Convert.ToInt32(cutOff) : calculatedCutOff;
            return alertSvc.GetPendingBatchesToSubmit(calculatedCutOff);
        }
    }
}
