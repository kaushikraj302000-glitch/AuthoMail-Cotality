using AM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace AM.Services
{
    public interface IDocTypeService
    {
        List<DocType> GetDocTypesByClientIdOrByDocTypeName(int? clientId, string docTypeName);
    }
    public class DocTypeService : ServiceBase, IDocTypeService
    {
        public List<DocType> GetDocTypesByClientIdOrByDocTypeName(int? clientId, string docTypeCode)
        {
            string query = "SELECT * FROM [AM].[DocTypes]";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            
            if (clientId.HasValue && string.IsNullOrEmpty(docTypeCode))
            {
                query = query + " WHERE ClientId=@clientId";
                parameters.Add("@clientId", clientId.Value);
            }
            else if (clientId==null && !string.IsNullOrEmpty(docTypeCode))
            {
                query = query + " WHERE DocTypeCode=@docTypeCode";
                parameters.Add("@DocTypeCode", docTypeCode);
            }
            else
            {
                query = query + " WHERE ClientId=@clientId AND DocTypeCode=@docTypeCode";
                parameters.Add("@DocTypeCode", docTypeCode);
                parameters.Add("@clientId", clientId.Value);
            }

            var data = db.Query<DocType>(query, parameters).ToList();
            return data;
        }
    }
}
