using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AM.Web.Models
{
    public class ErrorSLAReportViewModel:MailPieceCommonViewModel
    {
        public string DocType { get; set; }
        public int BatchId { get; set; }
        public Guid UniqueId { get; set; }
        public DateTime? DueDate { get; set; }
    }
}