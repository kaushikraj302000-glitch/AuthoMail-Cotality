using AM.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace AM.Services.Services
{
    public interface IHomeService//Home Service interface that has number of methods
    {
        List<DeliveryReportVM> GetDeliveryReportByDateAndDocumentType(DateTime startDate, DateTime endDate, string documentType);
    }
    public class HomeService : ServiceBase, IHomeService //It implements Interfaces 
    {
        public List<DeliveryReportVM> GetDeliveryReportByDateAndDocumentType(DateTime startDate, DateTime endDate, string documentType)
        {
           
            //Sql Query
            string query = "select * from [AM].[Batch]";
            string whereClause = string.Format("WHERE ReceivedOn>=@startDate AND ReceivedOn<=@endDate");
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            if (!documentType.IsNullOrWhiteSpace())
            {
                whereClause = string.Format("{0} AND DocumentOrPackageType=@documentType", whereClause);
                parameters.Add("@documentType", documentType);
            }
            query = string.Format("{0} {1}", query, whereClause);
            parameters.Add("@startDate", startDate);
            parameters.Add("@endDate", endDate);
            /*
             * Execution of query with parateters.
             * Without Dapper you can't create following code. So just Add "using Dapper" namespace at begin.
            */
            var data = db.Query<DeliveryReportVM>(query, parameters).ToList();
            return data;
        }
    }
}
