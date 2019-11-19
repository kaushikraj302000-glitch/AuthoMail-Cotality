using AM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AM.Lib;

namespace AM.Services
{
    public interface IDocTypeService : IDisposable
    {
        DocType GetDocTypeByDocTypeName(int clientId, string documentOrPackageName, int pageCount);
        List<DocType> GetFallBackDocTypeByPnSConfigId(int clientId, int pnSConfigId);
        DocType GetDocTypeById(int docTypeId, int pnsConfigDetailId);
        List<DocTypeCoverLetter> GetDocumentCoverLetterTemplates(int pnSConfigDetailId);
        DocType GetDocTypeWithMaximumCutOffTimeByClientId(int clientId);
    }

    public class DocTypeService : ServiceBase, IDocTypeService
    {
        public DocType GetDocTypeByDocTypeName(int clientId, string documentOrPackageName, int pageCount)
        {
            string query = "SELECT * FROM [dbo].[v_DocTypes] D WHERE D.DocTypeName = @DocumentOrPackageName AND D.ClientId = @ClientId AND (1 - D.IsPrintReady + {0}) BETWEEN D.MinPage AND D.MaxPage AND D.IsFallBack = 0 AND D.IsActive = 1".Fill(pageCount);

            return db.QuerySingleOrDefault<DocType>(query, new { ClientId = clientId, DocumentOrPackageName = documentOrPackageName, PageCount = pageCount });
        }

        public List<DocType> GetFallBackDocTypeByPnSConfigId(int clientId, int pnSConfigId)
        {
            string query = "SELECT D.* FROM [dbo].[v_DocTypes] D WHERE D.PnSConfigId=@PnSConfigId AND D.ClientId=@ClientId AND D.IsActive=1 AND D.IsFallBack=1";

            return db.Query<DocType>(query, new { ClientId = clientId, PnSConfigId = pnSConfigId }).ToList();
        }

        public DocType GetDocTypeById(int docTypeId, int pnsConfigDetailId)
        {
            string query = "SELECT D.* FROM [dbo].[v_DocTypes] D WHERE D.Id=@DocTypeId AND D.PnSConfigDetailId=@PnSConfigDetailId AND D.IsActive=1";

            return db.QuerySingleOrDefault<DocType>(query, new { DocTypeId = docTypeId, PnsConfigDetailId = pnsConfigDetailId });
        }

        public List<DocTypeCoverLetter> GetDocumentCoverLetterTemplates(int pnSConfigDetailId)
        {
            string query = "SELECT * FROM [AM].[DocTypeCoverLetters] WHERE PnSConfigDetailId=@PnSConfigDetailId";

            return db.Query<DocTypeCoverLetter>(query, new { PnSConfigDetailId = pnSConfigDetailId }).ToList();
        }

        public DocType GetDocTypeWithMaximumCutOffTimeByClientId(int clientId)
        {
            string query = "SELECT TOP 1 D.* FROM [dbo].[v_DocTypes] WHERE BizdayCutOff=(SELECT MAX(CAST(BizdayCutOff as time)) as maxCutOff FROM [dbo].[v_DocTypes] WHERE ClientId=@clientId AND IsActive=1) AND ClientId=@ClientId AND IsActive=1";

            return db.QuerySingleOrDefault<DocType>(query, new { ClientId = clientId });
        }
    }
}
