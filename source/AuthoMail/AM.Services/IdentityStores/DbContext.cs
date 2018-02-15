using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
namespace AM.Services.IdentityStores
{
    //public interface IConnectionFactory : IDisposable
    //{
    //    IDbConnection GetConnection { get; }
    //}
    public class DbContext : Disposable//IConnectionFactory
    {
        //Connection String
        string connectionString = ConfigurationManager.ConnectionStrings["CustomDb"].ConnectionString;
        public IDbConnection GetConnection
        {
            get
            {
                var factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
                var conn = factory.CreateConnection();
                conn.ConnectionString = connectionString;
                conn.Open();
                return conn;
            }
        }


    }
    public class Disposable : IDisposable
    {
        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                    DisposeCore();
                }

                // Note disposing has been done.
                disposed = true;
            }
        }

        protected virtual void DisposeCore()
        {

        }

        ~Disposable()
        {
            Dispose(false);
        }
    }
}
