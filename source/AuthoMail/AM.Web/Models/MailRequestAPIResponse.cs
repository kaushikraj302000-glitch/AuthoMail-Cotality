using AM.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace AM.Web.Models
{
    public class SuccessResponse
    {
        public Guid UniqueId { get; set; }
        public string Message { get; set; }
    }

    public class ErrorResponse
    {
        public Guid ID { get; set; }
        public DateTime ReceivedOn { get; set; }
        public string ClientCode { get; set; }
        public string BusinessFriendlyIdName { get; set; }
        public string BusinessFriendlyIdValue { get; set; }
        public DocumentOrPackage DocumentOrPackage { get; set; }
        public RecipientInfo RecipientInfo { get; set; }
        public string MailingStatus { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDesc { get; set; }
    }

}