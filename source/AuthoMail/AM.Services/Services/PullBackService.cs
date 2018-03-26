using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AM.Lib;
using System.Data;
namespace AM.Services
{
    public interface IPullBackService
    {
        List<PullBackViewModel> GetMailPieceData(List<string> mpIds);
        List<PullBackRequestForm> GetPullBackData(List<string> mpIds);
        int UpdatePullBackRequestedOn(string mpId, string userId);
        int InsertPullBackRequestData(string mpId,int clientId);
        List<PullBackInfo> GetPullBackRequestList(string businessFriendlyIdName, string businessFriendlyIdValue, string docType, DateTime startDate, DateTime endDate, int clientId);
        List<QuickPullBackResponse> GetQuickPullBackResponseFromBusinessFriendlyValue(string businessFriendlyIdName, string businessFriendlyIdValue, int clientId);
        List<QuickPullBackResponse> GetQuickPullBackResponse(List<string> mailPieceIds, int clientId);
        List<PullBackUser> GetUsersForPullBack();
    }
    public class PullBackService : ServiceBase, IPullBackService
    {
        public List<PullBackViewModel> GetMailPieceData(List<string> mpIds)
        {
            string qry = string.Format("SELECT AMM.MailBatchId,PM.[UniqueId] as MailPieceUniqueId,PB.PrintBatch,PB.PipelineStatus,PM.MailPieceStatus,AMM.IsPullBackRequested,AMM.PullBackRequestedOn,AMM.PulledBackRequestedBy,PU.UserName FROM PnS.MRDFRequest PM INNER JOIN PnS.BatchSummary PB ON PM.JobId = PB.PrintBatch INNER JOIN AM.MailPieces AMM ON PM.[UniqueId] = AMM.UniqueId LEFT OUTER JOIN [Profile].Users PU ON PU.Id = AMM.PulledBackRequestedBy WHERE PM.[UniqueId] IN @mailPieceIds");
            var data = db.Query<PullBackViewModel>(qry, new { mailPieceIds = mpIds }).ToList();
            return data;
        }
        public List<PullBackRequestForm> GetPullBackData(List<string> mpIds)
        {
            string qry = string.Format(";WITH batchSummary AS (SELECT PrintFileName,SubmittedOn,ClientId,PrintBatch,DocTypeId  FROM PnS.BatchSummary),docTypes AS (SELECT Id,DocTypeCode FROM AM.DocTypes WHERE IsActive = 1),mrdfReq AS(SELECT JobId,PieceId,[UniqueId] FROM PnS.MRDFRequest),Client AS(SELECT Id,ClientName FROM Common.Clients),RESULT AS(SELECT PBS.PrintFileName,PBS.SubmittedOn,PMR.JobId,PMR.PieceId,CC.ClientName,AD.DocTypeCode FROM batchSummary PBS	INNER JOIN AM.DocTypes AD ON AD.Id = PBS.DocTypeId	INNER JOIN mrdfReq PMR ON PBS.PrintBatch = PMR.JobId INNER JOIN Client CC ON PBS.ClientId = CC.Id WHERE PMR.[UniqueId] IN @mailPieceIds)(SELECT PrintFileName,SubmittedOn,JobId AS PrintBatch,PieceId = STUFF((SELECT DISTINCT ','+ CAST(pRES.PieceId AS VARCHAR(255)) FROM RESULT pRES WHERE pRES.JobId = RES.JobId FOR XML PATH('')),1,1,''),ClientName,DocTypeCode FROM RESULT RES GROUP BY RES.JobId,PrintFileName,SubmittedOn,ClientName,DocTypeCode)");

            var data = db.Query<PullBackRequestForm>(qry, new { mailPieceIds = mpIds }).ToList();
            return data;
        }

        public int UpdatePullBackRequestedOn(string mpId, string userId)
        {
            string qry = string.Format(@"UPDATE AM.MailPieces SET PullBackRequestedOn = @utcDate,PulledBackRequestedBy = @UserId WHERE UniqueId = @id");
            var data = db.Execute(qry, new { utcDate = DateTime.UtcNow, UserId = string.IsNullOrEmpty(userId) ? DBNull.Value.ToString() : userId, id = mpId });
            return data;
        }

        public int InsertPullBackRequestData(string mpId,int clientId)
        {
            return db.Execute("[dbo].[usp_InsertPullBackRequestsData]", new { @MailPieceUniqueId = mpId,@ClientId = clientId }, commandType: CommandType.StoredProcedure);
        }

