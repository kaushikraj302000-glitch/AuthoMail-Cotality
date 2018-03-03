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
        List<SLAMissErrorReport> GetSLAMissErrorReport(int clientId);
        //Nedd to move
        List<MailPieceSearch> GetMailPieceByBatchId(int batchId, int clientId);
        List<DocumentType> GetDocTypeList(int clientId);
        List<string> GetMailPieceIdFromLoanNo(string loanNo, int clientId);

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

        public List<SLAMissErrorReport> GetSLAMissErrorReport(int clientId)
        {
            string query = ReportServiceQuery.GetSLAMissErrorReport;
            var data = db.Query<SLAMissErrorReport>(query, new { clientId = clientId }).ToList();
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


    }
    public static class ReportServiceQuery
    {
        public static string GetSummaryControlReport = "select * from [dbo].[view_SummaryControlReport] WHERE ClientId=@clientId AND (cast(ReceivedOn as date) >= @startDate and cast(ReceivedOn as date)<=@endDate)";
        public static string GetDeliveryByParam = "select * from [dbo].[view_DeliveryReport] WHERE ClientId=@clientId AND (cast(ReceivedOn as date) >= @startDate and cast(ReceivedOn as date)<=@endDate)";
        public static string GetErrorReportByParam = "select * from [dbo].[view_ErrorReport] WHERE ClientId=@clientId AND (cast(ReceivedOn as date) >= @startDate and cast(ReceivedOn as date)<=@endDate)";
        public static string GetMonthlyReconReport = "select * from [dbo].[view_MonthlyReconReport] WHERE ClientId=@clientId AND (cast(ReceivedOn as date) >= @startDate and cast(ReceivedOn as date)<=@endDate)";
        public static string GetMailPieceByBatchId = "select m.* from [AM].[MailPieces] m,[AM].[MailBatches] b WHERE m.BatchId=b.Id AND (BatchId=@batchId AND b.ClientId=@clientId)";
        public static string GetDocTypeList = "select Id,DocTypeCode FROM [AM].[DocTypes] WHERE ClientId=@clientId";

        public static string GetMailPieceIdFromLoanNo = "select m.[UniqueId] as [MailPieceId] from [AM].[MailBatches] b,[AM].[MailPieces] m where b.Id = m.BatchId and ((cast(ReceivedOn as date)=cast(@todayDate as date) or (m.ShippingStatus=@shippingStatus and b.PipelineStatus=@pipelineStatus))) and b.BusinessFriendlyIdValue=@loanNo AND b.ClientId=@clientId";

        public static string GetSLAMissErrorReport = "";
    }
}
