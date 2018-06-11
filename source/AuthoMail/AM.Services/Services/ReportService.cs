using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using AM.Entities;
using AM.Lib;
namespace AM.Services
{
    public interface IReportService : IDisposable
    {
        List<MailPieces> GetDailyDetailOperationsByDateOrBatch(int clientId, DateTime? date, int? batchId, string channel);
        List<MailPieces> GetErrorReportByDateOrBatch(int clientId, DateTime? date, int? batchId, string channel);
        List<MailPieces> GetSummaryControlReport(int clientId, DateTime startDate, DateTime endDate, string channel);
        List<MailPieces> GetSLAMissErrorReportByDateOrBatch(int clientId, DateTime date, int? batchId, string docType, string filterBy);
        List<MailPieces> GetMonthlyReconReport(int clientId, DateTime startDate, DateTime endDate, string channel);
        List<ChannelCodeTitle> GetChannelTitle(int clientId);
        List<PullBackRequests> GetPullbackRequestReport(int clientId, DateTime startDate, DateTime endDate);
    }
    public class ReportService : ServiceBase, IReportService
    {
        public List<MailPieces> GetDailyDetailOperationsByDateOrBatch(int clientId, DateTime? date, int? batchId, string channel)
        {
            string query = "[dbo].[uspweb_DailyDetailOperationsReport] @clientId, @startDate, @endDate, @batchId, @channel, @shippingStatus";
            var data = db.Query<MailBatches, SFTChannels, Clients, MailPieces, DocTypes, MailPieces>(query,
                (b, s, c, m, d) =>
                {
                    m.Clients = c;
                    m.MailBatches = b;
                    b.SFTChannels = s;
                    m.DocTypes = d;
                    return m;
                },
                new
                {
                    clientId = clientId,
                    shippingstatus = ShippingStatus.Shipped.ToString(),
                    batchId = batchId,
                    channel = channel.IsNullOrWhiteSpace() ? null : channel,
                    startDate = date.HasValue ? (date.Value.ToStartDateTime()).EstToUtc() : date,
                    endDate = date.HasValue ? (date.Value.ToEndDateTime()).EstToUtc() : date
                }
                ).ToList();
            return data;
        }
        public List<MailPieces> GetErrorReportByDateOrBatch(int clientId, DateTime? date, int? batchId, string channel)
        {
            string query = "[dbo].[uspweb_ErrorReport] @clientId, @startDate, @endDate, @batchId, @channel, @shippingStatus";
            var data = db.Query<MailBatches, SFTChannels, Clients, MailPieces, DocTypes, MailPieces>(query,
                (b, s, c, m, d) =>
                {
                    m.Clients = c;
                    m.MailBatches = b;
                    b.SFTChannels = s;
                    m.DocTypes = d;
                    return m;
                },
                new
                {
                    clientId = clientId,
                    shippingstatus = ShippingStatus.Error.ToString(),
                    batchId = batchId,
                    channel = channel.IsNullOrWhiteSpace() ? null : channel,
                    startDate = date.HasValue ? (date.Value.ToStartDateTime()).EstToUtc() : date,
                    endDate = date.HasValue ? (date.Value.ToEndDateTime()).EstToUtc() : date
                }
                ).ToList();
            return data;
        }

        public List<MailPieces> GetSummaryControlReport(int clientId, DateTime startDate, DateTime endDate, string channel)
        {
            string query = "[dbo].[uspweb_SummaryControlReport] @clientId, @startDate, @endDate, @channel";

            var data = db.Query<MailBatches, SFTChannels, Clients, MailPieces, DocTypes, MailPieces>(query,
                (b, s, c, m, d) =>
                {
                    m.Clients = c;
                    m.MailBatches = b;
                    b.SFTChannels = s;
                    m.DocTypes = d;
                    return m;
                },
                new
                {
                    clientId = clientId,
                    startDate = startDate.EstToUtc(),
                    endDate = endDate.EstToUtc(),
                    channel = channel.IsNullOrWhiteSpace() ? null : channel
                }
                ).ToList();

            return data;
        }

