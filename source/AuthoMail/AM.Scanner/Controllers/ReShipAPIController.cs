using AM.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Transactions;
using System.Web;
using System.Web.Http;
using AM.Lib;

namespace AM.Scanner.Controllers
{
    [RoutePrefix("api/Reship")]
    public class ReShipAPIController : ApiController
    {
        IMailBatchService mailBatchSvc;

        public ReShipAPIController()
        {
            this.mailBatchSvc = new MailBatchService();
        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        [HttpGet]
        [Route("ReshipPackage")]
        public HttpResponseMessage ReshipPackage(int mailBatchId, int mailPieceId)
        {
            try
            {
                if (Unauthorized(HttpContext.Current.Request))
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                //Create HTTP Response.
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);

                var mailPieceInfo = mailBatchSvc.GetMailPiecesDataFromMailBatchIdAndMailPieceId(mailBatchId, mailPieceId);

                if (mailPieceInfo == null)
                {
                    //Throw 404 (Not Found) exception.
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.ReasonPhrase = $"Record not found for MailPieceId - {mailPieceId}";
                    return response;
                }

                using (TransactionScope scope = new TransactionScope())
                {
                    try
                    {
                        var newMailPiece = mailBatchSvc.ProcessReshipData(mailPieceInfo);

                        var mbAMRequestPath = Path.Combine(Common.Common.AMRootPath, newMailPiece.Client.AppCode, newMailPiece.MailBatchId.ToString(), "AMRequest");
                        var mbAMResponsePath = Path.Combine(Common.Common.AMRootPath, newMailPiece.Client.AppCode, newMailPiece.MailBatchId.ToString(), "AMResponse");
                        var mbPnSRequestPath = Path.Combine(Common.Common.AMRootPath, newMailPiece.Client.AppCode, newMailPiece.MailBatchId.ToString(), "PnSRequest");
                        var mbPnSResponsePath = Path.Combine(Common.Common.AMRootPath, newMailPiece.Client.AppCode, newMailPiece.MailBatchId.ToString(), "PnSResponse");

                        mbAMRequestPath.CreateDirectoryIfNotExist();
                        mbAMResponsePath.CreateDirectoryIfNotExist();
                        mbPnSRequestPath.CreateDirectoryIfNotExist();
                        mbPnSResponsePath.CreateDirectoryIfNotExist();

                        var oldPdfFilePath = Path.Combine(Common.Common.AMRootPath, mailPieceInfo.Client.AppCode, mailPieceInfo.MailBatchId.ToString(), "AMRequest", mailPieceInfo.FileName);

                        if (!File.Exists(oldPdfFilePath))
                        {
                            response.StatusCode = HttpStatusCode.NotFound;
                            response.ReasonPhrase = $"File not found for MailPieceId - {mailPieceId}";
                            return response;
                        }

                        File.Copy(oldPdfFilePath, Path.Combine(mbAMRequestPath, newMailPiece.FileName));

                        scope.Complete();
                    }
                    catch (Exception ex)
                    {
                        response.StatusCode = HttpStatusCode.NotFound;
                        response.ReasonPhrase = $"Error occured while executing reship process - {mailPieceId} with error : {ex.Message}";
                        return response;
                    }
                }

                return response;
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                return Request.CreateResponse(HttpStatusCode.BadRequest, e.Message);
            }
        }

        public bool Unauthorized(HttpRequest request)
        {
            if (request.Headers["Id"].Trim() != "5b189031-eecc-42d7-8b6d-b531caf1036e")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}