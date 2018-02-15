using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using AM.Entities.IdentityModels;
using AM.Entities.IdentityManage;
using Microsoft.AspNet.Identity;

namespace AM.Services.IdentityStores
{
    public class RoleStore<TRole> : IQueryableRoleStore<TRole> where TRole : IdentityRole
    {
        private AppRole appRole;
        private IDbConnection db;
        public RoleStore()
        {
            db = new DbContext().GetConnection;
            appRole = new AppRole(db);
        }

        public IQueryable<TRole> Roles
        {
            get
            {
                var result = appRole.GetAllRole();
                return (IQueryable<TRole>)result;
            }
        }

        public Task CreateAsync(TRole role)
        {
            if (role == null) throw new ArgumentNullException("role");

            var result = appRole.Insert(role);
            if (result != 0)
            {
                return Task.FromResult<Object>(result);
            }

            return Task.FromResult<Object>(null);
        }

        public Task DeleteAsync(TRole role)
        {
            if (role == null)
            {
                throw new ArgumentNullException("user");
            }

            var result = appRole.Delete(role.Id);
            if (result != null)
            {
                return Task.FromResult<Object>(result);
            }
            return Task.FromResult<Object>(null);
        }

        public Task<TRole> FindByIdAsync(string roleId)
        {
            TRole result = appRole.GetRoleById(roleId) as TRole;

            return Task.FromResult<TRole>(result);
        }

        public Task<TRole> FindByNameAsync(string roleName)
        {
            if (roleName == null) throw new ArgumentNullException("role");

            TRole result = appRole.GetRoleByName(roleName) as TRole;

            return Task.FromResult<TRole>(result);

        }

        public Task UpdateAsync(TRole role)
        {
            if (role == null)
            {
                throw new ArgumentNullException("user");
            }

            var result = appRole.Update(role);
            if (result != null)
            {
                return Task.FromResult<Object>(result);
            }

            return Task.FromResult<Object>(null);
        }
        public Task GetAllRole()
        {
            return Task.FromResult<Object>(null);
        }

        public void Dispose()
        {
            if (db != null)
            {
                db.Dispose();
                db = null;
            }
        }
    }
}
