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

        int UpdatePullBackRequestedOn(string mpId);
    }
    public class PullBackService : ServiceBase, IPullBackService
    {

        public List<dynamic> GetMailPieceData(List<string> mpIds)
        {
            //string qry = string.Format(";WITH batchSummary AS(SELECT PrintFileName,SubmittedOn,DocTypeCode,ClientId,PrintBatch  FROM PnS.BatchSummary),mrdfReq AS(SELECT JobId,PieceId,[UniqueIdentifier],MailPieceStatus FROM PnS.MRDFRequest),Client AS(SELECT Id,ClientName FROM Common.Clients),RESULT AS(SELECT PBS.PrintFileName,PBS.SubmittedOn,PMR.JobId,PMR.PieceId,CC.ClientName,PBS.DocTypeCode,PMR.[UniqueIdentifier],PMR.MailPieceStatus FROM batchSummary PBS INNER JOIN mrdfReq PMR ON PBS.PrintBatch = PMR.JobId INNER JOIN Client CC 	ON PBS.ClientId = CC.Id	WHERE PMR.[UniqueIdentifier] IN @mailPieceIds)(SELECT 	PrintFileName,SubmittedOn,JobId AS PrintBatch,PieceId = STUFF((SELECT DISTINCT ','+ CAST(pRES.PieceId AS VARCHAR(255)) FROM RESULT pRES WHERE pRES.JobId = RES.JobId FOR XML PATH('')),1,1,''),ClientName,DocTypeCode,[UniqueIdentifier],MailPieceStatus FROM RESULT RES)");

            string qry = string.Format("SELECT PM.[UniqueIdentifier],PB.PrintBatch,PB.PipelineStatus,PM.MailPieceStatus,AMM.IsPullBackRequested,AMM.PullBackRequestedOn FROM PnS.MRDFRequest PM INNER JOIN PnS.BatchSummary PB ON PM.JobId = PB.PrintBatch INNER JOIN AM.MailPiece AMM ON PM.[UniqueIdentifier] = AMM.Id WHERE PM.[UniqueIdentifier] IN @mailPieceIds");
            var data = db.Query(qry, new { mailPieceIds = mpIds }).ToList();
            return data;
        }
        public List<dynamic> GetPullBackData(List<string> mpIds)
        {
            string qry = string.Format(";WITH batchSummary AS(SELECT PrintFileName,SubmittedOn,DocTypeCode,ClientId,PrintBatch  FROM PnS.BatchSummary),mrdfReq AS(SELECT JobId,PieceId,[UniqueIdentifier] FROM PnS.MRDFRequest),Client AS(SELECT Id,ClientName FROM Common.Clients),RESULT AS(SELECT PBS.PrintFileName,PBS.SubmittedOn,PMR.JobId,PMR.PieceId,CC.ClientName,PBS.DocTypeCode FROM batchSummary PBS		INNER JOIN mrdfReq PMR 	ON PBS.PrintBatch = PMR.JobId INNER JOIN Client CC 	ON PBS.ClientId = CC.Id	WHERE PMR.[UniqueIdentifier] IN @mailPieceIds)(SELECT 	PrintFileName,SubmittedOn,JobId AS PrintBatch,PieceId = STUFF((SELECT DISTINCT ','+ CAST(pRES.PieceId AS VARCHAR(255)) FROM RESULT pRES WHERE pRES.JobId = RES.JobId FOR XML PATH('')),1,1,''),ClientName,DocTypeCode FROM RESULT RES GROUP BY RES.JobId,PrintFileName,SubmittedOn,ClientName,DocTypeCode)");

            var data = db.Query(qry, new { mailPieceIds = mpIds }).ToList();
            return data;
        }

        public int UpdatePullBackRequestedOn(string mpId)
        {
            string qry = string.Format(@"UPDATE AM.MailPiece SET PullBackRequestedOn = @utcDate WHERE Id = @id");
            var data = db.Execute(qry, new { utcDate = DateTime.UtcNow, id = mpId });
            return data;
        }

    }
}
