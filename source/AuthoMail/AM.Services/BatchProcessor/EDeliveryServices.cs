using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AM.Entities;
using AM.Lib;
using System.Data;

namespace AM.Services
{
    public interface IEDeliveryService : IDisposable
    {
        List<EDMailPiece> GetBatchesByPipelineStatusAndSFTChannelId(string batchPipelineStatus, int? sftChannelId = null);
        int InsertEDMailPieceDetails(EDMailPiece mailPiece);
        int UpdateEDMailPiecePipelineStatus(int mailPieceId, string pipeLineStatus, DateTime timestamp);
        int UpdateEDMailPieceDetails(EDMailPiece mailPiece);
        DocType GetEDDocTypeByDocTypeName(int clientId, string documentOrPackageName);
        int FallBackEDeliveryPackage(int edMPId, Guid uniqueId, int mailBatchId, string appCode);
        int ErrorOutEDeliveryPackage(Guid uniqueId, int mailBatchId, string errorCode, string errorDesc);
    }
    public class EDeliveryService : ServiceBase, IEDeliveryService
    {
        public List<EDMailPiece> GetBatchesByPipelineStatusAndSFTChannelId(string pipelineStatus, int? sftChannelId)
        {
            //Sql Query
            string query = "SELECT ED.Id, ED.UniqueId, ED.ClientId, ED.MailBatchId, ED.BusinessFriendlyIdName, ED.BusinessFriendlyIdValue, ED.FileName,ED.DocumentOrPackageType, ED.DocTypeId, ED.ClientDocTypeId, ED.ExpiryDate, ED.PipelineStatus, ED.RcptFirstName, ED.RcptMiddleName,ED.RcptLastName, ED.RcptEmailId, ED.TotalPages, ED.ErrorCode, ED.ErrorDesc, ED.SigningRoomId, ED.SigningRoomUniqueId, ED.SigningRoomName,ED.DocumentId, ED.ReceivedOn, ED.IsReceived, ED.DownloadedOn, ED.IsDownloaded, ED.ValidatedOn, ED.IsValidated, ED.SigningRoomCreatedOn,ED.IsSigningRoomCreated, ED.InvitationSentOn, ED.IsInvitationSent, ED.eDeliveredOn, ED.IseDelivered, ED.ConsentedOn, ED.IsConsented,ED.NotConsentedOn, ED.IsNotConsented, ED.DocReviewedOn, ED.IsDocReviewed, ED.DocDownloadedOn, ED.IsDocDownloaded, ED.ESignedOn, ED.IsESigned,ED.PDFFileSize, C.AppCode AS ClientCode,B.SFTChannelId AS ChannelId,S.Code AS ChannelCode,B.BatchName FROM [AM].[EDeliveryMailPieces] ED INNER JOIN AM.MailBatches B ON ED.MailBatchId = B.Id LEFT OUTER JOIN Common.Clients C ON ED.ClientId = C.Id LEFT OUTER JOIN Common.SFTChannels S ON B.SFTChannelId = S.Id WHERE ED.PipelineStatus = @PipelineStatus";

            if (sftChannelId.HasValue)
                query += " AND SFTChannelId= @SFTChannelId";

            switch (pipelineStatus)
            {
                case EDPipelineStatus.Received:
                    query += " AND ED.IsReceived=1 AND ED.IsDownloaded=0";
                    break;
                case EDPipelineStatus.Downloaded:
                    query += " AND ED.IsDownloaded=1 AND ED.IsValidated=0";
                    break;
                case EDPipelineStatus.Validated:
                    query += " AND ED.IsValidated=1 AND ED.IsSigningRoomCreated=0 AND ED.IsExpired=0";
                    break;
                case EDPipelineStatus.SigningRoomCreated:
                    query += " AND ED.IsSigningRoomCreated=1 AND ED.IsInvitationSent=0 AND ED.IsExpired=0";
                    break;
                case EDPipelineStatus.InvitationSent:
                    query += " AND ED.IsInvitationSent=1 AND ED.IseDelivered=0 AND ED.IsExpired=0";
                    break;
                case EDPipelineStatus.Expired:
                    query += " AND ED.IsExpired=1 AND ED.IseDelivered=0";
                    break;
                case EDPipelineStatus.FallBack:
                    query += " AND ED.Expired=1 AND ED.FallBack=0";
                    break;
                default:
                    break;
            }

            return db.Query<EDMailPiece>(query, new { PipelineStatus = pipelineStatus, SFTChannelId = sftChannelId }).ToList();
        }

