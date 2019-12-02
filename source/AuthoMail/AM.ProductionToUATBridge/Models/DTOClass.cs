using System;

namespace AM.ProductionToUATBridge
{
    public class MailPieceDTO
    {
        public int Id { get; set; }
        public int MailBatchId { get; set; }
        public int? ClientId { get; set; }
        public Guid UniqueId { get; set; }
        public string BusinessFriendlyIdName { get; set; }
        public string BusinessFriendlyIdValue { get; set; }
        public string FileName { get; set; }
        public string DocumentOrPackageType { get; set; }
        public string RcptFirstName { get; set; }
        public string RcptLastName { get; set; }
        public string RcptPhone { get; set; }
        public string RcptAddressStreet { get; set; }
        public string RcptAddressCity { get; set; }
        public string RcptAddressState { get; set; }
        public string RcptAddressZIP { get; set; }
        public string RetRcptFirstName { get; set; }
        public string RetRcptLastName { get; set; }
        public string RetRcptPhone { get; set; }
        public string RetRcptAddressStreet { get; set; }
        public string RetRcptAddressCity { get; set; }
        public string RetRcptAddressState { get; set; }
        public string RetRcptAddressZIP { get; set; }
        public MailBatchDTO MailBatch { get; set; }
    }

    public class MailBatchDTO
    {
        public int Id { get; set; }
        public Guid UniqueId { get; set; }
        public int? ClientId { get; set; }
        public string ClientBatchId { get; set; }
        public string BatchName { get; set; }
        public string DocumentOrPackageType { get; set; }
        public string Priority { get; set; }
        public string RetRcptFirstName { get; set; }
        public string RetRcptLastName { get; set; }
        public string RetRcptAddressStreet { get; set; }
        public string RetRcptAddressCity { get; set; }
        public string RetRcptAddressState { get; set; }
        public string RetRcptAddressZIP { get; set; }
        public ClientDTO Client { get; set; }
    }

    public class ClientDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AppCode { get; set; }
    }
}
