using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AM.Entities;

namespace AM.Services
{
    public interface IHomeService//Home Service interface that has number of methods
    {
        List<BatchReportInfo> GetBatchReportInfoByDateRange(DateTime startDate, DateTime endDate);
        List<MailPieceReportInfo> GetMailPieceInfoByBatchId(int batchId);//BatchClustId

        //List<Batch> GetBatchByDateRange(DateTime startDate, DateTime endDate);
        //List<MailPiece> GetMailPieceByBatchId(int batchId);
    }
    public class HomeService : ServiceBase, IHomeService //It implements Interfaces 
    {

        public List<BatchReportInfo> GetBatchReportInfoByDateRange(DateTime startDate, DateTime endDate)
        {
            string query = "select b.[Id],[BatchClustId],[PipelineStatus] as [CurrentStatus],[ClientId],[DocumentOrPackageType],[ReceivedOn],[RetRcptPrefix],[RetRcptFirstName],[RetRcptMiddleName],[RetRcptLastName],[RetRcptSuffix],[RetRcptPhone],[RetRcptOrganization],[RetRcptAddrOptionalLine1AboveStreet],[RetRcptAddrOptionalLine2AboveStreet],[RetRcptAddressStreet],[RetRcptAddressAptOrSuiteOrOther][RetRcptAddrOptionalLine1BelowStreet],[RetRcptAddrOptionalLine2BelowStreet],[RetRcptAddressCity],[RetRcptAddressState],[RetRcptAddressZIP],c.BusinessUnit as [BusinessId] from [AM].[Batch] b, [Common].[Clients] c Where ( ReceivedOn >= @startDate AND ReceivedOn <= @endDate ) AND b.ClientId=c.Id";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@startDate", startDate);
            parameters.Add("@endDate", endDate);
            var data = db.Query<BatchReportInfo>(query, parameters).ToList();
            return data;
        }

        public List<MailPieceReportInfo> GetMailPieceInfoByBatchId(int batchId)
        {
            string query = "select [Id],[BatchClustId],BusinessFriendlyIdName as [BusinessName],BusinessFriendlyIdValue as [BusinessValue], [RcptPrefix],[RcptFirstName],[RcptMiddleName],[RcptLastName],[RcptSuffix],[RcptPhone],  [RcptOrganization],[RcptAddrOptionalLine1AboveStreet],[RcptAddrOptionalLine2AboveStreet],[RcptAddressStreet],[RcptAddressAptOrSuiteOrOther],[RcptAddrOptionalLine1BelowStreet],[RcptAddrOptionalLine2BelowStreet],[RcptAddressCity],[RcptAddressState],[RcptAddressZIP],[ShippedOn],[ShippingCost],[ShippingStatus] from [AM].[MailPiece] Where BatchClustId = @batchId";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@batchId", batchId);
            var data = db.Query<MailPieceReportInfo>(query, parameters).ToList();
            return data;
        }

        //public List<Batch> GetBatchByDateRange(DateTime startDate, DateTime endDate)
        //{
        //    //Sql Query
        //    string query = "select * from [AM].[Batch]";
        //    string whereClause = string.Format("WHERE ReceivedOn>=@startDate AND ReceivedOn<=@endDate");
        //    Dictionary<string, object> parameters = new Dictionary<string, object>();
        //    parameters.Add("@startDate", startDate);
        //    parameters.Add("@endDate", endDate);
        //    /*
        //     * Execution of query with parateters.
        //     * Without Dapper you can't create following code. So just Add "using Dapper" namespace at begin.
        //    */
        //    var data = db.Query<Batch>(query, parameters).ToList();
        //    return data;
        //}


        //public List<MailPiece> GetMailPieceByBatchId(int batchId)
        //{
        //    string query = "select * from [AM].[MailPiece] where BatchClustId=@batchId";
        //    Dictionary<string, object> parameters = new Dictionary<string, object>();
        //    parameters.Add("@batchId", batchId);
        //    var data = db.Query<MailPiece>(query, parameters).ToList();
        //    return data;
        //}


    }
}
