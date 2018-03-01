using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using AM.Services.Models;
using AM.Entities;

namespace AM.Services
{
    public interface IReportService
    {
        List<SummaryControlReport> GetSummaryControlReport(int clientId, DateTime startDate, DateTime endDate, string loanNo, string docType);
        List<DeliveryReport> GetDeliveryByParam(int clientId, DateTime startDate, DateTime endDate, string loanNo, string docType);
        List<ErrorReport> GetErrorReportByParam(int clientId, DateTime startDate, DateTime endDate, string loanNo, string docType);
        List<MonthlyReconReport> GetMonthlyReconReport(int clientId, DateTime startDate, DateTime endDate, string loanNo, string docType);
        //Nedd to move
        List<MailPieceSearch> GetMailPieceByBatchId(int batchId, int clientId);
        List<DocumentType> GetDocTypeList(int clientId);
        List<string> GetMailPieceIdFromLoanNo(string loanNo, int clientId);
        //Above done


        //Pullback Request
        List<PullBackInfo> GetPullBackRequestList(string loanNo, string docType, DateTime startDate, DateTime endDate, int clientId);

        List<QuickPullBackResponse> GetQuickPullBackResponseFromLoanNo(string loanNo, int clientId);

        List<QuickPullBackResponse> GetQuickPullBackResponse(List<string> mailPieceIds, int clientId);
    }
    public class ReportService : ServiceBase, IReportService
    {
        public List<SummaryControlReport> GetSummaryControlReport(int clientId, DateTime startDate, DateTime endDate, string loanNo, string docType)
        {
            string query = ReportServiceQuery.GetSummaryControlReport;
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@clientId", clientId);
            parameters.Add("@startDate", startDate);
            parameters.Add("@endDate", endDate);
            if (!loanNo.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} AND (BusinessFriendlyIdName='LoanNumber' AND LoanNumber=@loanNo)", query);
                parameters.Add("@loanNo", loanNo);
            }
            if (!docType.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} AND (MailPieceDocumentPackageName=@docType OR BatchDocumentPackageName=@docType)", query);
                parameters.Add("@docType", docType);
            }
            var data = db.Query<SummaryControlReport>(query, parameters).ToList();
            return data;
        }
        public List<DeliveryReport> GetDeliveryByParam(int clientId, DateTime startDate, DateTime endDate, string loanNo, string docType)
        {
            string query = ReportServiceQuery.GetDeliveryByParam;

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@clientId", clientId);
            parameters.Add("@startDate", startDate);
            parameters.Add("@endDate", endDate);
            if (!loanNo.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} AND (BusinessFriendlyIdName='LoanNumber' AND LoanNumber=@loanNo)", query);
                parameters.Add("@loanNo", loanNo);
            }
            if (!docType.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} AND (MailPieceDocumentPackageName=@docType OR BatchDocumentPackageName=@docType)", query);
                parameters.Add("@docType", docType);
            }
            var data = db.Query<DeliveryReport>(query, parameters).ToList();
            return data;
        }


        public List<ErrorReport> GetErrorReportByParam(int clientId, DateTime startDate, DateTime endDate, string loanNo, string docType)
        {
            string query = ReportServiceQuery.GetErrorReportByParam;

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@clientId", clientId);
            parameters.Add("@startDate", startDate);
            parameters.Add("@endDate", endDate);
            if (!loanNo.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} AND (BusinessFriendlyIdName='LoanNumber' AND LoanNumber=@loanNo)", query);
                parameters.Add("@loanNo", loanNo);
            }
            if (!docType.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} AND (MailPieceDocumentPackageName=@docType OR BatchDocumentPackageName=@docType)", query);
                parameters.Add("@docType", docType);
            }
            var data = db.Query<ErrorReport>(query, parameters).ToList();
            return data;
        }


        public List<MonthlyReconReport> GetMonthlyReconReport(int clientId, DateTime startDate, DateTime endDate, string loanNo, string docType)
        {
            string query = ReportServiceQuery.GetMonthlyReconReport;

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@clientId", clientId);
            parameters.Add("@startDate", startDate);
            parameters.Add("@endDate", endDate);
            if (!loanNo.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} AND (BusinessFriendlyIdName='LoanNumber' AND LoanNumber=@loanNo)", query);
                parameters.Add("@loanNo", loanNo);
            }
            if (!docType.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} AND (MailPieceDocumentPackageName=@docType OR BatchDocumentPackageName=@docType)", query);
                parameters.Add("@docType", docType);
            }
            var data = db.Query<MonthlyReconReport>(query, parameters).ToList();
            return data;
        }


        public List<MailPieceSearch> GetMailPieceByBatchId(int batchId, int clientId)
        {
            string query = ReportServiceQuery.GetMailPieceByBatchId;
            var data = db.Query<MailPieceSearch>(query, new { batchId = batchId, clientId = clientId }).ToList();
            return data;
        }
        //public List<MonthlyReconReport> GetMailPieceByLoanNo(string loanNo)
        //{
        //    string query = "select m.BusinessFriendlyIdValue as [LoanNo],[FileName],m.DocumentOrPackageType,m.RetRcptFirstName,m.RetRcptMiddleName,m.RetRcptLastName,m.RcptAddressStreet,m.RcptAddressCity,m.RcptAddressState,m.RcptAddressZIP,b.ReceivedOn,m.TotalPages,m.TrackingNo,b.PipelineStatus as [Status],m.ShippedOn as [PrintedOn],m.ShippedOn as [MailedOn],d.ShippingCarrier as [ShippingMethod]from [AM].[MailPiece] m,[AM].[Batch] b left outer join [AM].[DocTypes] d on d.ClientId=b.ClientId where (m.BatchClustId=b.BatchClustId) and m.BusinessFriendlyIdValue=@loanNo";
        //    Dictionary<string, object> parameters = new Dictionary<string, object>();
        //    parameters.Add("@loanNo", loanNo);
        //    var data = db.Query<MonthlyReconReport>(query, parameters).ToList();
        //    return data;
        //}

        //public List<SummaryControlReport> GetSummaryControlByLoanNo(string loanNo,int clientId)
        //{
        //    string query = "select b.BatchClustId,b.Id,b.[DocumentOrPackageType],m.BusinessFriendlyIdValue as [LoanNo],[FileName],b.ReceivedOn,m.ShippedOn as [PrintedOn],m.ShippedOn as [MailedOn],c.AppCode as ClientCode,m.TrackingNo,d.ShippingCarrier as [ShippingMethod],c.Id as [CustomerIdentifier],m.RetRcptFirstName,m.RetRcptMiddleName,m.RetRcptLastName,m.RcptAddressStreet,m.RcptAddressCity,m.RcptAddressState,m.RcptAddressZIP,m.ErrorCode,m.ErrorDesc,m.TotalPages as [PageShipped]from [AM].[Batch] b,[AM].[MailPiece] m,[Common].[Clients] c left outer join [AM].[DocTypes] d on d.ClientId=c.Id where (m.BatchClustId=b.BatchClustId and c.Id=b.ClientId) and (m.BusinessFriendlyIdValue=@loanNo)";
        //    Dictionary<string, object> parameters = new Dictionary<string, object>();
        //    parameters.Add("@loanNo", loanNo);
        //    var data = db.Query<SummaryControlReport>(query, parameters).ToList();
        //    return data;
        //}


        public List<DocumentType> GetDocTypeList(int clientId)
        {
            string query = ReportServiceQuery.GetDocTypeList;
            var data = db.Query(query, new { clientId = clientId }).ToList();
            var list = new List<DocumentType>();
            foreach (var item in data)
            {
                list.Add(new DocumentType { Id = item.Id, DocTypeCode = item.DocTypeCode });
            }
            return list;
        }

        public List<PullBackInfo> GetPullBackRequestList(string loanNo, string docType, DateTime startDate, DateTime endDate, int clientId)
        {
            string query = ReportServiceQuery.GetPullBackRequestList;
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


        public List<string> GetMailPieceIdFromLoanNo(string loanNo, int clientId)
        {
            string query = ReportServiceQuery.GetMailPieceIdFromLoanNo;
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@loanNo", loanNo);
            parameters.Add("@clientId", clientId);
            parameters.Add("@todayDate", Common.Today.Date);
            parameters.Add("@shippingStatus", ShippingStatus.InProgress);
            parameters.Add("@pipelineStatus", BatchPipelineStatus.SubmittedToShipping);

            var mailPieces = db.Query<string>(query, parameters).ToList();
            return mailPieces;
        }
        public List<QuickPullBackResponse> GetQuickPullBackResponseFromLoanNo(string loanNo, int clientId)
        {
            string query = ReportServiceQuery.GetQuickPullBackResponseFromLoanNo;
            var mailPieces = db.Query<QuickPullBackResponse>(query, new { loanNo = loanNo, clientId = clientId }).ToList();
            return mailPieces;
        }


        public List<QuickPullBackResponse> GetQuickPullBackResponse(List<string> mailPieceIds, int clientId)
        {
            string query = ReportServiceQuery.GetQuickPullBackResponse;
            var response = db.Query<QuickPullBackResponse>(query, new { clientId = clientId, mailPieceIds = mailPieceIds }).ToList();
            return response;
        }
    }
    public static class ReportServiceQuery
    {
        public static string GetSummaryControlReport = "select * from [dbo].[view_SummaryControlReport] WHERE ClientId=@clientId AND (cast(ReceivedOn as date) >= @startDate and cast(ReceivedOn as date)<=@endDate)";
        public static string GetDeliveryByParam = "select * from [dbo].[view_DeliveryReport] WHERE ClientId=@clientId AND (cast(ReceivedOn as date) >= @startDate and cast(ReceivedOn as date)<=@endDate)";
        public static string GetErrorReportByParam = "select * from [dbo].[view_ErrorReport] WHERE ClientId=@clientId AND (cast(ReceivedOn as date) >= @startDate and cast(ReceivedOn as date)<=@endDate)";
        public static string GetMonthlyReconReport = "select * from [dbo].[view_MonthlyReconReport] WHERE ClientId=@clientId AND (cast(ReceivedOn as date) >= @startDate and cast(ReceivedOn as date)<=@endDate)";
        public static string GetMailPieceByBatchId = "select m.* from [AM].[MailPieces] m,[AM].[MailBatches] b WHERE m.BatchId=b.Id AND (BatchId=@batchId AND b.ClientId=@clientId)";
        public static string GetDocTypeList = "select Id,DocTypeCode FROM [AM].[DocTypes] WHERE ClientId=@clientId";
        public static string GetPullBackRequestList = "select m.[Id] as [MailPieceId],m.[UniqueId] as [MailPieceUniqueId],m.BusinessFriendlyIdValue,m.BusinessFriendlyIdName,m.[FileName],m.DocumentOrPackageType as [MailPieceDocumentOrPackageType],m.[RcptPrefix],m.[RcptFirstName],m.[RcptMiddleName],m.[RcptLastName],[RcptSuffix],b.DocumentOrPackageType as [BatchDocumentOrPackageType],b.ReceivedOn,m.TotalPages,m.TrackingNo,m.[ShippingStatus],m.PrintedOn,d.ShippingCarrier as [ShippingMethod],m.RcptOrganization,m.[RcptAddrOptionalLine1AboveStreet],m.[RcptAddrOptionalLine2AboveStreet],m.[RcptAddressStreet],m.[RcptAddressAptOrSuiteOrOther],m.[RcptAddrOptionalLine1BelowStreet],m.[RcptAddrOptionalLine2BelowStreet],m.[RcptAddressCity],m.[RcptAddressState],m.[RcptAddressZIP],m.[RetRcptPrefix],m.[RetRcptFirstName],m.[RetRcptMiddleName],m.[RetRcptLastName],m.[RetRcptSuffix],m.[RetRcptOrganization],m.[RetRcptAddrOptionalLine1AboveStreet],m.[RetRcptAddrOptionalLine2AboveStreet],m.[RetRcptAddressAptOrSuiteOrOther],m.[RetRcptAddrOptionalLine1BelowStreet],m.[RetRcptAddrOptionalLine2BelowStreet],m.[RetRcptAddressCity],m.[RetRcptAddressState],m.[RetRcptAddressZIP] from [AM].[MailPieces] m,[AM].[MailBatches] b left outer join [AM].[DocTypes] d on d.ClientId=b.ClientId where (m.BatchId=b.Id) and b.ClientId=@clientId AND (cast(b.ReceivedOn as date)>=@startDate and cast(b.ReceivedOn as date)<=@endDate)";
        public static string GetMailPieceIdFromLoanNo = "select m.[UniqueId] as [MailPieceId] from [AM].[MailBatches] b,[AM].[MailPieces] m where b.Id = m.BatchId and ((cast(ReceivedOn as date)=cast(@todayDate as date) or (m.ShippingStatus=@shippingStatus and b.PipelineStatus=@pipelineStatus))) and b.BusinessFriendlyIdValue=@loanNo AND b.ClientId=@clientId";
        public static string GetQuickPullBackResponseFromLoanNo = "select m.Id as [MailPieceId], m.UniqueId as [MailPieceUniqueId],b.Id as [BatchId],b.UniqueId as [BatchUniqueId],b.BatchName,b.ReceivedOn,m.BusinessFriendlyIdName,m.BusinessFriendlyIdValue,m.[FileName] from [AM].[MailBatches] b,[AM].[MailPieces] m where b.Id =m.BatchId and m.BusinessFriendlyIdValue=@loanNo AND b.ClientId=@clientId";
        public static string GetQuickPullBackResponse = "select m.Id as [MailPieceId], m.UniqueId as [MailPieceUniqueId],b.Id as [BatchId],b.UniqueId as [BatchUniqueId],b.BatchName,b.ReceivedOn,m.BusinessFriendlyIdName,m.BusinessFriendlyIdValue,m.[FileName] from [AM].[MailBatches] b,[AM].[MailPieces] m where b.Id =m.BatchId and m.UniqueId=@mailPieceIds AND b.ClientId=@clientId";
    }
}
