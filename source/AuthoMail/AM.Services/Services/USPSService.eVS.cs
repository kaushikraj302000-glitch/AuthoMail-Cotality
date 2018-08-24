using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;
using System.Net.Http;
using Dapper;
using AM.USPSLib;

namespace AM.Services
{
    public partial class USPSService
    {
        public eVSLabelResponse RequesteVSLabel(string userId, eVSLabelRequest request)
        {
            return new eVSLabelResponse();//this.RequesteVSLabelAsync(request).Result;
        }
        //private async Task<eVSResponse> RequesteVSLabelAsync(eVSRequest request)
        //{
        //    try
        //    {
        //        string userId = null;
        //        bool isAPIError = false;
        //        string apiErrorMSG = null;
        //        Task<int> dbLog = null;
        //        int logId = 0;

        //        //Todo remove this line
        //        //request = File.ReadAllText(@"E:\PriorityExp\PRIORITY EXPRESS_4X6LABEL.XML").Deserialize<eVSRequest>();

        //        if (request == null || request.UserId.IsNullOrWhiteSpace())
        //        {
        //            isAPIError = true;
        //            apiErrorMSG = "For create eVS Label, UserId must be specified or Request At least one.";
        //        }
        //        else
        //        {
        //            userId = request.UserId;
        //            //Todo for production used Dallas id
        //            if (request.USPSUSERID.IsNullOrWhiteSpace())
        //                request.USPSUSERID = Config.Web_API_USPS_UserId;
        //        }

        //        var uspsInputXML = request.Serialize();

        //        if (Config.Web_API_USPS_IsDbLoggingEnabled)
        //        {
        //            //Todo take name from db
        //            dbLog = db.ExecuteScalarAsync<int>("Insert into [API].[USPSeVS] ([APIName], [UserId], [USPSRequest], [USPSRequestedOn],[IsAPIError],[IsUSPSError],[USPSResponse], [USPSResponseOn]) OUTPUT INSERTED.Id VALUES(@APIName, @UserId, @USPSRequest, @USPSRequestedOn, @IsAPIError, @IsUSPSError, @USPSResponse, @USPSResponseOn);", new { APIName = Config.Web_API_USPS_eVSAPIName, UserId = userId, USPSRequest = uspsInputXML, USPSRequestedOn = Common.Now, IsAPIError = isAPIError, IsUSPSError = false, USPSResponse = apiErrorMSG, USPSResponseOn = isAPIError ? Common.Now : (DateTime?)null });
        //        }

        //        if (isAPIError)
        //        {
        //            if (Config.Web_API_USPS_IsDbLoggingEnabled)
        //            {
        //                logId = await dbLog;
        //            }

        //            return new eVSResponse()
        //            {
        //                Error = new USPSError()
        //                {
        //                    Source = ResponseSource.CLSVC,
        //                    Description = apiErrorMSG
        //                }
        //            };
        //        }

        //        FormUrlEncodedContent payload = null;
        //        Task<string> uspsReq = null;
        //        eVSResponse responseLabel = null;

        //        using (HttpClient httpClient = new HttpClient())
        //        {
        //            //string url = Config.Web_API_USPS_eVSURL;
        //            string url = @"https://stg-secure.shippingapis.com/ShippingAPI.dll";

        //            payload = new FormUrlEncodedContent(new[] 
        //                {
        //                    //Todo get from db api name
        //                    new KeyValuePair<string, string>("API",Config.Web_API_USPS_eVSAPIName),
        //                    //new KeyValuePair<string, string>("API","Verify"),
        //                    new KeyValuePair<string, string>("XML",uspsInputXML)
        //                });

        //            var payloadResult = await payload.ReadAsStringAsync();
        //            uspsReq = httpClient.GetStringAsync("{0}?{1}".Fill(url, payloadResult));

        //            var responseXML = await uspsReq;

        //            Task<int> uspsResponseLog = null;

        //            if (responseXML.Contains("eVSResponse"))
        //            {
        //                responseLabel = responseXML.Deserialize<eVSResponse>();
        //            }
        //            else
        //            {
        //                var err = responseXML.Deserialize<USPSError>();
        //                responseLabel = new eVSResponse { Error = err };
        //            }

        //            if (Config.Web_API_USPS_IsDbLoggingEnabled)
        //            {
        //                logId = await dbLog;
        //                uspsResponseLog = db.ExecuteAsync("Update [API].[USPSeVS] set USPSResponse = @USPSResponse, USPSResponseOn = @USPSResponseOn, BarcodeNumber = @BarcodeNumber, Postage = @Postage Where Id = @Id", new { USPSResponse = responseXML, USPSResponseOn = Common.Now, BarcodeNumber = responseLabel.BarcodeNumber, Postage = responseLabel.Postage, Id = logId });
        //            }

        //            var uspsResponseLogAwait = await uspsResponseLog;
        //        }

        //        return await Task.Run(() => responseLabel);
        //    }
        //    catch (Exception ex)
        //    {
        //        return new eVSResponse()
        //        {
        //            Error = new USPSError()
        //            {
        //                Source = ResponseSource.CLSVC,
        //                Description = ex.ToString()
        //            }
        //        };
        //    }
        //}
    }
}
