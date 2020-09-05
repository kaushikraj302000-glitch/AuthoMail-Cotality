using AM.Lib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AM.Services
{
    public class ServiceBase : Disposable
    {
        //public static ConcurrentDictionary<string, int> connections;
        //public static ConcurrentDictionary<string, int> maxConnections;

        private string serviceType;

        protected DbConnection db = null;

        static ServiceBase()
        {
            Handlers.Register();

            //connections = new ConcurrentDictionary<string, int>(Environment.ProcessorCount * 2, 16);
            //maxConnections = new ConcurrentDictionary<string, int>(Environment.ProcessorCount * 2, 16);
            //connections.TryAdd(typeof(CommonService).FullName, 0);
            //maxConnections.TryAdd(typeof(CommonService).FullName, 0);
        }

        protected ServiceBase()
        {
            db = DbFactory.Create("AuthoMail");

            //serviceType = this.GetType().FullName;

            //if (connections.ContainsKey(serviceType))
            //{
            //    connections[serviceType] += 1;
            //    maxConnections[serviceType] = Math.Max(connections[serviceType], maxConnections[serviceType]);
            //}
            //else
            //{
            //    var val = connections.TryAdd(serviceType, 1);
            //    maxConnections.TryAdd(serviceType, 1);

            //    if (!val)
            //    {
            //        Thread.Sleep(100);
            //        val = connections.TryAdd(serviceType, 1);
            //    }
            //}            
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

                //connections[serviceType] -= 1;
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
