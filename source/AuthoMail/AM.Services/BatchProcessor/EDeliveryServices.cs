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
                    query += " AND IsSigningRoomCreated=1 AND InvitationSent=0";
                    break;
                case EDPipelineStatus.InvitationSent:
                    query += " AND InvitationSent=1 AND eDelivered=0";
                    break;
                case EDPipelineStatus.Expired:
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
    }
}
