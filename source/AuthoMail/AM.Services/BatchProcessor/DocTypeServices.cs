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

        DocType GetDocTypeByDocTypeName(int clientId, string documentOrPackageName, int pageCount, DateTime estReceivedOn);

        DocType GetDocTypeByDocTypeName(int clientId, string documentOrPackageName, int pageCount, string shippingCarrier, string shippingServiceType);
        List<DocType> GetFallBackDocTypeByPnSConfigId(int clientId, int docTypeId, int pnSConfigId);
        DocType GetDocTypeById(int docTypeId, int pnsConfigDetailId);
        List<PnSConfigDetailCoverLetter> GetDocumentCoverLetterTemplates(int pnSConfigDetailId);
        DocType GetDocTypeWithMaximumCutOffTimeByClientId(int clientId);
    }

    public class DocTypeService : ServiceBase, IDocTypeService
    {
        public DocType GetDocTypeByDocTypeName(int clientId, string documentOrPackageName, int pageCount)
        {
            string query = "SELECT * FROM [dbo].[v_DocTypes] D WHERE D.DocTypeName = @DocumentOrPackageName AND D.ClientId = @ClientId AND (1 - D.IsPrintReady + {0}) BETWEEN D.MinPage AND D.MaxPage AND D.IsFallBack = 0 AND D.IsActive = 1".Fill(pageCount);

            return db.QuerySingleOrDefault<DocType>(query, new { ClientId = clientId, DocumentOrPackageName = documentOrPackageName, PageCount = pageCount });
        }

        public DocType GetDocTypeByDocTypeName(int clientId, string documentOrPackageName, int pageCount, string shippingCarrier, string shippingServiceType)
        {
            string query = "SELECT * FROM [dbo].[v_DocTypes] D WHERE D.DocTypeName = @DocumentOrPackageName AND D.ClientId = @ClientId AND (1 - D.IsPrintReady + {0}) BETWEEN D.MinPage AND D.MaxPage AND D.IsActive = 1 AND D.ShippingCarrier=@ShippingCarrier AND D.ShippingServiceType=@ShippingServiceType".Fill(pageCount);

            return db.QuerySingleOrDefault<DocType>(query, new { ClientId = clientId, DocumentOrPackageName = documentOrPackageName, PageCount = pageCount, ShippingCarrier = shippingCarrier, ShippingServiceType = shippingServiceType });
        }

        public List<DocType> GetFallBackDocTypeByPnSConfigId(int clientId, int docTypeId, int pnSConfigId)
        {
            string query = "SELECT D.* FROM [dbo].[v_DocTypes] D WHERE D.PnSConfigId=@PnSConfigId AND D.ClientId=@ClientId AND D.Id=@DocTypeId AND D.IsActive=1 AND D.IsFallBack=1";

            return db.Query<DocType>(query, new { ClientId = clientId, DocTypeId = docTypeId, PnSConfigId = pnSConfigId }).ToList();
        }

        public DocType GetDocTypeById(int docTypeId, int pnsConfigDetailId)
        {
            string query = "SELECT D.* FROM [dbo].[v_DocTypes] D WHERE D.Id=@DocTypeId AND D.PnSConfigDetailId=@PnSConfigDetailId AND D.IsActive=1";

            return db.QuerySingleOrDefault<DocType>(query, new { DocTypeId = docTypeId, PnsConfigDetailId = pnsConfigDetailId });
        }

        public List<PnSConfigDetailCoverLetter> GetDocumentCoverLetterTemplates(int pnSConfigDetailId)
        {
            string query = "SELECT * FROM [Common].[PnSConfigDetailCoverLetters] WHERE PnSConfigDetailId=@PnSConfigDetailId";

            return db.Query<PnSConfigDetailCoverLetter>(query, new { PnSConfigDetailId = pnSConfigDetailId }).ToList();
        }

        public DocType GetDocTypeWithMaximumCutOffTimeByClientId(int clientId)
        {
            string query = "SELECT TOP 1 D.* FROM [dbo].[v_DocTypes] D WHERE BizdayCutOff=(SELECT MAX(CAST(BizdayCutOff as time)) as maxCutOff FROM [dbo].[v_DocTypes] WHERE ClientId=@ClientId AND IsActive=1) AND ClientId=@ClientId AND IsActive=1";

            return db.QuerySingleOrDefault<DocType>(query, new { ClientId = clientId });
        }

        public DocType GetDocTypeByDocTypeName(int clientId, string documentOrPackageName, int pageCount, DateTime estReceivedOn)
        {
            using (ICommonService commonSvc = new CommonService())
            {
                string query = "SELECT * FROM [dbo].[v_DocTypes] D WHERE D.DocTypeName = @DocumentOrPackageName AND D.ClientId = @ClientId AND (1 - D.IsPrintReady + {0}) BETWEEN D.MinPage AND D.MaxPage AND D.IsFallBack = 0 AND D.IsActive = 1".Fill(pageCount);

                var docTypeList = db.Query<DocType>(query, new { ClientId = clientId, DocumentOrPackageName = documentOrPackageName, PageCount = pageCount }).ToList();

                if (docTypeList.Count == 0)
                    return null;

                return GetDocTypeByDocTypeName(docTypeList, estReceivedOn);
            }
        }

        private DocType GetDocTypeByDocTypeName(List<DocType> docTypeList, DateTime estReceivedOn)
        {
            var estReceivedOnDate = estReceivedOn.Date;
            bool isSatWorkDay = true;
            bool isSunWorkDay = true;

            DayOfWeek dayOfWeek = estReceivedOn.DayOfWeek;
            string slaCutOff = string.Empty;
            string slaType = string.Empty;
            string maxDocTypeCutOffTime = "";

            if (dayOfWeek == DayOfWeek.Sunday)
                maxDocTypeCutOffTime = docTypeList.OrderByDescending(dt => dt.SunCutOff).First().SunCutOff;
            else if (dayOfWeek == DayOfWeek.Saturday)
                maxDocTypeCutOffTime = docTypeList.OrderByDescending(dt => dt.SatCutOff).First().SatCutOff;
            else
                maxDocTypeCutOffTime = docTypeList.OrderByDescending(dt => dt.BizdayCutOff).First().BizdayCutOff;

            var maxCutOffDateTimeEst = estReceivedOnDate + TimeSpan.Parse(maxDocTypeCutOffTime);


            using (ICommonService commonSvc = new CommonService())
            {
                if (!commonSvc.IsBusinessDay(estReceivedOnDate, Config.Common_Holidays, isSatWorkDay, isSunWorkDay) || (estReceivedOn > maxCutOffDateTimeEst))
                {
                    estReceivedOn = commonSvc.GetNextWorkingDay(estReceivedOnDate, Config.Common_Holidays, isSatWorkDay, isSunWorkDay);

                    //Recalculate same function for updated document type with new effective received date;
                    return GetDocTypeByDocTypeName(docTypeList, estReceivedOn);
                }

                estReceivedOnDate = estReceivedOn.Date;

                dayOfWeek = estReceivedOn.DayOfWeek;

                DocType maxCutOffDocType = null;

                if (dayOfWeek == DayOfWeek.Sunday)
                    maxCutOffDocType = docTypeList.Where(dt => estReceivedOn >= estReceivedOnDate + TimeSpan.Parse(dt.SunStartTime) && estReceivedOn <= estReceivedOnDate + TimeSpan.Parse(dt.SunEndTime)).First();
                else if (dayOfWeek == DayOfWeek.Saturday)
                    maxCutOffDocType = docTypeList.Where(dt => estReceivedOn >= estReceivedOnDate + TimeSpan.Parse(dt.SatStartTime) && estReceivedOn <= estReceivedOnDate + TimeSpan.Parse(dt.SatEndTime)).First();
                else
                    maxCutOffDocType = docTypeList.Where(dt => estReceivedOn >= estReceivedOnDate + TimeSpan.Parse(dt.BizdayStartTime) && estReceivedOn <= estReceivedOnDate + TimeSpan.Parse(dt.BizdayEndTime)).First();


                return maxCutOffDocType;
            }
        }
    }
}
