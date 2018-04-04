using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AM.Lib;
using System.Data;
namespace AM.Services
{
    public interface ISearchService : IDisposable
    {
        List<MailPieceSearch> GetMailPieceByParam(int clientId, string businessFriendlyIdName, string businessFriendlyIdValue, DateTime startDate, DateTime endDate, string docType, string status, string channel);
        List<MailPieces> GetMailPieces();
    }
    public class SearchService : ServiceBase, ISearchService
    {
        public List<MailPieceSearch> GetMailPieceByParam(int clientId, string businessFriendlyIdName, string businessFriendlyIdValue, DateTime startDate, DateTime endDate, string docType, string status, string channel)
        {
            string query = "[dbo].[uspweb_MailPieceSearch] @clientId, @businessFriendlyIdName, @businessFriendlyIdValue, @startDate, @endDate, @docType, @shippingStatus, @channel,null";
            var data = db.Query<MailPieceSearch>(query, new
            {
                clientId = clientId,
                businessFriendlyIdName = businessFriendlyIdName.IsNullOrWhiteSpace() ? null : businessFriendlyIdName,
                businessFriendlyIdValue = businessFriendlyIdValue.IsNullOrWhiteSpace() ? null : businessFriendlyIdValue,
                startDate = startDate.ToStartDateTime().EstToUtc(),
                endDate = endDate.ToEndDateTime().EstToUtc(),
                docType = docType.IsNullOrWhiteSpace() ? null : docType,
                shippingStatus = status.IsNullOrWhiteSpace() ? null : status,
                channel = channel.IsNullOrWhiteSpace() ? null : channel
            }).ToList();

            //var data2 = GetMailPiecesData(clientId, businessFriendlyIdName, businessFriendlyIdValue, startDate, endDate, docType, status, channel);

            return data;
        }

        public List<MailPieces> GetMailPiecesData(int clientId, string businessFriendlyIdName, string businessFriendlyIdValue, DateTime startDate, DateTime endDate, string docType, string status, string channel)
        {
            string query = "[dbo].[uspweb_MailPieceSearchTest] @clientId, @businessFriendlyIdName, @businessFriendlyIdValue, @startDate, @endDate, @docType, @shippingStatus, @channel,null";
            var data = db.Query<Clients, MailBatches, SFTChannels, MailPieces, DocTypes, Users, MailPieces>(query,
                (c, b, s, m, d, u) =>
                {
                    m.Clients = c;
                    m.MailBatches = b;
                    b.SFTChannels = s;
                    m.DocTypes = d;
                    m.Users = u;
                    return m;
                },
                new
                {
                    clientId = clientId,
                    businessFriendlyIdName = businessFriendlyIdName.IsNullOrWhiteSpace() ? null : businessFriendlyIdName,
                    businessFriendlyIdValue = businessFriendlyIdValue.IsNullOrWhiteSpace() ? null : businessFriendlyIdValue,
                    startDate = startDate.ToStartDateTime().EstToUtc(),
                    endDate = endDate.ToEndDateTime().EstToUtc(),
                    docType = docType.IsNullOrWhiteSpace() ? null : docType,
                    shippingStatus = status.IsNullOrWhiteSpace() ? null : status,
                    channel = channel.IsNullOrWhiteSpace() ? null : channel
                }
                ).ToList();
            return data;
        }

        public List<MailPieces> GetMailPieces()
        {
            string query = "SELECT * FROM  [Common].[Clients] c,[AM].[MailBatches] b,[Common].[SFTChannels] ch,[AM].[MailPieces] m LEFT OUTER JOIN [AM].[DocTypes] d ON m.DocTypeId=d.Id LEFT OUTER JOIN [Profile].[Users] u ON m.PulledBackRequestedBy=u.Id WHERE m.ClientId=c.Id AND m.MailBatchId=b.Id AND b.SFTChannelId=ch.Id AND m.ClientId=101";

            var data = db.Query<Clients, MailBatches, SFTChannels, MailPieces, DocTypes, Users, MailPieces>(query,
               (c, b, s, m, d, u) =>
               {
                   m.Clients = c;
                   m.MailBatches = b;
                   b.SFTChannels = s;
                   m.DocTypes = d;
                   m.Users = u;
                   return m;
               }
                ).ToList();
            return data;
        }
    }
}
