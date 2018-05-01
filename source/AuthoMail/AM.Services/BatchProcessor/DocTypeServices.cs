using AM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace AM.Services
{
    public interface IDocTypeService : IDisposable
    {
        List<DocType> GetDocTypesByClientId(int clientId);
        DocType GetDocTypeByDocTypeCode(int clientId, string docTypeCode);
        DocType GetDocTypeByDocTypeName(int clientId, string documentOrPackageName);
        DocType GetDocTypeById(int docTypeId);
        List<DocTypeCoverLetter> GetDocumentCoverLetterTemplates(int docTypeId);
    }

    public class DocTypeService : ServiceBase, IDocTypeService
    {
        public List<DocType> GetDocTypesByClientId(int clientId)
        {
            string query = "SELECT * FROM [AM].[DocTypes]  WHERE ClientId=@ClientId AND IsActive=1";

            return db.Query<DocType>(query, new { ClientId = clientId }).ToList();
        }

        public DocType GetDocTypeByDocTypeCode(int clientId, string docTypeCode)
        {
            string query = "SELECT * FROM [AM].[DocTypes]  WHERE ClientId=@clientId AND DocTypeCode=@docTypeCode  AND IsActive=1";

            return db.QuerySingleOrDefault<DocType>(query, new { clientId = clientId, docTypeCode = docTypeCode });
        }

        public DocType GetDocTypeByDocTypeName(int clientId, string documentOrPackageName)
        {
            string query = "SELECT * FROM [AM].[DocTypes] WHERE ClientId=@clientId AND Description=@documentOrPackageName  AND IsActive=1";

            return db.QuerySingleOrDefault<DocType>(query, new { clientId = clientId, documentOrPackageName = documentOrPackageName });
        }

        public DocType GetDocTypeById(int docTypeId)
        {
            string query = "SELECT * FROM [AM].[DocTypes] WHERE Id=@docTypeId  AND IsActive=1";

            return db.QuerySingleOrDefault<DocType>(query, new { docTypeId = docTypeId });
        }

        public List<DocTypeCoverLetter> GetDocumentCoverLetterTemplates(int docTypeId)
        {
            string query = "SELECT * FROM [AM].[DocTypeCoverLetters] WHERE DocTypeId=@docTypeId";

            return db.Query<DocTypeCoverLetter>(query, new { docTypeId = docTypeId }).ToList();
        }
    }
}