        public List<PullBackInfo> GetPullBackRequestList(string businessFriendlyIdName, string businessFriendlyIdValue, string docType, DateTime startDate, DateTime endDate, int clientId)
        {
            string query = PullBackServiceQuery.GetPullBackRequestList;
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@startDate", startDate.EstToUtc());
            parameters.Add("@endDate", endDate.EstToUtc());
            parameters.Add("@clientId", clientId);
            if (!businessFriendlyIdName.IsNullOrWhiteSpace() && !businessFriendlyIdValue.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} AND (m.[BusinessFriendlyIdName]=@businessFriendlyIdName AND m.BusinessFriendlyIdValue=@businessFriendlyIdValue)", query);
                parameters.Add("@businessFriendlyIdName", businessFriendlyIdName);
                parameters.Add("@businessFriendlyIdValue", businessFriendlyIdValue);
            }
            if (!docType.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} AND (m.DocumentOrPackageType=@docType OR b.DocumentOrPackageType=@docType)", query);
                parameters.Add("@docType", docType);
            }
            var data = db.Query<PullBackInfo>(query, parameters).ToList();
            return data;
        }
        public List<QuickPullBackResponse> GetQuickPullBackResponseFromBusinessFriendlyValue(string businessFriendlyIdName, string businessFriendlyIdValue, int clientId)
        {
            string query = PullBackServiceQuery.GetQuickPullBackResponseFromBusinessFriendlyValue;
            var mailPieces = db.Query<QuickPullBackResponse>(query, new { businessFriendlyIdName = businessFriendlyIdName, businessFriendlyIdValue = businessFriendlyIdValue, clientId = clientId }).ToList();
            return mailPieces;
        }
        public List<QuickPullBackResponse> GetQuickPullBackResponse(List<string> mailPieceIds, int clientId)
        {
            string query = PullBackServiceQuery.GetQuickPullBackResponse;
            var response = db.Query<QuickPullBackResponse>(query, new { clientId = clientId, mailPieceIds = mailPieceIds }).ToList();
            return response;
        }


        public List<PullBackUser> GetUsersForPullBack()
        {
            string query = "SELECT DISTINCT Id,UserName FROM [Profile].[Users] WHERE Id IN(SELECT distinct RequestedBy  FROM [AuthoMail].[AM].[PullBackRequests])";
            return db.Query<PullBackUser>(query).ToList();
        }

    }

    public static class PullBackServiceQuery
    {
        public static string GetPullBackRequestList = "select b.ClientBatchId,c.TimeZone as [ClientTimeZone],m.[Id] as [MailPieceId],m.[UniqueId] as [MailPieceUniqueId], b.Id as [BatchId],b.[UniqueId] as [BatchUniqueId],b.BatchName, m.BusinessFriendlyIdValue,m.BusinessFriendlyIdName,m.[FileName], m.DocumentOrPackageType,m.[RcptPrefix],m.[RcptFirstName],m.[RcptMiddleName], m.[RcptLastName],[RcptSuffix],b.ReceivedOn,m.TotalPages,m.TrackingNo,m.[ShippingStatus], m.IsPullBackRequested,m.PrintedOn,d.ShippingCarrier as [ShippingMethod], m.RcptOrganization,m.[RcptAddrOptionalLine1AboveStreet],m.[RcptAddrOptionalLine2AboveStreet], m.[RcptAddressStreet],m.[RcptAddressAptOrSuiteOrOther],m.[RcptAddrOptionalLine1BelowStreet], m.[RcptAddrOptionalLine2BelowStreet],m.[RcptAddressCity],m.[RcptAddressState], m.[RcptAddressZIP],m.[RetRcptPrefix],m.[RetRcptFirstName],m.[RetRcptMiddleName], m.[RetRcptLastName],m.[RetRcptSuffix],m.[RetRcptOrganization], m.[RetRcptAddrOptionalLine1AboveStreet],m.[RetRcptAddrOptionalLine2AboveStreet], m.[RetRcptAddressAptOrSuiteOrOther],m.[RetRcptAddrOptionalLine1BelowStreet], m.[RetRcptAddrOptionalLine2BelowStreet],m.[RetRcptAddressCity],m.[RetRcptAddressState], m.[RetRcptAddressZIP],m.PulledBackOn,u.UserName as [PulledBackBy] from [AM].[MailBatches] b, [AM].[PrintBatches] p,[AM].[DocTypes] d,[Common].[Clients] c,[AM].[MailPieces] m left outer join [Profile].[Users] u on m.[PulledBackRequestedBy]=u.Id where ((m.MailBatchId=b.Id and m.ClientId=c.Id and p.Id=m.PrintBatchId and d.Id=p.DocTypeId) and b.ClientId=@clientId) AND ((cast(b.ReceivedOn as date)>=cast (@startDate as date) and cast(b.ReceivedOn as date)<= cast(@endDate as date)))";
        public static string GetQuickPullBackResponseFromBusinessFriendlyValue = "select b.ClientBatchId,c.TimeZone as [ClientTimeZone], m.Id as [MailPieceId], m.UniqueId as [MailPieceUniqueId],b.Id as [BatchId],b.UniqueId as [BatchUniqueId],b.BatchName,b.ReceivedOn,m.BusinessFriendlyIdName,m.BusinessFriendlyIdValue,m.[FileName],m.[ShippingStatus],m.IsPullBackRequested from [AM].[MailBatches] b,[AM].[MailPieces] m, [Common].[Clients] c where b.Id = m.MailBatchId and m.BusinessFriendlyIdName=@businessFriendlyIdName and m.ClientId=c.Id  m.BusinessFriendlyIdValue=@businessFriendlyIdValue AND b.ClientId=@clientId";
        public static string GetQuickPullBackResponse = "select b.ClientBatchId,c.TimeZone as [ClientTimeZone], m.Id as [MailPieceId], m.UniqueId as [MailPieceUniqueId],b.Id as [BatchId],b.UniqueId as [BatchUniqueId],b.BatchName,b.ReceivedOn,m.BusinessFriendlyIdName,m.BusinessFriendlyIdValue,m.[FileName]m.ShippingStatus,m.IsPullBackRequested from [AM].[MailBatches] b,[AM].[MailPieces] m, [Common].[Clients] c where b.Id =m.MailBatchId and m.ClientId=c.Id and m.UniqueId=@mailPieceIds AND b.ClientId=@clientId";
    }
}