        public List<MailPieces> GetSLAMissErrorReportByDateOrBatch(int clientId, DateTime date, int? batchId, string docType, string filterBy)
        {
            string query = "[dbo].[uspweb_ErrorReportSLA] @clientId, @startDateRec, @endDateRec, @startDateDue, @endDateDue, @batchId, @docType";
            bool isReceived = filterBy == "ByReceivedOn" ? true : false;
            DateTime? dt = null;

            var data = db.Query<MailBatches, SFTChannels, Clients, MailPieces, DocTypes, MailPieces>(query,
                (b, s, c, m, d) =>
                {
                    m.Clients = c;
                    m.MailBatches = b;
                    b.SFTChannels = s;
                    m.DocTypes = d;
                    return m;
                },
                new
                {
                    clientId = clientId,
                    batchId = batchId,
                    docType = docType.IsNullOrWhiteSpace() ? null : docType,
                    startDateRec = isReceived ? (date.ToStartDateTime()).EstToUtc() : dt,
                    endDateRec = isReceived ? (date.ToEndDateTime()).EstToUtc() : dt,
                    startDateDue = isReceived ? dt : (date.ToStartDateTime()).EstToUtc(),
                    endDateDue = isReceived ? dt : (date.ToEndDateTime()).EstToUtc()
                }
                ).ToList();
            return data;
        }

        public List<MailPieces> GetMonthlyReconReport(int clientId, DateTime startDate, DateTime endDate, string channel)
        {
            string query = "[dbo].[uspweb_MonthlyReconReport] @clientId, @startDate, @endDate, @channel,@shippingStatus";

            var data = db.Query<MailBatches, SFTChannels, Clients, MailPieces, DocTypes, MRDFRequests, MailPieces>(query,
                (b, s, c, m, d, r) =>
                {
                    m.Clients = c;
                    m.MailBatches = b;
                    b.SFTChannels = s;
                    m.DocTypes = d;
                    m.MRDFRequests = r;
                    return m;
                },
                new
                {
                    clientId = clientId,
                    startDate = startDate.ToStartDateTime().EstToUtc(),
                    endDate = endDate.ToEndDateTime().EstToUtc(),
                    channel = channel.IsNullOrWhiteSpace() ? null : channel,
                    shippingStatus = ShippingStatus.Shipped.ToString()
                }
                , splitOn: "Id,Id,Id,Id,Id,UniqueId").ToList();
            return data;
        }

        public List<ChannelCodeTitle> GetChannelTitle(int clientId)
        {
            string query = "SELECT Code,Title FROM [Common].[SFTChannels] WHERE IsActive=1 AND Id in (select SFTChannelId from [Common].[ClientSFTChannels] WHERE ClientId=@clientId)";
            var channelList = db.Query<ChannelCodeTitle>(query, new { clientId = clientId }).ToList();
            return channelList;
        }


        public List<PullBackRequests> GetPullbackRequestReport(int clientId, DateTime startDate, DateTime endDate)
        {
            string query = "SELECT * FROM [AM].[MailBatches] b,[AM].[MailPieces] m, [AM].[PullBackRequests] p LEFT OUTER JOIN [Profile].[Users] u ON p.[RequestedBy]=u.Id  WHERE p.[ClientId] = @clientId AND p.[MailBatchId]=b.Id AND p.MailPieceId = m.Id AND p.RequestedOn BETWEEN @startDate AND @endDate";
            var data = db.Query<MailBatches, MailPieces, PullBackRequests, Users, PullBackRequests>(query,
                (b, m, p, u) =>
                {
                    p.MailPieces = m;
                    m.MailBatches = b;
                    p.Users = u;
                    return p;
                },
                new
                {
                    clientId = clientId,
                    startDate = startDate.ToStartDateTime().EstToUtc(),
                    endDate = endDate.ToEndDateTime().EstToUtc()
                }).ToList();

            //return db.Query<MailPieces>(query, new { clientId = clientId, startDate = startDate.ToStartDateTime().EstToUtc(), endDate = endDate.ToEndDateTime().EstToUtc() }).ToList();
            return data;
        }
    }
}
