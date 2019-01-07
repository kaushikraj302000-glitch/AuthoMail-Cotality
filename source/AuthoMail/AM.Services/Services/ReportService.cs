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
        List<MailPieces> GetSLAMissErrorReportByDateOrBatch(int clientId, DateTime startDate, DateTime endDate, int? batchId, string docType, string filterBy);
        List<MonthlyReconReportViewModel> GetMonthlyReconReport(int clientId, DateTime startDate, DateTime endDate, string channel);
        List<ChannelCodeTitle> GetChannelTitle(int clientId);
        List<PullBackRequests> GetPullbackRequestReport(int clientId, DateTime startDate, DateTime endDate);
        List<MailPieces> GetFedExPickedUpReport(DateTime todayEffectiveDate, int isPickedUp = -1);
        List<MailPieces> GetMailPiecesDataFromPrintBatchId(int printBatchId);
        List<MailPieces> GetMailPiecesDataFromMailBatchId(int mailBatchId);
        List<PCISubmittedBatchDetail> GetPCIReconReport(DateTime startDate, DateTime endDate);
        List<EnvelopeTransactions> SupplyInventoryReport(List<string> envelopes);
        MailBatches GetMailBatchDataFromMailBatchId(int mailBatchId);
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

        public List<MailPieces> GetSLAMissErrorReportByDateOrBatch(int clientId, DateTime startDate, DateTime endDate, int? batchId, string docType, string filterBy)
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
                    startDateRec = isReceived ? (startDate.ToStartDateTime()).EstToUtc() : dt,
                    endDateRec = isReceived ? (endDate.ToEndDateTime()).EstToUtc() : dt,
                    startDateDue = isReceived ? dt : (startDate.ToStartDateTime()).EstToUtc(),
                    endDateDue = isReceived ? dt : (endDate.ToEndDateTime()).EstToUtc()
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

        public List<PCISubmittedBatchDetail> GetPCIReconReport(DateTime startDate, DateTime endDate)
        {
            string query = @"SELECT PT.Id as PrintTrailBatchId,PT.[PrintBatchName],PT.SubmittedOn,MAX(BS.DueDate) AS DueDate,D.ShippingCarrier,D.ShippingServiceType,COUNT(*) AS PackageCount,SUM(REQ.TotalSheets) AS TotalPages 
FROM PnS.PrintTrailBatches PT 
INNER JOIN PnS.MRDFRequest REQ ON PT.Id=REQ.PrintTrailBatchId
INNER JOIN PnS.BatchSummary BS ON REQ.JobId=BS.PrintBatch
INNER JOIN Common.PrintTrails PTS ON BS.PrintTrailId=PTS.Id
LEFT OUTER JOIN AM.DocTypes D ON BS.DocTypeId=D.Id
WHERE PT.SubmittedOn >= @StartDate AND  PT.SubmittedOn < @EndDate AND BS.PrintLocation='Fort Mill, SC' AND REQ.MailPieceStatus <> 'Error'
GROUP BY PT.Id,PT.PrintBatchName,PT.SubmittedOn,D.ShippingCarrier,D.ShippingServiceType
ORDER BY PrintTrailBatchId";
            return db.Query<PCISubmittedBatchDetail>(query, new { StartDate = startDate, EndDate = endDate }).ToList();
        }


        public List<EnvelopeTransactions> SupplyInventoryReport(List<string> envelopes)
        {
            envelopes.RemoveAll(e => e == "");
            string query = @"select * from Common.EnvelopeTransactions et
inner join Common.Envelopes e on et.EnvelopeId=e.Id";
            DynamicParameters parameter = new DynamicParameters();
            if (envelopes != null && envelopes.Count > 0)
            {
                query = string.Format("{0} where e.Id in @envelopes", query);
                parameter.Add("@envelopes", envelopes);
            }
            return db.Query<EnvelopeTransactions, Envelopes, EnvelopeTransactions>(query,
                (t, e) =>
                {
                    t.Envelopes = e;
                    return t;
                },
                parameter).ToList();
        }

        public MailBatches GetMailBatchDataFromMailBatchId(int mailBatchId)
        {
            string query = @"SELECT * FROM AM.MailBatches B INNER JOIN Common.SFTChannels S ON S.Id=B.SFTChannelId INNER JOIN Common.Clients C ON B.ClientId=C.Id WHERE B.Id=@MailBatchId";

            var data = db.Query<MailBatches, SFTChannels, Clients, MailBatches>(query,
                (mb, s, c) =>
                {

                    mb.SFTChannels = s;
                    mb.Clients = c;
                    return mb;
                },
                new
                {
                    MailBatchId = mailBatchId
                });
            return data.SingleOrDefault();
        }
    }
}
