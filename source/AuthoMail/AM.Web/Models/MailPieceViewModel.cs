using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AM.Web.Models
{
    public class MailPieceViewModel
    {
        public string BusinessIdValue { get; set; }
        public Guid UniqueId { get; set; }
        public string AppCode { get; set; }
        public string FileName { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int TotalPages { get; set; }
        public bool IsPullBackRequested { get; set; }
        public string Status { get; set; }
        public DateTime? ReceivedOn { get; set; }
        public DateTime? DueDate { get; set; }
        public string TrackingNo { get; set; }
        public string DocType { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDesc { get; set; }
    }
}