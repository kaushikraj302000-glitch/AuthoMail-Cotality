using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AM.Web.Models
{
    public class PullBackResponse
    {
        public string MailPieceId { get; set; }

        public string Message { get; set; }

        public bool IsSuccess { get; set; }
    }
}