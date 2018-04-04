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
using AM.Entities.IdentityModels;
using System.Web.Hosting;

namespace AM.Web.Controllers
{
    [Authorize]
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
            //try
            //{
            //    using (var existingFileStream = new FileStream(Config.API_PB_PullBackFormTemplatePath, FileMode.Open, FileAccess.Read))
            //    {
            //        var data = new List<PullBackRequestForm>() { 
            //        new PullBackRequestForm{
            //            PrintFileName = "002416-1102-USAA-Approval.zip",
            //            SubmittedOn = DateTime.UtcNow,
            //            PrintBatch = 1102,
            //            PieceId = "1",
            //            ClientName = "USAA",
            //            DocTypeCode = "Approval"
            //        }
            //        };
            //        GeneratePullBackForm(data);
            //        return "Valid";
            //    }
            //}
            //catch (Exception e)
            //{

            //    return e.ToString();
            //}
            //Testing Purpose
            //mailPieceIds = new List<string> { "0C76A5C0-3180-4F34-B04E-DD5338815EDB", "A12E6EFD-01CB-470F-898D-3200FDED4AA9", "C8EF4A23-7B37-4F33-B899-992A162CC31B" };
            if (mailPieceIds == null || mailPieceIds.Count == 0)
            {
                return JsonResponseFactory.ErrorResponse(Config.Msg_API_PB_Empty);
            }
            try
            {

                //Dictionary<string, string> mailPieceResponse = new Dictionary<string, string>();
                List<PullBackResponse> mailPieceResponse = new List<PullBackResponse>();
                List<string> mailPieceIdsToPullback = new List<string>();

                var mailPieceData = pullBackSvc.GetMailPieceData(mailPieceIds);

                if (mailPieceData == null || mailPieceData.Count == 0)
                {
                    return JsonResponseFactory.ErrorResponse(Config.Msg_API_PB_DataNull);
                }
                foreach (var item in mailPieceData)
                {
                    if (item.PipelineStatus.ToString() == "Received" || item.PipelineStatus.ToString() == "Downloaded" || item.PipelineStatus.ToString() == "Prepped")
                    {
                        mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.MailPieceUniqueId.ToString(), Message = Config.Msg_API_PB_BatchStatusSubmitted, Success = false });
                    }
                    else if (Convert.ToBoolean(item.IsPullBackRequested))
                    {
                        mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.MailPieceUniqueId.ToString(), Message = Config.Msg_API_PB_PullBackAlreadyRequested, PullBackRequestedOn = item.PullBackRequestedOn, PulledBackRequestedBy = item.UserName, Success = false });
                    }
                    else if (item.MailPieceStatus.ToString() == "Aborted")
                    {
                        mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.MailPieceUniqueId.ToString(), Message = Config.Msg_API_PB_Aborted, Success = false });
                    }
                    else if (item.MailPieceStatus.ToString() == "PulledBack")
                    {
                        mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.MailPieceUniqueId.ToString(), Message = Config.Msg_API_PB_PulledBack, Success = false });
                    }
                    else if (item.MailPieceStatus.ToString() == "Error")
                    {
                        mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.MailPieceUniqueId.ToString(), Message = Config.Msg_API_PB_Error, Success = false });
                    }
                    else if (item.MailPieceStatus.ToString() == "Shipped")
                    {
                        mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.MailPieceUniqueId.ToString(), Message = Config.Msg_API_PB_Shipped, Success = false });
                    }
                    else if (item.MailPieceStatus.ToString() == "InProgress" && item.PipelineStatus.ToString() == "Submitted")
                    {
                        mailPieceIdsToPullback.Add(item.MailPieceUniqueId.ToString());
                    }
                }

                var pullBackData = pullBackSvc.GetPullBackData(mailPieceIdsToPullback);

                bool isGenerated = pullBackSvc.GeneratePullBackForm(pullBackData);
                if (isGenerated)
                {
                    foreach (var item in mailPieceIdsToPullback)
                    {
                        int row = pullBackSvc.UpdatePullBackRequestedOn(item.ToString(), string.IsNullOrEmpty(User.Identity.GetUserId().ToString()) ? string.Empty : User.Identity.GetUserId().ToString());
                        if (row > 0)
                        {
                            mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.ToString(), Message = Config.Msg_API_PB_Success, PullBackRequestedOn = Common.Now, PulledBackRequestedBy = (User.Identity) == null ? string.Empty : User.Identity.Name, Success = true });
                        }
                        int row1 = pullBackSvc.InsertPullBackRequestData(item.ToString(), User.Identity.GetClientId().Value);
                    }
                }
                else
                {
                    return JsonResponseFactory.ErrorResponse(Config.Msg_API_PB_PBReqFormErr);
                }

                return JsonResponseFactory.SuccessResponse(mailPieceResponse);
            }
            catch (Exception ex)
            {
                return JsonResponseFactory.ErrorResponse(ex.Message);
            }

        }
       
    }
}
