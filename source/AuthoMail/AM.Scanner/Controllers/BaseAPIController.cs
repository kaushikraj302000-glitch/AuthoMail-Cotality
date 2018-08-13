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
    public class BaseAPIController : ApiController
    {
        [HttpGet]
        [Route("ScanedDocumentList")]
        public List<ScanedDocument> ScanedDocumentList(string clientCode, string barcodeValue, string loanNumber, string printOnFrom, string printOnTo, string shippedOnFrom, string shippedOnTo)
        {
            var model = DBHelper.GetScanReportData(clientCode, barcodeValue, loanNumber, printOnFrom, printOnTo, shippedOnFrom, shippedOnTo);
            return model;
        }

        public List<PendingScanning> PendingScanningList(string clientCode)
        {
            List<PendingScanning> model = new List<PendingScanning>();
            return model;
        }
    }
}
