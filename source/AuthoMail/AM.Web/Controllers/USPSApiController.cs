using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using AM.Entities;
using AM.Services;
using AM.USPSLib;
using Microsoft.AspNet.Identity;

namespace AM.Web.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    //[Authorize(Roles = AMRoles.API)]
    [RoutePrefix("api/usps")]
    public class USPSApiController : ApiController
    {
        IUSPSService uspsSvc;
        public USPSApiController(IUSPSService uspsSvc)
        {
            this.uspsSvc = uspsSvc;
        }

        [Route("verify")]
        [HttpPost]
        public VerifierAddressResponse VerifyAddress([FromBody]VerifierAddressRequest addressRequest)
        {
            return uspsSvc.VerifyAddressAsync(User.Identity.GetUserId(), addressRequest).Result;
        }

        [Route("eVS")]
        [HttpPost]
        public eVSLabelResponse RequesteVSLabel([FromBody]eVSLabelRequest request)
        {
            return uspsSvc.RequesteVSLabelAsync(User.Identity.GetUserId(), request).Result;
        }
    }
}
