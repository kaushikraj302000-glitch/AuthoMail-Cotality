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
    [Authorize(Roles = AMRoles.Admin + "," + AMRoles.BA + "," + AMRoles.Client + "," + AMRoles.ClientUserManager + "," + AMRoles.CoreLogicUserManager + "," + AMRoles.PnSOperator + "," + AMRoles.API)]
    [RoutePrefix("api/alert")]
    public class AlertApiController : ApiController
    {
        IMonitorBatchesService alertSvc;
        public AlertApiController(IMonitorBatchesService alertSvc)
        {
            this.alertSvc = alertSvc;
        }
        [Route("GetDelayedBatches/{*cutOff}")]
        [HttpGet]
        public MonitorReport GetDelayedBatches(string cutOff)
        {
            int defaultCutOff = 30;
            int effectiveCutOff;
            if (!int.TryParse(cutOff, out effectiveCutOff))
                effectiveCutOff = defaultCutOff;
            return alertSvc.GetPendingBatchesToSubmit(effectiveCutOff);
        }
        [HttpGet, Route("GetDelayedPackagesStatus/{*cutOff}")]
        public bool GetDelayedPackagesStatus(string cutOff)
        {
            //Remove if not used.
            int defaultCutOff = 30;
            int effectiveCutOff;
            if (!int.TryParse(cutOff, out effectiveCutOff))
                effectiveCutOff = defaultCutOff;
            var data = alertSvc.GetPendingBatchesToSubmit(effectiveCutOff);
            return data.Status;
        }
        [HttpGet, Route("GetAMGlobalStatus/{*cutOff}")]
        public AMGlobalStatus GetAMGlobalStatus(string cutOff)
        {
            int defaultCutOff = 30;
            int effectiveCutOff;
            if (!int.TryParse(cutOff, out effectiveCutOff))
                effectiveCutOff = defaultCutOff;
            var data = alertSvc.GetPendingBatchesToSubmit(effectiveCutOff);
            return new AMGlobalStatus { DelayedBatches = data.DelayedBatches.Count(), DelayedMailPieces = data.DelayedMailPieces.Count(), MinHour = data.MinHour, MaxHour = data.MaxHour, Status = data.Status, LastBatchReceivedOn = data.LastBatchReceivedOn, LastUpdatedOn = data.LastUpdatedOn };
        }
    }
}
