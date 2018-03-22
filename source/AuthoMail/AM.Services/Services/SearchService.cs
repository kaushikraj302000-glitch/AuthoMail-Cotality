using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AM.Services.Models;
using AM.Lib;
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
            string query = "select * from [dbo].[view_MailPieceByParam] WHERE ClientId=@clientId AND (ReceivedOn between @startDate and @endDate)";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@clientId", clientId);
            parameters.Add("@startDate", (startDate + new TimeSpan(00, 00, 00)).EstToUtc());
            parameters.Add("@endDate", (endDate + new TimeSpan(23, 59, 59)).EstToUtc());
            if (!businessFriendlyIdName.IsNullOrWhiteSpace() && !businessFriendlyIdValue.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} AND (BusinessFriendlyIdName=@businessFriendlyIdName AND BusinessFriendlyIdValue=@businessFriendlyIdValue)", query);
                parameters.Add("@businessFriendlyIdName", businessFriendlyIdName);
                parameters.Add("@businessFriendlyIdValue", businessFriendlyIdValue);
            }
            if (!docType.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} AND DocumentOrPackageType=@docType", query);
                parameters.Add("@docType", docType);
            }
            if (!status.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} AND ShippingStatus=@status", query);
                parameters.Add("@status", status);
            }
            if (!channel.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} AND ChannelCode=@channel", query);
                parameters.Add("@channel", channel);
            }

            //if (shippedOn.HasValue)
            //{
            //    query = string.Format("{0} AND (cast(ShippedOn as date)=cast(@shippedOn as date))", query);
            //    parameters.Add("@shippedOn", shippedOn);
            //}
            var data = db.Query<MailPieceSearch>(query, parameters).ToList();
            return data;
        }
    }
}
