using AM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AM.Lib;

namespace AM.Services
{
    public interface IMailPieceService : IDisposable
    {
        int InsertBatchMailPieceDetails(MailPieceDTO mailPiece, List<MailPieceCustomInfo> mailPieceCustomInfo, DateTime? receivedOn = null, DateTime? downloadedOn = null);
        List<MailPieceDTO> GetMailPiecesByMailBatchId(int mailBatchId);
        List<MailPieceDTO> GetMailPiecesByPrintBatchId(int printBatchId);
        int InsertBatchMailPieceCustomDetails(List<MailPieceCustomInfo> mailPieceCustomInfos);
        int UpdateBatchMailPieceDetails(MailPieceDTO mailPiece);
        int UpdateBatchMailPieceShippingStatusDetails(MailPieces mailPiece);
        int UpdateMailPiecePickedUpDetailsFromTrackingNo(FedExPickedPackagesDetail pickedUpDetail);
        MailPieceDTO GetMailPieceDataFromTrackingNo(string trackingNo);
        List<DallasMailPieceData> GetDallasMailPiecesByTimingsAndClient(string sweepName);
        BatchSummary GetBatchSummaryDetailByUniqueId(Guid uniqueId);
        MailPieceDTO GetMailPieceDataByUniqueIdAndMBId(Guid uniqueId, int mailBatchId);
    }
    public class MailPieceService : ServiceBase, IMailPieceService //It implements Interfaces 
    {
        public int InsertBatchMailPieceDetails(MailPieceDTO mailPiece, List<MailPieceCustomInfo> mailPieceCustomInfo, DateTime? receivedOn = null, DateTime? downloadedOn = null)
        {
            string query = "INSERT INTO [AM].[MailPieces](ClientId, UniqueId, ClientDocTypeId, MailBatchId, PrintBatchId, BusinessFriendlyIdName, BusinessFriendlyIdValue, FileName, FilePattern, DocumentOrPackageType, DueDate, ShippingStatus, IsRcptInfoSpecified, RcptPrefix, RcptFirstName, RcptMiddleName, RcptLastName, RcptSuffix, RcptPhone, RcptOrganization, CoRcptPrefix, CoRcptFirstName, CoRcptMiddleName, CoRcptLastName, CoRcptSuffix, CoRcptPhone, CoRcptOrganization, RcptAddrOptionalLine1AboveStreet, RcptAddrOptionalLine2AboveStreet, RcptAddressStreet, RcptAddressAptOrSuiteOrOther, RcptAddrOptionalLine1BelowStreet, RcptAddrOptionalLine2BelowStreet, RcptAddressCity, RcptAddressState, RcptAddressZIP, RcptAddressCountry, IsReturnInfoSpecified, RetRcptPrefix, RetRcptFirstName, RetRcptMiddleName, RetRcptLastName, RetRcptSuffix, RetRcptPhone, RetRcptOrganization, RetRcptAddrOptionalLine1AboveStreet, RetRcptAddrOptionalLine2AboveStreet, RetRcptAddressStreet, RetRcptAddressAptOrSuiteOrOther, RetRcptAddrOptionalLine1BelowStreet, RetRcptAddrOptionalLine2BelowStreet, RetRcptAddressCity, RetRcptAddressState, RetRcptAddressZIP,RetRcptAddressCountry, TotalPages, TotalSheets, ErrorCode, ErrorDesc, TrackingNo, PullBackRequestedOn, PulledBackOn,PrintedOn, ShippedOn, ShippingCost, Weight, IsQCRequested, IsQCPerformed, DocTypeId, PrintLocation, RcptAddressTypes, PDFFileSize, IsEDeliveryRequested, IsFallBackRequested, ExpiryDate, RcptEmailId) Output Inserted.Id VALUES (@ClientId, @UniqueId,@ClientDocTypeId, @MailBatchId, @PrintBatchId, @BusinessFriendlyIdName, @BusinessFriendlyIdValue, @FileName, @FilePattern, @DocumentOrPackageType, @DueDate, @ShippingStatus, @IsRcptInfoSpecified, @RcptPrefix, @RcptFirstName, @RcptMiddleName, @RcptLastName, @RcptSuffix, @RcptPhone, @RcptOrganization, @CoRcptPrefix, @CoRcptFirstName, @CoRcptMiddleName, @CoRcptLastName, @CoRcptSuffix, @CoRcptPhone, @CoRcptOrganization, @RcptAddrOptionalLine1AboveStreet, @RcptAddrOptionalLine2AboveStreet, @RcptAddressStreet, @RcptAddressAptOrSuiteOrOther, @RcptAddrOptionalLine1BelowStreet, @RcptAddrOptionalLine2BelowStreet, @RcptAddressCity, @RcptAddressState, @RcptAddressZIP, @RcptAddressCountry, @IsReturnInfoSpecified, @RetRcptPrefix, @RetRcptFirstName, @RetRcptMiddleName, @RetRcptLastName, @RetRcptSuffix, @RetRcptPhone, @RetRcptOrganization, @RetRcptAddrOptionalLine1AboveStreet, @RetRcptAddrOptionalLine2AboveStreet, @RetRcptAddressStreet, @RetRcptAddressAptOrSuiteOrOther, @RetRcptAddrOptionalLine1BelowStreet, @RetRcptAddrOptionalLine2BelowStreet, @RetRcptAddressCity, @RetRcptAddressState, @RetRcptAddressZIP, @RetRcptAddressCountry, 0, 0, @ErrorCode, @ErrorDesc, @TrackingNo, @PullBackRequestedOn, @PulledBackOn, @PrintedOn, @ShippedOn, @ShippingCost, @Weight, @IsQCRequested, @IsQCPerformed, @DocTypeId, @PrintLocation, @RcptAddressTypes, @PDFFileSize, @IsEDeliveryRequested, @IsFallBackRequested, @ExpiryDate, @RcptEmailId)";

            int mailPieceId = db.ExecuteScalar<int>(query, mailPiece);

            mailPieceCustomInfo.ForEach(c =>
            {
                c.MailPieceId = mailPieceId;
            });

            InsertBatchMailPieceCustomDetails(mailPieceCustomInfo);

            if (mailPiece.IsEDeliveryRequested && mailPiece.ShippingStatus != ShippingStatus.Error.ToString())
            {
                InsertMailPieceEDeliveryDetails(mailPiece, receivedOn, downloadedOn);
            }

            return mailPieceId;
        }

