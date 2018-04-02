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
        List<DocType> GetDocTypesByClientId(int clientId);
        DocType GetDocTypeByDocTypeCode(int clientId, string docTypeCode);
        DocType GetDocTypesByDocTypeName(int clientId, string documentOrPackageName);
        List<DocTypeCoverLetter> GetDocumentCoverLetterTemplates(int docTypeId);
    }

    public class DocTypeService : ServiceBase, IDocTypeService
    {
        public List<DocType> GetDocTypesByClientId(int clientId)
        {
            string query = "SELECT * FROM [AM].[DocTypes]  WHERE ClientId=@ClientId";

            var data = db.Query<DocType>(query, new { ClientId = clientId }).ToList();
            return data;
        }

        public DocType GetDocTypeByDocTypeCode(int clientId, string docTypeCode)
        {
            string query = "SELECT * FROM [AM].[DocTypes]  WHERE ClientId=@clientId AND DocTypeCode=@docTypeCode";

            var data = db.Query<DocType>(query, new { clientId = clientId, docTypeCode = docTypeCode }).SingleOrDefault();
            return data;
        }

        public DocType GetDocTypesByDocTypeName(int clientId, string documentOrPackageName)
        {
            string query = "SELECT * FROM [AM].[DocTypes] WHERE ClientId=@clientId AND Description=@documentOrPackageName";

            var data = db.Query<DocType>(query, new { clientId = clientId, documentOrPackageName = documentOrPackageName }).FirstOrDefault();
            return data;
        }

        public List<DocTypeCoverLetter> GetDocumentCoverLetterTemplates(int docTypeId)
        {
            string query = "SELECT * FROM [AM].[DocTypeCoverLetters] WHERE DocTypeId=@docTypeId";

            var data = db.Query<DocTypeCoverLetter>(query, new { docTypeId = docTypeId }).ToList();
            return data;
        }
    }
}