        public int InsertEDMailPieceDetails(EDMailPiece mailPiece)
        {
            string query = "INSERT INTO [AM].[EDeliveryMailPieces](UniqueId, ClientId, SFTChannelId, BatchUniqueId, BatchName, BusinessFriendlyIdName, BusinessFriendlyIdValue, FileName, FilePattern, DocumentOrPackageType, RcptPrefix, RcptFirstName, RcptMiddleName, RcptLastName, RcptSuffix, RcptPhone, RcptEmailId, RcptOrganization, RcptAddrOptionalLine1AboveStreet, RcptAddrOptionalLine2AboveStreet, RcptAddressStreet, RcptAddressAptOrSuiteOrOther, RcptAddrOptionalLine1BelowStreet, RcptAddrOptionalLine2BelowStreet, RcptAddressCity, RcptAddressState, RcptAddressZIP, RcptAddressCountry, RetRcptPrefix, RetRcptFirstName, RetRcptMiddleName, RetRcptLastName, RetRcptSuffix, RetRcptPhone, RetRcptOrganization, RetRcptAddrOptionalLine1AboveStreet, RetRcptAddrOptionalLine2AboveStreet, RetRcptAddressStreet, RetRcptAddressAptOrSuiteOrOther, RetRcptAddrOptionalLine1BelowStreet, RetRcptAddrOptionalLine2BelowStreet, RetRcptAddressCity, RetRcptAddressState, RetRcptAddressZIP, RetRcptAddressCountry, IsFallBackRequested) Output Inserted.Id VALUES (@UniqueId, @ClientId, @SFTChannelId, @BatchUniqueId, @BatchName, @BusinessFriendlyIdName, @BusinessFriendlyIdValue, @FileName, @FilePattern, @DocumentOrPackageType, @RcptPrefix, @RcptFirstName, @RcptMiddleName, @RcptLastName, @RcptSuffix, @RcptPhone, @RcptEmailId, @RcptOrganization, @RcptAddrOptionalLine1AboveStreet, @RcptAddrOptionalLine2AboveStreet, @RcptAddressStreet, @RcptAddressAptOrSuiteOrOther, @RcptAddrOptionalLine1BelowStreet, @RcptAddrOptionalLine2BelowStreet, @RcptAddressCity, @RcptAddressState, @RcptAddressZIP, @RcptAddressCountry, @RetRcptPrefix, @RetRcptFirstName, @RetRcptMiddleName, @RetRcptLastName, @RetRcptSuffix, @RetRcptPhone, @RetRcptOrganization, @RetRcptAddrOptionalLine1AboveStreet, @RetRcptAddrOptionalLine2AboveStreet, @RetRcptAddressStreet, @RetRcptAddressAptOrSuiteOrOther, @RetRcptAddrOptionalLine1BelowStreet, @RetRcptAddrOptionalLine2BelowStreet, @RetRcptAddressCity, @RetRcptAddressState, @RetRcptAddressZIP, @RetRcptAddressCountry, @IsFallBackRequested)";

            return db.ExecuteScalar<int>(query, mailPiece);
        }

        public int UpdateEDMailPiecePipelineStatus(int mailPieceId, string pipeLineStatus, DateTime timestamp)
        {
            string query = "UPDATE [AM].[EDeliveryMailPieces] SET [{0}On] = @TimeStamp, PipelineStatus=@PipelineStatus WHERE Id=@Id".Fill(pipeLineStatus);

            return db.Execute(query, new { TimeStamp = timestamp, Id = mailPieceId, PipelineStatus = pipeLineStatus });
        }

        public int UpdateEDMailPieceDetails(EDMailPiece mailPiece)
        {
            string query = @"UPDATE [AM].[EDeliveryMailPieces] SET DocTypeId=@DocTypeId, ClientDocTypeId=@ClientDocTypeId, ExpiryDate=@ExpiryDate, TotalPages=@TotalPages, ErrorCode=@ErrorCode, ErrorDesc=@ErrorDesc, SigningRoomId=@SigningRoomId, SigningRoomUniqueId=@SigningRoomUniqueId, SigningRoomName=@SigningRoomName,PDFFileSize=@PDFFileSize WHERE Id=@Id";

            return db.Execute(query, mailPiece);
        }

        public DocType GetEDDocTypeByDocTypeName(int clientId, string documentOrPackageName)
        {
            string query = "SELECT D.*,CDT.DocTypeName FROM [AM].[DocTypes] D INNER JOIN [AM].[ClientDocTypes] CDT ON D.ClientDocTypeId=CDT.Id WHERE CDT.ClientId=@ClientId AND CDT.DocTypeName = @DocumentOrPackageName AND ShippingCarrier='E-DELIVERY' AND D.IsActive=1";

            return db.QuerySingleOrDefault<DocType>(query, new { ClientId = clientId, DocumentOrPackageName = documentOrPackageName });
        }

        public int FallBackEDeliveryPackage(int edMPId, Guid uniqueId, int mailBatchId, string appCode)
        {
            using (ICommonService commonSvc = new CommonService())
            using (IMailPieceService mpSvc = new MailPieceService())
            using (IDocTypeService docTypeSvc = new DocTypeService())
            {
                MailPieceDTO mailPieceDTO = mpSvc.GetMailPieceDataByUniqueIdAndMBId(uniqueId, mailBatchId);
                DocType mpDocType = docTypeSvc.GetDocTypeById(mailPieceDTO.DocTypeId.Value);
                commonSvc.GetUpdatedDueDateAndPrintLocation(Common.Now, mailPieceDTO, mpDocType, appCode);
                mpSvc.UpdateBatchMailPieceDetails(mailPieceDTO);
                return commonSvc.UpdateBatchPipeLineStatus(mailBatchId, BatchPipelineStatus.Validated, Common.Now);
            }
        }
        public int ErrorOutEDeliveryPackage(Guid uniqueId, int mailBatchId, string errorCode, string errorDesc)
        {
            string query = "UPDATE AM.MailPieces SET ShippingStatus='Error', ErrorCode=@ErrorCode, ErrorDesc=@ErrorDesc WHERE MailBatchId=@MailBatchId AND UniqueId=@UniqueId";
            var retVal = db.Execute(query, new { ErrorCode = errorCode, ErrorDesc = errorCode, MailBatchId = mailBatchId, UniqueId = uniqueId });

            if (retVal == 1)
            {
                using (ICommonService commonSvc = new CommonService())
                {
                    commonSvc.UpdateBatchStatus(mailBatchId, BatchStatus.Failed.ToString());
                    commonSvc.UpdateBatchValidationStatus(mailBatchId, BatchValidationStatus.Invalid, errorDesc);
                    commonSvc.UpdateBatchPipeLineStatus(mailBatchId, BatchPipelineStatus.Validated, Common.Now);
                    return 1;
                }
            }
            return 0;
        }
    }
}
