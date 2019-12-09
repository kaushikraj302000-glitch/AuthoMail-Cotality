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

        DocType GetDocTypeByDocTypeName(int clientId, string documentOrPackageName, int pageCount, DateTime receivedOn);

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

        public DocType GetDocTypeByDocTypeName(int clientId, string documentOrPackageName, int pageCount, DateTime utcReceivedOn)
        {
            var estReceivedOn = utcReceivedOn.UtcToEst();
            var estReceivedOnDate = estReceivedOn.Date;
            var effectiveEstReceivedOn = estReceivedOn;

            DayOfWeek dayOfWeek = estReceivedOn.DayOfWeek;
            string query;
            List<DocType> docTypeList;
            string slaCutOff = string.Empty;
            string slaType = string.Empty;
            using (ICommonService commonSvc = new CommonService())
            {
                query = "SELECT * FROM [dbo].[v_DocTypes] D WHERE D.DocTypeName = @DocumentOrPackageName AND D.ClientId = @ClientId AND (1 - D.IsPrintReady + {0}) BETWEEN D.MinPage AND D.MaxPage AND D.IsFallBack = 0 AND D.IsActive = 1".Fill(pageCount);

                docTypeList = db.Query<DocType>(query, new { ClientId = clientId, DocumentOrPackageName = documentOrPackageName, PageCount = pageCount }).ToList();

                if (docTypeList.Count > 0)
                {
                    var maxDocTypeCutOff = docTypeList.OrderByDescending(dt => dt.BizdayCutOff).FirstOrDefault();
                    bool isSatWorkDay = maxDocTypeCutOff.SatSLAType == SLAType.NA ? false : true;
                    bool isSunWorkDay = maxDocTypeCutOff.SunSLAType == SLAType.NA ? false : true;

                    if (commonSvc.IsBusinessDay(estReceivedOn, Config.Common_Holidays, isSatWorkDay, isSunWorkDay))
                    {
                        if (dayOfWeek == DayOfWeek.Sunday)
                        {
                            slaCutOff = maxDocTypeCutOff.SunCutOff;
                            slaType = maxDocTypeCutOff.SunSLAType;
                        }

                        if (dayOfWeek == DayOfWeek.Saturday)
                        {
                            slaCutOff = maxDocTypeCutOff.SatCutOff;
                            slaType = maxDocTypeCutOff.SatSLAType;
                        }

                        if (dayOfWeek != DayOfWeek.Sunday && dayOfWeek != DayOfWeek.Saturday)
                        {
                            slaCutOff = maxDocTypeCutOff.BizdayCutOff;
                            slaType = maxDocTypeCutOff.BizdaySLAType;
                        }

                        if (slaType == SLAType.FixedTime)
                        {
                            var estSLACutOffDateTime = (estReceivedOnDate + TimeSpan.Parse(slaCutOff));

                            if (estSLACutOffDateTime > estReceivedOn)
                            {
                                var selectedDocType = docTypeList.Where(dt => estReceivedOn >= estReceivedOnDate + TimeSpan.Parse(dt.SunStartTime) && estReceivedOn <= estReceivedOnDate + TimeSpan.Parse(dt.SunEndTime)).FirstOrDefault();

                                return selectedDocType;
                            }
                            else
                            {
                                estReceivedOn = commonSvc.GetNextWorkingDay(estReceivedOnDate, Config.Common_Holidays, isSatWorkDay, isSunWorkDay);

                                //Recalculate same function for updated document type with new effective received date;
                                return GetDocTypeByDocTypeName(clientId, documentOrPackageName, pageCount, estReceivedOn.EstToUtc());
                            }
                        }
                        else
                        {
                            var selectedDocType = docTypeList.Where(dt => estReceivedOn >= estReceivedOnDate + TimeSpan.Parse(dt.SunStartTime) && estReceivedOn <= estReceivedOnDate + TimeSpan.Parse(dt.SunEndTime)).FirstOrDefault();

                            return selectedDocType;
                        }
                    }
                    else
                    {
                        estReceivedOn = commonSvc.GetNextWorkingDay(estReceivedOnDate, Config.Common_Holidays, isSatWorkDay, isSunWorkDay);

                        //Recalculate same function for updated document type with new effective received date;
                        return GetDocTypeByDocTypeName(clientId, documentOrPackageName, pageCount, estReceivedOn.EstToUtc());
                    }
                }
            }

            return null;
        }
    }
}
