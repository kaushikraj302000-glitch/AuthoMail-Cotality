using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AM.Web.Startup))]
namespace AM.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
