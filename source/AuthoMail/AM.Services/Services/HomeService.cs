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
        List<DeliveryReportVM> GetDeliveryReportByBatchId(string batchId);
    }
    public class HomeService : ServiceBase, IHomeService //It implements Interfaces 
    {
        public List<DeliveryReportVM> GetDeliveryReportByBatchId(string batchId)
        {
            //Sql Query
            string query = "select * from [AM].[Batch] WHERE Id=@id";

            //List of parameters declaration
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            //Assign value to parameters It may one or more. Note:
            parameters.Add("@id", batchId);

            /*
             * Execution of query with parateters.
             * Without Dapper you can't create following code. So just Add "using Dapper" namespace at begin.
            */
            var data = db.Query<DeliveryReportVM>(query, parameters).ToList();
            return data;
        }
    }
}
