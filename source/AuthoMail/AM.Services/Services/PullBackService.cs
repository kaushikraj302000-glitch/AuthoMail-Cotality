using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace AM.Services.Services
{
    public interface IPullBackService
    {
        List<dynamic> GetPullBackData(List<string> mpIds);
    }
    public class PullBackService : ServiceBase,IPullBackService
    {
        public List<dynamic> GetPullBackData(List<string> mpIds)
        {
            string qry = string.Format(";WITH batchSummary AS(SELECT PrintFileName,SubmittedOn,DocTypeCode,ClientId,PrintBatch  FROM PnS.BatchSummary),mrdfReq AS(SELECT JobId,PieceId,[UniqueIdentifier] FROM PnS.MRDFRequest),Client AS(SELECT Id,ClientName FROM Common.Clients),RESULT AS(SELECT PBS.PrintFileName,PBS.SubmittedOn,PMR.JobId,PMR.PieceId,CC.ClientName,PBS.DocTypeCode FROM batchSummary PBS		INNER JOIN mrdfReq PMR 	ON PBS.PrintBatch = PMR.JobId INNER JOIN Client CC 	ON PBS.ClientId = CC.Id	WHERE PMR.[UniqueIdentifier] IN @mailPieceIds)(SELECT 	PrintFileName,SubmittedOn,JobId AS PrintBatch,PieceId = STUFF((SELECT DISTINCT ','+ CAST(pRES.PieceId AS VARCHAR(255)) FROM RESULT pRES WHERE pRES.JobId = RES.JobId FOR XML PATH('')),1,1,''),ClientName,DocTypeCode FROM RESULT RES GROUP BY RES.JobId,PrintFileName,SubmittedOn,ClientName,DocTypeCode)");
            var data = db.Query(qry, new { mailPieceIds = mpIds }).ToList();
            return data;
        }
    }
}
