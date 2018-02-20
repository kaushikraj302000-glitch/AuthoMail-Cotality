using AM.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace AM.Services.Services
{
    public interface IReportService
    {
        List<SummaryControlReport> GetBatchByParam(string loanNo, string docType, DateTime startDate, DateTime endDate);
        List<MonthlyReconReport> GetMailPieceByParam(string loanNo, string docType, DateTime startDate, DateTime endDate);
        List<MonthlyReconReport> GetMailPieceByBatchId(int batchId);

        //Pullback Request
        List<PullBackInfo> GetPullBackRequestList(string loanNo, string docType, DateTime startDate, DateTime endDate);

        List<MonthlyReconReport> GetMailPieceByLoanNo(string loanNo);
        List<SummaryControlReport> GetSummaryControlByLoanNo(string loanNo);
        List<DocumentType> GetDocTypeList();
    }
    public class ReportService : ServiceBase, IReportService
    {
        public List<SummaryControlReport> GetBatchByParam(string loanNo, string docType, DateTime startDate, DateTime endDate)
        {
            //string query = "select * from dbo.GetBatchByParam WHERE (cast(ReceivedOn as date) >= @startDate and cast(ReceivedOn as date)<=@endDate)";
            string query = "select b.BatchClustId,b.Id,[DocumentOrPackageType],m.BusinessFriendlyIdValue as [LoanNo],[FileName],b.ReceivedOn,m.ShippedOn as [PrintedOn],m.ShippedOn as [MailedOn],c.AppCode as ClientCode,m.TrackingNo,d.ShippingCarrier as [ShippingMethod],c.Id as [CustomerIdentifier],m.RetRcptFirstName,m.RetRcptMiddleName,m.RetRcptLastName,m.RcptAddressStreet,m.RcptAddressCity,m.RcptAddressState,m.RcptAddressZIP,m.ErrorCode,m.ErrorDesc,m.TotalPages as [PageShipped]from [AM].[Batch] b,[AM].[MailPiece] m,[Common].[Clients] c left outer join [AM].[DocTypes] d on d.ClientId=c.Id where (m.BatchClustId=b.BatchClustId and c.Id=b.ClientId) and (cast(b.ReceivedOn as date)>=@startDate and cast(b.ReceivedOn as date)<=@endDate)";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@startDate", startDate);
            parameters.Add("@endDate", endDate);
            if (!loanNo.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} AND (m.BusinessFriendlyIdValue=@loanNo)", query);
                //query = string.Format("{0} AND (LoanNo=@loanNo)", query);
                parameters.Add("@loanNo", loanNo);
            }
            if (!docType.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} AND (DocumentOrPackageType=@docType)", query);
                parameters.Add("@docType", docType);
            }
            var data = db.Query<SummaryControlReport>(query, parameters).ToList();
            return data;
        }
        public List<MonthlyReconReport> GetMailPieceByBatchId(int batchId)
        {
            string query = "select b.BatchClustId, m.BusinessFriendlyIdValue as [LoanNo],[FileName],b.DocumentOrPackageType,m.RetRcptFirstName,m.RetRcptMiddleName,m.RetRcptLastName,m.RcptAddressStreet,m.RcptAddressCity,m.RcptAddressState,m.RcptAddressZIP,b.ReceivedOn,m.TotalPages,m.TrackingNo,b.PipelineStatus as [Status],m.ShippedOn as [PrintedOn],m.ShippedOn as [MailedOn],d.ShippingCarrier as [ShippingMethod]from [AM].[MailPiece] m,[AM].[Batch] b left outer join [AM].[DocTypes] d on d.ClientId=b.ClientId where (m.BatchClustId=b.BatchClustId) and b.BatchClustId=@batchId";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@batchId", batchId);
            var data = db.Query<MonthlyReconReport>(query, parameters).ToList();
            return data;
        }
        public List<MonthlyReconReport> GetMailPieceByLoanNo(string loanNo)
        {
            string query = "select m.BusinessFriendlyIdValue as [LoanNo],[FileName],b.DocumentOrPackageType,m.RetRcptFirstName,m.RetRcptMiddleName,m.RetRcptLastName,m.RcptAddressStreet,m.RcptAddressCity,m.RcptAddressState,m.RcptAddressZIP,b.ReceivedOn,m.TotalPages,m.TrackingNo,b.PipelineStatus as [Status],m.ShippedOn as [PrintedOn],m.ShippedOn as [MailedOn],d.ShippingCarrier as [ShippingMethod]from [AM].[MailPiece] m,[AM].[Batch] b left outer join [AM].[DocTypes] d on d.ClientId=b.ClientId where (m.BatchClustId=b.BatchClustId) and m.BusinessFriendlyIdValue=@loanNo";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@loanNo", loanNo);
            var data = db.Query<MonthlyReconReport>(query, parameters).ToList();
            return data;
        }

        public List<SummaryControlReport> GetSummaryControlByLoanNo(string loanNo)
        {
            string query = "select b.BatchClustId,b.Id,[DocumentOrPackageType],m.BusinessFriendlyIdValue as [LoanNo],[FileName],b.ReceivedOn,m.ShippedOn as [PrintedOn],m.ShippedOn as [MailedOn],c.AppCode as ClientCode,m.TrackingNo,d.ShippingCarrier as [ShippingMethod],c.Id as [CustomerIdentifier],m.RetRcptFirstName,m.RetRcptMiddleName,m.RetRcptLastName,m.RcptAddressStreet,m.RcptAddressCity,m.RcptAddressState,m.RcptAddressZIP,m.ErrorCode,m.ErrorDesc,m.TotalPages as [PageShipped]from [AM].[Batch] b,[AM].[MailPiece] m,[Common].[Clients] c left outer join [AM].[DocTypes] d on d.ClientId=c.Id where (m.BatchClustId=b.BatchClustId and c.Id=b.ClientId) and (m.BusinessFriendlyIdValue=@loanNo)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@loanNo", loanNo);
            var data = db.Query<SummaryControlReport>(query, parameters).ToList();
            return data;
        }


        public List<DocumentType> GetDocTypeList()
        {
            string query = "select Id,DocTypeCode FROM [AM].[DocTypes]";
            var data = db.Query(query).ToList();
            var list = new List<DocumentType>();
            foreach (var item in data)
            {
                list.Add(new DocumentType { Id = item.Id, DocTypeCode = item.DocTypeCode });
            }
            return list;
        }



        public List<MonthlyReconReport> GetMailPieceByParam(string loanNo, string docType, DateTime startDate, DateTime endDate)
        {
            string query = "select b.BatchClustId, m.BusinessFriendlyIdValue as [LoanNo],[FileName],b.DocumentOrPackageType,m.RetRcptFirstName,m.RetRcptMiddleName,m.RetRcptLastName,m.RcptAddressStreet,m.RcptAddressCity,m.RcptAddressState,m.RcptAddressZIP,b.ReceivedOn,m.TotalPages,m.TrackingNo,b.PipelineStatus as [Status],m.ShippedOn as [PrintedOn],m.ShippedOn as [MailedOn],d.ShippingCarrier as [ShippingMethod]from [AM].[MailPiece] m,[AM].[Batch] b left outer join [AM].[DocTypes] d on d.ClientId=b.ClientId where (m.BatchClustId=b.BatchClustId) and (cast(b.ReceivedOn as date)>=@startDate and cast(b.ReceivedOn as date)<=@endDate)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@startDate", startDate);
            parameters.Add("@endDate", endDate);
            if (!loanNo.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} AND (m.BusinessFriendlyIdValue=@loanNo)", query);
                parameters.Add("@loanNo", loanNo);
            }
            if (!docType.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} AND (b.DocumentOrPackageType=@docType)", query);
                parameters.Add("@docType", docType);
            }
            var data = db.Query<MonthlyReconReport>(query, parameters).ToList();
            return data;
        }


        public List<PullBackInfo> GetPullBackRequestList(string loanNo, string docType, DateTime startDate, DateTime endDate)
        {
            string query = "select m.[Id] as [MailPieceId],m.BusinessFriendlyIdValue as [LoanNo],[FileName],b.DocumentOrPackageType,m.[RcptPrefix],m.[RcptFirstName],m.[RcptMiddleName],m.[RcptLastName],[RcptSuffix],m.RcptAddressStreet,m.RcptAddressCity,m.RcptAddressState,m.RcptAddressZIP,b.ReceivedOn,m.TotalPages,m.TrackingNo,b.PipelineStatus as [Status],m.ShippedOn as [PrintedOn],d.ShippingCarrier as [ShippingMethod],m.[RetRcptAddrOptionalLine1AboveStreet],m.[RetRcptAddrOptionalLine2AboveStreet],m.[RetRcptAddressStreet],m.[RetRcptAddressAptOrSuiteOrOther],m.[RetRcptAddrOptionalLine1BelowStreet],m.[RetRcptAddrOptionalLine2BelowStreet],m.[RetRcptAddressCity],m.[RetRcptAddressState],m.[RetRcptAddressZIP] from [AM].[MailPiece] m,[AM].[Batch] b left outer join [AM].[DocTypes] d on d.ClientId=b.ClientId where (m.BatchClustId=b.BatchClustId) and (cast(b.ReceivedOn as date)>=@startDate and cast(b.ReceivedOn as date)<=@endDate)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@startDate", startDate);
            parameters.Add("@endDate", endDate);
            if (!loanNo.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} AND (m.BusinessFriendlyIdValue=@loanNo)", query);
                parameters.Add("@loanNo", loanNo);
            }
            if (!docType.IsNullOrWhiteSpace())
            {
                query = string.Format("{0} AND (b.DocumentOrPackageType=@docType)", query);
                parameters.Add("@docType", docType);
            }
            var data = db.Query<PullBackInfo>(query, parameters).ToList();
            return data;
        }
    }
}
