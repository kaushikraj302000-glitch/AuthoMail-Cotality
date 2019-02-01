using AM.Scanner.Common;
using AM.Scanner.Models;
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
        [HttpGet]
        [Route("ScanedDocumentList")]
        public List<ScanedDocument> ScanedDocumentList(string clientCode, string barcodeValue, string loanNumber, string printOnFrom, string printOnTo, string shippedOnFrom, string shippedOnTo)
        {
            using (new PerfTimerLogger("ScanedDocumentList[BaseAPIController]: "))
            {
                var model = DBHelper.GetScanReportData(clientCode, barcodeValue, loanNumber, printOnFrom, printOnTo, shippedOnFrom, shippedOnTo);
                return model;
            }
        }

        [HttpGet]
        [Route("PendingScanningList")]
        public List<PendingScanning> PendingScanningList(string clientCode)
        {
            using (new PerfTimerLogger("PendingScanningList[BaseAPIController]: "))
            {
                var model = DBHelper.GetPendingScanning(clientCode);
                return model;
            }
        }

        [HttpGet]
        [Route("GetAllBatchForScan")]
        public List<GetAllBatchCountDetails> GetAllBatchForScan(string clientCode)
        {
            using (new PerfTimerLogger("GetAllBatchForScan[BaseAPIController]: "))
            {
                var model = DBHelper.GetRecentBatchesStats(clientCode.Trim());
                return model;
            }
        }
    }
}
