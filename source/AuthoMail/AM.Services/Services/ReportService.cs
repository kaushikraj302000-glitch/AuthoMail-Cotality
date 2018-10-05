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
        //List<MailPieces> GetMonthlyReconReport(int clientId, DateTime startDate, DateTime endDate, string channel);
        List<MonthlyReconReportViewModel> GetMonthlyReconReport(int clientId, DateTime startDate, DateTime endDate, string channel);
        List<ChannelCodeTitle> GetChannelTitle(int clientId);
        List<PullBackRequests> GetPullbackRequestReport(int clientId, DateTime startDate, DateTime endDate);
        List<MailPieces> GetFedExPickedUpReport(DateTime todayEffectiveDate, int isPickedUp = -1);
        List<MailPieces> GetMailPiecesDataFromPrintBatchId(int printBatchId);
        List<MailPieces> GetMailPiecesDataFromMailBatchId(int mailBatchId);
        List<PCISubmittedBatchDetail> GetPCIReconReport(DateTime effectiveStartDate, DateTime effectiveEndDate);
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
                    startDate = startDate.ToStartDateTime().EstToUtc(),
                    endDate = endDate.ToEndDateTime().EstToUtc(),
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

        //public List<MailPieces> GetMonthlyReconReport(int clientId, DateTime startDate, DateTime endDate, string channel)
        //{
        //    string query = "[dbo].[uspweb_MonthlyReconReport] @clientId, @startDate, @endDate, @channel,@shippingStatus";

        //    var data = db.Query<MailBatches, SFTChannels, Clients, MailPieces, DocTypes, MRDFRequests, MailPieces>(query,
        //        (b, s, c, m, d, r) =>
        //        {
        //            m.Clients = c;
        //            m.MailBatches = b;
        //            b.SFTChannels = s;
        //            m.DocTypes = d;
        //            m.MRDFRequests = r;
        //            return m;
        //        },
        //        new
        //        {
        //            clientId = clientId,
        //            startDate = startDate.ToStartDateTime().EstToUtc(),
        //            endDate = endDate.ToEndDateTime().EstToUtc(),
        //            channel = channel.IsNullOrWhiteSpace() ? null : channel,
        //            shippingStatus = ShippingStatus.Shipped.ToString()
        //        }, splitOn: "Id,Id,Id,Id,Id,UniqueId").ToList();
        //    return data;
        //}
        public List<MonthlyReconReportViewModel> GetMonthlyReconReport(int clientId, DateTime startDate, DateTime endDate, string channel)
        {
            string query = "[dbo].[uspweb_MonthlyReconReport] @clientId, @startDate, @endDate, @channel,@shippingStatus";

            var data = db.Query<MonthlyReconReportViewModel>(query,
                new
                {
                    clientId = clientId,
                    startDate = startDate.ToStartDateTime().EstToUtc(),
                    endDate = endDate.ToEndDateTime().EstToUtc(),
                    channel = channel.IsNullOrWhiteSpace() ? null : channel,
                    shippingStatus = ShippingStatus.Shipped.ToString()
                }).ToList();
            return data;
        }

        public List<MailPieces> GetFedExPickedUpReport(DateTime todayEffectiveDate, int isPickedUp)
        {
            //Possible values are for isPickedUp parameter is : 1-for pickedup records, 0-for not pickedup records and default -1 for get all records
            string query = "[dbo].[uspweb_FedExPickedUpReport] @todayEffectiveDate, @isPickedUp";

            var data = db.Query<MailPieces, MailBatches, DocTypes, MailPieces>(query,
                (m, b, d) =>
                {
                    m.MailBatches = b;
                    m.DocTypes = d;
                    return m;
                },
                new
                {
                    isPickedUp = isPickedUp,
                    todayEffectiveDate = todayEffectiveDate,
                }).ToList();
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

        public List<MailPieces> GetMailPiecesDataFromPrintBatchId(int printBatchId)
        {
            string query = @"SELECT * FROM AM.MailPieces M INNER JOIN AM.MailBatches B ON M.MailBatchId=B.Id LEFT OUTER JOIN AM.MailPieceCustomInfo MC ON M.Id=MC.MailPieceId WHERE M.PrintBatchId=@PrintBatchId";

            var mailPieceDictionary = new Dictionary<int, MailPieces>();

            var data = db.Query<MailPieces, MailBatches, MailPieceCustomInfoTest, MailPieces>(query,
                (m, b, c) =>
                {
                    MailPieces mp;

                    if (!mailPieceDictionary.TryGetValue(m.Id, out mp))
                    {
                        mp = m;
                        mp.MailBatches = b;
                        mp.MailPieceCustomInfoTest = new List<MailPieceCustomInfoTest>();
                        mailPieceDictionary.Add(mp.Id, mp);
                    }

                    mp.MailPieceCustomInfoTest.Add(c);
                    return mp;
                },
                new
                {
                    PrintBatchId = printBatchId
                }, splitOn: "Id,MailPieceId").Distinct().ToList();
            return data;
        }

        public List<MailPieces> GetMailPiecesDataFromMailBatchId(int mailBatchId)
        {
            var mailPieceDictionary = new Dictionary<int, MailPieces>();

            string query = @"SELECT * FROM AM.MailPieces M INNER JOIN AM.MailBatches B ON M.MailBatchId=B.Id LEFT OUTER JOIN AM.MailPieceCustomInfo MC ON M.Id=MC.MailPieceId INNER JOIN Common.Clients C ON B.ClientId=C.Id WHERE M.MailBatchId=@MailBatchId";

            var data = db.Query<MailPieces, MailBatches, MailPieceCustomInfoTest, Clients, MailPieces>(query,
                (m, b, c, cl) =>
                {
                    MailPieces mp;

                    if (!mailPieceDictionary.TryGetValue(m.Id, out mp))
                    {
                        mp = m;
                        mp.MailBatches = b;
                        mp.Clients = cl;
                        mp.MailPieceCustomInfoTest = new List<MailPieceCustomInfoTest>();
                        mailPieceDictionary.Add(mp.Id, mp);
                    }

                    mp.MailPieceCustomInfoTest.Add(c);
                    return mp;
                },
                new
                {
                    MailBatchId = mailBatchId
                }, splitOn: "Id,MailPieceId,Id").Distinct().ToList();
            return data;
        }

        public List<PCISubmittedBatchDetail> GetPCIReconReport(DateTime effectiveStartDate, DateTime effectiveEndDate)
        {
            string query = "[dbo].[usp_GetPCIReconReport] @StartDate, @EndDate";

            return db.Query<PCISubmittedBatchDetail>(query, new { StartDate = effectiveStartDate, EndDate = effectiveEndDate }).ToList();
        }
    }
}
