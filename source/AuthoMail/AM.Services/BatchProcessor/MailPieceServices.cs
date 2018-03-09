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
        int InsertBatchMailPieceDetails(MailPieceDTO mailPiece, List<MailPieceCustomInfo> mailPieceCustomInfo);
        List<MailPieceDTO> GetMailPiecesByMailBatchId(int mailBatchId);
        int InsertBatchMailPieceCustomDetails(List<MailPieceCustomInfo> mailPieceCustomInfos);
        List<MailPieceCustomInfo> GetMailPiecesCustomInfosByMailPieceId(Guid mailPieceGuId);
        int UpdateBatchMailPieceDetails(MailPieceDTO mailPiece);
    }
    public class MailPieceService : ServiceBase, IMailPieceService //It implements Interfaces 
    {
        public int InsertBatchMailPieceDetails(MailPieceDTO mailPiece, List<MailPieceCustomInfo> mailPieceCustomInfo)
        {
            //Sql Query
            string query = "INSERT INTO [AM].[MailPieces](ClientId, UniqueId, MailBatchId, PrintBatchId, BusinessFriendlyIdName, BusinessFriendlyIdValue, FileName, FilePattern, DocumentOrPackageType, DueDate, ShippingStatus, IsRcptInfoSpecified, RcptPrefix, RcptFirstName, RcptMiddleName, RcptLastName, RcptSuffix, RcptPhone, RcptOrganization, CoRcptPrefix, CoRcptFirstName, CoRcptMiddleName, CoRcptLastName, CoRcptSuffix, CoRcptPhone, CoRcptOrganization, RcptAddrOptionalLine1AboveStreet, RcptAddrOptionalLine2AboveStreet, RcptAddressStreet, RcptAddressAptOrSuiteOrOther, RcptAddrOptionalLine1BelowStreet, RcptAddrOptionalLine2BelowStreet, RcptAddressCity, RcptAddressState, RcptAddressZIP, IsReturnInfoSpecified, RetRcptPrefix, RetRcptFirstName, RetRcptMiddleName, RetRcptLastName, RetRcptSuffix, RetRcptPhone, RetRcptOrganization, RetRcptAddrOptionalLine1AboveStreet, RetRcptAddrOptionalLine2AboveStreet, RetRcptAddressStreet, RetRcptAddressAptOrSuiteOrOther, RetRcptAddrOptionalLine1BelowStreet, RetRcptAddrOptionalLine2BelowStreet, RetRcptAddressCity, RetRcptAddressState, RetRcptAddressZIP, TotalPages, TotalSheets, ErrorCode, ErrorDesc, TrackingNo, PullBackRequestedOn, PulledBackOn,PrintedOn, ShippedOn, ShippingCost, Weight, IsQCRequested, IsQCPerformed) Output Inserted.Id VALUES (@ClientId, @UniqueId, @MailBatchId, @PrintBatchId, @BusinessFriendlyIdName, @BusinessFriendlyIdValue, @FileName, @FilePattern, @DocumentOrPackageType, @DueDate, @ShippingStatus, @IsRcptInfoSpecified, @RcptPrefix, @RcptFirstName, @RcptMiddleName, @RcptLastName, @RcptSuffix, @RcptPhone, @RcptOrganization, @CoRcptPrefix, @CoRcptFirstName, @CoRcptMiddleName, @CoRcptLastName, @CoRcptSuffix, @CoRcptPhone, @CoRcptOrganization, @RcptAddrOptionalLine1AboveStreet, @RcptAddrOptionalLine2AboveStreet, @RcptAddressStreet, @RcptAddressAptOrSuiteOrOther, @RcptAddrOptionalLine1BelowStreet, @RcptAddrOptionalLine2BelowStreet, @RcptAddressCity, @RcptAddressState, @RcptAddressZIP, @IsReturnInfoSpecified, @RetRcptPrefix, @RetRcptFirstName, @RetRcptMiddleName, @RetRcptLastName, @RetRcptSuffix, @RetRcptPhone, @RetRcptOrganization, @RetRcptAddrOptionalLine1AboveStreet, @RetRcptAddrOptionalLine2AboveStreet, @RetRcptAddressStreet, @RetRcptAddressAptOrSuiteOrOther, @RetRcptAddrOptionalLine1BelowStreet, @RetRcptAddrOptionalLine2BelowStreet, @RetRcptAddressCity, @RetRcptAddressState, @RetRcptAddressZIP, @TotalPages, @TotalSheets, @ErrorCode, @ErrorDesc, @TrackingNo, @PullBackRequestedOn, @PulledBackOn, @PrintedOn, @ShippedOn, @ShippingCost, @Weight, @IsQCRequested, @IsQCPerformed)";

            var data = db.ExecuteScalar(query, mailPiece);
            int mailPieceId = Convert.ToInt32(data);

            if (mailPieceId > 0)
            {
                mailPieceCustomInfo.ForEach(c =>
                {
                    c.MailPieceId = mailPieceId;
                });

                InsertBatchMailPieceCustomDetails(mailPieceCustomInfo);
            }
            return mailPieceId;
        }

        public List<MailPieceDTO> GetMailPiecesByMailBatchId(int batchClustId)
        {
            //Sql Query
            string query = "SELECT * FROM [AM].[MailPiece] WHERE BatchClustId=@batchClustId";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@batchClustId", batchClustId);

            var data = db.Query<MailPieceDTO>(query, parameters).ToList();
            return data;
        }

        public int InsertBatchMailPieceCustomDetails(List<MailPieceCustomInfo> mailPieceCustomInfos)
        {
            string query = "INSERT INTO [AM].[MailPieceCustomInfo](MailPieceId, [Key],[DataType], [Value]) VALUES (@MailPieceId, @Key, @DataType ,@Value)";

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

        public int UpdateBatchMailPieceDetails(MailPieceDTO mailPiece)
        {
            //Sql Query
            string query = @"UPDATE [AM].[MailPieces] SET [UniqueId] = @UniqueId,[ClientId] = @ClientId,[MailBatchId] = @MailBatchId,[PrintBatchId] = @PrintBatchId,[BusinessFriendlyIdName] = @BusinessFriendlyIdName,[BusinessFriendlyIdValue] = @BusinessFriendlyIdValue,[FileName] = @FileName,[FilePattern] = @FilePattern,[DocumentOrPackageType] = @DocumentOrPackageType,[DueDate] = @DueDate,[ShippingStatus] = @ShippingStatus,[IsRcptInfoSpecified] = @IsRcptInfoSpecified,[RcptPrefix] = @RcptPrefix,[RcptFirstName] = @RcptFirstName,[RcptMiddleName] = @RcptMiddleName
      ,[RcptLastName] = @RcptLastName,[RcptSuffix] = @RcptSuffix,[RcptPhone] = @RcptPhone,[RcptOrganization] = @RcptOrganization
      ,[CoRcptPrefix] = @CoRcptPrefix,[CoRcptFirstName] = @CoRcptFirstName,[CoRcptMiddleName] = @CoRcptMiddleName,[CoRcptLastName] = @CoRcptLastName,[CoRcptSuffix] = @CoRcptSuffix,[CoRcptPhone] = @CoRcptPhone,[CoRcptOrganization] = @CoRcptOrganization
      ,[RcptAddrOptionalLine1AboveStreet] = @RcptAddrOptionalLine1AboveStreet,[RcptAddrOptionalLine2AboveStreet] = @RcptAddrOptionalLine2AboveStreet,[RcptAddressStreet] = @RcptAddressStreet,[RcptAddressAptOrSuiteOrOther] = @RcptAddressAptOrSuiteOrOther,[RcptAddrOptionalLine1BelowStreet] = @RcptAddrOptionalLine1BelowStreet
      ,[RcptAddrOptionalLine2BelowStreet] = @RcptAddrOptionalLine2BelowStreet,[RcptAddressCity] = @RcptAddressCity
      ,[RcptAddressState] = @RcptAddressState,[RcptAddressZIP] = @RcptAddressZIP,[IsReturnInfoSpecified] = @IsReturnInfoSpecified
      ,[RetRcptPrefix] = @RetRcptPrefix,[RetRcptFirstName] = @RetRcptFirstName,[RetRcptMiddleName] = @RetRcptMiddleName
      ,[RetRcptLastName] = @RetRcptLastName,[RetRcptSuffix] = @RetRcptSuffix,[RetRcptPhone] = @RetRcptPhone
      ,[RetRcptOrganization] = @RetRcptOrganization,[RetRcptAddrOptionalLine1AboveStreet] = @RetRcptAddrOptionalLine1AboveStreet
      ,[RetRcptAddrOptionalLine2AboveStreet] = @RetRcptAddrOptionalLine2AboveStreet,[RetRcptAddressStreet] = @RetRcptAddressStreet
      ,[RetRcptAddressAptOrSuiteOrOther] = @RetRcptAddressAptOrSuiteOrOther,[RetRcptAddrOptionalLine1BelowStreet] = @RetRcptAddrOptionalLine1BelowStreet,[RetRcptAddrOptionalLine2BelowStreet] = @RetRcptAddrOptionalLine2BelowStreet
      ,[RetRcptAddressCity] = @RetRcptAddressCity,[RetRcptAddressState] = @RetRcptAddressState
      ,[RetRcptAddressZIP] = @RetRcptAddressZIP,[TotalPages] = @TotalPages,[TotalSheets] = @TotalSheets,[ErrorCode] = @ErrorCode
      ,[ErrorDesc] = @ErrorDesc,[TrackingNo] = @TrackingNo,[IsQCRequested] = @IsQCRequested
	  WHERE Id=@Id";

            var mailPieceId = db.Execute(query, mailPiece);
            return mailPieceId;
        }
    }
}
