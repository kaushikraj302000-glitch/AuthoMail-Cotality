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

    public class MailAPIResponse
    {
        public Guid Id { get; set; }
        public Guid UniqueId { get; set; }
        public string Message { get; set; }
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

        public bool ShouldSerializeId()
        {
            return this.MailingStatus != ShippingStatus.Ack.ToString();
        }
        public bool ShouldSerializeUniqueId()
        {
            return this.MailingStatus == ShippingStatus.Ack.ToString();
        }
        public bool ShouldSerializeMessage()
        {
            return this.MailingStatus == ShippingStatus.Ack.ToString();
        }
        public bool ShouldSerializeClientCode()
        {
            return this.MailingStatus != ShippingStatus.Ack.ToString();
        }
        public bool ShouldSerializeReceivedOn()
        {
            return this.MailingStatus != ShippingStatus.Ack.ToString();
        }
        public bool ShouldSerializeBusinessFriendlyIdName()
        {
            return this.MailingStatus != ShippingStatus.Ack.ToString();
        }
        public bool ShouldSerializeBusinessFriendlyIdValue()
        {
            return this.MailingStatus != ShippingStatus.Ack.ToString();
        }
        public bool ShouldSerializeDocumentOrPackage()
        {
            return this.MailingStatus != ShippingStatus.Ack.ToString();
        }
        public bool ShouldSerializeRecipientInfo()
        {
            return this.MailingStatus != ShippingStatus.Ack.ToString();
        }
        public bool ShouldSerializeMailingStatus()
        {
            return this.MailingStatus != ShippingStatus.Ack.ToString();
        }
        public bool ShouldSerializeErrorCode()
        {
            if (this.MailingStatus == ShippingStatus.Ack.ToString()) return false;
            return this.MailingStatus == ShippingStatus.Error.ToString() || this.MailingStatus == ShippingStatus.Unknown.ToString();
        }
        public bool ShouldSerializeErrorDesc()
        {
            if (this.MailingStatus == ShippingStatus.Ack.ToString()) return false;
            return this.MailingStatus == ShippingStatus.Error.ToString() || this.MailingStatus == ShippingStatus.Unknown.ToString();
        }

        public bool ShouldSerializeShippedOn()
        {
            if (this.MailingStatus == ShippingStatus.Ack.ToString()) return false;
            return this.MailingStatus == ShippingStatus.Shipped.ToString() || this.MailingStatus == ShippingStatus.eDelivered.ToString();
        }
        public bool ShouldSerializeShippingMethod()
        {
            if (this.MailingStatus == ShippingStatus.Ack.ToString()) return false;
            return this.MailingStatus == ShippingStatus.Shipped.ToString() || this.MailingStatus == ShippingStatus.eDelivered.ToString();
        }
        public bool ShouldSerializeShippingServiceType()
        {
            if (this.MailingStatus == ShippingStatus.Ack.ToString()) return false;
            return this.MailingStatus == ShippingStatus.Shipped.ToString() || this.MailingStatus == ShippingStatus.eDelivered.ToString();
        }
        public bool ShouldSerializeTrackingNumber()
        {
            if (this.MailingStatus == ShippingStatus.Ack.ToString()) return false;
            return this.MailingStatus == ShippingStatus.Shipped.ToString() || this.MailingStatus == ShippingStatus.eDelivered.ToString();
        }
        public bool ShouldSerializeTotalPages()
        {
            if (this.MailingStatus == ShippingStatus.Ack.ToString()) return false;
            return this.MailingStatus == ShippingStatus.Shipped.ToString() || this.MailingStatus == ShippingStatus.eDelivered.ToString();
        }
        public bool ShouldSerializeTotalSheets()
        {
            if (this.MailingStatus == ShippingStatus.Ack.ToString()) return false;
            return this.MailingStatus == ShippingStatus.Shipped.ToString() || this.MailingStatus == ShippingStatus.eDelivered.ToString();
        }
        public bool ShouldSerializePulledBackOn()
        {
            if (this.MailingStatus == ShippingStatus.Ack.ToString()) return false;
            return this.MailingStatus == ShippingStatus.PulledBack.ToString();
        }
    }
}
