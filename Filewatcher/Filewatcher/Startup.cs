using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Filewatcher.Startup))]
namespace Filewatcher
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
            ConfigureAuth(app);
        }
    }
}
