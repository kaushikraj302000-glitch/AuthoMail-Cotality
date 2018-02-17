using AM.Entities.IdentityModels;
using AM.Services;
using AM.Services.Services;
using Autofac;
using Autofac.Integration.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataProtection;
using Owin;
using System.Data.Common;
using System.Web;
using System.Web.Mvc;

[assembly: OwinStartupAttribute(typeof(AM.Web.Startup))]
namespace AM.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var builder = new ContainerBuilder();

            builder.Register<DbConnection>(d => DbFactory.Create()).InstancePerRequest();

            builder.RegisterControllers(typeof(MvcApplication).Assembly);
            builder.RegisterAssemblyTypes(typeof(HomeService).Assembly)
              .Where(t => t.Name.EndsWith("Service"))
              .AsImplementedInterfaces().InstancePerRequest();

            builder.RegisterType<AMUserStore>().As<IUserStore<ProfileUser>>().InstancePerRequest();
            builder.RegisterType<AMUserManager>().AsSelf().InstancePerRequest();
            builder.RegisterType<AMSignInManager>().AsSelf().InstancePerRequest();

            builder.Register<IAuthenticationManager>(c => HttpContext.Current.GetOwinContext().Authentication).InstancePerRequest();
            builder.Register<IDataProtectionProvider>(c => app.GetDataProtectionProvider()).InstancePerRequest();

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            app.UseAutofacMiddleware(container);
            app.UseAutofacMvc();
            ConfigureAuth(app);
        }
    }
}
