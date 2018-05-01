using AM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace AM.Services
{
    public interface IFedExService : IDisposable
    {
        FedExAccountInfo GetFedExAccountDetailsByAccountCode(string accountCode);
    }
    public class FedExService : ServiceBase, IFedExService
    {
        public FedExAccountInfo GetFedExAccountDetailsByAccountCode(string accountCode)
        {
            string query = @"SELECT [AccountCode],[WebAuthenticationKey],[WebAuthenticationPassword],[AccountNumber],[MeterNumber] FROM [AM].[ShippingAccounts] WHERE AccountCode=@AccountCode";

            return db.QuerySingleOrDefault<FedExAccountInfo>(query, new { AccountCode = accountCode });
        }
    }
}
