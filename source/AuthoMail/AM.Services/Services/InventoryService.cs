using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace AM.Services.Services
{
    public interface IInventoryService : IDisposable
    {
        KeyValuePair<bool, string> SaveEnvelopes(Envelopes envelopes);
        KeyValuePair<bool, string> SaveEnvelopesTrasaction(EnvelopeTransactions envtran);
        Envelopes FindByCodeEnvelope(string envelopeCode);
        List<Envelopes> GetEnvelopes();
    }
    public class InventoryService : ServiceBase, IInventoryService
    {

        public KeyValuePair<bool, string> SaveEnvelopes(Envelopes envelopes)
        {
            try
            {
                var envelope = FindByCodeEnvelope(envelopes.Id);
                if (envelope == null)
                {
                    string sql = "INSERT INTO [Common].[Envelopes] ([Id] ,[TotalQuantity] ,[CurrentQuantity] ,[IsBREEnvelope])      VALUES(@Id, varchar(255) ,@TotalQuantity ,@CurrentQuantity ,@IsBREEnvelope)";
                    db.Execute(sql, envelopes);
                    return new KeyValuePair<bool, string>(true, "Saved successfully");
                }
                else
                {
                    string sql = "UPDATE [Common].[Envelopes]  SET [TotalQuantity] = @TotalQuantity, [CurrentQuantity] = @CurrentQuantity, [IsBREEnvelope] = @IsBREEnvelope  WHERE Id=@Id";
                    db.Execute(sql, envelopes);
                    return new KeyValuePair<bool, string>(true, "Saved successfully");
                }
            }
            catch (Exception ex)
            {
                return new KeyValuePair<bool, string>(false, ex.Message);
            }

        }


        public KeyValuePair<bool, string> SaveEnvelopesTrasaction(EnvelopeTransactions envtran)
        {
            throw new NotImplementedException();
        }

        public Envelopes FindByCodeEnvelope(string envelopeCode)
        {
            return db.QuerySingleOrDefault<Envelopes>("select * from Common.Envelopes where Id=@envelopeCode", new { envelopeCode = envelopeCode });
        }

        public List<Envelopes> GetEnvelopes()
        {
            return db.Query<Envelopes>("select * from Common.Envelopes").ToList();
        }
    }
}
