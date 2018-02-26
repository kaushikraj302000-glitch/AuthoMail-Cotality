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
        List<dynamic> GetSharedStrings();
    }
    public class ConfigService : ServiceBase, IConfigService
    {
        public List<dynamic> GetSharedStrings()
        {
            string qry = string.Format("SELECT [Key],Value FROM [AM].[SharedStrings]");
            var data = db.Query(qry).ToList();            
            return data;
        }
    }
}
