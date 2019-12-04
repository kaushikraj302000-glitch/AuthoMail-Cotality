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
        public MailAPIResponse CreateMailRequest(MailPiece mailPiece)
        {
            DateTime receivedTime = Common.Now;
            SuccessResponse successResponse = new SuccessResponse();
            SFTChannel sFTChannel;
            Client client;
            MailBatch mailBatch = new MailBatch()
            {
                Id = Guid.NewGuid()
            };
            MailPiece mp = mailPiece;
            using (IClientService clientService = new ClientService())
            {
                sFTChannel = clientService.GetSFTChannelDetailsBySFTChannelId(User.Identity.SFTChannelId().Value);
            }
            try
            {
                mp.Id = mp.Id == Guid.Empty ? Guid.NewGuid() : mp.Id;
                mailBatch.MailPieces.Add(mp);

                using (IClientService clientService = new ClientService())
                {
                    client = clientService.GetClientByAppCode(mp.ClientCode);
                    if (client == null)
                    {
                        //Return error response and insert entry of invalid_client
                        // sagar's common method call                        
                        int clientId = clientService.GetClientByAppCode(sFTChannel.Code.Split('_')[0] + "Invalid").Id;
                        InsertInvalidBatch(receivedTime, sFTChannel, mailBatch, clientId, "Invalid client code specified.");
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
                            File.WriteAllText(xmlFilePath, xml);

                            //Check first DocumentOrPackage is not null
                            if (mp.DocumentOrPackage != null && mp.DocumentOrPackage.Base64String != null)
                            {
                                File.WriteAllBytes(pdfFilePath, Convert.FromBase64String(mp.DocumentOrPackage.Base64String));
                            }

                            filesToZip.Add(xmlFilePath);
                            filesToZip.Add(pdfFilePath);

                            using (ZipArchive newFile = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                            {
                                foreach (var file in filesToZip)
                                {
                                    newFile.CreateEntryFromFile(file, Path.GetFileName(file));
                                }
                            }

                            if (!File.Exists(triggerFilePath))
                            {
                                using (var file = File.Create(triggerFilePath))
                                {
                                    file.Close();
                                }
                                File.SetLastWriteTime(triggerFilePath, receivedTime);
                            }

                            //delete files
                            foreach (var fileToDelete in filesToZip)
                            {
                                if (File.Exists(fileToDelete))
                                {
                                    File.Delete(fileToDelete);
                                }
                            }

                            return new MailAPIResponse
                            {
                                UniqueId = mp.Id,
                                MailingStatus = ShippingStatus.Ack.ToString(),
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

                                int mailBatchId = mailBatchService.InsertBatchDetails(mailBatch, client.Id, sFTChannel);
                                // insert mailpiece
                                int mailPieceId = mailPieceService.InsertBatchMailPieceDetails(mp, client.Id, mailBatchId, result.Code, result.ErrorMessage);
                                commonSvc.UpdateBatchPipeLineStatus(mailBatchId, BatchPipelineStatus.Received, receivedTime);
                                mailBatchService.MarkMailBatchAsInvalid(mailBatchId, "Request does not contain valid package info.");
                                commonSvc.UpdateBatchStatus(mailBatchId, BatchStatus.Failed.ToString());
                                commonSvc.UpdateBatchPipeLineStatus(mailBatchId, BatchPipelineStatus.ReportPrepped, Common.Now);
                                commonSvc.UpdateBatchPipeLineStatus(mailBatchId, BatchPipelineStatus.ReportedBack, Common.Now);
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

        private static void InsertInvalidBatch(DateTime receivedTime, SFTChannel sFTChannel, MailBatch mailBatch, int clientId, string errorMessage)
        {
            using (IMailBatchService mailBatchService = new MailBatchService())
            using (ICommonService commonSvc = new CommonService())
            using (IMailPieceService mailPieceService = new MailPieceService())
            {
                int mailBatchId = mailBatchService.InsertBatchDetails(mailBatch, clientId, sFTChannel);
                commonSvc.UpdateBatchPipeLineStatus(mailBatchId, BatchPipelineStatus.Received, receivedTime);
                mailBatchService.MarkMailBatchAsInvalid(mailBatchId, errorMessage);
                commonSvc.UpdateBatchStatus(mailBatchId, BatchStatus.Failed.ToString());
                commonSvc.UpdateBatchPipeLineStatus(mailBatchId, BatchPipelineStatus.ReportPrepped, Common.Now);
                commonSvc.UpdateBatchPipeLineStatus(mailBatchId, BatchPipelineStatus.ReportedBack, Common.Now);
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
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Invalid_DuplicateKey, ErrorMessage = string.Format("Request cannot be accepted because provided unique id {0} is already exist in the system.", mailPiece.Id) };
                }
            }

            if (mailPiece.RecipientInfo != null)
            {
                if (mailPiece.RecipientInfo.Address.Street.IsNullOrWhiteSpace() || mailPiece.RecipientInfo.Address.City.IsNullOrWhiteSpace() || mailPiece.RecipientInfo.Address.State.IsNullOrWhiteSpace() || mailPiece.RecipientInfo.Address.Zip.IsNullOrWhiteSpace())
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Mailing_Address_Missing_Or_Invalid, ErrorMessage = "Recipient address details are missing or invalid in package." };

                if (mailPiece.RecipientInfo.Recipient.FirstName.IsNullOrWhiteSpace() || mailPiece.RecipientInfo.Recipient.LastName.IsNullOrWhiteSpace())
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Recipient_Details_Missing_Or_Invalid, ErrorMessage = "Recipient's first and last name are missing or invalid in package." };
            }

            if (mailPiece.ReturnInfo != null)
            {
                if (mailPiece.ReturnInfo.Address.Street.IsNullOrWhiteSpace() || mailPiece.ReturnInfo.Address.City.IsNullOrWhiteSpace() || mailPiece.ReturnInfo.Address.State.IsNullOrWhiteSpace() || mailPiece.ReturnInfo.Address.Zip.IsNullOrWhiteSpace())
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Mailing_Address_Missing_Or_Invalid, ErrorMessage = "Return recipient address details are missing or invalid in package." };

                if (mailPiece.ReturnInfo.Recipient.FirstName.IsNullOrWhiteSpace() || mailPiece.ReturnInfo.Recipient.LastName.IsNullOrWhiteSpace())
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Recipient_Details_Missing_Or_Invalid, ErrorMessage = "Return recipient's first and last name are missing or invalid in package." };
            }

            if (mailPiece.DocumentOrPackageType.IsNullOrWhiteSpace())
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Invalid_PackageType, ErrorMessage = "Document type is not specified in package." };

            if (mailPiece.eDelivery != null)
            {
                if (mailPiece.RecipientInfo.Recipient.EmailAddress.IsNullOrWhiteSpace())
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Recipient_Details_Missing_Or_Invalid, ErrorMessage = "Email address is not specified." };
                if (mailPiece.eDelivery.ExpiryDate < Common.Today.Date)
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Issue_In_EDelivery, ErrorMessage = "The expiration date must be greater than or equal to the request received date." };
            }

            if (mailPiece.BusinessFriendlyIdName.IsNullOrWhiteSpace() || mailPiece.BusinessFriendlyIdValue.IsNullOrWhiteSpace())
                yield return new MailPieceValidationResult() { Code = OutComeCodes.Invalid_ShippingDetail, ErrorMessage = "Business friendly Id or value is not specified in package." };

            if (mailPiece.DocumentOrPackage != null)
            {
                if (mailPiece.DocumentOrPackage.Name.IsNullOrWhiteSpace() || mailPiece.DocumentOrPackage.Base64String.IsNullOrWhiteSpace())
                    yield return new MailPieceValidationResult() { Code = OutComeCodes.Missing_PDF_File, ErrorMessage = "Pdf file is not provided in package." };
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
