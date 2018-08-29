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
        public async Task<eVSLabelResponse> RequesteVSLabelAsync(string userId, eVSLabelRequest request)
        {
            try
            {
                bool isAPIError = false;
                string apiErrorMSG = null;
                Task<int> dbLog = null;
                int logId = 0;

                if (request == null || userId.IsNullOrWhiteSpace())
                {
                    isAPIError = true;
                    apiErrorMSG = "For create eVS Label, UserId must be specified and Request At least one.";
                }

                var labelInputXML = request.Serialize();

                if (Config.Web_API_USPS_IsDbLoggingEnabled)
                {
                    dbLog = db.ExecuteScalarAsync<int>("Insert into [API].[USPSeVS] ([APIName], [UserId], [APIRequest], [APIRequestedOn],[IsAPIError],[IsUSPSError],[APIResponse], [APIResponseOn],[Postage]) OUTPUT INSERTED.Id VALUES(@APIName, @UserId, @APIRequest, @APIRequestedOn, @IsAPIError, @IsUSPSError, @APIResponse, @APIResponseOn,@Postage);", new { APIName = Config.Web_API_USPS_eVSAPIName, UserId = userId, APIRequest = labelInputXML, APIRequestedOn = Common.Now, IsAPIError = isAPIError, IsUSPSError = false, APIResponse = apiErrorMSG, APIResponseOn = isAPIError ? Common.Now : (DateTime?)null, Postage = 0 });
                }

                if (isAPIError)
                {
                    if (Config.Web_API_USPS_IsDbLoggingEnabled)
                    {
                        logId = await dbLog.ConfigureAwait(false);
                    }

                    return new eVSLabelResponse()
                    {
                        Error = new USPSError()
                        {
                            Source = ResponseSource.CLSVC,
                            Description = apiErrorMSG
                        }
                    };
                }

                FormUrlEncodedContent payload = null;
                Task<int> uspsReqLog = null;
                int uspsReqLogAwait = 0;
                eVSResponse eVSresponse = null;
                eVSLabelResponse eVSLabelresponse = null;

                var eVSrequest = new eVSRequest(request);
                eVSrequest.USERID = Config.Web_API_USPS_UserId;

                var eVSrequestXML = eVSrequest.Serialize();

                using (HttpClient httpClient = new HttpClient())
                {
                    string url = Config.Web_API_USPS_URL;
                    //string url = @"https://stg-secure.shippingapis.com/ShippingAPI.dll";

                    payload = new FormUrlEncodedContent(new[] 
                        {
                            new KeyValuePair<string, string>("API",Config.Web_API_USPS_eVSAPIName),
                            //new KeyValuePair<string, string>("API","Verify"),
                            new KeyValuePair<string, string>("XML",eVSrequestXML)
                        });

                    //var payloadResult = await payload.ReadAsStringAsync().ConfigureAwait(false);
                    var uspsReq = httpClient.PostAsync(url, payload);

                    if (Config.Web_API_USPS_IsDbLoggingEnabled)
                    {
                        logId = await dbLog.ConfigureAwait(false);
                        uspsReqLog = db.ExecuteScalarAsync<int>("Update [API].[USPSeVS] set [USPSRequest] = @USPSRequest, [USPSRequestedOn]= @USPSRequestedOn;", new { USPSRequest = eVSrequestXML, USPSRequestedOn = Common.Now });
                    }

                    string responseXML = null;
                    bool isUSPSError = false;

                    try
                    {
                        var response = await uspsReq.ConfigureAwait(false);
                        responseXML = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        isUSPSError = !responseXML.Contains("eVSResponse");

                        if (isUSPSError)
                        {
                            var err = responseXML.Deserialize<USPSError>();
                            eVSresponse = new eVSResponse { Error = err };
                        }
                    }
                    catch (Exception ex)
                    {
                        isUSPSError = true;
                        eVSresponse = new eVSResponse
                        {
                            Error = new USPSError
                            {
                                Source = ResponseSource.USPS,
                                Description = ex.ToString()
                            }
                        };
                    }

                    Task<int> uspsResXMLLog = null;
                    int uspsResXMLLogAwait = 0;
                    Task<int> apiResponseLog = null;
                    int apiResponseLogAwait = 0;

                    if (Config.Web_API_USPS_IsDbLoggingEnabled)
                    {
                        uspsReqLogAwait = await uspsReqLog.ConfigureAwait(false);
                        uspsResXMLLog = db.ExecuteScalarAsync<int>("Update [API].[USPSeVS] set IsUSPSError = @IsUSPSError, [USPSResponse] = @USPSResponse, [USPSResponseOn] = @USPSResponseOn;", new { IsUSPSError = isUSPSError, USPSResponse = responseXML, USPSResponseOn = Common.Now });
                    }

                    if (!isUSPSError)
                    {
                        eVSresponse = responseXML.Deserialize<eVSResponse>();
                    }

                    string commitmentName = null;
                    DateTime? scheduledDeliveryDate = null;

                    if (eVSresponse.Commitment != null)
                    {
                        commitmentName = eVSresponse.Commitment.CommitmentName;
                        scheduledDeliveryDate = DateTime.Parse(eVSresponse.Commitment.ScheduledDeliveryDate);
                    }

                    if (Config.Web_API_USPS_IsDbLoggingEnabled)
                    {
                        uspsResXMLLogAwait = await uspsResXMLLog.ConfigureAwait(false);
                        apiResponseLog = db.ExecuteAsync("Update [API].[USPSeVS] set APIResponse = @APIResponse, APIResponseOn = @APIResponseOn, BarcodeNumber = @BarcodeNumber, Postage = @Postage Where Id = @Id", new { APIResponse = responseXML, APIResponseOn = Common.Now, BarcodeNumber = eVSresponse.BarcodeNumber, Postage = eVSresponse.Postage, CommitmentName = commitmentName, ScheduledDeliveryDate = scheduledDeliveryDate, Id = logId });
                    }

                    apiResponseLogAwait = await apiResponseLog.ConfigureAwait(false);
                }

                //Todo convert response to label
                eVSLabelresponse = eVSLabelResponse(eVSresponse);

                return await Task.Run(() => eVSLabelresponse).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return new eVSLabelResponse()
                {
                    Error = new USPSError()
                    {
                        Source = ResponseSource.CLSVC,
                        Description = "Unexpected error"
                    }
                };
            }
        }
        private eVSLabelResponse eVSLabelResponse(eVSResponse response)
        {
            var labelResponse = new eVSLabelResponse();

            labelResponse.BarcodeNumber = response.BarcodeNumber;
            labelResponse.LabelImageString = response.LabelImageString;
            labelResponse.ReceiptImageString = response.ReceiptImageString;
            labelResponse.ToName = response.ToName;
            labelResponse.ToFirm = response.ToFirm;
            labelResponse.ToAddress1 = response.ToAddress1;
            labelResponse.ToAddress2 = response.ToAddress2;
            labelResponse.ToCity = response.ToCity;
            labelResponse.ToState = response.ToState;
            labelResponse.ToZip5 = response.ToZip5;
            labelResponse.ToZip4 = response.ToZip4;
            labelResponse.Postnet = response.Postnet;
            labelResponse.RDC = response.RDC;
            labelResponse.Postage = response.Postage;
            labelResponse.Extraservices = response.Extraservices;
            labelResponse.Zone = response.Zone;
            labelResponse.DimensionalWeight = response.DimensionalWeight;
            labelResponse.CarrierRoute = response.CarrierRoute;
            labelResponse.PermitHolderName = response.PermitHolderName;
            labelResponse.InductionType = response.InductionType;
            labelResponse.LogMessage = response.LogMessage;

            labelResponse.Commitment = response.Commitment != null ? new CommitmentDetail()
            {
                CommitmentName = response.Commitment.CommitmentName,
                ScheduledDeliveryDate = DateTime.ParseExact(response.Commitment.ScheduledDeliveryDate, "yyyy-MM-dd", null)
            } : null;

            labelResponse.Error = response.Error;

            return labelResponse;
        }
    }
}
