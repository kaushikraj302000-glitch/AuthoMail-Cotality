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
            var model = DBHelper.GetScanReportData(clientCode, barcodeValue, loanNumber, printOnFrom, printOnTo, shippedOnFrom, shippedOnTo);
            return model;
        }

        [HttpGet]
        [Route("PendingScanningList")]
        public List<PendingScanning> PendingScanningList(string clientCode)
        {
            var model = DBHelper.GetPendingScanning(clientCode);
            return model;
        }
    }
}
