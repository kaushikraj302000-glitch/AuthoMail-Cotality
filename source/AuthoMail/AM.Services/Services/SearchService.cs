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
        //Remove Method(make sure below method in use)
        //List<MailPieces> GetMailPieceByParam(int clientId, string businessFriendlyIdName, string businessFriendlyIdValue, DateTime startDate, DateTime endDate, string docType, string status, string channel, string clientBatchId, int? mailBatchId, string trackingNo);
        List<SearchViewModel> GetMailPieceByParam(int clientId, string businessFriendlyIdName, string businessFriendlyIdValue, DateTime? startDate, DateTime? endDate, List<int> docType, string status, string channel, string clientBatchId, int? mailBatchId, string trackingNo);
        MailPieces GetMailPieceByUniqueId(Guid uniqueId);
        List<MailPieces> GetAllData(int clientId);

        List<TestModel> TestMp();

        List<DateTime> GetHolidays();
    }
    public class SearchService : ServiceBase, ISearchService
    {
        public List<SearchViewModel> GetMailPieceByParam(int clientId, string businessFriendlyIdName, string businessFriendlyIdValue, DateTime? startDate, DateTime? endDate, List<int> docType, string status, string channel, string clientBatchId, int? mailBatchId, string trackingNo)
        {
            string query = @"SELECT Top 1000
	 m.BusinessFriendlyIdValue as BusinessIdValue,m.ShippingStatus as [Status],m.UniqueId,
	 m.RcptFirstName as FirstName,m.RcptLastName as LastName,m.RcptAddressCity as [City],m.RcptAddressZip as ZIP,
	 m.RcptAddressState as [State],CASE WHEN d.ShippingCarrier ='FedEx' THEN m.PickedUpOn ELSE m.ShippedOn END as ShippedOn,m.TrackingNo,m.DueDate,m.ErrorCode,m.ErrorDesc,m.PrintLocation,
	 b.ReceivedOn,m.DocTypeId,
	 d.DocTypeCode as DocType,d.ShippingServiceType as ShippingMethod,d.ShippingCarrier,
	 ch.Title as Channel
	 FROM  [AM].[MailBatches] b,[Common].[SFTChannels] ch,[Common].[Clients] c,[AM].[MailPieces] m
	 LEFT OUTER JOIN [Profile].[Users] u ON m.PulledBackRequestedBy=u.Id	 
	 LEFT OUTER JOIN [AM].[DocTypes] d ON m.DocTypeId=d.Id
	 WHERE m.MailBatchId=b.Id AND b.SFTChannelId=ch.Id AND m.ClientId=c.Id AND m.ClientId=@clientId";
            DynamicParameters parameter = new DynamicParameters();
            parameter.Add("@clientId", clientId);
            if (!string.IsNullOrWhiteSpace(businessFriendlyIdName))
            {
                query = string.Format("{0} and m.BusinessFriendlyIdName=@BusinessFriendlyIdName", query);
                parameter.Add("@BusinessFriendlyIdName", businessFriendlyIdName);
            }
            if (!string.IsNullOrWhiteSpace(businessFriendlyIdValue))
            {
                query = string.Format("{0} and m.businessFriendlyIdValue=@businessFriendlyIdValue", query);
                parameter.Add("@businessFriendlyIdValue", businessFriendlyIdValue);
            }
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = string.Format("{0} and m.ShippingStatus=@ShippingStatus", query);
                parameter.Add("@ShippingStatus", status);
            }
            if (!string.IsNullOrWhiteSpace(channel))
            {
                query = string.Format("{0} and ch.Code=@Code", query);
                parameter.Add("@Code", channel);
            }
            if (!string.IsNullOrWhiteSpace(clientBatchId))
            {
                query = string.Format("{0} and b.ClientBatchId=@ClientBatchId", query);
                parameter.Add("@ClientBatchId", clientBatchId);
            }
            if (!string.IsNullOrWhiteSpace(trackingNo))
            {
                query = string.Format("{0} and m.TrackingNo=@TrackingNo", query);
                parameter.Add("@TrackingNo", trackingNo);
            }
            if (mailBatchId.HasValue)
            {
                query = string.Format("{0} and m.MailBatchId=@MailBatchId", query);
                parameter.Add("@MailBatchId", mailBatchId);
            }
            if (startDate.HasValue && endDate.HasValue)
            {
                query = string.Format("{0} and b.ReceivedOn BETWEEN @startDate and @endDate", query);
                parameter.Add("@startDate", startDate.Value.ToStartDateTime().EstToUtc());
                parameter.Add("@endDate", endDate.Value.ToEndDateTime().EstToUtc());
            }
            if (docType != null && docType.Count > 0)
            {
                query = string.Format("{0} and m.DocTypeId in @docTypeId", query);
                parameter.Add("@DocTypeId", docType);
            }

            var data = db.Query<SearchViewModel>(query, parameter).ToList();
            return data;
            //var data = db.Query<SearchViewModel>(query,
            //     new
            //     {
            //         clientId = clientId,
            //         businessFriendlyIdName = businessFriendlyIdName.IsNullOrWhiteSpace() ? null : businessFriendlyIdName,
            //         businessFriendlyIdValue = businessFriendlyIdValue.IsNullOrWhiteSpace() ? null : businessFriendlyIdValue,
            //         startDate = startDate.ToStartDateTime().EstToUtc(),
            //         endDate = endDate.ToEndDateTime().EstToUtc(),
            //         docType = docType.IsNullOrWhiteSpace() ? null : docType,
            //         shippingStatus = status.IsNullOrWhiteSpace() ? null : status,
            //         channel = channel.IsNullOrWhiteSpace() ? null : channel,
            //         clientBatchId = clientBatchId.IsNullOrWhiteSpace() ? null : clientBatchId,
            //         trackingNo = trackingNo.IsNullOrWhiteSpace() ? null : trackingNo,
            //         mailBatchId = mailBatchId
            //     }
            //     ).ToList();
        }

        //public List<SearchViewModel> GetMailPieceByParam(int clientId, string businessFriendlyIdName, string businessFriendlyIdValue, DateTime startDate, DateTime endDate, string docType, string status, string channel, string clientBatchId, int? mailBatchId, string trackingNo)
        //{
        //    string query = "[dbo].[uspweb_MailPieceSearch] @clientId, @businessFriendlyIdName, @businessFriendlyIdValue, @startDate, @endDate, @docType, @shippingStatus, @channel,@clientBatchId,@mailBatchId,@trackingNo";
        //    var data = db.Query<SearchViewModel>(query,
        //         new
        //         {
        //             clientId = clientId,
        //             businessFriendlyIdName = businessFriendlyIdName.IsNullOrWhiteSpace() ? null : businessFriendlyIdName,
        //             businessFriendlyIdValue = businessFriendlyIdValue.IsNullOrWhiteSpace() ? null : businessFriendlyIdValue,
        //             startDate = startDate.ToStartDateTime().EstToUtc(),
        //             endDate = endDate.ToEndDateTime().EstToUtc(),
        //             docType = docType.IsNullOrWhiteSpace() ? null : docType,
        //             shippingStatus = status.IsNullOrWhiteSpace() ? null : status,
        //             channel = channel.IsNullOrWhiteSpace() ? null : channel,
        //             clientBatchId = clientBatchId.IsNullOrWhiteSpace() ? null : clientBatchId,
        //             trackingNo = trackingNo.IsNullOrWhiteSpace() ? null : trackingNo,
        //             mailBatchId = mailBatchId
        //         }
        //         ).ToList();
        //    return data;
        //}


        public MailPieces GetMailPieceByUniqueId(Guid uniqueId)
        {
            string query = "SELECT * FROM  [AM].[MailBatches] b,[Common].[SFTChannels] ch,[Common].[Clients] c,[AM].[MailPieces] m LEFT OUTER JOIN [Profile].[Users] u ON m.PulledBackRequestedBy=u.Id LEFT OUTER JOIN [AM].[DocTypes] d ON m.DocTypeId=d.Id WHERE m.MailBatchId=b.Id AND b.SFTChannelId=ch.Id AND b.ClientId=c.Id AND m.[UniqueId] = @uniqueId";
            var mp = db.Query<MailBatches, SFTChannels, Clients, MailPieces, Users, DocTypes, MailPieces>(query,
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
                 }).ToList().First();

            var mreq = db.QuerySingleOrDefault<MRDFRequests>("select r.*,p.PrintBatchName as PrintFileName from PnS.MRDFRequest r inner join PnS.PrintTrailBatches p on r.PrintTrailBatchId=p.Id where r.UniqueId=@uniqueId", new { uniqueId = mp.UniqueId });
            if (mreq != null) mp.MRDFRequests = mreq;
            return mp;
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



        public List<TestModel> TestMp()
        {
            string query = "select m.id,m.shippingstatus,m.SLADays,mb.receivedon,m.duedate,m.DocTypeId,m.Documentorpackagetype,mb.validationstatus,m.duedate,m.ErrorCode,m.errordesc,d.bizdaycutoff,d.bizdayslatype,d.bizdaytimespan,d.satcutoff,d.satslatype,d.sattimespan,d.suncutoff,d.sunslatype,d.suntimespan from [AM].[MailBatches] mb, [AM].[MailPieces] m left outer join [AM].[DocTypes] d ON m.DocTypeId=d.Id where m.[MailBatchId] = mb.Id";//and mb.receivedon > '2018-08-27 04:01:00.000'"; //'2018-07-02 04:01:00.000'";

            return db.Query<TestModel>(query).ToList();
        }


        public List<DateTime> GetHolidays()
        {
            string q = "select HolidayDate from Common.Holidays";
            return db.Query<DateTime>(q).ToList();
        }
    }

    public class TestModel
    {
        public int Id { get; set; }
        public DateTime ReceivedOn { get; set; }
        public int? DocTypeId { get; set; }
        public DateTime? DueDate { get; set; }
        public string BizdayCutOff { get; set; }

        public TimeSpan? BizCutOffTimeSpan
        {
            get
            {
                if (this.BizdayCutOff.IsNullOrWhiteSpace()) return null;
                return TimeSpan.Parse(this.BizdayCutOff);
            }
        }
        public string BizdaySLAType { get; set; }
        public string bizdaytimespan { get; set; }
        public TimeSpan? ActualBizdayTimeSpan
        {
            get
            {
                if (this.bizdaytimespan.IsNullOrWhiteSpace()) return null;
                return TimeSpan.Parse(this.bizdaytimespan);
            }
        }
        public string satcutoff { get; set; }
        public TimeSpan? satcutoffTimeSpan
        {
            get
            {
                if (this.satcutoff.IsNullOrWhiteSpace()) return null;
                return TimeSpan.Parse(this.satcutoff);
            }
        }
        public string SatSLAType { get; set; }
        public string sattimespan { get; set; }
        public TimeSpan? ActualSatTimeSpan
        {
            get
            {

                if (this.sattimespan.IsNullOrWhiteSpace()) return null;
                return TimeSpan.Parse(this.sattimespan);
            }
        }
        public string suncutoff { get; set; }
        public TimeSpan? suncutoffTimeSpan
        {
            get
            {
                if (this.suncutoff.IsNullOrWhiteSpace()) return null;
                return TimeSpan.Parse(this.suncutoff);
            }
        }
        public string SunSLAType { get; set; }
        public string suntimespan { get; set; }
        public TimeSpan? ActualSunTimeSpan
        {
            get
            {
                if (this.suntimespan.IsNullOrWhiteSpace()) return null;
                return TimeSpan.Parse(this.suntimespan);
            }
        }
        public string ErrorCode { get; set; }

    }

}
