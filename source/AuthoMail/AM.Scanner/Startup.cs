using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AM.Scanner.Startup))]
namespace AM.Scanner
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
