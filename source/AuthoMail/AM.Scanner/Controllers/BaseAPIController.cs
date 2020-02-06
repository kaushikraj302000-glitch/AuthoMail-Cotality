using AM.Scanner.Common;
using AM.Scanner.Models;
using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;


namespace AM.Scanner.Controllers
{

    [RoutePrefix("api/report")]
    [Authorize]
    public class BaseAPIController : ApiController
    {
        IScannerService scannerSvc;
        public BaseAPIController(IScannerService scannerSvc)
        {
            this.scannerSvc = scannerSvc;
        }
        [HttpGet]
        [Route("ScanedDocumentList")]
        public List<Services.ScanedDocument> ScanedDocumentList(string clientCode, string barcodeValue, string loanNumber, string printOnFrom, string printOnTo, string shippedOnFrom, string shippedOnTo)
        {
            using (new PerfTimerLogger("ScanedDocumentList[BaseAPIController]: "))
            {
                var model = scannerSvc.GetScanReportData(clientCode, barcodeValue, loanNumber, printOnFrom, printOnTo, shippedOnFrom, shippedOnTo);
                return model;
            }
        }

        [HttpGet]
        [Route("PendingScanningList")]
        public List<Services.PendingScanning> PendingScanningList(string clientCode)
        {
            using (new PerfTimerLogger("PendingScanningList[BaseAPIController]: "))
            {
                var model = scannerSvc.GetPendingScanning(clientCode);
                return model;
            }
        }

        [HttpGet]
        [Route("GetAllBatchForScan")]
        public List<Services.GetAllBatchCountDetails> GetAllBatchForScan(string clientCode)
        {
            using (new PerfTimerLogger("GetAllBatchForScan[BaseAPIController]: "))
            {
                var model = scannerSvc.GetRecentBatchesStats(clientCode.Trim());
                return model;
            }
        }
    }
}
