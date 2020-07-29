using AM.Entities;
using AM.Services;
using Newtonsoft.Json;
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
using System.Web.Http.Cors;
using System.Threading.Tasks;
using System.Diagnostics;
using iTextSharp;
using iTextSharp.text.pdf;
using iTextSharp.text;

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
                        // ZipFile.CreateFromDirectory("", "", CompressionLevel.Fastest, false);
                        ZipArchiveEntry xmlEntry = archive.CreateEntry(string.Format("{0}.xml", mailBatch.Id));
                        using (StreamWriter writer = new StreamWriter(xmlEntry.Open()))
                        {
                            await writer.WriteAsync(xml);
                        }
                        if (!mailPiece.DocumentOrPackage.Path.IsNullOrWhiteSpace())
                        {
                            ZipArchiveEntry pdfEntry = archive.CreateEntry(mailPiece.DocumentOrPackage.Name);
                            using (var pdfStream = pdfEntry.Open())
                            using (var payloadStream = new FileStream(mailPiece.DocumentOrPackage.Path, FileMode.Open, FileAccess.Read))
                            {
                                await payloadStream.CopyToAsync(pdfStream, 81920);
                            }
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
            List<double> perf = new List<double>();
            Stopwatch sw = new Stopwatch();
            sw.Start();
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
            sw.Stop();
            perf.Add(sw.Elapsed.TotalSeconds);
            log.DebugFormat("Request size check: {0}", sw.Elapsed.TotalSeconds);
            sw.Restart();
            var provider = new MultipartFormDataStreamProvider(root);

            try
            {
                await Request.Content.ReadAsMultipartAsync(provider);
                sw.Stop();
                perf.Add(sw.Elapsed.TotalSeconds);
                log.DebugFormat("Files Downloaded: {0}", sw.Elapsed.TotalSeconds);
                sw.Restart();
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
                        var result = ValidateClientSpecificRules(mailPiece,client);
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

                if (mailPiece.CustomInfo == null) mailPiece.CustomInfo = new List<Metadata>();

                foreach (var customInfo in mailPiece.CustomInfo)
                {
                    if (customInfo.Key.IsNullOrWhiteSpace())
                    {
                        var res = GetErrorResponse(mailPiece, new MailPieceValidationResult { Code = OutComeCodes.Manifest_Missing_Or_Invalid, ErrorMessage = "CustomInfo's Key is mandatory." }, receivedOn);
                        return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                    }
                }

                
                if ((mailPiece.DocumentOrPackage.Files != null && mailPiece.DocumentOrPackage.Files.Count == 0 && mailPiece.DocumentOrPackage.Inserts.IsNullOrWhiteSpace()) || (mailPiece.DocumentOrPackage.Files == null && mailPiece.DocumentOrPackage.Inserts.IsNullOrWhiteSpace()))
                {
                    var res = GetErrorResponse(mailPiece, new MailPieceValidationResult { Code = OutComeCodes.Missing_Files, ErrorMessage = "Request cannot proceed further without Files or Inserts value." }, receivedOn);
                    return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                }

                if (mailPiece.DocumentOrPackage.Files != null && mailPiece.DocumentOrPackage.Files.Count > 0)
                {
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

                    if (files.Count > 99)
                    {
                        var res = GetErrorResponse(mailPiece, new MailPieceValidationResult { Code = OutComeCodes.Manifest_Missing_Or_Invalid, ErrorMessage = "Up to 99 pdf files are supported per mailpiece." }, receivedOn);
                        return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                    }
                    sw.Stop();
                    perf.Add(sw.Elapsed.TotalSeconds);
                    log.DebugFormat("Validation complete: {0}", sw.Elapsed.TotalSeconds);

                    sw.Restart();
                    string finalPdfPath = null;

                    string destPdf = Path.Combine(root, Path.GetRandomFileName());
                    destPdf = Path.ChangeExtension(destPdf, ".pdf");
                    using (var ds = new FileStream(destPdf, FileMode.Create))
                    using (var doc = new Document())
                    using (var copy = new PdfSmartCopy(doc, ds))
                    {
                        doc.Open();
                        // This is done to open and merge (Secured) PDFs which allow printing but not copying or merging
                        PdfReader.unethicalreading = true;
                        //Loop through each input file                        
                        foreach (var file in files)
                        {
                            try
                            {
                                using (var reader = new PdfReader(file.Value.LocalFileName))
                                {
                                    mailPiece.CustomInfo.Add(new Metadata { Key = file.Key, Value = "FileName:{0} | PageCount: {1}".Fill(file.Value.Headers.ContentDisposition.FileName.Replace("\"", ""), reader.NumberOfPages) });
                                    //Add the entire document
                                    copy.AddDocument(reader);
                                    reader.Close();
                                }
                            }
                            catch (Exception e)
                            {
                                log.Error(e.Message, e);
                                // if there are any exceptions while counting pages or merging, don't delete files and send badreqeust with mailresponse object, it should contain file names
                                var res = GetErrorResponse(mailPiece, new MailPieceValidationResult { Code = OutComeCodes.Invalid_PDF_File, ErrorMessage = "'{0}' pdf file is either corrupt or has no pages.".Fill(file.Key) }, receivedOn);
                                return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                            }
                        }
                        PdfReader.unethicalreading = false;
                        doc.Close();
                    }
                    finalPdfPath = destPdf;
                    mailPiece.DocumentOrPackage.Path = finalPdfPath;
                    sw.Stop();
                    perf.Add(sw.Elapsed.TotalSeconds);
                    log.DebugFormat("Merge pdfs completed: {0}", sw.Elapsed.TotalSeconds);
                }

               
                sw.Restart();


                // call method passing mailpiece

                mailPiece.DocumentOrPackage.Name = mailPiece.DocumentOrPackage.Name.IsNullOrWhiteSpace() ? "{0}.pdf".Fill(mailPiece.BusinessFriendlyIdValue.GetValidFileName()) : mailPiece.DocumentOrPackage.Name;

               
                var response = await CreateAMBatch(mailBatch, mailPiece, sFTChannel, receivedOn);
                sw.Stop();
                perf.Add(sw.Elapsed.TotalSeconds);
                log.DebugFormat("Batch created: {0}", sw.Elapsed.TotalSeconds);
                sw.Restart();
                var responseContent = await response.Content.ReadAsStringAsync();
                var mailResponse = JsonConvert.DeserializeObject<MailAPIResponse>(responseContent);
                // if get success response then delete files. 
                if (response.IsSuccessStatusCode)
                {
                    DeleteFiles(downloadedFiles);
                    sw.Stop();
                    perf.Add(sw.Elapsed.TotalSeconds);
                    log.DebugFormat("Delete files: {0}", sw.Elapsed.TotalSeconds);
                    log.DebugFormat("Perf {0} {1} {2}", mailPiece.Id, ",", String.Join(",", perf));
                    //sw.Restart();
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

        private IEnumerable<MailPieceValidationResult> GetAllMailPieceBasicErrors(MailPiece mailPiece,Client client)
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
                if (mailPiece.RecipientInfo.Address != null)
                {
                    if (mailPiece.RecipientInfo.Address.Street.IsNullOrWhiteSpace() || mailPiece.RecipientInfo.Address.City.IsNullOrWhiteSpace() || mailPiece.RecipientInfo.Address.State.IsNullOrWhiteSpace() || mailPiece.RecipientInfo.Address.Zip.IsNullOrWhiteSpace())
                        yield return new MailPieceValidationResult() { Code = OutComeCodes.Mailing_Address_Missing_Or_Invalid, ErrorMessage = "Recipient address is mandatory." };
                }
                else
                {
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Mailing_Address_Missing_Or_Invalid, ErrorMessage = "Recipient Address is mandatory." };
                }

                if (mailPiece.RecipientInfo.Recipient.FirstName.IsNullOrWhiteSpace() || mailPiece.RecipientInfo.Recipient.LastName.IsNullOrWhiteSpace())
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Recipient_Details_Missing_Or_Invalid, ErrorMessage = "Recipient first and last name are mandatory." };
            }

            if (mailPiece.ReturnInfo != null)
            {
                if (mailPiece.ReturnInfo.Address != null)
                {
                    if (mailPiece.ReturnInfo.Address.Street.IsNullOrWhiteSpace() || mailPiece.ReturnInfo.Address.City.IsNullOrWhiteSpace() || mailPiece.ReturnInfo.Address.State.IsNullOrWhiteSpace() || mailPiece.ReturnInfo.Address.Zip.IsNullOrWhiteSpace())
                        yield return new MailPieceValidationResult() { Code = OutComeCodes.Mailing_Address_Missing_Or_Invalid, ErrorMessage = "Return recipient address is mandatory." };
                }
                else
                {
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Mailing_Address_Missing_Or_Invalid, ErrorMessage = "Return Address is mandatory." };
                }

                if (mailPiece.ReturnInfo.Recipient != null)
                {
                    if (mailPiece.ReturnInfo.Recipient.FirstName.IsNullOrWhiteSpace() || mailPiece.ReturnInfo.Recipient.LastName.IsNullOrWhiteSpace())
                        yield return new MailPieceValidationResult() { Code = OutComeCodes.Recipient_Details_Missing_Or_Invalid, ErrorMessage = "Return recipient first and last name are mandatory." };
                }
                else
                {
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Recipient_Details_Missing_Or_Invalid, ErrorMessage = "Return recipient is mandatory." };
                }

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

            bool allowInsertsOnly;
            using (IDocTypeService docSvc = new DocTypeService())
            {
                allowInsertsOnly = docSvc.AllowInsertsOnlyDocumentType(client.Id, mailPiece.DocumentOrPackageType);
            }

            // TODO : check DocumentOrPackage is not empty and DocumentOrPackage.Files property is not empty
            if (mailPiece.DocumentOrPackage == null)
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Missing_DocumentOrPackage, ErrorMessage = "DocumentOrPackage is mandatory." };
            else if (mailPiece.DocumentOrPackage.Files == null && !allowInsertsOnly)
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Missing_Files, ErrorMessage = "Files property is mandatory." };

            yield break;
        }

        private MailPieceValidationResult ValidateClientSpecificRules(MailPiece mailPiece,Client client)
        {
            var errors = GetAllMailPieceBasicErrors(mailPiece,client).ToList();

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
