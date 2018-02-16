using AM.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace AM.Services.Services
{
    public interface IHomeService
    {
        List<DeliveryReportVM> GetDeliveryReportByBatchId(string batchId);
    }
    public class HomeService : ServiceBase, IHomeService
    {
        public List<DeliveryReportVM> GetDeliveryReportByBatchId(string batchId)
        {
            string query = "select * from [AM].[Batch]";
            //Dictionary<string, object> parameters = new Dictionary<string, object>();
            //parameters.Add("@batchId", batchId);

            var data = db.Query<DeliveryReportVM>(query).ToList();
            return data;
        }
    }
}
