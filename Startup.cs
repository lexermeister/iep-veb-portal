using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Veb_portal_za_aukcijsku_prodaju.Startup))]
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "Web.config", Watch = true)]
namespace Veb_portal_za_aukcijsku_prodaju
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
