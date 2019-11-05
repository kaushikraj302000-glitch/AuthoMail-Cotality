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
        List<MailPieces> GetSLAMissErrorReportByDateOrBatch(int clientId, DateTime startDate, DateTime endDate, int? batchId, int? docType, string filterBy);
        List<MonthlyReconReportViewModel> GetMonthlyReconReport(int clientId, DateTime startDate, DateTime endDate, string channel);
        List<ChannelCodeTitle> GetChannelTitle(int clientId);
        List<PullBackRequests> GetPullbackRequestReport(int clientId, DateTime startDate, DateTime endDate);
        List<MailPieces> GetMailPiecesDataFromPrintBatchId(int printBatchId);
        List<MailPieces> GetMailPiecesDataFromMailBatchId(int mailBatchId);
        MailPieces GetMailPiecesDataFromMailBatchIdAndUniqueId(int mailBatchId, Guid uniqueId);
        List<PCISubmittedBatchDetail> GetPCIReconReport(DateTime startDate, DateTime endDate);
        List<DALLASSubmittedBatchDetail> GetDALLASReconReport(DateTime startDate, DateTime endDate);
        List<EnvelopeInventory> SupplyInventoryReport(List<string> envelopes);
        MailBatches GetMailBatchDataFromMailBatchId(int mailBatchId);
        List<ReportWizard> GetReportWizard(List<int> clientId, string businessFriendlyIdName, string businessFriendlyIdValue, DateTime? startDate, DateTime? endDate, List<int> docType, string status, string channel, List<string> shippingMethod, string trackingNo);
        List<FedExReport> GetFedExData(int[] isPickedUp = null);
        string GetAppcodeByClientid(int clientId);
        List<string> GetMonthlyReconDate(int clientId);
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

        public List<MailPieces> GetSLAMissErrorReportByDateOrBatch(int clientId, DateTime startDate, DateTime endDate, int? batchId, int? docType, string filterBy)
        {
            string query = "[dbo].[uspweb_ErrorReportSLA] @clientId, @startDateRec, @endDateRec, @startDateDue, @endDateDue, @batchId, @docType";
            bool isReceived = filterBy == "ByReceivedOn" ? true : false;
            DateTime? dt = null;

            var data = db.Query<MailBatches, SFTChannels, Clients, MailPieces, DocTypes, ClientDocTypes, MailPieces>(query,
                (b, s, c, m, d, cd) =>
                {
                    m.Clients = c;
                    m.MailBatches = b;
                    b.SFTChannels = s;
                    m.DocTypes = d;
                    if (m.DocTypes != null)
                        m.DocTypes.ClientDocTypes = cd;
                    return m;
                },
                new
                {
                    clientId = clientId,
                    batchId = batchId,
                    docType = docType,
                    startDateRec = isReceived ? (startDate.ToStartDateTime()).EstToUtc() : dt,
                    endDateRec = isReceived ? (endDate.ToEndDateTime()).EstToUtc() : dt,
                    startDateDue = isReceived ? dt : (startDate.ToStartDateTime()).EstToUtc(),
                    endDateDue = isReceived ? dt : (endDate.ToEndDateTime()).EstToUtc()
                }
                ).ToList();
            return data;
        }

        public List<MonthlyReconReportViewModel> GetMonthlyReconReport(int clientId, DateTime startDate, DateTime endDate, string channel)
        {
            string query = "[dbo].[uspweb_MonthlyReconReport] @clientId, @startDate, @endDate, @channel";
            string appCode = GetAppcodeByClientid(clientId);
            //if (appCode == "USAA")
            //{
            //    startDate = (startDate.Date + new TimeSpan(20, 00, 01)).EstToUtc();
            //    endDate = (endDate.Date + new TimeSpan(20, 00, 00)).EstToUtc();
            //}
            //else if (appCode == "HSBC" || appCode == "BOTW")
            //{
            //    startDate = (startDate + new TimeSpan(19, 30, 01)).EstToUtc();
            //    endDate = (endDate + new TimeSpan(19, 30, 00)).EstToUtc();
            //}
            //else
            //{
            //    startDate = startDate.ToStartDateTime().EstToUtc();
            //    endDate = endDate.ToEndDateTime().EstToUtc();
            //}
            startDate = startDate.ToStartDateTime();
            endDate = endDate.ToEndDateTime();
            var data = db.Query<MonthlyReconReportViewModel>(query,
                new
                {
                    clientId = clientId,
                    startDate = startDate,
                    endDate = endDate,
                    channel = channel.IsNullOrWhiteSpace() ? null : channel,
                    //shippingStatus = ShippingStatus.Shipped.ToString()
                }).ToList();
            return data != null ? data.OrderBy(d => d.ReceivedOn).ToList() : data;
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

        public MailPieces GetMailPiecesDataFromMailBatchIdAndUniqueId(int mailBatchId, Guid uniqueId)
        {
            var mailPieceDictionary = new Dictionary<int, MailPieces>();

            string query = @"SELECT * FROM AM.MailPieces M INNER JOIN AM.MailBatches B ON M.MailBatchId=B.Id LEFT OUTER JOIN AM.MailPieceCustomInfo MC ON M.Id=MC.MailPieceId INNER JOIN Common.Clients C ON M.ClientId=C.Id WHERE M.MailBatchId=@MailBatchId AND M.UniqueId=@UniqueId";

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
                    MailBatchId = mailBatchId,
                    UniqueId = uniqueId
                }, splitOn: "Id,MailPieceId,Id").Distinct().ToList();
            return data.SingleOrDefault();
        }

        public List<PCISubmittedBatchDetail> GetPCIReconReport(DateTime startDate, DateTime endDate)
        {
            string query = @"SELECT PT.Id as PrintTrailBatchId,PT.[PrintBatchName],PT.SubmittedOn ,MIN(MP.DueDate) AS DueDate, D.ShippingCarrier
,D.ShippingServiceType,COUNT(*) AS PackageCount,SUM(REQ.TotalSheets) AS TotalPages 
FROM PnS.PrintTrailBatches PT 
INNER JOIN PnS.MRDFRequest REQ ON PT.Id=REQ.PrintTrailBatchId
INNER JOIN AM.MailPieces MP ON REQ.UniqueId=MP.UniqueId
INNER JOIN Common.PrintTrails PTS ON MP.PrintTrailId=PTS.Id
LEFT OUTER JOIN AM.DocTypes D ON MP.DocTypeId=D.Id
WHERE PT.SubmittedOn >= @StartDate AND  PT.SubmittedOn < @EndDate AND MP.PrintLocation='Fort Mill, SC' 
AND REQ.MailPieceStatus NOT IN ('Error','PulledBack')
GROUP BY PT.Id,PT.PrintBatchName,PT.SubmittedOn,D.ShippingCarrier,D.ShippingServiceType
ORDER BY PrintTrailBatchId";
            return db.Query<PCISubmittedBatchDetail>(query, new { StartDate = startDate, EndDate = endDate }).ToList();
        }

        public List<DALLASSubmittedBatchDetail> GetDALLASReconReport(DateTime startDate, DateTime endDate)
        {
            string query = @"SELECT C.AppCode,D.ShippingCarrier,D.ShippingServiceType,MIN(P.DueDate) as DueDate,COUNT(*) AS PackageCount,SUM(P.TotalPages) AS TotalPages FROM AM.MailPieces P
INNER JOIN AM.DocTypes D ON P.DocTypeId=D.Id
INNER JOIN Common.Clients C ON P.ClientId=C.Id
WHERE P.ShippingStatus NOT IN ('Error','PulledBack') AND P.EffectiveReceivedOnEst >= @StartDate AND P.EffectiveReceivedOnEst < @EndDate
AND P.PrintLocation='Dallas CL'
GROUP BY C.AppCode,D.ShippingCarrier,D.ShippingServiceType";
            return db.Query<DALLASSubmittedBatchDetail>(query, new { StartDate = startDate, EndDate = endDate }).ToList();
        }

        public List<EnvelopeInventory> SupplyInventoryReport(List<string> envelopes)
        {
            envelopes.RemoveAll(e => e == "");
            string query = @"select * from Common.EnvelopeInventory et
inner join Common.Envelopes e on et.EnvelopeId=e.Id";
            DynamicParameters parameter = new DynamicParameters();
            if (envelopes != null && envelopes.Count > 0)
            {
                query = string.Format("{0} where e.Id in @envelopes", query);
                parameter.Add("@envelopes", envelopes);
            }
            return db.Query<EnvelopeInventory, Envelopes, EnvelopeInventory>(query,
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

        public List<ReportWizard> GetReportWizard(List<int> clientId, string businessFriendlyIdName, string businessFriendlyIdValue, DateTime? startDate, DateTime? endDate, List<int> docType, string status, string channel, List<string> shippingMethod, string trackingNo)
        {
            if (!startDate.HasValue && businessFriendlyIdValue.IsNullOrWhiteSpace() && trackingNo.IsNullOrWhiteSpace())
                throw new Exception("Date filter required.");

            string query = @"SELECT m.Id as MailPieceId,m.BusinessFriendlyIdValue as LoanNumber,m.ShippingStatus as [Status],m.UniqueId,
	 m.RcptFirstName as FirstName,m.RcptLastName as LastName,m.RcptAddressCity as [City],m.RcptAddressZip as ZIP,m.RcptAddressStreet,
	 m.RcptAddressState as [State], m.ShippedOn as MailedOn ,m.TrackingNo,m.DueDate,m.ErrorCode,m.ErrorDesc,m.PrintLocation,m.PrintedOn
	 ,b.BatchName as BatchId,b.ReceivedOn,m.DocTypeId,m.IsEDeliveryRequested,m.FileName,
	 cd.DocTypeName as DocType,(d.ShippingCarrier + '-' + d.ShippingServiceType) as ShippingMethod,
     m.DocumentOrPackageType,m.TotalPages as PagesShipped,
	 ch.Title as Channel,
     c.AppCode as ClientCode
	 FROM  [AM].[MailBatches] b,[Common].[SFTChannels] ch,[Common].[Clients] c,[AM].[MailPieces] m
	 LEFT OUTER JOIN [Profile].[Users] u ON m.PulledBackRequestedBy=u.Id	 
	 LEFT OUTER JOIN [AM].[DocTypes] d ON m.DocTypeId=d.Id
     left outer join AM.ClientDocTypes cd on d.ClientDocTypeId=cd.Id
	 WHERE m.MailBatchId=b.Id AND b.SFTChannelId=ch.Id AND m.ClientId=c.Id";
            DynamicParameters parameter = new DynamicParameters();
            if (clientId != null && clientId.Count > 0)
            {
                query = string.Format("{0} and m.ClientId in @clientId", query);
                parameter.Add("@clientId", clientId);
            }
            if (!string.IsNullOrWhiteSpace(businessFriendlyIdName) && !string.IsNullOrWhiteSpace(businessFriendlyIdValue))
            {
                query = string.Format("{0} and m.BusinessFriendlyIdName=@BusinessFriendlyIdName and m.businessFriendlyIdValue=@businessFriendlyIdValue", query);
                parameter.Add("@BusinessFriendlyIdName", businessFriendlyIdName);
                parameter.Add("@businessFriendlyIdValue", businessFriendlyIdValue);
            }
            if (!status.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} and m.ShippingStatus = @ShippingStatus", query);
                parameter.Add("@ShippingStatus", status);
            }
            if (!string.IsNullOrWhiteSpace(channel))
            {
                query = string.Format("{0} and ch.Code=@Code", query);
                parameter.Add("@Code", channel);
            }

            if (!string.IsNullOrWhiteSpace(trackingNo))
            {
                query = string.Format("{0} and m.TrackingNo=@TrackingNo", query);
                parameter.Add("@TrackingNo", trackingNo);
            }


            if (docType != null && docType.Count > 0)
            {
                query = string.Format("{0} and cd.ClientDocTypeId in @docType", query);
                parameter.Add("@docType", docType);
            }
            if (shippingMethod != null && shippingMethod.Count > 0)
            {
                List<string> shipMethod = new List<string>();
                List<string> shipCarrier = new List<string>();
                shippingMethod.ForEach(s => shipCarrier.Add(s.Split('-').First()));
                shippingMethod.ForEach(s => shipMethod.Add(s.Split('-')[1]));
                query = string.Format("{0} and d.ShippingServiceType in @shippingMethod and d.ShippingCarrier in @shippingCarrier", query);
                parameter.Add("@shippingMethod", shipMethod);
                parameter.Add("@shippingCarrier", shipCarrier);
            }
            if (parameter.ParameterNames.Count() == 0 && !startDate.HasValue && !endDate.HasValue)
                startDate = endDate = Common.EstToday;
            if (startDate.HasValue && endDate.HasValue)
            {
                query = string.Format("{0} and b.ReceivedOn BETWEEN @startDate and @endDate", query);
                parameter.Add("@startDate", startDate.Value.ToStartDateTime().EstToUtc());
                parameter.Add("@endDate", endDate.Value.ToEndDateTime().EstToUtc());
            }
            query = string.Format("{0} order by b.ReceivedOn desc", query);
            var data = db.Query<ReportWizard>(query, parameter).ToList();
            return data;
        }

        public List<FedExReport> GetFedExData(int[] isPickedUp)
        {
            isPickedUp = isPickedUp ?? new int[0];
            //Possible values are for isPickedUp parameter is : 1-for today's pickedup records, 0-for today's not pickedup records and default -1 for previous not pickedup records

            string queryPreviousNotPickedUp = "SELECT *,'PreviousNotPickedUp' as PickUpStatus FROM [dbo].[view_FedExData] WHERE EffectiveReceivedOnEst < @todayEffectiveDate AND IsPickedUp=0";

            string queryTodayNotPickedUp = "SELECT *,'TodayNotPickedUp' as PickUpStatus FROM [dbo].[view_FedExData] WHERE EffectiveReceivedOnEst >= @todayEffectiveDate AND EffectiveReceivedOnEst < DATEADD(DAY,1,@todayEffectiveDate) AND IsPickedUp=0";

            string queryTodayPickedUp = "SELECT *,'TodayPickedUp' as PickUpStatus FROM [dbo].[view_FedExData] WHERE EffectiveReceivedOnEst >= @todayEffectiveDate AND EffectiveReceivedOnEst < DATEADD(DAY,1,@todayEffectiveDate) AND IsPickedUp=1";


            if (isPickedUp.Any(i => i == 0) && isPickedUp.Any(i => i == -1))
            {
                //This will provide previous and today's not picked up records
                var data = db.Query<FedExReport>(queryPreviousNotPickedUp + " UNION " + queryTodayNotPickedUp, new
                {
                    todayEffectiveDate = Common.EstToday,
                }).ToList();
                return data;
            }
            else if (isPickedUp.Any(i => i == 0) && isPickedUp.Any(i => i == 1))
            {
                //This will provide today's not picked up and picked up records
                var data = db.Query<FedExReport>(queryTodayNotPickedUp + " UNION " + queryTodayPickedUp, new
                {
                    todayEffectiveDate = Common.EstToday,
                }).ToList();
                return data;
            }
            else if (isPickedUp.Any(i => i == 0))
            {
                //This will provide today's not picked up records
                var data = db.Query<FedExReport>(queryTodayNotPickedUp, new
                {
                    todayEffectiveDate = Common.EstToday,
                }).ToList();
                return data;
            }
            else if (isPickedUp.Any(i => i == -1))
            {
                //This will provide today's not picked up records
                var data = db.Query<FedExReport>(queryPreviousNotPickedUp, new
                {
                    todayEffectiveDate = Common.EstToday,
                }).ToList();
                return data;
            }
            else
            {
                //This will provide today's picked up records
                var data = db.Query<FedExReport>(queryTodayPickedUp, new
                {
                    todayEffectiveDate = Common.EstToday,
                }).ToList();
                return data;
            }
        }


        public string GetAppcodeByClientid(int clientId)
        {
            return db.QuerySingle<string>("select AppCode from Common.Clients where Id=@clientId", new { clientId = clientId });
        }


        public List<string> GetMonthlyReconDate(int clientId)
        {
            string appCode = GetAppcodeByClientid(clientId);
            //string startDate = appCode == "USAA" || appCode == "HSBC" || appCode == "BOTW" ? "25" : "26";
            string startDate = "26";
            var dateList = new List<string>();
            var currDate = DateTime.UtcNow.UtcToEst();
            if (currDate.Day <= 25) currDate = currDate.AddMonths(-1);
            for (var dt = currDate.Date; dt.Date >= currDate.AddMonths(-6).Date; dt = dt.AddMonths(-1).Date)
            {
                dateList.Add(string.Format("{0}/{1}/{2} - {3}/25/{4}", dt.Month, startDate, dt.Year, dt.AddMonths(1).Month, dt.AddMonths(1).Year));
            }
            return dateList;
        }
    }
}
