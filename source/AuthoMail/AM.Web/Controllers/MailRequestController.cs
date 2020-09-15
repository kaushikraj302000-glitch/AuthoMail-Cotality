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
using System.Xml.Schema;
using System.Xml;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;
using System.Globalization;
using iTextSharp.text.exceptions;

namespace AM.Web.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [Authorize(Roles = AMRoles.Admin + "," + AMRoles.API)]
    [RoutePrefix("api")]
    public class MailRequestController : ApiController
    {
        private static XmlSchemaSet schemas = new XmlSchemaSet();

        static MailRequestController()
        {
            schemas.Add(null, XmlReader.Create(new StringReader(Entities.Properties.Resources.MailingRequest)));
        }

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

                string xml = XmlUtil.Serialize(mailBatch);

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



                // Explictly checked to return appropriate HTTP error code  : 409
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
                        var result = ValidateClientSpecificRules(mailBatch, client);
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
                        var res = GetErrorResponse(mailPiece, new MailPieceValidationResult { Code = OutComeCodes.Missing_PDF_File, ErrorMessage = "Pdf file not found for the following file key(s): {0}.".Fill(string.Join(",", nullFilePathList.Select(n => n.Key))) }, receivedOn);
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

                    if (mailPiece.CustomInfo == null) mailPiece.CustomInfo = new List<Metadata>();

                    string destPdf = Path.Combine(root, Path.GetRandomFileName());
                    destPdf = Path.ChangeExtension(destPdf, ".pdf");
                    using (var ds = new FileStream(destPdf, FileMode.Create))
                    using (var doc = new Document())
                    using (var copy = new PdfSmartCopy(doc, ds))
                    {
                        
                        // This is done to open and merge (Secured) PDFs which allow printing but not copying or merging
                        PdfReader.unethicalreading = true;
                        //Loop through each input file                        
                        foreach (var file in files)
                        {
                            try
                            {
                                #region decrypt pdf files
                                var decryptedFilePath = file.Value.LocalFileName;
                                if (client.IsEncrypted)
                                {
                                    try
                                    {
                                        decryptedFilePath = Path.Combine(root, string.Format("Literal_{0}.pdf", Path.GetFileNameWithoutExtension(file.Value.LocalFileName)));
                                        await CryptoHelper.DecryptFileAsync(file.Value.LocalFileName, Config.PGP_Passphrase, decryptedFilePath);
                                    }
                                    catch (Exception e)
                                    {
                                        log.Error(e.Message, e);
                                        // if there are any exceptions while decrypting pdf, don't delete files and send bad reqeust with mailresponse object, it should contain file names
                                        var res = GetErrorResponse(mailPiece, new MailPieceValidationResult { Code = OutComeCodes.Invalid_PDF_File, ErrorMessage = "Unable to decrypt file : {0}.".Fill(file.Key) }, receivedOn);
                                        return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                                    } 
                                }
                                #endregion

                                using (var reader = new PdfReader(decryptedFilePath))
                                {
                                    mailPiece.CustomInfo.Add(new Metadata { Key = file.Key, Value = "FileName:{0} | PageCount: {1}".Fill(file.Value.Headers.ContentDisposition.FileName.Replace("\"", ""), reader.NumberOfPages) });
                                    
                                    //open document
                                    if (!doc.IsOpen()) doc.Open();
                                    //Add the entire document
                                    copy.AddDocument(reader);
                                    reader.Close();
                                }
                                downloadedFiles.Add(decryptedFilePath);
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
                    sw.Restart();
                }
                else
                {
                    sw.Stop();
                    perf.Add(sw.Elapsed.TotalSeconds);
                    log.DebugFormat("Validation complete: {0}", sw.Elapsed.TotalSeconds);
                    sw.Restart();
                }
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
                    log.DebugFormat("Perf {0} , {1}", mailPiece.Id, String.Join(",", perf));
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
                catch (JsonSerializationException jsEx)
                {
                    manifestErrDesc = string.Format("Could not process manifest: Path '{0}', line {1}, position {2}.", jsEx.Path, jsEx.LineNumber, jsEx.LinePosition);
                }
                catch (Exception)
                {
                    manifestErrDesc = string.Format("Unexpected error. Could not process manifest");
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
                using (IGeneralService generalSvc = new GeneralService())
                {
                    string deliveryReport = generalSvc.GeneralJsonReport(mailPieceId);
                    mailAPIResponses.Add(JsonConvert.DeserializeObject<MailAPIResponse>(deliveryReport));
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
            return new MailAPIResponse { Id = mailPiece.Id, ClientCode = mailPiece.ClientCode, BusinessFriendlyIdName = mailPiece.BusinessFriendlyIdName, BusinessFriendlyIdValue = mailPiece.BusinessFriendlyIdValue, DocumentOrPackageType = mailPiece.DocumentOrPackageType, DocumentOrPackage = mailPiece.DocumentOrPackage, RecipientInfo = mailPiece.RecipientInfo, MailingStatus = ShippingStatus.Error.ToString(), ErrorCode = validationResult.Code, ErrorDesc = validationResult.ErrorMessage, ReceivedOn = receivedOn };
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

        private IEnumerable<MailPieceValidationResult> GetAllMailPieceBasicErrors(MailBatch mailBatch, Client client)
        {
            var mailPiece = mailBatch.MailPieces.First();

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

                    if (!mailPiece.RecipientInfo.Address.Zip.IsNullOrWhiteSpace() && (mailPiece.RecipientInfo.Address.Country.IsNullOrWhiteSpace() || (!mailPiece.RecipientInfo.Address.Country.IsNullOrWhiteSpace() && (mailPiece.RecipientInfo.Address.Country == "USA" || mailPiece.RecipientInfo.Address.Country == "US" || mailPiece.RecipientInfo.Address.Country == "The United States of America" || mailPiece.RecipientInfo.Address.Country == "United States of America" || mailPiece.RecipientInfo.Address.Country == "United States"))))
                    {
                        if (!Regex.Match(mailPiece.RecipientInfo.Address.Zip, @"^(?(^00000(|-0000))|\d{5}(?:|[-\s]+\d{4}))$").Success)
                        {
                            yield return new MailPieceValidationResult() { Code = OutComeCodes.Mailing_Address_Missing_Or_Invalid, ErrorMessage = "Recipient zipcode is in invalid format." };
                        }
                    }
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

                    if (!mailPiece.ReturnInfo.Address.Zip.IsNullOrWhiteSpace() && (mailPiece.ReturnInfo.Address.Country.IsNullOrWhiteSpace() || (!mailPiece.ReturnInfo.Address.Country.IsNullOrWhiteSpace() && (mailPiece.ReturnInfo.Address.Country == "USA" || mailPiece.ReturnInfo.Address.Country == "US" || mailPiece.ReturnInfo.Address.Country == "The United States of America" || mailPiece.ReturnInfo.Address.Country == "United States of America" || mailPiece.ReturnInfo.Address.Country == "United States"))))
                    {
                        if (!Regex.Match(mailPiece.ReturnInfo.Address.Zip, @"^(?(^00000(|-0000))|\d{5}(?:|[-\s]+\d{4}))$").Success)
                        {
                            yield return new MailPieceValidationResult() { Code = OutComeCodes.Mailing_Address_Missing_Or_Invalid, ErrorMessage = "Return recipient zipcode is in invalid format." };
                        }
                    }
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
                //TODO : Email address validation if not empty.
                if (mailPiece.RecipientInfo.Recipient.EmailAddress.IsNullOrWhiteSpace())
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Recipient_Details_Missing_Or_Invalid, ErrorMessage = "Email address is mandatory." };
                else
                {
                    if (!IsValidEmail(mailPiece.RecipientInfo.Recipient.EmailAddress))
                    {
                        yield return new MailPieceValidationResult() { Code = OutComeCodes.Recipient_Details_Missing_Or_Invalid, ErrorMessage = "Email address is not in a valid format." };
                    }
                }
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
            else if ((mailPiece.DocumentOrPackage.Files == null || mailPiece.DocumentOrPackage.Files.Count == 0) && !allowInsertsOnly && !mailPiece.DocumentOrPackageType.IsNullOrWhiteSpace())
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Missing_Files, ErrorMessage = "Files property must be populated for non-inserts only packages." };
            else if ((mailPiece.DocumentOrPackage.Files == null || mailPiece.DocumentOrPackage.Files.Count == 0) && mailPiece.DocumentOrPackage.Inserts.IsNullOrWhiteSpace())
            {
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Missing_Files, ErrorMessage = "Request cannot proceed further without Files or Inserts value." };
            }
            else if (!mailPiece.DocumentOrPackage.Inserts.IsNullOrWhiteSpace())
            {
                if (mailPiece.DocumentOrPackage.Inserts.Contains("|"))
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Missing_Files, ErrorMessage = "Inserts  is not in a valid format." };
                else
                {
                    var insertLst = mailPiece.DocumentOrPackage.Inserts.ToUpper().Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    if ((mailPiece.DocumentOrPackage.Files == null || mailPiece.DocumentOrPackage.Files.Count == 0) && insertLst.Count == 0)
                    {
                        yield return new MailPieceValidationResult() { Code = OutComeCodes.Missing_Files, ErrorMessage = "Request cannot proceed further without Files or Inserts value." };
                    }
                    else if (insertLst.Count != insertLst.Distinct().Count())
                    {
                        yield return new MailPieceValidationResult() { Code = OutComeCodes.Missing_Files, ErrorMessage = "Multiple occurrences of an insert value are not allowed." };
                    }
                    else
                    {
                        // check if insert is BRE                            
                        using (IMailPieceService mailPieceService = new MailPieceService())
                        {
                            var invalidInserts = mailPieceService.GetInvalidInserts(insertLst);
                            if (invalidInserts.Count > 0)
                            {
                                yield return new MailPieceValidationResult() { Code = OutComeCodes.Missing_Files, ErrorMessage = $"Following specified inserts are invalid or not BRE: {string.Join(",", invalidInserts)}" };
                            }
                        }
                    }
                }
            }

            if (mailPiece.CustomInfo != null)
            {
                if (mailPiece.CustomInfo.Count == 0)
                {
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Manifest_Missing_Or_Invalid, ErrorMessage = "Metadata is mandatory in custominfo." };
                }

                foreach (var customInfo in mailPiece.CustomInfo)
                {
                    if (customInfo.Key.IsNullOrWhiteSpace())
                    {
                        yield return new MailPieceValidationResult() { Code = OutComeCodes.Manifest_Missing_Or_Invalid, ErrorMessage = "CustomInfo's Key is mandatory." };
                    }
                }
            }

            yield break;
        }

        private MailPieceValidationResult ValidateClientSpecificRules(MailBatch mailBatch, Client client)
        {
            var errors = GetAllMailPieceBasicErrors(mailBatch, client).ToList();

            if (errors.Count > 0)
            {
                return new MailPieceValidationResult() { Code = errors.First().Code, ErrorMessage = string.Join(" | ", errors.Select(row => row.ErrorMessage)) };
            }
            else
            {
                // Schema validation
                var scehmaErrors = GetAllSchemaErrors(mailBatch, client).ToList();

                if (scehmaErrors.Count > 0)
                {
                    return new MailPieceValidationResult() { Code = scehmaErrors.First().Code, ErrorMessage = string.Join(" | ", scehmaErrors.Select(row => row.ErrorMessage)) };
                }
            }

            return new MailPieceValidationResult() { Code = OutComeCodes.No_Error };
        }

        private IEnumerable<MailPieceValidationResult> GetAllSchemaErrors(MailBatch mailBatch, Client client)
        {
            //TODO : Take Phone and USZIP validation with others. After validating all checks, call ValidateXml
            var xmlValidationResult = XmlUtil.ValidateXml(schemas, XmlUtil.Serialize<MailBatch>(mailBatch));

            foreach (var error in xmlValidationResult.ErrorList)
            {
                if (error.Contains("http://schemas.corelogic.com/ade/authomail/") && error.Contains("NAPhone"))
                {
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Recipient_Details_Missing_Or_Invalid, ErrorMessage = "Provided phone number is not in a valid format. Please provide it in one of the following formats : (XXX) XXX XXXX, XXXXXXXXXX, XXX-XXX-XXXX, XXX XXX XXXX" };
                }
                else if (error.Contains("http://schemas.corelogic.com/ade/authomail/") && error.Contains("USZIP"))
                {
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Recipient_Details_Missing_Or_Invalid, ErrorMessage = "Provided zip code is not in a valid format." };
                }
                else if (error.Contains("http://schemas.corelogic.com/ade/authomail/") && error.Contains("EmailAddress"))
                {
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Recipient_Details_Missing_Or_Invalid, ErrorMessage = "Email address is not in a valid format or blank." };
                }
                else
                {
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Manifest_Missing_Or_Invalid, ErrorMessage = error };
                }
            }
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    var domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        public class MailPieceValidationResult
        {
            public string Code { get; set; }
            public string ErrorMessage { get; set; }
        }
    }

}
