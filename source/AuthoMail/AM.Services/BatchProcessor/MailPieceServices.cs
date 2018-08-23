using AM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
namespace AM.Services
{
    public interface IMailPieceService : IDisposable
    {
        int InsertBatchMailPieceDetails(MailPieceDTO mailPiece, List<MailPieceCustomInfo> mailPieceCustomInfo);
        List<MailPieceDTO> GetMailPiecesByMailBatchId(int mailBatchId);
        List<MailPieceDTO> GetMailPiecesByPrintBatchId(int printBatchId);
        int InsertBatchMailPieceCustomDetails(List<MailPieceCustomInfo> mailPieceCustomInfos);
        int UpdateBatchMailPieceDetails(MailPieceDTO mailPiece);
        int UpdateBatchMailPieceShippingStatusDetails(MailPieceDTO mailPiece);
        int UpdateMailPiecePickedUpDetailsFromTrackingNo(FedExPickedPackagesDetail pickedUpDetail);
        MailPieceDTO GetMailPieceDataFromTrackingNo(string trackingNo);
    }
    public class MailPieceService : ServiceBase, IMailPieceService //It implements Interfaces 
    {
        public int InsertBatchMailPieceDetails(MailPieceDTO mailPiece, List<MailPieceCustomInfo> mailPieceCustomInfo)
        {
            string query = "INSERT INTO [AM].[MailPieces](ClientId, UniqueId, MailBatchId, PrintBatchId, BusinessFriendlyIdName, BusinessFriendlyIdValue, FileName, FilePattern, DocumentOrPackageType, DueDate, ShippingStatus, IsRcptInfoSpecified, RcptPrefix, RcptFirstName, RcptMiddleName, RcptLastName, RcptSuffix, RcptPhone, RcptOrganization, CoRcptPrefix, CoRcptFirstName, CoRcptMiddleName, CoRcptLastName, CoRcptSuffix, CoRcptPhone, CoRcptOrganization, RcptAddrOptionalLine1AboveStreet, RcptAddrOptionalLine2AboveStreet, RcptAddressStreet, RcptAddressAptOrSuiteOrOther, RcptAddrOptionalLine1BelowStreet, RcptAddrOptionalLine2BelowStreet, RcptAddressCity, RcptAddressState, RcptAddressZIP, IsReturnInfoSpecified, RetRcptPrefix, RetRcptFirstName, RetRcptMiddleName, RetRcptLastName, RetRcptSuffix, RetRcptPhone, RetRcptOrganization, RetRcptAddrOptionalLine1AboveStreet, RetRcptAddrOptionalLine2AboveStreet, RetRcptAddressStreet, RetRcptAddressAptOrSuiteOrOther, RetRcptAddrOptionalLine1BelowStreet, RetRcptAddrOptionalLine2BelowStreet, RetRcptAddressCity, RetRcptAddressState, RetRcptAddressZIP, TotalPages, TotalSheets, ErrorCode, ErrorDesc, TrackingNo, PullBackRequestedOn, PulledBackOn,PrintedOn, ShippedOn, ShippingCost, Weight, IsQCRequested, IsQCPerformed, DocTypeId, PrintLocation) Output Inserted.Id VALUES (@ClientId, @UniqueId, @MailBatchId, @PrintBatchId, @BusinessFriendlyIdName, @BusinessFriendlyIdValue, @FileName, @FilePattern, @DocumentOrPackageType, @DueDate, @ShippingStatus, @IsRcptInfoSpecified, @RcptPrefix, @RcptFirstName, @RcptMiddleName, @RcptLastName, @RcptSuffix, @RcptPhone, @RcptOrganization, @CoRcptPrefix, @CoRcptFirstName, @CoRcptMiddleName, @CoRcptLastName, @CoRcptSuffix, @CoRcptPhone, @CoRcptOrganization, @RcptAddrOptionalLine1AboveStreet, @RcptAddrOptionalLine2AboveStreet, @RcptAddressStreet, @RcptAddressAptOrSuiteOrOther, @RcptAddrOptionalLine1BelowStreet, @RcptAddrOptionalLine2BelowStreet, @RcptAddressCity, @RcptAddressState, @RcptAddressZIP, @IsReturnInfoSpecified, @RetRcptPrefix, @RetRcptFirstName, @RetRcptMiddleName, @RetRcptLastName, @RetRcptSuffix, @RetRcptPhone, @RetRcptOrganization, @RetRcptAddrOptionalLine1AboveStreet, @RetRcptAddrOptionalLine2AboveStreet, @RetRcptAddressStreet, @RetRcptAddressAptOrSuiteOrOther, @RetRcptAddrOptionalLine1BelowStreet, @RetRcptAddrOptionalLine2BelowStreet, @RetRcptAddressCity, @RetRcptAddressState, @RetRcptAddressZIP, @TotalPages, @TotalSheets, @ErrorCode, @ErrorDesc, @TrackingNo, @PullBackRequestedOn, @PulledBackOn, @PrintedOn, @ShippedOn, @ShippingCost, @Weight, @IsQCRequested, @IsQCPerformed, @DocTypeId, @PrintLocation)";

            int mailPieceId = db.ExecuteScalar<int>(query, mailPiece);

            mailPieceCustomInfo.ForEach(c =>
            {
                c.MailPieceId = mailPieceId;
            });

            InsertBatchMailPieceCustomDetails(mailPieceCustomInfo);

            return mailPieceId;
        }

