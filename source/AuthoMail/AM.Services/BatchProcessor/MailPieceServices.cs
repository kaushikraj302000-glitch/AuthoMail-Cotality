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
        int InsertBatchMailPieceDetails(MailPiece mailPiece, int clientId, int mailBatchId, string errorCode, string errorDesc);
        int InsertBatchMailPieceCustomDetails(List<Metadata> mailPieceCustomInfos, int mailPieceId);
        List<MailPieceDTO> GetMailPiecesForUATBridge();
        List<MailPieceCustomInfo> GetMailPieceCustomInfos(int mailPieceId);
    }
    public class MailPieceService : ServiceBase, IMailPieceService //It implements Interfaces 
    {
        public int InsertBatchMailPieceDetails(MailPieceDTO mailPiece, List<MailPieceCustomInfo> mailPieceCustomInfo, DateTime? receivedOn = null, DateTime? downloadedOn = null)
        {
            string query = "INSERT INTO [AM].[MailPieces](ClientId, UniqueId, PnSConfigDetailId, MailBatchId, PrintBatchId, BusinessFriendlyIdName, BusinessFriendlyIdValue, FileName, FilePattern, DocumentOrPackageType, DueDate, ShippingStatus, IsRcptInfoSpecified, RcptPrefix, RcptFirstName, RcptMiddleName, RcptLastName, RcptSuffix, RcptPhone, RcptOrganization, CoRcptPrefix, CoRcptFirstName, CoRcptMiddleName, CoRcptLastName, CoRcptSuffix, CoRcptPhone, CoRcptOrganization, RcptAddrOptionalLine1AboveStreet, RcptAddrOptionalLine2AboveStreet, RcptAddressStreet, RcptAddressAptOrSuiteOrOther, RcptAddrOptionalLine1BelowStreet, RcptAddrOptionalLine2BelowStreet, RcptAddressCity, RcptAddressState, RcptAddressZIP, RcptAddressCountry, IsReturnInfoSpecified, RetRcptPrefix, RetRcptFirstName, RetRcptMiddleName, RetRcptLastName, RetRcptSuffix, RetRcptPhone, RetRcptOrganization, RetRcptAddrOptionalLine1AboveStreet, RetRcptAddrOptionalLine2AboveStreet, RetRcptAddressStreet, RetRcptAddressAptOrSuiteOrOther, RetRcptAddrOptionalLine1BelowStreet, RetRcptAddrOptionalLine2BelowStreet, RetRcptAddressCity, RetRcptAddressState, RetRcptAddressZIP,RetRcptAddressCountry, TotalPages, TotalSheets, ErrorCode, ErrorDesc, TrackingNo, PullBackRequestedOn, PulledBackOn,PrintedOn, ShippedOn, ShippingCost, Weight, IsQCRequested, IsQCPerformed, DocTypeId, PrintLocation, RcptAddressTypes, PDFFileSize, IsEDeliveryRequested, IsFallBackRequested, ExpiryDate, RcptEmailId) Output Inserted.Id VALUES (@ClientId, @UniqueId,@PnSConfigDetailId, @MailBatchId, @PrintBatchId, @BusinessFriendlyIdName, @BusinessFriendlyIdValue, @FileName, @FilePattern, @DocumentOrPackageType, @DueDate, @ShippingStatus, @IsRcptInfoSpecified, @RcptPrefix, @RcptFirstName, @RcptMiddleName, @RcptLastName, @RcptSuffix, @RcptPhone, @RcptOrganization, @CoRcptPrefix, @CoRcptFirstName, @CoRcptMiddleName, @CoRcptLastName, @CoRcptSuffix, @CoRcptPhone, @CoRcptOrganization, @RcptAddrOptionalLine1AboveStreet, @RcptAddrOptionalLine2AboveStreet, @RcptAddressStreet, @RcptAddressAptOrSuiteOrOther, @RcptAddrOptionalLine1BelowStreet, @RcptAddrOptionalLine2BelowStreet, @RcptAddressCity, @RcptAddressState, @RcptAddressZIP, @RcptAddressCountry, @IsReturnInfoSpecified, @RetRcptPrefix, @RetRcptFirstName, @RetRcptMiddleName, @RetRcptLastName, @RetRcptSuffix, @RetRcptPhone, @RetRcptOrganization, @RetRcptAddrOptionalLine1AboveStreet, @RetRcptAddrOptionalLine2AboveStreet, @RetRcptAddressStreet, @RetRcptAddressAptOrSuiteOrOther, @RetRcptAddrOptionalLine1BelowStreet, @RetRcptAddrOptionalLine2BelowStreet, @RetRcptAddressCity, @RetRcptAddressState, @RetRcptAddressZIP, @RetRcptAddressCountry, 0, 0, @ErrorCode, @ErrorDesc, @TrackingNo, @PullBackRequestedOn, @PulledBackOn, @PrintedOn, @ShippedOn, @ShippingCost, @Weight, @IsQCRequested, @IsQCPerformed, @DocTypeId, @PrintLocation, @RcptAddressTypes, @PDFFileSize, @IsEDeliveryRequested, @IsFallBackRequested, @ExpiryDate, @RcptEmailId)";

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
            string query = "INSERT INTO [AM].[EDeliveryMailPieces]([UniqueId],[MailBatchId],[ClientId],[ReceivedOn],[DownloadedOn],[TotalPages],[PipelineStatus]) Output Inserted.Id VALUES (@UniqueId,@MailBatchId,@ClientId,@ReceivedOn,@DownloadedOn,@TotalPages,@PipelineStatus)";

            return db.ExecuteScalar<int>(query, new { UniqueId = mailPiece.UniqueId, MailBatchId = mailPiece.MailBatchId, ClientId = mailPiece.ClientId, ReceivedOn = receivedOn.Value, DownloadedOn = downloadedOn.Value, TotalPages = mailPiece.TotalPages, PipelineStatus = EDPipelineStatus.Downloaded });
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
            string query = @"UPDATE [AM].[MailPieces] SET [DocTypeId] = @DocTypeId,[PnSConfigDetailId] = @PnSConfigDetailId, [EffectiveReceivedOnEst] = @EffectiveReceivedOnEst, RcptAddressTypes=@RcptAddressTypes, [DueDate] = @DueDate,[ShippingStatus] = @ShippingStatus,[TotalPages] = @TotalPages,[TotalSheets] = @TotalSheets,[ErrorCode] = @ErrorCode,[ErrorDesc] = @ErrorDesc,[TrackingNo] = @TrackingNo,[PrintLocation] = @PrintLocation, [StdSubmissionTime] = @StdSubmissionTime, [MaxSubmissionTime] = @MaxSubmissionTime, [PrintTrailId] = @PrintTrailId, [RetRcptFirstName]= @RetRcptFirstName, [RetRcptLastName]= @RetRcptLastName, [RetRcptPhone] = @RetRcptPhone, [RetRcptAddressStreet] = @RetRcptAddressStreet, [RetRcptAddrOptionalLine1BelowStreet] = @RetRcptAddrOptionalLine1BelowStreet, [RetRcptAddressCity] = @RetRcptAddressCity, [RetRcptAddressState] = @RetRcptAddressState, [RetRcptAddressZIP] = @RetRcptAddressZIP, [ShippingCost] = @ShippingCost WHERE Id=@Id";

            return db.Execute(query, mailPiece);
        }

        public int UpdateBatchMailPieceShippingStatusDetails(MailPieces mailPiece)
        {
            string query = @"UPDATE [AM].[MailPieces] SET [ShippingStatus] = @ShippingStatus,[ErrorCode] = @ErrorCode,[ErrorDesc] = @ErrorDesc,[PulledBackOn] = @PulledBackOn,[PrintedOn] = @PrintedOn,[ShippedOn] = @ShippedOn,[ShippingCost] = @ShippingCost,[Weight] = @Weight,[IsQCPerformed] = @IsQCPerformed, [MissedSLAReason] = @MissedSLAReason, [TotalPages]=@TotalPages, [TotalSheets]=@TotalSheets WHERE Id=@Id";

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

            return db.QuerySingleOrDefault<MailPieceDTO>(query, new { UniqueId = uniqueId, MailBatchId = mailBatchId });
        }

        public int InsertBatchMailPieceDetails(MailPiece mailPiece, int clientId, int mailBatchId, string errorCode, string errorDesc)
        {
            string query = "INSERT INTO [AM].[MailPieces](ClientId, UniqueId, MailBatchId,BusinessFriendlyIdName, BusinessFriendlyIdValue, FileName, DocumentOrPackageType, ShippingStatus, RcptPrefix, RcptFirstName, RcptMiddleName, RcptLastName, RcptSuffix, RcptPhone, RcptOrganization, RcptAddrOptionalLine1AboveStreet, RcptAddrOptionalLine2AboveStreet, RcptAddressStreet,RcptAddressAptOrSuiteOrOther, RcptAddrOptionalLine1BelowStreet, RcptAddrOptionalLine2BelowStreet, RcptAddressCity, RcptAddressState, RcptAddressZIP, RcptAddressCountry, RetRcptPrefix, RetRcptFirstName, RetRcptMiddleName, RetRcptLastName, RetRcptSuffix, RetRcptPhone, RetRcptOrganization, RetRcptAddrOptionalLine1AboveStreet, RetRcptAddrOptionalLine2AboveStreet, RetRcptAddressStreet, RetRcptAddressAptOrSuiteOrOther, RetRcptAddrOptionalLine1BelowStreet, RetRcptAddrOptionalLine2BelowStreet, RetRcptAddressCity, RetRcptAddressState, RetRcptAddressZIP,RetRcptAddressCountry,TotalPages, TotalSheets, ErrorCode, ErrorDesc,RcptEmailId,PullBackRequestedOn, PulledBackOn,PrintedOn, ShippedOn,PickedUpOn) Output Inserted.Id VALUES (@ClientId, @UniqueId,@MailBatchId, @BusinessFriendlyIdName, @BusinessFriendlyIdValue, @FileName, @DocumentOrPackageType,@ShippingStatus, @RcptPrefix, @RcptFirstName, @RcptMiddleName, @RcptLastName, @RcptSuffix, @RcptPhone, @RcptOrganization,  @RcptAddrOptionalLine2AboveStreet, @RcptAddressStreet,@RcptAddressAptOrSuiteOrOther, @RcptAddressAptOrSuiteOrOther, @RcptAddrOptionalLine1BelowStreet, @RcptAddrOptionalLine2BelowStreet, @RcptAddressCity, @RcptAddressState, @RcptAddressZIP, @RcptAddressCountry, @RetRcptPrefix, @RetRcptFirstName, @RetRcptMiddleName, @RetRcptLastName, @RetRcptSuffix, @RetRcptPhone, @RetRcptOrganization, @RetRcptAddrOptionalLine1AboveStreet, @RetRcptAddrOptionalLine2AboveStreet, @RetRcptAddressStreet, @RetRcptAddressAptOrSuiteOrOther, @RetRcptAddrOptionalLine1BelowStreet, @RetRcptAddrOptionalLine2BelowStreet, @RetRcptAddressCity, @RetRcptAddressState, @RetRcptAddressZIP, @RetRcptAddressCountry, 0, 0,@ErrorCode, @ErrorDesc,@RcptEmailId,@PullBackRequestedOn, @PulledBackOn,@PrintedOn, @ShippedOn,@PickedUpOn)";

            int mailPieceId = db.ExecuteScalar<int>(query, new { ClientId = clientId, UniqueId = mailPiece.Id, MailBatchId = mailBatchId, BusinessFriendlyIdName = mailPiece.BusinessFriendlyIdName == null ? DBNull.Value.ToString() : mailPiece.BusinessFriendlyIdName, BusinessFriendlyIdValue = mailPiece.BusinessFriendlyIdValue == null ? DBNull.Value.ToString() : mailPiece.BusinessFriendlyIdValue, FileName = mailPiece.DocumentOrPackage.Name == null ? DBNull.Value.ToString() : mailPiece.DocumentOrPackage.Name, DocumentOrPackageType = mailPiece.DocumentOrPackageType == null ? DBNull.Value.ToString() : mailPiece.DocumentOrPackageType, ShippingStatus = ShippingStatus.Error.ToString(), RcptPrefix = mailPiece.RecipientInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.Prefix == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.Prefix, RcptFirstName = mailPiece.RecipientInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.FirstName == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.FirstName, RcptMiddleName = mailPiece.RecipientInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.MiddleName == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.MiddleName, RcptLastName = mailPiece.RecipientInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.LastName == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.LastName, RcptSuffix = mailPiece.RecipientInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.Suffix == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.Suffix, RcptPhone = mailPiece.RecipientInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.Phone == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.Phone, RcptOrganization = mailPiece.RecipientInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.Organization == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.Organization, RcptAddrOptionalLine1AboveStreet = mailPiece.RecipientInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.OptionalLine1AboveStreet == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.OptionalLine1AboveStreet, RcptAddrOptionalLine2AboveStreet = mailPiece.RecipientInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.OptionalLine2AboveStreet == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.OptionalLine2AboveStreet, RcptAddressStreet = mailPiece.RecipientInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.Street == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.Street, RcptAddressAptOrSuiteOrOther = mailPiece.RecipientInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.AptOrSuiteOrOther == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.AptOrSuiteOrOther, RcptAddrOptionalLine1BelowStreet = mailPiece.RecipientInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.OptionalLine1BelowStreet == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.OptionalLine1BelowStreet, RcptAddrOptionalLine2BelowStreet = mailPiece.RecipientInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.OptionalLine2BelowStreet == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.OptionalLine2BelowStreet, RcptAddressCity = mailPiece.RecipientInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.City == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.City, RcptAddressState = mailPiece.RecipientInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.State == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.State, RcptAddressZIP = mailPiece.RecipientInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.Zip == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.Zip, RcptAddressCountry = mailPiece.RecipientInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.Country == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.Country, RetRcptPrefix = mailPiece.ReturnInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.Prefix, RetRcptFirstName = mailPiece.ReturnInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.FirstName, RetRcptMiddleName = mailPiece.ReturnInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.MiddleName, RetRcptLastName = mailPiece.ReturnInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.LastName, RetRcptSuffix = mailPiece.ReturnInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.Suffix, RetRcptPhone = mailPiece.ReturnInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.Phone, RetRcptOrganization = mailPiece.ReturnInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.Organization, RetRcptAddrOptionalLine1AboveStreet = mailPiece.ReturnInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.OptionalLine1AboveStreet, RetRcptAddrOptionalLine2AboveStreet = mailPiece.ReturnInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.OptionalLine2AboveStreet, RetRcptAddressStreet = mailPiece.ReturnInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.Street, RetRcptAddressAptOrSuiteOrOther = mailPiece.ReturnInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.AptOrSuiteOrOther, RetRcptAddrOptionalLine1BelowStreet = mailPiece.ReturnInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.OptionalLine1BelowStreet, RetRcptAddrOptionalLine2BelowStreet = mailPiece.ReturnInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.OptionalLine2BelowStreet, RetRcptAddressCity = mailPiece.ReturnInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.City, RetRcptAddressState = mailPiece.ReturnInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.State, RetRcptAddressZIP = mailPiece.ReturnInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.Zip, RetRcptAddressCountry = mailPiece.ReturnInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.Country, ErrorCode = errorCode, ErrorDesc = errorDesc, RcptEmailId = mailPiece.RecipientInfo.Recipient.EmailAddress == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.EmailAddress, PullBackRequestedOn = (DateTime?)null, PulledBackOn = (DateTime?)null, PrintedOn = (DateTime?)null, ShippedOn = (DateTime?)null, PickedUpOn = (DateTime?)null });

            if (mailPiece.CustomInfo != null)
            {
                InsertBatchMailPieceCustomDetails(mailPiece.CustomInfo, mailPieceId);
            }

            return mailPieceId;
        }

        public int InsertBatchMailPieceCustomDetails(List<Metadata> mailPieceCustomInfos, int mailPieceId)
        {
            string query = "INSERT INTO [AM].[MailPieceCustomInfo](MailPieceId, [Key],[DataType], [Value]) VALUES (@MailPieceId, @Key, @DataType ,@Value)";

            foreach (var mailPieceCustonInfo in mailPieceCustomInfos)
            {
                db.Execute(query, new { MailPieceId = mailPieceId, Key = mailPieceCustonInfo.Key, DataType = mailPieceCustonInfo.DataType, Value = mailPieceCustonInfo.Value });
            }
            return mailPieceId;
        }

        public List<MailPieceDTO> GetMailPiecesForUATBridge()
        {
            string query = @"SELECT M.*,B.ReceivedOn FROM AM.MailPieces M INNER JOIN AM.MailBatches B ON M.MailBatchId=B.Id WHERE M.ShippingStatus <> 'Error' AND B.ReceivedOn > ISNULL((SELECT TOP 1 LastProcessedOn FROM dbo.ProductionBridge ORDER BY Id DESC), DATEADD(DAY,-5,GETUTCDATE()))";

            var data = db.Query<MailPieceDTO>(query).ToList();
            return data;
        }

        public List<MailPieceCustomInfo> GetMailPieceCustomInfos(int mailPieceId)
        {
            string query = @"SELECT * FROM AM.MailPieceCustomInfo WHERE MailPieceId = @MailPieceId";

            var data = db.Query<MailPieceCustomInfo>(query, new { MailPieceId = mailPieceId }).ToList();
            return data;
        }
    }
}