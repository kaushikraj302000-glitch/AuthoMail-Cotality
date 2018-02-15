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

        protected ServiceBase()
        {
            db = DbFactory.Create();
        }

        protected override void DisposeCore()
        {
            if (db != null)
            {
                db.Close();
                db.Dispose();
                db = null;
            }
        }

        ~ServiceBase()
        {
            this.Dispose();
        }
    }

    public abstract class Disposable : IDisposable
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

        protected abstract void DisposeCore();

        ~Disposable()
        {
            Dispose(false);
        }
    }

}
