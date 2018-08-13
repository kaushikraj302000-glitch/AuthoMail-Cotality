using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AM.Scanner.Models
{
    public class ScanedDocument
    {
        public string BatchName { get; set; }
        public string LoanNumber { get; set; }
        public string BarcodeValue { get; set; }
        public string TotalPages { get; set; }
        public string ReceivedOn { get; set; }
        public string ShippedOn { get; set; }
        public string MailPieceStatus { get; set; }

    }

    public class PendingScanning
    {

    }
}