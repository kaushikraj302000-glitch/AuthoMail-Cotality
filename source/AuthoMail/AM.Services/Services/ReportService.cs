using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using AM.Services.Models;
using AM.Entities;
using AM.Lib;
namespace AM.Services
{
    public interface IReportService
    {
        List<DailyDetailOperationsReport> GetDailyDetailOperationsByDateOrBatch(int clientId, DateTime? date, int? batchId, string channel);
        List<ErrorReport> GetErrorReportByDateOrBatch(int clientId, DateTime? date, int? batchId, string channel);
        List<SummaryControlReport> GetSummaryControlReport(int clientId, DateTime startDate, DateTime endDate, string channel);
        List<ErrorReport> GetSLAMissErrorReportByDateOrBatch(int clientId, DateTime date, int? batchId, string docType, string filterBy);
        List<MonthlyReconReport> GetMonthlyReconReport(int clientId, DateTime startDate, DateTime endDate, string channel);
        List<ChannelCodeTitle> GetChannelTitle(int clientId);
        List<PullbackRequestReport> GetPullbackRequestReport(int clientId, DateTime startDate, DateTime endDate);
    }
    public class ReportService : ServiceBase, IReportService
    {
        public List<DailyDetailOperationsReport> GetDailyDetailOperationsByDateOrBatch(int clientId, DateTime? date, int? batchId, string channel)
        {
            string query = "[dbo].[uspweb_DailyDetailOperationsReport] @clientId, @startDate, @endDate, @batchId, @channel, @shippingStatus";
            var data = db.Query<DailyDetailOperationsReport>(query, new
            {
                clientId = clientId,
                shippingstatus = ShippingStatus.Shipped.ToString(),
                batchId = batchId,
                channel = channel.IsNullOrWhiteSpace() ? null : channel,
                startDate = date.HasValue ? (date.Value.Date + new TimeSpan(00, 00, 00)).EstToUtc() : date,
                endDate = date.HasValue ? (date.Value.Date + new TimeSpan(23, 59, 59)).EstToUtc() : date,
            }).ToList();
            return data;
        }
        public List<ErrorReport> GetErrorReportByDateOrBatch(int clientId, DateTime? date, int? batchId, string channel)
        {
            string query = "[dbo].[uspweb_ErrorReport] @clientId, @startDate, @endDate, @batchId, @channel, @shippingStatus";
            var data = db.Query<ErrorReport>(query, new
            {
                clientId = clientId,
                shippingstatus = ShippingStatus.Error.ToString(),
                batchId = batchId,
                channel = channel.IsNullOrWhiteSpace() ? null : channel,
                startDate = date.HasValue ? (date.Value.Date + new TimeSpan(00, 00, 00)).EstToUtc() : date,
                endDate = date.HasValue ? (date.Value.Date + new TimeSpan(23, 59, 59)).EstToUtc() : date,
            }).ToList();
            return data;
        }

        public List<SummaryControlReport> GetSummaryControlReport(int clientId, DateTime startDate, DateTime endDate, string channel)
        {
            string query = "[dbo].[uspweb_SummaryControlReport] @clientId, @startDate, @endDate, @channel";
            var data = db.Query<SummaryControlReport>(query, new
            {
                clientId = clientId,
                startDate = startDate.EstToUtc(),
                endDate = endDate.EstToUtc(),
                channel = channel.IsNullOrWhiteSpace() ? null : channel
            }).ToList();
            return data;
        }

        public List<ErrorReport> GetSLAMissErrorReportByDateOrBatch(int clientId, DateTime date, int? batchId, string docType, string filterBy)
        {
            string query = "[dbo].[uspweb_ErrorReportSLA] @clientId, @startDateRec, @endDateRec, @startDateDue, @endDateDue, @batchId, @docType";
            bool isReceived = filterBy == "ByReceivedOn" ? true : false;
            DateTime? dt = null;
            var data = db.Query<ErrorReport>(query, new
            {
                clientId = clientId,
                //shippingstatus = string.Format("'{0}','{1}'", ShippingStatus.InProgress.ToString(), ShippingStatus.Shipped.ToString()),
                batchId = batchId,
                docType = docType.IsNullOrWhiteSpace() ? null : docType,
                startDateRec = isReceived ? (date.Date + new TimeSpan(00, 00, 00)).EstToUtc() : dt,
                endDateRec = isReceived ? (date.Date + new TimeSpan(23, 59, 59)).EstToUtc() : dt,
                startDateDue = isReceived ? dt : (date.Date + new TimeSpan(00, 00, 00)).EstToUtc(),
                endDateDue = isReceived ? dt : (date.Date + new TimeSpan(23, 59, 59)).EstToUtc()
            }).ToList();
            return data;
        }

        public List<MonthlyReconReport> GetMonthlyReconReport(int clientId, DateTime startDate, DateTime endDate, string channel)
        {
            string query = "[dbo].[uspweb_MonthlyReconReport] @clientId, @startDate, @endDate, @channel,@shippingStatus";
            //date must be in 26th to 25th
            var data = db.Query<MonthlyReconReport>(query, new
            {
                clientId = clientId,
                startDate = (startDate.Date + new TimeSpan(00, 00, 00)).EstToUtc(),
                endDate = (endDate.Date + new TimeSpan(23, 59, 59)).EstToUtc(),
                channel = channel.IsNullOrWhiteSpace() ? null : channel,
                shippingStatus = ShippingStatus.Shipped.ToString()
            }).ToList();
            return data;
        }

        public List<ChannelCodeTitle> GetChannelTitle(int clientId)
        {
            string query = "SELECT Code,Title FROM [Common].[SFTChannels] WHERE IsActive=1 AND Id in (select SFTChannelId from [Common].[ClientSFTChannels] WHERE ClientId=@clientId)";
            var channelList = db.Query<ChannelCodeTitle>(query, new { clientId = clientId }).ToList();
            return channelList;
        }


        public List<PullbackRequestReport> GetPullbackRequestReport(int clientId, DateTime startDate, DateTime endDate)
        {
            startDate = (startDate.Date + new TimeSpan(00, 00, 00)).EstToUtc();
            endDate = (endDate.Date + new TimeSpan(23, 59, 59)).EstToUtc();
            string query = "SELECT p.[Id] ,[MailBatchId] ,[MailPieceId] ,[MailPieceUniqueId] ,[Status] ,[RequestedOn] ,[RequestedBy], u.[UserName],b.ClientBatchId FROM [AM].[MailBatches] b, [AM].[PullBackRequests] p LEFT OUTER JOIN [Profile].[Users] u ON p.[RequestedBy]=u.Id  WHERE p.[ClientId] = @clientId AND p.[MailBatchId]=b.Id AND p.RequestedOn BETWEEN @startDate AND @endDate";
            return db.Query<PullbackRequestReport>(query, new { clientId = clientId, startDate = startDate, endDate = endDate }).ToList();
        }
    }
}
