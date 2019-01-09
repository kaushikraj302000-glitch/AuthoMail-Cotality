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
        KeyValuePair<bool, string> SaveEnvelopes(Envelopes envelopes, string userId);
        Envelopes FindByCodeEnvelope(string envelopeCode);
        List<Envelopes> GetEnvelopes();
        EnvelopeTransactions FindByIdEnvelopeTransactions(int Id);
        KeyValuePair<bool, string> SaveEnvelopesTrasaction(EnvelopeTransactions envtran, string userId);
        List<EnvelopeTransactions> GetAllEnvelopeTransactions();
    }
    public class InventoryService : ServiceBase, IInventoryService
    {

        public Envelopes FindByCodeEnvelope(string envelopeCode)
        {
            return db.QuerySingleOrDefault<Envelopes>(@"select e.*,c.UserName as CreatedBy,m.UserName as ModifiedBy from Common.Envelopes e
inner join Profile.Users c on e.CreatedBy=c.Id
inner join Profile.Users m on e.ModifiedBy=m.Id
where Id=@envelopeCode", new { envelopeCode = envelopeCode });
        }

        public List<Envelopes> GetEnvelopes()
        {
            return db.Query<Envelopes>(@"select e.*,c.UserName as CreatedBy,m.UserName as ModifiedBy from Common.Envelopes e
inner join Profile.Users c on e.CreatedBy=c.Id
inner join Profile.Users m on e.ModifiedBy=m.Id").ToList();
        }

        public KeyValuePair<bool, string> SaveEnvelopes(Envelopes envelopes, string userId)
        {
            try
            {
                var envelope = FindByCodeEnvelope(envelopes.Id);
                if (envelope == null)
                {
                    string sql = "INSERT INTO [Common].[Envelopes] ([Id],[CurrentQuantity] ,[IsBREEnvelope],[CreatedBy],[ModifiedBy],[CreatedOn],[ModifiedOn]) VALUES(@Id ,0 ,@IsBREEnvelope,@CreatedBy,@ModifiedBy,@CreatedOn,ModifiedOn)";
                    envelopes.CreatedBy = envelopes.ModifiedBy = userId;
                    envelopes.CreatedOn = envelopes.ModifiedOn = DateTime.UtcNow;
                    db.Execute(sql, envelopes);
                    return new KeyValuePair<bool, string>(true, "Saved successfully");
                }
                else
                {
                    string sql = "UPDATE [Common].[Envelopes]  SET [IsBREEnvelope] = @IsBREEnvelope,[ModifiedBy]=@ModifiedBy,[ModifiedOn]=@ModifiedOn WHERE Id=@Id";
                    envelopes.ModifiedBy = userId;
                    envelopes.ModifiedOn = DateTime.UtcNow;
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
            string query = @"select e.*,c.UserName as CreatedBy,m.UserName as ModifiedBy from Common.EnvelopeTransactions e
inner join Profile.Users c on e.CreatedBy=c.Id
inner join Profile.Users m on e.ModifiedBy=m.Id
where Id=@Id";
            return db.QuerySingleOrDefault<EnvelopeTransactions>(query, new { Id = Id });
        }
        public KeyValuePair<bool, string> SaveEnvelopesTrasaction(EnvelopeTransactions envtran, string userId)
        {
            EnvelopeTransactions oldEnvelopeTransaction = null;
            Envelopes oldEnvelope = null;
            if (envtran.Id != 0)
            {
                oldEnvelopeTransaction = FindByIdEnvelopeTransactions(envtran.Id);
                oldEnvelope = FindByCodeEnvelope(envtran.EnvelopeId);
            }
            db.Open();
            using (var t = db.BeginTransaction())
            {
                try
                {
                    if (envtran.Id == 0)
                    {
                        string query = @"INSERT INTO [Common].[EnvelopeTransactions] ([EnvelopeId] ,[Description] ,[ReceivedOn] ,[ReceivedQuantity] ,[UnitPrice],[CreatedBy],[ModifiedBy],[CreatedOn],[ModifiedOn]) 
VALUES (@EnvelopeId ,@Description ,@ReceivedOn ,@ReceivedQuantity ,@UnitPrice,@CreatedBy,@ModifiedBy,@CreatedOn,ModifiedOn);";
                        envtran.ReceivedOn = DateTime.UtcNow;
                        envtran.CreatedBy = envtran.ModifiedBy = userId;
                        envtran.CreatedOn = envtran.ModifiedOn = DateTime.UtcNow;
                        db.Execute(query, envtran, transaction: t);

                        query = "UPDATE [Common].[Envelopes]  SET [CurrentQuantity] = [CurrentQuantity] + (@CurrentQuantity) WHERE Id=@Id";
                        db.Execute(query, new { CurrentQuantity = envtran.ReceivedQuantity, Id = envtran.EnvelopeId }, transaction: t);
                    }
                    else
                    {
                        string query = "UPDATE [Common].[EnvelopeTransactions] SET [EnvelopeId] = @EnvelopeId ,[Description] = @Description ,[ReceivedQuantity] = @ReceivedQuantity ,[UnitPrice] = @UnitPrice,[ModifiedBy]=@ModifiedBy,[ModifiedOn]=@ModifiedOn  WHERE Id=@Id";
                        envtran.ModifiedBy = userId;
                        envtran.ModifiedOn = DateTime.UtcNow;
                        db.Execute(query, envtran, transaction: t);

                        envtran.ReceivedQuantity = oldEnvelope.CurrentQuantity - oldEnvelopeTransaction.ReceivedQuantity + envtran.ReceivedQuantity;
                        query = "UPDATE [Common].[Envelopes]  SET [CurrentQuantity] =@CurrentQuantity WHERE Id=@Id";
                        db.Execute(query, new { CurrentQuantity = envtran.ReceivedQuantity, Id = envtran.EnvelopeId }, transaction: t);
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
            string query = @"select e.*,c.UserName as CreatedBy,m.UserName as ModifiedBy from Common.EnvelopeTransactionse
inner join Profile.Users c on e.CreatedBy=c.Id
inner join Profile.Users m on e.ModifiedBy=m.Id";
            return db.Query<EnvelopeTransactions>(query).ToList();
        }



    }
}
