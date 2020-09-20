using AM.Lib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class ServiceBase : Disposable
    {
        protected DbConnection db = null;

        static ServiceBase()
        {
            Handlers.Register();
        }

        protected ServiceBase(string connectionStringName)
        {
            db = DbFactory.Create(connectionStringName);
        }

        protected ServiceBase()
        {
            db = DbFactory.Create("AuthoMail");
        }

        /// <summary>
        /// Dispose managed resources.
        /// </summary>
        protected override void DisposeManagedObjects()
        {
            if (db != null)
            {
                // Closing a closed connection doesn't make a different so calling this method as a backup
                // in case the connection was kept open.
                db.Close();
                db.Dispose();
                db = null;
            }
        }

        /// <summary>
        /// Free your own state (unmanaged objects).
        /// Set large fields to null.
        /// </summary>
        protected override void DisposeUnManagedObjects()
        {

            // There are no unmanaged resources to free
            return;
        }
    }
}
