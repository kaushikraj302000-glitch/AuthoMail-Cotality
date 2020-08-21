using AM.Entities;
using AM.Lib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AM.Services
{
    public interface IGeneralService : IDisposable
    {
        StringBuilder GeneralErrorReport(List<MailPieces> mailPieces, string separator);
        StringBuilder GeneralDeliveryReport(List<MailPieces> mailPieces, string separator);
        string GeneralXMLReport(List<MailPieces> mailPieces, MailBatches mailBatch);
        string GeneralXMLReport(List<MailPieces> mailPieces, MailBatchDTO mailBatch, string clientCode);
        MailAPIResponse GenerateMailAPIResponse(string mpUniqueId);
        string GeneralJsonReport(string mpUniqueId);
        Task<HttpResponseMessage> SendEllieMaeResponse(List<MailAPIResponse> responseReport, string bearerToken);
    }

    public class GeneralService : ServiceBase, IGeneralService
    {
        public StringBuilder GeneralErrorReport(List<MailPieces> mailPieces, string separator)
        {
            if (separator == "\\t")
                separator = "\t";

            var columnName = String.Join(@separator, "BatchID", "ID", "Package Name", "Loan Number", "File Name", "Date Received", "First Name", "Last Name", "Recipient Street Address", "Recipient City", "Recipient State", "Recipient Zip", "Error Code", "Error Text", "Shipping Method");

            StringBuilder errorReport = new StringBuilder(columnName);

            errorReport.AppendLine();

            var erroredData = mailPieces.Where(l => l.ShippingStatus == ShippingStatus.Error.ToString()).ToList();

            if (erroredData == null || erroredData.Count == 0)
            {
                return null;
            }

            using (DocTypeService docTypeSvc = new DocTypeService())
            {
                // All the loans where outcome code is anything except no_error are processed invalid loans.
                foreach (MailPieces mailPiece in erroredData)
                {
                    DocType docType = mailPiece.DocTypeId.HasValue ? docTypeSvc.GetDocTypeById(mailPiece.DocTypeId.Value, mailPiece.PnSConfigDetailId.Value) : null;

                    var line = String.Join(@separator,
                       mailPiece.MailBatches.BatchName == null ? string.Empty : mailPiece.MailBatches.BatchName,
                        mailPiece.ErrorCode == OutComeCodes.Invalid_DuplicateKey ? Guid.Parse(mailPiece.ErrorDesc.Substring(0, mailPiece.ErrorDesc.IndexOf(" ")).Trim()) : mailPiece.UniqueId,
                            mailPiece.DocumentOrPackageType == null ? string.Empty : mailPiece.DocumentOrPackageType,
                            mailPiece.BusinessFriendlyIdValue == null ? string.Empty : mailPiece.BusinessFriendlyIdValue,
                            mailPiece.FileName == null ? string.Empty : mailPiece.FileName,
                            mailPiece.MailBatches.ReceivedOn == null ? string.Empty : mailPiece.MailBatches.ReceivedOn.Value.ToString(Config.Monitor_AMC_ReportDateFormat),
                            mailPiece.RcptFirstName == null ? string.Empty : mailPiece.RcptFirstName,
                            mailPiece.RcptLastName == null ? string.Empty : mailPiece.RcptLastName,
                            mailPiece.RcptAddressStreet == null ? string.Empty : string.IsNullOrEmpty(mailPiece.RcptAddressAptOrSuiteOrOther) ? mailPiece.RcptAddressStreet : mailPiece.RcptAddressStreet + " " + mailPiece.RcptAddressAptOrSuiteOrOther,
                            mailPiece.RcptAddressCity == null ? string.Empty : mailPiece.RcptAddressCity,
                            mailPiece.RcptAddressState == null ? string.Empty : mailPiece.RcptAddressState,
                            mailPiece.RcptAddressZIP == null ? string.Empty : mailPiece.RcptAddressZIP,
                            OutComeCodes.GetAMCMappingCode(mailPiece.ErrorCode),
                            mailPiece.ErrorDesc == null ? string.Empty : mailPiece.ErrorDesc.Replace("\r\n", " "),
                            docType == null ? string.Empty : docType.ShippingCarrier
                        );

                    errorReport.AppendLine(line);
                }
            }

            return errorReport;
        }

        public StringBuilder GeneralDeliveryReport(List<MailPieces> mailPieces, string separator)
        {
            if (separator == "\\t")
                separator = "\t";
            var columnName = String.Join(separator, "BatchID", "ID", "Package Name", "Loan Number", "File Name", "Date Received", "Date Printed", "Date Mailed", "Tracking Number", "Shipping Method", "First Name", "Last Name", "Recipient Street Address", "Recipient City", "Recipient State", "Recipient Zip", "Pages Shipped");

            StringBuilder deliveryReport = new StringBuilder(columnName);

            deliveryReport.AppendLine();

            var shippedData = mailPieces.Where(l => l.ShippingStatus == ShippingStatus.Shipped.ToString() || l.ShippingStatus == ShippingStatus.eDelivered.ToString() || l.ShippingStatus == ShippingStatus.PulledBack.ToString()).ToList();

            if (shippedData == null || shippedData.Count == 0)
            {
                return null;
            }

            using (DocTypeService docTypeSvc = new DocTypeService())
            {
                // All the loans where outcome code is anything except no_error are processed invalid loans.
                foreach (MailPieces mailPiece in shippedData)
                {
                    DocType docType = mailPiece.DocTypeId.HasValue ? docTypeSvc.GetDocTypeById(mailPiece.DocTypeId.Value, mailPiece.PnSConfigDetailId.Value) : null;


                    var line = String.Join(@separator,
                        mailPiece.MailBatches.BatchName == null ? string.Empty : mailPiece.MailBatches.BatchName,
                            mailPiece.UniqueId,
                            mailPiece.DocumentOrPackageType == null ? string.Empty : mailPiece.DocumentOrPackageType,
                            mailPiece.BusinessFriendlyIdValue == null ? string.Empty : mailPiece.BusinessFriendlyIdValue,
                            mailPiece.FileName == null ? string.Empty : mailPiece.FileName,
                            mailPiece.MailBatches.ReceivedOn == null ? string.Empty : mailPiece.MailBatches.ReceivedOn.Value.ToString(Config.Monitor_AMC_ReportDateFormat),
                            mailPiece.ShippingStatus == ShippingStatus.PulledBack.ToString() ? Config.Monitor_AMCPulledBackDate : mailPiece.PrintedOn.Value.ToString(Config.Monitor_AMC_ReportDateFormat),
                            mailPiece.ShippingStatus == ShippingStatus.PulledBack.ToString() ? Config.Monitor_AMCPulledBackDate : mailPiece.ShippedOn.Value.ToString(Config.Monitor_AMC_ReportDateFormat),
                            mailPiece.ShippingStatus == ShippingStatus.PulledBack.ToString() ? string.Empty : mailPiece.TrackingNo,
                            docType == null ? string.Empty : docType.ShippingCarrier,
                            mailPiece.RcptFirstName == null ? string.Empty : mailPiece.RcptFirstName,
                            mailPiece.RcptLastName == null ? string.Empty : mailPiece.RcptLastName,
                            mailPiece.RcptAddressStreet == null ? string.Empty : string.IsNullOrEmpty(mailPiece.RcptAddressAptOrSuiteOrOther) ? mailPiece.RcptAddressStreet : mailPiece.RcptAddressStreet + " " + mailPiece.RcptAddressAptOrSuiteOrOther,
                            mailPiece.RcptAddressCity == null ? string.Empty : mailPiece.RcptAddressCity,
                            mailPiece.RcptAddressState == null ? string.Empty : mailPiece.RcptAddressState,
                            mailPiece.RcptAddressZIP == null ? string.Empty : mailPiece.RcptAddressZIP,
                            mailPiece.ShippingStatus == ShippingStatus.PulledBack.ToString() ? 0 : mailPiece.TotalPages);

                    deliveryReport.AppendLine(line);
                }
            }
            return deliveryReport;
        }

        public string GeneralXMLReport(List<MailPieces> mailPieces, MailBatches mailBatch)
        {
            MailBatchResponse mbResponse = new MailBatchResponse()
            {
                Id = mailBatch.UniqueId,
                ClientBatchId = mailBatch.ClientBatchId,
                ClientCode = mailBatch.Clients.AppCode,
            };

            foreach (MailPieces mp in mailPieces)
            {
                mbResponse.MailPiecesReport.Add(new MailPieceReport() { BusinessFriendlyIdName = mp.BusinessFriendlyIdName, BusinessFriendlyIdValue = mp.BusinessFriendlyIdValue, DocumentOrPackageType = mp.DocumentOrPackageType, ErrorCode = mp.ErrorCode, ErrorDesc = mp.ErrorDesc, FileName = mp.FileName, Id = mp.UniqueId, Postage = mp.ShippingCost, MailingStatus = mp.ShippingStatus.ParseEnum<MailingStatus>(), TrackingNumber = mp.TrackingNo, PulledBackOn = mp.PulledBackOn.HasValue ? mp.PulledBackOn.Value : DateTime.MinValue, ShippedOn = mp.ShippedOn.HasValue ? mp.ShippedOn.Value : DateTime.MinValue, TotalSheets = mp.TotalPages, WeightMeasured = mp.Weight.HasValue ? mp.Weight.Value : 0 });
            }

            var serializedString = XmlUtil.Serialize<MailBatchResponse>(mbResponse);

            return serializedString;
        }

        public string GeneralXMLReport(List<MailPieces> mailPieces, MailBatchDTO mailBatch, string clientCode)
        {
            MailBatchResponse mbResponse = new MailBatchResponse()
            {
                Id = mailBatch.UniqueId,
                ClientBatchId = mailBatch.ClientBatchId,
                ClientCode = clientCode,
            };

            foreach (MailPieces mp in mailPieces)
            {
                mbResponse.MailPiecesReport.Add(new MailPieceReport() { BusinessFriendlyIdName = mp.BusinessFriendlyIdName, BusinessFriendlyIdValue = mp.BusinessFriendlyIdValue, DocumentOrPackageType = mp.DocumentOrPackageType, ErrorCode = mp.ErrorCode, ErrorDesc = mp.ErrorDesc, FileName = mp.FileName, Id = mp.UniqueId, Postage = mp.ShippingCost, MailingStatus = mp.ShippingStatus.ParseEnum<MailingStatus>(), TrackingNumber = mp.TrackingNo, PulledBackOn = mp.PulledBackOn.HasValue ? mp.PulledBackOn.Value : DateTime.MinValue, ShippedOn = mp.ShippedOn.HasValue ? mp.ShippedOn.Value : DateTime.MinValue, TotalSheets = mp.TotalPages, WeightMeasured = mp.Weight.HasValue ? mp.Weight.Value : 0 });
            }

            var serializedString = XmlUtil.Serialize<MailBatchResponse>(mbResponse);

            return serializedString;
        }

        public MailAPIResponse GenerateMailAPIResponse(string mpUniqueId)
        {
            using (IMailPieceService mailPieceService = new MailPieceService())
            {
                BatchResponse batchResponse = mailPieceService.GetMailPieceDetailsByMailPieceUniqueId(mpUniqueId);

                if (batchResponse != null)
                {
                    MailAPIResponse response = new MailAPIResponse()
                    {
                        Id = batchResponse.UniqueId,
                        ReceivedOn = batchResponse.ReceivedOn,
                        ClientCode = batchResponse.AppCode,
                        BusinessFriendlyIdName = batchResponse.BusinessFriendlyIdName,
                        BusinessFriendlyIdValue = batchResponse.BusinessFriendlyIdValue,
                        DocumentOrPackageType = batchResponse.DocumentOrPackageType,
                        DocumentOrPackage = new DocumentOrPackage { Name = batchResponse.FileName, Inserts = batchResponse.Inserts },
                        RecipientInfo = new RecipientInfo
                        {
                            Recipient = new Recipient
                            {
                                Organization = batchResponse.RcptOrganization,
                                Prefix = batchResponse.RcptPrefix,
                                Suffix = batchResponse.RcptSuffix,
                                FirstName = batchResponse.RcptFirstName,
                                MiddleName = batchResponse.RcptMiddleName,
                                LastName = batchResponse.RcptLastName,
                                Phone = batchResponse.RcptPhone,
                                EmailAddress = batchResponse.RcptEmailId
                            },
                            Address = new Entities.Address
                            {
                                OptionalLine1AboveStreet = batchResponse.RcptAddrOptionalLine1AboveStreet,
                                OptionalLine1BelowStreet = batchResponse.RcptAddrOptionalLine1BelowStreet,
                                OptionalLine2AboveStreet = batchResponse.RcptAddrOptionalLine2AboveStreet,
                                OptionalLine2BelowStreet = batchResponse.RcptAddrOptionalLine2BelowStreet,
                                AptOrSuiteOrOther = batchResponse.RcptAddressAptOrSuiteOrOther,
                                Street = batchResponse.RcptAddressStreet,
                                City = batchResponse.RcptAddressCity,
                                State = batchResponse.RcptAddressState,
                                Zip = batchResponse.RcptAddressZIP,
                                Country = batchResponse.RcptAddressCountry
                            }
                        },
                        MailingStatus = batchResponse.ShippingStatus,
                        ErrorCode = batchResponse.ErrorCode,
                        ErrorDesc = batchResponse.ErrorDesc,
                        ShippedOn = batchResponse.ShippedOn,
                        ShippingMethod = batchResponse.ShippingCarrier,
                        ShippingServiceType = batchResponse.ShippingServiceType,
                        TrackingNumber = batchResponse.TrackingNo,
                        TotalPages = batchResponse.TotalPages,
                        TotalSheets = batchResponse.TotalSheets,
                        PulledBackOn = batchResponse.PulledBackOn
                    };

                    return response;
                }
                else
                {
                    MailAPIResponse response = new MailAPIResponse()
                    {
                        Id = Guid.Parse(mpUniqueId),
                        MailingStatus = ShippingStatus.NotFound.ToString(),
                        Message = "The specified package does not exist."
                    };

                    return response;
                }
            }
        }

        public string GeneralJsonReport(string mpUniqueId)
        {
            var response = GenerateMailAPIResponse(mpUniqueId);
            var serializedString = JsonConvert.SerializeObject(response);
            return serializedString;
        }

        public async Task<HttpResponseMessage> SendEllieMaeResponse(List<MailAPIResponse> responseReport, string bearerToken)
        {
            JsonSerializerSettings dateFormatSettings = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat
            };

            string json = JsonConvert.SerializeObject(responseReport, dateFormatSettings);
            using (var client = SetupClient(bearerToken))
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(Config.EllieMae_UploadResponseURL, content);
                return response;
            }
        }

        private static HttpClient SetupClient(string token)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(Config.EllieMae_BaseURL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            return client;
        }
        public async static Task<string> GetToken()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    //setup client
                    client.BaseAddress = new Uri("http://localhost:17005/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    //setup login data
                    var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("username", "admin@milestonetechnoconsulting.com"),
                    new KeyValuePair<string, string>("password", "Admin@123")
                });

                    //send request
                    HttpResponseMessage responseMessage = await client.PostAsync("/token", formContent);

                    //get access token from response body
                    var responseJson = await responseMessage.Content.ReadAsStringAsync();
                    var jObject = JObject.Parse(responseJson);
                    return jObject.GetValue("access_token").ToString();
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}