        private int InsertMailPieceEDeliveryDetails(MailPieceDTO mailPiece, DateTime? receivedOn, DateTime? downloadedOn)
        {
            string query = "INSERT INTO [AM].[EDeliveryMailPieces]([UniqueId],[MailBatchId],[ClientId],[FileName],[ExpiryDate],[ReceivedOn],[DownloadedOn],[TotalPages],[PipelineStatus]) Output Inserted.Id VALUES (@UniqueId,@MailBatchId,@ClientId,@FileName,@ExpiryDate,@ReceivedOn,@DownloadedOn,@TotalPages,@PipelineStatus, @IsExpired)";

            return db.ExecuteScalar<int>(query, new { UniqueId = mailPiece.UniqueId, MailBatchId = mailPiece.MailBatchId, ClientId = mailPiece.ClientId, FileName = mailPiece.FileName, ExpiryDate = mailPiece.ExpiryDate, ReceivedOn = receivedOn.Value, DownloadedOn = downloadedOn.Value, TotalPages = mailPiece.TotalPages, PipelineStatus = EDPipelineStatus.Downloaded });
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
            string query = @"UPDATE [AM].[MailPieces] SET [DocTypeId] = @DocTypeId, [EffectiveReceivedOnEst] = @EffectiveReceivedOnEst, RcptAddressTypes=@RcptAddressTypes, [DueDate] = @DueDate,[ShippingStatus] = @ShippingStatus,[TotalPages] = @TotalPages,[TotalSheets] = @TotalSheets,[ErrorCode] = @ErrorCode,[ErrorDesc] = @ErrorDesc,[TrackingNo] = @TrackingNo,[PrintLocation] = @PrintLocation, [StdSubmissionTime] = @StdSubmissionTime, [MaxSubmissionTime] = @MaxSubmissionTime, [PrintTrailId] = @PrintTrailId, [RetRcptFirstName]= @RetRcptFirstName, [RetRcptLastName]= @RetRcptLastName, [RetRcptPhone] = @RetRcptPhone, [RetRcptAddressStreet] = @RetRcptAddressStreet, [RetRcptAddrOptionalLine1BelowStreet] = @RetRcptAddrOptionalLine1BelowStreet, [RetRcptAddressCity] = @RetRcptAddressCity, [RetRcptAddressState] = @RetRcptAddressState, [RetRcptAddressZIP] = @RetRcptAddressZIP, [ShippingCost] = @ShippingCost WHERE Id=@Id";

            return db.Execute(query, mailPiece);
        }

