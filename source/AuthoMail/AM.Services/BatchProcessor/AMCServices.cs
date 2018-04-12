using AM.Entities;
using AM.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public interface IAMCService : IDisposable
    {
        StringBuilder AMCErrorReport(BatchData data);
        StringBuilder AMCDeliveryReport(BatchData data);
    }

    public class AMCService : ServiceBase, IAMCService
    {
        public StringBuilder AMCErrorReport(BatchData data)
        {
            var columnName = String.Join(",", "BatchID", "ID", "Package Name", "Loan Number", "File Name", "Date Received", "First Name", "Last Name", "Recipient Street Address", "Recipient City", "Recipient State", "Recipient Zip", "Error Code", "Error Text", "Shipping Method", "Customer Identifier", "RequestID");

            StringBuilder errorReport = new StringBuilder(columnName);

            errorReport.AppendLine();

            var erroredData = data.MailPieces.Where(l => l.ErrorCode != OutComeCodes.No_Error).ToList();

            if (erroredData == null || erroredData.Count == 0)
            {
                return null;
            }

            using (DocTypeService docTypeSvc = new DocTypeService())
            {
                // All the loans where outcome code is anything except no_error are processed invalid loans.
                foreach (MailPieceDTO mailPiece in erroredData)
                {
                    DocType docType = mailPiece.DocTypeId.HasValue ? docTypeSvc.GetDocTypeById(mailPiece.DocTypeId.Value) : null;

                    var custId = data.MailPieceCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "CustomerIdentifier").SingleOrDefault();

                    var requestId = data.MailPieceCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "RequestId").SingleOrDefault();

                    var line = String.Join(",",
                        data.MailBatch.BatchName == null ? string.Empty : data.MailBatch.BatchName.ToCsvString(),
                            mailPiece.UniqueId,
                            mailPiece.DocumentOrPackageType == null ? string.Empty : mailPiece.DocumentOrPackageType.ToCsvString(),
                            mailPiece.BusinessFriendlyIdValue == null ? string.Empty : mailPiece.BusinessFriendlyIdValue.ToCsvString(),
                            mailPiece.FileName == null ? string.Empty : mailPiece.FileName.ToCsvString(),
                            data.MailBatch.ReceivedOn == null ? string.Empty : data.MailBatch.ReceivedOn.Value.ToString(Config.Monitor_AMC_ReportDateFormat),
                            mailPiece.RcptFirstName == null ? string.Empty : mailPiece.RcptFirstName.ToCsvString(),
                            mailPiece.RcptLastName == null ? string.Empty : mailPiece.RcptLastName.ToCsvString(),
                            mailPiece.RcptAddressStreet == null ? string.Empty : mailPiece.RcptAddressStreet.ToCsvString(),
                            mailPiece.RcptAddressCity == null ? string.Empty : mailPiece.RcptAddressCity,
                            mailPiece.RcptAddressState == null ? string.Empty : mailPiece.RcptAddressState,
                            mailPiece.RcptAddressZIP == null ? string.Empty : mailPiece.RcptAddressZIP,
                            OutComeCodes.GetAMCMappingCode(mailPiece.ErrorCode),
                            mailPiece.ErrorDesc == null ? string.Empty : mailPiece.ErrorDesc.Replace("\r\n", " ").ToCsvString(),
                            docType == null ? string.Empty : docType.ShippingCarrier,
                            custId == null ? string.Empty : custId.Value,
                            requestId == null ? string.Empty : requestId.Value
                        );

                    errorReport.AppendLine(line);
                }
            }

            return errorReport;
        }

        public StringBuilder AMCDeliveryReport(BatchData data)
        {
            var columnName = String.Join(",", "BatchID", "ID", "Package Name", "Loan Number", "File Name", "Date Received", "Date Printed", "Date Mailed", "Client Code", "Tracking Number", "Shipping Method", "Customer Identifier", "First Name", "Last Name", "Recipient Street Address", "Recipient City", "Recipient State", "Recipient Zip", "RequestID", "Pages Shipped");

            StringBuilder deliveryReport = new StringBuilder(columnName);

            deliveryReport.AppendLine();

            var shippedData = data.MailPieces.Where(l => l.ShippingStatus == ShippingStatus.Shipped.ToString() || l.ShippingStatus == ShippingStatus.PulledBack.ToString()).ToList();

            if (shippedData == null || shippedData.Count == 0)
            {
                return null;
            }

            using (DocTypeService docTypeSvc = new DocTypeService())
            {
                // All the loans where outcome code is anything except no_error are processed invalid loans.
                foreach (MailPieceDTO mailPiece in shippedData)
                {
                    DocType docType = mailPiece.DocTypeId.HasValue ? docTypeSvc.GetDocTypeById(mailPiece.DocTypeId.Value) : null;

                    var shippingCredCode = data.MailPieceCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "ShippingCredCode").SingleOrDefault();

                    var custId = data.MailPieceCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "CustomerIdentifier").SingleOrDefault();

                    var requestId = data.MailPieceCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "RequestId").SingleOrDefault();

                    var line = String.Join(",",
                        data.MailBatch.BatchName == null ? string.Empty : data.MailBatch.BatchName.ToCsvString(),
                            mailPiece.UniqueId,
                            mailPiece.DocumentOrPackageType == null ? string.Empty : mailPiece.DocumentOrPackageType.ToCsvString(),
                            mailPiece.BusinessFriendlyIdValue == null ? string.Empty : mailPiece.BusinessFriendlyIdValue.ToCsvString(),
                            mailPiece.FileName == null ? string.Empty : mailPiece.FileName,
                            data.MailBatch.ReceivedOn == null ? string.Empty : data.MailBatch.ReceivedOn.Value.ToString(Config.Monitor_AMC_ReportDateFormat),
                            mailPiece.PrintedOn == null ? string.Empty : mailPiece.PrintedOn.Value.ToString(Config.Monitor_AMC_ReportDateFormat),
                            mailPiece.ShippedOn == null ? string.Empty : mailPiece.ShippedOn.Value.ToString(Config.Monitor_AMC_ReportDateFormat),
                            shippingCredCode == null ? string.Empty : shippingCredCode.Value,
                            mailPiece.TrackingNo == null ? string.Empty : mailPiece.TrackingNo.ToCsvString(),
                            docType == null ? string.Empty : docType.ShippingCarrier,
                            custId == null ? string.Empty : custId.Value,
                            mailPiece.RcptFirstName == null ? string.Empty : mailPiece.RcptFirstName.ToCsvString(),
                            mailPiece.RcptLastName == null ? string.Empty : mailPiece.RcptLastName.ToCsvString(),
                            mailPiece.RcptAddressStreet == null ? string.Empty : mailPiece.RcptAddressStreet.ToCsvString(),
                            mailPiece.RcptAddressCity == null ? string.Empty : mailPiece.RcptAddressCity,
                            mailPiece.RcptAddressState == null ? string.Empty : mailPiece.RcptAddressState,
                            mailPiece.RcptAddressZIP == null ? string.Empty : mailPiece.RcptAddressZIP,
                            requestId == null ? string.Empty : requestId.Value,
                            mailPiece.TotalPages);

                    deliveryReport.AppendLine(line);
                }
            }
            return deliveryReport;
        }
    }
}
