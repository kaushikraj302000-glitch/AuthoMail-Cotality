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

        [Route("mailrequest")]
        [HttpPost]
        public async Task<MailAPIResponse> CreateMailRequest(MailPiece mailPiece)
        {
            if (mailPiece == null) throw new HttpResponseException(HttpStatusCode.BadRequest);
            DateTime receivedTime = Common.Now;
            SFTChannel sFTChannel;
            Client client;
            MailBatch mailBatch = new MailBatch()
            {
                Id = Guid.NewGuid()
            };
            MailPiece mp = mailPiece;


            using (ICommonService commonSvc = new CommonService())
            {
                if (mp.Id != Guid.Empty && commonSvc.CheckForDuplicateMailPieceUniqueId(mailPiece.Id))
                {
                    var result = new MailPieceValidationResult() { Code = OutComeCodes.Invalid_DuplicateKey, ErrorMessage = string.Format("The specified package id '{0}' conflicts with an existing package. Retry with a new GUID.", mailPiece.Id) };
                    if (result.Code != OutComeCodes.No_Error)
                    {
                        return new MailAPIResponse { Id = mp.Id, ClientCode = mp.ClientCode, BusinessFriendlyIdName = mp.BusinessFriendlyIdName, BusinessFriendlyIdValue = mp.BusinessFriendlyIdValue, DocumentOrPackage = mp.DocumentOrPackage, RecipientInfo = mp.RecipientInfo, MailingStatus = ShippingStatus.Error.ToString(), ErrorCode = result.Code, ErrorDesc = result.ErrorMessage, ReceivedOn = receivedTime };
                    }
                }
            }

            using (IClientService clientService = new ClientService())
            {
                sFTChannel = await clientService.GetSFTChannelDetailsBySFTChannelId(User.Identity.SFTChannelId().Value);
            }
            try
            {
                mp.Id = mp.Id == Guid.Empty ? Guid.NewGuid() : mp.Id;
                mailBatch.MailPieces.Add(mp);

                using (IClientService clientService = new ClientService())
                {
                    client = await clientService.GetClientByAppCode(mp.ClientCode);
                    if (client == null)
                    {
                        //Return error response and insert entry of invalid_client
                        // sagar's common method call                        
                        var invalidClient = await clientService.GetClientByAppCode(sFTChannel.Code.Split('_')[0] + "_Invalid");
                        await InsertInvalidBatch(receivedTime, sFTChannel, mailBatch, invalidClient.Id, "Invalid client code specified.");
                        mp.DocumentOrPackage.IncludePatternBaseString = false;
                        return new MailAPIResponse { Id = mp.Id, ClientCode = mp.ClientCode, BusinessFriendlyIdName = mp.BusinessFriendlyIdName, BusinessFriendlyIdValue = mp.BusinessFriendlyIdValue, DocumentOrPackage = mp.DocumentOrPackage, RecipientInfo = mp.RecipientInfo, MailingStatus = ShippingStatus.Error.ToString(), ErrorCode = "InvalidClientCode", ErrorDesc = "Invalid client code specified.", ReceivedOn = receivedTime };
                    }
                    else
                    {
                        mailBatch.ClientCode = client.AppCode;
                        mailBatch.ClientBatchId = mailBatch.Id.ToString();
                    }
                }

                switch (sFTChannel.Code.Split('_')[0].ToUpper())
                {
                    case "ELLIEMAE":
                    default:
                        var result = ValidateClientSpecificRules(mp);
                        if (result.Code == OutComeCodes.No_Error)
                        {
                            mp.DocumentOrPackage.IncludePatternBaseString = true;
                            //create zip and response
                            string zipFilePath = Path.Combine(sFTChannel.Host + sFTChannel.ToADEDir, string.Format("{0}.zip", mailBatch.Id));
                            string triggerFilePath = Path.Combine(sFTChannel.Host + sFTChannel.ToADEDir, string.Format("{0}.GO", mailBatch.Id));
                            string xmlFilePath = Path.Combine(sFTChannel.Host + sFTChannel.ToADEDir, string.Format("{0}.xml", mailBatch.Id));
                            string pdfFilePath = Path.Combine(sFTChannel.Host + Path.Combine(sFTChannel.ToADEDir, string.Format("{0}.pdf", mp.DocumentOrPackage.Name == null ? mp.Id.ToString() : Path.GetFileNameWithoutExtension(mp.DocumentOrPackage.Name))));

                            List<string> filesToZip = new List<string>();

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
                                    ZipArchiveEntry pdfEntry = archive.CreateEntry(string.Format("{0}.pdf", mp.DocumentOrPackage.Name == null ? mp.Id.ToString() : Path.GetFileNameWithoutExtension(mp.DocumentOrPackage.Name)));
                                    using (var pdfStream = pdfEntry.Open())
                                    {
                                        var payloadStream = new MemoryStream(Convert.FromBase64String(mp.DocumentOrPackage.Base64String));
                                        await payloadStream.CopyToAsync(pdfStream);
                                    }
                                }
                            }
                         
                            if (!File.Exists(triggerFilePath))
                            {
                                using (FileStream sourceStream = new FileStream(triggerFilePath,
        FileMode.Create, FileAccess.Write, FileShare.None,
        bufferSize: 4096, useAsync: true)) { }

                                File.SetLastWriteTime(triggerFilePath, receivedTime);
                            }

                            return new MailAPIResponse
                            {
                                UniqueId = mp.Id,
                                MailingStatus = ShippingStatus.Acknowledgement.ToString(),
                                Message = "Your request is queued for processing."
                            };
                        }
                        else
                        {
                            //insert error and response
                            using (IMailBatchService mailBatchService = new MailBatchService())
                            using (ICommonService commonSvc = new CommonService())
                            using (IMailPieceService mailPieceService = new MailPieceService())
                            {

                                int mailBatchId = await mailBatchService.InsertBatchDetails(mailBatch, client.Id, sFTChannel);
                                // insert mailpiece
                                int mailPieceId = await mailPieceService.InsertBatchMailPieceDetails(mp, client.Id, mailBatchId, result.Code, result.ErrorMessage);
                                await commonSvc.UpdateBatchPipeLineStatus(mailBatchId, BatchPipelineStatus.Received, receivedTime);
                                await mailBatchService.MarkMailBatchAsInvalid(mailBatchId, "Request does not contain valid package info.");
                                await commonSvc.UpdateBatchStatus(mailBatchId, BatchStatus.Failed.ToString());
                                await commonSvc.UpdateBatchPipeLineStatus(mailBatchId, BatchPipelineStatus.ReportPrepped, Common.Now);
                                await commonSvc.UpdateBatchPipeLineStatus(mailBatchId, BatchPipelineStatus.ReportedBack, Common.Now);
                            }
                            mp.DocumentOrPackage.IncludePatternBaseString = false;
                            return new MailAPIResponse { Id = mp.Id, ClientCode = mp.ClientCode, BusinessFriendlyIdName = mp.BusinessFriendlyIdName, BusinessFriendlyIdValue = mp.BusinessFriendlyIdValue, DocumentOrPackage = mp.DocumentOrPackage, RecipientInfo = mp.RecipientInfo, MailingStatus = ShippingStatus.Error.ToString(), ErrorCode = result.Code, ErrorDesc = result.ErrorMessage, ReceivedOn = receivedTime };
                        }
                }

            }
            catch (Exception ex)
            {
                string msg = string.Format("Unexpected error occured while processing your request.");
                log.Error(msg, ex);
                return new MailAPIResponse { MailingStatus = ShippingStatus.Error.ToString(), ErrorCode = OutComeCodes.Unknown_Error, ErrorDesc = msg, ReceivedOn = receivedTime };
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
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Invalid_ShippingDetail, ErrorMessage = "Business friendly Id or value is not specified in package." };

            if (mailPiece.DocumentOrPackage != null)
            {
                if (mailPiece.DocumentOrPackage.Name.IsNullOrWhiteSpace())
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Missing_PDF_File, ErrorMessage = "Pdf name  is mandatory." };
                if (mailPiece.DocumentOrPackage.Base64String.IsNullOrWhiteSpace())
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Missing_PDF_File, ErrorMessage = "Pdf file  is mandatory." };
            }

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