        public List<MailPieceDTO> GetMailPiecesByMailBatchId(int mailBatchId)
        {
            string query = "SELECT * FROM [AM].[MailPieces] WHERE MailBatchId=@mailBatchId";

            return db.Query<MailPieceDTO>(query, new { MailBatchId = mailBatchId }).ToList();
        }

        public List<MailPieceDTO> GetMailPiecesByPrintBatchId(int printBatchId)
        {
            string query = "SELECT * FROM [AM].[MailPieces] WHERE PrintBatchId=@PrintBatchId";

            return db.Query<MailPieceDTO>(query, new { PrintBatchId = printBatchId }).ToList();
        }

        public int InsertBatchMailPieceCustomDetails(List<MailPieceCustomInfo> mailPieceCustomInfos)
        {
            string query = "INSERT INTO [AM].[MailPieceCustomInfo](MailPieceId, [Key],[DataType], [Value]) VALUES (@MailPieceId, @Key, @DataType ,@Value)";

            return db.Execute(query, mailPieceCustomInfos);
        }

        public int UpdateBatchMailPieceDetails(MailPieceDTO mailPiece)
        {
            string query = @"UPDATE [AM].[MailPieces] SET [DueDate] = @DueDate,[ShippingStatus] = @ShippingStatus,[TotalPages] = @TotalPages,[ErrorCode] = @ErrorCode,[ErrorDesc] = @ErrorDesc,[TrackingNo] = @TrackingNo,[PrintLocation] = @PrintLocation WHERE Id=@Id";

            return db.Execute(query, mailPiece);
        }

        public int UpdateBatchMailPieceShippingStatusDetails(MailPieceDTO mailPiece)
        {
            string query = @"UPDATE [AM].[MailPieces] SET [ShippingStatus] = @ShippingStatus,[TotalSheets] = @TotalSheets,[ErrorCode] = @ErrorCode,[ErrorDesc] = @ErrorDesc,[PulledBackOn] = @PulledBackOn,[PrintedOn] = @PrintedOn,[ShippedOn] = @ShippedOn,[ShippingCost] = @ShippingCost,[Weight] = @Weight,[IsQCPerformed] = @IsQCPerformed, [TrackingNo] = @TrackingNo WHERE Id=@Id";

            var data = db.Execute(query, mailPiece);

            //Update data if any pulled back
            if (mailPiece.IsPullBackRequested == true)
            {
                UpdatePulledBackDetails(mailPiece.MailBatchId, mailPiece.Id, mailPiece.ShippingStatus);
            }
            return data;
        }

        public int UpdatePulledBackDetails(int mailBatchId, int mailPieceId, string shippingStatus)
        {
            string query = @"UPDATE [AM].[PullBackRequests] SET [Status] = @shippingStatus WHERE MailBatchId=@mailBatchId AND MailPieceId=@mailPieceId";

            return db.Execute(query, new { mailBatchId = mailBatchId, mailPieceId = mailPieceId, shippingStatus = shippingStatus });
        }

        public int UpdateMailPiecePickedUpDetailsFromTrackingNo(FedExPickedPackagesDetail pickedUpDetail)
        {
            string query = @"UPDATE [AM].[MailPieces] SET [PickedUpOn] = @PickedTimeStamp WHERE [TrackingNo] = @TrackingNo";

            return db.ExecuteScalar<int>(query, pickedUpDetail);
        }

        public MailPieceDTO GetMailPieceDataFromTrackingNo(string trackingNo)
        {
            string query = @"SELECT TOP 1 * FROM [AM].[MailPieces] WHERE [TrackingNo] = @TrackingNo";

            return db.ExecuteScalar<MailPieceDTO>(query, trackingNo);
        }
    }
}
