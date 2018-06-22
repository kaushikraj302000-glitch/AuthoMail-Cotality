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
        List<MailPieces> GetMailPieceByParam(int clientId, string businessFriendlyIdName, string businessFriendlyIdValue, DateTime startDate, DateTime endDate, string docType, string status, string channel, string clientBatchId, int? mailBatchId);
        MailPieces GetMailPieceByUniqueId(Guid uniqueId);
        List<MailPieces> GetAllData(int clientId);
    }
    public class SearchService : ServiceBase, ISearchService
    {
        public List<MailPieces> GetMailPieceByParam(int clientId, string businessFriendlyIdName, string businessFriendlyIdValue, DateTime startDate, DateTime endDate, string docType, string status, string channel, string clientBatchId, int? mailBatchId)
        {
            string query = "[dbo].[uspweb_MailPieceSearch] @clientId, @businessFriendlyIdName, @businessFriendlyIdValue, @startDate, @endDate, @docType, @shippingStatus, @channel,@clientBatchId,@mailBatchId";
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
                     channel = channel.IsNullOrWhiteSpace() ? null : channel,
                     clientBatchId = clientBatchId.IsNullOrWhiteSpace() ? null : clientBatchId,
                     mailBatchId = mailBatchId
                 }
                 ).ToList();
            return data;
        }


        public MailPieces GetMailPieceByUniqueId(Guid uniqueId)
        {
            string query = "SELECT * FROM  [AM].[MailBatches] b,[Common].[SFTChannels] ch,[Common].[Clients] c,[AM].[MailPieces] m LEFT OUTER JOIN [Profile].[Users] u ON m.PulledBackRequestedBy=u.Id LEFT OUTER JOIN [AM].[DocTypes] d ON m.DocTypeId=d.Id WHERE m.[UniqueId] = @uniqueId";
            return db.Query<MailBatches, SFTChannels, Clients, MailPieces, Users, DocTypes, MailPieces>(query,
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
                     uniqueId = uniqueId
                 }
                 ).ToList().First();
        }

        public List<MailPieces> GetAllData(int clientId)
        {
            //string query = "SELECT * FROM  [AM].[MailBatches] b,[Common].[SFTChannels] ch,[Common].[Clients] c,[AM].[MailPieces] m LEFT OUTER JOIN [Profile].[Users] u ON m.PulledBackRequestedBy=u.Id LEFT OUTER JOIN [AM].[DocTypes] d ON m.DocTypeId=d.Id LEFT OUTER JOIN [AM].[MailPieceCustomInfo] cust ON m.Id=cust.MailPieceId WHERE m.MailBatchId=b.Id AND b.SFTChannelId=ch.Id AND m.ClientId=c.Id AND m.ClientId=101";
            string query = "SELECT * FROM  [AM].[MailBatches] b,[AM].[MailPieces] m  LEFT OUTER JOIN [AM].[MailPieceCustomInfo] cust ON m.Id=cust.MailPieceId WHERE m.MailBatchId=b.Id AND m.ClientId=101";

            var dict = new Dictionary<int, MailPieces>();
            var data = db.Query<MailBatches, MailPieces, MailPieceCustomInfoTest, MailPieces>(query,
                (b, m, cust) =>
                {
                    MailPieces mp;
                    if (!dict.TryGetValue(m.Id, out mp))
                    {
                        mp = m;
                        mp.MailPieceCustomInfoTest = new List<MailPieceCustomInfoTest>();
                        mp.MailBatches = b;
                        dict.Add(mp.Id, mp);
                    }
                    mp.MailPieceCustomInfoTest.Add(cust);
                    //m.MailBatches = b;
                    return mp;
                }
                //splitOn: "MailPieceId"
                ).Distinct().ToList();

            return data;

            //    var data = db.Query<MailBatches, SFTChannels, Clients, MailPieces, Users, DocTypes, MailPieceCustomInfoTest, MailPieces>(query,
            //       (b, s, c, m, u, d, cust) =>
            //       {
            //           MailPieces mp;
            //           if (!dict.TryGetValue(m.Id, out mp))
            //           {
            //               mp = m;
            //               mp.MailPieceCustomInfoTest = new List<MailPieceCustomInfoTest>();
            //               dict.Add(mp.Id, mp);
            //           }
            //           mp.MailPieceCustomInfoTest.Add(cust);
            //           mp.Clients = c;
            //           mp.MailBatches = b;
            //           b.SFTChannels = s;
            //           mp.DocTypes = d;
            //           mp.Users = u;
            //           return mp;
            //       }
            //       ).Distinct().ToList();
        }

    }
}
