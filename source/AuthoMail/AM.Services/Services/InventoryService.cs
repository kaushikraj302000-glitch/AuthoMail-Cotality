using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AM.Lib;
using Dapper;

namespace AM.Services
{
    public interface IInventoryService : IDisposable
    {
        KeyValuePair<bool, string> SaveEnvelopes(EnvelopesVM envelope, string userId);
        EnvelopesVM GetEnvelopeWithShippers(int id);
        Envelopes FindByCodeEnvelope(string envelopeCode);
        Envelopes FindEnvelopeById(int Id);
        List<Envelopes> GetEnvelopes();
        EnvelopeInventory FindByIdEnvelopeInventory(int Id);
        KeyValuePair<bool, string> SaveEnvelopeInventory(EnvelopeInventory envtran, string userId);
        List<EnvelopeInventory> GetAllEnvelopeInventory();
        List<EnvelopeInventoryStockCalculation> GetEnvelopeInventoryStockCalculation();
        KeyValuePair<int, Dictionary<string, float>> InventoryStockAlertLevel(Dictionary<string, float> envelopeAvgRate);
        Tuple<List<StockDetails>, List<StockDetails>> GetEnvelopeStockDetails(string id);

        IEnumerable<ShipperInfoSelectListVM> ShipperSelectList();
        void RemoveEnvelopeInventoryById(int id);
        KeyValuePair<bool, string> IsPartNumberExists(int Id, string partNumber);
        KeyValuePair<bool, string> IsEnvelopeNameExists(int Id, string name);
        IEnumerable<EnvelopeInventory> GetLast3MonthsInventoryEnvelopes();
        KeyValuePair<bool, string> UpdateReorderDuration(int id, int reorderDuration);
    }
    public class InventoryService : ServiceBase, IInventoryService
    {
        public Envelopes FindEnvelopeById(int Id)
        {
            string query = @"select e.*,c.DisplayName as CreatedBy,m.DisplayName as ModifiedBy from Common.Envelopes e
inner join Profile.Users c on e.CreatedBy = c.Id
inner join Profile.Users m on e.ModifiedBy = m.Id
where e.Id = @Id";
            return db.QuerySingleOrDefault<Envelopes>(query, new { Id = Id });
        }
        public Envelopes FindByCodeEnvelope(string envelopeCode)
        {
            return db.QuerySingleOrDefault<Envelopes>(@"select e.*,c.DisplayName as CreatedBy,m.DisplayName as ModifiedBy from Common.Envelopes e
inner join Profile.Users c on e.CreatedBy=c.Id
inner join Profile.Users m on e.ModifiedBy=m.Id
where e.EnvelopeName=@envelopeCode", new { envelopeCode = envelopeCode });
        }

        public List<Envelopes> GetEnvelopes()
        {
            var envelopes = db.Query<Envelopes>(@"select e.*,c.DisplayName as CreatedBy,m.DisplayName as ModifiedBy from Common.Envelopes e
inner join Profile.Users c on e.CreatedBy=c.Id
inner join Profile.Users m on e.ModifiedBy=m.Id").ToList();
            return envelopes;
        }
        public List<EnvelopeInventoryStockCalculation> GetEnvelopeInventoryStockCalculation()
        {
            return db.Query<EnvelopeInventoryStockCalculation>("exec dbo.usp_EnvelopeInventoryStockCalculation").ToList();
        }

        public KeyValuePair<bool, string> SaveEnvelopes(EnvelopesVM envelope, string userId)
        {
            db.Open();
            using (var t = db.BeginTransaction())
            {
                try
                {
                    if (envelope.Id == 0)
                    {
                        string sql = "INSERT INTO [Common].[Envelopes] ([EnvelopeName],[EnvelopeDesc],[PartNumber] ,[IsBREEnvelope],[CreatedBy],[ModifiedBy],[CreatedOn],[ModifiedOn]) OUTPUT INSERTED.[Id] VALUES(@EnvelopeName ,@EnvelopeDesc,@PartNumber ,@IsBREEnvelope,@CreatedBy,@ModifiedBy,@CreatedOn,@ModifiedOn)";
                        envelope.CreatedBy = envelope.ModifiedBy = userId;
                        envelope.CreatedOn = envelope.ModifiedOn = DateTime.UtcNow;
                        envelope.Id=db.QuerySingleOrDefault<int>(sql, envelope, transaction: t);
                        List<EnvelopeInventory> envelopeInventories = new List<EnvelopeInventory>();
                        foreach (var inventory in envelope.EnvelopeStockVMs)
                        {
                            EnvelopeInventory envInventory = new EnvelopeInventory();
                            envInventory.CreatedBy = envInventory.ModifiedBy = userId;
                            envInventory.CreatedOn = envInventory.ModifiedOn = envInventory.ReceivedOn= DateTime.UtcNow;
                            envInventory.ReceivedQuantity = inventory.ReceivedQuantity.Value;
                            envInventory.UnitPrice = inventory.UnitPrice.Value;
                            envInventory.ShipperId = inventory.ShipperId;
                            envInventory.EnvelopeId = envelope.Id;
                            envelopeInventories.Add(envInventory);
                        }
                         sql = @"INSERT INTO [Common].[EnvelopeInventory] ([EnvelopeId] ,[Description] ,[ReceivedOn] ,[ReceivedQuantity] ,[UnitPrice],[CreatedBy],[ModifiedBy],[CreatedOn],[ModifiedOn],[ShipperId]) 
                            VALUES (@EnvelopeId ,@Description ,@ReceivedOn ,@ReceivedQuantity ,@UnitPrice,@CreatedBy,@ModifiedBy,@CreatedOn,@ModifiedOn,@ShipperId);";
                        db.Execute(sql, envelopeInventories, transaction: t);

                        sql = @"insert into [Common].[EnvelopeStock] (EnvelopeId,ShipperId,CurrentQuantity,ReOrderDuration) values (@EnvelopeId,@ShipperId,@CurrentQuantity,@ReOrderDuration)";
                        envelope.EnvelopeStockVMs.ForEach(s =>
                        {
                            s.EnvelopeId = envelope.Id;
                        });

                           db.Execute(sql, envelope.EnvelopeStockVMs, transaction: t);
                        t.Commit();
                        return new KeyValuePair<bool, string>(true, "Saved successfully");
                    }
                    else
                    {
                        string sql = "UPDATE [Common].[Envelopes]  SET [EnvelopeName]=@EnvelopeName, [IsBREEnvelope] = @IsBREEnvelope,[ModifiedBy]=@ModifiedBy,[ModifiedOn]=@ModifiedOn,EnvelopeDesc=@EnvelopeDesc,PartNumber=@PartNumber WHERE Id=@Id";
                        envelope.ModifiedBy = userId;
                        envelope.ModifiedOn = DateTime.UtcNow;
                        db.Execute(sql, envelope, transaction: t);
                        if (envelope.EnvelopeStockVMs.Count>0)
                        {
                            List<EnvelopeInventory> envelopeInventories = new List<EnvelopeInventory>();
                            foreach (var inventory in envelope.EnvelopeStockVMs)
                            {
                                EnvelopeInventory envInventory = new EnvelopeInventory();
                                envInventory.CreatedBy = envInventory.ModifiedBy = userId;
                                envInventory.CreatedOn = envInventory.ModifiedOn = envInventory.ReceivedOn = DateTime.UtcNow;
                                envInventory.ReceivedQuantity = inventory.ReceivedQuantity.Value;
                                envInventory.UnitPrice = inventory.UnitPrice.Value;
                                envInventory.ShipperId = inventory.ShipperId;
                                envInventory.EnvelopeId = envelope.Id;
                                envelopeInventories.Add(envInventory);
                            }
                            sql = @"INSERT INTO [Common].[EnvelopeInventory] ([EnvelopeId] ,[Description] ,[ReceivedOn] ,[ReceivedQuantity] ,[UnitPrice],[CreatedBy],[ModifiedBy],[CreatedOn],[ModifiedOn],[ShipperId]) 
                            VALUES (@EnvelopeId ,@Description ,@ReceivedOn ,@ReceivedQuantity ,@UnitPrice,@CreatedBy,@ModifiedBy,@CreatedOn,@ModifiedOn,@ShipperId);";
                            db.Execute(sql, envelopeInventories, transaction: t);

                            sql = @"insert into [Common].[EnvelopeStock] (EnvelopeId,ShipperId,CurrentQuantity,ReOrderDuration) values (@EnvelopeId,@ShipperId,@CurrentQuantity,@ReOrderDuration)";
                            db.Execute(sql, envelope.EnvelopeStockVMs, transaction: t);
                        }
                        t.Commit();
                        return new KeyValuePair<bool, string>(true, "Saved successfully");
                    }
                }
                catch (Exception ex)
                {
                    t.Rollback();
                    return new KeyValuePair<bool, string>(false, ex.Message);
                }
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
                oldEnvelope = FindEnvelopeById(envtran.GetEnvelopeId());
            }
            db.Open();
            using (var t = db.BeginTransaction())
            {
                try
                {
                    if (envtran.Id == 0)
                    {
                        string query = @"INSERT INTO [Common].[EnvelopeInventory] ([EnvelopeId] ,[Description] ,[ReceivedOn] ,[ReceivedQuantity] ,[UnitPrice],[CreatedBy],[ModifiedBy],[CreatedOn],[ModifiedOn],[ShipperId]) 
VALUES (@EnvelopeId ,@Description ,@ReceivedOn ,@ReceivedQuantity ,@UnitPrice,@CreatedBy,@ModifiedBy,@CreatedOn,@ModifiedOn,@ShipperId);";
                        envtran.ReceivedOn = DateTime.UtcNow;
                        envtran.CreatedBy = envtran.ModifiedBy = userId;
                        envtran.CreatedOn = envtran.ModifiedOn = DateTime.UtcNow;
                        db.Execute(query, envtran, transaction: t);

                        var isExists = db.QuerySingleOrDefault<int?>("select top 1 1 from Common.EnvelopeStock WHERE EnvelopeId=@EnvelopeId and ShipperId=@ShipperId", new { EnvelopeId = envtran.GetEnvelopeId(), ShipperId = envtran.ShipperId }, transaction: t);
                        if (isExists.HasValue)
                        {
                            query = "UPDATE [Common].[EnvelopeStock]  SET [CurrentQuantity] = [CurrentQuantity] + (@CurrentQuantity) WHERE EnvelopeId=@EnvelopeId and ShipperId=@ShipperId";
                            db.Execute(query, new { CurrentQuantity = envtran.ReceivedQuantity,  EnvelopeId = envtran.GetEnvelopeId(), ShipperId = envtran.ShipperId }, transaction: t);
                        }
                        //else
                        //{
                        //    query = @"insert into [Common].[EnvelopeStock] (EnvelopeId,ShipperId,CurrentQuantity,ReOrderDuration) values (@EnvelopeId,@ShipperId,@CurrentQuantity,@ReOrderDuration)";
                        //    db.Execute(query, new { CurrentQuantity = envtran.ReceivedQuantity, ReOrderDuration = envtran.ReorderDuration, EnvelopeId = envtran.GetEnvelopeId(), ShipperId = envtran.ShipperId }, transaction: t);
                        //}
                    }
                    else
                    {
                        string query = "UPDATE [Common].[EnvelopeInventory] SET [EnvelopeId] = @EnvelopeId ,[Description] = @Description ,[ReceivedQuantity] = @ReceivedQuantity ,[UnitPrice] = @UnitPrice,[ModifiedBy]=@ModifiedBy,[ModifiedOn]=@ModifiedOn  WHERE Id=@Id";
                        envtran.ModifiedBy = userId;
                        envtran.ModifiedOn = DateTime.UtcNow;
                        db.Execute(query, envtran, transaction: t);

                        var oldCurrentQuantity = db.QuerySingle<int>("select CurrentQuantity from [Common].[EnvelopeStock] where EnvelopeId=@EnvelopeId and ShipperId=@ShipperId", new { EnvelopeId = envtran.GetEnvelopeId(), ShipperId = envtran.ShipperId }, transaction: t);

                        envtran.ReceivedQuantity = oldCurrentQuantity - oldEnvelopeInventory.ReceivedQuantity + envtran.ReceivedQuantity;
                        query = "UPDATE [Common].[EnvelopeStock]  SET [CurrentQuantity] =@CurrentQuantity WHERE EnvelopeId=@EnvelopeId and ShipperId=@ShipperId";
                        db.Execute(query, new { CurrentQuantity = envtran.ReceivedQuantity, EnvelopeId = envtran.GetEnvelopeId(), ShipperId = envtran.ShipperId }, transaction: t);
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
inner join Profile.Users m on e.ModifiedBy=m.Id order by e.ReceivedOn desc";
            return db.Query<EnvelopeInventory>(query).ToList();
        }



        public KeyValuePair<int, Dictionary<string, float>> InventoryStockAlertLevel(Dictionary<string, float> envelopeAvgRate)
        {
            var envelopeInvStockCalc = GetEnvelopeInventoryStockCalculation();
            Dictionary<string, float> dict = new Dictionary<string, float>();
            int alertLevel = 0;
            //dict = envelopeInvStockCalc.Select(e => new { e.EnvelopeId, e.AvgRate }).ToDictionary(e => e.EnvelopeId.ToString(), e => e.AvgRate);
            if (envelopeInvStockCalc.Any(e => e.AlertLevel == 2)) alertLevel = 2;
            else if (envelopeInvStockCalc.Any(e => e.AlertLevel == 1)) alertLevel = 1;
            return new KeyValuePair<int, Dictionary<string, float>>(alertLevel, dict);

            //var envelopes = db.Query<EnvelopeIdCQuantity>("select Id,CurrentQuantity,ReorderDuration from Common.Envelopes").ToList();
            //bool isnotExist = envelopeAvgRate != null ? envelopes.Any(e => !envelopeAvgRate.Any(r => r.Key == e.Id)) : true;


            //if (isnotExist)
            //{
            //    var newEnvelopes = GetEnvelopes();
            //    dict = newEnvelopes.Select(e => new { e.Id, e.AvgRate }).ToDictionary(e => e.Id, e => e.AvgRate);
            //    if (newEnvelopes.Any(e => e.AlertLevel == 2)) alertLevel = 2;
            //    else if (newEnvelopes.Any(e => e.AlertLevel == 1)) alertLevel = 1;
            //}
            //else
            //{
            //    envelopes.ForEach(e =>
            //    {
            //        float? avgRate = envelopeAvgRate.Where(a => a.Key == e.Id).Select(a => a.Value).SingleOrDefault();
            //        //Fix if avgRate is null or 0 then exclude it.
            //        if (avgRate != null)
            //        {
            //            e.AvgRate = avgRate.Value;
            //        }
            //    });
            //    dict = envelopes.Select(e => new { e.Id, e.AvgRate }).ToDictionary(e => e.Id, e => e.AvgRate);
            //    if (envelopes.Any(e => e.AlertLevel == 2)) alertLevel = 2;
            //    else if (envelopes.Any(e => e.AlertLevel == 1)) alertLevel = 1;
            //}
            //return new KeyValuePair<int, Dictionary<string, float>>(alertLevel, dict);
        }



        public Tuple<List<StockDetails>, List<StockDetails>> GetEnvelopeStockDetails(string id)
        {
            string query = @"select top 30 CAST(ReceivedOn as date) as Date,sum(ReceivedQuantity) as UsageStock from [Common].[EnvelopeInventory] where EnvelopeId=6 group by Cast(ReceivedOn as date) order by Cast(ReceivedOn as date) desc;exec dbo.usp_GetEnvelopeInventoryUsage @Id;";
            using (var result = db.QueryMultiple(query, new { Id = id }))
            {
                var envelopeArrival = result.Read<StockDetails>().ToList();
                var envelopeInventoryUsage = result.Read<StockDetails>().ToList();
                return new Tuple<List<StockDetails>, List<StockDetails>>(envelopeInventoryUsage, envelopeArrival);
            }

        }

        public IEnumerable<ShipperInfoSelectListVM> ShipperSelectList()
        {
            return db.Query<ShipperInfoSelectListVM>(@"select s.Id,s.PrintVendorId,s.PrintLocation,s.PrintVendorCode,p.Name as PrintVendor from Common.ShipperInformation s
inner join Common.PrintVendors p on s.PrintVendorId = p.Id");
        }

        public void RemoveEnvelopeInventoryById(int id)
        {
            var envelopeInventory = db.QuerySingle<EnvelopeInventory>("select * from Common.EnvelopeInventory where Id=@Id", new { Id = id });
            db.Open();
            using (var t = db.BeginTransaction())
            {
                try
                {
                    string query = "UPDATE [Common].[EnvelopeStock]  SET [CurrentQuantity] =[CurrentQuantity]-(@CurrentQuantity) WHERE EnvelopeId=@EnvelopeId and ShipperId=@ShipperId";
                    db.Execute(query, new { CurrentQuantity = envelopeInventory.ReceivedQuantity, EnvelopeId = envelopeInventory.GetEnvelopeId(), ShipperId = envelopeInventory.ShipperId }, transaction: t);

                    db.Execute("delete from Common.EnvelopeInventory where Id=@Id", new { Id = id }, transaction: t);
                    t.Commit();
                }
                catch (Exception ex)
                {
                    t.Rollback();
                    throw ex;
                }
            }
        }

        public KeyValuePair<bool, string> IsPartNumberExists(int Id, string partNumber)
        {
            if (Id != 0)
            {
                int? output = db.QuerySingleOrDefault<int?>(@"select top 1 id from Common.Envelopes where Id=@Id and PartNumber=@PartNumber", new { Id = Id, PartNumber = partNumber });
                if (output != null)
                {
                    return new KeyValuePair<bool, string>(true, null);
                }
                output = db.QuerySingleOrDefault<int?>(@"select top 1 id from Common.Envelopes where Id<>@Id and PartNumber=@PartNumber", new { Id = Id, PartNumber = partNumber });
                if (output != null)
                {
                    return new KeyValuePair<bool, string>(false, "Part Number already exists.");
                }
                return new KeyValuePair<bool, string>(true, null);
            }
            else
            {
                int? output = db.QuerySingleOrDefault<int?>(@"select top 1 id from Common.Envelopes where  PartNumber=@PartNumber", new { PartNumber = partNumber });
                if (output != null)
                {
                    return new KeyValuePair<bool, string>(false, "Part Number already exists.");
                }
                return new KeyValuePair<bool, string>(true, null);
            }
        }

        public IEnumerable<EnvelopeInventory> GetLast3MonthsInventoryEnvelopes()
        {
            return db.Query<EnvelopeInventory>(@"select ei.*,e.EnvelopeName,s.PrintLocation as Shipper,c.DisplayName as CreatedBy, m.DisplayName as ModifiedBy from Common.EnvelopeInventory ei
inner join Common.Envelopes e on ei.EnvelopeId=e.Id
inner join Common.ShipperInformation s on ei.ShipperId=s.Id
inner join Profile.Users c on ei.CreatedBy=c.Id
inner join Profile.Users m on ei.ModifiedBy=m.Id
where ei.ModifiedOn>=@modifiedOn", new { modifiedOn = DateTime.UtcNow.AddMonths(-3) });
        }

        public KeyValuePair<bool, string> UpdateReorderDuration(int id, int reorderDuration)
        {
            try
            {
                    string sql = "UPDATE [Common].[EnvelopeStock]  SET ReOrderDuration=@ReOrderDuration WHERE Id=@Id";
                    db.Execute(sql, new { Id=id, ReOrderDuration=reorderDuration });
                    return new KeyValuePair<bool, string>(true, "Saved successfully");
            }
            catch (Exception ex)
            {
                return new KeyValuePair<bool, string>(false, ex.Message);
            }
        }

        public EnvelopesVM GetEnvelopeWithShippers(int id)
        {
            string sql = @"Select E.*,ES.*,ES.CurrentQuantity as ReceivedQuantity,CONCAT(SI.PrintLocation, CONCAT(' ( ', SI.PrintVendorCode, ' ) ')) as ShipperName,ES.EnvelopeId,0 as UnitPrice from [Common].[Envelopes] E 
                            Inner Join [Common].[EnvelopeStock] ES ON E.Id=ES.EnvelopeId
                            Inner Join [Common].[ShipperInformation] SI ON ES.ShipperId=SI.Id
                            Where E.Id=@Id";
            var orderDictionary = new Dictionary<int, EnvelopesVM>();


            var envelopeVMs = db.Query<EnvelopesVM, EnvelopeStockVM, EnvelopesVM>(
                sql,
                (envelopeVM, envelopeStockVM) =>
                {
                    EnvelopesVM envelopeVMEntry;

                    if (!orderDictionary.TryGetValue(envelopeVM.Id, out envelopeVMEntry))
                    {
                        envelopeVMEntry = envelopeVM;
                        envelopeVMEntry.EnvelopeStockVMs = new List<EnvelopeStockVM>();
                   
                        orderDictionary.Add(envelopeVMEntry.Id, envelopeVMEntry);
                    }

                    envelopeVMEntry.EnvelopeStockVMs.Add(envelopeStockVM);
                    return envelopeVMEntry;
                }, new { Id = id }, splitOn: "ShipperId")
            .Distinct()
            .SingleOrDefault();
            return envelopeVMs;
        }

        public KeyValuePair<bool, string> IsEnvelopeNameExists(int id, string name)
       {
            if (id != 0)
            {
                int? output = db.QuerySingleOrDefault<int?>(@"select top 1 id from Common.Envelopes where Id=@Id and EnvelopeName=@Name",new { Id=id,Name=name });
                if (output != null)
                {
                    return new KeyValuePair<bool, string>(true, null);
                }
                output = db.QuerySingleOrDefault<int?>(@"select top 1 id from Common.Envelopes where Id<>@Id and EnvelopeName=@Name", new { Id = id, Name = name });
                if (output !=null)
                {
                    return new KeyValuePair<bool, string>(false, "Envelope Name already exists.");
                }
                return new KeyValuePair<bool, string>(true, null);
            }
            else
            {
                int? output = db.QuerySingleOrDefault<int?>(@"select top 1 id from Common.Envelopes where  EnvelopeName=@Name", new { Name = name });
                if (output!=null)
                {
                    return new KeyValuePair<bool, string>(false, "Envelope Name already exists.");
                }
                return new KeyValuePair<bool, string>(true, null);
            }
        }
    }
}
