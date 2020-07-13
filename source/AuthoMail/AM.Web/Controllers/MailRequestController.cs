using AM.Entities;
using AM.Services;
using AM.Web.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Xml.Serialization;
using AM.Lib;
using Newtonsoft.Json.Serialization;
using System.Web.Http.Cors;
using System.Threading.Tasks;
using System.Text;
using System.Web.Http.Controllers;
using iTextSharp.text.exceptions;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Office2010.Excel;
using System.Web;

namespace AM.Web.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [Authorize(Roles = AMRoles.Admin + "," + AMRoles.API)]
    [RoutePrefix("api")]
    public class MailRequestController : ApiController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Route("test")]
        [HttpPost]
        public void test()
        {
            File.WriteAllText(@"\\Slxqn1vdappsr01.infosolco.net\FAGOSFTP\ADE_PnS\Test\test.txt", "Jai Swaminarayan");
        }

        public async Task<HttpResponseMessage> CreateAMBatch(MailBatch mailBatch, MailPiece mailPiece, SFTChannel sFTChannel, DateTime receivedOn)
        {
            try
            {
                mailPiece.DocumentOrPackage.IncludePatternBaseString = true;
                //create zip and response
                string zipFilePath = Path.Combine(sFTChannel.Host + sFTChannel.ToADEDir, string.Format("{0}.zip", mailBatch.Id));
                string triggerFilePath = Path.Combine(sFTChannel.Host + sFTChannel.ToADEDir, string.Format("{0}.GO", mailBatch.Id));

                string xml = ToXML(mailBatch);

                using (FileStream zipToOpen = new FileStream(zipFilePath, FileMode.OpenOrCreate))
                {
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                    {
                        ZipArchiveEntry xmlEntry = archive.CreateEntry(string.Format("{0}.xml", mailBatch.Id));
                        using (StreamWriter writer = new StreamWriter(xmlEntry.Open()))
                        {
                            await writer.WriteAsync(xml);
                        }
                        ZipArchiveEntry pdfEntry = archive.CreateEntry(mailPiece.DocumentOrPackage.Name);
                        using (var pdfStream = pdfEntry.Open())
                        using (var payloadStream = new FileStream(mailPiece.DocumentOrPackage.Path, FileMode.Open, FileAccess.Read))
                        {
                            await payloadStream.CopyToAsync(pdfStream, 81920);
                        }
                    }
                }

                if (!File.Exists(triggerFilePath))
                {
                    using (FileStream sourceStream = new FileStream(triggerFilePath,
FileMode.Create, FileAccess.Write, FileShare.None,
bufferSize: 4096, useAsync: true)) { }

                    File.SetLastWriteTime(triggerFilePath, receivedOn);
                }

                if (File.Exists(mailPiece.DocumentOrPackage.Path)) File.Delete(mailPiece.DocumentOrPackage.Path);

                var res = new MailAPIResponse
                {
                    Id = mailPiece.Id,
                    MailingStatus = ShippingStatus.Acknowledgement.ToString(),
                    Message = "Your request is queued for processing."
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception ex)
            {
                var traceId = Guid.NewGuid();
                log.Error("TraceId: {0}, Message: {1}".Fill(traceId, ex.Message), ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unexpected error. TraceId : {0}".Fill(traceId));
            }
        }

        [Route("mailrequest")]
        [HttpPost]
        public async Task<HttpResponseMessage> CreateMailRequest()
        {
            var receivedOn = Common.Now;
            var mailPiece = new MailPiece();
            if (Request.Content.Headers.ContentLength / 1024f / 1024f > Config.MailRequestAPI_MaxRequestSize)
            {
                var res = GetErrorResponse(mailPiece, new MailPieceValidationResult { Code = OutComeCodes.RequestTooLarge, ErrorMessage = "Maximum request length exceeded." }, receivedOn);
                return Request.CreateResponse(HttpStatusCode.RequestEntityTooLarge, res);
            }
            //TO DO: create local path and upload
            //.57/d:/UAT/WEB/
            string root = Config.MailRequestAPI_FilesDownloadPath;
            var provider = new MultipartFormDataStreamProvider(root);

            try
            {
                await Request.Content.ReadAsMultipartAsync(provider);
                SFTChannel sFTChannel;
                Client client;
                MailBatch mailBatch = new MailBatch()
                {
                    Id = Guid.NewGuid()
                };

                var manifest = provider.FormData.GetValues("Manifest");
                var filekeys = provider.FileData.Select(f => f.Headers.ContentDisposition.Name).ToList();
                //validation for "Manifest"
                string manifestErrDesc = ValidateManifestRequest(ref mailPiece, manifest, filekeys);
                var downloadedFiles = provider.FileData.Select(f => f.LocalFileName).ToList();
                if (!manifestErrDesc.IsNullOrWhiteSpace())
                {
                    // delete downloaded files in case of exception, send path, line no, position in bad request
                    DeleteFiles(downloadedFiles);
                    var res = GetErrorResponse(mailPiece, new MailPieceValidationResult { Code = OutComeCodes.Manifest_Missing_Or_Invalid, ErrorMessage = manifestErrDesc }, receivedOn);
                    return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                }

                using (IClientService clientService = new ClientService())
                {
                    sFTChannel = await clientService.GetSFTChannelDetailsBySFTChannelId(User.Identity.SFTChannelId().Value);
                }

                // Validate manifest contents
                mailBatch.MailPieces.Add(mailPiece);
                using (ICommonService commonSvc = new CommonService())
                {
                    //Check mailpiece id is unique.
                    if (commonSvc.CheckForDuplicateMailPieceUniqueId(mailPiece.Id))
                    {
                        DeleteFiles(downloadedFiles);
                        var res = GetErrorResponse(mailPiece, new MailPieceValidationResult { Code = OutComeCodes.Invalid_DuplicateKey, ErrorMessage = string.Format("The specified package id '{0}' conflicts with an existing package. Retry with a new GUID.", mailPiece.Id) }, receivedOn);
                        return Request.CreateResponse(HttpStatusCode.Conflict, res);
                    }
                }

                using (IClientService clientService = new ClientService())
                {
                    client = await clientService.GetClientByAppCode(mailPiece.ClientCode);
                    if (client == null)
                    {
                        DeleteFiles(downloadedFiles);
                        //Return error response and insert entry of invalid_client                                           
                        var invalidClient = await clientService.GetClientByAppCode(sFTChannel.Code.Split('_')[0] + "_Invalid");
                        await InsertInvalidBatch(receivedOn, sFTChannel, mailBatch, invalidClient.Id, "Invalid client code specified.");
                        if (mailPiece.DocumentOrPackage != null) mailPiece.DocumentOrPackage.IncludePatternBaseString = false;
                        var res = GetErrorResponse(mailPiece, new MailPieceValidationResult { Code = OutComeCodes.Invalid_ClientCode, ErrorMessage = string.Format("Invalid client code specified.") }, receivedOn);
                        return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                    }
                    mailBatch.ClientCode = client.AppCode;
                    mailBatch.ClientBatchId = mailBatch.Id.ToString();

                }

                switch (sFTChannel.Code.Split('_')[0].ToUpper())
                {
                    case "ELLIEMAE":
                    default:
                        var result = ValidateClientSpecificRules(mailPiece);
                        if (result.Code != OutComeCodes.No_Error)
                        {
                            DeleteFiles(downloadedFiles);
                            //insert error and response
                            using (IMailBatchService mailBatchService = new MailBatchService())
                            using (ICommonService commonSvc = new CommonService())
                            using (IMailPieceService mailPieceService = new MailPieceService())
                            {

                                int mailBatchId = await mailBatchService.InsertBatchDetails(mailBatch, client.Id, sFTChannel);
                                // insert mailpiece
                                int mailPieceId = await mailPieceService.InsertBatchMailPieceDetails(mailPiece, client.Id, mailBatchId, result.Code, result.ErrorMessage);
                                await commonSvc.UpdateBatchPipeLineStatus(mailBatchId, BatchPipelineStatus.Received, receivedOn);
                                await mailBatchService.MarkMailBatchAsInvalid(mailBatchId, "Request does not contain valid package info.");
                                await commonSvc.UpdateBatchStatus(mailBatchId, BatchStatus.Failed.ToString());
                                await commonSvc.UpdateBatchPipeLineStatus(mailBatchId, BatchPipelineStatus.ReportPrepped, Common.Now);
                                await commonSvc.UpdateBatchPipeLineStatus(mailBatchId, BatchPipelineStatus.ReportedBack, Common.Now);
                            }
                            if (mailPiece.DocumentOrPackage != null) mailPiece.DocumentOrPackage.IncludePatternBaseString = false;
                            var res = GetErrorResponse(mailPiece, new MailPieceValidationResult { Code = result.Code, ErrorMessage = result.ErrorMessage }, receivedOn);

                            return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                        }
                        break;
                }


                // merge PDFS into a single file, use File.GetRandomFileName method to get unique filename

                var files = mailPiece.DocumentOrPackage.Files
                    .Select(f =>
                    {
                        var fileData = provider.FileData.SingleOrDefault(fd => fd.Headers.ContentDisposition.Name.Replace("\"", "") == f);
                        return new KeyValuePair<string, MultipartFileData>(f, fileData);
                    })
                    .ToList();

                var nullFilePathList = files.Where(f => f.Value == null).ToList();

                // filepath null validation check
                if (nullFilePathList.Count > 0)
                {
                    var res = GetErrorResponse(mailPiece, new MailPieceValidationResult { Code = OutComeCodes.Missing_PDF_File, ErrorMessage = "Pdf file not found for the following file key(s): {0}".Fill(string.Join(",", nullFilePathList.Select(n => n.Key))) }, receivedOn);
                    return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                }

                //// count pdf pages of each file             
                int i = 0;
                string temp = null;
                string prevFile = null;
                if (mailPiece.CustomInfo == null) mailPiece.CustomInfo = new List<Metadata>();

                foreach (var customInfo in mailPiece.CustomInfo)
                {
                    if (customInfo.Key.IsNullOrWhiteSpace())
                    {
                        var res = GetErrorResponse(mailPiece, new MailPieceValidationResult { Code = OutComeCodes.Manifest_Missing_Or_Invalid, ErrorMessage = "CustomInfo's Key is mandatory." }, receivedOn);
                        return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                    }
                }

                do
                {
                    try
                    {
                        var file = files[i].Value;
                        var pageCount = PdfUtil.GetPageCount(file.LocalFileName);

                        mailPiece.CustomInfo.Add(new Metadata { Key = "Files", Value = "FileKey: {0} | FileName:{1} | PageCount: {2}".Fill(files[i].Key, file.Headers.ContentDisposition.FileName.Replace("\"", ""), pageCount) });

                        //if (temp != null) File.Delete(temp);
                        var fileName = mailPiece.DocumentOrPackage.Name.IsNullOrWhiteSpace() ? Path.GetRandomFileName() : mailPiece.DocumentOrPackage.Name;

                        temp = Path.Combine(root, fileName);
                        temp = Path.ChangeExtension(temp, ".pdf");

                        if (prevFile == null)
                            File.Copy(file.LocalFileName, temp);
                        else
                        {
                            PdfUtil.MergePDFs(prevFile, file.LocalFileName, temp);
                            File.Delete(prevFile);
                        }
                        prevFile = temp;
                    }
                    catch (Exception e)
                    {
                        log.Error(e.Message, e);
                        // if there are any exceptions while counting pages or merging, don't delete files and send badreqeust with mailresponse object, it should contain file names
                        var res = GetErrorResponse(mailPiece, new MailPieceValidationResult { Code = OutComeCodes.Invalid_PDF_File, ErrorMessage = "'{0}' pdf file is either corrupt or has no pages.".Fill(files[i].Key) }, receivedOn);
                        return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                    }
                    i++;
                } while (i < files.Count);

                //File.Move(temp, finalPdfPath);
                var finalPdfPath = temp;

                // call method passing mailpiece
                /// mp.DocumentOrPackage.Path = "new PDF name"
                mailPiece.DocumentOrPackage.Name = "{0}.pdf".Fill(mailPiece.BusinessFriendlyIdValue.GetValidFileName());
                mailPiece.DocumentOrPackage.Path = finalPdfPath;
                var response = await CreateAMBatch(mailBatch, mailPiece, sFTChannel, receivedOn);
                var responseContent = await response.Content.ReadAsStringAsync();
                var mailResponse = JsonConvert.DeserializeObject<MailAPIResponse>(responseContent);
                // if get success response then delete files. 
                if (response.IsSuccessStatusCode)
                {
                    DeleteFiles(downloadedFiles);
                    return Request.CreateResponse(HttpStatusCode.OK, mailResponse);
                }
                else
                {
                    return Request.CreateResponse(response.StatusCode, mailResponse);
                }

            }
            catch (Exception e)
            {
                var traceId = Guid.NewGuid();
                log.Error("TraceId: {0}, Message: {1}".Fill(traceId, e.Message), e);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unexpected error. TraceId : {0}".Fill(traceId));
            }
        }

        private static string ValidateManifestRequest(ref MailPiece mailPiece, string[] manifest, List<string> filekeys)
        {
            string manifestErrDesc = null;
            if (manifest == null)
            {
                manifestErrDesc = "Manifest is mandatory.";
            }
            else if (manifest.Length > 1)
            {
                manifestErrDesc = "Multiple manifest entries.";
            }
            //  File keys must be unique
            else if (filekeys.Count != filekeys.Distinct().Count())
            {
                manifestErrDesc = "File key names must be unique.";
            }
            else
            {
                // try catch special
                try
                {
                    mailPiece = JsonConvert.DeserializeObject<MailPiece>(manifest[0]);
                    mailPiece.Id = mailPiece.Id == Guid.Empty ? Guid.NewGuid() : mailPiece.Id;
                }
                catch (JsonReaderException jrEx)
                {
                    manifestErrDesc = string.Format("Could not process manifest: Path '{0}', line {1}, position {2}.", jrEx.Path, jrEx.LineNumber, jrEx.LinePosition);
                }
            }
            return manifestErrDesc;
        }

        private static void DeleteFiles(List<string> files)
        {
            foreach (var file in files)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
        }

        [Route("mailresponse")]
        [HttpGet]
        public List<MailAPIResponse> MailAPIResponse(string mailPieceIds)
        {
            if (mailPieceIds == null) throw new HttpResponseException(HttpStatusCode.BadRequest);

            List<string> uniqueIds = mailPieceIds.Split(',').ToList();
            List<MailAPIResponse> mailAPIResponses = new List<MailAPIResponse>();

            foreach (var mailPieceId in uniqueIds)
            {
                using (MailPieceService mailPieceService = new MailPieceService())
                {
                    var response = mailPieceService.GetMailPieceDetailsByMailPieceUniqueId(mailPieceId);
                    if (response != null)
                    {
                        mailAPIResponses.Add(new MailAPIResponse
                        {
                            Id = response.UniqueId,
                            ReceivedOn = response.ReceivedOn,
                            ClientCode = response.AppCode,
                            BusinessFriendlyIdName = response.BusinessFriendlyIdName,
                            BusinessFriendlyIdValue = response.BusinessFriendlyIdValue,
                            DocumentOrPackage = new DocumentOrPackage { Name = response.FileName, Inserts = response.Inserts },
                            RecipientInfo = new RecipientInfo
                            {
                                Recipient = new Recipient
                                {
                                    Organization = response.RcptOrganization,
                                    Prefix = response.RcptPrefix,
                                    Suffix = response.RcptSuffix,
                                    FirstName = response.RcptFirstName,
                                    MiddleName = response.RcptMiddleName,
                                    LastName = response.RcptLastName,
                                    Phone = response.RcptPhone,
                                    EmailAddress = response.RcptEmailId
                                },
                                Address = new Entities.Address
                                {
                                    OptionalLine1AboveStreet = response.RcptAddrOptionalLine1AboveStreet,
                                    OptionalLine1BelowStreet = response.RcptAddrOptionalLine1BelowStreet,
                                    OptionalLine2AboveStreet = response.RcptAddrOptionalLine2AboveStreet,
                                    OptionalLine2BelowStreet = response.RcptAddrOptionalLine2BelowStreet,
                                    AptOrSuiteOrOther = response.RcptAddressAptOrSuiteOrOther,
                                    Street = response.RcptAddressStreet,
                                    City = response.RcptAddressCity,
                                    State = response.RcptAddressState,
                                    Zip = response.RcptAddressZIP,
                                    Country = response.RcptAddressCountry
                                }
                            },
                            MailingStatus = response.ShippingStatus,
                            ErrorCode = response.ErrorCode,
                            ErrorDesc = response.ErrorDesc,
                            ShippedOn = response.ShippedOn,
                            ShippingMethod = response.ShippingCarrier,
                            ShippingServiceType = response.ShippingServiceType,
                            TrackingNumber = response.TrackingNo,
                            TotalPages = response.TotalPages,
                            TotalSheets = response.TotalSheets,
                            PulledBackOn = response.PulledBackOn
                        });
                    }
                    else
                    {
                        mailAPIResponses.Add(new MailAPIResponse
                        {
                            Id = Guid.Parse(mailPieceId),
                            MailingStatus = ShippingStatus.NotFound.ToString(),
                            Message = "The specified package does not exist."
                        });
                    }
                }
            }
            return mailAPIResponses;
        }

        //[Route("emresponse")]
        //[HttpPost]
        //public async Task<string> ReceviedResponse(List<MailAPIResponse> mailResponse)
        //{
        //    var re = Request;
        //    var headers = re.Headers;
        //    string token = string.Empty;
        //    if (headers.Contains("X-API-Key"))
        //    {
        //        token = headers.GetValues("X-API-Key").First();
        //    }
        //    if (headers.Contains("Elli-Signature"))
        //    {
        //        token = headers.GetValues("Elli-Signature").First();
        //    }
        //    return token;
        //}

        private MailAPIResponse GetErrorResponse(MailPiece mailPiece, MailPieceValidationResult validationResult, DateTime receivedOn)
        {
            return new MailAPIResponse { Id = mailPiece.Id, ClientCode = mailPiece.ClientCode, BusinessFriendlyIdName = mailPiece.BusinessFriendlyIdName, BusinessFriendlyIdValue = mailPiece.BusinessFriendlyIdValue, DocumentOrPackage = mailPiece.DocumentOrPackage, RecipientInfo = mailPiece.RecipientInfo, MailingStatus = ShippingStatus.Error.ToString(), ErrorCode = validationResult.Code, ErrorDesc = validationResult.ErrorMessage, ReceivedOn = receivedOn };
        }

        private async static Task InsertInvalidBatch(DateTime receivedTime, SFTChannel sFTChannel, MailBatch mailBatch, int clientId, string errorMessage)
        {
            using (IMailBatchService mailBatchService = new MailBatchService())
            using (ICommonService commonSvc = new CommonService())
            using (IMailPieceService mailPieceService = new MailPieceService())
            {
                int mailBatchId = await mailBatchService.InsertBatchDetails(mailBatch, clientId, sFTChannel);
                await commonSvc.UpdateBatchPipeLineStatus(mailBatchId, BatchPipelineStatus.Received, receivedTime);
                await mailBatchService.MarkMailBatchAsInvalid(mailBatchId, errorMessage);
                await commonSvc.UpdateBatchStatus(mailBatchId, BatchStatus.Failed.ToString());
                await commonSvc.UpdateBatchPipeLineStatus(mailBatchId, BatchPipelineStatus.ReportPrepped, Common.Now);
                await commonSvc.UpdateBatchPipeLineStatus(mailBatchId, BatchPipelineStatus.ReportedBack, Common.Now);
            }
        }

        public string ToXML(MailBatch mailBatch)
        {
            using (var stringwriter = new System.IO.StringWriter())
            {
                var serializer = new XmlSerializer(mailBatch.GetType());
                serializer.Serialize(stringwriter, mailBatch);
                return stringwriter.ToString();
            }
        }

        private IEnumerable<MailPieceValidationResult> GetAllMailPieceBasicErrors(MailPiece mailPiece)
        {
            //Duplicate key validation
            using (ICommonService commonSvc = new CommonService())
            {
                if (commonSvc.CheckForDuplicateMailPieceUniqueId(mailPiece.Id))
                {
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Invalid_DuplicateKey, ErrorMessage = string.Format("The specified package id '{0}' conflicts with an existing package. Retry with a new GUID.", mailPiece.Id) };
                }
            }

            if (mailPiece.RecipientInfo != null)
            {
                if (mailPiece.RecipientInfo.Address.Street.IsNullOrWhiteSpace() || mailPiece.RecipientInfo.Address.City.IsNullOrWhiteSpace() || mailPiece.RecipientInfo.Address.State.IsNullOrWhiteSpace() || mailPiece.RecipientInfo.Address.Zip.IsNullOrWhiteSpace())
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Mailing_Address_Missing_Or_Invalid, ErrorMessage = "Recipient address is mandatory." };

                if (mailPiece.RecipientInfo.Recipient.FirstName.IsNullOrWhiteSpace() || mailPiece.RecipientInfo.Recipient.LastName.IsNullOrWhiteSpace())
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Recipient_Details_Missing_Or_Invalid, ErrorMessage = "Recipient first and last name are mandatory." };
            }

            if (mailPiece.ReturnInfo != null)
            {
                if (mailPiece.ReturnInfo.Address.Street.IsNullOrWhiteSpace() || mailPiece.ReturnInfo.Address.City.IsNullOrWhiteSpace() || mailPiece.ReturnInfo.Address.State.IsNullOrWhiteSpace() || mailPiece.ReturnInfo.Address.Zip.IsNullOrWhiteSpace())
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Mailing_Address_Missing_Or_Invalid, ErrorMessage = "Return recipient address is mandatory." };

                if (mailPiece.ReturnInfo.Recipient.FirstName.IsNullOrWhiteSpace() || mailPiece.ReturnInfo.Recipient.LastName.IsNullOrWhiteSpace())
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Recipient_Details_Missing_Or_Invalid, ErrorMessage = "Return recipient first and last name are mandatory." };
            }

            if (mailPiece.DocumentOrPackageType.IsNullOrWhiteSpace())
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Invalid_PackageType, ErrorMessage = "Document or package type is not specified." };

            if (mailPiece.eDelivery != null)
            {
                if (mailPiece.RecipientInfo.Recipient.EmailAddress.IsNullOrWhiteSpace())
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Recipient_Details_Missing_Or_Invalid, ErrorMessage = "Email address is mandatory." };
                if (mailPiece.eDelivery.ExpiryDate < Common.EstToday)
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Issue_In_EDelivery, ErrorMessage = "The expiration date must be greater than or equal to current date." };
            }

            if (mailPiece.BusinessFriendlyIdName.IsNullOrWhiteSpace() || mailPiece.BusinessFriendlyIdValue.IsNullOrWhiteSpace())
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Invalid_BusinessIdentifier, ErrorMessage = "Business friendly Id or value is not specified in package." };

            // TODO : check DocumentOrPackage is not empty and DocumentOrPackage.Files property is not empty
            if (mailPiece.DocumentOrPackage == null)
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Missing_DocumentOrPackage, ErrorMessage = "DocumentOrPackage is mandatory." };
            else if (mailPiece.DocumentOrPackage.Files == null)
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Missing_Files, ErrorMessage = "Files property is mandatory." };

            yield break;
        }

        private MailPieceValidationResult ValidateClientSpecificRules(MailPiece mailPiece)
        {
            var errors = GetAllMailPieceBasicErrors(mailPiece).ToList();

            if (errors.Count > 0)
            {
                return new MailPieceValidationResult() { Code = errors.First().Code, ErrorMessage = string.Join(" | ", errors.Select(row => row.ErrorMessage)) };
            }

            return new MailPieceValidationResult() { Code = OutComeCodes.No_Error };
        }

        public class MailPieceValidationResult
        {
            public string Code { get; set; }
            public string ErrorMessage { get; set; }
        }
    }

}
