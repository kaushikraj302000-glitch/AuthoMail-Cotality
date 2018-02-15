using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    static class DbFactory
    {
        internal static IDbConnection Create(string connectionStringName)
        {
            var cs = ConfigurationManager.ConnectionStrings[connectionStringName];

            if (cs == null)
            {
                throw new ArgumentException("Connection string with name '{0}' does not exist.", connectionStringName);
            }

            var factory = DbProviderFactories.GetFactory(cs.ProviderName);
            var connection = factory.CreateConnection();
            connection.ConnectionString = cs.ConnectionString;
            return connection;
        }

        /// <summary>
        /// Uses connection string with name 'DefaultConnection' to create connection. If 'DefaultConnection' is not found and
        /// if there is ONLY one connection string defined in config file, it creates connection with that
        /// </summary>
        /// <returns></returns>
        static IDbConnection Create()
        {
            return Create("DefaultConnection");
        }
    }
}
