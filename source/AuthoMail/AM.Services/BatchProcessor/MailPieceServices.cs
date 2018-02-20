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
        int InsertBatchMailPiecesDetails(List<AMMailPiece> mailPieces);
        List<AMMailPiece> GetMailPiecesByBatchClustId(int batchClustId);
    }
    public class MailPieceService : ServiceBase, IMailPieceService //It implements Interfaces 
    {
        public int InsertBatchMailPiecesDetails(List<AMMailPiece> mailPieces)
        {
            //Sql Query
            string query = "INSERT INTO [AM].[MailPiece](Id, BatchClustId, PrintBatchClustId, BusinessFriendlyIdName, BusinessFriendlyIdValue, FileName, FilePattern, IsRcptInfoSpecified, RcptPrefix, RcptFirstName, RcptMiddleName, RcptLastName, RcptSuffix, RcptPhone, RcptOrganization, CoRcptPrefix, CoRcptFirstName, CoRcptMiddleName, CoRcptLastName, CoRcptSuffix, CoRcptPhone, CoRcptOrganization, RcptAddrOptionalLine1AboveStreet, RcptAddrOptionalLine2AboveStreet, RcptAddressStreet, RcptAddressAptOrSuiteOrOther, RcptAddrOptionalLine1BelowStreet, RcptAddrOptionalLine2BelowStreet, RcptAddressCity, RcptAddressState, RcptAddressZIP, IsReturnInfoSpecified, RetRcptPrefix, RetRcptFirstName, RetRcptMiddleName, RetRcptLastName, RetRcptSuffix, RetRcptPhone, RetRcptOrganization, RetRcptAddrOptionalLine1AboveStreet, RetRcptAddrOptionalLine2AboveStreet, RetRcptAddressStreet, RetRcptAddressAptOrSuiteOrOther, RetRcptAddrOptionalLine1BelowStreet, RetRcptAddrOptionalLine2BelowStreet, RetRcptAddressCity, RetRcptAddressState, RetRcptAddressZIP, PreppedFilePath, TotalPages, ErrorCode, ErrorDesc, TrackingNo, PullBackRequestedOn, PulledBackOn, ShippedOn, ShippingCost, ShippingStatus, IsQCRequested, IsQCPerformed, DocumentOrPackageType, DownloadedFilePath) VALUES (@Id, @BatchClustId, @PrintBatchClustId, @BusinessFriendlyIdName, @BusinessFriendlyIdValue, @FileName, @FilePattern, @IsRcptInfoSpecified, @RcptPrefix, @RcptFirstName, @RcptMiddleName, @RcptLastName, @RcptSuffix, @RcptPhone, @RcptOrganization, @CoRcptPrefix, @CoRcptFirstName, @CoRcptMiddleName, @CoRcptLastName, @CoRcptSuffix, @CoRcptPhone, @CoRcptOrganization, @RcptAddrOptionalLine1AboveStreet, @RcptAddrOptionalLine2AboveStreet, @RcptAddressStreet, @RcptAddressAptOrSuiteOrOther, @RcptAddrOptionalLine1BelowStreet, @RcptAddrOptionalLine2BelowStreet, @RcptAddressCity, @RcptAddressState, @RcptAddressZIP, @IsReturnInfoSpecified, @RetRcptPrefix, @RetRcptFirstName, @RetRcptMiddleName, @RetRcptLastName, @RetRcptSuffix, @RetRcptPhone, @RetRcptOrganization, @RetRcptAddrOptionalLine1AboveStreet, @RetRcptAddrOptionalLine2AboveStreet, @RetRcptAddressStreet, @RetRcptAddressAptOrSuiteOrOther, @RetRcptAddrOptionalLine1BelowStreet, @RetRcptAddrOptionalLine2BelowStreet, @RetRcptAddressCity, @RetRcptAddressState, @RetRcptAddressZIP, @PreppedFilePath, @TotalPages, @ErrorCode, @ErrorDesc, @TrackingNo, @PullBackRequestedOn, @PulledBackOn, @ShippedOn, @ShippingCost, @ShippingStatus, @IsQCRequested, @IsQCPerformed, @DocumentOrPackageType, @DownloadedFilePath)";           

            /*
             * Execution of query with parateters.
             * Without Dapper you can't create following code. So just Add "using Dapper" namespace at begin.
            */
            var data = db.Execute(query, mailPieces);
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
    }
}
