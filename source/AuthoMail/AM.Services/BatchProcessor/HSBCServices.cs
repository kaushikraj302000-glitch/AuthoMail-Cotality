using AM.Entities;
using AM.Lib;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public interface IHSBCService : IDisposable
    {
        StringBuilder HSBCErrorReport(BatchData data, string clientTimeZone, string delimeter);
        StringBuilder HSBCDeliveryReport(BatchData data, string clientTimeZone, string delimeter);
        List<HSBCConsolidateReport> ConsolidatedReport(int clientId, DateTime startReceivedOnDate, DateTime endReceivedOnDate);
    }

    public class HSBCService : ServiceBase, IHSBCService
    {
        public StringBuilder HSBCErrorReport(BatchData data, string clientTimeZone, string delimeter)
        {
            var columnName = String.Join(delimeter, "BatchID", "StatusID", "Loan Number", "File Name", "Package Name", "Date Received", "Recipient FirstName", "Recipient LastName", "Recipient Street Address", "Recipient City", "Recipient State", "Recipient Zip", "Error Code", "Error Text", "Total Postage Cost");

            StringBuilder errorReport = new StringBuilder(columnName);

            errorReport.AppendLine();

            var erroredData = data.MailPieces.Where(l => l.ErrorCode != OutComeCodes.No_Error).ToList();

            // All the loans where outcome code is anything except no_error are processed invalid loans.
            foreach (MailPieceDTO mailPiece in erroredData)
            {
                var statusID = data.MailPieceCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "StatusID").SingleOrDefault();

                var line = String.Join(delimeter,
                    data.MailBatch.BatchName == null ? string.Empty : data.MailBatch.BatchName + ".xml",
                    statusID == null ? string.Empty : statusID.Value,
                        mailPiece.BusinessFriendlyIdValue == null ? string.Empty : mailPiece.BusinessFriendlyIdValue,
                        mailPiece.FileName == null ? string.Empty : mailPiece.FileName,
                        mailPiece.FilePattern == null ? string.Empty : mailPiece.FilePattern.ToCsvString(),
                        data.MailBatch.ReceivedOn == null ? string.Empty : data.MailBatch.ReceivedOn.Value.UtcToClientTimeZone(clientTimeZone).ToString(),
                        mailPiece.RcptFirstName == null ? string.Empty : mailPiece.RcptFirstName.ToCsvString(),
                        mailPiece.RcptLastName == null ? string.Empty : mailPiece.RcptLastName.ToCsvString(),
                        mailPiece.RcptAddressStreet == null ? string.Empty : mailPiece.RcptAddressStreet.Replace(",", "").ToCsvString(),
                        mailPiece.RcptAddressCity == null ? string.Empty : mailPiece.RcptAddressCity.ToCsvString(),
                        mailPiece.RcptAddressState == null ? string.Empty : mailPiece.RcptAddressState,
                        mailPiece.RcptAddressZIP == null ? string.Empty : mailPiece.RcptAddressZIP,
                        OutComeCodes.GetHSBCMappingCode(mailPiece.ErrorCode),
                        mailPiece.ErrorDesc == null ? string.Empty : mailPiece.ErrorDesc.Replace("\r\n", " ").ToCsvString(),
                        string.Empty
                    );
                errorReport.AppendLine(line);
            }

            return errorReport;
        }

        public StringBuilder HSBCDeliveryReport(BatchData data, string clientTimeZone, string delimeter)
        {
            var columnName = String.Join(delimeter, "BatchID", "StatusID", "Loan Number", "File Name", "Package Name", "Date Received", "Date Printed", "Date Mailed", "Recipient FirstName", "Recipient LastName", "Recipient Street Address", "Recipient City", "Recipient State", "Recipient Zip", "Pages Shipped", "Total Postage Cost");

            StringBuilder deliveryReport = new StringBuilder(columnName);

            deliveryReport.AppendLine();

            var shippedData = data.MailPieces.Where(l => l.ShippingStatus == ShippingStatus.Shipped.ToString() || l.ShippingStatus == ShippingStatus.PulledBack.ToString()).ToList();

            foreach (MailPieceDTO mailPiece in shippedData)
            {
                var statusID = data.MailPieceCustomInfo.Where(c => c.MailPieceId == mailPiece.Id && c.Key == "StatusID").SingleOrDefault();

                var line = String.Join(delimeter,
                    data.MailBatch.BatchName == null ? string.Empty : data.MailBatch.BatchName + ".xml",
                    statusID == null ? string.Empty : statusID.Value,
                        mailPiece.BusinessFriendlyIdValue == null ? string.Empty : mailPiece.BusinessFriendlyIdValue,
                        mailPiece.FileName == null ? string.Empty : mailPiece.FileName,
                         mailPiece.FilePattern == null ? string.Empty : mailPiece.FilePattern.ToCsvString(),
                        data.MailBatch.ReceivedOn == null ? string.Empty : data.MailBatch.ReceivedOn.Value.UtcToClientTimeZone(clientTimeZone).ToString(),
                        mailPiece.ShippingStatus == ShippingStatus.PulledBack.ToString() ? string.Empty : mailPiece.PrintedOn.Value.UtcToClientTimeZone(clientTimeZone).ToString(),
                        mailPiece.ShippingStatus == ShippingStatus.PulledBack.ToString() ? string.Empty : mailPiece.ShippedOn.Value.UtcToClientTimeZone(clientTimeZone).ToString(),
                        mailPiece.RcptFirstName == null ? string.Empty : mailPiece.RcptFirstName.ToCsvString(),
                        mailPiece.RcptLastName == null ? string.Empty : mailPiece.RcptLastName.ToCsvString(),
                        mailPiece.RcptAddressStreet == null ? string.Empty : mailPiece.RcptAddressStreet.Replace(",", "").ToCsvString(),
                        mailPiece.RcptAddressCity == null ? string.Empty : mailPiece.RcptAddressCity.ToCsvString(),
                        mailPiece.RcptAddressState == null ? string.Empty : mailPiece.RcptAddressState,
                        mailPiece.RcptAddressZIP == null ? string.Empty : mailPiece.RcptAddressZIP,
                        mailPiece.ShippingStatus == ShippingStatus.PulledBack.ToString() ? 0 : mailPiece.TotalPages,
                        mailPiece.ShippingStatus == ShippingStatus.PulledBack.ToString() ? 0 : mailPiece.TotalPages
                        );

                deliveryReport.AppendLine(line);
            }
            return deliveryReport;
        }

        public List<HSBCConsolidateReport> ConsolidatedReport(int clientId, DateTime startReceivedOnDate, DateTime endReceivedOnDate)
        {
            string query = @"SELECT a.*,b.* FROM 
            (
	            SELECT b.BatchName, p.DocumentOrPackageType ,p.BusinessFriendlyIdValue, p.[FileName], p.[FilePattern], b.ReceivedOn, p.PrintedOn 
	            ,p.ShippedOn ,p.TotalPages ,p.RcptFirstName,p.RcptLastName ,p.RcptAddressStreet ,p.RcptAddressCity,p.RcptAddressState 
	            ,p.RcptAddressZIP,p.Id AS [MailPieceId],p.ShippingCost
	            FROM AM.MailBatches b
	            INNER JOIN AM.MailPieces p on p.MailBatchId=b.Id
	            LEFT JOIN AM.DocTypes d on d.Id=p.DocTypeId
	            WHERE p.ErrorCode = 'No_Error' AND p.ShippingStatus='Shipped' AND b.ReceivedOn >= @StartReceivedOnDate AND b.ReceivedOn < @EndReceivedOnDate AND b.ClientId=@ClientId
            )a
            INNER JOIN 
            (
	            SELECT *
	            FROM 
	            (
		            SELECT 
			            [MailPieceId],[Key], [Value]
		            FROM AM.MailPieceCustomInfo
	            ) AS s
	            PIVOT
	            (
		            MAX([Value])
		            FOR [Key] IN ([StatusID])
	            )AS pvt
            )b on a.MailPieceId=b.MailPieceId";

            return db.Query<HSBCConsolidateReport>(query, new { ClientId = clientId, StartReceivedOnDate = startReceivedOnDate, EndReceivedOnDate = endReceivedOnDate }).ToList();
        }
    }
}
