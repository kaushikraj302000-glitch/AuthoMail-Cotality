using AM.Entities;
using AM.Lib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
        string GeneralJsonReport(MailPieces mp, MailBatchDTO mailBatch, string clientCode);
        List<BatchAPIResponse> GeneralJsonAPIResponse(List<MailPieces> mailpieces, MailBatchDTO mailBatch, string clientCode);
        string GeneralJsonReport(MailPieces mp, MailBatches mailBatch);
        Task<string> SendEllieMaeResponse(List<MailAPIResponse> responseReport);
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
                mbResponse.MailPiecesReport.Add(new MailPieceReport() { BusinessFriendlyIdName = mp.BusinessFriendlyIdName, BusinessFriendlyIdValue = mp.BusinessFriendlyIdValue, DocumentOrPackageType = mp.DocumentOrPackageType, ErrorCode = mp.ErrorCode, ErrorDesc = mp.ErrorDesc, FileName = mp.FileName, Id = mp.UniqueId, Postage = mp.ShippingCost, MailingStatus = mp.ShippingStatus.ParseEnum<MailingStatus>(), PulledBackOn = mp.PulledBackOn.HasValue ? mp.PulledBackOn.Value : DateTime.MinValue, ShippedOn = mp.ShippedOn.HasValue ? mp.ShippedOn.Value : DateTime.MinValue, TotalSheets = mp.TotalPages, WeightMeasured = mp.Weight.HasValue ? mp.Weight.Value : 0 });
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
                mbResponse.MailPiecesReport.Add(new MailPieceReport() { BusinessFriendlyIdName = mp.BusinessFriendlyIdName, BusinessFriendlyIdValue = mp.BusinessFriendlyIdValue, DocumentOrPackageType = mp.DocumentOrPackageType, ErrorCode = mp.ErrorCode, ErrorDesc = mp.ErrorDesc, FileName = mp.FileName, Id = mp.UniqueId, Postage = mp.ShippingCost, MailingStatus = mp.ShippingStatus.ParseEnum<MailingStatus>(), PulledBackOn = mp.PulledBackOn.HasValue ? mp.PulledBackOn.Value : DateTime.MinValue, ShippedOn = mp.ShippedOn.HasValue ? mp.ShippedOn.Value : DateTime.MinValue, TotalSheets = mp.TotalPages, WeightMeasured = mp.Weight.HasValue ? mp.Weight.Value : 0 });
            }

            var serializedString = XmlUtil.Serialize<MailBatchResponse>(mbResponse);

            return serializedString;
        }

        public string GeneralJsonReport(MailPieces mp, MailBatchDTO mailBatch, string clientCode)
        {
            MailAPIResponse response = new MailAPIResponse()
            {
                BusinessFriendlyIdName = mp.BusinessFriendlyIdName,
                BusinessFriendlyIdValue = mp.BusinessFriendlyIdValue,
                ClientCode = clientCode,
                DocumentOrPackage = new DocumentOrPackage() { Name = mp.FileName },
                ReceivedOn = mailBatch.ReceivedOn.Value,
                RecipientInfo = new RecipientInfo() { Recipient = new Recipient() { FirstName = mp.RcptFirstName, LastName = mp.RcptLastName }, Address = new Entities.Address() { Street = mp.RcptAddressStreet, City = mp.RcptAddressCity, State = mp.RcptAddressState, Zip = mp.RcptAddressZIP } },
                ShippingMethod = mp.DocTypes == null ? "" : mp.DocTypes.ShippingCarrier,
                ShippingServiceType = mp.DocTypes == null ? "" : mp.DocTypes.ShippingServiceType,
                TotalPages = mp.TotalPages,
                TrackingNumber = mp.TrackingNo,
                ErrorCode = mp.ErrorCode,
                ErrorDesc = mp.ErrorDesc,
                Id = mp.UniqueId,
                MailingStatus = mp.ShippingStatus,
                PulledBackOn = mp.PulledBackOn.HasValue ? mp.PulledBackOn.Value : DateTime.MinValue,
                ShippedOn = mp.ShippedOn.HasValue ? mp.ShippedOn.Value : DateTime.MinValue,
                TotalSheets = mp.TotalPages
            };

            var serializedString = JsonConvert.SerializeObject(response);

            return serializedString;
        }

        public List<BatchAPIResponse> GeneralJsonAPIResponse(List<MailPieces> mailpieces, MailBatchDTO mailBatch, string clientCode)
        {
            List<BatchAPIResponse> batchApiResponses = new List<BatchAPIResponse>();
            foreach (var mp in mailpieces)
            {
                batchApiResponses.Add(new BatchAPIResponse()
                {
                    MBId = mailBatch.Id,
                    resp = new MailAPIResponse()
                    {
                        BusinessFriendlyIdName = mp.BusinessFriendlyIdName,
                        BusinessFriendlyIdValue = mp.BusinessFriendlyIdValue,
                        ClientCode = clientCode,
                        DocumentOrPackage = new DocumentOrPackage() { Name = mp.FileName },
                        ReceivedOn = mailBatch.ReceivedOn.Value,
                        RecipientInfo = new RecipientInfo() { Recipient = new Recipient() { FirstName = mp.RcptFirstName, LastName = mp.RcptLastName }, Address = new Entities.Address() { Street = mp.RcptAddressStreet, City = mp.RcptAddressCity, State = mp.RcptAddressState, Zip = mp.RcptAddressZIP } },
                        ShippingMethod = mp.DocTypes == null ? "" : mp.DocTypes.ShippingCarrier,
                        ShippingServiceType = mp.DocTypes == null ? "" : mp.DocTypes.ShippingServiceType,
                        TotalPages = mp.TotalPages,
                        TrackingNumber = mp.TrackingNo,
                        ErrorCode = mp.ErrorCode,
                        ErrorDesc = mp.ErrorDesc,
                        Id = mp.UniqueId,
                        MailingStatus = mp.ShippingStatus,
                        PulledBackOn = mp.PulledBackOn.HasValue ? mp.PulledBackOn.Value : DateTime.MinValue,
                        ShippedOn = mp.ShippedOn.HasValue ? mp.ShippedOn.Value : DateTime.MinValue,
                        TotalSheets = mp.TotalPages
                    }
                });
            }

            return batchApiResponses;
        }
        public string GeneralJsonReport(MailPieces mp, MailBatches mailBatch)
        {
            MailAPIResponse response = new MailAPIResponse()
            {
                BusinessFriendlyIdName = mp.BusinessFriendlyIdName,
                BusinessFriendlyIdValue = mp.BusinessFriendlyIdValue,
                ClientCode = mp.Clients.AppCode,
                DocumentOrPackage = new DocumentOrPackage() { Name = mp.FileName },
                ReceivedOn = mailBatch.ReceivedOn.Value,
                RecipientInfo = new RecipientInfo() { Recipient = new Recipient() { FirstName = mp.RcptFirstName, LastName = mp.RcptLastName }, Address = new Entities.Address() { Street = mp.RcptAddressStreet, City = mp.RcptAddressCity, State = mp.RcptAddressState, Zip = mp.RcptAddressZIP } },
                ShippingMethod = mp.DocTypes.ShippingCarrier,
                ShippingServiceType = mp.DocTypes.ShippingServiceType,
                TotalPages = mp.TotalPages,
                TrackingNumber = mp.TrackingNo,
                ErrorCode = mp.ErrorCode,
                ErrorDesc = mp.ErrorDesc,
                Id = mp.UniqueId,
                MailingStatus = mp.ShippingStatus,
                PulledBackOn = mp.PulledBackOn.HasValue ? mp.PulledBackOn.Value : DateTime.MinValue,
                ShippedOn = mp.ShippedOn.HasValue ? mp.ShippedOn.Value : DateTime.MinValue,
                TotalSheets = mp.TotalPages
            };

            var serializedString = JsonConvert.SerializeObject(response);

            return serializedString;
        }
        public string GenerateHashHMAC(string key, string message)
        {
            var encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(key);
            byte[] messageBytes = encoding.GetBytes(message);

            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage);
            }
            //return String.Concat(Array.ConvertAll(hash, x => x.ToString("x2")));
        }

        public async Task<string> SendEllieMaeResponse(List<MailAPIResponse> responseReport)
        {
            string json = JsonConvert.SerializeObject(responseReport);
            string hashHMAC = GenerateHashHMAC(Config.EllieMae_Secret, json);
            //var token = await GetToken();
            using (var client = SetupClient(Config.EllieMae_X_API_Key, hashHMAC))
            {
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(Config.EllieMae_UploadResponseURL, content);
                var responseString = await response.Content.ReadAsStringAsync();
                return response.StatusCode.ToString();
            }
        }

        private static HttpClient SetupClient(string xAPIKey, string hashHMAC)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(Config.EllieMae_BaseURL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            client.DefaultRequestHeaders.Add("X-API-KEY", xAPIKey);
            client.DefaultRequestHeaders.Add("X-Elli-Signature", hashHMAC);
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
