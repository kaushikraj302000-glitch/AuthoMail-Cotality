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
        EnvelopeInventory FindByIdEnvelopeInventory(int Id);
        KeyValuePair<bool, string> SaveEnvelopeInventory(EnvelopeInventory envtran, string userId);
        List<EnvelopeInventory> GetAllEnvelopeInventory();
        void UpdateMinStock();
        int InventoryStockAlertLevel();
    }
    public class InventoryService : ServiceBase, IInventoryService
    {

        public Envelopes FindByCodeEnvelope(string envelopeCode)
        {
            return db.QuerySingleOrDefault<Envelopes>(@"select e.*,c.DisplayName as CreatedBy,m.DisplayName as ModifiedBy from Common.Envelopes e
inner join Profile.Users c on e.CreatedBy=c.Id
inner join Profile.Users m on e.ModifiedBy=m.Id
where e.Id=@envelopeCode", new { envelopeCode = envelopeCode });
        }

        public List<Envelopes> GetEnvelopes()
        {
            return db.Query<Envelopes>(@"select e.*,c.DisplayName as CreatedBy,m.DisplayName as ModifiedBy from Common.Envelopes e
inner join Profile.Users c on e.CreatedBy=c.Id
inner join Profile.Users m on e.ModifiedBy=m.Id").ToList().OrderByDescending(e => e.AlertLevel).ToList();
        }

        public KeyValuePair<bool, string> SaveEnvelopes(Envelopes envelopes, string userId)
        {
            try
            {
                var envelope = FindByCodeEnvelope(envelopes.Id);
                if (envelope == null)
                {
                    string sql = "INSERT INTO [Common].[Envelopes] ([Id],[CurrentQuantity] ,[IsBREEnvelope],[CreatedBy],[ModifiedBy],[CreatedOn],[ModifiedOn],[ReorderDuration]) VALUES(@Id ,0 ,@IsBREEnvelope,@CreatedBy,@ModifiedBy,@CreatedOn,@ModifiedOn,@ReorderDuration)";
                    envelopes.CreatedBy = envelopes.ModifiedBy = userId;
                    envelopes.CreatedOn = envelopes.ModifiedOn = DateTime.UtcNow;
                    db.Execute(sql, envelopes);
                    return new KeyValuePair<bool, string>(true, "Saved successfully");
                }
                else
                {
                    string sql = "UPDATE [Common].[Envelopes]  SET [IsBREEnvelope] = @IsBREEnvelope,[ModifiedBy]=@ModifiedBy,[ModifiedOn]=@ModifiedOn,[ReorderDuration]=@ReorderDuration WHERE Id=@Id";
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

        public EnvelopeInventory FindByIdEnvelopeInventory(int Id)
        {
            string query = @"select e.*,c.DisplayName as CreatedBy,m.DisplayName as ModifiedBy from Common.EnvelopeInventory e
inner join Profile.Users c on e.CreatedBy=c.Id
inner join Profile.Users m on e.ModifiedBy=m.Id
where e.Id=@Id";
            return db.QuerySingleOrDefault<EnvelopeInventory>(query, new { Id = Id });
        }
        public KeyValuePair<bool, string> SaveEnvelopeInventory(EnvelopeInventory envtran, string userId)
        {
            EnvelopeInventory oldEnvelopeInventory = null;
            Envelopes oldEnvelope = null;
            if (envtran.Id != 0)
            {
                oldEnvelopeInventory = FindByIdEnvelopeInventory(envtran.Id);
                oldEnvelope = FindByCodeEnvelope(envtran.EnvelopeId);
            }
            db.Open();
            using (var t = db.BeginTransaction())
            {
                try
                {
                    if (envtran.Id == 0)
                    {
                        string query = @"INSERT INTO [Common].[EnvelopeInventory] ([EnvelopeId] ,[Description] ,[ReceivedOn] ,[ReceivedQuantity] ,[UnitPrice],[CreatedBy],[ModifiedBy],[CreatedOn],[ModifiedOn]) 
VALUES (@EnvelopeId ,@Description ,@ReceivedOn ,@ReceivedQuantity ,@UnitPrice,@CreatedBy,@ModifiedBy,@CreatedOn,@ModifiedOn);";
                        envtran.ReceivedOn = DateTime.UtcNow;
                        envtran.CreatedBy = envtran.ModifiedBy = userId;
                        envtran.CreatedOn = envtran.ModifiedOn = DateTime.UtcNow;
                        db.Execute(query, envtran, transaction: t);

                        query = "UPDATE [Common].[Envelopes]  SET [CurrentQuantity] = [CurrentQuantity] + (@CurrentQuantity) WHERE Id=@Id";
                        db.Execute(query, new { CurrentQuantity = envtran.ReceivedQuantity, Id = envtran.EnvelopeId }, transaction: t);
                    }
                    else
                    {
                        string query = "UPDATE [Common].[EnvelopeInventory] SET [EnvelopeId] = @EnvelopeId ,[Description] = @Description ,[ReceivedQuantity] = @ReceivedQuantity ,[UnitPrice] = @UnitPrice,[ModifiedBy]=@ModifiedBy,[ModifiedOn]=@ModifiedOn  WHERE Id=@Id";
                        envtran.ModifiedBy = userId;
                        envtran.ModifiedOn = DateTime.UtcNow;
                        db.Execute(query, envtran, transaction: t);

                        envtran.ReceivedQuantity = oldEnvelope.CurrentQuantity - oldEnvelopeInventory.ReceivedQuantity + envtran.ReceivedQuantity;
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
        public List<EnvelopeInventory> GetAllEnvelopeInventory()
        {
            string query = @"select e.*,c.DisplayName as CreatedBy,m.DisplayName as ModifiedBy from Common.EnvelopeInventory e
inner join Profile.Users c on e.CreatedBy=c.Id
inner join Profile.Users m on e.ModifiedBy=m.Id";
            return db.Query<EnvelopeInventory>(query).ToList();
        }

        public void UpdateMinStock()//Todo
        {
            //Execute every week(7 Days)
            var enventories = db.Query<EnvelopeInventoryView>("select * from dbo.view_EnvelopeInventoryData").ToList();
            if (enventories != null)
            {
                enventories.ForEach(i =>
                {
                    db.Execute("update Common.Envelopes set MinStock=@MinStock where Id=@id", new { id = i.EnvelopeId, MinStock = i.MinStockRequirement });
                });
            }
        }

        public int InventoryStockAlertLevel()
        {
            var envelopes = GetEnvelopes();
            if (envelopes.Any(e => e.AlertLevel == 2)) return 2;
            if (envelopes.Any(e => e.AlertLevel == 1)) return 1;
            return 0;
        }
    }
}
