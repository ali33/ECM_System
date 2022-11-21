using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ArchiveMVC5.Startup))]
namespace ArchiveMVC5
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
