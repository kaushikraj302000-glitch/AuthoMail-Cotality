using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace AM.Services.Services
{
    public interface ISearchService
    {
        List<MailPieceService> GetMailPieceByParam(int clientId, string loanNo, DateTime? printOn, DateTime? shippedOn);
    }
    public class SearchService : ServiceBase, ISearchService
    {
        public List<MailPieceService> GetMailPieceByParam(int clientId, string loanNo, DateTime? printOn, DateTime? shippedOn)
        {
            string query = "select * from [dbo].[view_MailPieceByParam] WHERE ClientId=@clientId";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@clientId", clientId);
            if (!loanNo.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} AND (BusinessFriendlyIdName='LoanNumber' AND LoanNumber=@loanNo)", query);
                parameters.Add("@loanNo", loanNo);
            }
            //if (printOn != null)
            //{
            //    query = string.Format("{0} AND (=@printOn)", query);
            //    parameters.Add("@printOn", printOn);
            //}
            if (shippedOn != null)
            {
                query = string.Format("{0} AND (ShippedOn=@shippedOn)", query);
                parameters.Add("@shippedOn", shippedOn);
            }
            var data = db.Query<MailPieceService>(query, parameters).ToList();
            return data;
        }
    }
}
