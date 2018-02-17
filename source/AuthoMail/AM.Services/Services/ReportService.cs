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
        List<MonthlyReconReport> GetMailPieceByLoanNo(string loanNo);
        List<SummaryControlReport> GetSummaryControlByLoanNo(string loanNo);
    }
    public class ReportService : ServiceBase, IReportService
    {
        public List<MonthlyReconReport> GetMailPieceByLoanNo(string loanNo)
        {
            string query = "select m.BusinessFriendlyIdValue as [LoanNo],[FileName],b.DocumentOrPackageType,m.RetRcptFirstName,m.RetRcptMiddleName,m.RetRcptLastName,m.RcptAddressStreet,m.RcptAddressCity,m.RcptAddressState,m.RcptAddressZIP,b.ReceivedOn,m.TotalPages,m.TrackingNo,b.PipelineStatus as [Status],m.ShippedOn as [PrintedOn],m.ShippedOn as [MailedOn],d.ShippingCarrier as [ShippingMethod]from [AM].[MailPiece] m,[AM].[Batch] binner join [AM].[DocTypes] d on d.ClientId=b.ClientIdwhere (m.BatchClustId=b.BatchClustId) and m.BusinessFriendlyIdValue=@loanNo";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@loanNo", loanNo);
            var data = db.Query<MonthlyReconReport>(query, parameters).ToList();
            return data;
        }

        public List<SummaryControlReport> GetSummaryControlByLoanNo(string loanNo)
        {
            string query = "select b.BatchClustId,b.Id,[DocumentOrPackageType],m.BusinessFriendlyIdValue as [LoanNo],[FileName],b.ReceivedOn,m.ShippedOn as [PrintedOn],m.ShippedOn as [MailedOn],c.AppCode as ClientCode,m.TrackingNo,d.ShippingCarrier as [ShippingMethod],c.Id as [CustomerIdentifier],m.RetRcptFirstName,m.RetRcptMiddleName,m.RetRcptLastName,m.RcptAddressStreet,m.RcptAddressCity,m.RcptAddressState,m.RcptAddressZIP,m.ErrorCode,m.ErrorDesc,m.TotalPages as [PageShipped]from [AM].[Batch] b,[AM].[MailPiece] m,[Common].[Clients] cinner join [AM].[DocTypes] d on d.ClientId=c.Idwhere (m.BatchClustId=b.BatchClustId and c.Id=b.ClientId) and (m.BusinessFriendlyIdValue=@loanNo)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@loanNo", loanNo);
            var data = db.Query<SummaryControlReport>(query, parameters).ToList();
            return data;
        }
    }
}
