using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace AM.Services
{
    public interface IInventoryService : IDisposable
    {
        KeyValuePair<bool, string> SaveEnvelopes(Envelopes envelopes);
        Envelopes FindByCodeEnvelope(string envelopeCode);
        List<Envelopes> GetEnvelopes();
        EnvelopeTransactions FindByIdEnvelopeTransactions(int Id);
        KeyValuePair<bool, string> SaveEnvelopesTrasaction(EnvelopeTransactions envtran);
        List<EnvelopeTransactions> GetAllEnvelopeTransactions();
        List<Envelopes> IsEnvelopeStockLess();
    }
    public class InventoryService : ServiceBase, IInventoryService
    {

        public Envelopes FindByCodeEnvelope(string envelopeCode)
        {
            return db.QuerySingleOrDefault<Envelopes>("select * from Common.Envelopes where Id=@envelopeCode", new { envelopeCode = envelopeCode });
        }

        public List<Envelopes> GetEnvelopes()
        {
            return db.Query<Envelopes>("select * from Common.Envelopes").ToList();
        }

        public KeyValuePair<bool, string> SaveEnvelopes(Envelopes envelopes)
        {
            try
            {
                var envelope = FindByCodeEnvelope(envelopes.Id);
                if (envelope == null)
                {
                    string sql = "INSERT INTO [Common].[Envelopes] ([Id] ,[TotalQuantity] ,[CurrentQuantity] ,[IsBREEnvelope])      VALUES(@Id ,@TotalQuantity ,@CurrentQuantity ,@IsBREEnvelope)";
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

        public EnvelopeTransactions FindByIdEnvelopeTransactions(int Id)
        {
            string query = "select * from Common.EnvelopeTransactions where Id=@Id";
            return db.QuerySingleOrDefault<EnvelopeTransactions>(query, new { Id = Id });
        }
        public KeyValuePair<bool, string> SaveEnvelopesTrasaction(EnvelopeTransactions envtran)
        {

            db.Open();
            using (var t = db.BeginTransaction())
            {
                try
                {
                    if (envtran.Id == 0)
                    {
                        string query = @"INSERT INTO [Common].[EnvelopeTransactions] ([EnvelopeId] ,[Description] ,[ReceivedOn] ,[ReceivedQuantity] ,[UnitPrice]) 
VALUES (@EnvelopeId ,@Description ,@ReceivedOn ,@ReceivedQua ,@UnitPrice);";
                        envtran.ReceivedOn = DateTime.UtcNow;
                        db.Execute(query, envtran);

                        query = "UPDATE [Common].[Envelopes]  SET [TotalQuantity] = [TotalQuantity] + (@TotalQuantity) WHERE Id=@Id";
                        db.Execute(query, new { TotalQuantity = envtran.ReceivedQuantity, Id = envtran.EnvelopeId });
                    }
                    else
                    {
                        string query = "UPDATE [Common].[EnvelopeTransactions] SET [EnvelopeId] = @EnvelopeId ,[Description] = @Description ,[ReceivedQuantity] = @ReceivedQuantity ,[UnitPrice] = @UnitPrice  WHERE Id=@Id";
                        db.Execute(query, envtran);
                        var oldEnvelopeTransaction = FindByIdEnvelopeTransactions(envtran.Id);
                        var oldEnvelope = FindByCodeEnvelope(envtran.EnvelopeId);
                        envtran.ReceivedQuantity = oldEnvelope.TotalQuantity - oldEnvelopeTransaction.ReceivedQuantity + envtran.ReceivedQuantity;
                        query = "UPDATE [Common].[Envelopes]  SET [TotalQuantity] = [TotalQuantity] + (@TotalQuantity) WHERE Id=@Id";
                        db.Execute(query, new { TotalQuantity = envtran.ReceivedQuantity, Id = envtran.EnvelopeId });
                    }
                    t.Commit();
                    return new KeyValuePair<bool, string>(true, "Saved successfully");
                }
                catch (Exception ex)
                {
                    t.Rollback();
                    return new KeyValuePair<bool, string>(false, ex.Message);
                }
            }


        }
        public List<EnvelopeTransactions> GetAllEnvelopeTransactions()
        {
            string query = "select * from Common.EnvelopeTransactions";
            return db.Query<EnvelopeTransactions>(query).ToList();
        }


        public List<Envelopes> IsEnvelopeStockLess()
        {
            var newList = new List<Envelopes>();
            var list = GetEnvelopes();
            foreach (var item in list)
            {
                
            }
            return newList;
        }
    }
}
