using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AM.Services.Models;
using AM.Lib;
using System.Data;
namespace AM.Services
{
    public interface ISearchService
    {
        List<MailPieceSearch> GetMailPieceByParam(int clientId, string businessFriendlyIdName, string businessFriendlyIdValue, DateTime startDate, DateTime endDate, string docType, string status, string channel);
    }
    public class SearchService : ServiceBase, ISearchService
    {
        public List<MailPieceSearch> GetMailPieceByParam(int clientId, string businessFriendlyIdName, string businessFriendlyIdValue, DateTime startDate, DateTime endDate, string docType, string status, string channel)
        {
            string query = "[dbo].[uspweb_MailPieceSearch] @clientId, @businessFriendlyIdName, @businessFriendlyIdValue, @startDate, @endDate, @docType, @shippingStatus, @channel";
            var data = db.Query<MailPieceSearch>(query, new
            {
                clientId = clientId,
                businessFriendlyIdName = businessFriendlyIdName.IsNullOrWhiteSpace() ? null : businessFriendlyIdName,
                businessFriendlyIdValue = businessFriendlyIdValue.IsNullOrWhiteSpace() ? null : businessFriendlyIdValue,
                startDate = (startDate.Date + new TimeSpan(00, 00, 00)).EstToUtc(),
                endDate = (endDate.Date + new TimeSpan(23, 59, 59)).EstToUtc(),
                docType = docType.IsNullOrWhiteSpace() ? null : docType,
                shippingStatus = status.IsNullOrWhiteSpace() ? null : status,
                channel = channel.IsNullOrWhiteSpace() ? null : channel
            }).ToList();
            return data;
        }
    }
}
