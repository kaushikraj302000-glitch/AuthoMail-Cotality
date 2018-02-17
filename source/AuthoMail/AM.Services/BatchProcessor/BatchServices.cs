using AM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AM.Entities;

namespace AM.Services
{
    public interface IBatchService//Home Service interface that has number of methods
    {
        List<Client> GetClientList(int? clientId, bool isPnSActive);
    }
    public class BatchService : ServiceBase, IBatchService //It implements Interfaces 
    {
        public List<Client> GetClientList(int? clientId, bool isPnSActive)
        {
            //Sql Query
            string query = "SELECT * FROM [Common].[Clients] WHERE IsClientActive=@isClientActive AND IsPnSActive=@isPnSActive";

            //Assign value to parameters It may one or more. Note:
            //List of parameters declaration
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@isClientActive", true);
            parameters.Add("@isPnSActive", isPnSActive);

            if (clientId.HasValue)
            {
                query = query + " AND Id=@clientId";
                parameters.Add("@clientId", clientId.Value);
            }

            /*
             * Execution of query with parateters.
             * Without Dapper you can't create following code. So just Add "using Dapper" namespace at begin.
            */
            var data = db.Query<Client>(query, parameters).ToList();
            return data;
        }
    }
}
