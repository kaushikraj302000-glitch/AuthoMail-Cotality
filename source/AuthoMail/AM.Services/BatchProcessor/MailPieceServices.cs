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
        Task<int> InsertBatchMailPieceDetails(MailPiece mailPiece, int clientId, int mailBatchId, string errorCode, string errorDesc);
        Task<int> InsertBatchMailPieceCustomDetails(List<Metadata> mailPieceCustomInfos, int mailPieceId);
        List<MailPieceDTO> GetMailPiecesForUATBridge();
        List<MailPieceDTO> GetMailPiecesForUATBridge(DateTime utcStartDate, int clientId);
        List<MailPieceCustomInfo> GetMailPieceCustomInfos(int mailPieceId);
        BatchResponse GetMailPieceDetailsByMailPieceUniqueId(string uniqueId);
        List<MBInfo> GetReportedBackRecordsFromUATBridge();
        List<MBInfo> GetPnSReportedBackRecordsFromUATBridge();
        List<MBInfo> GetReportedBackAMRecordsFromUATBridge();
        List<MBInfo> GetReportedBackPnSRecordsFromUATBridge();
        int DeletePrintBatchDetails(int printBatchId);
        dynamic GetCoversheetData(int mailPieceId);
    }
    public class MailPieceService : ServiceBase, IMailPieceService //It implements Interfaces 
    {
        public int InsertBatchMailPieceDetails(MailPieceDTO mailPiece, List<MailPieceCustomInfo> mailPieceCustomInfo, DateTime? receivedOn = null, DateTime? downloadedOn = null)
        {
            string query = "INSERT INTO [AM].[MailPieces](ClientId, UniqueId, PnSConfigDetailId, MailBatchId, PrintBatchId, BusinessFriendlyIdName, BusinessFriendlyIdValue, FileName, FilePattern, DocumentOrPackageType, DueDate, ShippingStatus, IsRcptInfoSpecified, RcptPrefix, RcptFirstName, RcptMiddleName, RcptLastName, RcptSuffix, RcptPhone, RcptOrganization, CoRcptPrefix, CoRcptFirstName, CoRcptMiddleName, CoRcptLastName, CoRcptSuffix, CoRcptPhone, CoRcptOrganization, RcptAddrOptionalLine1AboveStreet, RcptAddrOptionalLine2AboveStreet, RcptAddressStreet, RcptAddressAptOrSuiteOrOther, RcptAddrOptionalLine1BelowStreet, RcptAddrOptionalLine2BelowStreet, RcptAddressCity, RcptAddressState, RcptAddressZIP, RcptAddressCountry, IsReturnInfoSpecified, RetRcptPrefix, RetRcptFirstName, RetRcptMiddleName, RetRcptLastName, RetRcptSuffix, RetRcptPhone, RetRcptOrganization, RetRcptAddrOptionalLine1AboveStreet, RetRcptAddrOptionalLine2AboveStreet, RetRcptAddressStreet, RetRcptAddressAptOrSuiteOrOther, RetRcptAddrOptionalLine1BelowStreet, RetRcptAddrOptionalLine2BelowStreet, RetRcptAddressCity, RetRcptAddressState, RetRcptAddressZIP,RetRcptAddressCountry, TotalPages, TotalSheets, ErrorCode, ErrorDesc, TrackingNo, PullBackRequestedOn, PulledBackOn,PrintedOn, ShippedOn, ShippingCost, Weight, IsQCRequested, IsQCPerformed, DocTypeId, PrintLocation, RcptAddressTypes, PDFFileSize, IsEDeliveryRequested, IsFallBackRequested, ExpiryDate, RcptEmailId, PrintTrailId, Inserts,TotalBlankPages,BlankPageNumbers) Output Inserted.Id VALUES (@ClientId, @UniqueId,@PnSConfigDetailId, @MailBatchId, @PrintBatchId, SUBSTRING(@BusinessFriendlyIdName,1,255), SUBSTRING(@BusinessFriendlyIdValue,1,255), SUBSTRING(@FileName,1,255), @FilePattern, SUBSTRING(@DocumentOrPackageType,1,255), @DueDate, @ShippingStatus, @IsRcptInfoSpecified, SUBSTRING(@RcptPrefix,1,255), SUBSTRING(@RcptFirstName,1,255), SUBSTRING(@RcptMiddleName,1,255), SUBSTRING(@RcptLastName,1,255), SUBSTRING(@RcptSuffix,1,255), SUBSTRING(@RcptPhone,1,255), SUBSTRING(@RcptOrganization,1,255), @CoRcptPrefix, @CoRcptFirstName, @CoRcptMiddleName, @CoRcptLastName, @CoRcptSuffix, @CoRcptPhone, @CoRcptOrganization, SUBSTRING(@RcptAddrOptionalLine1AboveStreet,1,255), SUBSTRING(@RcptAddrOptionalLine2AboveStreet,1,255), SUBSTRING(@RcptAddressStreet,1,255), SUBSTRING(@RcptAddressAptOrSuiteOrOther,1,255), SUBSTRING(@RcptAddrOptionalLine1BelowStreet,1,255), SUBSTRING(@RcptAddrOptionalLine2BelowStreet,1,255), SUBSTRING(@RcptAddressCity,1,255), SUBSTRING(@RcptAddressState,1,255), SUBSTRING(@RcptAddressZIP,1,255), SUBSTRING(@RcptAddressCountry,1,255), @IsReturnInfoSpecified, SUBSTRING(@RetRcptPrefix,1,255), SUBSTRING(@RetRcptFirstName,1,255), SUBSTRING(@RetRcptMiddleName,1,255), SUBSTRING(@RetRcptLastName,1,255), SUBSTRING(@RetRcptSuffix,1,255), SUBSTRING(@RetRcptPhone,1,255), SUBSTRING(@RetRcptOrganization,1,255), SUBSTRING(@RetRcptAddrOptionalLine1AboveStreet,1,255), SUBSTRING(@RetRcptAddrOptionalLine2AboveStreet,1,255), SUBSTRING(@RetRcptAddressStreet,1,255), SUBSTRING(@RetRcptAddressAptOrSuiteOrOther,1,255), SUBSTRING(@RetRcptAddrOptionalLine1BelowStreet,1,255), SUBSTRING(@RetRcptAddrOptionalLine2BelowStreet,1,255), SUBSTRING(@RetRcptAddressCity,1,255), SUBSTRING(@RetRcptAddressState,1,255), SUBSTRING(@RetRcptAddressZIP,1,255), SUBSTRING(@RetRcptAddressCountry,1,255), 0, 0, @ErrorCode, @ErrorDesc, @TrackingNo, @PullBackRequestedOn, @PulledBackOn, @PrintedOn, @ShippedOn, @ShippingCost, @Weight, @IsQCRequested, @IsQCPerformed, @DocTypeId, @PrintLocation, @RcptAddressTypes, @PDFFileSize, @IsEDeliveryRequested, @IsFallBackRequested, @ExpiryDate, SUBSTRING(@RcptEmailId,1,255), @PrintTrailId, @Inserts,@TotalBlankPages,@BlankPageNumbers)";

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
            string query = "INSERT INTO [AM].[MailPieceCustomInfo](MailPieceId, [Key],[DataType], [Value]) VALUES (@MailPieceId, SUBSTRING(@Key,1,128), SUBSTRING(@DataType,1,128) ,SUBSTRING(@Value,1,2048))";

            return db.Execute(query, mailPieceCustomInfos);
        }

        public int UpdateBatchMailPieceDetails(MailPieceDTO mailPiece)
        {
            string query = @"UPDATE [AM].[MailPieces] SET [DocTypeId] = @DocTypeId,[PnSConfigDetailId] = @PnSConfigDetailId, [EffectiveReceivedOnEst] = @EffectiveReceivedOnEst, RcptAddressTypes=@RcptAddressTypes, [DueDate] = @DueDate,[ShippingStatus] = @ShippingStatus,[TotalPages] = @TotalPages,[TotalSheets] = @TotalSheets,[ErrorCode] = @ErrorCode,[ErrorDesc] = @ErrorDesc,[TrackingNo] = @TrackingNo,[PrintLocation] = @PrintLocation, [StdSubmissionTime] = @StdSubmissionTime, [MaxSubmissionTime] = @MaxSubmissionTime, [PrintTrailId] = @PrintTrailId, [RetRcptFirstName]= @RetRcptFirstName, [RetRcptLastName]= @RetRcptLastName, [RetRcptPhone] = @RetRcptPhone, [RetRcptAddressStreet] = @RetRcptAddressStreet, [RetRcptAddrOptionalLine1BelowStreet] = @RetRcptAddrOptionalLine1BelowStreet, [RetRcptAddressCity] = @RetRcptAddressCity, [RetRcptAddressState] = @RetRcptAddressState, [RetRcptAddressZIP] = @RetRcptAddressZIP, [ShippingCost] = @ShippingCost, [ShipperId] = @ShipperId WHERE Id=@Id";

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

        public async Task<int> InsertBatchMailPieceDetails(MailPiece mailPiece, int clientId, int mailBatchId, string errorCode, string errorDesc)
        {
            string query = "INSERT INTO [AM].[MailPieces](ClientId, UniqueId, MailBatchId,BusinessFriendlyIdName, BusinessFriendlyIdValue, FileName,Inserts, DocumentOrPackageType, ShippingStatus, RcptPrefix, RcptFirstName, RcptMiddleName, RcptLastName, RcptSuffix, RcptPhone, RcptOrganization, RcptAddrOptionalLine1AboveStreet, RcptAddrOptionalLine2AboveStreet, RcptAddressStreet,RcptAddressAptOrSuiteOrOther, RcptAddrOptionalLine1BelowStreet, RcptAddrOptionalLine2BelowStreet, RcptAddressCity, RcptAddressState, RcptAddressZIP, RcptAddressCountry, RetRcptPrefix, RetRcptFirstName, RetRcptMiddleName, RetRcptLastName, RetRcptSuffix, RetRcptPhone, RetRcptOrganization, RetRcptAddrOptionalLine1AboveStreet, RetRcptAddrOptionalLine2AboveStreet, RetRcptAddressStreet, RetRcptAddressAptOrSuiteOrOther, RetRcptAddrOptionalLine1BelowStreet, RetRcptAddrOptionalLine2BelowStreet, RetRcptAddressCity, RetRcptAddressState, RetRcptAddressZIP,RetRcptAddressCountry,TotalPages, TotalSheets, ErrorCode, ErrorDesc,RcptEmailId,PullBackRequestedOn, PulledBackOn,PrintedOn, ShippedOn,PickedUpOn) Output Inserted.Id VALUES (@ClientId, @UniqueId,@MailBatchId, @BusinessFriendlyIdName, @BusinessFriendlyIdValue, @FileName,@Inserts, @DocumentOrPackageType,@ShippingStatus, @RcptPrefix, @RcptFirstName, @RcptMiddleName, @RcptLastName, @RcptSuffix, @RcptPhone, @RcptOrganization,@RcptAddrOptionalLine1AboveStreet, @RcptAddrOptionalLine2AboveStreet, @RcptAddressStreet,@RcptAddressAptOrSuiteOrOther, @RcptAddrOptionalLine1BelowStreet, @RcptAddrOptionalLine2BelowStreet, @RcptAddressCity, @RcptAddressState, @RcptAddressZIP, @RcptAddressCountry, @RetRcptPrefix, @RetRcptFirstName, @RetRcptMiddleName, @RetRcptLastName, @RetRcptSuffix, @RetRcptPhone, @RetRcptOrganization, @RetRcptAddrOptionalLine1AboveStreet, @RetRcptAddrOptionalLine2AboveStreet, @RetRcptAddressStreet, @RetRcptAddressAptOrSuiteOrOther, @RetRcptAddrOptionalLine1BelowStreet, @RetRcptAddrOptionalLine2BelowStreet, @RetRcptAddressCity, @RetRcptAddressState, @RetRcptAddressZIP, @RetRcptAddressCountry, 0, 0,@ErrorCode, @ErrorDesc,@RcptEmailId,@PullBackRequestedOn, @PulledBackOn,@PrintedOn, @ShippedOn,@PickedUpOn)";

            Guid mpId;
            if (errorCode == OutComeCodes.Invalid_DuplicateKey)
            {
                mpId = Guid.NewGuid();
            }
            else
            {
                mpId = mailPiece.Id;
            }

            int mailPieceId = await db.ExecuteScalarAsync<int>(query, new
            {
                ClientId = clientId,
                UniqueId = mpId,
                MailBatchId = mailBatchId,
                BusinessFriendlyIdName = mailPiece.BusinessFriendlyIdName == null ? DBNull.Value.ToString() : mailPiece.BusinessFriendlyIdName,
                BusinessFriendlyIdValue = mailPiece.BusinessFriendlyIdValue == null ? DBNull.Value.ToString() : mailPiece.BusinessFriendlyIdValue,
                FileName = mailPiece.DocumentOrPackage == null ? DBNull.Value.ToString() : mailPiece.DocumentOrPackage.Name == null ? DBNull.Value.ToString() : mailPiece.DocumentOrPackage.Name,
                Inserts = mailPiece.DocumentOrPackage == null ? DBNull.Value.ToString() : mailPiece.DocumentOrPackage.Inserts == null ? DBNull.Value.ToString() : mailPiece.DocumentOrPackage.Inserts,
                DocumentOrPackageType = mailPiece.DocumentOrPackageType == null ? DBNull.Value.ToString() : mailPiece.DocumentOrPackageType,
                ShippingStatus = ShippingStatus.Error.ToString(),
                RcptPrefix = mailPiece.RecipientInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.Prefix == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.Prefix,
                RcptFirstName = mailPiece.RecipientInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.FirstName == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.FirstName,
                RcptMiddleName = mailPiece.RecipientInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.MiddleName == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.MiddleName,
                RcptLastName = mailPiece.RecipientInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.LastName == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.LastName,
                RcptSuffix = mailPiece.RecipientInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.Suffix == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.Suffix,
                RcptPhone = mailPiece.RecipientInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.Phone == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.Phone,
                RcptOrganization = mailPiece.RecipientInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.Organization == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.Organization,
                RcptAddrOptionalLine1AboveStreet = mailPiece.RecipientInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.OptionalLine1AboveStreet == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.OptionalLine1AboveStreet,
                RcptAddrOptionalLine2AboveStreet = mailPiece.RecipientInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.OptionalLine2AboveStreet == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.OptionalLine2AboveStreet,
                RcptAddressStreet = mailPiece.RecipientInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.Street == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.Street,
                RcptAddressAptOrSuiteOrOther = mailPiece.RecipientInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.AptOrSuiteOrOther == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.AptOrSuiteOrOther,
                RcptAddrOptionalLine1BelowStreet = mailPiece.RecipientInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.OptionalLine1BelowStreet == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.OptionalLine1BelowStreet,
                RcptAddrOptionalLine2BelowStreet = mailPiece.RecipientInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.OptionalLine2BelowStreet == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.OptionalLine2BelowStreet,
                RcptAddressCity = mailPiece.RecipientInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.City == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.City,
                RcptAddressState = mailPiece.RecipientInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.State == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.State,
                RcptAddressZIP = mailPiece.RecipientInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.Zip == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.Zip,
                RcptAddressCountry = mailPiece.RecipientInfo == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.Country == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Address.Country,
                RetRcptPrefix = mailPiece.ReturnInfo == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Recipient == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Recipient == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Recipient.Prefix,
                RetRcptFirstName = mailPiece.ReturnInfo == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Recipient == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Recipient == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Recipient.FirstName,
                RetRcptMiddleName = mailPiece.ReturnInfo == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Recipient == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Recipient == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Recipient.MiddleName,
                RetRcptLastName = mailPiece.ReturnInfo == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Recipient == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Recipient == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Recipient.LastName,
                RetRcptSuffix = mailPiece.ReturnInfo == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Recipient == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Recipient == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Recipient.Suffix,
                RetRcptPhone = mailPiece.ReturnInfo == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Recipient == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Recipient == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Recipient.Phone,
                RetRcptOrganization = mailPiece.ReturnInfo == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Recipient == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Recipient == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Recipient.Organization,
                RetRcptAddrOptionalLine1AboveStreet = mailPiece.ReturnInfo == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Address == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Address == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Address.OptionalLine1AboveStreet,
                RetRcptAddrOptionalLine2AboveStreet = mailPiece.ReturnInfo == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Address == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Address == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Address.OptionalLine2AboveStreet,
                RetRcptAddressStreet = mailPiece.ReturnInfo == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Address == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Address == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Address.Street,
                RetRcptAddressAptOrSuiteOrOther = mailPiece.ReturnInfo == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Address == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Address == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Address.AptOrSuiteOrOther,
                RetRcptAddrOptionalLine1BelowStreet = mailPiece.ReturnInfo == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Address == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Address == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Address.OptionalLine1BelowStreet,
                RetRcptAddrOptionalLine2BelowStreet = mailPiece.ReturnInfo == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Address == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Address == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Address.OptionalLine2BelowStreet,
                RetRcptAddressCity = mailPiece.ReturnInfo == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Address == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Address == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Address.City,
                RetRcptAddressState = mailPiece.ReturnInfo == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Address == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Address == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Address.State,
                RetRcptAddressZIP = mailPiece.ReturnInfo == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Address == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Address == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Address.Zip,
                RetRcptAddressCountry = mailPiece.ReturnInfo == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Address == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Address == null ? DBNull.Value.ToString() : mailPiece.ReturnInfo.Address.Country,
                ErrorCode = errorCode,
                ErrorDesc = errorDesc,
                RcptEmailId = mailPiece.RecipientInfo.Recipient.EmailAddress == null ? DBNull.Value.ToString() : mailPiece.RecipientInfo.Recipient.EmailAddress,
                PullBackRequestedOn = (DateTime?)null,
                PulledBackOn = (DateTime?)null,
                PrintedOn = (DateTime?)null,
                ShippedOn = (DateTime?)null,
                PickedUpOn = (DateTime?)null
            });

            if (mailPiece.CustomInfo != null)
            {
                await InsertBatchMailPieceCustomDetails(mailPiece.CustomInfo, mailPieceId);
            }

            return mailPieceId;
        }

        public async Task<int> InsertBatchMailPieceCustomDetails(List<Metadata> mailPieceCustomInfos, int mailPieceId)
        {
            string query = "INSERT INTO [AM].[MailPieceCustomInfo](MailPieceId, [Key],[DataType], [Value]) VALUES (@MailPieceId, @Key, @DataType ,@Value)";

            foreach (var mailPieceCustonInfo in mailPieceCustomInfos)
            {
                await db.ExecuteAsync(query, new { MailPieceId = mailPieceId, Key = mailPieceCustonInfo.Key, DataType = mailPieceCustonInfo.DataType, Value = mailPieceCustonInfo.Value });
            }
            return mailPieceId;
        }

        public List<MailPieceDTO> GetMailPiecesForUATBridge()
        {
            string query = @"SELECT M.*,B.ReceivedOn FROM AM.MailPieces M INNER JOIN AM.MailBatches B ON M.MailBatchId=B.Id WHERE M.ShippingStatus <> 'Error' AND B.ReceivedOn > ISNULL((SELECT TOP 1 LastProcessedOn FROM dbo.ProductionBridge ORDER BY Id DESC), DATEADD(DAY,-5,GETUTCDATE()))";

            var data = db.Query<MailPieceDTO>(query).ToList();
            return data;
        }

        public List<MailPieceDTO> GetMailPiecesForUATBridge(DateTime utcStartDate, int clientId)
        {
            string query = @"SELECT M.*,B.ReceivedOn FROM AM.MailPieces M INNER JOIN AM.MailBatches B ON M.MailBatchId=B.Id WHERE M.ShippingStatus <> 'Error' AND M.ClientId = @ClientId AND B.ReceivedOn > @ReceivedOn";

            var data = db.Query<MailPieceDTO>(query, new { ClientId = clientId, ReceivedOn = utcStartDate }).ToList();
            return data;
        }

        public List<MailPieceCustomInfo> GetMailPieceCustomInfos(int mailPieceId)
        {
            string query = @"SELECT * FROM AM.MailPieceCustomInfo WHERE MailPieceId = @MailPieceId";

            var data = db.Query<MailPieceCustomInfo>(query, new { MailPieceId = mailPieceId }).ToList();
            return data;
        }

        public BatchResponse GetMailPieceDetailsByMailPieceUniqueId(string uniqueId)
        {
            string query = @"SELECT MP.*,MB.ReceivedOn,CC.AppCode,D.ShippingCarrier,D.ShippingServiceType from AM.MailPieces MP 
inner join AM.MailBatches MB ON MP.MailBatchId = MB.Id
inner join Common.Clients CC ON MP.ClientId = CC.Id
left outer join [dbo].[v_DocTypes] D ON MP.DocTypeId=D.Id and MP.PnSConfigDetailId = D.PnSConfigDetailId
where MP.UniqueId = @UniqueId
";

            return db.QuerySingleOrDefault<BatchResponse>(query, new { UniqueId = uniqueId });
        }

        public List<MBInfo> GetReportedBackRecordsFromUATBridge()
        {
            string query = @"SELECT Distinct B.Id AS MailBatchId,C.AppCode FROM AM.MailBatches B INNER JOIN Common.Clients C ON B.ClientId=C.Id WHERE B.PipelineStatus='ReportedBack' AND B.IsReportedBack=1 AND B.BatchName LIKE 'UAT%'";

            var data = db.Query<MBInfo>(query).ToList();
            return data;
        }

        public List<MBInfo> GetPnSReportedBackRecordsFromUATBridge()
        {
            string query = @"SELECT BS.PrintBatch AS MailBatchId,C.AppCode FROM PnS.BatchSummary BS INNER JOIN Common.Clients C ON BS.ClientId=C.Id WHERE BS.BatchIdentifier IN (SELECT UniqueId FROM AM.PrintBatches WHERE MailBatchId IN (SELECT Distinct B.Id AS MailBatchId FROM AM.MailBatches B INNER JOIN Common.Clients C ON B.ClientId=C.Id WHERE B.PipelineStatus='ReportedBack' AND B.IsReportedBack=1 AND B.BatchName LIKE 'UAT%')) AND BS.IsReportedBack=1";

            var data = db.Query<MBInfo>(query).ToList();
            return data;
        }

        public List<MBInfo> GetReportedBackAMRecordsFromUATBridge()
        {
            string query = @"select B.Id AS MailBatchId,C.AppCode from AM.mailBatches B INNER JOIN Common.Clients C ON B.ClientId=C.Id WHERE B.PipelineStatus='ReportedBack' AND B.BatchName NOT LIKE 'UAT%' AND B.ReceivedOn >='2019-06-01'  AND B.ReceivedOn <'2019-12-01'";

            var data = db.Query<MBInfo>(query).ToList();
            return data;
        }

        public List<MBInfo> GetReportedBackPnSRecordsFromUATBridge()
        {
            string query = @"SELECT BS.PrintBatch AS MailBatchId,C.AppCode FROM PnS.BatchSummary BS INNER JOIN Common.Clients C ON BS.ClientId=C.Id WHERE BS.BatchIdentifier IN (SELECT UniqueId FROM AM.PrintBatches WHERE MailBatchId IN (select B.Id from AM.mailBatches B INNER JOIN Common.Clients C ON B.ClientId=C.Id WHERE B.PipelineStatus='ReportedBack' AND B.BatchName NOT LIKE 'UAT%' AND B.ReceivedOn >='2019-06-01'  AND B.ReceivedOn <'2019-12-01')) AND BS.IsReportedBack=1";

            var data = db.Query<MBInfo>(query).ToList();
            return data;
        }

        public int DeletePrintBatchDetails(int printBatchId)
        {
            string query = @"UPDATE AM.MailPieces SET PrintBatchId=NULL WHERE PrintBatchId=@PrintBatchId";

            var retVal = db.Execute(query, new { PrintBatchId = printBatchId });

            if (retVal == 1)
            {
                query = @"DELETE FROM AM.PrintBatches WHERE Id=@PrintBatchId";

                retVal = db.Execute(query, new { PrintBatchId = printBatchId });
            }

            return retVal;
        }

        public dynamic GetCoversheetData(int mailPieceId)
        {
            return db.QuerySingle<dynamic>("usp_GetCoversheetData", new { MailPieceId = mailPieceId }, commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}