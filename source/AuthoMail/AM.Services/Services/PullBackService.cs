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
        List<PullBackInfo> GetPullBackRequestList(string loanNo, string docType, DateTime startDate, DateTime endDate, int clientId);

        List<QuickPullBackResponse> GetQuickPullBackResponseFromLoanNo(string loanNo, int clientId);

        List<QuickPullBackResponse> GetQuickPullBackResponse(List<string> mailPieceIds, int clientId);
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

        public List<PullBackInfo> GetPullBackRequestList(string loanNo, string docType, DateTime startDate, DateTime endDate, int clientId)
        {
            string query = PullBackServiceQuery.GetPullBackRequestList;
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@startDate", startDate);
            parameters.Add("@endDate", endDate);
            parameters.Add("@clientId", clientId);
            if (!loanNo.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} AND (m.[BusinessFriendlyIdName]='LoanNumber' AND m.BusinessFriendlyIdValue=@loanNo)", query);
                parameters.Add("@loanNo", loanNo);
            }
            if (!docType.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} AND (m.DocumentOrPackageType=@docType OR b.DocumentOrPackageType=@docType)", query);
                parameters.Add("@docType", docType);
            }
            var data = db.Query<PullBackInfo>(query, parameters).ToList();
            return data;
        }
        public List<QuickPullBackResponse> GetQuickPullBackResponseFromLoanNo(string loanNo, int clientId)
        {
            string query = PullBackServiceQuery.GetQuickPullBackResponseFromLoanNo;
            var mailPieces = db.Query<QuickPullBackResponse>(query, new { loanNo = loanNo, clientId = clientId }).ToList();
            return mailPieces;
        }
        public List<QuickPullBackResponse> GetQuickPullBackResponse(List<string> mailPieceIds, int clientId)
        {
            string query = PullBackServiceQuery.GetQuickPullBackResponse;
            var response = db.Query<QuickPullBackResponse>(query, new { clientId = clientId, mailPieceIds = mailPieceIds }).ToList();
            return response;
        }
    }

    public static class PullBackServiceQuery
    {
        public static string GetPullBackRequestList = "select m.[Id] as [MailPieceId],m.[UniqueId] as [MailPieceUniqueId],b.Id as [BatchId],b.[UniqueId] as [BatchUniqueId],b.BatchName,m.BusinessFriendlyIdValue,m.BusinessFriendlyIdName,m.[FileName],m.DocumentOrPackageType as [MailPieceDocumentOrPackageType],m.[RcptPrefix],m.[RcptFirstName],m.[RcptMiddleName],m.[RcptLastName],[RcptSuffix],b.DocumentOrPackageType as [BatchDocumentOrPackageType],b.ReceivedOn,m.TotalPages,m.TrackingNo,m.[ShippingStatus],m.PrintedOn,d.ShippingCarrier as [ShippingMethod],m.RcptOrganization,m.[RcptAddrOptionalLine1AboveStreet],m.[RcptAddrOptionalLine2AboveStreet],m.[RcptAddressStreet],m.[RcptAddressAptOrSuiteOrOther],m.[RcptAddrOptionalLine1BelowStreet],m.[RcptAddrOptionalLine2BelowStreet],m.[RcptAddressCity],m.[RcptAddressState],m.[RcptAddressZIP],m.[RetRcptPrefix],m.[RetRcptFirstName],m.[RetRcptMiddleName],m.[RetRcptLastName],m.[RetRcptSuffix],m.[RetRcptOrganization],m.[RetRcptAddrOptionalLine1AboveStreet],m.[RetRcptAddrOptionalLine2AboveStreet],m.[RetRcptAddressAptOrSuiteOrOther],m.[RetRcptAddrOptionalLine1BelowStreet],m.[RetRcptAddrOptionalLine2BelowStreet],m.[RetRcptAddressCity],m.[RetRcptAddressState],m.[RetRcptAddressZIP],m.PulledBackOn,u.UserName as [PulledBackBy] from [AM].[MailBatches] b, [AM].[PrintBatches] p,[AM].[DocTypes] d,[AM].[MailPieces] m left outer join [Profile].[Users] u on m.[PulledBackRequestedBy]=u.Id where ((m.BatchId=b.Id and p.Id=m.PrintBatchId and d.Id=p.DocTypeId) and b.ClientId=@clientId) AND ((cast(b.ReceivedOn as date)>=@startDate and cast(b.ReceivedOn as date)<=@endDate))";
        public static string GetQuickPullBackResponseFromLoanNo = "select m.Id as [MailPieceId], m.UniqueId as [MailPieceUniqueId],b.Id as [BatchId],b.UniqueId as [BatchUniqueId],b.BatchName,b.ReceivedOn,m.BusinessFriendlyIdName,m.BusinessFriendlyIdValue,m.[FileName],m.[ShippingStatus] from [AM].[MailBatches] b,[AM].[MailPieces] m where b.Id =m.BatchId and m.BusinessFriendlyIdValue=@loanNo AND b.ClientId=@clientId";
        public static string GetQuickPullBackResponse = "select m.Id as [MailPieceId], m.UniqueId as [MailPieceUniqueId],b.Id as [BatchId],b.UniqueId as [BatchUniqueId],b.BatchName,b.ReceivedOn,m.BusinessFriendlyIdName,m.BusinessFriendlyIdValue,m.[FileName]m.ShippingStatus from [AM].[MailBatches] b,[AM].[MailPieces] m where b.Id =m.BatchId and m.UniqueId=@mailPieceIds AND b.ClientId=@clientId";
    }
}
