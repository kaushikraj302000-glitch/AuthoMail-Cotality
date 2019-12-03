using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Entities
{
    public class SuccessResponse
    {
        public Guid UniqueId { get; set; }
        public string Message { get; set; }
    }

    public class MailRequestResponse
    {
        public Guid Id { get; set; }
        public DateTime ReceivedOn { get; set; }
        public string ClientCode { get; set; }
        public string BusinessFriendlyIdName { get; set; }
        public string BusinessFriendlyIdValue { get; set; }
        public DocumentOrPackage DocumentOrPackage { get; set; }
        public RecipientInfo RecipientInfo { get; set; }
        public string MailingStatus { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDesc { get; set; }
        public DateTime? ShippedOn { get; set; }
        public string ShippingMethod { get; set; }
        public string ShippingServiceType { get; set; }
        public string TrackingNumber { get; set; }
        public int TotalPages { get; set; }
        public int TotalSheets { get; set; }
        public DateTime? PulledBackOn { get; set; }

        public bool ShouldSerializeErrorCode()
        {
            return this.MailingStatus == ShippingStatus.Error.ToString() || this.MailingStatus == ShippingStatus.Unknown.ToString();
        }
        public bool ShouldSerializeErrorDesc()
        {
            return this.MailingStatus == ShippingStatus.Error.ToString() || this.MailingStatus == ShippingStatus.Unknown.ToString();
        }

        public bool ShouldSerializeShippedOn()
        {
            return this.MailingStatus == ShippingStatus.Shipped.ToString();
        }
        public bool ShouldSerializeShippingMethod()
        {
            return this.MailingStatus == ShippingStatus.Shipped.ToString();
        }
        public bool ShouldSerializeShippingServiceType()
        {
            return this.MailingStatus == ShippingStatus.Shipped.ToString();
        }
        public bool ShouldSerializeTrackingNumber()
        {
            return this.MailingStatus == ShippingStatus.Shipped.ToString();
        }
        public bool ShouldSerializeTotalPages()
        {
            return this.MailingStatus == ShippingStatus.Shipped.ToString();
        }
        public bool ShouldSerializeTotalSheets()
        {
            return this.MailingStatus == ShippingStatus.Shipped.ToString();
        }
        public bool ShouldSerializePulledBackOn()
        {
            return this.MailingStatus == ShippingStatus.PulledBack.ToString();
        }
    }
}
