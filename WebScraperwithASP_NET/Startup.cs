using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WebScraperwithASP_NET.Startup))]
namespace WebScraperwithASP_NET
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
