using AM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
namespace AM.Services
{
    public interface IMailPieceService
    {
        int InsertBatchMailPieceDetails(AMMailPiece mailPiece);
        List<AMMailPiece> GetMailPiecesByBatchClustId(int batchClustId);
        int InsertBatchMailPieceCustomDetails(List<MailPieceCustomInfo> mailPieceCustomInfos);
        List<MailPieceCustomInfo> GetMailPiecesCustomInfosByMailPieceId(Guid mailPieceGuId);
    }
    public class MailPieceService : ServiceBase, IMailPieceService //It implements Interfaces 
    {
        public int InsertBatchMailPieceDetails(AMMailPiece mailPiece)
        {
            //Sql Query
            string query = "INSERT INTO [AM].[MailPiece](Id, BatchClustId, PrintBatchClustId, BusinessFriendlyIdName, BusinessFriendlyIdValue, FileName, FilePattern, IsRcptInfoSpecified, RcptPrefix, RcptFirstName, RcptMiddleName, RcptLastName, RcptSuffix, RcptPhone, RcptOrganization, CoRcptPrefix, CoRcptFirstName, CoRcptMiddleName, CoRcptLastName, CoRcptSuffix, CoRcptPhone, CoRcptOrganization, RcptAddrOptionalLine1AboveStreet, RcptAddrOptionalLine2AboveStreet, RcptAddressStreet, RcptAddressAptOrSuiteOrOther, RcptAddrOptionalLine1BelowStreet, RcptAddrOptionalLine2BelowStreet, RcptAddressCity, RcptAddressState, RcptAddressZIP, IsReturnInfoSpecified, RetRcptPrefix, RetRcptFirstName, RetRcptMiddleName, RetRcptLastName, RetRcptSuffix, RetRcptPhone, RetRcptOrganization, RetRcptAddrOptionalLine1AboveStreet, RetRcptAddrOptionalLine2AboveStreet, RetRcptAddressStreet, RetRcptAddressAptOrSuiteOrOther, RetRcptAddrOptionalLine1BelowStreet, RetRcptAddrOptionalLine2BelowStreet, RetRcptAddressCity, RetRcptAddressState, RetRcptAddressZIP, PreppedFilePath, TotalPages, ErrorCode, ErrorDesc, TrackingNo, PullBackRequestedOn, PulledBackOn, ShippedOn, ShippingCost, ShippingStatus, IsQCRequested, IsQCPerformed, DocumentOrPackageType, DownloadedFilePath) VALUES (@Id, @BatchClustId, @PrintBatchClustId, @BusinessFriendlyIdName, @BusinessFriendlyIdValue, @FileName, @FilePattern, @IsRcptInfoSpecified, @RcptPrefix, @RcptFirstName, @RcptMiddleName, @RcptLastName, @RcptSuffix, @RcptPhone, @RcptOrganization, @CoRcptPrefix, @CoRcptFirstName, @CoRcptMiddleName, @CoRcptLastName, @CoRcptSuffix, @CoRcptPhone, @CoRcptOrganization, @RcptAddrOptionalLine1AboveStreet, @RcptAddrOptionalLine2AboveStreet, @RcptAddressStreet, @RcptAddressAptOrSuiteOrOther, @RcptAddrOptionalLine1BelowStreet, @RcptAddrOptionalLine2BelowStreet, @RcptAddressCity, @RcptAddressState, @RcptAddressZIP, @IsReturnInfoSpecified, @RetRcptPrefix, @RetRcptFirstName, @RetRcptMiddleName, @RetRcptLastName, @RetRcptSuffix, @RetRcptPhone, @RetRcptOrganization, @RetRcptAddrOptionalLine1AboveStreet, @RetRcptAddrOptionalLine2AboveStreet, @RetRcptAddressStreet, @RetRcptAddressAptOrSuiteOrOther, @RetRcptAddrOptionalLine1BelowStreet, @RetRcptAddrOptionalLine2BelowStreet, @RetRcptAddressCity, @RetRcptAddressState, @RetRcptAddressZIP, @PreppedFilePath, @TotalPages, @ErrorCode, @ErrorDesc, @TrackingNo, @PullBackRequestedOn, @PulledBackOn, @ShippedOn, @ShippingCost, @ShippingStatus, @IsQCRequested, @IsQCPerformed, @DocumentOrPackageType, @DownloadedFilePath)";           

            /*
             * Execution of query with parateters.
             * Without Dapper you can't create following code. So just Add "using Dapper" namespace at begin.
            */
            var data = db.Execute(query, mailPiece);
            return data;
        }

        public List<AMMailPiece> GetMailPiecesByBatchClustId(int batchClustId)
        {
            //Sql Query
            string query = "SELECT * FROM [AM].[MailPiece] WHERE BatchClustId=@batchClustId";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@batchClustId", batchClustId);

            var data = db.Query<AMMailPiece>(query, parameters).ToList();
            return data;
        }

        public int InsertBatchMailPieceCustomDetails(List<MailPieceCustomInfo> mailPieceCustomInfos)
        {
            string query = "INSERT INTO [AM].[MailPieceCustomInfo](MailPieceId, [Key], [Value]) VALUES (@MailPieceId, @Key, @Value)";

            var data = db.Execute(query, mailPieceCustomInfos);
            return data;
        }

        public List<MailPieceCustomInfo> GetMailPiecesCustomInfosByMailPieceId(Guid mailPieceGuId)
        {
            string query = "SELECT * FROM [AM].[MailPieceCustomInfo] WHERE MailPieceId=@mailPieceGuId";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@mailPieceGuId", mailPieceGuId);

            var data = db.Query<MailPieceCustomInfo>(query, parameters).ToList();
            return data;
        }
    }
}
