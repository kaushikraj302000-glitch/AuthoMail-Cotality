using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AM.Web.Models
{
    public class PullBackResponse
    {
        public string MailPieceId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public DateTime? PullBackRequestedOn { get; set; }
        public string PulledBackRequestedBy { get; set; }
    }
}