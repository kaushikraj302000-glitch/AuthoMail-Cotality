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
        List<MailPieces> GetMailPieces(int clientId, string businessFriendlyIdName, string businessFriendlyIdValue, DateTime startDate, DateTime endDate, string docType, string status, string channel);
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

            return data;
        }

        public List<MailPieces> GetMailPieces(int clientId, string businessFriendlyIdName, string businessFriendlyIdValue, DateTime startDate, DateTime endDate, string docType, string status, string channel)
        {
            string query = "[dbo].[uspweb_MailPieceSearchTest] @clientId, @businessFriendlyIdName, @businessFriendlyIdValue, @startDate, @endDate, @docType, @shippingStatus, @channel,null";
            var data = db.Query<MailBatches, SFTChannels, Clients, MailPieces, Users, DocTypes, MailPieces>(query,
                (b, s, c, m, u, d) =>
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


    }
}
