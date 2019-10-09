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
    }
    public class EDeliveryService : ServiceBase, IEDeliveryService
    {
        public List<EDMailPiece> GetBatchesByPipelineStatusAndSFTChannelId(string pipelineStatus, int? sftChannelId)
        {
            //Sql Query
            string query = "SELECT * FROM [AM].[EDeliveryMailPieces] ED LEFT OUTER JOIN AM.DocTypes D ON ED.DocTypeId=D.Id LEFT OUTER JOIN AM.ClientDocTypes CDT ON D.ClientDocTypeId=CDT.Id LEFT OUTER JOIN Common.Clients C ON ED.ClientId=C.Id LEFT OUTER JOIN Common.SFTChannels S ON ED.SFTChannelId=S.Id WHERE ED.PipelineStatus= @PipelineStatus";

            if (sftChannelId.HasValue)
                query += " AND SFTChannelId= @SFTChannelId";

            switch (pipelineStatus)
            {
                case EDPipelineStatus.Received:
                    query += " AND IsReceived=1 AND IsDownloaded=0";
                    break;
                case EDPipelineStatus.Downloaded:
                    query += " AND IsDownloaded=1 AND IsValidated=0";
                    break;
                case EDPipelineStatus.Validated:
                    query += " AND IsValidated=1 AND IsSigningRoomCreated=0";
                    break;
                case EDPipelineStatus.SigningRoomCreated:
                    query += " AND IsSigningRoomCreated=1 AND IsInvitationSent=0";
                    break;
                case EDPipelineStatus.InvitationSent:
                    query += " AND IsInvitationSent=1 AND IseDelivered=0";
                    break;
                case EDPipelineStatus.Expired:
                    query += " AND IsExpired=1 AND IsFallBack=0";
                    break;
                case EDPipelineStatus.FallBack:
                    query += " AND Expired=1 AND FallBack=0";
                    break;
                default:
                    break;
            }

            return db.Query<EDMailPiece, DocTypes, ClientDocTypes, Clients, SFTChannels, EDMailPiece>(query,
                (m, d, cd, c, s) =>
                {
                    m.Clients = c;
                    m.DocTypes = d;
                    m.SFTChannel = s;
                    d.ClientDocTypes = cd;
                    return m;
                },
                new
                {
                    PipelineStatus = pipelineStatus,
                    SFTChannelId = sftChannelId
                }).ToList();
        }

        public int InsertEDMailPieceDetails(EDMailPiece mailPiece)
        {
            string query = "INSERT INTO [AM].[EDeliveryMailPieces](UniqueId, ClientId, SFTChannelId, BatchUniqueId, BatchName, BusinessFriendlyIdName, BusinessFriendlyIdValue, FileName, FilePattern, DocumentOrPackageType, RcptPrefix, RcptFirstName, RcptMiddleName, RcptLastName, RcptSuffix, RcptPhone, RcptEmailId, RcptOrganization, RcptAddrOptionalLine1AboveStreet, RcptAddrOptionalLine2AboveStreet, RcptAddressStreet, RcptAddressAptOrSuiteOrOther, RcptAddrOptionalLine1BelowStreet, RcptAddrOptionalLine2BelowStreet, RcptAddressCity, RcptAddressState, RcptAddressZIP, RcptAddressCountry, RetRcptPrefix, RetRcptFirstName, RetRcptMiddleName, RetRcptLastName, RetRcptSuffix, RetRcptPhone, RetRcptOrganization, RetRcptAddrOptionalLine1AboveStreet, RetRcptAddrOptionalLine2AboveStreet, RetRcptAddressStreet, RetRcptAddressAptOrSuiteOrOther, RetRcptAddrOptionalLine1BelowStreet, RetRcptAddrOptionalLine2BelowStreet, RetRcptAddressCity, RetRcptAddressState, RetRcptAddressZIP, RetRcptAddressCountry) Output Inserted.Id VALUES (@UniqueId, @ClientId, @SFTChannelId, @BatchUniqueId, @BatchName, @BusinessFriendlyIdName, @BusinessFriendlyIdValue, @FileName, @FilePattern, @DocumentOrPackageType, @RcptPrefix, @RcptFirstName, @RcptMiddleName, @RcptLastName, @RcptSuffix, @RcptPhone, @RcptEmailId, @RcptOrganization, @RcptAddrOptionalLine1AboveStreet, @RcptAddrOptionalLine2AboveStreet, @RcptAddressStreet, @RcptAddressAptOrSuiteOrOther, @RcptAddrOptionalLine1BelowStreet, @RcptAddrOptionalLine2BelowStreet, @RcptAddressCity, @RcptAddressState, @RcptAddressZIP, @RcptAddressCountry, @RetRcptPrefix, @RetRcptFirstName, @RetRcptMiddleName, @RetRcptLastName, @RetRcptSuffix, @RetRcptPhone, @RetRcptOrganization, @RetRcptAddrOptionalLine1AboveStreet, @RetRcptAddrOptionalLine2AboveStreet, @RetRcptAddressStreet, @RetRcptAddressAptOrSuiteOrOther, @RetRcptAddrOptionalLine1BelowStreet, @RetRcptAddrOptionalLine2BelowStreet, @RetRcptAddressCity, @RetRcptAddressState, @RetRcptAddressZIP, @RetRcptAddressCountry)";

            return db.ExecuteScalar<int>(query, mailPiece);
        }

        public int UpdateEDMailPiecePipelineStatus(int mailPieceId, string pipeLineStatus, DateTime timestamp)
        {
            string query = "UPDATE [AM].[EDeliveryMailPieces] SET [{0}On] = @TimeStamp WHERE Id=@Id".Fill(pipeLineStatus);

            return db.Execute(query, new { TimeStamp = timestamp, Id = mailPieceId });
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
    }
}