        public int UpdateBatchMailPieceShippingStatusDetails(MailPieces mailPiece)
        {
            string query = @"UPDATE [AM].[MailPieces] SET [ShippingStatus] = @ShippingStatus,[ErrorCode] = @ErrorCode,[ErrorDesc] = @ErrorDesc,[PulledBackOn] = @PulledBackOn,[PrintedOn] = @PrintedOn,[ShippedOn] = @ShippedOn,[ShippingCost] = @ShippingCost,[Weight] = @Weight,[IsQCPerformed] = @IsQCPerformed, [MissedSLAReason] = @MissedSLAReason WHERE Id=@Id";

            var data = db.Execute(query, mailPiece);

            //Update data if any pulled back
            if (mailPiece.IsPullBackRequested == 1)
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
            string query = @"UPDATE [AM].[MailPieces] SET [PickedUpOn] = @PickedTimeStamp WHERE Id = (SELECT TOP 1 Id FROM [AM].[MailPieces] WHERE [TrackingNo] = @TrackingNo ORDER BY EffectiveReceivedOnEst DESC)";

            return db.ExecuteScalar<int>(query, pickedUpDetail);
        }

        public MailPieceDTO GetMailPieceDataFromTrackingNo(string trackingNo)
        {
            string query = @"SELECT TOP 1 * FROM [AM].[MailPieces] WHERE [TrackingNo] = @TrackingNo";

            return db.ExecuteScalar<MailPieceDTO>(query, trackingNo);
        }

        public List<DallasMailPieceData> GetDallasMailPiecesByTimingsAndClient(string batchName)
        {
            string query = @"SELECT * FROM [dbo].[ufn_GetDallasBatchDetails](@EstDate) WHERE BatchName=@BatchName";

            return db.Query<DallasMailPieceData>(query, new { BatchName = batchName, EstDate = Common.EstToday }).ToList();
        }

        public BatchSummary GetBatchSummaryDetailByUniqueId(Guid uniqueId)
        {
            string query = @"SELECT * FROM [PnS].[BatchSummary] WHERE PrintBatch = (SELECT JobId FROM [PnS].[MRDFRequest] WHERE UniqueId=@UniqueId)";

            return db.QuerySingle<BatchSummary>(query, new { UniqueId = uniqueId });
        }

        public MailPieceDTO GetMailPieceDataByUniqueIdAndMBId(Guid uniqueId, int mailBatchId)
        {
            string query = @"SELECT TOP 1 * FROM [AM].[MailPieces] WHERE [UniqueId] = @UniqueId AND MailBatchId = @MailBatchId";

            return db.ExecuteScalar<MailPieceDTO>(query, new { UniqueId = uniqueId, MailBatchId = mailBatchId });
        }
    }
}
