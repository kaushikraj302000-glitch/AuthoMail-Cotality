using AM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;

namespace AM.Reporting
{
    static class Common
    {
        public static readonly DateTime Now = DateTime.UtcNow;

        public static readonly DateTime Today = DateTime.UtcNow.Date;
       
        static Common()
        {
        }

        public static StringBuilder USAAErrorReport(BatchData data)
        {
            StringBuilder errorReport = new StringBuilder("BatchID,ID,Channel,Package Name,Loan Number,File Name,Date Received,First Name,Last Name,Recipient Street Address,Recipient City,Recipient State,Recipient Zip,Error Code,Error Text,Shipping Method,Customer Identifier,RequestID");

            errorReport.AppendLine();

            // All the loans where outcome code is anything except no_error are processed invalid loans.
            foreach (MailPieceDTO mailPiece in data.MailPieces.Where(l => l.ErrorCode != OutComeCodes.No_Error))
            {
                errorReport.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17}",
                        data.MailBatch.BatchName == null ? string.Empty : data.MailBatch.BatchName,
                        mailPiece.UniqueId,
                        data.MailBatch.SFTChannelId,
                        mailPiece.DocumentOrPackageType == null ? string.Empty : mailPiece.DocumentOrPackageType,
                        mailPiece.BusinessFriendlyIdValue == null ? string.Empty : mailPiece.BusinessFriendlyIdValue,
                        mailPiece.FileName == null ? string.Empty : mailPiece.FileName,
                        data.MailBatch.ReceivedOn == null ? string.Empty : data.MailBatch.ReceivedOn.Value.UtcToEst().ToString(),
                        mailPiece.RcptFirstName == null ? string.Empty : mailPiece.RcptFirstName,
                        mailPiece.RcptLastName == null ? string.Empty : mailPiece.RcptLastName,
                        mailPiece.RcptAddressStreet == null ? string.Empty : mailPiece.RcptAddressStreet,
                        mailPiece.RcptAddressCity == null ? string.Empty : mailPiece.RcptAddressCity,
                        mailPiece.RcptAddressState == null ? string.Empty : mailPiece.RcptAddressState,
                        mailPiece.RcptAddressZIP == null ? string.Empty : mailPiece.RcptAddressZIP,
                        mailPiece.ErrorCode,
                        mailPiece.ErrorDesc == null ? string.Empty : mailPiece.ErrorDesc.Replace(',', ' ').Replace("\r\n", " "),
                        data.MailPieceCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "ShippingMethod").FirstOrDefault() == null ? string.Empty : data.MailPieceCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "ShippingMethod").FirstOrDefault().Value,
                        data.MailPieceCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "CustomerIdentifier").FirstOrDefault() == null ? string.Empty : data.MailPieceCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "CustomerIdentifier").FirstOrDefault().Value,
                        data.MailPieceCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "RequestId").FirstOrDefault() == null ? string.Empty : data.MailPieceCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "RequestId").FirstOrDefault().Value));
            }

            return errorReport;
        }

        public static StringBuilder USAADailyDetailOperationReport(BatchData data)
        {
            StringBuilder deliveryReport = new StringBuilder("BatchID,ID,Channel,Package Name,Loan Number,File Name,Date Received,Date Printed,Date Mailed,Client Code,Tracking Number,Shipping Method,Customer Identifier,First Name,Last Name,Recipient Street Address,Recipient City,Recipient State,Recipient Zip,RequestID,Print Location,Pages Shipped");

            deliveryReport.AppendLine();

            // All the loans where outcome code is anything except no_error are processed invalid loans.
            foreach (MailPieceDTO mailPiece in data.MailPieces.Where(l => l.ShippingStatus == ShippingStatus.Shipped.ToString()))
            {
                deliveryReport.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21}",
                        data.MailBatch.BatchName == null ? string.Empty : data.MailBatch.BatchName,
                        mailPiece.UniqueId,
                        data.MailBatch.SFTChannelId,
                        mailPiece.DocumentOrPackageType == null ? string.Empty : mailPiece.DocumentOrPackageType,
                        mailPiece.BusinessFriendlyIdValue == null ? string.Empty : mailPiece.BusinessFriendlyIdValue,
                        mailPiece.FileName == null ? string.Empty : mailPiece.FileName,
                        data.MailBatch.ReceivedOn == null ? string.Empty : data.MailBatch.ReceivedOn.Value.UtcToEst().ToString(),
                        mailPiece.PrintedOn == null ? string.Empty : mailPiece.PrintedOn.Value.UtcToEst().ToString(),
                        mailPiece.ShippedOn == null ? string.Empty : mailPiece.ShippedOn.Value.UtcToEst().ToString(),
                        data.MailPieceCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "ShippingCredCode").FirstOrDefault() == null ? string.Empty : data.MailPieceCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "ShippingCredCode").FirstOrDefault().Value,
                        mailPiece.TrackingNo == null ? string.Empty : mailPiece.TrackingNo,
                        data.MailPieceCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "ShippingMethod").FirstOrDefault() == null ? string.Empty : data.MailPieceCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "ShippingMethod").FirstOrDefault().Value,
                        data.MailPieceCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "CustomerIdentifier").FirstOrDefault() == null ? string.Empty : data.MailPieceCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "CustomerIdentifier").FirstOrDefault().Value,
                        mailPiece.RcptFirstName == null ? string.Empty : mailPiece.RcptFirstName,
                        mailPiece.RcptLastName == null ? string.Empty : mailPiece.RcptLastName,
                        mailPiece.RcptAddressStreet == null ? string.Empty : mailPiece.RcptAddressStreet,
                        mailPiece.RcptAddressCity == null ? string.Empty : mailPiece.RcptAddressCity,
                        mailPiece.RcptAddressState == null ? string.Empty : mailPiece.RcptAddressState,
                        mailPiece.RcptAddressZIP == null ? string.Empty : mailPiece.RcptAddressZIP,
                        data.MailPieceCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "RequestId").FirstOrDefault() == null ? string.Empty : data.MailPieceCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "RequestId").FirstOrDefault().Value,
                        data.MailBatch.PrintLocation,
                        mailPiece.TotalSheets));
            }

            return deliveryReport;
        }
    }


}
