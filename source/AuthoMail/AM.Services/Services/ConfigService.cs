using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace AM.Services
{
    public interface IConfigService
    {
        Dictionary<dynamic, dynamic> GetSharedStrings();
    }
    public class ConfigService : ServiceBase, IConfigService
    {
        public Dictionary<dynamic, dynamic> GetSharedStrings()
        {
            string qry = string.Format("SELECT [Key],Value FROM [AM].[SharedStrings]");
            var data = db.Query(qry).ToList();
            var res = data.ToDictionary(x => x, x => x);
            return res;
        }
    }
}
