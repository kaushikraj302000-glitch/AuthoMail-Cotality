using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AM.Services.Properties;

namespace AM.Services
{
    public interface IConfigService
    {
        List<AMSetting> GetSystemSettings();
        int InsertSystemSetting(AMSetting amSetting);
        int UpdateSystemSetting(AMSetting amSetting);
        void SeedAMSettings();
    }
    public class ConfigService : ServiceBase, IConfigService
    {
        public List<AMSetting> GetSystemSettings()
        {
            string qry = string.Format("SELECT [Key],[DataType],[Value],[Description],[IsSystem],[DefaultValue] FROM [Common].[SystemSettings]");
            return db.Query<AMSetting>(qry).ToList();
        }

        public int InsertSystemSetting(AMSetting amSetting)
        {
            var query = "INSERT INTO [Common].[SystemSettings] ([Key],[DataType],[Value],[Description],[IsSystem],[DefaultValue]) VALUES (@Key,@DataType,@Value,@Description,@IsSystem,@DefaultValue)";
            return db.Execute(query, amSetting);
        }

        public int UpdateSystemSetting(AMSetting amSetting)
        {
            var query = "UPDATE [Common].[SystemSettings] SET [DataType] = @DataType,[Value] = @Value,[Description] = @Description WHERE [Key] = @Key";
            return db.Execute(query, amSetting);
        }

        public void SeedAMSettings()
        {
            db.Execute(Resources.SeedAMSeetings);
        }
    }
}
