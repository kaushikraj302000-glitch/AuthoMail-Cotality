using AM.Services;
using AM.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using AM.Entities;
using System.Web.Hosting;
using System.Threading;
using System.Web.Http.Cors;

namespace AM.Web.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [Authorize(Roles = AMRoles.Admin + "," + AMRoles.BA + "," + AMRoles.Client + "," + AMRoles.ClientUserManager + "," + AMRoles.CoreLogicUserManager + "," + AMRoles.PnSOperator + "," + AMRoles.API)]
    [RoutePrefix("api/pullback")]
    public class PullBackApiController : ApiController
    {
        IPullBackService pullBackSvc;
        public PullBackApiController(IPullBackService pullBackSvc)
        {
            this.pullBackSvc = pullBackSvc;
        }
        [Route("GetDateTime")]
        [HttpPost]
        public object GetDateTime()
        {
            return DateTime.Now;
        }

        [Route("RequestPullBack")]
        [HttpPost]
        public object GeneratePullBackRequest([FromBody] List<string> mailPieceIds)
        {
            if (mailPieceIds == null || mailPieceIds.Count == 0)
            {
                return JsonResponseFactory.ErrorResponse(Config.Web_Msg_API_PB_Empty);
            }
            try
            {

                //Dictionary<string, string> mailPieceResponse = new Dictionary<string, string>();
                List<PullBackResponse> mailPieceResponse = new List<PullBackResponse>();
                List<string> mailPieceIdsToPullback = new List<string>();

                var mailPieceData = pullBackSvc.GetMailPieceData(mailPieceIds);

                if (mailPieceData == null || mailPieceData.Count == 0)
                {
                    return JsonResponseFactory.ErrorResponse(Config.Web_Msg_API_PB_DataNull);
                }
                foreach (var item in mailPieceData)
                {
                    if (item.AMMailPieceStatus == "PulledBack" || item.PnSMailPieceStatus == "PulledBack")
                    {
                        mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.MailPieceUniqueId.ToString(), Message = Config.Web_Msg_API_PB_PulledBack, PullBackRequestedOn = item.PullBackRequestedOn, PulledBackRequestedBy = item.UserName, MailPieceStatus = item.AMMailPieceStatus, Success = false });
                    }
                    else if (Convert.ToBoolean(item.IsPullBackRequested))
                    {
                        mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.MailPieceUniqueId.ToString(), Message = Config.Web_Msg_API_PB_PullBackAlreadyRequested, PullBackRequestedOn = item.PullBackRequestedOn, PulledBackRequestedBy = item.UserName, MailPieceStatus = item.AMMailPieceStatus, Success = false });
                    }
                    else if (item.AMBatchPipelineStatus == "Received" || item.AMBatchPipelineStatus == "Downloaded")
                    {
                        mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.MailPieceUniqueId.ToString(), Message = Config.Web_Msg_API_PB_BatchNotValidated, Success = false });
                    }
                    else if (item.AMMailPieceStatus == "Aborted" || item.PnSMailPieceStatus == "Aborted")
                    {
                        mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.MailPieceUniqueId.ToString(), Message = Config.Web_Msg_API_PB_Aborted, Success = false });
                    }
                    else if (item.AMMailPieceStatus == "Error" || item.PnSMailPieceStatus == "Error")
                    {
                        mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.MailPieceUniqueId.ToString(), Message = Config.Web_Msg_API_PB_Error, ErrorDescription = string.IsNullOrEmpty(item.PnSErrorDescription.ToString()) ? item.PnSErrorDescription.ToString() : item.AMErrorDescription.ToString(), Success = false });
                    }
                    else if (item.AMMailPieceStatus == "Shipped" || item.PnSMailPieceStatus == "Shipped")
                    {
                        mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.MailPieceUniqueId.ToString(), Message = Config.Web_Msg_API_PB_Shipped, Success = false });
                    }
                    else if (item.AMMailPieceStatus == "InProgress" && (item.AMBatchPipelineStatus == "Validated" || item.AMBatchPipelineStatus == "Acknowledged" || item.AMBatchPipelineStatus == "Prepped"))
                    {
                        int row1 = pullBackSvc.InsertPullBackRequestData(item.MailPieceUniqueId.ToString(), User.Identity.GetClientId().Value, "PulledBack", string.IsNullOrEmpty(User.Identity.GetUserId().ToString()) ? string.Empty : User.Identity.GetUserId().ToString());
                        int row = pullBackSvc.UpdatePullBackRequestedOn(item.MailPieceUniqueId.ToString(), string.IsNullOrEmpty(User.Identity.GetUserId().ToString()) ? string.Empty : User.Identity.GetUserId().ToString(), "PulledBack");                        
                        mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.MailPieceUniqueId.ToString(), Message = Config.Web_Msg_API_PB_PulledBackSuccess, MailPieceStatus = "PulledBack", PullBackRequestedOn = Common.Now, PulledBackRequestedBy = (User.Identity) == null ? string.Empty : User.Identity.Name, Success = true });
                    }
                    else if (item.AMMailPieceStatus == "InProgress" && item.AMBatchPipelineStatus == "SubmittedToShipping" && !item.PCIBatchPrepEnd.HasValue)
                    {
                        int row1 = pullBackSvc.InsertPullBackRequestData(item.MailPieceUniqueId.ToString(), User.Identity.GetClientId().Value, "PulledBack", string.IsNullOrEmpty(User.Identity.GetUserId().ToString()) ? string.Empty : User.Identity.GetUserId().ToString());
                        int row = pullBackSvc.UpdatePullBackRequestedOn(item.MailPieceUniqueId.ToString(), string.IsNullOrEmpty(User.Identity.GetUserId().ToString()) ? string.Empty : User.Identity.GetUserId().ToString(), "PulledBack");                        
                        mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.MailPieceUniqueId.ToString(), Message = Config.Web_Msg_API_PB_PulledBackSuccess, MailPieceStatus = "PulledBack", PullBackRequestedOn = Common.Now, PulledBackRequestedBy = (User.Identity) == null ? string.Empty : User.Identity.Name, Success = true });
                    }
                    else if (item.AMBatchPipelineStatus == "SubmittedToShipping" && item.PCIBatchPrepEnd.HasValue)
                    {
                        if (item.PrintTrailBatchId == null)
                        {
                            int row1 = pullBackSvc.InsertPullBackRequestData(item.MailPieceUniqueId.ToString(), User.Identity.GetClientId().Value, "PulledBack", string.IsNullOrEmpty(User.Identity.GetUserId().ToString()) ? string.Empty : User.Identity.GetUserId().ToString());
                            int row = pullBackSvc.UpdatePullBackRequestedOn(item.MailPieceUniqueId.ToString(), string.IsNullOrEmpty(User.Identity.GetUserId().ToString()) ? string.Empty : User.Identity.GetUserId().ToString(), "PulledBack");                            
                            mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.MailPieceUniqueId.ToString(), Message = Config.Web_Msg_API_PB_PulledBackSuccess, MailPieceStatus = "PulledBack", PullBackRequestedOn = Common.Now, PulledBackRequestedBy = (User.Identity) == null ? string.Empty : User.Identity.Name, Success = true });
                        }
                        else if (item.PnSMailPieceStatus == "InProgress" && (item.PrintTrailBatchId != null))
                        {
                            mailPieceIdsToPullback.Add(item.MailPieceUniqueId.ToString());
                        }
                    }
                }

                if (mailPieceIdsToPullback.Count > 0)
                {
                    var pullBackData = pullBackSvc.GetPullBackData(mailPieceIdsToPullback);

                    bool isGenerated = pullBackSvc.GeneratePullBackForm(pullBackData);
                    if (isGenerated)
                    {
                        foreach (var item in mailPieceIdsToPullback)
                        {
                            int row1 = pullBackSvc.InsertPullBackRequestData(item.ToString(), User.Identity.GetClientId().Value, "Requested", string.IsNullOrEmpty(User.Identity.GetUserId().ToString()) ? string.Empty : User.Identity.GetUserId().ToString());
                            int row = pullBackSvc.UpdatePullBackRequestedOn(item.ToString(), string.IsNullOrEmpty(User.Identity.GetUserId().ToString()) ? string.Empty : User.Identity.GetUserId().ToString(), string.Empty);
                            if (row > 0)
                            {
                                mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.ToString(), Message = Config.Web_Msg_API_PB_Success, PullBackRequestedOn = Common.Now, PulledBackRequestedBy = (User.Identity) == null ? string.Empty : User.Identity.Name, Success = true });
                            }
                        }
                    }
                    else
                    {
                        return JsonResponseFactory.ErrorResponse(Config.Web_Msg_API_PB_PBReqFormErr);
                    }
                }

                return JsonResponseFactory.SuccessResponse(mailPieceResponse);
            }
            catch (Exception ex)
            {
                return JsonResponseFactory.ErrorResponse(ex.Message);
            }

        }
        //Test Methods
        //[AllowAnonymous]
        //[Route("testpostusps")]
        //public string TestPostUSPS([FromBody] cls p)
        //{
        //    if (p == null && string.IsNullOrWhiteSpace(p.p)) return "String is Null";
        //    return "USPS_Value of P is:" + p.p;
        //}
        //[Route("testpostapi")]
        //public string TestPostAPI([FromBody] cls p)
        //{
        //    if (p == null && string.IsNullOrWhiteSpace(p.p)) return "String is Null";
        //    return "API_Value of P is:" + p.p;
        //}
    }
    [Authorize(Roles = AMRoles.APIUSPSeVS + "," + AMRoles.API), RoutePrefix("api/pullback")]
    public class TestController : ApiController
    {
        [Authorize(Roles = AMRoles.APIUSPSeVS), Route("testpostuspstest")]
        public string TestPostUSPS([FromBody] cls p)
        {
            if (p == null && string.IsNullOrWhiteSpace(p.p)) return "String is Null";
            return "TEST_USPS_Value of P is:" + p.p;
        }
        [Authorize(Roles = AMRoles.API), Route("testpostapitest")]
        public string TestPostAPI([FromBody] cls p)
        {
            if (p == null && string.IsNullOrWhiteSpace(p.p)) return "String is Null";
            return "TEST_API_Value of P is:" + p.p;
        }
    }

    public class cls
    {
        public string p { get; set; }
    }
}
