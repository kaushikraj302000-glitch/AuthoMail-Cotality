using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace AM.Services
{
    public interface IPullBackService
    {
        List<dynamic> GetMailPieceData(List<string> mpIds);
        List<dynamic> GetPullBackData(List<string> mpIds);

        int UpdatePullBackRequestedOn(string mpId, string userId);
    }
    public class PullBackService : ServiceBase, IPullBackService
    {

        public List<dynamic> GetMailPieceData(List<string> mpIds)
        {
            string qry = string.Format("SELECT PM.[UniqueId],PB.PrintBatch,PB.PipelineStatus,PM.MailPieceStatus,AMM.IsPullBackRequested,AMM.PullBackRequestedOn,AMM.PulledBackRequestedBy,PU.UserName FROM PnS.MRDFRequest PM INNER JOIN PnS.BatchSummary PB ON PM.JobId = PB.PrintBatch INNER JOIN AM.MailPieces AMM ON PM.[UniqueId] = AMM.UniqueId LEFT OUTER JOIN [Profile].Users PU ON PU.Id = AMM.PulledBackRequestedBy WHERE PM.[UniqueId] IN @mailPieceIds");
            var data = db.Query(qry, new { mailPieceIds = mpIds }).ToList();
            return data;
        }
        public List<dynamic> GetPullBackData(List<string> mpIds)
        {
            string qry = string.Format(";WITH batchSummary AS (SELECT PrintFileName,SubmittedOn,ClientId,PrintBatch,DocTypeId  FROM PnS.BatchSummary),docTypes AS (SELECT Id,DocTypeCode FROM AM.DocTypes WHERE IsActive = 1),mrdfReq AS(SELECT JobId,PieceId,[UniqueId] FROM PnS.MRDFRequest),Client AS(SELECT Id,ClientName FROM Common.Clients),RESULT AS(SELECT PBS.PrintFileName,PBS.SubmittedOn,PMR.JobId,PMR.PieceId,CC.ClientName,AD.DocTypeCode FROM batchSummary PBS	INNER JOIN AM.DocTypes AD ON AD.Id = PBS.DocTypeId	INNER JOIN mrdfReq PMR ON PBS.PrintBatch = PMR.JobId INNER JOIN Client CC ON PBS.ClientId = CC.Id WHERE PMR.[UniqueId] IN @mailPieceIds)(SELECT PrintFileName,SubmittedOn,JobId AS PrintBatch,PieceId = STUFF((SELECT DISTINCT ','+ CAST(pRES.PieceId AS VARCHAR(255)) FROM RESULT pRES WHERE pRES.JobId = RES.JobId FOR XML PATH('')),1,1,''),ClientName,DocTypeCode FROM RESULT RES GROUP BY RES.JobId,PrintFileName,SubmittedOn,ClientName,DocTypeCode)");

            var data = db.Query(qry, new { mailPieceIds = mpIds }).ToList();
            return data;
        }

        public int UpdatePullBackRequestedOn(string mpId, string userId)
        {
            string qry = string.Format(@"UPDATE AM.MailPieces SET PullBackRequestedOn = @utcDate,PulledBackRequestedBy = @UserId WHERE UniqueId = @id");            
            var data = db.Execute(qry, new { utcDate = DateTime.UtcNow, UserId = string.IsNullOrEmpty(userId) ? DBNull.Value.ToString() : userId, id = mpId });
            return data;
        }

    }
}
