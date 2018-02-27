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

namespace AM.Web.Controllers
{
    [RoutePrefix("api/pullback")]
    public class PullBackApiController : ApiController
    {
        IPullBackService pullBackSvc;
        public PullBackApiController(IPullBackService pullBackSvc)
        {
            this.pullBackSvc = pullBackSvc;
        }

        [Route("RequestPullBack")]
        [HttpPost]
        public object GeneratePullBackRequest([FromBody] List<string> mailPieceIds)
        {
            //Testing Purpose
            //mailPieceIds = new List<string> { "0C76A5C0-3180-4F34-B04E-DD5338815EDB", "A12E6EFD-01CB-470F-898D-3200FDED4AA9", "C8EF4A23-7B37-4F33-B899-992A162CC31B" };
            if (mailPieceIds == null || mailPieceIds.Count == 0)
            {
                return JsonResponseFactory.ErrorResponse(Config.Msg["MP_Empty"]);
            }
            try
            {

                //Dictionary<string, string> mailPieceResponse = new Dictionary<string, string>();
                List<PullBackResponse> mailPieceResponse = new List<PullBackResponse>();
                List<string> mailPieceIdsToPullback = new List<string>();

                var mailPieceData = pullBackSvc.GetMailPieceData(mailPieceIds);

                if (mailPieceData == null || mailPieceData.Count == 0)
                {
                    return JsonResponseFactory.ErrorResponse(Config.Msg["MP_DataNull"]);
                }
                foreach (var item in mailPieceData)
                {                    
                    if (item.PipelineStatus.ToString() != "Submitted")
                    {
                        mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.UniqueIdentifier.ToString(), Message = Config.Msg["MP_BatchStatusSubmitted"], IsSuccess = false });
                    }
                    else if (Convert.ToBoolean(item.IsPullBackRequested))
                    {
                         mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.UniqueIdentifier.ToString(), Message = Config.Msg["MP_PBRequested"], IsSuccess = false });
                    }
                    else if (item.MailPieceStatus.ToString() == "Aborted")
                    {
                        mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.UniqueIdentifier.ToString(), Message = Config.Msg["MP_Aborted"], IsSuccess = false });
                    }
                    else if (item.MailPieceStatus.ToString() == "PulledBack")
                    {
                        mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.UniqueIdentifier.ToString(), Message = Config.Msg["MP_PulledBack"], IsSuccess = false });
                    }
                    else if (item.MailPieceStatus.ToString() == "Error")
                    {
                        mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.UniqueIdentifier.ToString(), Message = Config.Msg["MP_Error"], IsSuccess = false });
                    }
                    else if (item.MailPieceStatus.ToString() == "Shipped")
                    {
                        mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.UniqueIdentifier.ToString(), Message = Config.Msg["MP_Shipped"], IsSuccess = false });
                    }
                    else if (item.MailPieceStatus.ToString() == "InProgress" && item.PipelineStatus.ToString() == "Submitted")
                    {
                        //UPDATE "PullBackRequestedOn" time
                        int row = pullBackSvc.UpdatePullBackRequestedOn(item.UniqueIdentifier.ToString());
                        if (row > 0)
                        {
                            mailPieceIdsToPullback.Add(item.UniqueIdentifier);
                            mailPieceResponse.Add(new PullBackResponse { MailPieceId = item.UniqueIdentifier, Message = Config.Msg["MP_PB_Success"], IsSuccess = true });
                        }
                    }
                }

                var pullBackData = pullBackSvc.GetPullBackData(mailPieceIdsToPullback);

                bool isGenerated = GeneratePullBackForm(pullBackData);
                if (!isGenerated)
                {
                    return JsonResponseFactory.ErrorResponse(Config.Msg["MP_PBReqFormErr"]);
                }

                return JsonResponseFactory.SuccessResponse(mailPieceResponse);
            }
            catch (Exception ex)
            {
                return JsonResponseFactory.ErrorResponse(ex.Message);
            }

        }

        private bool GeneratePullBackForm(List<dynamic> pullBackData)
        {
            bool isGenerated = true;
            try
            {
                foreach (var item in pullBackData)
                {
                    Dictionary<string, string> dataForPullBackForm = new Dictionary<string, string>();
                    DateTime eastern = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, Common.ClientTimeZone);
                    dataForPullBackForm.Add("RequestDateTime", eastern.ToString());
                    dataForPullBackForm.Add("FileName", string.Format("{0} {1}", item.PrintFileName, item.SubmittedOn.ToString("MM/dd/yy")));
                    dataForPullBackForm.Add("JobNumber", item.PrintBatch.ToString());
                    dataForPullBackForm.Add("PackagesToPull", string.Format("{0} : {1}", item.PrintBatch.ToString(), item.PieceId.ToString()));
                    dataForPullBackForm.Add("Coordinator", string.Format("AuthoMail, ADE, {0}-{1}", item.ClientName, item.DocTypeCode));
                    dataForPullBackForm.Add("PkgInstruction", string.Empty);
                    dataForPullBackForm.Add("Comments", string.Empty);

                    string outputPdfPath = Path.Combine(Common.OutputDir, string.Format("{0}_{1}.pdf", item.PrintBatch.ToString(), DateTime.Now.ToString("ddMMyyyyhhmmss")));
                    Helper.MailMerge(Common.PullBackFormTemplatePath, dataForPullBackForm, outputPdfPath);
                    bool isEmailSent = SendEmailWithPullBackForm(outputPdfPath);

                }
            }
            catch (Exception ex)
            {
                isGenerated = false;
            }
            return isGenerated;
        }
        private bool SendEmailWithPullBackForm(string attachmentPath)
        {
            bool isEmailSent = false;

            try
            {
                using (MailMessage msg = new MailMessage())
                {
                    msg.IsBodyHtml = true;
                    msg.From = new MailAddress(Common.Email_From);
                    msg.To.Add(Common.Email_To);
                    msg.CC.Add(Common.Email_Cc);
                    msg.Subject = string.Format("{0} \"{1}\"", "ADE", "PULL REQUEST");
                    msg.Priority = MailPriority.High;
                    msg.Body = string.Format("Please find attached PullBackRequestForm {0}.", Path.GetFileName(attachmentPath));

                    msg.Attachments.Add(new Attachment(attachmentPath));

                    Common.Email_SMTPClient.Send(msg);

                    isEmailSent = true;
                }

            }
            catch (Exception err)
            {
                throw err;
            }

            return isEmailSent;
        }
    }
}
