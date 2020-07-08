using AM.Entities;
using AM.Services;
using AM.Services.Services;
using DELFUS.TemplateImporter;
using System;
using System.Collections.Generic;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AM.Web.Models;

namespace AM.Web.Controllers
{
    [Authorize(Roles = AMRoles.Admin + "," + AMRoles.Delfus)]
    public class DelfusTemplateController : Controller
    {
        IDelfusTemplateService delfusTemplateSvc;
        public DelfusTemplateController(IDelfusTemplateService delfusTemplateSvc)
        {
            this.delfusTemplateSvc = delfusTemplateSvc;
        }
        // GET: Template
        [HttpGet]
        public ActionResult Templates()
        {
            return View();
        }
        [HttpGet]
        public ActionResult TemplateSummary()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> SaveDelfusTemplates(HttpPostedFileBase template)
        {
            IEnumerable<string> mastermergeList = await delfusTemplateSvc.GetDelfusMergeList(Config.Web_Delfus_ConnectionString);
            var temp = template.InputStream;
            TemplateValidation validation = new TemplateValidation();
            byte[] fileData = GetByteArrayFromFile(template);

            TemplateValidationInfo validationInfo = validation.VerifyTemplate(fileData, mastermergeList.ToList());
            string error = string.Empty;
            if (validationInfo.HasAnchorTagIssue)
            {
                error = "This template has anchor tag issue.";
            }
            if (validationInfo.NewMergeFields.Count > 0)
            {
                string newline = error.Length > 0 ? "\n" : "";
                error += $"{newline} This template have new merge fields ({string.Join(",", validationInfo.NewMergeFields)}).";
            }
            if (!string.IsNullOrEmpty(error))
            {
                await delfusTemplateSvc.SaveTemplate(
                     new DelfusTemplate
                     {
                         FileName = Path.GetFileNameWithoutExtension(template.FileName),
                         FilePath = template.FileName,
                         ModifiedOn = DateTime.UtcNow,
                         ModifiedById = User.Identity.GetUserId(),
                         Status = "Invalid",
                         Reason = error
                     }
                     );
                return Json(JsonResponseFactory.ErrorResponse(error));
            }
            //}

            // ToDo: UsesAgencyAddress implementation
            List<string> errors = new List<string>();
            string[] fileType = { ".doc", ".docx" };
            if (!fileType.Contains(Path.GetExtension(template.FileName)))
            {
                await delfusTemplateSvc.SaveTemplate(
                     new DelfusTemplate
                     {
                         FileName = Path.GetFileNameWithoutExtension(template.FileName),
                         FilePath = template.FileName,
                         ModifiedOn = DateTime.UtcNow,
                         ModifiedById = User.Identity.GetUserId(),
                         Status = "Invalid",
                         Reason = "File type not supported."
                     }
                     );
                return Json(JsonResponseFactory.ErrorResponse("File type not supported."));
            }
            string uploadURL = $@"{ConfigurationManager.AppSettings["DelfusAPIBaseUrl"]}\SaveTemplate";// ConfigurationManager.AppSettings["DelfusTemplateUploadUrl"].ToString();
            bool isTemplateExists = await IsTemplateExists(template.FileName);
            var uploadResult = await UploadFileToServer(template.FileName, fileData, uploadURL);


            if (uploadResult.Key)
            {
                var result = await delfusTemplateSvc.SaveTemplate(
                     new DelfusTemplate
                     {
                         FileName = Path.GetFileNameWithoutExtension(template.FileName),
                         FilePath = template.FileName,
                         ModifiedOn = DateTime.UtcNow,
                         ModifiedById = User.Identity.GetUserId(),
                         Status = isTemplateExists ? "Modified" : "Added"
                     }
                     );
                return Json(JsonResponseFactory.SuccessResponse("Template uploaded successfully."));
            }
            else
            {
                errors.Add(uploadResult.Value);
                await delfusTemplateSvc.SaveTemplate(
                     new DelfusTemplate
                     {
                         FileName = Path.GetFileNameWithoutExtension(template.FileName),
                         FilePath = template.FileName,
                         ModifiedOn = DateTime.UtcNow,
                         ModifiedById = User.Identity.GetUserId(),
                         Status = "Invalid",
                         Reason = uploadResult.Value
                     }
                     );
            }
            return Json(JsonResponseFactory.ErrorResponse(errors));

            //HttpPostedFileBase template = templates.FirstOrDefault();


        }
        [HttpPost]
        public async Task<ActionResult> DownloadDelfusTemplate(string fileName)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Id", "5b189031-eecc-42d7-8b6d-b531caf1036e");
                HttpResponseMessage response = new HttpResponseMessage();
                response = await client.GetAsync($@"{ConfigurationManager.AppSettings["DelfusAPIBaseUrl"]}\DownloadTemplate?fileName={fileName}");
                if (response.IsSuccessStatusCode)
                {
                    var stream = await response.Content.ReadAsByteArrayAsync();
                    return Json(JsonResponseFactory.SuccessResponse(
                    new
                    {
                        fileName = Path.GetFileNameWithoutExtension(fileName),
                        stream = Convert.ToBase64String(stream)
                    }
                    ));
                }
                else
                {
                    return Json(JsonResponseFactory.ErrorResponse("Template not found."));
                }
            }
            catch (Exception ex)
            {
                return Json(JsonResponseFactory.ErrorResponse(ex.Message));
            }
        }
        [HttpGet]
        public async Task<object> DelfusTemplatesList()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Id", "5b189031-eecc-42d7-8b6d-b531caf1036e");
            HttpResponseMessage response = new HttpResponseMessage();
            response = await client.GetAsync($@"{ConfigurationManager.AppSettings["DelfusAPIBaseUrl"]}\GetTemplates");
            if (response.IsSuccessStatusCode)
            {
                return Json(new { data = await response.Content.ReadAsAsync<List<FileDetails>>() }, JsonRequestBehavior.AllowGet);
            }

            return Json(null, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<object> DelfusTemplatesSummary()
        {
            var data = await delfusTemplateSvc.DelfusTemplateSummary();
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<object> DelfusTemplatesHistory(string fileName)
        {
            var data = await delfusTemplateSvc.DelfusTemplatesByFileName(fileName);
            return Json(new { data = data.ToList() }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> RemoveDelfusTemplate(string fileName)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Id", "5b189031-eecc-42d7-8b6d-b531caf1036e");
                HttpResponseMessage response = new HttpResponseMessage();
                response = await client.DeleteAsync($@"{ConfigurationManager.AppSettings["DelfusAPIBaseUrl"]}\DeleteTemplate?fileName={fileName}");
                if (response.IsSuccessStatusCode)
                {
                    await delfusTemplateSvc.SaveTemplate(
                     new DelfusTemplate
                     {
                         FileName = Path.GetFileNameWithoutExtension(fileName),
                         FilePath = fileName,
                         ModifiedOn = DateTime.UtcNow,
                         ModifiedById = User.Identity.GetUserId(),
                         Status = "Deleted"
                     }
                     );
                    return Json(JsonResponseFactory.SuccessResponse(), JsonRequestBehavior.AllowGet);
                }

                return Json(null, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(JsonResponseFactory.ErrorResponse(ex.Message));
            }
        }

        public async Task<bool> IsTemplateExists(string fileName)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Id", "5b189031-eecc-42d7-8b6d-b531caf1036e");
            HttpResponseMessage response = new HttpResponseMessage();
            response = await client.GetAsync($@"{ConfigurationManager.AppSettings["DelfusAPIBaseUrl"]}\IsTemplateExists?fileName={fileName}");
            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            return false;
        }
        
        public async Task<KeyValuePair<bool, string>> UploadFileToServer(string fileName, byte[] fileData, string uploadURL)
        {

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Id", "5b189031-eecc-42d7-8b6d-b531caf1036e");
            client.DefaultRequestHeaders.Add("FileName", fileName);
            MultipartFormDataContent form = new MultipartFormDataContent();
            HttpContent content = new StringContent("file");
            using (MemoryStream memoryStream = new MemoryStream(fileData))
            {
                content = new StreamContent(memoryStream);
                content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "file",
                    FileName = fileName
                };
                form.Add(content, "file", fileName);
                HttpResponseMessage response = await client.PostAsync(uploadURL, form);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsAsync<string>();
                    return new KeyValuePair<bool, string>(true, result);
                }
                else
                {
                    return new KeyValuePair<bool, string>(false, "File not uploaded, Please try again.");
                }

            }

        }

        public byte[] GetByteArrayFromFile(HttpPostedFileBase fileBase)
        {
            using (MemoryStream target = new MemoryStream())
            {
                fileBase.InputStream.CopyTo(target);
                return target.ToArray();
            }

        }
        
    }
}