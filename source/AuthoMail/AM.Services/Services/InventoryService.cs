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
        KeyValuePair<bool, string> SaveEnvelopes(Envelopes envelopes, string userId);
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
        IEnumerable<EnvelopeInventory> GetLast3MonthsInventoryEnvelopes();
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
            var envelopeInventoryStockCalc = GetEnvelopeInventoryStockCalculation();
            envelopes.ForEach(e =>
            {
                var env = envelopeInventoryStockCalc.SingleOrDefault(s => s.EnvelopeId == e.Id);
                if (env != null)
                {
                    e.TotalDays = env.TotalDays;
                    e.TotalQuantityUsed = env.TotalQuantityUsed;
                    e.AvgRate = env.AvgRate;
                    e.EnvDays = env.EnvDays;
                }
            });
            return envelopes;
        }
        public List<EnvelopeInventoryStockCalculation> GetEnvelopeInventoryStockCalculation()
        {
            return db.Query<EnvelopeInventoryStockCalculation>("exec dbo.usp_EnvelopeInventoryStockCalculation").ToList();
        }

        public KeyValuePair<bool, string> SaveEnvelopes(Envelopes envelopes, string userId)
        {
            try
            {
                if (envelopes.Id == 0)
                {
                    var envelope = FindByCodeEnvelope(envelopes.EnvelopeName);
                    if (envelope != null) return new KeyValuePair<bool, string>(false, "Envelope name already exists.");
                    string sql = "INSERT INTO [Common].[Envelopes] ([EnvelopeName],[EnvelopeDesc],[PartNumber] ,[IsBREEnvelope],[CreatedBy],[ModifiedBy],[CreatedOn],[ModifiedOn]) VALUES(@EnvelopeName ,@EnvelopeDesc,@PartNumber ,@IsBREEnvelope,@CreatedBy,@ModifiedBy,@CreatedOn,@ModifiedOn)";
                    envelopes.CreatedBy = envelopes.ModifiedBy = userId;
                    envelopes.CreatedOn = envelopes.ModifiedOn = DateTime.UtcNow;
                    db.Execute(sql, envelopes);
                    return new KeyValuePair<bool, string>(true, "Saved successfully");
                }
                else
                {
                    string sql = "UPDATE [Common].[Envelopes]  SET [EnvelopeName]=@EnvelopeName, [IsBREEnvelope] = @IsBREEnvelope,[ModifiedBy]=@ModifiedBy,[ModifiedOn]=@ModifiedOn,EnvelopeDesc=@EnvelopeDesc,PartNumber=@PartNumber WHERE Id=@Id";
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
                            query = "UPDATE [Common].[EnvelopeStock]  SET [CurrentQuantity] = [CurrentQuantity] + (@CurrentQuantity), ReOrderDuration=@ReOrderDuration WHERE EnvelopeId=@EnvelopeId and ShipperId=@ShipperId";
                            db.Execute(query, new { CurrentQuantity = envtran.ReceivedQuantity, ReOrderDuration = envtran.ReorderDuration, EnvelopeId = envtran.GetEnvelopeId(), ShipperId = envtran.ShipperId }, transaction: t);
                        }
                        else
                        {
                            query = @"insert into [Common].[EnvelopeStock] (EnvelopeId,ShippedId,CurrentQuantity,ReOrderDuration) values (@EnvelopeId,@ShipperId,@CurrentQuantity,@ReOrderDuration)";
                            db.Execute(query, envtran, transaction: t);
                        }
                    }
                    else
                    {
                        string query = "UPDATE [Common].[EnvelopeInventory] SET [EnvelopeId] = @EnvelopeId ,[Description] = @Description ,[ReceivedQuantity] = @ReceivedQuantity ,[UnitPrice] = @UnitPrice,[ModifiedBy]=@ModifiedBy,[ModifiedOn]=@ModifiedOn  WHERE Id=@Id";
                        envtran.ModifiedBy = userId;
                        envtran.ModifiedOn = DateTime.UtcNow;
                        db.Execute(query, envtran, transaction: t);

                        var oldCurrentQuantity = db.QuerySingle<int>("select CurrentQuantity from [Common].[EnvelopeStock] where EnvelopeId=@EnvelopeId and ShipperId=@ShipperId", new { EnvelopeId = envtran.GetEnvelopeId(), ShipperId = envtran.ShipperId }, transaction: t);

                        envtran.ReceivedQuantity = oldCurrentQuantity - oldEnvelopeInventory.ReceivedQuantity + envtran.ReceivedQuantity;
                        query = "UPDATE [Common].[EnvelopeStock]  SET [CurrentQuantity] =@CurrentQuantity,ReOrderDuration=@ReOrderDuration WHERE EnvelopeId=@EnvelopeId and ShipperId=@ShipperId";
                        db.Execute(query, new { CurrentQuantity = envtran.ReceivedQuantity, ReOrderDuration = envtran.ReorderDuration, EnvelopeId = envtran.GetEnvelopeId(), ShipperId = envtran.ShipperId }, transaction: t);
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
            dict = envelopeInvStockCalc.Select(e => new { e.EnvelopeId, e.AvgRate }).ToDictionary(e => e.EnvelopeId.ToString(), e => e.AvgRate);
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
            return db.Query<ShipperInfoSelectListVM>(@"select s.Id,s.PrintVendorId,s.PrintLocation,p.Name as PrintVendor from Common.ShipperInformation s
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
                    string query = "UPDATE [Common].[EnvelopeStock]  SET [CurrentQuantity] =[CurrentQuantity]-(@CurrentQuantity),ReOrderDuration=@ReOrderDuration WHERE EnvelopeId=@EnvelopeId and ShipperId=@ShipperId";
                    db.Execute(query, new { CurrentQuantity = envelopeInventory.ReceivedQuantity, EnvelopeId = envelopeInventory.GetEnvelopeId(), ShipperId = envelopeInventory.ShipperId }, transaction: t);

                    db.Execute("delete from Common.EnvelopeInventory where Id=@Id", new { Id = id });
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
            if (Id == 0)
            {
                var output = db.QuerySingleOrDefault<string>("select PartNumber from Common.Envelopes where Id <> @id and PartNumber = @partNumber", new { id = Id, partNumber = partNumber });
                if (output.IsNullOrWhiteSpace())
                {
                    return new KeyValuePair<bool, string>(true, null);
                }
                return new KeyValuePair<bool, string>(false, "Part Number already exists.");
            }
            else
            {
                var output = db.QuerySingleOrDefault<string>("select PartNumber from Common.Envelopes where PartNumber = @partNumber", new { id = Id, partNumber = partNumber });
                if (output.IsNullOrWhiteSpace())
                {
                    return new KeyValuePair<bool, string>(true, null);
                }
                return new KeyValuePair<bool, string>(false, "Part Number already exists.");
            }
        }

        public IEnumerable<EnvelopeInventory> GetLast3MonthsInventoryEnvelopes()
        {
            return db.Query<EnvelopeInventory>(@"select ei.*,e.*,s.PrintLocation as Shipper,c.DisplayName as CreatedBy, m.DisplayName as ModifiedBy from Common.EnvelopeInventory ei
inner join Common.Envelopes e on ei.EnvelopeId=e.Id
inner join Common.ShipperInformation s on ei.ShipperId=s.Id
inner join Profile.Users c on ei.CreatedBy=c.Id
inner join Profile.Users m on ei.ModifiedBy=m.Id
where ei.ModifiedOn>=@modifiedOn", new { modifiedOn = DateTime.UtcNow.AddMonths(-3) });
        }
    }
}
