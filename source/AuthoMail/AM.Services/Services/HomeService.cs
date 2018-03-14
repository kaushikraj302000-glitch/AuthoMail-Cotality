using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AM.Services.Models;
using AM.Entities;

namespace AM.Services
{
    public interface IHomeService//Home Service interface that has number of methods
    {
        List<MailPieceSearch> GetMailPieceByBatchId(int batchId, int clientId);
        List<DocumentType> GetDocTypeList(int clientId);
        List<Guid> GetMailPieceIdFromBusinessFriendlyValue(string businessFriendlyIdName, string businessFriendlyIdValue, int clientId);
        List<string> GetBusinessFriendlyIdNameByclientId(int clientId);
    }
    public class HomeService : ServiceBase, IHomeService //It implements Interfaces 
    {
        public List<MailPieceSearch> GetMailPieceByBatchId(int batchId, int clientId)
        {
            var data = db.Query<MailPieceSearch>(HomeServiceQuery.GetMailPieceByBatchId, new { batchId = batchId, clientId = clientId }).ToList();
            return data;
        }

        public List<DocumentType> GetDocTypeList(int clientId)
        {
            var data = db.Query(HomeServiceQuery.GetDocTypeList, new { clientId = clientId }).ToList();
            var list = new List<DocumentType>();
            foreach (var item in data)
            {
                list.Add(new DocumentType { Id = item.Id, DocTypeCode = item.DocTypeCode });
            }
            return list;
        }


        public List<Guid> GetMailPieceIdFromBusinessFriendlyValue(string businessFriendlyIdName, string businessFriendlyIdValue, int clientId)
        {
            string query = HomeServiceQuery.GetMailPieceIdFromBusinessFriendlyValue;
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@businessFriendlyIdName", businessFriendlyIdName);
            parameters.Add("@businessFriendlyIdValue", businessFriendlyIdValue);
            parameters.Add("@clientId", clientId);
            parameters.Add("@todayDate", Common.Today.Date);
            parameters.Add("@shippingStatus", ShippingStatus.InProgress.ToString());
            parameters.Add("@pipelineStatus", BatchPipelineStatus.SubmittedToShipping.ToString());

            var mailPieces = db.Query<Guid>(query, parameters).ToList();
            return mailPieces;
        }
        public List<string> GetBusinessFriendlyIdNameByclientId(int clientId)
        {
            string query = HomeServiceQuery.GetBusinessFriendlyIdNameByclientId;
            var data = db.Query<string>(query, new { clientId = clientId }).ToList();
            return data;
        }
    }
    public static class HomeServiceQuery
    {
        public static string GetMailPieceByBatchId = "select * from [dbo].[view_MailPieceByParam] where MailBatchId=@batchId and clientId=@clientId";

        public static string GetDocTypeList = "select Id,DocTypeCode FROM [AM].[DocTypes] WHERE ClientId=@clientId";

        public static string GetMailPieceIdFromBusinessFriendlyValue = "select m.[UniqueId] as [MailPieceId] from [AM].[MailBatches] b,[AM].[MailPieces] m where b.Id = m.MailBatchId and ((cast(ReceivedOn as date)=cast(@todayDate as date) or (m.ShippingStatus=@shippingStatus and b.PipelineStatus=@pipelineStatus))) and m.BusinessFriendlyIdName=@businessFriendlyIdName AND m.BusinessFriendlyIdValue=@businessFriendlyIdValue AND b.ClientId=@clientId";

        public static string GetBusinessFriendlyIdNameByclientId = "SELECT DISTINCT BusinessFriendlyIdName FROM [AM].[MailPieces] WHERE BusinessFriendlyIdName is not null AND ClientId=@clientId";
    }
}
