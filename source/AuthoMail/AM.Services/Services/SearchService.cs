using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AM.Services.Models;

namespace AM.Services
{
    public interface ISearchService
    {
        List<MailPieceSearch> GetMailPieceByParam(int clientId, string businessFriendlyIdName, string businessFriendlyIdValue, DateTime? printedOn, DateTime? shippedOn);
    }
    public class SearchService : ServiceBase, ISearchService
    {
        public List<MailPieceSearch> GetMailPieceByParam(int clientId, string businessFriendlyIdName, string businessFriendlyIdValue, DateTime? printedOn, DateTime? shippedOn)
        {
            string query = "select * from [dbo].[view_MailPieceByParam] WHERE ClientId=@clientId";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@clientId", clientId);
            if (!businessFriendlyIdName.IsNullOrWhiteSpace() && !businessFriendlyIdValue.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} AND (BusinessFriendlyIdName=@businessFriendlyIdName AND BusinessFriendlyIdValue=@businessFriendlyIdValue)", query);
                parameters.Add("@businessFriendlyIdName", businessFriendlyIdName);
                parameters.Add("@businessFriendlyIdValue", businessFriendlyIdValue);
            }
            if (printedOn.HasValue)
            {
                query = string.Format("{0} AND (cast(PrintedOn as date)=cast(@printedOn as date))", query);
                parameters.Add("@printedOn", printedOn);
            }
            if (shippedOn.HasValue)
            {
                query = string.Format("{0} AND (cast(ShippedOn as date)=cast(@shippedOn as date))", query);
                parameters.Add("@shippedOn", shippedOn);
            }
            var data = db.Query<MailPieceSearch>(query, parameters).ToList();
            return data;
        }
    }
}
