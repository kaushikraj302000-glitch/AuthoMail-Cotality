using AM.Services.Services;
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
    public class PullBackApiController : ApiController
    {
        IPullBackService pullBackSvc;
        public PullBackApiController()
        {
            this.pullBackSvc = new PullBackService();
        }

        [HttpGet]
        public string GeneratePullBackForm(List<string> mailPieceIds)
        {            
            //Testing Purpose
            mailPieceIds = new List<string> { "0C76A5C0-3180-4F34-B04E-DD5338815EDB", "A12E6EFD-01CB-470F-898D-3200FDED4AA9", "C8EF4A23-7B37-4F33-B899-992A162CC31B" };

            var pullBackData = pullBackSvc.GetPullBackData(mailPieceIds);

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

            return "true";
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
