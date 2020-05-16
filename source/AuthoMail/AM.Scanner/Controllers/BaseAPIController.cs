using AM.Scanner.Common;
using AM.Scanner.Models;
using AM.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;


namespace AM.Scanner.Controllers
{

    [RoutePrefix("api/report")]
    //[Authorize]
    public class BaseAPIController : ApiController
    {
        IScannerService scannerSvc;
        public BaseAPIController()
        {
            this.scannerSvc = new ScannerService();
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
        [Route("GetAllBatchForScan1")]
        public List<AppCodeBatchDetails> GetAllBatchForScan()
        {
            using (new PerfTimerLogger("GetAllBatchForScan[BaseAPIController]: "))
            {
                var model = scannerSvc.GetRecentBatchesStats();
                List<AppCodeBatchDetails> appCodeBatchDetails = new List<AppCodeBatchDetails>();
                foreach (var appCode in model.GroupBy(s => s.AppCode))
                {
                    AppCodeBatchDetails appCodeBatchDetail = new AppCodeBatchDetails();
                    appCodeBatchDetail.AppCode = appCode.Key;
                    appCodeBatchDetail.Batches = new List<Services.GetAllBatchCountDetails>();
                    appCodeBatchDetail.Batches.AddRange(appCode.ToList());
                    appCodeBatchDetails.Add(appCodeBatchDetail);
                }
                return appCodeBatchDetails;
            }
        }
        [HttpGet]
        [Route("GetAllBatchForScan")]
        public object DashBoard()
        {
            using (new PerfTimerLogger("GetAllBatchForScan[BaseAPIController]: "))
            {
                var headers = new List<string>() { "Client", "Status", "Day Total" };

                var model = scannerSvc.GetRecentBatchesStats();
                var scanCount = scannerSvc.GetScanStats();
                var batches = model.Select(s => s.BatchName).Distinct().ToList();
                batches = BuildOrder(batches);
                headers.AddRange(batches);

                var objList = new List<object>();
                foreach (var appCode in model.GroupBy(s => s.AppCode))
                {
                    //var dataObject = new List<string>();
                    //dynamic myobject = new ExpandoObject();
                    //IDictionary<string, object> myUnderlyingObject = myobject;
                    var values = new List<string>();
                    //myUnderlyingObject.Add("AppCode", appCode.Key);
                    values.Add(appCode.Key);

                    var data = scanCount.SingleOrDefault(s => s.AppCode == appCode.Key);
                    string dayTotal = "0/0";
                    string status = "No Production";

                    if (data != null)
                    {
                        dayTotal = $"{data.ScanCount}/{data.TotalCount}";
                        if (data.ScanCount > 0 && data.TotalCount != data.ScanCount)
                            status = "Production Incomplete";
                        if (data.ScanCount == data.TotalCount)
                            status = "Production Complete";
                    }

                    //myUnderlyingObject.Add("Status", status);
                    //myUnderlyingObject.Add("DayTotal", dayTotal);
                    values.Add(status);
                    values.Add(dayTotal);
                    //dataObject.Add(appCode.Key);
                    //dataObject.Add(status);
                    //dataObject.Add(dayTotal);
                    foreach (var batch in batches)
                    {
                        var b = appCode.SingleOrDefault(a => a.BatchName == batch);
                        if (b == null)
                        {
                            //myUnderlyingObject.Add(batch, "--");
                            values.Add("--");
                        }
                        else
                        {
                            //myUnderlyingObject.Add(b.BatchName, $"{b.ScannedLoans}/{b.TotalLoans}");
                            values.Add($"{b.ScannedLoans}/{b.TotalLoans}");
                        }
                        //dataObject.Add("--");
                        //ReceivedOn , AppCode,ScannedLoans,TotalLoans,BatchName
                    }
                    //foreach (var item in appCode)
                    //{
                    //    myUnderlyingObject.Remove(item.BatchName);
                    //    myUnderlyingObject.Add(item.BatchName, $"{item.ScannedLoans}/{item.TotalLoans}");
                    //    //dataObject.Remove(item.BatchName);
                    //    //dataObject.Add($"{item.ScannedLoans}/{item.TotalLoans}");
                    //}

                    //var props = GetProperties(myUnderlyingObject);
                    //dataObject.AddRange(props.Select(p => p.Value));
                    objList.Add(values);
                }

                return new
                {
                    headers = headers,
                    data = objList
                };
            }
        }
        private List<string> BuildOrder(List<string> batches)
        {
            var dict = new Dictionary<string, TimeSpan>();
            foreach (var batch in batches.Where(b => b != "Backlog"))
            {
                DateTime dateTime = DateTime.ParseExact(batch,
                                    "hhtt", CultureInfo.InvariantCulture);
                dict.Add(batch, dateTime.TimeOfDay);
            }
            var list = dict.OrderBy(d => d.Value).Select(d => d.Key).ToList();
            list.Add("Backlog");
            return list;

        }
        private static Dictionary<string, string> GetProperties(object obj)
        {
            var props = new Dictionary<string, string>();
            if (obj == null)
                return props;

            var type = obj.GetType();
            foreach (var prop in type.GetProperties())
            {
                var val = prop.GetValue(obj, new object[] { });
                var valStr = val == null ? "" : val.ToString();
                props.Add(prop.Name, valStr);
            }

            return props;
        }
    }
}
