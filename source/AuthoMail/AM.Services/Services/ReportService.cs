using AM.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data;

namespace AM.Services.Services
{
    public interface IReportService
    {
        List<SummaryControlReport> GetSummaryControlReport(int clientId, DateTime startDate, DateTime endDate, string loanNo, string docType);
        List<DeliveryReport> GetDeliveryByParam(int clientId, DateTime startDate, DateTime endDate, string loanNo, string docType);
        List<ErrorReport> GetErrorReportByParam(int clientId, DateTime startDate, DateTime endDate, string loanNo, string docType);
        List<MonthlyReconReport> GetMonthlyReconReport(int clientId, DateTime startDate, DateTime endDate, string loanNo, string docType);

        List<MonthlyReconReport> GetMailPieceByBatchId(int batchId, int clientId);
        //Above done

        //List<SupplyInventory> GetSupplyInventory(DateTime startDate, DateTime endDate);

        //Pullback Request
        List<PullBackInfo> GetPullBackRequestList(string loanNo, string docType, DateTime startDate, DateTime endDate, int clientId);
        List<string> GetMailPieceIdFromLoanNo(string loanNo, int clientId);

        //List<MonthlyReconReport> GetMailPieceByLoanNo(string loanNo);
        // List<SummaryControlReport> GetSummaryControlByLoanNo(string loanNo,int clientId);
        List<DocumentType> GetDocTypeList();
    }
    public class ReportService : ServiceBase, IReportService
    {
        public List<SummaryControlReport> GetSummaryControlReport(int clientId, DateTime startDate, DateTime endDate, string loanNo, string docType)
        {
            string query = "select * from [dbo].[view_SummaryControlReport] WHERE ClientId=@clientId AND (cast(ReceiveDate as date) >= @startDate and cast(ReceiveDate as date)<=@endDate)";

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
            string query = "select * from [dbo].[view_DeliveryReport] WHERE ClientId=@clientId AND (cast(ReceiveDate as date) >= @startDate and cast(ReceiveDate as date)<=@endDate)";

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
            string query = "select * from [dbo].[view_ErrorReport] WHERE ClientId=@clientId AND (cast(ReceiveDate as date) >= @startDate and cast(ReceiveDate as date)<=@endDate)";

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
            string query = "select * from [dbo].[view_MonthlyReconReport] WHERE ClientId=@clientId AND (cast(ReceiveDate as date) >= @startDate and cast(ReceiveDate as date)<=@endDate)";

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


        public List<MonthlyReconReport> GetMailPieceByBatchId(int batchId, int clientId)
        {
            string query = "select * from [dbo].[view_MonthlyReconReport] WHERE ClientId=@clientId AND BatchClustId=@batchId";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@batchId", batchId);
            parameters.Add("@clientId", clientId);
            var data = db.Query<MonthlyReconReport>(query, parameters).ToList();
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

        public List<PullBackInfo> GetPullBackRequestList(string loanNo, string docType, DateTime startDate, DateTime endDate, int clientId)
        {
            string query = "select m.[Id] as [MailPieceId],m.BusinessFriendlyIdValue as [LoanNo],[FileName],m.DocumentOrPackageType,m.[RcptPrefix],m.[RcptFirstName],m.[RcptMiddleName],m.[RcptLastName],[RcptSuffix],m.RcptAddressStreet,m.RcptAddressCity,m.RcptAddressState,m.RcptAddressZIP,b.ReceivedOn,m.TotalPages,m.TrackingNo,b.PipelineStatus as [Status],m.ShippedOn as [PrintedOn],d.ShippingCarrier as [ShippingMethod],m.[RetRcptAddrOptionalLine1AboveStreet],m.[RetRcptAddrOptionalLine2AboveStreet],m.[RetRcptAddressStreet],m.[RetRcptAddressAptOrSuiteOrOther],m.[RetRcptAddrOptionalLine1BelowStreet],m.[RetRcptAddrOptionalLine2BelowStreet],m.[RetRcptAddressCity],m.[RetRcptAddressState],m.[RetRcptAddressZIP] from [AM].[MailPiece] m,[AM].[Batch] b left outer join [AM].[DocTypes] d on d.ClientId=b.ClientId where (m.BatchClustId=b.BatchClustId) and b.ClientId=@clientId AND (cast(b.ReceivedOn as date)>=@startDate and cast(b.ReceivedOn as date)<=@endDate)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@startDate", startDate);
            parameters.Add("@endDate", endDate);
            parameters.Add("@clientId", clientId);
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


        public List<string> GetMailPieceIdFromLoanNo(string loanNo, int clientId)
        {
            string query = "select m.Id as [MailPieceId] from [AM].[Batch] b,[AM].[MailPiece] m where b.BatchClustId =m.BatchClustId and b.BusinessFriendlyIdValue=@loanNo AND b.ClientId=@clientId";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@loanNo", loanNo);
            parameters.Add("@clientId", clientId);
            var mailPieces = db.Query<string>(query, parameters).ToList();
            return mailPieces;
        }




        //public List<SupplyInventory> GetSupplyInventory(DateTime startDate, DateTime endDate)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
